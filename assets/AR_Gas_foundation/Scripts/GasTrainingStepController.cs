using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.UI;

public class GasTrainingStepController : MonoBehaviour
{
    public enum TrainingStep
    {
        ScanAndPlace = 0,               // Step 0: AR Placement
        WorkstationReady = 1,           // Placement complete: "✓ Workstation Ready. Gas leak response drill is ready."
        CylinderInteraction = 2,        // STEP 1 — START THE DRILL: Tap the Gas Cylinder to begin the simulated leak.
        GasLeakActive = 3,              // ⚠ GAS LEAK DETECTED: Observe the leak and check the Gas Detector.
        DetectorInteraction = 4,        // STEP 2 — CHECK THE ATMOSPHERE: Tap the Gas Detector to check the reading.
        DetectorChecked = 5,            // ⚠ GAS DETECTED | HAZARD ACTIVE | READING: ELEVATED -> "✓ Gas Detector Checked"
        HazardRecognition = 6,          // STEP 3 — IDENTIFY THE HAZARD AREA: Identify the area affected by the gas leak.
        HazardZoneRecognition = 6,      // Alias for backward compatibility
        PPECheck = 7,                   // STEP 4 — SELECT REQUIRED PPE: Select the PPE required by site procedure.
        BuddySystemCheck = 8,           // STEP 5 — BUDDY SYSTEM: Confirm designated buddy/standby procedure.
        BuddySystem = 8,                // Alias for BuddySystemCheck
        SafetyResponse = 9,             // STEP 6 — GAS ISOLATION: Tap the RED ISOLATION VALVE.
        IncidentResolved = 10,          // ✓ GAS ISOLATED: Leak stopped. Verify detector is returning to normal.
        PostResponseVerification = 11,  // STEP 7 — VERIFY SAFE STATE: Confirm gas leak stopped and detector returned toward normal.
        FinalAssessment = 12,           // Assessment Evaluation & Bridge Call
        PassResult = 13,                // Result Card (Competent)
        RetrainingResult = 14,          // Result Card (Needs Review + Missed Step)
        ConfinedSpaceIntro = 15,        // Confined Space Transition
        ConfinedSpaceDrill = 16,        // Pre-Entry Blower & Lifeline Check
        Completion = 17,                // Module Completion
        // Legacy compatibility values
        Intro = 18,
        EquipmentIdentification = 19,
        NormalState = 20,
        LeakRecognition = 21,
        DetectorRecognition = 22,
        ReadinessCheck = 23,
        TargetedRetraining = 24,
        Reassessment = 25
    }

    [Header("Current Step")]
    [SerializeField] private TrainingStep currentStep = TrainingStep.ScanAndPlace;
    public TrainingStep CurrentStep => currentStep;

    [Header("References")]
    [SerializeField] private GasARPlacement arPlacement;
    [SerializeField] private GasEnvironmentBuilder environmentBuilder;
    [SerializeField] private GasLeakVisualController leakVisualController;

    // Runtime UI Elements
    private GameObject canvasObject;
    private Canvas mainCanvas;
    private TextMeshProUGUI headerStepTitleText;
    private TextMeshProUGUI headerStepSubText;
    private TextMeshProUGUI instructionText;
    private GameObject bottomInstructionCard;

    // Optional Voice Guidance UI Elements & Audio Source
    private Button voiceButton;
    private Image voiceButtonBg;
    private TextMeshProUGUI voiceButtonText;
    private Button ppeVoiceButton;
    private Image ppeVoiceButtonBg;
    private TextMeshProUGUI ppeVoiceButtonText;
    private AudioSource voiceAudioSource;
    private bool isVoiceSpeaking = false;
    private Coroutine voiceMonitorCoroutine;
    private Coroutine voiceButtonPulseCoroutine;
    private readonly Dictionary<string, AudioClip> voiceClipCache = new Dictionary<string, AudioClip>();

    private GameObject actionButtonGO;
    private Button actionButton;
    private TextMeshProUGUI actionButtonText;

    private GameObject alertBanner;
    private TextMeshProUGUI alertBannerText;

    private GameObject ppeCard;
    private TextMeshProUGUI ppeCardTitleText;
    private TextMeshProUGUI ppeFeedbackText;
    private TextMeshProUGUI ppeStatusCounterText;
    private Image ppeRowBgHardhat;
    private TextMeshProUGUI ppeRowTextHardhat;
    private Image ppeRowBgGoggles;
    private TextMeshProUGUI ppeRowTextGoggles;
    private Image ppeRowBgGloves;
    private TextMeshProUGUI ppeRowTextGloves;
    private Image ppeRowBgBoots;
    private TextMeshProUGUI ppeRowTextBoots;
    private TextMeshProUGUI ppeCardInstructionText;
    private Button ppeContinueButton;
    private GameObject ppeContinueButtonGO;
    private Coroutine ppeFeedbackCoroutine;
    private List<GameObject> activePpeColliderObjects = new List<GameObject>();

    private GameObject buddyCard;

    private GameObject completionCard;
    private TextMeshProUGUI completionTitleText;
    private TextMeshProUGUI compSubtitleText;
    private TextMeshProUGUI compScoreValText;
    private TextMeshProUGUI compResultValText;
    private TextMeshProUGUI completionScoreText;
    private TextMeshProUGUI completionDetailsText;
    private Button compContinueButton;
    private Button restartButton;
    private Button retrainButton;

    private GameObject detectorDisplayCard;
    private TextMeshProUGUI detCoLabel;
    private TextMeshProUGUI detLelLabel;
    private TextMeshProUGUI detO2Label;
    private TextMeshProUGUI detH2sLabel;
    private TextMeshProUGUI detStatusText;
    private Image detStatusBadgeBg;

    private GameObject currentDetectorObj;
    private Transform originalDetectorParent;
    private Vector3 originalDetectorLocalPosition;
    private Quaternion originalDetectorLocalRotation;
    private Vector3 originalDetectorLocalScale;
    private bool detectorPickedUp = false;
    private bool detectorScanned = false;
    private Coroutine detectorPickupCoroutine;
    private Coroutine detectorAlarmCoroutine;

    private GameObject hazardZoneVisualDisk;
    private GameObject preLeakIndicatorGO;
    private Coroutine preLeakPulseCoroutine;
    private Coroutine hazardHighlightCoroutine;
    private Coroutine ppeHighlightCoroutine;
    private bool ppeHardhatSelected = false;
    private bool ppeGogglesSelected = false;
    private bool ppeGlovesSelected = false;
    private bool ppeBootsSelected = false;

    [Header("Equipment Audio Tuning")]
    [SerializeField] private float leakHissVolume = 0.35f;
    [SerializeField] private float detectorScanVolume = 0.25f;
    [SerializeField] private float detectorAlarmVolume = 0.55f;

    private AudioSource audioSource;
    private AudioSource leakAudioSource;
    private AudioSource detectorAudioSource;
    private Coroutine detectorScanAudioCoroutine;
    private static AudioClip cachedLeakHissClip;
    private static AudioClip cachedScanBeepClip;
    private static AudioClip cachedDetectorAlarmClip;
    private float stepStartTime;

    // Interaction Action Tracking
    private bool cylinderInteracted = false;
    private bool leakRecognized = false;
    private bool detectorInteracted = false;
    private bool hazardRecognized = false;
    private bool ppeComplete = false;
    private bool ppeSelectedCorrect = false;
    private bool buddyVerified = false;
    private bool isolationValveInteracted = false;
    private bool verificationComplete = false;

    private float userScore = 100f;
    private List<string> mistakeLog = new List<string>();
    private List<string> missedSteps = new List<string>();

    private void Awake()
    {
        if (arPlacement == null) arPlacement = FindFirstObjectByType<GasARPlacement>();
        if (environmentBuilder == null) environmentBuilder = GetComponent<GasEnvironmentBuilder>();
        if (environmentBuilder == null) environmentBuilder = FindFirstObjectByType<GasEnvironmentBuilder>();
        if (leakVisualController == null) leakVisualController = GetComponent<GasLeakVisualController>();
        if (leakVisualController == null) leakVisualController = FindFirstObjectByType<GasLeakVisualController>();

        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        CreateRuntimeUI();
    }

    private void Start()
    {
        CacheOriginalDetectorTransform();
        SetStep(TrainingStep.ScanAndPlace);
    }

    private void CreateRuntimeUI()
    {
        if (canvasObject != null)
        {
            if (Application.isPlaying) Destroy(canvasObject);
            else DestroyImmediate(canvasObject);
            canvasObject = null;
        }

        // Clean up ANY duplicate/stray Gas canvases in THIS scene only (do NOT touch MainCanvas or other scenes)
        Canvas[] existingCanvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in existingCanvases)
        {
            if (c != null && c.gameObject != null && c.gameObject.scene == gameObject.scene &&
                (c.name == "SurakshaAR_GasTrainingCanvas" || c.name.Contains("GasTraining")))
            {
                if (Application.isPlaying) Destroy(c.gameObject);
                else DestroyImmediate(c.gameObject);
            }
        }

        canvasObject = new GameObject("SurakshaAR_GasTrainingCanvas");
        mainCanvas = canvasObject.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

        // 1. TOP HEADER BAR
        RectTransform headerBar = UIHelper.MakeRect("TopHeaderBar", canvasObject.transform);
        UIHelper.AnchorTopStretch(headerBar, 160f, 0f);
        Image headerBg = headerBar.gameObject.AddComponent<Image>();
        headerBg.color = new Color(0.04f, 0.22f, 0.14f, 0.95f);

        TextMeshProUGUI titleLabel = UIHelper.MakeLabel("AppTitle", headerBar,
            "SURAKSHAAR — GAS LEAK & CONFINED SPACE DRILL", 28f, Color.white,
            TextAlignmentOptions.Center, true);
        titleLabel.rectTransform.anchorMin = new Vector2(0.02f, 0.52f);
        titleLabel.rectTransform.anchorMax = new Vector2(0.98f, 0.94f);
        titleLabel.rectTransform.offsetMin = Vector2.zero;
        titleLabel.rectTransform.offsetMax = Vector2.zero;

        headerStepTitleText = UIHelper.MakeLabel("StepTitle", headerBar,
            "GAS & CONFINED SPACE SAFETY", 24f, new Color(0.9f, 0.85f, 0.4f),
            TextAlignmentOptions.Center, true);
        headerStepTitleText.rectTransform.anchorMin = new Vector2(0.02f, 0.08f);
        headerStepTitleText.rectTransform.anchorMax = new Vector2(0.98f, 0.48f);
        headerStepTitleText.rectTransform.offsetMin = Vector2.zero;
        headerStepTitleText.rectTransform.offsetMax = Vector2.zero;

        // 2. BOTTOM INSTRUCTION CARD (Generic Bottom Panel)
        RectTransform bottomCard = UIHelper.MakeRect("BottomInstructionCard", canvasObject.transform);
        UIHelper.AnchorBottomStretch(bottomCard, 340f, 40f);
        Image cardBg = bottomCard.gameObject.AddComponent<Image>();
        cardBg.color = new Color(0.05f, 0.08f, 0.12f, 0.95f);
        UIHelper.SetImageRoundedSprite(cardBg, 18f);
        bottomInstructionCard = bottomCard.gameObject;

        headerStepSubText = UIHelper.MakeLabel("StepSubText", bottomCard,
            "Scan & Place", 22f, new Color(0.0f, 0.75f, 1.0f),
            TextAlignmentOptions.Left, true);
        headerStepSubText.rectTransform.anchorMin = new Vector2(0.05f, 0.81f);
        headerStepSubText.rectTransform.anchorMax = new Vector2(0.68f, 0.97f);
        headerStepSubText.rectTransform.offsetMin = Vector2.zero;
        headerStepSubText.rectTransform.offsetMax = Vector2.zero;

        // Optional Voice Guidance Button (Top-Right of Instruction Card)
        voiceButton = UIHelper.MakeButton("VoiceGuidanceBtn", bottomCard, "LISTEN", 20f,
            Hex("#2563EB"), Color.white, 14f);
        RectTransform vRt = voiceButton.GetComponent<RectTransform>();
        LayoutElement vLe = voiceButton.GetComponent<LayoutElement>();
        if (vLe != null) Destroy(vLe);
        vRt.anchorMin = new Vector2(0.70f, 0.81f);
        vRt.anchorMax = new Vector2(0.96f, 0.97f);
        vRt.offsetMin = Vector2.zero;
        vRt.offsetMax = Vector2.zero;
        voiceButtonBg = voiceButton.GetComponent<Image>();
        voiceButtonText = voiceButton.GetComponentInChildren<TextMeshProUGUI>();
        if (voiceButtonText != null)
        {
            voiceButtonText.fontSize = 20f;
            voiceButtonText.text = "🔊 LISTEN";
        }
        voiceButton.onClick.AddListener(OnVoiceButtonClicked);

        instructionText = UIHelper.MakeLabel("InstructionText", bottomCard,
            "Scan the floor and place the gas safety workstation.", 22f, Color.white,
            TextAlignmentOptions.Center, false, true);
        instructionText.rectTransform.anchorMin = new Vector2(0.04f, 0.42f);
        instructionText.rectTransform.anchorMax = new Vector2(0.96f, 0.80f);
        instructionText.rectTransform.offsetMin = Vector2.zero;
        instructionText.rectTransform.offsetMax = Vector2.zero;

        // Contextual Action Button (Only enabled when interactive step criteria met)
        actionButton = UIHelper.MakeButton("ActionButton", bottomCard, "Proceed", 22f, new Color(0.0f, 0.6f, 0.9f), Color.white, 14f);
        RectTransform actRt = actionButton.GetComponent<RectTransform>();
        actRt.anchorMin = new Vector2(0.5f, 0.12f);
        actRt.anchorMax = new Vector2(0.5f, 0.12f);
        actRt.anchoredPosition = Vector2.zero;
        UIHelper.SetSize(actRt, 460f, 54f);
        actionButtonGO = actionButton.gameObject;
        actionButtonText = actionButton.GetComponentInChildren<TextMeshProUGUI>();
        actionButton.onClick.AddListener(OnActionButtonClicked);
        actionButtonGO.SetActive(false);

