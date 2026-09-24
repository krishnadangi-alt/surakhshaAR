using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Scenario Detail Screen Builder (Screens 3, 4, 5 in reference UI):
    /// Replaces the old module introduction with the authoritative Scenario Detail UI:
    /// - Header: Back <, Scenario Title, Bell, Profile
    /// - Hero section: Realistic industrial photography + badge (Beginner / Intermediate)
    /// - Fire Icon + Scenario Title + Subtitle
    /// - 3 Compact Stat Chips: Duration, Level, Equipment/Focus
    /// - Scenario Overview with document icon
    /// - Training will include (clean 7-step numbered checklist with orange circular badges)
    /// - Bottom Sticky Bar: "▶ Start AR Training" in vibrant safety orange
    /// </summary>
    public static class ModuleDetailBuilder
    {
        private static Color Hex(string hex) => UIColors.Hex(hex);

        public static GameObject Build()
        {
            var root = new GameObject("ModuleDetailScreen");
            root.AddComponent<RectTransform>();
            var rootImg = root.AddComponent<Image>();
            rootImg.color = Hex("#F8FAFC");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // ── Scrollable Body ──────────────────────────────────────────
            var scrollRoot = UIHelper.MakeRect("ScrollArea", root.transform);
            scrollRoot.anchorMin = Vector2.zero;
            scrollRoot.anchorMax = Vector2.one;
            scrollRoot.offsetMin = new Vector2(0, 110); // Room for bottom sticky button
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
            content.sizeDelta = new Vector2(0, 2200);
            scrollRect.content = content;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.padding = new RectOffset(32, 32, 20, 40);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── Top Header ───────────────────────────────────────────────
            BuildTopHeader(content);

            // ── Hero Section ─────────────────────────────────────────────
            BuildHeroSection(content);

            // ── Scenario Title & Meta Row ────────────────────────────────
            BuildScenarioMeta(content);

            // ── Stat Chips (Duration, Level, Focus) ───────────────────────
            BuildStatChips(content);

            // ── Scenario Overview ────────────────────────────────────────
            BuildScenarioOverview(content);

            // ── Training Will Include ────────────────────────────────────
            BuildTrainingStepsList(content);

            // ── Sticky Bottom Button Bar ─────────────────────────────────
            BuildStickyBottomBar(root.transform);

            return root;
        }

        private static void BuildTopHeader(Transform parent)
        {
            var row = UIHelper.MakeHorizontal("TopHeaderRow", parent, 14, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 64, minHeight: 56);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var backBtn = UIHelper.MakeButton("btn-back", row, "‹", 50, Color.white, Hex("#0A192F"), 18);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 64, preferredHeight: 64);
            var backOutline = backBtn.gameObject.AddComponent<Outline>();
            backOutline.effectColor = Hex("#E2E8F0");
            backOutline.effectDistance = new Vector2(1, -1);

            var titleLbl = UIHelper.MakeLabel("label-title", row, "Electrical Panel Fire", 36, Hex("#0A192F"), bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 52);

            // Bell icon
            var bellBtn = UIHelper.MakeButton("btn-bell", row, "🔔", 28, Hex("#F1F5F9"), Hex("#334155"), 12);
            UIHelper.SetLayout(bellBtn.gameObject, preferredWidth: 50, preferredHeight: 46);

            // Profile icon
            var profBtn = UIHelper.MakeButton("btn-profile", row, "👤", 28, Hex("#F1F5F9"), Hex("#334155"), 12);
            UIHelper.SetLayout(profBtn.gameObject, preferredWidth: 50, preferredHeight: 46);
        }

        private static void BuildHeroSection(Transform parent)
        {
            var heroRT = UIHelper.MakeRect("HeroPhotoContainer", parent);
            UIHelper.SetLayout(heroRT.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 380, minHeight: 350);

            var heroImg = heroRT.gameObject.AddComponent<Image>();
            var spr = UIHelper.LoadProjectSprite("scenario_electrical_panel.jpg");
            if (spr != null)
            {
                heroImg.sprite = spr;
                heroImg.color = Color.white;
            }
            else
            {
                heroImg.color = Hex("#1E293B");
            }
            UIHelper.SetImageRoundedSprite(heroImg, 22);

            // Badge at bottom right of hero (e.g. Beginner or Intermediate)
            var badgePill = UIHelper.MakeRect("HeroLevelBadge", heroRT);
            badgePill.anchorMin = new Vector2(1, 0);
            badgePill.anchorMax = new Vector2(1, 0);
            badgePill.pivot     = new Vector2(1, 0);
            badgePill.anchoredPosition = new Vector2(-16, 16);
            badgePill.sizeDelta = new Vector2(170, 44);

            var bpImg = badgePill.gameObject.AddComponent<Image>();
            bpImg.color = new Color(0, 0, 0, 0.75f);
            UIHelper.SetImageRoundedSprite(bpImg, 12);

            var bpLbl = UIHelper.MakeLabel("label-hero-badge", badgePill, "📊 Beginner", 24, Hex("#FBBF24"), TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(bpLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);
        }

        private static void BuildScenarioMeta(Transform parent)
        {
            var metaCol = UIHelper.MakeVertical("MetaCol", parent, 6);
            UIHelper.SetLayout(metaCol.gameObject, flexibleWidth: true, flexWidth: 1);

            // Title row with Fire Icon
            var row = UIHelper.MakeHorizontal("TitleRow", metaCol.transform, 12, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 52);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var iconBox = UIHelper.MakeRect("FlameBox", row);
            UIHelper.SetLayout(iconBox.gameObject, preferredWidth: 46, preferredHeight: 46);
            var ibImg = iconBox.gameObject.AddComponent<Image>();
            ibImg.color = Hex("#EA580C");
            UIHelper.SetImageRoundedSprite(ibImg, 12);

            var fireSpr = UIHelper.GetFireEmojiSprite();
            if (fireSpr != null)
            {
                var fiGO = UIHelper.MakeRect("FlameImg", iconBox);
                UIHelper.Stretch(fiGO, 6, 6, 6, 6);
                var fi = fiGO.gameObject.AddComponent<Image>();
                fi.sprite = fireSpr;
                fi.preserveAspect = true;
            }
            else
            {
                var iconLbl = UIHelper.MakeLabel("FlameLbl", iconBox, "🔥", 26, Color.white, TextAlignmentOptions.Center);
                UIHelper.Stretch(iconLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);
            }

            var titleLbl = UIHelper.MakeLabel("label-scenario-title", row, "Electrical Panel Fire", 36, Hex("#0A192F"), bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 46);

            // Subtitle
            var descLbl = UIHelper.MakeLabel("label-scenario-desc", metaCol.transform,
                "Handle fire in electrical panels and control rooms.",
                26, Hex("#475569"), wrap: true);
            descLbl.lineSpacing = 1.15f;
            UIHelper.SetLayout(descLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 38);
        }

        private static void BuildStatChips(Transform parent)
        {
            var row = UIHelper.MakeHorizontal("StatChipsRow", parent, 12);
            UIHelper.SetLayout(row.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 88, minHeight: 80);

            // Chip 1: Duration (~ 10 mins)
            BuildStatChip(row, "chip-duration", "⏱", "~ 10 mins", "Duration");

            // Chip 2: Level (Beginner)
            BuildStatChip(row, "chip-level", "📊", "Beginner", "Level");

            // Chip 3: Focus (Extinguisher Use)
            BuildStatChip(row, "chip-focus", "🧯", "Extinguisher", "Use");
        }

        private static void BuildStatChip(Transform parent, string name, string icon, string boldVal, string subLabel)
        {
            var chip = UIHelper.MakeRect(name, parent);
            UIHelper.SetLayout(chip.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 84);

            var cImg = chip.gameObject.AddComponent<Image>();
            cImg.color = Hex("#F8FAFC");
            UIHelper.SetImageRoundedSprite(cImg, 14);

            var border = chip.gameObject.AddComponent<Outline>();
            border.effectColor = Hex("#E2E8F0");
            border.effectDistance = new Vector2(1, -1);

            var hlg = UIHelper.MakeHorizontal("Inner", chip, 8, new RectOffset(10, 8, 8, 8), childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(hlg, 0, 0, 0, 0);
            hlg.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var iconLbl = UIHelper.MakeLabel("Icon", hlg, icon, 26, Hex("#0A192F"), TextAlignmentOptions.Center);
            UIHelper.SetLayout(iconLbl.gameObject, preferredWidth: 32, preferredHeight: 32);

            var textCol = UIHelper.MakeVertical("TextCol", hlg, 2);
            UIHelper.SetLayout(textCol.gameObject, flexibleWidth: true, flexWidth: 1);
            var tvlg = textCol.GetComponent<VerticalLayoutGroup>();
            tvlg.childAlignment = TextAnchor.MiddleLeft;
            tvlg.childForceExpandHeight = false;

            var valLbl = UIHelper.MakeLabel("Value", textCol, boldVal, 22, Hex("#0A192F"), bold: true);
            UIHelper.SetLayout(valLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 28);

            var subLbl = UIHelper.MakeLabel("Sub", textCol, subLabel, 18, Hex("#64748B"));
            UIHelper.SetLayout(subLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 22);
        }

        private static void BuildScenarioOverview(Transform parent)
        {
            var box = UIHelper.MakeVertical("OverviewBox", parent, 8);
            UIHelper.SetLayout(box.gameObject, flexibleWidth: true, flexWidth: 1);

            var headerRow = UIHelper.MakeHorizontal("HeaderRow", box.transform, 10, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(headerRow.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 38);
            headerRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var docIcon = UIHelper.MakeLabel("DocIcon", headerRow, "📄", 24, Hex("#DC2626"), TextAlignmentOptions.Center);
            UIHelper.SetLayout(docIcon.gameObject, preferredWidth: 32, preferredHeight: 32);

            var headLbl = UIHelper.MakeLabel("OverviewHead", headerRow, "Scenario Overview", 30, Hex("#0A192F"), bold: true);
            UIHelper.SetLayout(headLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 36);

            var bodyLbl = UIHelper.MakeLabel("label-overview-body", box.transform,
                "A fire may start in an electrical control panel due to short circuit, overload or equipment failure. Learn to identify the hazard, activate the alarm and use the correct extinguisher to control the fire safely.",
                24, Hex("#475569"), wrap: true);
            bodyLbl.lineSpacing = 1.2f;
            UIHelper.SetLayout(bodyLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 96);
        }

        private static void BuildTrainingStepsList(Transform parent)
        {
            var box = UIHelper.MakeVertical("TrainingStepsBox", parent, 12);
            UIHelper.SetLayout(box.gameObject, flexibleWidth: true, flexWidth: 1);

            // Section title with orange accent bar
            var headRow = UIHelper.MakeHorizontal("HeadRow", box.transform, 10, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(headRow.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 38);
            headRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            var bar = UIHelper.MakeRect("Bar", headRow);
            UIHelper.SetLayout(bar.gameObject, preferredWidth: 4, preferredHeight: 28);
            var barImg = bar.gameObject.AddComponent<Image>();
            barImg.color = Hex("#EA580C");
            UIHelper.SetImageRoundedSprite(barImg, 2);

            var headLbl = UIHelper.MakeLabel("TrainingHead", headRow, "Training will include", 30, Hex("#0A192F"), bold: true);
            UIHelper.SetLayout(headLbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 36);

            // Container for dynamic steps
            var stepsCol = UIHelper.MakeVertical("steps-list", box.transform, 10);
            UIHelper.SetLayout(stepsCol.gameObject, flexibleWidth: true, flexWidth: 1);
            var csf = stepsCol.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Default 7 steps for Electrical Panel Fire
            string[] defaultSteps = new string[]
            {
                "Identify fire and hazard",
                "Activate fire alarm",
                "Select correct extinguisher (CO2 / Dry Chemical)",
                "Remove safety pin",
                "Grip and aim at fire base",
                "Press and spray",
                "Confirm extinguished and move to safe area"
            };

            for (int i = 0; i < defaultSteps.Length; i++)
            {
                MakeStepItem(stepsCol.transform, i + 1, defaultSteps[i]);
            }
        }

        public static void MakeStepItem(Transform parent, int stepNum, string stepText)
        {
            var row = UIHelper.MakeHorizontal($"Step_{stepNum}", parent, 14, childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 46, minHeight: 40);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;

            // Circular Orange Number Badge
            var badge = UIHelper.MakeRect("Badge", row);
            UIHelper.SetLayout(badge.gameObject, preferredWidth: 36, minWidth: 36, preferredHeight: 36, minHeight: 36);
            var bImg = badge.gameObject.AddComponent<Image>();
            bImg.color = Hex("#EA580C");
            UIHelper.SetImageRoundedSprite(bImg, 18);

            var numLbl = UIHelper.MakeLabel("Num", badge, stepNum.ToString(), 20, Color.white, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(numLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Step Instruction Text
            var lbl = UIHelper.MakeLabel("StepLbl", row, stepText, 25, Hex("#1E293B"), wrap: true);
            lbl.lineSpacing = 1.1f;
            UIHelper.SetLayout(lbl.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 42);
        }

        private static void BuildStickyBottomBar(Transform parent)
        {
            var bar = UIHelper.MakeRect("StickyBottomBar", parent);
            bar.anchorMin = new Vector2(0, 0);
            bar.anchorMax = new Vector2(1, 0);
            bar.pivot     = new Vector2(0.5f, 0);
            bar.sizeDelta = new Vector2(0, 110);

            var bg = bar.gameObject.AddComponent<Image>();
            bg.color = Color.white;

            var topBorder = UIHelper.MakeRect("TopBorder", bar);
            topBorder.anchorMin = new Vector2(0, 1);
            topBorder.anchorMax = new Vector2(1, 1);
            topBorder.pivot     = new Vector2(0.5f, 1);
            topBorder.sizeDelta = new Vector2(0, 1.5f);
            var tbImg = topBorder.gameObject.AddComponent<Image>();
            tbImg.color = Hex("#E2E8F0");

            var btn = UIHelper.MakeButton("btn-start-ar-training", bar, "▶ Start AR Training", 30, Hex("#EA580C"), Color.white, 16);
            UIHelper.SetLayout(btn.gameObject, preferredHeight: 74, flexibleWidth: true, flexWidth: 1);
            var rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(32, -37);
            rt.offsetMax = new Vector2(-32, 37);

            var btnLbl = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnLbl != null)
            {
                btnLbl.fontStyle = FontStyles.Bold;
            }
        }
    }
}
