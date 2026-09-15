using SurakshaAR.Data;
using SurakshaAR.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Core
{
    /// <summary>
    /// Component attached to UI text elements to enforce semantic typography
    /// and live multi-language switching without scene reload or text truncation.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SemanticText : MonoBehaviour
    {
        [SerializeField] private SemanticStyle _style = SemanticStyle.Body;
        [SerializeField] private string _localizationKey;

        private TextMeshProUGUI _tmp;

        public SemanticStyle Style
        {
            get => _style;
            set
            {
                _style = value;
                Apply();
            }
        }

        public string LocalizationKey
        {
            get => _localizationKey;
            set
            {
                _localizationKey = value;
                Apply();
            }
        }

        private void Awake()
        {
            _tmp = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            Apply();

            if (AppManager.Instance != null && AppManager.Instance.Localization != null)
            {
                AppManager.Instance.Localization.OnLanguageChanged += HandleLanguageChanged;
            }
        }

        private void OnDestroy()
        {
            if (AppManager.Instance != null && AppManager.Instance.Localization != null)
            {
                AppManager.Instance.Localization.OnLanguageChanged -= HandleLanguageChanged;
            }
        }

        public void SetKeyAndStyle(string key, SemanticStyle style)
        {
            _localizationKey = key;
            _style = style;
            Apply();
        }

        public void Apply()
        {
            if (_tmp == null) _tmp = GetComponent<TextMeshProUGUI>();
            if (_tmp == null) return;

            var lang = AppLanguage.English;
            if (AppManager.Instance != null && AppManager.Instance.Localization != null)
            {
                lang = AppManager.Instance.Localization.CurrentLanguage;

                if (!string.IsNullOrEmpty(_localizationKey))
                {
                    _tmp.text = AppManager.Instance.Localization.Get(_localizationKey);
                }
            }

            TypographyManager.ApplyStyle(_tmp, _style, lang);

            // Notify parent layout group to recalculate bounds cleanly
            if (transform.parent is RectTransform parentRect)
            {
                LayoutRebuilder.MarkLayoutForRebuild(parentRect);
            }
        }

        private void HandleLanguageChanged(AppLanguage newLang)
        {
            Apply();
        }
    }
}
