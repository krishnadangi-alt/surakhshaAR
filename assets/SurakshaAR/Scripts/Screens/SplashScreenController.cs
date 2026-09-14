using System.Collections;
using SurakshaAR.Core;
using SurakshaAR.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Screens
{
    public class SplashScreenController : IScreenController
    {
        private GameObject _root;
        private Coroutine _timerCoroutine;

        public void OnShow(GameObject root, object param)
        {
            _root = root;

            var appName = UIHelper.FindTMP(root, "label-app-name");
            var tagline = UIHelper.FindTMP(root, "label-tagline");
            var progressFill = UIHelper.FindRect(root, "progress-fill");

            var loc = AppManager.Instance?.Localization;
            if (loc != null)
            {
                if (appName != null) appName.text = loc.Get("splash.appName");
                if (tagline != null) tagline.text = loc.Get("splash.tagline");
            }

            float duration = AppManager.Instance != null ? AppManager.Instance.splashDurationSeconds : 2.5f;

            if (UIManager.Instance != null)
            {
                _timerCoroutine = UIManager.Instance.StartCoroutine(RunSplash(progressFill, duration));
            }
        }

        private IEnumerator RunSplash(RectTransform fill, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (fill != null)
                {
                    float pct = Mathf.Clamp01(elapsed / duration);
                    fill.anchorMax = new Vector2(pct, 1f);
                }
                yield return null;
            }

            UIManager.Instance?.ShowScreen(ScreenId.LanguageSelection);
        }

        public void OnHide()
        {
            if (_timerCoroutine != null && UIManager.Instance != null)
            {
                UIManager.Instance.StopCoroutine(_timerCoroutine);
                _timerCoroutine = null;
            }
            _root = null;
        }
    }
}
