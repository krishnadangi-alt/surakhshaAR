using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Fire &amp; Explosion Response — Scenario Selection Screen.
    ///
    /// TYPOGRAPHY matches the approved Home Dashboard system (CanvasScaler 1080×2400):
    ///   Screen title  = 42px bold   (matches Home card title 38px, +1 level)
    ///   Intro title   = 38px bold   (Home module card title)
    ///   Intro desc    = 28px        (Home module card subtitle)
    ///   Section head  = 36px bold
    ///   Card title    = 34px bold
    ///   Card desc     = 26px
    ///   Chip text     = 26px bold
    ///   Bottom nav    = 30px bold   (Home exact)
    ///
    /// LAYOUT: icon-based, NO photographic banner (reference image spec).
    /// Intro card = compact white card with flame icon + description.
    /// Scenario cards = numbered, icon illustration, white bg, orange accent.
    /// </summary>
    public static class ScenarioSelectionBuilder
    {
        // ── Accent palette (Fire orange) ─────────────────────────────────
        private static readonly Color AccentOrange = UIColors.Hex("#EA580C");
        private static readonly Color AccentLight  = UIColors.Hex("#FFF7ED");
        private static readonly Color AccentBorder = UIColors.Hex("#FFEDD5");
        private static readonly Color NavyText     = UIColors.Hex("#0A192F");
        private static readonly Color SlateText    = UIColors.Hex("#334155");
        private static readonly Color SubText      = UIColors.Hex("#64748B");

        // Home canvas constants (matches HomeDashboardBuilder and ModuleSelectionBuilder)
        private const float NAV_H = 165f;  // bottom nav height

        private static Color Hex(string h) => UIColors.Hex(h);

        public static GameObject Build()
        {
            var root = new GameObject("ScenarioSelectionScreen");
            root.AddComponent<RectTransform>();
            var rootImg = root.AddComponent<Image>();
            rootImg.color  = Hex("#F8FAFC");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // ── Bottom nav (fixed, behind scroll) ────────────────────────
            BuildBottomNav(root.transform);

            // ── Scrollable body ──────────────────────────────────────────
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
            vlg.spacing              = 20;
            vlg.padding              = new RectOffset(28, 28, 20, 40);
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth      = true;
            vlg.childControlHeight     = false;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── 1. Top header row (back + title + lang/bell/profile) ─────
            BuildTopHeader(content);

            // ── 2. Module intro card (icon + title + description) ─────────
            BuildIntroCard(content);

            // ── 3. "Select a Scenario" section heading ────────────────────
            BuildSectionHeading(content, "Select a Scenario");

            // ── 4. Three scenario cards ───────────────────────────────────
            BuildScenarioCard(content, "card-scenario-1", "01",
                "Electrical Panel Fire",
                "Handle fire in electrical panels and control rooms.",
                "icon_electrical_panel_fire.jpg", available: true);

            BuildScenarioCard(content, "card-scenario-2", "02",
                "Conveyor Belt Fire",
                "Respond to fire in conveyor belt systems.",
                "icon_conveyor_belt_fire.jpg", available: false);

            BuildScenarioCard(content, "card-scenario-3", "03",
                "Excavator / HEMM Fire",
                "Handle fire in heavy earth moving machinery.",
                "icon_excavator_hemm_fire.jpg", available: false);

            return root;
        }

        // ────────────────────────────────────────────────────────────────
        // TOP HEADER
        // ────────────────────────────────────────────────────────────────
        private static void BuildTopHeader(Transform parent)
        {
            var row = UIHelper.MakeHorizontal("TopHeaderRow", parent, 12,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 72, minHeight: 64);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // Back button — matches Home header icon buttons (78×78 → scale to 72)
            var backBtn = UIHelper.MakeButton("btn-back", row, "‹", 56,
                Color.white, NavyText, 18);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 70, preferredHeight: 70);
            var outline = backBtn.gameObject.AddComponent<Outline>();
            outline.effectColor    = Hex("#E2E8F0");
            outline.effectDistance = new Vector2(1, -1);

            // Screen title — 42px bold (matches Home module card title scale)
            var titleLbl = UIHelper.MakeLabel("label-title", row,
                "Fire & Explosion Response", 42, NavyText, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 56);

            // Language pill (compact, same as Home but shorter)
            var langPill = UIHelper.MakeButton("btn-language-picker", row,
                "🌐 EN ▾", 26, Hex("#F1F5F9"), SlateText, 14);
            UIHelper.SetLayout(langPill.gameObject, preferredWidth: 130, preferredHeight: 58);

            // Bell
            var bellBtn = UIHelper.MakeButton("btn-notifications", row,
                "", 14, Color.white, Color.white, 20);
            UIHelper.SetLayout(bellBtn.gameObject, preferredWidth: 58, preferredHeight: 58);
            var bellBorder = bellBtn.gameObject.AddComponent<Outline>();
            bellBorder.effectColor    = Hex("#E2E8F0");
            bellBorder.effectDistance = new Vector2(1, -1);
            var bellIconGO = UIHelper.MakeRect("BellIcon", bellBtn.transform);
            UIHelper.Stretch(bellIconGO, 12, 12, 12, 12);
            var bellImg = bellIconGO.gameObject.AddComponent<Image>();
            bellImg.sprite = UIHelper.GetBellSprite();
            bellImg.color  = SlateText;

            // Profile
            var profBtn = UIHelper.MakeButton("btn-profile", row,
                "", 14, Color.white, Color.white, 20);
            UIHelper.SetLayout(profBtn.gameObject, preferredWidth: 58, preferredHeight: 58);
            var profBorder = profBtn.gameObject.AddComponent<Outline>();
            profBorder.effectColor    = Hex("#E2E8F0");
            profBorder.effectDistance = new Vector2(1, -1);
            var profIconGO = UIHelper.MakeRect("ProfIcon", profBtn.transform);
            UIHelper.Stretch(profIconGO, 12, 12, 12, 12);
            var profImg = profIconGO.gameObject.AddComponent<Image>();
            profImg.sprite = UIHelper.GetProfileSprite();
            profImg.color  = SlateText;
        }

        // ────────────────────────────────────────────────────────────────
        // INTRO CARD (matches reference: white card, flame icon, title, desc)
        // ────────────────────────────────────────────────────────────────
        private static void BuildIntroCard(Transform parent)
        {
            var card = UIHelper.MakeRect("IntroCard", parent);
            UIHelper.SetLayout(card.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 220, minHeight: 200);

            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 22);

            var border = card.gameObject.AddComponent<Outline>();
            border.effectColor    = Hex("#E2E8F0");
            border.effectDistance = new Vector2(1.5f, -1.5f);

            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor    = new Color(0, 0, 0, 0.04f);
            shadow.effectDistance = new Vector2(0, -3f);

            var row = UIHelper.MakeHorizontal("InnerRow", card, 20,
                new RectOffset(20, 20, 16, 16),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(row, 0, 0, 0, 0);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // Icon area (light orange rounded square, 150×150 matches reference)
            var iconArea = UIHelper.MakeRect("IconArea", row);
            UIHelper.SetLayout(iconArea.gameObject,
                preferredWidth: 150, minWidth: 140,
                preferredHeight: 150, minHeight: 140);
            var iaImg = iconArea.gameObject.AddComponent<Image>();
            iaImg.color = AccentLight;
            UIHelper.SetImageRoundedSprite(iaImg, 18);

            // Module icon sprite
            var iconSpr = UIHelper.LoadProjectSprite("icon_fire_scenario.jpg");
            if (iconSpr != null)
            {
                var iconImgGO = UIHelper.MakeRect("ModuleIcon", iconArea);
                UIHelper.Stretch(iconImgGO, 16, 16, 16, 16);
                var ii = iconImgGO.gameObject.AddComponent<Image>();
                ii.sprite = iconSpr;
                ii.preserveAspect = true;
                ii.raycastTarget  = false;
            }
            else
            {
                // Fallback: orange flame emoji label
                var fbLbl = UIHelper.MakeLabel("FbLbl", iconArea, "🔥", 64,
                    AccentOrange, TextAlignmentOptions.Center);
                UIHelper.Stretch(fbLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);
            }

            // Right: text column
            var textCol = UIHelper.MakeVertical("TextCol", row, 10,
                childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(textCol.gameObject,
                flexibleWidth: true, flexWidth: 1);
            var tvlg = textCol.GetComponent<VerticalLayoutGroup>();
            tvlg.childAlignment       = TextAnchor.UpperLeft;
            tvlg.childForceExpandHeight = false;

            // Title — 38px bold (Home module card title scale)
            var titleLbl = UIHelper.MakeLabel("IntroTitle", textCol,
                "Fire & Explosion Response", 38, NavyText,
                bold: true, wrap: true);
            titleLbl.lineSpacing = 1.05f;
            UIHelper.SetLayout(titleLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 92);

            // Description — 28px (Home module card subtitle scale)
            var descLbl = UIHelper.MakeLabel("IntroDesc", textCol,
                "Learn to identify, prevent and respond to fire and explosion hazards in mining and industrial environments.",
                28, SlateText, wrap: true);
            descLbl.lineSpacing = 1.2f;
            UIHelper.SetLayout(descLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 80);
        }

        // ────────────────────────────────────────────────────────────────
        // SECTION HEADING with bold left label
        // ────────────────────────────────────────────────────────────────
        private static void BuildSectionHeading(Transform parent, string text)
        {
            var lbl = UIHelper.MakeLabel("label-select-scenario", parent,
                text, 36, NavyText, bold: true);
            UIHelper.SetLayout(lbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 50);
        }

        // ────────────────────────────────────────────────────────────────
        // SCENARIO CARD
        // Matches reference: numbered orange badge, icon illustration,
        // title, desc, orange circular chevron. White bg, light border.
        // ────────────────────────────────────────────────────────────────
        private static void BuildScenarioCard(
            Transform parent,
            string btnName,
            string numStr,
            string title,
            string desc,
            string iconFilename,
            bool available)
        {
            var card = UIHelper.MakeButton(btnName, parent, "",
                16, Color.white, Color.white, 22);
            UIHelper.SetLayout(card.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 180, minHeight: 168);

            var cardImg = card.GetComponent<Image>();
            cardImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 22);

            var border = card.gameObject.AddComponent<Outline>();
            border.effectColor    = Hex("#E2E8F0");
            border.effectDistance = new Vector2(1.5f, -1.5f);

            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor    = new Color(0, 0, 0, 0.03f);
            shadow.effectDistance = new Vector2(0, -2f);

            // Inner horizontal row
            var row = UIHelper.MakeHorizontal("Inner", card.transform, 18,
                new RectOffset(20, 16, 16, 16),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(row, 0, 0, 0, 0);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // ── Left: icon square with orange number badge ──────────────
            var iconWrap = UIHelper.MakeRect("IconWrap", row);
            UIHelper.SetLayout(iconWrap.gameObject,
                preferredWidth: 130, minWidth: 120,
                preferredHeight: 130, minHeight: 120);

            // Icon background (light orange)
            var ibImg = iconWrap.gameObject.AddComponent<Image>();
            ibImg.color = AccentLight;
            UIHelper.SetImageRoundedSprite(ibImg, 16);

            // Scenario illustration
            var spr = UIHelper.LoadProjectSprite(iconFilename);
            if (spr != null)
            {
                var iconImgGO = UIHelper.MakeRect("ScenarioIcon", iconWrap);
                UIHelper.Stretch(iconImgGO, 14, 14, 14, 14);
                var ii = iconImgGO.gameObject.AddComponent<Image>();
                ii.sprite = spr;
                ii.preserveAspect = true;
                ii.raycastTarget  = false;
            }

            // Numbered orange badge (top-left corner of icon area)
            var badge = UIHelper.MakeRect("NumBadge", iconWrap);
            badge.anchorMin        = new Vector2(0, 1);
            badge.anchorMax        = new Vector2(0, 1);
            badge.pivot            = new Vector2(0, 1);
            badge.anchoredPosition = new Vector2(0, 0);
            badge.sizeDelta        = new Vector2(52, 52);
            var badgeImg = badge.gameObject.AddComponent<Image>();
            badgeImg.color = AccentOrange;
            UIHelper.SetImageRoundedSprite(badgeImg, 14);

            var numLbl = UIHelper.MakeLabel("Num", badge, numStr, 28,
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

            // Title — 34px bold
            var titleLbl = UIHelper.MakeLabel("Title", textCol,
                title, 34, NavyText, bold: true, wrap: true);
            titleLbl.lineSpacing = 1.05f;
            UIHelper.SetLayout(titleLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 42);

            // Description — 26px
            var descLbl = UIHelper.MakeLabel("Desc", textCol,
                desc, 26, SubText, wrap: true);
            descLbl.lineSpacing = 1.15f;
            UIHelper.SetLayout(descLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 62);

            // Coming Soon pill
            if (!available)
            {
                var pill = UIHelper.MakeRect("ComingSoonPill", textCol);
                UIHelper.SetLayout(pill.gameObject,
                    preferredWidth: 200, preferredHeight: 38);
                var pImg = pill.gameObject.AddComponent<Image>();
                pImg.color = Hex("#F1F5F9");
                UIHelper.SetImageRoundedSprite(pImg, 10);
                var pLbl = UIHelper.MakeLabel("PillLbl", pill,
                    "Coming Soon", 22, Hex("#64748B"),
                    TextAlignmentOptions.Center);
                UIHelper.Stretch(pLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);
            }

            // ── Right: orange circle arrow button ───────────────────────
            var arrowCircle = UIHelper.MakeRect("ArrowCircle", row);
            UIHelper.SetLayout(arrowCircle.gameObject,
                preferredWidth: 64, minWidth: 60,
                preferredHeight: 64, minHeight: 60);
            var acImg = arrowCircle.gameObject.AddComponent<Image>();
            acImg.color = available ? AccentOrange : Hex("#CBD5E1");
            UIHelper.SetImageRoundedSprite(acImg, 32);

            var chevLbl = UIHelper.MakeLabel("Chev", arrowCircle,
                "›", 48, Color.white, TextAlignmentOptions.Center, bold: true);
            var chevRT = chevLbl.GetComponent<RectTransform>();
            UIHelper.Stretch(chevRT, 0, 0, 0, 0);
            chevRT.anchoredPosition = new Vector2(2f, 1f);
        }

        // ────────────────────────────────────────────────────────────────
        // BOTTOM NAVIGATION — exact Home scale (30px, 50×50 icons, 140h)
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
            var activeColor = active ? Hex("#16A34A") : Hex("#334155");

            var btn = UIHelper.MakeButton(name, parent, "",
                14, UIColors.Transparent, Color.white, 0);

            var col = UIHelper.MakeVertical("Col", btn.transform, 6,
                new RectOffset(0, 0, 4, 4),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(col, 0, 0, 0, 0);
            col.GetComponent<VerticalLayoutGroup>().childAlignment =
                TextAnchor.MiddleCenter;

            // Icon — exactly 50×50 (Home value)
            var iconBox = UIHelper.MakeRect("IconBox", col);
            UIHelper.SetLayout(iconBox.gameObject,
                preferredWidth: 50, minWidth: 50,
                preferredHeight: 50, minHeight: 50);
            var img = iconBox.gameObject.AddComponent<Image>();
            img.sprite         = iconSprite;
            img.color          = activeColor;
            img.preserveAspect = true;

            // Label — exactly 30px bold (Home value)
            var lbl = UIHelper.MakeLabel($"label-{name}", col,
                label, 30, activeColor,
                TextAlignmentOptions.Center, bold: active);
            lbl.textWrappingMode = TextWrappingModes.NoWrap;
            UIHelper.SetLayout(lbl.gameObject,
                preferredHeight: 42, minHeight: 38);
        }
    }
}
