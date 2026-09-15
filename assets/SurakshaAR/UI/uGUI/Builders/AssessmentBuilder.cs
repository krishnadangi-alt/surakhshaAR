using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Assessment / AR Step Builder — matches reference screenshot 5:
    /// Top bar with back button, step title, step count ("Step 1 / 4"), progress bar.
    /// Frosted glass instruction card with title, Hint button, and description.
    /// Quiz options list (option-1 to option-4) with radio indicator.
    /// Big primary action / submit button.
    /// </summary>
    public static class AssessmentBuilder
    {
        public static GameObject Build()
        {
            var root = new GameObject("AssessmentScreen");
            root.AddComponent<RectTransform>();
            var rootImg = root.AddComponent<Image>();
            rootImg.color = Hex("#0A1926"); // Dark camera/industrial simulation backdrop
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // ── Scrollable Body ──────────────────────────────────────────
            var scrollRoot = UIHelper.MakeRect("ScrollArea", root.transform);
            scrollRoot.anchorMin = Vector2.zero;
            scrollRoot.anchorMax = Vector2.one;
            scrollRoot.offsetMin = Vector2.zero;
            scrollRoot.offsetMax = Vector2.zero;

            var scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 25f;

            var viewport = UIHelper.MakeRect("Viewport", scrollRoot);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewport.gameObject.AddComponent<RectMask2D>();
            scrollRect.viewport = viewport;

            var content = UIHelper.MakeRect("Content", viewport);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.sizeDelta = new Vector2(0, 1200);
            scrollRect.content = content;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.padding = new RectOffset(28, 28, 36, 40);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── 1. Top Bar with Back + Step Progress ──────────────────────
            BuildTopBar(content);

            // ── 2. Frosted Instruction Card ───────────────────────────────
            BuildInstructionCard(content);

            // ── 3. Quiz Options (4 Options) ───────────────────────────────
            BuildQuizOptions(content);

            // ── 4. Submit / Action Button ─────────────────────────────────
            BuildSubmitButton(content);

            return root;
        }

        private static void BuildTopBar(Transform parent)
        {
            var barRow = UIHelper.MakeHorizontal("TopBarRow", parent, 16);
            UIHelper.SetLayout(barRow.gameObject, preferredHeight: 64);

            // Back button
            var backBtn = UIHelper.MakeButton("btn-back", barRow, "‹", 36, new Color(1, 1, 1, 0.15f), Color.white, 20);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 56, preferredHeight: 56);

            // Progress Banner
            var banner = new GameObject("StepBanner");
            banner.transform.SetParent(barRow, false);
            UIHelper.SetLayout(banner, flexibleWidth: true, flexWidth: 1, preferredHeight: 56);

            var bannerImg = banner.AddComponent<Image>();
            bannerImg.color = new Color(0, 0, 0, 0.45f);
            UIHelper.SetImageRoundedSprite(bannerImg, 18);

            var bannerCol = UIHelper.MakeVertical("Col", banner.transform, 6, new RectOffset(18, 18, 10, 10));
            UIHelper.Stretch(bannerCol, 0, 0, 0, 0);

            var textRow = UIHelper.MakeHorizontal("TextRow", bannerCol, 8);
            UIHelper.SetLayout(textRow.gameObject, preferredHeight: 24);

            var titleLbl = UIHelper.MakeLabel("label-title", textRow, "Fire Response", 18, Color.white, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1);

            var stepLbl = UIHelper.MakeLabel("StepLbl", textRow, "Step 1 / 4", 17, UIColors.TextOnNavyDim, TextAlignmentOptions.Right);
            UIHelper.SetLayout(stepLbl.gameObject, preferredWidth: 100);

            // Progress bar
            var barBg = UIHelper.MakeRect("ProgressBar", bannerCol);
            UIHelper.SetLayout(barBg.gameObject, preferredHeight: 5);
            var barImg = barBg.gameObject.AddComponent<Image>();
            barImg.color = new Color(1, 1, 1, 0.2f);
            UIHelper.SetImageRoundedSprite(barImg, 3);

            var fill = UIHelper.MakeRect("Fill", barBg);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0.25f, 1);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            var fillImg = fill.gameObject.AddComponent<Image>();
            fillImg.color = UIColors.Primary;
            UIHelper.SetImageRoundedSprite(fillImg, 3);
        }

        private static void BuildInstructionCard(Transform parent)
        {
            var card = new GameObject("InstructionCard");
            card.transform.SetParent(parent, false);

            var cardImg = card.AddComponent<Image>();
            cardImg.color = new Color(0.08f, 0.12f, 0.18f, 0.85f);
            UIHelper.SetImageRoundedSprite(cardImg, 22);

            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(24, 24, 20, 20);
            vlg.spacing = 10;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var titleRow = UIHelper.MakeHorizontal("TitleRow", card.transform, 12);
            UIHelper.SetLayout(titleRow.gameObject, minHeight: 36);

            var title = UIHelper.MakeLabel("CardTitle", titleRow, "Which extinguisher is correct for an electrical panel fire?", 20, Color.white, bold: true, wrap: true);
            UIHelper.SetLayout(title.gameObject, flexibleWidth: true, flexWidth: 1);

            var hintBtn = UIHelper.MakeButton("btn-hint", titleRow, "Hint", 16, new Color(0.9f, 0.7f, 0.1f, 0.25f), Hex("#FBBF24"), 16);
            UIHelper.SetLayout(hintBtn.gameObject, preferredWidth: 84, preferredHeight: 34);

            var desc = UIHelper.MakeLabel("CardDesc", card.transform,
                "A 415V live electrical panel fire is a CLASS C hazard. Select the ONLY agent that is electrically non-conductive and safe. Water and foam conduct electricity — DO NOT use them on live equipment.",
                16, new Color(1, 1, 1, 0.78f), wrap: true);
            UIHelper.SetLayout(desc.gameObject, minHeight: 60, flexibleWidth: true, flexWidth: 1);

            UIHelper.SetLayout(card, minHeight: 180);
            var csf = card.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private static void BuildQuizOptions(Transform parent)
        {
            var optCol = UIHelper.MakeVertical("QuizOptions", parent, 14);
            UIHelper.SetLayout(optCol.gameObject, minHeight: 320);
            var csf = optCol.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            string[] defaultOptions = {
                "CO<sub>2</sub> Extinguisher (Class C/Electrical)",
                "Water Hose Reel (Class A only)",
                "Dry Powder Extinguisher (Multipurpose)",
                "Foam Extinguisher (Flammable Liquids)"
            };

            for (int i = 0; i < 4; i++)
            {
                BuildOptionItem(optCol, $"option-{i + 1}", defaultOptions[i], i == 0);
            }
        }

        private static void BuildOptionItem(Transform parent, string name, string text, bool isSelected)
        {
            var btn = UIHelper.MakeButton(name, parent, "", 18,
                isSelected ? Hex("#1B3855") : Hex("#102233"), Color.white, 16);
            UIHelper.SetLayout(btn.gameObject, preferredHeight: 70, minHeight: 56);

            var inner = UIHelper.MakeHorizontal("Inner", btn.transform, 16, new RectOffset(20, 20, 14, 14));
            UIHelper.Stretch(inner, 0, 0, 0, 0);

            // Radio circle
            var radio = UIHelper.MakeRect("quiz-option-radio", inner);
            UIHelper.SetLayout(radio.gameObject, preferredWidth: 32, preferredHeight: 32);
            var radioImg = radio.gameObject.AddComponent<Image>();
            radioImg.sprite = UIHelper.GetCircleSprite();
            radioImg.color = isSelected ? UIColors.Primary : new Color(1, 1, 1, 0.25f);

            var checkGO = new GameObject("Check");
            checkGO.transform.SetParent(radio, false);
            var checkImg = checkGO.AddComponent<Image>();
            checkImg.sprite = UIHelper.GetCheckmarkSprite();
            checkImg.color = isSelected ? Color.white : Color.clear;
            UIHelper.AnchorCenter(checkGO.GetComponent<RectTransform>(), 20, 20);

            // Option text
            var optLbl = UIHelper.MakeLabel("Text", inner, text, 18, Color.white);
            UIHelper.SetLayout(optLbl.gameObject, flexibleWidth: true, flexWidth: 1);
        }

        private static void BuildSubmitButton(Transform parent)
        {
            var submitBtn = UIHelper.MakeButton("btn-submit-assessment", parent, "Submit Assessment Response",
                22, UIColors.SafetyGreen, Color.white, 20);
            UIHelper.SetLayout(submitBtn.gameObject, preferredHeight: 68, minHeight: 56);
        }

        private static Color Hex(string hex) => UIColors.Hex(hex);
    }
}
