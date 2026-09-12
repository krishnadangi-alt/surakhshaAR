using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    /// <summary>
    /// Home Dashboard screen controller.
    /// Binds dynamic data (worker name, progress) and localized strings.
    /// </summary>
    public class HomeDashboardController : IScreenController
    {
        private Button _btnViewAll, _btnContinueTraining, _btnProgressCard;
        private Button _navHome, _navLearn, _navProgress, _navCertificates;
        private Button _cardFire, _cardGas, _cardMachinery;

        public void OnShow(GameObject root, object param)
        {
            var loc   = AppManager.Instance?.Localization;
            var state = AppState.Instance;

            string workerName = state != null ? state.WorkerName  : "Ramesh Kumar";
            string workerId   = state != null ? state.EmployeeId  : "JH-MN-004821";

            // ── Greeting & worker info ─────────────────────────────────────
            var labelGreeting = UIHelper.FindTMP(root, "label-greeting");
            if (labelGreeting != null)
                labelGreeting.text = loc != null ? loc.Get("home.greeting") : "Good Morning,";

            var labelWorkerName = UIHelper.FindTMP(root, "label-worker-name");
            if (labelWorkerName != null) labelWorkerName.text = workerName;

            var labelRole = UIHelper.FindTMP(root, "label-role");
            if (labelRole != null)
                labelRole.text = $"ID: {workerId} • Jharia Mine";

            // ── Progress stats ─────────────────────────────────────────────
            int completed = state != null ? state.CompletedModulesCount : 1;
            int total     = state != null ? state.TotalModulesCount     : 5;
            int pct       = total > 0 ? Mathf.RoundToInt((float)completed / total * 100f) : 62;

            var labelPct = UIHelper.FindTMP(root, "label-overall-pct");
            if (labelPct != null) labelPct.text = $"{pct}%";

            // ── Localize static labels ─────────────────────────────────────
            if (loc != null)
            {
                SetLabel(root, "label-hero-title",     loc.Get("home.overallProgress"));
                SetLabel(root, "label-modules-section",loc.Get("home.modulesSection"));
                SetLabel(root, "label-view-all",       loc.Get("home.viewAll"));

                // Nav bar labels (named "label-nav-*" by HomeDashboardBuilder.MakeNavItem)
                SetLabel(root, "label-nav-home",         loc.Get("home.navHome"));
                SetLabel(root, "label-nav-learn",        loc.Get("home.navLearn"));
                SetLabel(root, "label-nav-progress",     loc.Get("home.navProgress"));
                SetLabel(root, "label-nav-certificates", loc.Get("home.navCertificates"));
            }

            // ── Sync Status ───────────────────────────────────────────────
            _rootRef = root;
            if (OfflineSyncManager.Instance != null)
            {
                OfflineSyncManager.Instance.OnSyncStatusChanged += OnSyncStatusChanged;
                var syncLbl = UIHelper.FindTMP(root, "label-sync-text");
                if (syncLbl != null) syncLbl.text = OfflineSyncManager.Instance.StatusMessage;
            }

            // ── Buttons ────────────────────────────────────────────────────
            var langPicker = UIHelper.FindButton(root, "btn-language-picker");
            if (langPicker != null)
                langPicker.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.LanguageSelection));

            var profBtn = UIHelper.FindButton(root, "btn-profile");
            if (profBtn != null)
                profBtn.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.ProfileSetup));

            _btnViewAll = UIHelper.FindButton(root, "btn-view-all");
            if (_btnViewAll != null)
                _btnViewAll.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.ModuleSelection));

            _btnContinueTraining = UIHelper.FindButton(root, "btn-continue-training");
            if (_btnContinueTraining != null)
                _btnContinueTraining.onClick.AddListener(ContinueTraining);

            _btnProgressCard = UIHelper.FindButton(root, "card-progress");
            if (_btnProgressCard != null)
                _btnProgressCard.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.Progress));

            // Module cards
            _cardFire     = UIHelper.FindButton(root, "card-fire");
            _cardGas      = UIHelper.FindButton(root, "card-gas");
            _cardMachinery = UIHelper.FindButton(root, "card-machinery");

            if (_cardFire      != null) _cardFire.onClick.AddListener(()      => OpenModule(ModuleId.FireAndExplosion));
            if (_cardGas       != null) _cardGas.onClick.AddListener(()       => OpenModule(ModuleId.GasLeakConfinedSpace));
            if (_cardMachinery != null) _cardMachinery.onClick.AddListener(() => OpenModule(ModuleId.MachinerySafety));

            // Bottom navigation
            _navHome         = UIHelper.FindButton(root, "nav-home");
            _navLearn        = UIHelper.FindButton(root, "nav-learn");
            _navProgress     = UIHelper.FindButton(root, "nav-progress");
            _navCertificates = UIHelper.FindButton(root, "nav-certificates");

            if (_navHome         != null) _navHome.onClick.AddListener(()         => UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard));
            if (_navLearn        != null) _navLearn.onClick.AddListener(()        => UIManager.Instance?.ShowScreen(ScreenId.ModuleSelection));
            if (_navProgress     != null) _navProgress.onClick.AddListener(()     => UIManager.Instance?.ShowScreen(ScreenId.Progress));
            if (_navCertificates != null) _navCertificates.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.Certificate));
        }

        // ─────────────────────────────────────────────────────────────────
        //  HELPERS
        // ─────────────────────────────────────────────────────────────────
        private static void SetLabel(GameObject root, string name, string text)
        {
            var tmp = UIHelper.FindTMP(root, name);
            if (tmp != null && !string.IsNullOrEmpty(text)) tmp.text = text;
        }

        private void ContinueTraining()
        {
            var module = AppManager.Instance?.GetModule(ModuleId.FireAndExplosion);
            if (AppState.Instance != null && module != null)
                AppState.Instance.SelectedModule = module;
            UIManager.Instance?.ShowScreen(ScreenId.ModuleDetail, module);
        }

        private void OpenModule(ModuleId moduleId)
        {
            var module = AppManager.Instance?.GetModule(moduleId);
            if (module != null)
            {
                if (AppState.Instance != null) AppState.Instance.SelectedModule = module;
                UIManager.Instance?.ShowScreen(ScreenId.ModuleDetail, module);
            }
        }

        private GameObject _rootRef;

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
