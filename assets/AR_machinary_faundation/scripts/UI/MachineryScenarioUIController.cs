using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using SurakshaAR.Data;
using SurakshaAR.UI;

namespace SurakshaAR.Machinery
{
    /// <summary>
    /// MachineryScenarioUIController
    /// =============================
    /// Complete industrial AR safety-training UI controller matching the reference storyboard:
    /// - Screen 1: Main Menu (White background, real machinery visual preview, 3 feature cards, Start AR Training button)
    /// - Screen 2: Scan & Detect Plane (Top instruction card, animated scanning spinner status)
    /// - Screen 3: Tap to Place (Top placement guidance, real world AR reticle indicator)
    /// - Screen 4: Machinery Placed ("Machinery Placed!", explore prompt, Start Training button)
    /// - Screen 5: Component Overview (7 AR Callouts attached to 3D parts + Component Info Cards)
    /// - 7 Guided Training Steps: Glowing AR 4-Corner Detection Frame tracking 3D targets + Step cards + Next/Prev
    /// - Training Complete Screen: 7-step verified checklist + Review & Navigation buttons
    /// 
    /// Follows the strict non-destructive rule: Zero changes to existing Machinery models, transforms, or scene hierarchy.
    /// </summary>
    [DisallowMultipleComponent]
    public class MachineryScenarioUIController : MonoBehaviour
    {
        public static MachineryScenarioUIController Instance { get; private set; }

        public enum MachineryUIState
        {
            MainMenu,          // Screen 1: Start Menu
            ARScanning,        // Screen 2: Scan floor
            TapToPlace,        // Screen 3: Tap on highlighted floor area
            MachineryPlaced,   // Screen 4: Placed confirmation & explore
            ComponentOverview, // Screen 5: 7 Component AR callouts
            TrainingStep,      // Steps 1 to 7: AR 4-corner detection frame + Guided procedure
            TrainingComplete,  // Complete screen with 7 checklist items

            // Backward-compatibility aliases for legacy scripts
            Intro = MainMenu,
            ReadyToPlace = TapToPlace,
            Inspection = TrainingStep
        }

        // ── Data Structures ──────────────────────────────────────────────────
        [System.Serializable]
        public struct ComponentInfoDef
        {
            public string id;
            public string targetGameObjectName;
            public string name;
            public string shortDesc;
            public string badgeCode;
            public Color badgeColor;
            public string whatIsIt;
            public string whyImportant;
            public string workerAction;
        }

        [System.Serializable]
        public struct GuidedStepDef
        {
            public int stepNumber; // 1 to 7
            public string targetGameObjectName;
            public string targetBadge;
            public string stepTitle;
            public Color badgeColor;
            public string instruction;
            public string guidance;
            public string safetyNote;
        }

        // ── Color Theme (Clean White & Industrial Teal/Navy) ─────────────────
        private static readonly Color ColorWhiteCard       = new Color(1f, 1f, 1f, 0.98f);
        private static readonly Color ColorTextNavy        = new Color(0.06f, 0.09f, 0.16f); // #0F172A
        private static readonly Color ColorTextMuted       = new Color(0.38f, 0.44f, 0.52f); // #64748B
        private static readonly Color ColorTealPrimary     = new Color(0.05f, 0.49f, 0.40f); // #0D7C66 (Industrial Teal/Green)
        private static readonly Color ColorTealHover       = new Color(0.04f, 0.40f, 0.33f);
        private static readonly Color ColorPillGreenBg     = new Color(0.92f, 0.98f, 0.95f); // #ECFDF5
        private static readonly Color ColorPillGreenText   = new Color(0.02f, 0.37f, 0.27f); // #065F46
        private static readonly Color ColorHazardRed       = new Color(0.86f, 0.15f, 0.15f); // #DC2626
        private static readonly Color ColorHazardRedBg     = new Color(0.99f, 0.93f, 0.93f); // #FEF2F2
        private static readonly Color ColorGrayButtonBg    = new Color(0.95f, 0.96f, 0.98f); // #F1F5F9
        private static readonly Color ColorGrayButtonText  = new Color(0.20f, 0.25f, 0.33f); // #334155
        private static readonly Color ColorDetectionCorner = new Color(0.06f, 0.85f, 0.60f); // Glowing Teal/Green

        // ── Component Definitions (Screen 5) ─────────────────────────────────
        public static readonly ComponentInfoDef[] ComponentDefs = new ComponentInfoDef[]
        {
            new ComponentInfoDef
            {
                id = "hopper",
                targetGameObjectName = "FeedHopper",
                name = "FEED HOPPER",
                shortDesc = "Feeds material onto the conveyor",
                badgeCode = "01",
                badgeColor = ColorTealPrimary,
                whatIsIt = "Feeds material onto the conveyor belt.",
                whyImportant = "Hoppers direct bulk material onto the belt and present severe entrapment and falling hazards.",
                workerAction = "Never reach into or climb inside the feed hopper while the machine is energized."
            },
            new ComponentInfoDef
            {
                id = "conveyor",
                targetGameObjectName = "ConveyorAssembly",
                name = "CONVEYOR BELT",
                shortDesc = "Moves material from one point to another",
                badgeCode = "02",
                badgeColor = ColorTealPrimary,
                whatIsIt = "Moves material from one point to another.",
                whyImportant = "Moving pinch points between belt and rollers can pull in loose clothing, tools, or limbs.",
                workerAction = "Ensure all fixed protective guards are in place. Never touch or straddle a moving belt."
            },
            new ComponentInfoDef
            {
                id = "jam",
                targetGameObjectName = "JamArea",
                name = "JAM AREA",
                shortDesc = "Material blockage / conveyor jam hazard",
                badgeCode = "!",
                badgeColor = ColorHazardRed,
                whatIsIt = "Material blockage / conveyor jam hazard zone.",
                whyImportant = "Mechanical jams store immense tension and spring energy across the belt and drive system.",
                workerAction = "Do not attempt to clear the blockage while the machine is running or energized."
            },
            new ComponentInfoDef
            {
                id = "motor",
                targetGameObjectName = "DriveMotor",
                name = "DRIVE MOTOR",
                shortDesc = "Powers the conveyor",
                badgeCode = "M",
                badgeColor = ColorTealPrimary,
                whatIsIt = "Powers the conveyor belt.",
                whyImportant = "The motor drives the conveyor system and must be safely isolated before maintenance.",
                workerAction = "Do not approach or work on the machine while it is running."
            },
            new ComponentInfoDef
            {
                id = "estop",
                targetGameObjectName = "EmergencyStop",
                name = "EMERGENCY STOP",
                shortDesc = "Stops the machine during an emergency",
                badgeCode = "STOP",
                badgeColor = ColorHazardRed,
                whatIsIt = "Stops the machine during an emergency.",
                whyImportant = "Immediately halts conveyor movement to prevent personnel injury or catastrophic failure.",
                workerAction = "Press firmly during an emergency or abnormal condition before initiating isolation."
            },
            new ComponentInfoDef
            {
                id = "isolator",
                targetGameObjectName = "MainIsolator",
                name = "MAIN ISOLATOR",
                shortDesc = "Isolates the machine power supply",
                badgeCode = "ISO",
                badgeColor = ColorTealPrimary,
                whatIsIt = "Isolates the machine power supply.",
                whyImportant = "Power must be isolated before maintenance or jam clearance.",
                workerAction = "Switch OFF the power supply and secure the handle with your lockout padlock."
            },
            new ComponentInfoDef
            {
                id = "loto",
                targetGameObjectName = "SteelWorkstationTable",
                name = "LOTO STATION",
                shortDesc = "Contains lockout/tagout equipment",
                badgeCode = "LOTO",
                badgeColor = ColorTealPrimary,
                whatIsIt = "Contains lockout/tagout equipment.",
                whyImportant = "Enables positive Lockout/Tagout so no unauthorized person can re-energize the machine.",
                workerAction = "Retrieve appropriate lockout equipment (hasp, padlock, and danger warning tag)."
            }
        };

        // ── 7 Guided Training Steps ───────────────────────────────────────────
        public static readonly GuidedStepDef[] GuidedSteps = new GuidedStepDef[]
        {
            new GuidedStepDef
            {
                stepNumber = 1,
                targetGameObjectName = "JamArea",
                targetBadge = "Jam Area",
                stepTitle = "Identify the Hazard",
                badgeColor = ColorHazardRed,
                instruction = "Look at the conveyor belt and identify the material blockage.",
                guidance = "Move closer and inspect the highlighted jam area.",
                safetyNote = "Do not attempt to clear the blockage while the machine is running."
            },
            new GuidedStepDef
            {
                stepNumber = 2,
                targetGameObjectName = "EmergencyStop",
                targetBadge = "Emergency Stop",
                stepTitle = "Stop the Machine",
                badgeColor = ColorHazardRed,
                instruction = "Locate the Emergency Stop and use it to stop the machine in an emergency.",
                guidance = "Move closer to the highlighted Emergency Stop.",
                safetyNote = "Stop the machine before proceeding with the isolation procedure."
            },
            new GuidedStepDef
            {
                stepNumber = 3,
                targetGameObjectName = "MainIsolator",
                targetBadge = "Main Isolator",
                stepTitle = "Isolate Power",
                badgeColor = ColorTealPrimary,
                instruction = "Locate the Main Isolator and switch OFF the power supply before intervention.",
                guidance = "Move closer to the highlighted Main Isolator.",
                safetyNote = "Power must be isolated before maintenance or jam clearance."
            },
            new GuidedStepDef
            {
                stepNumber = 4,
                targetGameObjectName = "SteelWorkstationTable",
                targetBadge = "LOTO Station",
                stepTitle = "Apply LOTO",
                badgeColor = ColorTealPrimary,
                instruction = "Use the appropriate lockout equipment (hasp, padlock and tag) to secure the isolation point.",
                guidance = "Move closer to the highlighted LOTO equipment.",
                safetyNote = "Prevent accidental re-energization during maintenance."
            },
            new GuidedStepDef
            {
                stepNumber = 5,
                targetGameObjectName = "DriveMotor",
                targetBadge = "Drive Motor",
                stepTitle = "Verify Safe State",
                badgeColor = ColorTealPrimary,
                instruction = "Ensure the machine is in a safe, zero-energy state before clearing the jam.",
                guidance = "Move closer and check the highlighted machine area.",
                safetyNote = "Do not begin maintenance until safe isolation has been verified."
            },
            new GuidedStepDef
            {
                stepNumber = 6,
                targetGameObjectName = "JamArea",
                targetBadge = "Jam Area",
                stepTitle = "Clear the Jam",
                badgeColor = ColorHazardRed,
                instruction = "After proper isolation, clear the material blockage using safe tools.",
                guidance = "Move closer to the highlighted jam area.",
                safetyNote = "Never clear a blockage while the machine is energized."
            },
            new GuidedStepDef
            {
                stepNumber = 7,
                targetGameObjectName = "MainIsolator",
                targetBadge = "Main Isolator",
                stepTitle = "Restore Operation",
                badgeColor = ColorTealPrimary,
                instruction = "After maintenance, remove LOTO and restore operation as per standard procedure.",
                guidance = "Move closer to the highlighted isolation point.",
                safetyNote = "Ensure the area is clear before restoring operation."
            }
        };

