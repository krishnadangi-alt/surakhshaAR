using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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

    private GameObject actionButtonGO;
    private Button actionButton;
    private TextMeshProUGUI actionButtonText;

    private GameObject alertBanner;
    private TextMeshProUGUI alertBannerText;

    private GameObject ppeCard;
    private TextMeshProUGUI ppeFeedbackText;

    private GameObject buddyCard;

    private GameObject completionCard;
    private TextMeshProUGUI completionTitleText;
    private TextMeshProUGUI completionScoreText;
    private TextMeshProUGUI completionDetailsText;
    private Button restartButton;
    private Button retrainButton;

    private GameObject hazardZoneVisualDisk;

    private AudioSource audioSource;
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

        GameObject existingCanvas = GameObject.Find("SurakshaAR_GasTrainingCanvas");
        if (existingCanvas != null)
        {
            if (Application.isPlaying) Destroy(existingCanvas);
            else DestroyImmediate(existingCanvas);
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

        // 2. BOTTOM INSTRUCTION CARD
        RectTransform bottomCard = UIHelper.MakeRect("BottomInstructionCard", canvasObject.transform);
        UIHelper.AnchorBottomStretch(bottomCard, 340f, 40f);
        Image cardBg = bottomCard.gameObject.AddComponent<Image>();
        cardBg.color = new Color(0.05f, 0.08f, 0.12f, 0.95f);
        UIHelper.SetImageRoundedSprite(cardBg, 18f);

        headerStepSubText = UIHelper.MakeLabel("StepSubText", bottomCard,
            "Scan & Place", 22f, new Color(0.0f, 0.75f, 1.0f),
            TextAlignmentOptions.Center, true);
        headerStepSubText.rectTransform.anchorMin = new Vector2(0.04f, 0.82f);
        headerStepSubText.rectTransform.anchorMax = new Vector2(0.96f, 0.96f);
        headerStepSubText.rectTransform.offsetMin = Vector2.zero;
        headerStepSubText.rectTransform.offsetMax = Vector2.zero;

        instructionText = UIHelper.MakeLabel("InstructionText", bottomCard,
            "Scan the floor and place the gas safety workstation.", 22f, Color.white,
            TextAlignmentOptions.Center, false, true);
        instructionText.rectTransform.anchorMin = new Vector2(0.04f, 0.42f);
        instructionText.rectTransform.anchorMax = new Vector2(0.96f, 0.84f);
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

        // 4. PPE SELECTION CARD
        RectTransform ppeRt = UIHelper.MakeRect("PpeSelectionCard", canvasObject.transform);
        UIHelper.SetSize(ppeRt, 940f, 500f);
        ppeRt.anchoredPosition = new Vector2(0, 40);
        Image ppeBg = ppeRt.gameObject.AddComponent<Image>();
        ppeBg.color = new Color(0.08f, 0.12f, 0.20f, 0.96f);
        UIHelper.SetImageRoundedSprite(ppeBg, 20f);

        UIHelper.MakeLabel("PpeTitle", ppeRt, "Select PPE Required by Site Safety Procedure:", 26f, new Color(0.0f, 0.85f, 1.0f), TextAlignmentOptions.Center, true).rectTransform.anchoredPosition = new Vector2(0, 190);

        Button appPpeBtn = UIHelper.MakeButton("AppPpeBtn", ppeRt, "☑ Approved Gas PPE / Breathing Protection", 22f, new Color(0.0f, 0.60f, 0.35f), Color.white, 12f);
        RectTransform appRt = appPpeBtn.GetComponent<RectTransform>();
        appRt.anchoredPosition = new Vector2(0, 110);
        UIHelper.SetSize(appRt, 860f, 60f);
        appPpeBtn.onClick.AddListener(() => SelectPpe("Approved_Gas_PPE", true));

        Button dustPpeBtn = UIHelper.MakeButton("DustPpeBtn", ppeRt, "☒ Standard Dust Mask", 22f, new Color(0.45f, 0.25f, 0.10f), Color.white, 12f);
        RectTransform dustRt = dustPpeBtn.GetComponent<RectTransform>();
        dustRt.anchoredPosition = new Vector2(0, 35);
        UIHelper.SetSize(dustRt, 860f, 60f);
        dustPpeBtn.onClick.AddListener(() => SelectPpe("Standard_Dust_Mask", false));

        Button noPpeBtn = UIHelper.MakeButton("NoPpeBtn", ppeRt, "☒ Basic Hardhat Only", 22f, new Color(0.45f, 0.25f, 0.10f), Color.white, 12f);
        RectTransform noRt = noPpeBtn.GetComponent<RectTransform>();
        noRt.anchoredPosition = new Vector2(0, -40);
        UIHelper.SetSize(noRt, 860f, 60f);
        noPpeBtn.onClick.AddListener(() => SelectPpe("Basic_Hardhat_Only", false));

        ppeFeedbackText = UIHelper.MakeLabel("PpeFeedback", ppeRt, "", 22f, new Color(1.0f, 0.35f, 0.35f), TextAlignmentOptions.Center, true);
        ppeFeedbackText.rectTransform.anchoredPosition = new Vector2(0, -135);

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

        // 6. COMPLETION / ASSESSMENT CARD
        RectTransform compCard = UIHelper.MakeRect("CompletionCard", canvasObject.transform);
        UIHelper.SetSize(compCard, 960f, 660f);
        compCard.anchoredPosition = new Vector2(0, 0);
        Image compBg = compCard.gameObject.AddComponent<Image>();
        compBg.color = new Color(0.04f, 0.18f, 0.10f, 0.98f);
        UIHelper.SetImageRoundedSprite(compBg, 24f);

        completionTitleText = UIHelper.MakeLabel("CompTitle", compCard, "SURAKSHAAR GAS SAFETY DRILL RESULT", 34f, new Color(0.2f, 1.0f, 0.4f), TextAlignmentOptions.Center, true);
        completionTitleText.rectTransform.anchoredPosition = new Vector2(0, 240);

        completionScoreText = UIHelper.MakeLabel("CompScore", compCard, "Score: 100% | Status: Competent", 26f, new Color(0.95f, 0.95f, 0.35f), TextAlignmentOptions.Center, true);
        completionScoreText.rectTransform.anchoredPosition = new Vector2(0, 170);

        completionDetailsText = UIHelper.MakeLabel("CompDetails", compCard, "All safety steps performed per approved site procedure.", 22f, Color.white, TextAlignmentOptions.Center, false);
        completionDetailsText.rectTransform.anchoredPosition = new Vector2(0, 50);

        restartButton = UIHelper.MakeButton("RestartButton", compCard, "Restart Full Drill", 22f, new Color(0.0f, 0.55f, 0.30f), Color.white, 14f);
        RectTransform btnRt = restartButton.GetComponent<RectTransform>();
        btnRt.anchoredPosition = new Vector2(-220, -190);
        UIHelper.SetSize(btnRt, 360f, 64f);
        restartButton.onClick.AddListener(OnRestartClicked);

        retrainButton = UIHelper.MakeButton("RetrainButton", compCard, "Review Retraining", 22f, new Color(0.85f, 0.40f, 0.10f), Color.white, 14f);
        RectTransform retRt = retrainButton.GetComponent<RectTransform>();
        retRt.anchoredPosition = new Vector2(220, -190);
        UIHelper.SetSize(retRt, 360f, 64f);
        retrainButton.onClick.AddListener(OnRetrainingRequested);

        completionCard = compCard.gameObject;
        completionCard.SetActive(false);
    }

    private void HideAllCards()
    {
        if (alertBanner != null) alertBanner.SetActive(false);
        if (ppeCard != null) ppeCard.SetActive(false);
        if (buddyCard != null) buddyCard.SetActive(false);
        if (completionCard != null) completionCard.SetActive(false);
        if (actionButtonGO != null) actionButtonGO.SetActive(false);
    }

    public void SetStep(TrainingStep step)
    {
        if (canvasObject == null) CreateRuntimeUI();
        currentStep = step;
        stepStartTime = Time.time;
        HideAllCards();

        switch (step)
        {
            case TrainingStep.ScanAndPlace:
                if (headerStepTitleText != null) headerStepTitleText.text = "GAS & CONFINED SPACE SAFETY";
                if (headerStepSubText != null) headerStepSubText.text = "Scan & Place";
                if (instructionText != null) instructionText.text = "Scan the floor and place the gas safety workstation.";
                break;

            case TrainingStep.WorkstationReady:
            case TrainingStep.Intro:
                if (headerStepTitleText != null) headerStepTitleText.text = "GAS & CONFINED SPACE SAFETY";
                if (headerStepSubText != null) headerStepSubText.text = "✓ Workstation Ready";
                if (instructionText != null) instructionText.text = "Gas leak response drill is ready.\n\nSTEP 1 — START THE DRILL\nTap the Gas Cylinder to begin the simulated leak.";
                // NO GENERIC NEXT BUTTON — Worker MUST tap 3D Gas Cylinder
                break;

            case TrainingStep.CylinderInteraction:
                if (headerStepTitleText != null) headerStepTitleText.text = "STEP 1 — START THE DRILL";
                if (headerStepSubText != null) headerStepSubText.text = "Tap Gas Cylinder";
                if (instructionText != null) instructionText.text = "Tap the Gas Cylinder to begin the simulated leak.";
                // NO GENERIC NEXT BUTTON
                break;

            case TrainingStep.GasLeakActive:
            case TrainingStep.LeakRecognition:
                if (headerStepTitleText != null) headerStepTitleText.text = "⚠ GAS LEAK DETECTED";
                if (headerStepSubText != null) headerStepSubText.text = "Observe Leak Plume";
                if (instructionText != null) instructionText.text = "Observe the leak and check the Gas Detector.\n\nTap the Gas Detector to check the reading.";
                if (alertBanner != null) alertBanner.SetActive(true);
                CreateHazardZoneVisualDisk();
                StartCoroutine(AutoAdvance(1.2f, TrainingStep.DetectorInteraction));
                break;

            case TrainingStep.DetectorInteraction:
            case TrainingStep.DetectorRecognition:
                if (headerStepTitleText != null) headerStepTitleText.text = "STEP 2 — CHECK THE ATMOSPHERE";
                if (headerStepSubText != null) headerStepSubText.text = "Tap Gas Detector";
                if (instructionText != null) instructionText.text = "Gas leak suspected.\n\nTap the Gas Detector to check the reading.";
                if (alertBanner != null) alertBanner.SetActive(true);
                CreateHazardZoneVisualDisk();
                // NO GENERIC NEXT BUTTON — Worker MUST tap 3D Multi-Gas Detector
                break;

            case TrainingStep.DetectorChecked:
                if (headerStepTitleText != null) headerStepTitleText.text = "✓ Gas Detector Checked";
                if (headerStepSubText != null) headerStepSubText.text = "⚠ GAS DETECTED";
                if (instructionText != null) instructionText.text = "⚠ GAS DETECTED\nHAZARD ACTIVE\nREADING: ELEVATED\n\nGas has been detected. Identify the hazardous area.";
                if (alertBanner != null) alertBanner.SetActive(true);
                CreateHazardZoneVisualDisk();
                ShowActionButton("Identify Hazardous Area");
                break;

            case TrainingStep.HazardRecognition:
                if (headerStepTitleText != null) headerStepTitleText.text = "STEP 3 — IDENTIFY THE HAZARD AREA";
                if (headerStepSubText != null) headerStepSubText.text = "Hazard Zone Perimeter";
                if (instructionText != null) instructionText.text = "Identify the area affected by the gas leak.";
                if (alertBanner != null) alertBanner.SetActive(true);
                CreateHazardZoneVisualDisk();
                ShowActionButton("Acknowledge Hazard Area");
                break;

            case TrainingStep.PPECheck:
                if (headerStepTitleText != null) headerStepTitleText.text = "STEP 4 — SELECT REQUIRED PPE";
                if (headerStepSubText != null) headerStepSubText.text = "Site Safety Procedure";
                if (instructionText != null) instructionText.text = "Select the PPE required by the approved site safety procedure.";
                if (alertBanner != null) alertBanner.SetActive(true);
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
                if (instructionText != null) instructionText.text = "✓ Response Complete\n\nCongratulations! You successfully completed the gas safety drill.";
                ShowCompletionCard(true);
                break;

            case TrainingStep.RetrainingResult:
                if (headerStepTitleText != null) headerStepTitleText.text = "SURAKSHAAR GAS SAFETY DRILL RESULT";
                if (headerStepSubText != null) headerStepSubText.text = "⚠ Status: Needs Review";
                if (instructionText != null) instructionText.text = "Review required for missed safety procedure steps.";
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
            case TrainingStep.DetectorChecked:
                SetStep(TrainingStep.HazardRecognition);
                break;

            case TrainingStep.HazardRecognition:
                OnHazardZoneAcknowledged();
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
                SetStep(TrainingStep.ConfinedSpaceIntro);
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

        if (currentStep == TrainingStep.WorkstationReady || currentStep == TrainingStep.Intro || currentStep == TrainingStep.CylinderInteraction || currentStep == TrainingStep.NormalState)
        {
            cylinderInteracted = true;
            Debug.Log("[GasAR] 3D Gas Cylinder tapped! Triggering gas leak emergency.");
            TriggerGasLeakEmergency();
        }
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
        if (currentStep == TrainingStep.GasLeakActive || currentStep == TrainingStep.DetectorInteraction || currentStep == TrainingStep.DetectorRecognition)
        {
            detectorInteracted = true;
            Debug.Log("[GasAR] 3D Multi-Gas Detector tapped!");

            // Trigger visual scale pulse indicator feedback on detector
            GameObject detectorObj = GameObject.Find("Multi_Gas_Detector") ?? GameObject.Find("detector");
            if (detectorObj != null)
            {
                StartCoroutine(PulseDetectorFeedback(detectorObj));
            }

            if (GasScenarioAssessmentBridge.Instance != null)
            {
                GasScenarioAssessmentBridge.Instance.OnHazardZonePerimeterIdentified(100f);
            }

            SetStep(TrainingStep.DetectorChecked);
        }
    }

    private IEnumerator PulseDetectorFeedback(GameObject target)
    {
        Vector3 origScale = target.transform.localScale;
        target.transform.localScale = origScale * 1.15f;
        yield return new WaitForSeconds(0.25f);
        target.transform.localScale = origScale;
    }

    public void OnHazardZoneAcknowledged()
    {
        if (currentStep == TrainingStep.HazardRecognition)
        {
            hazardRecognized = true;
            Debug.Log("[GasAR] Hazard area recognized and acknowledged.");
            SetStep(TrainingStep.PPECheck);
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
                instructionText.text = "Please tap the Gas Cylinder to begin the drill.";
                break;
            case "Detector":
                instructionText.text = "Tap the Gas Detector to check the reading.";
                break;
            case "Valve":
                instructionText.text = "Tap the RED ISOLATION VALVE.";
                break;
        }
    }

    private void CreateHazardZoneVisualDisk()
    {
        if (hazardZoneVisualDisk == null)
        {
            hazardZoneVisualDisk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hazardZoneVisualDisk.name = "HazardZoneVisualDisk";
            hazardZoneVisualDisk.transform.SetParent(transform, false);
            hazardZoneVisualDisk.transform.localPosition = new Vector3(0f, 0.01f, 0f);
            hazardZoneVisualDisk.transform.localScale = new Vector3(2.4f, 0.002f, 2.4f);
            Material hazardMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
            hazardMat.color = new Color(1.0f, 0.25f, 0.1f, 0.35f);
            hazardZoneVisualDisk.GetComponent<Renderer>().material = hazardMat;
        }
        else
        {
            hazardZoneVisualDisk.SetActive(true);
        }
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

    private void ShowCompletionCard(bool passed)
    {
        if (completionCard != null) completionCard.SetActive(true);

        if (completionTitleText != null)
        {
            completionTitleText.text = "SURAKSHAAR GAS SAFETY DRILL RESULT";
        }

        if (completionScoreText != null)
        {
            completionScoreText.text = $"Score: {userScore:F0}% | Status: {(passed ? "Competent" : "Needs Review")}";
            completionScoreText.color = passed ? new Color(0.2f, 1.0f, 0.4f) : new Color(1.0f, 0.35f, 0.35f);
        }

        if (completionDetailsText != null)
        {
            if (passed)
            {
                completionDetailsText.text = "✓ Response Complete\n\nAll emergency response steps executed safely per approved site safety procedures.";
            }
            else
            {
                string details = "Review Required:\n";
                foreach (var s in missedSteps) details += $"• Missed Step: {s}\n";
                foreach (var m in mistakeLog) details += $"• Violation: {m}\n";
                completionDetailsText.text = details;
            }
        }

        if (retrainButton != null) retrainButton.gameObject.SetActive(!passed);
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
}
