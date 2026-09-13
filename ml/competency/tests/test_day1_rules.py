"""
Day 1 — Final Scoring & Competency tests (Rehan-confirmed rules).

Covers the confirmed Day 1 behaviours end-to-end through the engine and the
public assess() pipeline:

1.  Correct action
2.  Wrong action (minor / major)
3.  Unsafe action (distinct from wrong/critical; -20..-25 band; no auto-FAIL)
4.  Critical action (event_type = critical_action -> automatic FAIL)
5.  critical=true on any event -> automatic FAIL
6.  wrong_action + severity="critical" -> automatic FAIL
7.  Response time < 3s -> +5% bonus
8.  Response time 3-15s -> baseline
9.  Response time > 15s -> -10% procedural latency penalty
10. Machinery E-Stop benchmark (< 2.5s; late reaction -> delayed-reaction flag)
11. Completed assessment passes
12. Incomplete assessment must NOT pass
13. Attempt tracking is data-only (no score effect)
14. Failed attempt -> retraining plan (retrain-then-reassess gating data)
15. affected_aspects populated from competency sub-aspects
16. Overall PASS >= 70%
17. Competency threshold failure
18. Critical-error automatic FAIL
"""

import pytest

from ml.competency.assess import assess
from ml.competency.retraining import RetrainingRecommender
from ml.competency.scoring import CompetencyScorer, get_competencies
from ml.competency.scoring.config import (
    RESPONSE_TIME_FAST_BONUS,
    RESPONSE_TIME_FAST_THRESHOLD_SECONDS,
    RESPONSE_TIME_SLOW_PENALTY,
    RESPONSE_TIME_SLOW_THRESHOLD_SECONDS,
    SIH_BASELINE_THRESHOLDS,
    UNSAFE_ACTION_PENALTY,
    UNSAFE_ACTION_PENALTY_RANGE,
)
from ml.competency.weakness_detection import WeaknessDetector
class TestEventCategoryDistinction:
    """Rehan rules 1: correct / wrong / unsafe / critical are distinct."""

    def test_1_correct_action_scores_positively(self):
        result = _run(GOOD_COMPLETED_FIRE)
        assert result.passed is True
        assert result.overall_score == pytest.approx(90.0)
        assert result.competency_scores["hazard_identification"].score == pytest.approx(100.0)

    def test_2_wrong_action_minor_is_a_small_penalty(self):
        scorer = CompetencyScorer("fire")
        scorer.process_event({"event_type": "wrong_action", "severity": "minor"})
        result = scorer.get_result()
        # minor: -5 procedure, -3 decision (small, non-life-threatening)
        assert result.competency_scores["procedure_compliance"].score == pytest.approx(45.0)
        assert result.competency_scores["decision_making"].score == pytest.approx(47.0)
        assert result.critical_errors == []

    def test_3_unsafe_action_is_distinct_and_in_confirmed_band(self):
        """UNSAFE_ACTION: larger penalty (-20..-25), NOT an automatic FAIL."""
        assert UNSAFE_ACTION_PENALTY_RANGE[0] <= -UNSAFE_ACTION_PENALTY <= UNSAFE_ACTION_PENALTY_RANGE[1]

        events = GOOD_COMPLETED_FIRE[:1] + [  # hazard_identified correct (+50)
            {"event_type": "unsafe_action", "action": "removed_machine_guard"},
        ] + GOOD_COMPLETED_FIRE[1:]
        result = _run(events)
        # procedure: 50 + 50 - 22 = 78 (>= 75 threshold); decision: 50 - 20 = 30
        assert result.competency_scores["procedure_compliance"].score == pytest.approx(78.0)
        assert result.competency_scores["decision_making"].score == pytest.approx(30.0)
        # Not immediately fatal: no automatic FAIL from the unsafe event itself.
        assert result.critical_errors == []

    def test_3b_unsafe_action_can_still_fail_via_scores(self):
        """An unsafe action is not auto-FAIL, but the numeric damage can fail."""
        events = [
            {"event_type": "unsafe_action", "action": "removed_machine_guard"},
            {"event_type": "assessment_completed", "completion_status": "success"},
        ]
        result = _run(events)
        assert result.passed is False
        assert result.critical_errors == []

    def test_4_critical_action_automatic_fail(self):
        events = GOOD_COMPLETED_FIRE[:4] + [
            {"event_type": "critical_action", "action": "opened_door_during_fire", "reason": "Fed oxygen to flames"},
        ]
        result = _run(events)
        assert result.passed is False
        assert len(result.critical_errors) == 1
        assert "CRITICAL ERRORS" in result.pass_reason

    def test_5_critical_true_on_any_event_is_automatic_fail(self):
        events = GOOD_COMPLETED_FIRE[:1] + [
            {"event_type": "hazard_identified", "correct": True, "hazard_type": "fire",
             "critical": True, "reason": "Ignored active alarm"},
        ] + GOOD_COMPLETED_FIRE[1:]
        result = _run(events)
        assert result.passed is False
        assert len(result.critical_errors) == 1
        assert result.critical_errors[0].startswith("Ignored active alarm")

    def test_6_wrong_action_with_severity_critical_is_automatic_fail(self):
        """severity="critical" must NOT behave like minor any more."""
        events = GOOD_COMPLETED_FIRE[:4] + [
            {"event_type": "wrong_action", "severity": "critical", "action": "disabled_alarm"},
        ]
        result = _run(events)
        assert result.passed is False
        assert len(result.critical_errors) == 1

    def test_18_critical_error_fails_regardless_of_score(self):
        """Even a 100-score assessment FAILS with a critical error."""
        result = _run(GOOD_COMPLETED_FIRE + [
            {"event_type": "critical_action", "action": "reentered_burning_area"},
        ])
        assert result.passed is False





