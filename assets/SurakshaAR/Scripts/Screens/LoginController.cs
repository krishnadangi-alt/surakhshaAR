using System;
using System.Collections;
using System.Text;
using SurakshaAR.Core;
using SurakshaAR.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SurakshaAR.Networking;
using SurakshaAR.Data;

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
        private TextMeshProUGUI _labelError;
        private Button _rememberCheckbox;
        private TextMeshProUGUI _rememberCheckSymbol;
        private bool _isRemembered = true;
        private bool _isGuestMode  = false;

        public void OnShow(GameObject root, object param)
        {
            var loc = AppManager.Instance?.Localization;
            _labelError = UIHelper.FindTMP(root, "label-login-error");
            ClearError();

            // ── Tabs ──────────────────────────────────────────────────────
            _tabWorker = UIHelper.FindButton(root, "tab-worker");
            _tabGuest  = UIHelper.FindButton(root, "tab-guest");

            if (_tabWorker != null)
            {
                _tabWorker.onClick.RemoveAllListeners();
                _tabWorker.onClick.AddListener(() => SetGuestMode(false, root));
                var wLbl = _tabWorker.GetComponentInChildren<TextMeshProUGUI>();
                if (wLbl != null) wLbl.text = loc?.Get("login.workerLogin") ?? "Worker Login";
            }
            if (_tabGuest != null)
            {
                _tabGuest.onClick.RemoveAllListeners();
                _tabGuest.onClick.AddListener(() => SetGuestMode(true, root));
                var gLbl = _tabGuest.GetComponentInChildren<TextMeshProUGUI>();
                if (gLbl != null) gLbl.text = loc?.Get("login.guestMode") ?? "Guest Mode";
            }

            // ── Input fields ──────────────────────────────────────────────
            var empRT = UIHelper.FindRect(root, "field-employee-id");
            if (empRT != null)
            {
                _fieldEmployeeId = empRT.GetComponentInChildren<TMP_InputField>();
                var empLbl = empRT.Find("Label")?.GetComponent<TextMeshProUGUI>();
                if (empLbl != null) empLbl.text = loc?.Get("login.employeeId") ?? "Employee ID";
            }

            var passRT = UIHelper.FindRect(root, "field-password");
            if (passRT != null)
            {
                _fieldPassword = passRT.GetComponentInChildren<TMP_InputField>();
                var passLbl = passRT.Find("Label")?.GetComponent<TextMeshProUGUI>();
                if (passLbl != null) passLbl.text = loc?.Get("login.password") ?? "Password / PIN";
            }

            // Apply localized placeholder text
            if (loc != null)
            {
                SetPlaceholder(_fieldEmployeeId, loc.Get("login.employeeIdPlaceholder"));
                SetPlaceholder(_fieldPassword,   loc.Get("login.passwordPlaceholder"));
            }

            // ── Options Row ───────────────────────────────────────────────
            _rememberCheckbox = UIHelper.FindButton(root, "remember-checkbox");
            if (_rememberCheckbox != null)
            {
                _rememberCheckSymbol = _rememberCheckbox.GetComponentInChildren<TextMeshProUGUI>();
                _rememberCheckbox.onClick.RemoveAllListeners();
                _rememberCheckbox.onClick.AddListener(ToggleRemember);
            }

            LocalizeLabelText(root, "RememberMeLabel", loc?.Get("login.rememberMe") ?? "Remember me");

            var forgotBtn = UIHelper.FindButton(root, "btn-forgot-password");
            if (forgotBtn != null)
            {
                var fLbl = forgotBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (fLbl != null) fLbl.text = loc?.Get("login.forgotPassword") ?? "Forgot Password?";
                forgotBtn.onClick.RemoveAllListeners();
                forgotBtn.onClick.AddListener(() =>
                {
                    ShowError(loc?.Get("login.forgotPasswordMessage") ?? "Please contact your mine supervisor or safety officer to reset your credentials.");
                });
            }

            // ── Login / QR buttons ────────────────────────────────────────
            _btnLogin = UIHelper.FindButton(root, "btn-login");
            if (_btnLogin != null)
            {
                _btnLogin.onClick.RemoveAllListeners();
                _btnLogin.onClick.AddListener(OnLogin);
            }

            _btnLoginQr = UIHelper.FindButton(root, "btn-login-qr");
            if (_btnLoginQr != null)
            {
                _btnLoginQr.onClick.RemoveAllListeners();
                _btnLoginQr.onClick.AddListener(OnLogin);
            }

            // Localize static text labels
            if (loc != null)
            {
                LocalizeLabelText(root, "LoginTitle",    loc.Get("login.workerLogin"));
                LocalizeLabelText(root, "LoginSubtitle", loc.Get("login.tagline"));
                LocalizeLabelText(root, "AppSubtitle",   loc.Get("splash.tagline"));
                LocalizeLabelText(root, "OrText",        loc.Get("login.or"));

                var loginLbl = UIHelper.FindTMP(root, "LoginText") ?? _btnLogin?.GetComponentInChildren<TextMeshProUGUI>();
                if (loginLbl != null) loginLbl.text = loc.Get("login.submit");

                var qrLbl = UIHelper.FindTMP(root, "QRText") ?? _btnLoginQr?.GetComponentInChildren<TextMeshProUGUI>();
                if (qrLbl != null) qrLbl.text = loc.Get("login.qrCode");
            }

            // Apply font for active language
            var currentLang = AppState.Instance != null
                ? AppState.Instance.CurrentLanguage
                : (loc != null ? loc.CurrentLanguage : AppLanguage.English);
            UIManager.Instance?.ApplyLanguageFonts(root, currentLang);
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
        private void SetGuestMode(bool guest, GameObject root = null)
        {
            _isGuestMode = guest;
            var loc = AppManager.Instance?.Localization;
            if (_tabWorker == null || _tabGuest == null) return;

            var workerImg  = _tabWorker.GetComponent<Image>();
            var guestImg   = _tabGuest.GetComponent<Image>();
            var workerText = _tabWorker.GetComponentInChildren<TextMeshProUGUI>();
            var guestText  = _tabGuest.GetComponentInChildren<TextMeshProUGUI>();

            var empRT = root != null ? UIHelper.FindRect(root, "field-employee-id") : null;
            var empLabel = empRT?.Find("Label")?.GetComponent<TextMeshProUGUI>();
            var loginLbl = root != null ? UIHelper.FindTMP(root, "LoginText") ?? _btnLogin?.GetComponentInChildren<TextMeshProUGUI>() : null;

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
                if (_fieldEmployeeId != null) _fieldEmployeeId.text = "";

                if (empLabel != null && loc != null) empLabel.text = loc.Get("login.guestIdLabel");
                if (loc != null) SetPlaceholder(_fieldEmployeeId, loc.Get("login.guestIdPlaceholder"));
                if (loginLbl != null && loc != null) loginLbl.text = loc.Get("login.continueAsGuest");
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

                if (empLabel != null && loc != null) empLabel.text = loc.Get("login.employeeId");
                if (loc != null) SetPlaceholder(_fieldEmployeeId, loc.Get("login.employeeIdPlaceholder"));
                if (loginLbl != null && loc != null) loginLbl.text = loc.Get("login.submit");
            }
            ClearError();
        }

        // ─────────────────────────────────────────────────────────────────
        //  LOGIN
        // ─────────────────────────────────────────────────────────────────
        private void OnLogin()
        {
            ClearError();
            var loc = AppManager.Instance?.Localization;

            if (_isGuestMode)
            {
                string rawGuest = _fieldEmployeeId != null ? _fieldEmployeeId.text.Trim() : "";
                if (string.IsNullOrEmpty(rawGuest))
                {
                    rawGuest = "GUEST-001";
                }
                EnterLocalSession(rawGuest, "Guest Trainee");
                return;
            }

            string username = _fieldEmployeeId != null ? _fieldEmployeeId.text.Trim() : "";
            string password = _fieldPassword != null ? _fieldPassword.text : "";

            // Direct login: if username is empty, provide direct access without prompting for employee id
            if (string.IsNullOrEmpty(username))
            {
                EnterLocalSession("EMP-PROD-CERT", "Krishna");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                // Offline play without credentials
                EnterLocalSession(username, $"Worker {username}");
                return;
            }

            if (_btnLogin != null) _btnLogin.interactable = false;
            CoroutineRunner.Run(LoginRoutine(username, password));
        }

        private void EnterLocalSession(string empId, string name)
        {
            if (AppState.Instance != null)
                AppState.Instance.SetUser(empId, name, _isGuestMode, _isGuestMode ? "Guest" : "Mine Worker");

            UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard);
        }

        private void ShowError(string message)
        {
            if (_labelError != null)
            {
                _labelError.text = message;
                _labelError.gameObject.SetActive(true);
            }
            Debug.LogWarning("[LOGIN] " + message);
        }

        private void ClearError()
        {
            if (_labelError != null)
            {
                _labelError.text = "";
                _labelError.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Authenticate against POST /api/v1/auth/login and store the
        /// Bearer token + worker id in AuthSession for every later API call.
        /// Falls back to the local session when the backend is unreachable.
        private IEnumerator LoginRoutine(string username, string password)
        {
            string baseUrl = SurakshaApiClient.Instance != null
                ? SurakshaApiClient.Instance.BaseUrl
                : (OfflineSyncManager.Instance != null
                    ? OfflineSyncManager.Instance.BackendBaseUrl.TrimEnd('/') + "/api/v1"
                    : (PlayerPrefs.HasKey("SurakshaAR_BackendUrl")
                        ? PlayerPrefs.GetString("SurakshaAR_BackendUrl").TrimEnd('/') + "/api/v1"
#if !UNITY_EDITOR
                        : "http://192.168.137.1:8000/api/v1"));
#else
                        : "http://127.0.0.1:8000/api/v1"));
#endif
            string url = baseUrl.TrimEnd('/') + "/auth/login";
            string json = JsonUtility.ToJson(new LoginRequest { username = username, password = password });

            using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 10;

                UnityWebRequestAsyncOperation op = null;
                try
                {
                    op = req.SendWebRequest();
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("[LOGIN] SendWebRequest exception: " + ex.Message + ". Continuing offline.");
                    if (_btnLogin != null) _btnLogin.interactable = true;
                    EnterLocalSession(username, $"Worker {username}");
                    yield break;
                }

                yield return op;

                if (_btnLogin != null) _btnLogin.interactable = true;

                if (req.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var token = JsonUtility.FromJson<LoginResponse>(req.downloadHandler.text);
                        AuthSession session = EnsureAuthSession();
                        if (session != null && token != null)
                            session.SetSession(token.access_token, token.role, token.username, token.worker_id);
                        if (AppState.Instance != null)
                            AppState.Instance.SetUser(username, token != null ? token.username : username, false, token != null ? token.role : "worker");
                    }
                    catch (System.Exception parseEx)
                    {
                        Debug.LogWarning("[LOGIN] Token parse error: " + parseEx.Message);
                    }
                    UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard);
                }
                else if (req.responseCode == 401)
                {
                    // Attempt to auto-register new worker account via /workers/register
                    yield return RegisterWorkerRoutine(username, password, baseUrl);
                }
                else
                {
                    // Backend unreachable — keep the offline-first promise.
                    Debug.LogWarning("[LOGIN] Backend unavailable (" + req.error + "). Continuing offline.");
                    EnterLocalSession(username, $"Worker {username}");
                }
            }
        }

        private IEnumerator RegisterWorkerRoutine(string username, string password, string baseUrl)
        {
            string url = baseUrl.TrimEnd('/') + "/workers/register";
            var payload = new RegisterRequest
            {
                employee_id = username,
                name = $"Worker {username}",
                role = "Mine Worker",
                is_guest = false,
                password = password
            };
            string json = JsonUtility.ToJson(payload);

            using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 8;

                UnityWebRequestAsyncOperation regOp = null;
                try
                {
                    regOp = req.SendWebRequest();
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("[LOGIN] Register SendWebRequest exception: " + ex.Message + ". Continuing offline.");
                    EnterLocalSession(username, $"Worker {username}");
                    yield break;
                }

                yield return regOp;

                if (req.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("[LOGIN] Auto-registered worker account for " + username);
                    yield return LoginRoutine(username, password);
                }
                else
                {
                    Debug.LogWarning("[LOGIN] Invalid credentials: " + req.error);
                    var loc = AppManager.Instance?.Localization;
                    ShowError(loc?.Get("login.invalidCredentials") ?? "Invalid credentials. Please check your Employee ID and password.");
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
        private class RegisterRequest
        {
            public string employee_id;
            public string name;
            public string role;
            public bool is_guest;
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
