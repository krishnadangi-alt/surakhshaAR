"""
Integration tests verifying the ML competency engine works with backend-style events.

These tests ensure the assess() function and underlying engine can process
the same event format that the backend API receives from the Unity worker app.
"""

import pytest
from ml.competency.assess import assess


class TestBackendEventFormatIntegration:
    """Integration tests verifying the competency engine works with backend-style events."""

    def test_assess_with_backend_good_fire_events(self):
        """assess() should process backend-format fire events and return a pass."""
        events = [
            {"event_type": "hazard_identified", "correct": True, "hazard_type": "electrical_fire"},
            {"event_type": "ppe_selected", "correct": True, "items": ["helmet", "gloves", "jacket"]},
            {"event_type": "equipment_selected", "correct": True, "action": "grab_extinguisher"},
            {"event_type": "evacuation_started", "correct": True, "route": "north_exit"},
        ]
        result = assess(events, scenario_type="fire")
        assert result["passed"] is True
        assert result["competency_status"] == "competent"
        assert result["score"] >= 70.0
        assert result["weaknesses"] == []
        assert result["retraining"] == []

    def test_assess_with_backend_bad_fire_events(self):
        """assess() should process a wrong_action event and return a fail."""
        events = [{"event_type": "wrong_action", "severity": "major"}]
        result = assess(events, scenario_type="fire")
        assert result["passed"] is False
        assert result["competency_status"] == "not_competent"
        assert len(result["weaknesses"]) > 0
        assert len(result["retraining"]) == len(result["weaknesses"])

    def test_assess_with_backend_critical_action(self):
        """assess() should trigger automatic FAIL on critical_action event."""
        events = [
            {"event_type": "hazard_identified", "correct": True, "hazard_type": "electrical_fire"},
            {"event_type": "critical_action", "action": "opened_door_during_fire", "reason": "Fed oxygen to flames"},
        ]
        result = assess(events, scenario_type="fire")
        assert result["passed"] is False
        assert result["competency_status"] == "not_competent"


    def test_assess_handles_response_time_seconds_gracefully(self):
        """assess() should handle events with response_time_seconds without errors.

        The API contract includes response_time_seconds as an optional field.
        The existing scoring rules do not define response-time-based scoring,
        so this field should be safely ignored (handled according to existing rules).
        """
        events = [
            {
                "event_type": "hazard_identified",
                "correct": True,
                "hazard_type": "electrical_fire",
                "response_time_seconds": 12.5,  # Slow response
            },
            {
                "event_type": "ppe_selected",
                "correct": True,
                "items": ["helmet", "gloves"],
                "response_time_seconds": 8.0,
            },
            {"event_type": "equipment_selected", "correct": True, "action": "grab_extinguisher"},
            {"event_type": "evacuation_started", "correct": True, "route": "north_exit"},
        ]
        # Should not raise an exception; response_time is not scored by existing rules
        result = assess(events, scenario_type="fire")
        assert result["passed"] is True
        assert result["score"] >= 70.0

    def test_assess_with_full_backend_assessment_payload(self):
        """assess() should handle a complete realistic fire assessment event sequence."""
        events = [
            {"event_type": "assessment_started", "timestamp": "2026-09-04T10:00:00Z", "scenario": "fire_storage_area"},
            {"event_type": "hazard_identified", "timestamp": "2026-09-04T10:01:15Z", "correct": True, "hazard_type": "fire_source"},
            {"event_type": "hazard_identified", "timestamp": "2026-09-04T10:02:30Z", "correct": True, "hazard_type": "fuel_source"},
            {"event_type": "ppe_selected", "timestamp": "2026-09-04T10:03:45Z", "correct": True, "items": ["fire_resistant_suit", "heat_resistant_gloves", "safety_helmet", "face_shield"]},
            {"event_type": "equipment_selected", "timestamp": "2026-09-04T10:05:00Z", "correct": True, "equipment": "fire_extinguisher_class_d"},
            {"event_type": "evacuation_started", "timestamp": "2026-09-04T10:08:30Z", "correct": True, "evacuees_assisted": 2},
            {"event_type": "assessment_completed", "timestamp": "2026-09-04T10:10:00Z", "completion_status": "success"},
        ]
        result = assess(events, scenario_type="fire")
        assert result["passed"] is True
        assert result["competency_status"] == "competent"
        assert result["score"] >= 70.0
        assert result["weaknesses"] == []

    def test_assess_weak_competency_area_with_retraining(self):
        """assess() should detect weak competency and provide targeted retraining."""
        events = [
            {"event_type": "assessment_started", "timestamp": "2026-09-04T11:00:00Z"},
            {"event_type": "hazard_identified", "timestamp": "2026-09-04T11:01:15Z", "correct": True, "hazard_type": "fire_source"},
            {"event_type": "ppe_selected", "timestamp": "2026-09-04T11:02:30Z", "correct": False, "items": []},
            {"event_type": "equipment_selected", "timestamp": "2026-09-04T11:03:45Z", "correct": True, "equipment": "fire_extinguisher_class_d"},
            {"event_type": "evacuation_started", "timestamp": "2026-09-04T11:05:00Z", "correct": True},
            {"event_type": "assessment_completed", "timestamp": "2026-09-04T11:06:15Z"},
        ]
        result = assess(events, scenario_type="fire")
        assert result["passed"] is False
        assert result["competency_status"] == "not_competent"
        assert "ppe_selection" in result["weaknesses"]
        assert "ppe_selection" in result["retraining"]
        assert len(result["retraining"]) == len(result["weaknesses"])

    def test_assess_with_minor_wrong_action(self):
        """assess() should handle minor wrong_action with smaller score penalty."""
        events = [
            {"event_type": "hazard_identified", "correct": True, "hazard_type": "electrical_fire"},
            {"event_type": "ppe_selected", "correct": True, "items": ["helmet", "gloves", "jacket"]},
            {"event_type": "equipment_selected", "correct": True, "action": "grab_extinguisher"},
            {"event_type": "wrong_action", "severity": "minor", "action": "approached_at_wrong_angle"},
            {"event_type": "evacuation_started", "correct": True, "route": "north_exit"},
        ]
        result = assess(events, scenario_type="fire")
        # Minor wrong action should still allow a pass
        assert result["passed"] is True
        assert result["score"] >= 70.0


if __name__ == "__main__":
    pytest.main([__file__, "-v"])