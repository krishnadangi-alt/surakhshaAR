"""
Competency scoring engine.

A deterministic scoring system that evaluates worker performance against
defined competencies and returns pass/fail decisions with detailed scoring.
"""

import uuid
from dataclasses import dataclass, field
from typing import Dict, List, Optional
from .config import (
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
            "events_processed": self.events_processed
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
        - assessment_completed
        
        Args:
            event: Event dict with 'event_type', 'timestamp', and event-specific data
        """
        self.event_log.append(event)
        event_type = event.get("event_type", "")
        
        # Route event to appropriate scoring logic
        if event_type == "hazard_identified":
            self._score_hazard_identification(event)
        elif event_type == "ppe_selected":
            self._score_ppe_selection(event)
        elif event_type == "equipment_selected":
            self._score_equipment_use(event)
        elif event_type == "wrong_action":
            self._score_wrong_action(event)
        elif event_type == "critical_action":
            self._score_critical_action(event)
        elif event_type == "evacuation_started":
            self._score_evacuation(event)
        elif event_type == "assessment_completed":
            pass  # Finalize in get_result()
    
    def _score_hazard_identification(self, event: Dict) -> None:
        """Score hazard identification competency."""
        correct = event.get("correct", False)
        hazard_type = event.get("hazard_type", "")
        
        if correct:
            # Correct identification gets full points
            self.competency_scores["hazard_identification"] = min(
                100.0,
                self.competency_scores["hazard_identification"] + 50.0
            )
        else:
            # Wrong or missed hazard reduces score
            self.competency_scores["hazard_identification"] = max(
                0.0,
                self.competency_scores["hazard_identification"] - 25.0
            )
    
    def _score_ppe_selection(self, event: Dict) -> None:
        """Score PPE selection competency."""
        correct_ppe = event.get("correct", False)
        ppe_items = event.get("items", [])
        
        if correct_ppe and len(ppe_items) > 0:
            self.competency_scores["ppe_selection"] = min(
                100.0,
                self.competency_scores["ppe_selection"] + 60.0
            )
        else:
            self.competency_scores["ppe_selection"] = max(
                0.0,
                self.competency_scores["ppe_selection"] - 30.0
            )
    
    def _score_equipment_use(self, event: Dict) -> None:
        """Score equipment use competency."""
        correct = event.get("correct", False)
        
        if correct:
            self.competency_scores["equipment_use"] = min(
                100.0,
                self.competency_scores["equipment_use"] + 50.0
            )
        else:
            self.competency_scores["equipment_use"] = max(
                0.0,
                self.competency_scores["equipment_use"] - 25.0
            )
    
    def _score_wrong_action(self, event: Dict) -> None:
        """Score wrong action - penalizes decision_making and procedure_compliance."""
        severity = event.get("severity", "minor")  # minor, major, critical
        
        if severity == "major":
            self.competency_scores["procedure_compliance"] = max(
                0.0,
                self.competency_scores["procedure_compliance"] - 30.0
            )
            self.competency_scores["decision_making"] = max(
                0.0,
                self.competency_scores["decision_making"] - 25.0
            )
        else:  # minor
            self.competency_scores["procedure_compliance"] = max(
                0.0,
                self.competency_scores["procedure_compliance"] - 5.0
            )
            self.competency_scores["decision_making"] = max(
                0.0,
                self.competency_scores["decision_making"] - 3.0
            )
    
    def _score_critical_action(self, event: Dict) -> None:
        """
        Record critical action - triggers automatic FAIL.
        
        Critical actions are safety-critical mistakes that must result in
        assessment failure regardless of other scores.
        """
        action = event.get("action", "unknown_critical")
        reason = event.get("reason", "Critical safety violation")
        
        self.critical_errors.append({
            "action": action,
            "reason": reason,
            "timestamp": event.get("timestamp")
        })
    
    def _score_evacuation(self, event: Dict) -> None:
        """Score evacuation behavior."""
        correct = event.get("correct", False)
        
        if self.scenario_type == "gas":
            competency = "evacuation"
        else:  # fire
            competency = "procedure_compliance"
        
        if correct:
            if competency in self.competency_scores:
                self.competency_scores[competency] = min(
                    100.0,
                    self.competency_scores[competency] + 50.0
                )
        else:
            if competency in self.competency_scores:
                self.competency_scores[competency] = max(
                    0.0,
                    self.competency_scores[competency] - 30.0
                )
    
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
        
        return ScoringResult(
            assessment_id=self.assessment_id,
            scenario_type=self.scenario_type,
            competency_scores=competency_scores_objs,
            overall_score=overall_score,
            passed=passed,
            critical_errors=[e["reason"] for e in self.critical_errors],
            pass_reason=pass_reason,
            events_processed=len(self.event_log)
        )