        // 3. HAZARD ALERT BANNER
        RectTransform alertBar = UIHelper.MakeRect("HazardAlertBanner", canvasObject.transform);
        UIHelper.AnchorTopStretch(alertBar, 100f, 170f);
        Image alertBg = alertBar.gameObject.AddComponent<Image>();
        alertBg.color = new Color(0.85f, 0.15f, 0.1f, 0.95f);
        alertBannerText = UIHelper.MakeLabel("AlertText", alertBar,
            "⚠ HAZARD ALERT: GAS DETECTED (SIMULATED SENSOR VALUE - DEMO)", 24f, Color.white,
            TextAlignmentOptions.Center, true);
        alertBannerText.rectTransform.anchorMin = new Vector2(0.02f, 0.05f);
        alertBannerText.rectTransform.anchorMax = new Vector2(0.98f, 0.95f);
        alertBannerText.rectTransform.offsetMin = Vector2.zero;
        alertBannerText.rectTransform.offsetMax = Vector2.zero;
        alertBanner = alertBar.gameObject;
        alertBanner.SetActive(false);

        // 4. PPE SELECTION CARD (Self-Contained Vertical Layout — No Generic Panel Behind It)
        RectTransform ppeRt = UIHelper.MakeRect("PpeSelectionCard", canvasObject.transform);
        ppeRt.anchorMin = new Vector2(0.5f, 0.5f);
        ppeRt.anchorMax = new Vector2(0.5f, 0.5f);
        ppeRt.pivot = new Vector2(0.5f, 0.5f);
        UIHelper.SetSize(ppeRt, 940f, 580f);
        ppeRt.anchoredPosition = new Vector2(0, -60f); // lower center, clear of top header

        Image ppeBg = ppeRt.gameObject.AddComponent<Image>();
        ppeBg.color = new Color(0.06f, 0.10f, 0.18f, 0.95f);
        UIHelper.SetImageRoundedSprite(ppeBg, 24f);

        Outline ppeOutline = ppeRt.gameObject.AddComponent<Outline>();
        ppeOutline.effectColor = new Color(1f, 1f, 1f, 0.15f);
        ppeOutline.effectDistance = new Vector2(1.5f, -1.5f);

        // Header: 30px bold (aligned left-center to make room for voice button)
        ppeCardTitleText = UIHelper.MakeLabel("PpeHeader", ppeRt,
            "SELECT REQUIRED PPE", 30f, Color.white,
            TextAlignmentOptions.Center, true);
        ppeCardTitleText.rectTransform.anchoredPosition = new Vector2(-70f, 235f);
        ppeCardTitleText.rectTransform.sizeDelta = new Vector2(580f, 44f);

        // Optional Voice Guidance Button on PPE Card
        ppeVoiceButton = UIHelper.MakeButton("PpeVoiceBtn", ppeRt, "LISTEN", 18f,
            Hex("#2563EB"), Color.white, 14f);
        RectTransform ppeVRt = ppeVoiceButton.GetComponent<RectTransform>();
        LayoutElement ppeLe = ppeVoiceButton.GetComponent<LayoutElement>();
        if (ppeLe != null) Destroy(ppeLe);
        ppeVRt.anchorMin = new Vector2(0.5f, 0.5f);
        ppeVRt.anchorMax = new Vector2(0.5f, 0.5f);
        ppeVRt.pivot = new Vector2(0.5f, 0.5f);
        ppeVRt.anchoredPosition = new Vector2(330f, 235f);
        UIHelper.SetSize(ppeVRt, 180f, 44f);
        ppeVoiceButtonBg = ppeVoiceButton.GetComponent<Image>();
        ppeVoiceButtonText = ppeVoiceButton.GetComponentInChildren<TextMeshProUGUI>();
        if (ppeVoiceButtonText != null)
        {
            ppeVoiceButtonText.fontSize = 18f;
            ppeVoiceButtonText.text = "🔊 LISTEN";
        }
        ppeVoiceButton.onClick.AddListener(OnVoiceButtonClicked);

        // Counter: 22px bold
        ppeStatusCounterText = UIHelper.MakeLabel("PpeCounter", ppeRt,
            "0 / 4 SELECTED", 22f, Hex("#38BDF8"),
            TextAlignmentOptions.Center, true);
        ppeStatusCounterText.rectTransform.anchoredPosition = new Vector2(0, 190f);
        ppeStatusCounterText.rectTransform.sizeDelta = new Vector2(860f, 36f);

        // FOUR SEPARATE ROWS:
        CreatePpeRow(ppeRt, "RowHardhat", 125f, out ppeRowBgHardhat, out ppeRowTextHardhat, "○  HARDHAT");
        CreatePpeRow(ppeRt, "RowGoggles", 60f, out ppeRowBgGoggles, out ppeRowTextGoggles, "○  SAFETY GOGGLES");
        CreatePpeRow(ppeRt, "RowGloves", -5f, out ppeRowBgGloves, out ppeRowTextGloves, "○  GLOVES");
        CreatePpeRow(ppeRt, "RowBoots", -70f, out ppeRowBgBoots, out ppeRowTextBoots, "○  SAFETY BOOTS");

        // Instruction inside the card:
        ppeCardInstructionText = UIHelper.MakeLabel("PpeInstruction", ppeRt,
            "Tap the required PPE items on the workstation.", 22f, Hex("#CBD5E1"),
            TextAlignmentOptions.Center, false, true);
        ppeCardInstructionText.rectTransform.anchoredPosition = new Vector2(0, -140f);
        ppeCardInstructionText.rectTransform.sizeDelta = new Vector2(860f, 44f);

        // Continue Button inside the card (Shown only when 4/4):
        ppeContinueButton = UIHelper.MakeButton("PpeContinueBtn", ppeRt, "CONTINUE", 26f, Hex("#1E3A8A"), Color.white, 16f);
        RectTransform ppeBtnRt = ppeContinueButton.GetComponent<RectTransform>();
        ppeBtnRt.anchoredPosition = new Vector2(0, -215f);
        UIHelper.SetSize(ppeBtnRt, 840f, 68f);
        ppeContinueButton.onClick.AddListener(OnPpeContinueClicked);
        ppeContinueButtonGO = ppeContinueButton.gameObject;
        ppeContinueButtonGO.SetActive(false);

        ppeCard = ppeRt.gameObject;
        ppeCard.SetActive(false);

        // 5. BUDDY SYSTEM CARD
        RectTransform buddyRt = UIHelper.MakeRect("BuddySystemCard", canvasObject.transform);
        UIHelper.SetSize(buddyRt, 880f, 400f);
        buddyRt.anchoredPosition = new Vector2(0, 40);
        Image buddyBg = buddyRt.gameObject.AddComponent<Image>();
        buddyBg.color = new Color(0.08f, 0.12f, 0.18f, 0.95f);
        UIHelper.SetImageRoundedSprite(buddyBg, 20f);

        UIHelper.MakeLabel("BuddyTitle", buddyRt, "Standby Buddy Verification:", 28f, new Color(0.0f, 0.85f, 1.0f), TextAlignmentOptions.Center, true).rectTransform.anchoredPosition = new Vector2(0, 130);
        UIHelper.MakeLabel("BuddyBody", buddyRt, "Confirm the designated buddy/standby procedure\nbefore approaching gas valve.", 24f, Color.white, TextAlignmentOptions.Center, false).rectTransform.anchoredPosition = new Vector2(0, 40);

        Button confirmBuddyBtn = UIHelper.MakeButton("ConfirmBuddyBtn", buddyRt, "✔ Confirm Standby Buddy Present", 24f, new Color(0.0f, 0.55f, 0.30f), Color.white, 14f);
        RectTransform cBuddyRt = confirmBuddyBtn.GetComponent<RectTransform>();
        cBuddyRt.anchoredPosition = new Vector2(0, -80);
        UIHelper.SetSize(cBuddyRt, 640f, 64f);
        confirmBuddyBtn.onClick.AddListener(OnBuddyConfirmed);

        buddyCard = buddyRt.gameObject;
        buddyCard.SetActive(false);

        // 6. COMPLETION / ASSESSMENT CARD (Aligned to Fire's Existing Result Pattern)
        RectTransform compCard = UIHelper.MakeRect("CompletionCard", canvasObject.transform);
        compCard.anchorMin = new Vector2(0.5f, 0f);
        compCard.anchorMax = new Vector2(0.5f, 0f);
        compCard.pivot = new Vector2(0.5f, 0f);
        UIHelper.SetSize(compCard, 960f, 520f);
        compCard.anchoredPosition = new Vector2(0, 52f);

        Image compBg = compCard.gameObject.AddComponent<Image>();
        compBg.color = new Color(0.06f, 0.10f, 0.18f, 0.95f);
        UIHelper.SetImageRoundedSprite(compBg, 30f);

        Outline compOutline = compCard.gameObject.AddComponent<Outline>();
        compOutline.effectColor = new Color(1f, 1f, 1f, 0.15f);
        compOutline.effectDistance = new Vector2(1.5f, -1.5f);

        VerticalLayoutGroup vlg = compCard.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(36, 36, 30, 30);
        vlg.spacing = 16;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        // Title: 38px bold white
        completionTitleText = UIHelper.MakeLabel("CompTitle", compCard, "SURAKSHAAR GAS SAFETY DRILL RESULT", 38f, Color.white, TextAlignmentOptions.Center, true, true);
        completionTitleText.overflowMode = TextOverflowModes.Overflow;
        completionTitleText.rectTransform.sizeDelta = new Vector2(880f, 60f);
        UIHelper.SetLayout(completionTitleText.gameObject, preferredWidth: 880f, minWidth: 880f, preferredHeight: 60f);

        // Subtitle: 30px bold mint
        compSubtitleText = UIHelper.MakeLabel("CompSubtitle", compCard, "Status: Competent", 30f, Hex("#4ADE80"), TextAlignmentOptions.Center, true, true);
        compSubtitleText.overflowMode = TextOverflowModes.Overflow;
        compSubtitleText.rectTransform.sizeDelta = new Vector2(880f, 44f);
        UIHelper.SetLayout(compSubtitleText.gameObject, preferredWidth: 880f, minWidth: 880f, preferredHeight: 44f);

        // Stats Row (Score + Result)
        RectTransform statRow = UIHelper.MakeHorizontal("StatRow", compCard, 28f);
        statRow.sizeDelta = new Vector2(620f, 46f);
        UIHelper.SetLayout(statRow.gameObject, preferredWidth: 620f, minWidth: 500f, preferredHeight: 46f);
        HorizontalLayoutGroup statHlg = statRow.GetComponent<HorizontalLayoutGroup>();
        if (statHlg != null)
        {
            statHlg.childControlWidth = false;
            statHlg.childControlHeight = false;
            statHlg.childAlignment = TextAnchor.MiddleCenter;
        }

        TextMeshProUGUI scoreLbl = UIHelper.MakeLabel("ScoreLbl", statRow, "Score", 26f, Hex("#94A3B8"), TextAlignmentOptions.Right, false);
        scoreLbl.rectTransform.sizeDelta = new Vector2(90f, 40f);
        UIHelper.SetLayout(scoreLbl.gameObject, preferredWidth: 90f, minWidth: 70f, preferredHeight: 40f);

        compScoreValText = UIHelper.MakeLabel("ScoreVal", statRow, "100%", 30f, Color.white, TextAlignmentOptions.Left, true);
        compScoreValText.rectTransform.sizeDelta = new Vector2(110f, 40f);
        UIHelper.SetLayout(compScoreValText.gameObject, preferredWidth: 110f, minWidth: 90f, preferredHeight: 40f);

        TextMeshProUGUI resultLbl = UIHelper.MakeLabel("ResultLbl", statRow, "Result", 26f, Hex("#94A3B8"), TextAlignmentOptions.Right, false);
        resultLbl.rectTransform.sizeDelta = new Vector2(90f, 40f);
        UIHelper.SetLayout(resultLbl.gameObject, preferredWidth: 90f, minWidth: 70f, preferredHeight: 40f);

        compResultValText = UIHelper.MakeLabel("ResultVal", statRow, "PASS", 30f, Hex("#4ADE80"), TextAlignmentOptions.Left, true);
        compResultValText.rectTransform.sizeDelta = new Vector2(120f, 40f);
        UIHelper.SetLayout(compResultValText.gameObject, preferredWidth: 120f, minWidth: 90f, preferredHeight: 40f);

        // Details / Description: 22px
        completionDetailsText = UIHelper.MakeLabel("CompDetails", compCard, "Emergency response sequence successfully executed in accordance with approved site safety procedures.", 22f, Hex("#E2E8F0"), TextAlignmentOptions.Center, false, true);
        completionDetailsText.overflowMode = TextOverflowModes.Overflow;
        completionDetailsText.rectTransform.sizeDelta = new Vector2(880f, 54f);
        UIHelper.SetLayout(completionDetailsText.gameObject, preferredWidth: 880f, minWidth: 880f, preferredHeight: 54f);

        // Primary Action CTA Button — 30px text, 80px height (matches Fire's _btnCompContinue)
        compContinueButton = UIHelper.MakeButton("btn-assessment", compCard, "Continue to Assessment →", 30f,
            Hex("#1E3A8A"), Color.white, 18f);
        RectTransform compBtnRt = compContinueButton.GetComponent<RectTransform>();
        compBtnRt.sizeDelta = new Vector2(880f, 80f);
        UIHelper.SetLayout(compContinueButton.gameObject, preferredWidth: 880f, minWidth: 880f, preferredHeight: 80f);
        compContinueButton.onClick.AddListener(OnFinalResultAction);

        completionCard = compCard.gameObject;
        completionCard.SetActive(false);

        // 7. DETECTOR DISPLAY CARD (Step 7 Normal Gas Reading HUD)
        RectTransform detRt = UIHelper.MakeRect("DetectorDisplayCard", canvasObject.transform);
        UIHelper.SetSize(detRt, 860f, 420f);
        detRt.anchoredPosition = new Vector2(0, 320f);
        Image detBg = detRt.gameObject.AddComponent<Image>();
        detBg.color = new Color(0.04f, 0.08f, 0.14f, 0.96f);
        UIHelper.SetImageRoundedSprite(detBg, 20f);

        // Header Banner inside card
        RectTransform detHeader = UIHelper.MakeRect("DetHeader", detRt);
        detHeader.anchorMin = new Vector2(0f, 0.80f);
        detHeader.anchorMax = new Vector2(1f, 1f);
        detHeader.offsetMin = Vector2.zero;
        detHeader.offsetMax = Vector2.zero;
        Image detHdrBg = detHeader.gameObject.AddComponent<Image>();
        detHdrBg.color = new Color(0.06f, 0.14f, 0.22f, 0.95f);
        UIHelper.SetImageRoundedSprite(detHdrBg, 14f);

        TextMeshProUGUI detTitle = UIHelper.MakeLabel("DetTitle", detHeader,
            "INDUSTRIAL MULTI-GAS DETECTOR (4-GAS)", 22f, new Color(0.0f, 0.85f, 1.0f),
            TextAlignmentOptions.Center, true);
        detTitle.rectTransform.anchorMin = Vector2.zero;
        detTitle.rectTransform.anchorMax = Vector2.one;
        detTitle.rectTransform.offsetMin = Vector2.zero;
        detTitle.rectTransform.offsetMax = Vector2.zero;

