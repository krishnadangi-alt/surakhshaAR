using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Upgraded Certificate Screen Builder:
    /// Dynamic Worker Certificates List & Latest Certificate Download.
    /// In compliance with Rules 53-57:
    /// - Dynamic list for the currently logged-in worker
    /// - Sorted by newest issued first
    /// - Latest Certificate spotlight card with direct PDF download
    /// - Clear status badges (PENDING_REVIEW, ISSUED, REJECTED, REVOKED)
    /// - Authentic empty / pending states
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

            // ── Scrollable Area ──────────────────────────────────────────
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

            // ── 2. Worker Identity Header ─────────────────────────────────
            BuildWorkerHeader(content);

            // ── 3. Latest Certificate Spotlight Card ──────────────────────
            BuildLatestCertificateSpotlight(content);

            // ── 4. Dynamic List Container ─────────────────────────────────
            BuildCertificatesListHeader(content);
            BuildCertificatesContainer(content);

            // ── 5. Empty / Status State Container ─────────────────────────
            BuildEmptyStateContainer(content);

            // ── 6. Bottom Action Buttons ──────────────────────────────────
            BuildActionButtons(content);

            return root;
        }

        private static void BuildTopBar(Transform parent)
        {
            var row = UIHelper.MakeHorizontal("TopBarRow", parent, 14);
            UIHelper.SetLayout(row.gameObject, preferredHeight: 68);

            var backBtn = UIHelper.MakeButton("btn-back", row, "‹", 38, Color.white, UIColors.PrimaryDark, 18);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 56, preferredHeight: 56);
            var backBorder = backBtn.gameObject.AddComponent<Outline>();
            backBorder.effectColor = UIColors.Border;
            backBorder.effectDistance = new Vector2(1, -1);

            var titleLbl = UIHelper.MakeLabel("label-title", row, "My Safety Certificates", 34, UIColors.PrimaryDark, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 44);
        }

        private static void BuildWorkerHeader(Transform parent)
        {
            var card = UIHelper.MakeHorizontal("WorkerHeaderCard", parent, 14, new RectOffset(18, 18, 14, 14));
            UIHelper.SetLayout(card.gameObject, preferredHeight: 88, minHeight: 80);
            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = UIColors.Card;
            UIHelper.SetImageRoundedSprite(cardImg, 18);

            var infoCol = UIHelper.MakeVertical("WorkerInfoCol", card, 3);
            UIHelper.SetLayout(infoCol.gameObject, flexibleWidth: true, flexWidth: 1);

            var nameLbl = UIHelper.MakeLabel("label-worker-name", infoCol, "Krishna", 30, UIColors.PrimaryDark, bold: true);
            UIHelper.SetLayout(nameLbl.gameObject, preferredHeight: 36);

            var idLbl = UIHelper.MakeLabel("label-worker-id", infoCol, "Mine Worker | ID: EMP-PROD-CERT", 22, UIColors.TextSecondary);
            UIHelper.SetLayout(idLbl.gameObject, preferredHeight: 28);

            var badgeBox = UIHelper.MakeRect("MineBadgeBox", card);
            UIHelper.SetLayout(badgeBox.gameObject, preferredWidth: 145, preferredHeight: 38);
            var badgeImg = badgeBox.gameObject.AddComponent<Image>();
            badgeImg.color = UIColors.Hex("#FEF3C7");
            UIHelper.SetImageRoundedSprite(badgeImg, 12);
            var badgeText = UIHelper.MakeLabel("label-mine-site", badgeBox, "Dhanbad Colliery", 20, UIColors.Hex("#B45309"), TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(badgeText.GetComponent<RectTransform>(), 0, 0, 0, 0);
        }

        private static void BuildLatestCertificateSpotlight(Transform parent)
        {
            var card = UIHelper.MakeVertical("LatestCertSpotlight", parent, 12, new RectOffset(20, 20, 20, 20));
            UIHelper.SetLayout(card.gameObject, preferredHeight: 880, minHeight: 850);
            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 22);

            var border = card.gameObject.AddComponent<Outline>();
            border.effectColor = UIColors.Hex("#D97706");
            border.effectDistance = new Vector2(1.5f, -1.5f);

            // Title row
            var titleRow = UIHelper.MakeHorizontal("SpotlightTitleRow", card, 8);
            UIHelper.SetLayout(titleRow.gameObject, preferredHeight: 28);
            var badge = UIHelper.MakeLabel("SpotlightBadge", titleRow, "OFFICIAL DGMS COMPLIANCE CREDENTIAL", 20, UIColors.Hex("#B45309"), bold: true);
            UIHelper.SetLayout(badge.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 26);

            // Recipient / Worker Name
            var workerLbl = UIHelper.MakeLabel("label-latest-worker", card, "Certified Recipient: Krishna", 24, UIColors.Hex("#0F172A"), bold: true);
            UIHelper.SetLayout(workerLbl.gameObject, preferredHeight: 30);

            // Module Title
            var modTitle = UIHelper.MakeLabel("label-latest-module", card, "Fire & Explosion Response", 30, UIColors.PrimaryDark, bold: true, wrap: true);
            UIHelper.SetLayout(modTitle.gameObject, preferredHeight: 38);

            // Score & Grade
            var scoreLbl = UIHelper.MakeLabel("label-latest-score", card, "Competency: Grade A • Competent (Score: 90/100)", 22, UIColors.SafetyGreen, bold: true);
            UIHelper.SetLayout(scoreLbl.gameObject, preferredHeight: 28);

            // Cert Number & Date
            var metaLbl = UIHelper.MakeLabel("label-latest-meta", card, "Certificate ID: SUR-2026-0002  •  Issued: 21 Sep 2026", 20, UIColors.TextSecondary);
            UIHelper.SetLayout(metaLbl.gameObject, preferredHeight: 26);

            // ── Official Certificate Preview Frame ────────────────────────
            var certPreviewBox = UIHelper.MakeVertical("CertPreviewBox", card, 6, new RectOffset(6, 6, 6, 6));
            UIHelper.SetLayout(certPreviewBox.gameObject, preferredHeight: 360);
            var certPreviewBoxImg = certPreviewBox.gameObject.AddComponent<Image>();
            certPreviewBoxImg.color = UIColors.Hex("#F8FAFC");
            UIHelper.SetImageRoundedSprite(certPreviewBoxImg, 14);
            var previewOutline = certPreviewBox.gameObject.AddComponent<Outline>();
            previewOutline.effectColor = UIColors.Hex("#CBD5E1");
            previewOutline.effectDistance = new Vector2(1, -1);

            var certImgGO = UIHelper.MakeRect("image-latest-cert-preview", certPreviewBox);
            UIHelper.SetLayout(certImgGO.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 348);
            var certImg = certImgGO.gameObject.AddComponent<Image>();
            certImg.preserveAspect = true;
            certImg.color = Color.white;
            Texture2D defaultCertTex = Resources.Load<Texture2D>("Images/certificate_preview_krishna");
            if (defaultCertTex != null)
            {
                certImg.sprite = Sprite.Create(defaultCertTex, new Rect(0, 0, defaultCertTex.width, defaultCertTex.height), new Vector2(0.5f, 0.5f));
            }

            // ── QR Code Showcase Section ──────────────────────────────────
            var qrRow = UIHelper.MakeHorizontal("SpotlightQrRow", card, 14);
            UIHelper.SetLayout(qrRow.gameObject, preferredHeight: 160);

            var qrBox = UIHelper.MakeVertical("QrBox", qrRow, 6, new RectOffset(8, 8, 8, 8));
            UIHelper.SetLayout(qrBox.gameObject, preferredWidth: 150, preferredHeight: 150);
            var qrBoxImg = qrBox.gameObject.AddComponent<Image>();
            qrBoxImg.color = UIColors.Hex("#F8FAFC");
            UIHelper.SetImageRoundedSprite(qrBoxImg, 14);
            var qrBoxOutline = qrBox.gameObject.AddComponent<Outline>();
            qrBoxOutline.effectColor = UIColors.Hex("#E2E8F0");
            qrBoxOutline.effectDistance = new Vector2(1, -1);

            // QR Image
            var qrImgGO = UIHelper.MakeRect("image-latest-qr", qrBox);
            UIHelper.SetLayout(qrImgGO.gameObject, preferredWidth: 134, preferredHeight: 134);
            var qrImg = qrImgGO.gameObject.AddComponent<Image>();
            qrImg.color = Color.white;
            qrImg.sprite = UIHelper.GetQRSprite();

            // QR Description / Instructions Column
            var qrInfoCol = UIHelper.MakeVertical("QrInfoCol", qrRow, 6);
            UIHelper.SetLayout(qrInfoCol.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 150);

            var qrTitle = UIHelper.MakeLabel("label-qr-title", qrInfoCol, "Scan to Verify Credential", 24, UIColors.PrimaryDark, bold: true);
            UIHelper.SetLayout(qrTitle.gameObject, preferredHeight: 28);

            var qrSub = UIHelper.MakeLabel("label-qr-sub", qrInfoCol, "Scan this QR code using Google Lens or camera to view official compliance certificate directly.", 20, UIColors.TextSecondary, wrap: true);
            qrSub.lineSpacing = 1.1f;
            UIHelper.SetLayout(qrSub.gameObject, preferredHeight: 52);

            var btnViewImage = UIHelper.MakeButton("btn-view-image", qrInfoCol, "Open Certificate Image >", 20, UIColors.Hex("#F59E0B"), UIColors.Hex("#0F172A"), 12);
            UIHelper.SetLayout(btnViewImage.gameObject, preferredHeight: 46);

            // Action Buttons Row (PDF & Online Verify)
            var btnRow = UIHelper.MakeHorizontal("LatestBtnRow", card, 12);
            UIHelper.SetLayout(btnRow.gameObject, preferredHeight: 56);

            var dlBtn = UIHelper.MakeButton("btn-download-latest", btnRow, "Download Official PDF", 22, UIColors.SafetyGreen, Color.white, 14);
            UIHelper.SetLayout(dlBtn.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 56);

            var viewBtn = UIHelper.MakeButton("btn-view-latest", btnRow, "Verify Online", 22, Color.white, UIColors.PrimaryDark, 14);
            UIHelper.SetLayout(viewBtn.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 56);
            var viewBorder = viewBtn.gameObject.AddComponent<Outline>();
            viewBorder.effectColor = UIColors.Border;
            viewBorder.effectDistance = new Vector2(1, -1);
        }

        private static void BuildCertificatesListHeader(Transform parent)
        {
            var lbl = UIHelper.MakeLabel("label-section-all", parent, "All Safety Credentials", 28, UIColors.PrimaryDark, bold: true);
            UIHelper.SetLayout(lbl.gameObject, preferredHeight: 36);
        }

        private static void BuildCertificatesContainer(Transform parent)
        {
            var container = UIHelper.MakeVertical("CertificatesListContainer", parent, 14);
            UIHelper.SetLayout(container.gameObject, flexibleWidth: true, flexWidth: 1);
            var csf = container.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private static void BuildEmptyStateContainer(Transform parent)
        {
            var box = UIHelper.MakeVertical("EmptyStateContainer", parent, 8, new RectOffset(20, 20, 22, 22));
            UIHelper.SetLayout(box.gameObject, preferredHeight: 140, minHeight: 130);
            var boxImg = box.gameObject.AddComponent<Image>();
            boxImg.color = UIColors.Card;
            UIHelper.SetImageRoundedSprite(boxImg, 18);

            var title = UIHelper.MakeLabel("label-empty-title", box, "No Issued Certificates Yet", 24, UIColors.PrimaryDark, TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(title.gameObject, preferredHeight: 30);

            var desc = UIHelper.MakeLabel("label-empty-desc", box,
                "Complete the practical AR training and assessment. Once reviewed and approved by a safety officer, your official certificate will appear here.",
                20, UIColors.TextSecondary, TextAlignmentOptions.Center, wrap: true);
            desc.lineSpacing = 1.1f;
            UIHelper.SetLayout(desc.gameObject, preferredHeight: 54);
        }

        private static void BuildActionButtons(Transform parent)
        {
            var col = UIHelper.MakeVertical("ActionButtons", parent, 10);
            UIHelper.SetLayout(col.gameObject, preferredHeight: 126);

            var refreshBtn = UIHelper.MakeButton("btn-refresh-certs", col, "Sync & Refresh Certificates", 22,
                UIColors.Hex("#F3F5F7"), UIColors.TextPrimary, 16);
            UIHelper.SetLayout(refreshBtn.gameObject, preferredHeight: 56);

            var homeBtn = UIHelper.MakeButton("btn-back-home", col, "Return to Dashboard", 22,
                UIColors.PrimaryDark, Color.white, 16);
            UIHelper.SetLayout(homeBtn.gameObject, preferredHeight: 56);
        }
    }
}
