using System;
using System.Collections.Generic;
using System.IO;
using SurakshaAR.Core;
using UnityEngine;

namespace SurakshaAR.Data
{
    [Serializable]
    public class OfflineStorageContainer
    {
        public string worker_id = "";
        public string worker_name = "";
        public string device_id = "";
        public string last_synced_at = "";
        public List<AssessmentResultData> assessments = new List<AssessmentResultData>();
        public List<SyncSessionData> pending_sync_queue = new List<SyncSessionData>();
    }

    /// <summary>
    /// Manages persistent offline storage for SurakshaAR.
    /// Operates completely offline, persisting assessment results, competency evaluations,
    /// and pending sync payloads to Application.persistentDataPath.
    /// </summary>
    public class OfflineDataStore : MonoBehaviour
    {
        public static OfflineDataStore Instance { get; private set; }

        public OfflineStorageContainer Data { get; private set; } = new OfflineStorageContainer();

        public int PendingSyncCount => Data.pending_sync_queue != null ? Data.pending_sync_queue.Count : 0;
        public string LastSyncedAt => Data.last_synced_at;

        private string FilePath => Path.Combine(Application.persistentDataPath, "suraksha_offline_data.json");

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);

            Load();
        }

        public void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    Data = JsonUtility.FromJson<OfflineStorageContainer>(json) ?? new OfflineStorageContainer();
                    Debug.Log($"[OFFLINE STORE] Loaded {Data.assessments.Count} assessment records, {Data.pending_sync_queue.Count} pending syncs.");
                }
                else
                {
                    Data = new OfflineStorageContainer();
                    Data.device_id = SystemInfo.deviceUniqueIdentifier ?? Guid.NewGuid().ToString();
                    Save();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[OFFLINE STORE] Error loading offline data: {ex.Message}");
                Data = new OfflineStorageContainer();
            }
        }

        public void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(Data, true);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[OFFLINE STORE] Error saving offline data: {ex.Message}");
            }
        }

        public void SaveAssessmentSession(string scenarioType, AssessmentResultData result, List<AssessmentEvent> events)
        {
            if (Data == null) Data = new OfflineStorageContainer();

            Data.assessments.Add(result);

            int moduleId = scenarioType.ToLower().Contains("gas") ? 2 : 1;
            string newAttemptId = $"att_{Guid.NewGuid():N}".Substring(0, 16);
            string guestId = (AppState.Instance != null && AppState.Instance.IsGuestMode)
                ? AppState.Instance.EmployeeId
                : null;

            var syncSession = new SyncSessionData
            {
                type = "assessment",
                module_id = moduleId,
                score = result.overall_score,
                passed = result.passed,
                occurred_at = DateTime.UtcNow.ToString("o"),
                scenario_type = scenarioType,
                attempt_number = Data.assessments.Count,
                client_session_id = AssessmentTelemetryManager.Instance != null && !string.IsNullOrEmpty(AssessmentTelemetryManager.Instance.CurrentSessionId)
                    ? AssessmentTelemetryManager.Instance.CurrentSessionId
                    : $"sess_{Guid.NewGuid():N}".Substring(0, 16),
                guest_id = guestId,
                attempt_id = newAttemptId,
                events = new List<AssessmentEvent>(events ?? new List<AssessmentEvent>())
            };

            foreach (var w in result.weaknesses)
            {
                syncSession.weaknesses.Add(w.competency_name);
            }

            Data.pending_sync_queue.Add(syncSession);
            Save();

            // Persist structured relational records to LocalDatabaseService
            if (LocalDatabaseService.Instance != null)
            {
                string workerIdStr = (AuthSession.Instance != null && AuthSession.Instance.WorkerId > 0)
                    ? AuthSession.Instance.WorkerId.ToString()
                    : (!string.IsNullOrEmpty(AppState.Instance?.EmployeeId) ? AppState.Instance.EmployeeId : "GUEST");

                var attemptRecord = new LocalDatabaseService.TrainingAttemptRecord
                {
                    attempt_id = newAttemptId,
                    worker_id = workerIdStr,
                    module_id = moduleId,
                    scenario_type = scenarioType,
                    client_session_id = syncSession.client_session_id,
                    attempt_number = Data.assessments.Count,
                    started_at = DateTime.UtcNow.AddSeconds(-result.duration_seconds).ToString("o"),
                    completed_at = DateTime.UtcNow.ToString("o"),
                    elapsed_seconds = result.duration_seconds,
                    provisional_score = result.overall_score,
                    passed = result.passed,
                    pass_reason = result.passed ? "Passed all competency criteria" : "Failed requirements",
                    timed_out = AppState.Instance != null && AppState.Instance.LastAttemptTimedOut,
                    critical_error_count = result.critical_errors != null ? result.critical_errors.Count : 0,
                    sync_status = "Pending"
                };
                LocalDatabaseService.Instance.RecordAttempt(attemptRecord);

                // Save events to local relational table
                if (events != null && events.Count > 0)
                {
                    var eventRecords = new List<LocalDatabaseService.TrainingEventRecord>();
                    int seq = 1;
                    foreach (var ev in events)
                    {
                        eventRecords.Add(new LocalDatabaseService.TrainingEventRecord
                        {
                            event_id = $"evt_{Guid.NewGuid():N}".Substring(0, 12),
                            attempt_id = attemptRecord.attempt_id,
                            client_session_id = syncSession.client_session_id,
                            sequence_number = seq++,
                            event_type = ev.event_type,
                            action = ev.action,
                            correct = ev.correct,
                            critical = ev.critical,
                            severity = ev.severity,
                            response_time_seconds = ev.response_time_seconds,
                            timestamp = ev.timestamp,
                            score_delta = 0f,
                            payload_json = JsonUtility.ToJson(ev)
                        });
                    }
                    LocalDatabaseService.Instance.RecordEvents(eventRecords);
                }

                // Save competency evaluation record
                var compRecord = new LocalDatabaseService.CompetencyResultRecord
                {
                    attempt_id = attemptRecord.attempt_id,
                    overall_score = result.overall_score,
                    passed = result.passed,
                    competency_scores_json = JsonUtility.ToJson(result),
                    weaknesses_json = JsonUtility.ToJson(syncSession),
                    retraining_json = "[]"
                };
                LocalDatabaseService.Instance.SaveCompetencyResult(compRecord);

                LocalDatabaseService.Instance.EnqueueSync("assessment", attemptRecord.attempt_id, JsonUtility.ToJson(syncSession));
            }

            Debug.Log($"[OFFLINE STORE] Saved session. Pending queue size: {Data.pending_sync_queue.Count}");

            // Opportunistically trigger sync if internet is available
            OfflineSyncManager.Instance?.TriggerSync();
        }

        public void MarkSessionsSynced(int count)
        {
            if (Data.pending_sync_queue.Count <= count)
            {
                Data.pending_sync_queue.Clear();
            }
            else
            {
                Data.pending_sync_queue.RemoveRange(0, count);
            }

            Data.last_synced_at = DateTime.UtcNow.ToString("o");
            Save();
            LocalDatabaseService.Instance?.PurgeSyncedQueue();
            Debug.Log($"[OFFLINE STORE] Marked {count} sessions synced. Remaining pending: {Data.pending_sync_queue.Count}");
        }
    }
}
