using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Language Selection screen — pixel-perfect recreation of design reference:
    /// 1080 × 2400 reference scaling:
    /// - Clean pure white background with status bar safe area.
    /// - Left-aligned header:
    ///     - "Select Your Language" (50px bold #0F172A).
    ///     - "Choose your preferred language" (27px #64748B).
    /// - Three stacked language cards (190px high, 26px rounded corners, 28px spacing):
    ///     1. English (Blue badge with white "Aअ", active blue border and blue chevron) — Selected.
    ///     2. Hindi (Warm orange badge with white "अ", subtle gray border, gray chevron).
    ///     3. Santali (Emerald green badge with white floral/leaf icon, subtle gray border, gray chevron).
    /// - Bottom Continue button (Solid Dark Navy #0A192F, 118px high, rounded 28px, white bold text).
    /// - Fully responsive across all portrait screen sizes with safe-area and scroll-safety.
    /// </summary>
    public static class LanguageSelectionBuilder
    {
        public static GameObject Build()
        {
            var root = new GameObject("LanguageSelectionScreen");
            var rootRT = root.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            // Pure clean light background
            var rootImg = root.AddComponent<Image>();
            rootImg.color = Color.white;
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // ── 1. Header (Left-aligned, spacious top padding for notch/status bar) ──
            var headerRT = UIHelper.MakeRect("HeaderArea", root.transform);
            headerRT.anchorMin = new Vector2(0f, 1f);
            headerRT.anchorMax = new Vector2(1f, 1f);
            headerRT.pivot = new Vector2(0f, 1f);
            headerRT.sizeDelta = new Vector2(0f, 260f);
            headerRT.anchoredPosition = Vector2.zero;

            var headerVLG = headerRT.gameObject.AddComponent<VerticalLayoutGroup>();
            headerVLG.padding = new RectOffset(60, 60, 130, 10); // 130px top breathing space for status bar
            headerVLG.spacing = 10f;
            headerVLG.childAlignment = TextAnchor.UpperLeft;
            headerVLG.childForceExpandWidth = true;
            headerVLG.childForceExpandHeight = false;
            headerVLG.childControlWidth = true;
            headerVLG.childControlHeight = true;

            // Title: "Select Your Language"
            var titleTMP = UIHelper.MakeLabel("label-title", headerRT, "Select Your Language", 50f, UIColors.Hex("#0F172A"), TextAlignmentOptions.Left, bold: true, wrap: false);
            var titleLE = titleTMP.gameObject.GetComponent<LayoutElement>() ?? titleTMP.gameObject.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 58f;
            titleLE.minHeight = 58f;

            // Subtitle: "Choose your preferred language"
            var subTMP = UIHelper.MakeLabel("label-subtitle", headerRT, "Choose your preferred language", 27f, UIColors.Hex("#64748B"), TextAlignmentOptions.Left, bold: false, wrap: false);
            var subLE = subTMP.gameObject.GetComponent<LayoutElement>() ?? subTMP.gameObject.AddComponent<LayoutElement>();
            subLE.preferredHeight = 34f;
            subLE.minHeight = 34f;

            // ── 2. Bottom Continue Button (Fixed to bottom safe area) ───────
            var bottomAreaRT = UIHelper.MakeRect("BottomArea", root.transform);
            bottomAreaRT.anchorMin = new Vector2(0f, 0f);
            bottomAreaRT.anchorMax = new Vector2(1f, 0f);
            bottomAreaRT.pivot = new Vector2(0.5f, 0f);
            bottomAreaRT.offsetMin = new Vector2(60f, 80f);
            bottomAreaRT.offsetMax = new Vector2(-60f, 198f); // 118px height

            var btn = UIHelper.MakeButton("btn-continue", bottomAreaRT,
                "Continue >", 34f, UIColors.Hex("#0A192F"), Color.white, 28f);
            var btnRT = btn.GetComponent<RectTransform>();
            UIHelper.Stretch(btnRT, 0, 0, 0, 0);

            // ── 3. Middle Cards Scroll Container (Between Header & Button) ──
            var scrollAreaRT = UIHelper.MakeRect("ScrollArea", root.transform);
            scrollAreaRT.anchorMin = new Vector2(0f, 0f);
            scrollAreaRT.anchorMax = new Vector2(1f, 1f);
            scrollAreaRT.offsetMin = new Vector2(0f, 218f);  // Above bottom button
            scrollAreaRT.offsetMax = new Vector2(0f, -270f); // Below header

            var scrollRect = scrollAreaRT.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 30f;

            var viewport = UIHelper.MakeRect("Viewport", scrollAreaRT);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewport.gameObject.AddComponent<RectMask2D>();
            scrollRect.viewport = viewport;

            var bodyRT = UIHelper.MakeRect("CardsBody", viewport);
            bodyRT.anchorMin = new Vector2(0f, 1f);
            bodyRT.anchorMax = new Vector2(1f, 1f);
            bodyRT.pivot = new Vector2(0.5f, 1f);
            bodyRT.offsetMin = Vector2.zero;
            bodyRT.offsetMax = Vector2.zero;
            bodyRT.sizeDelta = new Vector2(0, 800);
            scrollRect.content = bodyRT;

            var bodyVLG = bodyRT.gameObject.AddComponent<VerticalLayoutGroup>();
            bodyVLG.padding = new RectOffset(60, 60, 16, 24);
            bodyVLG.spacing = 28f;
            bodyVLG.childAlignment = TextAnchor.UpperCenter;
            bodyVLG.childForceExpandWidth = true;
            bodyVLG.childForceExpandHeight = false;
            bodyVLG.childControlWidth = true;
            bodyVLG.childControlHeight = true;

            var csf = bodyRT.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── 4. The 3 Language Cards matching Reference Mockup ──────────
            // Card 1: English (Blue badge with white "Aअ") - Selected by default
            BuildLanguageCard(bodyRT, "row-english", "check-english",
                badgeText: "Aअ",
                badgeIcon: null,
                badgeColor: UIColors.Hex("#2563EB"),
                title: "English",
                subtitle: "Continue in English",
                isDevanagari: false,
                selected: true);

            // Card 2: Hindi (Orange badge with white "अ")
            BuildLanguageCard(bodyRT, "row-hindi", "check-hindi",
                badgeText: "अ",
                badgeIcon: null,
                badgeColor: UIColors.Hex("#EA580C"),
                title: "हिंदी",
                subtitle: "हिंदी में जारी रखें",
                isDevanagari: true,
                selected: false);

            // Card 3: Santali (Green badge with floral/leaf icon)
            BuildLanguageCard(bodyRT, "row-santali", "check-santali",
                badgeText: null,
                badgeIcon: UIHelper.GetLeafSprite(),
                badgeColor: UIColors.Hex("#16A34A"),
                title: "Santali (संताली)",
                subtitle: "संताली में जारी रखें",
                isDevanagari: true,
                selected: false);

            return root;
        }

        private static void BuildLanguageCard(Transform parent,
            string cardName, string checkName,
            string badgeText, Sprite badgeIcon, Color badgeColor,
            string title, string subtitle, bool isDevanagari, bool selected)
        {
            var cardGO = UIHelper.MakeRect(cardName, parent);
            cardGO.sizeDelta = new Vector2(0, 190f);
            var le = cardGO.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 190f;
            le.minHeight = 190f;
            le.flexibleWidth = 1f;

            var cardImg = cardGO.gameObject.AddComponent<Image>();
            cardImg.color = Color.white;
            cardImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cardImg, 26f);

            var cardOutline = cardGO.gameObject.AddComponent<Outline>();
            cardOutline.effectColor = selected ? UIColors.Hex("#2563EB") : UIColors.Hex("#E2E8F0");
            cardOutline.effectDistance = selected ? new Vector2(3.5f, -3.5f) : new Vector2(1.5f, -1.5f);

            var btn = cardGO.gameObject.AddComponent<Button>();
            var cols = btn.colors;
            cols.normalColor = Color.white;
            cols.highlightedColor = new Color(0.96f, 0.98f, 1f);
            cols.pressedColor = new Color(0.92f, 0.95f, 1f);
            btn.colors = cols;

            // Inner Horizontal Layout
            var hlg = cardGO.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(32, 32, 24, 24);
            hlg.spacing = 26f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // 1. Left Badge Box (96x96)
            var badgeBox = UIHelper.MakeRect("BadgeBox", cardGO.transform);
            badgeBox.sizeDelta = new Vector2(96f, 96f);
            var bLE = badgeBox.gameObject.AddComponent<LayoutElement>();
            bLE.preferredWidth = 96f;
            bLE.preferredHeight = 96f;
            bLE.minWidth = 96f;
            bLE.minHeight = 96f;
            bLE.flexibleWidth = 0f;
            bLE.flexibleHeight = 0f;

            var badgeImg = badgeBox.gameObject.AddComponent<Image>();
            badgeImg.color = badgeColor;
            UIHelper.SetImageRoundedSprite(badgeImg, 22f);

            if (badgeIcon != null)
            {
                var iconRT = UIHelper.MakeRect("BadgeIcon", badgeBox);
                iconRT.sizeDelta = new Vector2(52f, 52f);
                var iconImg = iconRT.gameObject.AddComponent<Image>();
                iconImg.sprite = badgeIcon;
                iconImg.color = Color.white;
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
                UIHelper.Stretch(iconRT, 22, 22, 22, 22);
            }
            else
            {
                var badgeLbl = UIHelper.MakeLabel("BadgeText", badgeBox,
                    badgeText ?? "", 36f, Color.white,
                    TextAlignmentOptions.Center, bold: true, wrap: false);
                UIHelper.Stretch(badgeLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);
            }

            // 2. Center Text Column (Title + Subtitle)
            var textCol = UIHelper.MakeRect("TextCol", cardGO.transform);
            var textLE = textCol.gameObject.AddComponent<LayoutElement>();
            textLE.flexibleWidth = 1f;
            textLE.preferredHeight = 115f;
            textLE.minHeight = 90f;

            var textVLG = textCol.gameObject.AddComponent<VerticalLayoutGroup>();
            textVLG.spacing = 6f;
            textVLG.childAlignment = TextAnchor.MiddleLeft;
            textVLG.childForceExpandWidth = true;
            textVLG.childForceExpandHeight = false;
            textVLG.childControlWidth = true;
            textVLG.childControlHeight = true;

            var titleLbl = UIHelper.MakeLabel("Title", textCol,
                title, 38f, UIColors.Hex("#0F172A"),
                TextAlignmentOptions.Left, bold: true, wrap: false);
            var titleLE = titleLbl.gameObject.GetComponent<LayoutElement>() ?? titleLbl.gameObject.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 48f;
            titleLE.minHeight = 48f;

            var subLbl = UIHelper.MakeLabel("Sub", textCol,
                subtitle, 26f, UIColors.Hex("#64748B"),
                TextAlignmentOptions.Left, bold: false, wrap: false);
            var subLE = subLbl.gameObject.GetComponent<LayoutElement>() ?? subLbl.gameObject.AddComponent<LayoutElement>();
            subLE.preferredHeight = 34f;
            subLE.minHeight = 34f;

            // 3. Right Chevron Indicator (Sleek vector chevron)
            var chevronBox = UIHelper.MakeRect(checkName, cardGO.transform);
            chevronBox.sizeDelta = new Vector2(36f, 36f);
            var cLE = chevronBox.gameObject.AddComponent<LayoutElement>();
            cLE.preferredWidth = 36f;
            cLE.preferredHeight = 36f;
            cLE.minWidth = 36f;
            cLE.minHeight = 36f;
            cLE.flexibleWidth = 0f;
            cLE.flexibleHeight = 0f;

            var chevronImg = chevronBox.gameObject.AddComponent<Image>();
            chevronImg.sprite = UIHelper.GetRightChevronSprite();
            chevronImg.color = selected ? UIColors.Hex("#2563EB") : UIColors.Hex("#94A3B8");
            chevronImg.preserveAspect = true;
            chevronImg.raycastTarget = false;

            // Safe fallback label for legacy references
            var checkTMP = UIHelper.MakeLabel($"CheckText_{checkName}", chevronBox,
                "", 1f, Color.clear, TextAlignmentOptions.Center, bold: false, wrap: false);
            checkTMP.raycastTarget = false;
        }
    }
}