        // ── Current State ────────────────────────────────────────────────────
        public MachineryUIState CurrentState { get; private set; } = MachineryUIState.MainMenu;
        public int CurrentStepIndex { get; private set; } = 0; // 0 to 6 for Steps 1 to 7

        // ── Hierarchy References ─────────────────────────────────────────────
        private bool _isInitialized = false;
        private Canvas _canvas;
        private CanvasScaler _scaler;
        private RectTransform _safeArea;
        private Camera _mainCamera;

        // Screen 1: Main Menu
        private RectTransform _mainMenuPanel;

        // Screen 2: AR Scanning
        private RectTransform _scanningTopCard;
        private RectTransform _scanningBottomCard;
        private RectTransform _spinnerTransform;

        // Screen 3: Tap to Place
        private RectTransform _tapToPlaceTopCard;

        // Screen 4: Machinery Placed
        private RectTransform _placedTopCard;
        private RectTransform _placedBottomCard;

        // Screen 5: Component Overview
        private RectTransform _overviewTopCard;
        private RectTransform _overviewBottomCard;
        private RectTransform _calloutsContainer;
        private List<ComponentCalloutUI> _activeCallouts = new List<ComponentCalloutUI>();

        // Component Information Card (Modal)
        private RectTransform _componentInfoModal;
        private TextMeshProUGUI _modalTitleText;
        private TextMeshProUGUI _modalWhatText;
        private TextMeshProUGUI _modalWhyText;
        private TextMeshProUGUI _modalActionText;

        // 7-Step Training HUD & Step Card
        private RectTransform _trainingTopHUD;
        private TextMeshProUGUI _hudStepText;
        private TextMeshProUGUI _hudSubText;
        private List<Image> _stepIndicatorDots = new List<Image>();
        private List<TextMeshProUGUI> _stepIndicatorLabels = new List<TextMeshProUGUI>();

        private RectTransform _stepCardBottom;
        private Image _stepCardBadgeImg;
        private TextMeshProUGUI _stepCardBadgeText;
        private TextMeshProUGUI _stepCardTitle;
        private TextMeshProUGUI _stepCardInstruction;
        private TextMeshProUGUI _stepCardGuidance;
        private TextMeshProUGUI _stepCardSafety;
        private RectTransform _lotoEquipmentBadge;
        private Button _btnPreviousStep;
        private Button _btnNextStep;
        private TextMeshProUGUI _btnNextStepText;

        // AR Detection Frame
        private RectTransform _detectionContainer;
        private RectTransform _detectionBox;
        private Image _detectionTargetDot;
        private TextMeshProUGUI _detectionTargetBadge;
        private RectTransform _offScreenGuidanceBadge;
        private TextMeshProUGUI _offScreenGuidanceText;

        // Training Complete Screen
        private RectTransform _trainingCompletePanel;

        // ── Callout Helper Struct ────────────────────────────────────────────
        private class ComponentCalloutUI
        {
            public RectTransform root;
            public Transform target3D;
            public ComponentInfoDef def;
            public Button button;
        }

        // ── Unity Lifecycle ──────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            BuildMasterUI();
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            SetUIState(MachineryUIState.MainMenu);
        }

        private void Update()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;

            // Animate scanning spinner
            if (CurrentState == MachineryUIState.ARScanning && _spinnerTransform != null)
            {
                _spinnerTransform.Rotate(0, 0, -240f * Time.deltaTime);
            }

            // Handle AR Scanning / Placement transitions
            if (CurrentState == MachineryUIState.ARScanning)
            {
                var pc = MachineryARPlacementController.Instance;
                if (pc != null && pc.HasValidPose && !pc.IsPlaced)
                {
                    SetUIState(MachineryUIState.TapToPlace);
                }
            }
            else if (CurrentState == MachineryUIState.TapToPlace)
            {
                var pc = MachineryARPlacementController.Instance;
                if (pc != null && !pc.HasValidPose && !pc.IsPlaced)
                {
                    SetUIState(MachineryUIState.ARScanning);
                }
            }

            // Update AR Component Callouts (Screen 5)
            if (CurrentState == MachineryUIState.ComponentOverview)
            {
                UpdateComponentCallouts();
            }

