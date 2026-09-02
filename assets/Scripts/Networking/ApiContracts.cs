using System;

namespace SurakshaAR.Networking
{
    /// <summary>
    /// Data-transfer objects that exactly match the SurakshaAR backend API contract
    /// (see docs/api/API.md). Field names are intentionally snake_case to mirror the
    /// JSON keys so Unity's JsonUtility can (de)serialize them 1:1. Values are the
    /// fixed contract - change the backend and this file together.
    /// </summary>
    public static class ApiContracts
    {
        /// <summary>Standard error shape returned by the backend.</summary>
        [Serializable]
        public sealed class ApiError
        {
            public string detail;
        }

        // ------------------------------------------------------------------
        // Workers
        // ------------------------------------------------------------------

        [Serializable]
        public sealed class WorkerCreate
        {
            public string name;
            public string employee_id;
            public string role;
        }

        [Serializable]
        public sealed class WorkerOut
        {
            public int id;
            public string name;
            public string employee_id;
            public string role;
            public string created_at;
        }

        // ------------------------------------------------------------------
        // Modules
        // ------------------------------------------------------------------

        [Serializable]
        public sealed class ModuleOut
        {
            public int id;
            public string code;
            public string name;
            public string description;
        }

        [Serializable]
        public sealed class ModuleListOut
        {
            public ModuleOut[] modules;
        }

        // ------------------------------------------------------------------
        // Progress
        // ------------------------------------------------------------------

        [Serializable]
        public sealed class ProgressCreate
        {
            public int worker_id;
            public int module_id;
            public string stage;
            public string status;
        }

        [Serializable]
        public sealed class ProgressOut
        {
            public int worker_id;
            public int module_id;
            public string stage;
            public string status;
            public string updated_at;
        }

        [Serializable]
        public sealed class ProgressItemOut
        {
            public int module_id;
            public string module_code;
            public string module_name;
            public string stage;
            public string status;
            public string last_updated;
        }

        [Serializable]
        public sealed class ProgressListOut
        {
            public int worker_id;
            public ProgressItemOut[] progress;
        }

        // ------------------------------------------------------------------
        // Assessments (event-based; scored server-side by the ML engine)
        // ------------------------------------------------------------------

        /// <summary>
        /// A single behavioural event from the VR/AR session. Only event_type is
        /// contractually required; event-specific fields are forwarded as-is.
        /// JsonUtility always serialises all fields, so unused ones are sent as
        /// empty/false - the backend's AssessmentEvent schema allows extra fields.
        /// </summary>
        [Serializable]
        public sealed class AssessmentEvent
        {
            public string event_type;
            public string timestamp;
            public bool correct;
            public string[] items;
            public string severity;   // wrong_action: minor | major
            public string action;
            public string hazard_type;
            public string reason;     // critical_action
            public string route;      // evacuation_started
            public string direction;  // evacuation_started (gas: upwind)
            public string completion_status; // assessment_completed
        }

        [Serializable]
        public sealed class AssessmentCreate
        {
            public int worker_id;
            public int module_id;
            public string scenario_type; // optional; "" -> derived from module code
            public int attempt_number;   // 0 -> auto-incremented server-side
            public AssessmentEvent[] events;
        }

        [Serializable]
        public sealed class CompetencyScoreOut
        {
            public string name;
            public float score;
            public bool passed;
            public float pass_threshold;
        }

        [Serializable]
        public sealed class WeaknessOut
        {
            public string competency_name;
            public float score;
            public float threshold;
            public string severity; // severe | moderate | mild
            public string reason;
            public string[] affected_aspects;
        }

        [Serializable]
        public sealed class RetrainingModuleOut
        {
            public string module_id;
            public string name;
            public string description;
            public int estimated_duration_minutes;
            public string difficulty_level;
            public string[] competencies_addressed;
            public string reason;
        }

        [Serializable]
        public sealed class RetrainingPlanOut
        {
            public string scenario_type;
            public RetrainingModuleOut[] recommended_modules;
            public int total_estimated_duration_minutes;
            public bool time_limit_exceeded;
            public int weaknesses_addressed;
            public int total_weaknesses;
        }

