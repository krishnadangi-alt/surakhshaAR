using SurakshaAR.Core;
using SurakshaAR.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    /// <summary>
    /// ScenarioSelectionController
    /// ===========================
    /// Handles safety training scenario card selection, details navigation,
    /// and transition to ModuleDetail or ARTraining.
    /// </summary>
    public class ScenarioSelectionController : IScreenController
    {
        private GameObject _root;
        private Button _btnBack;
        private Button _cardScenario1;
        private Button _cardScenario2;
        private Button _cardScenario3;

        public void OnShow(GameObject root, object param)
        {
            _root = root;
            if (_root == null) return;

            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null)
                _btnBack.onClick.AddListener(OnBack);

            _cardScenario1 = UIHelper.FindButton(root, "card-scenario-1");
            if (_cardScenario1 != null)
                _cardScenario1.onClick.AddListener(() => OnSelectScenario(1, "Electrical Fire Response"));

            _cardScenario2 = UIHelper.FindButton(root, "card-scenario-2");
            if (_cardScenario2 != null)
                _cardScenario2.onClick.AddListener(() => OnSelectScenario(2, "Chemical Storage Fire"));

            _cardScenario3 = UIHelper.FindButton(root, "card-scenario-3");
            if (_cardScenario3 != null)
                _cardScenario3.onClick.AddListener(() => OnSelectScenario(3, "Conveyor Belt Fire"));
        }

        private void OnSelectScenario(int scenarioIndex, string scenarioTitle)
        {
            if (AppState.Instance != null)
            {
                AppState.Instance.SelectScenario(scenarioIndex, scenarioTitle);
            }

            // Route to module detail for the active scenario
            UIManager.Instance?.ShowScreen(ScreenId.ModuleDetail);
        }

        private void OnBack()
        {
            UIManager.Instance?.ShowScreen(ScreenId.ModuleSelection);
        }

        public void OnHide()
        {
            if (_btnBack != null) _btnBack.onClick.RemoveAllListeners();
            if (_cardScenario1 != null) _cardScenario1.onClick.RemoveAllListeners();
            if (_cardScenario2 != null) _cardScenario2.onClick.RemoveAllListeners();
            if (_cardScenario3 != null) _cardScenario3.onClick.RemoveAllListeners();
            _root = null;
        }
    }
}
