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

            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null)
            {
                _btnBack.onClick.RemoveAllListeners();
                _btnBack.onClick.AddListener(GoBack);
            }

            // Start button is Coming Soon — register listener but no-op
            _btnStart = UIHelper.FindButton(root, "btn-start-ar-training");
            if (_btnStart != null)
            {
                _btnStart.onClick.RemoveAllListeners();
            }

            // Bottom Navigation
            _navHome         = UIHelper.FindButton(root, "nav-home");
            _navLearn        = UIHelper.FindButton(root, "nav-learn");
            _navProgress     = UIHelper.FindButton(root, "nav-progress");
            _navCertificates = UIHelper.FindButton(root, "nav-certificates");

            if (_navHome         != null) { _navHome.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard)); }
            if (_navLearn        != null) { _navLearn.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.ModuleSelection)); }
            if (_navProgress     != null) { _navProgress.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.Progress)); }
            if (_navCertificates != null) { _navCertificates.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.Certificate)); }

            BindScenarioData(root);
        }

        // ──────────────────────────────────────────────────────────────────
        private void BindScenarioData(GameObject root)
        {
            string title, subtitle, scenarioNumberBadge, duration, level,
                   focusVal, focusSub, overview;
            string[] steps;

            if (_gasScenarioIndex == 2)
            {
                // Scenario 2 — Confined Space Entry
                title               = "Confined Space Entry";
                subtitle            = "Follow safe entry procedures and use gas detection & PPE.";
                scenarioNumberBadge = "02";
                duration            = "~ 12 mins";
                level               = "Intermediate";
                focusVal            = "PPE Use";
                focusSub            = "Entry";
                overview            = "Learn the safe entry procedure for confined spaces, use gas detection equipment, verify atmospheric conditions and follow PPE requirements.";
                steps = new string[]
                {
                    "Identify confined space hazards",
                    "Perform atmospheric gas testing",
                    "Check oxygen, toxic and flammable gases",
                    "Use appropriate PPE",
                    "Follow entry and work permit procedure",
                    "Safe exit and emergency response"
                };
            }
            else if (_gasScenarioIndex == 3)
            {
                // Scenario 3 — Gas Cylinder Leak
                title               = "Gas Cylinder Leak";
                subtitle            = "Respond to a gas cylinder leak and control the hazard safely.";
                scenarioNumberBadge = "03";
                duration            = "~ 10 mins";
                level               = "Beginner";
                focusVal            = "Isolation";
                focusSub            = "Procedure";
                overview            = "Learn to identify a gas cylinder leak, isolate the source, raise the alarm and follow safe handling and shut-off procedures.";
                steps = new string[]
                {
                    "Identify cylinder leak signs",
                    "Stop and isolate the source",
                    "Raise alarm and inform control room",
                    "Use appropriate PPE",
                    "Follow safe handling and shut-off procedure",
                    "Move to safe area and report"
                };
            }
            else
            {
                // Scenario 1 — Underground Gas Release (default)
                title               = "Underground Gas Release";
                subtitle            = "Recognize a gas release, raise the alarm and move to a safe area.";
                scenarioNumberBadge = "01";
                duration            = "~ 10 mins";
                level               = "Beginner";
                focusVal            = "Gas Detector";
                focusSub            = "Use";
                overview            = "A gas leak has been detected in an underground mine area. Learn to identify the leak, raise the alarm, communicate the emergency and move to a safe area following proper procedures.";
                steps = new string[]
                {
                    "Identify gas leak signs and hazard area",
                    "Activate alarm and inform control room",
                    "Use gas detector and interpret readings",
                    "Follow safe withdrawal procedure",
                    "Follow ventilation and evacuation route",
                    "Maintain safe distance and move to safe area"
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

            var lev = UIHelper.FindRect(root, "chip-level")?.Find("Inner/TextCol/Value")?.GetComponent<TextMeshProUGUI>();
            if (lev != null) lev.text = level;

            var foc = UIHelper.FindRect(root, "chip-focus")?.Find("Inner/TextCol/Value")?.GetComponent<TextMeshProUGUI>();
            if (foc != null) foc.text = focusVal;

            var focSub = UIHelper.FindRect(root, "chip-focus")?.Find("Inner/TextCol/Sub")?.GetComponent<TextMeshProUGUI>();
            if (focSub != null) focSub.text = focusSub;

            // Steps list — rebuild
            var stepsList = UIHelper.FindRect(root, "steps-list");
            if (stepsList != null)
            {
                for (int i = stepsList.childCount - 1; i >= 0; i--)
                    UIHelper.SafeDestroy(stepsList.GetChild(i).gameObject);

                for (int i = 0; i < steps.Length; i++)
                    GasModuleDetailBuilder.MakeStepItem(stepsList, i + 1, steps[i]);
            }

            var currentLang = AppState.Instance?.CurrentLanguage ?? AppLanguage.English;
            UIManager.Instance?.ApplyLanguageFonts(root, currentLang);
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
