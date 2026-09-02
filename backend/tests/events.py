"""Shared behavioural event fixtures for backend tests.

These mirror the ML competency engine's event contract (see
``ml/competency/scoring/engine.py``) and are submitted through the API the
same way the VR worker app does.

Scoring notes (baseline 50 per competency, thresholds per scenario):
- GOOD_FIRE_EVENTS -> overall 90.0, all competencies pass (decision_making
  stays at its baseline 50 >= threshold 45).
- GOOD_GAS_EVENTS  -> overall 100.0, all competencies pass.
- BAD_FIRE_EVENTS  -> single major wrong_action -> overall 39.0, FAIL.
"""

GOOD_FIRE_EVENTS = [
    {
        "event_type": "hazard_identified",
        "correct": True,
        "hazard_type": "electrical_fire",
    },
    {
        "event_type": "ppe_selected",
        "correct": True,
        "items": ["helmet", "gloves", "jacket"],
    },
    {
        "event_type": "equipment_selected",
        "correct": True,
        "action": "grab_extinguisher",
    },
    {
        "event_type": "evacuation_started",
        "correct": True,
        "route": "north_exit",
    },
]

GOOD_GAS_EVENTS = [
    {
        "event_type": "hazard_identified",
        "correct": True,
        "hazard_type": "gas_leak",
    },
    {
        "event_type": "ppe_selected",
        "correct": True,
        "items": ["respirator", "gloves"],
    },
    {
        "event_type": "equipment_selected",
        "correct": True,
        "action": "gas_detector",
    },
    {
        "event_type": "evacuation_started",
        "correct": True,
        "direction": "upwind",
    },
    {
        "event_type": "emergency_procedure",
        "correct": True,
        "action": "alert_supervisor",
    },
]

BAD_FIRE_EVENTS = [{"event_type": "wrong_action", "severity": "major"}]

CRITICAL_FIRE_EVENTS = GOOD_FIRE_EVENTS + [
    {
        "event_type": "critical_action",
        "action": "opened_door_during_fire",
        "reason": "Opened door during fire - fed oxygen to the flames",
    }
]