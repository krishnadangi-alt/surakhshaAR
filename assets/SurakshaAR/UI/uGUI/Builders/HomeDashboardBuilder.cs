using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Home Dashboard builder — Mobile-first production UI matching Reference 1.
    /// </summary>
    public static class HomeDashboardBuilder
    {
        private const float NAV_H  = 180f;

        private static Color Hex(string h) => UIColors.Hex(h);

        public static GameObject Build()
        {
            UIHelper.ClearSpriteCache();

            var root   = new GameObject("HomeDashboardScreen");
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;
            rootRT.sizeDelta = Vector2.zero;

            var rootImg = root.AddComponent<Image>();
            rootImg.color  = Hex("#F8FAFC");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            var scrollRoot = UIHelper.MakeRect("ScrollArea", root.transform);
            scrollRoot.anchorMin = Vector2.zero;
            scrollRoot.anchorMax = Vector2.one;
            scrollRoot.offsetMin = new Vector2(0, NAV_H);
            scrollRoot.offsetMax = Vector2.zero;

            var sr = scrollRoot.gameObject.AddComponent<ScrollRect>();
            sr.horizontal        = false;
            sr.vertical          = true;
            sr.movementType      = ScrollRect.MovementType.Clamped;
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
            vlg.spacing            = 0;
            vlg.padding            = new RectOffset(0, 0, 0, 20);
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth      = true;
            vlg.childControlHeight     = true;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            BuildHeaderBanner(content);
            BuildWorkerProfileCard(content);
            BuildTrainingModulesSection(content);
            BuildMissionBanner(content);

            MakeBottomNav(root.transform);

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

        private static RectTransform MakeCardWrap(Transform parent, string name, float preferredH, int paddingLeft = 32, int paddingRight = 32, int paddingTop = 0, int paddingBottom = 0)
        {
            var wrap = UIHelper.MakeRect(name + "Wrap", parent);
            UIHelper.SetLayout(wrap.gameObject, preferredHeight: preferredH + paddingTop + paddingBottom, minHeight: preferredH + paddingTop + paddingBottom);
            var hlg = wrap.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(paddingLeft, paddingRight, paddingTop, paddingBottom);
            hlg.childForceExpandWidth  = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth      = true;
            hlg.childControlHeight     = true;
            return wrap;
        }

        // =================================================================
        // 1. HEADER BANNER (Top controls + Hero Image)
        // =================================================================
        private static void BuildHeaderBanner(Transform parent)
        {
            var wrap = UIHelper.MakeRect("HeaderBannerWrap", parent);
            UIHelper.SetLayout(wrap.gameObject, preferredHeight: 660, minHeight: 660);

            var headerRT = UIHelper.MakeRect("HeaderCard", wrap);
            var headerGO = headerRT.gameObject;
            UIHelper.Stretch(headerRT, 0, 0, 0, 0);

            var headerImg = headerGO.AddComponent<Image>();
            headerImg.color = Color.white;
            headerImg.sprite = UIHelper.GetWhiteSprite();

            var mask = headerGO.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Background banner graphic (Clean daylight sky + washery structure)
            var bannerSpr = UIHelper.LoadProjectSprite("home_menu_mine_baground.png")
                         ?? UIHelper.LoadProjectSprite("jharkhand_mine_banner_clean.jpg")
                         ?? UIHelper.LoadProjectSprite("jharkhand_mine_banner.jpg");
            if (bannerSpr != null)
            {
                var bgRT = UIHelper.MakeRect("BgMineBanner", headerGO.transform);
                UIHelper.Stretch(bgRT, 0, 0, 0, 0);
                var bgImg = bgRT.gameObject.AddComponent<Image>();
                bgImg.sprite = bannerSpr;
                bgImg.color  = Color.white;
                bgImg.preserveAspect = false;
                bgImg.raycastTarget = false;
            }

            // Top interactive controls row (Language, Notification, Profile)
            var topControls = UIHelper.MakeRect("TopControlsRow", headerGO.transform);
            topControls.anchorMin = new Vector2(1f, 1f);
            topControls.anchorMax = new Vector2(1f, 1f);
            topControls.pivot     = new Vector2(1f, 1f);
            topControls.anchoredPosition = new Vector2(-28f, -28f);
            topControls.sizeDelta = new Vector2(400f, 64f);

            var hlg = topControls.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16;
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth      = true;
            hlg.childControlHeight     = true;

            // 1. Language picker pill
            var langPill = UIHelper.MakeButton("btn-language-picker", topControls, "", 14, Color.white, Color.white, 32);
            UIHelper.SetLayout(langPill.gameObject, preferredWidth: 180, minWidth: 180, preferredHeight: 64, minHeight: 64);
            var langBorder = langPill.gameObject.AddComponent<Outline>();
            langBorder.effectColor = Hex("#E2E8F0");
            langBorder.effectDistance = new Vector2(1.2f, -1.2f);

            var pillRow = UIHelper.MakeHorizontal("PillRow", langPill.transform, 8,
                new RectOffset(16, 16, 0, 0), childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(pillRow, 0, 0, 0, 0);
            pillRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var globeGO = UIHelper.MakeRect("GlobeIcon", pillRow);
            UIHelper.SetLayout(globeGO.gameObject, preferredWidth: 24, minWidth: 24, preferredHeight: 24, minHeight: 24);
            var globeImg = globeGO.gameObject.AddComponent<Image>();
            globeImg.sprite = UIHelper.GetGlobeSprite(); // user's image shows a different icon, but we keep globe
            globeImg.color  = Hex("#0E2A47");

            var langLbl = UIHelper.MakeLabel("label-lang-text", pillRow, "English", 26, Hex("#0E2A47"), TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(langLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 34, minHeight: 34);

            var chevGO = UIHelper.MakeRect("ChevIcon", pillRow);
            UIHelper.SetLayout(chevGO.gameObject, preferredWidth: 18, minWidth: 18, preferredHeight: 18, minHeight: 18);
            var chevImg = chevGO.gameObject.AddComponent<Image>();
            chevImg.sprite = UIHelper.GetDownChevronSprite();
            chevImg.color  = Hex("#0E2A47");

            // 2. Bell button
            var bellBtn = UIHelper.MakeButton("btn-notifications", topControls, "", 14, Color.white, Color.white, 32);
            UIHelper.SetLayout(bellBtn.gameObject, preferredWidth: 64, minWidth: 64, preferredHeight: 64, minHeight: 64);
            var bellBorder = bellBtn.gameObject.AddComponent<Outline>();
            bellBorder.effectColor = Hex("#E2E8F0");
            bellBorder.effectDistance = new Vector2(1.2f, -1.2f);

            var bellIconGO = UIHelper.MakeRect("BellIcon", bellBtn.transform);
            AnchorCenter(bellIconGO, 32, 32);
            var bellImg = bellIconGO.gameObject.AddComponent<Image>();
            bellImg.sprite = UIHelper.GetBellSprite();
            bellImg.color  = Hex("#0E2A47");

            var badgeGO = UIHelper.MakeRect("RedBadge", bellBtn.transform);
            badgeGO.anchorMin = new Vector2(1f, 1f);
            badgeGO.anchorMax = new Vector2(1f, 1f);
            badgeGO.pivot     = new Vector2(1f, 1f);
            badgeGO.anchoredPosition = new Vector2(4f, 4f);
            badgeGO.sizeDelta = new Vector2(28f, 28f);
            var badgeImg = badgeGO.gameObject.AddComponent<Image>();
            badgeImg.sprite = UIHelper.GetCircleSprite();
            badgeImg.color  = Hex("#EF4444");

            var bNum = UIHelper.MakeLabel("BNum", badgeGO, "3", 18, Color.white, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(bNum.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // 3. Profile button
            var profBtn = UIHelper.MakeButton("btn-profile", topControls, "", 14, Color.white, Color.white, 32);
            UIHelper.SetLayout(profBtn.gameObject, preferredWidth: 64, minWidth: 64, preferredHeight: 64, minHeight: 64);
            var profBorder = profBtn.gameObject.AddComponent<Outline>();
            profBorder.effectColor = Hex("#E2E8F0");
            profBorder.effectDistance = new Vector2(1.2f, -1.2f);

            var profIconGO = UIHelper.MakeRect("ProfIcon", profBtn.transform);
            AnchorCenter(profIconGO, 34, 34);
            var profImg = profIconGO.gameObject.AddComponent<Image>();
            profImg.sprite = UIHelper.GetProfileSprite();
            profImg.color  = Hex("#0E2A47");
        }

        // =================================================================
        // 2. WORKER PROFILE CARD
        // =================================================================
        private static void BuildWorkerProfileCard(Transform parent)
        {
            var wrap = MakeCardWrap(parent, "WorkerCard", 250, 32, 32, -30, 18);

            var card = new GameObject("WorkerProfileCard");
            card.transform.SetParent(wrap, false);

            var cardImg = card.AddComponent<Image>();
            cardImg.color  = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 24);

            var shadow = card.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.05f);
            shadow.effectDistance = new Vector2(0, -4f);

            var border = card.AddComponent<Outline>();
            border.effectColor = Hex("#E2E8F0");
            border.effectDistance = new Vector2(1.2f, -1.2f);

            var cardBtn = card.AddComponent<Button>();
            cardBtn.targetGraphic = cardImg;

            var hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.padding        = new RectOffset(32, 24, 24, 24);
            hlg.spacing        = 28;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth      = true;
            hlg.childControlHeight     = true;

            // Avatar frame (136x136 circular)
            var avatarWrap = UIHelper.MakeRect("AvatarWrap", card.transform);
            UIHelper.SetLayout(avatarWrap.gameObject, preferredWidth: 136, minWidth: 136, preferredHeight: 136, minHeight: 136);

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

            // Info column
            var info = UIHelper.MakeVertical("InfoCol", card.transform, 4, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(info.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 160, minHeight: 160);
            info.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var greeting = UIHelper.MakeLabel("label-greeting", info, "Welcome,", 26, Hex("#64748B"));
            UIHelper.SetLayout(greeting.gameObject, preferredHeight: 32, minHeight: 32);

            var wName = UIHelper.MakeLabel("label-worker-name", info, "Worker", 52, Hex("#0F172A"), TextAlignmentOptions.Left, bold: true);
            UIHelper.SetLayout(wName.gameObject, preferredHeight: 60, minHeight: 60);

            var idRow = UIHelper.MakeHorizontal("IdRow", info, 8, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(idRow.gameObject, preferredHeight: 32, minHeight: 32);
            idRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var idLbl = UIHelper.MakeLabel("label-worker-id", idRow, "ID: Unassigned", 24, Hex("#475569"));
            UIHelper.SetLayout(idLbl.gameObject, preferredHeight: 32, minHeight: 32);

            var dot = UIHelper.MakeLabel("Dot", idRow, "|", 24, Hex("#CBD5E1"));
            UIHelper.SetLayout(dot.gameObject, preferredWidth: 12, preferredHeight: 32, minHeight: 32);

            var pinGO = UIHelper.MakeRect("PinIcon", idRow);
            UIHelper.SetLayout(pinGO.gameObject, preferredWidth: 20, minWidth: 20, preferredHeight: 20, minHeight: 20);
            var pinImg = pinGO.gameObject.AddComponent<Image>();
            pinImg.sprite = UIHelper.GetPinSprite();
            pinImg.color  = Hex("#10B981");

            var mineLbl = UIHelper.MakeLabel("label-role", idRow, "Mine Facility", 24, Hex("#0F172A"), TextAlignmentOptions.Left, bold: false);
            UIHelper.SetLayout(mineLbl.gameObject, preferredHeight: 32, minHeight: 32);

            var tagGO = UIHelper.MakeRect("TagGO", info);
            UIHelper.SetLayout(tagGO.gameObject, preferredWidth: 145, minWidth: 145, preferredHeight: 36, minHeight: 36);
            var tagImg = tagGO.gameObject.AddComponent<Image>();
            tagImg.color = Hex("#DCFCE7");
            UIHelper.SetImageRoundedSprite(tagImg, 12);
            var tagLbl = UIHelper.MakeLabel("TagLbl", tagGO, "Mine Worker", 21, Hex("#059669"), TextAlignmentOptions.Center, bold: false);
            UIHelper.Stretch(tagLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Right arrow
            var arrow = UIHelper.MakeLabel("ArrowLbl", card.transform, ">", 44, Hex("#94A3B8"), TextAlignmentOptions.Right, bold: false);
            UIHelper.SetLayout(arrow.gameObject, preferredWidth: 28, minWidth: 28, preferredHeight: 56, minHeight: 56);
        }

        // =================================================================
        // 3. TRAINING MODULES SECTION
        // =================================================================
        private static void BuildTrainingModulesSection(Transform parent)
        {
            var wrap = MakeCardWrap(parent, "TrainingSection", 800, 32, 32, 8, 18);

            var section = new GameObject("TrainingModulesSection");
            section.transform.SetParent(wrap, false);

            var vlg = section.AddComponent<VerticalLayoutGroup>();
            vlg.padding            = new RectOffset(0, 0, 0, 0);
            vlg.spacing            = 16;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth      = true;
            vlg.childControlHeight     = true;

            // Section Header
            var headerRow = UIHelper.MakeHorizontal("SectionHeader", section.transform, 14, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(headerRow.gameObject, preferredHeight: 60, minHeight: 60);
            headerRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var bookIcon = UIHelper.MakeRect("BookIcon", headerRow);
            UIHelper.SetLayout(bookIcon.gameObject, preferredWidth: 44, minWidth: 44, preferredHeight: 44, minHeight: 44);
            var bkImg = bookIcon.gameObject.AddComponent<Image>();
            bkImg.color  = Hex("#059669");
            bkImg.sprite = UIHelper.GetBookSprite();

            var secTitle = UIHelper.MakeLabel("label-modules-section", headerRow, "Training Modules", 50, Hex("#0F172A"), TextAlignmentOptions.Left, bold: true);
            UIHelper.SetLayout(secTitle.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 60, minHeight: 60);

            var viewAll = UIHelper.MakeButton("btn-view-all", headerRow, "View All >", 28, UIColors.Transparent, Hex("#059669"), 0);
            UIHelper.SetLayout(viewAll.gameObject, preferredWidth: 170, minWidth: 150, preferredHeight: 50, minHeight: 50);
            var vaLbl = viewAll.GetComponentInChildren<TextMeshProUGUI>();
            if (vaLbl != null) { vaLbl.name = "label-view-all"; vaLbl.fontStyle = FontStyles.Normal; }

            // Vertical list of module cards
            BuildModuleListCard(section.transform, "card-fire",
                bgCard:        Hex("#FFF7ED"),
                borderColor:   Hex("#FED7AA"),
                iconFilename:  "icon_fire_ref1.png",
                englishTitle:  "Fire & Explosion\nResponse",
                hindiLabelName: "HindiTitle-fire",
                engLabelName:   "EngTitle-fire",
                subtitle:      "Learn to identify, respond and control fire hazards in mining environments.",
                ctaBg:         Hex("#EA580C"),
                ctaTextColor:  Color.white);

            BuildModuleListCard(section.transform, "card-gas",
                bgCard:        Hex("#F0F9FF"),
                borderColor:   Hex("#BAE6FD"),
                iconFilename:  "icon_gas_ref1.png",
                englishTitle:  "Gas Leak &\nConfined Space",
                hindiLabelName: "HindiTitle-gas",
                engLabelName:   "EngTitle-gas",
                subtitle:      "Stay safe in hazardous gas environments and confined spaces.",
                ctaBg:         Hex("#2563EB"),
                ctaTextColor:  Color.white);

            BuildModuleListCard(section.transform, "card-machinery",
                bgCard:        Hex("#F0FDF4"),
                borderColor:   Hex("#BBF7D0"),
                iconFilename:  "icon_gear_ref1.png",
                englishTitle:  "Machinery Safety",
                hindiLabelName: "HindiTitle-machinery",
                engLabelName:   "EngTitle-machinery",
                subtitle:      "Identify machinery, understand risks and follow safe procedures.",
                ctaBg:         Hex("#16A34A"),
                ctaTextColor:  Color.white);
        }

        private static void BuildModuleListCard(Transform parent, string btnName,
            Color bgCard, Color borderColor, string iconFilename,
            string englishTitle, string hindiLabelName, string engLabelName,
            string subtitle, Color ctaBg, Color ctaTextColor)
        {
            var card = new GameObject(btnName);
            card.transform.SetParent(parent, false);
            UIHelper.SetLayout(card.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 226, minHeight: 226);

            var cardImg = card.AddComponent<Image>();
            cardImg.color  = bgCard;
            UIHelper.SetImageRoundedSprite(cardImg, 24);

            var border = card.AddComponent<Outline>();
            border.effectColor = borderColor;
            border.effectDistance = new Vector2(1.2f, -1.2f);

            var btn = card.AddComponent<Button>();
            btn.targetGraphic = cardImg;

            var hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.padding        = new RectOffset(24, 24, 24, 24);
            hlg.spacing        = 24;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth      = true;
            hlg.childControlHeight     = true;
            hlg.childAlignment         = TextAnchor.MiddleCenter;

            // Left: Icon Container (Rounded square)
            var illuArea = UIHelper.MakeRect("IlluArea", card.transform);
            UIHelper.SetLayout(illuArea.gameObject, preferredWidth: 136, minWidth: 136, preferredHeight: 136, minHeight: 136);
            var illuImg = illuArea.gameObject.AddComponent<Image>();
            illuImg.color  = Color.white;
            UIHelper.SetImageRoundedSprite(illuImg, 22);

            var spr = UIHelper.LoadProjectSprite(iconFilename);
            if (spr != null)
            {
                var iconGO = UIHelper.MakeRect("ModuleIconImg", illuArea);
                UIHelper.Stretch(iconGO, 0, 0, 0, 0);
                var artImg = iconGO.gameObject.AddComponent<Image>();
                artImg.sprite = spr;
                artImg.preserveAspect = true;
                artImg.raycastTarget = false;
            }

            // Center: Content column
            var contentCol = UIHelper.MakeVertical("ContentCol", card.transform, 6, childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(contentCol.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 170, minHeight: 170);
            contentCol.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var titleLbl = UIHelper.MakeLabel(hindiLabelName, contentCol, englishTitle, 34, Hex("#0F172A"), TextAlignmentOptions.Left, bold: true);
            titleLbl.lineSpacing = -4f;
            UIHelper.SetLayout(titleLbl.gameObject, preferredHeight: 76, minHeight: 76);

            var subLbl = UIHelper.MakeLabel(engLabelName, contentCol, subtitle, 24, Hex("#475569"), TextAlignmentOptions.Left);
            subLbl.lineSpacing = -6f;
            subLbl.textWrappingMode = TextWrappingModes.Normal;
            UIHelper.SetLayout(subLbl.gameObject, preferredHeight: 74, minHeight: 74);

            // Right: CTA Pill Button with "Start →" (Text + Arrow Icon)
            var startBtn = UIHelper.MakeButton(btnName == "card-fire" ? "btn-badge" : "btn-start", card.transform, "", 26, ctaBg, ctaTextColor, 28);
            UIHelper.SetLayout(startBtn.gameObject, preferredWidth: 170, minWidth: 170, preferredHeight: 68, minHeight: 68);

            var sRow = UIHelper.MakeHorizontal("StartRow", startBtn.transform, 6, new RectOffset(16, 16, 0, 0), childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(sRow, 0, 0, 0, 0);
            sRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            string statusLblName = btnName == "card-fire" ? "label-fire-status" : (btnName == "card-gas" ? "label-gas-btn" : "label-machinery-btn");
            var sLbl = UIHelper.MakeLabel(statusLblName, sRow, "Start", 26, Color.white, TextAlignmentOptions.Center, bold: true);

            var arrowGO = UIHelper.MakeRect("ArrowIcon", sRow);
            UIHelper.SetLayout(arrowGO.gameObject, preferredWidth: 24, minWidth: 24, preferredHeight: 24, minHeight: 24);
            var arrowImg = arrowGO.gameObject.AddComponent<Image>();
            arrowImg.sprite = UIHelper.LoadProjectSprite("icon_arrow_right.png");
            arrowImg.color = Color.white;
            arrowImg.preserveAspect = true;
            arrowImg.raycastTarget = false;
        }

        // =================================================================
        // 4. MISSION BANNER
        // =================================================================
        private static void BuildMissionBanner(Transform parent)
        {
            var wrap = MakeCardWrap(parent, "MissionBanner", 290, 32, 32, 8, 20);

            var banner = new GameObject("MissionBanner");
            banner.transform.SetParent(wrap, false);

            var bannerImg = banner.AddComponent<Image>();
            bannerImg.color  = Color.white;
            UIHelper.SetImageRoundedSprite(bannerImg, 24);

            var mask = banner.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            var bgSpr = UIHelper.LoadProjectSprite("mission_banner_perfect.png")
                     ?? UIHelper.LoadProjectSprite("miners_team_banner.jpg");
            if (bgSpr != null)
            {
                var bgRT = UIHelper.MakeRect("BannerBg", banner.transform);
                UIHelper.Stretch(bgRT, 0, 0, 0, 0);
                var bgImage = bgRT.gameObject.AddComponent<Image>();
                bgImage.sprite = bgSpr;
                bgImage.preserveAspect = false;
                bgImage.raycastTarget = false;
            }
        }

        // =================================================================
        // 5. BOTTOM NAVIGATION
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

            MakeNavItem(navRT, "nav-home",         "Home",         UIHelper.GetHomeSprite(),  true);
            MakeNavItem(navRT, "nav-learn",        "Learn",        UIHelper.GetBookSprite(),  false);
            MakeNavItem(navRT, "nav-progress",     "My Progress",  UIHelper.GetChartSprite(), false);
            MakeNavItem(navRT, "nav-certificates", "Certificates", UIHelper.GetMedalSprite(), false);

            // Phone Home Indicator (Bottom horizontal pill bar)
            var homeInd = UIHelper.MakeRect("HomeBarIndicator", navRT);
            homeInd.anchorMin = new Vector2(0.5f, 0f);
            homeInd.anchorMax = new Vector2(0.5f, 0f);
            homeInd.pivot     = new Vector2(0.5f, 0f);
            homeInd.anchoredPosition = new Vector2(0, 8f);
            homeInd.sizeDelta = new Vector2(134f, 5f);
            var hiImg = homeInd.gameObject.AddComponent<Image>();
            hiImg.color = Hex("#CBD5E1");
            UIHelper.SetImageRoundedSprite(hiImg, 3);
            homeInd.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        }

        private static void MakeNavItem(Transform parent, string name, string label, Sprite iconSprite, bool active)
        {
            var activeColor = active ? Hex("#059669") : Hex("#94A3B8");
            var activeFontSize = 32f;

            var btn = UIHelper.MakeButton(name, parent, "", 14, UIColors.Transparent, Color.white, 0);

            var col = UIHelper.MakeVertical("Col", btn.transform, 6, new RectOffset(0, 0, 4, 4), childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(col, 0, 0, 0, 0);
            col.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

            var iconBox = UIHelper.MakeRect("IconBox", col);
            UIHelper.SetLayout(iconBox.gameObject, preferredWidth: 76, minWidth: 76, preferredHeight: 76, minHeight: 76);
            var iconImg = iconBox.gameObject.AddComponent<Image>();
            iconImg.sprite = iconSprite;
            iconImg.color  = activeColor;
            iconImg.preserveAspect = true;

            var lbl = UIHelper.MakeLabel($"label-{name}", col, label, activeFontSize, activeColor, TextAlignmentOptions.Center, bold: active);
            UIHelper.SetLayout(lbl.gameObject, preferredHeight: 36, minHeight: 36);

            // Bottom indicator line under Home
            var indRow = UIHelper.MakeRect("IndRow", col);
            UIHelper.SetLayout(indRow.gameObject, preferredWidth: 54, minWidth: 54, preferredHeight: 5, minHeight: 5);
            if (active)
            {
                var indImg = indRow.gameObject.AddComponent<Image>();
                indImg.color  = Hex("#059669");
                indImg.sprite = UIHelper.GetWhiteSprite();
                UIHelper.SetImageRoundedSprite(indImg, 2);
            }
        }

        // =================================================================
        // 6. LANGUAGE DROPDOWN OVERLAY
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
            panel.anchoredPosition = new Vector2(-190f, -85f);
            panel.sizeDelta = new Vector2(260f, 230f);

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
            vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.spacing = 6;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            MakeLangOption(panel, "btn-lang-en", "English", true);
            MakeLangOption(panel, "btn-lang-hi", "हिंदी", false);
            MakeLangOption(panel, "btn-lang-sa", "Santali", false);

            overlay.gameObject.SetActive(false);
        }

        private static void MakeLangOption(Transform parent, string btnName, string label, bool isSelected)
        {
            var btn = UIHelper.MakeButton(btnName, parent, "", 14, isSelected ? Hex("#F0FDF4") : Color.white, Color.white, 10);
            UIHelper.SetLayout(btn.gameObject, preferredHeight: 60, minHeight: 60);

            var hlg = btn.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(14, 14, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childControlWidth = true;

            var checkGO = UIHelper.MakeRect("CheckIcon", btn.transform);
            UIHelper.SetLayout(checkGO.gameObject, preferredWidth: 26, minWidth: 26, preferredHeight: 26, minHeight: 26);
            if (isSelected)
            {
                var checkImg = checkGO.gameObject.AddComponent<Image>();
                checkImg.sprite = UIHelper.GetCheckmarkSprite();
                checkImg.color = Hex("#059669");
            }

            var spacer = UIHelper.MakeRect("Spacer", btn.transform);
            UIHelper.SetLayout(spacer.gameObject, preferredWidth: 10, minWidth: 10);

            var txtColor = isSelected ? Hex("#059669") : Hex("#0F172A");
            var lbl = UIHelper.MakeLabel($"label-{btnName}", btn.transform, label, 24, txtColor, TextAlignmentOptions.Left, bold: isSelected);
            UIHelper.SetLayout(lbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 34, minHeight: 34);
        }
    }
}
