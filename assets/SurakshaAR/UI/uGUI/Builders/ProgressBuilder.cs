using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Progress Screen Builder:
    /// Navy header with back button.
    /// Overall completion card with percentage and animated/filled progress bar.
    /// Breakdown list showing module statuses.
    /// </summary>
    public static class ProgressBuilder
    {
        public static GameObject Build()
        {
            var root = new GameObject("ProgressScreen");
            root.AddComponent<RectTransform>();
            var rootImg = root.AddComponent<Image>();
            rootImg.color = UIColors.Background;
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // ── Top Header ───────────────────────────────────────────────
            var header = UIHelper.MakeRect("Header", root.transform);
            header.anchorMin = new Vector2(0, 1);
            header.anchorMax = Vector2.one;
            header.offsetMin = new Vector2(0, -120);
            header.offsetMax = Vector2.zero;

            var headerImg = header.gameObject.AddComponent<Image>();
            headerImg.color = UIColors.Primary;
            headerImg.sprite = UIHelper.GetWhiteSprite();

            var headerRow = UIHelper.MakeHorizontal("Row", header, 16, new RectOffset(24, 24, 0, 0));
            headerRow.anchorMin = Vector2.zero;
            headerRow.anchorMax = Vector2.one;
            headerRow.offsetMin = Vector2.zero;
            headerRow.offsetMax = Vector2.zero;

            var backBtn = UIHelper.MakeButton("btn-back", headerRow, "‹", 38, new Color(1, 1, 1, 0.16f), Color.white, 20);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 54, preferredHeight: 54);

            var titleLbl = UIHelper.MakeLabel("label-title", headerRow, "Training Progress", 26, Color.white, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1);

            // ── Scrollable Body ──────────────────────────────────────────
            var scrollRoot = UIHelper.MakeRect("ScrollArea", root.transform);
            scrollRoot.anchorMin = Vector2.zero;
            scrollRoot.anchorMax = Vector2.one;
            scrollRoot.offsetMin = Vector2.zero;
            scrollRoot.offsetMax = new Vector2(0, -120);

            var scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

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
            content.sizeDelta = new Vector2(0, 1100);
            scrollRect.content = content;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.padding = new RectOffset(28, 28, 28, 40);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── Overall Progress Summary Card ────────────────────────────
            var card = new GameObject("SummaryCard");
            card.transform.SetParent(content, false);
            UIHelper.SetLayout(card, preferredHeight: 180);

            var cardImg = card.AddComponent<Image>();
            cardImg.color = UIColors.Card;
            cardImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cardImg, 22);

            var cardInner = UIHelper.MakeVertical("Inner", card.transform, 12, new RectOffset(24, 24, 24, 24));
            UIHelper.Stretch(cardInner, 0, 0, 0, 0);

            var headRow = UIHelper.MakeHorizontal("HeadRow", cardInner, 12);
            UIHelper.SetLayout(headRow.gameObject, preferredHeight: 34);

            var sumTitle = UIHelper.MakeLabel("label-modules-summary", headRow, "3 of 5 Modules Completed", 22, UIColors.TextPrimary, bold: true);
            UIHelper.SetLayout(sumTitle.gameObject, flexibleWidth: true, flexWidth: 1);

            var pctLbl = UIHelper.MakeLabel("label-percentage", headRow, "60%", 28, UIColors.Primary, TextAlignmentOptions.Right, bold: true);
            UIHelper.SetLayout(pctLbl.gameObject, preferredWidth: 90);

            // Progress Bar Track
            var track = UIHelper.MakeRect("Track", cardInner);
            UIHelper.SetLayout(track.gameObject, preferredHeight: 16);
            var trackImg = track.gameObject.AddComponent<Image>();
            trackImg.color = UIColors.Border;
            trackImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(trackImg, 8);

            // Progress Bar Fill
            var fill = UIHelper.MakeRect("progress-bar-fill", track);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0.6f, 1);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            var fillImg = fill.gameObject.AddComponent<Image>();
            fillImg.color = UIColors.Primary;
            fillImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(fillImg, 8);

            var subTip = UIHelper.MakeLabel("SubTip", cardInner, "Complete remaining modules to renew your annual safety certification.", 17, UIColors.TextSecondary);
            UIHelper.SetLayout(subTip.gameObject, preferredHeight: 24);

            // ── Section Title: Detailed Modules ──────────────────────────
            var secLbl = UIHelper.MakeLabel("SecLbl", content, "Module Status Breakdown", 24, UIColors.TextPrimary, bold: true);
            UIHelper.SetLayout(secLbl.gameObject, preferredHeight: 36);

            // Module list items
            MakeProgressItem(content, "🔥", "Fire & Explosion Response", "Passed - 100%", UIColors.Success);
            MakeProgressItem(content, "☁️", "Gas Leak & Confined Space", "Passed - 90%", UIColors.Success);
            MakeProgressItem(content, "⚙️", "Machinery Safety", "Passed - 85%", UIColors.Success);
            MakeProgressItem(content, "⚡", "Electrical Hazards", "Not Started", UIColors.TextMuted);
            MakeProgressItem(content, "🧱", "Roof & Strata Control", "Locked", UIColors.TextMuted);

            // ── Section Title: Retention Tracking ────────────────────────
            var retLbl = UIHelper.MakeLabel("RetLbl", content, "🧠 Knowledge Retention Tracking", 24, UIColors.TextPrimary, bold: true);
            UIHelper.SetLayout(retLbl.gameObject, preferredHeight: 36);

            var retCard = new GameObject("RetentionCard");
            retCard.transform.SetParent(content, false);
            UIHelper.SetLayout(retCard, preferredHeight: 190);
            var retImg = retCard.AddComponent<Image>();
            retImg.color = UIColors.Card;
            UIHelper.SetImageRoundedSprite(retImg, 18);

            var retCol = UIHelper.MakeVertical("Inner", retCard.transform, 10, new RectOffset(20, 20, 16, 16));
            UIHelper.Stretch(retCol, 0, 0, 0, 0);

            MakeRetentionRow(retCol, "Day 1 Retention Check", "Passed (100% Score)", UIColors.Success, "✓");
            MakeRetentionRow(retCol, "Day 7 Refresher Check", "Scheduled (Due in 4 days)", UIColors.Primary, "⏱");
            MakeRetentionRow(retCol, "Day 30 Competency Audit", "Pending Schedule", UIColors.TextMuted, "○");

            return root;
        }

        private static void MakeRetentionRow(Transform parent, string title, string status, Color color, string icon)
        {
            var row = UIHelper.MakeHorizontal("Row", parent, 10);
            UIHelper.SetLayout(row.gameObject, preferredHeight: 32);

            var iconLbl = UIHelper.MakeLabel("Icon", row, icon, 18, color, TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(iconLbl.gameObject, preferredWidth: 26);

            var titleLbl = UIHelper.MakeLabel("Title", row, title, 16, UIColors.TextPrimary);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1);

            var statLbl = UIHelper.MakeLabel("Status", row, status, 14, color, TextAlignmentOptions.Right, bold: true);
            UIHelper.SetLayout(statLbl.gameObject, preferredWidth: 190);
        }

        private static void MakeProgressItem(Transform parent, string icon, string title, string status, Color statusColor)
        {
            var item = new GameObject("ProgressItem");
            item.transform.SetParent(parent, false);
            UIHelper.SetLayout(item, preferredHeight: 76);

            var itemImg = item.AddComponent<Image>();
            itemImg.color = UIColors.Card;
            itemImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(itemImg, 16);

            var inner = UIHelper.MakeHorizontal("Inner", item.transform, 16, new RectOffset(18, 18, 12, 12));
            UIHelper.Stretch(inner, 0, 0, 0, 0);

            var iconLbl = UIHelper.MakeLabel("Icon", inner, icon, 26, Color.white, TextAlignmentOptions.Center);
            UIHelper.SetLayout(iconLbl.gameObject, preferredWidth: 36, preferredHeight: 36);

            var titleLbl = UIHelper.MakeLabel("Title", inner, title, 19, UIColors.TextPrimary, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1);

            var statLbl = UIHelper.MakeLabel("Status", inner, status, 17, statusColor, TextAlignmentOptions.Right, bold: true);
            UIHelper.SetLayout(statLbl.gameObject, preferredWidth: 140);
        }
    }
}
