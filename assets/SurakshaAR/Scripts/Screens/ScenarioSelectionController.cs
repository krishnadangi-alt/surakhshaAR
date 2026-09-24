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

            var loc = AppManager.Instance?.Localization;
            var currentLang = AppState.Instance != null
                ? AppState.Instance.CurrentLanguage
                : (loc != null ? loc.CurrentLanguage : AppLanguage.English);

            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null)
                _btnBack.onClick.AddListener(OnBack);

            var titleLbl = UIHelper.FindTMP(root, "label-title");
            if (titleLbl != null && loc != null) titleLbl.text = loc.Get("scenarioSelection.title");

            var bannerTitle = UIHelper.FindTMP(root, "BannerTitle");
            if (bannerTitle != null && loc != null) bannerTitle.text = loc.Get("scenarioSelection.bannerTitle");

            var bannerDesc = UIHelper.FindTMP(root, "BannerDesc");
            if (bannerDesc != null && loc != null) bannerDesc.text = loc.Get("scenarioSelection.bannerDesc");

            _cardScenario1 = UIHelper.FindButton(root, "card-scenario-1");
            if (_cardScenario1 != null)
            {
                var s1Title = _cardScenario1.transform.Find("Inner/TextCol/Title")?.GetComponent<TextMeshProUGUI>();
                if (s1Title != null && loc != null) s1Title.text = loc.Get("scenario.s1.title");
                _cardScenario1.onClick.AddListener(() => OnSelectScenario(1, loc?.Get("scenario.s1.title") ?? "1. Electrical Panel Fire"));
            }

            _cardScenario2 = UIHelper.FindButton(root, "card-scenario-2");
            if (_cardScenario2 != null)
            {
                var s2Title = _cardScenario2.transform.Find("Inner/TextCol/Title")?.GetComponent<TextMeshProUGUI>();
                if (s2Title != null && loc != null) s2Title.text = loc.Get("scenario.s2.title");
                _cardScenario2.onClick.AddListener(() => OnSelectScenario(2, loc?.Get("scenario.s2.title") ?? "2. Chemical Storage Fire"));
            }

            _cardScenario3 = UIHelper.FindButton(root, "card-scenario-3");
            if (_cardScenario3 != null)
            {
                var s3Title = _cardScenario3.transform.Find("Inner/TextCol/Title")?.GetComponent<TextMeshProUGUI>();
                if (s3Title != null && loc != null) s3Title.text = loc.Get("scenario.s3.title");
                _cardScenario3.onClick.AddListener(() => OnSelectScenario(3, loc?.Get("scenario.s3.title") ?? "3. Conveyor Belt Fire"));
            }

            UIManager.Instance?.ApplyLanguageFonts(root, currentLang);
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
