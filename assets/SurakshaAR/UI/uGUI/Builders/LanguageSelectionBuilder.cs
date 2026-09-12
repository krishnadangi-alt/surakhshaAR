using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Language Selection screen — pixel-perfect recreation of Figma reference:
    /// 1080 × 2400 reference scaling:
    /// - Deep navy header with language badge, 46px title, 26px subtitle, and Skip pill button.
    /// - 3 rich language cards (190px high) with 96x96 country code badges (GB, IN, ST),
    ///   clean 38px typography without □ glyph boxes, and 54x54 circular radio indicators.
    /// - Fixed-height elements (no vertical sausage stretching).
    /// - Rich safety green "Continue →" CTA button (128px high) in #1B5E3C.
    /// </summary>
    public static class LanguageSelectionBuilder
    {
        public static GameObject Build()
        {
            var root = new GameObject("LanguageSelectionScreen");
            root.AddComponent<RectTransform>();
            var bg = root.AddComponent<Image>();
            bg.color = UIColors.Hex("#F8FAFC");
            bg.sprite = UIHelper.GetWhiteSprite();

            // ── 1. Header (Anchored to top, 320px) ──────────────────────────
            BuildHeader(root.transform);

            // ── 2. Cards & CTA Body (Below Header) ─────────────────────────
            var bodyRT = UIHelper.MakeRect("CardsBody", root.transform);
            bodyRT.anchorMin = new Vector2(0f, 0f);
            bodyRT.anchorMax = new Vector2(1f, 1f);
            bodyRT.offsetMin = Vector2.zero;
            bodyRT.offsetMax = new Vector2(0f, -320f); // below 320px header

            var bodyVLG = bodyRT.gameObject.AddComponent<VerticalLayoutGroup>();
            bodyVLG.padding = new RectOffset(48, 48, 48, 60);
            bodyVLG.spacing = 28;
            bodyVLG.childForceExpandWidth = true;
            bodyVLG.childForceExpandHeight = false;
            bodyVLG.childControlWidth = true;
            bodyVLG.childControlHeight = false; // NEVER force-expand children heights

            string hindiTitle = UIHelper.HasDevanagariSupport() ? "हिन्दी (Hindi)" : "Hindi";
            string santaliTitle = UIHelper.HasOlChikiSupport() ? "ᱥᱟᱱᱛᱟᱲᱤ (Santali)" : "Santali";

            // Language Cards (English selected by default)
            BuildCard(bodyRT, "row-english", "check-english", "EN", "English", "English · Standard", selected: true);
            BuildCard(bodyRT, "row-hindi",   "check-hindi",   "HI", hindiTitle, "Hindi · Devanagari", selected: false);
            BuildCard(bodyRT, "row-santali", "check-santali", "SAT", santaliTitle, "Santali · Ol Chiki / Latin", selected: false);

            // Spacer
            var spacer = UIHelper.MakeRect("Spacer", bodyRT);
            spacer.sizeDelta = new Vector2(0, 24);
            UIHelper.SetLayout(spacer.gameObject, preferredHeight: 24);

            // Continue Button (Rich Safety Green, 128px high)
            var btn = UIHelper.MakeButton("btn-continue", bodyRT,
                "Continue →", 36f, UIColors.SafetyGreen, Color.white, 28f);
            btn.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 128);
            var le = btn.gameObject.GetComponent<LayoutElement>() ?? btn.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 128;
            le.minHeight = 128;

            return root;
        }

        private static void BuildHeader(Transform parent)
        {
            var headerRT = UIHelper.MakeRect("Header", parent);
            headerRT.anchorMin = new Vector2(0f, 1f);
            headerRT.anchorMax = new Vector2(1f, 1f);
            headerRT.pivot = new Vector2(0.5f, 1f);
            headerRT.sizeDelta = new Vector2(0f, 320f);
            headerRT.anchoredPosition = Vector2.zero;

            var bg = headerRT.gameObject.AddComponent<Image>();
            bg.color = UIColors.PrimaryDark;
            bg.sprite = UIHelper.GetWhiteSprite();

            // Language Emblem Box (top-left, 90x90)
            var emblemBox = UIHelper.MakeRect("EmblemBox", headerRT);
            emblemBox.anchorMin = new Vector2(0f, 1f);
            emblemBox.anchorMax = new Vector2(0f, 1f);
            emblemBox.pivot = new Vector2(0f, 1f);
            emblemBox.sizeDelta = new Vector2(90f, 90f);
            emblemBox.anchoredPosition = new Vector2(48f, -64f);

            var emblemImg = emblemBox.gameObject.AddComponent<Image>();
            emblemImg.color = new Color(1f, 1f, 1f, 0.16f);
            UIHelper.SetImageRoundedSprite(emblemImg, 24f);

            var emblemLbl = UIHelper.MakeLabel("EmblemText", emblemBox,
                "EN", 34f, Color.white, TextAlignmentOptions.Center, bold: true, wrap: false);
            UIHelper.Stretch(emblemLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Skip button (top-right pill)
            var skipBtn = UIHelper.MakeButton("btn-skip", headerRT,
                "Skip", 26f, new Color(1f, 1f, 1f, 0.18f), Color.white, 24f);
            var skipRT = skipBtn.GetComponent<RectTransform>();
            skipRT.anchorMin = new Vector2(1f, 1f);
            skipRT.anchorMax = new Vector2(1f, 1f);
            skipRT.pivot = new Vector2(1f, 1f);
            skipRT.sizeDelta = new Vector2(120f, 58f);
            skipRT.anchoredPosition = new Vector2(-48f, -80f);

            // Title & Subtitle column
            var textCol = UIHelper.MakeRect("HeaderTextCol", headerRT);
            textCol.anchorMin = new Vector2(0f, 1f);
            textCol.anchorMax = new Vector2(1f, 1f);
            textCol.pivot = new Vector2(0f, 1f);
            textCol.offsetMin = new Vector2(160f, -300f);
            textCol.offsetMax = new Vector2(-190f, -50f);

            var titleTMP = UIHelper.MakeLabel("label-title", textCol, "Select Language", 44f, Color.white, bold: true, wrap: false);
            var titleRT = titleTMP.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0f, 1f);
            titleRT.anchorMax = new Vector2(1f, 1f);
            titleRT.pivot = new Vector2(0f, 1f);
            titleRT.sizeDelta = new Vector2(0f, 52f);
            titleRT.anchoredPosition = new Vector2(0f, 0f);

            var subTMP = UIHelper.MakeLabel("label-subtitle", textCol, "Choose your preferred language to continue", 24f, Hex("#94A3B8"), bold: false, wrap: false);
            var subRT = subTMP.GetComponent<RectTransform>();
            subRT.anchorMin = new Vector2(0f, 1f);
            subRT.anchorMax = new Vector2(1f, 1f);
            subRT.pivot = new Vector2(0f, 1f);
            subRT.sizeDelta = new Vector2(0f, 34f);
            subRT.anchoredPosition = new Vector2(0f, -56f);
        }

        private static void BuildCard(Transform parent,
            string cardName, string checkName,
            string badge, string title, string subtitle, bool selected)
        {
            var cardGO = UIHelper.MakeRect(cardName, parent);
            cardGO.sizeDelta = new Vector2(0, 180f);
            var le = cardGO.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 180f;
            le.minHeight = 180f;

            var cardImg = cardGO.gameObject.AddComponent<Image>();
            cardImg.color = selected ? UIColors.Primary : Color.white;
            cardImg.sprite = UIHelper.GetWhiteSprite();
            UIHelper.SetImageRoundedSprite(cardImg, 28f);

            var cardOutline = cardGO.gameObject.AddComponent<Outline>();
            cardOutline.effectColor = selected ? UIColors.PrimaryMid : Hex("#E2E8F0");
            cardOutline.effectDistance = new Vector2(2, -2);

            var btn = cardGO.gameObject.AddComponent<Button>();
            var cols = btn.colors;
            cols.normalColor = Color.white;
            cols.highlightedColor = new Color(0.96f, 0.97f, 1f);
            cols.pressedColor = new Color(0.92f, 0.94f, 0.98f);
            btn.colors = cols;

            // Inner Horizontal Row — childControlHeight = false to prevent stretching!
            var hlg = cardGO.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(32, 32, 0, 0);
            hlg.spacing = 28;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false; // NEVER true
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;     // NEVER true

            // Badge Box (left, 92x92)
            var badgeBox = UIHelper.MakeRect("BadgeBox", cardGO.transform);
            badgeBox.sizeDelta = new Vector2(92f, 92f);
            var bLE = badgeBox.gameObject.AddComponent<LayoutElement>();
            bLE.preferredWidth = 92f;
            bLE.preferredHeight = 92f;
            bLE.minWidth = 92f;
            bLE.minHeight = 92f;

            var badgeImg = badgeBox.gameObject.AddComponent<Image>();
            badgeImg.color = selected ? new Color(1f, 1f, 1f, 0.16f) : Hex("#EEF2F6");
            UIHelper.SetImageRoundedSprite(badgeImg, 22f);

            var badgeLbl = UIHelper.MakeLabel("BadgeText", badgeBox,
                badge, 36f, selected ? Color.white : UIColors.Primary,
                TextAlignmentOptions.Center, bold: true, wrap: false);
            UIHelper.Stretch(badgeLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Center Text Column
            var textCol = UIHelper.MakeRect("TextCol", cardGO.transform);
            var textLE = textCol.gameObject.AddComponent<LayoutElement>();
            textLE.flexibleWidth = 1f;

            var textVLG = textCol.gameObject.AddComponent<VerticalLayoutGroup>();
            textVLG.spacing = 6;
            textVLG.childAlignment = TextAnchor.MiddleLeft;
            textVLG.childForceExpandWidth = true;
            textVLG.childForceExpandHeight = false;
            textVLG.childControlWidth = true;
            textVLG.childControlHeight = true;

            var titleLbl = UIHelper.MakeLabel("Title", textCol,
                title, 36f, selected ? Color.white : UIColors.TextPrimary,
                TextAlignmentOptions.Left, bold: true, wrap: false);
            UIHelper.SetLayout(titleLbl.gameObject, preferredHeight: 46f);

            var subLbl = UIHelper.MakeLabel("Sub", textCol,
                subtitle, 24f, selected ? UIColors.TextOnNavyDim : Hex("#64748B"),
                TextAlignmentOptions.Left, bold: false, wrap: false);
            UIHelper.SetLayout(subLbl.gameObject, preferredHeight: 32f);

            // Right Radio Indicator Box (54x54)
            var radioBox = UIHelper.MakeRect(checkName, cardGO.transform);
            radioBox.sizeDelta = new Vector2(54f, 54f);
            var rLE = radioBox.gameObject.AddComponent<LayoutElement>();
            rLE.preferredWidth = 54f;
            rLE.preferredHeight = 54f;
            rLE.minWidth = 54f;
            rLE.minHeight = 54f;

            var radioImg = radioBox.gameObject.AddComponent<Image>();
            radioImg.sprite = UIHelper.GetCircleOutlineSprite();
            radioImg.color = selected ? Color.white : Hex("#CBD5E1");

            var dotBox = UIHelper.MakeRect("InnerDot", radioBox);
            dotBox.sizeDelta = new Vector2(26f, 26f);
            var dotImg = dotBox.gameObject.AddComponent<Image>();
            dotImg.sprite = UIHelper.GetCircleSprite();
            dotImg.color = Color.white;
            dotBox.gameObject.SetActive(selected);

            // Safe fallback TMP with empty string for legacy references
            var checkTMP = UIHelper.MakeLabel($"CheckText_{checkName}", radioBox,
                "", 20f, Color.clear,
                TextAlignmentOptions.Center, bold: false, wrap: false);
            checkTMP.raycastTarget = false;
            UIHelper.Stretch(checkTMP.GetComponent<RectTransform>(), 0, 0, 0, 0);
        }

        private static Color Hex(string hex) => UIColors.Hex(hex);
    }
}
