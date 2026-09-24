using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Gas Leak &amp; Confined Space — Scenario Selection Screen.
    ///
    /// IDENTICAL layout to ScenarioSelectionBuilder, but with:
    ///   - Blue accent (#2563EB / #EFF6FF)
    ///   - Gas mask icon instead of flame
    ///   - Three gas sub-modules from the reference image:
    ///     01 — Underground Gas Release
    ///     02 — Confined Space Entry
    ///     03 — Gas Cylinder Leak
    ///
    /// All modules are COMING SOON (no Gas AR scene yet).
    ///
    /// TYPOGRAPHY — same Home Dashboard system as Fire selection.
    /// </summary>
    public static class GasScenarioSelectionBuilder
    {
        // ── Accent palette (Gas blue) ────────────────────────────────────
        private static readonly Color AccentBlue   = UIColors.Hex("#2563EB");
        private static readonly Color AccentLight   = UIColors.Hex("#EFF6FF");
        private static readonly Color AccentBorder  = UIColors.Hex("#BFDBFE");
        private static readonly Color NavyText      = UIColors.Hex("#0A192F");
        private static readonly Color SlateText     = UIColors.Hex("#334155");
        private static readonly Color SubText       = UIColors.Hex("#64748B");

        private const float NAV_H = 140f;

        private static Color Hex(string h) => UIColors.Hex(h);

        public static GameObject Build()
        {
            var root = new GameObject("GasScenarioSelectionScreen");
            root.AddComponent<RectTransform>();
            var rootImg = root.AddComponent<Image>();
            rootImg.color  = Hex("#F8FAFC");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            BuildBottomNav(root.transform);

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
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot     = new Vector2(0.5f, 1);
            content.sizeDelta = Vector2.zero;
            sr.content        = content;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing              = 20;
            vlg.padding              = new RectOffset(28, 28, 20, 40);
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth      = true;
            vlg.childControlHeight     = false;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            BuildTopHeader(content);
            BuildIntroCard(content);
            BuildSectionHeading(content, "Select a Scenario");

            // Exact scenarios from the reference image:
            BuildScenarioCard(content, "card-scenario-1", "01",
                "Underground Gas Release",
                "Recognize a gas release, raise the alarm and move to a safe area.",
                "icon_gas_scenario.jpg");

            BuildScenarioCard(content, "card-scenario-2", "02",
                "Confined Space Entry",
                "Follow safe entry procedures and use gas detection & PPE.",
                "icon_gas_scenario.jpg");

            BuildScenarioCard(content, "card-scenario-3", "03",
                "Gas Cylinder Leak",
                "Respond to a gas cylinder leak and control the hazard safely.",
                "icon_gas_scenario.jpg");

            return root;
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildTopHeader(Transform parent)
        {
            var row = UIHelper.MakeHorizontal("TopHeaderRow", parent, 12,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 72, minHeight: 64);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment =
                TextAnchor.MiddleLeft;

            var backBtn = UIHelper.MakeButton("btn-back", row,
                "‹", 56, Color.white, NavyText, 18);
            UIHelper.SetLayout(backBtn.gameObject,
                preferredWidth: 70, preferredHeight: 70);
            backBtn.gameObject.AddComponent<Outline>().effectColor = Hex("#E2E8F0");

            var titleLbl = UIHelper.MakeLabel("label-title", row,
                "Gas Leak & Confined Space", 42, NavyText, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 56);

            var langPill = UIHelper.MakeButton("btn-language-picker", row,
                "🌐 EN ▾", 26, Hex("#F1F5F9"), SlateText, 14);
            UIHelper.SetLayout(langPill.gameObject, preferredWidth: 130, preferredHeight: 58);

            var bellBtn = UIHelper.MakeButton("btn-notifications", row,
                "", 14, Color.white, Color.white, 20);
            UIHelper.SetLayout(bellBtn.gameObject, preferredWidth: 58, preferredHeight: 58);
            bellBtn.gameObject.AddComponent<Outline>().effectColor = Hex("#E2E8F0");
            var bellGO = UIHelper.MakeRect("BellIcon", bellBtn.transform);
            UIHelper.Stretch(bellGO, 12, 12, 12, 12);
            bellGO.gameObject.AddComponent<Image>().sprite = UIHelper.GetBellSprite();

            var profBtn = UIHelper.MakeButton("btn-profile", row,
                "", 14, Color.white, Color.white, 20);
            UIHelper.SetLayout(profBtn.gameObject, preferredWidth: 58, preferredHeight: 58);
            profBtn.gameObject.AddComponent<Outline>().effectColor = Hex("#E2E8F0");
            var profGO = UIHelper.MakeRect("ProfIcon", profBtn.transform);
            UIHelper.Stretch(profGO, 12, 12, 12, 12);
            profGO.gameObject.AddComponent<Image>().sprite = UIHelper.GetProfileSprite();
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildIntroCard(Transform parent)
        {
            var card = UIHelper.MakeRect("IntroCard", parent);
            UIHelper.SetLayout(card.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 220, minHeight: 200);

            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 22);

            card.gameObject.AddComponent<Outline>().effectColor = Hex("#E2E8F0");
            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor    = new Color(0, 0, 0, 0.04f);
            shadow.effectDistance = new Vector2(0, -3f);

            var row = UIHelper.MakeHorizontal("InnerRow", card, 20,
                new RectOffset(20, 20, 16, 16),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(row, 0, 0, 0, 0);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment =
                TextAnchor.MiddleLeft;

            // Icon area (light blue)
            var iconArea = UIHelper.MakeRect("IconArea", row);
            UIHelper.SetLayout(iconArea.gameObject,
                preferredWidth: 150, minWidth: 140,
                preferredHeight: 150, minHeight: 140);
            var iaImg = iconArea.gameObject.AddComponent<Image>();
            iaImg.color = AccentLight;
            UIHelper.SetImageRoundedSprite(iaImg, 18);

            var spr = UIHelper.LoadProjectSprite("icon_gas_scenario.jpg");
            if (spr != null)
            {
                var iconGO = UIHelper.MakeRect("Icon", iconArea);
                UIHelper.Stretch(iconGO, 16, 16, 16, 16);
                var ii = iconGO.gameObject.AddComponent<Image>();
                ii.sprite = spr;
                ii.preserveAspect = true;
                ii.raycastTarget  = false;
            }
            else
            {
                var fbLbl = UIHelper.MakeLabel("FbLbl", iconArea,
                    "⛽", 64, AccentBlue, TextAlignmentOptions.Center);
                UIHelper.Stretch(fbLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);
            }

            // Text column
            var textCol = UIHelper.MakeVertical("TextCol", row, 10,
                childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(textCol.gameObject,
                flexibleWidth: true, flexWidth: 1);
            textCol.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;

            var titleLbl = UIHelper.MakeLabel("IntroTitle", textCol,
                "Gas Leak & Confined Space",
                38, NavyText, bold: true, wrap: true);
            titleLbl.lineSpacing = 1.05f;
            UIHelper.SetLayout(titleLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 92);

            var descLbl = UIHelper.MakeLabel("IntroDesc", textCol,
                "Practice gas detection, safe withdrawal, ventilation awareness and confined space precautions.",
                28, SlateText, wrap: true);
            descLbl.lineSpacing = 1.2f;
            UIHelper.SetLayout(descLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 80);
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildSectionHeading(Transform parent, string text)
        {
            var lbl = UIHelper.MakeLabel("label-select-scenario", parent,
                text, 36, NavyText, bold: true);
            UIHelper.SetLayout(lbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 50);
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildScenarioCard(
            Transform parent, string btnName, string numStr,
            string title, string desc, string iconFilename)
        {
            var card = UIHelper.MakeButton(btnName, parent, "",
                16, Color.white, Color.white, 22);
            UIHelper.SetLayout(card.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 180, minHeight: 168);

            var cardImg = card.GetComponent<Image>();
            cardImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 22);

            card.gameObject.AddComponent<Outline>().effectColor = Hex("#E2E8F0");
            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor    = new Color(0, 0, 0, 0.03f);
            shadow.effectDistance = new Vector2(0, -2f);

            var row = UIHelper.MakeHorizontal("Inner", card.transform, 18,
                new RectOffset(20, 16, 16, 16),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(row, 0, 0, 0, 0);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment =
                TextAnchor.MiddleLeft;

            // Icon square (light blue)
            var iconWrap = UIHelper.MakeRect("IconWrap", row);
            UIHelper.SetLayout(iconWrap.gameObject,
                preferredWidth: 130, minWidth: 120,
                preferredHeight: 130, minHeight: 120);
            var ibImg = iconWrap.gameObject.AddComponent<Image>();
            ibImg.color = AccentLight;
            UIHelper.SetImageRoundedSprite(ibImg, 16);

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

            // Blue number badge (top-left)
            var badge = UIHelper.MakeRect("NumBadge", iconWrap);
            badge.anchorMin        = new Vector2(0, 1);
            badge.anchorMax        = new Vector2(0, 1);
            badge.pivot            = new Vector2(0, 1);
            badge.anchoredPosition = Vector2.zero;
            badge.sizeDelta        = new Vector2(52, 52);
            var badgeImg = badge.gameObject.AddComponent<Image>();
            badgeImg.color = AccentBlue;
            UIHelper.SetImageRoundedSprite(badgeImg, 14);

            var numLbl = UIHelper.MakeLabel("Num", badge,
                numStr, 28, Color.white,
                TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(numLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Text column
            var textCol = UIHelper.MakeVertical("TextCol", row, 8,
                childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(textCol.gameObject,
                flexibleWidth: true, flexWidth: 1);
            textCol.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;

            var titleLbl = UIHelper.MakeLabel("Title", textCol,
                title, 34, NavyText, bold: true, wrap: true);
            titleLbl.lineSpacing = 1.05f;
            UIHelper.SetLayout(titleLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 42);

            var descLbl = UIHelper.MakeLabel("Desc", textCol,
                desc, 26, SubText, wrap: true);
            descLbl.lineSpacing = 1.15f;
            UIHelper.SetLayout(descLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 62);

            // Coming Soon pill (all gas modules are currently unavailable)
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

            // Grey circle arrow (coming soon = disabled)
            var arrowCircle = UIHelper.MakeRect("ArrowCircle", row);
            UIHelper.SetLayout(arrowCircle.gameObject,
                preferredWidth: 64, minWidth: 60,
                preferredHeight: 64, minHeight: 60);
            var acImg = arrowCircle.gameObject.AddComponent<Image>();
            acImg.color = Hex("#CBD5E1");
            UIHelper.SetImageRoundedSprite(acImg, 32);

            var chevLbl = UIHelper.MakeLabel("Chev", arrowCircle,
                "›", 48, Color.white, TextAlignmentOptions.Center, bold: true);
            var chevRT = chevLbl.GetComponent<RectTransform>();
            UIHelper.Stretch(chevRT, 0, 0, 0, 0);
            chevRT.anchoredPosition = new Vector2(2f, 1f);
        }

        // ─────────────────────────────────────────────────────────────────
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

            MakeNavItem(navRT, "nav-home",         "Home",        UIHelper.GetHomeSprite(),  false);
            MakeNavItem(navRT, "nav-learn",        "Learn",       UIHelper.GetBookSprite(),  true);
            MakeNavItem(navRT, "nav-progress",     "My Progress", UIHelper.GetChartSprite(), false);
            MakeNavItem(navRT, "nav-certificates", "Certificates",UIHelper.GetMedalSprite(), false);
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

            var iconBox = UIHelper.MakeRect("IconBox", col);
            UIHelper.SetLayout(iconBox.gameObject,
                preferredWidth: 50, minWidth: 50,
                preferredHeight: 50, minHeight: 50);
            var img = iconBox.gameObject.AddComponent<Image>();
            img.sprite         = iconSprite;
            img.color          = activeColor;
            img.preserveAspect = true;

            var lbl = UIHelper.MakeLabel($"label-{name}", col,
                label, 30, activeColor,
                TextAlignmentOptions.Center, bold: active);
            lbl.textWrappingMode = TextWrappingModes.NoWrap;
            UIHelper.SetLayout(lbl.gameObject,
                preferredHeight: 42, minHeight: 38);
        }
    }
}