        /// <summary>
        /// Mirror of the backend's competency_scores JSON *object*. JsonUtility
        /// cannot deserialise dictionaries, but the competency key set is fixed
        /// per scenario (see ml/competency/scoring/config.py), so we model the map
        /// as a flat class. Missing keys deserialise to null.
        /// </summary>
        [Serializable]
        public sealed class CompetencyScoreMap
        {
            // fire + gas
            public CompetencyScoreOut hazard_identification;
            public CompetencyScoreOut ppe_selection;
            public CompetencyScoreOut equipment_use;
            // fire only
            public CompetencyScoreOut procedure_compliance;
            public CompetencyScoreOut decision_making;
            // gas only
            public CompetencyScoreOut evacuation;
            public CompetencyScoreOut emergency_response;

            public int Count()
            {
                int n = 0;
                if (hazard_identification != null) n++;
                if (ppe_selection != null) n++;
                if (equipment_use != null) n++;
                if (procedure_compliance != null) n++;
                if (decision_making != null) n++;
                if (evacuation != null) n++;
                if (emergency_response != null) n++;
                return n;
            }
        }

        [Serializable]
        public sealed class AssessmentOut
        {
            public int id;
            public int worker_id;
            public int module_id;
            public int attempt_number;
            public string scenario_type;
            public float score;
            public bool passed;
            public string pass_reason;
            public WeaknessOut[] weaknesses;
            public CompetencyScoreMap competency_scores;
            public string[] critical_errors;
            public string created_at;
        }

        [Serializable]
        public sealed class AssessmentHistoryOut
        {
            public int worker_id;
            public AssessmentOut[] assessments;
        }
        // ------------------------------------------------------------------
        // Sync (offline session data)
        // ------------------------------------------------------------------

        [Serializable]
        public sealed class SyncSession
        {
            public string type;
            public int module_id;
            public float score;
            public bool passed;
            public string[] weaknesses;
            public string occurred_at;
        }

        [Serializable]
        public sealed class SyncCreate
        {
            public int worker_id;
            public string device_id;
            public SyncSession[] sessions;
        }

        [Serializable]
        public sealed class SyncOut
        {
            public int sync_id;
            public int worker_id;
            public string synced_at;
            public int sessions_synced;
        }

        [Serializable]
        public sealed class SyncStatusOut
        {
            public int worker_id;
            public string last_synced_at;
            public int pending_sessions;
        }

        // ------------------------------------------------------------------
        // Certificates
        // ------------------------------------------------------------------

        [Serializable]
        public sealed class CertificateCreate
        {
            public int worker_id;
            public int module_id;
        }

        [Serializable]
        public sealed class CertificateOut
        {
            public int id;
            public string certificate_number;
            public int worker_id;
            public int module_id;
            public string issued_at;
            public string valid_until;
            public string status;
        }

        [Serializable]
        public sealed class CertificateListOut
        {
            public int worker_id;
            public CertificateOut[] certificates;
        }

        [Serializable]
        public sealed class CertificateVerifyOut
        {
            public string certificate_number;
            public bool valid;
            public string worker_name;
            public string module_name;
            public string issued_at;
            public string valid_until;
            public string status;
        }

        // ------------------------------------------------------------------
        // Vision / ML (docs/api/API.md #18, #19)
        // ------------------------------------------------------------------

        [Serializable]
        public sealed class PPECheckRequest
        {
            public string image_base64;
            public string[] required_ppe;
            public string mode; // "auto" (default) | "mock" | "model"
        }

        [Serializable]
        public sealed class PPEDetectionOut
        {
            public string item;
            public float confidence;
        }

        [Serializable]
        public sealed class PPECheckOut
        {
            public string status; // ok | low_confidence | model_error | disabled
            public bool ppe_ok;
            public PPEDetectionOut[] detections;
            public string[] missing_items;
            public float confidence;
            public bool fallback_used;
            public string message;
            public string[] required_ppe;
        }

        [Serializable]
        public sealed class VisionStatusOut
        {
            public string status;
            public string module;
            public string version;
            public string mode;
            public string model_path;
            public bool model_loaded;
            public bool fallback_enabled;
            public string[] supported_items;
        }
    }
}
