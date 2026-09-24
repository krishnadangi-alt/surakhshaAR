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
    /// GasModuleDetailController:
    /// Manages the Scenario Detail screen for Gas Leak &amp; Confined Space sub-modules.
    /// Reads AppState.SelectedScenarioIndex (100-offset: 101 / 102 / 103).
    /// Tapping "Start AR Training" currently routes to Assessment (Gas AR TBD).
    /// </summary>
    public class GasModuleDetailController : IScreenController
    {
        private Button _btnBack, _btnStart;
        private int _gasScenarioIndex = 1; // 1 = Underground, 2 = Detection, 3 = Confined

        public void OnShow(GameObject root, object param)
        {
            // Decode the 100-offset scenario index stored by GasScenarioSelectionController
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

            _btnStart = UIHelper.FindButton(root, "btn-start-ar-training");
            if (_btnStart != null)
            {
                _btnStart.onClick.RemoveAllListeners();
                _btnStart.onClick.AddListener(OpenTrainingOrAssessment);
            }

            BindScenarioData(root);
        }

        // ────────────────────────────────────────────────────────────────
        private void BindScenarioData(GameObject root)
        {
            string title, subtitle, heroImg, levelBadge, duration, level, focusVal, focusSub, overview;
            string[] steps;

            if (_gasScenarioIndex == 2)
            {
                // Scenario 2 — Gas Detection & Ventilation
                title      = "Gas Detection & Ventilation";
                subtitle   = "Use gas detectors and ensure safe ventilation.";
                heroImg    = "scenario_gas_detection.jpg";
                levelBadge = "📊 Intermediate";
                duration   = "~ 12 mins";
                level      = "Intermediate";
                focusVal   = "Detector";
                focusSub   = "& Ventilation";
                overview   = "Proper gas detection and ventilation are critical before entering any underground or confined area. Learn to operate a multi-gas detector, check ventilation systems and interpret alarm signals.";
                steps = new string[]
                {
                    "Check gas detector battery & calibration",
                    "Enter hazardous zone with detector active",
                    "Monitor O2, CH4, CO and H2S readings",
                    "Raise alarm if readings exceed threshold",
                    "Activate ventilation fan or blower",
                    "Wait for gas levels to normalise",
                    "Re-test before authorising entry"
                };
            }
            else if (_gasScenarioIndex == 3)
            {
                // Scenario 3 — Confined Space Gas Testing
                title      = "Confined Space Gas Testing";
                subtitle   = "Pre-entry gas testing and PPE for confined spaces.";
                heroImg    = "scenario_gas_confined.jpg";
                levelBadge = "📊 Intermediate";
                duration   = "~ 12 mins";
                level      = "Intermediate";
                focusVal   = "Confined";
                focusSub   = "Entry PPE";
                overview   = "Entering a confined space without proper gas testing can be fatal. Learn the pre-entry permit procedure, multi-gas testing, PPE donning and emergency rescue protocols.";
                steps = new string[]
                {
                    "Obtain Confined Space Entry Permit",
                    "Test atmosphere (O2, LEL, toxic gases)",
                    "Don full PPE — SCBA / airline respirator",
                    "Station an attendant at entry point",
                    "Enter slowly and continuously monitor",
                    "Maintain communication with attendant",
                    "Exit immediately if alarm triggers"
                };
            }
            else
            {
                // Default: Scenario 1 — Underground Gas Release
                title      = "Underground Gas Release";
                subtitle   = "Detect and respond to sudden methane/CO release.";
                heroImg    = "scenario_gas_underground.jpg";
                levelBadge = "📊 Beginner";
                duration   = "~ 10 mins";
                level      = "Beginner";
                focusVal   = "Emergency";
                focusSub   = "Evacuation";
                overview   = "A sudden gas release underground can create explosive and suffocation hazards. Learn to identify warning signs, raise the alarm, evacuate personnel and use proper PPE.";
                steps = new string[]
                {
                    "Detect gas alarm / warning sign",
                    "Stop work and raise alarm",
                    "Evacuate personnel from danger zone",
                    "Report to control room",
                    "Do not use ignition sources",
                    "Wear self-contained breathing apparatus",
                    "Wait for all-clear signal before re-entry"
                };
            }

            // ── Bind labels ───────────────────────────────────────────────

            // Top header title
            var labelTitle = UIHelper.FindTMP(root, "label-title");
            if (labelTitle != null) labelTitle.text = title;

            // Scenario meta
            var labelScenTitle = UIHelper.FindTMP(root, "label-scenario-title");
            if (labelScenTitle != null) labelScenTitle.text = title;

            var labelScenDesc = UIHelper.FindTMP(root, "label-scenario-desc");
            if (labelScenDesc != null) labelScenDesc.text = subtitle;

            // Hero badge
            var heroBadgeLbl = UIHelper.FindTMP(root, "label-hero-badge");
            if (heroBadgeLbl != null) heroBadgeLbl.text = levelBadge;

            // Hero photo
            var heroPhotoContainer = UIHelper.FindRect(root, "HeroPhotoContainer");
            if (heroPhotoContainer != null)
            {
                var img = heroPhotoContainer.GetComponent<Image>();
                var spr = UIHelper.LoadProjectSprite(heroImg);
                if (img != null && spr != null)
                {
                    img.sprite = spr;
                    img.color  = Color.white;
                }
            }

            // Stat chips
            var chipDurVal = UIHelper.FindRect(root, "chip-duration")?.Find("Inner/TextCol/Value")?.GetComponent<TextMeshProUGUI>();
            if (chipDurVal != null) chipDurVal.text = duration;

            var chipLevVal = UIHelper.FindRect(root, "chip-level")?.Find("Inner/TextCol/Value")?.GetComponent<TextMeshProUGUI>();
            if (chipLevVal != null) chipLevVal.text = level;

            var chipFocVal = UIHelper.FindRect(root, "chip-focus")?.Find("Inner/TextCol/Value")?.GetComponent<TextMeshProUGUI>();
            if (chipFocVal != null) chipFocVal.text = focusVal;

            var chipFocSub = UIHelper.FindRect(root, "chip-focus")?.Find("Inner/TextCol/Sub")?.GetComponent<TextMeshProUGUI>();
            if (chipFocSub != null) chipFocSub.text = focusSub;

            // Overview body
            var labelOverview = UIHelper.FindTMP(root, "label-overview-body");
            if (labelOverview != null) labelOverview.text = overview;

            // Steps list — rebuild with scenario-specific content
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
                : AppLanguage.English;
            UIManager.Instance?.ApplyLanguageFonts(root, currentLang);
        }

        // ────────────────────────────────────────────────────────────────
        private void OpenTrainingOrAssessment()
        {
            // Gas AR scene is not yet implemented — route to Assessment screen.
            // When a GasTraining scene is added, wire it through ARModuleLauncher here.
            var gasModule = AppManager.Instance?.GetModule(ModuleId.GasLeakConfinedSpace);
            if (gasModule != null && gasModule.isImplemented && ARModuleLauncher.Instance != null)
            {
                bool launched = ARModuleLauncher.Instance.TryLaunchModule(gasModule);
                if (!launched)
                {
                    Debug.LogWarning("[GasModuleDetail] Gas AR scene not available, routing to Assessment.");
                    UIManager.Instance?.ShowScreen(ScreenId.Assessment);
                }
            }
            else
            {
                UIManager.Instance?.ShowScreen(ScreenId.Assessment);
            }
        }

        private void GoBack()
        {
            // Back navigation: Gas Scenario Detail → Gas Sub-Modules Selection
            UIManager.Instance?.ShowScreen(ScreenId.GasScenarioSelection);
        }

        public void OnHide()
        {
            if (_btnBack  != null) _btnBack.onClick.RemoveAllListeners();
            if (_btnStart != null) _btnStart.onClick.RemoveAllListeners();
        }
    }
}
