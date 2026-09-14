using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    public sealed class ScenarioSelectionController : IScreenController
    {
        private ModuleData _module;
        private Button _btnBack;
        private Button _cardS1, _cardS2, _cardS3;

        public void OnShow(GameObject root, object param)
        {
            _module = param as ModuleData ?? AppState.Instance?.SelectedModule ?? AppManager.Instance?.GetModule(ModuleId.FireAndExplosion);

            var loc = AppManager.Instance?.Localization;

            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null) _btnBack.onClick.AddListener(GoBack);

            var labelTitle = UIHelper.FindTMP(root, "label-title");
            if (labelTitle != null && loc != null) labelTitle.text = loc.Get("scenario.title");

            _cardS1 = UIHelper.FindButton(root, "card-scenario-1");
            _cardS2 = UIHelper.FindButton(root, "card-scenario-2");
            _cardS3 = UIHelper.FindButton(root, "card-scenario-3");

            if (_cardS1 != null) _cardS1.onClick.AddListener(() => LaunchScenario(1, "1. Electrical Panel Fire"));
            if (_cardS2 != null) _cardS2.onClick.AddListener(() => LaunchScenario(2, "2. Chemical Storage Fire"));
            if (_cardS3 != null) _cardS3.onClick.AddListener(() => LaunchScenario(3, "3. Workshop Fire"));
        }

        private void LaunchScenario(int scenarioIndex, string scenarioTitle)
        {
            if (AppState.Instance != null)
            {
                AppState.Instance.SelectScenario(scenarioIndex, scenarioTitle);
            }

            var moduleToLaunch = _module ?? AppManager.Instance?.GetModule(ModuleId.FireAndExplosion);
            if (moduleToLaunch != null && moduleToLaunch.isImplemented && ARModuleLauncher.Instance != null)
            {
                bool launched = ARModuleLauncher.Instance.TryLaunchModule(moduleToLaunch);
                if (!launched)
                {
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
            UIManager.Instance?.ShowScreen(ScreenId.ModuleDetail, _module);
        }

        public void OnHide()
        {
            if (_btnBack != null) _btnBack.onClick.RemoveListener(GoBack);
        }
    }
}
