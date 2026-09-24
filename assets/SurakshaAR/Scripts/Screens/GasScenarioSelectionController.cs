using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.Localization;
using SurakshaAR.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    /// <summary>
    /// GasScenarioSelectionController — Gas Leak & Confined Space sub-modules.
    ///
    /// Exact scenarios from reference image:
    ///   01 — Underground Gas Release
    ///   02 — Confined Space Entry
    ///   03 — Gas Cylinder Leak
    ///
    /// ALL are COMING SOON — clicking a card navigates to GasModuleDetail
    /// which shows the scenario info and a disabled Coming Soon button.
    /// </summary>
    public class GasScenarioSelectionController : IScreenController
    {
        private GameObject _root;
        private Button _btnBack;
        private Button _cardScenario1, _cardScenario2, _cardScenario3;
        private Button _navHome, _navLearn, _navProgress, _navCertificates;

        public void OnShow(GameObject root, object param)
        {
            _root = root;
            if (_root == null) return;

            var currentLang = AppState.Instance?.CurrentLanguage ?? AppLanguage.English;

            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null)
                _btnBack.onClick.AddListener(OnBack);

            // Cards navigate to detail screens for info only
            _cardScenario1 = UIHelper.FindButton(root, "card-scenario-1");
            if (_cardScenario1 != null)
                _cardScenario1.onClick.AddListener(() => OnSelectScenario(101, "Underground Gas Release"));

            _cardScenario2 = UIHelper.FindButton(root, "card-scenario-2");
            if (_cardScenario2 != null)
                _cardScenario2.onClick.AddListener(() => OnSelectScenario(102, "Confined Space Entry"));

            _cardScenario3 = UIHelper.FindButton(root, "card-scenario-3");
            if (_cardScenario3 != null)
                _cardScenario3.onClick.AddListener(() => OnSelectScenario(103, "Gas Cylinder Leak"));

            // Bottom navigation
            _navHome         = UIHelper.FindButton(root, "nav-home");
            _navLearn        = UIHelper.FindButton(root, "nav-learn");
            _navProgress     = UIHelper.FindButton(root, "nav-progress");
            _navCertificates = UIHelper.FindButton(root, "nav-certificates");

            if (_navHome         != null) _navHome.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard));
            if (_navLearn        != null) _navLearn.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.ModuleSelection));
            if (_navProgress     != null) _navProgress.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.Progress));
            if (_navCertificates != null) _navCertificates.onClick.AddListener(() => UIManager.Instance?.ShowScreen(ScreenId.Certificate));

            UIManager.Instance?.ApplyLanguageFonts(root, currentLang);
        }

        private void OnSelectScenario(int scenarioIndex, string scenarioTitle)
        {
            if (AppState.Instance != null)
                AppState.Instance.SelectScenario(scenarioIndex, scenarioTitle);

            UIManager.Instance?.ShowScreen(ScreenId.GasModuleDetail);
        }

        private void OnBack()
        {
            UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard);
        }

        public void OnHide()
        {
            if (_btnBack         != null) _btnBack.onClick.RemoveAllListeners();
            if (_cardScenario1   != null) _cardScenario1.onClick.RemoveAllListeners();
            if (_cardScenario2   != null) _cardScenario2.onClick.RemoveAllListeners();
            if (_cardScenario3   != null) _cardScenario3.onClick.RemoveAllListeners();
            if (_navHome         != null) _navHome.onClick.RemoveAllListeners();
            if (_navLearn        != null) _navLearn.onClick.RemoveAllListeners();
            if (_navProgress     != null) _navProgress.onClick.RemoveAllListeners();
            if (_navCertificates != null) _navCertificates.onClick.RemoveAllListeners();
            _root = null;
        }
    }
}
