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
        private Button _btnLangPicker;
        private TextMeshProUGUI _langPillText;
        private Image _checkSquareImg;
        private TextMeshProUGUI _checkMarkSymbol;
        private bool _isAgreed = true; // Checked by default as shown in reference UI

        public void OnShow(GameObject root, object param)
        {
            var loc = AppManager.Instance?.Localization ?? LocalizationManager.Instance;
            var currentLang = AppState.Instance != null
                ? AppState.Instance.CurrentLanguage
                : (loc != null ? loc.CurrentLanguage : AppLanguage.English);

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

                var startLbl = _btnStartTraining.GetComponentInChildren<TextMeshProUGUI>();
                if (startLbl != null && loc != null)
                {
                    startLbl.text = loc.Get("inst.btn.start");
                }
            }

            _btnLangPicker = UIHelper.FindButton(root, "btn-language-picker");
            if (_btnLangPicker != null)
            {
                _langPillText = _btnLangPicker.GetComponentInChildren<TextMeshProUGUI>();
                if (_langPillText != null)
                {
                    _langPillText.font = UIHelper.GetFontForLanguage(currentLang);
                    _langPillText.text = currentLang switch
                    {
                        AppLanguage.Hindi => DevanagariShaper.Shape("हिन्दी"),
                        AppLanguage.Santali => "ᱥᱟᱱᱛᱟᱲᱤ",
                        _ => "English"
                    };
                }
                _btnLangPicker.onClick.RemoveAllListeners();
                _btnLangPicker.onClick.AddListener(CycleLanguage);
            }

            // ── Localize Header ──────────────────────────────────────────
            var titleLbl = UIHelper.FindTMP(root, "label-title");
            if (titleLbl != null && loc != null)
            {
                titleLbl.text = loc.Get("inst.header.title");
            }

            // ── Localize Banner ──────────────────────────────────────────
            var headLbl = root.transform.Find("ScrollArea/Viewport/Content/InfoBanner/InnerRow/TextCol/HeadLbl")?.GetComponent<TextMeshProUGUI>();
            if (headLbl != null && loc != null) headLbl.text = loc.Get("inst.banner.title");

            var descLbl = root.transform.Find("ScrollArea/Viewport/Content/InfoBanner/InnerRow/TextCol/DescLbl")?.GetComponent<TextMeshProUGUI>();
            if (descLbl != null && loc != null) descLbl.text = loc.Get("inst.banner.desc");

            // ── Localize Instruction Cards ───────────────────────────────
            var cards = new[] { "card-inst-1", "card-inst-2", "card-inst-3", "card-inst-4", "card-inst-5" };
            for (int i = 0; i < cards.Length; i++)
            {
                var cardText = root.transform.Find($"ScrollArea/Viewport/Content/CardsCol/{cards[i]}/CardRow/TextLbl")?.GetComponent<TextMeshProUGUI>();
                if (cardText != null && loc != null)
                {
                    cardText.text = loc.Get($"inst.card.{i + 1}");
                }
            }

            // ── Localize Agreement Checkbox ──────────────────────────────
            var agreeLbl = UIHelper.FindTMP(root, "label-agreement-text");
            if (agreeLbl != null && loc != null)
            {
                agreeLbl.text = loc.Get("inst.checkbox.agreement");
            }

            _btnAgreementToggle = UIHelper.FindButton(root, "btn-agreement-toggle");
            _checkSquareImg = UIHelper.FindRect(root, "check-box-square")?.GetComponent<Image>();
            _checkMarkSymbol = UIHelper.FindTMP(root, "check-mark-symbol");

            if (_btnAgreementToggle != null)
            {
                _btnAgreementToggle.onClick.RemoveAllListeners();
                _btnAgreementToggle.onClick.AddListener(ToggleAgreement);
            }

            UpdateCheckboxVisuals();

            UIManager.Instance?.ApplyLanguageFonts(root, currentLang);
            Canvas.ForceUpdateCanvases();
            var rootRT = root.GetComponent<RectTransform>();
            if (rootRT != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rootRT);
            }
        }

        private void CycleLanguage()
        {
            var current = AppState.Instance != null ? AppState.Instance.CurrentLanguage : AppLanguage.English;
            AppLanguage nextLang = current switch
            {
                AppLanguage.English => AppLanguage.Hindi,
                AppLanguage.Hindi => AppLanguage.Santali,
                _ => AppLanguage.English
            };

            AppState.Instance?.SetLanguage(nextLang);
            UIManager.Instance?.RefreshCurrentScreen();
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
            if (_btnLangPicker != null) _btnLangPicker.onClick.RemoveAllListeners();
        }
    }
}
