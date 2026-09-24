using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Progress Screen Builder matching Reference 1:
    /// "Training Progress — Track Your Learning Journey"
    /// Features:
    /// - Forest Emerald (#0A3C36) header with back arrow, title, subtitle.
    /// - Large summary card: "X of Y Modules Completed" + green percentage + progress bar.
    /// - "Module Status Breakdown" section: per-module rows (icon, title, desc, score•status, arrow).
    /// - "Knowledge Retention Tracking" section: table card with Module | Last Assessment | Retention Score.
    /// - 4-tab bottom navigation (active: Progress).
    /// </summary>
    public static class ProgressBuilder
    {
        private const float NAV_H = 204f;
        private const float HDR_H = 210f;
        private static Color Hex(string h) => UIColors.Hex(h);

        public static GameObject Build()
        {
            var root   = new GameObject("ProgressScreen");
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;
            rootRT.sizeDelta = Vector2.zero;

            var rootImg = root.AddComponent<Image>();
            rootImg.color  = Hex("#F8FAFC");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // ── 1. Bottom Navigation ──────────────────────────────────────
            MakeBottomNav(root.transform);

            // ── 2. Forest Emerald Header ──────────────────────────────────
            var header = UIHelper.MakeRect("Header", root.transform);
            header.anchorMin = new Vector2(0, 1);
            header.anchorMax = Vector2.one;
            header.offsetMin = new Vector2(0, -HDR_H);
            header.offsetMax = Vector2.zero;

            var headerImg = header.gameObject.AddComponent<Image>();
            headerImg.color  = Hex("#0A3C36");
            headerImg.sprite = UIHelper.GetWhiteSprite();

            var headerRow = UIHelper.MakeHorizontal("Row", header, 20,
                new RectOffset(32, 32, 28, 24),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(headerRow, 0, 0, 0, 0);
            headerRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var backBtn = UIHelper.MakeButton("btn-back", headerRow, "‹", 48,
                new Color(1, 1, 1, 0.18f), Color.white, 24);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 76, minWidth: 76,
                preferredHeight: 76, minHeight: 76);

            var titleCol = UIHelper.MakeVertical("TitleCol", headerRow, 6);
            UIHelper.SetLayout(titleCol.gameObject, flexibleWidth: true, flexWidth: 1);
            titleCol.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var titleLbl = UIHelper.MakeLabel("label-title", titleCol,
                "Training Progress", 52, Color.white, TextAlignmentOptions.Left, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, preferredHeight: 64, minHeight: 60);

            var subLbl = UIHelper.MakeLabel("label-sub", titleCol,
                "Track Your Learning Journey", 32, Hex("#A7F3D0"), TextAlignmentOptions.Left);
            UIHelper.SetLayout(subLbl.gameObject, preferredHeight: 40, minHeight: 38);

            // ── 3. Scrollable Body ────────────────────────────────────────
            var scrollRoot = UIHelper.MakeRect("ScrollArea", root.transform);
            scrollRoot.anchorMin = Vector2.zero;
            scrollRoot.anchorMax = Vector2.one;
            scrollRoot.offsetMin = new Vector2(0, NAV_H);
            scrollRoot.offsetMax = new Vector2(0, -HDR_H);

            var sr = scrollRoot.gameObject.AddComponent<ScrollRect>();
            sr.horizontal        = false;
            sr.vertical          = true;
            sr.movementType      = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 40f;

            var viewport = UIHelper.MakeRect("Viewport", scrollRoot);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewport.gameObject.AddComponent<RectMask2D>();
            sr.viewport = viewport;

            var content = UIHelper.MakeRect("Content", viewport);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot     = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;
            sr.content        = content;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing                = 28;
            vlg.padding                = new RectOffset(28, 28, 28, 48);
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth      = true;
            vlg.childControlHeight     = true;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── Overall Completion Summary Card ───────────────────────────
            BuildSummaryCard(content);

            // ── Section: Module Status Breakdown ──────────────────────────
            var secBreakdown = UIHelper.MakeLabel("label-sec-breakdown", content,
                "Module Status Breakdown", 44, UIColors.TextPrimary, bold: true);
            UIHelper.SetLayout(secBreakdown.gameObject, preferredHeight: 56, minHeight: 54);

            // Module rows (names/icons set by controller at runtime)
            BuildModuleRow(content, "row-fire",       "Fire & Explosion Response",
                "Learn to identify, respond and control fire hazards.", UIHelper.GetFireEmojiSprite(), Hex("#FFEDD5"));
            BuildModuleRow(content, "row-gas",        "Gas Leak & Confined Space",
                "Stay safe in hazardous gas environments.", UIHelper.GetGasEmojiSprite(), Hex("#E0F2FE"));
            BuildModuleRow(content, "row-machinery",  "Machinery Safety",
                "Identify machinery, understand risks and follow safe procedures.", UIHelper.GetGearEmojiSprite(), Hex("#DCFCE7"));
            BuildModuleRow(content, "row-electrical", "Electrical Safety",
                "Learn electrical safety practices for mining environments.", UIHelper.GetBoltSprite(), Hex("#FEF3C7"),
                locked: true, iconColor: Hex("#D97706"));
            BuildModuleRow(content, "row-mine-hazard","Mine Hazard & Environment",
                "Understand mine hazards and environmental risks.", UIHelper.GetHardHatSprite(), Hex("#FFEDD5"),
                locked: true, iconColor: Hex("#EA580C"));

            // ── Section: Knowledge Retention Tracking ─────────────────────
            var secRetention = UIHelper.MakeLabel("label-sec-retention", content,
                "Knowledge Retention Tracking", 44, UIColors.TextPrimary, bold: true);
            UIHelper.SetLayout(secRetention.gameObject, preferredHeight: 56, minHeight: 54);

            BuildRetentionTable(content);

            return root;
        }

        // =================================================================
        // SUMMARY CARD
        // =================================================================
        private static void BuildSummaryCard(Transform parent)
        {
            var card = new GameObject("SummaryCard");
            card.transform.SetParent(parent, false);
            UIHelper.SetLayout(card, preferredHeight: 330, minHeight: 310);

            var cardImg = card.AddComponent<Image>();
            cardImg.color  = Color.white;
            cardImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cardImg, 26);

            var shadow = card.AddComponent<Shadow>();
            shadow.effectColor    = new Color(0, 0, 0, 0.06f);
            shadow.effectDistance = new Vector2(0, -3f);

            var inner = UIHelper.MakeVertical("Inner", card.transform, 20,
                new RectOffset(32, 32, 28, 28));
            UIHelper.Stretch(inner, 0, 0, 0, 0);
            inner.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = true;

            // Top row: "X of Y Modules Completed" + percent label
            var headRow = UIHelper.MakeHorizontal("HeadRow", inner, 12,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(headRow.gameObject, preferredHeight: 64, minHeight: 58);
            headRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var sumTitle = UIHelper.MakeLabel("label-modules-summary", headRow,
                "0 of 0 Modules Completed", 44, UIColors.TextPrimary,
                TextAlignmentOptions.Left, bold: true);
            sumTitle.textWrappingMode = TextWrappingModes.Normal;
            UIHelper.SetLayout(sumTitle.gameObject, flexibleWidth: true, flexWidth: 1,
                preferredHeight: 60, minHeight: 48);

            var pctLbl = UIHelper.MakeLabel("label-percentage", headRow,
                "0%", 64, Hex("#059669"), TextAlignmentOptions.Right, bold: true);
            UIHelper.SetLayout(pctLbl.gameObject,
                preferredWidth: 150, minWidth: 120, preferredHeight: 66, minHeight: 52);

            // Progress bar track
            var track = UIHelper.MakeRect("Track", inner);
            UIHelper.SetLayout(track.gameObject, preferredHeight: 24, minHeight: 24);
            var trackImg = track.gameObject.AddComponent<Image>();
            trackImg.color  = Hex("#E2E8F0");
            trackImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(trackImg, 12);

            var fill = UIHelper.MakeRect("progress-bar-fill", track);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0f, 1f);   // controller sets anchorMax.x = pct/100
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            var fillImg = fill.gameObject.AddComponent<Image>();
            fillImg.color  = Hex("#059669");
            fillImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(fillImg, 12);

            // Subtitle
            var subTip = UIHelper.MakeLabel("label-summary-tip", inner,
                "Complete all modules to build a safer and stronger tomorrow.",
                34, UIColors.TextSecondary, TextAlignmentOptions.Left);
            subTip.lineSpacing = 3f;
            subTip.textWrappingMode = TextWrappingModes.Normal;
            UIHelper.SetLayout(subTip.gameObject, preferredHeight: 88, minHeight: 64);
        }

        // =================================================================
        // MODULE STATUS ROW
        // =================================================================
        private static void BuildModuleRow(
            Transform parent, string rowName,
            string title, string desc,
            Sprite iconSprite, Color iconBg,
            bool locked = false, Color? iconColor = null)
        {
            var item = new GameObject(rowName);
            item.transform.SetParent(parent, false);
            UIHelper.SetLayout(item, preferredHeight: 260, minHeight: 220);

            var itemImg = item.AddComponent<Image>();
            itemImg.color  = locked ? Hex("#F8FAFC") : Color.white;
            itemImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(itemImg, 24);

            var outline = item.AddComponent<Outline>();
            outline.effectColor    = Hex("#E2E8F0");
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var inner = UIHelper.MakeHorizontal("Inner", item.transform, 18,
                new RectOffset(22, 22, 18, 18),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(inner, 0, 0, 0, 0);
            inner.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // Icon box (104×104)
            var iconBox = UIHelper.MakeRect("IconBox", inner);
            UIHelper.SetLayout(iconBox.gameObject,
                preferredWidth: 104, minWidth: 104, preferredHeight: 104, minHeight: 104);
            var iconBoxImg = iconBox.gameObject.AddComponent<Image>();
            iconBoxImg.color  = iconBg;
            UIHelper.SetImageRoundedSprite(iconBoxImg, 20);

            if (iconSprite != null)
            {
                var iconGO  = UIHelper.MakeRect("Icon", iconBox);
                UIHelper.AnchorCenter(iconGO, 60, 60);
                var iconImg = iconGO.gameObject.AddComponent<Image>();
                iconImg.sprite         = iconSprite;
                iconImg.preserveAspect = true;
                iconImg.color          = iconColor.HasValue ? iconColor.Value : Color.white;
                iconImg.raycastTarget  = false;
            }

            // Text column
            var textCol = UIHelper.MakeVertical("TextCol", inner, 6);
            UIHelper.SetLayout(textCol.gameObject, flexibleWidth: true, flexWidth: 1,
                preferredHeight: 190, minHeight: 130);
            textCol.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var titleColor = Hex("#0F172A");
            var descColor  = Hex("#64748B");

            var titleLbl = UIHelper.MakeLabel($"label-title-{rowName}", textCol,
                title, 42, titleColor, TextAlignmentOptions.Left, bold: true);
            titleLbl.lineSpacing = 2f;
            titleLbl.textWrappingMode = TextWrappingModes.Normal;
            UIHelper.SetLayout(titleLbl.gameObject, preferredHeight: 68, minHeight: 48);

            var descLbl = UIHelper.MakeLabel($"label-desc-{rowName}", textCol,
                desc, 32, descColor, TextAlignmentOptions.Left);
            descLbl.lineSpacing = 3f;
            descLbl.textWrappingMode = TextWrappingModes.Normal;
            UIHelper.SetLayout(descLbl.gameObject, preferredHeight: 88, minHeight: 56);

            // Right side: status + chevron horizontal row
            var rightRow = UIHelper.MakeHorizontal("RightRow", inner, 12,
                new RectOffset(0, 0, 0, 0),
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(rightRow.gameObject, preferredWidth: 270, minWidth: 210,
                preferredHeight: 60);
            rightRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleRight;

            if (locked)
            {
                var notStartedLbl = UIHelper.MakeLabel($"label-status-{rowName}", rightRow,
                    "Not Started", 30, Hex("#64748B"), TextAlignmentOptions.Right, bold: true);
                UIHelper.SetLayout(notStartedLbl.gameObject, preferredHeight: 42, minHeight: 40);
            }
            else
            {
                // Placeholder — will be overwritten by controller with real data
                var statusLbl = UIHelper.MakeLabel($"label-status-{rowName}", rightRow,
                    "—", 30, Hex("#059669"), TextAlignmentOptions.Right, bold: true);
                UIHelper.SetLayout(statusLbl.gameObject, preferredHeight: 42, minHeight: 40);
            }

            // Arrow chevron
            var arrowColor = Hex("#94A3B8");
            var arrowLbl   = UIHelper.MakeLabel($"arrow-{rowName}", rightRow,
                ">", 44, arrowColor, TextAlignmentOptions.Right, bold: false);
            UIHelper.SetLayout(arrowLbl.gameObject,
                preferredWidth: 28, minWidth: 24, preferredHeight: 42, minHeight: 40);
        }

        // =================================================================
        // KNOWLEDGE RETENTION TABLE
        // =================================================================
        private static void BuildRetentionTable(Transform parent)
        {
            var card = new GameObject("RetentionCard");
            card.transform.SetParent(parent, false);
            UIHelper.SetLayout(card, preferredHeight: 620, minHeight: 500);

            var cardImg = card.AddComponent<Image>();
            cardImg.color  = Color.white;
            cardImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cardImg, 26);

            var shadow = card.AddComponent<Shadow>();
            shadow.effectColor    = new Color(0, 0, 0, 0.06f);
            shadow.effectDistance = new Vector2(0, -3f);

            var tableCol = UIHelper.MakeVertical("TableCol", card.transform, 0,
                new RectOffset(32, 32, 28, 28));
            UIHelper.Stretch(tableCol, 0, 0, 0, 0);
            tableCol.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = true;

            // Column header row
            BuildRetentionHeaderRow(tableCol);

            // Divider
            BuildDivider(tableCol, Hex("#E2E8F0"));

            // Data rows — labels named so controller can find and update them
            BuildRetentionDataRow(tableCol, "Fire & Explosion Response",
                "label-ret-date-fire",   "label-ret-score-fire");
            BuildRetentionDataRow(tableCol, "Gas Leak & Confined Space",
                "label-ret-date-gas",    "label-ret-score-gas");
            BuildRetentionDataRow(tableCol, "Machinery Safety",
                "label-ret-date-machinery", "label-ret-score-machinery");
            BuildRetentionDataRow(tableCol, "Electrical Safety",
                "label-ret-date-electrical", "label-ret-score-electrical",
                locked: true);
            BuildRetentionDataRow(tableCol, "Mine Hazard & Environment",
                "label-ret-date-mine",   "label-ret-score-mine",
                locked: true);
        }

        private static void BuildRetentionHeaderRow(Transform parent)
        {
            var row = UIHelper.MakeHorizontal("HeaderRow", parent, 12,
                new RectOffset(0, 0, 0, 16),
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject, preferredHeight: 54, minHeight: 52);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var modLbl = UIHelper.MakeLabel("HeaderModule", row,
                "Module", 32, Hex("#64748B"), TextAlignmentOptions.Left, bold: true);
            UIHelper.SetLayout(modLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 40);

            var dateLbl = UIHelper.MakeLabel("HeaderDate", row,
                "Last Assessment", 32, Hex("#64748B"), TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(dateLbl.gameObject, preferredWidth: 260, minWidth: 220, preferredHeight: 40);

            var scoreLbl = UIHelper.MakeLabel("HeaderScore", row,
                "Retention Score", 32, Hex("#059669"), TextAlignmentOptions.Right, bold: true);
            UIHelper.SetLayout(scoreLbl.gameObject, preferredWidth: 260, minWidth: 220, preferredHeight: 40);
        }

        private static void BuildRetentionDataRow(
            Transform parent, string moduleName,
            string dateLabelName, string scoreLabelName,
            bool locked = false)
        {
            BuildDivider(parent, Hex("#F1F5F9"));

            var row = UIHelper.MakeHorizontal($"Row-{dateLabelName}", parent, 8,
                new RectOffset(0, 0, 18, 18),
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject, preferredHeight: 108, minHeight: 84);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var nameColor = locked ? Hex("#94A3B8") : Hex("#1E293B");

            var modLbl = UIHelper.MakeLabel($"label-ret-module-{dateLabelName}", row,
                moduleName, 32, nameColor, TextAlignmentOptions.Left);
            modLbl.textWrappingMode = TextWrappingModes.Normal;
            UIHelper.SetLayout(modLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 76);

            // Date cell
            var dateLbl = UIHelper.MakeLabel(dateLabelName, row,
                locked ? "–" : "Not Available", 28, Hex("#64748B"), TextAlignmentOptions.Center);
            UIHelper.SetLayout(dateLbl.gameObject, preferredWidth: 260, minWidth: 220, preferredHeight: 40);

            // Score cell
            var scoreColor = locked ? Hex("#94A3B8") : Hex("#94A3B8");
            var scoreLbl   = UIHelper.MakeLabel(scoreLabelName, row,
                "Not Available", 30, scoreColor, TextAlignmentOptions.Right, bold: true);
            UIHelper.SetLayout(scoreLbl.gameObject, preferredWidth: 260, minWidth: 220, preferredHeight: 40);
        }

        private static void BuildDivider(Transform parent, Color color)
        {
            var divRT = UIHelper.MakeRect("Divider", parent);
            UIHelper.SetLayout(divRT.gameObject, preferredHeight: 1.5f, minHeight: 1.5f);
            var divImg = divRT.gameObject.AddComponent<Image>();
            divImg.color  = color;
            divImg.sprite = UIHelper.GetWhiteSprite();
        }

        // =================================================================
        // BOTTOM NAVIGATION
        // =================================================================
        private static void MakeBottomNav(Transform parent)
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

            MakeNavItem(navRT, "nav-home",         "Home",         UIHelper.GetHomeSprite(),  false);
            MakeNavItem(navRT, "nav-learn",        "Learn",        UIHelper.GetBookSprite(),  false);
            MakeNavItem(navRT, "nav-progress",     "My Progress",  UIHelper.GetChartSprite(), true);
            MakeNavItem(navRT, "nav-certificates", "Certificates", UIHelper.GetMedalSprite(), false);
        }

        private static void MakeNavItem(Transform parent, string name,
            string label, Sprite iconSprite, bool active)
        {
            var activeColor = active ? Hex("#16A34A") : Hex("#94A3B8");
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
                preferredWidth: 58, minWidth: 58, preferredHeight: 58, minHeight: 58);
            var iconImg = iconBox.gameObject.AddComponent<Image>();
            iconImg.sprite        = iconSprite;
            iconImg.color         = activeColor;
            iconImg.preserveAspect = true;

            var lbl = UIHelper.MakeLabel($"label-{name}", col, label,
                fontSize, activeColor, TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(lbl.gameObject, preferredHeight: 46, minHeight: 46);

            var indRow = UIHelper.MakeRect("IndRow", col);
            UIHelper.SetLayout(indRow.gameObject,
                preferredWidth: 54, minWidth: 54, preferredHeight: 6, minHeight: 6);
            if (active)
            {
                var indImg = indRow.gameObject.AddComponent<Image>();
                indImg.color  = Hex("#16A34A");
                indImg.sprite = UIHelper.GetWhiteSprite();
                UIHelper.SetImageRoundedSprite(indImg, 3);
            }
        }
    }
}
