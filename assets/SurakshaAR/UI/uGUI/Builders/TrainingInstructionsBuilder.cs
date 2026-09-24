using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Training Instructions Screen Builder (Screen 6 in reference UI):
    /// Shows the "Before You Start" warning and SOP preparation checklist:
    /// 1. Safe open area with good lighting
    /// 2. Camera and motion permissions
    /// 3. Follow on-screen guidance
    /// 4. Simulation safety disclaimer
    /// 5. Performance assessment record
    /// Followed by interactive confirmation checkbox and green "▶ Start Training" button.
    ///
    /// TYPOGRAPHY — Unified with Learn (ModuleSelection) &amp; Progress screens (CanvasScaler 1080×2400):
    ///   Header title   = 48px bold
    ///   Banner heading = 44px bold
    ///   Banner desc    = 34px
    ///   Card text      = 34px        (matches Learn/Progress body scale)
    ///   Agreement text = 34px bold
    ///   Start button   = 36px bold
    ///
    /// LAYOUT: Full-screen filling with spacious cards, rich padding, and no empty gaps.
    /// </summary>
    public static class TrainingInstructionsBuilder
    {
        private static readonly Color AccentOrange = UIColors.Hex("#EA580C");
        private static readonly Color AccentLight  = UIColors.Hex("#FFF7ED");
        private static readonly Color AccentBorder = UIColors.Hex("#FFEDD5");
        private static readonly Color NavyText     = UIColors.Hex("#0A192F");
        private static readonly Color DarkText     = UIColors.Hex("#0F172A");
        private static readonly Color SlateText    = UIColors.Hex("#334155");
        private static readonly Color MutedText    = UIColors.Hex("#475569");
        private static readonly Color SafetyGreen  = UIColors.Hex("#16A34A");

        private static Color Hex(string hex) => UIColors.Hex(hex);

        public static GameObject Build()
        {
            var root = new GameObject("TrainingInstructionsScreen");
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            var rootImg = root.AddComponent<Image>();
            rootImg.color  = Hex("#F8FAFC");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // ── Scrollable Body ──────────────────────────────────────────
            var scrollRoot = UIHelper.MakeRect("ScrollArea", root.transform);
            scrollRoot.anchorMin = Vector2.zero;
            scrollRoot.anchorMax = Vector2.one;
            scrollRoot.offsetMin = new Vector2(0, 126); // Room for bottom sticky button
            scrollRoot.offsetMax = Vector2.zero;

            var scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal        = false;
            scrollRect.vertical          = true;
            scrollRect.movementType      = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 40f;

            var viewport = UIHelper.MakeRect("Viewport", scrollRoot);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewport.gameObject.AddComponent<RectMask2D>();
            scrollRect.viewport = viewport;

            var content = UIHelper.MakeRect("Content", viewport);
            content.anchorMin  = new Vector2(0, 1);
            content.anchorMax  = new Vector2(1, 1);
            content.pivot      = new Vector2(0.5f, 1);
            content.offsetMin  = Vector2.zero;
            content.offsetMax  = Vector2.zero;
            content.sizeDelta  = Vector2.zero;
            scrollRect.content = content;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing                = 22;
            vlg.padding                = new RectOffset(30, 30, 18, 48);
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth      = true;
            vlg.childControlHeight     = true; // Allows LayoutElements to drive heights

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── 1. Top Header ─────────────────────────────────────────────
            BuildTopHeader(content);

            // ── 2. Before You Start Info Banner ───────────────────────────
            BuildInfoBanner(content);

            // ── 3. Five Instruction Cards ─────────────────────────────────
            BuildInstructionCards(content);

            // ── 4. Interactive Confirmation Checkbox ──────────────────────
            BuildAgreementCheckbox(content);

            // ── 5. Sticky Bottom Button: ▶ Start Training ────────────────
            BuildStickyBottomBar(root.transform);

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
            var backOutline = backBtn.gameObject.AddComponent<Outline>();
            backOutline.effectColor    = Hex("#E2E8F0");
            backOutline.effectDistance = new Vector2(1, -1);

            var titleLbl = UIHelper.MakeLabel("label-title", row,
                "Training Instructions", 48, NavyText, bold: true);
            titleLbl.textWrappingMode = TextWrappingModes.NoWrap;
            titleLbl.overflowMode     = TextOverflowModes.Ellipsis;
            UIHelper.SetLayout(titleLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 60);

            var langPill = UIHelper.MakeButton("btn-language-picker", row,
                "🌐 EN ▾", 26, Hex("#F1F5F9"), SlateText, 14);
            UIHelper.SetLayout(langPill.gameObject, preferredWidth: 130, preferredHeight: 60);

            var bellBtn = UIHelper.MakeButton("btn-bell", row, "", 14,
                Color.white, Color.white, 20);
            UIHelper.SetLayout(bellBtn.gameObject, preferredWidth: 60, preferredHeight: 60);
            bellBtn.gameObject.AddComponent<Outline>().effectColor = Hex("#E2E8F0");
            var bellIconGO = UIHelper.MakeRect("BellIcon", bellBtn.transform);
            UIHelper.Stretch(bellIconGO, 14, 14, 14, 14);
            var bellImg = bellIconGO.gameObject.AddComponent<Image>();
            bellImg.sprite        = UIHelper.GetBellSprite();
            bellImg.preserveAspect = true;

            var profBtn = UIHelper.MakeButton("btn-profile", row, "", 14,
                Color.white, Color.white, 20);
            UIHelper.SetLayout(profBtn.gameObject, preferredWidth: 60, preferredHeight: 60);
            profBtn.gameObject.AddComponent<Outline>().effectColor = Hex("#E2E8F0");
            var profIconGO = UIHelper.MakeRect("ProfIcon", profBtn.transform);
            UIHelper.Stretch(profIconGO, 14, 14, 14, 14);
            var profImg = profIconGO.gameObject.AddComponent<Image>();
            profImg.sprite        = UIHelper.GetProfileSprite();
            profImg.preserveAspect = true;
        }

        // ────────────────────────────────────────────────────────────────
        // BEFORE YOU START BANNER (large, prominent, 44px title, 34px desc)
        // ────────────────────────────────────────────────────────────────
        private static void BuildInfoBanner(Transform parent)
        {
            var banner = UIHelper.MakeRect("InfoBanner", parent);
            UIHelper.SetLayout(banner.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 190, minHeight: 175);

            var bImg = banner.gameObject.AddComponent<Image>();
            bImg.color  = AccentLight;
            bImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(bImg, 24);

            var outline = banner.gameObject.AddComponent<Outline>();
            outline.effectColor    = AccentBorder;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var shadow = banner.gameObject.AddComponent<Shadow>();
            shadow.effectColor    = new Color(0, 0, 0, 0.04f);
            shadow.effectDistance = new Vector2(0, -3f);

            var row = UIHelper.MakeHorizontal("InnerRow", banner, 20,
                new RectOffset(24, 24, 20, 20),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(row, 0, 0, 0, 0);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // Orange (i) circle icon
            var iconBox = UIHelper.MakeRect("InfoIconBox", row);
            UIHelper.SetLayout(iconBox.gameObject,
                preferredWidth: 76, minWidth: 76,
                preferredHeight: 76, minHeight: 76);
            var ibImg = iconBox.gameObject.AddComponent<Image>();
            ibImg.color  = AccentOrange;
            ibImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(ibImg, 38); // perfect circle

            var iLbl = UIHelper.MakeLabel("ILbl", iconBox,
                "i", 42, Color.white, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(iLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Text column
            var textCol = UIHelper.MakeVertical("TextCol", row, 6,
                childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(textCol.gameObject, flexibleWidth: true, flexWidth: 1);
            var tvlg = textCol.GetComponent<VerticalLayoutGroup>();
            tvlg.childAlignment       = TextAnchor.MiddleLeft;
            tvlg.childForceExpandHeight = false;

            var headLbl = UIHelper.MakeLabel("HeadLbl", textCol,
                "Before You Start", 44, Hex("#9A3412"), bold: true);
            UIHelper.SetLayout(headLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 52);

            var descLbl = UIHelper.MakeLabel("DescLbl", textCol,
                "Follow the instructions below for the best training experience.",
                34, Hex("#C2410C"), wrap: true);
            descLbl.lineSpacing = 1.15f;
            UIHelper.SetLayout(descLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 88);
        }

        // ────────────────────────────────────────────────────────────────
        // FIVE INSTRUCTION CARDS (spacious, 34px text matching Learn scale)
        // ────────────────────────────────────────────────────────────────
        private static void BuildInstructionCards(Transform parent)
        {
            var col = UIHelper.MakeVertical("CardsCol", parent, 16,
                childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(col.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 864, minHeight: 800);

            // 1. Safe open area
            BuildSingleCard(col.transform, "card-inst-1",
                icon: "📱", iconBg: Hex("#DCFCE7"), iconColor: Hex("#16A34A"),
                text: "Use in a safe, open area with good lighting.");

            // 2. Camera and motion permissions
            BuildSingleCard(col.transform, "card-inst-2",
                icon: "✋", iconBg: Hex("#DBEAFE"), iconColor: Hex("#2563EB"),
                text: "Allow camera and motion permissions.");

            // 3. On-screen guidance
            BuildSingleCard(col.transform, "card-inst-3",
                icon: "🎯", iconBg: Hex("#FEE2E2"), iconColor: Hex("#DC2626"),
                text: "Follow on-screen guidance and complete all steps.");

            // 4. Do not try real equipment
            BuildSingleCard(col.transform, "card-inst-4",
                icon: "🛡️", iconBg: Hex("#F3E8FF"), iconColor: Hex("#9333EA"),
                text: "Do not try real equipment. This is a training simulation.");

            // 5. Assessment recorded
            BuildSingleCard(col.transform, "card-inst-5",
                icon: "📋", iconBg: Hex("#FEF3C7"), iconColor: Hex("#D97706"),
                text: "Your performance will be recorded for assessment.");
        }

        private static void BuildSingleCard(Transform parent, string cardName,
            string icon, Color iconBg, Color iconColor, string text)
        {
            var card = UIHelper.MakeRect(cardName, parent);
            UIHelper.SetLayout(card.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 160, minHeight: 150);

            var cImg = card.gameObject.AddComponent<Image>();
            cImg.color  = Color.white;
            cImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cImg, 22);

            var border = card.gameObject.AddComponent<Outline>();
            border.effectColor    = Hex("#E2E8F0");
            border.effectDistance = new Vector2(1.5f, -1.5f);

            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor    = new Color(0, 0, 0, 0.03f);
            shadow.effectDistance = new Vector2(0, -2f);

            var row = UIHelper.MakeHorizontal("CardRow", card, 20,
                new RectOffset(24, 24, 18, 18),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(row, 0, 0, 0, 0);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // Colored circle icon container
            var iconBox = UIHelper.MakeRect("IconBox", row);
            UIHelper.SetLayout(iconBox.gameObject,
                preferredWidth: 76, minWidth: 76,
                preferredHeight: 76, minHeight: 76);
            var ibImg = iconBox.gameObject.AddComponent<Image>();
            ibImg.color  = iconBg;
            ibImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(ibImg, 38); // perfect circle

            var iconLbl = UIHelper.MakeLabel("IconLbl", iconBox,
                icon, 38, iconColor, TextAlignmentOptions.Center);
            UIHelper.Stretch(iconLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Instruction text: 34px (matches Learn/Progress body text)
            var textLbl = UIHelper.MakeLabel("TextLbl", row,
                text, 34, DarkText, wrap: true);
            textLbl.lineSpacing = 1.18f;
            UIHelper.SetLayout(textLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 88);
        }

        // ────────────────────────────────────────────────────────────────
        // INTERACTIVE AGREEMENT CHECKBOX (matches exact controller path)
        // ────────────────────────────────────────────────────────────────
        private static void BuildAgreementCheckbox(Transform parent)
        {
            var box = UIHelper.MakeRect("AgreementBox", parent);
            UIHelper.SetLayout(box.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 110, minHeight: 100);

            var cImg = box.gameObject.AddComponent<Image>();
            cImg.color  = Color.white;
            cImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cImg, 22);

            var border = box.gameObject.AddComponent<Outline>();
            border.effectColor    = Hex("#E2E8F0");
            border.effectDistance = new Vector2(1.5f, -1.5f);

            var btn = box.gameObject.AddComponent<Button>();
            btn.name = "btn-agreement-toggle";

            var row = UIHelper.MakeHorizontal("Row", box, 18,
                new RectOffset(24, 24, 16, 16),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(row, 0, 0, 0, 0);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // Green Checkbox Square
            var checkSquare = UIHelper.MakeRect("check-box-square", row);
            UIHelper.SetLayout(checkSquare.gameObject,
                preferredWidth: 56, minWidth: 56,
                preferredHeight: 56, minHeight: 56);
            var csImg = checkSquare.gameObject.AddComponent<Image>();
            csImg.color  = SafetyGreen;
            csImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(csImg, 14);

            var checkMark = UIHelper.MakeLabel("check-mark-symbol", checkSquare,
                "✓", 36, Color.white, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(checkMark.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Label: "I understand the instructions and am ready to start."
            var lbl = UIHelper.MakeLabel("label-agreement-text", row,
                "I understand the instructions and am ready to start.",
                34, NavyText, bold: true, wrap: true);
            lbl.lineSpacing = 1.1f;
            UIHelper.SetLayout(lbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 72);
        }

        // ────────────────────────────────────────────────────────────────
        // STICKY BOTTOM BUTTON (36px bold green ▶ Start Training button)
        // ────────────────────────────────────────────────────────────────
        private static void BuildStickyBottomBar(Transform parent)
        {
            var bar = UIHelper.MakeRect("StickyBottomBar", parent);
            bar.anchorMin = new Vector2(0, 0);
            bar.anchorMax = new Vector2(1, 0);
            bar.pivot     = new Vector2(0.5f, 0);
            bar.sizeDelta = new Vector2(0, 126);

            var bg = bar.gameObject.AddComponent<Image>();
            bg.color  = Color.white;
            bg.sprite = UIHelper.GetWhiteSprite();

            var topBorder = UIHelper.MakeRect("TopBorder", bar);
            topBorder.anchorMin = new Vector2(0, 1);
            topBorder.anchorMax = new Vector2(1, 1);
            topBorder.pivot     = new Vector2(0.5f, 1);
            topBorder.sizeDelta = new Vector2(0, 1.5f);
            var tbImg = topBorder.gameObject.AddComponent<Image>();
            tbImg.color = Hex("#E2E8F0");

            var btn = UIHelper.MakeButton("btn-start-training", bar,
                "▶   Start Training", 36, SafetyGreen, Color.white, 20);
            var rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(30, -45);
            rt.offsetMax = new Vector2(-30, 45);

            var btnLbl = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnLbl != null) btnLbl.fontStyle = FontStyles.Bold;
        }
    }
}