        // Reading Content Grid Panel
        RectTransform gridPanel = UIHelper.MakeRect("ReadingGrid", detRt);
        gridPanel.anchorMin = new Vector2(0.04f, 0.22f);
        gridPanel.anchorMax = new Vector2(0.96f, 0.78f);
        gridPanel.offsetMin = Vector2.zero;
        gridPanel.offsetMax = Vector2.zero;
        Image gridBg = gridPanel.gameObject.AddComponent<Image>();
        gridBg.color = new Color(0.02f, 0.04f, 0.08f, 0.96f);
        UIHelper.SetImageRoundedSprite(gridBg, 12f);

        // 4 Digital Gas Readout Channels: CO 0 ppm | LEL 0 % | O2 20.9 % | H2S 0 ppm
        detCoLabel = UIHelper.MakeLabel("GasCO", gridPanel,
            "<color=#00D4FF>CO</color>       <b><color=#00FF88>0</color></b> ppm\n<size=16><color=#88A0B8>Carbon Monoxide</color></size>",
            24f, Color.white, TextAlignmentOptions.Center, false);
        detCoLabel.rectTransform.anchorMin = new Vector2(0.02f, 0.52f);
        detCoLabel.rectTransform.anchorMax = new Vector2(0.48f, 0.96f);
        detCoLabel.rectTransform.offsetMin = Vector2.zero;
        detCoLabel.rectTransform.offsetMax = Vector2.zero;

        detLelLabel = UIHelper.MakeLabel("GasLEL", gridPanel,
            "<color=#00D4FF>LEL</color>      <b><color=#00FF88>0</color></b> %\n<size=16><color=#88A0B8>Combustibles</color></size>",
            24f, Color.white, TextAlignmentOptions.Center, false);
        detLelLabel.rectTransform.anchorMin = new Vector2(0.52f, 0.52f);
        detLelLabel.rectTransform.anchorMax = new Vector2(0.98f, 0.96f);
        detLelLabel.rectTransform.offsetMin = Vector2.zero;
        detLelLabel.rectTransform.offsetMax = Vector2.zero;

        detO2Label = UIHelper.MakeLabel("GasO2", gridPanel,
            "<color=#00D4FF>O₂</color>       <b><color=#00FF88>20.9</color></b> %\n<size=16><color=#88A0B8>Oxygen (Normal)</color></size>",
            24f, Color.white, TextAlignmentOptions.Center, false);
        detO2Label.rectTransform.anchorMin = new Vector2(0.02f, 0.04f);
        detO2Label.rectTransform.anchorMax = new Vector2(0.48f, 0.48f);
        detO2Label.rectTransform.offsetMin = Vector2.zero;
        detO2Label.rectTransform.offsetMax = Vector2.zero;

        detH2sLabel = UIHelper.MakeLabel("GasH2S", gridPanel,
            "<color=#00D4FF>H₂S</color>      <b><color=#00FF88>0</color></b> ppm\n<size=16><color=#88A0B8>Hydrogen Sulfide</color></size>",
            24f, Color.white, TextAlignmentOptions.Center, false);
        detH2sLabel.rectTransform.anchorMin = new Vector2(0.52f, 0.04f);
        detH2sLabel.rectTransform.anchorMax = new Vector2(0.98f, 0.48f);
        detH2sLabel.rectTransform.offsetMin = Vector2.zero;
        detH2sLabel.rectTransform.offsetMax = Vector2.zero;

        // Bottom Status Badge: "STATUS: NORMAL"
        RectTransform statusBadge = UIHelper.MakeRect("StatusBadge", detRt);
        statusBadge.anchorMin = new Vector2(0.12f, 0.03f);
        statusBadge.anchorMax = new Vector2(0.88f, 0.18f);
        statusBadge.offsetMin = Vector2.zero;
        statusBadge.offsetMax = Vector2.zero;
        detStatusBadgeBg = statusBadge.gameObject.AddComponent<Image>();
        detStatusBadgeBg.color = new Color(0.05f, 0.32f, 0.15f, 0.95f);
        UIHelper.SetImageRoundedSprite(detStatusBadgeBg, 12f);

        detStatusText = UIHelper.MakeLabel("StatusLabel", statusBadge,
            "✔ STATUS: NORMAL — ALL READINGS SAFE", 20f, new Color(0.3f, 1.0f, 0.5f),
            TextAlignmentOptions.Center, true);
        detStatusText.rectTransform.anchorMin = Vector2.zero;
        detStatusText.rectTransform.anchorMax = Vector2.one;
        detStatusText.rectTransform.offsetMin = Vector2.zero;
        detStatusText.rectTransform.offsetMax = Vector2.zero;

