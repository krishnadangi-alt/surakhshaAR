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
        private const float NAV_H = 190f;
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
            vlg.padding = new RectOffset(0, 0, 0, 40);
            vlg.spacing = 16;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            BuildHeader(content);
            BuildProfileCard(content);
            BuildStatsRow(content);
            BuildInfoList(content);
            BuildLogoutButton(content);
            BuildMissionBanner(content);

            return root;
        }

        // =================================================================
        // 1. HEADER SECTION
        // =================================================================
        private static void BuildHeader(Transform parent)
        {
            var wrap = UIHelper.MakeRect("HeaderWrap", parent);
            UIHelper.SetLayout(wrap.gameObject, preferredHeight: 339, minHeight: 339);

            var headerImg = wrap.gameObject.AddComponent<Image>();
            headerImg.color = Color.white;
            var spr = UIHelper.LoadProjectSprite("header_ref2_clean_bg.png")
                   ?? UIHelper.LoadProjectSprite("jharkhand_mine_banner_clean.jpg");
            if (spr != null)
            {
                headerImg.sprite = spr;
                headerImg.preserveAspect = false;
            }

            // Interactive Back button placed right over the circular back button icon
            var backBtnGO = UIHelper.MakeRect("btn-back", wrap);
            backBtnGO.anchorMin = new Vector2(0f, 1f);
            backBtnGO.anchorMax = new Vector2(0f, 1f);
            backBtnGO.pivot = new Vector2(0.5f, 0.5f);
            backBtnGO.anchoredPosition = new Vector2(99f, -71f);
            backBtnGO.sizeDelta = new Vector2(100f, 100f);

            var btnImg = backBtnGO.gameObject.AddComponent<Image>();
            btnImg.color = Color.clear;
            btnImg.raycastTarget = true;

            var btn = backBtnGO.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.2f);
            colors.pressedColor = new Color(0f, 0f, 0f, 0.1f);
            btn.colors = colors;
        }

        // =================================================================
        // 2. PROFILE CARD
        // =================================================================
        private static void BuildProfileCard(Transform parent)
        {
            var wrap = UIHelper.MakeRect("ProfileCardWrap", parent);
            UIHelper.SetLayout(wrap.gameObject, preferredHeight: 320, minHeight: 320);

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
            hlg.padding = new RectOffset(28, 28, 22, 22);
            hlg.spacing = 24;
            hlg.childAlignment = TextAnchor.UpperLeft;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childControlHeight = true;

            // Avatar on Left
            var avatarWrap = UIHelper.MakeRect("AvatarWrap", card.transform);
            UIHelper.SetLayout(avatarWrap.gameObject, preferredWidth: 200, minWidth: 200, preferredHeight: 200, minHeight: 200);

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
            UIHelper.SetLayout(infoCol.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 250, minHeight: 250);
            infoCol.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperLeft;

            // Top Row: Worker Name + Edit Button
            var topRow = UIHelper.MakeHorizontal("TopRow", infoCol, 10, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(topRow.gameObject, preferredHeight: 52, minHeight: 52);
            topRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var nameLbl = UIHelper.MakeLabel("label-worker-name", topRow, "Ramesh Kumar", 42, Hex("#0F172A"), TextAlignmentOptions.Left, bold: true);
            UIHelper.SetLayout(nameLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 52, minHeight: 52);

            // Edit button (pill)
            var editBtn = UIHelper.MakeButton("btn-edit", topRow, "", 14, Hex("#F8FAFC"), Hex("#334155"), 18);
            UIHelper.SetLayout(editBtn.gameObject, preferredWidth: 106, minWidth: 106, preferredHeight: 46, minHeight: 46);
            var editBorder = editBtn.gameObject.AddComponent<Outline>();
            editBorder.effectColor = Hex("#E2E8F0");
            editBorder.effectDistance = new Vector2(1f, -1f);

            var editRow = UIHelper.MakeHorizontal("EditRow", editBtn.transform, 8, childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(editRow, 0, 0, 0, 0);
            editRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var pencilGO = UIHelper.MakeRect("PencilIcon", editRow);
            UIHelper.SetLayout(pencilGO.gameObject, preferredWidth: 20, minWidth: 20, preferredHeight: 20, minHeight: 20);
            var pencilImg = pencilGO.gameObject.AddComponent<Image>();
            pencilImg.sprite = UIHelper.GetPencilSprite();
            pencilImg.color = Hex("#334155");
            pencilImg.preserveAspect = true;

            var editLbl = UIHelper.MakeLabel("EditLbl", editRow, "Edit", 22, Hex("#334155"), TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(editLbl.gameObject, preferredWidth: 44, minWidth: 44);

            // Role Tag
            var tagGO = UIHelper.MakeRect("TagGO", infoCol);
            UIHelper.SetLayout(tagGO.gameObject, preferredWidth: 144, minWidth: 144, preferredHeight: 38, minHeight: 38);
            var tagImg = tagGO.gameObject.AddComponent<Image>();
            tagImg.color = Hex("#DCFCE7");
            tagImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(tagImg, 10);

            var tagLbl = UIHelper.MakeLabel("label-worker-role", tagGO, "Mine Worker", 21, Hex("#15803D"), TextAlignmentOptions.Center, bold: false);
            UIHelper.Stretch(tagLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Details metadata rows
            var detailsList = UIHelper.MakeVertical("DetailsList", infoCol, 6, childForceWidth: false, childForceHeight: false);
            MakeIconRow(detailsList, "label-worker-id", UIHelper.GetIdCardSprite(), "Employee ID: JH-MN-004821");
            MakeIconRow(detailsList, "label-site", UIHelper.GetPinSprite(), "Jharia Mine, Dhanbad");
            MakeIconRow(detailsList, "label-department", UIHelper.GetWorkforceSprite(), "Department of Mines, Jharkhand");
        }

        private static void MakeIconRow(Transform parent, string id, Sprite icon, string text)
        {
            var row = UIHelper.MakeHorizontal(id + "Row", parent, 10, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject, preferredHeight: 30, minHeight: 30);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var icGO = UIHelper.MakeRect("Icon", row);
            UIHelper.SetLayout(icGO.gameObject, preferredWidth: 24, minWidth: 24, preferredHeight: 24, minHeight: 24);
            var img = icGO.gameObject.AddComponent<Image>();
            img.sprite = icon;
            img.color = Hex("#64748B");
            img.preserveAspect = true;

            var lbl = UIHelper.MakeLabel(id, row, text, 22, Hex("#475569"), TextAlignmentOptions.Left);
            UIHelper.SetLayout(lbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 30, minHeight: 30);
        }

        // =================================================================
        // 3. STATS ROW
        // =================================================================
        private static void BuildStatsRow(Transform parent)
        {
            var wrap = UIHelper.MakeRect("StatsWrap", parent);
            UIHelper.SetLayout(wrap.gameObject, preferredHeight: 125, minHeight: 125);

            var hlg = wrap.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(32, 32, 0, 0);
            hlg.childForceExpandWidth = true;
            hlg.childControlWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlHeight = true;

            var card = new GameObject("StatsCard");
            card.transform.SetParent(wrap, false);
            var cardImg = card.AddComponent<Image>();
            cardImg.color = Hex("#F0F9FF");
            cardImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cardImg, 22);

            var outline = card.AddComponent<Outline>();
            outline.effectColor = Hex("#E0F2FE");
            outline.effectDistance = new Vector2(1f, -1f);

            var cardHlg = card.AddComponent<HorizontalLayoutGroup>();
            cardHlg.padding = new RectOffset(20, 20, 14, 14);
            cardHlg.spacing = 8;
            cardHlg.childForceExpandWidth = false; // Prevents expanding dividers into boxes!
            cardHlg.childControlWidth = true;
            cardHlg.childForceExpandHeight = true;
            cardHlg.childControlHeight = true;
            cardHlg.childAlignment = TextAnchor.MiddleCenter;

            var shieldSpr = UIHelper.LoadProjectSprite("icon_shield_check.png") ?? UIHelper.GetShieldSprite();
            MakeStatItem(card.transform, shieldSpr, "label-stat-train", "Training", "3 Completed", Hex("#059669"));
            MakeDivider(card.transform);
            MakeStatItem(card.transform, UIHelper.GetChartSprite(), "label-stat-cert", "Certificates", "2 Earned", Hex("#059669"));
            MakeDivider(card.transform);
            MakeStatItem(card.transform, UIHelper.GetClockSprite(), "label-stat-hours", "Total Hours", "4.5 Hours", Hex("#3B82F6"));
        }

        private static void MakeStatItem(Transform parent, Sprite icon, string id, string subTitle, string valText, Color iconColor)
        {
            var row = UIHelper.MakeHorizontal("StatItem", parent, 10, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 80, minHeight: 80);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var icGO = UIHelper.MakeRect("Icon", row);
            UIHelper.SetLayout(icGO.gameObject, preferredWidth: 42, minWidth: 42, preferredHeight: 42, minHeight: 42);
            var img = icGO.gameObject.AddComponent<Image>();
            img.sprite = icon;
            img.color = iconColor;
            img.preserveAspect = true;

            var col = UIHelper.MakeVertical("TextCol", row, 2, childForceWidth: false, childForceHeight: false);
            col.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var sLbl = UIHelper.MakeLabel($"sub-{id}", col, subTitle, 20, Hex("#64748B"), TextAlignmentOptions.Left);
            var vLbl = UIHelper.MakeLabel(id, col, valText, 24, Hex("#0F172A"), TextAlignmentOptions.Left, bold: true);
            vLbl.textWrappingMode = TextWrappingModes.NoWrap;
        }

        private static void MakeDivider(Transform parent)
        {
            var div = UIHelper.MakeRect("Div", parent);
            UIHelper.SetLayout(div.gameObject, preferredWidth: 1.5f, minWidth: 1.5f, flexibleWidth: false, flexWidth: 0, preferredHeight: 56, minHeight: 56);
            var img = div.gameObject.AddComponent<Image>();
            img.color = Hex("#CBD5E1");
        }

        // =================================================================
        // 4. INFO LIST
        // =================================================================
        private static void BuildInfoList(Transform parent)
        {
            var wrap = UIHelper.MakeRect("InfoListWrap", parent);
            var vlg = wrap.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(32, 32, 0, 0);
            vlg.spacing = 14;
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
            MakeListButton(wrap, "btn-settings", gearSpr, "App Settings", "Sound, Privaccy, Help", Hex("#F1F5F9"), Hex("#64748B"), Color.white, Hex("#0F172A"));
        }

        private static void MakeListButton(Transform parent, string id, Sprite icon, string title, string sub, Color bgCol, Color iconCol, Color cardBg, Color titleColor)
        {
            var btn = UIHelper.MakeButton(id, parent, "", 14, cardBg, cardBg, 20);
            UIHelper.SetLayout(btn.gameObject, preferredHeight: 104, minHeight: 104);

            var outline = btn.gameObject.AddComponent<Outline>();
            outline.effectColor = Hex("#F1F5F9");
            outline.effectDistance = new Vector2(1f, -1f);

            var shadow = btn.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.03f);
            shadow.effectDistance = new Vector2(0, -4f);

            var hlg = btn.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(22, 22, 0, 0);
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childControlHeight = true;

            var icBox = UIHelper.MakeRect("IconBox", btn.transform);
            UIHelper.SetLayout(icBox.gameObject, preferredWidth: 64, minWidth: 64, preferredHeight: 64, minHeight: 64);
            var icBg = icBox.gameObject.AddComponent<Image>();
            icBg.color = bgCol;
            icBg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(icBg, 16);

            var ic = UIHelper.MakeRect("Icon", icBox);
            ic.anchorMin = new Vector2(0.5f, 0.5f);
            ic.anchorMax = new Vector2(0.5f, 0.5f);
            ic.pivot = new Vector2(0.5f, 0.5f);
            ic.sizeDelta = new Vector2(34, 34);
            var icImg = ic.gameObject.AddComponent<Image>();
            icImg.sprite = icon;
            icImg.color = iconCol;
            icImg.preserveAspect = true;

            var col = UIHelper.MakeVertical("TextCol", btn.transform, 4, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(col.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 64, minHeight: 64);
            col.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var tLbl = UIHelper.MakeLabel("label-" + id, col, title, 28, titleColor, TextAlignmentOptions.Left, bold: true);
            var sLbl = UIHelper.MakeLabel("label-sub-" + id, col, sub, 21, Hex("#64748B"), TextAlignmentOptions.Left);

            var arrowGO = UIHelper.MakeRect("Arrow", btn.transform);
            UIHelper.SetLayout(arrowGO.gameObject, preferredWidth: 26, minWidth: 26, preferredHeight: 26, minHeight: 26);
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
            UIHelper.SetLayout(wrap.gameObject, preferredHeight: 104, minHeight: 104);

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
            UIHelper.SetLayout(wrap.gameObject, preferredHeight: 210, minHeight: 210);

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
            MakeNavItem(navRT, "nav-progress", "Progress", UIHelper.GetChartSprite(), false);
            MakeNavItem(navRT, "nav-certificates", "Profile", UIHelper.GetProfileSprite(), true);

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
            var activeColor = active ? Hex("#059669") : Hex("#94A3B8");
            var activeFontSize = 24f;

            var btn = UIHelper.MakeButton(name, parent, "", 14, UIColors.Transparent, Color.white, 0);

            var col = UIHelper.MakeVertical("Col", btn.transform, 6, new RectOffset(0, 0, 0, 4), childForceWidth: true, childForceHeight: false);
            UIHelper.Stretch(col, 0, 0, 0, 0);
            col.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var iconBox = UIHelper.MakeRect("IconBox", col);
            UIHelper.SetLayout(iconBox.gameObject, preferredWidth: 54, minWidth: 54, preferredHeight: 54, minHeight: 54);
            var iconImg = iconBox.gameObject.AddComponent<Image>();
            iconImg.sprite = iconSprite;
            iconImg.color = activeColor;
            iconImg.preserveAspect = true;

            var lbl = UIHelper.MakeLabel($"label-{name}", col, label, activeFontSize, activeColor, TextAlignmentOptions.Center, bold: active);
            UIHelper.SetLayout(lbl.gameObject, preferredHeight: 32, minHeight: 32);

            // Horizontal underline indicator under the active tab
            var indBox = UIHelper.MakeRect("IndBox", col);
            UIHelper.SetLayout(indBox.gameObject, preferredWidth: 70, minWidth: 70, preferredHeight: 6, minHeight: 6);
            if (active)
            {
                var indImg = indBox.gameObject.AddComponent<Image>();
                indImg.color = Hex("#059669");
                indImg.sprite = UIHelper.GetWhiteSprite();
                UIHelper.SetImageRoundedSprite(indImg, 3);
            }
        }
    }
}
