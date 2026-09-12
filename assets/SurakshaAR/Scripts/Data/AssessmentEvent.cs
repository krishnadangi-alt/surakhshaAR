using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurakshaAR.Data
{
    /// <summary>
    /// Event model representing a single behavioural action from AR or UI assessment.
    /// Strictly matches backend/ml event contract:
    /// training_started, assessment_started, hazard_identified, ppe_selected,
    /// equipment_selected, wrong_action, critical_action, evacuation_started, assessment_completed.
    /// </summary>
    [Serializable]
    public class AssessmentEvent
    {
        public string event_type;
        public string timestamp;

        // Contextual fields (forwarded to Python ML competency engine)
        public string action;
        public string hazard_type;
        public bool correct = true;
        public string ppe_type;
        public string equipment_type;
        public string severity; // "minor", "major", "critical"
        public string reason;
        public string route;
        public bool safe = true;
        public float response_time_seconds;
        public float duration_seconds;

        public AssessmentEvent()
        {
            timestamp = DateTime.UtcNow.ToString("o");
        }

        public AssessmentEvent(string eventType) : this()
        {
            event_type = eventType;
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
        public List<AssessmentEvent> events = new List<AssessmentEvent>();
    }

    [Serializable]
    public class SyncCreatePayload
    {
        public int worker_id = 1;
        public string device_id;
        public List<SyncSessionData> sessions = new List<SyncSessionData>();
    }
}
