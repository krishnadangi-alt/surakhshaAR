using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.Localization;
using SurakshaAR.UI;
using SurakshaAR.UI.Builders;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    /// <summary>
    /// GasModuleDetailController — Gas Scenario Detail screen.
    ///
    /// ALL gas modules are COMING SOON. The UI displays the scenario information
    /// but the Start Training button is disabled and shows "Coming Soon".
    /// Includes full 4-tab bottom navigation bar support.
    ///
    /// Scenarios indexed 101/102/103 (from GasScenarioSelectionController):
    ///   101 — Underground Gas Release
    ///   102 — Confined Space Entry
    ///   103 — Gas Cylinder Leak
    /// </summary>
    public class GasModuleDetailController : IScreenController
    {
        private Button _btnBack, _btnStart;
        private Button _navHome, _navLearn, _navProgress, _navCertificates;
        private int _gasScenarioIndex = 1;

        public void OnShow(GameObject root, object param)
        {
            if (AppState.Instance != null)
            {
                int raw = AppState.Instance.SelectedScenarioIndex;
                _gasScenarioIndex = (raw >= 101 && raw <= 103) ? raw - 100 : 1;
            }

            var loc = AppManager.Instance?.Localization;
            var currentLang = AppState.Instance != null
                ? AppState.Instance.CurrentLanguage
                : (loc != null ? loc.CurrentLanguage : AppLanguage.English);

            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null)
            {
                _btnBack.onClick.RemoveAllListeners();
                _btnBack.onClick.AddListener(GoBack);
            }

            _btnStart = UIHelper.FindButton(root, "btn-start-ar-training");
            if (_btnStart != null)
            {
                _btnStart.onClick.RemoveAllListeners();
                _btnStart.onClick.AddListener(LaunchGasARScenario);
            }

            // Language Picker Pill: Cycle English -> Hindi -> Santali -> English
            var langPill = UIHelper.FindButton(root, "btn-language-picker");
            if (langPill != null)
            {
                var pillText = langPill.GetComponentInChildren<TextMeshProUGUI>();
                if (pillText != null)
                {
                    pillText.font = UIHelper.GetFontForLanguage(currentLang);
                    switch (currentLang)
                    {
                        case AppLanguage.Hindi:
                            pillText.text = DevanagariShaper.Shape("हिन्दी");
                            break;
                        case AppLanguage.Santali:
                            pillText.text = "ᱥᱟᱱᱛᱟᱲᱤ";
                            break;
                        default:
                            pillText.text = "English";
                            break;
                    }
                }

                langPill.onClick.RemoveAllListeners();
                langPill.onClick.AddListener(() =>
                {
                    var nextLang = currentLang switch
                    {
                        AppLanguage.English => AppLanguage.Hindi,
                        AppLanguage.Hindi   => AppLanguage.Santali,
                        _                   => AppLanguage.English
                    };
                    if (AppState.Instance != null) AppState.Instance.SetLanguage(nextLang);
                    else AppManager.Instance?.Localization?.SetLanguage(nextLang);
                });
            }

            // Bottom Navigation
            _navHome         = UIHelper.FindButton(root, "nav-home");
            _navLearn        = UIHelper.FindButton(root, "nav-learn");
            _navProgress     = UIHelper.FindButton(root, "nav-progress");
            _navCertificates = UIHelper.FindButton(root, "nav-certificates");

            if (_navHome         != null) { _navHome.onClick.RemoveAllListeners(); _navHome.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard)); }
            if (_navLearn        != null) { _navLearn.onClick.RemoveAllListeners(); _navLearn.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.ModuleSelection)); }
            if (_navProgress     != null) { _navProgress.onClick.RemoveAllListeners(); _navProgress.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.Progress)); }
            if (_navCertificates != null) { _navCertificates.onClick.RemoveAllListeners(); _navCertificates.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.Certificate)); }

            // Localize Bottom Nav Labels
            if (loc != null)
            {
                var hLbl = UIHelper.FindTMP(root, "label-nav-home") ?? _navHome?.GetComponentInChildren<TextMeshProUGUI>();
                if (hLbl != null) hLbl.text = loc.Get("nav.home");
                var lLbl = UIHelper.FindTMP(root, "label-nav-learn") ?? _navLearn?.GetComponentInChildren<TextMeshProUGUI>();
                if (lLbl != null) lLbl.text = loc.Get("nav.learn");
                var pLbl = UIHelper.FindTMP(root, "label-nav-progress") ?? _navProgress?.GetComponentInChildren<TextMeshProUGUI>();
                if (pLbl != null) pLbl.text = loc.Get("nav.progress");
                var cLbl = UIHelper.FindTMP(root, "label-nav-certificates") ?? _navCertificates?.GetComponentInChildren<TextMeshProUGUI>();
                if (cLbl != null) cLbl.text = loc.Get("nav.certificates");
            }

            BindScenarioData(root);
        }

        // ──────────────────────────────────────────────────────────────────
        private void BindScenarioData(GameObject root)
        {
            var loc = AppManager.Instance?.Localization;
            string title, subtitle, scenarioNumberBadge, duration, level,
                   focusVal, focusSub, overview;
            string[] steps;

            if (_gasScenarioIndex == 2)
            {
                // Scenario 2 — Confined Space Entry
                title               = loc?.Get("scenario.gas.confined.title") ?? "Confined Space Entry";
                subtitle            = loc?.Get("scenario.gas.confined.desc") ?? "Follow safe entry procedures and use gas detection & PPE.";
                scenarioNumberBadge = "02";
                duration            = loc?.Get("chip.val.12mins") ?? "~ 12 mins";
                level               = loc?.Get("chip.val.intermediate") ?? "Intermediate";
                focusVal            = loc?.Get("chip.val.ppe_use") ?? "PPE Use";
                focusSub            = loc?.Get("chip.sub.entry") ?? "Entry";
                overview            = loc?.Get("scenario.gas.confined.overview") ?? "Learn the safe entry procedure for confined spaces, use gas detection equipment, verify atmospheric conditions and follow PPE requirements.";
                steps = new string[]
                {
                    loc?.Get("gas.confined.step1") ?? "Identify confined space hazards",
                    loc?.Get("gas.confined.step2") ?? "Perform atmospheric gas testing",
                    loc?.Get("gas.confined.step3") ?? "Check oxygen, toxic and flammable gases",
                    loc?.Get("gas.confined.step4") ?? "Use appropriate PPE",
                    loc?.Get("gas.confined.step5") ?? "Follow entry and work permit procedure",
                    loc?.Get("gas.confined.step6") ?? "Safe exit and emergency response"
                };
            }
            else if (_gasScenarioIndex == 3)
            {
                // Scenario 3 — Gas Leak Response & Emergency Isolation
                title               = "Gas Leak Response & Emergency Isolation";
                subtitle            = "Simulate a gas leak incident, detect multi-gas levels, wear required PPE, verify standby buddy, and isolate the gas cylinder.";
                scenarioNumberBadge = "03";
                duration            = loc?.Get("chip.val.10mins") ?? "~ 10 mins";
                level               = loc?.Get("chip.val.beginner") ?? "Beginner";
                focusVal            = loc?.Get("chip.val.isolation") ?? "Isolation";
                focusSub            = loc?.Get("chip.sub.procedure") ?? "Procedure";
                overview            = "Learn to detect gas release from cylinder & pipeline, verify multi-gas detector PPM levels, select appropriate SCBA/PPE, verify with standby buddy, and safely isolate the red cylinder valve.";
                steps = new string[]
                {
                    "1. Scan surface and place Gas workstation",
                    "2. Detect gas leak plume & audio hiss from cylinder",
                    "3. Tap Multi-Gas Detector to verify hazard reading",
                    "4. Recognize hazard area and select appropriate PPE",
                    "5. Confirm standby buddy and tap Red Isolation Valve",
                    "6. Verify safe state reset and proceed to assessment"
                };
            }
            else
            {
                // Scenario 1 — Underground Gas Release (default)
                title               = loc?.Get("scenario.gas.underground.title") ?? "Underground Gas Release";
                subtitle            = loc?.Get("scenario.gas.underground.desc") ?? "Recognize a gas release, raise the alarm and move to a safe area.";
                scenarioNumberBadge = "01";
                duration            = loc?.Get("chip.val.10mins") ?? "~ 10 mins";
                level               = loc?.Get("chip.val.beginner") ?? "Beginner";
                focusVal            = loc?.Get("chip.val.gas_detector") ?? "Gas Detector";
                focusSub            = loc?.Get("chip.sub.use") ?? "Use";
                overview            = loc?.Get("scenario.gas.underground.overview") ?? "A gas leak has been detected in an underground mine area. Learn to identify the leak, raise the alarm, communicate the emergency and move to a safe area following proper procedures.";
                steps = new string[]
                {
                    "1. Scan surface and place Gas workstation",
                    "2. Detect gas leak plume & audio hiss from cylinder",
                    "3. Tap Multi-Gas Detector to verify hazard reading",
                    "4. Recognize hazard area and select appropriate PPE",
                    "5. Confirm standby buddy and tap Red Isolation Valve",
                    "6. Verify safe state reset and proceed to assessment"
                };
            }

            // ── Bind all labels ─────────────────────────────────────────
            UIHelper.FindTMP(root, "label-title")?.SetText(title);
            UIHelper.FindTMP(root, "label-scenario-title")?.SetText(title);
            UIHelper.FindTMP(root, "label-scenario-desc")?.SetText(subtitle);
            UIHelper.FindTMP(root, "label-hero-badge")?.SetText(scenarioNumberBadge);
            UIHelper.FindTMP(root, "label-overview-body")?.SetText(overview);

            // Stat chips
            var dur = UIHelper.FindRect(root, "chip-duration")?.Find("Inner/TextCol/Value")?.GetComponent<TextMeshProUGUI>();
            if (dur != null) dur.text = duration;
            var durSub = UIHelper.FindRect(root, "chip-duration")?.Find("Inner/TextCol/Sub")?.GetComponent<TextMeshProUGUI>();
            if (durSub != null && loc != null) durSub.text = loc.Get("chip.label.duration");

            var lev = UIHelper.FindRect(root, "chip-level")?.Find("Inner/TextCol/Value")?.GetComponent<TextMeshProUGUI>();
            if (lev != null) lev.text = level;
            var levSub = UIHelper.FindRect(root, "chip-level")?.Find("Inner/TextCol/Sub")?.GetComponent<TextMeshProUGUI>();
            if (levSub != null && loc != null) levSub.text = loc.Get("chip.label.level");

            var foc = UIHelper.FindRect(root, "chip-focus")?.Find("Inner/TextCol/Value")?.GetComponent<TextMeshProUGUI>();
            if (foc != null) foc.text = focusVal;

            var focSub = UIHelper.FindRect(root, "chip-focus")?.Find("Inner/TextCol/Sub")?.GetComponent<TextMeshProUGUI>();
            if (focSub != null) focSub.text = focusSub;

            // Section Headings
            var overviewHead = UIHelper.FindTMP(root, "OverviewHead");
            if (overviewHead != null && loc != null) overviewHead.text = loc.Get("ui.overview.heading");

            var trainingHead = UIHelper.FindTMP(root, "TrainingHead");
            if (trainingHead != null && loc != null) trainingHead.text = loc.Get("ui.training.include");

            // Start Training Button configuration
            if (_btnStart != null)
            {
                var btnImg = _btnStart.GetComponent<Image>();
                var btnTxt = _btnStart.GetComponentInChildren<TextMeshProUGUI>();

                if (_gasScenarioIndex == 3 || _gasScenarioIndex == 1)
                {
                    _btnStart.interactable = true;
                    if (btnImg != null) btnImg.color = UIColors.Hex("#EA580C");
                    if (btnTxt != null) btnTxt.text = loc?.Get("ui.start_training") ?? "Start AR Training";
                }
                else
                {
                    _btnStart.interactable = false;
                    if (btnImg != null) btnImg.color = UIColors.Hex("#94A3B8");
                    if (btnTxt != null) btnTxt.text = loc?.Get("ui.coming_soon") ?? "Coming Soon";
                }
            }

            // Steps list — rebuild
            var stepsList = UIHelper.FindRect(root, "steps-list");
            if (stepsList != null)
            {
                for (int i = stepsList.childCount - 1; i >= 0; i--)
                    UIHelper.SafeDestroy(stepsList.GetChild(i).gameObject);

                for (int i = 0; i < steps.Length; i++)
                    GasModuleDetailBuilder.MakeStepItem(stepsList, i + 1, steps[i]);
            }

            var currentLang = AppState.Instance != null
                ? AppState.Instance.CurrentLanguage
                : (loc != null ? loc.CurrentLanguage : AppLanguage.English);
            UIManager.Instance?.ApplyLanguageFonts(root, currentLang);
        }

        private void LaunchGasARScenario()
        {
            var gasModule = AppManager.Instance?.GetModule(ModuleId.GasLeakConfinedSpace);
            if (gasModule == null)
            {
                gasModule = new ModuleData
                {
                    id = ModuleId.GasLeakConfinedSpace,
                    titleKey = "module.gas.title",
                    arSceneName = "AR_gas_Foundation",
                    isImplemented = true
                };
            }

            if (AppState.Instance != null)
            {
                AppState.Instance.SelectedModule = gasModule;
            }

            UIManager.Instance?.ShowScreen(ScreenId.TrainingInstructions);
        }

        // ──────────────────────────────────────────────────────────────────
        private void GoBack()
        {
            UIManager.Instance?.ShowScreen(ScreenId.GasScenarioSelection);
        }

        public void OnHide()
        {
            if (_btnBack         != null) _btnBack.onClick.RemoveAllListeners();
            if (_btnStart        != null) _btnStart.onClick.RemoveAllListeners();
            if (_navHome         != null) _navHome.onClick.RemoveAllListeners();
            if (_navLearn        != null) _navLearn.onClick.RemoveAllListeners();
            if (_navProgress     != null) _navProgress.onClick.RemoveAllListeners();
            if (_navCertificates != null) _navCertificates.onClick.RemoveAllListeners();
        }
    }
}
