using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Assessment / Quiz Screen Builder:
    /// Refreshed with pure white background, crisp high-contrast cards,
    /// dynamic step indicator ("Question 1 of 3"), and prominent PASS/DGMS questions.
    /// </summary>
    public static class AssessmentBuilder
    {
        public static GameObject Build()
        {
            var root = new GameObject("AssessmentScreen");
            root.AddComponent<RectTransform>();
            var rootImg = root.AddComponent<Image>();
            rootImg.color = Color.white; // Pure white background as requested
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
            UIHelper.SetLayout(barRow.gameObject, preferredHeight: 110, minHeight: 96);

            // Back button
            var backBtn = UIHelper.MakeButton("btn-back", barRow, "‹", 52, Color.white, UIColors.PrimaryDark, 20);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 76, minWidth: 76, preferredHeight: 76, minHeight: 76);
            var backBorder = backBtn.gameObject.AddComponent<Outline>();
            backBorder.effectColor = UIColors.Hex("#CBD5E1");
            backBorder.effectDistance = new Vector2(1, -1);

            // Progress Banner
            var banner = new GameObject("StepBanner");
            banner.transform.SetParent(barRow, false);
            UIHelper.SetLayout(banner, flexibleWidth: true, flexWidth: 1, preferredHeight: 104, minHeight: 92);

            var bannerImg = banner.AddComponent<Image>();
            bannerImg.color = UIColors.Hex("#F8FAFC");
            UIHelper.SetImageRoundedSprite(bannerImg, 18);
            var bannerOutline = banner.AddComponent<Outline>();
            bannerOutline.effectColor = UIColors.Hex("#E2E8F0");
            bannerOutline.effectDistance = new Vector2(1, -1);

            var bannerCol = UIHelper.MakeVertical("Col", banner.transform, 6, new RectOffset(20, 20, 12, 12));
            UIHelper.Stretch(bannerCol, 0, 0, 0, 0);

            var textRow = UIHelper.MakeHorizontal("TextRow", bannerCol, 12);
            UIHelper.SetLayout(textRow.gameObject, preferredHeight: 52);

            var titleLbl = UIHelper.MakeLabel("label-title", textRow, "Fire Safety Assessment", 36, UIColors.PrimaryDark, bold: true, wrap: false);
            titleLbl.overflowMode = TextOverflowModes.Ellipsis;
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 48);

            var stepLbl = UIHelper.MakeLabel("StepLbl", textRow, "Question 1 of 3", 30, UIColors.Hex("#64748B"), TextAlignmentOptions.Right, bold: true, wrap: false);
            UIHelper.SetLayout(stepLbl.gameObject, preferredWidth: 260, minWidth: 200, preferredHeight: 48);

            // Progress bar
            var barBg = UIHelper.MakeRect("ProgressBar", bannerCol);
            UIHelper.SetLayout(barBg.gameObject, preferredHeight: 10);
            var barImg = barBg.gameObject.AddComponent<Image>();
            barImg.color = UIColors.Hex("#E2E8F0");
            UIHelper.SetImageRoundedSprite(barImg, 5);

            var fill = UIHelper.MakeRect("Fill", barBg);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0.333f, 1f); // Question 1 of 3
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            var fillImg = fill.gameObject.AddComponent<Image>();
            fillImg.color = UIColors.SafetyGreen;
            UIHelper.SetImageRoundedSprite(fillImg, 5);
        }

        private static void BuildInstructionCard(Transform parent)
        {
            var card = new GameObject("InstructionCard");
            card.transform.SetParent(parent, false);

            var cardImg = card.AddComponent<Image>();
            cardImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 24);

            var cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = UIColors.Hex("#E2E8F0");
            cardOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(28, 28, 24, 24);
            vlg.spacing = 14;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var titleRow = UIHelper.MakeHorizontal("TitleRow", card.transform, 12);
            UIHelper.SetLayout(titleRow.gameObject, minHeight: 52);

            var title = UIHelper.MakeLabel("CardTitle", titleRow, "Which extinguisher is correct for an electrical panel fire?", 40, UIColors.PrimaryDark, bold: true, wrap: true);
            title.lineSpacing = 2f;
            UIHelper.SetLayout(title.gameObject, flexibleWidth: true, flexWidth: 1);

            var hintBtn = UIHelper.MakeButton("btn-hint", titleRow, "Hint", 30, UIColors.Hex("#FEF3C7"), UIColors.Hex("#B45309"), 14);
            UIHelper.SetLayout(hintBtn.gameObject, preferredWidth: 120, preferredHeight: 60);
            var hintOutline = hintBtn.gameObject.AddComponent<Outline>();
            hintOutline.effectColor = UIColors.Hex("#FDE68A");
            hintOutline.effectDistance = new Vector2(1, -1);

            // Hint box container
            var hintBox = UIHelper.MakeVertical("HintBox", card.transform, 8, new RectOffset(18, 18, 14, 14));
            var hintBoxImg = hintBox.gameObject.AddComponent<Image>();
            hintBoxImg.color = UIColors.Hex("#FFFBEB");
            UIHelper.SetImageRoundedSprite(hintBoxImg, 16);
            var hintBoxOutline = hintBox.gameObject.AddComponent<Outline>();
            hintBoxOutline.effectColor = UIColors.Hex("#FDE68A");
            hintBoxOutline.effectDistance = new Vector2(1, -1);

            var desc = UIHelper.MakeLabel("CardDesc", hintBox.transform,
                "A 415V live electrical panel fire is a CLASS C hazard. Select the ONLY agent that is electrically non-conductive and safe. Water and foam conduct electricity — DO NOT use them on live equipment.",
                32, UIColors.Hex("#92400E"), wrap: true);
            desc.lineSpacing = 3f;
            UIHelper.SetLayout(desc.gameObject, flexibleWidth: true, flexWidth: 1);

            UIHelper.SetLayout(card, minHeight: 240);
            var csf = card.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private static void BuildQuizOptions(Transform parent)
        {
            var optCol = UIHelper.MakeVertical("QuizOptions", parent, 16);
            UIHelper.SetLayout(optCol.gameObject, minHeight: 380);
            var csf = optCol.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            string[] defaultOptions = {
                "CO₂ Extinguisher (Class C/Electrical)",
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
                isSelected ? UIColors.Hex("#EFF6FF") : Color.white, UIColors.PrimaryDark, 18);
            UIHelper.SetLayout(btn.gameObject, preferredHeight: 116, minHeight: 104);
            var csf = btn.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var border = btn.gameObject.AddComponent<Outline>();
            border.effectColor = isSelected ? UIColors.Hex("#3B82F6") : UIColors.Hex("#E2E8F0");
            border.effectDistance = isSelected ? new Vector2(2, -2) : new Vector2(1, -1);

            var inner = UIHelper.MakeHorizontal("Inner", btn.transform, 18, new RectOffset(22, 22, 16, 16));
            UIHelper.Stretch(inner, 0, 0, 0, 0);

            // Radio circle
            var radio = UIHelper.MakeRect("quiz-option-radio", inner);
            UIHelper.SetLayout(radio.gameObject, preferredWidth: 48, minWidth: 48, preferredHeight: 48, minHeight: 48);
            var radioImg = radio.gameObject.AddComponent<Image>();
            radioImg.sprite = UIHelper.GetCircleSprite();
            radioImg.color = isSelected ? UIColors.Hex("#3B82F6") : UIColors.Hex("#E2E8F0");

            var checkGO = new GameObject("Check");
            checkGO.transform.SetParent(radio, false);
            var checkImg = checkGO.AddComponent<Image>();
            checkImg.sprite = UIHelper.GetCheckmarkSprite();
            checkImg.color = isSelected ? Color.white : Color.clear;
            UIHelper.AnchorCenter(checkGO.GetComponent<RectTransform>(), 28, 28);

            // Option text
            var optLbl = UIHelper.MakeLabel("Text", inner, text, 34, isSelected ? UIColors.Hex("#1E3A8A") : UIColors.Hex("#1E293B"), wrap: true);
            optLbl.lineSpacing = 2f;
            optLbl.textWrappingMode = TextWrappingModes.Normal;
            UIHelper.SetLayout(optLbl.gameObject, flexibleWidth: true, flexWidth: 1, minHeight: 60);
        }

        private static void BuildSubmitButton(Transform parent)
        {
            var submitBtn = UIHelper.MakeButton("btn-submit-assessment", parent, "Next Question ➔",
                38, UIColors.SafetyGreen, Color.white, 22);
            UIHelper.SetLayout(submitBtn.gameObject, preferredHeight: 114, minHeight: 100);
        }
    }
}
