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
                _btnStart.onClick.AddListener(OpenTrainingInstructions);
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

        private void BindScenarioData(GameObject root)
        {
            var loc = AppManager.Instance?.Localization;
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
                title               = loc?.Get("scenario.fire.conveyor.title") ?? "Conveyor Belt Fire";
                subtitle            = loc?.Get("scenario.fire.conveyor.desc") ?? "Respond to fire in conveyor belt systems.";
                heroImg             = "icon_conveyor_belt_fire.jpg";
                scenarioNumberBadge = "02";
                duration            = loc?.Get("chip.val.12mins") ?? "~ 12 mins";
                level               = loc?.Get("chip.val.intermediate") ?? "Intermediate";
                focusVal            = loc?.Get("chip.val.isolation") ?? "Isolation";
                focusSub            = loc?.Get("chip.sub.evacuation") ?? "& Evacuation";
                overview            = loc?.Get("scenario.fire.conveyor.overview") ?? "A fire has been detected on the conveyor belt system. Learn to raise the alarm, stop and isolate the conveyor, and respond to the fire safely.";
                steps = new string[]
                {
                    loc?.Get("fire.conveyor.step1") ?? "Detect smoke/fire on conveyor",
                    loc?.Get("fire.conveyor.step2") ?? "Raise alarm",
                    loc?.Get("fire.conveyor.step3") ?? "Stop and isolate conveyor (E-Stop)",
                    loc?.Get("fire.conveyor.step4") ?? "Keep workers away from danger zone",
                    loc?.Get("fire.conveyor.step5") ?? "Use appropriate firefighting equipment",
                    loc?.Get("fire.conveyor.step6") ?? "Control the fire if safe"
                };
            }
            else if (_scenarioIndex == 3)
            {
                // Scenario 3 — Excavator / HEMM Fire (Coming Soon)
                title               = loc?.Get("scenario.fire.excavator.title") ?? "Excavator / HEMM Fire";
                subtitle            = loc?.Get("scenario.fire.excavator.desc") ?? "Handle fire in heavy earth moving machinery.";
                heroImg             = "icon_excavator_hemm_fire.jpg";
                scenarioNumberBadge = "03";
                duration            = loc?.Get("chip.val.12mins") ?? "~ 12 mins";
                level               = loc?.Get("chip.val.intermediate") ?? "Intermediate";
                focusVal            = loc?.Get("chip.val.emergency") ?? "Emergency";
                focusSub            = loc?.Get("chip.sub.response") ?? "Response";
                overview            = loc?.Get("scenario.fire.excavator.overview") ?? "A fire has started in a heavy earth moving machine. Learn to stop the machine, raise the alarm and respond to the fire while maintaining a safe distance.";
                steps = new string[]
                {
                    loc?.Get("fire.excavator.step1") ?? "Detect engine/machine fire",
                    loc?.Get("fire.excavator.step2") ?? "Stop the machine safely",
                    loc?.Get("fire.excavator.step3") ?? "Raise alarm and inform control room",
                    loc?.Get("fire.excavator.step4") ?? "Exit the operator area",
                    loc?.Get("fire.excavator.step5") ?? "Maintain safe distance",
                    loc?.Get("fire.excavator.step6") ?? "Use appropriate extinguisher"
                };
            }
            else
            {
                // Scenario 1 — Electrical Panel Fire (AVAILABLE — real AR)
                title               = loc?.Get("scenario.fire.electrical.title") ?? "Electrical Panel Fire";
                subtitle            = loc?.Get("scenario.fire.electrical.desc") ?? "Handle fire in electrical panels and control rooms.";
                heroImg             = "icon_electrical_panel_fire.jpg";
                scenarioNumberBadge = "01";
                duration            = loc?.Get("chip.val.10mins") ?? "~ 10 mins";
                level               = loc?.Get("chip.val.beginner") ?? "Beginner";
                focusVal            = loc?.Get("chip.val.extinguisher") ?? "Extinguisher";
                focusSub            = loc?.Get("chip.sub.use") ?? "Use";
                overview            = loc?.Get("scenario.fire.electrical.overview") ?? "A fire may start in an electrical control panel due to short circuit, overload or equipment failure. Learn to identify the hazard, activate the alarm and use the correct extinguisher to control the fire safely.";
                steps = new string[]
                {
                    loc?.Get("fire.sop.step1") ?? "Identify fire hazard",
                    loc?.Get("fire.sop.step2") ?? "Activate fire alarm",
                    loc?.Get("fire.sop.step3") ?? "Select correct extinguisher",
                    loc?.Get("fire.sop.step4") ?? "Remove safety pin",
                    loc?.Get("fire.sop.step5") ?? "Grip and aim at fire base",
                    loc?.Get("fire.sop.step6") ?? "Press and spray"
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
            var chipDurSub = UIHelper.FindRect(root, "chip-duration")?.Find("Inner/TextCol/Sub")?.GetComponent<TextMeshProUGUI>();
            if (chipDurSub != null && loc != null) chipDurSub.text = loc.Get("chip.label.duration");

            var chipLevVal = UIHelper.FindRect(root, "chip-level")?.Find("Inner/TextCol/Value")?.GetComponent<TextMeshProUGUI>();
            if (chipLevVal != null) chipLevVal.text = level;
            var chipLevSub = UIHelper.FindRect(root, "chip-level")?.Find("Inner/TextCol/Sub")?.GetComponent<TextMeshProUGUI>();
            if (chipLevSub != null && loc != null) chipLevSub.text = loc.Get("chip.label.level");

            var chipFocVal = UIHelper.FindRect(root, "chip-focus")?.Find("Inner/TextCol/Value")?.GetComponent<TextMeshProUGUI>();
            if (chipFocVal != null) chipFocVal.text = focusVal;

            var chipFocSub = UIHelper.FindRect(root, "chip-focus")?.Find("Inner/TextCol/Sub")?.GetComponent<TextMeshProUGUI>();
            if (chipFocSub != null) chipFocSub.text = focusSub;

            // Section Headings
            var overviewHead = UIHelper.FindTMP(root, "OverviewHead");
            if (overviewHead != null && loc != null) overviewHead.text = loc.Get("ui.overview.heading");

            var trainingHead = UIHelper.FindTMP(root, "TrainingHead");
            if (trainingHead != null && loc != null) trainingHead.text = loc.Get("ui.training.include");

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
                    if (btnTxt != null) btnTxt.text = loc?.Get("ui.start_training") ?? "Start AR Training";
                }
                else
                {
                    if (btnImg != null) btnImg.color = UIColors.Hex("#94A3B8");
                    if (btnTxt != null) btnTxt.text = loc?.Get("ui.coming_soon") ?? "Coming Soon";
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
