using System;
using UnityEngine;

namespace SurakshaAR.Core
{
    /// <summary>
    /// Manages the authenticated user session, JWT Bearer tokens,
    /// worker identity, and RBAC authorization state.
    /// Preserves session across scenes.
    /// </summary>
    [DisallowMultipleComponent]
    public class AuthSession : MonoBehaviour
    {
        public static AuthSession Instance { get; private set; }

        private const string PrefKeyToken = "suraksha_auth_token";
        private const string PrefKeyRole = "suraksha_auth_role";
        private const string PrefKeyUsername = "suraksha_auth_username";
        private const string PrefKeyWorkerId = "suraksha_auth_worker_id";

        [Header("Active Session")]
        [SerializeField] private string _accessToken;
        [SerializeField] private string _role = "worker";
        [SerializeField] private string _username;
        [SerializeField] private int _workerId = -1;

        public string AccessToken => _accessToken;
        public string Role => _role;
        public string Username => _username;
        public int WorkerId => _workerId;

        public bool HasValidToken => !string.IsNullOrEmpty(_accessToken);
        public bool IsAdmin => string.Equals(_role, "admin", StringComparison.OrdinalIgnoreCase);
        public bool IsWorker => string.Equals(_role, "worker", StringComparison.OrdinalIgnoreCase);

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
            LoadCachedSession();
        }

        public void SetSession(string token, string role, string username, int workerId)
        {
            _accessToken = token ?? string.Empty;
            _role = string.IsNullOrEmpty(role) ? "worker" : role;
            _username = username ?? string.Empty;
            _workerId = workerId;

            PlayerPrefs.SetString(PrefKeyToken, _accessToken);
            PlayerPrefs.SetString(PrefKeyRole, _role);
            PlayerPrefs.SetString(PrefKeyUsername, _username);
            PlayerPrefs.SetInt(PrefKeyWorkerId, _workerId);
            PlayerPrefs.Save();

            OnSessionChanged?.Invoke();
            Debug.Log($"[AUTH SESSION] Session established for user '{_username}' (Role: {_role}, WorkerID: {_workerId})");
        }

        public void ClearSession()
        {
            _accessToken = string.Empty;
            _role = "worker";
            _username = string.Empty;
            _workerId = -1;

            PlayerPrefs.DeleteKey(PrefKeyToken);
            PlayerPrefs.DeleteKey(PrefKeyRole);
            PlayerPrefs.DeleteKey(PrefKeyUsername);
            PlayerPrefs.DeleteKey(PrefKeyWorkerId);
            PlayerPrefs.Save();

            OnSessionChanged?.Invoke();
            Debug.Log("[AUTH SESSION] Session cleared.");
        }

        private void LoadCachedSession()
        {
            _accessToken = PlayerPrefs.GetString(PrefKeyToken, string.Empty);
            _role = PlayerPrefs.GetString(PrefKeyRole, "worker");
            _username = PlayerPrefs.GetString(PrefKeyUsername, string.Empty);
            _workerId = PlayerPrefs.GetInt(PrefKeyWorkerId, -1);
        }
    }
}
