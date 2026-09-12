using System;
using System.Collections.Generic;
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

        // ── Internal canvas hierarchy ─────────────────────────────────
        private Canvas _mainCanvas;
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
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Disable the legacy UIDocument (if any) so it doesn't render
            DisableLegacyUIToolkit();

            // Build Canvas hierarchy
            BuildCanvasHierarchy();

            // EventSystem (required for uGUI input)
            EnsureEventSystem();
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
                Destroy(_activeScreenGO);
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
            _activeController?.OnShow(screenGO, param);
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
                DontDestroyOnLoad(esGO);
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
                case ScreenId.ProfileSetup:       return new ProfileSetupController();
                default:                          return null;
            }
        }
    }
}
