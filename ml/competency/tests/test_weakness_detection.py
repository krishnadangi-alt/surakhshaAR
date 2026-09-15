"""Unit tests for Weakness Detection & Repeated Mistake Tracking (Day 4 Deliverable)."""

import pytest
from ml.competency.scoring.engine import CompetencyScorer
from ml.competency.weakness_detection.detector import WeaknessDetector, Weakness


def test_single_weak_competency_detection():
    """Verify single weakness identification with affected aspects."""
    events = [
        {"event_type": "hazard_identified", "correct": True},
        {"event_type": "ppe_selected", "correct": False, "items": []},  # PPE selection fails
        {"event_type": "equipment_selected", "correct": True},
        {"event_type": "correct_action", "action": "remove_pin"},
        {"event_type": "assessment_completed"}
    ]
    scorer = CompetencyScorer("fire")
    for e in events:
        scorer.process_event(e)
    res = scorer.get_result()

    detector = WeaknessDetector()
    weaknesses = detector.detect_weaknesses(res)
    assert len(weaknesses) == 1
    assert weaknesses[0].competency_name == "ppe_selection"
    assert len(weaknesses[0].affected_aspects) > 0


def test_repeated_mistakes_tracking():
    """Verify persistent weakness detection across attempt history."""
    # Attempt 1: Failed PPE selection
    events1 = [
        {"event_type": "ppe_selected", "correct": False},
        {"event_type": "assessment_completed"}
    ]
    scorer1 = CompetencyScorer("fire")
    for e in events1:
        scorer1.process_event(e)
    res1 = scorer1.get_result()

    # Attempt 2: Failed PPE selection again
    events2 = [
        {"event_type": "ppe_selected", "correct": False},
        {"event_type": "assessment_completed"}
    ]
    scorer2 = CompetencyScorer("fire")
    for e in events2:
        scorer2.process_event(e)
    res2 = scorer2.get_result()

    detector = WeaknessDetector()
    weaknesses = detector.detect_weaknesses(res2, attempt_history=[res1])
    assert len(weaknesses) >= 1
    ppe_w = next(w for w in weaknesses if w.competency_name == "ppe_selection")
    assert ppe_w.is_persistent is True
    assert "PERSISTENT" in ppe_w.reason


def test_improvement_after_retraining_removes_persistent_flag():
    """Verify improvement in subsequent attempt shows non-weak state."""
    # Attempt 1: Weak hazard identification
    events1 = [{"event_type": "hazard_identified", "correct": False}, {"event_type": "assessment_completed"}]
    scorer1 = CompetencyScorer("fire")
    for e in events1:
        scorer1.process_event(e)
    res1 = scorer1.get_result()

    # Attempt 2: Passed hazard identification
    events2 = [
        {"event_type": "hazard_identified", "correct": True},
        {"event_type": "ppe_selected", "correct": True, "items": ["suit"]},
        {"event_type": "equipment_selected", "correct": True},
        {"event_type": "correct_action", "action": "pin"},
        {"event_type": "evacuation_started", "correct": True},
        {"event_type": "assessment_completed"}
    ]
    scorer2 = CompetencyScorer("fire")
    for e in events2:
        scorer2.process_event(e)
    res2 = scorer2.get_result()

    detector = WeaknessDetector()
    profile = detector.analyze_full_profile(res2, attempt_history=[res1])
    assert "hazard_identification" in profile["strong_areas"]
    assert "hazard_identification" not in profile["weak_areas"]
    assert len(profile["repeated_mistakes"]) == 0


def test_structured_profile_output():
    """Verify analyze_full_profile returns complete required dict structure."""
    events = [
        {"event_type": "hazard_identified", "correct": True},
        {"event_type": "assessment_completed"}
    ]
    scorer = CompetencyScorer("gas")
    for e in events:
        scorer.process_event(e)
    res = scorer.get_result()

    detector = WeaknessDetector()
    profile = detector.analyze_full_profile(res)
    assert "competency_status" in profile
    assert "strong_areas" in profile
    assert "weak_areas" in profile
    assert "affected_aspects" in profile
    assert "weakness_reasons" in profile
    assert "repeated_mistakes" in profile