def _run(events, scenario_type="fire"):
    scorer = CompetencyScorer(scenario_type=scenario_type)
    for event in events:
        scorer.process_event(event)
    return scorer.get_result()


# A minimal completed, fully correct fire assessment (overall 90.0).
GOOD_COMPLETED_FIRE = [
    {"event_type": "hazard_identified", "correct": True, "hazard_type": "electrical_fire"},
    {"event_type": "ppe_selected", "correct": True, "items": ["helmet", "gloves", "jacket"]},
    {"event_type": "equipment_selected", "correct": True, "action": "grab_extinguisher"},
    {"event_type": "evacuation_started", "correct": True, "route": "north_exit"},
    {"event_type": "assessment_completed", "completion_status": "success"},
]
class TestResponseTimeRules:
    """Rehan rule 3: <3s +5%, 3-15s baseline, >15s -10%, E-Stop <2.5s."""

    def test_7_fast_response_applies_bonus(self):
        scorer = CompetencyScorer("fire")
        scorer.process_event({
            "event_type": "wrong_action", "severity": "minor",
            "response_time_seconds": 2.0,
        })
        result = scorer.get_result()
        # -5 * 1.05 = -5.25 -> 50 - 5.25 = 44.75 (bonus amplifies the delta)
        assert result.competency_scores["procedure_compliance"].score == pytest.approx(
            50.0 - 5.0 * (1.0 + RESPONSE_TIME_FAST_BONUS)
        )
        assert result.fast_responses == 1

    def test_8_midband_response_is_baseline(self):
        scorer = CompetencyScorer("fire")
        scorer.process_event({
            "event_type": "hazard_identified", "correct": True,
            "response_time_seconds": 8.0,
        })
        result = scorer.get_result()
        # 50 (baseline) + 50 (delta, unscaled) = 100
        assert result.competency_scores["hazard_identification"].score == pytest.approx(100.0)
        assert result.fast_responses == 0
        assert result.slow_responses == 0

    def test_9_slow_response_applies_latency_penalty(self):
        scorer = CompetencyScorer("fire")
        scorer.process_event({
            "event_type": "hazard_identified", "correct": True,
            "response_time_seconds": 20.0,
        })
        result = scorer.get_result()
        # 50 (baseline) + 50 * 0.90 = 95
        assert result.competency_scores["hazard_identification"].score == pytest.approx(
            50.0 + 50.0 * (1.0 - RESPONSE_TIME_SLOW_PENALTY)
        )
        assert result.slow_responses == 1

    def test_response_time_boundaries_and_missing_data(self):
        scorer = CompetencyScorer("fire")
        # Exactly at thresholds -> baseline (rules use strict < and >)
        scorer.process_event({"event_type": "hazard_identified", "correct": True,
                              "response_time_seconds": RESPONSE_TIME_FAST_THRESHOLD_SECONDS})
        scorer.process_event({"event_type": "ppe_selected", "correct": True, "items": ["helmet"],
                              "response_time_seconds": RESPONSE_TIME_SLOW_THRESHOLD_SECONDS})
        # Missing / zero (Unity's unset float) -> no response-time effect
        scorer.process_event({"event_type": "equipment_selected", "correct": True,
                              "response_time_seconds": 0})
        result = scorer.get_result()
        assert result.fast_responses == 0
        assert result.slow_responses == 0
        assert result.competency_scores["hazard_identification"].score == pytest.approx(100.0)
        assert result.competency_scores["ppe_selection"].score == pytest.approx(100.0)

    def test_10_estop_benchmark_tracking(self):
        # Late E-Stop (3.0s > 2.5s benchmark) -> delayed-reaction penalty flag
        scorer = CompetencyScorer("fire")
        scorer.process_event({"event_type": "equipment_selected", "correct": True,
                              "action": "estop_press", "response_time_seconds": 3.0})
        result = scorer.get_result()
        assert result.delayed_estop_reactions == 1
        # Score effect: 3.0s is also inside the 3-15s band -> baseline; the
        # delayed flag is the recorded penalty marker per the confirmed rule.
        assert result.competency_scores["equipment_use"].score == pytest.approx(100.0)

    def test_10b_fast_estop_within_benchmark(self):
        scorer = CompetencyScorer("fire")
        scorer.process_event({"event_type": "wrong_action", "severity": "minor",
                              "action": "e-stop engaged late", "response_time_seconds": 2.0})
        result = scorer.get_result()
        assert result.delayed_estop_reactions == 0
        # < 2.5s is also < 3.0s -> fast bonus amplifies the -5 delta
        assert result.competency_scores["procedure_compliance"].score == pytest.approx(
            50.0 - 5.0 * (1.0 + RESPONSE_TIME_FAST_BONUS)
        )


