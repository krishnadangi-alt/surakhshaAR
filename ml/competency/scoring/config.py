"""
Scoring configuration and competency definitions.

IMPORTANT: These are prototype/demo values and must be validated against
official industrial SOPs and domain experts before production deployment.
"""

from dataclasses import dataclass, field
from typing import Dict, List


@dataclass
class CompetencyDefinition:
    """Definition of a single competency and its scoring rules."""
    
    name: str
    description: str
    min_score: float = 0.0
    max_score: float = 100.0
    pass_threshold: float = 70.0  # Minimum score to pass
    
    # Sub-competencies or aspects of this competency
    aspects: List[str] = field(default_factory=list)


# ============================================================================
# FIRE & EXPLOSION RESPONSE COMPETENCY DEFINITIONS
# ============================================================================
# These are prototype competency categories for fire/explosion scenarios.
# Production must align with official industrial safety standards.

FIRE_COMPETENCIES = {
    "hazard_identification": CompetencyDefinition(
        name="hazard_identification",
        description="Ability to identify fire/explosion hazards and triggers",
        pass_threshold=75.0,
        aspects=[
            "spot_fire_sources",
            "identify_fuel",
            "identify_ignition_sources",
            "assess_hazard_level"
        ]
    ),
    "ppe_selection": CompetencyDefinition(
        name="ppe_selection",
        description="Correct selection and use of personal protective equipment",
        pass_threshold=80.0,
        aspects=[
            "select_correct_ppe",
            "proper_donning",
            "ppe_completeness",
            "ppe_inspection"
        ]
    ),
    "procedure_compliance": CompetencyDefinition(
        name="procedure_compliance",
        description="Following correct fire response procedures",
        pass_threshold=75.0,
        aspects=[
            "evacuation_steps",
            "alarm_activation",
            "communication",
            "safe_exit_route"
        ]
    ),
    "equipment_use": CompetencyDefinition(
        name="equipment_use",
        description="Correct use of fire safety equipment",
        pass_threshold=75.0,
        aspects=[
            "fire_extinguisher_type",
            "equipment_operation",
            "targeting",
            "technique"
        ]
    ),
    "decision_making": CompetencyDefinition(
        name="decision_making",
        description="Sound safety decisions under pressure",
        pass_threshold=45.0,
        aspects=[
            "threat_assessment",
            "escape_vs_fight_decision",
            "resource_allocation",
            "priority_judgment"
        ]
    ),
}

# ============================================================================
# GAS HAZARD RESPONSE COMPETENCY DEFINITIONS
# ============================================================================

GAS_COMPETENCIES = {
    "hazard_identification": CompetencyDefinition(
        name="hazard_identification",
        description="Ability to detect gas leaks and toxic atmosphere hazards",
        pass_threshold=75.0,
        aspects=[
            "detect_gas_signs",
            "identify_gas_type",
            "assess_concentration",
            "recognize_symptoms"
        ]
    ),
    "ppe_selection": CompetencyDefinition(
        name="ppe_selection",
        description="Correct selection of respiratory protection and PPE",
        pass_threshold=80.0,
        aspects=[
            "select_respirator",
            "proper_fit_test",
            "donning_procedure",
            "seal_verification"
        ]
    ),
    "evacuation": CompetencyDefinition(
        name="evacuation",
        description="Safe evacuation from contaminated area",
        pass_threshold=75.0,
        aspects=[
            "upwind_movement",
            "emergency_exit",
            "assist_others",
            "decontamination"
        ]
    ),
    "equipment_use": CompetencyDefinition(
        name="equipment_use",
        description="Use of detection and ventilation equipment",
        pass_threshold=75.0,
        aspects=[
            "gas_detector_operation",
            "ventilation_setup",
            "monitoring",
            "equipment_maintenance"
        ]
    ),
    "emergency_response": CompetencyDefinition(
        name="emergency_response",
        description="Proper emergency procedures for gas incidents",
        pass_threshold=70.0,
        aspects=[
            "alert_procedures",
            "rescue_coordination",
            "first_aid",
            "incident_reporting"
        ]
    ),
}


# ============================================================================
# SCORING THRESHOLDS
# ============================================================================

