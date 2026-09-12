using SurakshaAR.Core;
using SurakshaAR.UI;
using TMPro;
using UnityEngine;
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

                var loginLbl = _btnLogin?.GetComponentInChildren<TextMeshProUGUI>();
                if (loginLbl != null) loginLbl.text = loc.Get("login.submit");

                var qrLbl = _btnLoginQr?.GetComponentInChildren<TextMeshProUGUI>();
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
                if (workerImg  != null) workerImg.color  = UIColors.Transparent;
                if (guestImg   != null) guestImg.color   = UIColors.Primary;
                if (workerText != null) workerText.color = UIColors.TextSecondary;
                if (guestText  != null) guestText.color  = Color.white;
                if (_fieldEmployeeId != null) _fieldEmployeeId.text = "GUEST_USER";
            }
            else
            {
                if (workerImg  != null) workerImg.color  = UIColors.Primary;
                if (guestImg   != null) guestImg.color   = UIColors.Transparent;
                if (workerText != null) workerText.color = Color.white;
                if (guestText  != null) guestText.color  = UIColors.TextSecondary;
                if (_fieldEmployeeId != null) _fieldEmployeeId.text = "";
            }
        }

        // ─────────────────────────────────────────────────────────────────
        //  LOGIN
        // ─────────────────────────────────────────────────────────────────
        private void OnLogin()
        {
            string empId = _fieldEmployeeId != null && !string.IsNullOrWhiteSpace(_fieldEmployeeId.text)
                ? _fieldEmployeeId.text.Trim()
                : "JH-MN-004821";
            string name = _isGuestMode ? "Guest Worker" : "Ramesh Kumar";

            if (AppState.Instance != null)
                AppState.Instance.SetUser(empId, name, _isGuestMode);

            UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard);
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
