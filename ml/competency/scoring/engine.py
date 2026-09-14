"""
Competency scoring engine.

A deterministic scoring system that evaluates worker performance against
defined competencies and returns pass/fail decisions with detailed scoring.
"""

import uuid
from dataclasses import dataclass, field
from typing import Dict, List, Optional
from .config import (
    COMPLETION_EVENT_TYPES,
    ESTOP_BENCHMARK_SECONDS,
    RESPONSE_TIME_FAST_BONUS,
    RESPONSE_TIME_FAST_THRESHOLD_SECONDS,
    RESPONSE_TIME_SLOW_PENALTY,
    RESPONSE_TIME_SLOW_THRESHOLD_SECONDS,
    UNSAFE_ACTION_DECISION_PENALTY,
    UNSAFE_ACTION_PENALTY,
    WRONG_ACTION_MAJOR_DECISION_PENALTY,
    WRONG_ACTION_MAJOR_PROCEDURE_PENALTY,
    WRONG_ACTION_MINOR_DECISION_PENALTY,
    WRONG_ACTION_MINOR_PROCEDURE_PENALTY,
    get_competencies, OVERALL_PASS_THRESHOLD, WEAKNESS_THRESHOLD,
    SEVERE_WEAKNESS_THRESHOLD
)

@dataclass
class CompetencyScore:
    """Score for a single competency."""
    name: str
    score: float
    max_score: float = 100.0
    pass_threshold: float = 70.0
    passed: bool = field(init=False)
    
    def __post_init__(self):
        self.passed = self.score >= self.pass_threshold


@dataclass
class ScoringResult:
    """Complete scoring result from an assessment."""
    assessment_id: str
    scenario_type: str  # "fire" or "gas"
    competency_scores: Dict[str, CompetencyScore]  # name -> score
    overall_score: float  # Average across competencies
    passed: bool  # Overall PASS/FAIL
    critical_errors: List[str] = field(default_factory=list)  # Errors causing FAIL
    pass_reason: str = ""  # Why it passed or failed
    events_processed: int = 0  # Number of events analyzed
    # --- Day 1 (Rehan-confirmed) tracking fields ---
    # The live CompetencyDefinition objects are attached here so downstream
    # stages (weakness detection) can populate affected_aspects.
    competency_definitions: Dict = field(default_factory=dict)
    completed: bool = False  # SCENARIO_COMPLETED / required completion present
    completion_events: int = 0  # Number of completion events observed
    fast_responses: int = 0  # Events with response_time_seconds < 3.0s
    slow_responses: int = 0  # Events with response_time_seconds > 15.0s
    delayed_estop_reactions: int = 0  # E-Stop reactions slower than 2.5s
    
    def to_dict(self):
        """Convert to dictionary for serialization."""
        return {
            "assessment_id": self.assessment_id,
            "scenario_type": self.scenario_type,
            "competency_scores": {
                k: {
                    "name": v.name,
                    "score": v.score,
                    "passed": v.passed,
                    "pass_threshold": v.pass_threshold
                }
                for k, v in self.competency_scores.items()
            },
            "overall_score": self.overall_score,
            "passed": self.passed,
            "critical_errors": self.critical_errors,
            "pass_reason": self.pass_reason,
            "events_processed": self.events_processed,
            "completed": self.completed,
            "completion_events": self.completion_events,
            "fast_responses": self.fast_responses,
            "slow_responses": self.slow_responses,
            "delayed_estop_reactions": self.delayed_estop_reactions,
        }


