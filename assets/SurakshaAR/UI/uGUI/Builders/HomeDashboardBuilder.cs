using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Home Dashboard builder — pixel-perfect match to Figma reference (Image 2):
    /// 1080 × 2400 reference scaling:
    /// - Deep Navy/Blue top header with Govt of Jharkhand seal, English selector, bell badge, profile avatar,
    ///   SurakshaAR branding, and vector worker backside overlooking the mine.
    /// - Clean white Worker Profile Card with circular avatar, Namaste greeting, Ramesh Kumar, Jharia Mine.
    /// - Training Progress Card with horizontal progress bar, 60% badge, 3 stat indicators, and big "▶ Continue Training" button.
    /// - Training Modules section with 3 side-by-side cards (Fire, Gas, Machinery) with top illustration, Hindi/English titles, and action buttons.
    /// - Bottom Mission Banner with mining haul truck, "हर श्रमिक सुरक्षित, हर परिवार मजबूत", and "सुरक्षित झारखंड" badge.
    /// - Fixed Bottom Navigation bar with 4 clean tabs.
    /// </summary>
    public static class HomeDashboardBuilder
    {
        public static GameObject Build()
        {
            var root = new GameObject("HomeDashboardScreen");
            root.AddComponent<RectTransform>();
            var rootImg = root.AddComponent<Image>();
            rootImg.color = UIColors.Hex("#F3F6F9");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // ── Fixed Bottom Navigation Bar (160px) ───────────────────────
            MakeBottomNav(root.transform);

            // ── Scrollable Body ──────────────────────────────────────────
            var scrollRoot = UIHelper.MakeRect("ScrollArea", root.transform);
            scrollRoot.anchorMin = Vector2.zero;
            scrollRoot.anchorMax = Vector2.one;
            scrollRoot.offsetMin = new Vector2(0, 160); // space for bottom nav
            scrollRoot.offsetMax = Vector2.zero;

            var scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 30f;

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
            content.sizeDelta = new Vector2(0, 2400);
            scrollRect.content = content;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.padding = new RectOffset(32, 32, 0, 48);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── 1. Top Blue Header Banner ────────────────────────────────
            BuildHeaderBanner(content);

            // ── 2. Worker Profile Card ───────────────────────────────────
            BuildWorkerProfileCard(content);

            // ── 3. Training Progress Card ────────────────────────────────
            BuildProgressCard(content);

            // ── 4. Training Modules (3 Horizontal Cards) ─────────────────
            BuildTrainingModulesSection(content);

            // ── 5. Bottom Mission Banner ─────────────────────────────────
            BuildMissionBanner(content);

            return root;
        }

        // =====================================================================
        //  1. TOP BLUE HEADER BANNER
        // =====================================================================
        private static void BuildHeaderBanner(Transform parent)
        {
            var headerRT = UIHelper.MakeRect("HeaderBanner", parent);
            UIHelper.SetLayout(headerRT.gameObject, preferredHeight: 400, minHeight: 400);

            // Base deep navy/blue background
            var headerImg = headerRT.gameObject.AddComponent<Image>();
            headerImg.color = UIColors.PrimaryDark; // Deep professional navy blue #0D2F4A

            // Worker backside overlooking mine banner
            var bgSpr = UIHelper.LoadProjectSprite("worker_backside_banner");
            if (bgSpr != null)
            {
                var bgRT = UIHelper.MakeStretchRect("BgArt", headerRT);
                var bgImg = bgRT.gameObject.AddComponent<Image>();
                bgImg.sprite = bgSpr;
                bgImg.color = new Color(1f, 1f, 1f, 0.45f);
            }

            // Dark blue gradient overlay for text legibility
            var gradientOverlay = UIHelper.MakeStretchRect("GradientOverlay", headerRT);
            var gradImg = gradientOverlay.gameObject.AddComponent<Image>();
            gradImg.color = new Color(0.05f, 0.15f, 0.28f, 0.80f);

            var innerCol = UIHelper.MakeVertical("InnerCol", headerRT, 12, new RectOffset(32, 32, 36, 20),
                childForceWidth: true, childForceHeight: false, controlChildHeight: true, controlChildWidth: true);
            UIHelper.Stretch(innerCol, 0, 0, 0, 0);

            // Top Row: Govt Seal (Left) + Lang & Notifications (Right)
            var topBar = UIHelper.MakeHorizontal("TopBar", innerCol, 12, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(topBar.gameObject, preferredHeight: 52, minHeight: 52);

            // Left: Govt of Jharkhand seal + Department of Mines
            var govtBox = UIHelper.MakeHorizontal("GovtBox", topBar, 10, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(govtBox.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 50, minHeight: 50);

            var emblemCircle = UIHelper.MakeRect("EmblemCircle", govtBox);
            UIHelper.SetLayout(emblemCircle.gameObject, preferredWidth: 44, preferredHeight: 44, minWidth: 44, minHeight: 44);
            var embImg = emblemCircle.gameObject.AddComponent<Image>();
            embImg.sprite = UIHelper.GetCircleSprite();
            embImg.color = new Color(1f, 1f, 1f, 0.20f);
            var embIcon = UIHelper.MakeLabel("Icon", emblemCircle, "JH", 18, Color.white, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(embIcon.GetComponent<RectTransform>(), 0, 0, 0, 0);

            var govtTextCol = UIHelper.MakeVertical("GovtText", govtBox, 2, childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(govtTextCol.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 44, minHeight: 44);
            var gTitle = UIHelper.MakeLabel("GovtTitle", govtTextCol, "Government of Jharkhand", 15, Color.white, bold: true, wrap: false);
            UIHelper.SetLayout(gTitle.gameObject, preferredHeight: 20, minHeight: 20);
            var gSub = UIHelper.MakeLabel("GovtSub", govtTextCol, "Department of Mines • झारखण्ड सरकार", 12, Hex("#93C5FD"), wrap: false);
            UIHelper.SetLayout(gSub.gameObject, preferredHeight: 18, minHeight: 18);

            // Right: Actions (Language, Bell, Profile)
            var rightActions = UIHelper.MakeHorizontal("RightActions", topBar, 8, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(rightActions.gameObject, preferredWidth: 280, preferredHeight: 48, minWidth: 280, minHeight: 48);

            // Language pill button: "English ⌵"
            var langPill = UIHelper.MakeButton("btn-language-picker", rightActions, "English ⌵", 14,
                new Color(1f, 1f, 1f, 0.18f), Color.white, 16);
            UIHelper.SetLayout(langPill.gameObject, preferredWidth: 120, preferredHeight: 42, minWidth: 120, minHeight: 42);
            var langBorder = langPill.gameObject.AddComponent<Outline>();
            langBorder.effectColor = new Color(1f, 1f, 1f, 0.30f);
            langBorder.effectDistance = new Vector2(1, -1);

            // Notification Bell with badge "3"
            var bellBtn = UIHelper.MakeButton("btn-notifications", rightActions, "3", 15,
                UIColors.Danger, Color.white, 21);
            UIHelper.SetLayout(bellBtn.gameObject, preferredWidth: 42, preferredHeight: 42, minWidth: 42, minHeight: 42);

            // Profile circle
            var profBtn = UIHelper.MakeButton("btn-profile", rightActions, "R", 18,
                UIColors.Hex("#1E40AF"), Color.white, 21);
            UIHelper.SetLayout(profBtn.gameObject, preferredWidth: 42, preferredHeight: 42, minWidth: 42, minHeight: 42);

            // Hero Brand Title Row
            var heroRow = UIHelper.MakeHorizontal("HeroRow", innerCol, 12, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(heroRow.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 120, minHeight: 120);

            var heroLeft = UIHelper.MakeVertical("HeroLeft", heroRow, 4, childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(heroLeft.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 110, minHeight: 110);

            var titleLbl = UIHelper.MakeLabel("label-app-title", heroLeft, "Suraksha<color=#FBBF24>AR</color>", 42, Color.white, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, preferredHeight: 50, minHeight: 50);

            var taglineLbl = UIHelper.MakeLabel("label-tagline", heroLeft, "Learn Safe  |  Work Safe  |  Build a Safer Jharkhand", 15, Hex("#CBD5E1"));
            UIHelper.SetLayout(taglineLbl.gameObject, preferredHeight: 22, minHeight: 22);

            // Right Hero Motto Box: "सुरक्षित श्रमिक | समृद्ध झारखंड"
            var mottoCol = UIHelper.MakeVertical("MottoCol", heroRow, 4, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(mottoCol.gameObject, preferredWidth: 260, preferredHeight: 100, minWidth: 260, minHeight: 100);

            var mottoLbl = UIHelper.MakeLabel("MottoLbl", mottoCol, "सुरक्षित श्रमिक\nसमृद्ध झारखंड", 18, Color.white, TextAlignmentOptions.Right, bold: true);
            UIHelper.SetLayout(mottoLbl.gameObject, preferredWidth: 260, preferredHeight: 46, minHeight: 46);

            // Fixed Tricolor underline bar (strictly 5px height!)
            var tricolorBar = UIHelper.MakeRect("Tricolor", mottoCol);
            UIHelper.SetLayout(tricolorBar.gameObject, preferredWidth: 160, preferredHeight: 5, minWidth: 160, minHeight: 5);
            var triHlg = tricolorBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            triHlg.spacing = 0;
            triHlg.childForceExpandWidth = true;
            triHlg.childForceExpandHeight = true;
            triHlg.childControlWidth = true;
            triHlg.childControlHeight = true;

            MakeColorStrip(tricolorBar, Hex("#FF9933")); // Saffron
            MakeColorStrip(tricolorBar, Color.white);    // White
            MakeColorStrip(tricolorBar, Hex("#128807")); // Green
        }

        private static void MakeColorStrip(Transform parent, Color color)
        {
            var strip = UIHelper.MakeRect("Strip", parent);
            var img = strip.gameObject.AddComponent<Image>();
            img.color = color;
            img.sprite = UIHelper.GetWhiteSprite();
        }

        // =====================================================================
        //  2. WORKER PROFILE CARD
        // =====================================================================
        private static void BuildWorkerProfileCard(Transform parent)
        {
            var card = new GameObject("WorkerProfileCard");
            card.transform.SetParent(parent, false);
            UIHelper.SetLayout(card, preferredHeight: 110, minHeight: 110);

            var cardImg = card.AddComponent<Image>();
            cardImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 18);

            var border = card.AddComponent<Outline>();
            border.effectColor = UIColors.Hex("#E2E8F0");
            border.effectDistance = new Vector2(1, -1);

            var hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(18, 18, 14, 14);
            hlg.spacing = 14;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Worker hardhat avatar circle (strictly no human face)
            var avatarBox = UIHelper.MakeRect("AvatarBox", card.transform);
            UIHelper.SetLayout(avatarBox.gameObject, preferredWidth: 64, preferredHeight: 64, minWidth: 64, minHeight: 64);
            var avImg = avatarBox.gameObject.AddComponent<Image>();
            avImg.sprite = UIHelper.GetCircleSprite();
            avImg.color = Hex("#E0F2FE"); // Clean soft blue
            var avIcon = UIHelper.MakeLabel("AvIcon", avatarBox, "R", 30, UIColors.PrimaryDark, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(avIcon.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Middle info
            var infoCol = UIHelper.MakeVertical("InfoCol", card.transform, 2, childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(infoCol.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 70, minHeight: 70);

            var greetLbl = UIHelper.MakeLabel("label-greeting", infoCol, "नमस्ते,", 15, Hex("#64748B"));
            UIHelper.SetLayout(greetLbl.gameObject, preferredHeight: 18, minHeight: 18);

            var nameLbl = UIHelper.MakeLabel("label-worker-name", infoCol, "Ramesh Kumar", 22, UIColors.TextPrimary, bold: true);
            UIHelper.SetLayout(nameLbl.gameObject, preferredHeight: 26, minHeight: 26);

            var roleLbl = UIHelper.MakeLabel("label-role", infoCol, "ID: JH-MN-004821 • Jharia Mine", 13, Hex("#64748B"));
            UIHelper.SetLayout(roleLbl.gameObject, preferredHeight: 18, minHeight: 18);

            // Right Chevron
            var arrow = UIHelper.MakeLabel("Arrow", card.transform, "›", 32, Hex("#94A3B8"), TextAlignmentOptions.Center);
            UIHelper.SetLayout(arrow.gameObject, preferredWidth: 28, preferredHeight: 44, minWidth: 28, minHeight: 44);
        }

        // =====================================================================
        //  3. TRAINING PROGRESS CARD ("आपकी प्रशिक्षण प्रगति")
        // =====================================================================
        private static void BuildProgressCard(Transform parent)
        {
            var card = new GameObject("TrainingProgressCard");
            card.transform.SetParent(parent, false);
            UIHelper.SetLayout(card, preferredHeight: 220, minHeight: 220);

            var cardImg = card.AddComponent<Image>();
            cardImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 18);

            var border = card.AddComponent<Outline>();
            border.effectColor = UIColors.Hex("#E2E8F0");
            border.effectDistance = new Vector2(1, -1);

            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(20, 20, 16, 16);
            vlg.spacing = 14;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            // Section Title
            var titleLbl = UIHelper.MakeLabel("label-hero-title", card.transform, "आपकी प्रशिक्षण प्रगति (Progress)", 19, UIColors.TextPrimary, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, preferredHeight: 24, minHeight: 24);

            // Progress Bar Track + Pct
            var barRow = UIHelper.MakeHorizontal("BarRow", card.transform, 14, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(barRow.gameObject, preferredHeight: 28, minHeight: 28);

            var track = UIHelper.MakeRect("Track", barRow);
            UIHelper.SetLayout(track.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 14, minHeight: 14);
            var trImg = track.gameObject.AddComponent<Image>();
            trImg.color = Hex("#E2E8F0");
            UIHelper.SetImageRoundedSprite(trImg, 7);

            var fill = UIHelper.MakeRect("Fill", track);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0.60f, 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            var fImg = fill.gameObject.AddComponent<Image>();
            fImg.color = Hex("#10B981"); // Vibrant progress green
            UIHelper.SetImageRoundedSprite(fImg, 7);

            var pctLbl = UIHelper.MakeLabel("label-overall-pct", barRow, "60%", 24, UIColors.TextPrimary, TextAlignmentOptions.Right, bold: true);
            UIHelper.SetLayout(pctLbl.gameObject, preferredWidth: 70, minWidth: 70, preferredHeight: 28);

            // Bottom Row: 3 Stats on left, Big Action button on right
            var bottomRow = UIHelper.MakeHorizontal("BottomRow", card.transform, 12, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(bottomRow.gameObject, preferredHeight: 74, minHeight: 74);

            // 3 Stat items (matching Image 2)
            var statsGroup = UIHelper.MakeHorizontal("StatsGroup", bottomRow, 8, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(statsGroup.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 70, minHeight: 70);

            BuildStatBadge(statsGroup, Hex("#16A34A"), Hex("#DCFCE7"), "1", "पूर्ण\nDone");
            BuildStatBadge(statsGroup, Hex("#EA580C"), Hex("#FFEDD5"), "1", "प्रगति\nActive");
            BuildStatBadge(statsGroup, Hex("#64748B"), Hex("#F1F5F9"), "3", "शेष\nLeft");

            // Continue Training CTA Button (DEEP BLUE strictly per user request: "blue only not green")
            var continueBtn = UIHelper.MakeButton("btn-continue-training", bottomRow, "", 16,
                UIColors.PrimaryDark, Color.white, 14);
            UIHelper.SetLayout(continueBtn.gameObject, preferredWidth: 240, preferredHeight: 68, minWidth: 240, minHeight: 68);

            var btnInner = UIHelper.MakeVertical("BtnInner", continueBtn.transform, 2, new RectOffset(12, 12, 10, 10),
                childForceWidth: true, childForceHeight: false);
            UIHelper.Stretch(btnInner, 0, 0, 0, 0);

            var bText1 = UIHelper.MakeLabel("BText1", btnInner, "▶  जारी रखें", 17, Color.white, TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(bText1.gameObject, preferredHeight: 22, minHeight: 22);

            var bText2 = UIHelper.MakeLabel("BText2", btnInner, "Continue Training", 11, Hex("#93C5FD"), TextAlignmentOptions.Center);
            UIHelper.SetLayout(bText2.gameObject, preferredHeight: 14, minHeight: 14);
        }

        private static void BuildStatBadge(Transform parent, Color badgeColor, Color badgeBg, string count, string label)
        {
            var col = UIHelper.MakeHorizontal("StatItem", parent, 6, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(col.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 64, minHeight: 64);

            var circle = UIHelper.MakeRect("Circle", col);
            UIHelper.SetLayout(circle.gameObject, preferredWidth: 36, preferredHeight: 36, minWidth: 36, minHeight: 36);
            var cImg = circle.gameObject.AddComponent<Image>();
            cImg.sprite = UIHelper.GetCircleSprite();
            cImg.color = badgeBg;
            var cLbl = UIHelper.MakeLabel("Count", circle, count, 18, badgeColor, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(cLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            var textCol = UIHelper.MakeVertical("TextCol", col, 2, childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(textCol.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 60, minHeight: 60);

            var subLbl = UIHelper.MakeLabel("Label", textCol, label, 11, Hex("#64748B"));
            UIHelper.SetLayout(subLbl.gameObject, preferredHeight: 34, minHeight: 34);
        }

        // =====================================================================
        //  4. TRAINING MODULES SECTION (3 Side-by-Side Cards)
        // =====================================================================
        private static void BuildTrainingModulesSection(Transform parent)
        {
            var sectionBox = UIHelper.MakeVertical("ModulesSection", parent, 12, childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(sectionBox.gameObject, preferredHeight: 460, minHeight: 460);

            // Header: Title + "View all →"
            var headRow = UIHelper.MakeHorizontal("HeadRow", sectionBox, 14, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(headRow.gameObject, preferredHeight: 36, minHeight: 36);

            var secTitle = UIHelper.MakeLabel("label-modules-section", headRow, "प्रशिक्षण मॉड्यूल (Modules)", 20, UIColors.TextPrimary, bold: true);
            UIHelper.SetLayout(secTitle.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 32, minHeight: 32);

            var viewAllBtn = UIHelper.MakeButton("btn-view-all", headRow, "सभी देखें ›", 15, UIColors.Transparent, Hex("#1E40AF"), 8);
            UIHelper.SetLayout(viewAllBtn.gameObject, preferredWidth: 120, preferredHeight: 34, minWidth: 120, minHeight: 34);

            // Horizontal Row of 3 Cards
            var cardsRow = UIHelper.MakeHorizontal("CardsRow", sectionBox, 12, childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(cardsRow.gameObject, preferredHeight: 390, minHeight: 390);

            // Card 1: Fire & Explosion
            BuildModuleCardItem(
                parent: cardsRow,
                btnName: "card-fire",
                badgeText: "FIRE SAFETY\nआग बचाव",
                headerBgColor: Hex("#FEE2E2"),
                badgeTextColor: Hex("#DC2626"),
                hindiTitle: "Fire & Explosion\nआग से बचाव",
                englishSubtitle: "45 min • Interactive AR",
                progressPct: 0.75f,
                progressText: "75%",
                ctaText: "प्रगति में ›",
                ctaColor: Hex("#EA580C"),
                ctaTextColor: Color.white
            );

            // Card 2: Gas Leak & Confined Space
            BuildModuleCardItem(
                parent: cardsRow,
                btnName: "card-gas",
                badgeText: "GAS LEAK\nगैस रिसाव",
                headerBgColor: Hex("#E0F2FE"),
                badgeTextColor: Hex("#0284C7"),
                hindiTitle: "Gas & Confined\nसीमित स्थान",
                englishSubtitle: "60 min • Advanced SOP",
                progressPct: 0.0f,
                progressText: "0%",
                ctaText: "शुरू करें ›",
                ctaColor: Hex("#E0F2FE"),
                ctaTextColor: Hex("#0369A1")
            );

            // Card 3: Machinery Safety
            BuildModuleCardItem(
                parent: cardsRow,
                btnName: "card-machinery",
                badgeText: "MACHINERY\nमशीन सुरक्षा",
                headerBgColor: Hex("#DCFCE7"),
                badgeTextColor: Hex("#16A34A"),
                hindiTitle: "Machinery Safety\nमशीनों की सुरक्षा",
                englishSubtitle: "60 min • Equipment SOP",
                progressPct: 0.0f,
                progressText: "0%",
                ctaText: "शुरू करें ›",
                ctaColor: Hex("#E0F2FE"),
                ctaTextColor: Hex("#0369A1")
            );
        }

        private static void BuildModuleCardItem(
            Transform parent,
            string btnName,
            string badgeText,
            Color headerBgColor,
            Color badgeTextColor,
            string hindiTitle,
            string englishSubtitle,
            float progressPct,
            string progressText,
            string ctaText,
            Color ctaColor,
            Color ctaTextColor)
        {
            var card = new GameObject(btnName);
            card.transform.SetParent(parent, false);
            UIHelper.SetLayout(card, flexibleWidth: true, flexWidth: 1, preferredHeight: 380, minHeight: 380);

            var cardImg = card.AddComponent<Image>();
            cardImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(cardImg, 16);

            var border = card.AddComponent<Outline>();
            border.effectColor = UIColors.Hex("#E2E8F0");
            border.effectDistance = new Vector2(1, -1);

            var col = UIHelper.MakeVertical("Col", card.transform, 6, new RectOffset(10, 10, 10, 12),
                childForceWidth: true, childForceHeight: false);
            UIHelper.Stretch(col, 0, 0, 0, 0);

            // Top Illustration Box
            var artBox = UIHelper.MakeRect("ArtBox", col);
            UIHelper.SetLayout(artBox.gameObject, preferredHeight: 110, minHeight: 110);
            var aImg = artBox.gameObject.AddComponent<Image>();
            aImg.color = headerBgColor;
            UIHelper.SetImageRoundedSprite(aImg, 12);

            var artLbl = UIHelper.MakeLabel("ArtLbl", artBox, badgeText, 14, badgeTextColor, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(artLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Hindi Title
            var hLbl = UIHelper.MakeLabel("HindiTitle", col, hindiTitle, 14, UIColors.TextPrimary, bold: true);
            UIHelper.SetLayout(hLbl.gameObject, preferredHeight: 40, minHeight: 40);

            // English Subtitle
            var eLbl = UIHelper.MakeLabel("EngTitle", col, englishSubtitle, 11, Hex("#64748B"));
            UIHelper.SetLayout(eLbl.gameObject, preferredHeight: 28, minHeight: 28);

            // Progress bar
            var barRow = UIHelper.MakeHorizontal("BarRow", col, 4, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(barRow.gameObject, preferredHeight: 14, minHeight: 14);

            var bar = UIHelper.MakeRect("Bar", barRow);
            UIHelper.SetLayout(bar.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 6, minHeight: 6);
            var bImg = bar.gameObject.AddComponent<Image>();
            bImg.color = Hex("#E2E8F0");
            UIHelper.SetImageRoundedSprite(bImg, 3);

            if (progressPct > 0f)
            {
                var fill = UIHelper.MakeRect("Fill", bar);
                fill.anchorMin = Vector2.zero;
                fill.anchorMax = new Vector2(progressPct, 1f);
                fill.offsetMin = Vector2.zero;
                fill.offsetMax = Vector2.zero;
                var fImg = fill.gameObject.AddComponent<Image>();
                fImg.color = Hex("#10B981");
                UIHelper.SetImageRoundedSprite(fImg, 3);
            }

            var pText = UIHelper.MakeLabel("PText", barRow, progressText, 11, Hex("#64748B"), TextAlignmentOptions.Right);
            UIHelper.SetLayout(pText.gameObject, preferredWidth: 34, minWidth: 34, preferredHeight: 14);

            // Bottom CTA button
            var ctaBtn = UIHelper.MakeButton("btn-cta", col, ctaText, 14, ctaColor, ctaTextColor, 10);
            UIHelper.SetLayout(ctaBtn.gameObject, preferredHeight: 42, minHeight: 42);

            // Make whole card clickable
            var cardBtn = card.AddComponent<Button>();
            cardBtn.targetGraphic = cardImg;
        }

        // =====================================================================
        //  5. BOTTOM MISSION BANNER ("हर श्रमिक सुरक्षित, हर परिवार मजबूत")
        // =====================================================================
        private static void BuildMissionBanner(Transform parent)
        {
            var banner = new GameObject("MissionBannerCard");
            banner.transform.SetParent(parent, false);
            UIHelper.SetLayout(banner, preferredHeight: 170, minHeight: 170);

            var bImg = banner.AddComponent<Image>();
            bImg.color = Hex("#F8FAFC");
            UIHelper.SetImageRoundedSprite(bImg, 18);

            var border = banner.AddComponent<Outline>();
            border.effectColor = UIColors.Hex("#E2E8F0");
            border.effectDistance = new Vector2(1, -1);

            var hlg = banner.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(14, 16, 12, 12);
            hlg.spacing = 14;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Mine truck artwork on left (strictly no human faces)
            var artBox = UIHelper.MakeRect("TruckArtBox", banner.transform);
            UIHelper.SetLayout(artBox.gameObject, preferredWidth: 220, preferredHeight: 140, minWidth: 220, minHeight: 140);
            var truckImg = artBox.gameObject.AddComponent<Image>();
            truckImg.color = Color.white;
            var truckSpr = UIHelper.LoadProjectSprite("mine_truck_banner");
            if (truckSpr != null)
            {
                truckImg.sprite = truckSpr;
                truckImg.preserveAspect = true;
            }
            UIHelper.SetImageRoundedSprite(truckImg, 14);

            // Banner text content on right side
            var textCol = UIHelper.MakeVertical("TextCol", banner.transform, 3, childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(textCol.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 140, minHeight: 140);

            var t1 = UIHelper.MakeLabel("T1", textCol, "हर श्रमिक सुरक्षित,\nहर परिवार मजबूत", 18, UIColors.TextPrimary, bold: true);
            UIHelper.SetLayout(t1.gameObject, preferredHeight: 46, minHeight: 46);

            var t2 = UIHelper.MakeLabel("T2", textCol, "Every Worker Safe, Every Family Strong", 12, Hex("#64748B"));
            UIHelper.SetLayout(t2.gameObject, preferredHeight: 18, minHeight: 18);

            var badge = UIHelper.MakeRect("Badge", textCol);
            UIHelper.SetLayout(badge.gameObject, preferredWidth: 180, preferredHeight: 26, minWidth: 180, minHeight: 26);
            var badgeImg = badge.gameObject.AddComponent<Image>();
            badgeImg.color = Hex("#FEF3C7");
            UIHelper.SetImageRoundedSprite(badgeImg, 6);

            var badgeLbl = UIHelper.MakeLabel("BLbl", badge, "सुरक्षित झारखंड • Safe Jharkhand", 11, Hex("#92400E"), TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(badgeLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);
        }

        // =====================================================================
        //  6. FIXED BOTTOM NAVIGATION BAR (DEEP BLUE THEME)
        // =====================================================================
        private static void MakeBottomNav(Transform parent)
        {
            var navRT = UIHelper.MakeRect("BottomNavBar", parent);
            navRT.anchorMin = Vector2.zero;
            navRT.anchorMax = new Vector2(1, 0);
            navRT.pivot = new Vector2(0.5f, 0);
            navRT.sizeDelta = new Vector2(0, 140);

            var navImg = navRT.gameObject.AddComponent<Image>();
            navImg.color = Color.white;
            navImg.sprite = UIHelper.GetWhiteSprite();

            var topBorder = navRT.gameObject.AddComponent<Outline>();
            topBorder.effectColor = UIColors.Hex("#E2E8F0");
            topBorder.effectDistance = new Vector2(0, 1);

            var hlg = navRT.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.padding = new RectOffset(16, 16, 6, 14);

            // Tab 1: Home (Active - Deep Blue)
            MakeNavItem(navRT, "nav-home", "HOME", "होम", true);

            // Tab 2: Learn
            MakeNavItem(navRT, "nav-learn", "LEARN", "सीखें", false);

            // Tab 3: Progress
            MakeNavItem(navRT, "nav-progress", "PROGRESS", "प्रगति", false);

            // Tab 4: Certificates
            MakeNavItem(navRT, "nav-certificates", "CERTIFICATES", "प्रमाणपत्र", false);
        }

        private static void MakeNavItem(Transform parent, string btnName, string iconText, string label, bool isActive)
        {
            var btn = UIHelper.MakeButton(btnName, parent, "", 16, UIColors.Transparent, Color.white, 0);

            var col = UIHelper.MakeVertical("Col", btn.transform, 2, new RectOffset(4, 4, 4, 6),
                childForceWidth: true, childForceHeight: false);
            UIHelper.Stretch(col, 0, 0, 0, 0);

            var activeColor = isActive ? UIColors.PrimaryDark : Hex("#64748B");

            // Top active indicator bar (DEEP BLUE strictly per user request: "blue only not green")
            if (isActive)
            {
                var indicator = UIHelper.MakeRect("ActiveIndicator", col);
                UIHelper.SetLayout(indicator.gameObject, preferredWidth: 48, preferredHeight: 4, minWidth: 48, minHeight: 4);
                var indImg = indicator.gameObject.AddComponent<Image>();
                indImg.color = UIColors.PrimaryDark; // Deep Navy Blue
                UIHelper.SetImageRoundedSprite(indImg, 2);
            }
            else
            {
                var spacer = UIHelper.MakeRect("Spacer", col);
                UIHelper.SetLayout(spacer.gameObject, preferredHeight: 4, minHeight: 4);
            }

            var textLbl1 = UIHelper.MakeLabel("IconText", col, iconText, 13, activeColor, TextAlignmentOptions.Center, bold: isActive);
            UIHelper.SetLayout(textLbl1.gameObject, preferredHeight: 20, minHeight: 20);

            var textLbl2 = UIHelper.MakeLabel($"label-{btnName}", col, label, 12, activeColor, TextAlignmentOptions.Center, bold: isActive);
            UIHelper.SetLayout(textLbl2.gameObject, preferredHeight: 18, minHeight: 18);
        }

        private static Color Hex(string hex) => UIColors.Hex(hex);
    }
}
