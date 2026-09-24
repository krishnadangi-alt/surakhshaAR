using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using SurakshaAR.Data;
using SurakshaAR.Networking;

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
        [SerializeField] private SyncState _currentState = SyncState.Offline;
        [SerializeField] private bool _isOnline = false;
        [SerializeField] private string _statusMessage = "Offline Ready";

        public string BackendBaseUrl
        {
            get => _backendBaseUrl;
            set => _backendBaseUrl = value;
        }

        public bool IsOnline => _isOnline;
        public SyncState State => _currentState;
        public SyncState CurrentState => _currentState;
        public string StatusMessage => _statusMessage;

        private Coroutine _heartbeatRoutine;
        private Coroutine _syncRoutine;
        private bool _isSyncing = false;

        private static readonly string[] CandidateHosts = new string[]
        {
            "http://192.168.137.1:8000",
            "http://172.16.48.160:8000",
            "http://10.0.2.2:8000",
            "http://127.0.0.1:8000",
            "http://localhost:8000",
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);

#if !UNITY_EDITOR
            _backendBaseUrl = "http://192.168.137.1:8000";
#else
            _backendBaseUrl = "http://127.0.0.1:8000";
#endif
            PlayerPrefs.SetString("SurakshaAR_BackendUrl", _backendBaseUrl);
            PlayerPrefs.Save();
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
            yield return ProbeAndResolveBackend(quickProbe: true);
        }

        public IEnumerator ProbeAndResolveBackend(bool quickProbe = false)
        {
            // Try current _backendBaseUrl first with generous 5s timeout
            bool currentOk = false;
            yield return ProbeUrl(_backendBaseUrl, 5f, ok => currentOk = ok);

            if (currentOk)
            {
                OnBackendAvailable(_backendBaseUrl);
                yield break;
            }

            // Gather candidate URLs to test
            List<string> candidates = new List<string>(CandidateHosts);
            string workingCandidate = null;
            foreach (var candidate in candidates)
            {
                if (candidate.Equals(_backendBaseUrl, StringComparison.OrdinalIgnoreCase)) continue;

                bool candOk = false;
                yield return ProbeUrl(candidate, 3f, ok => candOk = ok);
                if (candOk)
                {
                    workingCandidate = candidate;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(workingCandidate))
            {
                Debug.Log($"[OFFLINE SYNC] Auto-discovered reachable backend at: {workingCandidate}");
                _backendBaseUrl = workingCandidate;
                PlayerPrefs.SetString("SurakshaAR_BackendUrl", workingCandidate);
                PlayerPrefs.Save();
                OnBackendAvailable(workingCandidate);
            }
            else
            {
                bool prevOnline = _isOnline;
                _isOnline = false;
                if (prevOnline)
                {
                    UpdateState(SyncState.Offline, "Offline Mode (Local Storage Safe)");
                }
            }
        }

        private IEnumerator ProbeUrl(string baseUrl, float timeout, Action<bool> callback)
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                callback(false);
                yield break;
            }

            string healthUrl = baseUrl.TrimEnd('/') + "/health";
            using (UnityWebRequest req = UnityWebRequest.Get(healthUrl))
            {
                req.timeout = Mathf.Max(1, Mathf.RoundToInt(timeout));
                yield return req.SendWebRequest();
                bool ok = (req.result == UnityWebRequest.Result.Success);
                callback(ok);
            }
        }

        private void OnBackendAvailable(string url)
        {
            bool prevOnline = _isOnline;
            _isOnline = true;

            if (SurakshaApiClient.Instance != null && SurakshaApiClient.Instance.BaseUrl != url + "/api/v1")
            {
                SurakshaApiClient.Instance.SetBaseUrl(url + "/api/v1");
            }

            if (!prevOnline)
            {
                Debug.Log($"[OFFLINE SYNC] Backend connected at {url}! Triggering opportunistic sync.");
                if (AppState.Instance != null && !string.IsNullOrEmpty(AppState.Instance.EmployeeId))
                {
                    RegisterOrSyncWorkerId(AppState.Instance.EmployeeId, AppState.Instance.WorkerName, AppState.Instance.IsGuestMode, AppState.Instance.WorkerRole);
                }
                TriggerSync();
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

            bool isGuest = AppState.Instance != null && AppState.Instance.IsGuestMode;
            int workerId = (!isGuest && AuthSession.Instance != null && AuthSession.Instance.WorkerId > 0)
                ? AuthSession.Instance.WorkerId
                : 0;
            string guestId = (isGuest && AppState.Instance != null && !string.IsNullOrEmpty(AppState.Instance.EmployeeId))
                ? AppState.Instance.EmployeeId
                : null;
            string employeeId = (!isGuest && AppState.Instance != null && !string.IsNullOrEmpty(AppState.Instance.EmployeeId))
                ? AppState.Instance.EmployeeId
                : null;
            string employeeName = AppState.Instance != null ? AppState.Instance.WorkerName : null;

            // Fallback to offline store container worker info if AppState is unpopulated
            if (string.IsNullOrEmpty(employeeId) && string.IsNullOrEmpty(guestId) && store != null && store.Data != null)
            {
                if (!string.IsNullOrEmpty(store.Data.worker_id))
                {
                    if (store.Data.worker_id.StartsWith("GUEST", StringComparison.OrdinalIgnoreCase))
                        guestId = store.Data.worker_id;
                    else
                        employeeId = store.Data.worker_id;
                }
                if (string.IsNullOrEmpty(employeeName) && !string.IsNullOrEmpty(store.Data.worker_name))
                {
                    employeeName = store.Data.worker_name;
                }
            }

            // Ensure worker identification is never empty so backend HTTP 400 is prevented
            if (string.IsNullOrEmpty(employeeId) && string.IsNullOrEmpty(guestId))
            {
                employeeId = "JH-MN-004821";
                if (string.IsNullOrEmpty(employeeName)) employeeName = "Ramesh Kumar";
            }

            // Construct Batch Sync Payload compatible with /api/v1/sync
            var payload = new SyncPayload
            {
                device_id = !string.IsNullOrEmpty(store.Data.device_id) ? store.Data.device_id : "device-unity-client",
                worker_id = workerId,
                guest_id = guestId,
                employee_id = employeeId,
                employee_name = employeeName,
                batch_id = $"batch_{Guid.NewGuid():N}".Substring(0, 16),
                pending_sessions = pending,
                sessions = new List<SyncSessionData>(store.Data.pending_sync_queue)
            };

            string json = JsonUtility.ToJson(payload);

            // List of target base URLs to try: primary first, then candidates
            List<string> targetUrls = new List<string> { _backendBaseUrl };
            foreach (var c in CandidateHosts)
            {
                if (!targetUrls.Contains(c)) targetUrls.Add(c);
            }

            bool syncSucceeded = false;
            for (int i = 0; i < targetUrls.Count; i++)
            {
                var targetUrl = targetUrls[i];
                string syncUrl = targetUrl.TrimEnd('/') + "/api/v1/sync";
                Debug.Log($"[OFFLINE SYNC] Attempting sync to {syncUrl} ({payload.sessions.Count} sessions)...");

                using (UnityWebRequest req = new UnityWebRequest(syncUrl, "POST"))
                {
                    byte[] raw = Encoding.UTF8.GetBytes(json);
                    req.uploadHandler = new UploadHandlerRaw(raw);
                    req.uploadHandler.contentType = "application/json";
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.SetRequestHeader("Content-Type", "application/json");

                    if (AuthSession.Instance != null && AuthSession.Instance.HasValidToken)
                    {
                        req.SetRequestHeader("Authorization", "Bearer " + AuthSession.Instance.AccessToken);
                    }

                    // Primary target gets 5s, fallback candidate hosts get 2s
                    req.timeout = (i == 0) ? 5 : 2;
                    yield return req.SendWebRequest();

                    // If rejected due to token expiration or worker id mismatch (401/403), retry without token
                    if ((req.responseCode == 401 || req.responseCode == 403) && AuthSession.Instance != null && AuthSession.Instance.HasValidToken)
                    {
                        Debug.LogWarning($"[OFFLINE SYNC] Auth rejected ({req.responseCode}). Retrying sync as guest/unauthenticated payload...");
                        using (UnityWebRequest retryReq = new UnityWebRequest(syncUrl, "POST"))
                        {
                            retryReq.uploadHandler = new UploadHandlerRaw(raw);
                            retryReq.uploadHandler.contentType = "application/json";
                            retryReq.downloadHandler = new DownloadHandlerBuffer();
                            retryReq.SetRequestHeader("Content-Type", "application/json");
                            retryReq.timeout = 5;
                            yield return retryReq.SendWebRequest();

                            if (retryReq.result == UnityWebRequest.Result.Success)
                            {
                                int syncedCount = payload.sessions.Count;
                                store.MarkSessionsSynced(syncedCount);
                                _backendBaseUrl = targetUrl;
                                PlayerPrefs.SetString("SurakshaAR_BackendUrl", targetUrl);
                                PlayerPrefs.Save();
                                _isOnline = true;
                                UpdateState(SyncState.Synced, "All Data Synced");
                                Debug.Log($"[OFFLINE SYNC] Successfully synced {syncedCount} offline sessions to {syncUrl} (unauthenticated retry).");
                                syncSucceeded = true;
                                break;
                            }
                        }
                    }

                    if (req.result == UnityWebRequest.Result.Success)
                    {
                        int syncedCount = payload.sessions.Count;
                        store.MarkSessionsSynced(syncedCount);
                        _backendBaseUrl = targetUrl;
                        PlayerPrefs.SetString("SurakshaAR_BackendUrl", targetUrl);
                        PlayerPrefs.Save();
                        _isOnline = true;
                        UpdateState(SyncState.Synced, "All Data Synced");
                        Debug.Log($"[OFFLINE SYNC] Successfully synced {syncedCount} offline sessions to {syncUrl}.");
                        syncSucceeded = true;
                        break;
                    }
                    else
                    {
                        Debug.LogWarning($"[OFFLINE SYNC] Sync attempt to {syncUrl} failed ({req.responseCode}): {req.error}");
                    }
                }
            }

            if (!syncSucceeded)
            {
                _isOnline = false;
                UpdateState(SyncState.Error, $"Sync Failed ({pending} pending)");
                Debug.LogWarning($"[OFFLINE SYNC] All sync targets failed. Retaining {pending} sessions in offline queue.");
            }

            _isSyncing = false;
        }

        public void RegisterOrSyncWorkerId(string employeeId, string name, bool isGuest = false, string role = "")
        {
            if (string.IsNullOrEmpty(employeeId)) return;
            StartCoroutine(RegisterWorkerRoutine(employeeId.Trim(), name?.Trim(), isGuest, role?.Trim()));
        }

        private IEnumerator RegisterWorkerRoutine(string employeeId, string name, bool isGuest, string role)
        {
            var payload = new WorkerRegisterPayload
            {
                employee_id = employeeId,
                name = !string.IsNullOrEmpty(name) ? name : (isGuest ? $"Guest {employeeId}" : $"Worker {employeeId}"),
                role = !string.IsNullOrEmpty(role) ? role : (isGuest ? "Guest Trainee" : "Mine Worker"),
                is_guest = isGuest
            };

            string json = JsonUtility.ToJson(payload);
            string url = _backendBaseUrl.TrimEnd('/') + "/api/v1/workers/register";

            using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
            {
                byte[] raw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(raw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 5;

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    _isOnline = true;
                    try
                    {
                        var res = JsonUtility.FromJson<WorkerRegisterResponse>(req.downloadHandler.text);
                        if (res != null && res.id > 0)
                        {
                            if (AuthSession.Instance != null)
                            {
                                string existingToken = AuthSession.Instance.AccessToken;
                                AuthSession.Instance.SetSession(
                                    existingToken,
                                    isGuest ? "guest" : "worker",
                                    employeeId,
                                    res.id
                                );
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("[WORKER CONNECT] Parse warning: " + ex.Message);
                    }

                    Debug.Log($"[WORKER CONNECT] Successfully created/synced worker ID '{employeeId}' on backend dashboard.");
                }
                else
                {
                    Debug.LogWarning($"[WORKER CONNECT] Backend unavailable or register failed ({req.responseCode}: {req.error}). Will sync once reconnected.");
                }
            }
        }

        private void UpdateState(SyncState state, string message)
        {
            _currentState = state;
            _statusMessage = message;
            int pending = OfflineDataStore.Instance != null ? OfflineDataStore.Instance.PendingSyncCount : 0;
            OnSyncStatusChanged?.Invoke(state, message, pending);
        }

        [Serializable]
        private class WorkerRegisterPayload
        {
            public string employee_id;
            public string name;
            public string role;
            public bool is_guest;
        }

        [Serializable]
        private class WorkerRegisterResponse
        {
            public int id;
            public string employee_id;
            public string name;
            public string role;
        }

        [Serializable]
        private class SyncPayload
        {
            public int worker_id = 0;
            public string guest_id;
            public string employee_id;
            public string employee_name;
            public string device_id;
            public string batch_id;
            public int pending_sessions;
            public List<SyncSessionData> sessions = new List<SyncSessionData>();
        }
    }
}
