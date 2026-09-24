using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    /// <summary>
    /// Profile screen controller.
    /// Manages worker details, interactive modals (Personal Info/Edit, Safety Preferences, App Settings),
    /// live language switching, bottom navigation, and secure logout.
    /// All user-facing strings are strictly localized from LocalizedStrings.cs.
    /// </summary>
    public class ProfileController : IScreenController
    {
        private Button _navHome, _navLearn, _navProgress, _navCertificates;
        private Button _btnBack, _btnEdit, _btnPersonal, _btnSafety, _btnCerts, _btnHistory, _btnSettings, _btnLogout;

        public void OnShow(GameObject root, object param)
        {
            var loc   = AppManager.Instance?.Localization;
            var state = AppState.Instance;
            var currentLang = state != null
                ? state.CurrentLanguage
                : (loc != null ? loc.CurrentLanguage : AppLanguage.English);

            ApplyLanguageFonts(root, currentLang);

            // ── 1. Worker Profile Details from Real State ──────────────────
            string notAvailable = loc?.Get("profile.notAvailable") ?? "Not available";
            string notProvided  = loc?.Get("profile.notProvided")  ?? "Not provided";

            string workerName = !string.IsNullOrWhiteSpace(state?.WorkerName)
                ? state.WorkerName
                : notAvailable;
            string workerId   = !string.IsNullOrWhiteSpace(state?.EmployeeId) ? state.EmployeeId : notAvailable;
            string workerRole = !string.IsNullOrWhiteSpace(state?.WorkerRole)
                ? state.WorkerRole
                : notProvided;

            var nameLbl = UIHelper.FindTMP(root, "label-worker-name");
            if (nameLbl != null) SetText(nameLbl, workerName);

            bool isGuest   = state != null && state.IsGuestMode;
            string idLabel = isGuest
                ? (loc?.Get("profile.guestIdLabel")    ?? "Guest ID:")
                : (loc?.Get("profile.employeeIdLabel") ?? "Employee ID:");
            string idDisplay = workerId.StartsWith("JH-") ? workerId
                : (workerId.StartsWith("M") ? $"JH-MN-00{workerId.TrimStart('M')}" : workerId);

            var idLbl = UIHelper.FindTMP(root, "label-worker-id");
            if (idLbl != null) SetText(idLbl, $"{idLabel} {idDisplay}");

            var roleLbl = UIHelper.FindTMP(root, "label-worker-role");
            if (roleLbl != null) SetText(roleLbl, workerRole);

            var siteLbl = UIHelper.FindTMP(root, "label-site");
            if (siteLbl != null)
                SetText(siteLbl, !string.IsNullOrWhiteSpace(state?.MineSite)
                    ? state.MineSite
                    : notProvided);

            var deptLbl = UIHelper.FindTMP(root, "label-department");
            if (deptLbl != null)
                SetText(deptLbl, loc?.Get("profile.department") ?? "Department of Mines, Jharkhand");

            // ── 2. Training & Assessment Statistics from Real State ─────────
            int completedModules = state != null ? state.CompletedModulesCount : 0;
            int earnedCerts = (state != null && state.IsPassed && !string.IsNullOrEmpty(state.CertificateId)) ? 1 : 0;
            float totalHours = completedModules * 1.5f;

            string completedLabel = loc?.Get("profile.completedLabel") ?? "Completed";
            string earnedLabel    = loc?.Get("profile.earnedLabel")    ?? "Earned";
            string hoursLabel     = loc?.Get("profile.hoursLabel")     ?? "Hours";

            var trainLbl = UIHelper.FindTMP(root, "label-stat-train");
            if (trainLbl != null) SetText(trainLbl, $"{completedModules} {completedLabel}");

            var certLbl = UIHelper.FindTMP(root, "label-stat-cert");
            if (certLbl != null) SetText(certLbl, $"{earnedCerts} {earnedLabel}");

            var hoursLbl = UIHelper.FindTMP(root, "label-stat-hours");
            if (hoursLbl != null) SetText(hoursLbl, $"{totalHours:0.0} {hoursLabel}");

            // ── 3. Localize list buttons & Edit pill ────────────────────────
            SetLabel(root, "EditLbl",                 loc?.Get("profile.edit")                ?? "Edit");
            SetLabel(root, "label-btn-personal",     loc?.Get("profile.personalInfo")        ?? "Personal Information");
            SetLabel(root, "label-sub-btn-personal", loc?.Get("profile.personalInfoSub")     ?? "Name, Contact, Department");
            SetLabel(root, "label-btn-safety",       loc?.Get("profile.safetyPreferences")    ?? "Safety Preferences");
            SetLabel(root, "label-sub-btn-safety",   loc?.Get("profile.safetyPreferencesSub") ?? "Language, Notifications");
            SetLabel(root, "label-btn-certs",        loc?.Get("profile.myCertificates")       ?? "My Certificates");
            SetLabel(root, "label-sub-btn-certs",    loc?.Get("profile.myCertificatesSub")    ?? "View and download your certificates");
            SetLabel(root, "label-btn-history",      loc?.Get("profile.trainingHistory")     ?? "Training History");
            SetLabel(root, "label-sub-btn-history",  loc?.Get("profile.trainingHistorySub")  ?? "Completed modules and scores");
            SetLabel(root, "label-btn-settings",     loc?.Get("profile.appSettings")         ?? "App Settings");
            SetLabel(root, "label-sub-btn-settings", loc?.Get("profile.appSettingsSub")      ?? "Sound, Privacy, Help");
            SetLabel(root, "label-btn-logout",       loc?.Get("profile.logout")              ?? "Logout");
            SetLabel(root, "label-sub-btn-logout",   loc?.Get("profile.logoutSub")          ?? "Sign out from this device");

            // ── 4. Modals Setup & Wiring ───────────────────────────────────
            var modalPersonal = UIHelper.FindRect(root, "PersonalInfoModal")?.gameObject;
            var modalSafety   = UIHelper.FindRect(root, "SafetyPreferencesModal")?.gameObject;
            var modalSettings = UIHelper.FindRect(root, "AppSettingsModal")?.gameObject;

            SetupPersonalInfoModal(root, modalPersonal, state, loc, idLabel, idDisplay, workerRole, notProvided);
            SetupSafetyPreferencesModal(root, modalSafety, loc);
            SetupAppSettingsModal(root, modalSettings, loc);

            // ── 5. Main Screen Buttons Wiring ──────────────────────────────
            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null)
            {
                _btnBack.onClick.RemoveAllListeners();
                _btnBack.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard));
            }

            _btnEdit = UIHelper.FindButton(root, "btn-edit");
            if (_btnEdit != null)
            {
                _btnEdit.onClick.RemoveAllListeners();
                _btnEdit.onClick.AddListener(() => OpenModal(modalPersonal));
            }

            _btnPersonal = UIHelper.FindButton(root, "btn-personal");
            if (_btnPersonal != null)
            {
                _btnPersonal.onClick.RemoveAllListeners();
                _btnPersonal.onClick.AddListener(() => OpenModal(modalPersonal));
            }

            _btnSafety = UIHelper.FindButton(root, "btn-safety");
            if (_btnSafety != null)
            {
                _btnSafety.onClick.RemoveAllListeners();
                _btnSafety.onClick.AddListener(() => OpenModal(modalSafety));
            }

            _btnCerts = UIHelper.FindButton(root, "btn-certs");
            if (_btnCerts != null)
            {
                _btnCerts.onClick.RemoveAllListeners();
                _btnCerts.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.Certificate));
            }

            _btnHistory = UIHelper.FindButton(root, "btn-history");
            if (_btnHistory != null)
            {
                _btnHistory.onClick.RemoveAllListeners();
                _btnHistory.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.Progress));
            }

            _btnSettings = UIHelper.FindButton(root, "btn-settings");
            if (_btnSettings != null)
            {
                _btnSettings.onClick.RemoveAllListeners();
                _btnSettings.onClick.AddListener(() => OpenModal(modalSettings));
            }

            // ── 6. Logout ──────────────────────────────────────────────────
            _btnLogout = UIHelper.FindButton(root, "btn-logout");
            if (_btnLogout != null)
            {
                _btnLogout.onClick.RemoveAllListeners();
                _btnLogout.onClick.AddListener(() => {
                    // Complete session teardown to prevent cross-user leakage
                    AuthSession.Instance?.ClearSession();
                    if (AppState.Instance != null)
                    {
                        AppState.Instance.SetUser("", "", true, "", "");
                        AppState.Instance.AssessmentScore = 0;
                        AppState.Instance.CertificateId = "";
                        AppState.Instance.CertificationDate = "";
                        AppState.Instance.CompletedModulesCount = 0;
                        AppState.Instance.LastARTimerSeconds = 0f;
                        AppState.Instance.CriticalErrorsCount = 0;
                        AppState.Instance.WrongActionsCount = 0;
                        AppState.Instance.FireExtinguishedSuccess = false;
                    }
                    UIManager.Instance?.ShowScreen(ScreenId.Login);
                });
            }

            // ── 7. Bottom Navigation ───────────────────────────────────────
            _navHome         = UIHelper.FindButton(root, "nav-home");
            _navLearn        = UIHelper.FindButton(root, "nav-learn");
            _navProgress     = UIHelper.FindButton(root, "nav-progress");
            _navCertificates = UIHelper.FindButton(root, "nav-certificates");

            if (_navHome         != null) { _navHome.onClick.RemoveAllListeners();         _navHome.onClick.AddListener(()         => UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard)); }
            if (_navLearn        != null) { _navLearn.onClick.RemoveAllListeners();        _navLearn.onClick.AddListener(()        => UIManager.Instance?.ShowScreen(ScreenId.ModuleSelection)); }
            if (_navProgress     != null) { _navProgress.onClick.RemoveAllListeners();     _navProgress.onClick.AddListener(()     => UIManager.Instance?.ShowScreen(ScreenId.Progress)); }
            if (_navCertificates != null) { _navCertificates.onClick.RemoveAllListeners(); _navCertificates.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.Certificate)); }

            // ── 8. Localize nav labels ─────────────────────────────────────
            SetLabel(root, "label-nav-home",         loc?.Get("home.navHome")         ?? "Home");
            SetLabel(root, "label-nav-learn",        loc?.Get("home.navLearn")        ?? "Learn");
            SetLabel(root, "label-nav-progress",     loc?.Get("home.navProgress")     ?? "My Progress");
            SetLabel(root, "label-nav-certificates", loc?.Get("home.navCertificates") ?? "Certificates");
        }

        private static void OpenModal(GameObject modal)
        {
            if (modal != null) modal.SetActive(true);
        }

        private static void CloseModal(GameObject modal)
        {
            if (modal != null) modal.SetActive(false);
        }

        private void SetupPersonalInfoModal(GameObject root, GameObject modal, AppState state, Localization.LocalizationManager loc, string idLabel, string idDisplay, string workerRole, string notProvided)
        {
            if (modal == null) return;

            // Localize modal headers and labels
            SetLabel(modal, "label-modal-personal-title", loc?.Get("profile.personalInfo") ?? "Personal Information");
            SetLabel(modal, "modal-label-edit-heading",   loc?.Get("profile.edit")         ?? "Edit Profile Details");

            // Populate Read-only details
            SetLabel(modal, "modal-field-worker-id",  $"{idLabel} {idDisplay}");
            SetLabel(modal, "modal-field-department", loc?.Get("profile.department") ?? "Department of Mines, Jharkhand");
            SetLabel(modal, "modal-field-role",        $"{loc?.Get("profile.defaultRole") ?? "Role"}: {workerRole}");
            SetLabel(modal, "modal-field-contact",     $"{loc?.Get("profile.contact") ?? "Contact"}: {notProvided}");

            // Input fields pre-fill
            var inputName = UIHelper.FindInputField(modal, "input-edit-name");
            var inputSite = UIHelper.FindInputField(modal, "input-edit-site");
            var inputRole = UIHelper.FindInputField(modal, "input-edit-role");

            if (inputName != null) inputName.text = state?.WorkerName ?? "";
            if (inputSite != null) inputSite.text = state?.MineSite ?? "";
            if (inputRole != null) inputRole.text = state?.WorkerRole ?? "";

            var statusLbl = UIHelper.FindTMP(modal, "label-edit-status");
            if (statusLbl != null) statusLbl.text = "";

            // Close buttons
            var closeBtn   = UIHelper.FindButton(modal, "btn-close-personal");
            var dismissBtn = UIHelper.FindButton(modal, "btn-dismiss-personal");
            var bgBtn      = UIHelper.FindButton(modal, "modal-bg-close-personal");

            if (closeBtn   != null) { closeBtn.onClick.RemoveAllListeners();   closeBtn.onClick.AddListener(()   => CloseModal(modal)); }
            if (dismissBtn != null) { dismissBtn.onClick.RemoveAllListeners(); dismissBtn.onClick.AddListener(() => CloseModal(modal)); }
            if (bgBtn      != null) { bgBtn.onClick.RemoveAllListeners();      bgBtn.onClick.AddListener(()      => CloseModal(modal)); }

            // Save button
            var saveBtn = UIHelper.FindButton(modal, "btn-save-profile");
            if (saveBtn != null)
            {
                saveBtn.onClick.RemoveAllListeners();
                saveBtn.onClick.AddListener(() =>
                {
                    string newName = inputName != null ? inputName.text.Trim() : "";
                    string newSite = inputSite != null ? inputSite.text.Trim() : "";
                    string newRole = inputRole != null ? inputRole.text.Trim() : "";

                    if (string.IsNullOrEmpty(newName)) newName = state?.WorkerName ?? "Worker";

                    if (state != null)
                    {
                        state.WorkerName = newName;
                        if (!string.IsNullOrEmpty(newSite)) state.MineSite = newSite;
                        if (!string.IsNullOrEmpty(newRole)) state.WorkerRole = newRole;
                    }

                    // Persist to SQLite / Local Database
                    if (LocalDatabaseService.Instance != null && state != null)
                    {
                        var userRec = new LocalDatabaseService.UserRecord
                        {
                            worker_id = state.EmployeeId,
                            name = newName,
                            mine_site = newSite,
                            role = newRole,
                            last_login = System.DateTime.UtcNow.ToString("o")
                        };
                        LocalDatabaseService.Instance.UpsertUser(userRec);
                    }

                    // Refresh main screen labels immediately
                    var nameMain = UIHelper.FindTMP(root, "label-worker-name");
                    if (nameMain != null) SetText(nameMain, newName);

                    var siteMain = UIHelper.FindTMP(root, "label-site");
                    if (siteMain != null && !string.IsNullOrEmpty(newSite)) SetText(siteMain, newSite);

                    var roleMain = UIHelper.FindTMP(root, "label-worker-role");
                    if (roleMain != null && !string.IsNullOrEmpty(newRole)) SetText(roleMain, newRole);

                    if (statusLbl != null)
                    {
                        statusLbl.text = loc?.Get("profile.saveSuccess") ?? "Profile updated successfully.";
                        statusLbl.color = UIColors.Hex("#059669");
                    }
                });
            }
        }

        private void SetupSafetyPreferencesModal(GameObject root, GameObject modal, Localization.LocalizationManager loc)
        {
            if (modal == null) return;

            SetLabel(modal, "label-modal-safety-title", loc?.Get("profile.safetyPreferences") ?? "Safety Preferences");

            var closeBtn = UIHelper.FindButton(modal, "btn-close-safety");
            var doneBtn  = UIHelper.FindButton(modal, "btn-done-safety");
            var bgBtn    = UIHelper.FindButton(modal, "modal-bg-close-safety");

            if (closeBtn != null) { closeBtn.onClick.RemoveAllListeners(); closeBtn.onClick.AddListener(() => CloseModal(modal)); }
            if (doneBtn  != null) { doneBtn.onClick.RemoveAllListeners();  doneBtn.onClick.AddListener(()  => CloseModal(modal)); }
            if (bgBtn    != null) { bgBtn.onClick.RemoveAllListeners();    bgBtn.onClick.AddListener(()    => CloseModal(modal)); }

            // Language pills
            WireLangButton(root, modal, "btn-safety-lang-en", AppLanguage.English);
            WireLangButton(root, modal, "btn-safety-lang-hi", AppLanguage.Hindi);
            WireLangButton(root, modal, "btn-safety-lang-sa", AppLanguage.Santali);

            // Audio Alerts Toggle
            WireToggle(modal, "toggle-safety-audio", "pref_safety_audio", true);
            // High Contrast Toggle
            WireToggle(modal, "toggle-high-contrast", "pref_high_contrast", false);
        }

        private void SetupAppSettingsModal(GameObject root, GameObject modal, Localization.LocalizationManager loc)
        {
            if (modal == null) return;

            SetLabel(modal, "label-modal-settings-title", loc?.Get("profile.appSettings") ?? "App Settings");
            SetLabel(modal, "label-settings-version",      loc?.Get("settings.version")     ?? "SurakshaAR v1.0.0 (Build 6000.6)");
            SetLabel(modal, "label-settings-privacy",      loc?.Get("settings.privacyNote") ?? "All training records are stored securely on-device with offline-first encryption.");

            var closeBtn = UIHelper.FindButton(modal, "btn-close-settings");
            var doneBtn  = UIHelper.FindButton(modal, "btn-done-settings");
            var bgBtn    = UIHelper.FindButton(modal, "modal-bg-close-settings");

            if (closeBtn != null) { closeBtn.onClick.RemoveAllListeners(); closeBtn.onClick.AddListener(() => CloseModal(modal)); }
            if (doneBtn  != null) { doneBtn.onClick.RemoveAllListeners();  doneBtn.onClick.AddListener(()  => CloseModal(modal)); }
            if (bgBtn    != null) { bgBtn.onClick.RemoveAllListeners();    bgBtn.onClick.AddListener(()    => CloseModal(modal)); }

            // Toggles
            WireToggle(modal, "toggle-settings-sound",  "pref_sound_enabled",   true);
            WireToggle(modal, "toggle-settings-haptic", "pref_haptic_enabled",  true);
            WireToggle(modal, "toggle-settings-sync",   "pref_autosync_enabled", true);
        }

        private void WireLangButton(GameObject root, GameObject modal, string btnName, AppLanguage lang)
        {
            var btn = UIHelper.FindButton(modal, btnName);
            if (btn == null) return;

            var currentLang = AppState.Instance != null ? AppState.Instance.CurrentLanguage : AppLanguage.English;
            bool isSelected = (currentLang == lang);
            var btnImg = btn.GetComponent<Image>();
            if (btnImg != null)
                btnImg.color = isSelected ? UIColors.Hex("#F0FDF4") : UIColors.Hex("#F8FAFC");
            var btnOutline = btn.GetComponent<Outline>();
            if (btnOutline != null)
                btnOutline.effectColor = isSelected ? UIColors.Hex("#059669") : UIColors.Hex("#CBD5E1");

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                if (AppState.Instance != null) AppState.Instance.SetLanguage(lang);
                if (AppManager.Instance?.Localization != null) AppManager.Instance.Localization.SetLanguage(lang);
                OnShow(root, null);
                OpenModal(modal);
            });
        }

        private static void WireToggle(GameObject modal, string toggleBtnName, string prefKey, bool defaultVal)
        {
            var btn = UIHelper.FindButton(modal, toggleBtnName);
            if (btn == null) return;

            bool isChecked = PlayerPrefs.GetInt(prefKey, defaultVal ? 1 : 0) == 1;
            UpdateToggleVisual(btn, isChecked);

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                bool newState = !(PlayerPrefs.GetInt(prefKey, defaultVal ? 1 : 0) == 1);
                PlayerPrefs.SetInt(prefKey, newState ? 1 : 0);
                PlayerPrefs.Save();
                UpdateToggleVisual(btn, newState);
            });
        }

        private static void UpdateToggleVisual(Button btn, bool isChecked)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = isChecked ? UIColors.Hex("#059669") : UIColors.Hex("#CBD5E1");

            var dot = btn.transform.Find("CheckIndicator") as RectTransform;
            if (dot != null)
            {
                dot.anchorMin = new Vector2(isChecked ? 0.72f : 0.28f, 0.5f);
                dot.anchorMax = new Vector2(isChecked ? 0.72f : 0.28f, 0.5f);
            }
        }

        private static void ApplyLanguageFonts(GameObject root, AppLanguage lang)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ApplyLanguageFonts(root, lang);
            }
            else
            {
                var font = UIHelper.GetFontForLanguage(lang);
                if (font == null) return;
                foreach (var tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
                    tmp.font = font;
            }
        }

        private static void SetLabel(GameObject root, string name, string text)
        {
            var tmp = UIHelper.FindTMP(root, name);
            if (tmp != null && !string.IsNullOrEmpty(text)) tmp.text = text;
        }

        private static void SetText(TextMeshProUGUI tmp, string text)
        {
            if (tmp == null || string.IsNullOrEmpty(text)) return;
            UIHelper.SetTMPText(tmp, text);
        }

        public void OnHide()
        {
        }
    }
}
