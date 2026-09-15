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

        public void OnShow(GameObject root, object param)
        {
            var loc = AppManager.Instance?.Localization;

            // Read previously-selected language (or default to English)
            _selected = AppState.Instance != null
                ? AppState.Instance.CurrentLanguage
                : (loc != null ? loc.CurrentLanguage : AppLanguage.English);

            // Header labels
            _titleTMP    = UIHelper.FindTMP(root, "label-title");
            _subtitleTMP = UIHelper.FindTMP(root, "label-subtitle");
            UpdateHeaderText(loc);

            // Skip
            _btnSkip = UIHelper.FindButton(root, "btn-skip");
            if (_btnSkip != null) _btnSkip.onClick.AddListener(OnContinue);

            // Language cards
            _btnEnglish = UIHelper.FindButton(root, "row-english");
            _btnHindi   = UIHelper.FindButton(root, "row-hindi");
            _btnSantali = UIHelper.FindButton(root, "row-santali");

            if (_btnEnglish != null) _btnEnglish.onClick.AddListener(() => SelectLang(AppLanguage.English));
            if (_btnHindi   != null) _btnHindi.onClick.AddListener(()   => SelectLang(AppLanguage.Hindi));
            if (_btnSantali != null) _btnSantali.onClick.AddListener(() => SelectLang(AppLanguage.Santali));

            // Continue button
            _btnContinue = UIHelper.FindButton(root, "btn-continue");
            if (_btnContinue != null)
            {
                UpdateContinueLabel(loc);
                _btnContinue.onClick.AddListener(OnContinue);
            }

            // Reflect initial selection
            RefreshVisuals();
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
            UpdateHeaderText(loc);
            UpdateContinueLabel(loc);
            RefreshVisuals();
        }

        private void UpdateHeaderText(LocalizationManager loc)
        {
            if (loc == null) return;
            if (_titleTMP    != null) _titleTMP.text    = loc.Get("language.title");
            if (_subtitleTMP != null) _subtitleTMP.text = loc.Get("language.subtitle");
            if (_btnSkip     != null)
            {
                var skipTMP = _btnSkip.GetComponentInChildren<TextMeshProUGUI>();
                if (skipTMP != null) skipTMP.text = loc.Get("language.skip");
            }
        }

        private void UpdateContinueLabel(LocalizationManager loc)
        {
            if (_btnContinue == null || loc == null) return;
            var lbl = _btnContinue.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl != null) lbl.text = $"{loc.Get("language.continue")} →";
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
                rowImg.color = Color.white;
                UIHelper.SetImageRoundedSprite(rowImg, 26f);
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
                sub.color = UIColors.Hex("#64748B");

            // Right chevron indicator (vector icon or TMP arrow)
            var chevronTr = rowBtn.transform.Find(checkName) ?? rowBtn.transform.Find($"Inner/{checkName}");
            if (chevronTr != null)
            {
                var chevImg = chevronTr.GetComponentInChildren<Image>();
                if (chevImg != null)
                    chevImg.color = isSelected ? UIColors.Hex("#2563EB") : UIColors.Hex("#94A3B8");

                var chevronTMP = chevronTr.GetComponentInChildren<TextMeshProUGUI>();
                if (chevronTMP != null)
                    chevronTMP.color = isSelected ? UIColors.Hex("#2563EB") : UIColors.Hex("#94A3B8");
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
