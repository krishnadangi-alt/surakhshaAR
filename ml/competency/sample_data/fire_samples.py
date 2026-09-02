"""Sample assessment data for Fire & Explosion Response scenarios."""

import json
from datetime import datetime, timedelta


# Sample Fire Assessment - Good Performance (PASS)
FIRE_ASSESSMENT_GOOD = {
    "assessment_id": "fire_assess_001_good",
    "scenario_type": "fire",
    "worker_id": "worker_42",
    "start_time": "2026-09-02T08:00:00Z",
    "scenario_name": "Fire in Storage Area",
    "description": "Worker discovers active fire in chemical storage area and must respond safely",
    "events": [
        {
            "event_type": "assessment_started",
            "timestamp": "2026-09-02T08:00:00Z",
            "scenario": "fire_storage_area"
        },
        {
            "event_type": "hazard_identified",
            "timestamp": "2026-09-02T08:01:15Z",
            "correct": True,
            "hazard_type": "fire_source",
            "description": "Correctly identified active fire in storage area"
        },
        {
            "event_type": "hazard_identified",
            "timestamp": "2026-09-02T08:02:30Z",
            "correct": True,
            "hazard_type": "fuel_source",
            "description": "Identified chemical storage as fuel source for fire"
        },
        {
            "event_type": "ppe_selected",
            "timestamp": "2026-09-02T08:03:45Z",
            "correct": True,
            "items": [
                "fire_resistant_suit",
                "heat_resistant_gloves",
                "safety_helmet",
                "face_shield"
            ],
            "description": "Selected appropriate fire-resistant PPE"
        },
        {
            "event_type": "equipment_selected",
            "timestamp": "2026-09-02T08:05:00Z",
            "correct": True,
            "equipment": "fire_extinguisher_class_d",
            "description": "Selected correct fire extinguisher for chemical fire"
        },
        {
            "event_type": "wrong_action",
            "timestamp": "2026-09-02T08:06:15Z",
            "severity": "minor",
            "action": "approached_fire_too_directly",
            "description": "Approached fire at slightly wrong angle but recovered"
        },
        {
            "event_type": "evacuation_started",
            "timestamp": "2026-09-02T08:08:30Z",
            "correct": True,
            "description": "Correctly initiated evacuation procedures",
            "evacuees_assisted": 2
        },
        {
            "event_type": "assessment_completed",
            "timestamp": "2026-09-02T08:10:00Z",
            "completion_status": "success"
        }
    ]
}


# Sample Fire Assessment - Poor Performance (FAIL)
FIRE_ASSESSMENT_POOR = {
    "assessment_id": "fire_assess_001_poor",
    "scenario_type": "fire",
    "worker_id": "worker_43",
    "start_time": "2026-09-02T09:00:00Z",
    "scenario_name": "Fire in Storage Area",
    "description": "Worker discovers active fire in chemical storage area and must respond safely",
    "events": [
        {
            "event_type": "assessment_started",
            "timestamp": "2026-09-02T09:00:00Z",
            "scenario": "fire_storage_area"
        },
        {
            "event_type": "hazard_identified",
            "timestamp": "2026-09-02T09:01:00Z",
            "correct": False,
            "hazard_type": "fire_source",
            "description": "Failed to recognize fire hazard initially"
        },
        {
            "event_type": "ppe_selected",
            "timestamp": "2026-09-02T09:02:00Z",
            "correct": False,
            "items": [
                "hard_hat",
                "safety_glasses"
            ],
            "description": "Selected inadequate PPE - no fire-resistant protection"
        },
        {
            "event_type": "wrong_action",
            "timestamp": "2026-09-02T09:03:00Z",
            "severity": "major",
            "action": "operated_equipment_incorrectly",
            "description": "Attempted to operate equipment before proper assessment"
        },
        {
            "event_type": "wrong_action",
            "timestamp": "2026-09-02T09:04:00Z",
            "severity": "major",
            "action": "ignored_evacuation_order",
            "description": "Did not follow evacuation procedures when instructed"
        },
        {
            "event_type": "assessment_completed",
            "timestamp": "2026-09-02T09:05:00Z",
            "completion_status": "failure"
        }
    ]
}


# Sample Fire Assessment - Critical Error (FAIL regardless of score)
FIRE_ASSESSMENT_CRITICAL_ERROR = {
    "assessment_id": "fire_assess_001_critical",
    "scenario_type": "fire",
    "worker_id": "worker_44",
    "start_time": "2026-09-02T10:00:00Z",
    "scenario_name": "Fire in Storage Area",
    "description": "Worker discovers active fire in chemical storage area and must respond safely",
    "events": [
        {
            "event_type": "assessment_started",
            "timestamp": "2026-09-02T10:00:00Z",
            "scenario": "fire_storage_area"
        },
        {
            "event_type": "hazard_identified",
            "timestamp": "2026-09-02T10:01:15Z",
            "correct": True,
            "hazard_type": "fire_source",
            "description": "Correctly identified fire hazard"
        },
        {
            "event_type": "ppe_selected",
            "timestamp": "2026-09-02T10:02:30Z",
            "correct": True,
            "items": [
                "fire_resistant_suit",
                "heat_resistant_gloves",
                "safety_helmet",
                "face_shield"
            ],
            "description": "Selected appropriate PPE"
        },
        {
            "event_type": "critical_action",
            "timestamp": "2026-09-02T10:03:45Z",
            "action": "re_entered_unsafe_area",
            "reason": "CRITICAL: Re-entered severely contaminated area without proper clearance - severe safety violation"
        },
        {
            "event_type": "assessment_completed",
            "timestamp": "2026-09-02T10:05:00Z",
            "completion_status": "failure_critical"
        }
    ]
}


def get_sample_fire_assessment(assessment_type: str = "good"):
    """
    Get sample fire assessment data.
    
    Args:
        assessment_type: "good", "poor", or "critical"
        
    Returns:
        Dict with assessment events
    """
    assessments = {
        "good": FIRE_ASSESSMENT_GOOD,
        "poor": FIRE_ASSESSMENT_POOR,
        "critical": FIRE_ASSESSMENT_CRITICAL_ERROR,
    }
    return assessments.get(assessment_type, FIRE_ASSESSMENT_GOOD)


def get_all_fire_samples():
    """Get all fire sample assessments."""
    return {
        "good": FIRE_ASSESSMENT_GOOD,
        "poor": FIRE_ASSESSMENT_POOR,
        "critical": FIRE_ASSESSMENT_CRITICAL_ERROR,
    }


if __name__ == "__main__":
    # Example: Print sample assessments
    samples = get_all_fire_samples()
    for name, assessment in samples.items():
        print(f"\n{name.upper()} - {assessment['assessment_id']}")
        print(f"Events: {len(assessment['events'])}")
        print(json.dumps(assessment, indent=2))
