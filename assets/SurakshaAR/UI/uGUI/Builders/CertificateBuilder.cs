using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Upgraded Certificate Screen Builder:
    /// Official Government of India & Ministry of Mines Industrial Safety Certification.
    /// Includes:
    /// - Formal National Emblem / Seal header
    /// - Worker Name & Mine Employee ID
    /// - Module certification details & Score
    /// - QR Code Verification box & unique Certificate ID
    /// - Action buttons (Download PDF, Share Verification, Return to Dashboard)
    /// </summary>
    public static class CertificateBuilder
    {
        public static GameObject Build()
        {
            var root = new GameObject("CertificateScreen");
            root.AddComponent<RectTransform>();
            var rootImg = root.AddComponent<Image>();
            rootImg.color = UIColors.Background;
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // ── Scrollable Certificate Card ──────────────────────────────
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
            content.sizeDelta = new Vector2(0, 1500);
            scrollRect.content = content;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.padding = new RectOffset(24, 24, 36, 44);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── 1. Top Bar Navigation ─────────────────────────────────────
            BuildTopBar(content);

            // ── 2. Official Formal Certificate Document Card ──────────────
            BuildCertificateCard(content);

            // ── 3. Bottom Action Buttons (Download, Share, Dashboard) ──────
            BuildActionButtons(content);

            return root;
        }

        private static void BuildTopBar(Transform parent)
        {
            var row = UIHelper.MakeHorizontal("TopBarRow", parent, 16);
            UIHelper.SetLayout(row.gameObject, preferredHeight: 56);

            var backBtn = UIHelper.MakeButton("btn-back", row, "‹", 38, Color.white, UIColors.PrimaryDark, 16);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 54, preferredHeight: 54);
            var backBorder = backBtn.gameObject.AddComponent<Outline>();
            backBorder.effectColor = UIColors.Border;
            backBorder.effectDistance = new Vector2(1, -1);

            var titleLbl = UIHelper.MakeLabel("label-title", row, "Official Safety Certificate", 22, UIColors.PrimaryDark, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1);
        }

        private static void BuildCertificateCard(Transform parent)
        {
            var card = new GameObject("CertCard");
            card.transform.SetParent(parent, false);
            var cardImg = card.AddComponent<Image>();
            cardImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 24);

            var border = card.AddComponent<Outline>();
            border.effectColor = UIColors.Hex("#E2E8F0");
            border.effectDistance = new Vector2(2, -2);

            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(28, 28, 32, 32);
            vlg.spacing = 14;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            var csf = card.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Transform inner = card.transform;

            // Header Emblem & Ministry
            var govHeader = UIHelper.MakeVertical("GovHeader", inner, 4);
            UIHelper.SetLayout(govHeader.gameObject, preferredHeight: 64);

            var govLbl = UIHelper.MakeLabel("GovLbl", govHeader, "GOVERNMENT OF INDIA", 14, UIColors.PrimaryDark, TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(govLbl.gameObject, preferredHeight: 18);

            var minLbl = UIHelper.MakeLabel("MinLbl", govHeader, "MINISTRY OF MINES • SURAKSHA-AR", 12, UIColors.SafetyGreen, TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(minLbl.gameObject, preferredHeight: 18);

            // Gold Emblem Circle
            var sealBox = UIHelper.MakeRect("SealBox", inner);
            UIHelper.SetLayout(sealBox.gameObject, preferredHeight: 74);
            var sealCircle = UIHelper.MakeRect("SealCircle", sealBox);
            sealCircle.anchorMin = new Vector2(0.5f, 0.5f);
            sealCircle.anchorMax = new Vector2(0.5f, 0.5f);
            sealCircle.sizeDelta = new Vector2(70, 70);
            var sealImg = sealCircle.gameObject.AddComponent<Image>();
            sealImg.sprite = UIHelper.GetCircleSprite();
            sealImg.color = UIColors.Hex("#FEF3C7"); // Warm Gold

            var sealIconGO = new GameObject("SealIcon");
            sealIconGO.transform.SetParent(sealCircle, false);
            var sealIconImg = sealIconGO.AddComponent<Image>();
            sealIconImg.sprite = UIHelper.GetShieldSprite();
            sealIconImg.color = UIColors.PrimaryDark;
            UIHelper.AnchorCenter(sealIconGO.GetComponent<RectTransform>(), 40, 40);

            // Title
            var certTitle = UIHelper.MakeLabel("CertTitle", inner, "CERTIFICATE OF SAFETY COMPETENCY", 22, UIColors.PrimaryDark, TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(certTitle.gameObject, preferredHeight: 30);

            var certSub = UIHelper.MakeLabel("CertSub", inner, "This is to certify that industrial worker", 15, UIColors.TextSecondary, TextAlignmentOptions.Center);
            UIHelper.SetLayout(certSub.gameObject, preferredHeight: 20);

            // Worker Name
            var nameLbl = UIHelper.MakeLabel("label-worker-name", inner, "Trainee Worker", 28, UIColors.PrimaryDark, TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(nameLbl.gameObject, preferredHeight: 36);

            // Worker ID
            var idLbl = UIHelper.MakeLabel("label-worker-id", inner, "Mine Worker | ID: Unassigned", 16, UIColors.TextSecondary, TextAlignmentOptions.Center);
            UIHelper.SetLayout(idLbl.gameObject, preferredHeight: 22);

            var descLbl = UIHelper.MakeLabel("DescLbl", inner,
                "has demonstrated validated, certified practical safety proficiency in accordance with the National Mining Safety Standards for:",
                14, UIColors.TextSecondary, TextAlignmentOptions.Center);
            UIHelper.SetLayout(descLbl.gameObject, preferredHeight: 40);

            // Module Title Badge
            var modBadge = UIHelper.MakeVertical("ModBadge", inner, 4, new RectOffset(16, 16, 12, 12));
            UIHelper.SetLayout(modBadge.gameObject, preferredHeight: 58);
            var bImg = modBadge.gameObject.AddComponent<Image>();
            bImg.color = UIColors.Hex("#F0FDF4"); // Soft mint green
            UIHelper.SetImageRoundedSprite(bImg, 12);

            var modTitle = UIHelper.MakeLabel("label-module-title", modBadge, "Fire & Explosion Response", 18, UIColors.SafetyGreen, TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(modTitle.gameObject, preferredHeight: 24);

            var scoreText = UIHelper.MakeLabel("label-score-grade", modBadge, "Competency Grade: Level 1 (95% Score • 0 Critical Errors)", 13, UIColors.PrimaryDark, TextAlignmentOptions.Center);
            UIHelper.SetLayout(scoreText.gameObject, preferredHeight: 18);

            // Verification & QR Box
            var qrRow = UIHelper.MakeHorizontal("QRRow", inner, 16, new RectOffset(16, 16, 14, 14));
            UIHelper.SetLayout(qrRow.gameObject, preferredHeight: 110);
            var qrBg = qrRow.gameObject.AddComponent<Image>();
            qrBg.color = UIColors.Hex("#F8FAFC");
            UIHelper.SetImageRoundedSprite(qrBg, 12);

            // QR Box Graphic
            var qrBox = UIHelper.MakeRect("QRBox", qrRow);
            UIHelper.SetLayout(qrBox.gameObject, preferredWidth: 80, preferredHeight: 80);
            var qrImg = qrBox.gameObject.AddComponent<Image>();
            qrImg.color = UIColors.PrimaryDark;
            UIHelper.SetImageRoundedSprite(qrImg, 8);

            var qrText = UIHelper.MakeLabel("QRText", qrBox, "QR", 22, Color.white, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(qrText.GetComponent<RectTransform>(), 0, 0, 0, 0);

            var qrInfoCol = UIHelper.MakeVertical("QRInfo", qrRow, 4);
            UIHelper.SetLayout(qrInfoCol.gameObject, flexibleWidth: true, flexWidth: 1);

            var certIdLbl = UIHelper.MakeLabel("label-cert-id", qrInfoCol, "Cert ID: IND-SAR-2026-0941", 13, UIColors.PrimaryDark, bold: true);
            UIHelper.SetLayout(certIdLbl.gameObject, preferredHeight: 18);

            var dateLbl = UIHelper.MakeLabel("label-cert-date", qrInfoCol, "Date Issued: 12 Sept 2026", 12, UIColors.TextSecondary);
            UIHelper.SetLayout(dateLbl.gameObject, preferredHeight: 16);

            var statusLbl = UIHelper.MakeLabel("StatusLbl", qrInfoCol, "Status: Cryptographically Verified", 12, UIColors.SafetyGreen, bold: true);
            UIHelper.SetLayout(statusLbl.gameObject, preferredHeight: 16);
        }

        private static void BuildActionButtons(Transform parent)
        {
            var col = UIHelper.MakeVertical("ActionButtons", parent, 12);
            UIHelper.SetLayout(col.gameObject, preferredHeight: 180, minHeight: 188);

            // Download Button
            var downloadBtn = UIHelper.MakeButton("btn-download-cert", col, "Download Certificate (PDF)", 18,
                UIColors.SafetyGreen, Color.white, 16);
            UIHelper.SetLayout(downloadBtn.gameObject, preferredHeight: 58, minHeight: 50);

            // Share Button
            var shareBtn = UIHelper.MakeButton("btn-share-cert", col, "Share Official Verification", 17,
                Color.white, UIColors.PrimaryDark, 16);
            UIHelper.SetLayout(shareBtn.gameObject, preferredHeight: 54, minHeight: 46);
            var shareBorder = shareBtn.gameObject.AddComponent<Outline>();
            shareBorder.effectColor = UIColors.Border;
            shareBorder.effectDistance = new Vector2(1, -1);

            // Return to Home Button
            var homeBtn = UIHelper.MakeButton("btn-back-home", col, "Return to Dashboard", 17,
                UIColors.Hex("#F3F5F7"), UIColors.TextPrimary, 16);
            UIHelper.SetLayout(homeBtn.gameObject, preferredHeight: 52, minHeight: 44);
        }
    }
}
