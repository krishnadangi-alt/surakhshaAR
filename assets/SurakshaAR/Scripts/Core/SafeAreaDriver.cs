using UnityEngine;

namespace SurakshaAR.Core
{
    /// <summary>
    /// Applies Android safe-area insets to a RectTransform panel.
    /// Replaces the old UIToolkit SafeAreaDriver which operated on VisualElements.
    ///
    /// Attach this to the SafeArea RectTransform child of the main Canvas.
    /// It adjusts the RectTransform anchors/offsets each frame to match
    /// Screen.safeArea, keeping UI content out of the status bar, notch,
    /// camera cutout, and gesture navigation bar.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SafeAreaDriver : MonoBehaviour
    {
        public static SafeAreaDriver Instance { get; private set; }

        private RectTransform _rectTransform;
        private Rect _lastSafeArea = Rect.zero;
        private ScreenOrientation _lastOrientation = ScreenOrientation.AutoRotation;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            _rectTransform = GetComponent<RectTransform>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            Rect safe = Screen.safeArea;
            ScreenOrientation orientation = Screen.orientation;

            if (safe != _lastSafeArea || orientation != _lastOrientation)
            {
                _lastSafeArea = safe;
                _lastOrientation = orientation;
                ApplySafeArea(safe);
            }
        }

        private void ApplySafeArea(Rect safe)
        {
            if (_rectTransform == null) return;

            // Convert safe area from screen pixels to canvas-relative anchors.
            float sw = Screen.width;
            float sh = Screen.height;

            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;

            anchorMin.x /= sw;
            anchorMin.y /= sh;
            anchorMax.x /= sw;
            anchorMax.y /= sh;

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }

        /// <summary>Legacy compatibility stub — no longer used.</summary>
        public void RegisterRoot(object root) { }
    }
}