class CompetencyScorer:
    """
    Deterministic competency scoring engine.
    
    Scores worker performance across defined competencies and provides
    pass/fail verdicts based on configurable thresholds.
    """
    
    def __init__(self, scenario_type: str = "fire"):
        """
        Initialize scorer for a specific scenario type.
        
        Args:
            scenario_type: "fire" or "gas"
        """
        self.scenario_type = scenario_type.lower()
        self.competencies = get_competencies(self.scenario_type)
        # Initialize all competencies with a baseline score of 50 (neutral)
        # Correct actions increase the score; mistakes decrease it
        self.competency_scores = {
            name: 50.0 for name in self.competencies.keys()
        }
        self.critical_errors = []
        self.event_log = []
        self.assessment_id = f"assessment_{uuid.uuid4().hex[:12]}"
        # --- Day 1 tracking (see ScoringResult fields) ---
        self.completed = False
        self.completion_events = 0
        self.fast_responses = 0
        self.slow_responses = 0
        self.delayed_estop_reactions = 0

    # ------------------------------------------------------------------
    # Day 1 helpers (Rehan-confirmed rules; single centralized logic)
    # ------------------------------------------------------------------

    @staticmethod
    def _is_critical_event(event: Dict) -> bool:
        """Return True when an event is a Critical Safety Error.

        Any of these marks an event critical, regardless of event_type::
            - event_type == "critical_action"
            - critical is True
            - severity == "critical"
        """
        if not isinstance(event, dict):
            return False
        if str(event.get("event_type", "")).lower() == "critical_action":
            return True
        if event.get("critical") is True:
            return True
        severity = event.get("severity")
        return isinstance(severity, str) and severity.strip().lower() == "critical"

    @staticmethod
    def _response_time_seconds(event: Dict) -> Optional[float]:
        """Return the numeric response_time_seconds for an event, if any."""
        raw = event.get("response_time_seconds")
        if raw is None:
            return None
        try:
            value = float(raw)
        except (TypeError, ValueError):
            return None
        # Unity's JsonUtility always serialises floats, so unset fields arrive
        # as 0.0. Treat non-positive values as "no response time recorded"
        # (a real response time is always > 0) so every event is not mistaken
        # for an instant 0-second reaction.
        if value <= 0:
            return None
        return value

    @staticmethod
    def _is_estop_event(event: Dict) -> bool:
        """Best-effort detection of a Machinery E-Stop reaction event."""
        haystacks = []
        for key in ("event_type", "action", "hazard_type", "description", "reason"):
            value = event.get(key)
            if isinstance(value, str):
                haystacks.append(value.lower())
        return any("estop" in text or "e-stop" in text or "e_stop" in text for text in haystacks)

    def _apply_response_time(self, delta: float, event: Dict) -> float:
        """Scale a competency score delta per Rehan's response-time rules.

        - response_time_seconds < 3.0s  -> +5% bonus (delta grows by 5%).
        - 3.0-15.0s                    -> baseline / no change.
        - response_time_seconds > 15.0s -> -10% procedural latency penalty.
        - Machinery E-Stop benchmark: < 2.5s; a late E-Stop reaction records
          a delayed-reaction penalty (counted on the result; the score effect
          flows through the same latency-penalty path).

        ``delta`` is signed: positive for correct-action gains, negative for
        mistake penalties. The bonus amplifies the delta's magnitude; the
        latency penalty shrinks it.

        Fast/slow counters are incremented once per EVENT in ``process_event``
        (an event may produce several competency deltas).
        """
        response_time = self._response_time_seconds(event)
        if response_time is None:
            return delta
        if response_time < RESPONSE_TIME_FAST_THRESHOLD_SECONDS:
            return delta * (1.0 + RESPONSE_TIME_FAST_BONUS)
        if response_time > RESPONSE_TIME_SLOW_THRESHOLD_SECONDS:
            return delta * (1.0 - RESPONSE_TIME_SLOW_PENALTY)
        return delta

    def _track_response_time(self, event: Dict) -> None:
        """Count one fast/slow response per event (Day 1 rule 3 audit data)."""
        response_time = self._response_time_seconds(event)
        if response_time is None:
            return
        if response_time < RESPONSE_TIME_FAST_THRESHOLD_SECONDS:
            self.fast_responses += 1
        elif response_time > RESPONSE_TIME_SLOW_THRESHOLD_SECONDS:
            self.slow_responses += 1

    def _check_estop_timing(self, event: Dict) -> None:
        """Record late Machinery E-Stop reactions (benchmark < 2.5s)."""
        if not self._is_estop_event(event):
            return
        response_time = self._response_time_seconds(event)
        if response_time is None:
            return
        if response_time >= ESTOP_BENCHMARK_SECONDS:
            self.delayed_estop_reactions += 1

    def _apply_delta(self, competency: Optional[str], delta: float, event: Dict) -> None:
        """Apply a signed score delta after response-time scaling.

        Note: E-Stop timing and fast/slow counters are tracked once per event
        in ``process_event`` (an event may produce several deltas).
        """
        if competency not in self.competency_scores:
            return
        scaled = self._apply_response_time(delta, event)
        if scaled >= 0:
            self.competency_scores[competency] = min(
                100.0, self.competency_scores[competency] + scaled
            )
        else:
            self.competency_scores[competency] = max(
                0.0, self.competency_scores[competency] + scaled
            )
    
    def process_event(self, event: Dict) -> None:
        """
        Process an assessment event and update scores.
        
        Event types (from API):
        - training_started
        - assessment_started
        - hazard_identified
        - ppe_selected
        - equipment_selected
        - wrong_action
        - critical_action
        - evacuation_started
        - emergency_procedure
        - assessment_completed
        
        Args:
            event: Event dict with 'event_type', 'timestamp', and event-specific data
        """
        self.event_log.append(event)
        raw_type = event.get("event_type", "")
        event_type = raw_type.lower().strip()

        # Day 1 (Rehan) rule 3: audit counters are tracked once per event.
        self._track_response_time(event)
        self._check_estop_timing(event)

        # Day 1 (Rehan): any critical-severity event is a Critical Safety
        # Error and forces automatic FAIL regardless of event_type/score.
        # Handled centrally here so critical=true / severity="critical" on
        # ANY event type (incl. wrong_action / unsafe_action) FAILs.
        if self._is_critical_event(event):
            self._score_critical_action(event)
            return

        # Route event to appropriate scoring logic
        if event_type == "hazard_identified":
            self._score_hazard_identification(event)
        elif event_type == "ppe_selected":
            self._score_ppe_selection(event)
        elif event_type in ("equipment_selected", "object_interaction"):
            self._score_equipment_use(event)
        elif event_type in ("wrong_action", "sequence_error"):
            self._score_wrong_action(event)
        elif event_type == "unsafe_action":
            self._score_unsafe_action(event)
        elif event_type == "critical_action":
            self._score_critical_action(event)
        elif event_type == "evacuation_started":
            self._score_evacuation(event)
        elif event_type in ("emergency_procedure", "correct_action"):
            self._score_emergency_procedure(event)
        elif event_type in COMPLETION_EVENT_TYPES:
            self.completion_events += 1
            self.completed = True
    
    def _score_hazard_identification(self, event: Dict) -> None:
        """Score hazard identification competency."""
        correct = event.get("correct", True)
        if correct:
            # Correct identification gets full points
            self._apply_delta("hazard_identification", 50.0, event)
        else:
            # Wrong or missed hazard reduces score
            self._apply_delta("hazard_identification", -25.0, event)

    def _score_ppe_selection(self, event: Dict) -> None:
        """Score PPE selection competency."""
        correct_ppe = event.get("correct", False)
        ppe_items = event.get("items", [])
        ppe_type = event.get("ppe_type")
        has_items = (len(ppe_items) > 0) or (ppe_type is not None and len(str(ppe_type).strip()) > 0)

        if correct_ppe and has_items:
            self._apply_delta("ppe_selection", 60.0, event)
        else:
            self._apply_delta("ppe_selection", -30.0, event)

    def _score_equipment_use(self, event: Dict) -> None:
        """Score equipment use competency."""
        correct = event.get("correct", False)

        if correct:
            self._apply_delta("equipment_use", 50.0, event)
        else:
            self._apply_delta("equipment_use", -25.0, event)

    def _procedure_and_decision_competencies(self):
        """Scenario-aware (procedure, decision) competency pair."""
        if self.scenario_type == "gas":
            return ("emergency_response", "hazard_identification")
        return ("procedure_compliance", "decision_making")

    def _score_wrong_action(self, event: Dict) -> None:
        """Score wrong action - penalizes procedure compliance and decision making.

        WRONG_ACTION = non-life-threatening procedural/operational mistake
        (minor/major). A ``severity == "critical"`` payload never reaches here:
        it is intercepted in ``process_event`` as a Critical Safety Error.

        Scenario-aware: gas scenarios do not define procedure_compliance or
        decision_making, so the penalties are mapped onto the closest gas
        competencies (emergency_response for procedure violations,
        hazard_identification for faulty decisions).
        """
        severity = str(event.get("severity", "minor") or "minor").strip().lower()

        procedure_competency, decision_competency = (
            self._procedure_and_decision_competencies()
        )

        if severity == "major":
            self._apply_delta(
                procedure_competency,
                -WRONG_ACTION_MAJOR_PROCEDURE_PENALTY,
                event,
            )
            self._apply_delta(
                decision_competency,
                -WRONG_ACTION_MAJOR_DECISION_PENALTY,
                event,
            )
        else:  # minor (default)
            self._apply_delta(
                procedure_competency,
                -WRONG_ACTION_MINOR_PROCEDURE_PENALTY,
                event,
            )
            self._apply_delta(
                decision_competency,
                -WRONG_ACTION_MINOR_DECISION_PENALTY,
                event,
            )

    def _score_unsafe_action(self, event: Dict) -> None:
        """Score UNSAFE_ACTION — distinct from WRONG_ACTION and CRITICAL_ACTION.

        UNSAFE_ACTION increases hazard exposure but is not immediately fatal:
        it carries a larger numerical penalty (-20 to -25 band, project default
        UNSAFE_ACTION_PENALTY) yet does NOT automatically FAIL. Uses the same
        scenario-aware procedure/decision competency mapping as wrong_action.
        """
        procedure_competency, decision_competency = (
            self._procedure_and_decision_competencies()
        )
        self._apply_delta(
            procedure_competency, -UNSAFE_ACTION_PENALTY, event
        )
        self._apply_delta(
            decision_competency, -UNSAFE_ACTION_DECISION_PENALTY, event
        )

    def _score_critical_action(self, event: Dict) -> None:
        """Record Critical Safety Error — triggers automatic FAIL.

        Reached via ``event_type == "critical_action"`` OR ``critical is True``
        OR ``severity == "critical"`` on any event type (see _is_critical_event).
        Safety-critical mistakes fail regardless of numerical scores.
        """
        action = event.get("action", event.get("event_type", "unknown_critical"))
        reason = event.get("reason", "Critical safety violation")
        # Preserve the trigger so audits can see WHY this counted as critical
        # (explicit critical_action vs critical=true vs severity=critical).
        trigger = event.get("event_type", "critical_action")
        if event.get("critical") is True and trigger != "critical_action":
            trigger = f"{trigger}+critical=true"
        severity = event.get("severity")
        if isinstance(severity, str) and severity.strip().lower() == "critical":
            trigger = f"{trigger}+severity=critical"

        self.critical_errors.append({
            "action": action,
            "reason": reason,
            "trigger": trigger,
            "timestamp": event.get("timestamp")
        })
    
    def _score_evacuation(self, event: Dict) -> None:
        """Score evacuation behavior."""
        correct = event.get("correct", event.get("safe", False))
        if self.scenario_type == "gas":
            competency = "evacuation"
        else:  # fire / machinery
            competency = "procedure_compliance"

        if correct:
            self._apply_delta(competency, 50.0, event)
        else:
            self._apply_delta(competency, -30.0, event)

    def _score_emergency_procedure(self, event: Dict) -> None:
        """Score emergency response procedures, correct actions, and LOTO steps."""
        correct = event.get("correct", True)
        action = str(event.get("action", "")).lower()
        
        # 1. Fire: procedure compliance
        if "procedure_compliance" in self.competency_scores:
            if correct:
                self._apply_delta("procedure_compliance", 25.0, event)
        
        # 2. Gas / Machinery: emergency response
        if "emergency_response" in self.competency_scores:
            if correct:
                self._apply_delta("emergency_response", 50.0, event)
            else:
                self._apply_delta("emergency_response", -25.0, event)
        
        # 3. Machinery: LOTO procedure
        if "loto_procedure" in self.competency_scores:
            if "loto" in action and correct:
                self._apply_delta("loto_procedure", 25.0, event)
    
    def get_result(self) -> ScoringResult:
        """
        Finalize and return the scoring result.
        
        Returns:
            ScoringResult with all competency scores and pass/fail decision
        """
        # Clamp scores to valid range
        for competency_name in self.competency_scores:
            score = self.competency_scores[competency_name]
            self.competency_scores[competency_name] = max(0.0, min(100.0, score))
        
        # Calculate overall score
        scores = list(self.competency_scores.values())
        overall_score = sum(scores) / len(scores) if scores else 0.0
        
        # Create CompetencyScore objects
        competency_scores_objs = {}
        for name, score in self.competency_scores.items():
            comp_def = self.competencies[name]
            competency_scores_objs[name] = CompetencyScore(
                name=name,
                score=score,
                pass_threshold=comp_def.pass_threshold
            )
        
        # Determine pass/fail
        passed = True
        pass_reason = ""
        
        # Critical errors cause automatic FAIL
        if self.critical_errors:
            passed = False
            error_summary = "; ".join(
                [e["reason"] for e in self.critical_errors]
            )
            pass_reason = f"CRITICAL ERRORS: {error_summary}"
        # Overall score must meet threshold
        elif overall_score < OVERALL_PASS_THRESHOLD:
            passed = False
            pass_reason = f"Insufficient overall competency (score: {overall_score:.1f}, required: {OVERALL_PASS_THRESHOLD})"
        # All competencies must pass
        elif not all(cs.passed for cs in competency_scores_objs.values()):
            passed = False
            failed_comps = [
                name for name, cs in competency_scores_objs.items()
                if not cs.passed
            ]
            pass_reason = f"Failed competencies: {', '.join(failed_comps)}"
        else:
            passed = True
            pass_reason = f"Assessment passed (overall score: {overall_score:.1f})"

        # Rehan rule 4: assessment completion is mandatory. An incomplete
        # assessment must NOT pass, even when scores are otherwise passing.
        completed = self.completed or self.completion_events > 0
        if completed:
            pass
        elif any(
            str(e.get("event_type", "") or "").lower() in COMPLETION_EVENT_TYPES
            for e in self.event_log
            if isinstance(e, dict)
        ):
            completed = True
        if passed and not completed:
            passed = False
            pass_reason = (
                "Incomplete assessment: "
                "SCENARIO_COMPLETED / required completion not observed"
            )

        return ScoringResult(
            assessment_id=self.assessment_id,
            scenario_type=self.scenario_type,
            competency_scores=competency_scores_objs,
            overall_score=overall_score,
            passed=passed,
            critical_errors=[e["reason"] for e in self.critical_errors],
            pass_reason=pass_reason,
            events_processed=len(self.event_log),
            competency_definitions=dict(self.competencies),
            completed=completed,
            completion_events=self.completion_events,
            fast_responses=self.fast_responses,
            slow_responses=self.slow_responses,
            delayed_estop_reactions=self.delayed_estop_reactions,
        )
