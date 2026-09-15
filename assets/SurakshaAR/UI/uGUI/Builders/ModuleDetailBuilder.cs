using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Module Detail builder — pixel-perfect match to Figma reference (media_1789202041374.png):
    /// 1080 × 2400 reference scaling:
    /// - Hero Header (720px) with welder photo, gradient overlay, circular back button,
    ///   title "Fire & Explosion Response", subtitle, and 3 stat chips (45 min, Intermediate, 12 Steps).
    /// - "What you'll learn" card with 5 checklist items, green checkmark circles (52x52), wrapped 30px text.
    /// - Warm amber "SAFETY NOTE" card with warning badge and advisory text.
    /// - Bottom "▶ START MODULE" button (128px high) in #1B5E3C safety green.
    /// DIRECT children of Content — no intermediate 0-height wrappers or overlapping elements.
    /// </summary>
    public static class ModuleDetailBuilder
    {
        public static GameObject Build()
        {
            var root = new GameObject("ModuleDetailScreen");
            root.AddComponent<RectTransform>();
            var rootImg = root.AddComponent<Image>();
            rootImg.color = UIColors.Hex("#F8FAFC");
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
            scrollRect.scrollSensitivity = 30f;

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
            content.sizeDelta = new Vector2(0, 2400);
            scrollRect.content = content;

            // Content uses VerticalLayoutGroup that drives width and respects child preferred heights
            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 32;
            vlg.padding = new RectOffset(48, 48, 0, 80);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── 1. Hero Header ────────────────────────────────────────────
            BuildHeroHeader(content);

            // ── 2. "What you'll learn" Card ───────────────────────────────
            BuildLearnCard(content);

            // ── 3. "SAFETY NOTE" Card ─────────────────────────────────────
            BuildSafetyNote(content);

            // ── 4. "START MODULE" Action Button ───────────────────────────
            BuildStartButton(content);

            return root;
        }

        private static void BuildHeroHeader(Transform parent)
        {
            var heroRT = UIHelper.MakeRect("HeroHeader", parent);
            heroRT.sizeDelta = new Vector2(0, 720);
            var le = heroRT.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 720;
            le.minHeight = 720;

            var heroImg = heroRT.gameObject.AddComponent<Image>();
            var heroSprite = UIHelper.LoadProjectSprite("fire intro");
            if (heroSprite != null)
            {
                heroImg.sprite = heroSprite;
                heroImg.color = Color.white;
            }
            else
            {
                heroImg.color = UIColors.PrimaryDark;
                heroImg.sprite = UIHelper.GetWhiteSprite();
            }

            // Dark gradient overlay for crystal clear text readability
            var overlay = UIHelper.MakeStretchRect("Overlay", heroRT);
            var overImg = overlay.gameObject.AddComponent<Image>();
            overImg.color = new Color(0.04f, 0.09f, 0.14f, 0.65f);

            // Back button (top left circular/rounded button)
            var backBtn = UIHelper.MakeButton("btn-back", heroRT, "‹", 56, new Color(0, 0, 0, 0.50f), Color.white, 24);
            var backRT = backBtn.GetComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0, 1);
            backRT.anchorMax = new Vector2(0, 1);
            backRT.pivot = new Vector2(0, 1);
            backRT.sizeDelta = new Vector2(88, 88);
            backRT.anchoredPosition = new Vector2(40, -50);

            // Hero Bottom Info Block (Anchored to bottom of Hero)
            var infoBox = UIHelper.MakeRect("HeroInfo", heroRT);
            infoBox.anchorMin = new Vector2(0, 0);
            infoBox.anchorMax = new Vector2(1, 0);
            infoBox.pivot = new Vector2(0.5f, 0);
            infoBox.sizeDelta = new Vector2(-80, 320);
            infoBox.anchoredPosition = new Vector2(0, 36);

            var infoVLG = infoBox.gameObject.AddComponent<VerticalLayoutGroup>();
            infoVLG.spacing = 16;
            infoVLG.padding = new RectOffset(0, 0, 0, 0);
            infoVLG.childForceExpandWidth = true;
            infoVLG.childForceExpandHeight = false;
            infoVLG.childControlWidth = true;
            infoVLG.childControlHeight = false;

            // Title Row
            var titleRow = UIHelper.MakeRect("TitleRow", infoBox);
            titleRow.sizeDelta = new Vector2(0, 64);
            UIHelper.SetLayout(titleRow.gameObject, preferredHeight: 64);

            var titleLbl = UIHelper.MakeLabel("label-module-name", titleRow, "Fire & Explosion Response", 52, Color.white, bold: true, wrap: false);
            UIHelper.Stretch(titleLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Subtitle
            var subRow = UIHelper.MakeRect("SubRow", infoBox);
            subRow.sizeDelta = new Vector2(0, 40);
            UIHelper.SetLayout(subRow.gameObject, preferredHeight: 40);

            var subLbl = UIHelper.MakeLabel("label-subtitle", subRow, "Learn • Practice • Assess", 30, Hex("#CBD5E1"), wrap: false);
            UIHelper.Stretch(subLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Chips row (Duration, Difficulty, Steps)
            var chipsRow = UIHelper.MakeRect("ChipsRow", infoBox);
            chipsRow.sizeDelta = new Vector2(0, 64);
            UIHelper.SetLayout(chipsRow.gameObject, preferredHeight: 64);

            var chipsHLG = chipsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            chipsHLG.spacing = 16;
            chipsHLG.childForceExpandWidth = false;
            chipsHLG.childForceExpandHeight = true;
            chipsHLG.childControlWidth = false;
            chipsHLG.childControlHeight = true;

            MakeChip(chipsRow, "chip-duration-value", "45 min", 200);
            MakeChip(chipsRow, "chip-difficulty-value", "Intermediate", 260);
            MakeChip(chipsRow, "chip-scenarios-value", "12 Steps", 210);
        }

        private static void MakeChip(Transform parent, string name, string text, float width)
        {
            var chipGO = UIHelper.MakeRect(name, parent);
            chipGO.sizeDelta = new Vector2(width, 60);
            UIHelper.SetLayout(chipGO.gameObject, preferredWidth: width, preferredHeight: 60);

            var chipImg = chipGO.gameObject.AddComponent<Image>();
            chipImg.color = new Color(1f, 1f, 1f, 0.20f);
            UIHelper.SetImageRoundedSprite(chipImg, 30);

            var lbl = UIHelper.MakeLabel(name + "_lbl", chipGO, text, 26, Color.white, TextAlignmentOptions.Center, bold: true, wrap: false);
            UIHelper.Stretch(lbl.GetComponent<RectTransform>(), 12, 12, 4, 4);
        }

        private static void BuildLearnCard(Transform parent)
        {
            // Direct child of parent Content — no intermediate 0-height margin wrapper!
            var cardGO = new GameObject("CardBox");
            cardGO.transform.SetParent(parent, false);

            var cardImg = cardGO.AddComponent<Image>();
            cardImg.color = Color.white;
            cardImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cardImg, 32);

            var cardOutline = cardGO.AddComponent<Outline>();
            cardOutline.effectColor = Hex("#E2E8F0");
            cardOutline.effectDistance = new Vector2(2, -2);

            // Vertical layout DIRECTLY on CardBox
            var vlg = cardGO.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(40, 40, 40, 44);
            vlg.spacing = 24;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            var csf = cardGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Header: Icon + "What you'll learn"
            var headRow = UIHelper.MakeRect("HeadRow", cardGO.transform);
            headRow.sizeDelta = new Vector2(0, 52);
            UIHelper.SetLayout(headRow.gameObject, preferredHeight: 52);

            var headHLG = headRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            headHLG.spacing = 16;
            headHLG.childAlignment = TextAnchor.MiddleLeft;
            headHLG.childForceExpandWidth = false;
            headHLG.childForceExpandHeight = true;
            headHLG.childControlWidth = false;
            headHLG.childControlHeight = true;

            var headBadge = UIHelper.MakeRect("HeadBadge", headRow);
            headBadge.sizeDelta = new Vector2(40, 40);
            var badgeImg = headBadge.gameObject.AddComponent<Image>();
            badgeImg.color = UIColors.Primary;
            UIHelper.SetImageRoundedSprite(badgeImg, 12);

            var headLbl = UIHelper.MakeLabel("HeadText", headRow, "What you'll learn", 38, UIColors.TextPrimary, bold: true, wrap: false);
            headLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 52);

            // Learn Items (5 items matching Figma)
            MakeLearnItem(cardGO.transform, "Identify fire classes and appropriate extinguisher types");
            MakeLearnItem(cardGO.transform, "Execute emergency evacuation routes correctly");
            MakeLearnItem(cardGO.transform, "Operate fire suppression equipment safely");
            MakeLearnItem(cardGO.transform, "Communicate with emergency response teams");
            MakeLearnItem(cardGO.transform, "Assess explosion risk zones in mining areas");
        }

        public static void MakeLearnItem(Transform parent, string text)
        {
            var row = UIHelper.MakeRect("LearnItem", parent);
            row.sizeDelta = new Vector2(0, 72);
            UIHelper.SetLayout(row.gameObject, preferredHeight: 72, minHeight: 60);

            var hlg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.UpperLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;

            // Green Check Circle (48x48)
            var checkCircle = UIHelper.MakeRect("CheckCircle", row);
            checkCircle.sizeDelta = new Vector2(48, 48);
            UIHelper.SetLayout(checkCircle.gameObject, preferredWidth: 48, preferredHeight: 48, minWidth: 48, minHeight: 48);

            var checkImg = checkCircle.gameObject.AddComponent<Image>();
            checkImg.sprite = UIHelper.GetCircleSprite();
            checkImg.color = Hex("#DCFCE7");

            var innerDot = UIHelper.MakeRect("InnerDot", checkCircle);
            innerDot.sizeDelta = new Vector2(20, 20);
            var dotImg = innerDot.gameObject.AddComponent<Image>();
            dotImg.sprite = UIHelper.GetCircleSprite();
            dotImg.color = UIColors.SafetyGreen;

            // Wrapped Text Label (flexible width fills remaining card space)
            var itemLbl = UIHelper.MakeLabel("Text", row, text, 28, Hex("#1E293B"), bold: false, wrap: true);
            var itemLE = itemLbl.gameObject.GetComponent<LayoutElement>() ?? itemLbl.gameObject.AddComponent<LayoutElement>();
            itemLE.flexibleWidth = 1f;
            itemLE.minWidth = 300;
        }

        private static void BuildSafetyNote(Transform parent)
        {
            var box = new GameObject("SafetyNoteBox");
            box.transform.SetParent(parent, false);

            var boxImg = box.AddComponent<Image>();
            boxImg.color = Hex("#FEF3C7"); // Warm light amber
            boxImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(boxImg, 28);

            var boxOutline = box.AddComponent<Outline>();
            boxOutline.effectColor = Hex("#FDE68A");
            boxOutline.effectDistance = new Vector2(2, -2);

            // Direct VerticalLayoutGroup on SafetyNoteBox
            var vlg = box.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(36, 36, 32, 32);
            vlg.spacing = 14;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            var csf = box.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Title Row: Amber Warning Badge + SAFETY NOTE
            var titleRow = UIHelper.MakeRect("TitleRow", box.transform);
            titleRow.sizeDelta = new Vector2(0, 40);
            UIHelper.SetLayout(titleRow.gameObject, preferredHeight: 40);

            var titleHLG = titleRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            titleHLG.spacing = 14;
            titleHLG.childAlignment = TextAnchor.MiddleLeft;
            titleHLG.childForceExpandWidth = false;
            titleHLG.childForceExpandHeight = true;
            titleHLG.childControlWidth = false;
            titleHLG.childControlHeight = true;

            var warnBadge = UIHelper.MakeRect("WarnBadge", titleRow);
            warnBadge.sizeDelta = new Vector2(36, 36);
            var wbImg = warnBadge.gameObject.AddComponent<Image>();
            wbImg.color = Hex("#D97706");
            UIHelper.SetImageRoundedSprite(wbImg, 10);

            var titleLbl = UIHelper.MakeLabel("WarnTitle", titleRow, "SAFETY NOTE", 26, Hex("#B45309"), bold: true, wrap: false);
            titleLbl.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 40);

            // Body Warning Text
            var noteLbl = UIHelper.MakeLabel("WarnText", box.transform,
                "Never attempt to fight a fire if evacuation routes are compromised. Always alert colleagues first before responding.",
                26, Hex("#92400E"), bold: false, wrap: true);
            var noteLE = noteLbl.gameObject.GetComponent<LayoutElement>() ?? noteLbl.gameObject.AddComponent<LayoutElement>();
            noteLE.flexibleWidth = 1f;
        }

        private static void BuildStartButton(Transform parent)
        {
            var btn = UIHelper.MakeButton("btn-start-module", parent, "START MODULE  >", 34,
                UIColors.Hex("#0A192F"), Color.white, 28f);
            btn.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 120);
            var le = btn.gameObject.GetComponent<LayoutElement>() ?? btn.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 120;
            le.minHeight = 120;
        }

        private static Color Hex(string hex) => UIColors.Hex(hex);
    }
}