            // Update AR 4-Corner Detection Frame (Training Steps 1-7)
            if (CurrentState == MachineryUIState.TrainingStep)
            {
                UpdateDetectionFrame();
            }
        }

        // =====================================================================
        //  MASTER UI BUILDER (PURE uGUI)
        // =====================================================================
        private void BuildMasterUI()
        {
            // Ensure Canvas on this GameObject
            _canvas = GetComponent<Canvas>();
            if (_canvas == null) _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 50;

            _scaler = GetComponent<CanvasScaler>();
            if (_scaler == null) _scaler = gameObject.AddComponent<CanvasScaler>();
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = new Vector2(1080, 2400); // Standard mobile portrait resolution
            _scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            _scaler.matchWidthOrHeight = 0.5f;

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            // Safe Area
            var safeGO = new GameObject("SafeArea", typeof(RectTransform));
            safeGO.transform.SetParent(transform, false);
            _safeArea = safeGO.GetComponent<RectTransform>();
            _safeArea.anchorMin = Vector2.zero;
            _safeArea.anchorMax = Vector2.one;
            _safeArea.offsetMin = new Vector2(32, 48);
            _safeArea.offsetMax = new Vector2(-32, -48);

            // Build All Screens
            BuildScreen1_MainMenu();
            BuildScreen2_ARScanning();
            BuildScreen3_TapToPlace();
            BuildScreen4_MachineryPlaced();
            BuildScreen5_ComponentOverview();
            BuildComponentInfoModal();
            BuildTrainingStepUI();
            BuildARDetectionFrame();
            BuildTrainingCompleteScreen();
        }

        // =====================================================================
        //  SCREEN 1 — MAIN MENU
        // =====================================================================
        private void BuildScreen1_MainMenu()
        {
            _mainMenuPanel = UIHelper.MakeRect("Screen1_MainMenu", _safeArea);
            UIHelper.Stretch(_mainMenuPanel);

            // Clean Pure White Background Card
            var bg = _mainMenuPanel.gameObject.AddComponent<Image>();
            bg.color = Color.white;
            UIHelper.SetImageRoundedSprite(bg, 32f);

            var vBox = UIHelper.MakeVertical("MenuContent", _mainMenuPanel, 18, new RectOffset(40, 40, 36, 36));
            UIHelper.Stretch(vBox);

            // Top Header Row with back arrow, titles, settings gear
            var hHeader = UIHelper.MakeHorizontal("HeaderRow", vBox, 16);
            UIHelper.SetLayout(hHeader.gameObject, preferredHeight: 100);

            var btnBack = UIHelper.MakeButton("BackBtn", hHeader, "<", 30, ColorGrayButtonBg, ColorTextNavy, 16f);
            UIHelper.SetLayout(btnBack.gameObject, preferredWidth: 70, preferredHeight: 70);

            var titleBox = UIHelper.MakeVertical("TitleBox", hHeader, 2);
            UIHelper.SetLayout(titleBox.gameObject, flexibleWidth: true, flexWidth: 1);
            UIHelper.MakeLabel("BrandLabel", titleBox, "SurakshaAR", 24, ColorTextMuted, bold: true);
            UIHelper.MakeLabel("ModuleLabel", titleBox, "Machinery AR", 44, ColorTextNavy, bold: true);
            UIHelper.MakeLabel("SubLabel", titleBox, "Safety Training", 24, ColorTealPrimary, bold: true);

            var btnSettings = UIHelper.MakeButton("SettingsBtn", hHeader, "", 24, ColorGrayButtonBg, ColorTextNavy, 16f);
            UIHelper.SetLayout(btnSettings.gameObject, preferredWidth: 70, preferredHeight: 70);
            var setRT = btnSettings.GetComponent<RectTransform>();
            AddSettingsIconGraphic(setRT, ColorTextNavy, 28f);

            // Preview Illustration Container
            var previewBox = UIHelper.MakeRect("PreviewContainer", vBox);
            UIHelper.SetLayout(previewBox.gameObject, preferredHeight: 460);
            var prevBg = previewBox.gameObject.AddComponent<Image>();
            prevBg.color = new Color(0.96f, 0.98f, 0.99f);
            UIHelper.SetImageRoundedSprite(prevBg, 24f);

            Sprite previewSprite = LoadPreviewSprite();
            if (previewSprite != null)
            {
                var imgGO = new GameObject("PreviewImg", typeof(RectTransform));
                imgGO.transform.SetParent(previewBox, false);
                var imgRT = imgGO.GetComponent<RectTransform>();
                UIHelper.Stretch(imgRT);
                imgRT.offsetMin = new Vector2(16, 16);
                imgRT.offsetMax = new Vector2(-16, -16);
                var pImg = imgGO.AddComponent<Image>();
                pImg.sprite = previewSprite;
                pImg.preserveAspect = true;
            }
            else
            {
                var prevVBox = UIHelper.MakeVertical("PrevLayout", previewBox, 12, new RectOffset(24, 24, 30, 24));
                UIHelper.Stretch(prevVBox);
                UIHelper.MakeLabel("PrevIcon", prevVBox, "🏭", 90, ColorTealPrimary, TextAlignmentOptions.Center);
                UIHelper.MakeLabel("PrevTitle", prevVBox, "Heavy Coal Conveyor System", 32, ColorTextNavy, TextAlignmentOptions.Center, bold: true);
                UIHelper.MakeLabel("PrevDesc", prevVBox, "Conveyor Jam • Safe Isolation • Lockout / Tagout (LOTO)", 22, ColorTextMuted, TextAlignmentOptions.Center);
            }

            // 3 Feature Bullet Cards
            var featList = UIHelper.MakeVertical("FeatureList", vBox, 14);
            UIHelper.SetLayout(featList.gameObject, flexibleHeight: true, flexHeight: 1);

            MakeFeatureCard(featList, "01", ColorTealPrimary, "Learn machine components", "Understand the purpose of each major component.");
            MakeFeatureCard(featList, "02", new Color(0.85f, 0.45f, 0.08f), "Understand safety risks", "Identify hazards and critical safety controls.");
            MakeFeatureCard(featList, "03", new Color(0.10f, 0.50f, 0.75f), "Follow correct procedure", "Learn the correct conveyor-jam safety sequence.");

            // Primary Start Button
            var btnStart = UIHelper.MakeButton("StartARTrainingBtn", vBox, "Start AR Training  →", 32, ColorTealPrimary, Color.white, 20f);
            UIHelper.SetLayout(btnStart.gameObject, preferredHeight: 90);
            btnStart.onClick.AddListener(() =>
            {
                SetUIState(MachineryUIState.ARScanning);
                MachineryARPlacementController.Instance?.StartScanning();
            });

            // Small footer options
            var hFooter = UIHelper.MakeHorizontal("FooterLinks", vBox, 24);
            UIHelper.SetLayout(hFooter.gameObject, preferredHeight: 40);
            var f1 = UIHelper.MakeLabel("Help", hFooter, "Help", 20, ColorTextMuted, TextAlignmentOptions.Center);
            var f2 = UIHelper.MakeLabel("Guide", hFooter, "|  Guide  |", 20, ColorTextMuted, TextAlignmentOptions.Center);
            var f3 = UIHelper.MakeLabel("Settings", hFooter, "Settings", 20, ColorTextMuted, TextAlignmentOptions.Center);
            UIHelper.Stretch(f1.rectTransform);
            UIHelper.Stretch(f2.rectTransform);
            UIHelper.Stretch(f3.rectTransform);
        }

        private Sprite LoadPreviewSprite()
        {
            try
            {
                var rSprite = Resources.Load<Sprite>("MachineryConveyorPreview");
                if (rSprite != null) return rSprite;

                string p = Path.Combine(Application.dataPath, "AR_machinary_faundation/textures/MachineryConveyorPreview.png");
                if (File.Exists(p))
                {
                    byte[] bytes = File.ReadAllBytes(p);
                    Texture2D tex = new Texture2D(2, 2);
                    if (tex.LoadImage(bytes))
                    {
                        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[MachineryScenarioUIController] Preview texture notice: " + ex.Message);
            }
            return null;
        }

        private void MakeFeatureCard(Transform parent, string badgeNumber, Color badgeColor, string title, string subtitle)
        {
            var card = UIHelper.MakeRect("FeatureCard", parent);
            UIHelper.SetLayout(card.gameObject, preferredHeight: 104);
            var bg = card.gameObject.AddComponent<Image>();
            bg.color = new Color(0.97f, 0.98f, 0.99f);
            UIHelper.SetImageRoundedSprite(bg, 16f);

            var hBox = UIHelper.MakeHorizontal("CardHBox", card, 20, new RectOffset(20, 20, 16, 16));
            UIHelper.Stretch(hBox);

            var badgeGO = new GameObject("IconBadge", typeof(RectTransform));
            badgeGO.transform.SetParent(hBox, false);
            var badgeRT = badgeGO.GetComponent<RectTransform>();
            badgeRT.sizeDelta = new Vector2(52, 52);
            var badgeImg = badgeGO.AddComponent<Image>();
            badgeImg.color = badgeColor;
            UIHelper.SetImageRoundedSprite(badgeImg, 26f);

            var badgeLbl = UIHelper.MakeLabel("Lbl", badgeRT, badgeNumber, 22, Color.white, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(badgeLbl.rectTransform);

            var tBox = UIHelper.MakeVertical("TextBox", hBox, 4);
            UIHelper.SetLayout(tBox.gameObject, flexibleWidth: true, flexWidth: 1);
            UIHelper.MakeLabel("Title", tBox, title, 26, ColorTextNavy, bold: true);
            UIHelper.MakeLabel("Sub", tBox, subtitle, 20, ColorTextMuted);
        }

        private static void AddCheckmarkGraphic(RectTransform parent, Color color, float size = 32f)
        {
            var go = new GameObject("CheckmarkGraphic", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = Vector2.zero;

            // Short stem
            var shortStem = new GameObject("ShortStem", typeof(RectTransform));
            shortStem.transform.SetParent(rt, false);
            var sRT = shortStem.GetComponent<RectTransform>();
            sRT.sizeDelta = new Vector2(size * 0.16f, size * 0.44f);
            sRT.anchoredPosition = new Vector2(-size * 0.18f, -size * 0.06f);
            sRT.localRotation = Quaternion.Euler(0, 0, -45f);
            var sImg = shortStem.AddComponent<Image>();
            sImg.color = color;
            sImg.raycastTarget = false;

            // Long stem
            var longStem = new GameObject("LongStem", typeof(RectTransform));
            longStem.transform.SetParent(rt, false);
            var lRT = longStem.GetComponent<RectTransform>();
            lRT.sizeDelta = new Vector2(size * 0.16f, size * 0.85f);
            lRT.anchoredPosition = new Vector2(size * 0.14f, size * 0.08f);
            lRT.localRotation = Quaternion.Euler(0, 0, 45f);
            var lImg = longStem.AddComponent<Image>();
            lImg.color = color;
            lImg.raycastTarget = false;
        }

        private static void AddSettingsIconGraphic(RectTransform parent, Color color, float size = 28f)
        {
            var go = new GameObject("SettingsIconGraphic", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = Vector2.zero;

            // 3 horizontal bars
            float barH = size * 0.16f;
            float w = size * 0.85f;

            for (int i = -1; i <= 1; i++)
            {
                var bar = new GameObject($"Bar_{i}", typeof(RectTransform));
                bar.transform.SetParent(rt, false);
                var bRT = bar.GetComponent<RectTransform>();
                bRT.sizeDelta = new Vector2(w, barH);
                bRT.anchoredPosition = new Vector2(0, i * size * 0.30f);
                var bImg = bar.AddComponent<Image>();
                bImg.color = color;
                bImg.raycastTarget = false;
                UIHelper.SetImageRoundedSprite(bImg, barH * 0.5f);
            }
        }

        // =====================================================================
        //  SCREEN 2 — SCAN & DETECT PLANE
        // =====================================================================
        private void BuildScreen2_ARScanning()
        {
            // Top Floating Card
            _scanningTopCard = UIHelper.MakeRect("Screen2_ScanningTop", _safeArea);
            UIHelper.AnchorTopStretch(_scanningTopCard, 170, 0);
            var bgTop = _scanningTopCard.gameObject.AddComponent<Image>();
            bgTop.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(bgTop, 24f);

            var hTop = UIHelper.MakeHorizontal("HTop", _scanningTopCard, 14, new RectOffset(20, 24, 20, 20));
            UIHelper.Stretch(hTop);

            var btnBack = UIHelper.MakeButton("BackBtn", hTop, "<", 28, ColorGrayButtonBg, ColorTextNavy, 14f);
            UIHelper.SetLayout(btnBack.gameObject, preferredWidth: 54, preferredHeight: 54);
            btnBack.onClick.AddListener(() => SetUIState(MachineryUIState.MainMenu));

            var vBoxTop = UIHelper.MakeVertical("TopContent", hTop, 4);
            UIHelper.SetLayout(vBoxTop.gameObject, flexibleWidth: true, flexWidth: 1);
            UIHelper.MakeLabel("Title", vBoxTop, "Scan the floor to place the machinery", 28, ColorTextNavy, bold: true);
            UIHelper.MakeLabel("Sub", vBoxTop, "Move your phone slowly until a suitable horizontal surface is detected.", 20, ColorTextMuted);

            // Bottom Floating Card
            _scanningBottomCard = UIHelper.MakeRect("Screen2_ScanningBottom", _safeArea);
            UIHelper.AnchorBottomStretch(_scanningBottomCard, 110, 50);
            var bgBot = _scanningBottomCard.gameObject.AddComponent<Image>();
            bgBot.color = ColorWhiteCard;
            bgBot.raycastTarget = false;
            UIHelper.SetImageRoundedSprite(bgBot, 24f);

            var hBoxBot = UIHelper.MakeHorizontal("BotContent", _scanningBottomCard, 16, new RectOffset(32, 32, 16, 16));
            UIHelper.Stretch(hBoxBot);

            // Rotating vector spinner
            var spinGO = new GameObject("SpinGraphic", typeof(RectTransform));
            spinGO.transform.SetParent(hBoxBot, false);
            _spinnerTransform = spinGO.GetComponent<RectTransform>();
            _spinnerTransform.sizeDelta = new Vector2(40, 40);
            var spinImg = spinGO.AddComponent<Image>();
            spinImg.color = ColorTealPrimary;
            UIHelper.SetImageRoundedSprite(spinImg, 20f);

            var innerGO = new GameObject("InnerHole", typeof(RectTransform));
            innerGO.transform.SetParent(_spinnerTransform, false);
            var innerRT = innerGO.GetComponent<RectTransform>();
            innerRT.sizeDelta = new Vector2(26, 26);
            var innerImg = innerGO.AddComponent<Image>();
            innerImg.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(innerImg, 13f);

            var dotGO = new GameObject("SpinDot", typeof(RectTransform));
            dotGO.transform.SetParent(_spinnerTransform, false);
            var dotRT = dotGO.GetComponent<RectTransform>();
            dotRT.sizeDelta = new Vector2(10, 10);
            dotRT.anchoredPosition = new Vector2(0, 14);
            var dotImg = dotGO.AddComponent<Image>();
            dotImg.color = Color.white;
            UIHelper.SetImageRoundedSprite(dotImg, 5f);

            var scanText = UIHelper.MakeLabel("ScanText", hBoxBot, "Scanning for surface...", 26, ColorTextNavy, bold: true);
            UIHelper.SetLayout(scanText.gameObject, flexibleWidth: true, flexWidth: 1);
        }

        // =====================================================================
        //  SCREEN 3 — TAP TO PLACE
        // =====================================================================
        private void BuildScreen3_TapToPlace()
        {
            _tapToPlaceTopCard = UIHelper.MakeRect("Screen3_TapToPlaceTop", _safeArea);
            UIHelper.AnchorTopStretch(_tapToPlaceTopCard, 160, 0);
            var bg = _tapToPlaceTopCard.gameObject.AddComponent<Image>();
            bg.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(bg, 24f);

            var hTop = UIHelper.MakeHorizontal("HTop", _tapToPlaceTopCard, 14, new RectOffset(20, 24, 20, 20));
            UIHelper.Stretch(hTop);

            var btnBack = UIHelper.MakeButton("BackBtn", hTop, "<", 28, ColorGrayButtonBg, ColorTextNavy, 14f);
            UIHelper.SetLayout(btnBack.gameObject, preferredWidth: 54, preferredHeight: 54);
            btnBack.onClick.AddListener(() => SetUIState(MachineryUIState.MainMenu));

            var vBox = UIHelper.MakeVertical("Content", hTop, 4);
            UIHelper.SetLayout(vBox.gameObject, flexibleWidth: true, flexWidth: 1);
            UIHelper.MakeLabel("Title", vBox, "Tap on the highlighted area to place the machinery", 28, ColorTextNavy, bold: true);
            UIHelper.MakeLabel("Sub", vBox, "The blue placement indicator tracks the detected floor.", 20, ColorTealPrimary);
        }

        // =====================================================================
        //  SCREEN 4 — MACHINERY PLACED
        // =====================================================================
        private void BuildScreen4_MachineryPlaced()
        {
            // Top Confirmation Card
            _placedTopCard = UIHelper.MakeRect("Screen4_PlacedTop", _safeArea);
            UIHelper.AnchorTopStretch(_placedTopCard, 170, 0);
            var bgTop = _placedTopCard.gameObject.AddComponent<Image>();
            bgTop.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(bgTop, 24f);

            var hTop = UIHelper.MakeHorizontal("HTop", _placedTopCard, 14, new RectOffset(20, 24, 20, 20));
            UIHelper.Stretch(hTop);

            var btnBack = UIHelper.MakeButton("BackBtn", hTop, "<", 28, ColorGrayButtonBg, ColorTextNavy, 14f);
            UIHelper.SetLayout(btnBack.gameObject, preferredWidth: 54, preferredHeight: 54);
            btnBack.onClick.AddListener(() => SetUIState(MachineryUIState.MainMenu));

            var vBoxTop = UIHelper.MakeVertical("TopContent", hTop, 6);
            UIHelper.SetLayout(vBoxTop.gameObject, flexibleWidth: true, flexWidth: 1);

            var hCheck = UIHelper.MakeHorizontal("CheckRow", vBoxTop, 12);
            UIHelper.SetLayout(hCheck.gameObject, preferredHeight: 40);

            var checkCircle = UIHelper.MakeRect("CheckCircle", hCheck);
            UIHelper.SetLayout(checkCircle.gameObject, preferredWidth: 36, preferredHeight: 36);
            var cImg = checkCircle.gameObject.AddComponent<Image>();
            cImg.color = ColorPillGreenBg;
            UIHelper.SetImageRoundedSprite(cImg, 18f);
            AddCheckmarkGraphic(checkCircle, ColorTealPrimary, 24f);

            var titleLbl = UIHelper.MakeLabel("PlacedTitle", hCheck, "Machinery Placed!", 28, ColorTextNavy, bold: true);
            UIHelper.SetLayout(titleLbl.gameObject, flexibleWidth: true, flexWidth: 1);

            UIHelper.MakeLabel("PlacedSub", vBoxTop, "You can now explore the components. Tap on any part to learn more.", 19, ColorTextMuted);

            // Bottom Action Card
            _placedBottomCard = UIHelper.MakeRect("Screen4_PlacedBottom", _safeArea);
            UIHelper.AnchorBottomStretch(_placedBottomCard, 220, 30);
            var bgBot = _placedBottomCard.gameObject.AddComponent<Image>();
            bgBot.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(bgBot, 24f);

            var vBoxBot = UIHelper.MakeVertical("BotContent", _placedBottomCard, 16, new RectOffset(32, 32, 24, 24));
            UIHelper.Stretch(vBoxBot);

            var hInfo = UIHelper.MakeHorizontal("InfoRow", vBoxBot, 12);
            UIHelper.SetLayout(hInfo.gameObject, preferredHeight: 38);

            var infoCircle = UIHelper.MakeRect("InfoCircle", hInfo);
            UIHelper.SetLayout(infoCircle.gameObject, preferredWidth: 32, preferredHeight: 32);
            var iImg = infoCircle.gameObject.AddComponent<Image>();
            iImg.color = ColorPillGreenBg;
            UIHelper.SetImageRoundedSprite(iImg, 16f);
            var iLbl = UIHelper.MakeLabel("InfoI", infoCircle, "i", 20, ColorTealPrimary, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(iLbl.rectTransform);

            var infoLbl = UIHelper.MakeLabel("InfoText", hInfo, "Walk around the machinery to view from different angles.", 22, ColorTextNavy);
            UIHelper.SetLayout(infoLbl.gameObject, flexibleWidth: true, flexWidth: 1);

            var btnStartTrain = UIHelper.MakeButton("StartTrainBtn", vBoxBot, "Start Training  →", 28, ColorTealPrimary, Color.white, 16f);
            UIHelper.SetLayout(btnStartTrain.gameObject, preferredHeight: 80);
            btnStartTrain.onClick.AddListener(() =>
            {
                SetUIState(MachineryUIState.ComponentOverview);
            });
        }

        // =====================================================================
        //  SCREEN 5 — COMPONENT OVERVIEW
        // =====================================================================
        private void BuildScreen5_ComponentOverview()
        {
            // Top Overview Card
            _overviewTopCard = UIHelper.MakeRect("Screen5_OverviewTop", _safeArea);
            UIHelper.AnchorTopStretch(_overviewTopCard, 140, 0);
            var bgTop = _overviewTopCard.gameObject.AddComponent<Image>();
            bgTop.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(bgTop, 24f);

            var hTop = UIHelper.MakeHorizontal("HTop", _overviewTopCard, 14, new RectOffset(20, 24, 20, 20));
            UIHelper.Stretch(hTop);

            var btnBack = UIHelper.MakeButton("BackBtn", hTop, "<", 28, ColorGrayButtonBg, ColorTextNavy, 14f);
            UIHelper.SetLayout(btnBack.gameObject, preferredWidth: 54, preferredHeight: 54);
            btnBack.onClick.AddListener(() => SetUIState(MachineryUIState.MachineryPlaced));

            var vBoxTop = UIHelper.MakeVertical("TopContent", hTop, 4);
            UIHelper.SetLayout(vBoxTop.gameObject, flexibleWidth: true, flexWidth: 1);
            UIHelper.MakeLabel("Title", vBoxTop, "MACHINERY OVERVIEW", 28, ColorTextNavy, bold: true);
            UIHelper.MakeLabel("Sub", vBoxTop, "Tap any component to learn its purpose.", 20, ColorTealPrimary);

            // Container for dynamic 3D-tracked AR callouts
            var calloutsGO = new GameObject("ARCalloutsContainer", typeof(RectTransform));
            calloutsGO.transform.SetParent(_safeArea, false);
            _calloutsContainer = calloutsGO.GetComponent<RectTransform>();
            UIHelper.Stretch(_calloutsContainer);

            // Bottom Prompt & Start Button
            _overviewBottomCard = UIHelper.MakeRect("Screen5_OverviewBottom", _safeArea);
            UIHelper.AnchorBottomStretch(_overviewBottomCard, 190, 20);
            var bgBot = _overviewBottomCard.gameObject.AddComponent<Image>();
            bgBot.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(bgBot, 24f);

            var vBoxBot = UIHelper.MakeVertical("BotContent", _overviewBottomCard, 12, new RectOffset(32, 32, 20, 20));
            UIHelper.Stretch(vBoxBot);

            var hInfo = UIHelper.MakeHorizontal("InfoRow", vBoxBot, 12);
            UIHelper.SetLayout(hInfo.gameObject, preferredHeight: 36);

            var infoBadge = UIHelper.MakeRect("InfoBadge", hInfo);
            UIHelper.SetLayout(infoBadge.gameObject, preferredWidth: 32, preferredHeight: 32);
            var bImg = infoBadge.gameObject.AddComponent<Image>();
            bImg.color = ColorPillGreenBg;
            UIHelper.SetImageRoundedSprite(bImg, 16f);
            var bLbl = UIHelper.MakeLabel("InfoI", infoBadge, "i", 20, ColorTealPrimary, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(bLbl.rectTransform);

            var promptLbl = UIHelper.MakeLabel("PromptText", hInfo, "Tap on any component to learn its purpose", 22, ColorTextNavy);
            UIHelper.SetLayout(promptLbl.gameObject, flexibleWidth: true, flexWidth: 1);

            var btnStartTraining = UIHelper.MakeButton("StartTrainingBtn", vBoxBot, "Start Training  →", 28, ColorTealPrimary, Color.white, 16f);
            UIHelper.SetLayout(btnStartTraining.gameObject, preferredHeight: 76);
            btnStartTraining.onClick.AddListener(() =>
            {
                CurrentStepIndex = 0;
                SetUIState(MachineryUIState.TrainingStep);
            });
        }

        private void InitializeComponentCallouts()
        {
            // Clear existing callouts
            foreach (var c in _activeCallouts)
            {
                if (c.root != null) Destroy(c.root.gameObject);
            }
            _activeCallouts.Clear();

            foreach (var def in ComponentDefs)
            {
                Transform targetT = FindMachineryPart(def.targetGameObjectName);
                if (targetT == null) continue;

                var calloutRT = UIHelper.MakeRect($"Callout_{def.id}", _calloutsContainer);
                calloutRT.sizeDelta = new Vector2(300, 72);
                calloutRT.pivot = new Vector2(0.5f, 0f);

                var img = calloutRT.gameObject.AddComponent<Image>();
                img.color = ColorWhiteCard;
                UIHelper.SetImageRoundedSprite(img, 14f);

                var btn = calloutRT.gameObject.AddComponent<Button>();
                var defCopy = def;
                btn.onClick.AddListener(() => ShowComponentInfoModal(defCopy));

                var hBox = UIHelper.MakeHorizontal("HBox", calloutRT, 10, new RectOffset(12, 12, 8, 8));
                UIHelper.Stretch(hBox);

                var badgeGO = new GameObject("CalloutBadge", typeof(RectTransform));
                badgeGO.transform.SetParent(hBox, false);
                var badgeRT = badgeGO.GetComponent<RectTransform>();
                badgeRT.sizeDelta = new Vector2(38, 38);
                var badgeImg = badgeGO.AddComponent<Image>();
                badgeImg.color = def.badgeColor;
                UIHelper.SetImageRoundedSprite(badgeImg, 19f);

                var badgeLbl = UIHelper.MakeLabel("BadgeText", badgeRT, def.badgeCode, 15, Color.white, TextAlignmentOptions.Center, bold: true);
                UIHelper.Stretch(badgeLbl.rectTransform);

                var vText = UIHelper.MakeVertical("VText", hBox, 2);
                UIHelper.SetLayout(vText.gameObject, flexibleWidth: true, flexWidth: 1);
                UIHelper.MakeLabel("Name", vText, def.name, 18, ColorTextNavy, bold: true);
                UIHelper.MakeLabel("Desc", vText, def.shortDesc, 13, ColorTextMuted);

                _activeCallouts.Add(new ComponentCalloutUI
                {
                    root = calloutRT,
                    target3D = targetT,
                    def = def,
                    button = btn
                });
            }
        }

        public void UpdateCalloutsNow()
        {
            if (_mainCamera == null) _mainCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            UpdateComponentCallouts();
        }

        private void UpdateComponentCallouts()
        {
            if (_mainCamera == null) _mainCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            if (_mainCamera == null) return;

            float safeWidth = _safeArea != null && _safeArea.rect.width > 200f ? _safeArea.rect.width : 1016f;
            float safeHeight = _safeArea != null && _safeArea.rect.height > 200f ? _safeArea.rect.height : 2304f;
            float totalWidth = safeWidth + 64f;
            float totalHeight = safeHeight + 96f;

            foreach (var callout in _activeCallouts)
            {
                if (callout.target3D == null || callout.root == null) continue;

                Vector3 targetPos = GetPartWorldCenter(callout.target3D) + Vector3.up * 0.22f;
                Vector3 vp = _mainCamera.WorldToViewportPoint(targetPos);

                if (vp.z > 0.1f && vp.x >= 0.05f && vp.x <= 0.95f && vp.y >= 0.08f && vp.y <= 0.92f)
                {
                    callout.root.gameObject.SetActive(true);
                    float posX = Mathf.Clamp((vp.x - 0.5f) * totalWidth, -safeWidth * 0.44f, safeWidth * 0.44f);
                    float posY = Mathf.Clamp((vp.y - 0.5f) * totalHeight, -safeHeight * 0.35f, safeHeight * 0.40f);
                    callout.root.anchoredPosition = new Vector2(posX, posY);
                }
                else
                {
                    callout.root.gameObject.SetActive(false);
                }
            }
        }

        private Vector3 GetPartWorldCenter(Transform t)
        {
            if (t == null) return Vector3.zero;
            var renderers = t.GetComponentsInChildren<Renderer>();
            if (renderers != null && renderers.Length > 0)
            {
                Bounds b = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
                return b.center;
            }
            return t.position;
        }

        // =====================================================================
        //  COMPONENT INFORMATION MODAL
        // =====================================================================
        private void BuildComponentInfoModal()
        {
            _componentInfoModal = UIHelper.MakeRect("ComponentInfoModal", _safeArea);
            UIHelper.AnchorBottomStretch(_componentInfoModal, 620, 40);

            var bg = _componentInfoModal.gameObject.AddComponent<Image>();
            bg.color = Color.white;
            UIHelper.SetImageRoundedSprite(bg, 28f);

            var vBox = UIHelper.MakeVertical("ModalContent", _componentInfoModal, 16, new RectOffset(36, 36, 28, 28));
            UIHelper.Stretch(vBox);

            // Modal Header with Title & Close X Button
            var hHeader = UIHelper.MakeHorizontal("ModalHeader", vBox, 16);
            UIHelper.SetLayout(hHeader.gameObject, preferredHeight: 48);

            _modalTitleText = UIHelper.MakeLabel("ModalTitle", hHeader, "DRIVE MOTOR", 32, ColorTextNavy, bold: true);
            UIHelper.SetLayout(_modalTitleText.gameObject, flexibleWidth: true, flexWidth: 1);

            var btnClose = UIHelper.MakeButton("CloseBtn", hHeader, "X", 24, ColorGrayButtonBg, ColorTextNavy, 12f);
            UIHelper.SetLayout(btnClose.gameObject, preferredWidth: 54, preferredHeight: 48);
            btnClose.onClick.AddListener(() => _componentInfoModal.gameObject.SetActive(false));

            // Section 1: What is it?
            UIHelper.MakeLabel("Sec1Title", vBox, "What is it?", 22, ColorTealPrimary, bold: true);
            _modalWhatText = UIHelper.MakeLabel("Sec1Body", vBox, "", 20, ColorTextNavy);

            // Section 2: Why is it important?
            UIHelper.MakeLabel("Sec2Title", vBox, "Why is it important?", 22, ColorTealPrimary, bold: true);
            _modalWhyText = UIHelper.MakeLabel("Sec2Body", vBox, "", 20, ColorTextNavy);

            // Section 3: Worker Action
            UIHelper.MakeLabel("Sec3Title", vBox, "Worker Action", 22, ColorTealPrimary, bold: true);
            _modalActionText = UIHelper.MakeLabel("Sec3Body", vBox, "", 20, ColorTextNavy);

            // Got it / Dismiss Button
            var btnGotIt = UIHelper.MakeButton("GotItBtn", vBox, "GOT IT", 26, ColorTealPrimary, Color.white, 16f);
            UIHelper.SetLayout(btnGotIt.gameObject, preferredHeight: 70);
            btnGotIt.onClick.AddListener(() => _componentInfoModal.gameObject.SetActive(false));

            _componentInfoModal.gameObject.SetActive(false);
        }

        private void ShowComponentInfoModal(ComponentInfoDef def)
        {
            _modalTitleText.text = def.name;
            _modalWhatText.text = def.whatIsIt;
            _modalWhyText.text = def.whyImportant;
            _modalActionText.text = def.workerAction;
            _componentInfoModal.gameObject.SetActive(true);
        }

        // =====================================================================
        //  TRAINING STEP HUD & STEP CARD (STEPS 1 TO 7)
        // =====================================================================
        private void BuildTrainingStepUI()
        {
            // Top HUD Bar with 7 Step Indicators & Step Title
            _trainingTopHUD = UIHelper.MakeRect("TrainingTopHUD", _safeArea);
            UIHelper.AnchorTopStretch(_trainingTopHUD, 140, 0);

            var bgTop = _trainingTopHUD.gameObject.AddComponent<Image>();
            bgTop.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(bgTop, 24f);

            var vBoxTop = UIHelper.MakeVertical("HUDLayout", _trainingTopHUD, 10, new RectOffset(30, 30, 16, 16));
            UIHelper.Stretch(vBoxTop);

            // Top Row: Back button, Title & 7 Step Indicator Dots
            var hRow = UIHelper.MakeHorizontal("HRow", vBoxTop, 12);
            UIHelper.SetLayout(hRow.gameObject, preferredHeight: 48);

            var btnBack = UIHelper.MakeButton("HUDBackBtn", hRow, "<", 26, ColorGrayButtonBg, ColorTextNavy, 12f);
            UIHelper.SetLayout(btnBack.gameObject, preferredWidth: 50, preferredHeight: 46);
            btnBack.onClick.AddListener(() =>
            {
                if (CurrentStepIndex > 0)
                {
                    CurrentStepIndex--;
                    UpdateTrainingStepView();
                }
                else
                {
                    SetUIState(MachineryUIState.ComponentOverview);
                }
            });

            var tBox = UIHelper.MakeVertical("TBox", hRow, 2);
            UIHelper.SetLayout(tBox.gameObject, flexibleWidth: true, flexWidth: 1);
            _hudStepText = UIHelper.MakeLabel("HUDStep", tBox, "Step 1 / 7", 26, ColorTextNavy, bold: true);
            _hudSubText = UIHelper.MakeLabel("HUDSub", tBox, "Conveyor Jam Procedure", 18, ColorTextMuted);

            // 7 Step Dots Row: (1) (2) (3) (4) (5) (6) (7)
            var dotsRow = UIHelper.MakeHorizontal("DotsRow", hRow, 8);
            UIHelper.SetLayout(dotsRow.gameObject, preferredWidth: 320, preferredHeight: 44);

            _stepIndicatorDots.Clear();
            _stepIndicatorLabels.Clear();

            for (int i = 1; i <= 7; i++)
            {
                var dotGO = new GameObject($"Dot_{i}", typeof(RectTransform));
                dotGO.transform.SetParent(dotsRow, false);
                var dotRT = dotGO.GetComponent<RectTransform>();
                dotRT.sizeDelta = new Vector2(36, 36);

                var dotImg = dotGO.AddComponent<Image>();
                dotImg.color = ColorGrayButtonBg;
                UIHelper.SetImageRoundedSprite(dotImg, 18f);

                var numLbl = UIHelper.MakeLabel("Num", dotRT, i.ToString(), 18, ColorGrayButtonText, TextAlignmentOptions.Center, bold: true);
                UIHelper.Stretch(numLbl.rectTransform);

                _stepIndicatorDots.Add(dotImg);
                _stepIndicatorLabels.Add(numLbl);
            }

            // Bottom Step Instruction Card
            _stepCardBottom = UIHelper.MakeRect("StepCardBottom", _safeArea);
            UIHelper.AnchorBottomStretch(_stepCardBottom, 340, 20);

            var bgBot = _stepCardBottom.gameObject.AddComponent<Image>();
            bgBot.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(bgBot, 28f);

            var vBoxBot = UIHelper.MakeVertical("CardContent", _stepCardBottom, 12, new RectOffset(32, 32, 22, 22));
            UIHelper.Stretch(vBoxBot);

            // Title Header Row: Icon Badge + Step Title
            var hStepTitle = UIHelper.MakeHorizontal("StepTitleRow", vBoxBot, 10);
            UIHelper.SetLayout(hStepTitle.gameObject, preferredHeight: 40);

            var stepBadgeGO = new GameObject("StepBadge", typeof(RectTransform));
            stepBadgeGO.transform.SetParent(hStepTitle, false);
            var stepBadgeRT = stepBadgeGO.GetComponent<RectTransform>();
            stepBadgeRT.sizeDelta = new Vector2(32, 32);
            _stepCardBadgeImg = stepBadgeGO.AddComponent<Image>();
            _stepCardBadgeImg.color = ColorHazardRed;
            UIHelper.SetImageRoundedSprite(_stepCardBadgeImg, 16f);

            _stepCardBadgeText = UIHelper.MakeLabel("BadgeText", stepBadgeRT, "!", 18, Color.white, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(_stepCardBadgeText.rectTransform);

            _stepCardTitle = UIHelper.MakeLabel("Title", hStepTitle, "Identify the Hazard", 28, ColorTextNavy, bold: true);
            UIHelper.SetLayout(_stepCardTitle.gameObject, flexibleWidth: true, flexWidth: 1);

            // Instruction Text
            _stepCardInstruction = UIHelper.MakeLabel("Instruction", vBoxBot, "Look at the conveyor belt and identify the material blockage.", 22, ColorTextNavy);

            // Small LOTO Equipment Badge (Step 4 specific)
            var lotoBox = UIHelper.MakeRect("LotoEquipmentBox", vBoxBot);
            UIHelper.SetLayout(lotoBox.gameObject, preferredHeight: 46);
            var lotoBg = lotoBox.gameObject.AddComponent<Image>();
            lotoBg.color = new Color(0.94f, 0.97f, 1.0f);
            UIHelper.SetImageRoundedSprite(lotoBg, 12f);

            var lotoH = UIHelper.MakeHorizontal("LotoH", lotoBox, 8, new RectOffset(16, 16, 8, 8));
            UIHelper.Stretch(lotoH);
            var lotoLbl = UIHelper.MakeLabel("LotoText", lotoH, "LOTO EQUIPMENT:  Hasp  |  Padlock  |  Warning Tag", 19, new Color(0.12f, 0.32f, 0.58f), bold: true);
            UIHelper.Stretch(lotoLbl.rectTransform);
            _lotoEquipmentBadge = lotoBox;
            _lotoEquipmentBadge.gameObject.SetActive(false);

            // Guidance Pill Box (Light Green #ECFDF5)
            var pillBox = UIHelper.MakeRect("GuidancePill", vBoxBot);
            UIHelper.SetLayout(pillBox.gameObject, preferredHeight: 52);
            var pillBg = pillBox.gameObject.AddComponent<Image>();
            pillBg.color = ColorPillGreenBg;
            UIHelper.SetImageRoundedSprite(pillBg, 14f);

            var pillH = UIHelper.MakeHorizontal("PillH", pillBox, 8, new RectOffset(16, 16, 8, 8));
            UIHelper.Stretch(pillH);
            _stepCardGuidance = UIHelper.MakeLabel("GuidanceText", pillH, "> Move closer and inspect the highlighted jam area.", 19, ColorPillGreenText, bold: true);
            UIHelper.Stretch(_stepCardGuidance.rectTransform);

            // Safety Warning Note
            _stepCardSafety = UIHelper.MakeLabel("SafetyText", vBoxBot, "Do not attempt to clear the blockage while the machine is running.", 18, ColorHazardRed);

            // Button Row: Previous & Next →
            var hBtns = UIHelper.MakeHorizontal("NavBtns", vBoxBot, 16);
            UIHelper.SetLayout(hBtns.gameObject, preferredHeight: 74);

            _btnPreviousStep = UIHelper.MakeButton("PrevBtn", hBtns, "Previous", 24, ColorGrayButtonBg, ColorGrayButtonText, 16f);
            UIHelper.SetLayout(_btnPreviousStep.gameObject, preferredWidth: 260);
            _btnPreviousStep.onClick.AddListener(OnPreviousStepClicked);

            _btnNextStep = UIHelper.MakeButton("NextBtn", hBtns, "Next  →", 26, ColorTealPrimary, Color.white, 16f);
            UIHelper.SetLayout(_btnNextStep.gameObject, flexibleWidth: true, flexWidth: 1);
            _btnNextStep.onClick.AddListener(OnNextStepClicked);

            _btnNextStepText = _btnNextStep.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void UpdateTrainingStepView()
        {
            if (CurrentStepIndex < 0) CurrentStepIndex = 0;
            if (CurrentStepIndex >= GuidedSteps.Length) CurrentStepIndex = GuidedSteps.Length - 1;

            var step = GuidedSteps[CurrentStepIndex];

            // Update Top HUD
            _hudStepText.text = $"Step {step.stepNumber} / 7";
            _hudSubText.text = "Conveyor Jam Procedure";

            // Update Step Dots (1 to 7)
            for (int i = 0; i < 7; i++)
            {
                if (i == CurrentStepIndex)
                {
                    _stepIndicatorDots[i].color = ColorTealPrimary;
                    _stepIndicatorLabels[i].color = Color.white;
                }
                else if (i < CurrentStepIndex)
                {
                    _stepIndicatorDots[i].color = new Color(0.80f, 0.95f, 0.90f);
                    _stepIndicatorLabels[i].color = ColorTealPrimary;
                }
                else
                {
                    _stepIndicatorDots[i].color = ColorGrayButtonBg;
                    _stepIndicatorLabels[i].color = ColorGrayButtonText;
                }
            }

            // Update Step Card Content
            _stepCardTitle.text = step.stepTitle;
            if (_stepCardBadgeImg != null)
            {
                _stepCardBadgeImg.color = step.badgeColor;
                _stepCardBadgeText.text = (step.badgeColor == ColorHazardRed) ? "!" : ">";
            }
            _stepCardInstruction.text = step.instruction;
            _stepCardGuidance.text = ">  " + step.guidance;
            _stepCardSafety.text = "Safety Note:  " + step.safetyNote;

            // Step 4 LOTO Equipment Card
            if (_lotoEquipmentBadge != null)
            {
                bool isLoto = (step.stepNumber == 4);
                _lotoEquipmentBadge.gameObject.SetActive(isLoto);
                if (_stepCardBottom != null)
                {
                    _stepCardBottom.sizeDelta = new Vector2(_stepCardBottom.sizeDelta.x, isLoto ? 380 : 340);
                }
            }

            // Update Buttons
            _btnPreviousStep.interactable = (CurrentStepIndex > 0);

            if (CurrentStepIndex == GuidedSteps.Length - 1)
            {
                _btnNextStepText.text = "Training Complete";
            }
            else
            {
                _btnNextStepText.text = "Next  →";
            }

            // Update Target Badge on AR Detection Frame
            _detectionTargetBadge.text = step.targetBadge;
            if (_detectionTargetDot != null)
            {
                _detectionTargetDot.color = step.badgeColor;
            }
        }

        private void OnNextStepClicked()
        {
            if (CurrentStepIndex < GuidedSteps.Length - 1)
            {
                CurrentStepIndex++;
                UpdateTrainingStepView();
            }
            else
            {
                SetUIState(MachineryUIState.TrainingComplete);
            }
        }

        private void OnPreviousStepClicked()
        {
            if (CurrentStepIndex > 0)
            {
                CurrentStepIndex--;
                UpdateTrainingStepView();
            }
        }

        // =====================================================================
        //  AR 4-CORNER DETECTION FRAME
        // =====================================================================
        private void BuildARDetectionFrame()
        {
            var containerGO = new GameObject("ARDetectionContainer", typeof(RectTransform));
            containerGO.transform.SetParent(_safeArea, false);
            _detectionContainer = containerGO.GetComponent<RectTransform>();
            UIHelper.Stretch(_detectionContainer);

            // Bounding Box
            var boxGO = new GameObject("DetectionBox", typeof(RectTransform));
            boxGO.transform.SetParent(_detectionContainer, false);
            _detectionBox = boxGO.GetComponent<RectTransform>();
            _detectionBox.sizeDelta = new Vector2(300, 300);
            _detectionBox.pivot = new Vector2(0.5f, 0.5f);

            // Build 4 L-shaped Glowing Teal Corner Brackets
            float barLen = 44f;
            float barThick = 6f;

            // 1. Top-Left Corner
            CreateCornerBracket(_detectionBox, "CornerTL", new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(barLen, barThick), new Vector2(barThick, barLen), Vector2.zero);

            // 2. Top-Right Corner
            CreateCornerBracket(_detectionBox, "CornerTR", new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(barLen, barThick), new Vector2(barThick, barLen), Vector2.zero);

            // 3. Bottom-Left Corner
            CreateCornerBracket(_detectionBox, "CornerBL", new Vector2(0, 0), new Vector2(0, 0),
                new Vector2(barLen, barThick), new Vector2(barThick, barLen), Vector2.zero);

            // 4. Bottom-Right Corner
            CreateCornerBracket(_detectionBox, "CornerBR", new Vector2(1, 0), new Vector2(1, 0),
                new Vector2(barLen, barThick), new Vector2(barThick, barLen), Vector2.zero);

            // Target Badge above the Frame
            var badgeGO = new GameObject("TargetBadge", typeof(RectTransform));
            badgeGO.transform.SetParent(_detectionBox, false);
            var badgeRT = badgeGO.GetComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0.5f, 1f);
            badgeRT.anchorMax = new Vector2(0.5f, 1f);
            badgeRT.pivot = new Vector2(0.5f, 0f);
            badgeRT.anchoredPosition = new Vector2(0, 16);
            badgeRT.sizeDelta = new Vector2(250, 48);

            var badgeBg = badgeGO.AddComponent<Image>();
            badgeBg.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(badgeBg, 14f);

            var hBadge = UIHelper.MakeHorizontal("HBadge", badgeRT, 8, new RectOffset(16, 16, 6, 6));
            UIHelper.Stretch(hBadge);

            var dotGO = new GameObject("TargetIndicatorDot", typeof(RectTransform));
            dotGO.transform.SetParent(hBadge, false);
            var dotRT = dotGO.GetComponent<RectTransform>();
            dotRT.sizeDelta = new Vector2(16, 16);
            _detectionTargetDot = dotGO.AddComponent<Image>();
            _detectionTargetDot.color = ColorHazardRed;
            UIHelper.SetImageRoundedSprite(_detectionTargetDot, 8f);

            _detectionTargetBadge = UIHelper.MakeLabel("TargetText", hBadge, "Jam Area", 20, ColorTextNavy, TextAlignmentOptions.Center, bold: true);
            UIHelper.SetLayout(_detectionTargetBadge.gameObject, flexibleWidth: true, flexWidth: 1);

            // Off-Screen Directional Indicator
            var offGO = new GameObject("OffScreenGuidance", typeof(RectTransform));
            offGO.transform.SetParent(_detectionContainer, false);
            _offScreenGuidanceBadge = offGO.GetComponent<RectTransform>();
            _offScreenGuidanceBadge.anchorMin = new Vector2(0.5f, 0.5f);
            _offScreenGuidanceBadge.anchorMax = new Vector2(0.5f, 0.5f);
            _offScreenGuidanceBadge.pivot = new Vector2(0.5f, 0.5f);
            _offScreenGuidanceBadge.sizeDelta = new Vector2(380, 60);

            var offBg = offGO.AddComponent<Image>();
            offBg.color = ColorWhiteCard;
            UIHelper.SetImageRoundedSprite(offBg, 16f);

            _offScreenGuidanceText = UIHelper.MakeLabel("GuidanceText", _offScreenGuidanceBadge, "FIND THE HIGHLIGHTED COMPONENT", 20, ColorTealPrimary, TextAlignmentOptions.Center, bold: true);
            UIHelper.Stretch(_offScreenGuidanceText.rectTransform);
            _offScreenGuidanceBadge.gameObject.SetActive(false);
        }

        private void CreateCornerBracket(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 hSize, Vector2 vSize, Vector2 offset)
        {
            var cornerGO = new GameObject(name, typeof(RectTransform));
            cornerGO.transform.SetParent(parent, false);
            var rt = cornerGO.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = offset;
            rt.sizeDelta = new Vector2(hSize.x, vSize.y);

            // Horizontal bar
            var hBarGO = new GameObject("HBar", typeof(RectTransform));
            hBarGO.transform.SetParent(rt, false);
            var hRT = hBarGO.GetComponent<RectTransform>();
            hRT.anchorMin = anchor;
            hRT.anchorMax = anchor;
            hRT.pivot = pivot;
            hRT.sizeDelta = hSize;
            var hImg = hBarGO.AddComponent<Image>();
            hImg.color = ColorDetectionCorner;

            // Vertical bar
            var vBarGO = new GameObject("VBar", typeof(RectTransform));
            vBarGO.transform.SetParent(rt, false);
            var vRT = vBarGO.GetComponent<RectTransform>();
            vRT.anchorMin = anchor;
            vRT.anchorMax = anchor;
            vRT.pivot = pivot;
            vRT.sizeDelta = vSize;
            var vImg = vBarGO.AddComponent<Image>();
            vImg.color = ColorDetectionCorner;
        }

        public void UpdateDetectionFrameNow()
        {
            if (_mainCamera == null) _mainCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            UpdateDetectionFrame();
        }

        private void UpdateDetectionFrame()
        {
            if (_mainCamera == null) _mainCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            if (_mainCamera == null) return;
            if (CurrentStepIndex < 0 || CurrentStepIndex >= GuidedSteps.Length) return;

            var step = GuidedSteps[CurrentStepIndex];
            Transform targetT = FindMachineryPart(step.targetGameObjectName);
            if (targetT == null)
            {
                _detectionBox.gameObject.SetActive(false);
                return;
            }

            Vector3 worldCenter = GetPartWorldCenter(targetT);
            Vector3 vp = _mainCamera.WorldToViewportPoint(worldCenter);

            float safeWidth = _safeArea != null && _safeArea.rect.width > 200f ? _safeArea.rect.width : 1016f;
            float safeHeight = _safeArea != null && _safeArea.rect.height > 200f ? _safeArea.rect.height : 2304f;
            float totalWidth = safeWidth + 64f;
            float totalHeight = safeHeight + 96f;

            if (vp.z > 0.1f)
            {
                // In front of camera
                bool isInsideScreen = (vp.x >= 0.08f && vp.x <= 0.92f && vp.y >= 0.10f && vp.y <= 0.90f);

                if (isInsideScreen)
                {
                    _detectionBox.gameObject.SetActive(true);
                    _offScreenGuidanceBadge.gameObject.SetActive(false);

                    float posX = (vp.x - 0.5f) * totalWidth;
                    float posY = (vp.y - 0.5f) * totalHeight;
                    _detectionBox.anchoredPosition = new Vector2(posX, posY);

                    // Compute dynamic distance-scaled size
                    float dist = vp.z;
                    float boxSize = Mathf.Clamp(720f / Mathf.Max(dist, 0.8f), 240f, 440f);
                    _detectionBox.sizeDelta = new Vector2(boxSize, boxSize);

                    // Gentle subtle breathing pulse
                    float pulse = 1.0f + Mathf.Sin(Time.time * 4f) * 0.03f;
                    _detectionBox.localScale = new Vector3(pulse, pulse, 1f);
                }
                else
                {
                    // Target in front but off to the side
                    _detectionBox.gameObject.SetActive(false);
                    _offScreenGuidanceBadge.gameObject.SetActive(true);

                    if (vp.x < 0.08f)
                    {
                        _offScreenGuidanceText.text = $"< Look Left to find {step.targetBadge}";
                        _offScreenGuidanceBadge.anchoredPosition = new Vector2(-280, 0);
                    }
                    else if (vp.x > 0.92f)
                    {
                        _offScreenGuidanceText.text = $"Look Right to find {step.targetBadge} >";
                        _offScreenGuidanceBadge.anchoredPosition = new Vector2(280, 0);
                    }
                    else if (vp.y < 0.10f)
                    {
                        _offScreenGuidanceText.text = $"Look Down to find {step.targetBadge} v";
                        _offScreenGuidanceBadge.anchoredPosition = new Vector2(0, -320);
                    }
                    else
                    {
                        _offScreenGuidanceText.text = $"^ Look Up to find {step.targetBadge}";
                        _offScreenGuidanceBadge.anchoredPosition = new Vector2(0, 320);
                    }
                }
            }
            else
            {
                // Target behind camera
                _detectionBox.gameObject.SetActive(false);
                _offScreenGuidanceBadge.gameObject.SetActive(true);
                _offScreenGuidanceText.text = $"Turn Around to find {step.targetBadge}";
                _offScreenGuidanceBadge.anchoredPosition = Vector2.zero;
            }
        }

        // =====================================================================
        //  TRAINING COMPLETE SCREEN
        // =====================================================================
        private void BuildTrainingCompleteScreen()
        {
            _trainingCompletePanel = UIHelper.MakeRect("Screen_TrainingComplete", _safeArea);
            UIHelper.Stretch(_trainingCompletePanel);

            var bg = _trainingCompletePanel.gameObject.AddComponent<Image>();
            bg.color = Color.white;
            UIHelper.SetImageRoundedSprite(bg, 32f);

            var vBox = UIHelper.MakeVertical("CompleteContent", _trainingCompletePanel, 18, new RectOffset(40, 40, 36, 36));
            vBox.anchorMin = new Vector2(0.5f, 0.5f);
            vBox.anchorMax = new Vector2(0.5f, 0.5f);
            vBox.pivot = new Vector2(0.5f, 0.5f);
            vBox.sizeDelta = new Vector2(1000, 1180);
            vBox.anchoredPosition = Vector2.zero;

            // Centered Big Green Checkmark Circle
            var checkRow = UIHelper.MakeHorizontal("CheckCenterRow", vBox, 0);
            UIHelper.SetLayout(checkRow.gameObject, preferredHeight: 104);
            var checkCenter = checkRow.gameObject.GetComponent<HorizontalLayoutGroup>();
            checkCenter.childAlignment = TextAnchor.MiddleCenter;

            var checkCircle = UIHelper.MakeRect("CheckCircle", checkRow);
            UIHelper.SetLayout(checkCircle.gameObject, preferredWidth: 96, preferredHeight: 96);
            var circleImg = checkCircle.gameObject.AddComponent<Image>();
            circleImg.color = ColorPillGreenBg;
            UIHelper.SetImageRoundedSprite(circleImg, 48f);

            AddCheckmarkGraphic(checkCircle, ColorTealPrimary, 48f);

            // Title & Subtitle
            UIHelper.MakeLabel("CompTitle", vBox, "TRAINING COMPLETE", 40, ColorTextNavy, TextAlignmentOptions.Center, bold: true);
            UIHelper.MakeLabel("CompSub", vBox, "Conveyor Jam Safety Procedure Demonstrated", 22, ColorTextMuted, TextAlignmentOptions.Center);

            // 7 Steps Checklist Container
            var checklistCard = UIHelper.MakeRect("ChecklistCard", vBox);
            UIHelper.SetLayout(checklistCard.gameObject, preferredHeight: 380);
            var checkBg = checklistCard.gameObject.AddComponent<Image>();
            checkBg.color = new Color(0.97f, 0.98f, 0.99f);
            UIHelper.SetImageRoundedSprite(checkBg, 20f);

            var vChecklist = UIHelper.MakeVertical("ChecklistV", checklistCard, 8, new RectOffset(24, 24, 18, 18));
            UIHelper.Stretch(vChecklist);

            string[] stepTitles = new string[]
            {
                "Identify Hazard",
                "Stop Machine",
                "Isolate Power",
                "Apply LOTO",
                "Verify Safe State",
                "Clear Jam",
                "Restore Operation"
            };

            for (int i = 0; i < stepTitles.Length; i++)
            {
                var hItem = UIHelper.MakeHorizontal($"Item_{i}", vChecklist, 14);
                UIHelper.SetLayout(hItem.gameObject, preferredHeight: 38);

                var chkCircle = UIHelper.MakeRect("ChkCircle", hItem);
                UIHelper.SetLayout(chkCircle.gameObject, preferredWidth: 26, preferredHeight: 26);
                var chkImg = chkCircle.gameObject.AddComponent<Image>();
                chkImg.color = ColorPillGreenBg;
                UIHelper.SetImageRoundedSprite(chkImg, 13f);
                AddCheckmarkGraphic(chkCircle, ColorTealPrimary, 16f);

                var txt = UIHelper.MakeLabel("Txt", hItem, $"{i + 1}.  {stepTitles[i]}", 22, ColorTextNavy, bold: true);
                UIHelper.SetLayout(txt.gameObject, flexibleWidth: true, flexWidth: 1);
            }

            // Buttons: REVIEW TRAINING & BACK TO MACHINERY
            var btnReview = UIHelper.MakeButton("ReviewBtn", vBox, "REVIEW TRAINING", 28, ColorGrayButtonBg, ColorGrayButtonText, 16f);
            UIHelper.SetLayout(btnReview.gameObject, preferredHeight: 76);
            btnReview.onClick.AddListener(() =>
            {
                CurrentStepIndex = 0;
                SetUIState(MachineryUIState.TrainingStep);
            });

            var btnBackMachinery = UIHelper.MakeButton("BackMachineryBtn", vBox, "BACK TO MACHINERY", 28, ColorTealPrimary, Color.white, 16f);
            UIHelper.SetLayout(btnBackMachinery.gameObject, preferredHeight: 76);
            btnBackMachinery.onClick.AddListener(() =>
            {
                SetUIState(MachineryUIState.ComponentOverview);
            });

            var btnMainMenu = UIHelper.MakeButton("MainMenuBtn", vBox, "MAIN MENU", 24, Color.clear, ColorTextMuted, 16f);
            UIHelper.SetLayout(btnMainMenu.gameObject, preferredHeight: 46);
            btnMainMenu.onClick.AddListener(() =>
            {
                SetUIState(MachineryUIState.MainMenu);
            });
        }

        // =====================================================================
        //  STATE FLOW CONTROLLER
        // =====================================================================
        public void SetUIState(MachineryUIState newState)
        {
            CurrentState = newState;

            // Hide all screens
            if (_mainMenuPanel != null) _mainMenuPanel.gameObject.SetActive(false);
            if (_scanningTopCard != null) _scanningTopCard.gameObject.SetActive(false);
            if (_scanningBottomCard != null) _scanningBottomCard.gameObject.SetActive(false);
            if (_tapToPlaceTopCard != null) _tapToPlaceTopCard.gameObject.SetActive(false);
            if (_placedTopCard != null) _placedTopCard.gameObject.SetActive(false);
            if (_placedBottomCard != null) _placedBottomCard.gameObject.SetActive(false);
            if (_overviewTopCard != null) _overviewTopCard.gameObject.SetActive(false);
            if (_overviewBottomCard != null) _overviewBottomCard.gameObject.SetActive(false);
            if (_calloutsContainer != null) _calloutsContainer.gameObject.SetActive(false);
            if (_componentInfoModal != null) _componentInfoModal.gameObject.SetActive(false);
            if (_trainingTopHUD != null) _trainingTopHUD.gameObject.SetActive(false);
            if (_stepCardBottom != null) _stepCardBottom.gameObject.SetActive(false);
            if (_detectionContainer != null) _detectionContainer.gameObject.SetActive(false);
            if (_trainingCompletePanel != null) _trainingCompletePanel.gameObject.SetActive(false);

            switch (newState)
            {
                case MachineryUIState.MainMenu:
                    if (_mainMenuPanel != null) _mainMenuPanel.gameObject.SetActive(true);
                    break;

                case MachineryUIState.ARScanning:
                    if (_scanningTopCard != null) _scanningTopCard.gameObject.SetActive(true);
                    if (_scanningBottomCard != null) _scanningBottomCard.gameObject.SetActive(true);
                    break;

                case MachineryUIState.TapToPlace:
                    if (_tapToPlaceTopCard != null) _tapToPlaceTopCard.gameObject.SetActive(true);
                    break;

                case MachineryUIState.MachineryPlaced:
                    if (_placedTopCard != null) _placedTopCard.gameObject.SetActive(true);
                    if (_placedBottomCard != null) _placedBottomCard.gameObject.SetActive(true);
                    break;

                case MachineryUIState.ComponentOverview:
                    if (_overviewTopCard != null) _overviewTopCard.gameObject.SetActive(true);
                    if (_overviewBottomCard != null) _overviewBottomCard.gameObject.SetActive(true);
                    if (_calloutsContainer != null)
                    {
                        _calloutsContainer.gameObject.SetActive(true);
                        InitializeComponentCallouts();
                    }
                    break;

                case MachineryUIState.TrainingStep:
                    if (_trainingTopHUD != null) _trainingTopHUD.gameObject.SetActive(true);
                    if (_stepCardBottom != null) _stepCardBottom.gameObject.SetActive(true);
                    if (_detectionContainer != null) _detectionContainer.gameObject.SetActive(true);
                    UpdateTrainingStepView();
                    break;

                case MachineryUIState.TrainingComplete:
                    if (_trainingCompletePanel != null) _trainingCompletePanel.gameObject.SetActive(true);
                    break;
            }
        }

        // =====================================================================
        //  EXTERNAL & BACKWARD-COMPATIBILITY EVENT HANDLERS
        // =====================================================================
        public void OnMachineryPlaced()
        {
            SetUIState(MachineryUIState.MachineryPlaced);
        }

        public void OnPlacementReset()
        {
            SetUIState(MachineryUIState.ARScanning);
        }

        public void UpdateInstruction(string title, string body)
        {
            if (_stepCardTitle != null) _stepCardTitle.text = title;
            if (_stepCardInstruction != null) _stepCardInstruction.text = body;
        }

        public void SetStatusMessage(string message, Color? color = null)
        {
            if (_stepCardSafety != null)
            {
                _stepCardSafety.text = message;
                if (color.HasValue) _stepCardSafety.color = color.Value;
            }
        }

        public void SetCamera(Camera cam)
        {
            _mainCamera = cam;
        }

        private Transform FindMachineryPart(string name)
        {
            // 1. Search active scene roots first
            GameObject scenario = GameObject.Find("MachineryScenario");
            if (scenario == null)
            {
                var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (activeScene.isLoaded)
                {
                    foreach (var root in activeScene.GetRootGameObjects())
                    {
                        if (root.name == "MachineryScenario") { scenario = root; break; }
                    }
                }
            }
            if (scenario == null)
            {
                var allScenarios = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var go in allScenarios)
                {
                    if (go.name == "MachineryScenario" && go.scene.isLoaded)
                    {
                        scenario = go;
                        if (go.activeInHierarchy) break;
                    }
                }
            }

            if (scenario != null)
            {
                Transform exact = scenario.transform.Find(name);
                if (exact != null) return exact;

                foreach (Transform t in scenario.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name.Equals(name, StringComparison.OrdinalIgnoreCase)) return t;
                }

                foreach (Transform t in scenario.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0) return t;
                }

                if (name.Contains("LOTO") || name.Contains("Table") || name.Contains("Hasp") || name.Contains("Padlock"))
                {
                    foreach (Transform t in scenario.GetComponentsInChildren<Transform>(true))
                    {
                        if (t.name.Contains("SteelWorkstationTable") || t.name.Contains("Table") || t.name.Contains("LOTOAssembly") || t.name.Contains("Hasp")) return t;
                    }
                }

                if (name.Contains("Jam") || name.Contains("Coal"))
                {
                    foreach (Transform t in scenario.GetComponentsInChildren<Transform>(true))
                    {
                        if (t.name.Contains("JamArea") || t.name.Contains("Coal") || t.name.Contains("Blockage")) return t;
                    }
                }
            }
            return null;
        }
    }
}
