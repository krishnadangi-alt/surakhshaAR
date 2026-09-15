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
# MACHINERY HAZARD & LOTO RESPONSE COMPETENCY DEFINITIONS
# ============================================================================

MACHINERY_COMPETENCIES = {
    "hazard_identification": CompetencyDefinition(
        name="hazard_identification",
        description="Ability to detect industrial machine pinch points, exposed drives, and mechanical hazards",
        pass_threshold=75.0,
        aspects=[
            "identify_pinch_points",
            "spot_unshielded_gears",
            "detect_jam_hazards",
            "recognize_electrical_isolation_state"
        ]
    ),
    "ppe_selection": CompetencyDefinition(
        name="ppe_selection",
        description="Correct selection of mechanical safety PPE and restraint of loose articles",
        pass_threshold=80.0,
        aspects=[
            "select_impact_eyewear",
            "ear_protection",
            "steel_toe_boots",
            "hair_clothing_restraint"
        ]
    ),
    "loto_procedure": CompetencyDefinition(
        name="loto_procedure",
        description="Execution of standard Lockout/Tagout energy isolation procedures",
        pass_threshold=80.0,
        aspects=[
            "isolate_power_switch",
            "apply_lockout_hasp",
            "affix_danger_tag",
            "verify_zero_energy_state"
        ]
    ),
    "equipment_use": CompetencyDefinition(
        name="equipment_use",
        description="Proper operation of machine guards, interlocks, and safety feeding tools",
        pass_threshold=75.0,
        aspects=[
            "inspect_safety_guard",
            "interlock_verification",
            "push_stick_operation",
            "tool_maintenance"
        ]
    ),
    "emergency_response": CompetencyDefinition(
        name="emergency_response",
        description="Immediate emergency response and emergency stop reaction",
        pass_threshold=70.0,
        aspects=[
            "e_stop_operation",
            "immediate_alert",
            "first_aid_entrapment",
            "incident_isolation"
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

# Retraining recommendations
RETRAINING_SEVERITY_THRESHOLDS = {
    "urgent": 40.0,      # Critical retraining needed
    "high": 50.0,        # Important retraining
    "medium": 65.0,      # Recommended retraining
}


def get_competencies(scenario_type: str) -> Dict[str, CompetencyDefinition]:
    """Get competency definitions for a scenario type."""
    st = scenario_type.lower()
    if st == "fire":
        return FIRE_COMPETENCIES
    elif st == "gas":
        return GAS_COMPETENCIES
    elif st == "machinery":
        return MACHINERY_COMPETENCIES
    else:
        raise ValueError(f"Unknown scenario type: {scenario_type}")
