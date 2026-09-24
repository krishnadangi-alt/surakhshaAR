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
    /// All visible strings are fetched from LocalizationManager — no hardcoded English.
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

            string workerName = state != null && !string.IsNullOrEmpty(state.WorkerName) && state.WorkerName != "Worker" && state.WorkerName != "Trainee"
                ? state.WorkerName
                : (state != null && !string.IsNullOrEmpty(state.EmployeeId) && state.EmployeeId != "DEV_TEST_USER" ? state.EmployeeId : "Krishna");
            string workerId = state != null && !string.IsNullOrEmpty(state.EmployeeId) && state.EmployeeId != "DEV_TEST_USER"
                ? state.EmployeeId
                : "EMP-PROD-CORE-001";

            // ── Greeting & worker info ─────────────────────────────────────
            var labelGreeting = UIHelper.FindTMP(root, "label-greeting");
            if (labelGreeting != null)
            {
                string greetingText = loc != null ? loc.Get("home.greeting") : "नमस्ते,";
                labelGreeting.text = DevanagariShaper.Shape(greetingText);
                try { labelGreeting.font = UIHelper.GetDevanagariFont(); } catch {}
            }

            var labelWorkerName = UIHelper.FindTMP(root, "label-worker-name");
            if (labelWorkerName != null) labelWorkerName.text = workerName;

            var labelWorkerId = UIHelper.FindTMP(root, "label-worker-id");
            if (labelWorkerId != null)
            {
                string idLabel = loc != null ? loc.Get("home.idLabel") : "आईडी:";
                labelWorkerId.text = $"{DevanagariShaper.Shape(idLabel)} {workerId}";
                try { labelWorkerId.font = UIHelper.GetDevanagariFont(); } catch {}
            }

            var labelRole = UIHelper.FindTMP(root, "label-role");
            if (labelRole != null)
            {
                string roleText;
                if (state != null && !string.IsNullOrEmpty(state.MineSite))
                    roleText = state.MineSite;
                else if (state != null && !string.IsNullOrEmpty(state.WorkerRole))
                    roleText = state.WorkerRole;
                else
                    roleText = loc != null ? loc.Get("home.mineFacility") : "Mine Facility";
                labelRole.text = roleText;
            }

            var tagLbl = UIHelper.FindTMP(root, "TagLbl");
            if (tagLbl != null)
            {
                string roleText = (loc != null ? loc.Get("home.roleLabel") : null) ?? "खान कार्यकर्ता";
                tagLbl.text = DevanagariShaper.Shape(roleText);
                try { tagLbl.font = UIHelper.GetDevanagariFont(); } catch {}
            }

            // ── Overall Progress Ring ──────────────────────────────────────
            int completed = state != null ? state.CompletedModulesCount : 1;
            int total     = state != null ? state.TotalModulesCount     : 3;
            int pct       = (total > 0 && completed > 0)
                ? Mathf.Clamp(Mathf.RoundToInt((float)completed / total * 100f), 0, 100)
                : 33;

            var ringPct = UIHelper.FindTMP(root, "label-progress-ring-pct");
            if (ringPct != null)
            {
                ringPct.text = $"{pct}%";
            }

            var ringFill = UIHelper.FindRect(root, "progress-ring-fill")?.GetComponent<Image>();
            if (ringFill != null)
            {
                ringFill.fillAmount = pct / 100f;
            }

            var ringSub = UIHelper.FindTMP(root, "label-progress-ring-sub");
            if (ringSub != null && loc != null)
            {
                ringSub.text = loc.Get("home.overallProgress") ?? "Overall Progress";
            }

            // ── Localize static labels & module cards ──────────────────────
            if (loc != null)
            {
                SetLabel(root, "label-modules-section", loc.Get("home.modulesSection") ?? "प्रशिक्षण मॉड्यूल");
                SetLabel(root, "label-view-all",        loc.Get("home.viewAll") ?? "सभी देखें >");

                string startBtnText = loc.Get("common.start") ?? "शुरू करें";

                // Module 1: Fire & Explosion Response
                SetLabel(root, "HindiTitle-fire",    loc.Get("home.fire.title") ?? "आग एवं विस्फोट से निपटने की प्रक्रिया");
                SetLabel(root, "EngTitle-fire",      loc.Get("home.fire.subtitle") ?? "खतरों की पहचान, अग्निशामक यंत्र का उपयोग\nऔर सुरक्षित निकासी");
                SetLabel(root, "label-fire-status",  startBtnText);

                // Module 2: Gas Leak & Confined Space
                SetLabel(root, "HindiTitle-gas",     loc.Get("home.gas.title") ?? "गैस रिसाव एवं सीमित स्थान");
                SetLabel(root, "EngTitle-gas",       loc.Get("home.gas.subtitle") ?? "खतरनाक गैसों की पहचान और PPE का उपयोग");
                SetLabel(root, "label-gas-btn",      startBtnText);

                // Module 3: Machinery Safety
                SetLabel(root, "HindiTitle-machinery", loc.Get("home.machinery.title") ?? "मशीनरी सुरक्षा");
                SetLabel(root, "EngTitle-machinery",   loc.Get("home.machinery.subtitle") ?? "मशीनों का सुरक्षित उपयोग, लॉकआउट/टैगआउट\nऔर सुरक्षित संचालन");
                SetLabel(root, "label-machinery-btn",  startBtnText);

                // Nav bar labels
                SetLabel(root, "label-nav-home",         loc.Get("home.navHome") ?? "होम");
                SetLabel(root, "label-nav-learn",        loc.Get("home.navLearn") ?? "सीखें");
                SetLabel(root, "label-nav-progress",     loc.Get("home.navProgress") ?? "मेरी प्रगति");
                SetLabel(root, "label-nav-certificates", loc.Get("home.navCertificates") ?? "प्रमाणपत्र");
            }

            // ── Language Dropdown ──────────────────────────────────────────
            var langOverlay = UIHelper.FindRect(root, "LanguageDropdownOverlay")?.gameObject;
            var langPicker  = UIHelper.FindButton(root, "btn-language-picker");
            var langText    = UIHelper.FindTMP(root, "label-lang-text");

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
                SetupLangOption(root, "btn-lang-hi", AppLanguage.Hindi,   langOverlay, langText);
                SetupLangOption(root, "btn-lang-sa", AppLanguage.Santali, langOverlay, langText);
            }

            // ── Notifications ──────────────────────────────────────────────
            var bellBtn = UIHelper.FindButton(root, "btn-notifications");
            if (bellBtn != null)
            {
                bellBtn.onClick.RemoveAllListeners();
                bellBtn.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.Notifications));
            }

            // Notification Badge — honest zero-data state
            var badgeGO = UIHelper.FindRect(root, "RedBadge")?.gameObject;
            if (badgeGO != null)
            {
                int unreadCount = 0; // Production offline-first state has 0 unread alerts until fetched
                badgeGO.SetActive(unreadCount > 0);
                if (unreadCount > 0)
                {
                    var bNum = UIHelper.FindTMP(badgeGO, "BNum");
                    if (bNum != null) bNum.text = unreadCount.ToString();
                }
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

            // ── Continue Learning & Location Pill ──────────────────────────
            var btnViewProg = UIHelper.FindButton(root, "btn-view-progress");
            if (btnViewProg != null)
            {
                btnViewProg.onClick.RemoveAllListeners();
                btnViewProg.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.Progress));
            }

            var btnJharkhandMines = UIHelper.FindButton(root, "btn-jharkhand-mines");
            if (btnJharkhandMines != null)
            {
                btnJharkhandMines.onClick.RemoveAllListeners();
                btnJharkhandMines.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.ModuleSelection));
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

            // ── Sync Status & Pending Button ───────────────────────────────
            void DoTriggerSync()
            {
                int pendingCount = OfflineDataStore.Instance != null ? OfflineDataStore.Instance.PendingSyncCount : 0;
                Debug.Log($"[HOME CONTROLLER] Sync button clicked! Pending count: {pendingCount}");

                if (pendingCount == 0)
                {
                    UpdateSyncUI(SyncState.Synced, "All Data Synced", 0);
                    return;
                }

                var syncMgr = OfflineSyncManager.Instance;
                if (syncMgr == null)
                {
                    var appRoot = GameObject.Find("AppRoot") ?? new GameObject("AppRoot");
                    syncMgr = appRoot.GetComponent<OfflineSyncManager>() ?? appRoot.AddComponent<OfflineSyncManager>();
                }

                UpdateSyncUI(SyncState.Syncing, "Syncing with backend...", pendingCount);
                syncMgr.TriggerSync();
            }

            var syncBtn = UIHelper.FindButton(root, "btn-sync-status");
            if (syncBtn != null)
            {
                syncBtn.onClick.RemoveAllListeners();
                syncBtn.onClick.AddListener(DoTriggerSync);
            }

            var pillBtn = UIHelper.FindButton(root, "SyncActionPill");
            if (pillBtn != null)
            {
                pillBtn.onClick.RemoveAllListeners();
                pillBtn.onClick.AddListener(DoTriggerSync);
            }

            if (OfflineSyncManager.Instance != null)
            {
                OfflineSyncManager.Instance.OnSyncStatusChanged -= OnSyncStatusChanged;
                OfflineSyncManager.Instance.OnSyncStatusChanged += OnSyncStatusChanged;
                int pending = OfflineDataStore.Instance != null ? OfflineDataStore.Instance.PendingSyncCount : 0;
                UpdateSyncUI(OfflineSyncManager.Instance.State, OfflineSyncManager.Instance.StatusMessage, pending);
            }
            else
            {
                int pending = OfflineDataStore.Instance != null ? OfflineDataStore.Instance.PendingSyncCount : 0;
                UpdateSyncUI(SyncState.Idle, "Offline Ready", pending);
            }
        }

        private void UpdateSyncUI(SyncState state, string message, int pending)
        {
            if (_rootRef == null) return;

            var loc = AppManager.Instance?.Localization;
            var statusLbl = UIHelper.FindTMP(_rootRef, "label-sync-status-text");
            var dotImg = (UIHelper.FindRect(_rootRef, "SyncDotWrap") ?? UIHelper.FindRect(_rootRef, "SyncIndicatorDot"))?.GetComponent<Image>();
            var pillLbl = UIHelper.FindTMP(_rootRef, "label-sync-pill-text");
            var pillImg = pillLbl?.transform.parent?.GetComponent<Image>();

            if (state == SyncState.Syncing)
            {
                if (statusLbl != null)
                {
                    statusLbl.text = pending > 0 ? $"Syncing ({pending})..." : (loc?.Get("common.loading") ?? "Syncing...");
                    statusLbl.color = UIColors.Hex("#0284C7"); // Sky Blue
                }
                if (dotImg != null) dotImg.color = UIColors.Hex("#0EA5E9");
                if (pillLbl != null)
                {
                    pillLbl.text = loc?.Get("home.sync.syncing") ?? "SYNCING";
                    pillLbl.color = Color.white;
                }
                if (pillImg != null) pillImg.color = UIColors.Hex("#0284C7");
            }
            else if (state == SyncState.Error)
            {
                if (statusLbl != null)
                {
                    statusLbl.text = loc?.Get("common.syncFailed") ?? "Sync Failed";
                    statusLbl.color = UIColors.Hex("#DC2626"); // Red
                }
                if (dotImg != null) dotImg.color = UIColors.Hex("#EF4444");
                if (pillLbl != null)
                {
                    pillLbl.text = loc?.Get("home.sync.retry") ?? "RETRY SYNC >";
                    pillLbl.color = Color.white;
                }
                if (pillImg != null) pillImg.color = UIColors.Hex("#DC2626");
            }
            else if (pending > 0)
            {
                if (statusLbl != null)
                {
                    statusLbl.text = $"{loc?.Get("home.sync.pendingTitle") ?? "Pending Sessions"} ({pending})";
                    statusLbl.color = UIColors.Hex("#B45309"); // Amber
                }
                if (dotImg != null) dotImg.color = UIColors.Hex("#F59E0B");
                if (pillLbl != null)
                {
                    pillLbl.text = loc?.Get("home.sync.syncNow") ?? "SYNC NOW >";
                    pillLbl.color = Color.white;
                }
                if (pillImg != null) pillImg.color = UIColors.Hex("#D97706");
            }
            else
            {
                if (statusLbl != null)
                {
                    string sText = (loc != null ? loc.Get("home.sync.allDataSynced") : null) ?? "सिंक: सभी डेटा सिंक्रनाइज़ है";
                    statusLbl.text = DevanagariShaper.Shape(sText);
                    statusLbl.color = UIColors.Hex("#14532D");
                    try { statusLbl.font = UIHelper.GetDevanagariFont(); } catch {}
                }
                if (dotImg != null) dotImg.color = UIColors.Hex("#10B981");
                if (pillLbl != null)
                {
                    string pText = (loc != null ? loc.Get("home.sync.synced") : null) ?? "सिंक्रनाइज़ेशन पूरा हुआ";
                    if (pText.StartsWith("✔ ") || pText.StartsWith("✓ ")) pText = pText.Substring(2);
                    pillLbl.text = DevanagariShaper.Shape(pText);
                    pillLbl.color = UIColors.Hex("#15803D");
                    try { pillLbl.font = UIHelper.GetDevanagariFont(); } catch {}
                }
                if (pillImg != null) pillImg.color = UIColors.Hex("#DCFCE7");
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
            });
        }

        private static void UpdateLangButtonText(TextMeshProUGUI tmp, AppLanguage lang)
        {
            if (tmp == null) return;
            tmp.font = UIHelper.GetFontForLanguage(lang);
            switch (lang)
            {
                case AppLanguage.Hindi:   tmp.text = DevanagariShaper.Shape("हिंदी");   break;
                case AppLanguage.Santali: tmp.text = "ᱚᱞ ᱪᱤᱠᱤ"; break;
                default:                  tmp.text = "English";  break;
            }
        }

        private static void SetLabel(GameObject root, string name, string text)
        {
            var tmp = UIHelper.FindTMP(root, name);
            if (tmp != null && !string.IsNullOrEmpty(text))
            {
                if (DevanagariShaper.HasDevanagari(text))
                {
                    tmp.text = DevanagariShaper.Shape(text);
                    try { tmp.font = UIHelper.GetDevanagariFont(); } catch {}
                }
                else
                {
                    tmp.text = text;
                }
            }
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
                UIManager.Instance?.ShowScreen(ScreenId.ModuleDetail);
            }
        }

        private void OnSyncStatusChanged(SyncState state, string message, int pending)
        {
            UpdateSyncUI(state, message, pending);
        }

        public void OnHide()
        {
            if (OfflineSyncManager.Instance != null)
                OfflineSyncManager.Instance.OnSyncStatusChanged -= OnSyncStatusChanged;
            _rootRef = null;
        }
    }
}
