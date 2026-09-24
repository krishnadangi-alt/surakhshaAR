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
            var row = UIHelper.MakeHorizontal("TopBarRow", parent, 18);
            UIHelper.SetLayout(row.gameObject, preferredHeight: 84);

            var backBtn = UIHelper.MakeButton("btn-back", row, "‹", 52, Color.white, UIColors.PrimaryDark, 20);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 76, preferredHeight: 76);
            var backBorder = backBtn.gameObject.AddComponent<Outline>();
            backBorder.effectColor = UIColors.Border;
            backBorder.effectDistance = new Vector2(1, -1);

            var titleLbl = UIHelper.MakeLabel("label-title", row, "My Safety Certificates", 52, UIColors.PrimaryDark, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1);
        }

        private static void BuildWorkerHeader(Transform parent)
        {
            var card = UIHelper.MakeHorizontal("WorkerHeaderCard", parent, 16, new RectOffset(20, 20, 16, 16));
            UIHelper.SetLayout(card.gameObject, preferredHeight: 110, minHeight: 96);
            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = UIColors.Card;
            UIHelper.SetImageRoundedSprite(cardImg, 20);

            var infoCol = UIHelper.MakeVertical("WorkerInfoCol", card, 4);
            UIHelper.SetLayout(infoCol.gameObject, flexibleWidth: true, flexWidth: 1);

            var nameLbl = UIHelper.MakeLabel("label-worker-name", infoCol, "Krishna", 42, UIColors.PrimaryDark, bold: true);
            UIHelper.SetLayout(nameLbl.gameObject, preferredHeight: 48);

            var idLbl = UIHelper.MakeLabel("label-worker-id", infoCol, "Mine Worker | ID: EMP-PROD-CERT", 30, UIColors.TextSecondary);
            UIHelper.SetLayout(idLbl.gameObject, preferredHeight: 34);

            var badgeBox = UIHelper.MakeRect("MineBadgeBox", card);
            UIHelper.SetLayout(badgeBox.gameObject, preferredWidth: 160, preferredHeight: 48);
            var badgeImg = badgeBox.gameObject.AddComponent<Image>();
            badgeImg.color = UIColors.Hex("#FEF3C7");
            UIHelper.SetImageRoundedSprite(badgeImg, 14);
            var badgeText = UIHelper.MakeLabel("label-mine-site", badgeBox, "Dhanbad Colliery", 26, UIColors.Hex("#B45309"), TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(badgeText.GetComponent<RectTransform>(), 0, 0, 0, 0);
        }

        private static void BuildLatestCertificateSpotlight(Transform parent)
        {
            var card = UIHelper.MakeVertical("LatestCertSpotlight", parent, 16, new RectOffset(24, 24, 24, 24));
            UIHelper.SetLayout(card.gameObject, preferredHeight: 1120, minHeight: 1040);
            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 24);

            var border = card.gameObject.AddComponent<Outline>();
            border.effectColor = UIColors.Hex("#D97706");
            border.effectDistance = new Vector2(2f, -2f);

            // Title row
            var titleRow = UIHelper.MakeHorizontal("SpotlightTitleRow", card, 8);
            UIHelper.SetLayout(titleRow.gameObject, preferredHeight: 38);
            var badge = UIHelper.MakeLabel("SpotlightBadge", titleRow, "★ OFFICIAL DGMS COMPLIANCE CREDENTIAL", 28, UIColors.Hex("#B45309"), bold: true);
            UIHelper.SetLayout(badge.gameObject, flexibleWidth: true, flexWidth: 1);

            // Recipient / Worker Name
            var workerLbl = UIHelper.MakeLabel("label-latest-worker", card, "Certified Recipient: Krishna", 36, UIColors.Hex("#0F172A"), bold: true);
            UIHelper.SetLayout(workerLbl.gameObject, preferredHeight: 44);

            // Module Title
            var modTitle = UIHelper.MakeLabel("label-latest-module", card, "Fire & Explosion Response", 42, UIColors.PrimaryDark, bold: true, wrap: true);
            UIHelper.SetLayout(modTitle.gameObject, preferredHeight: 50);

            // Score & Grade
            var scoreLbl = UIHelper.MakeLabel("label-latest-score", card, "Competency: Grade A • Competent (Score: 90/100)", 32, UIColors.SafetyGreen, bold: true);
            UIHelper.SetLayout(scoreLbl.gameObject, preferredHeight: 38);

            // Cert Number & Date
            var metaLbl = UIHelper.MakeLabel("label-latest-meta", card, "Certificate ID: SUR-2026-0002  •  Issued: 21 Sep 2026", 28, UIColors.TextSecondary);
            UIHelper.SetLayout(metaLbl.gameObject, preferredHeight: 34);

            // ── Official Certificate Preview Frame ────────────────────────
            var certPreviewBox = UIHelper.MakeVertical("CertPreviewBox", card, 6, new RectOffset(8, 8, 8, 8));
            UIHelper.SetLayout(certPreviewBox.gameObject, preferredHeight: 440);
            var certPreviewBoxImg = certPreviewBox.gameObject.AddComponent<Image>();
            certPreviewBoxImg.color = UIColors.Hex("#F8FAFC");
            UIHelper.SetImageRoundedSprite(certPreviewBoxImg, 16);
            var previewOutline = certPreviewBox.gameObject.AddComponent<Outline>();
            previewOutline.effectColor = UIColors.Hex("#CBD5E1");
            previewOutline.effectDistance = new Vector2(1, -1);

            var certImgGO = UIHelper.MakeRect("image-latest-cert-preview", certPreviewBox);
            UIHelper.SetLayout(certImgGO.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 424);
            var certImg = certImgGO.gameObject.AddComponent<Image>();
            certImg.preserveAspect = true;
            certImg.color = Color.white;
            Texture2D defaultCertTex = Resources.Load<Texture2D>("Images/certificate_preview_krishna");
            if (defaultCertTex != null)
            {
                certImg.sprite = Sprite.Create(defaultCertTex, new Rect(0, 0, defaultCertTex.width, defaultCertTex.height), new Vector2(0.5f, 0.5f));
            }

            // ── QR Code Showcase Section ──────────────────────────────────
            var qrRow = UIHelper.MakeHorizontal("SpotlightQrRow", card, 16);
            UIHelper.SetLayout(qrRow.gameObject, preferredHeight: 210);

            var qrBox = UIHelper.MakeVertical("QrBox", qrRow, 6, new RectOffset(10, 10, 10, 10));
            UIHelper.SetLayout(qrBox.gameObject, preferredWidth: 190, preferredHeight: 200);
            var qrBoxImg = qrBox.gameObject.AddComponent<Image>();
            qrBoxImg.color = UIColors.Hex("#F8FAFC");
            UIHelper.SetImageRoundedSprite(qrBoxImg, 16);
            var qrBoxOutline = qrBox.gameObject.AddComponent<Outline>();
            qrBoxOutline.effectColor = UIColors.Hex("#E2E8F0");
            qrBoxOutline.effectDistance = new Vector2(1, -1);

            // QR Image
            var qrImgGO = UIHelper.MakeRect("image-latest-qr", qrBox);
            UIHelper.SetLayout(qrImgGO.gameObject, preferredWidth: 170, preferredHeight: 170);
            var qrImg = qrImgGO.gameObject.AddComponent<Image>();
            qrImg.color = Color.white;
            qrImg.sprite = UIHelper.GetQRSprite();

            // QR Description / Instructions Column
            var qrInfoCol = UIHelper.MakeVertical("QrInfoCol", qrRow, 6);
            UIHelper.SetLayout(qrInfoCol.gameObject, flexibleWidth: true, flexWidth: 1);

            var qrTitle = UIHelper.MakeLabel("label-qr-title", qrInfoCol, "Scan to Verify Credential", 32, UIColors.PrimaryDark, bold: true);
            UIHelper.SetLayout(qrTitle.gameObject, preferredHeight: 36);

            var qrSub = UIHelper.MakeLabel("label-qr-sub", qrInfoCol, "Scan this QR code using Google Lens or any smartphone camera to open the official public verification page directly.", 26, UIColors.TextSecondary, wrap: true);
            UIHelper.SetLayout(qrSub.gameObject, flexibleHeight: true, flexHeight: 1);

            var btnViewImage = UIHelper.MakeButton("btn-view-image", qrInfoCol, "Open Certificate Image ↗", 30, UIColors.Hex("#F59E0B"), UIColors.Hex("#0F172A"), 14);
            UIHelper.SetLayout(btnViewImage.gameObject, preferredHeight: 64);

            // Action Buttons Row (PDF & Online Verify)
            var btnRow = UIHelper.MakeHorizontal("LatestBtnRow", card, 14);
            UIHelper.SetLayout(btnRow.gameObject, preferredHeight: 84);

            var dlBtn = UIHelper.MakeButton("btn-download-latest", btnRow, "Download Official PDF", 32, UIColors.SafetyGreen, Color.white, 16);
            UIHelper.SetLayout(dlBtn.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 84);

            var viewBtn = UIHelper.MakeButton("btn-view-latest", btnRow, "Verify Online", 32, Color.white, UIColors.PrimaryDark, 16);
            UIHelper.SetLayout(viewBtn.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 84);
            var viewBorder = viewBtn.gameObject.AddComponent<Outline>();
            viewBorder.effectColor = UIColors.Border;
            viewBorder.effectDistance = new Vector2(1, -1);
        }

        private static void BuildCertificatesListHeader(Transform parent)
        {
            var lbl = UIHelper.MakeLabel("label-section-all", parent, "All Safety Credentials", 42, UIColors.PrimaryDark, bold: true);
            UIHelper.SetLayout(lbl.gameObject, preferredHeight: 48);
        }

        private static void BuildCertificatesContainer(Transform parent)
        {
            var container = UIHelper.MakeVertical("CertificatesListContainer", parent, 16);
            UIHelper.SetLayout(container.gameObject, flexibleWidth: true, flexWidth: 1);
            var csf = container.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private static void BuildEmptyStateContainer(Transform parent)
        {
            var box = UIHelper.MakeVertical("EmptyStateContainer", parent, 12, new RectOffset(24, 24, 32, 32));
            UIHelper.SetLayout(box.gameObject, preferredHeight: 200, minHeight: 180);
            var boxImg = box.gameObject.AddComponent<Image>();
            boxImg.color = UIColors.Card;
            UIHelper.SetImageRoundedSprite(boxImg, 20);

            var title = UIHelper.MakeLabel("label-empty-title", box, "No Issued Certificates Yet", 36, UIColors.PrimaryDark, TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(title.gameObject, preferredHeight: 44);

            var desc = UIHelper.MakeLabel("label-empty-desc", box,
                "Complete the practical AR training and assessment. Once reviewed and approved by a safety officer, your official certificate will appear here.",
                30, UIColors.TextSecondary, TextAlignmentOptions.Center, wrap: true);
            UIHelper.SetLayout(desc.gameObject, preferredHeight: 76);
        }

        private static void BuildActionButtons(Transform parent)
        {
            var col = UIHelper.MakeVertical("ActionButtons", parent, 14);
            UIHelper.SetLayout(col.gameObject, preferredHeight: 200);

            var refreshBtn = UIHelper.MakeButton("btn-refresh-certs", col, "Sync & Refresh Certificates", 34,
                UIColors.Hex("#F3F5F7"), UIColors.TextPrimary, 20);
            UIHelper.SetLayout(refreshBtn.gameObject, preferredHeight: 90);

            var homeBtn = UIHelper.MakeButton("btn-back-home", col, "Return to Dashboard", 34,
                UIColors.PrimaryDark, Color.white, 20);
            UIHelper.SetLayout(homeBtn.gameObject, preferredHeight: 90);
        }
    }
}
