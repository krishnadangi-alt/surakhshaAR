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

            // Card background
            var rowImg = rowBtn.GetComponent<Image>();
            if (rowImg != null)
                rowImg.color = isSelected ? UIColors.Primary : Color.white;

            // Badge box background (direct child or under Inner/)
            var badgeTr = rowBtn.transform.Find("BadgeBox") ?? rowBtn.transform.Find("Inner/BadgeBox");
            var badgeImg = badgeTr?.GetComponent<Image>();
            if (badgeImg != null)
                badgeImg.color = isSelected
                    ? new Color(1f, 1f, 1f, 0.16f)
                    : UIColors.Hex("#EBF2F8");

            // Badge text colour
            var badgeText = badgeTr?.Find("BadgeText")?.GetComponent<TextMeshProUGUI>();
            if (badgeText != null)
                badgeText.color = isSelected ? Color.white : UIColors.PrimaryDark;

            // Title & Subtitle colour
            var textCol = rowBtn.transform.Find("TextCol") ?? rowBtn.transform.Find("Inner/TextCol");
            var title = textCol?.Find("Title")?.GetComponent<TextMeshProUGUI>();
            if (title != null)
                title.color = isSelected ? Color.white : UIColors.TextPrimary;

            var sub = textCol?.Find("Sub")?.GetComponent<TextMeshProUGUI>();
            if (sub != null)
                sub.color = isSelected ? UIColors.TextOnNavyDim : UIColors.TextSecondary;

            // Radio / checkmark circle
            var radioTr = rowBtn.transform.Find(checkName) ?? rowBtn.transform.Find($"Inner/{checkName}");
            var radioImg = radioTr?.GetComponent<Image>();
            if (radioImg != null)
            {
                radioImg.sprite = UIHelper.GetCircleOutlineSprite();
                radioImg.color = isSelected ? Color.white : UIColors.Hex("#CBD5E1");
            }

            var innerDot = radioTr?.Find("InnerDot");
            if (innerDot != null)
            {
                innerDot.gameObject.SetActive(isSelected);
            }

            var checkTmp = radioTr?.Find($"CheckText_{checkName}")?.GetComponent<TextMeshProUGUI>();
            if (checkTmp != null)
                checkTmp.text = "";
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