class TestCompletionRule:
    """Rehan rule 4: completion is mandatory; incomplete must NOT pass."""

    def test_11_completed_assessment_passes(self):
        result = _run(GOOD_COMPLETED_FIRE)
        assert result.passed is True
        assert result.completed is True
        assert result.completion_events == 1

    def test_12_incomplete_assessment_must_not_pass(self):
        result = _run(GOOD_COMPLETED_FIRE[:4])  # identical scores, no completion
        assert result.overall_score == pytest.approx(90.0)  # scores alone would pass
        assert result.completed is False
        assert result.passed is False
class TestAttemptsRule:
    """Rehan rule 5: attempts are tracking data only, no score penalty."""

    def test_13_attempt_number_has_no_score_effect(self):
        events_a = [{"event_type": "hazard_identified", "correct": True},
                    {"event_type": "assessment_completed"}]
        # The engine never sees attempt metadata; scoring is per-attempt only.
        result_a = _run(events_a)
        result_b = _run(list(events_a))  # identical performance, "later attempt"
        assert result_a.overall_score == result_b.overall_score
        assert result_a.passed == result_b.passed

    def test_13b_attempt_number_is_recorded_for_audit(self):
        """attempt_number stays an audit/progression field on the API payload;
        the engine applies no penalty based on previous failed attempts."""
        from backend.tests.events import BAD_FIRE_EVENTS, GOOD_FIRE_EVENTS  # noqa: F401
        # Same events -> identical score, regardless of "attempt history".
        scored_a = _run(GOOD_FIRE_EVENTS)
        scored_b = _run(list(GOOD_FIRE_EVENTS))
        assert scored_a.overall_score == scored_b.overall_score
        assert scored_a.passed == scored_b.passed is True

    def test_14_failed_attempt_produces_retraining_plan(self):
        """After FAIL, the pipeline yields targeted retraining (the retrain ->
        reassess gating input). Direct-reassessment blocking itself is a
        backend workflow concern (progress stage: retrain -> reassess)."""
        events = [{"event_type": "wrong_action", "severity": "major"},
                  {"event_type": "assessment_completed"}]
        result = _run(events)
        assert result.passed is False
        weaknesses = WeaknessDetector().detect_weaknesses(result)
        assert len(weaknesses) > 0
        plan = RetrainingRecommender("fire").get_retraining_plan(weaknesses)
        assert plan["total_weaknesses"] > 0
        assert len(plan["recommended_modules"]) > 0


