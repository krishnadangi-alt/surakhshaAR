using System;
using System.Collections;
using System.Text;
using SurakshaAR.Core;
using SurakshaAR.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    /// <summary>
    /// Controller for the Login screen.
    /// Handles Worker Login / Guest Mode tab switching, field validation,
    /// and navigation to Home Dashboard on successful login.
    /// Localizes all visible text from LocalizationManager.
    /// </summary>
    public class LoginController : IScreenController
    {
        private Button _tabWorker, _tabGuest, _btnLogin, _btnLoginQr;
        private TMP_InputField _fieldEmployeeId, _fieldPassword;
        private Button _rememberCheckbox;
        private TextMeshProUGUI _rememberCheckSymbol;
        private bool _isRemembered = true;
        private bool _isGuestMode  = false;

        public void OnShow(GameObject root, object param)
        {
            var loc = AppManager.Instance?.Localization;

            // ── Tabs ──────────────────────────────────────────────────────
            _tabWorker = UIHelper.FindButton(root, "tab-worker");
            _tabGuest  = UIHelper.FindButton(root, "tab-guest");

            if (_tabWorker != null) _tabWorker.onClick.AddListener(() => SetGuestMode(false));
            if (_tabGuest  != null) _tabGuest.onClick.AddListener(()  => SetGuestMode(true));

            // ── Input fields ──────────────────────────────────────────────
            // The builder puts TMP_InputField directly inside a RectTransform named "field-*"
            var empRT = UIHelper.FindRect(root, "field-employee-id");
            if (empRT != null) _fieldEmployeeId = empRT.GetComponentInChildren<TMP_InputField>();

            var passRT = UIHelper.FindRect(root, "field-password");
            if (passRT != null) _fieldPassword = passRT.GetComponentInChildren<TMP_InputField>();

            // Apply localized placeholder text
            if (loc != null)
            {
                SetPlaceholder(_fieldEmployeeId, loc.Get("login.employeeIdPlaceholder"));
                SetPlaceholder(_fieldPassword,   loc.Get("login.passwordPlaceholder"));
            }

            // ── Remember checkbox ─────────────────────────────────────────
            _rememberCheckbox = UIHelper.FindButton(root, "remember-checkbox");
            if (_rememberCheckbox != null)
            {
                _rememberCheckSymbol = _rememberCheckbox.GetComponentInChildren<TextMeshProUGUI>();
                _rememberCheckbox.onClick.AddListener(ToggleRemember);
            }

            // ── Login / QR buttons ────────────────────────────────────────
            _btnLogin = UIHelper.FindButton(root, "btn-login");
            if (_btnLogin != null) _btnLogin.onClick.AddListener(OnLogin);

            _btnLoginQr = UIHelper.FindButton(root, "btn-login-qr");
            if (_btnLoginQr != null) _btnLoginQr.onClick.AddListener(OnLogin);

            // Localize static text labels
            if (loc != null)
            {
                LocalizeLabelText(root, "LoginTitle",    loc.Get("login.workerLogin"));
                LocalizeLabelText(root, "LoginSubtitle", loc.Get("login.tagline"));
                LocalizeLabelText(root, "AppSubtitle",   loc.Get("splash.tagline"));

                var loginLbl = UIHelper.FindTMP(root, "LoginText") ?? _btnLogin?.GetComponentInChildren<TextMeshProUGUI>();
                if (loginLbl != null) loginLbl.text = loc.Get("login.submit");

                var qrLbl = UIHelper.FindTMP(root, "QRText") ?? _btnLoginQr?.GetComponentInChildren<TextMeshProUGUI>();
                if (qrLbl != null) qrLbl.text = loc.Get("login.qrCode");
            }
        }

        // ─────────────────────────────────────────────────────────────────
        //  HELPERS
        // ─────────────────────────────────────────────────────────────────
        private static void SetPlaceholder(TMP_InputField field, string text)
        {
            if (field == null || string.IsNullOrEmpty(text)) return;
            var ph = field.placeholder as TextMeshProUGUI;
            if (ph != null) ph.text = text;
        }

        private static void LocalizeLabelText(GameObject root, string name, string text)
        {
            var tmp = UIHelper.FindTMP(root, name);
            if (tmp != null && !string.IsNullOrEmpty(text)) tmp.text = text;
        }

        // ─────────────────────────────────────────────────────────────────
        //  REMEMBER
        // ─────────────────────────────────────────────────────────────────
        private void ToggleRemember()
        {
            _isRemembered = !_isRemembered;
            var cmk = _rememberCheckbox?.transform.Find("CheckMark")?.gameObject;
            if (cmk != null) cmk.SetActive(_isRemembered);
            if (_rememberCheckSymbol != null)
                _rememberCheckSymbol.text = "";
        }

        // ─────────────────────────────────────────────────────────────────
        //  GUEST / WORKER TAB
        // ─────────────────────────────────────────────────────────────────
        private void SetGuestMode(bool guest)
        {
            _isGuestMode = guest;
            if (_tabWorker == null || _tabGuest == null) return;

            var workerImg  = _tabWorker.GetComponent<Image>();
            var guestImg   = _tabGuest.GetComponent<Image>();
            var workerText = _tabWorker.GetComponentInChildren<TextMeshProUGUI>();
            var guestText  = _tabGuest.GetComponentInChildren<TextMeshProUGUI>();

            if (guest)
            {
                if (workerImg  != null) workerImg.color  = Color.clear;
                if (guestImg   != null)
                {
                    guestImg.color = UIColors.Hex("#0A192F");
                    UIHelper.SetImageRoundedSprite(guestImg, 16f);
                }
                if (workerText != null) workerText.color = UIColors.Hex("#64748B");
                if (guestText  != null) guestText.color  = Color.white;
                if (_fieldEmployeeId != null) _fieldEmployeeId.text = "GUEST_USER";
            }
            else
            {
                if (workerImg  != null)
                {
                    workerImg.color = UIColors.Hex("#0A192F");
                    UIHelper.SetImageRoundedSprite(workerImg, 16f);
                }
                if (guestImg   != null) guestImg.color   = Color.clear;
                if (workerText != null) workerText.color = Color.white;
                if (guestText  != null) guestText.color  = UIColors.Hex("#64748B");
                if (_fieldEmployeeId != null) _fieldEmployeeId.text = "";
            }
        }

        // ─────────────────────────────────────────────────────────────────
        //  LOGIN
        // ─────────────────────────────────────────────────────────────────
        private void OnLogin()
        {
            string username = _fieldEmployeeId != null && !string.IsNullOrWhiteSpace(_fieldEmployeeId.text)
                ? _fieldEmployeeId.text.Trim()
                : "JH-MN-004821";
            string password = _fieldPassword != null ? _fieldPassword.text : "";

            if (_isGuestMode || string.IsNullOrEmpty(password))
            {
                // Day 6: guest mode and offline play keep working without a
                // backend account — local-only session, no Bearer token.
                EnterLocalSession(username, _isGuestMode ? "Guest Worker" : "Ramesh Kumar");
                return;
            }

            if (_btnLogin != null) _btnLogin.interactable = false;
            CoroutineRunner.Run(LoginRoutine(username, password));
        }

        private void EnterLocalSession(string empId, string name)
        {
            if (AppState.Instance != null)
                AppState.Instance.SetUser(empId, name, _isGuestMode);

            UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard);
        }

        /// <summary>
        /// Day 6: authenticate against POST /api/v1/auth/login and store the
        /// Bearer token + worker id in AuthSession for every later API call.
        /// Falls back to the local session when the backend is unreachable.
        /// </summary>
        private IEnumerator LoginRoutine(string username, string password)
        {
            string baseUrl = OfflineSyncManager.Instance != null
                ? OfflineSyncManager.Instance.BackendBaseUrl
                : "http://127.0.0.1:8000";
            string url = baseUrl.TrimEnd('/') + "/api/v1/auth/login";
            string json = JsonUtility.ToJson(new LoginRequest { username = username, password = password });

            using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 10;

                yield return req.SendWebRequest();

                if (_btnLogin != null) _btnLogin.interactable = true;

                if (req.result == UnityWebRequest.Result.Success)
                {
                    var token = JsonUtility.FromJson<LoginResponse>(req.downloadHandler.text);
                    AuthSession session = EnsureAuthSession();
                    if (session != null && token != null)
                        session.SetSession(token.access_token, token.role, token.username, token.worker_id);
                    if (AppState.Instance != null)
                        AppState.Instance.SetUser(username, token != null ? token.username : username, false);
                    UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard);
                }
                else if (req.responseCode == 401)
                {
                    Debug.LogWarning("[LOGIN] Invalid credentials. Check username/password.");
                }
                else
                {
                    // Backend unreachable — keep the offline-first promise.
                    Debug.LogWarning("[LOGIN] Backend unavailable (" + req.error + "). Continuing offline.");
                    EnterLocalSession(username, "Ramesh Kumar");
                }
            }
        }

        private static AuthSession EnsureAuthSession()
        {
            if (AuthSession.Instance != null) return AuthSession.Instance;
            var go = new GameObject("AuthSession");
            UnityEngine.Object.DontDestroyOnLoad(go);
            return go.AddComponent<AuthSession>();
        }

        [Serializable]
        private class LoginRequest
        {
            public string username;
            public string password;
        }

        [Serializable]
        private class LoginResponse
        {
            public string access_token;
            public string token_type;
            public string role;
            public string username;
            public int worker_id = -1;
        }

        // ─────────────────────────────────────────────────────────────────
        //  CLEANUP
        // ─────────────────────────────────────────────────────────────────
        public void OnHide()
        {
            if (_btnLogin         != null) _btnLogin.onClick.RemoveListener(OnLogin);
            if (_btnLoginQr       != null) _btnLoginQr.onClick.RemoveListener(OnLogin);
            if (_rememberCheckbox != null) _rememberCheckbox.onClick.RemoveListener(ToggleRemember);
        }
    }
}
