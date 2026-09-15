using System.Collections;
using UnityEngine;
using SurakshaAR.Networking;

namespace SurakshaAR.Networking
{
    /// <summary>
    /// Example MonoBehaviour that demonstrates the full worker-app integration:
    /// health check -> worker registration -> progress -> event-based assessment
    /// (scored server-side by the ML competency engine) -> retraining plan ->
    /// vision PPE check -> offline-session sync.
    ///
    /// Attach it to the same GameObject that has SurakshaApiClient (or it will
    /// add one automatically).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BackendSyncExample : MonoBehaviour
    {
        [Header("Worker")]
        [SerializeField] private string workerName = "Ramesh Kumar";
        [SerializeField] private string employeeId = "EMP001";
        [SerializeField] private string role = "Fire Safety Worker";
        [SerializeField]
        [Tooltip("Used when the worker already exists (POST /workers returns 409).")]
        private int existingWorkerId = 0;

        [Header("Module / Scenario")]
        [Tooltip("1 = fire, 2 = gas")]
        [SerializeField] private int moduleId = 1;

        [Header("Assessment")]
        [Tooltip("false = submit a failing assessment (critical error) so the retraining plan is populated.")]
        [SerializeField] private bool passScenario = true;

        [Header("Vision (PPE check)")]
        [SerializeField] private bool runPpeCheck = true;

        [Header("Sync")]
        [SerializeField] private string deviceId = "device-xyz-789";

        // A minimal valid PNG (1x1) used purely to exercise the PPE check
        // pipeline end-to-end in mock mode (no camera capture in this example).
        private const string SamplePngBase64 =
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk" +
            "+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

        private SurakshaApiClient api;
        private int workerId;

        private void Start()
        {
            api = SurakshaApiClient.Instance;
            if (api == null)
            {
                api = gameObject.AddComponent<SurakshaApiClient>();
            }

            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            // 1) Backend reachable?
            bool healthy = false;
            yield return api.CheckHealth(result =>
            {
                if (!result.Success)
                {
                    Debug.LogError("[SurakshaAR] Backend unreachable: " + result.Error);
                    return;
                }

                healthy = true;
                Debug.Log("[SurakshaAR] Backend health: OK");
            });

            if (!healthy)
            {
                yield break;
            }

            // 2) Register the worker (fall back to existingWorkerId on duplicate).
            bool hasWorker = false;
            var create = new ApiContracts.WorkerCreate
            {
                name = workerName,
                employee_id = employeeId,
                role = role,
            };

            yield return api.CreateWorker(create, result =>
            {
                if (result.Success)
                {
                    workerId = result.Data.id;
                    hasWorker = true;
                    Debug.Log("[SurakshaAR] Registered worker #" + workerId);
                }
                else if (result.StatusCode == 409 && existingWorkerId > 0)
                {
                    workerId = existingWorkerId;
                    hasWorker = true;
                    Debug.Log("[SurakshaAR] Worker already exists, using id " + workerId);
                }
                else
                {
                    Debug.LogError("[SurakshaAR] Worker registration failed: " + result.Error);
                }
            });

            if (!hasWorker)
            {
                yield break;
            }

            // 3) Record progress for the training workflow stage.
            var progress = new ApiContracts.ProgressCreate
            {
                worker_id = workerId,
                module_id = moduleId,
                stage = "assess",
                status = "in_progress",
            };

            yield return api.UpdateProgress(progress, result =>
            {
                if (result.Success)
                {
                    Debug.Log("[SurakshaAR] Progress updated: " + result.Data.stage + " / " + result.Data.status);
                }
                else
                {
                    Debug.LogError("[SurakshaAR] Progress update failed: " + result.Error);
                }
            });

            // 4) Submit the event-based assessment (scored server-side by the ML engine).
            int assessmentId = 0;
            var assessment = new ApiContracts.AssessmentCreate
            {
                worker_id = workerId,
                module_id = moduleId,
                events = BuildScenarioEvents(passScenario),
            };

            yield return api.SubmitAssessment(assessment, result =>
            {
                if (result.Success)
                {
                    assessmentId = result.Data.id;
                    Debug.Log("[SurakshaAR] Assessment #" + assessmentId + " score=" +
                              result.Data.score.ToString("0.0") + " passed=" + result.Data.passed);
                    Debug.Log("[SurakshaAR] Pass reason: " + result.Data.pass_reason);
                    LogCompetencyScores(result.Data.competency_scores);
                    LogWeaknesses(result.Data.weaknesses);
                }
                else
                {
                    Debug.LogError("[SurakshaAR] Assessment submission failed: " + result.Error);
                }
            });

            // 5) Fetch the targeted retraining plan (empty for a passing run).
            if (assessmentId > 0)
            {
                yield return api.GetRetrainingPlan(assessmentId, result =>
                {
                    if (result.Success)
                    {
                        Debug.Log("[SurakshaAR] Retraining plan: " +
                                  result.Data.weaknesses_addressed + "/" + result.Data.total_weaknesses +
                                  " weaknesses addressed in " + result.Data.total_estimated_duration_minutes + " min");
                        foreach (var module in result.Data.recommended_modules)
                        {
                            Debug.Log("[SurakshaAR]   -> " + module.name + " (" + module.estimated_duration_minutes + " min) " + module.reason);
                        }
                    }
                    else
                    {
                        Debug.LogError("[SurakshaAR] Retraining plan fetch failed: " + result.Error);
                    }
                });
            }

            // 6) Vision: PPE detection check (mock fallback until a model exists).
            if (runPpeCheck)
            {
                var ppe = new ApiContracts.PPECheckRequest
                {
                    image_base64 = SamplePngBase64,
                    required_ppe = new[] { "helmet", "safety_vest" },
                    mode = "auto",
                };

                yield return api.CheckPPE(ppe, result =>
                {
                    if (result.Success)
                    {
                        Debug.Log("[SurakshaAR] PPE check: status=" + result.Data.status +
                                  " ppe_ok=" + result.Data.ppe_ok +
                                  " fallback_used=" + result.Data.fallback_used);
                    }
                    else
                    {
                        Debug.LogError("[SurakshaAR] PPE check failed: " + result.Error);
                    }
                });
            }

            // 7) Sync an offline session to the backend.
            var sync = new ApiContracts.SyncCreate
            {
                worker_id = workerId,
                device_id = deviceId,
                sessions = new[]
                {
                    new ApiContracts.SyncSession
                    {
                        type = "assessment",
                        module_id = moduleId,
                        score = passScenario ? 90.0f : 39.0f,
                        passed = passScenario,
                        weaknesses = new string[0],
                        occurred_at = System.DateTime.UtcNow.ToString("o"),
                    },
                },
            };

            yield return api.SyncSessions(sync, result =>
            {
                if (result.Success)
                {
                    Debug.Log("[SurakshaAR] Synced " + result.Data.sessions_synced + " offline session(s)");
                }
                else
                {
                    Debug.LogError("[SurakshaAR] Sync failed: " + result.Error);
                }
            });

            Debug.Log("[SurakshaAR] Integration demo finished.");
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Build a minimal, well-formed scenario. The passing run covers all
        /// competencies; the failing run appends a critical safety violation so
        /// the server fails the assessment regardless of the other scores.
        /// </summary>
        private ApiContracts.AssessmentEvent[] BuildScenarioEvents(bool pass)
        {
            return new[]
            {
                AssessmentEvents.HazardIdentified(true, "electrical_fire"),
                AssessmentEvents.PpeSelected(true, "helmet", "gloves", "jacket"),
                AssessmentEvents.EquipmentSelected(true, "grab_extinguisher"),
                AssessmentEvents.EvacuationStarted(true, "north_exit"),
                // Append the safety violation for the failing run.
                pass
                    ? new ApiContracts.AssessmentEvent { event_type = "assessment_completed", completion_status = "success" }
                    : AssessmentEvents.CriticalAction("re_entered_unsafe_area", "CRITICAL: Re-entered the fire zone without clearance"),
            };
        }

        private static void LogCompetencyScores(ApiContracts.CompetencyScoreMap map)
        {
            if (map == null)
            {
                return;
            }

            LogScore(map.hazard_identification);
            LogScore(map.ppe_selection);
            LogScore(map.equipment_use);
            LogScore(map.procedure_compliance);
            LogScore(map.decision_making);
            LogScore(map.evacuation);
            LogScore(map.emergency_response);
        }

        private static void LogScore(ApiContracts.CompetencyScoreOut score)
        {
            if (score != null)
            {
                Debug.Log("[SurakshaAR]   competency " + score.name + ": " +
                          score.score.ToString("0.0") + " (pass=" + score.passed + ")");
            }
        }

        private static void LogWeaknesses(ApiContracts.WeaknessOut[] weaknesses)
        {
            if (weaknesses == null || weaknesses.Length == 0)
            {
                Debug.Log("[SurakshaAR]   weaknesses: none");
                return;
            }

            foreach (var w in weaknesses)
            {
                Debug.Log("[SurakshaAR]   weakness " + w.competency_name + ": " +
                          w.score.ToString("0.0") + " (" + w.severity + ") " + w.reason);
            }
        }
    }
}