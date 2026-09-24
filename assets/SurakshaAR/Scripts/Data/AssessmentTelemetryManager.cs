using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SurakshaAR.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace SurakshaAR.Data
{
    /// <summary>
    /// Central manager for recording assessment events during AR and quiz workflows.
    /// Hooks into TrainingEventManager to log behavioural telemetry in real-time.
    /// Evaluates results offline via CompetencyEngine and feeds OfflineDataStore and OfflineSyncManager.
    /// Dispatches live events to the backend (/api/v1/events) when online.
    /// </summary>
    public class AssessmentTelemetryManager : MonoBehaviour
    {
        public static AssessmentTelemetryManager Instance { get; private set; }

        public bool IsSessionActive { get; private set; }
        public string CurrentSessionId { get; private set; } = "";
        public string CurrentScenarioType { get; private set; } = "fire";
        public float SessionStartTime { get; private set; }

        private readonly List<AssessmentEvent> _currentEvents = new List<AssessmentEvent>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);

            SubscribeTrainingEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeTrainingEvents();
        }

        private void SubscribeTrainingEvents()
        {
            // Legacy / Scenario events
            TrainingEventManager.OnTrainingStarted += HandleTrainingStarted;
            TrainingEventManager.OnHazardIdentified += HandleHazardIdentified;
            TrainingEventManager.OnAlarmActivated += HandleAlarmActivated;
            TrainingEventManager.OnExtinguisherPickedUp += HandleExtinguisherPicked;
            TrainingEventManager.OnPinRemoved += HandlePinRemoved;
            TrainingEventManager.OnExtinguisherUsed += HandleExtinguisherUsed;
            TrainingEventManager.OnFireExtinguished += HandleFireExtinguished;
            TrainingEventManager.OnTrainingCompleted += HandleTrainingCompleted;

            TrainingEventManager.OnWrongAction += HandleWrongAction;
            TrainingEventManager.OnCriticalAction += HandleCriticalAction;
            TrainingEventManager.OnEvacuationStarted += HandleEvacuation;
            TrainingEventManager.OnPpeSelected += HandlePpeSelected;
            TrainingEventManager.OnEquipmentSelected += HandleEquipmentSelected;

            // ── 12 Common Assessment Events (Day 1 Standard) ──
            TrainingEventManager.OnScenarioStartedCommon += HandleCommonScenarioStarted;
            TrainingEventManager.OnHazardIdentifiedCommon += HandleCommonHazardIdentified;
            TrainingEventManager.OnPpeSelectedCommon += HandleCommonPpeSelected;
            TrainingEventManager.OnEquipmentSelectedCommon += HandleCommonEquipmentSelected;
            TrainingEventManager.OnObjectInteractionCommon += HandleCommonObjectInteraction;
            TrainingEventManager.OnCorrectActionCommon += HandleCommonCorrectAction;
            TrainingEventManager.OnUnsafeActionCommon += HandleCommonUnsafeAction;
            TrainingEventManager.OnSequenceErrorCommon += HandleCommonSequenceError;
            TrainingEventManager.OnResponseTimeCommon += HandleCommonResponseTime;
            TrainingEventManager.OnScenarioCompletedCommon += HandleCommonScenarioCompleted;
        }

        private void UnsubscribeTrainingEvents()
        {
            TrainingEventManager.OnTrainingStarted -= HandleTrainingStarted;
            TrainingEventManager.OnHazardIdentified -= HandleHazardIdentified;
            TrainingEventManager.OnAlarmActivated -= HandleAlarmActivated;
            TrainingEventManager.OnExtinguisherPickedUp -= HandleExtinguisherPicked;
            TrainingEventManager.OnPinRemoved -= HandlePinRemoved;
            TrainingEventManager.OnExtinguisherUsed -= HandleExtinguisherUsed;
            TrainingEventManager.OnFireExtinguished -= HandleFireExtinguished;
            TrainingEventManager.OnTrainingCompleted -= HandleTrainingCompleted;

            TrainingEventManager.OnWrongAction -= HandleWrongAction;
            TrainingEventManager.OnCriticalAction -= HandleCriticalAction;
            TrainingEventManager.OnEvacuationStarted -= HandleEvacuation;
            TrainingEventManager.OnPpeSelected -= HandlePpeSelected;
            TrainingEventManager.OnEquipmentSelected -= HandleEquipmentSelected;

            // ── 12 Common Assessment Events (Day 1 Standard) ──
            TrainingEventManager.OnScenarioStartedCommon -= HandleCommonScenarioStarted;
            TrainingEventManager.OnHazardIdentifiedCommon -= HandleCommonHazardIdentified;
            TrainingEventManager.OnPpeSelectedCommon -= HandleCommonPpeSelected;
            TrainingEventManager.OnEquipmentSelectedCommon -= HandleCommonEquipmentSelected;
            TrainingEventManager.OnObjectInteractionCommon -= HandleCommonObjectInteraction;
            TrainingEventManager.OnCorrectActionCommon -= HandleCommonCorrectAction;
            TrainingEventManager.OnUnsafeActionCommon -= HandleCommonUnsafeAction;
            TrainingEventManager.OnSequenceErrorCommon -= HandleCommonSequenceError;
            TrainingEventManager.OnResponseTimeCommon -= HandleCommonResponseTime;
            TrainingEventManager.OnScenarioCompletedCommon -= HandleCommonScenarioCompleted;
        }

        public void StartSession(string scenarioType = "fire")
        {
            CurrentScenarioType = scenarioType;
            CurrentSessionId = $"sess_{Guid.NewGuid():N}".Substring(0, 16);
            _currentEvents.Clear();
            IsSessionActive = true;
            SessionStartTime = Time.time;

            LogEvent(new AssessmentEvent("assessment_started")
            {
                action = "start_assessment",
                hazard_type = scenarioType
            });
            Debug.Log($"[TELEMETRY] Assessment session started: {CurrentSessionId} for scenario: {scenarioType}");
        }

        public void LogEvent(AssessmentEvent ev)
        {
            if (ev == null) return;
            if (!IsSessionActive)
            {
                StartSession(CurrentScenarioType);
            }
            _currentEvents.Add(ev);
            Debug.Log($"[TELEMETRY] Logged event '{ev.event_type}' (Total in session: {_currentEvents.Count})");

            TryDispatchLiveEvent(ev);
        }

        private void TryDispatchLiveEvent(AssessmentEvent ev)
        {
            if (OfflineSyncManager.Instance != null && OfflineSyncManager.Instance.IsOnline)
            {
                StartCoroutine(DispatchLiveEventRoutine(ev));
            }
        }

        private IEnumerator DispatchLiveEventRoutine(AssessmentEvent ev)
        {
            string baseUrl = OfflineSyncManager.Instance != null ? OfflineSyncManager.Instance.BackendBaseUrl : "http://127.0.0.1:8000";
            string endpoint = $"{baseUrl.TrimEnd('/')}/api/v1/events";

            string json = BuildEventJson(ev);
            using (var req = new UnityWebRequest(endpoint, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 5;

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[TELEMETRY-LIVE] Dispatched '{ev.event_type}' to backend -> 201 Created");
                }
                else
                {
                    Debug.LogWarning($"[TELEMETRY-LIVE] Event '{ev.event_type}' dispatch returned: {req.error}");
                }
            }
        }

        private string BuildEventJson(AssessmentEvent ev)
        {
            int workerId = 1;
            int moduleId = CurrentScenarioType == "fire" ? 1 : CurrentScenarioType == "gas" ? 2 : 3;

            string actionStr = ev.action ?? "";
            string reasonStr = ev.reason ?? "";
            string hazardStr = ev.hazard_type ?? "";
            string equipStr = ev.equipment_type ?? "";
            string ppeStr = ev.ppe_type ?? "";
            string sevStr = string.IsNullOrEmpty(ev.severity) ? "info" : ev.severity;

            return $"{{\"event_type\":\"{ev.event_type}\",\"session_id\":\"{CurrentSessionId}\",\"worker_id\":{workerId},\"module_id\":{moduleId},\"scenario_type\":\"{CurrentScenarioType}\",\"severity\":\"{sevStr}\",\"payload\":{{\"action\":\"{actionStr}\",\"correct\":{(ev.correct ? "true" : "false")},\"reason\":\"{reasonStr}\",\"hazard_type\":\"{hazardStr}\",\"equipment_type\":\"{equipStr}\",\"ppe_type\":\"{ppeStr}\",\"duration\":{ev.duration_seconds.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)}}}}}";
        }

        public AssessmentResultData CompleteSession()
        {
            float duration = Time.time - SessionStartTime;
            LogEvent(new AssessmentEvent("assessment_completed")
            {
                duration_seconds = duration
            });

            IsSessionActive = false;

            // Offline deterministic scoring via CompetencyEngine
            var result = CompetencyEngine.Evaluate(CurrentScenarioType, _currentEvents, duration);

            // Sync with AppState
            if (AppState.Instance != null)
            {
                AppState.Instance.AssessmentScore = Mathf.RoundToInt(result.overall_score);
                AppState.Instance.CriticalErrorsCount = result.critical_errors.Count;
                AppState.Instance.LastARTimerSeconds = duration;
                AppState.Instance.NotifyChange();
            }

            // Save to offline storage & enqueue for backend sync
            if (OfflineDataStore.Instance != null)
            {
                OfflineDataStore.Instance.SaveAssessmentSession(CurrentScenarioType, result, _currentEvents);
            }

            Debug.Log($"[TELEMETRY] Session finalized. Overall: {result.overall_score:F1}%, Passed: {result.passed}, Critical: {result.critical_errors.Count}");
            return result;
        }

        // ── Event Handlers ────────────────────────────────────────────────
        private void HandleTrainingStarted()
        {
            StartSession(CurrentScenarioType);
        }

        private void HandleHazardIdentified()
        {
            LogEvent(new AssessmentEvent("hazard_identified")
            {
                correct = true,
                hazard_type = CurrentScenarioType,
                action = "identify_hazard"
            });
        }

        private void HandleAlarmActivated()
        {
            LogEvent(new AssessmentEvent("emergency_procedure")
            {
                correct = true,
                action = "activate_alarm"
            });
        }

        private void HandleExtinguisherPicked()
        {
            LogEvent(new AssessmentEvent("equipment_selected")
            {
                correct = true,
                equipment_type = "co2_extinguisher",
                action = "pick_extinguisher"
            });
        }

        private void HandlePinRemoved()
        {
            LogEvent(new AssessmentEvent("equipment_selected")
            {
                correct = true,
                action = "remove_safety_pin"
            });
        }

        private void HandleExtinguisherUsed()
        {
            LogEvent(new AssessmentEvent("equipment_selected")
            {
                correct = true,
                action = "discharge_spray_at_base"
            });
        }

        private void HandleFireExtinguished()
        {
            LogEvent(new AssessmentEvent("equipment_selected")
            {
                correct = true,
                action = "fire_suppressed"
            });
        }

        private void HandleTrainingCompleted()
        {
            CompleteSession();
        }

        private void HandleWrongAction(string action, string severity, string reason)
        {
            LogEvent(new AssessmentEvent("wrong_action")
            {
                action = action,
                severity = severity,
                reason = reason,
                correct = false
            });
        }

        private void HandleCriticalAction(string action, string reason)
        {
            LogEvent(new AssessmentEvent("critical_action")
            {
                action = action,
                reason = reason,
                correct = false
            });
        }

        private void HandleEvacuation(string route, bool safe)
        {
            LogEvent(new AssessmentEvent("evacuation_started")
            {
                route = route,
                safe = safe,
                action = "evacuate_exit"
            });
        }

        private void HandlePpeSelected(string ppeType, bool correct)
        {
            LogEvent(new AssessmentEvent("ppe_selected")
            {
                ppe_type = ppeType,
                correct = correct,
                action = "select_ppe"
            });
        }

        private void HandleEquipmentSelected(string equipmentType, bool correct)
        {
            LogEvent(new AssessmentEvent("equipment_selected")
            {
                equipment_type = equipmentType,
                correct = correct,
                action = "select_equipment"
            });
        }

        // ── Day 1 Common Assessment Event Handlers ───────────────────────
        private int GetCurrentWorkerId()
        {
            if (AppState.Instance != null && !string.IsNullOrEmpty(AppState.Instance.EmployeeId))
            {
                string digits = System.Text.RegularExpressions.Regex.Replace(AppState.Instance.EmployeeId, @"[^\d]", "");
                if (int.TryParse(digits, out int id))
                {
                    return id;
                }
            }
            return 1;
        }

        private void HandleCommonScenarioStarted(string module, string scenario)
        {
            StartSession(module);
            var ev = AssessmentEvent.Create(CommonAssessmentEvents.SCENARIO_STARTED, "scenario_start", "info");
            ev.worker_id = GetCurrentWorkerId();
            ev.module = module;
            ev.scenario = scenario;
            LogEvent(ev);
        }

        private void HandleCommonHazardIdentified(string hazardType, float responseTime)
        {
            var ev = AssessmentEvent.CreateHazardIdentified(hazardType, responseTime);
            ev.worker_id = GetCurrentWorkerId();
            ev.module = CurrentScenarioType;
            LogEvent(ev);
        }

        private void HandleCommonPpeSelected(string ppeType, bool correct, float responseTime)
        {
            var ev = AssessmentEvent.CreatePpeSelected(ppeType, correct, responseTime);
            ev.worker_id = GetCurrentWorkerId();
            ev.module = CurrentScenarioType;
            LogEvent(ev);
        }

        private void HandleCommonEquipmentSelected(string equipType, bool correct, float responseTime)
        {
            var ev = AssessmentEvent.CreateEquipmentSelected(equipType, correct, responseTime);
            ev.worker_id = GetCurrentWorkerId();
            ev.module = CurrentScenarioType;
            LogEvent(ev);
        }

        private void HandleCommonObjectInteraction(string objectId, string interactionType, float responseTime)
        {
            var ev = AssessmentEvent.CreateObjectInteraction(objectId, interactionType, responseTime);
            ev.worker_id = GetCurrentWorkerId();
            ev.module = CurrentScenarioType;
            LogEvent(ev);
        }

        private void HandleCommonCorrectAction(string action, float responseTime)
        {
            var ev = AssessmentEvent.CreateCorrectAction(action, responseTime);
            ev.worker_id = GetCurrentWorkerId();
            ev.module = CurrentScenarioType;
            LogEvent(ev);
        }

        private void HandleCommonUnsafeAction(string action, string reason)
        {
            var ev = AssessmentEvent.CreateUnsafeAction(action, reason);
            ev.worker_id = GetCurrentWorkerId();
            ev.module = CurrentScenarioType;
            LogEvent(ev);
        }

        private void HandleCommonSequenceError(string expectedAction, string actualAction)
        {
            var ev = AssessmentEvent.CreateSequenceError(expectedAction, actualAction);
            ev.worker_id = GetCurrentWorkerId();
            ev.module = CurrentScenarioType;
            LogEvent(ev);
        }

        private void HandleCommonResponseTime(string action, float seconds)
        {
            var ev = AssessmentEvent.CreateResponseTime(action, seconds);
            ev.worker_id = GetCurrentWorkerId();
            ev.module = CurrentScenarioType;
            LogEvent(ev);
        }

        private void HandleCommonScenarioCompleted(string module, float score, bool passed)
        {
            var ev = AssessmentEvent.Create(CommonAssessmentEvents.SCENARIO_COMPLETED, "complete_scenario", passed ? "correct" : "wrong");
            ev.worker_id = GetCurrentWorkerId();
            ev.module = module;
            LogEvent(ev);
            CompleteSession();
        }
    }
}
