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
                // Day 6: real worker id from the login session. -1 means the
                // account has no linked worker (admin) or we are offline-only;
                // such batches must NOT be posted as another worker's data.
                worker_id = AuthSession.Instance != null ? AuthSession.Instance.WorkerId : -1,
                device_id = store.Data.device_id,
                batch_id = $"batch_{Guid.NewGuid():N}".Substring(0, 16),
                sessions = pendingList
            };

            string json = JsonUtility.ToJson(payload);
            string url = $"{_backendBaseUrl.TrimEnd('/')}/api/v1/sync";

            if (payload.worker_id < 0)
            {
                // Day 6: refuse to attribute offline sessions to a worker we
                // have not authenticated as. Keep them queued locally.
                CurrentState = SyncState.Offline;
                StatusMessage = $"Offline ({store.PendingSyncCount} stored locally - sign in to sync)";
                Debug.Log("[SYNC] No authenticated worker session. Keeping queue local until login.");
                _isSyncing = false;
                OnSyncStatusChanged?.Invoke(CurrentState, StatusMessage, store.PendingSyncCount);
                yield break;
            }

            using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                // Day 6: Bearer-token authentication (wired in Phase 2).
                if (AuthSession.Instance != null)
                    AuthSession.Instance.ApplyAuthHeader(req);
                req.timeout = 10;

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    store.MarkSessionsSynced(batchCount);
                    CurrentState = SyncState.Synced;
                    StatusMessage = "Cloud Synced ✓";
                    Debug.Log($"[SYNC] Successfully synced {batchCount} sessions to FastAPI backend!");
                }
                else if (req.responseCode == 401 || req.responseCode == 403)
                {
                    // Day 6: auth failure is NOT a network failure — keep the
                    // queue intact and surface re-login instead of retrying.
                    CurrentState = SyncState.Error;
                    StatusMessage = "Sync blocked: session expired - please sign in again";
                    Debug.LogWarning($"[SYNC] Auth rejected ({req.responseCode}). Queue preserved; re-login required.");
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
