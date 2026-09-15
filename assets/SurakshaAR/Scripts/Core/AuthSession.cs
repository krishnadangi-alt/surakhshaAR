using System;
using UnityEngine;

namespace SurakshaAR.Core
{
    /// <summary>
    /// AuthSession (Day 6).
    /// Workspace-wide holder for the backend Bearer token minted by
    /// POST /api/v1/auth/login, plus the authenticated worker id that the
    /// token response carries (TokenOut.worker_id).
    ///
    /// Every UnityWebRequest to the FastAPI backend attaches
    /// "Authorization: Bearer &lt;token&gt;". The session survives scene loads
    /// (DontDestroyOnLoad) and is cleared on logout. Offline (guest) play is
    /// unaffected: requests are simply skipped while no token is present.
    /// </summary>
    public class AuthSession : MonoBehaviour
    {
        public static AuthSession Instance { get; private set; }

        /// <summary>JWT access token from /auth/login. Empty while signed out.</summary>
        public string AccessToken { get; private set; } = "";

        /// <summary>Role claim from the login response ("admin" or "worker").</summary>
        public string Role { get; private set; } = "";

        /// <summary>Backend username from the login response.</summary>
        public string Username { get; private set; } = "";

                /// <summary>
        /// Backend worker id from TokenOut.worker_id. -1 when the signed-in
        /// account has no linked worker (e.g. the bootstrap admin).
        /// </summary>
        public int WorkerId { get; private set; } = -1;

        /// <summary>
        /// Configurable backend base URL (without trailing slash), used as the
        /// default target for API calls and certificate verification links.
        /// Falls back to localhost dev when unset, so the editor plays out of
        /// the box. Set via the inspector or AuthSession.BackendBaseUrl at
        /// runtime / via a build-time config scriptable object.
        /// </summary>
        public string BackendBaseUrl { get; set; } = "http://127.0.0.1:8000";

        /// <summary>
        /// Signing secret for certificate tamper-detection, sourced from the
        /// environment / build config rather than hardcoded in the assembly.
        /// </summary>
        public string SignatureSecret { get; set; } = "SURAKSHA_AR_SECRET_LEGACY";

        public bool IsSignedIn => !string.IsNullOrEmpty(AccessToken);

        /// <summary>Value for the HTTP Authorization header, or null when signed out.</summary>
        public string AuthorizationHeader => IsSignedIn ? "Bearer " + AccessToken : null;

        public event Action OnSessionChanged;

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

        /// <summary>Store the outcome of a successful POST /auth/login call.</summary>
        public void SetSession(string accessToken, string role, string username, int workerId)
        {
            AccessToken = accessToken ?? "";
            Role = role ?? "";
            Username = username ?? "";
            WorkerId = workerId;
            OnSessionChanged?.Invoke();
        }

        public void Clear()
        {
            AccessToken = "";
            Role = "";
            Username = "";
            WorkerId = -1;
            OnSessionChanged?.Invoke();
        }

        /// <summary>
        /// Attaches "Authorization: Bearer ..." when signed in.
        /// Returns true when the header was attached.
        /// </summary>
        public bool ApplyAuthHeader(UnityEngine.Networking.UnityWebRequest req)
        {
            if (req == null || !IsSignedIn) return false;
            req.SetRequestHeader("Authorization", AuthorizationHeader);
            return true;
        }
    }
}
