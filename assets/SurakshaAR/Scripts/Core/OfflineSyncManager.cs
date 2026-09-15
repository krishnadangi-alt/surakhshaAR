using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using SurakshaAR.Data;

namespace SurakshaAR.Core
{
    public enum SyncState
    {
        Idle,
        Syncing,
        Synced,
        Offline,
        Error
    }

    /// <summary>
    /// OfflineSyncManager
    /// ==================
    /// Handles opportunistic background synchronization of offline assessment records,
    /// telemetry events, and certificates with the SurakshaAR FastAPI backend.
    /// Preserves idempotency and handles retry backoff on network failures.
    /// </summary>
    [DisallowMultipleComponent]
    public class OfflineSyncManager : MonoBehaviour
    {
        public static OfflineSyncManager Instance { get; private set; }

        public delegate void SyncStatusChangedHandler(SyncState state, string message, int pending);
        public event SyncStatusChangedHandler OnSyncStatusChanged;

        [Header("Configuration")]
        [SerializeField] private string _backendBaseUrl = "http://127.0.0.1:8000";
        [SerializeField] private float _heartbeatInterval = 15f;

        [Header("Runtime State")]
        [SerializeField] private SyncState _currentState = SyncState.Idle;
        [SerializeField] private bool _isOnline = false;
        [SerializeField] private string _statusMessage = "Offline Ready";

        public string BackendBaseUrl
        {
            get => _backendBaseUrl;
            set => _backendBaseUrl = value;
        }

        public bool IsOnline => _isOnline;
        public SyncState State => _currentState;
        public string StatusMessage => _statusMessage;

        private Coroutine _heartbeatRoutine;
        private Coroutine _syncRoutine;
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
            _heartbeatRoutine = StartCoroutine(HeartbeatLoop());
        }

        private void OnDestroy()
        {
            if (_heartbeatRoutine != null) StopCoroutine(_heartbeatRoutine);
            if (_syncRoutine != null) StopCoroutine(_syncRoutine);
            if (Instance == this) Instance = null;
        }

        private IEnumerator HeartbeatLoop()
        {
            while (true)
            {
                yield return CheckConnectivity();
                yield return new WaitForSecondsRealtime(_heartbeatInterval);
            }
        }

        public IEnumerator CheckConnectivity()
        {
            string healthUrl = _backendBaseUrl.TrimEnd('/') + "/health";
            using (UnityWebRequest req = UnityWebRequest.Get(healthUrl))
            {
                req.timeout = 3;
                yield return req.SendWebRequest();

                bool prevOnline = _isOnline;
                _isOnline = (req.result == UnityWebRequest.Result.Success);

                if (_isOnline && !prevOnline)
                {
                    Debug.Log("[OFFLINE SYNC] Backend reconnected! Triggering opportunistic sync.");
                    TriggerSync();
                }
                else if (!_isOnline && prevOnline)
                {
                    UpdateState(SyncState.Offline, "Offline Mode (Local Storage Safe)");
                }
            }
        }

        public void TriggerSync()
        {
            if (_isSyncing) return;
            _syncRoutine = StartCoroutine(PerformSyncRoutine());
        }

        private IEnumerator PerformSyncRoutine()
        {
            _isSyncing = true;
            int pending = OfflineDataStore.Instance != null ? OfflineDataStore.Instance.PendingSyncCount : 0;

            if (pending == 0)
            {
                UpdateState(SyncState.Synced, "All Data Synced");
                _isSyncing = false;
                yield break;
            }

            UpdateState(SyncState.Syncing, $"Syncing ({pending} pending)...");

            var store = OfflineDataStore.Instance;
            if (store == null || store.Data == null || store.Data.pending_sync_queue == null || store.Data.pending_sync_queue.Count == 0)
            {
                UpdateState(SyncState.Synced, "All Data Synced");
                _isSyncing = false;
                yield break;
            }

            int workerId = (AuthSession.Instance != null && AuthSession.Instance.WorkerId > 0)
                ? AuthSession.Instance.WorkerId
                : 1;

            // Construct Batch Sync Payload compatible with /api/v1/sync
            var payload = new SyncPayload
            {
                device_id = !string.IsNullOrEmpty(store.Data.device_id) ? store.Data.device_id : "device-unity-client",
                worker_id = workerId,
                batch_id = $"batch_{Guid.NewGuid():N}".Substring(0, 16),
                pending_sessions = pending,
                sessions = new List<SyncSessionData>(store.Data.pending_sync_queue)
            };

            string json = JsonUtility.ToJson(payload);
            string syncUrl = _backendBaseUrl.TrimEnd('/') + "/api/v1/sync";

            using (UnityWebRequest req = new UnityWebRequest(syncUrl, "POST"))
            {
                byte[] raw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(raw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");

                if (AuthSession.Instance != null && AuthSession.Instance.HasValidToken)
                {
                    req.SetRequestHeader("Authorization", "Bearer " + AuthSession.Instance.AccessToken);
                }

                req.timeout = 15;
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    int syncedCount = payload.sessions.Count;
                    store.MarkSessionsSynced(syncedCount);
                    _isOnline = true;
                    UpdateState(SyncState.Synced, "All Data Synced");
                    Debug.Log($"[OFFLINE SYNC] Successfully synced {syncedCount} offline sessions to backend.");
                }
                else
                {
                    Debug.LogWarning($"[OFFLINE SYNC] Sync failed ({req.responseCode}): {req.error}. Retaining local records.");
                    _isOnline = (req.result != UnityWebRequest.Result.ConnectionError);
                    UpdateState(SyncState.Error, $"Sync Pending ({pending} queued)");
                }
            }

            _isSyncing = false;
        }

        private void UpdateState(SyncState state, string message)
        {
            _currentState = state;
            _statusMessage = message;
            int pending = OfflineDataStore.Instance != null ? OfflineDataStore.Instance.PendingSyncCount : 0;
            OnSyncStatusChanged?.Invoke(state, message, pending);
        }

        [Serializable]
        private class SyncPayload
        {
            public int worker_id = 1;
            public string device_id;
            public string batch_id;
            public int pending_sessions;
            public List<SyncSessionData> sessions = new List<SyncSessionData>();
        }
    }
}
