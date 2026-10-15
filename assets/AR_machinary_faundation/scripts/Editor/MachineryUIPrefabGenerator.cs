#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using SurakshaAR.UI;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryUIPrefabGenerator
    {
        private const string PrefabDir = "Assets/AR_machinary_faundation/UI/Prefabs";

        private static readonly Color ColorWhiteCard       = new Color(1f, 1f, 1f, 0.98f);
        private static readonly Color ColorTextNavy        = new Color(0.06f, 0.09f, 0.16f);
        private static readonly Color ColorTextMuted       = new Color(0.38f, 0.44f, 0.52f);
        private static readonly Color ColorTealPrimary     = new Color(0.05f, 0.49f, 0.40f);
        private static readonly Color ColorPillGreenBg     = new Color(0.92f, 0.98f, 0.95f);
        private static readonly Color ColorPillGreenText   = new Color(0.02f, 0.37f, 0.27f);
        private static readonly Color ColorHazardRed       = new Color(0.86f, 0.15f, 0.15f);
        private static readonly Color ColorGrayButtonBg    = new Color(0.95f, 0.96f, 0.98f);
        private static readonly Color ColorGrayButtonText  = new Color(0.20f, 0.25f, 0.33f);
        private static readonly Color ColorDetectionCorner = new Color(0.06f, 0.85f, 0.60f);

        [MenuItem("SurakshaAR/Machinery/Generate All UI Prefabs")]
        public static void GenerateAllPrefabs()
        {
            if (!Directory.Exists(PrefabDir)) Directory.CreateDirectory(PrefabDir);

            GenerateMainMenuPrefab();
            GenerateARScanningPanelPrefab();
            GeneratePlacementPanelPrefab();
            GenerateMachineryPlacedPanelPrefab();
            GenerateComponentOverviewPrefab();
            GenerateComponentInfoCardPrefab();
            GenerateDetectionTargetPrefab();
            GenerateTrainingStepPanelPrefab();
            GenerateTrainingCompletePanelPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MachineryUIPrefabGenerator] Successfully generated all 9 UI prefabs in " + PrefabDir);
        }

        private static void SavePrefab(GameObject root, string filename)
        {
            string path = Path.Combine(PrefabDir, filename);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        // 1. MachineryMainMenu.prefab
        private static void GenerateMainMenuPrefab()
        {
            var go = new GameObject("MachineryMainMenu", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(1000, 2200);

            var bg = go.AddComponent<Image>();
            bg.color = Color.white;
            UIHelper.SetImageRoundedSprite(bg, 32f);

            var vBox = UIHelper.MakeVertical("MenuContent", rt, 18, new RectOffset(40, 40, 36, 36));
            UIHelper.Stretch(vBox);

            var hHeader = UIHelper.MakeHorizontal("HeaderRow", vBox, 16);
            UIHelper.SetLayout(hHeader.gameObject, preferredHeight: 100);
            UIHelper.MakeButton("BackBtn", hHeader, "←", 34, ColorGrayButtonBg, ColorTextNavy, 16f);
            var titleBox = UIHelper.MakeVertical("TitleBox", hHeader, 2);
            UIHelper.SetLayout(titleBox.gameObject, flexibleWidth: true, flexWidth: 1);
            UIHelper.MakeLabel("BrandLabel", titleBox, "SurakshaAR", 24, ColorTextMuted, bold: true);
            UIHelper.MakeLabel("ModuleLabel", titleBox, "Machinery AR", 44, ColorTextNavy, bold: true);
            UIHelper.MakeLabel("SubLabel", titleBox, "Safety Training", 24, ColorTealPrimary, bold: true);
            UIHelper.MakeButton("SettingsBtn", hHeader, "⚙", 34, ColorGrayButtonBg, ColorTextNavy, 16f);

            var prevBox = UIHelper.MakeRect("PreviewContainer", vBox);
            UIHelper.SetLayout(prevBox.gameObject, preferredHeight: 460);
            var prevBg = prevBox.gameObject.AddComponent<Image>();
            prevBg.color = new Color(0.96f, 0.98f, 0.99f);
            UIHelper.SetImageRoundedSprite(prevBg, 24f);

            var featList = UIHelper.MakeVertical("FeatureList", vBox, 14);
            UIHelper.SetLayout(featList.gameObject, flexibleHeight: true, flexHeight: 1);

            MakeFeatureCard(featList, "📖", "Learn machine components", "Understand the purpose of each major component.");
            MakeFeatureCard(featList, "🛡️", "Understand safety risks", "Identify hazards and critical safety controls.");
            MakeFeatureCard(featList, "📋", "Follow correct procedure", "Learn the correct conveyor-jam safety sequence.");

            var btnStart = UIHelper.MakeButton("StartARTrainingBtn", vBox, "Start AR Training  →", 32, ColorTealPrimary, Color.white, 20f);
            UIHelper.SetLayout(btnStart.gameObject, preferredHeight: 90);

            SavePrefab(go, "MachineryMainMenu.prefab");
        }

        // 2. ARScanningPanel.prefab
        private static void GenerateARScanningPanelPrefab()
        {
            var go = new GameObject("ARScanningPanel", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(1000, 2200);

            var topCard = UIHelper.MakeRect("ScanningTopCard", rt);
            UIHelper.AnchorTopStretch(topCard, 170, 0);
            var bgTop = topCard.gameObject.AddComponent<Image>();
            bgTop.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(bgTop, 24f);
            var hTop = UIHelper.MakeHorizontal("HTop", topCard, 14, new RectOffset(20, 24, 20, 20));
            UIHelper.Stretch(hTop);
            UIHelper.MakeButton("BackBtn", hTop, "←", 28, ColorGrayButtonBg, ColorTextNavy, 14f);
            var vTop = UIHelper.MakeVertical("TopContent", hTop, 4);
            UIHelper.SetLayout(vTop.gameObject, flexibleWidth: true, flexWidth: 1);
            UIHelper.MakeLabel("Title", vTop, "Scan the floor to place the machinery", 28, ColorTextNavy, bold: true);
            UIHelper.MakeLabel("Sub", vTop, "Move your phone slowly until a suitable horizontal surface is detected.", 20, ColorTextMuted);

            var botCard = UIHelper.MakeRect("ScanningBottomCard", rt);
            UIHelper.AnchorBottomStretch(botCard, 110, 50);
            var bgBot = botCard.gameObject.AddComponent<Image>();
            bgBot.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(bgBot, 24f);
            var hBot = UIHelper.MakeHorizontal("BotContent", botCard, 16, new RectOffset(32, 32, 16, 16));
            UIHelper.Stretch(hBot);
            UIHelper.MakeLabel("SpinIcon", hBot, "◌", 36, ColorTealPrimary, TextAlignmentOptions.Center);
            UIHelper.MakeLabel("ScanText", hBot, "Scanning for surface...", 26, ColorTextNavy, bold: true);

            SavePrefab(go, "ARScanningPanel.prefab");
        }

        // 3. PlacementPanel.prefab
        private static void GeneratePlacementPanelPrefab()
        {
            var go = new GameObject("PlacementPanel", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(1000, 2200);

            var topCard = UIHelper.MakeRect("PlacementTopCard", rt);
            UIHelper.AnchorTopStretch(topCard, 160, 0);
            var bg = topCard.gameObject.AddComponent<Image>();
            bg.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(bg, 24f);
            var hTop = UIHelper.MakeHorizontal("HTop", topCard, 14, new RectOffset(20, 24, 20, 20));
            UIHelper.Stretch(hTop);
            UIHelper.MakeButton("BackBtn", hTop, "←", 28, ColorGrayButtonBg, ColorTextNavy, 14f);
            var v = UIHelper.MakeVertical("Content", hTop, 4);
            UIHelper.SetLayout(v.gameObject, flexibleWidth: true, flexWidth: 1);
            UIHelper.MakeLabel("Title", v, "Tap on the highlighted area to place the machinery", 28, ColorTextNavy, bold: true);
            UIHelper.MakeLabel("Sub", v, "The blue placement indicator tracks the detected floor.", 20, ColorTealPrimary);

            SavePrefab(go, "PlacementPanel.prefab");
        }

        // 4. MachineryPlacedPanel.prefab
        private static void GenerateMachineryPlacedPanelPrefab()
        {
            var go = new GameObject("MachineryPlacedPanel", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(1000, 2200);

            var topCard = UIHelper.MakeRect("PlacedTopCard", rt);
            UIHelper.AnchorTopStretch(topCard, 170, 0);
            var bgTop = topCard.gameObject.AddComponent<Image>();
            bgTop.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(bgTop, 24f);
            var hTop = UIHelper.MakeHorizontal("HTop", topCard, 14, new RectOffset(20, 24, 20, 20));
            UIHelper.Stretch(hTop);
            UIHelper.MakeButton("BackBtn", hTop, "←", 28, ColorGrayButtonBg, ColorTextNavy, 14f);
            var vTop = UIHelper.MakeVertical("TopContent", hTop, 6);
            UIHelper.SetLayout(vTop.gameObject, flexibleWidth: true, flexWidth: 1);
            var hCheck = UIHelper.MakeHorizontal("CheckRow", vTop, 10);
            UIHelper.SetLayout(hCheck.gameObject, preferredHeight: 38);
            UIHelper.MakeLabel("CheckIcon", hCheck, "✓", 30, ColorTealPrimary, bold: true);
            UIHelper.MakeLabel("PlacedTitle", hCheck, "Machinery Placed!", 28, ColorTextNavy, bold: true);
            UIHelper.MakeLabel("PlacedSub", vTop, "You can now explore the components. Tap on any part to learn more.", 19, ColorTextMuted);

            var botCard = UIHelper.MakeRect("PlacedBottomCard", rt);
            UIHelper.AnchorBottomStretch(botCard, 230, 30);
            var bgBot = botCard.gameObject.AddComponent<Image>();
            bgBot.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(bgBot, 24f);
            var vBot = UIHelper.MakeVertical("BotContent", botCard, 16, new RectOffset(32, 32, 24, 24));
            UIHelper.Stretch(vBot);
            var hInfo = UIHelper.MakeHorizontal("InfoRow", vBot, 12);
            UIHelper.SetLayout(hInfo.gameObject, preferredHeight: 38);
            UIHelper.MakeLabel("InfoIcon", hInfo, "ℹ", 28, ColorTealPrimary, bold: true);
            UIHelper.MakeLabel("InfoText", hInfo, "Walk around the machinery to view from different angles.", 22, ColorTextNavy);
            var hBtns = UIHelper.MakeHorizontal("BtnsRow", vBot, 16);
            UIHelper.SetLayout(hBtns.gameObject, preferredHeight: 80);
            UIHelper.MakeButton("ExploreBtn", hBtns, "OVERVIEW", 24, ColorGrayButtonBg, ColorGrayButtonText, 16f);
            UIHelper.MakeButton("StartTrainBtn", hBtns, "Start Training  →", 28, ColorTealPrimary, Color.white, 16f);

            SavePrefab(go, "MachineryPlacedPanel.prefab");
        }

        // 5. ComponentOverview.prefab
        private static void GenerateComponentOverviewPrefab()
        {
            var go = new GameObject("ComponentOverview", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(1000, 2200);

            var topCard = UIHelper.MakeRect("OverviewTopCard", rt);
            UIHelper.AnchorTopStretch(topCard, 140, 0);
            var bgTop = topCard.gameObject.AddComponent<Image>();
            bgTop.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(bgTop, 24f);
            var hTop = UIHelper.MakeHorizontal("HTop", topCard, 14, new RectOffset(20, 24, 20, 20));
            UIHelper.Stretch(hTop);
            UIHelper.MakeButton("BackBtn", hTop, "←", 28, ColorGrayButtonBg, ColorTextNavy, 14f);
            var vTop = UIHelper.MakeVertical("TopContent", hTop, 4);
            UIHelper.SetLayout(vTop.gameObject, flexibleWidth: true, flexWidth: 1);
            UIHelper.MakeLabel("Title", vTop, "MACHINERY OVERVIEW", 28, ColorTextNavy, bold: true);
            UIHelper.MakeLabel("Sub", vTop, "Tap any component to learn its purpose.", 20, ColorTealPrimary);

            var botCard = UIHelper.MakeRect("OverviewBottomCard", rt);
            UIHelper.AnchorBottomStretch(botCard, 190, 20);
            var bgBot = botCard.gameObject.AddComponent<Image>();
            bgBot.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(bgBot, 24f);
            var vBot = UIHelper.MakeVertical("BotContent", botCard, 12, new RectOffset(32, 32, 20, 20));
            UIHelper.Stretch(vBot);
            var hInfo = UIHelper.MakeHorizontal("InfoRow", vBot, 12);
            UIHelper.SetLayout(hInfo.gameObject, preferredHeight: 36);
            UIHelper.MakeLabel("BookIcon", hInfo, "📖", 26, ColorTealPrimary);
            UIHelper.MakeLabel("PromptText", hInfo, "Tap on any component to learn its purpose", 22, ColorTextNavy);
            UIHelper.MakeButton("StartTrainingBtn", vBot, "Start Training  →", 28, ColorTealPrimary, Color.white, 16f);

            SavePrefab(go, "ComponentOverview.prefab");
        }

        // 6. ComponentInfoCard.prefab
        private static void GenerateComponentInfoCardPrefab()
        {
            var go = new GameObject("ComponentInfoCard", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(920, 620);

            var bg = go.AddComponent<Image>();
            bg.color = Color.white;
            UIHelper.SetImageRoundedSprite(bg, 28f);

            var vBox = UIHelper.MakeVertical("ModalContent", rt, 16, new RectOffset(36, 36, 28, 28));
            UIHelper.Stretch(vBox);

            var hHeader = UIHelper.MakeHorizontal("ModalHeader", vBox, 16);
            UIHelper.SetLayout(hHeader.gameObject, preferredHeight: 48);
            UIHelper.MakeLabel("ModalTitle", hHeader, "DRIVE MOTOR", 32, ColorTextNavy, bold: true);
            UIHelper.MakeButton("CloseBtn", hHeader, "✕", 26, ColorGrayButtonBg, ColorTextNavy, 12f);

            UIHelper.MakeLabel("Sec1Title", vBox, "What is it?", 22, ColorTealPrimary, bold: true);
            UIHelper.MakeLabel("Sec1Body", vBox, "Description here...", 20, ColorTextNavy);

            UIHelper.MakeLabel("Sec2Title", vBox, "Why is it important?", 22, ColorTealPrimary, bold: true);
            UIHelper.MakeLabel("Sec2Body", vBox, "Importance here...", 20, ColorTextNavy);

            UIHelper.MakeLabel("Sec3Title", vBox, "Worker Action", 22, ColorTealPrimary, bold: true);
            UIHelper.MakeLabel("Sec3Body", vBox, "Worker action instructions...", 20, ColorTextNavy);

            UIHelper.MakeButton("GotItBtn", vBox, "GOT IT", 26, ColorTealPrimary, Color.white, 16f);

            SavePrefab(go, "ComponentInfoCard.prefab");
        }

        // 7. DetectionTarget.prefab
        private static void GenerateDetectionTargetPrefab()
        {
            var go = new GameObject("DetectionTarget", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(300, 300);

            float barLen = 44f;
            float barThick = 6f;

            CreateCorner(rt, "CornerTL", new Vector2(0, 1), new Vector2(0, 1), barLen, barThick);
            CreateCorner(rt, "CornerTR", new Vector2(1, 1), new Vector2(1, 1), barLen, barThick);
            CreateCorner(rt, "CornerBL", new Vector2(0, 0), new Vector2(0, 0), barLen, barThick);
            CreateCorner(rt, "CornerBR", new Vector2(1, 0), new Vector2(1, 0), barLen, barThick);

            var badgeGO = new GameObject("TargetBadge", typeof(RectTransform));
            badgeGO.transform.SetParent(rt, false);
            var badgeRT = badgeGO.GetComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0.5f, 1f);
            badgeRT.anchorMax = new Vector2(0.5f, 1f);
            badgeRT.pivot = new Vector2(0.5f, 0f);
            badgeRT.anchoredPosition = new Vector2(0, 16);
            badgeRT.sizeDelta = new Vector2(250, 48);

            var badgeBg = badgeGO.AddComponent<Image>();
            badgeBg.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(badgeBg, 14f);
            UIHelper.MakeLabel("TargetText", badgeRT, "⚠️ Jam Area", 20, ColorTextNavy, TextAlignmentOptions.Center, bold: true);

            SavePrefab(go, "DetectionTarget.prefab");
        }

        // 8. TrainingStepPanel.prefab
        private static void GenerateTrainingStepPanelPrefab()
        {
            var go = new GameObject("TrainingStepPanel", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(1000, 2200);

            // Top HUD
            var topHUD = UIHelper.MakeRect("TrainingTopHUD", rt);
            UIHelper.AnchorTopStretch(topHUD, 140, 0);
            var bgTop = topHUD.gameObject.AddComponent<Image>();
            bgTop.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(bgTop, 24f);
            var vTop = UIHelper.MakeVertical("HUDLayout", topHUD, 10, new RectOffset(30, 30, 16, 16));
            UIHelper.Stretch(vTop);
            var hRow = UIHelper.MakeHorizontal("HRow", vTop, 12);
            UIHelper.SetLayout(hRow.gameObject, preferredHeight: 48);
            UIHelper.MakeButton("HUDBackBtn", hRow, "←", 26, ColorGrayButtonBg, ColorTextNavy, 12f);
            var tBox = UIHelper.MakeVertical("TBox", hRow, 2);
            UIHelper.SetLayout(tBox.gameObject, flexibleWidth: true, flexWidth: 1);
            UIHelper.MakeLabel("HUDStep", tBox, "Step 1 / 7", 26, ColorTextNavy, bold: true);
            UIHelper.MakeLabel("HUDSub", tBox, "Conveyor Jam Procedure", 18, ColorTextMuted);

            // Bottom Step Card
            var botCard = UIHelper.MakeRect("StepCardBottom", rt);
            UIHelper.AnchorBottomStretch(botCard, 340, 20);
            var bgBot = botCard.gameObject.AddComponent<Image>();
            bgBot.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(bgBot, 28f);
            var vBot = UIHelper.MakeVertical("CardContent", botCard, 12, new RectOffset(32, 32, 22, 22));
            UIHelper.Stretch(vBot);
            UIHelper.MakeLabel("Title", vBot, "⚠️ Identify the Hazard", 28, ColorTextNavy, bold: true);
            UIHelper.MakeLabel("Instruction", vBot, "Look at the conveyor belt and identify the material blockage.", 22, ColorTextNavy);
            var pillBox = UIHelper.MakeRect("GuidancePill", vBot);
            UIHelper.SetLayout(pillBox.gameObject, preferredHeight: 52);
            var pillBg = pillBox.gameObject.AddComponent<Image>();
            pillBg.color = ColorPillGreenBg;
            UIHelper.SetImageRoundedSprite(pillBg, 14f);
            UIHelper.MakeLabel("GuidanceText", pillBox, "➔ Move closer and inspect the highlighted jam area.", 19, ColorPillGreenText, bold: true);
            UIHelper.MakeLabel("SafetyText", vBot, "Do not attempt to clear the blockage while the machine is running.", 18, ColorHazardRed);
            var hBtns = UIHelper.MakeHorizontal("NavBtns", vBot, 16);
            UIHelper.SetLayout(hBtns.gameObject, preferredHeight: 74);
            UIHelper.MakeButton("PrevBtn", hBtns, "Previous", 24, ColorGrayButtonBg, ColorGrayButtonText, 16f);
            UIHelper.MakeButton("NextBtn", hBtns, "Next  →", 26, ColorTealPrimary, Color.white, 16f);

            SavePrefab(go, "TrainingStepPanel.prefab");
        }

        // 9. TrainingCompletePanel.prefab
        private static void GenerateTrainingCompletePanelPrefab()
        {
            var go = new GameObject("TrainingCompletePanel", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(1000, 2200);

            var bg = go.AddComponent<Image>();
            bg.color = Color.white;
            UIHelper.SetImageRoundedSprite(bg, 32f);

            var vBox = UIHelper.MakeVertical("CompleteContent", rt, 18, new RectOffset(40, 40, 44, 44));
            UIHelper.Stretch(vBox);

            var checkCircle = UIHelper.MakeRect("CheckCircle", vBox);
            UIHelper.SetLayout(checkCircle.gameObject, preferredWidth: 96, preferredHeight: 96);
            var circleImg = checkCircle.gameObject.AddComponent<Image>();
            circleImg.color = ColorPillGreenBg;
            UIHelper.SetImageRoundedSprite(circleImg, 48f);
            UIHelper.MakeLabel("CheckIcon", checkCircle, "✓", 54, ColorTealPrimary, TextAlignmentOptions.Center, bold: true);

            UIHelper.MakeLabel("CompTitle", vBox, "TRAINING COMPLETE", 40, ColorTextNavy, TextAlignmentOptions.Center, bold: true);
            UIHelper.MakeLabel("CompSub", vBox, "Conveyor Jam Safety Procedure Demonstrated", 22, ColorTextMuted, TextAlignmentOptions.Center);

            var checklistCard = UIHelper.MakeRect("ChecklistCard", vBox);
            UIHelper.SetLayout(checklistCard.gameObject, flexibleHeight: true, flexHeight: 1);
            var checkBg = checklistCard.gameObject.AddComponent<Image>();
            checkBg.color = new Color(0.97f, 0.98f, 0.99f);
            UIHelper.SetImageRoundedSprite(checkBg, 20f);

            UIHelper.MakeButton("ReviewBtn", vBox, "REVIEW TRAINING", 28, ColorGrayButtonBg, ColorGrayButtonText, 16f);
            UIHelper.MakeButton("BackMachineryBtn", vBox, "BACK TO MACHINERY", 28, ColorTealPrimary, Color.white, 16f);
            UIHelper.MakeButton("MainMenuBtn", vBox, "MAIN MENU", 24, Color.clear, ColorTextMuted, 16f);

            SavePrefab(go, "TrainingCompletePanel.prefab");
        }

        private static void MakeFeatureCard(Transform parent, string icon, string title, string subtitle)
        {
            var card = UIHelper.MakeRect("FeatureCard", parent);
            UIHelper.SetLayout(card.gameObject, preferredHeight: 104);
            var bg = card.gameObject.AddComponent<Image>();
            bg.color = new Color(0.97f, 0.98f, 0.99f);
            UIHelper.SetImageRoundedSprite(bg, 16f);
            var hBox = UIHelper.MakeHorizontal("CardHBox", card, 20, new RectOffset(24, 24, 16, 16));
            UIHelper.Stretch(hBox);
            UIHelper.MakeLabel("Icon", hBox, icon, 38, ColorTealPrimary, TextAlignmentOptions.Center);
            var tBox = UIHelper.MakeVertical("TextBox", hBox, 4);
            UIHelper.SetLayout(tBox.gameObject, flexibleWidth: true, flexWidth: 1);
            UIHelper.MakeLabel("Title", tBox, title, 26, ColorTextNavy, bold: true);
            UIHelper.MakeLabel("Sub", tBox, subtitle, 20, ColorTextMuted);
        }

        private static void CreateCorner(Transform parent, string name, Vector2 anchor, Vector2 pivot, float len, float thick)
        {
            var cornerGO = new GameObject(name, typeof(RectTransform));
            cornerGO.transform.SetParent(parent, false);
            var rt = cornerGO.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = new Vector2(len, len);

            var hBarGO = new GameObject("HBar", typeof(RectTransform));
            hBarGO.transform.SetParent(rt, false);
            var hRT = hBarGO.GetComponent<RectTransform>();
            hRT.anchorMin = anchor;
            hRT.anchorMax = anchor;
            hRT.pivot = pivot;
            hRT.sizeDelta = new Vector2(len, thick);
            var hImg = hBarGO.AddComponent<Image>();
            hImg.color = ColorDetectionCorner;

            var vBarGO = new GameObject("VBar", typeof(RectTransform));
            vBarGO.transform.SetParent(rt, false);
            var vRT = vBarGO.GetComponent<RectTransform>();
            vRT.anchorMin = anchor;
            vRT.anchorMax = anchor;
            vRT.pivot = pivot;
            vRT.sizeDelta = new Vector2(thick, len);
            var vImg = vBarGO.AddComponent<Image>();
            vImg.color = ColorDetectionCorner;
        }
    }
}
#endif
