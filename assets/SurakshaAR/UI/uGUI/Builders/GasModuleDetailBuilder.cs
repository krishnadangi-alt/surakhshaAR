using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Gas Scenario Detail Screen — reference-accurate full-screen rebuild.
    ///
    /// Unified typography matching Learn (ModuleSelection) &amp; Progress screens (CanvasScaler 1080×2400):
    ///   Header title   = 48px bold
    ///   Scenario title = 46px bold   (matches Learn/Progress card titles)
    ///   Scenario desc  = 34px        (matches Learn/Progress descriptions)
    ///   Hero badge     = 32px bold   ("01", "02", "03" scenario number)
    ///   Chip value     = 30px bold
    ///   Chip label     = 24px
    ///   Section heads  = 44px bold   (matches Progress section headings)
    ///   Overview body  = 34px        (matches Learn/Progress body text)
    ///   Step text      = 34px        (matches Learn/Progress body text)
    ///   Step number    = 26px bold
    ///   Start button   = 36px bold
    ///   Bottom nav     = 30px bold
    ///
    /// All Gas scenarios are COMING SOON.
    /// </summary>
    public static class GasModuleDetailBuilder
    {
        private const float NAV_H = 165f;

        private static readonly Color AccentBlue   = UIColors.Hex("#0284C7");
        private static readonly Color AccentLight  = UIColors.Hex("#F0F9FF");
        private static readonly Color AccentBorder = UIColors.Hex("#BAE6FD");
        private static readonly Color NavyText     = UIColors.Hex("#0A192F");
        private static readonly Color DarkText     = UIColors.Hex("#0F172A");
        private static readonly Color SlateText    = UIColors.Hex("#334155");
        private static readonly Color MutedText    = UIColors.Hex("#475569");
        private static readonly Color SubText      = UIColors.Hex("#64748B");

        private static Color Hex(string h) => UIColors.Hex(h);

        public static GameObject Build()
        {
            var root = new GameObject("GasModuleDetailScreen");
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            var rootImg = root.AddComponent<Image>();
            rootImg.color  = Hex("#F8FAFC");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // ── 1. Fixed Bottom Navigation Bar (matches Learn/Progress/Home) ──
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
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot     = new Vector2(0.5f, 1);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            sr.content        = content;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing                = 24;
            vlg.padding                = new RectOffset(30, 30, 18, 48);
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth      = true;
            vlg.childControlHeight     = true; // Required for LayoutElements to drive heights

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            BuildTopHeader(content);
            BuildHeroIconArea(content);
            BuildScenarioTitleRow(content);
            BuildStatChips(content);
            BuildOverviewSection(content);
            BuildTrainingStepsList(content);
            BuildStartButton(content);

            return root;
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildTopHeader(Transform parent)
        {
            var row = UIHelper.MakeHorizontal("TopHeaderRow", parent, 14,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 80, minHeight: 74);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var backBtn = UIHelper.MakeButton("btn-back", row,
                "‹", 52, Color.white, NavyText, 20);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 72, preferredHeight: 72);
            var bOutline = backBtn.gameObject.AddComponent<Outline>();
            bOutline.effectColor    = Hex("#E2E8F0");
            bOutline.effectDistance = new Vector2(1, -1);

            var titleLbl = UIHelper.MakeLabel("label-title", row,
                "Underground Gas Release", 48, NavyText, bold: true);
            titleLbl.textWrappingMode = TextWrappingModes.NoWrap;
            titleLbl.overflowMode     = TextOverflowModes.Ellipsis;
            UIHelper.SetLayout(titleLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 60);

            var langPill = UIHelper.MakeButton("btn-language-picker", row,
                "English", 26, Hex("#F1F5F9"), SlateText, 14);
            UIHelper.SetLayout(langPill.gameObject, preferredWidth: 160, preferredHeight: 60);

            var bellBtn = UIHelper.MakeButton("btn-bell", row, "", 14,
                Color.white, Color.white, 20);
            UIHelper.SetLayout(bellBtn.gameObject, preferredWidth: 60, preferredHeight: 60);
            bellBtn.gameObject.AddComponent<Outline>().effectColor = Hex("#E2E8F0");
            var bellGO = UIHelper.MakeRect("BellIcon", bellBtn.transform);
            UIHelper.Stretch(bellGO, 14, 14, 14, 14);
            var bellImg = bellGO.gameObject.AddComponent<Image>();
            bellImg.sprite        = UIHelper.GetBellSprite();
            bellImg.preserveAspect = true;

            var profBtn = UIHelper.MakeButton("btn-profile", row, "", 14,
                Color.white, Color.white, 20);
            UIHelper.SetLayout(profBtn.gameObject, preferredWidth: 60, preferredHeight: 60);
            profBtn.gameObject.AddComponent<Outline>().effectColor = Hex("#E2E8F0");
            var profGO = UIHelper.MakeRect("ProfIcon", profBtn.transform);
            UIHelper.Stretch(profGO, 14, 14, 14, 14);
            var profImg = profGO.gameObject.AddComponent<Image>();
            profImg.sprite        = UIHelper.GetProfileSprite();
            profImg.preserveAspect = true;
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildHeroIconArea(Transform parent)
        {
            var heroCard = UIHelper.MakeRect("HeroPhotoContainer", parent);
            UIHelper.SetLayout(heroCard.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 480, minHeight: 460);

            var heroImg = heroCard.gameObject.AddComponent<Image>();
            heroImg.color  = AccentLight;
            heroImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(heroImg, 26);

            var border = heroCard.gameObject.AddComponent<Outline>();
            border.effectColor    = AccentBorder;
            border.effectDistance = new Vector2(1.5f, -1.5f);

            var shadow = heroCard.gameObject.AddComponent<Shadow>();
            shadow.effectColor    = new Color(0, 0, 0, 0.05f);
            shadow.effectDistance = new Vector2(0, -3f);

            var iconGO = UIHelper.MakeRect("ScenarioIconImg", heroCard);
            iconGO.anchorMin = new Vector2(0.5f, 0.5f);
            iconGO.anchorMax = new Vector2(0.5f, 0.5f);
            iconGO.pivot     = new Vector2(0.5f, 0.5f);
            iconGO.sizeDelta = new Vector2(440, 380);
            var sIconImg = iconGO.gameObject.AddComponent<Image>();
            sIconImg.preserveAspect = true;
            var spr = UIHelper.LoadProjectSprite("icon_gas_scenario.jpg");
            if (spr != null)
            {
                sIconImg.sprite = spr;
                sIconImg.color  = Color.white;
            }
            else
            {
                sIconImg.color = AccentBlue;
            }

            // Scenario number badge at top-left ("01", "02", "03" matching reference image)
            var numBadge = UIHelper.MakeRect("HeroNumBadge", heroCard);
            numBadge.anchorMin        = new Vector2(0, 1);
            numBadge.anchorMax        = new Vector2(0, 1);
            numBadge.pivot            = new Vector2(0, 1);
            numBadge.anchoredPosition = new Vector2(24, -24);
            numBadge.sizeDelta        = new Vector2(92, 58);

            var nbImg = numBadge.gameObject.AddComponent<Image>();
            nbImg.color  = AccentBlue;
            nbImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(nbImg, 14);

            var nbLbl = UIHelper.MakeLabel("label-hero-badge", numBadge,
                "01", 32, Color.white,
                TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(nbLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildScenarioTitleRow(Transform parent)
        {
            var col = UIHelper.MakeVertical("ScenarioMeta", parent, 6,
                childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(col.gameObject,
                flexibleWidth: true, flexWidth: 1,
                minHeight: 114);

            var cvlg = col.GetComponent<VerticalLayoutGroup>();
            cvlg.childForceExpandHeight = false;
            var csf = col.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var row = UIHelper.MakeHorizontal("TitleRow", col.transform, 14,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject,
                flexibleWidth: true, flexWidth: 1,
                minHeight: 58);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var badge = UIHelper.MakeRect("GasBox", row);
            UIHelper.SetLayout(badge.gameObject, preferredWidth: 54, preferredHeight: 54);
            var bImg = badge.gameObject.AddComponent<Image>();
            bImg.color  = AccentBlue;
            bImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(bImg, 14);

            var gasLbl = UIHelper.MakeLabel("GasLbl", badge,
                "⛽", 32, Color.white, TextAlignmentOptions.Center);
            UIHelper.Stretch(gasLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            var titleLbl = UIHelper.MakeLabel("label-scenario-title", row,
                "Underground Gas Release", 46, DarkText, bold: true, wrap: true);
            titleLbl.lineSpacing = 1.05f;
            UIHelper.SetLayout(titleLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, minHeight: 56);

            var descLbl = UIHelper.MakeLabel("label-scenario-desc", col.transform,
                "Recognize a gas release, raise the alarm and move to a safe area.",
                34, MutedText, wrap: true);
            descLbl.lineSpacing = 1.15f;
            UIHelper.SetLayout(descLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, minHeight: 48);
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildStatChips(Transform parent)
        {
            var row = UIHelper.MakeHorizontal("StatChipsRow", parent, 14,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 104, minHeight: 98);

            BuildChip(row, "chip-duration", "⏱",  "~ 10 mins",    "Duration");
            BuildChip(row, "chip-level",    "📊", "Beginner",      "Level");
            BuildChip(row, "chip-focus",    "⚡", "Gas Detector",  "Use");
        }

        private static void BuildChip(Transform parent, string name,
            string icon, string value, string label)
        {
            var chip = UIHelper.MakeRect(name, parent);
            UIHelper.SetLayout(chip.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 100);

            var cImg = chip.gameObject.AddComponent<Image>();
            cImg.color  = Hex("#F8FAFC");
            cImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cImg, 16);

            var border = chip.gameObject.AddComponent<Outline>();
            border.effectColor    = Hex("#E2E8F0");
            border.effectDistance = new Vector2(1, -1);

            var innerRow = UIHelper.MakeHorizontal("Inner", chip, 10,
                new RectOffset(12, 10, 10, 10),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(innerRow, 0, 0, 0, 0);
            innerRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var iconLbl = UIHelper.MakeLabel("Icon", innerRow,
                icon, 32, AccentBlue, TextAlignmentOptions.Center);
            UIHelper.SetLayout(iconLbl.gameObject, preferredWidth: 40, preferredHeight: 40);

            var textCol = UIHelper.MakeVertical("TextCol", innerRow, 0,
                childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(textCol.gameObject, flexibleWidth: true, flexWidth: 1);
            textCol.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;

            var valLbl = UIHelper.MakeLabel("Value", textCol,
                value, 30, DarkText, bold: true);
            UIHelper.SetLayout(valLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 38);

            var subLbl = UIHelper.MakeLabel("Sub", textCol,
                label, 24, SubText);
            UIHelper.SetLayout(subLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 30);
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildOverviewSection(Transform parent)
        {
            var box = UIHelper.MakeVertical("OverviewBox", parent, 10,
                childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(box.gameObject,
                flexibleWidth: true, flexWidth: 1,
                minHeight: 160);

            var csf = box.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var headRow = UIHelper.MakeHorizontal("HeadRow", box.transform, 12,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(headRow.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 48);
            headRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var bar = UIHelper.MakeRect("AccentBar", headRow);
            UIHelper.SetLayout(bar.gameObject, preferredWidth: 6, preferredHeight: 38);
            var barImg = bar.gameObject.AddComponent<Image>();
            barImg.color  = AccentBlue;
            barImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(barImg, 3);

            var headLbl = UIHelper.MakeLabel("OverviewHead", headRow,
                "Scenario Overview", 44, DarkText, bold: true);
            UIHelper.SetLayout(headLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 48);

            var bodyLbl = UIHelper.MakeLabel("label-overview-body", box.transform,
                "A gas leak has been detected in an underground mine area. Learn to identify the leak, raise the alarm, communicate the emergency and move to a safe area following proper procedures.",
                34, SlateText, wrap: true);
            bodyLbl.lineSpacing = 1.25f;
            UIHelper.SetLayout(bodyLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, minHeight: 80);
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildTrainingStepsList(Transform parent)
        {
            var box = UIHelper.MakeVertical("TrainingStepsBox", parent, 14,
                childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(box.gameObject,
                flexibleWidth: true, flexWidth: 1,
                minHeight: 400);

            var csf = box.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var headRow = UIHelper.MakeHorizontal("HeadRow", box.transform, 12,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(headRow.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 48);
            headRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var bar = UIHelper.MakeRect("Bar", headRow);
            UIHelper.SetLayout(bar.gameObject, preferredWidth: 6, preferredHeight: 38);
            var barImg = bar.gameObject.AddComponent<Image>();
            barImg.color  = AccentBlue;
            barImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(barImg, 3);

            var headLbl = UIHelper.MakeLabel("TrainingHead", headRow,
                "Training will include", 44, DarkText, bold: true);
            UIHelper.SetLayout(headLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 48);

            var stepsCol = UIHelper.MakeVertical("steps-list", box.transform, 10,
                childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(stepsCol.gameObject,
                flexibleWidth: true, flexWidth: 1);
            var scCsf = stepsCol.gameObject.AddComponent<ContentSizeFitter>();
            scCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            string[] defaultSteps =
            {
                "Identify gas leak signs and hazard area",
                "Activate alarm and inform control room",
                "Use gas detector and interpret readings",
                "Follow safe withdrawal procedure",
                "Follow ventilation and evacuation route",
                "Maintain safe distance and move to safe area"
            };

            for (int i = 0; i < defaultSteps.Length; i++)
                MakeStepItem(stepsCol.transform, i + 1, defaultSteps[i]);
        }

        /// <summary>Called at runtime by GasModuleDetailController to swap steps.</summary>
        public static void MakeStepItem(Transform parent, int stepNum, string stepText)
        {
            var row = UIHelper.MakeHorizontal($"Step_{stepNum}", parent, 16,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject,
                flexibleWidth: true, flexWidth: 1,
                minHeight: 58);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var csf = row.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var badge = UIHelper.MakeRect("Badge", row);
            UIHelper.SetLayout(badge.gameObject,
                preferredWidth: 48, minWidth: 48,
                preferredHeight: 48, minHeight: 48);
            var bImg = badge.gameObject.AddComponent<Image>();
            bImg.color  = AccentBlue;
            bImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(bImg, 24);

            var numLbl = UIHelper.MakeLabel("Num", badge,
                stepNum.ToString(), 26, Color.white,
                TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(numLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            var lbl = UIHelper.MakeLabel("StepLbl", row,
                stepText, 34, DarkText, wrap: true);
            lbl.lineSpacing = 1.15f;
            UIHelper.SetLayout(lbl.gameObject,
                flexibleWidth: true, flexWidth: 1, minHeight: 48);
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildStartButton(Transform parent)
        {
            var btnRow = UIHelper.MakeRect("StartBtnRow", parent);
            UIHelper.SetLayout(btnRow.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 96, minHeight: 90);

            // Coming Soon button (disabled gray button)
            var btn = UIHelper.MakeButton("btn-start-ar-training", btnRow,
                "Coming Soon", 36, Hex("#94A3B8"), Color.white, 20);
            UIHelper.Stretch(btn.GetComponent<RectTransform>(), 0, 0, 0, 0);

            var btnLbl = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnLbl != null) btnLbl.fontStyle = FontStyles.Bold;
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildBottomNav(Transform parent)
        {
            var navRT = UIHelper.MakeRect("BottomNavBar", parent);
            navRT.anchorMin = Vector2.zero;
            navRT.anchorMax = new Vector2(1, 0);
            navRT.pivot     = new Vector2(0.5f, 0f);
            navRT.sizeDelta = new Vector2(0, NAV_H);
            navRT.anchoredPosition = Vector2.zero;

            var bg = navRT.gameObject.AddComponent<Image>();
            bg.color  = Color.white;
            bg.sprite = UIHelper.GetWhiteSprite();

            var topBorder = UIHelper.MakeRect("TopBorder", navRT);
            topBorder.anchorMin = new Vector2(0, 1);
            topBorder.anchorMax = new Vector2(1, 1);
            topBorder.pivot     = new Vector2(0.5f, 1f);
            topBorder.sizeDelta = new Vector2(0, 1.5f);
            topBorder.anchoredPosition = Vector2.zero;
            topBorder.gameObject.AddComponent<Image>().color = Hex("#E2E8F0");
            topBorder.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

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

            var btn = UIHelper.MakeButton(name, parent, "", 14,
                UIColors.Transparent, Color.white, 0);

            var col = UIHelper.MakeVertical("Col", btn.transform, 6,
                new RectOffset(0, 0, 4, 4),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(col, 0, 0, 0, 0);
            col.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var iconBox = UIHelper.MakeRect("IconBox", col);
            UIHelper.SetLayout(iconBox.gameObject,
                preferredWidth: 50, minWidth: 50, preferredHeight: 50, minHeight: 50);
            var iconImg = iconBox.gameObject.AddComponent<Image>();
            iconImg.sprite        = iconSprite;
            iconImg.color         = activeColor;
            iconImg.preserveAspect = true;

            var lbl = UIHelper.MakeLabel($"label-{name}", col, label,
                fontSize, activeColor, TextAlignmentOptions.Center, bold: true);
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
