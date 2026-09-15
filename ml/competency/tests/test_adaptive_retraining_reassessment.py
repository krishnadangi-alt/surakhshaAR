"""Integration tests for Day 5 Adaptive Retraining, Reassessment & Contracts."""

import pytest
from ml.competency.scoring.engine import CompetencyScorer
from ml.competency.weakness_detection.detector import WeaknessDetector
from ml.competency.retraining.recommender import RetrainingRecommender
from ml.competency.reassessment.reassessment import evaluate_reassessment
from ml.competency.integration_contracts import (
    format_rehan_handshake,
    format_omesh_backend_payload,
    format_kanishka_dashboard_feed
)


def test_weakness_to_targeted_retraining_mapping():
    """Verify weak competency maps to exact targeted retraining module."""
    # Worker fails PPE selection
    events = [
        {"event_type": "hazard_identified", "correct": True},
        {"event_type": "ppe_selected", "correct": False, "items": []},
        {"event_type": "assessment_completed"}
    ]
    scorer = CompetencyScorer("fire")
    for e in events:
        scorer.process_event(e)
    res = scorer.get_result()

    detector = WeaknessDetector()
    weaknesses = detector.detect_weaknesses(res)
    assert len(weaknesses) >= 1
    assert "ppe_selection" in [w.competency_name for w in weaknesses]

    recommender = RetrainingRecommender("fire")
    plan = recommender.get_retraining_plan(weaknesses)
    assert len(plan["recommended_modules"]) >= 1
    mod = plan["recommended_modules"][0]
    assert "ppe_selection" in mod["competencies_addressed"]
    assert "required_practice" in mod
    assert "reassessment_condition" in mod


def test_reassessment_improvement_and_resolution():
    """Verify reassessment loop: baseline FAIL -> retraining -> post-reassessment PASS & resolved weakness."""
    # Baseline attempt 1: Failed PPE selection
    events1 = [
        {"event_type": "hazard_identified", "correct": True},
        {"event_type": "ppe_selected", "correct": False, "items": []},
        {"event_type": "assessment_completed"}
    ]
    scorer1 = CompetencyScorer("fire")
    for e in events1:
        scorer1.process_event(e)
    res1 = scorer1.get_result()
    assert res1.passed is False

    # Post-retraining reassessment attempt 2: Clean PASS
    events2 = [
        {"event_type": "hazard_identified", "correct": True},
        {"event_type": "ppe_selected", "correct": True, "items": ["suit"]},
        {"event_type": "equipment_selected", "correct": True},
        {"event_type": "correct_action", "action": "remove_pin"},
        {"event_type": "evacuation_started", "correct": True},
        {"event_type": "assessment_completed"}
    ]
    scorer2 = CompetencyScorer("fire")
    for e in events2:
        scorer2.process_event(e)
    res2 = scorer2.get_result()
    assert res2.passed is True

    eval_res = evaluate_reassessment(res1, res2, target_weaknesses=["ppe_selection"])
    assert eval_res["overall_improved"] is True
    assert eval_res["score_gain"] > 0
    assert "ppe_selection" in eval_res["resolved_weaknesses"]
    assert len(eval_res["persistent_weaknesses"]) == 0
    assert eval_res["learning_loop_status"] == "COMPLETED_PASS"


def test_integration_contracts_formatting():
    """Verify formatters for Rehan, Omesh, and Kanishka produce valid dictionaries."""
    events = [
        {"event_type": "hazard_identified", "correct": True},
        {"event_type": "assessment_completed"}
    ]
    scorer = CompetencyScorer("machinery")
    for e in events:
        scorer.process_event(e)
    res = scorer.get_result()

    detector = WeaknessDetector()
    profile = detector.analyze_full_profile(res)

    recommender = RetrainingRecommender("machinery")
    plan = recommender.get_retraining_plan(detector.detect_weaknesses(res))

    # Rehan handshake
    rehan_msg = format_rehan_handshake(res, profile, plan)
    assert "assessment_id" in rehan_msg
    assert "recommended_training_modules" in rehan_msg

    # Omesh DB payload
    omesh_payload = format_omesh_backend_payload(101, 3, res, profile, plan)
    assert omesh_payload["worker_id"] == 101
    assert "progress_status" in omesh_payload

    # Kanishka dashboard feed
    dashboard_feed = format_kanishka_dashboard_feed(101, res, profile, plan)
    assert dashboard_feed["worker_id"] == 101
    assert "strong_areas" in dashboard_feed
    assert "weak_areas" in dashboard_feed
