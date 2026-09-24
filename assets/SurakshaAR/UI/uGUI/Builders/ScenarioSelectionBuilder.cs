using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.UI.Builders
{
    /// <summary>
    /// Scenario Selection screen builder:
    /// Displays industrial safety scenarios for the chosen module.
    /// Provides 3 interactive scenario cards wired to ScenarioSelectionController:
    /// - card-scenario-1: Electrical Panel Fire (AR-ready)
    /// - card-scenario-2: Chemical Storage Fire
    /// - card-scenario-3: Workshop Fire
    /// </summary>
    public static class ScenarioSelectionBuilder
    {
        public static GameObject Build()
        {
            var root = new GameObject("ScenarioSelectionScreen");
            root.AddComponent<RectTransform>();
            var rootImg = root.AddComponent<Image>();
            rootImg.color = UIColors.Hex("#F8FAFC");
            rootImg.sprite = UIHelper.GetWhiteSprite();

            // ── Scrollable Body ──────────────────────────────────────────
            var scrollRoot = UIHelper.MakeRect("ScrollArea", root.transform);
            scrollRoot.anchorMin = Vector2.zero;
            scrollRoot.anchorMax = Vector2.one;
            scrollRoot.offsetMin = Vector2.zero;
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
            content.sizeDelta = new Vector2(0, 1600);
            scrollRect.content = content;

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 24;
            vlg.padding = new RectOffset(40, 40, 48, 60);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            var csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── Top Header ───────────────────────────────────────────────
            BuildTopBar(content);

            // ── Banner Info ──────────────────────────────────────────────
            BuildInfoBanner(content);

            // ── Scenarios List ───────────────────────────────────────────
            BuildScenarioCards(content);

            return root;
        }

        private static void BuildTopBar(Transform parent)
        {
            var row = UIHelper.MakeHorizontal("TopBarRow", parent, 16);
            UIHelper.SetLayout(row.gameObject, preferredHeight: 84);

            var backBtn = UIHelper.MakeButton("btn-back", row, "‹", 52, Color.white, UIColors.PrimaryDark, 20);
            UIHelper.SetLayout(backBtn.gameObject, preferredWidth: 76, preferredHeight: 76);
            var border = backBtn.gameObject.AddComponent<Outline>();
            border.effectColor = UIColors.Border;
            border.effectDistance = new Vector2(1, -1);

            var titleLbl = UIHelper.MakeLabel("label-title", row, "Select Scenario", 52, UIColors.PrimaryDark, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1);
        }

        private static void BuildInfoBanner(Transform parent)
        {
            var banner = new GameObject("InfoBanner");
            banner.transform.SetParent(parent, false);
            UIHelper.SetLayout(banner, preferredHeight: 180, minHeight: 160);

            var bImg = banner.AddComponent<Image>();
            bImg.color = UIColors.Hex("#EBF5FF");
            UIHelper.SetImageRoundedSprite(bImg, 20);

            var inner = UIHelper.MakeVertical("Inner", banner.transform, 8, new RectOffset(24, 24, 18, 18));
            UIHelper.Stretch(inner, 0, 0, 0, 0);

            var title = UIHelper.MakeLabel("BannerTitle", inner, "Standard Industrial Scenarios", 38, UIColors.PrimaryDark, bold: true);
            UIHelper.SetLayout(title.gameObject, preferredHeight: 46);

            var desc = UIHelper.MakeLabel("BannerDesc", inner,
                "Select an emergency situation below to begin mobile AR assessment. Zero critical errors are required for compliance certification.",
                32, UIColors.TextSecondary, wrap: true);
            desc.lineSpacing = 4f;
            UIHelper.SetLayout(desc.gameObject, preferredHeight: 96, flexibleWidth: true, flexWidth: 1);
        }

        private static void BuildScenarioCards(Transform parent)
        {
            var col = UIHelper.MakeVertical("ScenarioList", parent, 18);
            UIHelper.SetLayout(col.gameObject, minHeight: 480);
            var csf = col.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Card 1: Electrical Panel Fire
            BuildScenarioCard(
                parent: col,
                btnName: "card-scenario-1",
                scenarioNum: "01",
                title: "1. Electrical Panel Fire",
                subtitle: "High voltage breaker ignition in substation. Requires Class C/CO2 agent selection.",
                badge: "AR PRACTICAL READY",
                badgeColor: UIColors.SafetyGreen,
                badgeBg: UIColors.Hex("#E6F4EC"),
                icon: "electrical"
            );

            // Card 2: Chemical Storage Fire
            BuildScenarioCard(
                parent: col,
                btnName: "card-scenario-2",
                scenarioNum: "02",
                title: "2. Chemical Storage Fire",
                subtitle: "Flammable hydrocarbon leak near storage tanks. Dry chemical powder protocol.",
                badge: "SIMULATION READY",
                badgeColor: UIColors.Primary,
                badgeBg: UIColors.Hex("#EBF5FF"),
                icon: "gas"
            );

            // Card 3: Workshop Conveyor Fire
            BuildScenarioCard(
                parent: col,
                btnName: "card-scenario-3",
                scenarioNum: "03",
                title: "3. Workshop Conveyor Fire",
                subtitle: "Friction ignition on coal conveyor belt. Alarm activation and emergency exit path.",
                badge: "SIMULATION READY",
                badgeColor: UIColors.Warning,
                badgeBg: UIColors.Hex("#FEF3C7"),
                icon: "machinery"
            );
        }

        private static void BuildScenarioCard(
            Transform parent,
            string btnName,
            string scenarioNum,
            string title,
            string subtitle,
            string badge,
            Color badgeColor,
            Color badgeBg,
            string icon)
        {
            var btn = UIHelper.MakeButton(btnName, parent, "", 16, Color.white, UIColors.TextPrimary, 22);
            UIHelper.SetLayout(btn.gameObject, preferredHeight: 270, minHeight: 240);

            var outline = btn.gameObject.AddComponent<Outline>();
            outline.effectColor = UIColors.Border;
            outline.effectDistance = new Vector2(1, -1);

            var inner = UIHelper.MakeHorizontal("Inner", btn.transform, 20, new RectOffset(22, 22, 20, 20));
            UIHelper.Stretch(inner, 0, 0, 0, 0);

            // Left Icon Box
            var iconBox = UIHelper.MakeRect("IconBox", inner);
            UIHelper.SetLayout(iconBox.gameObject, preferredWidth: 104, preferredHeight: 104, minWidth: 104, minHeight: 104);
            var iconBg = iconBox.gameObject.AddComponent<Image>();
            iconBg.color = badgeBg;
            UIHelper.SetImageRoundedSprite(iconBg, 20);

            Sprite spr = null;
            if (icon.Contains("🔥") || icon.ToLower().Contains("fire")) spr = UIHelper.GetFireEmojiSprite();
            else if (icon.Contains("☁") || icon.ToLower().Contains("gas")) spr = UIHelper.GetGasEmojiSprite();
            else if (icon.Contains("⚙") || icon.ToLower().Contains("gear") || icon.ToLower().Contains("machin")) spr = UIHelper.GetGearEmojiSprite();
            else if (icon.ToLower().Contains("electr")) spr = UIHelper.GetShieldSprite();

            if (spr != null)
            {
                var iconImgGo = new GameObject("IconImg");
                iconImgGo.transform.SetParent(iconBox, false);
                var img = iconImgGo.AddComponent<Image>();
                img.sprite = spr;
                img.preserveAspect = true;
                var rt = iconImgGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(60, 60);
            }
            else
            {
                var iconLbl = UIHelper.MakeLabel("IconLbl", iconBox, icon, 44, Color.white, TextAlignmentOptions.Center);
                UIHelper.Stretch(iconLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);
            }

            // Middle Content Column
            var textCol = UIHelper.MakeVertical("TextCol", inner, 6);
            UIHelper.SetLayout(textCol.gameObject, flexibleWidth: true, flexWidth: 1);

            var titleLbl = UIHelper.MakeLabel("Title", textCol, title, 42, UIColors.PrimaryDark, bold: true, wrap: true);
            titleLbl.lineSpacing = 2f;
            UIHelper.SetLayout(titleLbl.gameObject, preferredHeight: 58, minHeight: 48);

            var descLbl = UIHelper.MakeLabel("Desc", textCol, subtitle, 30, UIColors.TextSecondary, wrap: true);
            descLbl.lineSpacing = 4f;
            UIHelper.SetLayout(descLbl.gameObject, preferredHeight: 96, minHeight: 72);

            var badgeRow = UIHelper.MakeHorizontal("BadgeRow", textCol, 6);
            UIHelper.SetLayout(badgeRow.gameObject, preferredHeight: 44);

            var badgePill = UIHelper.MakeRect("BadgePill", badgeRow);
            UIHelper.SetLayout(badgePill.gameObject, preferredWidth: 260, preferredHeight: 44);
            var bImg = badgePill.gameObject.AddComponent<Image>();
            bImg.color = badgeBg;
            UIHelper.SetImageRoundedSprite(bImg, 10);

            var bLbl = UIHelper.MakeLabel("BLbl", badgePill, badge, 28, badgeColor, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(bLbl.GetComponent<RectTransform>(), 0, 0, 0, 0);

            // Right Arrow
            var arrow = UIHelper.MakeLabel("Arrow", inner, ">", 44, UIColors.TextMuted, TextAlignmentOptions.Right);
            UIHelper.SetLayout(arrow.gameObject, preferredWidth: 32);
        }
    }
}
