using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.Localization;
using SurakshaAR.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    /// <summary>
    /// Home Dashboard screen controller.
    /// Binds dynamic data (worker name, ID, language, notifications) and navigation.
    /// </summary>
    public class HomeDashboardController : IScreenController
    {
        private Button _btnViewAll;
        private Button _navHome, _navLearn, _navProgress, _navCertificates;
        private Button _cardFire, _cardGas, _cardMachinery;
        private GameObject _rootRef;

        public void OnShow(GameObject root, object param)
        {
            _rootRef = root;
            var loc   = AppManager.Instance?.Localization;
            var state = AppState.Instance;

            string workerName = state != null && !string.IsNullOrEmpty(state.WorkerName)
                ? state.WorkerName
                : (state != null && !string.IsNullOrEmpty(state.EmployeeId) ? state.EmployeeId : "Worker");
            string workerId   = state != null && !string.IsNullOrEmpty(state.EmployeeId) ? state.EmployeeId : "Trainee";

            // ── Greeting & worker info ─────────────────────────────────────
            var labelGreeting = UIHelper.FindTMP(root, "label-greeting");
            if (labelGreeting != null)
                labelGreeting.text = "Welcome,";

            var labelWorkerName = UIHelper.FindTMP(root, "label-worker-name");
            if (labelWorkerName != null) labelWorkerName.text = workerName;

            var labelWorkerId = UIHelper.FindTMP(root, "label-worker-id");
            if (labelWorkerId != null) labelWorkerId.text = $"ID: {workerId}";

            var labelRole = UIHelper.FindTMP(root, "label-role");
            if (labelRole != null)
                labelRole.text = state != null && !string.IsNullOrEmpty(state.MineSite)
                    ? state.MineSite
                    : (state != null ? state.WorkerRole : "Mine Safety Trainee");

            // ── Localize static labels ─────────────────────────────────────
            if (loc != null)
            {
                SetLabel(root, "label-modules-section", loc.Get("home.modulesSection"));
                SetLabel(root, "label-view-all",        loc.Get("home.viewAll"));

                // Nav bar labels
                SetLabel(root, "label-nav-home",         loc.Get("home.navHome"));
                SetLabel(root, "label-nav-learn",        loc.Get("home.navLearn"));
                SetLabel(root, "label-nav-progress",     loc.Get("home.navProgress"));
                SetLabel(root, "label-nav-certificates", loc.Get("home.navCertificates"));
            }

            // ── Language Dropdown ──────────────────────────────────────────
            var langOverlay = UIHelper.FindRect(root, "LanguageDropdownOverlay")?.gameObject;
            var langPicker = UIHelper.FindButton(root, "btn-language-picker");
            var langText = UIHelper.FindTMP(root, "label-lang-text");

            var currentLang = state != null ? state.CurrentLanguage : (loc != null ? loc.CurrentLanguage : AppLanguage.English);
            UpdateLangButtonText(langText, currentLang);

            if (langPicker != null && langOverlay != null)
            {
                langPicker.onClick.RemoveAllListeners();
                langPicker.onClick.AddListener(() =>
                {
                    langOverlay.SetActive(!langOverlay.activeSelf);
                });

                var overlayBgBtn = langOverlay.GetComponent<Button>();
                if (overlayBgBtn != null)
                {
                    overlayBgBtn.onClick.RemoveAllListeners();
                    overlayBgBtn.onClick.AddListener(() => langOverlay.SetActive(false));
                }

                SetupLangOption(root, "btn-lang-en", AppLanguage.English, langOverlay, langText);
                SetupLangOption(root, "btn-lang-hi", AppLanguage.Hindi, langOverlay, langText);
                SetupLangOption(root, "btn-lang-sa", AppLanguage.Santali, langOverlay, langText);
            }

            // ── Notifications ──────────────────────────────────────────────
            var bellBtn = UIHelper.FindButton(root, "btn-notifications");
            if (bellBtn != null)
            {
                bellBtn.onClick.RemoveAllListeners();
                bellBtn.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.Notifications));
            }

            // Notification Badge: Show count 3 matching reference image
            var badgeGO = UIHelper.FindRect(root, "RedBadge")?.gameObject;
            if (badgeGO != null)
            {
                int unreadCount = 3;
                badgeGO.SetActive(true);
                var bNum = UIHelper.FindTMP(badgeGO, "BNum");
                if (bNum != null) bNum.text = unreadCount.ToString();
            }

            // ── Profile Button & Card ──────────────────────────────────────
            var profBtn = UIHelper.FindButton(root, "btn-profile");
            if (profBtn != null)
            {
                profBtn.onClick.RemoveAllListeners();
                profBtn.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.ProfileSetup));
            }

            var workerCardBtn = UIHelper.FindButton(root, "WorkerProfileCard");
            if (workerCardBtn != null)
            {
                workerCardBtn.onClick.RemoveAllListeners();
                workerCardBtn.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.ProfileSetup));
            }

            // ── Module Cards ───────────────────────────────────────────────
            _btnViewAll = UIHelper.FindButton(root, "btn-view-all");
            if (_btnViewAll != null)
            {
                _btnViewAll.onClick.RemoveAllListeners();
                _btnViewAll.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.ModuleSelection));
            }

            _cardFire = UIHelper.FindButton(root, "card-fire");
            if (_cardFire != null)
            {
                _cardFire.onClick.RemoveAllListeners();
                _cardFire.onClick.AddListener(() => OpenModule(ModuleId.FireAndExplosion));
            }

            var fireBadgeBtn = UIHelper.FindButton(root, "btn-badge");
            if (fireBadgeBtn != null)
            {
                fireBadgeBtn.onClick.RemoveAllListeners();
                fireBadgeBtn.onClick.AddListener(() => OpenModule(ModuleId.FireAndExplosion));
            }

            _cardGas = UIHelper.FindButton(root, "card-gas");
            if (_cardGas != null)
            {
                _cardGas.onClick.RemoveAllListeners();
                _cardGas.onClick.AddListener(() => OpenModule(ModuleId.GasLeakConfinedSpace));
            }

            _cardMachinery = UIHelper.FindButton(root, "card-machinery");
            if (_cardMachinery != null)
            {
                _cardMachinery.onClick.RemoveAllListeners();
                _cardMachinery.onClick.AddListener(() => OpenModule(ModuleId.MachinerySafety));
            }

            // ── Bottom Navigation ──────────────────────────────────────────
            _navHome         = UIHelper.FindButton(root, "nav-home");
            _navLearn        = UIHelper.FindButton(root, "nav-learn");
            _navProgress     = UIHelper.FindButton(root, "nav-progress");
            _navCertificates = UIHelper.FindButton(root, "nav-certificates");

            if (_navHome         != null) { _navHome.onClick.RemoveAllListeners(); _navHome.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard)); }
            if (_navLearn        != null) { _navLearn.onClick.RemoveAllListeners(); _navLearn.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.ModuleSelection)); }
            if (_navProgress     != null) { _navProgress.onClick.RemoveAllListeners(); _navProgress.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.Progress)); }
            if (_navCertificates != null) { _navCertificates.onClick.RemoveAllListeners(); _navCertificates.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.Certificate)); }

            // ── Sync Status ───────────────────────────────────────────────
            if (OfflineSyncManager.Instance != null)
            {
                OfflineSyncManager.Instance.OnSyncStatusChanged += OnSyncStatusChanged;
                var syncLbl = UIHelper.FindTMP(root, "label-sync-text");
                if (syncLbl != null) syncLbl.text = OfflineSyncManager.Instance.StatusMessage;
            }
        }

        private void SetupLangOption(GameObject root, string btnName, AppLanguage lang, GameObject overlay, TextMeshProUGUI langText)
        {
            var btn = UIHelper.FindButton(root, btnName);
            if (btn == null) return;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                overlay.SetActive(false);
                if (AppState.Instance != null) AppState.Instance.SetLanguage(lang);
                if (AppManager.Instance?.Localization != null) AppManager.Instance.Localization.SetLanguage(lang);
                UpdateLangButtonText(langText, lang);
                // Re-render strings
                OnShow(root, null);
            });
        }

        private static void UpdateLangButtonText(TextMeshProUGUI tmp, AppLanguage lang)
        {
            if (tmp == null) return;
            switch (lang)
            {
                case AppLanguage.Hindi:   tmp.text = "हिंदी"; break;
                case AppLanguage.Santali: tmp.text = "Santali"; break;
                default:                  tmp.text = "English"; break;
            }
        }

        private static void SetLabel(GameObject root, string name, string text)
        {
            var tmp = UIHelper.FindTMP(root, name);
            if (tmp != null && !string.IsNullOrEmpty(text)) tmp.text = text;
        }

        private void OpenModule(ModuleId moduleId)
        {
            var module = AppManager.Instance?.GetModule(moduleId);
            if (module != null)
            {
                if (AppState.Instance != null) AppState.Instance.SelectedModule = module;
                UIManager.Instance?.ShowScreen(ScreenId.ModuleDetail, module);
            }
            else
            {
                // Direct fallback for fire training
                UIManager.Instance?.ShowScreen(ScreenId.ModuleDetail);
            }
        }

        private void OnSyncStatusChanged(SyncState state, string message, int pending)
        {
            if (_rootRef == null) return;
            var syncLbl = UIHelper.FindTMP(_rootRef, "label-sync-text");
            if (syncLbl != null)
            {
                syncLbl.text = message;
            }
        }

        public void OnHide()
        {
            if (OfflineSyncManager.Instance != null)
            {
                OfflineSyncManager.Instance.OnSyncStatusChanged -= OnSyncStatusChanged;
            }
            _rootRef = null;
        }
    }
}