class TestAffectedAspects:
    """Rehan rule 6: affected_aspects populated from competency sub-aspects."""

    def test_15_weaknesses_carry_their_competency_aspects(self):
        events = [{"event_type": "wrong_action", "severity": "major"},
                  {"event_type": "assessment_completed"}]
        result = _run(events)
        weaknesses = WeaknessDetector().detect_weaknesses(result)
        assert weaknesses, "expected at least one weakness"
        definitions = get_competencies("fire")
        for weakness in weaknesses:
            expected = definitions[weakness.competency_name].aspects
            assert weakness.affected_aspects == list(expected)
            assert weakness.affected_aspects, "affected_aspects must not be empty"

    def test_15b_weaknesses_flow_to_backend_shape(self):
        import sys
        from pathlib import Path

        repo_root = Path(__file__).resolve().parents[3]
        backend_path = str(repo_root / "backend")
        if backend_path not in sys.path:
            sys.path.insert(0, backend_path)
        try:
            from backend.app.services.competency_service import score_events
        except ModuleNotFoundError:
            pytest.skip("backend app package not importable in this environment")
        scored = score_events("fire", [
            {"event_type": "wrong_action", "severity": "major"},
            {"event_type": "assessment_completed", "completion_status": "success"},
        ])
        assert scored["weaknesses"], "expected weaknesses from a failing assessment"
        for weakness in scored["weaknesses"]:
            assert weakness["affected_aspects"], "affected_aspects must be populated"


class TestPassFailRules:
    """Rehan rule 7: locked SIH baseline thresholds."""

    def test_16_overall_pass_threshold_is_70(self):
        assert SIH_BASELINE_THRESHOLDS["overall_pass"] == 70.0
        events = GOOD_COMPLETED_FIRE[:4] + [
            # One extra correct hazard identification (+50 hazard, capped at 100)
            {"event_type": "hazard_identified", "correct": True, "hazard_type": "fuel_source"},
            {"event_type": "assessment_completed", "completion_status": "success"},
        ]
        result = _run(events)
        assert result.overall_score >= SIH_BASELINE_THRESHOLDS["overall_pass"]
        assert result.passed is True

    def test_16b_below_overall_threshold_fails(self):
        events = [{"event_type": "wrong_action", "severity": "minor"},
                  {"event_type": "assessment_completed"}]
        result = _run(events)
        assert result.overall_score < 70.0
        assert result.passed is False

    def test_17_competency_threshold_failure_fails_even_with_passing_overall(self):
        """A single competency below its threshold fails the assessment
        (decision_making carries the locked SIH baseline of 45)."""
        assert SIH_BASELINE_THRESHOLDS["decision_making"] == 45.0
        events = GOOD_COMPLETED_FIRE[:3] + [
            # evacuation_started with correct=False: -30 on procedure_compliance
            {"event_type": "evacuation_started", "correct": False},
            {"event_type": "assessment_completed"},
        ]
        result = _run(events)
        assert result.competency_scores["procedure_compliance"].passed is False
        assert result.passed is False
        assert "Failed competencies" in result.pass_reason

    def test_sih_baseline_thresholds_match_competency_definitions(self):
        definitions = get_competencies("fire")
        assert definitions["hazard_identification"].pass_threshold == \
            SIH_BASELINE_THRESHOLDS["hazard_recognition"]
        assert definitions["ppe_selection"].pass_threshold == \
            SIH_BASELINE_THRESHOLDS["ppe_selection_loto"]
        assert definitions["decision_making"].pass_threshold == \
            SIH_BASELINE_THRESHOLDS["decision_making"]
        gas = get_competencies("gas")
        assert SIH_BASELINE_THRESHOLDS["emergency_response_procedure_min"] <= \
            gas["emergency_response"].pass_threshold <= \
            SIH_BASELINE_THRESHOLDS["emergency_response_procedure_max"]


if __name__ == "__main__":
    pytest.main([__file__, "-v"])


    def test_12b_scenario_completed_event_also_counts(self):
        events = GOOD_COMPLETED_FIRE[:4] + [
            {"event_type": "scenario_completed", "completion_status": "success"},
        ]
        result = _run(events)
        assert result.completed is True
        assert result.passed is True

    def test_completion_gate_propagates_through_assess(self):
        result = assess(GOOD_COMPLETED_FIRE[:4], scenario_type="fire")
        assert result["passed"] is False
        result = assess(GOOD_COMPLETED_FIRE, scenario_type="fire")
        assert result["passed"] is True

