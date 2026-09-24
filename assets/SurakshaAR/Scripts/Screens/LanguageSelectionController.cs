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
    /// Controller for Language Selection screen.
    /// Handles card selection state, localizes header text, and navigates on Continue/Skip.
    /// </summary>
    public class LanguageSelectionController : IScreenController
    {
        private Button _btnEnglish, _btnHindi, _btnSantali;
        private Button _btnContinue, _btnSkip;
        private AppLanguage _selected;

        private TextMeshProUGUI _titleTMP;
        private TextMeshProUGUI _subtitleTMP;
        private GameObject _root;

        public void OnShow(GameObject root, object param)
        {
            _root = root;
            var loc = AppManager.Instance?.Localization;

            // Read previously-selected language (or default to English)
            _selected = AppState.Instance != null
                ? AppState.Instance.CurrentLanguage
                : (loc != null ? loc.CurrentLanguage : AppLanguage.English);

            // Header labels
            _titleTMP    = UIHelper.FindTMP(root, "label-title");
            _subtitleTMP = UIHelper.FindTMP(root, "label-subtitle");
            UpdateHeaderText(loc, _selected);

            // Skip
            _btnSkip = UIHelper.FindButton(root, "btn-skip");
            if (_btnSkip != null)
            {
                _btnSkip.onClick.RemoveAllListeners();
                _btnSkip.onClick.AddListener(OnContinue);
            }

            // Language cards
            _btnEnglish = UIHelper.FindButton(root, "row-english");
            _btnHindi   = UIHelper.FindButton(root, "row-hindi");
            _btnSantali = UIHelper.FindButton(root, "row-santali");

            if (_btnEnglish != null)
            {
                _btnEnglish.onClick.RemoveAllListeners();
                _btnEnglish.onClick.AddListener(() => SelectLang(AppLanguage.English));
            }
            if (_btnHindi != null)
            {
                _btnHindi.onClick.RemoveAllListeners();
                _btnHindi.onClick.AddListener(() => SelectLang(AppLanguage.Hindi));
            }
            if (_btnSantali != null)
            {
                _btnSantali.onClick.RemoveAllListeners();
                _btnSantali.onClick.AddListener(() => SelectLang(AppLanguage.Santali));
            }

            // Continue button
            _btnContinue = UIHelper.FindButton(root, "btn-continue");
            if (_btnContinue != null)
            {
                _btnContinue.onClick.RemoveAllListeners();
                UpdateContinueLabel(loc, _selected);
                _btnContinue.onClick.AddListener(OnContinue);
            }

            // Reflect initial selection
            RefreshVisuals();

            // Apply active language font to header/skip/continue
            ApplyDynamicFonts(_selected);
        }

        // ─────────────────────────────────────────────────────────────────
        //  SELECTION
        // ─────────────────────────────────────────────────────────────────
        private void SelectLang(AppLanguage lang)
        {
            _selected = lang;

            // Immediately apply language so the continue label updates to the chosen language
            if (AppState.Instance != null)
                AppState.Instance.SetLanguage(lang);
            else
                AppManager.Instance?.Localization?.SetLanguage(lang);

            var loc = AppManager.Instance?.Localization;
            UpdateHeaderText(loc, lang);
            UpdateContinueLabel(loc, lang);
            ApplyDynamicFonts(lang);
            RefreshVisuals();
        }

        private void UpdateHeaderText(LocalizationManager loc, AppLanguage lang)
        {
            if (loc == null) return;
            if (_titleTMP != null)
                _titleTMP.text = loc.Get("language.title");
            if (_subtitleTMP != null)
                _subtitleTMP.text = loc.Get("language.subtitle");
            if (_btnSkip != null)
            {
                var skipTMP = _btnSkip.GetComponentInChildren<TextMeshProUGUI>();
                if (skipTMP != null)
                    skipTMP.text = loc.Get("language.skip");
            }
        }

        private void UpdateContinueLabel(LocalizationManager loc, AppLanguage lang)
        {
            if (_btnContinue == null || loc == null) return;
            var lbl = _btnContinue.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl != null)
            {
                string cont = loc.Get("language.continue");
                lbl.text = $"{cont}  >";
            }
        }

        private void ApplyDynamicFonts(AppLanguage lang)
        {
            TMP_FontAsset font = null;
            try { font = UIHelper.GetFontForLanguage(lang); } catch {}
            if (font == null) return;

            try { if (_titleTMP != null) _titleTMP.font = font; } catch {}
            try { if (_subtitleTMP != null) _subtitleTMP.font = font; } catch {}

            try
            {
                if (_btnSkip != null)
                {
                    var skipTMP = _btnSkip.GetComponentInChildren<TextMeshProUGUI>();
                    if (skipTMP != null) skipTMP.font = font;
                }
            } catch {}

            try
            {
                if (_btnContinue != null)
                {
                    var lbl = _btnContinue.GetComponentInChildren<TextMeshProUGUI>();
                    if (lbl != null) lbl.font = font;
                }
            } catch {}

            try
            {
                if (_root != null)
                    UIManager.Instance?.ApplyLanguageFonts(_root, lang);
            } catch {}
        }

        // ─────────────────────────────────────────────────────────────────
        //  VISUAL STATE
        // ─────────────────────────────────────────────────────────────────
        private void RefreshVisuals()
        {
            UpdateCard(_btnEnglish, _selected == AppLanguage.English, "check-english");
            UpdateCard(_btnHindi,   _selected == AppLanguage.Hindi,   "check-hindi");
            UpdateCard(_btnSantali, _selected == AppLanguage.Santali, "check-santali");
        }

        private void UpdateCard(Button rowBtn, bool isSelected, string checkName)
        {
            if (rowBtn == null) return;

            // Card background & outline
            var rowImg = rowBtn.GetComponent<Image>();
            if (rowImg != null)
            {
                rowImg.color = isSelected ? UIColors.Hex("#F8FAFC") : Color.white;
                UIHelper.SetImageRoundedSprite(rowImg, 28f);
            }

            var outline = rowBtn.GetComponent<Outline>() ?? rowBtn.gameObject.AddComponent<Outline>();
            outline.effectColor = isSelected ? UIColors.Hex("#2563EB") : UIColors.Hex("#E2E8F0");
            outline.effectDistance = isSelected ? new Vector2(3.5f, -3.5f) : new Vector2(1.5f, -1.5f);

            // Title & Subtitle colour
            var textCol = rowBtn.transform.Find("TextCol") ?? rowBtn.transform.Find("Inner/TextCol");
            var title = textCol?.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (title != null)
                title.color = UIColors.Hex("#0F172A");

            var sub = textCol?.Find("Sub")?.GetComponent<TextMeshProUGUI>();
            if (sub != null)
                sub.color = isSelected ? UIColors.Hex("#334155") : UIColors.Hex("#64748B");

            // Right circular radio indicator matching Screen 2
            var radioTr = rowBtn.transform.Find(checkName) 
                       ?? rowBtn.transform.Find($"Inner/{checkName}") 
                       ?? rowBtn.transform.Find("RadioBox");
            if (radioTr != null)
            {
                var radioImg = radioTr.GetComponent<Image>();
                var radioOutline = radioTr.GetComponent<Outline>();
                if (radioImg != null)
                {
                    radioImg.color = isSelected ? UIColors.Hex("#2563EB") : Color.white;
                }
                if (radioOutline != null)
                {
                    radioOutline.effectColor = isSelected ? UIColors.Hex("#2563EB") : UIColors.Hex("#CBD5E1");
                }
                var checkIcon = radioTr.Find("CheckMarkIcon");
                if (checkIcon != null)
                {
                    checkIcon.gameObject.SetActive(isSelected);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        //  NAVIGATION
        // ─────────────────────────────────────────────────────────────────
        private void OnContinue()
        {
            // Persist selection (may have already been set in SelectLang)
            if (AppState.Instance != null)
                AppState.Instance.SetLanguage(_selected);
            else
                AppManager.Instance?.Localization?.SetLanguage(_selected);

            UIManager.Instance?.ShowScreen(ScreenId.Login);
        }

        public void OnHide()
        {
            if (_btnSkip     != null) _btnSkip.onClick.RemoveListener(OnContinue);
            if (_btnContinue != null) _btnContinue.onClick.RemoveListener(OnContinue);
        }
    }
}
