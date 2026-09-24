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
                _btnBack.onClick.AddListener(OnBack);

            var titleLbl = UIHelper.FindTMP(root, "label-title");
            if (titleLbl != null && loc != null)
            {
                string t = loc.Get("module.fire.title");
                if (!string.IsNullOrEmpty(t) && t != "module.fire.title") titleLbl.text = t;
            }

            // Card 1: 01 Electrical Panel Fire
            _cardScenario1 = UIHelper.FindButton(root, "card-scenario-1");
            if (_cardScenario1 != null)
            {
                _cardScenario1.onClick.AddListener(() => OnSelectScenario(1, "Electrical Panel Fire"));
            }

            // Card 2: 02 Conveyor Belt Fire
            _cardScenario2 = UIHelper.FindButton(root, "card-scenario-2");
            if (_cardScenario2 != null)
            {
                _cardScenario2.onClick.AddListener(() => OnSelectScenario(2, "Conveyor Belt Fire"));
            }

            // Card 3: 03 Excavator / HEMM Fire
            _cardScenario3 = UIHelper.FindButton(root, "card-scenario-3");
            if (_cardScenario3 != null)
            {
                _cardScenario3.onClick.AddListener(() => OnSelectScenario(3, "Excavator / HEMM Fire"));
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
