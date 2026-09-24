using System;
using System.Collections.Generic;
using SurakshaAR.Data;
using SurakshaAR.Localization;
using SurakshaAR.Screens;
using SurakshaAR.UI;
using SurakshaAR.UI.Builders;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace SurakshaAR.Core
{
    /// <summary>
    /// uGUI application shell manager.
    /// Replaces the old UIToolkit UIDocument-based implementation.
    /// Builds and manages a Canvas hierarchy at runtime. Loads screens
    /// by instantiating uGUI GameObjects built by ScreenBuilders.
    /// 
    /// Public API is identical to the old UIManager so AppManager,
    /// UIController, and ARModuleLauncher require zero changes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        // ── Public state ──────────────────────────────────────────────
        public ScreenId CurrentScreen { get; private set; }
        public ScreenId PreviousScreen { get; private set; }
        public bool HasPreviousScreen { get; private set; }

        // Stores the last param passed to ShowScreen so we can re-call OnShow on language change
        private object _activeScreenParam;

        // ── Internal canvas hierarchy ─────────────────────────────────
        private Canvas _mainCanvas;
        private Image _canvasBg;
        private RectTransform _screenContainer;
        private GameObject _activeScreenGO;
        private IScreenController _activeController;
        private bool _hasShownAnyScreen;

        // ──────────────────────────────────────────────────────────────
        //  AWAKE — Build the uGUI shell
        // ──────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                UI.UIHelper.SafeDestroy(gameObject);
                return;
            }
            Instance = this;

            // Disable the legacy UIDocument (if any) so it doesn't render
            DisableLegacyUIToolkit();

            // Build Canvas hierarchy
            BuildCanvasHierarchy();

            // EventSystem (required for uGUI input)
            EnsureEventSystem();

            // Subscribe to language change — refresh active screen text immediately
            StartCoroutine(SubscribeToLocalizationWhenReady());
        }

        // ──────────────────────────────────────────────────────────────
        //  LOCALIZATION SUBSCRIPTION
        // ──────────────────────────────────────────────────────────────
        private System.Collections.IEnumerator SubscribeToLocalizationWhenReady()
        {
            // Wait until AppManager is initialized (it sets Localization in Awake)
            int maxWait = 60;
            while (AppManager.Instance?.Localization == null && maxWait-- > 0)
                yield return null;

            if (AppManager.Instance?.Localization != null)
            {
                AppManager.Instance.Localization.OnLanguageChanged += OnLanguageChanged;
                Debug.Log("[UIManager] Subscribed to OnLanguageChanged.");
            }
        }

        private void OnLanguageChanged(SurakshaAR.Data.AppLanguage lang)
        {
            Debug.Log($"[UIManager] Language changed to {lang} — refreshing screen.");
            RefreshCurrentScreen();
        }

        private void OnDestroy()
        {
            if (AppManager.Instance?.Localization != null)
                AppManager.Instance.Localization.OnLanguageChanged -= OnLanguageChanged;
        }

        // ──────────────────────────────────────────────────────────────
        //  SHOW SCREEN
        // ──────────────────────────────────────────────────────────────
        public void ShowScreen(ScreenId screenId, object param = null)
        {
            // Hide / cleanup active screen
            _activeController?.OnHide();
            _activeController = null;

            if (_activeScreenGO != null)
            {
                UI.UIHelper.SafeDestroy(_activeScreenGO);
                _activeScreenGO = null;
            }

            // Build new screen GO
            GameObject screenGO = ScreenFactory.Build(screenId);
            if (screenGO == null)
            {
                Debug.LogError($"[UIManager] ScreenFactory returned null for {screenId}.");
                return;
            }

            // Parent into container (full stretch)
            screenGO.transform.SetParent(_screenContainer, false);
            var rt = screenGO.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            _activeScreenGO = screenGO;
            UpdateCanvasBackground(screenId);

            // Force immediate layout update so layout groups calculate with the actual container bounds
            Canvas.ForceUpdateCanvases();
            if (rt != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }

            // Synchronize font for active language across all text components on every screen
            var currentLang = AppState.Instance != null
                ? AppState.Instance.CurrentLanguage
                : (Localization.LocalizationManager.Instance?.CurrentLanguage ?? AppLanguage.English);
            ApplyLanguageFonts(screenGO, currentLang);

            // Track navigation history
            if (_hasShownAnyScreen)
            {
                PreviousScreen = CurrentScreen;
                HasPreviousScreen = true;
            }
            _hasShownAnyScreen = true;
            CurrentScreen = screenId;

            // Bind controller
            _activeController = CreateController(screenId);
            _activeScreenParam = param;
            _activeController?.OnShow(screenGO, param);

            // Re-apply fonts to catch any dynamically created UI elements created during OnShow
            ApplyLanguageFonts(screenGO, currentLang);
        }

        // ──────────────────────────────────────────────────────────────
        //  LANGUAGE REFRESH — Re-renders the active screen in-place
        // ──────────────────────────────────────────────────────────────
        /// <summary>
        /// Called when the language changes. Re-invokes OnShow on the
        /// current screen controller so all text refreshes immediately
        /// without a scene reload or screen transition.
        /// </summary>
        public void RefreshCurrentScreen()
        {
            if (_activeController != null && _activeScreenGO != null)
            {
                try
                {
                    var currentLang = AppState.Instance != null
                        ? AppState.Instance.CurrentLanguage
                        : (Localization.LocalizationManager.Instance?.CurrentLanguage ?? AppLanguage.English);
                    ApplyLanguageFonts(_activeScreenGO, currentLang);
                    _activeController.OnShow(_activeScreenGO, _activeScreenParam);
                    ApplyLanguageFonts(_activeScreenGO, currentLang);
                    Canvas.ForceUpdateCanvases();
                    foreach (var fitter in _activeScreenGO.GetComponentsInChildren<ContentSizeFitter>(true))
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(fitter.GetComponent<RectTransform>());
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[UIManager] RefreshCurrentScreen error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Applies the language-appropriate font (Devanagari for Hindi, Ol Chiki for Santali, Latin for English)
        /// to all TextMeshProUGUI components within the given root hierarchy.
        /// Strictly enforces Left-to-Right (LTR) rendering path, eliminating any RTL mirroring,
        /// inverted scaleX (-1), or character reversals for Santali (Ol Chiki) and all supported languages.
        /// </summary>
        public void ApplyLanguageFonts(GameObject screenGO, AppLanguage lang)
        {
            if (screenGO == null) return;

            // 1. Enforce strict Left-to-Right layout across the whole screen hierarchy
            EnforceStrictLtrLayout(screenGO);

            var font = UI.UIHelper.GetFontForLanguage(lang);
            if (font == null) return;

            foreach (var tmp in screenGO.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
            {
                try
                {
                    // Strict LTR guarantee: never allow TextMeshPro RTL reversal for Santali, Hindi, or English
                    tmp.isRightToLeftText = false;

                    // Ensure RectTransform horizontal scale is positive (never inverted/mirrored)
                    var tmpRT = tmp.rectTransform;
                    if (tmpRT != null && tmpRT.localScale.x < 0f)
                    {
                        var s = tmpRT.localScale;
                        s.x = Mathf.Abs(s.x);
                        tmpRT.localScale = s;
                    }

                    // On language selection screen, protect each individual language card's title/subtitle so they always display in their own native script
                    var parentCard = tmp.GetComponentInParent<Button>();
                    if (parentCard != null)
                    {
                        if (parentCard.name == "row-hindi")
                        {
                            var hiFont = UI.UIHelper.GetDevanagariFont();
                            if (hiFont != null) tmp.font = hiFont;
                            continue;
                        }
                        if (parentCard.name == "row-santali")
                        {
                            var satFont = UI.UIHelper.GetSantaliFont();
                            if (satFont != null) tmp.font = satFont;
                            continue;
                        }
                        if (parentCard.name == "row-english")
                        {
                            var enFont = UI.UIHelper.GetDefaultFont();
                            if (enFont != null) tmp.font = enFont;
                            continue;
                        }
                    }

                    tmp.font = font;

                    if (lang == AppLanguage.Hindi && !string.IsNullOrEmpty(tmp.text) && DevanagariShaper.HasDevanagari(tmp.text))
                    {
                        tmp.text = DevanagariShaper.Shape(tmp.text);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[UIManager] Failed to apply language font to {tmp.name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Traverses all RectTransforms, HorizontalLayoutGroups, and Text components
        /// in the hierarchy, guaranteeing that scaleX is never negative (-1),
        /// child arrangement is never reversed, and text direction is strictly Left-to-Right.
        /// Santali (Ol Chiki), Hindi (Devanagari), and English (Latin) are strictly Left-to-Right.
        /// </summary>
        public void EnforceStrictLtrLayout(GameObject root)
        {
            if (root == null) return;

            // 1. Ensure all RectTransforms have strictly positive localScale.x (never -1 / mirrored)
            foreach (var rt in root.GetComponentsInChildren<RectTransform>(true))
            {
                var s = rt.localScale;
                if (s.x < 0f)
                {
                    s.x = Mathf.Abs(s.x);
                    rt.localScale = s;
                }
            }

            // 2. Ensure all HorizontalLayoutGroups flow normally Left-to-Right (reverseArrangement = false)
            foreach (var hlg in root.GetComponentsInChildren<HorizontalLayoutGroup>(true))
            {
                if (hlg.reverseArrangement)
                {
                    hlg.reverseArrangement = false;
                }
            }

            // 3. Ensure all TextMeshProUGUI components have isRightToLeftText = false
            foreach (var tmp in root.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
            {
                tmp.isRightToLeftText = false;
            }
        }


        // ──────────────────────────────────────────────────────────────
        //  SET UI VISIBLE  (called by ARModuleLauncher)
        // ──────────────────────────────────────────────────────────────
        public void SetUIVisible(bool visible)
        {
            if (_mainCanvas != null)
                _mainCanvas.gameObject.SetActive(visible);
        }

        // ──────────────────────────────────────────────────────────────
        //  BUILD CANVAS HIERARCHY
        // ──────────────────────────────────────────────────────────────
        private void BuildCanvasHierarchy()
        {
            // ── Main Canvas ───────────────────────────────────────────
            var canvasGO = new GameObject("MainCanvas");
            canvasGO.transform.SetParent(transform, false);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            _mainCanvas = canvas;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 2400);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // ── Fullscreen Canvas Background (fills notches, camera cutouts, nav insets) ──
            var bgGO = new GameObject("CanvasBackground");
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            _canvasBg = bgGO.AddComponent<Image>();
            _canvasBg.color = UIColors.Background;
            _canvasBg.sprite = UIHelper.GetWhiteSprite();
            _canvasBg.raycastTarget = false;

            // ── Safe Area Panel ───────────────────────────────────────
            var safeAreaGO = new GameObject("SafeArea");
            safeAreaGO.transform.SetParent(canvasGO.transform, false);

            var safeAreaRT = safeAreaGO.AddComponent<RectTransform>();
            safeAreaRT.anchorMin = Vector2.zero;
            safeAreaRT.anchorMax = Vector2.one;
            safeAreaRT.offsetMin = Vector2.zero;
            safeAreaRT.offsetMax = Vector2.zero;

            safeAreaGO.AddComponent<SafeAreaDriver>();

            // ── Screen Container ──────────────────────────────────────
            var containerGO = new GameObject("ScreenContainer");
            containerGO.transform.SetParent(safeAreaGO.transform, false);

            _screenContainer = containerGO.AddComponent<RectTransform>();
            _screenContainer.anchorMin = Vector2.zero;
            _screenContainer.anchorMax = Vector2.one;
            _screenContainer.offsetMin = Vector2.zero;
            _screenContainer.offsetMax = Vector2.zero;
        }

        private void UpdateCanvasBackground(ScreenId screenId)
        {
            if (_canvasBg == null) return;
            switch (screenId)
            {
                case ScreenId.Splash:
                    _canvasBg.color = UIColors.PrimaryDark;
                    break;
                case ScreenId.Login:
                    _canvasBg.color = UIColors.Hex("#CAE4F5");
                    break;
                case ScreenId.Assessment:
                    _canvasBg.color = UIColors.Hex("#0A1926");
                    break;
                case ScreenId.Certificate:
                    _canvasBg.color = UIColors.Hex("#07131F");
                    break;
                default:
                    _canvasBg.color = UIColors.Hex("#F8FAFC");
                    break;
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  DISABLE LEGACY UI TOOLKIT
        // ──────────────────────────────────────────────────────────────
        private void DisableLegacyUIToolkit()
        {
            // Use reflection so we don't need a using for UIElements
            foreach (var comp in GetComponents<MonoBehaviour>())
            {
                if (comp == null || comp == this) continue;
                string typeName = comp.GetType().Name;
                if (typeName == "UIDocument" || typeName == "PanelRenderer")
                {
                    comp.enabled = false;
                    Debug.Log($"[UIManager] Disabled legacy UIToolkit component: {typeName}");
                }
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  EVENT SYSTEM
        // ──────────────────────────────────────────────────────────────
        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                // This project uses the new Input System only
                // (ProjectSettings activeInputHandler = 1), so the legacy
                // StandaloneInputModule cannot process uGUI clicks.
                // InputSystemUIInputModule auto-assigns DefaultInputActions
                // in its OnEnable, so it works out of the box.
                esGO.AddComponent<InputSystemUIInputModule>();
                if (Application.isPlaying) DontDestroyOnLoad(esGO);
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  CONTROLLER FACTORY
        // ──────────────────────────────────────────────────────────────
        private static IScreenController CreateController(ScreenId screenId)
        {
            switch (screenId)
            {
                case ScreenId.Splash:             return new SplashScreenController();
                case ScreenId.LanguageSelection:  return new LanguageSelectionController();
                case ScreenId.Login:              return new LoginController();
                case ScreenId.HomeDashboard:      return new HomeDashboardController();
                case ScreenId.ModuleSelection:    return new ModuleSelectionController();
                case ScreenId.ModuleDetail:       return new ModuleDetailController();
                case ScreenId.ScenarioSelection:  return new ScenarioSelectionController();
                case ScreenId.Assessment:         return new AssessmentController();
                case ScreenId.Result:             return new ResultController();
                case ScreenId.ARTraining:         return null; // AR scene, no UI controller
                case ScreenId.Certificate:        return new CertificateController();
                case ScreenId.Progress:           return new ProgressController();
                case ScreenId.ProfileSetup:       return new ProfileController();
                case ScreenId.Notifications:      return new NotificationsController();
                case ScreenId.TrainingInstructions: return new TrainingInstructionsController();
                case ScreenId.GasScenarioSelection: return new GasScenarioSelectionController();
                case ScreenId.GasModuleDetail:      return new GasModuleDetailController();
                default:                          return null;
            }
        }
    }
}