# Overall assessment scoring (prototype values)
OVERALL_PASS_THRESHOLD = 70.0  # Minimum average competency score
OVERALL_SCORE_WEIGHT = 0.5     # How much to weight scores in FAIL logic

# Critical errors cause automatic FAIL regardless of score
CRITICAL_ERROR_WEIGHT = 0.5    # How much critical errors factor into FAIL

# Weakness detection thresholds
WEAKNESS_THRESHOLD = 60.0      # Score below this = weakness
SEVERE_WEAKNESS_THRESHOLD = 50.0  # Score below this = severe weakness

# ============================================================================
# DAY 1 — REHAN CONFIRMED RULES (FINAL SCORING & COMPETENCY)
# ============================================================================
# Locked SIH/demo baseline values. These are provisional for production and
# must be re-validated against official industrial SOPs before deployment.

# Event-type penalty ladder (points deducted per affected competency):
# - WRONG_ACTION (minor, non-life-threatening procedural mistake): small penalty.
# - UNSAFE_ACTION (increases hazard exposure, NOT immediately fatal): -20..-25.
#   The exact project-level value is UNSAFE_ACTION_PENALTY (default 22.0);
#   UNSAFE_ACTION_PENALTY_RANGE documents the confirmed (-20 to -25) band.
# - CRITICAL_ACTION (life-threatening/catastrophic): instant automatic FAIL.
UNSAFE_ACTION_PENALTY = 22.0
UNSAFE_ACTION_PENALTY_RANGE = (-25.0, -20.0)
UNSAFE_ACTION_DECISION_PENALTY = 20.0
WRONG_ACTION_MINOR_PROCEDURE_PENALTY = 5.0
WRONG_ACTION_MINOR_DECISION_PENALTY = 3.0
WRONG_ACTION_MAJOR_PROCEDURE_PENALTY = 30.0
WRONG_ACTION_MAJOR_DECISION_PENALTY = 25.0

# Response-time rules (seconds) — applied per event carrying
# ``response_time_seconds`` (see scoring/engine.py):
# - < 3.0s  -> +5% speed bonus on the event's score delta.
# - 3.0-15.0s -> baseline / no change.
# - > 15.0s -> -10% procedural latency penalty on the event's score delta.
# - Machinery E-Stop benchmark: < 2.5s; a late E-Stop reaction is recorded as
#   a delayed-reaction penalty (tracked on the result, score effect applied
#   through the same latency penalty path).
RESPONSE_TIME_FAST_THRESHOLD_SECONDS = 3.0
RESPONSE_TIME_SLOW_THRESHOLD_SECONDS = 15.0
RESPONSE_TIME_FAST_BONUS = 0.05
RESPONSE_TIME_SLOW_PENALTY = 0.10
ESTOP_BENCHMARK_SECONDS = 2.5

# Completion: an assessment without SCENARIO_COMPLETED / required completion
# must NOT pass.
COMPLETION_EVENT_TYPES = ("assessment_completed", "scenario_completed")

# Competency pass thresholds are defined per CompetencyDefinition above and
# mirror the locked SIH/demo baseline below. OVERALL_PASS_THRESHOLD is the
# overall PASS gate (>= 70%).

# Retraining recommendations (prototype severity bands; still used by docs/tests).
RETRAINING_SEVERITY_THRESHOLDS = {
    "urgent": 40.0,      # Critical retraining needed
    "high": 50.0,        # Important retraining
    "medium": 65.0,      # Recommended retraining
}

# ============================================================================
# SIH BASELINE THRESHOLDS (locked demo values, provisional for production)
# ============================================================================
SIH_BASELINE_THRESHOLDS = {
    "overall_pass": 70.0,
    "hazard_recognition": 75.0,
    "ppe_selection_loto": 80.0,
    "emergency_response_procedure_min": 70.0,
    "emergency_response_procedure_max": 75.0,
    "decision_making": 45.0,
}



def get_competencies(scenario_type: str) -> Dict[str, CompetencyDefinition]:
    """Get competency definitions for a scenario type."""
    if scenario_type.lower() == "fire":
        return FIRE_COMPETENCIES
    elif scenario_type.lower() == "gas":
        return GAS_COMPETENCIES
    else:
        raise ValueError(f"Unknown scenario type: {scenario_type}")
