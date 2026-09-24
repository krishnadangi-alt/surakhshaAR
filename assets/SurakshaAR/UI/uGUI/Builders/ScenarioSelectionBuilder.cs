using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Fire &amp; Explosion Response — Scenario Selection Screen.
    ///
    /// TYPOGRAPHY — Unified with Learn (ModuleSelection) &amp; Progress screens (CanvasScaler 1080×2400):
    ///   Header title   = 48px bold
    ///   Intro title    = 46px bold   (matches Learn/Progress card titles)
    ///   Intro desc     = 34px        (matches Learn/Progress descriptions)
    ///   Section head   = 44px bold   (matches Progress section headings)
    ///   Card title     = 46px bold   (matches Learn/Progress card titles)
    ///   Card desc      = 34px        (matches Learn/Progress descriptions)
    ///   Number badge   = 30px bold   ("01", "02", "03")
    ///   Bottom nav     = 30px bold
    ///
    /// LAYOUT:
    ///   - Fixed 4-tab bottom navigation bar (height 165px, Home active).
    ///   - Full-screen scrollable body with large, rich cards (no empty gaps).
    ///   - Intro card (300px) with prominent flame illustration and description.
    ///   - Three tall, spacious scenario cards (250px each) with large icons, titles, and chevrons.
    /// </summary>
    public static class ScenarioSelectionBuilder
    {
        private const float NAV_H = 165f;

        private static readonly Color AccentOrange = UIColors.Hex("#EA580C");
        private static readonly Color AccentLight  = UIColors.Hex("#FFF7ED");
        private static readonly Color AccentBorder = UIColors.Hex("#FFEDD5");
        private static readonly Color NavyText     = UIColors.Hex("#0A192F");
        private static readonly Color DarkText     = UIColors.Hex("#0F172A");
        private static readonly Color SlateText    = UIColors.Hex("#334155");
        private static readonly Color MutedText    = UIColors.Hex("#475569");
        private static readonly Color SubText      = UIColors.Hex("#64748B");

        private static Color Hex(string h) => UIColors.Hex(h);

        public static GameObject Build()
        {
            var root = new GameObject("ScenarioSelectionScreen");
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            var rootImg = root.AddComponent<Image>();
            rootImg.color  = Hex("#F8FAFC");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // ── 1. Fixed Bottom Navigation Bar ────────────────────────────
            BuildBottomNav(root.transform);

            // ── 2. Full-Screen Scrollable Area ─────────────────────────────
            var scrollRoot = UIHelper.MakeRect("ScrollArea", root.transform);
            scrollRoot.anchorMin = Vector2.zero;
            scrollRoot.anchorMax = Vector2.one;
            scrollRoot.offsetMin = new Vector2(0, NAV_H);
            scrollRoot.offsetMax = Vector2.zero;

            var sr = scrollRoot.gameObject.AddComponent<ScrollRect>();
            sr.horizontal        = false;
            sr.vertical          = true;
            sr.movementType      = ScrollRect.MovementType.Elastic;
            sr.scrollSensitivity = 40f;

            var vp = UIHelper.MakeRect("Viewport", scrollRoot);
            vp.anchorMin = Vector2.zero;
            vp.anchorMax = Vector2.one;
            vp.offsetMin = Vector2.zero;
            vp.offsetMax = Vector2.zero;
            vp.gameObject.AddComponent<RectMask2D>();
            sr.viewport = vp;

            var content = UIHelper.MakeRect("Content", vp);
            content.anchorMin  = new Vector2(0, 1);
            content.anchorMax  = new Vector2(1, 1);
            content.pivot      = new Vector2(0.5f, 1);
            content.offsetMin  = Vector2.zero;
            content.offsetMax  = Vector2.zero;
            content.sizeDelta  = Vector2.zero;
            sr.content         = content;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing                = 24;
            vlg.padding                = new RectOffset(30, 30, 18, 48);
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth      = true;
            vlg.childControlHeight     = true;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── 1. Top Header Row ─────────────────────────────────────────
            BuildTopHeader(content);

            // ── 2. Module Intro Card ──────────────────────────────────────
            BuildIntroCard(content);

            // ── 3. Section Heading ────────────────────────────────────────
            BuildSectionHeading(content, "Select a Scenario");

            // ── 4. Three Scenario Cards ───────────────────────────────────
            BuildScenarioCard(content, "card-scenario-1", "01",
                "Electrical Panel Fire",
                "Handle fire in electrical panels and control rooms.",
                "icon_electrical_panel_fire.jpg");

            BuildScenarioCard(content, "card-scenario-2", "02",
                "Conveyor Belt Fire",
                "Respond to fire in conveyor belt systems.",
                "icon_conveyor_belt_fire.jpg");

            BuildScenarioCard(content, "card-scenario-3", "03",
                "Excavator / HEMM Fire",
                "Handle fire in heavy earth moving machinery.",
                "icon_excavator_hemm_fire.jpg");

            return root;
        }

        // ────────────────────────────────────────────────────────────────
        // TOP HEADER
        // ────────────────────────────────────────────────────────────────
        private static void BuildTopHeader(Transform parent)
        {
            var row = UIHelper.MakeHorizontal("TopHeaderRow", parent, 14,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 80, minHeight: 74);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var backBtn = UIHelper.MakeButton("btn-back", row, "‹", 52,
                Color.white, NavyText, 20);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 72, preferredHeight: 72);
            var outline = backBtn.gameObject.AddComponent<Outline>();
            outline.effectColor    = Hex("#E2E8F0");
            outline.effectDistance = new Vector2(1, -1);

            var titleLbl = UIHelper.MakeLabel("label-title", row,
                "Fire & Explosion Response", 48, NavyText, bold: true);
            titleLbl.textWrappingMode = TextWrappingModes.NoWrap;
            titleLbl.overflowMode     = TextOverflowModes.Ellipsis;
            UIHelper.SetLayout(titleLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 60);

            var langPill = UIHelper.MakeButton("btn-language-picker", row,
                "🌐 EN ▾", 26, Hex("#F1F5F9"), SlateText, 14);
            UIHelper.SetLayout(langPill.gameObject, preferredWidth: 130, preferredHeight: 60);

            var bellBtn = UIHelper.MakeButton("btn-notifications", row,
                "", 14, Color.white, Color.white, 20);
            UIHelper.SetLayout(bellBtn.gameObject, preferredWidth: 60, preferredHeight: 60);
            bellBtn.gameObject.AddComponent<Outline>().effectColor = Hex("#E2E8F0");
            var bellIconGO = UIHelper.MakeRect("BellIcon", bellBtn.transform);
            UIHelper.Stretch(bellIconGO, 14, 14, 14, 14);
            var bellImg = bellIconGO.gameObject.AddComponent<Image>();
            bellImg.sprite        = UIHelper.GetBellSprite();
            bellImg.preserveAspect = true;

            var profBtn = UIHelper.MakeButton("btn-profile", row,
                "", 14, Color.white, Color.white, 20);
            UIHelper.SetLayout(profBtn.gameObject, preferredWidth: 60, preferredHeight: 60);
            profBtn.gameObject.AddComponent<Outline>().effectColor = Hex("#E2E8F0");
            var profIconGO = UIHelper.MakeRect("ProfIcon", profBtn.transform);
            UIHelper.Stretch(profIconGO, 14, 14, 14, 14);
            var profImg = profIconGO.gameObject.AddComponent<Image>();
            profImg.sprite        = UIHelper.GetProfileSprite();
            profImg.preserveAspect = true;
        }

        // ────────────────────────────────────────────────────────────────
        // INTRO CARD (large prominent card, 46px title, 34px desc)
        // ────────────────────────────────────────────────────────────────
        private static void BuildIntroCard(Transform parent)
        {
            var card = UIHelper.MakeRect("IntroCard", parent);
            UIHelper.SetLayout(card.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 300, minHeight: 280);

            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color  = Color.white;
            cardImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cardImg, 26);

            var border = card.gameObject.AddComponent<Outline>();
            border.effectColor    = Hex("#E2E8F0");
            border.effectDistance = new Vector2(1.5f, -1.5f);

            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor    = new Color(0, 0, 0, 0.05f);
            shadow.effectDistance = new Vector2(0, -3f);

            var row = UIHelper.MakeHorizontal("InnerRow", card, 22,
                new RectOffset(24, 24, 20, 20),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(row, 0, 0, 0, 0);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // Icon area (160×160 light orange rounded square)
            var iconArea = UIHelper.MakeRect("IconArea", row);
            UIHelper.SetLayout(iconArea.gameObject,
                preferredWidth: 160, minWidth: 150,
                preferredHeight: 160, minHeight: 150);
            var iaImg = iconArea.gameObject.AddComponent<Image>();
            iaImg.color  = AccentLight;
            iaImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(iaImg, 22);

            var spr = UIHelper.LoadProjectSprite("icon_fire_scenario.jpg");
            if (spr != null)
            {
                var iconImgGO = UIHelper.MakeRect("ModuleIcon", iconArea);
                UIHelper.Stretch(iconImgGO, 16, 16, 16, 16);
                var ii = iconImgGO.gameObject.AddComponent<Image>();
                ii.sprite         = spr;
                ii.preserveAspect = true;
                ii.raycastTarget  = false;
            }
            else
            {
                var fbLbl = UIHelper.MakeLabel("FbLbl", iconArea, "🔥", 72,
                    AccentOrange, TextAlignmentOptions.Center);
                UIHelper.Stretch(fbLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);
            }

            // Right: text column
            var textCol = UIHelper.MakeVertical("TextCol", row, 8,
                childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(textCol.gameObject, flexibleWidth: true, flexWidth: 1);
            var tvlg = textCol.GetComponent<VerticalLayoutGroup>();
            tvlg.childAlignment       = TextAnchor.UpperLeft;
            tvlg.childForceExpandHeight = false;

            // Title: 46px bold (Learn/Progress screen module title scale)
            var titleLbl = UIHelper.MakeLabel("IntroTitle", textCol,
                "Fire & Explosion Response", 46, DarkText,
                bold: true, wrap: true);
            titleLbl.lineSpacing = 1.05f;
            UIHelper.SetLayout(titleLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 110);

            // Description: 34px (Learn/Progress screen description scale)
            var descLbl = UIHelper.MakeLabel("IntroDesc", textCol,
                "Learn to identify, prevent and respond to fire and explosion hazards in mining and industrial environments.",
                34, MutedText, wrap: true);
            descLbl.lineSpacing = 1.15f;
            UIHelper.SetLayout(descLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 130);
        }

        // ────────────────────────────────────────────────────────────────
        // SECTION HEADING (with accent bar matching reference)
        // ────────────────────────────────────────────────────────────────
        private static void BuildSectionHeading(Transform parent, string text)
        {
            var row = UIHelper.MakeHorizontal("SecHeadRow", parent, 12,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 52, minHeight: 48);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var bar = UIHelper.MakeRect("Bar", row);
            UIHelper.SetLayout(bar.gameObject, preferredWidth: 6, preferredHeight: 38);
            var barImg = bar.gameObject.AddComponent<Image>();
            barImg.color  = AccentOrange;
            barImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(barImg, 3);

            var lbl = UIHelper.MakeLabel("label-select-scenario", row,
                text, 44, DarkText, bold: true);
            UIHelper.SetLayout(lbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 48);
        }

        // ────────────────────────────────────────────────────────────────
        // SCENARIO CARD (tall, spacious, 46px title, 34px desc)
        // ────────────────────────────────────────────────────────────────
        private static void BuildScenarioCard(
            Transform parent,
            string btnName,
            string numStr,
            string title,
            string desc,
            string iconFilename)
        {
            var card = UIHelper.MakeButton(btnName, parent, "",
                16, Color.white, Color.white, 24);
            UIHelper.SetLayout(card.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 250, minHeight: 240);

            var cardImg = card.GetComponent<Image>();
            cardImg.color  = Color.white;
            cardImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cardImg, 24);

            var border = card.gameObject.AddComponent<Outline>();
            border.effectColor    = Hex("#E2E8F0");
            border.effectDistance = new Vector2(1.5f, -1.5f);

            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor    = new Color(0, 0, 0, 0.04f);
            shadow.effectDistance = new Vector2(0, -3f);

            // Inner horizontal row
            var row = UIHelper.MakeHorizontal("Inner", card.transform, 20,
                new RectOffset(20, 20, 16, 16),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(row, 0, 0, 0, 0);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // ── Left: icon container with number badge ──────────────────
            var iconWrap = UIHelper.MakeRect("IconWrap", row);
            UIHelper.SetLayout(iconWrap.gameObject,
                preferredWidth: 150, minWidth: 140,
                preferredHeight: 150, minHeight: 140);

            var ibImg = iconWrap.gameObject.AddComponent<Image>();
            ibImg.color  = AccentLight;
            ibImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(ibImg, 20);

            var spr = UIHelper.LoadProjectSprite(iconFilename);
            if (spr != null)
            {
                var iconImgGO = UIHelper.MakeRect("ScenarioIcon", iconWrap);
                UIHelper.Stretch(iconImgGO, 16, 16, 16, 16);
                var ii = iconImgGO.gameObject.AddComponent<Image>();
                ii.sprite         = spr;
                ii.preserveAspect = true;
                ii.raycastTarget  = false;
            }

            // Numbered orange capsule badge at top-left
            var badge = UIHelper.MakeRect("NumBadge", iconWrap);
            badge.anchorMin        = new Vector2(0, 1);
            badge.anchorMax        = new Vector2(0, 1);
            badge.pivot            = new Vector2(0, 1);
            badge.anchoredPosition = new Vector2(0, 0);
            badge.sizeDelta        = new Vector2(60, 52);
            var badgeImg = badge.gameObject.AddComponent<Image>();
            badgeImg.color  = AccentOrange;
            badgeImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(badgeImg, 14);

            var numLbl = UIHelper.MakeLabel("Num", badge, numStr, 30,
                Color.white, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(numLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // ── Centre: text column ─────────────────────────────────────
            var textCol = UIHelper.MakeVertical("TextCol", row, 8,
                childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(textCol.gameObject,
                flexibleWidth: true, flexWidth: 1);
            var tvlg = textCol.GetComponent<VerticalLayoutGroup>();
            tvlg.childAlignment       = TextAnchor.UpperLeft;
            tvlg.childForceExpandHeight = false;

            // Title: 46px bold (Learn/Progress screen module title scale)
            var titleLbl = UIHelper.MakeLabel("Title", textCol,
                title, 46, DarkText, bold: true, wrap: true);
            titleLbl.lineSpacing = 1.05f;
            UIHelper.SetLayout(titleLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 60);

            // Description: 34px (Learn/Progress screen description scale)
            var descLbl = UIHelper.MakeLabel("Desc", textCol,
                desc, 34, MutedText, wrap: true);
            descLbl.lineSpacing = 1.15f;
            UIHelper.SetLayout(descLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 88);

            // ── Right: orange circle arrow button ───────────────────────
            var arrowCircle = UIHelper.MakeRect("ArrowCircle", row);
            UIHelper.SetLayout(arrowCircle.gameObject,
                preferredWidth: 68, minWidth: 64,
                preferredHeight: 68, minHeight: 64);
            var acImg = arrowCircle.gameObject.AddComponent<Image>();
            acImg.color  = AccentOrange;
            acImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(acImg, 34);

            var chevLbl = UIHelper.MakeLabel("Chev", arrowCircle,
                "›", 52, Color.white, TextAlignmentOptions.Center, bold: true);
            var chevRT = chevLbl.GetComponent<RectTransform>();
            UIHelper.Stretch(chevRT, 0, 0, 0, 0);
            chevRT.anchoredPosition = new Vector2(2f, 1f);
        }

        // ────────────────────────────────────────────────────────────────
        // BOTTOM NAVIGATION
        // ────────────────────────────────────────────────────────────────
        private static void BuildBottomNav(Transform parent)
        {
            var navRT = UIHelper.MakeRect("BottomNavBar", parent);
            navRT.anchorMin = Vector2.zero;
            navRT.anchorMax = new Vector2(1, 0);
            navRT.pivot     = new Vector2(0.5f, 0);
            navRT.sizeDelta = new Vector2(0, NAV_H);

            var bg = navRT.gameObject.AddComponent<Image>();
            bg.color  = Color.white;
            bg.sprite = UIHelper.GetWhiteSprite();

            var topBorder = UIHelper.MakeRect("TopBorder", navRT);
            topBorder.anchorMin = new Vector2(0, 1);
            topBorder.anchorMax = new Vector2(1, 1);
            topBorder.pivot     = new Vector2(0.5f, 1);
            topBorder.sizeDelta = new Vector2(0, 1.5f);
            topBorder.gameObject.AddComponent<Image>().color = Hex("#E2E8F0");
            topBorder.gameObject.AddComponent<LayoutElement>().ignoreLayout = true; // Prevents layout interference

            var hlg = navRT.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth  = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth      = true;
            hlg.childControlHeight     = true;
            hlg.padding = new RectOffset(0, 0, 8, 20);

            MakeNavItem(navRT, "nav-home",         "Home",         UIHelper.GetHomeSprite(),  true);
            MakeNavItem(navRT, "nav-learn",        "Learn",        UIHelper.GetBookSprite(),  false);
            MakeNavItem(navRT, "nav-progress",     "My Progress",  UIHelper.GetChartSprite(), false);
            MakeNavItem(navRT, "nav-certificates", "Certificates", UIHelper.GetMedalSprite(), false);
        }

        private static void MakeNavItem(Transform parent, string name,
            string label, Sprite iconSprite, bool active)
        {
            var activeColor = active ? Hex("#059669") : Hex("#94A3B8");
            var fontSize    = 30f;

            var btn = UIHelper.MakeButton(name, parent, "",
                14, UIColors.Transparent, Color.white, 0);

            var col = UIHelper.MakeVertical("Col", btn.transform, 6,
                new RectOffset(0, 0, 4, 4),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(col, 0, 0, 0, 0);
            col.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var iconBox = UIHelper.MakeRect("IconBox", col);
            UIHelper.SetLayout(iconBox.gameObject,
                preferredWidth: 50, minWidth: 50, preferredHeight: 50, minHeight: 50);
            var img = iconBox.gameObject.AddComponent<Image>();
            img.sprite         = iconSprite;
            img.color          = activeColor;
            img.preserveAspect = true;

            var lbl = UIHelper.MakeLabel($"label-{name}", col,
                label, fontSize, activeColor,
                TextAlignmentOptions.Center, bold: true);
            lbl.textWrappingMode = TextWrappingModes.NoWrap;
            lbl.overflowMode     = TextOverflowModes.Overflow;
            UIHelper.SetLayout(lbl.gameObject, preferredHeight: 42, minHeight: 38);

            var indRow = UIHelper.MakeRect("IndRow", col);
            UIHelper.SetLayout(indRow.gameObject,
                preferredWidth: 64, minWidth: 64, preferredHeight: 6, minHeight: 6);
            if (active)
            {
                var indImg = indRow.gameObject.AddComponent<Image>();
                indImg.color  = Hex("#059669");
                indImg.sprite = UIHelper.GetWhiteSprite();
                UIHelper.SetImageRoundedSprite(indImg, 3);
            }
        }
    }
}
