using System.Collections;
using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.Localization;
using SurakshaAR.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    /// <summary>
    /// SplashScreenController
    /// ======================
    /// Drives the Splash screen loading animation, branding presentation,
    /// and transition into LanguageSelection or HomeDashboard.
    /// Supports automatic advance and tap/click to continue.
    /// </summary>
    public class SplashScreenController : IScreenController
    {
        private GameObject _root;
        private Coroutine _splashRoutine;
        private bool _hasAdvanced;

        public void OnShow(GameObject root, object param)
        {
            _root = root;
            _hasAdvanced = false;
            if (_root == null) return;

            var loc = AppManager.Instance?.Localization;
            var currentLang = AppState.Instance != null
                ? AppState.Instance.CurrentLanguage
                : (loc != null ? loc.CurrentLanguage : AppLanguage.English);

            var taglineTMP = UIHelper.FindTMP(root, "label-tagline");
            if (taglineTMP != null && loc != null)
            {
                taglineTMP.text = loc.Get("splash.tagline");
            }

            UIManager.Instance?.ApplyLanguageFonts(root, currentLang);

            // Tap anywhere on splash screen to advance immediately
            var tapBtn = root.GetComponent<Button>();
            if (tapBtn != null)
            {
                tapBtn.onClick.RemoveAllListeners();
                tapBtn.onClick.AddListener(AdvanceToNextScreen);
            }

            // Start timed progression
            if (UIManager.Instance != null)
            {
                _splashRoutine = UIManager.Instance.StartCoroutine(SplashSequence());
            }
            else
            {
                _splashRoutine = CoroutineRunner.Run(SplashSequence());
            }
        }

        private IEnumerator SplashSequence()
        {
            float elapsed = 0f;
            float duration = 2.0f;

            var fillRT = UIHelper.FindRect(_root, "ProgressPillFill") ?? UIHelper.FindRect(_root, "progress-fill");

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (fillRT != null)
                {
                    fillRT.anchorMax = new Vector2(Mathf.Max(0.02f, t), fillRT.anchorMax.y);
                }

                yield return null;
            }

            AdvanceToNextScreen();
        }

        public void AdvanceToNextScreen()
        {
            if (_hasAdvanced) return;
            _hasAdvanced = true;

            StopSplashRoutine();

            Debug.Log("[SplashScreenController] Transitioning to LanguageSelection screen...");
            UIManager.Instance?.ShowScreen(ScreenId.LanguageSelection);
        }

        private void StopSplashRoutine()
        {
            if (_splashRoutine != null)
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.StopCoroutine(_splashRoutine);
                }
                else
                {
                    CoroutineRunner.Stop(_splashRoutine);
                }
                _splashRoutine = null;
            }
        }

        public void OnHide()
        {
            StopSplashRoutine();
            _root = null;
        }
    }
}
