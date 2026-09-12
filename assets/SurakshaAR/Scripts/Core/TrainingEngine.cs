using System;
using System.Collections.Generic;
using UnityEngine;
using SurakshaAR.Data;

namespace SurakshaAR.Core
{
    /// <summary>
    /// TrainingEngine
    /// ==============
    /// Core state machine and progress gate for SurakshaAR.
    /// Implements the standardized 5-stage instructional hierarchy:
    ///   1. Introduction   - Context, safety briefing, hazard orientation
    ///   2. Instruction    - Standard Operating Procedures (SOPs) & safety rules
    ///   3. Demonstration  - 3D AR guided animation showing proper execution
    ///   4. GuidedPractice - Interactive drill with step-by-step hints and audio assists
    ///   5. FreePractice   - Independent drill with error feedback and retries
    /// 
    /// Followed by the formal gating pipeline:
    ///   --> Readiness Check (Knowledge & physical drill check)
    ///   --> Assessment Engine (No hints, no retries, critical error instant-fail)
    /// </summary>
    public class TrainingEngine : MonoBehaviour
    {
        public static TrainingEngine Instance { get; private set; }

        public enum TrainingStage
        {
            Introduction,
            Instruction,
            Demonstration,
            GuidedPractice,
            FreePractice,
            ReadinessCheck,
            Assessment,
            Completed
        }

        [Serializable]
        public class WorkerModuleProgress
        {
            public int worker_id;
            public string module_id;
            public TrainingStage current_stage = TrainingStage.Introduction;
            public bool introduction_completed;
            public bool instruction_completed;
            public bool demonstration_completed;
            public bool guided_practice_completed;
            public bool free_practice_completed;
            public bool readiness_check_passed;
            public float readiness_score;
            public string last_updated;
        }

        public const float READINESS_PASS_THRESHOLD = 80.0f;

        public static event Action<TrainingStage> OnTrainingStageChanged;
        public static event Action<int, string, float> OnReadinessCheckPassed;
        public static event Action<int, string, string> OnReadinessCheckFailed;
        public static event Action<int, string> OnAssessmentUnlocked;

        [Header("Runtime State")]
        public TrainingStage activeStage = TrainingStage.Introduction;
        public string activeModuleId = "fire";

        private readonly Dictionary<string, WorkerModuleProgress> _progressCache = new Dictionary<string, WorkerModuleProgress>();

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

        private string GetCacheKey(int workerId, string moduleId) => $"{workerId}_{moduleId}";

        public WorkerModuleProgress GetProgress(int workerId, string moduleId)
        {
            string key = GetCacheKey(workerId, moduleId);
            if (!_progressCache.TryGetValue(key, out var prog))
            {
                // Attempt load from persistent storage
                string jsonKey = $"TRAINING_PROG_{key}";
                string saved = PlayerPrefs.GetString(jsonKey, "");
                if (!string.IsNullOrEmpty(saved))
                {
                    try
                    {
                        prog = JsonUtility.FromJson<WorkerModuleProgress>(saved);
                    }
                    catch
                    {
                        prog = null;
                    }
                }

                if (prog == null)
                {
                    prog = new WorkerModuleProgress
                    {
                        worker_id = workerId,
                        module_id = moduleId,
                        current_stage = TrainingStage.Introduction,
                        last_updated = DateTime.UtcNow.ToString("o")
                    };
                }
                _progressCache[key] = prog;
            }
            return prog;
        }

