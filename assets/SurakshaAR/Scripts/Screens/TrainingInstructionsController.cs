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

            bool isGasModule = (AppState.Instance?.SelectedModule?.id == ModuleId.GasLeakConfinedSpace) ||
                               (AppState.Instance != null && AppState.Instance.SelectedScenarioIndex >= 100);

            _btnBack = UIHelper.FindButton(root, "btn-back");
            if (_btnBack != null)
            {
                _btnBack.onClick.RemoveAllListeners();
                _btnBack.onClick.AddListener(() =>
                {
                    Debug.Log("[GAS UI INPUT] BACK TAP RECEIVED");
                    Debug.Log($"[GAS UI INPUT] Back Button details: name={_btnBack.gameObject.name}, activeInHierarchy={_btnBack.gameObject.activeInHierarchy}, interactable={_btnBack.interactable}");
                    OnBack();
                });
                _btnBack.interactable = true;
                _btnBack.gameObject.SetActive(true);
            }

            _btnStartTraining = UIHelper.FindButton(root, "btn-start-training");
            if (_btnStartTraining != null)
            {
                _btnStartTraining.onClick.RemoveAllListeners();
                _btnStartTraining.onClick.AddListener(() =>
                {
                    Debug.Log("[GAS UI INPUT] START TRAINING TAP RECEIVED");
                    Debug.Log($"[GAS UI INPUT] Start Button details: name={_btnStartTraining.gameObject.name}, activeInHierarchy={_btnStartTraining.gameObject.activeInHierarchy}, interactable={_btnStartTraining.interactable}");
                    LaunchARScenario();
                });
                _btnStartTraining.interactable = true;
                _btnStartTraining.gameObject.SetActive(true);

                var startLbl = _btnStartTraining.GetComponentInChildren<TextMeshProUGUI>();
                if (startLbl != null)
                {
                    startLbl.text = loc?.Get("inst.btn.start") ?? "START TRAINING";
                }
            }

            var es = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            var gr = root.GetComponentInParent<GraphicRaycaster>();
            Debug.Log($"[GAS UI INPUT] Diagnostic Check: btnStart={(_btnStartTraining != null)}, btnBack={(_btnBack != null)}, eventSystem={es != null && es.enabled}, graphicRaycaster={gr != null && gr.enabled}");

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
            if (titleLbl != null)
            {
                titleLbl.text = isGasModule ? "Gas Leak Response & Emergency Isolation" : (loc?.Get("inst.header.title") ?? "Electrical Panel Fire");
            }

            // ── Localize Banner ──────────────────────────────────────────
            var headLbl = root.transform.Find("ScrollArea/Viewport/Content/InfoBanner/InnerRow/TextCol/HeadLbl")?.GetComponent<TextMeshProUGUI>();
            if (headLbl != null)
            {
                headLbl.text = isGasModule ? "Gas Leak Response & Emergency Isolation" : (loc?.Get("inst.banner.title") ?? "Electrical Panel Fire Instructions");
            }

            var descLbl = root.transform.Find("ScrollArea/Viewport/Content/InfoBanner/InnerRow/TextCol/DescLbl")?.GetComponent<TextMeshProUGUI>();
            if (descLbl != null)
            {
                descLbl.text = isGasModule
                    ? "Practice identifying a simulated gas leak, checking the atmosphere, recognizing the hazard area, following the required PPE and standby procedure, and performing simulated emergency isolation."
                    : (loc?.Get("inst.banner.desc") ?? "Follow all safety guidelines and instructions before beginning the practical AR training drill.");
            }

            // ── Localize Instruction Cards ───────────────────────────────
            string[] gasCards = new string[]
            {
                "1. Start the drill by identifying the Gas Cylinder.",
                "2. Check the atmosphere using the Multi-Gas Detector.",
                "3. Identify the hazard area.",
                "4. Select the required PPE.",
                "5. Confirm the standby/buddy procedure.",
                "6. Perform simulated isolation using the RED Isolation Valve.",
                "7. Verify the safe state."
            };

            var cards = new[] { "card-inst-1", "card-inst-2", "card-inst-3", "card-inst-4", "card-inst-5" };
            for (int i = 0; i < cards.Length; i++)
            {
                var cardText = root.transform.Find($"ScrollArea/Viewport/Content/CardsCol/{cards[i]}/CardRow/TextLbl")?.GetComponent<TextMeshProUGUI>();
                if (cardText != null)
                {
                    if (isGasModule)
                    {
                        cardText.text = (i < gasCards.Length) ? gasCards[i] : "";
                    }
                    else if (loc != null)
                    {
                        cardText.text = loc.Get($"inst.card.{i + 1}");
                    }
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

        private void LaunchARScenario()
        {
            if (!_isAgreed) return;

            bool isGasModule = (AppState.Instance?.SelectedModule?.id == ModuleId.GasLeakConfinedSpace) ||
                               (AppState.Instance != null && AppState.Instance.SelectedScenarioIndex >= 100);

            ModuleData targetModule;

            if (isGasModule)
            {
                targetModule = AppManager.Instance?.GetModule(ModuleId.GasLeakConfinedSpace);
                if (targetModule == null)
                {
                    targetModule = new ModuleData
                    {
                        id = ModuleId.GasLeakConfinedSpace,
                        titleKey = "module.gas.title",
                        arSceneName = "AR_gas_Foundation",
                        isImplemented = true
                    };
                }
                else
                {
                    targetModule.isImplemented = true;
                    targetModule.arSceneName = "AR_gas_Foundation";
                }
            }
            else
            {
                targetModule = AppManager.Instance?.GetModule(ModuleId.FireAndExplosion);
                if (targetModule == null)
                {
                    targetModule = new ModuleData
                    {
                        id = ModuleId.FireAndExplosion,
                        titleKey = "module.fire.title",
                        arSceneName = "FireTraining",
                        isImplemented = true
                    };
                }
            }

            if (AppState.Instance != null)
            {
                AppState.Instance.SelectedModule = targetModule;
            }

            if (ARModuleLauncher.Instance != null && targetModule.isImplemented)
            {
                bool launched = ARModuleLauncher.Instance.TryLaunchModule(targetModule);
                if (!launched)
                {
                    Debug.LogWarning("[TrainingInstructions] ARModuleLauncher returned false. Loading scene directly: " + targetModule.arSceneName);
                    UnityEngine.SceneManagement.SceneManager.LoadScene(targetModule.arSceneName);
                }
            }
            else
            {
                Debug.LogWarning("[TrainingInstructions] Direct loading AR scene: " + targetModule.arSceneName);
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetModule.arSceneName);
            }
        }

        private void OnBack()
        {
            bool isGasModule = (AppState.Instance?.SelectedModule?.id == ModuleId.GasLeakConfinedSpace) ||
                               (AppState.Instance != null && AppState.Instance.SelectedScenarioIndex >= 100);

            if (isGasModule)
            {
                UIManager.Instance?.ShowScreen(ScreenId.GasModuleDetail);
            }
            else
            {
                UIManager.Instance?.ShowScreen(ScreenId.ModuleDetail);
            }
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
