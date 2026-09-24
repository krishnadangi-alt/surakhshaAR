using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Module Selection screen builder matching Reference 2:
    /// "Safety Training Modules — Build Skills for a Safer Tomorrow"
    /// - Forest Emerald header (#0A3C36) with back button + title/subtitle.
    /// - Scrollable list of large module cards (~280px each) with:
    ///     · 90x90 icon box (rounded square) with coloured background.
    ///     · Bold title (32px) + description (22px) column.
    ///     · Right arrow chevron.
    ///     · Bottom row: "📖 Training Module" chip + "Available" / "🔒 Locked" pill.
    /// - Full 4-tab bottom navigation (active: Learn).
    /// </summary>
    public static class ModuleSelectionBuilder
    {
        private const float NAV_H   = 165f;
        private const float HDR_H   = 195f;
        private static Color Hex(string h) => UIColors.Hex(h);

        public static GameObject Build()
        {
            var root   = new GameObject("ModuleSelectionScreen");
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;
            rootRT.sizeDelta = Vector2.zero;

            var rootImg = root.AddComponent<Image>();
            rootImg.color  = Hex("#F8FAFC");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // ── 1. Bottom Navigation Bar ──────────────────────────────────
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

            var headerRow = UIHelper.MakeHorizontal("HeaderRow", header, 18,
                new RectOffset(24, 24, 20, 20),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(headerRow, 0, 0, 0, 0);
            headerRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // Back button
            var backBtn = UIHelper.MakeButton("btn-back", headerRow, "‹", 48,
                new Color(1, 1, 1, 0.18f), Color.white, 24);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 72, minWidth: 72,
                preferredHeight: 72, minHeight: 72);

            // Title + subtitle column
            var titleCol = UIHelper.MakeVertical("TitleCol", headerRow, 8);
            UIHelper.SetLayout(titleCol.gameObject, flexibleWidth: true, flexWidth: 1);
            var tcVlg = titleCol.GetComponent<VerticalLayoutGroup>();
            tcVlg.childAlignment = TextAnchor.MiddleLeft;
            tcVlg.childControlWidth = true;
            tcVlg.childControlHeight = true;
            tcVlg.childForceExpandHeight = false;

            var titleLbl = UIHelper.MakeLabel("label-title", titleCol,
                "Select Training Module", 46, Color.white, TextAlignmentOptions.Left, bold: true);
            titleLbl.lineSpacing = 1.05f;
            UIHelper.SetLayout(titleLbl.gameObject, preferredHeight: 56, minHeight: 48);

            var subLbl = UIHelper.MakeLabel("label-sub", titleCol,
                "Build Skills for a Safer Tomorrow", 28, Hex("#A7F3D0"), TextAlignmentOptions.Left);
            subLbl.lineSpacing = 1.15f;
            subLbl.textWrappingMode = TextWrappingModes.Normal;
            UIHelper.SetLayout(subLbl.gameObject, preferredHeight: 68, minHeight: 54);

            // ── 3. Scrollable Module List ─────────────────────────────────
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

            var content = UIHelper.MakeRect("module-list", viewport);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot     = new Vector2(0.5f, 1f);
            content.sizeDelta = Vector2.zero;
            sr.content        = content;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing            = 22;
            vlg.padding            = new RectOffset(22, 22, 22, 36);
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth      = true;
            vlg.childControlHeight     = true;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return root;
        }

        // =================================================================
        // MODULE CARD — called dynamically by ModuleSelectionController
        // =================================================================
        /// <summary>
        /// Creates a reference-matching module card and parents it under <paramref name="parent"/>.
        /// </summary>
        public static GameObject CreateModuleCard(
            Transform parent,
            string     cardName,
            Sprite     iconSprite,
            Color      iconBg,
            string     title,
            string     description,
            bool       isLocked,
            Color      statusColor,
            string     statusLabel,
            Color?     iconColor = null,
            string     typeLabel = null)
        {
            var card = new GameObject(cardName);
            card.transform.SetParent(parent, false);
            UIHelper.SetLayout(card.gameObject, flexibleWidth: true, flexWidth: 1,
                preferredHeight: 310, minHeight: 295);

            var cardImg = card.AddComponent<Image>();
            cardImg.color  = isLocked ? Hex("#F8FAFC") : Color.white;
            cardImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cardImg, 24);

            var outline = card.AddComponent<Outline>();
            outline.effectColor    = Hex("#E2E8F0");
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var shadow = card.AddComponent<Shadow>();
            shadow.effectColor    = isLocked ? new Color(0, 0, 0, 0.03f) : new Color(0, 0, 0, 0.06f);
            shadow.effectDistance = isLocked ? new Vector2(0, -2f) : new Vector2(0, -3f);

            // Only add Button to interactive (unlocked) cards to prevent Unity from tinting the card disabled-gray
            if (!isLocked)
            {
                var btn = card.AddComponent<Button>();
                btn.targetGraphic = cardImg;
            }

            // ── Main vertical column ──────────────────────────────────────
            var mainCol = UIHelper.MakeVertical("MainCol", card.transform, 14,
                new RectOffset(22, 22, 20, 20));
            UIHelper.Stretch(mainCol, 0, 0, 0, 0);
            mainCol.GetComponent<VerticalLayoutGroup>().childForceExpandWidth = true;

            // ── Top row: icon + title/desc + arrow ────────────────────────
            var topRow = UIHelper.MakeHorizontal("TopRow", mainCol, 16,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(topRow.gameObject, preferredHeight: 175, minHeight: 160);
            topRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // Icon box (106×106 rounded square)
            var iconBox = UIHelper.MakeRect("IconBox", topRow);
            UIHelper.SetLayout(iconBox.gameObject, preferredWidth: 106, minWidth: 106,
                preferredHeight: 106, minHeight: 106);
            var iconBoxImg = iconBox.gameObject.AddComponent<Image>();
            iconBoxImg.color  = iconBg;
            UIHelper.SetImageRoundedSprite(iconBoxImg, 22);

            if (iconSprite != null)
            {
                var iconGO  = UIHelper.MakeRect("ModuleIcon", iconBox);
                UIHelper.AnchorCenter(iconGO, 66, 66);
                var iconImg = iconGO.gameObject.AddComponent<Image>();
                iconImg.sprite         = iconSprite;
                iconImg.preserveAspect = true;
                iconImg.color          = iconColor.HasValue ? iconColor.Value : Color.white;
                iconImg.raycastTarget  = false;
            }

            // Text column (title + description)
            var textCol = UIHelper.MakeVertical("TextCol", topRow, 8);
            UIHelper.SetLayout(textCol.gameObject, flexibleWidth: true, flexWidth: 1,
                preferredHeight: 175, minHeight: 160);
            var textVlg = textCol.GetComponent<VerticalLayoutGroup>();
            textVlg.childAlignment = TextAnchor.MiddleLeft;
            textVlg.childControlWidth = true;
            textVlg.childControlHeight = true;
            textVlg.childForceExpandHeight = false;

            var titleColor  = Hex("#0F172A");
            var descColor   = Hex("#334155");

            var titleLbl = UIHelper.MakeLabel($"label-title-{cardName}", textCol,
                title, 38, titleColor, TextAlignmentOptions.Left, bold: true);
            titleLbl.lineSpacing = 1.05f;
            titleLbl.textWrappingMode = TextWrappingModes.Normal;
            UIHelper.SetLayout(titleLbl.gameObject, preferredHeight: 84, minHeight: 70);

            var descLbl = UIHelper.MakeLabel($"label-desc-{cardName}", textCol,
                description, 26, descColor, TextAlignmentOptions.Left);
            descLbl.lineSpacing = 1.15f;
            descLbl.textWrappingMode = TextWrappingModes.Normal;
            UIHelper.SetLayout(descLbl.gameObject, preferredHeight: 74, minHeight: 60);

            // Right arrow
            var arrowColor = isLocked ? Hex("#CBD5E1") : Hex("#94A3B8");
            var arrowLbl   = UIHelper.MakeLabel("ArrowLbl", topRow,
                ">", 32, arrowColor, TextAlignmentOptions.Right, bold: true);
            UIHelper.SetLayout(arrowLbl.gameObject,
                preferredWidth: 26, minWidth: 26, preferredHeight: 40, minHeight: 40);

            // ── Bottom row: tag chip + status pill ────────────────────────
            var botRow = UIHelper.MakeHorizontal("BotRow", mainCol, 14,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(botRow.gameObject, preferredHeight: 52, minHeight: 48);
            var botHlg = botRow.GetComponent<HorizontalLayoutGroup>();
            botHlg.childAlignment = TextAnchor.MiddleLeft;
            botHlg.childControlWidth = false;
            botHlg.childControlHeight = false;

            // "📖 Training Module" left chip
            var chip = UIHelper.MakeHorizontal("TypeChip", botRow, 10,
                new RectOffset(16, 16, 8, 8),
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(chip.gameObject, preferredWidth: 235, minWidth: 210, preferredHeight: 50, minHeight: 46);
            chip.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
            var chipImg = chip.gameObject.AddComponent<Image>();
            chipImg.color  = Hex("#F1F5F9");
            UIHelper.SetImageRoundedSprite(chipImg, 14);

            var bookIconGO  = UIHelper.MakeRect("BookIcon", chip);
            UIHelper.SetLayout(bookIconGO.gameObject,
                preferredWidth: 24, minWidth: 24, preferredHeight: 24, minHeight: 24);
            var bookImg = bookIconGO.gameObject.AddComponent<Image>();
            bookImg.sprite        = UIHelper.GetBookSprite();
            bookImg.color         = Hex("#64748B");
            bookImg.preserveAspect = true;

            var chipLbl = UIHelper.MakeLabel($"label-type-{cardName}", chip,
                typeLabel ?? "Training Module", 25, Hex("#334155"), TextAlignmentOptions.Left, bold: true, wrap: false);
            chipLbl.overflowMode = TextOverflowModes.Ellipsis;
            UIHelper.SetLayout(chipLbl.gameObject, preferredHeight: 34, minHeight: 30);

            // Status pill (Available / Locked / THEORY / ASSESSMENT)
            var pillGO  = new GameObject("StatusPill");
            pillGO.transform.SetParent(botRow, false);
            UIHelper.SetLayout(pillGO.gameObject,
                preferredWidth: 320, minWidth: 270, preferredHeight: 50, minHeight: 46);

            var pillImg = pillGO.AddComponent<Image>();
            if (isLocked)
            {
                pillImg.color = Hex("#E2E8F0");
            }
            else
            {
                // Use a tinted pastel matching the module accent
                var pillBg = new Color(statusColor.r, statusColor.g, statusColor.b, 0.16f);
                pillImg.color = pillBg;
            }
            UIHelper.SetImageRoundedSprite(pillImg, 14);

            var pillRow = UIHelper.MakeHorizontal("PillRow", pillGO.transform, 8,
                new RectOffset(16, 16, 8, 8),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(pillRow, 0, 0, 0, 0);
            pillRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            if (isLocked)
            {
                var lockIconGO  = UIHelper.MakeRect("LockIcon", pillRow);
                UIHelper.SetLayout(lockIconGO.gameObject,
                    preferredWidth: 20, minWidth: 20, preferredHeight: 20, minHeight: 20);
                var lockImg = lockIconGO.gameObject.AddComponent<Image>();
                lockImg.sprite         = UIHelper.GetLockSprite();
                lockImg.color          = Hex("#64748B");
                lockImg.preserveAspect = true;
            }

            var pillLbl = UIHelper.MakeLabel($"label-status-{cardName}", pillRow,
                statusLabel ?? (isLocked ? "Locked" : "Available"),
                25, isLocked ? Hex("#64748B") : statusColor,
                TextAlignmentOptions.Center, bold: true, wrap: false);
            pillLbl.overflowMode = TextOverflowModes.Ellipsis;
            UIHelper.SetLayout(pillLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 34, minHeight: 30);

            return card;
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
            MakeNavItem(navRT, "nav-learn",        "Learn",        UIHelper.GetBookSprite(),  true);
            MakeNavItem(navRT, "nav-progress",     "My Progress",  UIHelper.GetChartSprite(), false);
            MakeNavItem(navRT, "nav-certificates", "Certificates", UIHelper.GetMedalSprite(), false);
        }

        private static void MakeNavItem(Transform parent, string name,
            string label, Sprite iconSprite, bool active)
        {
            var activeColor    = active ? Hex("#059669") : Hex("#94A3B8");
            var activeFontSize = 30f;

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
                activeFontSize, activeColor, TextAlignmentOptions.Center, bold: true);
            lbl.textWrappingMode = TextWrappingModes.NoWrap;
            lbl.overflowMode = TextOverflowModes.Overflow;
            UIHelper.SetLayout(lbl.gameObject, preferredHeight: 42, minHeight: 38);

            // Active underline indicator
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
