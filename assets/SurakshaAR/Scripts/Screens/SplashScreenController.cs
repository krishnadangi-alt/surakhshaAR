using System.Collections;
using SurakshaAR.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    /// <summary>
    /// SplashScreenController
    /// ======================
    /// Drives the Splash screen loading animation, branding presentation,
    /// and transition into LanguageSelection or HomeDashboard.
    /// </summary>
    public class SplashScreenController : IScreenController
    {
        private GameObject _root;
        private Coroutine _splashRoutine;

        public void OnShow(GameObject root, object param)
        {
            _root = root;
            if (_root == null) return;

            _splashRoutine = CoroutineRunner.Run(SplashSequence());
        }

        private IEnumerator SplashSequence()
        {
            float elapsed = 0f;
            float duration = 2.5f;

            var fillRT = UIHelper.FindRect(_root, "ProgressPillFill");

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (fillRT != null)
                {
                    fillRT.anchorMax = new Vector2(t, 1f);
                }

                yield return null;
            }

            // Transition to LanguageSelection or Login
            bool hasLanguageSet = PlayerPrefs.HasKey("suraksha_selected_language");
            if (!hasLanguageSet)
            {
                UIManager.Instance?.ShowScreen(ScreenId.LanguageSelection);
            }
            else
            {
                UIManager.Instance?.ShowScreen(ScreenId.Login);
            }
        }

        public void OnHide()
        {
            if (_splashRoutine != null)
            {
                CoroutineRunner.Stop(_splashRoutine);
                _splashRoutine = null;
            }
            _root = null;
        }
    }
}
