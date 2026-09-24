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

            // ── 1. Header (Centered layout matching Reference Screen 2) ──
            var headerRT = UIHelper.MakeRect("HeaderArea", root.transform);
            headerRT.anchorMin = new Vector2(0f, 1f);
            headerRT.anchorMax = new Vector2(1f, 1f);
            headerRT.pivot = new Vector2(0.5f, 1f);
            headerRT.sizeDelta = new Vector2(0f, 320f);
            headerRT.anchoredPosition = Vector2.zero;

            var headerVLG = headerRT.gameObject.AddComponent<VerticalLayoutGroup>();
            headerVLG.padding = new RectOffset(48, 48, 80, 10);
            headerVLG.spacing = 14f;
            headerVLG.childAlignment = TextAnchor.UpperCenter;
            headerVLG.childForceExpandWidth = true;
            headerVLG.childForceExpandHeight = false;
            headerVLG.childControlWidth = true;
            headerVLG.childControlHeight = true;

            // Globe icon in circular badge (96x96 for clear mobile branding)
            var globeWrap = UIHelper.MakeRect("GlobeWrap", headerRT);
            var globeWrapLE = globeWrap.gameObject.AddComponent<LayoutElement>();
            globeWrapLE.preferredWidth = 96f;
            globeWrapLE.preferredHeight = 96f;
            globeWrapLE.minWidth = 96f;
            globeWrapLE.minHeight = 96f;
            globeWrapLE.flexibleWidth = 0f;

            var globeBg = globeWrap.gameObject.AddComponent<Image>();
            globeBg.color = UIColors.Hex("#EFF6FF"); // soft sky blue
            globeBg.sprite = UIHelper.GetCircleSprite();

            var globeIconRT = UIHelper.MakeRect("GlobeIcon", globeWrap);
            globeIconRT.sizeDelta = new Vector2(54f, 54f);
            var globeImg = globeIconRT.gameObject.AddComponent<Image>();
            globeImg.sprite = UIHelper.GetGlobeSprite();
            globeImg.color = UIColors.Hex("#2563EB"); // Royal blue
            globeImg.preserveAspect = true;
            UIHelper.Stretch(globeIconRT, 20, 20, 20, 20);

            // Title: "Select Language" (Centered, 58px bold)
            var titleTMP = UIHelper.MakeLabel("label-title", headerRT, "Select Language", 58f, UIColors.Hex("#0F172A"), TextAlignmentOptions.Center, bold: true, wrap: false);
            var titleLE = titleTMP.gameObject.GetComponent<LayoutElement>() ?? titleTMP.gameObject.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 72f;
            titleLE.minHeight = 70f;

            // Subtitle: "Choose your preferred language to continue" (Centered, 36px)
            var subTMP = UIHelper.MakeLabel("label-subtitle", headerRT, "Choose your preferred language\nto continue", 36f, UIColors.Hex("#334155"), TextAlignmentOptions.Center, bold: false, wrap: true);
            var subLE = subTMP.gameObject.GetComponent<LayoutElement>() ?? subTMP.gameObject.AddComponent<LayoutElement>();
            subLE.preferredHeight = 84f;
            subLE.minHeight = 80f;

            // ── 2. Bottom Continue Button (Fixed to bottom safe area, 126px high) ───────
            var bottomAreaRT = UIHelper.MakeRect("BottomArea", root.transform);
            bottomAreaRT.anchorMin = new Vector2(0f, 0f);
            bottomAreaRT.anchorMax = new Vector2(1f, 0f);
            bottomAreaRT.pivot = new Vector2(0.5f, 0f);
            bottomAreaRT.offsetMin = new Vector2(56f, 60f);
            bottomAreaRT.offsetMax = new Vector2(-56f, 186f); // 126px height

            var btn = UIHelper.MakeButton("btn-continue", bottomAreaRT,
                "Continue  >", 42f, UIColors.Hex("#0A192F"), Color.white, 28f);
            var btnRT = btn.GetComponent<RectTransform>();
            UIHelper.Stretch(btnRT, 0, 0, 0, 0);

            // ── 3. Middle Cards Scroll Container (Between Header & Button) ──
            var scrollAreaRT = UIHelper.MakeRect("ScrollArea", root.transform);
            scrollAreaRT.anchorMin = new Vector2(0f, 0f);
            scrollAreaRT.anchorMax = new Vector2(1f, 1f);
            scrollAreaRT.offsetMin = new Vector2(0f, 200f);  // Above bottom button
            scrollAreaRT.offsetMax = new Vector2(0f, -340f); // Below header

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
            bodyRT.sizeDelta = new Vector2(0, 850);
            scrollRect.content = bodyRT;

            var bodyVLG = bodyRT.gameObject.AddComponent<VerticalLayoutGroup>();
            bodyVLG.padding = new RectOffset(56, 56, 16, 24);
            bodyVLG.spacing = 26f;
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
                cardLang: SurakshaAR.Data.AppLanguage.English,
                selected: true);

            // Card 2: Hindi (Orange badge with white "अ")
            BuildLanguageCard(bodyRT, "row-hindi", "check-hindi",
                badgeText: "अ",
                badgeIcon: null,
                badgeColor: UIColors.Hex("#EA580C"),
                title: "हिंदी",
                subtitle: "हिंदी में जारी रखें",
                cardLang: SurakshaAR.Data.AppLanguage.Hindi,
                selected: false);

            // Card 3: Santali (Green badge with floral/leaf icon)
            BuildLanguageCard(bodyRT, "row-santali", "check-santali",
                badgeText: null,
                badgeIcon: UIHelper.GetLeafSprite(),
                badgeColor: UIColors.Hex("#16A34A"),
                title: "Santali (ᱥᱟᱱᱛᱟᱲᱤ)",
                subtitle: "ᱥᱟᱱᱛᱟᱲᱤ ᱛᱮ ᱞᱟᱦᱟᱜ ᱢᱮ",
                cardLang: SurakshaAR.Data.AppLanguage.Santali,
                selected: false);

            return root;
        }

        private static void BuildLanguageCard(Transform parent,
            string cardName, string checkName,
            string badgeText, Sprite badgeIcon, Color badgeColor,
            string title, string subtitle, SurakshaAR.Data.AppLanguage cardLang, bool selected)
        {
            var cardGO = UIHelper.MakeRect(cardName, parent);
            cardGO.sizeDelta = new Vector2(0, 256f);
            var le = cardGO.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 256f;
            le.minHeight = 256f;
            le.flexibleWidth = 1f;

            var cardImg = cardGO.gameObject.AddComponent<Image>();
            cardImg.color = Color.white;
            cardImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cardImg, 28f);

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

            // 1. Left Badge Box (118x118 for bold visibility)
            var badgeBox = UIHelper.MakeRect("BadgeBox", cardGO.transform);
            badgeBox.sizeDelta = new Vector2(118f, 118f);
            var bLE = badgeBox.gameObject.AddComponent<LayoutElement>();
            bLE.preferredWidth = 118f;
            bLE.preferredHeight = 118f;
            bLE.minWidth = 118f;
            bLE.minHeight = 118f;
            bLE.flexibleWidth = 0f;
            bLE.flexibleHeight = 0f;

            var badgeImg = badgeBox.gameObject.AddComponent<Image>();
            badgeImg.color = badgeColor;
            UIHelper.SetImageRoundedSprite(badgeImg, 26f);

            TMP_FontAsset cardFont = null;
            try { cardFont = UIHelper.GetFontForLanguage(cardLang); } catch {}

            if (badgeIcon != null)
            {
                var iconRT = UIHelper.MakeRect("BadgeIcon", badgeBox);
                iconRT.sizeDelta = new Vector2(60f, 60f);
                var iconImg = iconRT.gameObject.AddComponent<Image>();
                iconImg.sprite = badgeIcon;
                iconImg.color = Color.white;
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
                UIHelper.Stretch(iconRT, 26, 26, 26, 26);
            }
            else
            {
                var badgeLbl = UIHelper.MakeLabel("BadgeText", badgeBox,
                    badgeText ?? "", 48f, Color.white,
                    TextAlignmentOptions.Center, bold: true, wrap: false);
                if (cardFont != null)
                {
                    try { badgeLbl.font = cardFont; } catch {}
                }
                UIHelper.Stretch(badgeLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);
            }

            // 2. Center Text Column (Title + Subtitle)
            var textCol = UIHelper.MakeRect("TextCol", cardGO.transform);
            var textLE = textCol.gameObject.AddComponent<LayoutElement>();
            textLE.flexibleWidth = 1f;
            textLE.preferredHeight = 130f;
            textLE.minHeight = 110f;

            var textVLG = textCol.gameObject.AddComponent<VerticalLayoutGroup>();
            textVLG.spacing = 6f;
            textVLG.childAlignment = TextAnchor.MiddleLeft;
            textVLG.childForceExpandWidth = true;
            textVLG.childForceExpandHeight = false;
            textVLG.childControlWidth = true;
            textVLG.childControlHeight = true;

            var titleLbl = UIHelper.MakeLabel("Title", textCol,
                title, 52f, UIColors.Hex("#0F172A"),
                TextAlignmentOptions.Left, bold: true, wrap: false);
            if (cardFont != null)
            {
                try { titleLbl.font = cardFont; } catch {}
            }
            var titleLE = titleLbl.gameObject.GetComponent<LayoutElement>() ?? titleLbl.gameObject.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 64f;
            titleLE.minHeight = 60f;

            var subLbl = UIHelper.MakeLabel("Sub", textCol,
                subtitle, 36f, UIColors.Hex("#475569"),
                TextAlignmentOptions.Left, bold: false, wrap: false);
            if (cardFont != null)
            {
                try { subLbl.font = cardFont; } catch {}
            }
            var subLE = subLbl.gameObject.GetComponent<LayoutElement>() ?? subLbl.gameObject.AddComponent<LayoutElement>();
            subLE.preferredHeight = 52f;
            subLE.minHeight = 48f;

            // 3. Right Circular Radio/Check Indicator (48x48)
            var radioBox = UIHelper.MakeRect(checkName, cardGO.transform);
            radioBox.sizeDelta = new Vector2(48f, 48f);
            var cLE = radioBox.gameObject.AddComponent<LayoutElement>();
            cLE.preferredWidth = 48f;
            cLE.preferredHeight = 48f;
            cLE.minWidth = 48f;
            cLE.minHeight = 48f;
            cLE.flexibleWidth = 0f;
            cLE.flexibleHeight = 0f;

            var radioImg = radioBox.gameObject.AddComponent<Image>();
            radioImg.sprite = UIHelper.GetCircleSprite();
            radioImg.color = selected ? UIColors.Hex("#2563EB") : Color.white;
            radioImg.preserveAspect = true;

            var radioOutline = radioBox.gameObject.AddComponent<Outline>();
            radioOutline.effectColor = selected ? UIColors.Hex("#2563EB") : UIColors.Hex("#CBD5E1");
            radioOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var checkIconRT = UIHelper.MakeRect("CheckMarkIcon", radioBox);
            UIHelper.AnchorCenter(checkIconRT, 26f, 26f);
            var checkImg = checkIconRT.gameObject.AddComponent<Image>();
            checkImg.sprite = UIHelper.GetCheckmarkSprite();
            checkImg.color = Color.white;
            checkImg.preserveAspect = true;
            checkImg.raycastTarget = false;
            checkIconRT.gameObject.SetActive(selected);

            // Safe fallback label for legacy references
            var checkTMP = UIHelper.MakeLabel($"CheckText_{checkName}", radioBox,
                "", 1f, Color.clear, TextAlignmentOptions.Center, bold: false, wrap: false);
            checkTMP.raycastTarget = false;
        }
    }
}