        private void SaveProgress(WorkerModuleProgress prog)
        {
            if (prog == null) return;
            string key = GetCacheKey(prog.worker_id, prog.module_id);
            _progressCache[key] = prog;
            prog.last_updated = DateTime.UtcNow.ToString("o");
            string jsonKey = $"TRAINING_PROG_{key}";
            PlayerPrefs.SetString(jsonKey, JsonUtility.ToJson(prog));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Advances the worker through the 5 training stages.
        /// </summary>
        public void AdvanceStage(int workerId, string moduleId, TrainingStage completedStage)
        {
            var prog = GetProgress(workerId, moduleId);
            switch (completedStage)
            {
                case TrainingStage.Introduction:
                    prog.introduction_completed = true;
                    prog.current_stage = TrainingStage.Instruction;
                    break;
                case TrainingStage.Instruction:
                    prog.instruction_completed = true;
                    prog.current_stage = TrainingStage.Demonstration;
                    break;
                case TrainingStage.Demonstration:
                    prog.demonstration_completed = true;
                    prog.current_stage = TrainingStage.GuidedPractice;
                    break;
                case TrainingStage.GuidedPractice:
                    prog.guided_practice_completed = true;
                    prog.current_stage = TrainingStage.FreePractice;
                    break;
                case TrainingStage.FreePractice:
                    prog.free_practice_completed = true;
                    prog.current_stage = TrainingStage.ReadinessCheck;
                    break;
            }
            SaveProgress(prog);
            activeStage = prog.current_stage;
            OnTrainingStageChanged?.Invoke(activeStage);
            Debug.Log($"[TRAINING ENGINE] Worker {workerId} advanced {moduleId} to {activeStage}");
        }

        /// <summary>
        /// Evaluates a readiness check attempt.
        /// If passed, unlocks the Assessment Engine.
        /// </summary>
        public bool SubmitReadinessCheck(int workerId, string moduleId, float score, out string reason)
        {
            var prog = GetProgress(workerId, moduleId);
            prog.readiness_score = score;

            if (score >= READINESS_PASS_THRESHOLD)
            {
                prog.readiness_check_passed = true;
                prog.current_stage = TrainingStage.Assessment;
                SaveProgress(prog);
                reason = $"Readiness check PASSED ({score:F1}% >= {READINESS_PASS_THRESHOLD:F0}%). Assessment unlocked.";
                Debug.Log($"[TRAINING ENGINE] {reason}");
                OnReadinessCheckPassed?.Invoke(workerId, moduleId, score);
                OnAssessmentUnlocked?.Invoke(workerId, moduleId);
                return true;
            }
            else
            {
                prog.readiness_check_passed = false;
                prog.current_stage = TrainingStage.GuidedPractice; // Return to guided drill
                SaveProgress(prog);
                reason = $"Readiness check FAILED ({score:F1}% < {READINESS_PASS_THRESHOLD:F0}%). Remedial practice required.";
                Debug.LogWarning($"[TRAINING ENGINE] {reason}");
                OnReadinessCheckFailed?.Invoke(workerId, moduleId, reason);
                return false;
            }
        }

        /// <summary>
        /// Formal Assessment Gate.
        /// Checks whether the worker has met all prerequisites to attempt the final assessment.
        /// </summary>
        public bool CanAccessAssessment(int workerId, string moduleId, out string gatingReason)
        {
            var prog = GetProgress(workerId, moduleId);

            if (!prog.guided_practice_completed)
            {
                gatingReason = "Prerequisite incomplete: Guided Practice drill must be completed before assessment.";
                return false;
            }

            if (!prog.readiness_check_passed)
            {
                gatingReason = $"Prerequisite incomplete: Readiness Check must be passed with >= {READINESS_PASS_THRESHOLD:F0}% score.";
                return false;
            }

            gatingReason = "Eligible for formal assessment.";
            return true;
        }

        /// <summary>
        /// Transitions into the Assessment Engine.
        /// </summary>
        public bool LaunchAssessment(int workerId, string moduleId)
        {
            if (!CanAccessAssessment(workerId, moduleId, out string reason))
            {
                Debug.LogError($"[TRAINING ENGINE] Access denied to assessment: {reason}");
                return false;
            }

            activeStage = TrainingStage.Assessment;
            activeModuleId = moduleId;

            if (AssessmentTelemetryManager.Instance != null)
            {
                AssessmentTelemetryManager.Instance.StartSession(moduleId);
            }

            TrainingEventManager.RaiseScenarioStarted(moduleId, $"{moduleId}_assessment_drill");
            Debug.Log($"[TRAINING ENGINE] Assessment started for Worker {workerId} in Module '{moduleId}'");
            return true;
        }
    }
}
