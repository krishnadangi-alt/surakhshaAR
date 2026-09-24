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
    /// TrainingInstructionsController (Screen 6 in reference UI):
    /// Handles the pre-AR instructions checklist, confirmation agreement toggle,
    /// and launches the authoritative existing Fire AR scenario.
    /// </summary>
    public class TrainingInstructionsController : IScreenController
    {
        private Button _btnBack;
        private Button _btnStartTraining;
        private Button _btnAgreementToggle;
        private Image _checkSquareImg;
        private TextMeshProUGUI _checkMarkSymbol;
        private bool _isAgreed = true; // Checked by default as shown in reference UI

        public void OnShow(GameObject root, object param)
        {
            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null)
            {
                _btnBack.onClick.RemoveAllListeners();
                _btnBack.onClick.AddListener(OnBack);
            }

            _btnStartTraining = UIHelper.FindButton(root, "btn-start-training");
            if (_btnStartTraining != null)
            {
                _btnStartTraining.onClick.RemoveAllListeners();
                _btnStartTraining.onClick.AddListener(LaunchFireARScenario);
            }

            _btnAgreementToggle = UIHelper.FindButton(root, "btn-agreement-toggle");
            _checkSquareImg = root.transform.Find("ScrollArea/Viewport/Content/AgreementBox/Row/check-box-square")?.GetComponent<Image>();
            _checkMarkSymbol = root.transform.Find("ScrollArea/Viewport/Content/AgreementBox/Row/check-box-square/check-mark-symbol")?.GetComponent<TextMeshProUGUI>();

            if (_btnAgreementToggle != null)
            {
                _btnAgreementToggle.onClick.RemoveAllListeners();
                _btnAgreementToggle.onClick.AddListener(ToggleAgreement);
            }

            UpdateCheckboxVisuals();

            var currentLang = AppState.Instance != null
                ? AppState.Instance.CurrentLanguage
                : AppLanguage.English;
            UIManager.Instance?.ApplyLanguageFonts(root, currentLang);
        }

        private void ToggleAgreement()
        {
            _isAgreed = !_isAgreed;
            UpdateCheckboxVisuals();
        }

        private void UpdateCheckboxVisuals()
        {
            if (_checkSquareImg != null)
            {
                _checkSquareImg.color = _isAgreed ? UIColors.Hex("#16A34A") : UIColors.Hex("#E2E8F0");
            }

            if (_checkMarkSymbol != null)
            {
                _checkMarkSymbol.text = _isAgreed ? "✓" : "";
            }

            if (_btnStartTraining != null)
            {
                _btnStartTraining.interactable = _isAgreed;
                var img = _btnStartTraining.GetComponent<Image>();
                if (img != null)
                {
                    img.color = _isAgreed ? UIColors.Hex("#16A34A") : UIColors.Hex("#94A3B8");
                }
            }
        }

        private void LaunchFireARScenario()
        {
            if (!_isAgreed) return;

            var fireModule = AppManager.Instance?.GetModule(ModuleId.FireAndExplosion);
            if (fireModule == null)
            {
                fireModule = new ModuleData
                {
                    id = ModuleId.FireAndExplosion,
                    titleKey = "module.fire.title",
                    arSceneName = "FireTraining",
                    isImplemented = true
                };
            }

            if (AppState.Instance != null)
            {
                AppState.Instance.SelectedModule = fireModule;
            }

            // Launch the authoritative existing Fire AR scenario
            if (ARModuleLauncher.Instance != null && fireModule.isImplemented)
            {
                bool launched = ARModuleLauncher.Instance.TryLaunchModule(fireModule);
                if (!launched)
                {
                    Debug.LogWarning("[TrainingInstructions] AR scene could not be launched directly. Routing to simulation assessment.");
                    UIManager.Instance?.ShowScreen(ScreenId.Assessment);
                }
            }
            else
            {
                UIManager.Instance?.ShowScreen(ScreenId.Assessment);
            }
        }

        private void OnBack()
        {
            // Back navigation: Training Instructions → Electrical Panel Fire detail
            UIManager.Instance?.ShowScreen(ScreenId.ModuleDetail);
        }

        public void OnHide()
        {
            if (_btnBack != null) _btnBack.onClick.RemoveAllListeners();
            if (_btnStartTraining != null) _btnStartTraining.onClick.RemoveAllListeners();
            if (_btnAgreementToggle != null) _btnAgreementToggle.onClick.RemoveAllListeners();
        }
    }
}
