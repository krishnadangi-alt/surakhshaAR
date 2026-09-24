using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Scenario Selection Screen Builder (Screen 2 in official reference UI):
    /// Displays Fire & Explosion Response sub-modules:
    /// - 01: Electrical Panel Fire (AR Ready)
    /// - 02: Conveyor Belt Fire
    /// - 03: Excavator / HEMM Fire
    /// Uses photorealistic industrial imagery, rounded cards, high contrast,
    /// and standard bottom navigation.
    /// </summary>
    public static class ScenarioSelectionBuilder
    {
        private static Color Hex(string hex) => UIColors.Hex(hex);

        public static GameObject Build()
        {
            var root = new GameObject("ScenarioSelectionScreen");
            root.AddComponent<RectTransform>();
            var rootImg = root.AddComponent<Image>();
            rootImg.color = Hex("#F8FAFC");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // ── Scrollable Body ──────────────────────────────────────────
            var scrollRoot = UIHelper.MakeRect("ScrollArea", root.transform);
            scrollRoot.anchorMin = Vector2.zero;
            scrollRoot.anchorMax = Vector2.one;
            scrollRoot.offsetMin = new Vector2(0, 140); // Leave room for bottom nav bar
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
            content.sizeDelta = new Vector2(0, 1800);
            scrollRect.content = content;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.padding = new RectOffset(32, 32, 24, 40);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── Top Header ───────────────────────────────────────────────
            BuildTopHeader(content);

            // ── Hero Banner ──────────────────────────────────────────────
            BuildHeroBanner(content);

            // ── Section Title ────────────────────────────────────────────
            BuildSectionHeader(content);

            // ── Sub-module Scenario Cards ────────────────────────────────
            BuildScenarioCards(content);

            // ── Bottom Navigation Bar ────────────────────────────────────
            BuildBottomNav(root.transform);

            return root;
        }

        private static void BuildTopHeader(Transform parent)
        {
            var headerBox = UIHelper.MakeVertical("HeaderBox", parent, 10);
            UIHelper.SetLayout(headerBox.gameObject, flexibleWidth: true, flexWidth: 1);

            // Row 1: SurakshaAR Brand + Lang + Bell + Profile
            var brandRow = UIHelper.MakeHorizontal("BrandRow", headerBox, 14, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(brandRow.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 60, minHeight: 52);
            brandRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var brandLbl = UIHelper.MakeLabel("BrandLbl", brandRow, "Suraksha<color=#16A34A>AR</color>", 38, Hex("#0A192F"), bold: true);
            UIHelper.SetLayout(brandLbl.gameObject, flexibleWidth: true, flexWidth: 1);

            // English dropdown pill
            var langPill = UIHelper.MakeButton("btn-lang-top", brandRow, "🌐 English ▾", 24, Hex("#F1F5F9"), Hex("#334155"), 12);
            UIHelper.SetLayout(langPill.gameObject, preferredWidth: 170, preferredHeight: 46);

            // Bell icon
            var bellBtn = UIHelper.MakeButton("btn-bell", brandRow, "🔔", 28, Hex("#F1F5F9"), Hex("#334155"), 12);
            UIHelper.SetLayout(bellBtn.gameObject, preferredWidth: 50, preferredHeight: 46);

            // Profile icon
            var profBtn = UIHelper.MakeButton("btn-profile", brandRow, "👤", 28, Hex("#F1F5F9"), Hex("#334155"), 12);
            UIHelper.SetLayout(profBtn.gameObject, preferredWidth: 50, preferredHeight: 46);

            // Row 2: Back button + Title
            var titleRow = UIHelper.MakeHorizontal("TitleRow", headerBox, 14, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(titleRow.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 64, minHeight: 56);
            titleRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var backBtn = UIHelper.MakeButton("btn-back", titleRow, "‹", 50, Color.white, Hex("#0A192F"), 18);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 64, preferredHeight: 64);
            var backOutline = backBtn.gameObject.AddComponent<Outline>();
            backOutline.effectColor = Hex("#E2E8F0");
            backOutline.effectDistance = new Vector2(1, -1);

            var titleLbl = UIHelper.MakeLabel("label-title", titleRow, "Fire & Explosion Response", 38, Hex("#0A192F"), bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 54);
        }

        private static void BuildHeroBanner(Transform parent)
        {
            var heroRT = UIHelper.MakeRect("HeroBanner", parent);
            UIHelper.SetLayout(heroRT.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 380, minHeight: 360);

            var heroImg = heroRT.gameObject.AddComponent<Image>();
            var spr = UIHelper.LoadProjectSprite("fire_submodule_banner.jpg")
                   ?? UIHelper.LoadProjectSprite("fire_module_icon.jpg")
                   ?? UIHelper.LoadProjectSprite("fire_intro.jpg");
            if (spr != null)
            {
                heroImg.sprite = spr;
                heroImg.color = Color.white;
            }
            else
            {
                heroImg.color = Hex("#1E293B");
            }
            UIHelper.SetImageRoundedSprite(heroImg, 22);

            // Dark gradient overlay at bottom
            var overlayGO = UIHelper.MakeRect("DarkOverlay", heroRT);
            UIHelper.Stretch(overlayGO, 0, 0, 0, 0);
            var ovImg = overlayGO.gameObject.AddComponent<Image>();
            ovImg.color = new Color(10f / 255f, 25f / 255f, 47f / 255f, 0.78f);
            UIHelper.SetImageRoundedSprite(ovImg, 22);

            // Hero content inside bottom
            var inner = UIHelper.MakeVertical("HeroInner", heroRT, 10, new RectOffset(24, 24, 18, 20));
            UIHelper.Stretch(inner, 0, 0, 0, 0);
            var vlg = inner.GetComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.LowerLeft;
            vlg.childForceExpandHeight = false;

            // Row with Fire Icon & Title
            var iconTitleRow = UIHelper.MakeHorizontal("IconTitleRow", inner, 14, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(iconTitleRow.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 64);
            iconTitleRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var iconBox = UIHelper.MakeRect("FireIconBox", iconTitleRow);
            UIHelper.SetLayout(iconBox.gameObject, preferredWidth: 64, preferredHeight: 64);
            var ibImg = iconBox.gameObject.AddComponent<Image>();
            ibImg.color = Hex("#EA580C");
            UIHelper.SetImageRoundedSprite(ibImg, 16);

            var fireIconSpr = UIHelper.GetFireEmojiSprite();
            if (fireIconSpr != null)
            {
                var fImgGO = UIHelper.MakeRect("FireImg", iconBox);
                UIHelper.Stretch(fImgGO, 8, 8, 8, 8);
                var fi = fImgGO.gameObject.AddComponent<Image>();
                fi.sprite = fireIconSpr;
                fi.preserveAspect = true;
            }
            else
            {
                var iconLbl = UIHelper.MakeLabel("FireIconLbl", iconBox, "🔥", 32, Color.white, TextAlignmentOptions.Center);
                UIHelper.Stretch(iconLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);
            }

            var heroTitle = UIHelper.MakeLabel("HeroTitle", iconTitleRow, "Fire & Explosion\nResponse", 34, Color.white, bold: true);
            heroTitle.lineSpacing = 1.05f;
            UIHelper.SetLayout(heroTitle.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 64);

            // Subtitle
            var heroDesc = UIHelper.MakeLabel("HeroDesc", inner,
                "Learn to identify, prevent and respond to fire and explosion hazards in mining and industrial environments.",
                24, Hex("#E2E8F0"), wrap: true);
            heroDesc.lineSpacing = 1.15f;
            UIHelper.SetLayout(heroDesc.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 64);
        }

        private static void BuildSectionHeader(Transform parent)
        {
            var secCol = UIHelper.MakeVertical("SectionCol", parent, 6);
            UIHelper.SetLayout(secCol.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 52);

            var titleLbl = UIHelper.MakeLabel("label-select-scenario", secCol, "Select a Scenario", 36, Hex("#0A192F"), bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 44);

            // Orange accent line
            var lineGO = UIHelper.MakeRect("AccentLine", secCol);
            UIHelper.SetLayout(lineGO.gameObject, preferredWidth: 52, preferredHeight: 4);
            var lImg = lineGO.gameObject.AddComponent<Image>();
            lImg.color = Hex("#EA580C");
            UIHelper.SetImageRoundedSprite(lImg, 2);
        }

        private static void BuildScenarioCards(Transform parent)
        {
            var col = UIHelper.MakeVertical("ScenarioList", parent, 16);
            UIHelper.SetLayout(col.gameObject, flexibleWidth: true, flexWidth: 1);
            var csf = col.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 01 — Electrical Panel Fire
            BuildScenarioCard(
                parent: col,
                btnName: "card-scenario-1",
                numStr: "01",
                title: "Electrical Panel Fire",
                desc: "Handle fire in electrical panels and control rooms.",
                imgFilename: "scenario_electrical_panel.jpg",
                badgeText: null
            );

            // 02 — Conveyor Belt Fire
            BuildScenarioCard(
                parent: col,
                btnName: "card-scenario-2",
                numStr: "02",
                title: "Conveyor Belt Fire",
                desc: "Respond to fire in conveyor belt systems.",
                imgFilename: "scenario_conveyor_belt.jpg",
                badgeText: null
            );

            // 03 — Excavator / HEMM Fire
            BuildScenarioCard(
                parent: col,
                btnName: "card-scenario-3",
                numStr: "03",
                title: "Excavator / HEMM Fire",
                desc: "Handle fire in heavy earth moving machinery.",
                imgFilename: "scenario_excavator.jpg",
                badgeText: "Intermediate"
            );
        }

        private static void BuildScenarioCard(
            Transform parent,
            string btnName,
            string numStr,
            string title,
            string desc,
            string imgFilename,
            string badgeText)
        {
            var btn = UIHelper.MakeButton(btnName, parent, "", 16, Hex("#1E293B"), Color.white, 20);
            UIHelper.SetLayout(btn.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 200, minHeight: 190);

            // Background Photo
            var bgImg = btn.GetComponent<Image>();
            var spr = UIHelper.LoadProjectSprite(imgFilename);
            if (spr != null)
            {
                bgImg.sprite = spr;
                bgImg.color = Color.white;
            }

            // Dark tint gradient overlay so text is crisp and readable
            var overlay = UIHelper.MakeRect("CardOverlay", btn.transform);
            UIHelper.Stretch(overlay, 0, 0, 0, 0);
            var ovImg = overlay.gameObject.AddComponent<Image>();
            ovImg.color = new Color(10f / 255f, 25f / 255f, 47f / 255f, 0.72f);
            UIHelper.SetImageRoundedSprite(ovImg, 20);
            ovImg.raycastTarget = false;

            // Border
            var outline = btn.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.15f);
            outline.effectDistance = new Vector2(1, -1);

            // Inner Content
            var inner = UIHelper.MakeHorizontal("Inner", btn.transform, 14, new RectOffset(20, 20, 16, 16));
            UIHelper.Stretch(inner, 0, 0, 0, 0);
            inner.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // Left Col: 01 Badge at top-left, Title & Desc at bottom-left
            var leftCol = UIHelper.MakeVertical("LeftCol", inner, 8);
            UIHelper.SetLayout(leftCol.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 168);
            var lvlg = leftCol.GetComponent<VerticalLayoutGroup>();
            lvlg.childAlignment = TextAnchor.UpperLeft;
            lvlg.childForceExpandHeight = false;

            // Scenario Number Badge (Orange rounded square)
            var numBox = UIHelper.MakeRect("NumBadge", leftCol);
            UIHelper.SetLayout(numBox.gameObject, preferredWidth: 54, preferredHeight: 54);
            var nbImg = numBox.gameObject.AddComponent<Image>();
            nbImg.color = Hex("#EA580C");
            UIHelper.SetImageRoundedSprite(nbImg, 14);

            var numLbl = UIHelper.MakeLabel("NumLbl", numBox, numStr, 28, Color.white, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(numLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Title
            var titleLbl = UIHelper.MakeLabel("Title", leftCol, title, 32, Color.white, bold: true, wrap: true);
            titleLbl.lineSpacing = 1.05f;
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 38);

            // Subtitle
            var descLbl = UIHelper.MakeLabel("Desc", leftCol, desc, 24, Hex("#E2E8F0"), wrap: true);
            descLbl.lineSpacing = 1.15f;
            UIHelper.SetLayout(descLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 32);

            // Optional Badge (e.g. Intermediate)
            if (!string.IsNullOrEmpty(badgeText))
            {
                var pill = UIHelper.MakeRect("LevelPill", leftCol);
                UIHelper.SetLayout(pill.gameObject, preferredWidth: 180, preferredHeight: 32);
                var pImg = pill.gameObject.AddComponent<Image>();
                pImg.color = new Color(0, 0, 0, 0.65f);
                UIHelper.SetImageRoundedSprite(pImg, 8);

                var pLbl = UIHelper.MakeLabel("LevelLbl", pill, $"📊 {badgeText}", 22, Hex("#FBBF24"), TextAlignmentOptions.Center, bold: true);
                UIHelper.Stretch(pLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);
            }

            // Right: Orange circular chevron button
            var arrowBox = UIHelper.MakeRect("ArrowBox", inner);
            UIHelper.SetLayout(arrowBox.gameObject, preferredWidth: 56, preferredHeight: 56);
            var abImg = arrowBox.gameObject.AddComponent<Image>();
            abImg.color = Hex("#EA580C");
            UIHelper.SetImageRoundedSprite(abImg, 28); // Perfect circle

            var chevLbl = UIHelper.MakeLabel("Chevron", arrowBox, "›", 46, Color.white, TextAlignmentOptions.Center, bold: true);
            var rt = chevLbl.GetComponent<RectTransform>();
            UIHelper.Stretch(rt, 0, 0, 0, 0);
            rt.anchoredPosition = new Vector2(2, 2); // Optical center
        }

        private static void BuildBottomNav(Transform parent)
        {
            var navBar = UIHelper.MakeRect("BottomNavBar", parent);
            navBar.anchorMin = new Vector2(0, 0);
            navBar.anchorMax = new Vector2(1, 0);
            navBar.pivot     = new Vector2(0.5f, 0);
            navBar.sizeDelta = new Vector2(0, 140);

            var bg = navBar.gameObject.AddComponent<Image>();
            bg.color = Color.white;

            var topBorder = UIHelper.MakeRect("TopBorder", navBar);
            topBorder.anchorMin = new Vector2(0, 1);
            topBorder.anchorMax = new Vector2(1, 1);
            topBorder.pivot     = new Vector2(0.5f, 1);
            topBorder.sizeDelta = new Vector2(0, 1.5f);
            var tbImg = topBorder.gameObject.AddComponent<Image>();
            tbImg.color = Hex("#E2E8F0");

            var hlg = navBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth  = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth      = true;
            hlg.childControlHeight     = true;
            hlg.padding = new RectOffset(0, 0, 8, 20);

            MakeNavItem(navBar, "nav-home",         "Home",         UIHelper.GetHomeSprite(),  true);
            MakeNavItem(navBar, "nav-learn",        "Learn",        UIHelper.GetBookSprite(),  false);
            MakeNavItem(navBar, "nav-progress",     "My Progress",  UIHelper.GetChartSprite(), false);
            MakeNavItem(navBar, "nav-certificates", "Certificates", UIHelper.GetMedalSprite(), false);
        }

        private static void MakeNavItem(Transform parent, string name, string label, Sprite iconSprite, bool active)
        {
            var activeColor = active ? Hex("#16A34A") : Hex("#334155");

            var btn = UIHelper.MakeButton(name, parent, "", 14, UIColors.Transparent, Color.white, 0);

            var col = UIHelper.MakeVertical("Col", btn.transform, 6, new RectOffset(0, 0, 4, 4), childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(col, 0, 0, 0, 0);
            col.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var iconBox = UIHelper.MakeRect("IconBox", col);
            UIHelper.SetLayout(iconBox.gameObject, preferredWidth: 46, minWidth: 46, preferredHeight: 46, minHeight: 46);
            var img = iconBox.gameObject.AddComponent<Image>();
            img.sprite = iconSprite;
            img.color  = activeColor;
            img.preserveAspect = true;

            var lbl = UIHelper.MakeLabel($"label-{name}", col, label, 24, activeColor, TextAlignmentOptions.Center, bold: active);
            UIHelper.SetLayout(lbl.gameObject, preferredWidth: 140, preferredHeight: 30);
        }
    }
}
