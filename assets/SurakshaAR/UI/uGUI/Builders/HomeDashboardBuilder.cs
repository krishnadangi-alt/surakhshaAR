using SurakshaAR.Core;
using SurakshaAR.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Home Dashboard builder — Reference-driven modern mobile-first Android UI.
    /// Strictly matches the primary visual reference image:
    /// - Fixed Top Header: SurakshaAR brand title on the left (NO government logos/department text),
    ///   Language selector pill, Notification bell, and Profile button on the right.
    /// - Scrollable Main Content:
    ///   1. Hero Banner Card: HD Mining Banner composition (SurakshaAR, Learn Safe | Work Safe,
    ///      Safe Mines / Skilled Workforce / Stronger Jharkhand badges, mining plant, and
    ///      Safer Jharkhand Brighter Tomorrow script).
    ///   2. Worker Profile Card: Circular worker avatar (yellow hardhat, orange vest, blue circle),
    ///      "Welcome,", worker name, "ID: ... | 📍 Mine Worker", status pill "[ Mine Worker ]",
    ///      real-time circular progress ring, and right chevron.
    ///   3. Sync Bar: Light peach rounded container with solid orange dot, "Sync: X Pending Sessions",
    ///      and rounded orange "SYNC NOW" button.
    ///   4. Training Modules Section: "Training Modules" (green book icon) + "View All" (green text),
    ///      with 3 module cards (Fire & Explosion, Gas Leak, Machinery) featuring themed squircle icons,
    ///      titles, subtitles, and colored rounded "Start" buttons (Red, Blue, Green).
    ///   5. Mission Banner Card: "Every Worker Safe, Every Family Strong", "Safe Mines | Strong Communities",
    ///      featuring miners overlooking landscape at sunrise.
    /// - Fixed Bottom Navigation: White bar with Home (active green icon + label + indicator bar),
    ///   Learn, My Progress, Certificates, and bottom home indicator pill.
    /// - Floating Language Dropdown overlay.
    /// </summary>
    public static class HomeDashboardBuilder
    {
        private const float HDR_H = 135f;
        private const float NAV_H = 165f;

        private static Color Hex(string h) => UIColors.Hex(h);

        public static GameObject Build()
        {
            UIHelper.ClearSpriteCache();

            var root = new GameObject("HomeDashboardScreen");
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;
            rootRT.sizeDelta = Vector2.zero;

            var rootImg = root.AddComponent<Image>();
            rootImg.color = Hex("#F8FAFC");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // 1. Fixed Top Header (SurakshaAR branding on left — NO Gov Branding; Controls on right)
            BuildTopHeader(root.transform);

            // 2. Fixed Bottom Navigation Bar
            MakeBottomNav(root.transform);

            // 3. Scrollable Main Content between Top Header and Bottom Nav
            var scrollRoot = UIHelper.MakeRect("ScrollArea", root.transform);
            scrollRoot.anchorMin = Vector2.zero;
            scrollRoot.anchorMax = Vector2.one;
            scrollRoot.offsetMin = new Vector2(0, NAV_H);     // Sits above bottom nav
            scrollRoot.offsetMax = new Vector2(0, -HDR_H);    // Sits below top header

            var sr = scrollRoot.gameObject.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
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
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            sr.content        = content;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing            = 22;
            vlg.padding            = new RectOffset(24, 24, 16, 32);
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth      = true;
            vlg.childControlHeight     = false;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // ── Scrollable Content Elements ──────────────────────────────
            BuildHeroBannerCard(content);
            BuildWorkerProfileCard(content);
            BuildSyncStatusBar(content);
            BuildTrainingModulesSection(content);

            // Hidden button for controller compatibility
            var hiddenProgBtn = UIHelper.MakeButton("btn-view-progress", root.transform, "", 1, UIColors.Transparent, UIColors.Transparent, 0);
            hiddenProgBtn.gameObject.SetActive(false);

            // 4. Floating Language Dropdown overlay
            BuildLanguageDropdown(root.transform);

            return root;
        }

        private static void AnchorCenter(RectTransform rt, float w, float h)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
        }

        // =================================================================
        // 1. TOP HEADER (SurakshaAR branding on left; Language, Bell, Profile on right)
        // =================================================================
        private static void BuildTopHeader(Transform parent)
        {
            var headerRT = UIHelper.MakeRect("TopHeaderBar", parent);
            headerRT.anchorMin = new Vector2(0f, 1f);
            headerRT.anchorMax = new Vector2(1f, 1f);
            headerRT.pivot     = new Vector2(0.5f, 1f);
            headerRT.anchoredPosition = Vector2.zero;
            headerRT.sizeDelta = new Vector2(0f, HDR_H);

            var headerImg = headerRT.gameObject.AddComponent<Image>();
            headerImg.color = Color.white;
            headerImg.sprite = UIHelper.GetWhiteSprite();

            var headerShadow = headerRT.gameObject.AddComponent<Shadow>();
            headerShadow.effectColor = new Color(0, 0, 0, 0.04f);
            headerShadow.effectDistance = new Vector2(0, -3f);

            // Left: SurakshaAR Brand Title
            var brandGO = UIHelper.MakeRect("BrandTitle", headerRT);
            brandGO.anchorMin = new Vector2(0f, 0.5f);
            brandGO.anchorMax = new Vector2(0f, 0.5f);
            brandGO.pivot     = new Vector2(0f, 0.5f);
            brandGO.anchoredPosition = new Vector2(24f, 0f);
            brandGO.sizeDelta = new Vector2(380f, 84f);

            var brandLbl = UIHelper.MakeLabel("label-app-brand", brandGO,
                "<b><color=#0A192F>Suraksha</color><color=#EA580C>A</color><color=#16A34A>R</color></b>",
                54, Hex("#0A192F"), TextAlignmentOptions.Left, bold: true);
            UIHelper.Stretch(brandLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Right: Interactive controls (Language, Notification, Profile)
            var topControls = UIHelper.MakeRect("TopControlsRow", headerRT);
            topControls.anchorMin = new Vector2(1f, 0.5f);
            topControls.anchorMax = new Vector2(1f, 0.5f);
            topControls.pivot     = new Vector2(1f, 0.5f);
            topControls.anchoredPosition = new Vector2(-20f, 0f);
            topControls.sizeDelta = new Vector2(480f, 80f);

            var hlg = topControls.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 14;
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth      = true;
            hlg.childControlHeight     = true;

            // 1. Language picker pill
            var langPill = UIHelper.MakeButton("btn-language-picker", topControls, "", 14, Color.white, Color.white, 36);
            UIHelper.SetLayout(langPill.gameObject, preferredWidth: 220, minWidth: 200, preferredHeight: 76, minHeight: 74);
            var langBorder = langPill.gameObject.AddComponent<Outline>();
            langBorder.effectColor = Hex("#E2E8F0");
            langBorder.effectDistance = new Vector2(1.2f, -1.2f);

            var pillRow = UIHelper.MakeHorizontal("PillRow", langPill.transform, 10,
                new RectOffset(14, 14, 0, 0), childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(pillRow, 0, 0, 0, 0);
            pillRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var globeGO = UIHelper.MakeRect("GlobeIcon", pillRow);
            UIHelper.SetLayout(globeGO.gameObject, preferredWidth: 30, minWidth: 30, preferredHeight: 30, minHeight: 30);
            var globeImg = globeGO.gameObject.AddComponent<Image>();
            globeImg.sprite = UIHelper.GetGlobeSprite();
            globeImg.color  = Hex("#0E2A47");

            var currentLang = AppState.Instance != null ? AppState.Instance.CurrentLanguage : AppLanguage.Hindi;
            string initialLangStr = currentLang == AppLanguage.Hindi ? "हिंदी" : (currentLang == AppLanguage.Santali ? "ᱚᱞ ᱪᱤᱠᱤ" : "English");
            var langLbl = UIHelper.MakeLabel("label-lang-text", pillRow, initialLangStr, 32, Hex("#0E2A47"), TextAlignmentOptions.Center, bold: true);
            if (currentLang == AppLanguage.Hindi)
            {
                try { langLbl.font = UIHelper.GetDevanagariFont(); } catch {}
            }
            UIHelper.SetLayout(langLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 44, minHeight: 40);

            var chevGO = UIHelper.MakeRect("ChevIcon", pillRow);
            UIHelper.SetLayout(chevGO.gameObject, preferredWidth: 22, minWidth: 22, preferredHeight: 22, minHeight: 22);
            var chevImg = chevGO.gameObject.AddComponent<Image>();
            chevImg.sprite = UIHelper.GetDownChevronSprite();
            chevImg.color  = Hex("#0E2A47");

            // 2. Bell button
            var bellBtn = UIHelper.MakeButton("btn-notifications", topControls, "", 14, Color.white, Color.white, 36);
            UIHelper.SetLayout(bellBtn.gameObject, preferredWidth: 78, minWidth: 78, preferredHeight: 78, minHeight: 78);
            var bellBorder = bellBtn.gameObject.AddComponent<Outline>();
            bellBorder.effectColor = Hex("#E2E8F0");
            bellBorder.effectDistance = new Vector2(1.2f, -1.2f);

            var bellIconGO = UIHelper.MakeRect("BellIcon", bellBtn.transform);
            AnchorCenter(bellIconGO, 42, 42);
            var bellImg = bellIconGO.gameObject.AddComponent<Image>();
            bellImg.sprite = UIHelper.GetBellSprite();
            bellImg.color  = Hex("#0E2A47");

            var badgeGO = UIHelper.MakeRect("RedBadge", bellBtn.transform);
            badgeGO.anchorMin = new Vector2(1f, 1f);
            badgeGO.anchorMax = new Vector2(1f, 1f);
            badgeGO.pivot     = new Vector2(1f, 1f);
            badgeGO.anchoredPosition = new Vector2(4f, 4f);
            badgeGO.sizeDelta = new Vector2(26f, 26f);
            var badgeImg = badgeGO.gameObject.AddComponent<Image>();
            badgeImg.sprite = UIHelper.GetCircleSprite();
            badgeImg.color  = Hex("#EF4444");

            var bNum = UIHelper.MakeLabel("BNum", badgeGO, "0", 18, Color.white, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(bNum.GetComponent<RectTransform>(), 0, 0, 0, 0);
            badgeGO.gameObject.SetActive(false);

            // 3. Profile button
            var profBtn = UIHelper.MakeButton("btn-profile", topControls, "", 14, Color.white, Color.white, 36);
            UIHelper.SetLayout(profBtn.gameObject, preferredWidth: 78, minWidth: 78, preferredHeight: 78, minHeight: 78);
            var profBorder = profBtn.gameObject.AddComponent<Outline>();
            profBorder.effectColor = Hex("#E2E8F0");
            profBorder.effectDistance = new Vector2(1.2f, -1.2f);

            var profIconGO = UIHelper.MakeRect("ProfIcon", profBtn.transform);
            AnchorCenter(profIconGO, 42, 42);
            var profImg = profIconGO.gameObject.AddComponent<Image>();
            profImg.sprite = UIHelper.GetProfileSprite();
            profImg.color  = Hex("#0E2A47");
        }

        // =================================================================
        // 2. HERO BANNER CARD (HD Mine Banner Composition)
        // =================================================================
        private static void BuildHeroBannerCard(Transform parent)
        {
            var card = new GameObject("HeroBannerCard");
            card.transform.SetParent(parent, false);
            UIHelper.SetLayout(card, flexibleWidth: true, flexWidth: 1, preferredHeight: 440, minHeight: 420);

            var cardImg = card.AddComponent<Image>();
            cardImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 22);

            var shadow = card.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.05f);
            shadow.effectDistance = new Vector2(0, -3f);

            var border = card.AddComponent<Outline>();
            border.effectColor = Hex("#E2E8F0");
            border.effectDistance = new Vector2(1.2f, -1.2f);

            var mask = card.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            // Background Image: home_hero_banner.jpg
            var bgRT = UIHelper.MakeRect("BgMineBanner", card.transform);
            UIHelper.Stretch(bgRT, 0, 0, 0, 0);
            var bgImg = bgRT.gameObject.AddComponent<Image>();
            var bannerSpr = UIHelper.LoadProjectSprite("home_hero_banner.jpg")
                         ?? UIHelper.LoadProjectSprite("home_hero_banner")
                         ?? UIHelper.LoadProjectSprite("home_menu_mine_baground.png")
                         ?? UIHelper.LoadProjectSprite("mine_hero_banner.jpg");
            bgImg.sprite = bannerSpr;
            bgImg.color  = Color.white;
            bgImg.preserveAspect = false;
            bgImg.raycastTarget = false;

            var bannerBtn = card.AddComponent<Button>();
            bannerBtn.targetGraphic = cardImg;

            var hiddenBtn = UIHelper.MakeButton("btn-jharkhand-mines", card.transform, "", 14, UIColors.Transparent, UIColors.Transparent, 0);
            UIHelper.Stretch(hiddenBtn.GetComponent<RectTransform>(), 0, 0, 0, 0);
        }

        // =================================================================
        // 3. WORKER PROFILE CARD
        // =================================================================
        private static void BuildWorkerProfileCard(Transform parent)
        {
            var card = new GameObject("WorkerProfileCard");
            card.transform.SetParent(parent, false);
            UIHelper.SetLayout(card, preferredHeight: 220, minHeight: 215, flexibleWidth: true, flexWidth: 1);

            var cardImg = card.AddComponent<Image>();
            cardImg.color  = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 22);

            var shadow = card.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.05f);
            shadow.effectDistance = new Vector2(0, -3f);

            var border = card.AddComponent<Outline>();
            border.effectColor = Hex("#E2E8F0");
            border.effectDistance = new Vector2(1.2f, -1.2f);

            var cardBtn = card.AddComponent<Button>();
            cardBtn.targetGraphic = cardImg;

            var hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.padding        = new RectOffset(22, 20, 16, 16);
            hlg.spacing        = 18;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth      = true;
            hlg.childControlHeight     = true;

            // Left: Avatar circular frame (116x116)
            var avatarWrap = UIHelper.MakeRect("AvatarWrap", card.transform);
            UIHelper.SetLayout(avatarWrap.gameObject, preferredWidth: 116, minWidth: 116, preferredHeight: 116, minHeight: 116);

            var bgCirc = avatarWrap.gameObject.AddComponent<Image>();
            bgCirc.sprite = UIHelper.GetCircleSprite();
            bgCirc.color  = Hex("#E0F2FE");

            var avatarSpr = UIHelper.LoadProjectSprite("worker_avatar_ref1.png")
                         ?? UIHelper.LoadProjectSprite("worker_miner_avatar.jpg");
            if (avatarSpr != null)
            {
                var avImgGO = UIHelper.MakeRect("AvatarImg", avatarWrap);
                UIHelper.Stretch(avImgGO, 0, 0, 0, 0);
                var avImg = avImgGO.gameObject.AddComponent<Image>();
                avImg.sprite = avatarSpr;
                avImg.preserveAspect = true;
                avImg.raycastTarget = false;
            }

            // Center: Info Column (flexible width)
            var info = UIHelper.MakeVertical("InfoCol", card.transform, 5, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(info.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 200, minHeight: 190);
            var infoVLG = info.GetComponent<VerticalLayoutGroup>();
            infoVLG.childAlignment = TextAnchor.MiddleLeft;
            infoVLG.childControlWidth = true;
            infoVLG.childForceExpandWidth = true;
            infoVLG.childControlHeight = true;
            infoVLG.childForceExpandHeight = false;
            infoVLG.spacing = 5;

            // Greeting: "नमस्ते,"
            var greetingLbl = UIHelper.MakeLabel("label-greeting", info, "नमस्ते,", 30, Hex("#1E293B"), TextAlignmentOptions.Left, bold: true, wrap: false);
            UIHelper.SetLayout(greetingLbl.gameObject, preferredHeight: 38, minHeight: 34);

            // Worker Name (Krishna)
            var wName = UIHelper.MakeLabel("label-worker-name", info, "Krishna", 48, Hex("#0A192F"), TextAlignmentOptions.Left, bold: true, wrap: false);
            UIHelper.SetLayout(wName.gameObject, preferredHeight: 56, minHeight: 50);

            // Role / ID row: "आईडी: EMP-PROD-CORE-001"
            var roleRow = UIHelper.MakeHorizontal("RoleRow", info, 0, childForceWidth: false, childForceHeight: false, childControlWidth: true, childControlHeight: true);
            UIHelper.SetLayout(roleRow.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 38, minHeight: 34);
            var roleHlg = roleRow.GetComponent<HorizontalLayoutGroup>();
            roleHlg.childAlignment = TextAnchor.MiddleLeft;
            roleHlg.childControlWidth = true;
            roleHlg.childControlHeight = true;
            roleHlg.childForceExpandWidth = true;
            roleHlg.childForceExpandHeight = false;

            var idLbl = UIHelper.MakeLabel("label-worker-id", roleRow, "आईडी: EMP-PROD-CORE-001", 30, Hex("#1E293B"), TextAlignmentOptions.Left, bold: true, wrap: false);
            idLbl.textWrappingMode = TextWrappingModes.NoWrap;
            idLbl.overflowMode = TextOverflowModes.Ellipsis;
            UIHelper.SetLayout(idLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 38, minHeight: 34);

            var sepLbl = UIHelper.MakeLabel("SepLbl", roleRow, "", 0, Hex("#94A3B8"), TextAlignmentOptions.Center, bold: false, wrap: false);
            sepLbl.gameObject.SetActive(false);

            var pinGO = UIHelper.MakeRect("PinIcon", roleRow);
            pinGO.gameObject.SetActive(false);

            var roleLbl = UIHelper.MakeLabel("label-role", roleRow, "", 0, Hex("#64748B"), TextAlignmentOptions.Left, bold: false, wrap: false);
            roleLbl.gameObject.SetActive(false);

            // Status pill: "[ खान कार्यकर्ता ]"
            var statusPill = UIHelper.MakeRect("StatusPill", info);
            UIHelper.SetLayout(statusPill.gameObject, preferredWidth: 240, minWidth: 220, preferredHeight: 48, minHeight: 44);
            var spImg = statusPill.gameObject.AddComponent<Image>();
            spImg.color = Hex("#DCFCE7");
            UIHelper.SetImageRoundedSprite(spImg, 14);

            var tagLbl = UIHelper.MakeLabel("TagLbl", statusPill, "खान कार्यकर्ता", 26, Hex("#166534"), TextAlignmentOptions.Center, bold: true, wrap: false);
            tagLbl.textWrappingMode = TextWrappingModes.NoWrap;
            tagLbl.overflowMode = TextOverflowModes.Ellipsis;
            UIHelper.Stretch(tagLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Right: Circular Progress Ring & Chevron
            var progCol = UIHelper.MakeHorizontal("ProgressCol", card.transform, 12, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(progCol.gameObject, preferredWidth: 155, minWidth: 140, preferredHeight: 170, minHeight: 160);
            progCol.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleRight;

            // Circular Progress Ring (100x100)
            var ringWrap = UIHelper.MakeRect("ProgressRingWrap", progCol);
            UIHelper.SetLayout(ringWrap.gameObject, preferredWidth: 100, minWidth: 100, preferredHeight: 100, minHeight: 100);

            var trackGO = UIHelper.MakeRect("ProgressRingTrack", ringWrap);
            UIHelper.Stretch(trackGO, 0, 0, 0, 0);
            var trackImg = trackGO.gameObject.AddComponent<Image>();
            trackImg.sprite = UIHelper.CreateCircleOutlineSprite(thickness: 8f);
            trackImg.color  = Hex("#E2E8F0");
            trackImg.raycastTarget = false;

            var fillGO = UIHelper.MakeRect("progress-ring-fill", ringWrap);
            UIHelper.Stretch(fillGO, 0, 0, 0, 0);
            var fillImg = fillGO.gameObject.AddComponent<Image>();
            fillImg.sprite = UIHelper.CreateCircleOutlineSprite(thickness: 8f);
            fillImg.color  = Hex("#16A34A");
            fillImg.type   = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Radial360;
            fillImg.fillOrigin = (int)Image.Origin360.Top;
            fillImg.fillClockwise = true;
            fillImg.fillAmount = 0.33f;
            fillImg.raycastTarget = false;

            // Center Percentage Text: "33%"
            var pctLbl = UIHelper.MakeLabel("label-progress-ring-pct", ringWrap, "33%", 34, Hex("#0A192F"), TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(pctLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            var progSub = UIHelper.MakeLabel("label-progress-ring-sub", ringWrap, "", 1, UIColors.Transparent);
            progSub.gameObject.SetActive(false);

            // Right Chevron `>`
            var chevGO2 = UIHelper.MakeRect("ChevronRight", progCol);
            UIHelper.SetLayout(chevGO2.gameObject, preferredWidth: 20, minWidth: 20, preferredHeight: 30, minHeight: 30);
            var chevImg2 = chevGO2.gameObject.AddComponent<Image>();
            chevImg2.sprite = UIHelper.GetRightChevronSprite();
            chevImg2.color  = Hex("#94A3B8");
            chevImg2.preserveAspect = true;
        }

        // =================================================================
        // 4. SYNC STATUS STRIP (Enlarged, Bright Mint, Clear Typography)
        // =================================================================
        private static void BuildSyncStatusBar(Transform parent)
        {
            var syncBtn = UIHelper.MakeButton("btn-sync-status", parent, "", 16, Hex("#F0FDF4"), Hex("#BBF7D0"), 20);
            UIHelper.SetLayout(syncBtn.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 96, minHeight: 92);

            var border = syncBtn.gameObject.AddComponent<Outline>();
            border.effectColor = Hex("#BBF7D0");
            border.effectDistance = new Vector2(1.5f, -1.5f);

            var shadow = syncBtn.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.04f);
            shadow.effectDistance = new Vector2(0, -3f);

            var row = UIHelper.MakeHorizontal("SyncRow", syncBtn.transform, 16,
                new RectOffset(22, 18, 0, 0), childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(row, 0, 0, 0, 0);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            int pending = OfflineDataStore.Instance != null ? OfflineDataStore.Instance.PendingSyncCount : 0;
            bool hasPending = (pending > 0);

            // Dot (24x24)
            var dotWrap = UIHelper.MakeRect("SyncDotWrap", row);
            UIHelper.SetLayout(dotWrap.gameObject, preferredWidth: 24, minWidth: 24, preferredHeight: 24, minHeight: 24);
            var dotImg = dotWrap.gameObject.AddComponent<Image>();
            dotImg.sprite = UIHelper.GetCircleSprite();
            dotImg.color  = hasPending ? Hex("#EA580C") : Hex("#10B981");
            dotImg.raycastTarget = false;

            // Status Text (30px bold, crisp contrast)
            string statusText = hasPending
                ? $"Sync: {pending} Pending Session{(pending > 1 ? "s" : "")} (Tap to Sync)"
                : "सिंक: सभी डेटा सिंक्रनाइज़ है";
            Color statusColor = hasPending ? Hex("#C2410C") : Hex("#14532D");
            var statusLbl = UIHelper.MakeLabel("label-sync-status-text", row, statusText, 30, statusColor, TextAlignmentOptions.Left, bold: true);
            UIHelper.SetLayout(statusLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 50, minHeight: 46);
            statusLbl.raycastTarget = false;

            // Action Pill Button (28px bold, 320x62)
            var pillGO = UIHelper.MakeRect("SyncActionPill", row);
            UIHelper.SetLayout(pillGO.gameObject, preferredWidth: 320, minWidth: 290, preferredHeight: 62, minHeight: 58);
            var pillImg = pillGO.gameObject.AddComponent<Image>();
            pillImg.color = hasPending ? Hex("#EA580C") : Hex("#DCFCE7");
            UIHelper.SetImageRoundedSprite(pillImg, 18);
            pillImg.raycastTarget = true;

            var pillRow = UIHelper.MakeHorizontal("PillInnerRow", pillGO.transform, 10,
                new RectOffset(16, 16, 0, 0), childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(pillRow, 0, 0, 0, 0);
            pillRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var checkIconGO = UIHelper.MakeRect("SyncCheckIcon", pillRow);
            UIHelper.SetLayout(checkIconGO.gameObject, preferredWidth: 24, minWidth: 24, preferredHeight: 24, minHeight: 24);
            var ciImg = checkIconGO.gameObject.AddComponent<Image>();
            ciImg.sprite = UIHelper.GetCheckmarkCircleSprite();
            ciImg.preserveAspect = true;
            checkIconGO.gameObject.SetActive(!hasPending);

            string pillText = hasPending ? "SYNC NOW >" : "सिंक्रनाइज़ेशन पूरा हुआ";
            Color pillTextColor = hasPending ? Color.white : Hex("#15803D");
            var pillLbl = UIHelper.MakeLabel("label-sync-pill-text", pillRow, pillText, 28, pillTextColor, TextAlignmentOptions.Center, bold: true, wrap: false);
            pillLbl.textWrappingMode = TextWrappingModes.NoWrap;
            pillLbl.overflowMode = TextOverflowModes.Overflow;
            pillLbl.raycastTarget = false;

            var pillBtn = pillGO.gameObject.AddComponent<Button>();
            pillBtn.transition = Selectable.Transition.ColorTint;
        }

        // =================================================================
        // 5. TRAINING MODULES SECTION (Spacious, Generous, High Contrast)
        // =================================================================
        private static void BuildTrainingModulesSection(Transform parent)
        {
            var section = new GameObject("TrainingModulesSection");
            section.transform.SetParent(parent, false);
            UIHelper.SetLayout(section, flexibleWidth: true, flexWidth: 1);

            var vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.padding            = new RectOffset(0, 0, 0, 0);
            vlg.spacing            = 20;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth      = true;
            vlg.childControlHeight     = false;

            var csf = section.AddComponent<ContentSizeFitter>();
            csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Section Header: "📖 Training Modules" + "View All"
            var headerRow = UIHelper.MakeHorizontal("SectionHeader", section.transform, 14, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(headerRow.gameObject, preferredHeight: 64, minHeight: 60);
            headerRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var bookIcon = UIHelper.MakeRect("BookIcon", headerRow);
            UIHelper.SetLayout(bookIcon.gameObject, preferredWidth: 44, minWidth: 44, preferredHeight: 44, minHeight: 44);
            var bkImg = bookIcon.gameObject.AddComponent<Image>();
            bkImg.color  = Hex("#16A34A");
            bkImg.sprite = UIHelper.GetBookSprite();

            var secTitle = UIHelper.MakeLabel("label-modules-section", headerRow, "प्रशिक्षण मॉड्यूल", 44, Hex("#0A192F"), TextAlignmentOptions.Left, bold: true);
            UIHelper.SetLayout(secTitle.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 54, minHeight: 50);

            var viewAll = UIHelper.MakeButton("btn-view-all", headerRow, "सभी देखें >", 32, UIColors.Transparent, Hex("#16A34A"), 0);
            UIHelper.SetLayout(viewAll.gameObject, preferredWidth: 170, minWidth: 150, preferredHeight: 52, minHeight: 48);
            var vaLbl = viewAll.GetComponentInChildren<TextMeshProUGUI>();
            if (vaLbl != null) { vaLbl.name = "label-view-all"; vaLbl.fontStyle = FontStyles.Bold; }

            // Vertical list of module cards (250px high, comfortable spacing, zero title collision)
            BuildModuleCard(section.transform, "card-fire",
                cardBgColor:   Hex("#FFFBF7"),
                borderColor:   Hex("#FDBA74"),
                iconBgColor:   Hex("#FFEDD5"),
                iconFilename:  "icon_fire_ref1.png",
                englishTitle:  "आग एवं विस्फोट से निपटने की प्रक्रिया",
                hindiLabelName: "HindiTitle-fire",
                engLabelName:   "EngTitle-fire",
                subtitle:      "खतरों की पहचान, अग्निशामक यंत्र का उपयोग\nऔर सुरक्षित निकासी",
                btnBgColor:    Hex("#DC2626"),
                statusLabelName: "label-fire-status");

            BuildModuleCard(section.transform, "card-gas",
                cardBgColor:   Hex("#F0F9FF"),
                borderColor:   Hex("#93C5FD"),
                iconBgColor:   Hex("#E0F2FE"),
                iconFilename:  "icon_gas_ref1.png",
                englishTitle:  "गैस रिसाव एवं सीमित स्थान",
                hindiLabelName: "HindiTitle-gas",
                engLabelName:   "EngTitle-gas",
                subtitle:      "खतरनाक गैसों की पहचान और PPE का उपयोग",
                btnBgColor:    Hex("#2563EB"),
                statusLabelName: "label-gas-btn");

            BuildModuleCard(section.transform, "card-machinery",
                cardBgColor:   Hex("#F0FDF4"),
                borderColor:   Hex("#86EFAC"),
                iconBgColor:   Hex("#DCFCE7"),
                iconFilename:  "icon_gear_ref1.png",
                englishTitle:  "मशीनरी सुरक्षा",
                hindiLabelName: "HindiTitle-machinery",
                engLabelName:   "EngTitle-machinery",
                subtitle:      "मशीनों का सुरक्षित उपयोग, लॉकआउट/टैगआउट\nऔर सुरक्षित संचालन",
                btnBgColor:    Hex("#16A34A"),
                statusLabelName: "label-machinery-btn");
        }

        private static void BuildModuleCard(Transform parent, string btnName,
            Color cardBgColor, Color borderColor, Color iconBgColor, string iconFilename,
            string englishTitle, string hindiLabelName, string engLabelName,
            string subtitle, Color btnBgColor, string statusLabelName)
        {
            var card = new GameObject(btnName);
            card.transform.SetParent(parent, false);
            UIHelper.SetLayout(card.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 250, minHeight: 240);

            var cardImg = card.AddComponent<Image>();
            cardImg.color  = cardBgColor;
            UIHelper.SetImageRoundedSprite(cardImg, 24);

            var shadow = card.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.05f);
            shadow.effectDistance = new Vector2(0, -3f);

            var border = card.AddComponent<Outline>();
            border.effectColor = borderColor;
            border.effectDistance = new Vector2(1.5f, -1.5f);

            var btn = card.AddComponent<Button>();
            btn.targetGraphic = cardImg;

            // Card Inner Row
            var hlg = UIHelper.MakeHorizontal("CardRow", card.transform, 16,
                new RectOffset(20, 18, 18, 18), childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(hlg, 0, 0, 0, 0);
            hlg.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // Icon Container (Rounded square 110x110)
            var iconBox = UIHelper.MakeRect("IconBox", hlg);
            UIHelper.SetLayout(iconBox.gameObject, preferredWidth: 110, minWidth: 110, preferredHeight: 110, minHeight: 110);
            var ibImg = iconBox.gameObject.AddComponent<Image>();
            ibImg.color = iconBgColor;
            UIHelper.SetImageRoundedSprite(ibImg, 24);

            var spr = UIHelper.LoadProjectSprite(iconFilename);
            if (spr != null)
            {
                var iconImgGO = UIHelper.MakeRect("ModuleIconImg", iconBox);
                UIHelper.Stretch(iconImgGO, 0, 0, 0, 0);
                var artImg = iconImgGO.gameObject.AddComponent<Image>();
                artImg.sprite = spr;
                artImg.preserveAspect = true;
                artImg.raycastTarget = false;
            }

            // Center: Content column
            var contentCol = UIHelper.MakeVertical("ContentCol", hlg, 10, childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(contentCol.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 180, minHeight: 165);
            var cvlg = contentCol.GetComponent<VerticalLayoutGroup>();
            cvlg.childAlignment = TextAnchor.MiddleLeft;
            cvlg.childControlWidth = true;
            cvlg.childControlHeight = true;
            cvlg.childForceExpandHeight = false;
            cvlg.padding = new RectOffset(0, 0, 0, 0);

            // Title: 38px bold with 84px height so multiline "Gas Leak & Confined Space" does not overflow!
            var titleLbl = UIHelper.MakeLabel(hindiLabelName, contentCol, englishTitle, 38, Hex("#0A192F"), TextAlignmentOptions.Left, bold: true);
            titleLbl.lineSpacing = 1.08f;
            titleLbl.textWrappingMode = TextWrappingModes.Normal;
            titleLbl.overflowMode = TextOverflowModes.Overflow;
            UIHelper.SetLayout(titleLbl.gameObject, preferredHeight: 84, minHeight: 72);

            // Subtitle: 28px, dark slate (#334155) with 10px light distance from title
            var subLbl = UIHelper.MakeLabel(engLabelName, contentCol, subtitle, 28, Hex("#334155"), TextAlignmentOptions.Left);
            subLbl.lineSpacing = 1.15f;
            subLbl.textWrappingMode = TextWrappingModes.Normal;
            subLbl.overflowMode = TextOverflowModes.Overflow;
            UIHelper.SetLayout(subLbl.gameObject, preferredHeight: 72, minHeight: 62);

            // Right: Rounded Colored "Start" Button + Chevron side-by-side
            var rightCol = UIHelper.MakeHorizontal("RightCol", hlg, 10, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(rightCol.gameObject, preferredWidth: 165, minWidth: 150, preferredHeight: 68, minHeight: 60);
            var rcHlg = rightCol.GetComponent<HorizontalLayoutGroup>();
            rcHlg.childAlignment = TextAnchor.MiddleRight;
            rcHlg.childControlWidth = true;
            rcHlg.childControlHeight = true;
            rcHlg.childForceExpandWidth = false;
            rcHlg.childForceExpandHeight = false;

            var startBtnGO = UIHelper.MakeRect("StartBtnWrap", rightCol);
            UIHelper.SetLayout(startBtnGO.gameObject, preferredWidth: 130, minWidth: 115, preferredHeight: 62, minHeight: 56);
            var startBtnImg = startBtnGO.gameObject.AddComponent<Image>();
            startBtnImg.color = btnBgColor;
            UIHelper.SetImageRoundedSprite(startBtnImg, 20);

            // Start button text: 30px bold
            var startLbl = UIHelper.MakeLabel(statusLabelName, startBtnGO, "शुरू करें", 30, Color.white, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(startLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Right chevron `>`
            var cardChev = UIHelper.MakeRect("CardChevron", rightCol);
            UIHelper.SetLayout(cardChev.gameObject, preferredWidth: 16, minWidth: 16, preferredHeight: 28, minHeight: 28);
            var cImg = cardChev.gameObject.AddComponent<Image>();
            cImg.sprite = UIHelper.GetRightChevronSprite();
            cImg.color = Hex("#64748B");
            cImg.preserveAspect = true;
        }

        // =================================================================
        // 7. FIXED BOTTOM NAVIGATION (Matching Reference Image)
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
            var tbImg = topBorder.gameObject.AddComponent<Image>();
            tbImg.color = Hex("#E2E8F0");
            topBorder.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

            var hlg = navRT.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth  = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth      = true;
            hlg.childControlHeight     = true;
            hlg.padding = new RectOffset(0, 0, 8, 20);

            MakeNavItem(navRT, "nav-home",         "होम",         UIHelper.GetHomeSprite(),  true);
            MakeNavItem(navRT, "nav-learn",        "सीखें",        UIHelper.GetBookSprite(),  false);
            MakeNavItem(navRT, "nav-progress",     "मेरी प्रगति",  UIHelper.GetChartSprite(), false);
            MakeNavItem(navRT, "nav-certificates", "प्रमाणपत्र",   UIHelper.GetMedalSprite(), false);

            // Phone Home Indicator (Bottom horizontal pill bar)
            var homeInd = UIHelper.MakeRect("HomeBarIndicator", navRT);
            homeInd.anchorMin = new Vector2(0.5f, 0f);
            homeInd.anchorMax = new Vector2(0.5f, 0f);
            homeInd.pivot     = new Vector2(0.5f, 0f);
            homeInd.anchoredPosition = new Vector2(0, 8f);
            homeInd.sizeDelta = new Vector2(140f, 5f);
            var hiImg = homeInd.gameObject.AddComponent<Image>();
            hiImg.color = Hex("#CBD5E1");
            UIHelper.SetImageRoundedSprite(hiImg, 3);
            homeInd.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        }

        private static void MakeNavItem(Transform parent, string name, string label, Sprite iconSprite, bool active)
        {
            var activeColor = active ? Hex("#16A34A") : Hex("#334155");
            var activeFontSize = 30f;

            var btn = UIHelper.MakeButton(name, parent, "", 14, UIColors.Transparent, Color.white, 0);

            var col = UIHelper.MakeVertical("Col", btn.transform, 6, new RectOffset(0, 0, 4, 4), childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(col, 0, 0, 0, 0);
            col.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var iconBox = UIHelper.MakeRect("IconBox", col);
            UIHelper.SetLayout(iconBox.gameObject, preferredWidth: 50, minWidth: 50, preferredHeight: 50, minHeight: 50);
            var iconImg = iconBox.gameObject.AddComponent<Image>();
            iconImg.sprite = iconSprite;
            iconImg.color  = activeColor;
            iconImg.preserveAspect = true;

            var lbl = UIHelper.MakeLabel($"label-{name}", col, label, activeFontSize, activeColor, TextAlignmentOptions.Center, bold: true);
            lbl.textWrappingMode = TextWrappingModes.NoWrap;
            lbl.overflowMode = TextOverflowModes.Overflow;
            UIHelper.SetLayout(lbl.gameObject, preferredHeight: 42, minHeight: 38);

            // Bottom indicator line under active tab
            var indRow = UIHelper.MakeRect("IndRow", col);
            UIHelper.SetLayout(indRow.gameObject, preferredWidth: 64, minWidth: 64, preferredHeight: 6, minHeight: 6);
            if (active)
            {
                var indImg = indRow.gameObject.AddComponent<Image>();
                indImg.color  = Hex("#16A34A");
                indImg.sprite = UIHelper.GetWhiteSprite();
                UIHelper.SetImageRoundedSprite(indImg, 3);
            }
        }

        // =================================================================
        // 8. LANGUAGE DROPDOWN OVERLAY
        // =================================================================
        private static void BuildLanguageDropdown(Transform parent)
        {
            var overlay = UIHelper.MakeRect("LanguageDropdownOverlay", parent);
            UIHelper.Stretch(overlay, 0, 0, 0, 0);

            var bgImg = overlay.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.02f);
            var bgBtn = overlay.gameObject.AddComponent<Button>();
            bgBtn.targetGraphic = bgImg;

            var panel = UIHelper.MakeRect("DropdownPanel", overlay);
            panel.anchorMin = new Vector2(1, 1);
            panel.anchorMax = new Vector2(1, 1);
            panel.pivot     = new Vector2(1, 1);
            panel.anchoredPosition = new Vector2(-190f, -100f);
            panel.sizeDelta = new Vector2(260f, 240f);

            var panelImg = panel.gameObject.AddComponent<Image>();
            panelImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(panelImg, 16);

            var shadow = panel.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.12f);
            shadow.effectDistance = new Vector2(0, -6f);

            var border = panel.gameObject.AddComponent<Outline>();
            border.effectColor = Hex("#E2E8F0");
            border.effectDistance = new Vector2(1.2f, -1.2f);

            var vlg = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 6;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            MakeLangOption(panel, "btn-lang-en", "English", true, UIHelper.GetDefaultFont());
            MakeLangOption(panel, "btn-lang-hi", "हिंदी", false, UIHelper.GetDevanagariFont());
            MakeLangOption(panel, "btn-lang-sa", "ᱚᱞ ᱪᱤᱠᱤ", false, UIHelper.GetSantaliFont());

            overlay.gameObject.SetActive(false);
        }

        private static void MakeLangOption(Transform parent, string btnName, string label, bool isSelected, TMP_FontAsset font = null)
        {
            var btn = UIHelper.MakeButton(btnName, parent, "", 14, isSelected ? Hex("#F0FDF4") : Color.white, Color.white, 10);
            UIHelper.SetLayout(btn.gameObject, preferredHeight: 62, minHeight: 58);

            var hlg = btn.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(14, 14, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = true;

            var checkGO = UIHelper.MakeRect("CheckIcon", btn.transform);
            UIHelper.SetLayout(checkGO.gameObject, preferredWidth: 24, minWidth: 24, preferredHeight: 24, minHeight: 24);
            if (isSelected)
            {
                var checkImg = checkGO.gameObject.AddComponent<Image>();
                checkImg.sprite = UIHelper.GetCheckmarkSprite();
                checkImg.color = Hex("#16A34A");
            }

            var spacer = UIHelper.MakeRect("Spacer", btn.transform);
            UIHelper.SetLayout(spacer.gameObject, preferredWidth: 10, minWidth: 10);

            var txtColor = isSelected ? Hex("#16A34A") : Hex("#0A192F");
            var lbl = UIHelper.MakeLabel($"label-{btnName}", btn.transform, label, 28, txtColor, TextAlignmentOptions.Left, bold: isSelected);
            if (font != null) lbl.font = font;
            UIHelper.SetLayout(lbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 40, minHeight: 36);
        }
    }
}
