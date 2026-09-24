using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Gas Scenario Detail Screen — identical structure to ModuleDetailBuilder
    /// but with blue/teal accent colors. All Gas scenarios are COMING SOON.
    ///
    /// Reference scenarios (from reference image):
    ///   01 — Underground Gas Release
    ///   02 — Confined Space Entry
    ///   03 — Gas Cylinder Leak
    ///
    /// TYPOGRAPHY — same Home Dashboard system.
    /// </summary>
    public static class GasModuleDetailBuilder
    {
        private static readonly Color AccentBlue  = UIColors.Hex("#2563EB");
        private static readonly Color AccentLight  = UIColors.Hex("#EFF6FF");
        private static readonly Color NavyText     = UIColors.Hex("#0A192F");
        private static readonly Color SlateText    = UIColors.Hex("#334155");
        private static readonly Color SubText      = UIColors.Hex("#64748B");

        private static Color Hex(string h) => UIColors.Hex(h);

        public static GameObject Build()
        {
            var root = new GameObject("GasModuleDetailScreen");
            root.AddComponent<RectTransform>();
            var rootImg = root.AddComponent<Image>();
            rootImg.color  = Hex("#F8FAFC");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            BuildStickyBottomBar(root.transform);

            var scrollRoot = UIHelper.MakeRect("ScrollArea", root.transform);
            scrollRoot.anchorMin = Vector2.zero;
            scrollRoot.anchorMax = Vector2.one;
            scrollRoot.offsetMin = new Vector2(0, 100);
            scrollRoot.offsetMax = Vector2.zero;

            var sr = scrollRoot.gameObject.AddComponent<ScrollRect>();
            sr.horizontal        = false;
            sr.vertical          = true;
            sr.movementType      = ScrollRect.MovementType.Elastic;
            sr.scrollSensitivity = 40f;

            var vp = UIHelper.MakeRect("Viewport", scrollRoot);
            vp.anchorMin = Vector2.zero;
            vp.anchorMax = Vector2.one;
            vp.offsetMin = Vector2.zero;
            vp.offsetMax = Vector2.zero;
            vp.gameObject.AddComponent<RectMask2D>();
            sr.viewport = vp;

            var content = UIHelper.MakeRect("Content", vp);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot     = new Vector2(0.5f, 1);
            content.sizeDelta = Vector2.zero;
            sr.content        = content;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing              = 22;
            vlg.padding              = new RectOffset(28, 28, 18, 40);
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth      = true;
            vlg.childControlHeight     = false;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            BuildTopHeader(content);
            BuildHeroIconArea(content);
            BuildScenarioTitleRow(content);
            BuildStatChips(content);
            BuildOverviewSection(content);
            BuildTrainingStepsList(content);

            return root;
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildTopHeader(Transform parent)
        {
            var row = UIHelper.MakeHorizontal("TopHeaderRow", parent, 12,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 72, minHeight: 64);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment =
                TextAnchor.MiddleLeft;

            var backBtn = UIHelper.MakeButton("btn-back", row,
                "‹", 56, Color.white, NavyText, 18);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 70, preferredHeight: 70);
            backBtn.gameObject.AddComponent<Outline>().effectColor = Hex("#E2E8F0");

            var titleLbl = UIHelper.MakeLabel("label-title", row,
                "Underground Gas Release", 40, NavyText, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 54);

            var bellBtn = UIHelper.MakeButton("btn-bell", row, "", 14,
                Color.white, Color.white, 20);
            UIHelper.SetLayout(bellBtn.gameObject, preferredWidth: 58, preferredHeight: 58);
            bellBtn.gameObject.AddComponent<Outline>().effectColor = Hex("#E2E8F0");
            var bellGO = UIHelper.MakeRect("BellIcon", bellBtn.transform);
            UIHelper.Stretch(bellGO, 12, 12, 12, 12);
            bellGO.gameObject.AddComponent<Image>().sprite = UIHelper.GetBellSprite();

            var profBtn = UIHelper.MakeButton("btn-profile", row, "", 14,
                Color.white, Color.white, 20);
            UIHelper.SetLayout(profBtn.gameObject, preferredWidth: 58, preferredHeight: 58);
            profBtn.gameObject.AddComponent<Outline>().effectColor = Hex("#E2E8F0");
            var profGO = UIHelper.MakeRect("ProfIcon", profBtn.transform);
            UIHelper.Stretch(profGO, 12, 12, 12, 12);
            profGO.gameObject.AddComponent<Image>().sprite = UIHelper.GetProfileSprite();
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildHeroIconArea(Transform parent)
        {
            var heroCard = UIHelper.MakeRect("HeroPhotoContainer", parent);
            UIHelper.SetLayout(heroCard.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 320, minHeight: 300);

            var heroImg = heroCard.gameObject.AddComponent<Image>();
            heroImg.color = AccentLight;
            UIHelper.SetImageRoundedSprite(heroImg, 22);

            var border = heroCard.gameObject.AddComponent<Outline>();
            border.effectColor    = Hex("#BFDBFE");
            border.effectDistance = new Vector2(1.5f, -1.5f);

            var iconGO = UIHelper.MakeRect("ScenarioIconImg", heroCard);
            iconGO.anchorMin = new Vector2(0.5f, 0.5f);
            iconGO.anchorMax = new Vector2(0.5f, 0.5f);
            iconGO.pivot     = new Vector2(0.5f, 0.5f);
            iconGO.sizeDelta = new Vector2(240, 240);
            var sIconImg = iconGO.gameObject.AddComponent<Image>();
            var spr = UIHelper.LoadProjectSprite("icon_gas_scenario.jpg");
            if (spr != null)
            {
                sIconImg.sprite = spr;
                sIconImg.color  = Color.white;
                sIconImg.preserveAspect = true;
            }
            else
            {
                sIconImg.color = AccentBlue;
            }

            var badgePill = UIHelper.MakeRect("HeroLevelBadge", heroCard);
            badgePill.anchorMin        = new Vector2(1, 0);
            badgePill.anchorMax        = new Vector2(1, 0);
            badgePill.pivot            = new Vector2(1, 0);
            badgePill.anchoredPosition = new Vector2(-16, 14);
            badgePill.sizeDelta        = new Vector2(190, 48);

            var bpImg = badgePill.gameObject.AddComponent<Image>();
            bpImg.color = Hex("#EFF6FF");
            UIHelper.SetImageRoundedSprite(bpImg, 12);
            badgePill.gameObject.AddComponent<Outline>().effectColor = Hex("#BFDBFE");

            var bpLbl = UIHelper.MakeLabel("label-hero-badge", badgePill,
                "Beginner", 24, AccentBlue,
                TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(bpLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildScenarioTitleRow(Transform parent)
        {
            var col = UIHelper.MakeVertical("ScenarioMeta", parent, 8,
                childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(col.gameObject, flexibleWidth: true, flexWidth: 1);

            var row = UIHelper.MakeHorizontal("TitleRow", col.transform, 12,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 56);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment =
                TextAnchor.MiddleLeft;

            var badge = UIHelper.MakeRect("GasBox", row);
            UIHelper.SetLayout(badge.gameObject, preferredWidth: 48, preferredHeight: 48);
            var bImg = badge.gameObject.AddComponent<Image>();
            bImg.color = AccentBlue;
            UIHelper.SetImageRoundedSprite(bImg, 12);
            var gasLbl = UIHelper.MakeLabel("GasLbl", badge,
                "⛽", 28, Color.white, TextAlignmentOptions.Center);
            UIHelper.Stretch(gasLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            var titleLbl = UIHelper.MakeLabel("label-scenario-title", row,
                "Underground Gas Release", 38, NavyText, bold: true, wrap: false);
            UIHelper.SetLayout(titleLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 50);

            var descLbl = UIHelper.MakeLabel("label-scenario-desc",
                col.transform,
                "Recognize a gas release, raise the alarm and move to a safe area.",
                28, SlateText, wrap: true);
            descLbl.lineSpacing = 1.15f;
            UIHelper.SetLayout(descLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 40);
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildStatChips(Transform parent)
        {
            var row = UIHelper.MakeHorizontal("StatChipsRow", parent, 14,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 90, minHeight: 82);

            BuildChip(row, "chip-duration", "⏱",  "~ 10 mins",    "Duration");
            BuildChip(row, "chip-level",    "📊", "Beginner",      "Level");
            BuildChip(row, "chip-focus",    "🔍", "Gas Detector",  "Use");
        }

        private static void BuildChip(Transform parent, string name,
            string icon, string value, string label)
        {
            var chip = UIHelper.MakeRect(name, parent);
            UIHelper.SetLayout(chip.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 86);

            var cImg = chip.gameObject.AddComponent<Image>();
            cImg.color = Hex("#F8FAFC");
            UIHelper.SetImageRoundedSprite(cImg, 14);
            chip.gameObject.AddComponent<Outline>().effectColor = Hex("#E2E8F0");

            var innerRow = UIHelper.MakeHorizontal("Inner", chip, 8,
                new RectOffset(12, 8, 8, 8),
                childForceWidth: false, childForceHeight: false);
            UIHelper.Stretch(innerRow, 0, 0, 0, 0);
            innerRow.GetComponent<HorizontalLayoutGroup>().childAlignment =
                TextAnchor.MiddleLeft;

            var iconLbl = UIHelper.MakeLabel("Icon", innerRow,
                icon, 28, AccentBlue, TextAlignmentOptions.Center);
            UIHelper.SetLayout(iconLbl.gameObject, preferredWidth: 36, preferredHeight: 36);

            var textCol = UIHelper.MakeVertical("TextCol", innerRow, 0,
                childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(textCol.gameObject,
                flexibleWidth: true, flexWidth: 1);
            textCol.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;

            var valLbl = UIHelper.MakeLabel("Value", textCol, value, 26, NavyText, bold: true);
            UIHelper.SetLayout(valLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 32);

            var subLbl = UIHelper.MakeLabel("Sub", textCol, label, 20, SubText);
            UIHelper.SetLayout(subLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 26);
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildOverviewSection(Transform parent)
        {
            var box = UIHelper.MakeVertical("OverviewBox", parent, 10,
                childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(box.gameObject, flexibleWidth: true, flexWidth: 1);

            var headRow = UIHelper.MakeHorizontal("HeadRow", box.transform, 10,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(headRow.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 44);
            headRow.GetComponent<HorizontalLayoutGroup>().childAlignment =
                TextAnchor.MiddleLeft;

            var docIcon = UIHelper.MakeLabel("DocIcon", headRow,
                "📄", 26, AccentBlue, TextAlignmentOptions.Center);
            UIHelper.SetLayout(docIcon.gameObject, preferredWidth: 36, preferredHeight: 36);

            var headLbl = UIHelper.MakeLabel("OverviewHead", headRow,
                "Scenario Overview", 32, NavyText, bold: true);
            UIHelper.SetLayout(headLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 42);

            var bodyLbl = UIHelper.MakeLabel("label-overview-body",
                box.transform,
                "A gas leak has been detected in an underground mine area. Learn to identify the leak, raise the alarm, communicate the emergency and move to a safe area following proper procedures.",
                26, SlateText, wrap: true);
            bodyLbl.lineSpacing = 1.25f;
            UIHelper.SetLayout(bodyLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 110);
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildTrainingStepsList(Transform parent)
        {
            var box = UIHelper.MakeVertical("TrainingStepsBox", parent, 14,
                childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(box.gameObject, flexibleWidth: true, flexWidth: 1);

            var headRow = UIHelper.MakeHorizontal("HeadRow", box.transform, 12,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(headRow.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 44);
            headRow.GetComponent<HorizontalLayoutGroup>().childAlignment =
                TextAnchor.MiddleLeft;

            var bar = UIHelper.MakeRect("Bar", headRow);
            UIHelper.SetLayout(bar.gameObject, preferredWidth: 5, preferredHeight: 32);
            var barImg = bar.gameObject.AddComponent<Image>();
            barImg.color = AccentBlue;
            UIHelper.SetImageRoundedSprite(barImg, 2);

            var headLbl = UIHelper.MakeLabel("TrainingHead", headRow,
                "Training will include", 32, NavyText, bold: true);
            UIHelper.SetLayout(headLbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 42);

            var stepsCol = UIHelper.MakeVertical("steps-list", box.transform, 12,
                childForceWidth: true, childForceHeight: false);
            UIHelper.SetLayout(stepsCol.gameObject, flexibleWidth: true, flexWidth: 1);
            stepsCol.gameObject.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            // Default: Underground Gas Release (6 steps, matching reference)
            string[] defaultSteps =
            {
                "Identify gas leak signs and hazard area",
                "Activate alarm and inform control room",
                "Use gas detector and interpret readings",
                "Follow safe withdrawal procedure",
                "Follow ventilation and evacuation route",
                "Maintain safe distance and move to safe area"
            };

            for (int i = 0; i < defaultSteps.Length; i++)
                MakeStepItem(stepsCol.transform, i + 1, defaultSteps[i]);
        }

        /// <summary>Called at runtime by GasModuleDetailController to swap steps.</summary>
        public static void MakeStepItem(Transform parent, int stepNum, string stepText)
        {
            var row = UIHelper.MakeHorizontal($"Step_{stepNum}", parent, 16,
                childForceWidth: false, childForceHeight: false);
            UIHelper.SetLayout(row.gameObject,
                flexibleWidth: true, flexWidth: 1,
                preferredHeight: 52, minHeight: 46);
            row.GetComponent<HorizontalLayoutGroup>().childAlignment =
                TextAnchor.MiddleLeft;

            var badge = UIHelper.MakeRect("Badge", row);
            UIHelper.SetLayout(badge.gameObject,
                preferredWidth: 44, minWidth: 42,
                preferredHeight: 44, minHeight: 42);
            var bImg = badge.gameObject.AddComponent<Image>();
            bImg.color = AccentBlue;
            UIHelper.SetImageRoundedSprite(bImg, 22);

            var numLbl = UIHelper.MakeLabel("Num", badge,
                stepNum.ToString(), 24, Color.white,
                TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(numLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            var lbl = UIHelper.MakeLabel("StepLbl", row,
                stepText, 26, NavyText, wrap: true);
            lbl.lineSpacing = 1.15f;
            UIHelper.SetLayout(lbl.gameObject,
                flexibleWidth: true, flexWidth: 1, preferredHeight: 48);
        }

        // ─────────────────────────────────────────────────────────────────
        private static void BuildStickyBottomBar(Transform parent)
        {
            var bar = UIHelper.MakeRect("StickyBottomBar", parent);
            bar.anchorMin = Vector2.zero;
            bar.anchorMax = new Vector2(1, 0);
            bar.pivot     = new Vector2(0.5f, 0);
            bar.sizeDelta = new Vector2(0, 100);

            bar.gameObject.AddComponent<Image>().color = Color.white;

            var topBorder = UIHelper.MakeRect("TopBorder", bar);
            topBorder.anchorMin = new Vector2(0, 1);
            topBorder.anchorMax = new Vector2(1, 1);
            topBorder.pivot     = new Vector2(0.5f, 1);
            topBorder.sizeDelta = new Vector2(0, 1.5f);
            topBorder.gameObject.AddComponent<Image>().color = Hex("#E2E8F0");

            // Coming Soon button (blue bg, not launching anything)
            var btn = UIHelper.MakeButton("btn-start-ar-training", bar,
                "Coming Soon", 32, Hex("#94A3B8"), Color.white, 18);
            var rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(1, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(28, -33);
            rt.offsetMax = new Vector2(-28, 33);

            var btnLbl = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnLbl != null) btnLbl.fontStyle = FontStyles.Bold;
        }
    }
}
