using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SurakshaAR.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace SurakshaAR.Core
{
    public enum SyncState
    {
        Offline,
        Syncing,
        Synced,
        Error
    }

    /// <summary>
    /// Opportunistic background sync manager for SurakshaAR.
    /// Operates on the premise of zero-connectivity by default.
    /// When an active network is detected, it pushes queued assessment sessions
    /// with raw behavioural events to the FastAPI backend (/api/v1/sync).
    /// </summary>
    public class OfflineSyncManager : MonoBehaviour
    {
        public static OfflineSyncManager Instance { get; private set; }

        [Header("Backend Configuration")]
        [SerializeField] private string _backendBaseUrl = "http://127.0.0.1:8000";
        [SerializeField] private float _pollIntervalSeconds = 15f;

        public string BackendBaseUrl
        {
            get => _backendBaseUrl;
            set => _backendBaseUrl = value;
        }

        public SyncState CurrentState { get; private set; } = SyncState.Offline;
        public string StatusMessage { get; private set; } = "Local Mode (Zero Connectivity)";
        public bool IsOnline => Application.internetReachability != NetworkReachability.NotReachable;

        public event Action<SyncState, string, int> OnSyncStatusChanged;

        private Coroutine _syncMonitorRoutine;
        private bool _isSyncing = false;

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

        private void Start()
        {
            _syncMonitorRoutine = StartCoroutine(SyncMonitorLoop());
        }

        private void OnDestroy()
        {
            if (_syncMonitorRoutine != null)
            {
                StopCoroutine(_syncMonitorRoutine);
            }
        }

        public void TriggerSync()
        {
            if (!_isSyncing && IsOnline)
            {
                StartCoroutine(PerformSyncRoutine());
            }
            else
            {
                UpdateStatus();
            }
        }

        private IEnumerator SyncMonitorLoop()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(_pollIntervalSeconds);

                if (IsOnline && OfflineDataStore.Instance != null && OfflineDataStore.Instance.PendingSyncCount > 0)
                {
                    yield return PerformSyncRoutine();
                }
                else
                {
                    UpdateStatus();
                }
            }
        }

        private void UpdateStatus()
        {
            int pending = OfflineDataStore.Instance != null ? OfflineDataStore.Instance.PendingSyncCount : 0;
            if (!IsOnline)
            {
                CurrentState = SyncState.Offline;
                StatusMessage = pending > 0 ? $"Offline ({pending} pending sync)" : "Offline (Local Mode)";
            }
            else if (pending > 0)
            {
                CurrentState = SyncState.Syncing;
                StatusMessage = $"Connecting... ({pending} queued)";
            }
            else
            {
                CurrentState = SyncState.Synced;
                StatusMessage = "Cloud Synced ✓";
            }

            OnSyncStatusChanged?.Invoke(CurrentState, StatusMessage, pending);
        }

        private IEnumerator PerformSyncRoutine()
        {
            if (_isSyncing || OfflineDataStore.Instance == null) yield break;

            var store = OfflineDataStore.Instance;
            if (store.PendingSyncCount == 0)
            {
                UpdateStatus();
                yield break;
            }

            _isSyncing = true;
            CurrentState = SyncState.Syncing;
            StatusMessage = "Syncing assessment sessions...";
            OnSyncStatusChanged?.Invoke(CurrentState, StatusMessage, store.PendingSyncCount);

            var pendingList = new List<SyncSessionData>(store.Data.pending_sync_queue);
            int batchCount = pendingList.Count;

            var payload = new SyncCreatePayload
            {
                worker_id = 1,
                device_id = store.Data.device_id,
                sessions = pendingList
            };

            string json = JsonUtility.ToJson(payload);
            string url = $"{_backendBaseUrl.TrimEnd('/')}/api/v1/sync";

            using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 10;

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    store.MarkSessionsSynced(batchCount);
                    CurrentState = SyncState.Synced;
                    StatusMessage = "Cloud Synced ✓";
                    Debug.Log($"[SYNC] Successfully synced {batchCount} sessions to FastAPI backend!");
                }
                else
                {
                    // Backend not reachable or error - preserve offline queue cleanly
                    CurrentState = SyncState.Offline;
                    StatusMessage = $"Offline ({store.PendingSyncCount} stored locally)";
                    Debug.Log($"[SYNC] Backend unavailable ({req.error}). Queued {store.PendingSyncCount} records safely in offline store.");
                }
            }

            _isSyncing = false;
            OnSyncStatusChanged?.Invoke(CurrentState, StatusMessage, store.PendingSyncCount);
        }
    }
}
