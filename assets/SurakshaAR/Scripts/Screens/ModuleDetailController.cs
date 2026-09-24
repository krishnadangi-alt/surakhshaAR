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
    /// Includes full 4-tab bottom navigation bar support.
    /// </summary>
    public class ModuleDetailController : IScreenController
    {
        private Button _btnBack, _btnStart;
        private Button _navHome, _navLearn, _navProgress, _navCertificates;
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

        private void BindScenarioData(GameObject root)
        {
            string title;
            string subtitle;
            string heroImg;
            string scenarioNumberBadge;
            string duration;
            string level;
            string focusVal;
            string focusSub;
            string overview;
            string[] steps;

            if (_scenarioIndex == 2)
            {
                // Scenario 2 — Conveyor Belt Fire (Coming Soon)
                title               = "Conveyor Belt Fire";
                subtitle            = "Respond to fire in conveyor belt systems.";
                heroImg             = "icon_conveyor_belt_fire.jpg";
                scenarioNumberBadge = "02";
                duration            = "~ 12 mins";
                level               = "Intermediate";
                focusVal            = "Isolation";
                focusSub            = "& Evacuation";
                overview            = "A fire has been detected on the conveyor belt system. Learn to raise the alarm, stop and isolate the conveyor, and respond to the fire safely.";
                steps = new string[]
                {
                    "Detect smoke/fire on conveyor",
                    "Raise alarm",
                    "Stop and isolate conveyor (E-Stop)",
                    "Keep workers away from danger zone",
                    "Use appropriate firefighting equipment",
                    "Control the fire if safe"
                };
            }
            else if (_scenarioIndex == 3)
            {
                // Scenario 3 — Excavator / HEMM Fire (Coming Soon)
                title               = "Excavator / HEMM Fire";
                subtitle            = "Handle fire in heavy earth moving machinery.";
                heroImg             = "icon_excavator_hemm_fire.jpg";
                scenarioNumberBadge = "03";
                duration            = "~ 12 mins";
                level               = "Intermediate";
                focusVal            = "Emergency";
                focusSub            = "Response";
                overview            = "A fire has started in a heavy earth moving machine. Learn to stop the machine, raise the alarm and respond to the fire while maintaining a safe distance.";
                steps = new string[]
                {
                    "Detect engine/machine fire",
                    "Stop the machine safely",
                    "Raise alarm and inform control room",
                    "Exit the operator area",
                    "Maintain safe distance",
                    "Use appropriate extinguisher"
                };
            }
            else
            {
                // Scenario 1 — Electrical Panel Fire (AVAILABLE — real AR)
                title               = "Electrical Panel Fire";
                subtitle            = "Handle fire in electrical panels and control rooms.";
                heroImg             = "icon_electrical_panel_fire.jpg";
                scenarioNumberBadge = "01";
                duration            = "~ 10 mins";
                level               = "Beginner";
                focusVal            = "Extinguisher";
                focusSub            = "Use";
                overview            = "A fire may start in an electrical control panel due to short circuit, overload or equipment failure. Learn to identify the hazard, activate the alarm and use the correct extinguisher to control the fire safely.";
                // Exactly 6 steps — matches the real FireScenarioFlowManager workflow
                steps = new string[]
                {
                    "Identify fire hazard",
                    "Activate fire alarm",
                    "Select correct extinguisher",
                    "Remove safety pin",
                    "Grip and aim at fire base",
                    "Press and spray"
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

            // Hero Number Badge ("01", "02", "03")
            var heroBadgeLbl = UIHelper.FindTMP(root, "label-hero-badge");
            if (heroBadgeLbl != null) heroBadgeLbl.text = scenarioNumberBadge;

            // Hero icon illustration (swap the centred icon sprite)
            var heroPhotoContainer = UIHelper.FindRect(root, "HeroPhotoContainer");
            if (heroPhotoContainer != null)
            {
                var iconImgRT = heroPhotoContainer.Find("ScenarioIconImg");
                if (iconImgRT != null)
                {
                    var img = iconImgRT.GetComponent<Image>();
                    var spr = UIHelper.LoadProjectSprite(heroImg);
                    if (img != null && spr != null)
                    {
                        img.sprite = spr;
                        img.color  = Color.white;
                    }
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

            // Start Button styling for Coming Soon scenarios
            if (_btnStart != null)
            {
                var btnImg = _btnStart.GetComponent<Image>();
                var btnTxt = _btnStart.GetComponentInChildren<TextMeshProUGUI>();
                if (_scenarioIndex == 1)
                {
                    if (btnImg != null) btnImg.color = UIColors.Hex("#EA580C");
                    if (btnTxt != null) btnTxt.text = "▶   Start Training";
                }
                else
                {
                    if (btnImg != null) btnImg.color = UIColors.Hex("#94A3B8");
                    if (btnTxt != null) btnTxt.text = "Coming Soon";
                }
            }

            var currentLang = AppState.Instance != null
                ? AppState.Instance.CurrentLanguage
                : AppLanguage.English;
            UIManager.Instance?.ApplyLanguageFonts(root, currentLang);
        }

        private void OpenTrainingInstructions()
        {
            // Scenario 1 (Electrical Panel Fire) routes to Training Instructions → real Fire AR.
            // Scenarios 2 and 3 are Coming Soon — do nothing.
            if (_scenarioIndex == 1)
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
            if (_navHome != null) _navHome.onClick.RemoveAllListeners();
            if (_navLearn != null) _navLearn.onClick.RemoveAllListeners();
            if (_navProgress != null) _navProgress.onClick.RemoveAllListeners();
            if (_navCertificates != null) _navCertificates.onClick.RemoveAllListeners();
        }
    }
}
