using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurakshaAR.Data
{
    /// <summary>
    /// Canonical 12 Common Assessment Events standardized for SurakshaAR Day 1.
    /// Used across Unity (Krishna/Anjani/Harshita), Assessment Engine (Rehan), and Backend (Omesh).
    /// </summary>
    public static class CommonAssessmentEvents
    {
        public const string SCENARIO_STARTED    = "SCENARIO_STARTED";
        public const string HAZARD_IDENTIFIED   = "HAZARD_IDENTIFIED";
        public const string PPE_SELECTED        = "PPE_SELECTED";
        public const string EQUIPMENT_SELECTED  = "EQUIPMENT_SELECTED";
        public const string OBJECT_INTERACTION  = "OBJECT_INTERACTION";
        public const string CORRECT_ACTION      = "CORRECT_ACTION";
        public const string WRONG_ACTION        = "WRONG_ACTION";
        public const string UNSAFE_ACTION       = "UNSAFE_ACTION";
        public const string CRITICAL_ACTION     = "CRITICAL_ACTION";
        public const string SEQUENCE_ERROR      = "SEQUENCE_ERROR";
        public const string RESPONSE_TIME       = "RESPONSE_TIME";
        public const string SCENARIO_COMPLETED  = "SCENARIO_COMPLETED";
    }

    /// <summary>
    /// Event model representing a single behavioural action from AR or UI assessment.
    /// Strictly matches the Day 1 schema:
    /// worker_id, module, scenario, event_type, action, result, timestamp, response_time, critical.
    /// </summary>
    [Serializable]
    public class AssessmentEvent
    {
        // ── Day 1 Standard Event Data Fields ─────────────────────────
        public int worker_id = 1;
        public string module = "fire";            // e.g. "fire", "gas", "machinery"
        public string scenario = "fire_drill_01";
        public string event_type;                 // One of CommonAssessmentEvents or legacy lowercase
        public string action = "";                // Action identifier
        public string result = "correct";         // "correct", "wrong", "unsafe", "critical", "info"
        public string timestamp;                  // ISO-8601 UTC
        public float response_time = 0f;          // Elapsed time in seconds for this action
        public bool critical = false;             // True if this is an automatic-fail critical error

        // ── Extended Contextual Fields (Backward compatibility & ML pipeline) ──
        public string hazard_type;
        public bool correct = true;
        public string ppe_type;
        public string equipment_type;
        public string severity = "info";          // "info", "minor", "major", "critical"
        public string reason = "";
        public string route = "";
        public bool safe = true;
        public float response_time_seconds
        {
            get => response_time;
            set => response_time = value;
        }
        public float duration_seconds;

        public AssessmentEvent()
        {
            timestamp = DateTime.UtcNow.ToString("o");
        }

        public AssessmentEvent(string eventType) : this()
        {
            event_type = eventType;
        }

        public AssessmentEvent(string eventType, string actionName, string actionResult, bool isCritical = false, float responseTimeSec = 0f) : this(eventType)
        {
            action = actionName;
            result = actionResult;
            critical = isCritical;
            response_time = responseTimeSec;
            correct = (actionResult == "correct");
            safe = (actionResult != "unsafe" && !isCritical);
            severity = isCritical ? "critical" : (actionResult == "wrong" ? "major" : "info");
        }

        // ── Factory Helpers ──────────────────────────────────────────
        public static AssessmentEvent Create(string eventType, string action, string result, bool critical = false, float responseTime = 0f)
        {
            return new AssessmentEvent(eventType, action, result, critical, responseTime);
        }

        public static AssessmentEvent CreateHazardIdentified(string hazardType, float responseTime = 0f)
        {
            var ev = new AssessmentEvent(CommonAssessmentEvents.HAZARD_IDENTIFIED, "identify_hazard", "correct", false, responseTime);
            ev.hazard_type = hazardType;
            return ev;
        }

        public static AssessmentEvent CreatePpeSelected(string ppeType, bool isCorrect, float responseTime = 0f)
        {
            var ev = new AssessmentEvent(CommonAssessmentEvents.PPE_SELECTED, "select_ppe", isCorrect ? "correct" : "wrong", false, responseTime);
            ev.ppe_type = ppeType;
            ev.correct = isCorrect;
            return ev;
        }

        public static AssessmentEvent CreateEquipmentSelected(string equipmentType, bool isCorrect, float responseTime = 0f)
        {
            var ev = new AssessmentEvent(CommonAssessmentEvents.EQUIPMENT_SELECTED, "select_equipment", isCorrect ? "correct" : "wrong", false, responseTime);
            ev.equipment_type = equipmentType;
            ev.correct = isCorrect;
            return ev;
        }

        public static AssessmentEvent CreateObjectInteraction(string objectId, string interactionType, float responseTime = 0f)
        {
            var ev = new AssessmentEvent(CommonAssessmentEvents.OBJECT_INTERACTION, interactionType, "correct", false, responseTime);
            ev.equipment_type = objectId;
            return ev;
        }

        public static AssessmentEvent CreateCorrectAction(string action, float responseTime = 0f)
        {
            return new AssessmentEvent(CommonAssessmentEvents.CORRECT_ACTION, action, "correct", false, responseTime);
        }

        public static AssessmentEvent CreateWrongAction(string action, string reason, string severity = "minor")
        {
            var ev = new AssessmentEvent(CommonAssessmentEvents.WRONG_ACTION, action, "wrong", false, 0f);
            ev.reason = reason;
            ev.severity = severity;
            ev.correct = false;
            return ev;
        }

        public static AssessmentEvent CreateUnsafeAction(string action, string reason)
        {
            var ev = new AssessmentEvent(CommonAssessmentEvents.UNSAFE_ACTION, action, "unsafe", false, 0f);
            ev.reason = reason;
            ev.severity = "major";
            ev.safe = false;
            ev.correct = false;
            return ev;
        }

        public static AssessmentEvent CreateCriticalAction(string action, string reason)
        {
            var ev = new AssessmentEvent(CommonAssessmentEvents.CRITICAL_ACTION, action, "critical", true, 0f);
            ev.reason = reason;
            ev.severity = "critical";
            ev.safe = false;
            ev.correct = false;
            return ev;
        }

        public static AssessmentEvent CreateSequenceError(string expectedAction, string actualAction)
        {
            var ev = new AssessmentEvent(CommonAssessmentEvents.SEQUENCE_ERROR, actualAction, "wrong", false, 0f);
            ev.reason = $"Sequence violation: expected '{expectedAction}', got '{actualAction}'";
            ev.severity = "major";
            ev.correct = false;
            return ev;
        }

        public static AssessmentEvent CreateResponseTime(string action, float seconds)
        {
            var ev = new AssessmentEvent(CommonAssessmentEvents.RESPONSE_TIME, action, "info", false, seconds);
            ev.duration_seconds = seconds;
            return ev;
        }
    }

    [Serializable]
    public class CompetencyScoreData
    {
        public string name;
        public float score;
        public bool passed;
        public float pass_threshold;
    }

    [Serializable]
    public class WeaknessData
    {
        public string competency_name;
        public float score;
        public float threshold;
        public string severity; // "mild", "moderate", "severe"
        public string reason;
        public List<string> affected_aspects = new List<string>();
    }

    [Serializable]
    public class RetrainingModuleData
    {
        public string module_id;
        public string name;
        public string description;
        public int estimated_duration_minutes;
        public string difficulty_level;
        public List<string> competencies_addressed = new List<string>();
        public string reason;
    }

    [Serializable]
    public class AssessmentResultData
    {
        public string scenario_type;
        public float overall_score;
        public bool passed;
        public string pass_reason;
        public List<string> critical_errors = new List<string>();
        public List<CompetencyScoreData> competency_scores = new List<CompetencyScoreData>();
        public List<WeaknessData> weaknesses = new List<WeaknessData>();
        public List<RetrainingModuleData> recommended_retraining = new List<RetrainingModuleData>();
        public float duration_seconds;
        public string recorded_at;
    }

    [Serializable]
    public class SyncSessionData
    {
        public string type = "assessment";
        public int module_id = 1;
        public float score;
        public bool passed;
        public List<string> weaknesses = new List<string>();
        public string occurred_at;
        public string scenario_type = "fire";
        public int attempt_number = 1;
        public string client_session_id;
        public string guest_id;
        public string attempt_id;
        public List<AssessmentEvent> events = new List<AssessmentEvent>();
    }

    [Serializable]
    public class SyncCreatePayload
    {
        public int worker_id = 1;
        public string guest_id;
        public string device_id;
        public string batch_id;
        public List<SyncSessionData> sessions = new List<SyncSessionData>();
    }
}