        detectorDisplayCard = detRt.gameObject;
        detectorDisplayCard.SetActive(false);
    }

    private void UpdateDetectorCardValues(bool isGasDetected)
    {
        if (detCoLabel != null)
        {
            detCoLabel.text = isGasDetected
                ? "<color=#00D4FF>CO</color>       <b><color=#FF4444>35</color></b> ppm\n<size=16><color=#FF8888>Carbon Monoxide</color></size>"
                : "<color=#00D4FF>CO</color>       <b><color=#00FF88>0</color></b> ppm\n<size=16><color=#88A0B8>Carbon Monoxide</color></size>";
        }
        if (detLelLabel != null)
        {
            detLelLabel.text = isGasDetected
                ? "<color=#00D4FF>LEL</color>      <b><color=#FF4444>25</color></b> %\n<size=16><color=#FF8888>Combustibles</color></size>"
                : "<color=#00D4FF>LEL</color>      <b><color=#00FF88>0</color></b> %\n<size=16><color=#88A0B8>Combustibles</color></size>";
        }
        if (detO2Label != null)
        {
            detO2Label.text = isGasDetected
                ? "<color=#00D4FF>O₂</color>       <b><color=#FF9933>19.1</color></b> %\n<size=16><color=#FFAA55>Oxygen (Deficient)</color></size>"
                : "<color=#00D4FF>O₂</color>       <b><color=#00FF88>20.9</color></b> %\n<size=16><color=#88A0B8>Oxygen (Normal)</color></size>";
        }
        if (detH2sLabel != null)
        {
            detH2sLabel.text = "<color=#00D4FF>H₂S</color>      <b><color=#00FF88>0</color></b> ppm\n<size=16><color=#88A0B8>Hydrogen Sulfide</color></size>";
        }
        if (detStatusText != null)
        {
            detStatusText.text = isGasDetected
                ? "⚠ STATUS: HAZARD DETECTED — ALARM ACTIVE"
                : "✔ STATUS: NORMAL — ALL READINGS SAFE";
            detStatusText.color = isGasDetected ? new Color(1.0f, 0.35f, 0.35f) : new Color(0.3f, 1.0f, 0.5f);
        }
        if (detStatusBadgeBg != null)
        {
            detStatusBadgeBg.color = isGasDetected
                ? new Color(0.55f, 0.10f, 0.10f, 0.95f)
                : new Color(0.05f, 0.32f, 0.15f, 0.95f);
        }
    }

    private void HideAllCards()
    {
        if (alertBanner != null) alertBanner.SetActive(false);
        if (ppeCard != null) ppeCard.SetActive(false);
        if (ppeFeedbackText != null) ppeFeedbackText.gameObject.SetActive(false);
        if (ppeContinueButtonGO != null) ppeContinueButtonGO.SetActive(false);
        if (buddyCard != null) buddyCard.SetActive(false);
        if (completionCard != null) completionCard.SetActive(false);
        if (detectorDisplayCard != null) detectorDisplayCard.SetActive(false);
        if (actionButtonGO != null) actionButtonGO.SetActive(false);
        ShowPreLeakIndicator(false);
    }

    public void SetStep(TrainingStep step)
    {
        StopVoicePlayback();
        if (canvasObject == null) CreateRuntimeUI();
        currentStep = step;
        stepStartTime = Time.time;
        HideAllCards();

        // During PPECheck and Final Result states, hide generic bottom instruction panel completely
        bool isPpeOrResult = (step == TrainingStep.PPECheck || step == TrainingStep.PassResult || step == TrainingStep.RetrainingResult || step == TrainingStep.Completion);
        if (bottomInstructionCard != null)
        {
            bottomInstructionCard.SetActive(!isPpeOrResult);
        }

        switch (step)
        {
            case TrainingStep.ScanAndPlace:
                if (headerStepTitleText != null) headerStepTitleText.text = "GAS & CONFINED SPACE SAFETY";
                if (headerStepSubText != null) headerStepSubText.text = "Scan & Place";
                if (instructionText != null) instructionText.text = "Scan the floor and place the gas safety workstation.";
                break;

            case TrainingStep.WorkstationReady:
            case TrainingStep.Intro:
                if (headerStepTitleText != null) headerStepTitleText.text = "STEP 2 — CHECK GAS DETECTOR";
                if (headerStepSubText != null) headerStepSubText.text = "Initial Atmosphere Check";
                if (instructionText != null)
                {
                    instructionText.text = "First, check the gas detector.\nTap the orange gas detector to verify the atmosphere is normal.";
                }
                if (detectorDisplayCard != null) detectorDisplayCard.SetActive(false);
                break;

            case TrainingStep.CylinderInteraction:
                cylinderInteracted = false;
                if (headerStepTitleText != null) headerStepTitleText.text = "GAS LEAK SCENARIO";
                if (headerStepSubText != null) headerStepSubText.text = "IDENTIFY LEAK POINT";
                if (instructionText != null)
                {
                    instructionText.text = "Inspect the gas cylinder and pipeline.\nTap the highlighted connection to identify the simulated leak.";
                }
                if (detectorDisplayCard != null) detectorDisplayCard.SetActive(false);
                if (alertBanner != null) alertBanner.SetActive(false);
                if (actionButtonGO != null) actionButtonGO.SetActive(false);
                ShowPreLeakIndicator(true);
                break;

            case TrainingStep.GasLeakActive:
            case TrainingStep.LeakRecognition:
                if (headerStepTitleText != null) headerStepTitleText.text = "GAS LEAK DETECTED!";
                if (headerStepSubText != null) headerStepSubText.text = "CYLINDER CONNECTION LEAK";
                if (instructionText != null)
                {
                    instructionText.text = "Gas is leaking from the cylinder connection.\nObserve the leak carefully.";
                }
                if (alertBanner != null)
                {
                    alertBanner.SetActive(true);
                    if (alertBannerText != null) alertBannerText.text = "⚠ GAS LEAK DETECTED!";
                }
                ShowPreLeakIndicator(false);
                ShowActionButton("CONTINUE");
                break;

            case TrainingStep.DetectorInteraction:
            case TrainingStep.DetectorRecognition:
                detectorPickedUp = false;
                detectorScanned = false;
                if (headerStepTitleText != null) headerStepTitleText.text = "DETECT THE GAS";
                if (headerStepSubText != null) headerStepSubText.text = "Pick Up Detector";
                if (instructionText != null)
                {
                    instructionText.text = "The gas leak is active.\nTap the gas detector to pick it up.";
                }
                if (alertBanner != null)
                {
                    alertBanner.SetActive(true);
                    if (alertBannerText != null) alertBannerText.text = "⚠ GAS LEAK DETECTED!";
                }
                RemoveHazardZoneVisualDisk();
                if (detectorDisplayCard != null) detectorDisplayCard.SetActive(false);
                if (ppeCard != null) ppeCard.SetActive(false);
                if (buddyCard != null) buddyCard.SetActive(false);
                if (actionButtonGO != null) actionButtonGO.SetActive(false);
                break;

            case TrainingStep.DetectorChecked:
            case TrainingStep.HazardRecognition:
                hazardRecognized = false;
                if (headerStepTitleText != null) headerStepTitleText.text = "IDENTIFY HAZARD AREA";
                if (headerStepSubText != null) headerStepSubText.text = "Perimeter Identification";
                if (instructionText != null)
                {
                    instructionText.text = "Gas detected.\nIdentify the hazardous area before proceeding.\n\nTap inside the marked hazard zone.";
                }
                if (alertBanner != null) alertBanner.SetActive(true);
                if (detectorDisplayCard != null) detectorDisplayCard.SetActive(true);
                CreateHazardZoneVisualDisk();
                if (ppeCard != null) ppeCard.SetActive(false);
                if (buddyCard != null) buddyCard.SetActive(false);
                if (actionButtonGO != null) actionButtonGO.SetActive(false);
                break;

            case TrainingStep.PPECheck:
                ReturnDetectorToOriginalPosition();

                ppeHardhatSelected = false;
                ppeGogglesSelected = false;
                ppeGlovesSelected = false;
                ppeBootsSelected = false;
                ppeComplete = false;
                ppeSelectedCorrect = false;

                if (headerStepTitleText != null) headerStepTitleText.text = "STEP 4 — SELECT REQUIRED PPE";
                if (headerStepSubText != null) headerStepSubText.text = "Workstation PPE Selection";
                if (bottomInstructionCard != null) bottomInstructionCard.SetActive(false);
                if (alertBanner != null) alertBanner.SetActive(false);
                if (actionButtonGO != null) actionButtonGO.SetActive(false);

                EnsurePpeColliders();
                UpdatePpeCardUI();
                if (ppeCard != null) ppeCard.SetActive(true);
                break;

            case TrainingStep.BuddySystemCheck:
                if (headerStepTitleText != null) headerStepTitleText.text = "STEP 5 — BUDDY SYSTEM";
                if (headerStepSubText != null) headerStepSubText.text = "Standby Buddy Verification";
                if (instructionText != null) instructionText.text = "Confirm the designated buddy/standby procedure.";
                if (alertBanner != null) alertBanner.SetActive(true);
                if (buddyCard != null) buddyCard.SetActive(true);
                break;

            case TrainingStep.SafetyResponse:
                if (headerStepTitleText != null) headerStepTitleText.text = "STEP 6 — GAS ISOLATION";
                if (headerStepSubText != null) headerStepSubText.text = "Tap Red Isolation Valve";
                if (instructionText != null) instructionText.text = "Tap the RED ISOLATION VALVE to perform the simulated gas isolation.";
                if (alertBanner != null) alertBanner.SetActive(true);
                CreateHazardZoneVisualDisk();
                // NO GENERIC NEXT BUTTON — Worker MUST tap 3D Red Isolation Valve
                break;

            case TrainingStep.IncidentResolved:
                if (headerStepTitleText != null) headerStepTitleText.text = "✓ GAS ISOLATED";
                if (headerStepSubText != null) headerStepSubText.text = "Leak Stopped";
                if (instructionText != null) instructionText.text = "Leak stopped.\n\nVerify that the detector is returning to normal.";
                RemoveHazardZoneVisualDisk();
                ShowActionButton("Verify Safe State");
                break;

            case TrainingStep.PostResponseVerification:
                if (headerStepTitleText != null) headerStepTitleText.text = "STEP 7 — VERIFY SAFE STATE";
                if (headerStepSubText != null) headerStepSubText.text = "Atmosphere Normalizing";
                if (instructionText != null) instructionText.text = "Confirm that the gas leak has stopped and the detector has returned toward normal.";
                RemoveHazardZoneVisualDisk();
                ShowActionButton("Complete Response Verification");
                break;

            case TrainingStep.FinalAssessment:
                EvaluateAssessment();
                break;

            case TrainingStep.PassResult:
                if (headerStepTitleText != null) headerStepTitleText.text = "SURAKSHAAR GAS SAFETY DRILL RESULT";
                if (headerStepSubText != null) headerStepSubText.text = "✔ Status: Competent";
                ShowCompletionCard(true);
                break;

            case TrainingStep.RetrainingResult:
                if (headerStepTitleText != null) headerStepTitleText.text = "SURAKSHAAR GAS SAFETY DRILL RESULT";
                if (headerStepSubText != null) headerStepSubText.text = "⚠ Status: Needs Review";
                ShowCompletionCard(false);
                break;

            case TrainingStep.TargetedRetraining:
                if (headerStepTitleText != null) headerStepTitleText.text = "TARGETED RETRAINING";
                if (headerStepSubText != null) headerStepSubText.text = "Safety Protocol Review";
                ShowTargetedRetrainingText();
                ShowActionButton("Re-attempt Gas Drill");
                break;

            case TrainingStep.ConfinedSpaceIntro:
                if (headerStepTitleText != null) headerStepTitleText.text = "GAS LEAK RESPONSE COMPLETE";
                if (headerStepSubText != null) headerStepSubText.text = "Next: CONFINED SPACE SAFETY CHECK";
                if (instructionText != null) instructionText.text = "Gas leak response complete.\n\nNext:\nCONFINED SPACE SAFETY CHECK\n\nTap the 3D Confined Space Vessel to begin pre-entry check.";
                ShowActionButton("Begin Pre-Entry Drill");
                break;

            case TrainingStep.ConfinedSpaceDrill:
                if (headerStepTitleText != null) headerStepTitleText.text = "CONFINED SPACE SAFETY DRILL";
                if (headerStepSubText != null) headerStepSubText.text = "Ventilation & Lifeline Protocol";
                if (instructionText != null) instructionText.text = "Verify continuous ventilation blower operation and safety lifeline attachment prior to entry.";
                ShowActionButton("Complete Pre-Entry Protocol");
                break;

            case TrainingStep.Completion:
                if (headerStepTitleText != null) headerStepTitleText.text = "SURAKSHAAR SAFETY DRILL COMPLETED";
                if (headerStepSubText != null) headerStepSubText.text = "Certified Competent";
                if (instructionText != null) instructionText.text = "Gas Leak & Confined Space Safety Module fully completed!";
                ShowCompletionCard(true);
                break;

            case TrainingStep.Reassessment:
                SetStep(TrainingStep.WorkstationReady);
                break;
        }

        Debug.Log($"[GasAR Flow Engine] Training Step Changed -> {step}");
    }

    private void ShowActionButton(string label)
    {
        if (actionButtonText != null) actionButtonText.text = label;
        if (actionButtonGO != null) actionButtonGO.SetActive(true);
    }

    private void OnActionButtonClicked()
    {
        switch (currentStep)
        {
            case TrainingStep.WorkstationReady:
            case TrainingStep.Intro:
                if (detectorDisplayCard != null) detectorDisplayCard.SetActive(false);
                if (actionButtonGO != null) actionButtonGO.SetActive(false);
                SetStep(TrainingStep.CylinderInteraction);
                break;

            case TrainingStep.CylinderInteraction:
            case TrainingStep.GasLeakActive:
            case TrainingStep.LeakRecognition:
                if (actionButtonGO != null) actionButtonGO.SetActive(false);
                SetStep(TrainingStep.DetectorInteraction);
                break;

            case TrainingStep.DetectorInteraction:
            case TrainingStep.DetectorRecognition:
                if (detectorPickedUp && !detectorScanned)
                {
                    StartCoroutine(PerformAtmosphereScan());
                }
                else if (detectorScanned)
                {
                    if (actionButtonGO != null) actionButtonGO.SetActive(false);
                    SetStep(TrainingStep.DetectorChecked);
                }
                break;

            case TrainingStep.DetectorChecked:
            case TrainingStep.HazardRecognition:
                if (hazardRecognized)
                {
                    if (actionButtonGO != null) actionButtonGO.SetActive(false);
                    SetStep(TrainingStep.PPECheck);
                }
                break;

            case TrainingStep.PPECheck:
                OnPpeContinueClicked();
                break;

            case TrainingStep.IncidentResolved:
                SetStep(TrainingStep.PostResponseVerification);
                break;

            case TrainingStep.PostResponseVerification:
                verificationComplete = true;
                if (instructionText != null) instructionText.text = "✓ Response Complete\n\nReview your actions before assessment.";
                StartCoroutine(AutoAdvance(1.0f, TrainingStep.FinalAssessment));
                break;

            case TrainingStep.PassResult:
                OnFinalResultAction();
                break;

            case TrainingStep.ConfinedSpaceIntro:
                SetStep(TrainingStep.ConfinedSpaceDrill);
                break;

            case TrainingStep.ConfinedSpaceDrill:
                SetStep(TrainingStep.Completion);
                break;

            case TrainingStep.TargetedRetraining:
                SetStep(TrainingStep.Reassessment);
                break;
        }
    }

    public void OnScenarioPlaced()
    {
        if (currentStep == TrainingStep.ScanAndPlace)
        {
            Debug.Log("[GasAR] Workstation placed on AR surface. Advancing to WorkstationReady.");
            if (GasScenarioAssessmentBridge.Instance != null)
            {
                GasScenarioAssessmentBridge.Instance.StartGasScenario("gas_leak_drill_01");
            }
            SetStep(TrainingStep.WorkstationReady);
        }
    }

    public void OnCylinderOrLeakPointTapped()
    {
        if (currentStep == TrainingStep.ScanAndPlace) return;

        if (currentStep == TrainingStep.WorkstationReady || currentStep == TrainingStep.Intro)
        {
            // Detector check must be completed first; do not trigger leak
            OnWrongObjectTapped("Detector");
            return;
        }

        if (currentStep == TrainingStep.CylinderInteraction)
        {
            if (!cylinderInteracted)
            {
                cylinderInteracted = true;
                leakRecognized = true;
                Debug.Log("[GasAR] 3D Gas Cylinder / Simulated Leak Point tapped! Step 8 Gas Leak Started.");

                // 1. Hide pre-leak prompt indicator
                ShowPreLeakIndicator(false);

                // 2. Locate real 3D cylinder object
                GameObject cylinderObj = GameObject.Find("Realistic_Gas_Cylinder");
                if (cylinderObj == null) cylinderObj = GameObject.Find("gas_cylinder");
                if (cylinderObj == null && environmentBuilder != null) cylinderObj = environmentBuilder.CurrentGasCylinder;

                // 3. Start existing gas leak visual controller
                if (leakVisualController != null && cylinderObj != null)
                {
                    leakVisualController.StartGasLeak(cylinderObj);
                }

                // 4. Highlight leak point to ensure it remains visually identifiable
                Transform leakPt = cylinderObj != null ? cylinderObj.transform.Find("LeakPoint") : null;
                if (leakPt != null)
                {
                    StartCoroutine(HighlightLeakPoint(leakPt.gameObject));
                    StartLeakHiss(leakPt);
                }
                else if (cylinderObj != null)
                {
                    StartLeakHiss(cylinderObj.transform);
                }

                // 5. Update state to GasLeakActive with clear post-leak instructions & CONTINUE button
                SetStep(TrainingStep.GasLeakActive);

                // 6. Check existing audio asset (no gas audio available in project; cleanly skip)
                if (audioSource != null && audioSource.clip != null)
                {
                    audioSource.Play();
                }

                // 7. Assessment notification
                if (GasScenarioAssessmentBridge.Instance != null)
                {
                    GasScenarioAssessmentBridge.Instance.OnGasLeakDetected("simulated_gas_leak");
                }
            }
        }
    }

    private void ShowPreLeakIndicator(bool show)
    {
        if (!show)
        {
            if (preLeakPulseCoroutine != null)
            {
                StopCoroutine(preLeakPulseCoroutine);
                preLeakPulseCoroutine = null;
            }
            if (preLeakIndicatorGO != null)
            {
                preLeakIndicatorGO.SetActive(false);
            }
            return;
        }

        GameObject cylinderObj = GameObject.Find("Realistic_Gas_Cylinder");
        if (cylinderObj == null) cylinderObj = GameObject.Find("gas_cylinder");
        if (cylinderObj == null && environmentBuilder != null) cylinderObj = environmentBuilder.CurrentGasCylinder;
        if (cylinderObj == null) return;

        Transform leakPt = cylinderObj.transform.Find("LeakPoint");
        if (leakPt == null)
        {
            GameObject lpObj = new GameObject("LeakPoint");
            lpObj.transform.SetParent(cylinderObj.transform, false);
            leakPt = lpObj.transform;
        }

        // Align leak point to physical upper valve / regulator connection
        GameObject regulatorObj = GameObject.Find("Real_Gas_Regulator");
        if (regulatorObj != null)
        {
            leakPt.position = regulatorObj.transform.position + new Vector3(-0.035f, -0.01f, 0f);
        }
        else
        {
            leakPt.position = cylinderObj.transform.position + new Vector3(0.04f, 0.61f, 0f);
        }

        if (preLeakIndicatorGO == null)
        {
            preLeakIndicatorGO = new GameObject("PreLeakIndicator");
            preLeakIndicatorGO.transform.position = leakPt.position;

            // 1. Interactive contact ring at the cylinder/regulator connection
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "MarkerSphere"; // Maps hit category to "Cylinder"
            ring.transform.SetParent(preLeakIndicatorGO.transform, false);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            ring.transform.localScale = new Vector3(0.045f, 0.002f, 0.045f);

            SphereCollider col = ring.GetComponent<SphereCollider>();
            if (col == null) col = ring.AddComponent<SphereCollider>();
            col.radius = 0.8f; // Responsive touch volume

            Shader s = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            if (s != null)
            {
                Material m = new Material(s);
                m.color = new Color(1.0f, 0.75f, 0.18f, 0.90f); // Amber accent
                Renderer r = ring.GetComponent<Renderer>();
                if (r != null) r.material = m;
            }

            // 2. Compact professional AR training callout badge (floating ~0.12m above connection)
            GameObject calloutGo = new GameObject("LeakCallout");
            calloutGo.transform.SetParent(preLeakIndicatorGO.transform, false);
            calloutGo.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            calloutGo.transform.localScale = Vector3.one * 0.0009f; // World width ~0.225m, height ~0.068m

            Canvas canvas = calloutGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            calloutGo.AddComponent<GraphicRaycaster>();

            RectTransform calloutRt = calloutGo.GetComponent<RectTransform>();
            calloutRt.sizeDelta = new Vector2(250f, 75f);

            // Box collider so tapping anywhere on the floating badge triggers interaction
            BoxCollider calloutCol = calloutGo.AddComponent<BoxCollider>();
            calloutCol.size = new Vector3(250f, 75f, 15f);

            // Outer amber border / glow container
            Image borderImg = calloutGo.AddComponent<Image>();
            borderImg.color = new Color(1.0f, 0.72f, 0.15f, 0.85f);
            UIHelper.SetImageRoundedSprite(borderImg, 14f);

            // Inner dark charcoal/navy background (slightly translucent)
            RectTransform innerRt = UIHelper.MakeRect("InnerCard", calloutRt);
            innerRt.anchorMin = Vector2.zero;
            innerRt.anchorMax = Vector2.one;
            innerRt.offsetMin = new Vector2(2.5f, 2.5f);
            innerRt.offsetMax = new Vector2(-2.5f, -2.5f);
            Image innerBg = innerRt.gameObject.AddComponent<Image>();
            innerBg.color = new Color(0.06f, 0.09f, 0.14f, 0.92f);
            UIHelper.SetImageRoundedSprite(innerBg, 12f);

            // Left amber location/target icon
            RectTransform iconRt = UIHelper.MakeRect("CalloutIcon", innerRt);
            iconRt.anchorMin = new Vector2(0f, 0f);
            iconRt.anchorMax = new Vector2(0.25f, 1f);
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;
            TextMeshProUGUI iconTmp = UIHelper.MakeLabel("IconText", iconRt,
                "<b>◎</b>", 28f, new Color(1.0f, 0.75f, 0.18f),
                TextAlignmentOptions.Center, true);
            iconTmp.rectTransform.anchorMin = Vector2.zero;
            iconTmp.rectTransform.anchorMax = Vector2.one;
            iconTmp.rectTransform.offsetMin = Vector2.zero;
            iconTmp.rectTransform.offsetMax = Vector2.zero;

            // Content text area
            RectTransform textPanelRt = UIHelper.MakeRect("TextPanel", innerRt);
            textPanelRt.anchorMin = new Vector2(0.24f, 0f);
            textPanelRt.anchorMax = new Vector2(0.96f, 1f);
            textPanelRt.offsetMin = Vector2.zero;
            textPanelRt.offsetMax = Vector2.zero;

            // Main text: "LEAK POINT" (White, bold, crisp)
            TextMeshProUGUI titleTmp = UIHelper.MakeLabel("CalloutTitle", textPanelRt,
                "LEAK POINT", 21f, Color.white,
                TextAlignmentOptions.Left, true);
            titleTmp.rectTransform.anchorMin = new Vector2(0f, 0.46f);
            titleTmp.rectTransform.anchorMax = new Vector2(1f, 0.94f);
            titleTmp.rectTransform.offsetMin = Vector2.zero;
            titleTmp.rectTransform.offsetMax = Vector2.zero;

            // Secondary text: "Tap to identify" (clean blue-gray)
            TextMeshProUGUI subTmp = UIHelper.MakeLabel("CalloutSub", textPanelRt,
                "Tap to identify", 14f, new Color(0.72f, 0.82f, 0.92f),
                TextAlignmentOptions.Left, false);
            subTmp.rectTransform.anchorMin = new Vector2(0f, 0.08f);
            subTmp.rectTransform.anchorMax = new Vector2(1f, 0.48f);
            subTmp.rectTransform.offsetMin = Vector2.zero;
            subTmp.rectTransform.offsetMax = Vector2.zero;
        }
        else
        {
            preLeakIndicatorGO.transform.position = leakPt.position;
            preLeakIndicatorGO.SetActive(true);
        }

        if (preLeakPulseCoroutine != null) StopCoroutine(preLeakPulseCoroutine);
        preLeakPulseCoroutine = StartCoroutine(PulsePreLeakIndicator(preLeakIndicatorGO));
    }

    private IEnumerator PulsePreLeakIndicator(GameObject indicator)
    {
        if (indicator == null) yield break;

        Transform ring = indicator.transform.Find("MarkerSphere");
        Transform callout = indicator.transform.Find("LeakCallout");
        Image borderImg = callout != null ? callout.GetComponent<Image>() : null;
        TextMeshProUGUI iconText = callout != null ? callout.Find("InnerCard/CalloutIcon/IconText")?.GetComponent<TextMeshProUGUI>() : null;
        Vector3 baseRingScale = ring != null ? ring.localScale : new Vector3(0.045f, 0.002f, 0.045f);

        while (indicator.activeInHierarchy)
        {
            Camera cam = Camera.main ?? FindFirstObjectByType<Camera>();
            if (cam != null && callout != null)
            {
                // Billboard toward the AR camera
                callout.rotation = Quaternion.LookRotation(callout.position - cam.transform.position);
            }

            float t = (Mathf.Sin(Time.time * 3.5f) + 1.0f) * 0.5f;

            // Subtle ring pulse at physical connection
            if (ring != null)
            {
                ring.localScale = baseRingScale * Mathf.Lerp(0.92f, 1.16f, t);
            }

            // Subtle amber glow pulse on callout border and icon
            if (borderImg != null)
            {
                borderImg.color = Color.Lerp(new Color(1.0f, 0.72f, 0.15f, 0.55f), new Color(1.0f, 0.85f, 0.25f, 0.95f), t);
            }
            if (iconText != null)
            {
                iconText.color = Color.Lerp(new Color(1.0f, 0.72f, 0.15f, 0.75f), new Color(1.0f, 0.92f, 0.35f, 1.0f), t);
            }

            yield return null;
        }
    }

    private IEnumerator HighlightLeakPoint(GameObject target)
    {
        if (target == null) yield break;

        Transform existingIndicator = target.transform.Find("LeakPointIndicator");
        GameObject indGo;
        if (existingIndicator == null)
        {
            indGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indGo.name = "LeakPointIndicator";
            indGo.transform.SetParent(target.transform, false);
            indGo.transform.localPosition = Vector3.zero;
            indGo.transform.localRotation = Quaternion.identity;
            indGo.transform.localScale = Vector3.one * 0.08f;

            Collider col = indGo.GetComponent<Collider>();
            if (col != null) Destroy(col);

            Shader s = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            if (s != null)
            {
                Material indMat = new Material(s);
                indMat.color = new Color(1.0f, 0.75f, 0.1f, 0.65f);
                if (indMat.HasProperty("_Surface")) indMat.SetFloat("_Surface", 1f);
                Renderer r = indGo.GetComponent<Renderer>();
                if (r != null) r.material = indMat;
            }
        }
        else
        {
            indGo = existingIndicator.gameObject;
            indGo.SetActive(true);
        }

        Vector3 baseScale = indGo.transform.localScale;
        for (int i = 0; i < 4; i++)
        {
            float elapsed = 0f;
            float pulseDur = 0.25f;
            while (elapsed < pulseDur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Sin((elapsed / pulseDur) * Mathf.PI);
                indGo.transform.localScale = baseScale * (1f + 0.35f * t);
                yield return null;
            }
        }
        indGo.transform.localScale = baseScale;
    }

    public void TriggerGasLeakEmergency()
    {
        leakRecognized = true;
        GameObject cylinderObj = environmentBuilder != null ? environmentBuilder.CurrentGasCylinder : GameObject.Find("gas_cylinder");
        if (leakVisualController != null)
        {
            leakVisualController.StartGasLeak(cylinderObj);
        }

        if (GasScenarioAssessmentBridge.Instance != null)
        {
            GasScenarioAssessmentBridge.Instance.OnGasLeakDetected("methane_toxic_gas");
        }

        SetStep(TrainingStep.GasLeakActive);
    }

    public void OnDetectorTapped()
    {
        if (currentStep == TrainingStep.WorkstationReady || currentStep == TrainingStep.Intro)
        {
            detectorInteracted = true;
            Debug.Log("[GasAR] 3D Multi-Gas Detector tapped for Step 7 Detector Check!");

            // 1. Trigger visual highlight and scale pulse on real 3D detector on floor
            GameObject detectorObj = GameObject.Find("Multi_Gas_Detector") ?? GameObject.Find("detector");
            if (detectorObj != null)
            {
                StartCoroutine(HighlightAndPulseDetector(detectorObj));
            }

            // 2. Display the detector normal readings card
            UpdateDetectorCardValues(false);
            if (detectorDisplayCard != null)
            {
                detectorDisplayCard.SetActive(true);
            }

            // 3. Update bottom card UI instructions per Step 7 specification
            if (headerStepTitleText != null) headerStepTitleText.text = "DETECTOR CHECK COMPLETE";
            if (headerStepSubText != null) headerStepSubText.text = "Atmosphere Safe";
            if (instructionText != null)
            {
                instructionText.text = "Atmospheric conditions verified safe.\nAll gas readings are normal.";
            }

            // 4. Enable CONTINUE action button
            ShowActionButton("CONTINUE");
        }
        else if (currentStep == TrainingStep.DetectorInteraction || currentStep == TrainingStep.DetectorRecognition)
        {
            if (!detectorPickedUp)
            {
                GameObject detectorObj = GameObject.Find("Multi_Gas_Detector") ?? GameObject.Find("detector");
                if (detectorObj != null)
                {
                    if (detectorPickupCoroutine != null) StopCoroutine(detectorPickupCoroutine);
                    detectorPickupCoroutine = StartCoroutine(PickUpDetectorRoutine(detectorObj));
                }
            }
        }
    }

    private IEnumerator PickUpDetectorRoutine(GameObject detectorObj)
    {
        if (detectorObj == null) yield break;

        currentDetectorObj = detectorObj;
        if (originalDetectorParent == null)
        {
            originalDetectorParent = detectorObj.transform.parent;
            originalDetectorLocalPosition = detectorObj.transform.localPosition;
            originalDetectorLocalRotation = detectorObj.transform.localRotation;
            originalDetectorLocalScale = detectorObj.transform.localScale;
        }

        Camera cam = Camera.main ?? FindFirstObjectByType<Camera>();
        if (cam == null) yield break;

        Vector3 startWorldPos = detectorObj.transform.position;
        Quaternion startWorldRot = detectorObj.transform.rotation;
        Vector3 baseScale = originalDetectorLocalScale != Vector3.zero ? originalDetectorLocalScale : new Vector3(0.05f, 0.05f, 0.05f);

        float duration = 0.75f;
        float elapsed = 0f;

        // Controlled comfortable inspection pose in front of camera (~0.38m forward, slightly down and right)
        Vector3 inspectionOffset = new Vector3(0.07f, -0.13f, 0.38f);
        Quaternion inspectionRot = Quaternion.Euler(22f, 180f, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            if (cam != null && detectorObj != null)
            {
                Vector3 currentTargetPos = cam.transform.TransformPoint(inspectionOffset);
                Quaternion currentTargetRot = cam.transform.rotation * inspectionRot;

                detectorObj.transform.position = Vector3.Lerp(startWorldPos, currentTargetPos, smoothT);
                detectorObj.transform.rotation = Quaternion.Slerp(startWorldRot, currentTargetRot, smoothT);
                detectorObj.transform.localScale = baseScale; // Maintains realistic handheld scale, NEVER grows huge
            }
            yield return null;
        }

        if (cam != null && detectorObj != null)
        {
            detectorObj.transform.SetParent(cam.transform, true);
            detectorObj.transform.localPosition = inspectionOffset;
            detectorObj.transform.localRotation = inspectionRot;
            detectorObj.transform.localScale = baseScale;
        }

        detectorPickedUp = true;
        StopVoicePlayback();

        // Problem 4 UI state after pickup:
        // Title: "ATMOSPHERE MEASUREMENT"
        // Instruction: "Detector ready.\nTap SCAN to measure the atmosphere."
        // Button: "SCAN ATMOSPHERE"
        if (headerStepTitleText != null) headerStepTitleText.text = "ATMOSPHERE MEASUREMENT";
        if (headerStepSubText != null) headerStepSubText.text = "Handheld Inspection";
        if (instructionText != null)
        {
            instructionText.text = "Detector ready.\nTap SCAN to measure the atmosphere.";
        }
        ShowActionButton("SCAN ATMOSPHERE");
    }

    private IEnumerator PerformAtmosphereScan()
    {
        if (actionButtonGO != null) actionButtonGO.SetActive(false);

        if (headerStepTitleText != null) headerStepTitleText.text = "ATMOSPHERE MEASUREMENT";
        if (headerStepSubText != null) headerStepSubText.text = "Measuring...";
        if (instructionText != null)
        {
            instructionText.text = "MEASURING ATMOSPHERE...";
        }

        if (currentDetectorObj != null)
        {
            StartCoroutine(PulseDetectorScreenWhileMeasuring(currentDetectorObj, 1.8f));
        }

        StopDetectorScanAudio();
        detectorScanAudioCoroutine = StartCoroutine(DetectorScanAudioRoutine(1.8f));

        yield return new WaitForSeconds(1.8f);

        StopDetectorScanAudio();

        detectorScanned = true;
        detectorInteracted = true;
        UpdateDetectorCardValues(true);
        if (detectorDisplayCard != null)
        {
            detectorDisplayCard.SetActive(true);
        }

        if (currentDetectorObj != null)
        {
            if (detectorAlarmCoroutine != null) StopCoroutine(detectorAlarmCoroutine);
            detectorAlarmCoroutine = StartCoroutine(DetectorAlarmRoutine(currentDetectorObj));
        }

        StopVoicePlayback();
        if (headerStepTitleText != null) headerStepTitleText.text = "⚠ GAS DETECTED";
        if (headerStepSubText != null) headerStepSubText.text = "Alarm Active";
        if (instructionText != null)
        {
            instructionText.text = "Gas detected.\nIdentify the hazardous area before proceeding.";
        }
        if (alertBanner != null)
        {
            alertBanner.SetActive(true);
            if (alertBannerText != null) alertBannerText.text = "⚠ HAZARD ACTIVE: GAS DETECTED";
        }

        ShowActionButton("CONTINUE");
    }

    private IEnumerator PulseDetectorScreenWhileMeasuring(GameObject target, float duration)
    {
        if (target == null) yield break;

        Renderer[] rends = target.GetComponentsInChildren<Renderer>(true);
        List<Material> mats = new List<Material>();
        foreach (var r in rends)
        {
            if (r != null && r.materials != null)
            {
                foreach (var m in r.materials)
                {
                    if (m != null && m.HasProperty("_EmissionColor"))
                    {
                        m.EnableKeyword("_EMISSION");
                        mats.Add(m);
                    }
                }
            }
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float pulse = (Mathf.Sin(elapsed * 12f) + 1f) * 0.5f;
            Color pulseColor = Color.Lerp(Color.black, new Color(0.0f, 0.8f, 1.0f) * 1.0f, pulse);
            foreach (var m in mats)
            {
                if (m != null) m.SetColor("_EmissionColor", pulseColor);
            }
            yield return null;
        }

        foreach (var m in mats)
        {
            if (m != null) m.SetColor("_EmissionColor", Color.black);
        }
    }

    private IEnumerator DetectorAlarmRoutine(GameObject target)
    {
        if (target == null) yield break;

        Renderer[] rends = target.GetComponentsInChildren<Renderer>(true);
        List<Material> mats = new List<Material>();
        foreach (var r in rends)
        {
            if (r != null && r.materials != null)
            {
                foreach (var m in r.materials)
                {
                    if (m != null && m.HasProperty("_EmissionColor"))
                    {
                        m.EnableKeyword("_EMISSION");
                        mats.Add(m);
                    }
                }
            }
        }

        while (detectorScanned && target != null)
        {
            foreach (var m in mats)
            {
                if (m != null) m.SetColor("_EmissionColor", new Color(1.0f, 0.1f, 0.05f) * 1.2f);
            }
            PlayDetectorAlarmBeep();

            yield return new WaitForSeconds(0.35f);

            foreach (var m in mats)
            {
                if (m != null) m.SetColor("_EmissionColor", Color.black);
            }
            yield return new WaitForSeconds(0.35f);
        }
    }

    private IEnumerator HighlightAndPulseDetector(GameObject target)
    {
        if (target == null) yield break;

        Vector3 origScale = target.transform.localScale;
        Vector3 peakScale = origScale * 1.18f;

        // Visual highlight: pulse emission on detector materials
        Renderer[] rends = target.GetComponentsInChildren<Renderer>(true);
        List<Material> mats = new List<Material>();
        foreach (var r in rends)
        {
            if (r != null && r.materials != null)
            {
                foreach (var m in r.materials)
                {
                    if (m != null && m.HasProperty("_EmissionColor"))
                    {
                        m.EnableKeyword("_EMISSION");
                        m.SetColor("_EmissionColor", new Color(0.0f, 0.9f, 0.4f) * 0.9f);
                        mats.Add(m);
                    }
                }
            }
        }

        // Smooth scale pulse up
        float elapsed = 0f;
        float durUp = 0.20f;
        while (elapsed < durUp)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / durUp;
            target.transform.localScale = Vector3.Lerp(origScale, peakScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        // Brief hold
        yield return new WaitForSeconds(0.12f);

        // Smooth scale back down
        elapsed = 0f;
        float durDown = 0.22f;
        while (elapsed < durDown)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / durDown;
            target.transform.localScale = Vector3.Lerp(peakScale, origScale, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        target.transform.localScale = origScale;

        // Reset emission after inspection glow
        yield return new WaitForSeconds(0.6f);
        foreach (var m in mats)
        {
            if (m != null && m.HasProperty("_EmissionColor"))
            {
                m.SetColor("_EmissionColor", Color.black);
            }
        }
    }

    public void OnHazardZoneTapped()
    {
        if (currentStep == TrainingStep.DetectorChecked || currentStep == TrainingStep.HazardRecognition)
        {
            if (hazardRecognized) return;
            hazardRecognized = true;

            Debug.Log("[GasAR] Worker correctly identified hazard zone!");

            // 1. Briefly highlight the hazard boundary
            if (hazardHighlightCoroutine != null) StopCoroutine(hazardHighlightCoroutine);
            hazardHighlightCoroutine = StartCoroutine(HighlightHazardBoundary());

            // 2. Play subtle confirmation audio if available
            if (audioSource != null && audioSource.clip != null) audioSource.Play();

            // 3. Update bottom card UI instructions
            if (headerStepTitleText != null) headerStepTitleText.text = "✓ HAZARD ZONE IDENTIFIED";
            if (headerStepSubText != null) headerStepSubText.text = "Perimeter Acknowledged";
            if (instructionText != null)
            {
                instructionText.text = "✓ HAZARD ZONE IDENTIFIED\nPerimeter acknowledged per safety protocol.";
            }

            // 4. Enable CONTINUE button
            ShowActionButton("CONTINUE");

            // 5. Assessment bridge event
            if (GasScenarioAssessmentBridge.Instance != null)
            {
                GasScenarioAssessmentBridge.Instance.OnHazardZonePerimeterIdentified(100f);
            }
        }
    }

    public void OnHazardZoneAcknowledged()
    {
        hazardRecognized = true;
        Debug.Log("[GasAR] Hazard area recognized and acknowledged.");
        SetStep(TrainingStep.PPECheck);
    }

    private void CreatePpeRow(RectTransform parent, string name, float posY, out Image rowBg, out TextMeshProUGUI rowText, string defaultText)
    {
        RectTransform rowRt = UIHelper.MakeRect(name, parent);
        rowRt.anchoredPosition = new Vector2(0, posY);
        UIHelper.SetSize(rowRt, 840f, 52f);

        rowBg = rowRt.gameObject.AddComponent<Image>();
        rowBg.color = new Color(0.10f, 0.15f, 0.22f, 0.85f);
        UIHelper.SetImageRoundedSprite(rowBg, 12f);

        rowText = UIHelper.MakeLabel(name + "_Text", rowRt,
            defaultText, 22f, new Color(0.80f, 0.88f, 0.96f),
            TextAlignmentOptions.Left, true);
        rowText.rectTransform.anchorMin = new Vector2(0f, 0f);
        rowText.rectTransform.anchorMax = new Vector2(1f, 1f);
        rowText.rectTransform.offsetMin = new Vector2(28f, 0f);
        rowText.rectTransform.offsetMax = new Vector2(-20f, 0f);
    }

    private void CacheOriginalDetectorTransform()
    {
        if (currentDetectorObj == null)
        {
            currentDetectorObj = GameObject.Find("Multi_Gas_Detector") ?? GameObject.Find("detector");
        }
        if (currentDetectorObj != null && originalDetectorParent == null)
        {
            originalDetectorParent = currentDetectorObj.transform.parent;
            originalDetectorLocalPosition = currentDetectorObj.transform.localPosition;
            originalDetectorLocalRotation = currentDetectorObj.transform.localRotation;
            originalDetectorLocalScale = currentDetectorObj.transform.localScale;
        }
    }

    private void ReturnDetectorToOriginalPosition()
    {
        if (detectorPickupCoroutine != null)
        {
            StopCoroutine(detectorPickupCoroutine);
            detectorPickupCoroutine = null;
        }

        if (detectorAlarmCoroutine != null)
        {
            StopCoroutine(detectorAlarmCoroutine);
            detectorAlarmCoroutine = null;
        }

        if (currentDetectorObj == null)
        {
            currentDetectorObj = GameObject.Find("Multi_Gas_Detector") ?? GameObject.Find("detector");
        }

        if (currentDetectorObj != null && originalDetectorParent != null)
        {
            currentDetectorObj.transform.SetParent(originalDetectorParent, true);
            currentDetectorObj.transform.localPosition = originalDetectorLocalPosition;
            currentDetectorObj.transform.localRotation = originalDetectorLocalRotation;
            currentDetectorObj.transform.localScale = originalDetectorLocalScale;
            detectorPickedUp = false;

            Renderer[] rends = currentDetectorObj.GetComponentsInChildren<Renderer>(true);
            foreach (var r in rends)
            {
                if (r != null && r.materials != null)
                {
                    foreach (var m in r.materials)
                    {
                        if (m != null && m.HasProperty("_EmissionColor"))
                        {
                            m.SetColor("_EmissionColor", Color.black);
                        }
                    }
                }
            }
        }

        if (detectorDisplayCard != null)
        {
            detectorDisplayCard.SetActive(false);
        }
    }

    public void OnPpeObjectTapped(string ppeCategory, GameObject tappedObj)
    {
        if (currentStep != TrainingStep.PPECheck) return;

        Debug.Log($"[GasAR] PPE item tapped: {ppeCategory} ({tappedObj.name})");

        // 1. Highlight tapped object briefly without moving or resizing
        if (ppeHighlightCoroutine != null) StopCoroutine(ppeHighlightCoroutine);
        ppeHighlightCoroutine = StartCoroutine(HighlightPpeObject(tappedObj));

        // 2. Play subtle confirmation audio if available
        if (audioSource != null && audioSource.clip != null) audioSource.Play();

        // 3. Mark only that item as selected
        string itemName = "";
        switch (ppeCategory)
        {
            case "PPE_Hardhat":
                ppeHardhatSelected = true;
                itemName = "Hardhat";
                break;
            case "PPE_Goggles":
                ppeGogglesSelected = true;
                itemName = "Safety Goggles";
                break;
            case "PPE_Gloves":
                ppeGlovesSelected = true;
                itemName = "Gloves";
                break;
            case "PPE_Boots":
                ppeBootsSelected = true;
                itemName = "Safety Boots";
                break;
        }

        // 4. Record assessment event
        if (GasScenarioAssessmentBridge.Instance != null && !string.IsNullOrEmpty(itemName))
        {
            GasScenarioAssessmentBridge.Instance.OnPpeSelected(itemName, true);
        }

        // 5. Update UI status card
        UpdatePpeCardUI();
    }

    private void UpdatePpeCardUI()
    {
        int count = 0;
        if (ppeHardhatSelected) count++;
        if (ppeGogglesSelected) count++;
        if (ppeGlovesSelected) count++;
        if (ppeBootsSelected) count++;

        if (ppeStatusCounterText != null)
        {
            ppeStatusCounterText.text = $"{count} / 4 SELECTED";
            ppeStatusCounterText.color = (count == 4) ? Hex("#4ADE80") : Hex("#38BDF8");
        }

        UpdateRowVisual(ppeRowBgHardhat, ppeRowTextHardhat, ppeHardhatSelected, "HARDHAT");
        UpdateRowVisual(ppeRowBgGoggles, ppeRowTextGoggles, ppeGogglesSelected, "SAFETY GOGGLES");
        UpdateRowVisual(ppeRowBgGloves, ppeRowTextGloves, ppeGlovesSelected, "GLOVES");
        UpdateRowVisual(ppeRowBgBoots, ppeRowTextBoots, ppeBootsSelected, "SAFETY BOOTS");

        if (count == 4)
        {
            StopVoicePlayback();
            ppeComplete = true;
            ppeSelectedCorrect = true;

            if (ppeCardTitleText != null) ppeCardTitleText.text = "PPE CHECK COMPLETE";
            if (ppeCardInstructionText != null) ppeCardInstructionText.text = "All required PPE has been identified.";
            if (headerStepTitleText != null) headerStepTitleText.text = "PPE CHECK COMPLETE";
            if (headerStepSubText != null) headerStepSubText.text = "";
            if (ppeContinueButtonGO != null) ppeContinueButtonGO.SetActive(true);
        }
        else
        {
            if (ppeCardTitleText != null) ppeCardTitleText.text = "SELECT REQUIRED PPE";
            if (ppeCardInstructionText != null) ppeCardInstructionText.text = "Tap the required PPE items on the workstation.";
            if (ppeContinueButtonGO != null) ppeContinueButtonGO.SetActive(false);
        }
    }

    private void OnPpeContinueClicked()
    {
        if (ppeHardhatSelected && ppeGogglesSelected && ppeGlovesSelected && ppeBootsSelected)
        {
            if (ppeCard != null) ppeCard.SetActive(false);
            if (ppeContinueButtonGO != null) ppeContinueButtonGO.SetActive(false);
            ClearPpeColliders();
            if (bottomInstructionCard != null) bottomInstructionCard.SetActive(true);
            SetStep(TrainingStep.BuddySystemCheck);
        }
    }

    private void UpdateRowVisual(Image bg, TextMeshProUGUI text, bool isSelected, string label)
    {
        if (bg == null || text == null) return;

        if (isSelected)
        {
            bg.color = new Color(0.08f, 0.32f, 0.16f, 0.92f);
            text.text = $"✓  {label}";
            text.color = new Color(0.35f, 1.0f, 0.55f);
        }
        else
        {
            bg.color = new Color(0.10f, 0.15f, 0.22f, 0.85f);
            text.text = $"○  {label}";
            text.color = new Color(0.80f, 0.88f, 0.96f);
        }
    }

    private IEnumerator HighlightPpeObject(GameObject target)
    {
        if (target == null) yield break;

        GameObject visualModel = target;
        if (target.name.Contains("InteractionCol"))
        {
            string baseName = target.name.Replace("_InteractionCol", "");
            visualModel = GameObject.Find(baseName) ?? target;
        }

        Renderer[] rends = visualModel.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) yield break;

        List<Material> mats = new List<Material>();
        List<Color> origEmissions = new List<Color>();

        foreach (var r in rends)
        {
            if (r != null && r.materials != null)
            {
                foreach (var m in r.materials)
                {
                    if (m != null)
                    {
                        mats.Add(m);
                        if (m.HasProperty("_EmissionColor"))
                        {
                            origEmissions.Add(m.GetColor("_EmissionColor"));
                            m.EnableKeyword("_EMISSION");
                        }
                        else
                        {
                            origEmissions.Add(Color.black);
                        }
                    }
                }
            }
        }

        Color subtleHighlight = new Color(0.15f, 0.85f, 0.45f) * 0.7f;

        float dur = 0.4f;
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Sin((elapsed / dur) * Mathf.PI);

            for (int i = 0; i < mats.Count; i++)
            {
                var m = mats[i];
                if (m != null && m.HasProperty("_EmissionColor"))
                {
                    m.SetColor("_EmissionColor", Color.Lerp(origEmissions[i], subtleHighlight, t));
                }
            }
            yield return null;
        }

        for (int i = 0; i < mats.Count; i++)
        {
            var m = mats[i];
            if (m != null && m.HasProperty("_EmissionColor"))
            {
                m.SetColor("_EmissionColor", origEmissions[i]);
            }
        }
    }

    private void EnsurePpeColliders()
    {
        ClearPpeColliders();

        SetupPpeTargetCollider("Safety_Helmet", "PPE_Hardhat", new Vector3(0.28f, 0.24f, 0.28f));
        SetupPpeTargetCollider("Helmet", "PPE_Hardhat", new Vector3(0.28f, 0.24f, 0.28f));

        SetupPpeTargetCollider("Safety_Goggles", "PPE_Goggles", new Vector3(0.22f, 0.16f, 0.18f));

        SetupPpeTargetCollider("Work_Gloves_Left", "PPE_Gloves", new Vector3(0.22f, 0.16f, 0.22f));
        SetupPpeTargetCollider("Work_Gloves_Right", "PPE_Gloves", new Vector3(0.22f, 0.16f, 0.22f));

        SetupPpeTargetCollider("Safety_Boots_Left", "PPE_Boots", new Vector3(0.24f, 0.32f, 0.26f));
        SetupPpeTargetCollider("Safety_Boots_Right", "PPE_Boots", new Vector3(0.24f, 0.32f, 0.26f));
    }

    private void SetupPpeTargetCollider(string objName, string category, Vector3 comfortableWorldSize)
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var go in allObjects)
        {
            if (go == null) continue;
            if (!go.name.Equals(objName, System.StringComparison.OrdinalIgnoreCase)) continue;

            Renderer[] rends = go.GetComponentsInChildren<Renderer>(true);
            if (rends.Length == 0) continue;

            Bounds wb = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++)
            {
                wb.Encapsulate(rends[i].bounds);
            }

            Transform rootParent = go.transform.parent;
            string colName = go.name + "_InteractionCol";
            Transform existingCol = rootParent != null ? rootParent.Find(colName) : null;
            GameObject colGo;
            if (existingCol == null)
            {
                colGo = new GameObject(colName);
                if (rootParent != null) colGo.transform.SetParent(rootParent, true);
                activePpeColliderObjects.Add(colGo);
            }
            else
            {
                colGo = existingCol.gameObject;
            }

            colGo.transform.position = wb.center;
            colGo.transform.rotation = go.transform.rotation;
            colGo.SetActive(true);

            BoxCollider bc = colGo.GetComponent<BoxCollider>();
            if (bc == null) bc = colGo.AddComponent<BoxCollider>();

            Vector3 lossy = colGo.transform.lossyScale;
            float lx = lossy.x != 0 ? Mathf.Max(wb.size.x, comfortableWorldSize.x) / Mathf.Abs(lossy.x) : comfortableWorldSize.x;
            float ly = lossy.y != 0 ? Mathf.Max(wb.size.y, comfortableWorldSize.y) / Mathf.Abs(lossy.y) : comfortableWorldSize.y;
            float lz = lossy.z != 0 ? Mathf.Max(wb.size.z, comfortableWorldSize.z) / Mathf.Abs(lossy.z) : comfortableWorldSize.z;

            bc.center = Vector3.zero;
            bc.size = new Vector3(lx, ly, lz);
        }
    }

    private void ClearPpeColliders()
    {
        foreach (var colGo in activePpeColliderObjects)
        {
            if (colGo != null)
            {
                colGo.SetActive(false);
            }
        }
    }

    public void SelectPpe(string ppeType, bool isApproved)
    {
        ppeComplete = true;
        ppeSelectedCorrect = isApproved;

        if (GasScenarioAssessmentBridge.Instance != null)
        {
            GasScenarioAssessmentBridge.Instance.OnPpeSelected(ppeType, isApproved);
        }

        if (isApproved)
        {
            if (ppeFeedbackText != null)
            {
                ppeFeedbackText.color = new Color(0.3f, 1.0f, 0.4f);
                ppeFeedbackText.text = "✓ PPE Check Complete\nBefore responding, confirm the buddy-system procedure.";
            }
            StartCoroutine(AutoAdvance(1.0f, TrainingStep.BuddySystemCheck));
        }
        else
        {
            userScore -= 15f;
            mistakeLog.Add("Inadequate PPE selected");
            if (!missedSteps.Contains("PPE Selection")) missedSteps.Add("PPE Selection");
            if (ppeFeedbackText != null)
            {
                ppeFeedbackText.color = new Color(1.0f, 0.35f, 0.35f);
                ppeFeedbackText.text = "That selection does not meet the configured site procedure.\nReview the PPE requirement and try again.";
            }
        }
    }

    public void OnBuddyConfirmed()
    {
        buddyVerified = true;
        if (GasScenarioAssessmentBridge.Instance != null)
        {
            GasScenarioAssessmentBridge.Instance.OnBuddySystemChecked(true);
        }
        SetStep(TrainingStep.SafetyResponse);
    }

    public void OnPipelineValveTapped()
    {
        if (currentStep == TrainingStep.SafetyResponse || currentStep == TrainingStep.GasLeakActive)
        {
            isolationValveInteracted = true;
            Debug.Log("[GasAR] 3D Red Isolation Valve tapped! Isolating gas leak.");

            if (leakVisualController != null)
            {
                leakVisualController.StopGasLeak();
            }
            StopLeakHiss();
            detectorScanned = false;
            StopDetectorAlarmAudio();

            if (alertBanner != null)
            {
                alertBanner.SetActive(false);
            }

            RemoveHazardZoneVisualDisk();

            if (GasScenarioAssessmentBridge.Instance != null)
            {
                GasScenarioAssessmentBridge.Instance.OnIsolationValveClosed(true);
            }

            SetStep(TrainingStep.IncidentResolved);
        }
    }

    public void OnConfinedSpaceTapped()
    {
        if (currentStep == TrainingStep.ConfinedSpaceIntro || currentStep == TrainingStep.PassResult || currentStep == TrainingStep.IncidentResolved)
        {
            Debug.Log("[GasAR] 3D Confined Space Vessel tapped! Starting pre-entry drill.");
            SetStep(TrainingStep.ConfinedSpaceDrill);
        }
    }

    public void OnWrongObjectTapped(string targetObjectType)
    {
        if (instructionText == null) return;

        switch (targetObjectType)
        {
            case "Cylinder":
                instructionText.text = "Inspect the gas cylinder and pipeline.\nTap the highlighted leak point to identify the simulated leak.";
                break;
            case "Detector":
                if (currentStep == TrainingStep.DetectorInteraction || currentStep == TrainingStep.DetectorRecognition)
                {
                    if (!detectorPickedUp)
                    {
                        instructionText.text = "The gas leak is active.\nTap the gas detector to pick it up.";
                    }
                    else if (!detectorScanned)
                    {
                        instructionText.text = "Detector ready.\nTap SCAN to measure the atmosphere.";
                    }
                }
                else
                {
                    instructionText.text = "First, check the gas detector.\nTap the orange gas detector to verify the atmosphere is normal.";
                }
                break;
            case "Valve":
                instructionText.text = "Tap the RED ISOLATION VALVE.";
                break;
            case "HazardZone":
                instructionText.text = "Outside the hazard zone.\nTap inside the marked danger area.";
                break;
            case "PPE":
                instructionText.text = "Select all required PPE before continuing.";
                break;
        }
    }

    private void CreateHazardZoneVisualDisk()
    {
        // 1. Locate cylinder to center hazard zone accurately on floor around leaking cylinder
        GameObject cylinderObj = GameObject.Find("Realistic_Gas_Cylinder");
        if (cylinderObj == null) cylinderObj = GameObject.Find("gas_cylinder");
        if (cylinderObj == null && environmentBuilder != null) cylinderObj = environmentBuilder.CurrentGasCylinder;

        Vector3 centerPos = cylinderObj != null ? cylinderObj.transform.position : transform.position;
        float floorY = transform.position.y + 0.003f;
        Vector3 targetPos = new Vector3(centerPos.x, floorY, centerPos.z);

        if (hazardZoneVisualDisk == null)
        {
            hazardZoneVisualDisk = new GameObject("HazardZoneVisualDisk");
            hazardZoneVisualDisk.transform.SetParent(transform, true);
            hazardZoneVisualDisk.transform.position = targetPos;
            hazardZoneVisualDisk.transform.rotation = transform.rotation;

            // Interactive BoxCollider for reliable AR touch raycasts
            BoxCollider col = hazardZoneVisualDisk.AddComponent<BoxCollider>();
            col.size = new Vector3(1.70f, 0.15f, 1.70f);
            col.center = new Vector3(0f, 0.075f, 0f);

            // A. Circular translucent floor danger surface with soft glow and warning perimeter ring
            GameObject surfaceObj = new GameObject("HazardZoneSurface");
            surfaceObj.transform.SetParent(hazardZoneVisualDisk.transform, false);
            surfaceObj.transform.localPosition = Vector3.zero;
            surfaceObj.transform.localRotation = Quaternion.identity;

            MeshFilter mf = surfaceObj.AddComponent<MeshFilter>();
            mf.mesh = CreateHazardCircleMesh(0.85f, 64);
            MeshRenderer mr = surfaceObj.AddComponent<MeshRenderer>();

            Texture2D hazardTex = GenerateHazardZoneTexture(512);
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            Material hazardMat = new Material(shader);
            hazardMat.name = "HazardZoneMat";
            hazardMat.mainTexture = hazardTex;
            hazardMat.color = Color.white;
            hazardMat.SetFloat("_Surface", 1f); // Transparent
            hazardMat.SetFloat("_Blend", 0f);
            hazardMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            hazardMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            hazardMat.SetInt("_ZWrite", 0);
            hazardMat.renderQueue = 3000;
            mr.material = hazardMat;

            // B. Subtle warning floor label: "⚠ DANGER — HAZARD AREA"
            GameObject labelObj = new GameObject("HazardZoneFloorLabel");
            labelObj.transform.SetParent(hazardZoneVisualDisk.transform, false);
            labelObj.transform.localPosition = new Vector3(0f, 0.002f, -0.62f);
            labelObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            Canvas floorCanvas = labelObj.AddComponent<Canvas>();
            floorCanvas.renderMode = RenderMode.WorldSpace;
            RectTransform canvasRt = labelObj.GetComponent<RectTransform>();
            canvasRt.sizeDelta = new Vector2(300f, 60f);
            labelObj.transform.localScale = Vector3.one * 0.0016f;

            // Dark translucent badge background
            GameObject bgObj = new GameObject("BadgeBg");
            bgObj.transform.SetParent(labelObj.transform, false);
            RectTransform bgRt = bgObj.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.10f, 0.10f, 0.12f, 0.85f);

            // Warning icon and text
            GameObject txtObj = new GameObject("LabelText");
            txtObj.transform.SetParent(labelObj.transform, false);
            RectTransform txtRt = txtObj.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;
            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "<color=#FF9900>⚠</color> DANGER — HAZARD AREA";
            tmp.fontSize = 20f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.95f, 0.95f, 0.95f, 0.95f);
        }
        else
        {
            hazardZoneVisualDisk.transform.position = targetPos;
            hazardZoneVisualDisk.SetActive(true);
        }
    }

    private Mesh CreateHazardCircleMesh(float radius, int segments)
    {
        Mesh m = new Mesh();
        m.name = "HazardZoneCircleMesh";
        Vector3[] vertices = new Vector3[segments + 1];
        Vector2[] uv = new Vector2[segments + 1];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        uv[0] = new Vector2(0.5f, 0.5f);

        float angleStep = 360f / segments;
        for (int i = 0; i < segments; i++)
        {
            float rad = Mathf.Deg2Rad * (i * angleStep);
            float x = Mathf.Cos(rad);
            float z = Mathf.Sin(rad);
            vertices[i + 1] = new Vector3(x * radius, 0f, z * radius);
            uv[i + 1] = new Vector2((x + 1f) * 0.5f, (z + 1f) * 0.5f);
        }

        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = next + 1;
        }

        m.vertices = vertices;
        m.uv = uv;
        m.triangles = triangles;
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    private Texture2D GenerateHazardZoneTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.name = "ProceduralHazardZoneTexture";
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color[] colors = new Color[size * size];
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float maxR = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center) / maxR;
                Color c = Color.clear;

                if (d <= 1.0f)
                {
                    if (d > 0.93f)
                    {
                        // Outer soft transparent glow edge falloff
                        float t = (d - 0.93f) / 0.07f;
                        float alpha = Mathf.SmoothStep(0.75f, 0f, t);
                        c = new Color(1.0f, 0.32f, 0.08f, alpha);
                    }
                    else if (d > 0.86f)
                    {
                        // High-visibility amber-red perimeter warning boundary
                        c = new Color(1.0f, 0.38f, 0.05f, 0.82f);
                    }
                    else if (d > 0.82f)
                    {
                        // Inner boundary accent line
                        c = new Color(1.0f, 0.65f, 0.15f, 0.45f);
                    }
                    else
                    {
                        // Translucent danger-zone surface
                        float t = d / 0.82f;
                        float alpha = Mathf.Lerp(0.14f, 0.22f, t);
                        c = new Color(0.95f, 0.22f, 0.08f, alpha);
                    }
                }
                colors[y * size + x] = c;
            }
        }

        tex.SetPixels(colors);
        tex.Apply(false);
        return tex;
    }

    private IEnumerator HighlightHazardBoundary()
    {
        if (hazardZoneVisualDisk == null) yield break;

        Renderer rend = hazardZoneVisualDisk.GetComponentInChildren<Renderer>();
        if (rend == null) yield break;

        Material mat = rend.material;
        Color origColor = mat.color;

        for (int i = 0; i < 3; i++)
        {
            float elapsed = 0f;
            float dur = 0.25f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Sin((elapsed / dur) * Mathf.PI);
                mat.color = Color.Lerp(origColor, new Color(1.0f, 0.95f, 0.4f, 1.0f), t);
                yield return null;
            }
        }
        mat.color = origColor;
    }

    private void RemoveHazardZoneVisualDisk()
    {
        if (hazardZoneVisualDisk != null)
        {
            hazardZoneVisualDisk.SetActive(false);
        }
    }

    private void EvaluateAssessment()
    {
        missedSteps.Clear();
        if (!cylinderInteracted) missedSteps.Add("Gas Cylinder Interaction");
        if (!detectorInteracted) missedSteps.Add("Gas Detector Interaction");
        if (!hazardRecognized) missedSteps.Add("Hazard Area Recognition");
        if (!ppeComplete || !ppeSelectedCorrect) missedSteps.Add("PPE Selection");
        if (!buddyVerified) missedSteps.Add("Buddy System Verification");
        if (!isolationValveInteracted) missedSteps.Add("Isolation Valve Interaction");

        bool passed = userScore >= 80f && missedSteps.Count == 0;
        if (GasScenarioAssessmentBridge.Instance != null)
        {
            GasScenarioAssessmentBridge.Instance.CompleteGasScenario(userScore, passed);
        }

        if (passed)
        {
            SetStep(TrainingStep.PassResult);
        }
        else
        {
            SetStep(TrainingStep.RetrainingResult);
        }
    }

    private void ShowTargetedRetrainingText()
    {
        if (instructionText != null)
        {
            string retrainText = "TARGETED RETRAINING — REVIEW MISSED STEPS:\n";
            if (missedSteps.Count > 0)
            {
                foreach (var step in missedSteps)
                {
                    retrainText += $"• Review required for: {step}\n";
                }
            }
            else
            {
                retrainText += "• Review approved site emergency procedures before re-evaluating.";
            }
            instructionText.text = retrainText;
        }
    }

    private static Color Hex(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color c)) return c;
        return Color.white;
    }

    private void ShowCompletionCard(bool passed)
    {
        // 1. Hide generic bottom instruction panel & old training HUD
        if (bottomInstructionCard != null) bottomInstructionCard.SetActive(false);
        if (actionButtonGO != null) actionButtonGO.SetActive(false);
        if (detectorDisplayCard != null) detectorDisplayCard.SetActive(false);
        if (alertBanner != null) alertBanner.SetActive(false);
        if (ppeCard != null) ppeCard.SetActive(false);
        if (buddyCard != null) buddyCard.SetActive(false);

        // 2. Populate Card content matching Fire's visual pattern
        if (completionTitleText != null)
        {
            completionTitleText.text = "SURAKSHAAR GAS SAFETY DRILL RESULT";
        }

        if (compSubtitleText != null)
        {
            compSubtitleText.text = passed ? "Status: Competent" : "Status: Needs Review";
            compSubtitleText.color = passed ? Hex("#4ADE80") : Hex("#F87171");
        }

        if (compScoreValText != null)
        {
            compScoreValText.text = $"{userScore:F0}%";
        }

        if (compResultValText != null)
        {
            compResultValText.text = passed ? "PASS" : "REVIEW";
            compResultValText.color = passed ? Hex("#4ADE80") : Hex("#F87171");
        }

        if (completionDetailsText != null)
        {
            if (passed)
            {
                completionDetailsText.text = "Response Complete\nEmergency response sequence successfully executed in accordance with approved site safety procedures.";
            }
            else
            {
                string details = "Review Required:\n";
                if (missedSteps.Count > 0)
                {
                    foreach (var s in missedSteps) details += $"• Missed: {s} ";
                }
                else
                {
                    details += "One or more safety steps require review before re-attempting.";
                }
                completionDetailsText.text = details;
            }
        }

        if (compContinueButton != null)
        {
            TextMeshProUGUI btnText = compContinueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = passed ? "Continue to Assessment →" : "Re-attempt Gas Drill ↺";
            }
        }

        if (completionCard != null) completionCard.SetActive(true);

        if (SurakshaAR.Core.AudioManager.Instance != null && passed)
        {
            SurakshaAR.Core.AudioManager.Instance.PlayCompletion();
        }
    }

    private void OnFinalResultAction()
    {
        bool passed = userScore >= 80f && missedSteps.Count == 0;
        if (passed)
        {
            Debug.Log("[GasAR] Drill Completed. Continuing to Assessment / Main Navigation.");
            if (AppState.Instance != null)
            {
                AppState.Instance.RecordAssessmentResult(Mathf.RoundToInt(userScore), 6);
            }
            if (AppSession.Instance != null)
            {
                AppSession.Instance.SetState(AppSession.TrainingState.Completed);
            }
            TrainingEventManager.RaiseTrainingCompleted();

            if (ARModuleLauncher.Instance != null)
            {
                ARModuleLauncher.Instance.ExitCurrentARScene();
            }
            else
            {
                OnRestartClicked();
            }
        }
        else
        {
            OnRestartClicked();
        }
    }

    private IEnumerator AutoAdvance(float delay, TrainingStep nextStep)
    {
        yield return new WaitForSeconds(delay);
        SetStep(nextStep);
    }

    private void OnRetrainingRequested()
    {
        SetStep(TrainingStep.TargetedRetraining);
    }

    private void OnRestartClicked()
    {
        Debug.Log("[GasAR] Restarting Gas AR Training Scenario.");
        StopVoicePlayback();
        cylinderInteracted = false;
        leakRecognized = false;
        detectorInteracted = false;
        hazardRecognized = false;
        ppeComplete = false;
        ppeSelectedCorrect = false;
        buddyVerified = false;
        isolationValveInteracted = false;
        verificationComplete = false;
        userScore = 100f;
        mistakeLog.Clear();
        missedSteps.Clear();

        if (currentDetectorObj != null)
        {
            if (originalDetectorParent != null) currentDetectorObj.transform.SetParent(originalDetectorParent, true);
            currentDetectorObj.transform.localPosition = originalDetectorLocalPosition;
            currentDetectorObj.transform.localRotation = originalDetectorLocalRotation;
            currentDetectorObj.transform.localScale = originalDetectorLocalScale;
        }
        if (detectorPickupCoroutine != null)
        {
            StopCoroutine(detectorPickupCoroutine);
            detectorPickupCoroutine = null;
        }
        if (detectorAlarmCoroutine != null)
        {
            StopCoroutine(detectorAlarmCoroutine);
            detectorAlarmCoroutine = null;
        }
        StopDetectorScanAudio();
        StopDetectorAlarmAudio();
        detectorPickedUp = false;
        detectorScanned = false;

        if (leakVisualController != null)
        {
            leakVisualController.StopGasLeak();
        }
        StopLeakHiss();

        RemoveHazardZoneVisualDisk();

        if (arPlacement != null)
        {
            arPlacement.ResetPlacement();
        }
        SetStep(TrainingStep.ScanAndPlace);
    }

    // Public helper for proof capture tools
    public void ForceCompleteInteractions()
    {
        cylinderInteracted = true;
        leakRecognized = true;
        detectorInteracted = true;
        hazardRecognized = true;
        ppeComplete = true;
        ppeSelectedCorrect = true;
        buddyVerified = true;
        isolationValveInteracted = true;
        verificationComplete = true;
    }

    private void OnDestroy()
    {
        StopVoicePlayback();
        StopLeakHiss();
        StopDetectorScanAudio();
        StopDetectorAlarmAudio();

        if (canvasObject != null)
        {
            Destroy(canvasObject);
            canvasObject = null;
        }
    }

    // =========================================================================
    // PROCEDURAL EQUIPMENT AUDIO SYNTHESIS & 3D SPATIAL CONTROLLERS
    // =========================================================================

    private static AudioClip GetOrCreateLeakHissClip()
    {
        if (cachedLeakHissClip != null) return cachedLeakHissClip;

        int sampleRate = 22050;
        int sampleCount = sampleRate; // 1.0 second seamless loop
        float[] samples = new float[sampleCount];
        System.Random rand = new System.Random(42);

        float lastWhite = 0f;
        float hpState = 0f;

        // Band-shaped noise centered around 2-6 kHz for realistic escaping pressurized gas hiss
        for (int i = 0; i < sampleCount; i++)
        {
            float white = (float)(rand.NextDouble() * 2.0 - 1.0);
            // High-pass filter
            float hp = white - lastWhite;
            lastWhite = white * 0.82f;

            // Low-pass smooth to eliminate harsh Nyquist crackle
            hpState += (hp - hpState) * 0.70f;

            // Subtle air turbulence oscillation (6-9 Hz)
            float flutter = 0.88f + 0.12f * Mathf.Sin(2f * Mathf.PI * 7.5f * ((float)i / sampleRate));

            samples[i] = hpState * flutter;
        }

        // Seamless loop crossfade (1200 samples)
        int fadeLen = 1200;
        for (int i = 0; i < fadeLen; i++)
        {
            float t = (float)i / fadeLen;
            samples[i] = Mathf.Lerp(samples[sampleCount - fadeLen + i], samples[i], t);
        }

        // Normalize
        float peak = 0.001f;
        for (int i = 0; i < sampleCount; i++)
        {
            float abs = Mathf.Abs(samples[i]);
            if (abs > peak) peak = abs;
        }
        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = (samples[i] / peak) * 0.70f;
        }

        cachedLeakHissClip = AudioClip.Create("GasLeakHiss_Procedural", sampleCount, 1, sampleRate, false);
        cachedLeakHissClip.SetData(samples, 0);
        return cachedLeakHissClip;
    }

    private void StartLeakHiss(Transform leakPoint)
    {
        StopLeakHiss();

        if (leakPoint == null) return;

        GameObject audioGO = new GameObject("GasLeakAudioSource");
        audioGO.transform.SetParent(leakPoint, false);
        audioGO.transform.localPosition = Vector3.zero;

        leakAudioSource = audioGO.AddComponent<AudioSource>();
        leakAudioSource.clip = GetOrCreateLeakHissClip();
        leakAudioSource.loop = true;
        leakAudioSource.spatialBlend = 1.0f; // Full 3D spatial audio
        leakAudioSource.minDistance = 0.25f;
        leakAudioSource.maxDistance = 5.0f;
        leakAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        leakAudioSource.volume = leakHissVolume;
        leakAudioSource.playOnAwake = false;
        leakAudioSource.Play();

        Debug.Log($"[GasAudio] 3D Gas leak hiss started at {leakPoint.name} (volume: {leakHissVolume})");
    }

    private void StopLeakHiss()
    {
        if (leakAudioSource != null)
        {
            leakAudioSource.Stop();
            if (leakAudioSource.gameObject != null)
            {
                if (Application.isPlaying) Destroy(leakAudioSource.gameObject);
                else DestroyImmediate(leakAudioSource.gameObject);
            }
            leakAudioSource = null;
            Debug.Log("[GasAudio] Gas leak hiss stopped.");
        }
    }

    private static AudioClip GetOrCreateScanBeepClip()
    {
        if (cachedScanBeepClip != null) return cachedScanBeepClip;

        int sampleRate = 22050;
        float duration = 0.045f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];
        float freq = 2200f; // Industrial electronic sampling chirp

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 90f); // Snappy tick attack and fast decay
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.75f;
        }

        cachedScanBeepClip = AudioClip.Create("DetectorScanBeep_Procedural", sampleCount, 1, sampleRate, false);
        cachedScanBeepClip.SetData(samples, 0);
        return cachedScanBeepClip;
    }

    private AudioSource GetOrCreateDetectorAudioSource(GameObject detector)
    {
        if (detector == null) return null;
        if (detectorAudioSource != null && detectorAudioSource.gameObject != null) return detectorAudioSource;

        detectorAudioSource = detector.GetComponent<AudioSource>();
        if (detectorAudioSource == null)
        {
            detectorAudioSource = detector.AddComponent<AudioSource>();
        }
        detectorAudioSource.spatialBlend = 0.8f; // Handheld 3D spatialized
        detectorAudioSource.minDistance = 0.2f;
        detectorAudioSource.maxDistance = 4.0f;
        detectorAudioSource.playOnAwake = false;
        return detectorAudioSource;
    }

    private IEnumerator DetectorScanAudioRoutine(float duration)
    {
        AudioSource src = GetOrCreateDetectorAudioSource(currentDetectorObj);
        AudioClip clip = GetOrCreateScanBeepClip();

        float interval = 0.42f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (src != null && clip != null)
            {
                src.PlayOneShot(clip, detectorScanVolume);
            }
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
        detectorScanAudioCoroutine = null;
    }

    private void StopDetectorScanAudio()
    {
        if (detectorScanAudioCoroutine != null)
        {
            StopCoroutine(detectorScanAudioCoroutine);
            detectorScanAudioCoroutine = null;
        }
    }

    private static AudioClip GetOrCreateDetectorAlarmClip()
    {
        if (cachedDetectorAlarmClip != null) return cachedDetectorAlarmClip;

        int sampleRate = 22050;
        float duration = 0.22f; // Fits within the 0.35s visual strobe window
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Real industrial gas detector dual-tone alternating alert (2850 Hz and 3400 Hz)
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float freq = (t < duration * 0.5f) ? 2850f : 3400f;
            float env = Mathf.Sin(Mathf.PI * (t / duration)); // Smooth envelope, prevents clicks
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.85f;
        }

        cachedDetectorAlarmClip = AudioClip.Create("DetectorAlarm_Procedural", sampleCount, 1, sampleRate, false);
        cachedDetectorAlarmClip.SetData(samples, 0);
        return cachedDetectorAlarmClip;
    }

    private void PlayDetectorAlarmBeep()
    {
        AudioSource src = GetOrCreateDetectorAudioSource(currentDetectorObj);
        AudioClip clip = GetOrCreateDetectorAlarmClip();
        if (src != null && clip != null)
        {
            src.PlayOneShot(clip, detectorAlarmVolume);
        }
    }

    private void StopDetectorAlarmAudio()
    {
        if (detectorAlarmCoroutine != null)
        {
            StopCoroutine(detectorAlarmCoroutine);
            detectorAlarmCoroutine = null;
        }
        if (detectorAudioSource != null)
        {
            detectorAudioSource.Stop();
        }
    }

    // =========================================================================
    // OPTIONAL 2D INSTRUCTIONAL VOICE GUIDANCE (ACCESSIBILITY)
    // =========================================================================

    private void EnsureVoiceAudioSource()
    {
        if (voiceAudioSource == null)
        {
            GameObject voiceGO = new GameObject("GasVoiceAudioSource");
            voiceGO.transform.SetParent(transform, false);
            voiceAudioSource = voiceGO.AddComponent<AudioSource>();
            voiceAudioSource.playOnAwake = false;
            voiceAudioSource.loop = false;
            voiceAudioSource.spatialBlend = 0f; // 2D clean instructional voice, NOT 3D equipment SFX
            voiceAudioSource.volume = 1.0f;
            voiceAudioSource.mute = false;
        }
    }

    private AudioClip GetVoiceClipForStep(TrainingStep step)
    {
        string clipName = null;
        switch (step)
        {
            case TrainingStep.CylinderInteraction:
                clipName = "step_leak_point";
                break;
            case TrainingStep.GasLeakActive:
            case TrainingStep.LeakRecognition:
                clipName = "step_gas_leak_detected";
                break;
            case TrainingStep.DetectorInteraction:
            case TrainingStep.DetectorRecognition:
                clipName = detectorScanned ? "step_gas_detected" : (detectorPickedUp ? "step_measure_atmosphere" : "step_detect_gas");
                break;
            case TrainingStep.DetectorChecked:
                clipName = "step_gas_detected";
                break;
            case TrainingStep.HazardRecognition:
                clipName = "step_hazard_zone";
                break;
            case TrainingStep.PPECheck:
                clipName = ppeComplete ? "step_ppe_complete" : "step_ppe_selection";
                break;
            case TrainingStep.BuddySystemCheck:
                clipName = "step_buddy_system";
                break;
            case TrainingStep.SafetyResponse:
                clipName = "step_valve_isolation";
                break;
            case TrainingStep.IncidentResolved:
            case TrainingStep.PostResponseVerification:
                clipName = "step_incident_resolved";
                break;
            default:
                break;
        }

        if (string.IsNullOrEmpty(clipName)) return null;

        if (voiceClipCache.TryGetValue(clipName, out AudioClip cached) && cached != null)
        {
            return cached;
        }

        AudioClip loaded = Resources.Load<AudioClip>($"Audio/GasTraining/English/{clipName}");
        if (loaded != null)
        {
            voiceClipCache[clipName] = loaded;
        }
        return loaded;
    }

    private void OnVoiceButtonClicked()
    {
        EnsureVoiceAudioSource();

        if (isVoiceSpeaking)
        {
            StopVoicePlayback();
            return;
        }

        AudioClip clip = GetVoiceClipForStep(currentStep);
        if (clip == null)
        {
            if (voiceButtonPulseCoroutine != null) StopCoroutine(voiceButtonPulseCoroutine);
            voiceButtonPulseCoroutine = StartCoroutine(VoiceButtonPulseRoutine("Pending..."));
            return;
        }

        PlayVoiceClip(clip);
    }

    private void PlayVoiceClip(AudioClip clip)
    {
        if (clip == null) return;
        EnsureVoiceAudioSource();

        voiceAudioSource.Stop();
        voiceAudioSource.clip = clip;
        voiceAudioSource.Play();

        isVoiceSpeaking = true;
        UpdateVoiceButtonVisuals(true);

        if (voiceMonitorCoroutine != null) StopCoroutine(voiceMonitorCoroutine);
        voiceMonitorCoroutine = StartCoroutine(MonitorVoicePlaybackRoutine());
    }

    private void StopVoicePlayback()
    {
        if (voiceMonitorCoroutine != null)
        {
            StopCoroutine(voiceMonitorCoroutine);
            voiceMonitorCoroutine = null;
        }

        if (voiceButtonPulseCoroutine != null)
        {
            StopCoroutine(voiceButtonPulseCoroutine);
            voiceButtonPulseCoroutine = null;
        }

        if (voiceAudioSource != null && voiceAudioSource.isPlaying)
        {
            voiceAudioSource.Stop();
        }

        isVoiceSpeaking = false;
        UpdateVoiceButtonVisuals(false);
    }

    private IEnumerator MonitorVoicePlaybackRoutine()
    {
        while (voiceAudioSource != null && voiceAudioSource.isPlaying)
        {
            yield return null;
        }

        isVoiceSpeaking = false;
        UpdateVoiceButtonVisuals(false);
        voiceMonitorCoroutine = null;
    }

    private void UpdateVoiceButtonVisuals(bool speaking)
    {
        Color bgColor = speaking ? Hex("#DC2626") : Hex("#2563EB");
        string btnText = speaking ? "■ STOP" : "🔊 LISTEN";

        if (voiceButtonBg != null) voiceButtonBg.color = bgColor;
        if (voiceButtonText != null)
        {
            voiceButtonText.text = btnText;
            voiceButtonText.fontSize = 20f;
        }

        if (ppeVoiceButtonBg != null) ppeVoiceButtonBg.color = bgColor;
        if (ppeVoiceButtonText != null)
        {
            ppeVoiceButtonText.text = btnText;
            ppeVoiceButtonText.fontSize = 18f;
        }
    }

    private IEnumerator VoiceButtonPulseRoutine(string message)
    {
        Color amber = Hex("#F59E0B");
        if (voiceButtonBg != null) voiceButtonBg.color = amber;
        if (voiceButtonText != null) voiceButtonText.text = message;
        if (ppeVoiceButtonBg != null) ppeVoiceButtonBg.color = amber;
        if (ppeVoiceButtonText != null) ppeVoiceButtonText.text = message;

        yield return new WaitForSeconds(1.8f);

        UpdateVoiceButtonVisuals(false);
        voiceButtonPulseCoroutine = null;
    }
}
