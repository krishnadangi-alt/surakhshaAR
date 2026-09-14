using System;
using System.Collections.Generic;
using UnityEngine;
using SurakshaAR.Data;

namespace SurakshaAR.Core
{
    /// <summary>
    /// RetrainingManager
    /// =================
    /// Coordinates the adaptive retraining and reassessment lifecycle.
    /// Satisfies Rehan's Day 5 deliverable:
    ///   - Triggers when an assessment FAILS or competency weaknesses are identified.
    ///   - Binds directly with Harshita's Weakness Detection and ML recommendation engine.
    ///   - Enforces remedial drill completion before permitting reassessment.
    ///   - Computes before-and-after score deltas to verify skill remediation.
    /// </summary>
    public class RetrainingManager : MonoBehaviour
    {
        public static RetrainingManager Instance { get; private set; }

        [Serializable]
        public class ActiveRetrainingAssignment
        {
            public int worker_id;
            public string module_id;
            public string original_assessment_date;
            public float original_score;
            public List<string> identified_weaknesses = new List<string>();
            public List<RetrainingModuleData> assigned_drills = new List<RetrainingModuleData>();
            public bool all_drills_completed;
            public bool reassessment_eligible;
            public float reassessment_score;
            public bool reassessment_passed;
        }

        public static event Action<ActiveRetrainingAssignment> OnRetrainingAssigned;
        public static event Action<int, string, string> OnDrillCompleted;
        public static event Action<int, string, float, bool> OnReassessmentCompleted;

        private readonly Dictionary<string, ActiveRetrainingAssignment> _assignments = new Dictionary<string, ActiveRetrainingAssignment>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private string GetKey(int workerId, string moduleId) => $"{workerId}_{moduleId}";

        /// <summary>
        /// Triggered when an assessment fails or weaknesses are detected.
        /// </summary>
        public ActiveRetrainingAssignment TriggerRetraining(int workerId, string moduleId, AssessmentResultData result)
        {
            var assignment = new ActiveRetrainingAssignment
            {
                worker_id = workerId,
                module_id = moduleId,
                original_assessment_date = DateTime.UtcNow.ToString("o"),
                original_score = result.overall_score,
                assigned_drills = result.recommended_retraining ?? new List<RetrainingModuleData>()
            };

            if (result.weaknesses != null)
            {
                foreach (var w in result.weaknesses)
                {
                    assignment.identified_weaknesses.Add(w.competency_name);
                }
            }

            _assignments[GetKey(workerId, moduleId)] = assignment;
            SaveAssignment(assignment);

            Debug.Log($"[RETRAINING] Assigned {assignment.assigned_drills.Count} remedial drills to Worker {workerId} for module '{moduleId}'");
            OnRetrainingAssigned?.Invoke(assignment);
            return assignment;
        }

        public ActiveRetrainingAssignment GetAssignment(int workerId, string moduleId)
        {
            string key = GetKey(workerId, moduleId);
            if (!_assignments.TryGetValue(key, out var assignment))
            {
                string prefKey = $"RETRAIN_{key}";
                string json = PlayerPrefs.GetString(prefKey, "");
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        assignment = JsonUtility.FromJson<ActiveRetrainingAssignment>(json);
                        _assignments[key] = assignment;
                    }
                    catch
                    {
                        assignment = null;
                    }
                }
            }
            return assignment;
        }

        private void SaveAssignment(ActiveRetrainingAssignment assignment)
        {
            if (assignment == null) return;
            string key = GetKey(assignment.worker_id, assignment.module_id);
            _assignments[key] = assignment;
            string prefKey = $"RETRAIN_{key}";
            PlayerPrefs.SetString(prefKey, JsonUtility.ToJson(assignment));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Called when the worker finishes an assigned targeted practice drill.
        /// </summary>
        public void CompleteDrill(int workerId, string moduleId, string drillId)
        {
            var assignment = GetAssignment(workerId, moduleId);
            if (assignment == null) return;

            // Remove or mark drill complete
            assignment.assigned_drills.RemoveAll(d => d.module_id == drillId);

            if (assignment.assigned_drills.Count == 0)
            {
                assignment.all_drills_completed = true;
                assignment.reassessment_eligible = true;
                Debug.Log($"[RETRAINING] Worker {workerId} has cleared all remedial drills! Reassessment unlocked.");
            }

            SaveAssignment(assignment);
            OnDrillCompleted?.Invoke(workerId, moduleId, drillId);
        }

        /// <summary>
        /// Gating check: Can the worker attempt the reassessment?
        /// </summary>
        public bool CanAttemptReassessment(int workerId, string moduleId, out string reason)
        {
            var assignment = GetAssignment(workerId, moduleId);
            if (assignment == null)
            {
                reason = "No active retraining assignment found.";
                return true;
            }

            if (!assignment.all_drills_completed)
            {
                reason = $"Incomplete retraining: Worker has {assignment.assigned_drills.Count} remaining remedial drill(s) to finish.";
                return false;
            }

            reason = "Eligible for reassessment.";
            return true;
        }

        /// <summary>
        /// Records the reassessment result and computes improvement metrics.
        /// </summary>
        public void FinalizeReassessment(int workerId, string moduleId, AssessmentResultData newResult)
        {
            var assignment = GetAssignment(workerId, moduleId);
            if (assignment != null)
            {
                assignment.reassessment_score = newResult.overall_score;
                assignment.reassessment_passed = newResult.passed;
                SaveAssignment(assignment);
            }

            float delta = (assignment != null) ? (newResult.overall_score - assignment.original_score) : 0f;
            Debug.Log($"[RETRAINING REASSESSMENT] Worker {workerId} '{moduleId}': New Score={newResult.overall_score:F1}%, Passed={newResult.passed}, Delta={delta:+0.0;-0.0}%");

            // Record into attempt history
            if (AssessmentAttemptTracker.Instance != null)
            {
                AssessmentAttemptTracker.Instance.RecordAttempt(workerId, moduleId, newResult.overall_score, newResult.passed, newResult.critical_errors.Count);
            }

            OnReassessmentCompleted?.Invoke(workerId, moduleId, newResult.overall_score, newResult.passed);
        }
    }
}
