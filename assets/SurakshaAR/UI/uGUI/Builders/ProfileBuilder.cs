using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Profile screen builder matching Reference 2.
    /// Mobile-first responsive uGUI implementation.
    /// </summary>
    public static class ProfileBuilder
    {
        private const float NAV_H = 204f;
        private static Color Hex(string h) => UIColors.Hex(h);

        public static GameObject Build()
        {
            var root = new GameObject("ProfileScreen");
            var rootRT = root.AddComponent<RectTransform>();
            UIHelper.Stretch(rootRT, 0, 0, 0, 0);

            var rootImg = root.AddComponent<Image>();
            rootImg.color = Hex("#F8FAFC");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            MakeBottomNav(root.transform);

            var scrollRoot = UIHelper.MakeRect("ScrollArea", root.transform);
            scrollRoot.anchorMin = Vector2.zero;
            scrollRoot.anchorMax = Vector2.one;
            scrollRoot.offsetMin = new Vector2(0, NAV_H);
            scrollRoot.offsetMax = Vector2.zero;

            var sr = scrollRoot.gameObject.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 40f;

            var viewport = UIHelper.MakeRect("Viewport", scrollRoot);
            UIHelper.Stretch(viewport, 0, 0, 0, 0);
            viewport.gameObject.AddComponent<RectMask2D>();
            sr.viewport = viewport;

            var content = UIHelper.MakeRect("Content", viewport);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            sr.content = content;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(0, 0, 0, 48);
            vlg.spacing = 18;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            BuildHeader(content);
            BuildProfileCard(content);
            BuildStatsRow(content);
            BuildActionList(content);
            BuildLogoutButton(content);
            BuildMissionBanner(content);

            BuildPersonalInfoModal(root.transform);
            BuildSafetyPreferencesModal(root.transform);
            BuildAppSettingsModal(root.transform);

            return root;
        }

        // =================================================================
        // 1. CLEAN MOBILE HEADER SECTION (No standalone government block)
        // =================================================================
        private static void BuildHeader(Transform parent)
        {
            var wrap = UIHelper.MakeRect("HeaderWrap", parent);
            UIHelper.SetLayout(wrap.gameObject, preferredHeight: 210, minHeight: 210);

            var headerImg = wrap.gameObject.AddComponent<Image>();
            headerImg.color = Hex("#0A3C36"); // Forest Emerald matching Learn & Progress
            headerImg.sprite = UIHelper.GetWhiteSprite();

            var headerRow = UIHelper.MakeHorizontal("HeaderRow", wrap, 20,
                new RectOffset(32, 32, 28, 24),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(headerRow, 0, 0, 0, 0);
            headerRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // Comfortable mobile back button (76x76 touch target)
            var backBtn = UIHelper.MakeButton("btn-back", headerRow, "‹", 48,
                new Color(1, 1, 1, 0.18f), Color.white, 24);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 76, minWidth: 76,
                preferredHeight: 76, minHeight: 76);

            // Title + subtitle column
            var titleCol = UIHelper.MakeVertical("TitleCol", headerRow, 6);
            UIHelper.SetLayout(titleCol.gameObject, flexibleWidth: true, flexWidth: 1);
            titleCol.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var titleLbl = UIHelper.MakeLabel("label-title", titleCol,
                "My Profile", 52, Color.white, TextAlignmentOptions.Left, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, preferredHeight: 64, minHeight: 60);

            var subLbl = UIHelper.MakeLabel("label-sub", titleCol,
                "Worker Identity & Safety Settings", 32, Hex("#A7F3D0"), TextAlignmentOptions.Left);
            UIHelper.SetLayout(subLbl.gameObject, preferredHeight: 40, minHeight: 38);
        }

        // =================================================================
        // 2. PROFILE CARD
        // =================================================================
        private static void BuildProfileCard(Transform parent)
        {
            var wrap = UIHelper.MakeRect("ProfileCardWrap", parent);
            UIHelper.SetLayout(wrap.gameObject, preferredHeight: 380, minHeight: 360);

            var vlg = wrap.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(32, 32, 0, 0);
            vlg.childForceExpandWidth = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = true;
            vlg.childControlHeight = true;

            var card = new GameObject("Card");
            card.transform.SetParent(wrap, false);
            var cardImg = card.AddComponent<Image>();
            cardImg.color = Color.white;
            cardImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cardImg, 28);

            var outline = card.AddComponent<Outline>();
            outline.effectColor = Hex("#F1F5F9");
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var shadow = card.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.04f);
            shadow.effectDistance = new Vector2(0, -6f);

            var hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(30, 30, 24, 24);
            hlg.spacing = 24;
            hlg.childAlignment = TextAnchor.UpperLeft;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childControlHeight = true;

            // Avatar on Left
            var avatarWrap = UIHelper.MakeRect("AvatarWrap", card.transform);
            UIHelper.SetLayout(avatarWrap.gameObject, preferredWidth: 210, minWidth: 210, preferredHeight: 210, minHeight: 210);

            var avSpr = UIHelper.LoadProjectSprite("profile_avatar_ref2.png")
                     ?? UIHelper.LoadProjectSprite("worker_avatar_ref1.png")
                     ?? UIHelper.LoadProjectSprite("worker_miner_avatar.jpg");
            if (avSpr != null)
            {
                var avImg = avatarWrap.gameObject.AddComponent<Image>();
                avImg.sprite = avSpr;
                avImg.preserveAspect = true;
            }

            // Info Column on Right
            var infoCol = UIHelper.MakeVertical("InfoCol", card.transform, 6, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(infoCol.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 280, minHeight: 260);
            infoCol.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperLeft;

            // Top Row: Worker Name + Edit Button
            var topRow = UIHelper.MakeHorizontal("TopRow", infoCol, 10, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(topRow.gameObject, preferredHeight: 64, minHeight: 60);
            topRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var nameLbl = UIHelper.MakeLabel("label-worker-name", topRow, "Trainee Worker", 46, Hex("#0F172A"), TextAlignmentOptions.Left, bold: true, wrap: true);
            UIHelper.SetLayout(nameLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 58, minHeight: 48);

            // Edit button (pill)
            var editBtn = UIHelper.MakeButton("btn-edit", topRow, "", 14, Hex("#F8FAFC"), Hex("#334155"), 18);
            UIHelper.SetLayout(editBtn.gameObject, preferredWidth: 130, minWidth: 120, preferredHeight: 54, minHeight: 48);
            var editBorder = editBtn.gameObject.AddComponent<Outline>();
            editBorder.effectColor = Hex("#E2E8F0");
            editBorder.effectDistance = new Vector2(1f, -1f);

            var editRow = UIHelper.MakeHorizontal("EditRow", editBtn.transform, 8, childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(editRow, 0, 0, 0, 0);
            editRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var pencilGO = UIHelper.MakeRect("PencilIcon", editRow);
            UIHelper.SetLayout(pencilGO.gameObject, preferredWidth: 22, minWidth: 22, preferredHeight: 22, minHeight: 22);
            var pencilImg = pencilGO.gameObject.AddComponent<Image>();
            pencilImg.sprite = UIHelper.GetPencilSprite();
            pencilImg.color = Hex("#334155");
            pencilImg.preserveAspect = true;

            var editLbl = UIHelper.MakeLabel("EditLbl", editRow, "Edit", 28, Hex("#334155"), TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(editLbl.gameObject, preferredWidth: 54, minWidth: 54);

            // Role Tag
            var tagGO = UIHelper.MakeRect("TagGO", infoCol);
            UIHelper.SetLayout(tagGO.gameObject, preferredWidth: 210, minWidth: 190, preferredHeight: 52, minHeight: 48);
            var tagImg = tagGO.gameObject.AddComponent<Image>();
            tagImg.color = Hex("#DCFCE7");
            tagImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(tagImg, 12);

            var tagLbl = UIHelper.MakeLabel("label-worker-role", tagGO, "Mine Worker", 30, Hex("#15803D"), TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(tagLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Details metadata rows
            var detailsList = UIHelper.MakeVertical("DetailsList", infoCol, 6, childForceWidth: false, childForceHeight: false);
            MakeIconRow(detailsList, "label-worker-id", UIHelper.GetIdCardSprite(), "Employee ID: Unassigned");
            MakeIconRow(detailsList, "label-site", UIHelper.GetPinSprite(), "Site: Industrial Facility");
            MakeIconRow(detailsList, "label-department", UIHelper.GetWorkforceSprite(), "Department of Mines, Jharkhand");
        }

        private static void MakeIconRow(Transform parent, string id, Sprite icon, string text)
        {
            var row = UIHelper.MakeHorizontal(id + "Row", parent, 10, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject, preferredHeight: 44, minHeight: 40);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var icGO = UIHelper.MakeRect("Icon", row);
            UIHelper.SetLayout(icGO.gameObject, preferredWidth: 30, minWidth: 28, preferredHeight: 30, minHeight: 28);
            var img = icGO.gameObject.AddComponent<Image>();
            img.sprite = icon;
            img.color = Hex("#64748B");
            img.preserveAspect = true;

            var lbl = UIHelper.MakeLabel(id, row, text, 32, Hex("#475569"), TextAlignmentOptions.Left, wrap: true);
            UIHelper.SetLayout(lbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 44, minHeight: 40);
        }

        // =================================================================
        // 3. STATS ROW
        // =================================================================
        private static void BuildStatsRow(Transform parent)
        {
            var wrap = UIHelper.MakeRect("StatsWrap", parent);
            UIHelper.SetLayout(wrap.gameObject, preferredHeight: 180, minHeight: 180);

            var hlg = wrap.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(32, 32, 0, 0);
            hlg.childForceExpandWidth = true;
            hlg.childControlWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlHeight = true;
            hlg.spacing = 16;

            var stat1 = MakeStatCard(wrap, "stat-1", "0", "Completed Modules", UIHelper.GetMedalSprite(), Hex("#059669"), Hex("#ECFDF5"));
            var stat2 = MakeStatCard(wrap, "stat-2", "0%", "Average Score", UIHelper.GetChartSprite(), Hex("#2563EB"), Hex("#EFF6FF"));
            var stat3 = MakeStatCard(wrap, "stat-3", "0", "Total Badges", UIHelper.GetShieldSprite(), Hex("#D97706"), Hex("#FFFBEB"));
        }

        private static GameObject MakeStatCard(Transform parent, string id, string value, string label, Sprite icon, Color accentColor, Color bgColor)
        {
            var card = new GameObject(id);
            card.transform.SetParent(parent, false);

            var cardImg = card.AddComponent<Image>();
            cardImg.color = Color.white;
            cardImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cardImg, 22);

            var outline = card.AddComponent<Outline>();
            outline.effectColor = Hex("#F1F5F9");
            outline.effectDistance = new Vector2(1f, -1f);

            var shadow = card.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.03f);
            shadow.effectDistance = new Vector2(0, -4f);

            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(16, 16, 18, 18);
            vlg.spacing = 6;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandWidth = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlHeight = true;

            var valLbl = UIHelper.MakeLabel("label-" + id + "-value", card.transform, value, 46, Hex("#0F172A"), TextAlignmentOptions.Center, bold: true);
            var txtLbl = UIHelper.MakeLabel("label-" + id + "-text", card.transform, label, 28, Hex("#64748B"), TextAlignmentOptions.Center);
            txtLbl.textWrappingMode = TextWrappingModes.Normal;

            return card;
        }

        // =================================================================
        // 4. ACTION LIST
        // =================================================================
        private static void BuildActionList(Transform parent)
        {
            var wrap = UIHelper.MakeRect("ActionListWrap", parent);
            var vlg = wrap.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(32, 32, 0, 0);
            vlg.spacing = 16;
            vlg.childForceExpandWidth = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlHeight = true;

            var csf = wrap.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var shieldSpr = UIHelper.LoadProjectSprite("icon_shield_check.png") ?? UIHelper.GetShieldSprite();
            var medalSpr  = UIHelper.LoadProjectSprite("icon_medal_cert.png") ?? UIHelper.GetMedalSprite();
            var gearSpr   = UIHelper.LoadProjectSprite("icon_gear_settings.png") ?? UIHelper.GetLockSprite();

            MakeListButton(wrap, "btn-personal", UIHelper.GetProfileSprite(), "Personal Information", "Name, Contact, Department", Hex("#EFF6FF"), Hex("#2563EB"), Color.white, Hex("#0F172A"));
            MakeListButton(wrap, "btn-safety", shieldSpr, "Safety Preferences", "Language, Notifications", Hex("#DCFCE7"), Hex("#16A34A"), Color.white, Hex("#0F172A"));
            MakeListButton(wrap, "btn-certs", medalSpr, "My Certificates", "View and download your certificates", Hex("#FEF3C7"), Hex("#D97706"), Color.white, Hex("#0F172A"));
            MakeListButton(wrap, "btn-history", UIHelper.GetChartSprite(), "Training History", "Completed modules and scores", Hex("#EFF6FF"), Hex("#2563EB"), Color.white, Hex("#0F172A"));
            MakeListButton(wrap, "btn-settings", gearSpr, "App Settings", "Sound, Privacy, Help", Hex("#F1F5F9"), Hex("#64748B"), Color.white, Hex("#0F172A"));
        }

        private static void MakeListButton(Transform parent, string id, Sprite icon, string title, string sub, Color bgCol, Color iconCol, Color cardBg, Color titleColor)
        {
            var btn = UIHelper.MakeButton(id, parent, "", 14, cardBg, cardBg, 22);
            UIHelper.SetLayout(btn.gameObject, preferredHeight: 140, minHeight: 140);

            var outline = btn.gameObject.AddComponent<Outline>();
            outline.effectColor = Hex("#F1F5F9");
            outline.effectDistance = new Vector2(1f, -1f);

            var shadow = btn.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.03f);
            shadow.effectDistance = new Vector2(0, -4f);

            var hlg = btn.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(24, 24, 0, 0);
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childControlHeight = true;

            var icBox = UIHelper.MakeRect("IconBox", btn.transform);
            UIHelper.SetLayout(icBox.gameObject, preferredWidth: 80, minWidth: 80, preferredHeight: 80, minHeight: 80);
            var icBg = icBox.gameObject.AddComponent<Image>();
            icBg.color = bgCol;
            icBg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(icBg, 18);

            var ic = UIHelper.MakeRect("Icon", icBox);
            ic.anchorMin = new Vector2(0.5f, 0.5f);
            ic.anchorMax = new Vector2(0.5f, 0.5f);
            ic.pivot = new Vector2(0.5f, 0.5f);
            ic.sizeDelta = new Vector2(46, 46);
            var icImg = ic.gameObject.AddComponent<Image>();
            icImg.sprite = icon;
            icImg.color = iconCol;
            icImg.preserveAspect = true;

            var col = UIHelper.MakeVertical("TextCol", btn.transform, 4, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(col.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 86, minHeight: 80);
            col.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var tLbl = UIHelper.MakeLabel("label-" + id, col, title, 38, titleColor, TextAlignmentOptions.Left, bold: true, wrap: true);
            var sLbl = UIHelper.MakeLabel("label-sub-" + id, col, sub, 30, Hex("#64748B"), TextAlignmentOptions.Left, wrap: true);

            var arrowGO = UIHelper.MakeRect("Arrow", btn.transform);
            UIHelper.SetLayout(arrowGO.gameObject, preferredWidth: 32, minWidth: 32, preferredHeight: 32, minHeight: 32);
            var arrowImg = arrowGO.gameObject.AddComponent<Image>();
            arrowImg.sprite = UIHelper.GetRightChevronSprite();
            arrowImg.color = Hex("#94A3B8");
            arrowImg.preserveAspect = true;
        }

        // =================================================================
        // 5. LOGOUT BUTTON
        // =================================================================
        private static void BuildLogoutButton(Transform parent)
        {
            var wrap = UIHelper.MakeRect("LogoutWrap", parent);
            UIHelper.SetLayout(wrap.gameObject, preferredHeight: 140, minHeight: 140);

            var vlg = wrap.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(32, 32, 0, 0);
            vlg.childForceExpandWidth = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = true;
            vlg.childControlHeight = true;

            MakeListButton(wrap, "btn-logout", UIHelper.GetLogoutSprite(), "Logout", "Sign out from this device", Hex("#FEE2E2"), Hex("#DC2626"), Hex("#FFF1F2"), Hex("#DC2626"));
        }

        // =================================================================
        // 6. MISSION BANNER
        // =================================================================
        private static void BuildMissionBanner(Transform parent)
        {
            var wrap = UIHelper.MakeRect("MissionWrap", parent);
            UIHelper.SetLayout(wrap.gameObject, preferredHeight: 220, minHeight: 220);

            var vlg = wrap.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(32, 32, 4, 12);
            vlg.childForceExpandWidth = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = true;
            vlg.childControlHeight = true;

            var bannerGO = new GameObject("MissionBannerCard");
            bannerGO.transform.SetParent(wrap, false);

            var maskImg = bannerGO.AddComponent<Image>();
            maskImg.color = Color.white;
            maskImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(maskImg, 24);

            var mask = bannerGO.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            var bgSpr = UIHelper.LoadProjectSprite("mission_banner_perfect.png")
                     ?? UIHelper.LoadProjectSprite("miners_team_banner.jpg");
            if (bgSpr != null)
            {
                var bgRT = UIHelper.MakeRect("BannerBg", bannerGO.transform);
                UIHelper.Stretch(bgRT, 0, 0, 0, 0);
                var bgImage = bgRT.gameObject.AddComponent<Image>();
                bgImage.sprite = bgSpr;
                bgImage.preserveAspect = false;
                bgImage.raycastTarget = false;
            }

            var shadow = bannerGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.05f);
            shadow.effectDistance = new Vector2(0, -4f);
        }

        // =================================================================
        // 7. BOTTOM NAVIGATION
        // =================================================================
        private static void MakeBottomNav(Transform parent)
        {
            var navRT = UIHelper.MakeRect("BottomNavBar", parent);
            navRT.anchorMin = Vector2.zero;
            navRT.anchorMax = new Vector2(1, 0);
            navRT.pivot = new Vector2(0.5f, 0f);
            navRT.sizeDelta = new Vector2(0, NAV_H);
            navRT.offsetMin = Vector2.zero;
            navRT.offsetMax = new Vector2(0, NAV_H);

            var bg = navRT.gameObject.AddComponent<Image>();
            bg.color = Color.white;
            bg.sprite = UIHelper.GetWhiteSprite();

            var topBorder = UIHelper.MakeRect("TopBorder", navRT);
            topBorder.anchorMin = new Vector2(0, 1);
            topBorder.anchorMax = new Vector2(1, 1);
            topBorder.pivot = new Vector2(0.5f, 1f);
            topBorder.sizeDelta = new Vector2(0, 1.5f);
            topBorder.anchoredPosition = Vector2.zero;
            var tbImg = topBorder.gameObject.AddComponent<Image>();
            tbImg.color = Hex("#F1F5F9");
            var tbLE = topBorder.gameObject.AddComponent<LayoutElement>();
            tbLE.ignoreLayout = true;

            var hlg = navRT.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.padding = new RectOffset(0, 0, 8, 36);

            MakeNavItem(navRT, "nav-home", "Home", UIHelper.GetHomeSprite(), false);
            MakeNavItem(navRT, "nav-learn", "Learn", UIHelper.GetBookSprite(), false);
            MakeNavItem(navRT, "nav-progress", "My Progress", UIHelper.GetChartSprite(), false);
            MakeNavItem(navRT, "nav-certificates", "Certificates", UIHelper.GetMedalSprite(), false);

            // Centered phone home indicator bar at bottom
            var barGO = UIHelper.MakeRect("PhoneHomeIndicator", navRT);
            barGO.anchorMin = new Vector2(0.5f, 0f);
            barGO.anchorMax = new Vector2(0.5f, 0f);
            barGO.pivot = new Vector2(0.5f, 0f);
            barGO.sizeDelta = new Vector2(280f, 8f);
            barGO.anchoredPosition = new Vector2(0f, 14f);

            var barImg = barGO.gameObject.AddComponent<Image>();
            barImg.color = Hex("#94A3B8");
            barImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(barImg, 4);

            var barLE = barGO.gameObject.AddComponent<LayoutElement>();
            barLE.ignoreLayout = true;
        }

        private static void MakeNavItem(Transform parent, string name, string label, Sprite iconSprite, bool active)
        {
            var activeColor = active ? Hex("#16A34A") : Hex("#94A3B8");
            var activeFontSize = 30f;

            var btn = UIHelper.MakeButton(name, parent, "", 14, UIColors.Transparent, Color.white, 0);

            var col = UIHelper.MakeVertical("Col", btn.transform, 6, new RectOffset(0, 0, 0, 4), childForceWidth: true, childForceHeight: false);
            UIHelper.Stretch(col, 0, 0, 0, 0);
            col.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var iconBox = UIHelper.MakeRect("IconBox", col);
            UIHelper.SetLayout(iconBox.gameObject, preferredWidth: 58, minWidth: 58, preferredHeight: 58, minHeight: 58);
            var iconImg = iconBox.gameObject.AddComponent<Image>();
            iconImg.sprite = iconSprite;
            iconImg.color = activeColor;
            iconImg.preserveAspect = true;

            var lbl = UIHelper.MakeLabel($"label-{name}", col, label, activeFontSize, activeColor, TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(lbl.gameObject, preferredHeight: 46, minHeight: 46);

            // Horizontal underline indicator under the active tab
            var indBox = UIHelper.MakeRect("IndBox", col);
            UIHelper.SetLayout(indBox.gameObject, preferredWidth: 54, minWidth: 54, preferredHeight: 6, minHeight: 6);
            if (active)
            {
                var indImg = indBox.gameObject.AddComponent<Image>();
                indImg.color = Hex("#059669");
                indImg.sprite = UIHelper.GetWhiteSprite();
                UIHelper.SetImageRoundedSprite(indImg, 3);
            }
        }

        // =================================================================
        // 8. PERSONAL INFO & EDIT MODAL
        // =================================================================
        private static void BuildPersonalInfoModal(Transform parent)
        {
            var overlay = UIHelper.MakeRect("PersonalInfoModal", parent);
            UIHelper.Stretch(overlay, 0, 0, 0, 0);

            var bgImg = overlay.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.5f);
            var bgBtn = overlay.gameObject.AddComponent<Button>();
            bgBtn.targetGraphic = bgImg;

            var card = UIHelper.MakeRect("Card", overlay);
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(1000f, 1220f);

            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 24);

            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.15f);
            shadow.effectDistance = new Vector2(0, -8f);

            var border = card.gameObject.AddComponent<Outline>();
            border.effectColor = Hex("#E2E8F0");
            border.effectDistance = new Vector2(1.5f, -1.5f);

            var vlg = card.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(36, 36, 30, 30);
            vlg.spacing = 16;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            // Header Row
            var header = UIHelper.MakeHorizontal("Header", card, 12, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(header.gameObject, preferredHeight: 68, minHeight: 68);
            header.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var titleLbl = UIHelper.MakeLabel("label-modal-personal-title", header, "Personal Information", 46, Hex("#0F172A"), TextAlignmentOptions.Left, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 68, minHeight: 68);

            var closeBtn = UIHelper.MakeButton("btn-close-personal", header, "X", 24, Hex("#F1F5F9"), Hex("#334155"), 20);
            UIHelper.SetLayout(closeBtn.gameObject, preferredWidth: 68, minWidth: 68, preferredHeight: 68, minHeight: 68);

            // Read-only Details
            var detailsBox = UIHelper.MakeVertical("ReadOnlyDetails", card, 8, childForceWidth: true, childForceHeight: false);
            MakeModalInfoRow(detailsBox, "modal-field-worker-id", UIHelper.GetIdCardSprite(), "Employee ID: Unassigned");
            MakeModalInfoRow(detailsBox, "modal-field-department", UIHelper.GetWorkforceSprite(), "Department of Mines, Jharkhand");
            MakeModalInfoRow(detailsBox, "modal-field-contact", UIHelper.GetPinSprite(), "Contact: Not provided");

            // Divider
            var div = UIHelper.MakeRect("Div", card);
            UIHelper.SetLayout(div.gameObject, preferredHeight: 2, minHeight: 2);
            div.gameObject.AddComponent<Image>().color = Hex("#E2E8F0");

            // Editable Section
            var editHeader = UIHelper.MakeLabel("modal-label-edit-heading", card, "Edit Profile Details", 36, Hex("#0F172A"), TextAlignmentOptions.Left, bold: true);
            UIHelper.SetLayout(editHeader.gameObject, preferredHeight: 44, minHeight: 44);

            UIHelper.MakeLabel("lbl-name-field", card, "Worker Full Name", 30, Hex("#64748B"), TextAlignmentOptions.Left, bold: true);
            MakeInputField(card, "input-edit-name", "Enter worker name", UIHelper.GetProfileSprite());

            UIHelper.MakeLabel("lbl-site-field", card, "Mining Site / Facility", 30, Hex("#64748B"), TextAlignmentOptions.Left, bold: true);
            MakeInputField(card, "input-edit-site", "Enter mining site", UIHelper.GetPinSprite());

            UIHelper.MakeLabel("lbl-role-field", card, "Designation / Role", 30, Hex("#64748B"), TextAlignmentOptions.Left, bold: true);
            MakeInputField(card, "input-edit-role", "Enter worker role", UIHelper.GetWorkforceSprite());

            // Status feedback label
            var statusLbl = UIHelper.MakeLabel("label-edit-status", card, "", 28, Hex("#059669"), TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(statusLbl.gameObject, preferredHeight: 32, minHeight: 32);

            // Action Buttons
            var btnRow = UIHelper.MakeHorizontal("BtnRow", card, 16, childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(btnRow.gameObject, preferredHeight: 92, minHeight: 92);

            var saveBtn = UIHelper.MakeButton("btn-save-profile", btnRow, "Save Changes", 30, Hex("#059669"), Color.white, 16);
            UIHelper.SetLayout(saveBtn.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 88, minHeight: 84);
            var cancelBtn = UIHelper.MakeButton("btn-dismiss-personal", btnRow, "Close", 30, Hex("#F1F5F9"), Hex("#334155"), 16);
            UIHelper.SetLayout(cancelBtn.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 88, minHeight: 84);

            overlay.gameObject.SetActive(false);
        }

        private static void MakeModalInfoRow(Transform parent, string id, Sprite icon, string text)
        {
            var row = UIHelper.MakeHorizontal(id + "Row", parent, 12, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject, preferredHeight: 44, minHeight: 44);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var icGO = UIHelper.MakeRect("Icon", row);
            UIHelper.SetLayout(icGO.gameObject, preferredWidth: 28, minWidth: 28, preferredHeight: 28, minHeight: 28);
            var icImg = icGO.gameObject.AddComponent<Image>();
            icImg.sprite = icon;
            icImg.color = Hex("#64748B");
            icImg.preserveAspect = true;

            var lbl = UIHelper.MakeLabel(id, row, text, 32, Hex("#334155"), TextAlignmentOptions.Left);
            UIHelper.SetLayout(lbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 44, minHeight: 44);
        }

        private static TMP_InputField MakeInputField(Transform parent, string name, string placeholder, Sprite icon)
        {
            var box = UIHelper.MakeRect(name, parent);
            UIHelper.SetLayout(box.gameObject, preferredHeight: 96, minHeight: 96);

            var boxImg = box.gameObject.AddComponent<Image>();
            boxImg.color = Hex("#F8FAFC");
            boxImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(boxImg, 14f);

            var boxOutline = box.gameObject.AddComponent<Outline>();
            boxOutline.effectColor = Hex("#CBD5E1");
            boxOutline.effectDistance = new Vector2(1.2f, -1.2f);

            var boxHLG = box.gameObject.AddComponent<HorizontalLayoutGroup>();
            boxHLG.padding = new RectOffset(18, 18, 0, 0);
            boxHLG.spacing = 14;
            boxHLG.childAlignment = TextAnchor.MiddleLeft;
            boxHLG.childForceExpandWidth = false;
            boxHLG.childControlWidth = true;
            boxHLG.childForceExpandHeight = false;
            boxHLG.childControlHeight = true;

            if (icon != null)
            {
                var icGO = UIHelper.MakeRect("Icon", box);
                UIHelper.SetLayout(icGO.gameObject, preferredWidth: 36, minWidth: 36, preferredHeight: 36, minHeight: 36);
                var icImg = icGO.gameObject.AddComponent<Image>();
                icImg.sprite = icon;
                icImg.color = Hex("#64748B");
                icImg.preserveAspect = true;
            }

            var inputGO = UIHelper.MakeRect("TMPInput", box);
            UIHelper.SetLayout(inputGO.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 76, minHeight: 76);

            var inputField = inputGO.gameObject.AddComponent<TMP_InputField>();

            var textGO = UIHelper.MakeRect("Text", inputGO);
            UIHelper.Stretch(textGO, 0, 0, 0, 0);
            var textTMP = UIHelper.AddTMP(textGO.gameObject);
            textTMP.fontSize = 28f;
            textTMP.color = Hex("#0F172A");
            textTMP.alignment = TextAlignmentOptions.Left;
            inputField.textComponent = textTMP;

            var phGO = UIHelper.MakeRect("Placeholder", inputGO);
            UIHelper.Stretch(phGO, 0, 0, 0, 0);
            var phTMP = UIHelper.AddTMP(phGO.gameObject);
            phTMP.fontSize = 28f;
            phTMP.color = Hex("#94A3B8");
            phTMP.alignment = TextAlignmentOptions.Left;
            phTMP.text = placeholder;
            inputField.placeholder = phTMP;

            return inputField;
        }

        // =================================================================
        // 9. SAFETY PREFERENCES MODAL
        // =================================================================
        private static void BuildSafetyPreferencesModal(Transform parent)
        {
            var overlay = UIHelper.MakeRect("SafetyPreferencesModal", parent);
            UIHelper.Stretch(overlay, 0, 0, 0, 0);

            var bgImg = overlay.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.5f);
            var bgBtn = overlay.gameObject.AddComponent<Button>();
            bgBtn.targetGraphic = bgImg;

            var card = UIHelper.MakeRect("Card", overlay);
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(1000f, 980f);

            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 24);

            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.15f);
            shadow.effectDistance = new Vector2(0, -8f);

            var border = card.gameObject.AddComponent<Outline>();
            border.effectColor = Hex("#E2E8F0");
            border.effectDistance = new Vector2(1.5f, -1.5f);

            var vlg = card.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(36, 36, 30, 30);
            vlg.spacing = 18;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            // Header
            var header = UIHelper.MakeHorizontal("Header", card, 12, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(header.gameObject, preferredHeight: 68, minHeight: 68);
            header.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var titleLbl = UIHelper.MakeLabel("label-modal-safety-title", header, "Safety Preferences", 46, Hex("#0F172A"), TextAlignmentOptions.Left, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 68, minHeight: 68);

            var closeBtn = UIHelper.MakeButton("btn-close-safety", header, "X", 24, Hex("#F1F5F9"), Hex("#334155"), 20);
            UIHelper.SetLayout(closeBtn.gameObject, preferredWidth: 68, minWidth: 68, preferredHeight: 68, minHeight: 68);

            // Language Selector Section
            UIHelper.MakeLabel("label-pref-lang-title", card, "Application Language", 32, Hex("#64748B"), TextAlignmentOptions.Left, bold: true);
            var langRow = UIHelper.MakeHorizontal("LangRow", card, 12, childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(langRow.gameObject, preferredHeight: 76, minHeight: 76);

            MakeLangPill(langRow, "btn-safety-lang-en", "English", UIHelper.GetDefaultFont());
            MakeLangPill(langRow, "btn-safety-lang-hi", "हिंदी", UIHelper.GetDevanagariFont());
            MakeLangPill(langRow, "btn-safety-lang-sa", "ᱚᱞ ᱪᱤᱠᱤ", UIHelper.GetSantaliFont());

            // Toggles
            MakeToggleRow(card, "toggle-safety-audio", "Audio Safety Alerts", "Hazard sound alerts and voice cues", UIHelper.GetBellSprite(), true);
            MakeToggleRow(card, "toggle-high-contrast", "High Contrast Mode", "Optimize visibility for harsh mine lighting", UIHelper.GetShieldSprite(), false);

            var div = UIHelper.MakeRect("Div", card);
            UIHelper.SetLayout(div.gameObject, preferredHeight: 2, minHeight: 2);
            div.gameObject.AddComponent<Image>().color = Hex("#E2E8F0");

            var doneBtn = UIHelper.MakeButton("btn-done-safety", card, "Close", 30, Hex("#059669"), Color.white, 16);
            UIHelper.SetLayout(doneBtn.gameObject, preferredHeight: 88, minHeight: 84);

            overlay.gameObject.SetActive(false);
        }

        private static void MakeLangPill(Transform parent, string id, string label, TMP_FontAsset font)
        {
            var btn = UIHelper.MakeButton(id, parent, label, 32, Hex("#F8FAFC"), Hex("#0F172A"), 14);
            UIHelper.SetLayout(btn.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 76, minHeight: 76);
            var btnTmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnTmp != null)
            {
                if (font != null) btnTmp.font = font;
                btnTmp.fontSize = 32f;
                btnTmp.fontStyle = FontStyles.Bold;
            }
            var border = btn.gameObject.AddComponent<Outline>();
            border.effectColor = Hex("#CBD5E1");
            border.effectDistance = new Vector2(1f, -1f);
        }

        // =================================================================
        // 10. APP SETTINGS MODAL
        // =================================================================
        private static void BuildAppSettingsModal(Transform parent)
        {
            var overlay = UIHelper.MakeRect("AppSettingsModal", parent);
            UIHelper.Stretch(overlay, 0, 0, 0, 0);

            var bgImg = overlay.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.5f);
            var bgBtn = overlay.gameObject.AddComponent<Button>();
            bgBtn.targetGraphic = bgImg;

            var card = UIHelper.MakeRect("Card", overlay);
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(1000f, 1020f);

            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 24);

            var shadow = card.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.15f);
            shadow.effectDistance = new Vector2(0, -8f);

            var border = card.gameObject.AddComponent<Outline>();
            border.effectColor = Hex("#E2E8F0");
            border.effectDistance = new Vector2(1.5f, -1.5f);

            var vlg = card.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(36, 36, 30, 30);
            vlg.spacing = 16;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            // Header
            var header = UIHelper.MakeHorizontal("Header", card, 12, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(header.gameObject, preferredHeight: 68, minHeight: 68);
            header.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var titleLbl = UIHelper.MakeLabel("label-modal-settings-title", header, "App Settings", 46, Hex("#0F172A"), TextAlignmentOptions.Left, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 68, minHeight: 68);

            var closeBtn = UIHelper.MakeButton("btn-close-settings", header, "X", 24, Hex("#F1F5F9"), Hex("#334155"), 20);
            UIHelper.SetLayout(closeBtn.gameObject, preferredWidth: 68, minWidth: 68, preferredHeight: 68, minHeight: 68);

            // Toggles
            MakeToggleRow(card, "toggle-settings-sound", "Sound Effects", "Audio cues and spray sound", UIHelper.GetBellSprite(), true);
            MakeToggleRow(card, "toggle-settings-haptic", "Haptic Vibration", "Tactile feedback on pin pull and spray", UIHelper.GetLockSprite(), true);
            MakeToggleRow(card, "toggle-settings-sync", "Offline Auto-Sync", "Synchronize sessions when connected to network", UIHelper.GetClockSprite(), true);

            // Version info card
            var verCard = UIHelper.MakeVertical("VersionCard", card, 4, new RectOffset(20, 20, 16, 16), childForceWidth: true, childForceHeight: false);
            var vcImg = verCard.gameObject.AddComponent<Image>();
            vcImg.color = Hex("#F8FAFC");
            UIHelper.SetImageRoundedSprite(vcImg, 14);

            var verLbl = UIHelper.MakeLabel("label-settings-version", verCard, "SurakshaAR v1.0.0 (Build 6000.6)", 32, Hex("#0F172A"), TextAlignmentOptions.Left, bold: true);
            var engLbl = UIHelper.MakeLabel("label-settings-engine", verCard, "Unity 6000.6.0f1 • Pure uGUI • DGMS Mining Safety", 28, Hex("#64748B"), TextAlignmentOptions.Left);
            var privLbl = UIHelper.MakeLabel("label-settings-privacy", verCard, "All training records are stored securely on-device with offline-first encryption.", 26, Hex("#94A3B8"), TextAlignmentOptions.Left);

            var doneBtn = UIHelper.MakeButton("btn-done-settings", card, "Close", 30, Hex("#059669"), Color.white, 16);
            UIHelper.SetLayout(doneBtn.gameObject, preferredHeight: 88, minHeight: 84);

            overlay.gameObject.SetActive(false);
        }

        private static Button MakeToggleRow(Transform parent, string id, string title, string subtitle, Sprite icon, bool isChecked)
        {
            var row = UIHelper.MakeRect(id + "Row", parent);
            UIHelper.SetLayout(row.gameObject, preferredHeight: 104, minHeight: 104);

            var hlg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 0, 0);
            hlg.spacing = 16;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = true;

            if (icon != null)
            {
                var icGO = UIHelper.MakeRect("Icon", row);
                UIHelper.SetLayout(icGO.gameObject, preferredWidth: 44, minWidth: 44, preferredHeight: 44, minHeight: 44);
                var icImg = icGO.gameObject.AddComponent<Image>();
                icImg.sprite = icon;
                icImg.color = Hex("#2563EB");
                icImg.preserveAspect = true;
            }

            var col = UIHelper.MakeVertical("TextCol", row, 2, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(col.gameObject, flexibleWidth: true, flexWidth: 1);
            col.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var tLbl = UIHelper.MakeLabel("label-" + id, col, title, 34, Hex("#0F172A"), TextAlignmentOptions.Left, bold: true);
            if (!string.IsNullOrEmpty(subtitle))
            {
                var sLbl = UIHelper.MakeLabel("sub-" + id, col, subtitle, 28, Hex("#64748B"), TextAlignmentOptions.Left);
            }

            var btn = UIHelper.MakeButton(id, row, "", 14, isChecked ? Hex("#059669") : Hex("#CBD5E1"), Color.white, 24);
            UIHelper.SetLayout(btn.gameObject, preferredWidth: 84, minWidth: 84, preferredHeight: 48, minHeight: 48);

            var dot = UIHelper.MakeRect("CheckIndicator", btn.transform);
            dot.anchorMin = new Vector2(isChecked ? 0.72f : 0.28f, 0.5f);
            dot.anchorMax = new Vector2(isChecked ? 0.72f : 0.28f, 0.5f);
            dot.pivot = new Vector2(0.5f, 0.5f);
            dot.sizeDelta = new Vector2(34, 34);
            var dotImg = dot.gameObject.AddComponent<Image>();
            dotImg.color = Color.white;
            dotImg.sprite = UIHelper.GetCircleSprite();

            return btn;
        }
    }
}
