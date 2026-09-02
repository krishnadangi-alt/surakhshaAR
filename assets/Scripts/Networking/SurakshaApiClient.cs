using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SurakshaAR.Networking
{
    /// <summary>Result of a request to the SurakshaAR backend.</summary>
    public sealed class ApiResult<T>
    {
        public bool Success { get; internal set; }
        public long StatusCode { get; internal set; }
        public T Data { get; internal set; }
        public string RawBody { get; internal set; }
        public string Error { get; internal set; }
    }

    /// <summary>
    /// Coroutine-based HTTP client for the SurakshaAR backend API (docs/api/API.md).
    /// Attach one instance to a persistent GameObject (e.g. the scene's XR Origin or
    /// a dedicated "Backend" GameObject). All methods start a coroutine on this
    /// MonoBehaviour and invoke the completion callback on the main thread.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SurakshaApiClient : MonoBehaviour
    {
        public const string DefaultBaseUrl = "http://127.0.0.1:8000/api/v1";

        public static SurakshaApiClient Instance { get; private set; }

        [Header("Backend")]
        [Tooltip("Base URL of the SurakshaAR backend API (no trailing slash).")]
        [SerializeField] private string baseUrl = DefaultBaseUrl;

        [Tooltip("Request timeout in seconds.")]
        [SerializeField, Min(1)] private int timeoutSeconds = 15;

        public string BaseUrl => baseUrl;

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

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        // ------------------------------------------------------------------
        // Low-level GET / POST
        // ------------------------------------------------------------------

        private IEnumerator GetJson<T>(string path, Action<ApiResult<T>> onDone)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(BuildUrl(path)))
            {
                request.timeout = timeoutSeconds;
                yield return request.SendWebRequest();
                Finish(request, onDone);
            }
        }

        private IEnumerator PostJson<TReq, TRes>(string path, TReq payload, Action<ApiResult<TRes>> onDone)
        {
            string json = JsonUtility.ToJson(payload);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(BuildUrl(path), "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = timeoutSeconds;
                yield return request.SendWebRequest();
                Finish(request, onDone);
            }
        }

        private string BuildUrl(string path)
        {
            string url = baseUrl;
            if (string.IsNullOrEmpty(url))
            {
                url = DefaultBaseUrl;
            }

            url = url.TrimEnd('/');
            return path.StartsWith("/", StringComparison.Ordinal) ? url + path : url + "/" + path;
        }

        private static void Finish<T>(UnityWebRequest request, Action<ApiResult<T>> onDone)
        {
            ApiResult<T> result = new ApiResult<T>
            {
                Success = request.result == UnityWebRequest.Result.Success,
                StatusCode = request.responseCode,
                RawBody = request.downloadHandler != null ? request.downloadHandler.text : string.Empty,
            };

            if (result.Success)
            {
                try
                {
                    result.Data = JsonUtility.FromJson<T>(result.RawBody);
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Error = "Failed to parse response: " + ex.Message;
                }
            }
            else
            {
                result.Error = ExtractError(request);
            }

            if (onDone != null)
            {
                onDone(result);
            }
        }

        private static string ExtractError(UnityWebRequest request)
        {
            string body = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
            if (!string.IsNullOrEmpty(body))
            {
                try
                {
                    ApiContracts.ApiError error = JsonUtility.FromJson<ApiContracts.ApiError>(body);
                    if (error != null && !string.IsNullOrEmpty(error.detail))
                    {
                        return error.detail;
                    }
                }
                catch (Exception)
                {
                    // Fall through to the generic message below.
                }
            }

            return !string.IsNullOrEmpty(request.error)
                ? request.error
                : "HTTP " + request.responseCode;
        }

        // ------------------------------------------------------------------
        // Health
        // ------------------------------------------------------------------

        /// <summary>GET /health - returns true when the backend is reachable.</summary>
        public Coroutine CheckHealth(Action<ApiResult<bool>> onDone)
        {
            return StartCoroutine(GetJson<bool>("/health", onDone));
        }

        // ------------------------------------------------------------------
        // Workers (docs/api/API.md #1, #2)
        // ------------------------------------------------------------------

        public Coroutine CreateWorker(ApiContracts.WorkerCreate payload, Action<ApiResult<ApiContracts.WorkerOut>> onDone)
        {
            return StartCoroutine(PostJson<ApiContracts.WorkerCreate, ApiContracts.WorkerOut>("/workers", payload, onDone));
        }

        public Coroutine GetWorker(int workerId, Action<ApiResult<ApiContracts.WorkerOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.WorkerOut>("/workers/" + workerId, onDone));
        }

        // ------------------------------------------------------------------
        // Modules (docs/api/API.md #3)
        // ------------------------------------------------------------------

        public Coroutine ListModules(Action<ApiResult<ApiContracts.ModuleListOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.ModuleListOut>("/modules", onDone));
        }

        // ------------------------------------------------------------------
        // Progress (docs/api/API.md #4, #5)
        // ------------------------------------------------------------------

        public Coroutine GetProgress(int workerId, Action<ApiResult<ApiContracts.ProgressListOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.ProgressListOut>("/progress/" + workerId, onDone));
        }

        public Coroutine UpdateProgress(ApiContracts.ProgressCreate payload, Action<ApiResult<ApiContracts.ProgressOut>> onDone)
        {
            return StartCoroutine(PostJson<ApiContracts.ProgressCreate, ApiContracts.ProgressOut>("/progress", payload, onDone));
        }

        // ------------------------------------------------------------------
        // Assessments (docs/api/API.md #6, #7, #8)
        // ------------------------------------------------------------------

        public Coroutine SubmitAssessment(ApiContracts.AssessmentCreate payload, Action<ApiResult<ApiContracts.AssessmentOut>> onDone)
        {
            return StartCoroutine(PostJson<ApiContracts.AssessmentCreate, ApiContracts.AssessmentOut>("/assessments", payload, onDone));
        }

        public Coroutine GetAssessmentHistory(int workerId, Action<ApiResult<ApiContracts.AssessmentHistoryOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.AssessmentHistoryOut>("/assessments/" + workerId, onDone));
        }

        public Coroutine GetLatestAssessment(int workerId, Action<ApiResult<ApiContracts.AssessmentOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.AssessmentOut>("/assessments/" + workerId + "/latest", onDone));
        }

        // ------------------------------------------------------------------
        // Sync (docs/api/API.md #9, #10)
        // ------------------------------------------------------------------

        public Coroutine SyncSessions(ApiContracts.SyncCreate payload, Action<ApiResult<ApiContracts.SyncOut>> onDone)
        {
            return StartCoroutine(PostJson<ApiContracts.SyncCreate, ApiContracts.SyncOut>("/sync", payload, onDone));
        }

        public Coroutine GetSyncStatus(int workerId, Action<ApiResult<ApiContracts.SyncStatusOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.SyncStatusOut>("/sync/status/" + workerId, onDone));
        }

        // ------------------------------------------------------------------
        // Certificates (docs/api/API.md #11, #12, #13)
        // ------------------------------------------------------------------

        public Coroutine IssueCertificate(ApiContracts.CertificateCreate payload, Action<ApiResult<ApiContracts.CertificateOut>> onDone)
        {
            return StartCoroutine(PostJson<ApiContracts.CertificateCreate, ApiContracts.CertificateOut>("/certificates", payload, onDone));
        }

        public Coroutine GetWorkerCertificates(int workerId, Action<ApiResult<ApiContracts.CertificateListOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.CertificateListOut>("/certificates/" + workerId, onDone));
        }

        public Coroutine VerifyCertificate(string certificateNumber, Action<ApiResult<ApiContracts.CertificateVerifyOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.CertificateVerifyOut>("/certificates/verify/" + Uri.EscapeDataString(certificateNumber), onDone));
        }

        // ------------------------------------------------------------------
        // ML Competency - Retraining (docs/api/API.md #17)
        // ------------------------------------------------------------------

        public Coroutine GetRetrainingPlan(int assessmentId, Action<ApiResult<ApiContracts.RetrainingPlanOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.RetrainingPlanOut>("/assessments/" + assessmentId + "/retraining-plan", onDone));
        }

        // ------------------------------------------------------------------
        // Vision / ML - PPE detection (docs/api/API.md #18, #19)
        // ------------------------------------------------------------------

        public Coroutine CheckPPE(ApiContracts.PPECheckRequest payload, Action<ApiResult<ApiContracts.PPECheckOut>> onDone)
        {
            return StartCoroutine(PostJson<ApiContracts.PPECheckRequest, ApiContracts.PPECheckOut>("/vision/ppe-check", payload, onDone));
        }

        public Coroutine GetVisionStatus(Action<ApiResult<ApiContracts.VisionStatusOut>> onDone)
        {
            return StartCoroutine(GetJson<ApiContracts.VisionStatusOut>("/vision/status", onDone));
        }
    }
}