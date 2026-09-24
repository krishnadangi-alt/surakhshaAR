using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SurakshaAR.Core
{
    /// <summary>
    /// Central UI navigation controller. Manages navigation stack,
    /// backward navigation, and transitions between screens.
    /// </summary>
    [DisallowMultipleComponent]
    public class UIController : MonoBehaviour
    {
        public static UIController Instance { get; private set; }

        private readonly Stack<ScreenId> _history = new Stack<ScreenId>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            // Android hardware/gesture back (and desktop Escape) walks the
            // app's navigation stack backwards. While an AR scene is active,
            // ARModuleLauncher owns the Back key and exits AR instead.
            // NOTE: project uses the new Input System only (activeInputHandler=1),
            // so we use Keyboard.current instead of the legacy Input class.
            if (Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                var launcher = AppManager.Instance != null ? AppManager.Instance.ARLauncher : null;
                bool arActive = launcher != null && launcher.IsArActive;
                if (!arActive)
                {
                    GoBack();
                }
            }
        }

        public void NavigateTo(ScreenId screenId, object param = null, bool addToHistory = true)
        {
            if (addToHistory && UIManager.Instance != null && UIManager.Instance.CurrentScreen != screenId)
            {
                // Don't push splash to history
                if (UIManager.Instance.CurrentScreen != ScreenId.Splash)
                {
                    _history.Push(UIManager.Instance.CurrentScreen);
                }
            }

            UIManager.Instance?.ShowScreen(screenId, param);
        }

        public void GoBack()
        {
            var manager = UIManager.Instance;
            if (manager == null) return;

            // Never navigate away from the splash screen.
            if (manager.CurrentScreen == ScreenId.Splash) return;

            if (_history.Count > 0)
            {
                ScreenId prevScreen = _history.Pop();
                manager.ShowScreen(prevScreen);
            }
            else if (manager.HasPreviousScreen)
            {
                // Fall back to the screen that led here (e.g. Language -> Login).
                manager.ShowScreen(manager.PreviousScreen);
            }
            // Else: nothing to go back to; stay on the current screen.
        }

        public void ClearHistory()
        {
            _history.Clear();
        }
    }
}
