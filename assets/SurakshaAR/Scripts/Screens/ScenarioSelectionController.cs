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
    /// ScenarioSelectionController (Screen 2: Fire & Explosion Response Sub-Modules)
    /// Manages the 3 realistic fire training sub-modules:
    /// - 01 Electrical Panel Fire (AR Ready)
    /// - 02 Conveyor Belt Fire
    /// - 03 Excavator / HEMM Fire
    /// Connects to ModuleDetail (Scenario Detail) and back to HomeDashboard.
    /// </summary>
    public class ScenarioSelectionController : IScreenController
    {
        private GameObject _root;
        private Button _btnBack;
        private Button _cardScenario1;
        private Button _cardScenario2;
        private Button _cardScenario3;
        private Button _navHome, _navLearn, _navProgress, _navCertificates;

        public void OnShow(GameObject root, object param)
        {
            _root = root;
            if (_root == null) return;

            var loc = AppManager.Instance?.Localization;
            var currentLang = AppState.Instance != null
                ? AppState.Instance.CurrentLanguage
                : (loc != null ? loc.CurrentLanguage : AppLanguage.English);

            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null)
            {
                _btnBack.onClick.RemoveAllListeners();
                _btnBack.onClick.AddListener(OnBack);
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

            // Top Header Title
            var titleLbl = UIHelper.FindTMP(root, "label-title");
            if (titleLbl != null && loc != null)
            {
                titleLbl.text = loc.Get("fire.module.title");
            }

            // Intro Card Title & Description
            var introTitle = root.transform.Find("ScrollArea/Viewport/Content/IntroCard/InnerRow/TextCol/IntroTitle")?.GetComponent<TextMeshProUGUI>();
            if (introTitle != null && loc != null)
            {
                introTitle.text = loc.Get("fire.module.title");
            }
            var introDesc = root.transform.Find("ScrollArea/Viewport/Content/IntroCard/InnerRow/TextCol/IntroDesc")?.GetComponent<TextMeshProUGUI>();
            if (introDesc != null && loc != null)
            {
                introDesc.text = loc.Get("fire.module.desc");
            }

            // Section Heading: "Select a Scenario"
            var secHead = UIHelper.FindTMP(root, "label-select-scenario");
            if (secHead != null && loc != null)
            {
                secHead.text = loc.Get("scenario.select.heading");
            }

            // Card 1: 01 Electrical Panel Fire
            _cardScenario1 = UIHelper.FindButton(root, "card-scenario-1");
            if (_cardScenario1 != null)
            {
                _cardScenario1.onClick.RemoveAllListeners();
                _cardScenario1.onClick.AddListener(() => OnSelectScenario(1, "Electrical Panel Fire"));
                var c1Title = _cardScenario1.transform.Find("Inner/TextCol/Title")?.GetComponent<TextMeshProUGUI>();
                if (c1Title != null && loc != null) c1Title.text = loc.Get("scenario.fire.electrical.title");
                var c1Desc = _cardScenario1.transform.Find("Inner/TextCol/Desc")?.GetComponent<TextMeshProUGUI>();
                if (c1Desc != null && loc != null) c1Desc.text = loc.Get("scenario.fire.electrical.desc");
            }

            // Card 2: 02 Conveyor Belt Fire
            _cardScenario2 = UIHelper.FindButton(root, "card-scenario-2");
            if (_cardScenario2 != null)
            {
                _cardScenario2.onClick.RemoveAllListeners();
                _cardScenario2.onClick.AddListener(() => OnSelectScenario(2, "Conveyor Belt Fire"));
                var c2Title = _cardScenario2.transform.Find("Inner/TextCol/Title")?.GetComponent<TextMeshProUGUI>();
                if (c2Title != null && loc != null) c2Title.text = loc.Get("scenario.fire.conveyor.title");
                var c2Desc = _cardScenario2.transform.Find("Inner/TextCol/Desc")?.GetComponent<TextMeshProUGUI>();
                if (c2Desc != null && loc != null) c2Desc.text = loc.Get("scenario.fire.conveyor.desc");
            }

            // Card 3: 03 Excavator / HEMM Fire
            _cardScenario3 = UIHelper.FindButton(root, "card-scenario-3");
            if (_cardScenario3 != null)
            {
                _cardScenario3.onClick.RemoveAllListeners();
                _cardScenario3.onClick.AddListener(() => OnSelectScenario(3, "Excavator / HEMM Fire"));
                var c3Title = _cardScenario3.transform.Find("Inner/TextCol/Title")?.GetComponent<TextMeshProUGUI>();
                if (c3Title != null && loc != null) c3Title.text = loc.Get("scenario.fire.excavator.title");
                var c3Desc = _cardScenario3.transform.Find("Inner/TextCol/Desc")?.GetComponent<TextMeshProUGUI>();
                if (c3Desc != null && loc != null) c3Desc.text = loc.Get("scenario.fire.excavator.desc");
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
                var hLbl = _navHome?.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
                if (hLbl != null) hLbl.text = loc.Get("nav.home");
                var lLbl = _navLearn?.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
                if (lLbl != null) lLbl.text = loc.Get("nav.learn");
                var pLbl = _navProgress?.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
                if (pLbl != null) pLbl.text = loc.Get("nav.progress");
                var cLbl = _navCertificates?.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
                if (cLbl != null) cLbl.text = loc.Get("nav.certificates");
            }

            UIManager.Instance?.ApplyLanguageFonts(root, currentLang);
        }

        private void OnSelectScenario(int scenarioIndex, string scenarioTitle)
        {
            if (AppState.Instance != null)
            {
                AppState.Instance.SelectScenario(scenarioIndex, scenarioTitle);
            }

            // Route to scenario detail screen
            UIManager.Instance?.ShowScreen(ScreenId.ModuleDetail);
        }

        private void OnBack()
        {
            // Back navigation: Fire Sub-Modules → Home
            UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard);
        }

        public void OnHide()
        {
            if (_btnBack != null) _btnBack.onClick.RemoveAllListeners();
            if (_cardScenario1 != null) _cardScenario1.onClick.RemoveAllListeners();
            if (_cardScenario2 != null) _cardScenario2.onClick.RemoveAllListeners();
            if (_cardScenario3 != null) _cardScenario3.onClick.RemoveAllListeners();
            if (_navHome != null) _navHome.onClick.RemoveAllListeners();
            if (_navLearn != null) _navLearn.onClick.RemoveAllListeners();
            if (_navProgress != null) _navProgress.onClick.RemoveAllListeners();
            if (_navCertificates != null) _navCertificates.onClick.RemoveAllListeners();
            _root = null;
        }
    }
}
