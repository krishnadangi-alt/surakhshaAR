using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Upgraded Result Screen Builder:
    /// High-impact government-grade industrial competency assessment report.
    /// Supports both PASS (Certification readiness) and FAIL/RETRAINING states.
    /// Displays:
    /// - Pass/Fail badge & icon
    /// - Comprehensive metrics (Score, Time Taken, Critical Errors, Sequence Steps)
    /// - Industry competency breakdown (Hazard Recognition, Equipment Selection, Evacuation)
    /// - Retraining comparison (previous attempt vs current attempt)
    /// - Primary CTA ("View Certificate" or "Start Targeted Retraining") + "Return to Dashboard"
    /// </summary>
    public static class ResultBuilder
    {
        public static GameObject Build()
        {
            var root = new GameObject("ResultScreen");
            root.AddComponent<RectTransform>();
            var rootImg = root.AddComponent<Image>();
            rootImg.color = UIColors.Background;
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
            content.sizeDelta = new Vector2(0, 1400);
            scrollRect.content = content;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.padding = new RectOffset(24, 24, 40, 48);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── 1. Top Status Badge & Title ──────────────────────────────
            BuildHeaderStatus(content);

            // ── 2. Primary Score & Metrics Grid ───────────────────────────
            BuildMetricsCard(content);

            // ── 3. Competency Breakdown ──────────────────────────────────
            BuildCompetencyCard(content);

            // ── 4. Reassessment Comparison (Conditional) ─────────────────
            BuildComparisonCard(content);

            // ── 5. Action Buttons ────────────────────────────────────────
            BuildActions(content);

            return root;
        }

        private static void BuildHeaderStatus(Transform parent)
        {
            var headerBox = UIHelper.MakeVertical("HeaderBox", parent, 12);
            UIHelper.SetLayout(headerBox.gameObject, minHeight: 220);
            var csf = headerBox.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Circular Badge
            var circle = UIHelper.MakeRect("Circle", headerBox);
            circle.sizeDelta = new Vector2(120, 120);
            var circleImg = circle.gameObject.AddComponent<Image>();
            circleImg.sprite = UIHelper.GetCircleSprite();
            circleImg.color = UIColors.Hex("#E6F4EC"); // Pass green tint (or amber for fail)

            var badgeIconGO = new GameObject("label-badge-icon");
            badgeIconGO.transform.SetParent(circle, false);
            var badgeImg = badgeIconGO.AddComponent<Image>();
            badgeImg.sprite = UIHelper.GetCheckmarkSprite();
            badgeImg.color = UIColors.SafetyGreen;
            UIHelper.AnchorCenter(badgeIconGO.GetComponent<RectTransform>(), 64, 64);

            var titleLbl = UIHelper.MakeLabel("label-result-title", headerBox, "Assessment Passed!",
                52, UIColors.PrimaryDark, TextAlignmentOptions.Center, bold: true, wrap: true);
            UIHelper.SetLayout(titleLbl.gameObject, minHeight: 62);

            var subLbl = UIHelper.MakeLabel("label-result-sub", headerBox,
                "Demonstrated compliance with Ministry of Mines Industrial Safety SOP.",
                34, UIColors.TextSecondary, TextAlignmentOptions.Center, wrap: true);
            subLbl.lineSpacing = 4f;
            UIHelper.SetLayout(subLbl.gameObject, minHeight: 52);
        }

        private static void BuildMetricsCard(Transform parent)
        {
            var card = new GameObject("MetricsCard");
            card.transform.SetParent(parent, false);
            UIHelper.SetLayout(card, preferredHeight: 280, minHeight: 260);

            var cardImg = card.AddComponent<Image>();
            cardImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 24);

            var cardCol = UIHelper.MakeVertical("Inner", card.transform, 16, new RectOffset(24, 24, 24, 24));
            UIHelper.Stretch(cardCol, 0, 0, 0, 0);

            // Primary score text
            var topRow = UIHelper.MakeHorizontal("TopRow", cardCol, 12);
            UIHelper.SetLayout(topRow.gameObject, preferredHeight: 72);

            var scoreLbl = UIHelper.MakeLabel("label-score", topRow, "95%", 72, UIColors.SafetyGreen, bold: true);
            UIHelper.SetLayout(scoreLbl.gameObject, flexibleWidth: true, flexWidth: 1);

            var passChip = UIHelper.MakeButton("label-pass-chip", topRow, "COMPETENT", 32, UIColors.Hex("#E6F4EC"), UIColors.SafetyGreen, 16);
            UIHelper.SetLayout(passChip.gameObject, preferredWidth: 230, preferredHeight: 56);

            // 4-Column Stats Grid
            var gridRow = UIHelper.MakeHorizontal("StatsGrid", cardCol, 10);
            UIHelper.SetLayout(gridRow.gameObject, preferredHeight: 120);

            BuildStatBox(gridRow, "label-time-val", "00:48", "Time Taken");
            BuildStatBox(gridRow, "label-crit-val", "0", "Critical Errors");
            BuildStatBox(gridRow, "label-steps-val", "7 / 7", "SOP Steps");
            BuildStatBox(gridRow, "label-penalty-val", "-0", "Penalties");
        }

        private static void BuildStatBox(Transform parent, string valLabelName, string valText, string titleText)
        {
            var box = UIHelper.MakeVertical("StatBox", parent, 4, new RectOffset(6, 6, 10, 10));
            UIHelper.SetLayout(box.gameObject, flexibleWidth: true, flexWidth: 1);

            var bg = box.gameObject.AddComponent<Image>();
            bg.color = UIColors.Hex("#F3F5F7");
            UIHelper.SetImageRoundedSprite(bg, 14);

            var val = UIHelper.MakeLabel(valLabelName, box, valText, 38, UIColors.PrimaryDark, TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(val.gameObject, preferredHeight: 46);

            var lbl = UIHelper.MakeLabel("Title", box, titleText, 28, UIColors.TextSecondary, TextAlignmentOptions.Center, wrap: true);
            UIHelper.SetLayout(lbl.gameObject, preferredHeight: 36);
        }

        private static void BuildCompetencyCard(Transform parent)
        {
            var card = new GameObject("CompetencyCard");
            card.transform.SetParent(parent, false);
            UIHelper.SetLayout(card, preferredHeight: 380, minHeight: 360);

            var cardImg = card.AddComponent<Image>();
            cardImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 24);

            var col = UIHelper.MakeVertical("Inner", card.transform, 14, new RectOffset(24, 24, 22, 22));
            UIHelper.Stretch(col, 0, 0, 0, 0);

            var titleLbl = UIHelper.MakeLabel("CompTitle", col, "Competency Breakdown", 42, UIColors.PrimaryDark, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, preferredHeight: 50);

            BuildCompetencyRow(col, "Hazard Recognition", "Strong", UIColors.SafetyGreen, "Pass");
            BuildCompetencyRow(col, "Emergency Alarm Response", "Strong", UIColors.SafetyGreen, "Pass");
            BuildCompetencyRow(col, "Equipment Selection (CO2)", "Strong", UIColors.SafetyGreen, "Pass");
            BuildCompetencyRow(col, "Suppression & Evacuation", "Strong", UIColors.SafetyGreen, "Pass");
        }

        private static void BuildCompetencyRow(Transform parent, string name, string status, Color statusColor, string icon)
        {
            var row = UIHelper.MakeHorizontal("Row", parent, 12);
            UIHelper.SetLayout(row.gameObject, preferredHeight: 60);

            var nameLbl = UIHelper.MakeLabel("Name", row, name, 34, UIColors.TextPrimary, wrap: true);
            UIHelper.SetLayout(nameLbl.gameObject, flexibleWidth: true, flexWidth: 1);

            var statusBadge = UIHelper.MakeHorizontal("Badge", row, 6, new RectOffset(14, 14, 6, 6));
            UIHelper.SetLayout(statusBadge.gameObject, preferredHeight: 46);
            var bImg = statusBadge.gameObject.AddComponent<Image>();
            bImg.color = UIColors.Hex("#E6F4EC");
            UIHelper.SetImageRoundedSprite(bImg, 12);

            var statusLbl = UIHelper.MakeLabel("Status", statusBadge, $"{icon} {status}", 30, statusColor, bold: true);
            UIHelper.SetLayout(statusLbl.gameObject, preferredWidth: 150);
        }

        private static void BuildComparisonCard(Transform parent)
        {
            var card = new GameObject("ComparisonCard");
            card.transform.SetParent(parent, false);
            UIHelper.SetLayout(card, preferredHeight: 190, minHeight: 170);

            var cardImg = card.AddComponent<Image>();
            cardImg.color = UIColors.Hex("#F0F7FF"); // Light calm blue
            UIHelper.SetImageRoundedSprite(cardImg, 20);

            var col = UIHelper.MakeVertical("Inner", card.transform, 6, new RectOffset(22, 22, 18, 18));
            UIHelper.Stretch(col, 0, 0, 0, 0);

            var titleLbl = UIHelper.MakeLabel("CompTitle", col, "Continuous Safety Improvement", 38, UIColors.Primary, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, preferredHeight: 44);

            var descLbl = UIHelper.MakeLabel("label-comp-desc", col,
                "Previous Attempt: 65%  >  Current Performance: 95% (+30% Improvement)",
                32, UIColors.TextPrimary, wrap: true);
            UIHelper.SetLayout(descLbl.gameObject, preferredHeight: 42);

            var subLbl = UIHelper.MakeLabel("Sub", col, "Zero critical errors maintained across all industrial procedures.", 28, UIColors.TextSecondary, wrap: true);
            UIHelper.SetLayout(subLbl.gameObject, preferredHeight: 36);
        }

        private static void BuildActions(Transform parent)
        {
            var actionsCol = UIHelper.MakeVertical("ActionsCol", parent, 16);
            UIHelper.SetLayout(actionsCol.gameObject, preferredHeight: 236);

            // Primary action button (Safety Green)
            var certBtn = UIHelper.MakeButton("btn-view-certificate", actionsCol, "View Official Certificate", 36,
                UIColors.SafetyGreen, Color.white, 22);
            UIHelper.SetLayout(certBtn.gameObject, preferredHeight: 110, minHeight: 100);

            // Secondary action button (Return to Dashboard)
            var homeBtn = UIHelper.MakeButton("btn-home", actionsCol, "Return to Dashboard", 36,
                Color.white, UIColors.PrimaryDark, 22);
            UIHelper.SetLayout(homeBtn.gameObject, preferredHeight: 106, minHeight: 96);
            var border = homeBtn.gameObject.AddComponent<Outline>();
            border.effectColor = UIColors.Border;
            border.effectDistance = new Vector2(1, -1);
        }
    }
}
