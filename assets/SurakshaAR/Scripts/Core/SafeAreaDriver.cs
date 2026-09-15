using UnityEngine;
using UnityEngine.UI;

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
        private CanvasScaler _scaler;
        private Rect _lastSafeArea = Rect.zero;
        private ScreenOrientation _lastOrientation = ScreenOrientation.AutoRotation;

        private void Awake()
        {
            if (!Application.isPlaying) return;
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            _rectTransform = GetComponent<RectTransform>();
            _scaler = GetComponentInParent<CanvasScaler>();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (!Application.isPlaying) return;
            ApplySafeArea(Screen.safeArea);
        }

        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            ApplySafeArea(Screen.safeArea);
        }

        private void Update()
        {
            if (!Application.isPlaying) return;
            Rect safe = Screen.safeArea;
            ScreenOrientation orientation = Screen.orientation;

            if (safe != _lastSafeArea || orientation != _lastOrientation)
            {
                _lastSafeArea = safe;
                _lastOrientation = orientation;
                ApplySafeArea(safe);
            }

            if (_scaler != null)
            {
                // In portrait mobile (height >= width), match width (0f).
                // If rotated or split-screen landscape (width > height), match height (1f) to prevent vertical squash.
                float targetMatch = Screen.width > Screen.height ? 1f : 0f;
                if (!Mathf.Approximately(_scaler.matchWidthOrHeight, targetMatch))
                {
                    _scaler.matchWidthOrHeight = targetMatch;
                }
            }
        }

        private void ApplySafeArea(Rect safe)
        {
            if (_rectTransform == null) return;

            // Convert safe area from screen pixels to canvas-relative anchors.
            float sw = Screen.width;
            float sh = Screen.height;
            if (sw <= 0f || sh <= 0f) return;

            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;

            anchorMin.x /= sw;
            anchorMin.y /= sh;
            anchorMax.x /= sw;
            anchorMax.y /= sh;

            anchorMin.x = Mathf.Clamp01(anchorMin.x);
            anchorMin.y = Mathf.Clamp01(anchorMin.y);
            anchorMax.x = Mathf.Clamp01(anchorMax.x);
            anchorMax.y = Mathf.Clamp01(anchorMax.y);

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }

        /// <summary>Legacy compatibility stub — no longer used.</summary>
        public void RegisterRoot(object root) { }
    }
}
