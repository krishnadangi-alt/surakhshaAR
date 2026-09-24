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
    /// Followed by interactive checkbox and green "▶ Start Training" button.
    /// </summary>
    public static class TrainingInstructionsBuilder
    {
        private static Color Hex(string hex) => UIColors.Hex(hex);

        public static GameObject Build()
        {
            var root = new GameObject("TrainingInstructionsScreen");
            root.AddComponent<RectTransform>();
            var rootImg = root.AddComponent<Image>();
            rootImg.color = Hex("#F8FAFC");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // ── Scrollable Body ──────────────────────────────────────────
            var scrollRoot = UIHelper.MakeRect("ScrollArea", root.transform);
            scrollRoot.anchorMin = Vector2.zero;
            scrollRoot.anchorMax = Vector2.one;
            scrollRoot.offsetMin = new Vector2(0, 110); // Room for bottom sticky button
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
            vlg.spacing = 18;
            vlg.padding = new RectOffset(32, 32, 20, 40);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

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

        private static void BuildTopHeader(Transform parent)
        {
            var row = UIHelper.MakeHorizontal("TopHeaderRow", parent, 14, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 64, minHeight: 56);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var backBtn = UIHelper.MakeButton("btn-back", row, "‹", 50, Color.white, Hex("#0A192F"), 18);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 64, preferredHeight: 64);
            var backOutline = backBtn.gameObject.AddComponent<Outline>();
            backOutline.effectColor = Hex("#E2E8F0");
            backOutline.effectDistance = new Vector2(1, -1);

            var titleLbl = UIHelper.MakeLabel("label-title", row, "Training Instructions", 36, Hex("#0A192F"), bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 52);
        }

        private static void BuildInfoBanner(Transform parent)
        {
            var banner = UIHelper.MakeRect("InfoBanner", parent);
            UIHelper.SetLayout(banner.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 120, minHeight: 110);

            var bImg = banner.gameObject.AddComponent<Image>();
            bImg.color = Hex("#FFF7ED");
            UIHelper.SetImageRoundedSprite(bImg, 18);

            var outline = banner.gameObject.AddComponent<Outline>();
            outline.effectColor = Hex("#FFEDD5");
            outline.effectDistance = new Vector2(1, -1);

            var row = UIHelper.MakeHorizontal("InnerRow", banner, 16, new RectOffset(20, 20, 16, 16), childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(row, 0, 0, 0, 0);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // Orange (i) circle icon
            var iconBox = UIHelper.MakeRect("InfoIconBox", row);
            UIHelper.SetLayout(iconBox.gameObject, preferredWidth: 52, preferredHeight: 52);
            var ibImg = iconBox.gameObject.AddComponent<Image>();
            ibImg.color = Hex("#EA580C");
            UIHelper.SetImageRoundedSprite(ibImg, 26); // circle

            var iLbl = UIHelper.MakeLabel("ILbl", iconBox, "i", 30, Color.white, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(iLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Text column
            var textCol = UIHelper.MakeVertical("TextCol", row, 4);
            UIHelper.SetLayout(textCol.gameObject, flexibleWidth: true, flexWidth: 1);
            var tvlg = textCol.GetComponent<VerticalLayoutGroup>();
            tvlg.childAlignment = TextAnchor.MiddleLeft;
            tvlg.childForceExpandHeight = false;

            var headLbl = UIHelper.MakeLabel("HeadLbl", textCol, "Before You Start", 30, Hex("#9A3412"), bold: true);
            UIHelper.SetLayout(headLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 36);

            var descLbl = UIHelper.MakeLabel("DescLbl", textCol, "Follow the instructions below for the best training experience.", 22, Hex("#C2410C"), wrap: true);
            descLbl.lineSpacing = 1.15f;
            UIHelper.SetLayout(descLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 32);
        }

        private static void BuildInstructionCards(Transform parent)
        {
            var col = UIHelper.MakeVertical("CardsCol", parent, 14);
            UIHelper.SetLayout(col.gameObject, flexibleWidth: true, flexWidth: 1);
            var csf = col.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

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

        private static void BuildSingleCard(Transform parent, string cardName, string icon, Color iconBg, Color iconColor, string text)
        {
            var card = UIHelper.MakeRect(cardName, parent);
            UIHelper.SetLayout(card.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 96, minHeight: 90);

            var cImg = card.gameObject.AddComponent<Image>();
            cImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(cImg, 18);

            var border = card.gameObject.AddComponent<Outline>();
            border.effectColor = Hex("#E2E8F0");
            border.effectDistance = new Vector2(1, -1);

            var row = UIHelper.MakeHorizontal("CardRow", card, 16, new RectOffset(20, 20, 16, 16), childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(row, 0, 0, 0, 0);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // Colored circle icon container
            var iconBox = UIHelper.MakeRect("IconBox", row);
            UIHelper.SetLayout(iconBox.gameObject, preferredWidth: 54, minWidth: 54, preferredHeight: 54, minHeight: 54);
            var ibImg = iconBox.gameObject.AddComponent<Image>();
            ibImg.color = iconBg;
            UIHelper.SetImageRoundedSprite(ibImg, 27); // perfect circle

            var iconLbl = UIHelper.MakeLabel("IconLbl", iconBox, icon, 28, iconColor, TextAlignmentOptions.Center);
            UIHelper.Stretch(iconLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Instruction text
            var textLbl = UIHelper.MakeLabel("TextLbl", row, text, 26, Hex("#1E293B"), wrap: true);
            textLbl.lineSpacing = 1.15f;
            UIHelper.SetLayout(textLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 56);
        }

        private static void BuildAgreementCheckbox(Transform parent)
        {
            var box = UIHelper.MakeRect("AgreementBox", parent);
            UIHelper.SetLayout(box.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 72, minHeight: 64);

            var btn = box.gameObject.AddComponent<Button>();
            btn.name = "btn-agreement-toggle";

            var row = UIHelper.MakeHorizontal("Row", box, 14, new RectOffset(4, 4, 8, 8), childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(row, 0, 0, 0, 0);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // Green Checkbox Square
            var checkSquare = UIHelper.MakeRect("check-box-square", row);
            UIHelper.SetLayout(checkSquare.gameObject, preferredWidth: 44, minWidth: 44, preferredHeight: 44, minHeight: 44);
            var csImg = checkSquare.gameObject.AddComponent<Image>();
            csImg.color = Hex("#16A34A");
            UIHelper.SetImageRoundedSprite(csImg, 10);

            var checkMark = UIHelper.MakeLabel("check-mark-symbol", checkSquare, "✓", 28, Color.white, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(checkMark.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Label: "I understand the instructions and am ready to start."
            var lbl = UIHelper.MakeLabel("label-agreement-text", row,
                "I understand the instructions\nand am ready to start.",
                26, Hex("#0A192F"), bold: true, wrap: true);
            lbl.lineSpacing = 1.05f;
            UIHelper.SetLayout(lbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 56);
        }

        private static void BuildStickyBottomBar(Transform parent)
        {
            var bar = UIHelper.MakeRect("StickyBottomBar", parent);
            bar.anchorMin = new Vector2(0, 0);
            bar.anchorMax = new Vector2(1, 0);
            bar.pivot     = new Vector2(0.5f, 0);
            bar.sizeDelta = new Vector2(0, 110);

            var bg = bar.gameObject.AddComponent<Image>();
            bg.color = Color.white;

            var topBorder = UIHelper.MakeRect("TopBorder", bar);
            topBorder.anchorMin = new Vector2(0, 1);
            topBorder.anchorMax = new Vector2(1, 1);
            topBorder.pivot     = new Vector2(0.5f, 1);
            topBorder.sizeDelta = new Vector2(0, 1.5f);
            var tbImg = topBorder.gameObject.AddComponent<Image>();
            tbImg.color = Hex("#E2E8F0");

            var btn = UIHelper.MakeButton("btn-start-training", bar, "▶ Start Training", 32, Hex("#16A34A"), Color.white, 16);
            UIHelper.SetLayout(btn.gameObject, preferredHeight: 74, flexibleWidth: true, flexWidth: 1);
            var rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(32, -37);
            rt.offsetMax = new Vector2(-32, 37);

            var btnLbl = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnLbl != null)
            {
                btnLbl.fontStyle = FontStyles.Bold;
            }
        }
    }
}
