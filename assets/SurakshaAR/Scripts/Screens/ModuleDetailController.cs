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
    /// ModuleDetailController (Screens 3, 4, 5 in reference UI):
    /// Manages the Scenario Detail screen for Electrical Panel Fire,
    /// Conveyor Belt Fire, and Excavator / HEMM Fire.
    /// Tapping "Start AR Training" routes directly to Training Instructions (Screen 6).
    /// </summary>
    public class ModuleDetailController : IScreenController
    {
        private Button _btnBack, _btnStart;
        private int _scenarioIndex = 1;

        public void OnShow(GameObject root, object param)
        {
            if (AppState.Instance != null)
            {
                _scenarioIndex = AppState.Instance.SelectedScenarioIndex;
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
                _btnStart.onClick.AddListener(OpenTrainingInstructions);
            }

            BindScenarioData(root);
        }

        private void BindScenarioData(GameObject root)
        {
            string title;
            string subtitle;
            string heroImg;
            string levelBadge;
            string duration;
            string level;
            string focusVal;
            string focusSub;
            string overview;
            string[] steps;

            if (_scenarioIndex == 2)
            {
                title = "Conveyor Belt Fire";
                subtitle = "Respond to fire in conveyor belt systems.";
                heroImg = "scenario_conveyor_belt.jpg";
                levelBadge = "📊 Intermediate";
                duration = "~ 12 mins";
                level = "Intermediate";
                focusVal = "Isolation";
                focusSub = "& Evacuation";
                overview = "A fire has been detected on the conveyor belt system. Learn to raise the alarm, stop and isolate the conveyor, and respond to the fire safely.";
                steps = new string[]
                {
                    "Detect smoke/fire on conveyor",
                    "Raise alarm",
                    "Stop and isolate conveyor (E-Stop)",
                    "Keep workers away from danger zone",
                    "Use appropriate firefighting equipment",
                    "Control the fire if safe",
                    "Evacuate to safe area and report"
                };
            }
            else if (_scenarioIndex == 3)
            {
                title = "Excavator / HEMM Fire";
                subtitle = "Handle fire in heavy earth moving machinery.";
                heroImg = "scenario_excavator.jpg";
                levelBadge = "📊 Intermediate";
                duration = "~ 12 mins";
                level = "Intermediate";
                focusVal = "Emergency";
                focusSub = "Response";
                overview = "A fire has started in a heavy earth moving machine. Learn to stop the machine, raise the alarm and respond to the fire while maintaining a safe distance.";
                steps = new string[]
                {
                    "Detect engine/machine fire",
                    "Stop the machine safely",
                    "Raise alarm and inform control room",
                    "Exit the operator area",
                    "Maintain safe distance",
                    "Use appropriate extinguisher",
                    "Report and move to safe area"
                };
            }
            else
            {
                // Default: Scenario 1 - Electrical Panel Fire
                title = "Electrical Panel Fire";
                subtitle = "Handle fire in electrical panels and control rooms.";
                heroImg = "scenario_electrical_panel.jpg";
                levelBadge = "📊 Beginner";
                duration = "~ 10 mins";
                level = "Beginner";
                focusVal = "Extinguisher";
                focusSub = "Use";
                overview = "A fire may start in an electrical control panel due to short circuit, overload or equipment failure. Learn to identify the hazard, activate the alarm and use the correct extinguisher to control the fire safely.";
                steps = new string[]
                {
                    "Identify fire and hazard",
                    "Activate fire alarm",
                    "Select correct extinguisher (CO2 / Dry Chemical)",
                    "Remove safety pin",
                    "Grip and aim at fire base",
                    "Press and spray",
                    "Confirm extinguished and move to safe area"
                };
            }

            // Top Header Title
            var labelTitle = UIHelper.FindTMP(root, "label-title");
            if (labelTitle != null) labelTitle.text = title;

            // Scenario Title
            var labelScenarioTitle = UIHelper.FindTMP(root, "label-scenario-title");
            if (labelScenarioTitle != null) labelScenarioTitle.text = title;

            // Scenario Description
            var labelScenarioDesc = UIHelper.FindTMP(root, "label-scenario-desc");
            if (labelScenarioDesc != null) labelScenarioDesc.text = subtitle;

            // Hero Badge
            var heroBadgeLbl = UIHelper.FindTMP(root, "label-hero-badge");
            if (heroBadgeLbl != null) heroBadgeLbl.text = levelBadge;

            // Hero Photo
            var heroPhotoContainer = UIHelper.FindRect(root, "HeroPhotoContainer");
            if (heroPhotoContainer != null)
            {
                var img = heroPhotoContainer.GetComponent<Image>();
                var spr = UIHelper.LoadProjectSprite(heroImg);
                if (img != null && spr != null)
                {
                    img.sprite = spr;
                    img.color = Color.white;
                }
            }

            // Stat Chips
            var chipDurVal = UIHelper.FindRect(root, "chip-duration")?.Find("Inner/TextCol/Value")?.GetComponent<TextMeshProUGUI>();
            if (chipDurVal != null) chipDurVal.text = duration;

            var chipLevVal = UIHelper.FindRect(root, "chip-level")?.Find("Inner/TextCol/Value")?.GetComponent<TextMeshProUGUI>();
            if (chipLevVal != null) chipLevVal.text = level;

            var chipFocVal = UIHelper.FindRect(root, "chip-focus")?.Find("Inner/TextCol/Value")?.GetComponent<TextMeshProUGUI>();
            if (chipFocVal != null) chipFocVal.text = focusVal;

            var chipFocSub = UIHelper.FindRect(root, "chip-focus")?.Find("Inner/TextCol/Sub")?.GetComponent<TextMeshProUGUI>();
            if (chipFocSub != null) chipFocSub.text = focusSub;

            // Overview Body
            var labelOverview = UIHelper.FindTMP(root, "label-overview-body");
            if (labelOverview != null) labelOverview.text = overview;

            // Step List
            var stepsList = UIHelper.FindRect(root, "steps-list");
            if (stepsList != null)
            {
                for (int i = stepsList.childCount - 1; i >= 0; i--)
                {
                    UIHelper.SafeDestroy(stepsList.GetChild(i).gameObject);
                }

                for (int i = 0; i < steps.Length; i++)
                {
                    ModuleDetailBuilder.MakeStepItem(stepsList, i + 1, steps[i]);
                }
            }

            var currentLang = AppState.Instance != null
                ? AppState.Instance.CurrentLanguage
                : AppLanguage.English;
            UIManager.Instance?.ApplyLanguageFonts(root, currentLang);
        }

        private void OpenTrainingInstructions()
        {
            // Route to Training Instructions screen (Screen 6 in reference UI)
            UIManager.Instance?.ShowScreen(ScreenId.TrainingInstructions);
        }

        private void GoBack()
        {
            // Back navigation: Scenario Detail → Fire Sub-Modules Selection
            UIManager.Instance?.ShowScreen(ScreenId.ScenarioSelection);
        }

        public void OnHide()
        {
            if (_btnBack != null) _btnBack.onClick.RemoveAllListeners();
            if (_btnStart != null) _btnStart.onClick.RemoveAllListeners();
        }
    }
}
