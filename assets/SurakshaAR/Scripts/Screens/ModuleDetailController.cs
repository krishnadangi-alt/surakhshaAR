using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.UI;
using SurakshaAR.UI.Builders;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    public class ModuleDetailController : IScreenController
    {
        private ModuleData _module;
        private Button _btnBack, _btnStart;

        public void OnShow(GameObject root, object param)
        {
            _module = param as ModuleData ?? AppState.Instance?.SelectedModule ?? AppManager.Instance?.GetModule(ModuleId.FireAndExplosion);

            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null) _btnBack.onClick.AddListener(GoBack);

            _btnStart = UIHelper.FindButton(root, "btn-start-module");
            if (_btnStart != null) _btnStart.onClick.AddListener(StartTraining);

            BindModuleData(root);
        }

        private void BindModuleData(GameObject root)
        {
            if (_module == null) return;

            var loc = AppManager.Instance?.Localization;
            string title = loc != null ? loc.Get(_module.titleKey) : _module.titleKey;
            string description = loc != null ? loc.Get(_module.descriptionKey) : _module.descriptionKey;
            string difficulty = loc != null ? loc.Get(_module.difficultyKey) : _module.difficultyKey;

            var labelTitle = UIHelper.FindTMP(root, "label-title");
            if (labelTitle != null) labelTitle.text = title;

            var labelModuleName = UIHelper.FindTMP(root, "label-module-name");
            if (labelModuleName != null) labelModuleName.text = title;

            var labelDescription = UIHelper.FindTMP(root, "label-description");
            if (labelDescription != null) labelDescription.text = description;

            // Stat chips
            var chipScenarios = UIHelper.FindTMP(root, "chip-scenarios-value");
            if (chipScenarios != null) chipScenarios.text = $"📑 {_module.scenarioCount} Steps";

            var chipDuration = UIHelper.FindTMP(root, "chip-duration-value");
            if (chipDuration != null && !string.IsNullOrEmpty(_module.durationLabel))
            {
                chipDuration.text = $"⏱ {_module.durationLabel}";
            }

            var chipDifficulty = UIHelper.FindTMP(root, "chip-difficulty-value");
            if (chipDifficulty != null) chipDifficulty.text = $"📊 {difficulty}";

            // Learn list
            var learnList = UIHelper.FindRect(root, "learn-list");
            if (learnList != null && _module.learningPointKeys != null && _module.learningPointKeys.Count > 0)
            {
                for (int i = learnList.childCount - 1; i >= 0; i--)
                {
                    Object.Destroy(learnList.GetChild(i).gameObject);
                }

                foreach (string key in _module.learningPointKeys)
                {
                    ModuleDetailBuilder.MakeLearnItem(learnList, loc != null ? loc.Get(key) : key);
                }
            }
        }

        private void StartTraining()
        {
            if (AppState.Instance != null && _module != null)
            {
                AppState.Instance.SelectedModule = _module;
            }

            var loc = AppManager.Instance?.Localization;
            string scenarioTitle = loc != null ? loc.Get("scenario.s1.title") : null;
            if (string.IsNullOrEmpty(scenarioTitle) || scenarioTitle == "scenario.s1.title")
            {
                scenarioTitle = "1. Electrical Panel Fire";
            }

            if (AppState.Instance != null)
            {
                AppState.Instance.SelectScenario(1, scenarioTitle);
            }

            var moduleToLaunch = _module ?? AppManager.Instance?.GetModule(ModuleId.FireAndExplosion);
            if (moduleToLaunch != null && moduleToLaunch.isImplemented && ARModuleLauncher.Instance != null)
            {
                bool launched = ARModuleLauncher.Instance.TryLaunchModule(moduleToLaunch);
                if (!launched)
                {
                    Debug.LogWarning(
                        $"[ModuleDetail] AR scene '{moduleToLaunch.arSceneName}' could not be launched directly. Navigating to Assessment simulation.");
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
            UIManager.Instance?.ShowScreen(ScreenId.HomeDashboard);
        }

        public void OnHide()
        {
            if (_btnBack != null) _btnBack.onClick.RemoveListener(GoBack);
            if (_btnStart != null) _btnStart.onClick.RemoveListener(StartTraining);
        }
    }
}
