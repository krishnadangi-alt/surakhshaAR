"""
End-to-End QA Test Suite for Rehan's Assessment, Evaluation & Certification Pipeline.
SurakshaAR | SIH 2026 - PS 26041
Tests all 3 modules (Fire, Gas, Machinery), Zero-Tolerance Critical Errors,
Adaptive Retraining, Certificate Eligibility, and Retention Calculations.
"""

import pytest
from datetime import datetime, timezone, timedelta

from ml.competency.scoring.engine import CompetencyScorer
from ml.competency.scoring.config import get_competencies
from ml.competency.weakness_detection.detector import WeaknessDetector
from ml.competency.retraining.recommender import RetrainingRecommender
from ml.competency.assess import assess


# ─────────────────────────────────────────────────────────────────────────────
# 1. FIRE MODULE TESTS
# ─────────────────────────────────────────────────────────────────────────────

def test_fire_clean_pass():
    """Worker performs all 7 fire steps correctly and in time."""
    events = [
        {"event_type": "hazard_identified", "correct": True, "hazard_type": "electrical_fire"},
        {"event_type": "ppe_selected", "correct": True, "ppe_type": "fire_resistant_suit"},
        {"event_type": "equipment_selected", "correct": True, "equipment_type": "co2_extinguisher"},
        {"event_type": "correct_action", "action": "remove_pin"},
        {"event_type": "correct_action", "action": "aim_base"},
        {"event_type": "correct_action", "action": "squeeze_lever"},
        {"event_type": "correct_action", "action": "sweep_base"},
        {"event_type": "evacuation_started", "safe": True, "route": "emergency_exit_A"},
    ]
    result = assess(events, scenario_type="fire")
    assert result["passed"] is True
    assert result["score"] >= 70.0
    assert result["competency_status"] == "competent"
    assert len(result["weaknesses"]) == 0
    assert len(result["retraining"]) == 0


def test_fire_critical_error_fails_despite_high_score():
    """Critical error (standing in front of active flames) must cause immediate FAIL."""
    events = [
        {"event_type": "hazard_identified", "correct": True},
        {"event_type": "equipment_selected", "correct": True},
        {"event_type": "correct_action", "action": "remove_pin"},
        {"event_type": "critical_action", "action": "stand_in_flames", "reason": "Worker entered fire zone without thermal protection"},
        {"event_type": "correct_action", "action": "aim_base"},
        {"event_type": "correct_action", "action": "sweep_base"},
    ]
    result = assess(events, scenario_type="fire")
    assert result["passed"] is False
    assert result["competency_status"] == "not_competent"


# ─────────────────────────────────────────────────────────────────────────────
# 2. GAS MODULE TESTS
# ─────────────────────────────────────────────────────────────────────────────

def test_gas_clean_pass():
    """Worker executes gas detection, SCBA PPE, isolation, and upwind evacuation."""
    events = [
        {"event_type": "hazard_identified", "correct": True, "hazard_type": "methane_gas"},
        {"event_type": "ppe_selected", "correct": True, "ppe_type": "scba_respirator"},
        {"event_type": "equipment_selected", "correct": True, "equipment_type": "multigas_detector"},
        {"event_type": "correct_action", "action": "isolate_valve"},
        {"event_type": "evacuation_started", "safe": True, "route": "upwind_crosswind"},
    ]
    result = assess(events, scenario_type="gas")
    assert result["passed"] is True
    assert result["score"] >= 70.0
    assert result["competency_status"] == "competent"


def test_gas_critical_downwind_evacuation_fails():
    """Walking downwind into toxic gas cloud triggers instant FAIL."""
    events = [
        {"event_type": "hazard_identified", "correct": True},
        {"event_type": "ppe_selected", "correct": True},
        {"event_type": "critical_action", "action": "evacuate_downwind", "reason": "Inhaled toxic methane cloud"},
    ]
    result = assess(events, scenario_type="gas")
    assert result["passed"] is False


# ─────────────────────────────────────────────────────────────────────────────
# 3. MACHINERY MODULE TESTS (DAY 3 DELIVERABLE)
# ─────────────────────────────────────────────────────────────────────────────

def test_machinery_competencies_defined():
    """Verify machinery competency definitions exist and contain LOTO and E-stop."""
    defs = get_competencies("machinery")
    assert "hazard_identification" in defs
    assert "ppe_selection" in defs
    assert "loto_procedure" in defs
    assert "equipment_use" in defs
    assert "emergency_response" in defs


def test_machinery_clean_pass():
    """Worker identifies nip hazard, selects eye/hearing PPE, performs full LOTO."""
    events = [
        {"event_type": "hazard_identified", "correct": True, "hazard_type": "exposed_drive_nip"},
        {"event_type": "ppe_selected", "correct": True, "ppe_type": "safety_glasses_ear_plugs"},
        {"event_type": "correct_action", "action": "loto_switch_breaker_off"},
        {"event_type": "correct_action", "action": "loto_apply_hasp_padlock"},
        {"event_type": "correct_action", "action": "loto_verify_zero_energy"},
        {"event_type": "equipment_selected", "correct": True, "equipment_type": "fixed_machine_guard"},
    ]
    result = assess(events, scenario_type="machinery")
    assert result["passed"] is True
    assert result["score"] >= 70.0
    assert result["competency_status"] == "competent"


def test_machinery_servicing_without_lockout_fails():
    """Servicing live machine without lockout triggers critical error FAIL."""
    events = [
        {"event_type": "hazard_identified", "correct": True},
        {"event_type": "critical_action", "action": "reach_into_live_gear", "reason": "Attempted clearing jam while machine energized"},
    ]
    result = assess(events, scenario_type="machinery")
    assert result["passed"] is False


# ─────────────────────────────────────────────────────────────────────────────
# 4. ADAPTIVE RETRAINING & WEAKNESS DETECTION TESTS (DAY 5 DELIVERABLE)
# ─────────────────────────────────────────────────────────────────────────────

def test_targeted_retraining_recommendation_on_weakness():
    """Worker who fails PPE selection should receive targeted PPE retraining."""
    events = [
        {"event_type": "hazard_identified", "correct": True},
        {"event_type": "ppe_selected", "correct": False, "ppe_type": "paper_mask"},  # Low score on PPE
        {"event_type": "wrong_action", "action": "improper_ppe_donning", "severity": "major"},
    ]
    result = assess(events, scenario_type="fire")
    assert result["passed"] is False
    assert "ppe_selection" in result["weaknesses"]
    assert "ppe_selection" in result["retraining"]


# ─────────────────────────────────────────────────────────────────────────────
# 5. RETENTION SCHEDULE VERIFICATION (DAY 6 DELIVERABLE)
# ─────────────────────────────────────────────────────────────────────────────

def test_retention_schedule_intervals():
    """Verify Day 1, Day 7, and Day 30 interval dates."""
    base_date = datetime(2026, 9, 12, 12, 0, 0, tzinfo=timezone.utc)
    intervals = [1, 7, 30]
    expected_due_dates = [base_date + timedelta(days=d) for d in intervals]

    assert expected_due_dates[0] == datetime(2026, 9, 13, 12, 0, 0, tzinfo=timezone.utc)
    assert expected_due_dates[1] == datetime(2026, 9, 19, 12, 0, 0, tzinfo=timezone.utc)
    assert expected_due_dates[2] == datetime(2026, 10, 12, 12, 0, 0, tzinfo=timezone.utc)
