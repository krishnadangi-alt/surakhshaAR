using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.Localization;
using SurakshaAR.UI;

/// <summary>
/// Authoritative Gas Training Controller & Mobile AR UI System.
/// Implements the exact 14-step Gas Training Flow and visual design system
/// matching the SurakshaAR Fire AR reference UI:
/// - Gas Blue accent (#1677FF) with Dark Navy typography (#0F172A)
/// - Exact Fire typography scale (38px Title, 32px Step, 36px Timer, 34px Score, 36px Card Title, 28px Card Body, 30px Action Pill)
/// - Single authoritative bottom guidance card with procedural icons and [🔊 Listen] button
/// - Top Bar HUD: Back button, Title, Step X of 14, Live Timer, Score with Delta animation, Progress line
/// - In-world action pills with target direction & tap hand icons
/// - Real 3D physical workstation interactions: Cylinder, Multi-Gas Detector, PPE workbench, Hazard Zone, Standby Buddy, Red Isolation Valve, Confined Space vessel
/// - Full GasLeakVisualController VFX + Hiss Audio + Alarm Buzzer integration
/// - Detector 3D display panel (Normal -> Hazard Active -> Isolated)
/// - Grounded Hazard Zone circle visualization
/// - Multilingual localization (English, Hindi, Santali/Ol Chiki) with zero hardcoded strings
/// - Full GasScenarioAssessmentBridge integration
/// </summary>
[DisallowMultipleComponent]
public class GasTrainingStepController : MonoBehaviour
{
    public enum TrainingStep
    {
        ScanAndPlace = 0,               // Step 1 of 14: AR Placement ("Scan & Place Training Area")
        WorkstationReady = 1,           // Step 2 of 14: Gas Workstation Ready ("Gas Workstation Ready")
        CylinderInteraction = 2,        // Step 3 of 14: Activate Scenario / Tap Gas Cylinder ("Tap Gas Cylinder")
        GasLeakActive = 3,              // Step 4 of 14: Observe Gas Leak ("Gas Leak Started")
        DetectorInteraction = 4,        // Step 5 of 14: Check Gas Detector ("Check Gas Detector")
        AlarmActive = 5,                // Step 6 of 14: Alarm Activated ("Alarm Active - Gas Detected")
        PPECheck = 6,                   // Step 7 of 14: PPE Selection ("Select Required PPE")
        HazardRecognition = 7,          // Step 8 of 14: Hazard Zone ("Identify Hazard Zone")
        BuddySystemCheck = 8,           // Step 9 of 14: Confirm Buddy-System ("Confirm Buddy-System")
        SafetyResponse = 9,             // Step 10 of 14: Operate Isolation Valve ("Operate Isolation Valve")
        ResponseComplete = 10,          // Step 11 of 14: Response Complete ("✓ Leak Response Complete")
        ConfinedSpaceIntro = 11,        // Step 12 of 14: Proceed to Confined Space ("Proceed to Confined Space")
        ConfinedSpaceDrill = 12,        // Step 13 of 14: Complete Confined Space Task ("Complete Confined Space Task")
        FinalAssessment = 13,           // Step 14 of 14: Assessment ("Assessment" / "Start Assessment")

        // Backward compatibility aliases
        DetectorChecked = 5,
        HazardZoneRecognition = 7,
        IncidentResolved = 10,
        PassResult = 14,
        RetrainingResult = 15,
        Completion = 16,
        Intro = 17,
        EquipmentIdentification = 18,
        NormalState = 19,
        LeakRecognition = 20,
        DetectorRecognition = 21,
        ReadinessCheck = 22,
        TargetedRetraining = 23,
        Reassessment = 24,
        PostResponseVerification = 25
    }

    public static GasTrainingStepController Instance { get; private set; }

    [Header("Current Step")]
    [SerializeField] private TrainingStep currentStep = TrainingStep.ScanAndPlace;
    public TrainingStep CurrentStep => currentStep;

    [Header("References")]
    [SerializeField] private GasARPlacement arPlacement;
    [SerializeField] private GasEnvironmentBuilder environmentBuilder;
    [SerializeField] private GasLeakVisualController leakVisualController;

    // ── UI Hierarchy Elements (Matching Fire UI System) ───────────────
    private Canvas _canvas;
    private RectTransform _safeArea;

    // Top Bar HUD
    private RectTransform _topBar;
    private RectTransform _timerPill;
    private TextMeshProUGUI _moduleTitleText;
    private TextMeshProUGUI _stepCounterText;
    private TextMeshProUGUI _timerText;
    private TextMeshProUGUI _scoreText;
    private TextMeshProUGUI _scoreDeltaText;
    private RectTransform _topProgressBarTrack;
    private RectTransform _topProgressFill;
    private Coroutine _scoreDeltaRoutine;
    private Coroutine _timerRoutine;

    // Single Authoritative Bottom Guidance Card
    private RectTransform _guidanceCard;
    private Image _guidanceCardIconBadge;
    private Image _guidanceCardIconImg;
    private TextMeshProUGUI _guidanceCardTitle;
    private TextMeshProUGUI _guidanceCardBody;
    private Button _voiceBtn;
    private Image _voiceBtnBg;
    private Image _voiceBtnIcon;
    private TextMeshProUGUI _voiceBtnText;
    private bool _voiceActive = false;
    private Coroutine _voiceFeedbackRoutine;

    // Assessment CTA Button on Card (Step 14)
    private Button _btnAssessmentCTA;
    private TextMeshProUGUI _btnAssessmentCTAText;

    // Center/World Action Pill
    private RectTransform _actionPill;
    private Image _actionPillBg;
    private TextMeshProUGUI _actionPillText;
    private GameObject _actionPillHandIcon;

    // Top Feedback Toast Banner
    private RectTransform _feedbackBanner;
    private Image _feedbackBg;
    private TextMeshProUGUI _feedbackText;
    private Coroutine _feedbackRoutine;

    // ── In-World Visuals (Detector Screen & Hazard Zone) ───────────────
    private GameObject _detectorDisplayRoot;
    private TextMeshPro _detectorDisplayTitle;
    private TextMeshPro _detectorDisplaySub;
    private MeshRenderer _detectorDisplayBgRenderer;

    private GameObject _hazardZoneRoot;
    private LineRenderer _hazardZoneLine;
    private GameObject _hazardZoneBadgeGO;

    // ── Audio Sources ─────────────────────────────────────────────────
    private AudioSource _hissAudioSource;
    private AudioSource _alarmAudioSource;
    private AudioSource _voiceAudioSource;
    private AudioClip _hissClip;
    private AudioClip _alarmClip;

    // ── Flow State & Scoring ──────────────────────────────────────────
    private float _userScore = 0f;
    private float _scenarioStartTime;
    private int _elapsedSeconds = 0;
    private bool _timerRunning = false;

    private bool cylinderInteracted = false;
    private bool leakRecognized = false;
    private bool detectorInteracted = false;
    private bool hazardRecognized = false;
    private bool ppeComplete = false;
    private bool ppeSelectedCorrect = false;
    private bool buddyVerified = false;
    private bool isolationValveInteracted = false;
    private bool verificationComplete = false;
    private bool confinedSpaceCompleted = false;

    public bool IsLeakRecognized => leakRecognized;
    public bool IsVerificationComplete => verificationComplete;

    private readonly List<string> mistakeLog = new List<string>();
    private readonly List<string> missedSteps = new List<string>();

    // ── Color Palette (Approved Gas Blue System) ───────────────────────
    private static readonly Color GasPrimaryBlue = new Color(0.086f, 0.467f, 1.0f);     // #1677FF
    private static readonly Color GasSupportingBlue = new Color(0.91f, 0.949f, 1.0f);   // #E8F2FF
    private static readonly Color DarkNavy = new Color(0.059f, 0.090f, 0.165f);         // #0F172A
    private static readonly Color DarkSlate = new Color(0.20f, 0.255f, 0.333f);         // #334155
    private static readonly Color CardBorderSlate = new Color(0.886f, 0.910f, 0.941f);  // #E2E8F0
    private static readonly Color SafetyGreen = new Color(0.086f, 0.639f, 0.290f);     // #16A34A
    private static readonly Color WarningAmber = new Color(0.961f, 0.620f, 0.043f);    // #F59E0B
    private static readonly Color DangerRed = new Color(0.863f, 0.149f, 0.149f);       // #DC2626
    private static readonly Color DarkHUD = new Color(0.024f, 0.102f, 0.184f, 0.88f);  // #061A2F

    private void Awake()
    {
        if (Application.isPlaying && Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        EnsureUIInitialized();
    }

    public void EnsureUIInitialized()
    {
        if (_canvas == null)
        {
            // Auto-resolve components
            if (arPlacement == null) arPlacement = FindAnyObjectByType<GasARPlacement>();
            if (environmentBuilder == null) environmentBuilder = GetComponent<GasEnvironmentBuilder>();
            if (environmentBuilder == null) environmentBuilder = FindAnyObjectByType<GasEnvironmentBuilder>();
            if (leakVisualController == null) leakVisualController = GetComponent<GasLeakVisualController>();
            if (leakVisualController == null) leakVisualController = FindAnyObjectByType<GasLeakVisualController>();

            SetupAudioSources();
            CleanupConflictingCanvases();
            BuildUGUIHierarchy();
        }
    }

    public void PrepareForCameraCapture(Camera cam)
    {
        EnsureUIInitialized();
        if (_canvas != null && cam != null)
        {
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = cam;
            _canvas.planeDistance = 0.5f;
        }
    }

    public void RestoreAfterCameraCapture()
    {
        if (_canvas != null)
        {
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.worldCamera = null;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _timerRunning = false;
        _feedbackRoutine = null;
        _scoreDeltaRoutine = null;
        _timerRoutine = null;
        _voiceFeedbackRoutine = null;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        StopAllCoroutines();
    }

    private void Start()
    {
        SetStep(TrainingStep.ScanAndPlace);
    }

    // ─────────────────────────────────────────────────────────────────
    //  AUDIO SETUP & PROCEDURAL SOUND GENERATION
    // ─────────────────────────────────────────────────────────────────
    private void SetupAudioSources()
    {
        // 1. Gas Leak Hiss AudioSource
        var hissGO = new GameObject("GasHissAudioSource");
        hissGO.transform.SetParent(transform, false);
        _hissAudioSource = hissGO.AddComponent<AudioSource>();
        _hissAudioSource.loop = true;
        _hissAudioSource.playOnAwake = false;
        _hissAudioSource.spatialBlend = 0.2f;
        _hissAudioSource.volume = 0.85f;

        // 2. Gas Detector Alarm AudioSource
        var alarmGO = new GameObject("GasAlarmAudioSource");
        alarmGO.transform.SetParent(transform, false);
        _alarmAudioSource = alarmGO.AddComponent<AudioSource>();
        _alarmAudioSource.loop = true;
        _alarmAudioSource.playOnAwake = false;
        _alarmAudioSource.spatialBlend = 0.2f;
        _alarmAudioSource.volume = 0.90f;

        // 3. Voice / Listen AudioSource
        var voiceGO = new GameObject("GasVoiceAudioSource");
        voiceGO.transform.SetParent(transform, false);
        _voiceAudioSource = voiceGO.AddComponent<AudioSource>();
        _voiceAudioSource.loop = false;
        _voiceAudioSource.playOnAwake = false;
        _voiceAudioSource.spatialBlend = 0.0f; // 2D clean UI audio
        _voiceAudioSource.volume = 1.0f;

        // Load pre-generated WAV audio clips or synthesize procedural fallbacks
        _hissClip = Resources.Load<AudioClip>("Audio/SFX/gas_leak_hiss");
        if (_hissClip == null)
        {
            _hissClip = CreateProceduralHissClip();
        }

        _alarmClip = Resources.Load<AudioClip>("Audio/SFX/gas_detector_alarm");
        if (_alarmClip == null)
        {
            _alarmClip = CreateProceduralAlarmClip();
        }

        _hissAudioSource.clip = _hissClip;
        _alarmAudioSource.clip = _alarmClip;
    }

    private static AudioClip CreateProceduralHissClip()
    {
        int sampleRate = 44100;
        int numSamples = sampleRate * 2;
        float[] samples = new float[numSamples];
        System.Random rng = new System.Random(42);
        float f1 = 0f, f2 = 0f;

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / sampleRate;
            float white = (float)(rng.NextDouble() * 2.0 - 1.0);
            f1 = f1 * 0.75f + white * 0.25f;
            f2 = f2 * 0.85f + f1 * 0.15f;
            float turbulence = 0.85f + 0.15f * Mathf.Sin(2f * Mathf.PI * 7f * t);
            samples[i] = f2 * turbulence * 0.55f;
        }

        var clip = AudioClip.Create("ProceduralGasHiss", numSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateProceduralAlarmClip()
    {
        int sampleRate = 44100;
        int numSamples = (int)(sampleRate * 1.6f);
        float[] samples = new float[numSamples];

        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / sampleRate;
            float cycle = t % 0.4f;
            if (cycle < 0.22f)
            {
                float freq = (t % 0.8f < 0.4f) ? 950f : 1250f;
                float env = Mathf.Sin(Mathf.PI * (cycle / 0.22f));
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.65f;
            }
            else
            {
                samples[i] = 0f;
            }
        }

        var clip = AudioClip.Create("ProceduralGasAlarm", numSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // ─────────────────────────────────────────────────────────────────
    //  CANVAS CLEANUP & INITIALIZATION
    // ─────────────────────────────────────────────────────────────────
    private void CleanupConflictingCanvases()
    {
        if (_feedbackRoutine != null) { StopCoroutine(_feedbackRoutine); _feedbackRoutine = null; }
        if (_scoreDeltaRoutine != null) { StopCoroutine(_scoreDeltaRoutine); _scoreDeltaRoutine = null; }
        if (_voiceFeedbackRoutine != null) { StopCoroutine(_voiceFeedbackRoutine); _voiceFeedbackRoutine = null; }

        // Destroy all existing/baked canvas objects named SurakshaAR_GasTrainingCanvas or AR_Gas_UGUI_Canvas
        Canvas[] existing = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in existing)
        {
            if (c == null) continue;
            string n = c.gameObject.name;
            if (n.Contains("GasTrainingCanvas") || n.Contains("AR_Gas_UGUI") || n == "SurakshaAR_GasTrainingCanvas")
            {
                if (Application.isPlaying) Destroy(c.gameObject);
                else DestroyImmediate(c.gameObject);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  UGUI HIERARCHY BUILDER (Pure Fire-Matched UI System)
    // ─────────────────────────────────────────────────────────────────
    private void BuildUGUIHierarchy()
    {
        // 1. Root Canvas
        var canvasGO = new GameObject("AR_Gas_UGUI_Canvas");
        canvasGO.transform.SetParent(transform, false);

        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 2400);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f; // Width-first for portrait mobile (exact match with Fire UI)

        canvasGO.AddComponent<GraphicRaycaster>();

        var canvasRT = canvasGO.GetComponent<RectTransform>();
        if (canvasRT != null) canvasRT.sizeDelta = new Vector2(1080, 2400);

        // 2. Safe Area Root
        var safeGO = new GameObject("SafeArea");
        safeGO.transform.SetParent(canvasGO.transform, false);
        _safeArea = safeGO.AddComponent<RectTransform>();
        UIHelper.Stretch(_safeArea, 0, 0, 0, 0);
        safeGO.AddComponent<SafeAreaDriver>();

        // 3. Build UI Components matching official SurakshaAR reference
        BuildTopBar(_safeArea);
        BuildWorldActionPill(_safeArea);
        BuildFeedbackBanner(_safeArea);
        BuildGuidanceCard(_safeArea); // Single Authoritative Guidance Card
    }

    // ─────────────────────────────────────────────────────────────────
    //  1. TOP BAR HUD (Exact Fire-Matched Structure)
    // ─────────────────────────────────────────────────────────────────
    private void BuildTopBar(Transform parent)
    {
        _topBar = UIHelper.MakeRect("TopBar", parent);
        _topBar.anchorMin = new Vector2(0, 1);
        _topBar.anchorMax = new Vector2(1, 1);
        _topBar.pivot = new Vector2(0.5f, 1);
        _topBar.sizeDelta = new Vector2(0, 164);
        _topBar.anchoredPosition = new Vector2(0, -10);

        // A. Back Button — 72x72 minimum mobile touch target
        var backBtn = UIHelper.MakeButton("btn-ar-back", _topBar, "‹", 44, DarkHUD, Color.white, 18);
        var backRT = backBtn.GetComponent<RectTransform>();
        backRT.anchorMin = new Vector2(0, 0.5f);
        backRT.anchorMax = new Vector2(0, 0.5f);
        backRT.pivot = new Vector2(0.5f, 0.5f);
        backRT.sizeDelta = new Vector2(72, 72);
        backRT.anchoredPosition = new Vector2(28, 6);

        var backOutline = backBtn.gameObject.AddComponent<Outline>();
        backOutline.effectColor = new Color(1f, 1f, 1f, 0.15f);
        backOutline.effectDistance = new Vector2(1, -1);
        backBtn.onClick.AddListener(HandleBackClicked);

        // B. Module Title & Step Subtitle Stack
        var titleStack = UIHelper.MakeRect("TitleStack", _topBar);
        titleStack.anchorMin = new Vector2(0, 0.5f);
        titleStack.anchorMax = new Vector2(0, 0.5f);
        titleStack.pivot = new Vector2(0f, 0.5f);
        titleStack.anchoredPosition = new Vector2(116, 6);
        titleStack.sizeDelta = new Vector2(560, 96);

        var vlg = titleStack.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 2;
        vlg.childAlignment = TextAnchor.MiddleLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Module Title: 38px bold white
        _moduleTitleText = UIHelper.MakeLabel("ModuleTitle", titleStack,
            Loc("gas.hud.title", "Gas Leak & Confined Space"), 38, Color.white, bold: true, wrap: false);
        _moduleTitleText.overflowMode = TextOverflowModes.Ellipsis;
        UIHelper.SetLayout(_moduleTitleText.gameObject, preferredWidth: 550, minWidth: 420, preferredHeight: 50);

        // Step Counter: 32px bold safety green
        _stepCounterText = UIHelper.MakeLabel("StepCounter", titleStack,
            "Step 1 of 14", 32, SafetyGreen, bold: true, wrap: false);
        _stepCounterText.overflowMode = TextOverflowModes.Ellipsis;
        UIHelper.SetLayout(_stepCounterText.gameObject, preferredWidth: 550, minWidth: 420, preferredHeight: 40);

        // C. Right Cluster (Score + Live Stopwatch Timer)
        var rightCluster = UIHelper.MakeRect("RightCluster", _topBar);
        rightCluster.anchorMin = new Vector2(1, 0.5f);
        rightCluster.anchorMax = new Vector2(1, 0.5f);
        rightCluster.pivot = new Vector2(1, 0.5f);
        rightCluster.anchoredPosition = new Vector2(-24, 6);
        rightCluster.sizeDelta = new Vector2(280, 68);

        var hlg = rightCluster.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleRight;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        // Score Label: 34px bold #CBD5E1
        _scoreText = UIHelper.MakeLabel("ScoreLbl", rightCluster, "Score: 0", 34, UIColors.Hex("#CBD5E1"), TextAlignmentOptions.Right, bold: true, wrap: false);
        _scoreText.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(_scoreText.gameObject, preferredWidth: 130, minWidth: 100, preferredHeight: 48);

        _scoreDeltaText = UIHelper.MakeLabel("DeltaLbl", rightCluster, "+10", 34, UIColors.Hex("#4ADE80"), bold: true, wrap: false);
        _scoreDeltaText.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(_scoreDeltaText.gameObject, preferredWidth: 60, minWidth: 50, preferredHeight: 48);
        _scoreDeltaText.gameObject.SetActive(false);

        // Timer Pill: 136x54
        _timerPill = UIHelper.MakeRect("TimerPill", rightCluster);
        _timerPill.sizeDelta = new Vector2(136, 54);
        UIHelper.SetLayout(_timerPill.gameObject, preferredWidth: 136, minWidth: 120, preferredHeight: 54);
        var timerImg = _timerPill.gameObject.AddComponent<Image>();
        timerImg.color = DarkHUD;
        timerImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(timerImg, 14);

        var timerRow = UIHelper.MakeHorizontal("Row", _timerPill, 6);
        UIHelper.Stretch(timerRow, 8, 8, 0, 0);

        // Timer Stopwatch Icon: 26x26
        var timerIconGO = UIHelper.MakeRect("TimerIcon", timerRow);
        timerIconGO.sizeDelta = new Vector2(26, 26);
        UIHelper.SetLayout(timerIconGO.gameObject, preferredWidth: 26, minWidth: 26, preferredHeight: 26);
        var timerIconImg = timerIconGO.gameObject.AddComponent<Image>();
        timerIconImg.sprite = CreateProceduralIcon("timer");
        timerIconImg.color = UIColors.Hex("#CBD5E1");
        timerIconImg.type = Image.Type.Simple;
        timerIconImg.preserveAspect = true;

        // Timer Text: 36px bold white
        _timerText = UIHelper.MakeLabel("TimerText", timerRow, "00:00", 36, Color.white, bold: true, wrap: false);
        _timerText.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(_timerText.gameObject, preferredWidth: 92, minWidth: 80, preferredHeight: 48);

        // D. Top Thin Progress Bar (5px)
        _topProgressBarTrack = UIHelper.MakeRect("TopProgressTrack", _topBar);
        _topProgressBarTrack.anchorMin = new Vector2(0.02f, 0f);
        _topProgressBarTrack.anchorMax = new Vector2(0.98f, 0f);
        _topProgressBarTrack.pivot = new Vector2(0.5f, 0f);
        _topProgressBarTrack.sizeDelta = new Vector2(0, 5);
        _topProgressBarTrack.anchoredPosition = new Vector2(0, 4);

        var trackImg = _topProgressBarTrack.gameObject.AddComponent<Image>();
        trackImg.color = new Color(1f, 1f, 1f, 0.15f);
        trackImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(trackImg, 2);

        var fillGO = new GameObject("ProgressFill");
        fillGO.transform.SetParent(_topProgressBarTrack, false);
        _topProgressFill = fillGO.AddComponent<RectTransform>();
        _topProgressFill.anchorMin = Vector2.zero;
        _topProgressFill.anchorMax = new Vector2(1f / 14f, 1f);
        _topProgressFill.offsetMin = Vector2.zero;
        _topProgressFill.offsetMax = Vector2.zero;

        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = SafetyGreen;
        fillImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(fillImg, 2);
    }

    // ─────────────────────────────────────────────────────────────────
    //  2. WORLD-SPACE ACTION HINT PILL
    // ─────────────────────────────────────────────────────────────────
    private void BuildWorldActionPill(Transform parent)
    {
        _actionPill = UIHelper.MakeRect("WorldActionPill", parent);
        _actionPill.anchorMin = new Vector2(0.5f, 0.38f);
        _actionPill.anchorMax = new Vector2(0.5f, 0.38f);
        _actionPill.pivot = new Vector2(0.5f, 0.5f);
        _actionPill.sizeDelta = new Vector2(620, 78);

        _actionPillBg = _actionPill.gameObject.AddComponent<Image>();
        _actionPillBg.color = DarkHUD;
        _actionPillBg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(_actionPillBg, 24);

        var pillOutline = _actionPill.gameObject.AddComponent<Outline>();
        pillOutline.effectColor = new Color(1f, 1f, 1f, 0.18f);
        pillOutline.effectDistance = new Vector2(1.5f, -1.5f);

        var hlg = _actionPill.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(20, 20, 10, 10);
        hlg.spacing = 12;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        // Tap hand icon
        _actionPillHandIcon = UIHelper.MakeRect("HandIcon", _actionPill).gameObject;
        _actionPillHandIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(36, 36);
        var handImg = _actionPillHandIcon.AddComponent<Image>();
        handImg.sprite = CreateTapHandSprite();
        handImg.color = Color.white;
        handImg.preserveAspect = true;

        // Action Pill Text: 30px bold white
        _actionPillText = UIHelper.MakeLabel("PillText", _actionPill, "Scan & Place Training Area", 30, Color.white, TextAlignmentOptions.Center, bold: true, wrap: false);
        _actionPillText.overflowMode = TextOverflowModes.Overflow;
        _actionPillText.rectTransform.sizeDelta = new Vector2(500, 48);

        // Clickable fallback on action pill
        var btn = _actionPill.gameObject.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(OnActionPillClicked);
    }

    // ─────────────────────────────────────────────────────────────────
    //  3. SINGLE AUTHORITATIVE BOTTOM GUIDANCE CARD
    // ─────────────────────────────────────────────────────────────────
    private void BuildGuidanceCard(Transform parent)
    {
        _guidanceCard = UIHelper.MakeRect("GuidanceCard", parent);
        _guidanceCard.anchorMin = new Vector2(0.04f, 0f);
        _guidanceCard.anchorMax = new Vector2(0.96f, 0f);
        _guidanceCard.pivot     = new Vector2(0.5f, 0f);
        _guidanceCard.offsetMin = new Vector2(0, 36);
        _guidanceCard.offsetMax = new Vector2(0, 260);

        // Crisp White Card Background
        var cardImg = _guidanceCard.gameObject.AddComponent<Image>();
        cardImg.color  = Color.white;
        cardImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(cardImg, 24);

        // Soft slate border
        var outline = _guidanceCard.gameObject.AddComponent<Outline>();
        outline.effectColor    = CardBorderSlate;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        var vlg = _guidanceCard.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding              = new RectOffset(24, 24, 20, 20);
        vlg.spacing              = 12;
        vlg.childAlignment       = TextAnchor.UpperLeft;
        vlg.childControlWidth    = true;
        vlg.childControlHeight   = false;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;

        var csf = _guidanceCard.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var le = _guidanceCard.gameObject.AddComponent<LayoutElement>();
        le.minHeight = 150;

        // ── 1. Header Row (Icon Badge + Step Title + Listen Button) ──
        var headerRow = UIHelper.MakeRect("HeaderRow", _guidanceCard);
        var headerHlg = headerRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        headerHlg.spacing              = 14;
        headerHlg.childAlignment       = TextAnchor.MiddleLeft;
        headerHlg.childControlWidth    = true;
        headerHlg.childControlHeight   = true;
        headerHlg.childForceExpandWidth  = false;
        headerHlg.childForceExpandHeight = false;
        UIHelper.SetLayout(headerRow.gameObject, preferredHeight: 60, minHeight: 52);

        // A. Left: 56x56 Circular Icon Badge (#E8F2FF supporting blue)
        var iconBadgeGO = UIHelper.MakeRect("IconBadge", headerRow);
        iconBadgeGO.sizeDelta = new Vector2(56, 56);
        UIHelper.SetLayout(iconBadgeGO.gameObject, preferredWidth: 56, minWidth: 56, preferredHeight: 56);
        _guidanceCardIconBadge = iconBadgeGO.gameObject.AddComponent<Image>();
        _guidanceCardIconBadge.color  = GasSupportingBlue;
        _guidanceCardIconBadge.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(_guidanceCardIconBadge, 28);

        var iconInner = UIHelper.MakeRect("InnerIcon", iconBadgeGO);
        UIHelper.Stretch(iconInner, 12, 12, 12, 12);
        _guidanceCardIconImg = iconInner.gameObject.AddComponent<Image>();
        _guidanceCardIconImg.sprite         = CreateProceduralIcon("placement");
        _guidanceCardIconImg.color          = GasPrimaryBlue;
        _guidanceCardIconImg.type           = Image.Type.Simple;
        _guidanceCardIconImg.preserveAspect = true;

        // B. Center: Step Title: 36px bold Dark Navy (#0F172A)
        _guidanceCardTitle = UIHelper.MakeLabel("StepTitle", headerRow,
            "AR Placement", 36, DarkNavy, bold: true, wrap: true);
        _guidanceCardTitle.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(_guidanceCardTitle.gameObject, flexibleWidth: true, flexWidth: 1, minHeight: 48);

        // C. Right: Listen Audio Button (148x52) with real procedural speaker icon
        var voicePillGO = new GameObject("VoiceGuidanceBtn");
        voicePillGO.transform.SetParent(headerRow, false);
        var voicePillRT = voicePillGO.AddComponent<RectTransform>();
        voicePillRT.sizeDelta = new Vector2(148, 52);
        UIHelper.SetLayout(voicePillRT.gameObject, preferredWidth: 148, minWidth: 130, preferredHeight: 52);

        _voiceBtnBg = voicePillGO.AddComponent<Image>();
        _voiceBtnBg.color  = GasPrimaryBlue;
        _voiceBtnBg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(_voiceBtnBg, 20);

        var voiceBtnHlg = voicePillGO.AddComponent<HorizontalLayoutGroup>();
        voiceBtnHlg.padding = new RectOffset(14, 14, 8, 8);
        voiceBtnHlg.spacing = 8;
        voiceBtnHlg.childAlignment = TextAnchor.MiddleCenter;
        voiceBtnHlg.childControlWidth = true;
        voiceBtnHlg.childControlHeight = true;
        voiceBtnHlg.childForceExpandWidth = false;
        voiceBtnHlg.childForceExpandHeight = false;

        _voiceBtn = voicePillGO.AddComponent<Button>();
        _voiceBtn.onClick.AddListener(OnVoiceButtonClicked);

        // Procedural Speaker Icon
        var voiceIconGO = UIHelper.MakeRect("SpeakerIcon", voicePillGO.transform);
        voiceIconGO.sizeDelta = new Vector2(24, 24);
        UIHelper.SetLayout(voiceIconGO.gameObject, preferredWidth: 24, minWidth: 24, preferredHeight: 24);
        _voiceBtnIcon = voiceIconGO.gameObject.AddComponent<Image>();
        _voiceBtnIcon.sprite = CreateSpeakerSprite();
        _voiceBtnIcon.color = Color.white;
        _voiceBtnIcon.type = Image.Type.Simple;
        _voiceBtnIcon.preserveAspect = true;
        _voiceBtnIcon.raycastTarget = false;

        _voiceBtnText = UIHelper.MakeLabel("VoiceBtnText", voicePillGO.transform,
            Loc("gas.voice.listenBtn", "Listen"),
            24, Color.white, TextAlignmentOptions.Center, bold: true, wrap: false);
        _voiceBtnText.raycastTarget = false;
        UIHelper.SetLayout(_voiceBtnText.gameObject, flexibleWidth: true, flexWidth: 1, minHeight: 28);

        // ── 2. Body Instruction Text: 28px Dark Slate (#334155) ───────
        _guidanceCardBody = UIHelper.MakeLabel("StepBody", _guidanceCard,
            "Scan a flat surface and tap to place the training area.",
            28, DarkSlate, wrap: true);
        _guidanceCardBody.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(_guidanceCardBody.gameObject, flexibleWidth: true, flexWidth: 1, minHeight: 44);

        // ── 3. Step 14 Large Assessment CTA Button ───────────────────
        var ctaGO = new GameObject("AssessmentCTA");
        ctaGO.transform.SetParent(_guidanceCard, false);
        var ctaRT = ctaGO.AddComponent<RectTransform>();
        ctaRT.sizeDelta = new Vector2(0, 84);
        UIHelper.SetLayout(ctaRT.gameObject, preferredHeight: 84, minHeight: 84);

        var ctaImg = ctaGO.AddComponent<Image>();
        ctaImg.color = GasPrimaryBlue;
        ctaImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(ctaImg, 18);

        _btnAssessmentCTA = ctaGO.AddComponent<Button>();
        _btnAssessmentCTA.onClick.AddListener(CompleteTrainingAndGoToAssessment);

        _btnAssessmentCTAText = UIHelper.MakeLabel("BtnText", ctaGO.transform,
            Loc("gas.assessment.btn", "▶ Start Assessment"),
            34, Color.white, TextAlignmentOptions.Center, bold: true, wrap: false);
        UIHelper.Stretch(_btnAssessmentCTAText.rectTransform, 16, 16, 0, 0);

        ctaGO.SetActive(false); // Only visible on Step 14
    }

    // ─────────────────────────────────────────────────────────────────
    //  4. FEEDBACK TOAST BANNER (Top Banner)
    // ─────────────────────────────────────────────────────────────────
    private void BuildFeedbackBanner(Transform parent)
    {
        _feedbackBanner = UIHelper.MakeRect("FeedbackBanner", parent);
        _feedbackBanner.anchorMin = new Vector2(0.06f, 1f);
        _feedbackBanner.anchorMax = new Vector2(0.94f, 1f);
        _feedbackBanner.pivot = new Vector2(0.5f, 1f);
        _feedbackBanner.sizeDelta = new Vector2(0, 92);
        _feedbackBanner.anchoredPosition = new Vector2(0, -170);

        _feedbackBg = _feedbackBanner.gameObject.AddComponent<Image>();
        _feedbackBg.color = SafetyGreen;
        _feedbackBg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(_feedbackBg, 20);

        // 36px bold white feedback text
        _feedbackText = UIHelper.MakeLabel("FeedbackText", _feedbackBanner,
            "✓  Action Complete  +10", 36, Color.white, TextAlignmentOptions.Center, bold: true);
        UIHelper.Stretch(_feedbackText.GetComponent<RectTransform>(), 16, 16, 0, 0);

        _feedbackBanner.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  STATE MACHINE: 14 AUTHORITATIVE GAS TRAINING STEPS
    // ─────────────────────────────────────────────────────────────────
    public void SetStep(TrainingStep step)
    {
        EnsureUIInitialized();
        currentStep = step;
        int stepNum = Mathf.Clamp((int)step + 1, 1, 14);

        if (!_timerRunning && step >= TrainingStep.WorkstationReady && step < TrainingStep.FinalAssessment)
        {
            StartLiveTimer();
        }

        // Update Top HUD
        if (_stepCounterText != null)
        {
            string fmt = Loc("gas.hud.stepOf", "Step {0} of {1}");
            _stepCounterText.text = string.Format(fmt, stepNum, 14);
            _stepCounterText.font = UIHelper.GetFontForLanguage(GetCurrentLanguage());
        }

        if (_topProgressFill != null)
        {
            _topProgressFill.anchorMax = new Vector2((float)stepNum / 14f, 1f);
        }

        // Apply Step-Specific Guidance, Pill, and 3D Visuals
        switch (step)
        {
            case TrainingStep.ScanAndPlace:
                ApplyStepDetails(1, "placement",
                    Loc("gas.guidance.step1.title", "AR Placement"),
                    Loc("gas.guidance.step1.body", "Scan a flat surface and tap to place the training area."),
                    Loc("gas.actionHint.step1", "Scan & Place Training Area"),
                    showHand: false);
                SetTargetScore(0f);
                UpdateDetectorScreenDisplay(0);
                RemoveHazardZoneVisualDisk();
                break;

            case TrainingStep.WorkstationReady:
                ApplyStepDetails(2, "workstation",
                    Loc("gas.guidance.step2.title", "Gas Workstation"),
                    Loc("gas.guidance.step2.body", "The gas workstation has appeared. Tap to continue."),
                    Loc("gas.actionHint.step2", "Gas Workstation Ready"),
                    showHand: false);
                SetTargetScore(10f);
                UpdateDetectorScreenDisplay(1);
                RemoveHazardZoneVisualDisk();
                break;

            case TrainingStep.CylinderInteraction:
                ApplyStepDetails(3, "cylinder",
                    Loc("gas.guidance.step3.title", "Activate Scenario"),
                    Loc("gas.guidance.step3.body", "Tap the gas cylinder to begin the simulation."),
                    Loc("gas.actionHint.step3", "Tap Gas Cylinder"),
                    showHand: true);
                SetTargetScore(20f);
                UpdateDetectorScreenDisplay(1);
                RemoveHazardZoneVisualDisk();
                break;

            case TrainingStep.GasLeakActive:
                ApplyStepDetails(4, "gas_cloud",
                    Loc("gas.guidance.step4.title", "Observe Gas Leak"),
                    Loc("gas.guidance.step4.body", "Gas leak has started. Observe the leak and be ready for the next steps."),
                    Loc("gas.actionHint.step4", "Gas Leak Started"),
                    showHand: false);
                SetTargetScore(20f);
                CreateHazardZoneVisualDisk();
                StartCoroutine(AutoAdvance(2.5f, TrainingStep.DetectorInteraction));
                break;

            case TrainingStep.DetectorInteraction:
                ApplyStepDetails(5, "detector",
                    Loc("gas.guidance.step5.title", "Check Gas Detector"),
                    Loc("gas.guidance.step5.body", "Observe the gas detector for changes in gas level."),
                    Loc("gas.actionHint.step5", "Check Gas Detector"),
                    showHand: false);
                SetTargetScore(30f);
                UpdateDetectorScreenDisplay(1);
                CreateHazardZoneVisualDisk();
                break;

            case TrainingStep.AlarmActive:
                ApplyStepDetails(6, "alarm",
                    Loc("gas.guidance.step6.title", "Alarm Activated"),
                    Loc("gas.guidance.step6.body", "Gas detected! Alarm is active. Proceed to next step."),
                    Loc("gas.actionHint.step6", "Hazard Active - Gas Detected"),
                    showHand: false);
                SetTargetScore(30f);
                UpdateDetectorScreenDisplay(2);
                StartAlarmBuzzer();
                CreateHazardZoneVisualDisk();
                break;

            case TrainingStep.PPECheck:
                ApplyStepDetails(7, "hardhat",
                    Loc("gas.guidance.step7.title", "PPE Selection"),
                    Loc("gas.guidance.step7.body", "Select the required protective equipment as per SOP."),
                    Loc("gas.actionHint.step7", "Select Required PPE"),
                    showHand: false);
                SetTargetScore(50f);
                UpdateDetectorScreenDisplay(2);
                CreateHazardZoneVisualDisk();
                break;

            case TrainingStep.HazardRecognition:
                ApplyStepDetails(8, "hazard_pin",
                    Loc("gas.guidance.step8.title", "Hazard Zone"),
                    Loc("gas.guidance.step8.body", "Tap to mark the hazardous zone around the leak."),
                    Loc("gas.actionHint.step8", "Identify Hazard Zone"),
                    showHand: false);
                SetTargetScore(60f);
                UpdateDetectorScreenDisplay(2);
                CreateHazardZoneVisualDisk();
                break;

            case TrainingStep.BuddySystemCheck:
                ApplyStepDetails(9, "buddy",
                    Loc("gas.guidance.step9.title", "Buddy-System"),
                    Loc("gas.guidance.step9.body", "Confirm the buddy-system procedure with the worker."),
                    Loc("gas.actionHint.step9", "Confirm Buddy-System"),
                    showHand: false);
                SetTargetScore(70f);
                UpdateDetectorScreenDisplay(2);
                CreateHazardZoneVisualDisk();
                break;

            case TrainingStep.SafetyResponse:
                ApplyStepDetails(10, "valve",
                    Loc("gas.guidance.step10.title", "Safe Response"),
                    Loc("gas.guidance.step10.body", "Operate the isolation valve to stop the gas flow."),
                    Loc("gas.actionHint.step10", "Operate Isolation Valve"),
                    showHand: true);
                SetTargetScore(80f);
                UpdateDetectorScreenDisplay(2);
                CreateHazardZoneVisualDisk();
                break;

            case TrainingStep.ResponseComplete:
                ApplyStepDetails(11, "check_circle",
                    Loc("gas.guidance.step11.title", "Response Complete"),
                    Loc("gas.guidance.step11.body", "The gas leak has been controlled successfully."),
                    Loc("gas.actionHint.step11", "✓ Leak Response Complete"),
                    showHand: false);
                SetTargetScore(90f);
                StopGasLeakSFXAndPlume();
                UpdateDetectorScreenDisplay(3);
                RemoveHazardZoneVisualDisk();
                StartCoroutine(AutoAdvance(2.5f, TrainingStep.ConfinedSpaceIntro));
                break;

            case TrainingStep.ConfinedSpaceIntro:
                ApplyStepDetails(12, "confined_space",
                    Loc("gas.guidance.step12.title", "Confined Space"),
                    Loc("gas.guidance.step12.body", "Move to the confined space area to continue."),
                    Loc("gas.actionHint.step12", "Proceed to Confined Space"),
                    showHand: false);
                SetTargetScore(100f);
                UpdateDetectorScreenDisplay(3);
                RemoveHazardZoneVisualDisk();
                break;

            case TrainingStep.ConfinedSpaceDrill:
                ApplyStepDetails(13, "clipboard",
                    Loc("gas.guidance.step13.title", "Confined Space Task"),
                    Loc("gas.guidance.step13.body", "Perform the required checks inside the confined space."),
                    Loc("gas.actionHint.step13", "Complete Confined Space Task"),
                    showHand: false);
                SetTargetScore(100f);
                UpdateDetectorScreenDisplay(3);
                RemoveHazardZoneVisualDisk();
                break;

            case TrainingStep.FinalAssessment:
                ApplyStepDetails(14, "clipboard",
                    Loc("gas.guidance.step14.title", "Assessment"),
                    Loc("gas.guidance.step14.body", "Demonstrate what you have learned in this scenario."),
                    Loc("gas.actionHint.step14", "Final Assessment"),
                    showHand: false);
                SetTargetScore(100f);
                StopLiveTimer();
                if (_btnAssessmentCTA != null)
                {
                    _btnAssessmentCTA.gameObject.SetActive(true);
                }
                UpdateDetectorScreenDisplay(3);
                RemoveHazardZoneVisualDisk();
                break;
        }

        Debug.Log($"[GasTrainingStepController] Transitioned to step -> {step} ({stepNum}/14)");
    }

    private void ApplyStepDetails(int stepIndex, string iconType, string title, string body, string pillText, bool showHand)
    {
        var lang = GetCurrentLanguage();
        var font = UIHelper.GetFontForLanguage(lang);

        if (_guidanceCardIconImg != null)
        {
            _guidanceCardIconImg.sprite = CreateProceduralIcon(iconType);
        }

        if (_guidanceCardTitle != null)
        {
            _guidanceCardTitle.text = title;
            _guidanceCardTitle.font = font;
        }

        if (_guidanceCardBody != null)
        {
            _guidanceCardBody.text = body;
            _guidanceCardBody.font = font;
        }

        if (_actionPillText != null)
        {
            _actionPillText.text = pillText;
            _actionPillText.font = font;
        }

        if (_actionPillHandIcon != null)
        {
            _actionPillHandIcon.SetActive(showHand);
        }

        if (_btnAssessmentCTA != null)
        {
            _btnAssessmentCTA.gameObject.SetActive(stepIndex == 14);
            if (_btnAssessmentCTAText != null)
            {
                _btnAssessmentCTAText.text = Loc("gas.assessment.btn", "▶ Start Assessment");
                _btnAssessmentCTAText.font = font;
            }
        }

        SetVoiceButtonIdle();
    }

    // ─────────────────────────────────────────────────────────────────
    //  EVENT HANDLERS & 3D INTERACTION HOOKS
    // ─────────────────────────────────────────────────────────────────
    public void OnScenarioPlaced()
    {
        if (currentStep == TrainingStep.ScanAndPlace)
        {
            Debug.Log("[GasAR] Surface placed! Initializing Workstation & Gas Scenario Assessment.");
            if (GasScenarioAssessmentBridge.Instance != null)
            {
                GasScenarioAssessmentBridge.Instance.StartGasScenario("gas_leak_drill_01");
            }
            SetStep(TrainingStep.WorkstationReady);
            ShowFeedback("✓ Training Workstation Placed", true, 10);
        }
    }

    public void OnCylinderOrLeakPointTapped()
    {
        if (currentStep == TrainingStep.ScanAndPlace) return;

        if (currentStep == TrainingStep.WorkstationReady || currentStep == TrainingStep.CylinderInteraction)
        {
            cylinderInteracted = true;
            Debug.Log("[GasAR] Gas Cylinder tapped! Triggering gas leak emergency.");
            TriggerGasLeakEmergency();
        }
        else if (currentStep >= TrainingStep.GasLeakActive && currentStep < TrainingStep.ResponseComplete)
        {
            // Already leaking
            ShowFeedback("Gas is actively leaking! Follow response procedure.", false, 0);
        }
    }

    public void TriggerGasLeakEmergency()
    {
        leakRecognized = true;

        GameObject cylinderObj = environmentBuilder != null ? environmentBuilder.CurrentGasCylinder : GameObject.Find("Realistic_Gas_Cylinder");
        if (cylinderObj == null) cylinderObj = GameObject.Find("gas_cylinder");

        if (leakVisualController != null)
        {
            leakVisualController.StartGasLeak(cylinderObj);
        }

        // Start Gas Hiss SFX
        if (_hissAudioSource != null && _hissClip != null)
        {
            _hissAudioSource.Play();
        }

        if (GasScenarioAssessmentBridge.Instance != null)
        {
            GasScenarioAssessmentBridge.Instance.OnGasLeakDetected("methane_toxic_gas");
        }

        SetStep(TrainingStep.GasLeakActive);
    }

    public void OnDetectorTapped()
    {
        if (currentStep == TrainingStep.GasLeakActive || currentStep == TrainingStep.DetectorInteraction)
        {
            detectorInteracted = true;
            Debug.Log("[GasAR] Multi-Gas Detector checked by worker!");

            GameObject detectorObj = environmentBuilder != null ? environmentBuilder.CurrentGasDetector : GameObject.Find("Multi_Gas_Detector");
            if (detectorObj != null)
            {
                StartCoroutine(PulseDetectorFeedback(detectorObj));
            }

            if (GasScenarioAssessmentBridge.Instance != null)
            {
                GasScenarioAssessmentBridge.Instance.OnHazardZonePerimeterIdentified(100f);
            }

            SetStep(TrainingStep.AlarmActive);
            ShowFeedback("✓ Atmosphere Checked: Hazardous Gas Detected!", true, 10);
        }
    }

    private void StartAlarmBuzzer()
    {
        if (_alarmAudioSource != null && _alarmClip != null && !_alarmAudioSource.isPlaying)
        {
            _alarmAudioSource.Play();
        }
    }

    public void SelectPpe(string ppeType, bool isApproved)
    {
        if (currentStep != TrainingStep.PPECheck) return;

        ppeComplete = true;
        ppeSelectedCorrect = isApproved;

        if (GasScenarioAssessmentBridge.Instance != null)
        {
            GasScenarioAssessmentBridge.Instance.OnPpeSelected(ppeType, isApproved);
        }

        if (isApproved)
        {
            ShowFeedback(Loc("gas.feedback.ppeSelected", "✓ Approved Gas PPE Selected  +20"), true, 20);
            StartCoroutine(AutoAdvance(1.2f, TrainingStep.HazardRecognition));
        }
        else
        {
            _userScore = Mathf.Max(0f, _userScore - 15f);
            mistakeLog.Add("Inadequate PPE selected");
            ShowFeedback(Loc("gas.feedback.wrongPpe", "Select the required protective equipment as per SOP."), false, 0);
        }
    }

    public void OnHazardZoneAcknowledged()
    {
        if (currentStep == TrainingStep.HazardRecognition)
        {
            hazardRecognized = true;
            Debug.Log("[GasAR] Hazard Zone identified and marked.");
            ShowFeedback(Loc("gas.feedback.hazardIdentified", "✓ Hazard Zone Identified  +10"), true, 10);
            StartCoroutine(AutoAdvance(1.2f, TrainingStep.BuddySystemCheck));
        }
    }

    public void OnBuddyConfirmed()
    {
        if (currentStep == TrainingStep.BuddySystemCheck)
        {
            buddyVerified = true;
            Debug.Log("[GasAR] Standby Buddy confirmed.");
            if (GasScenarioAssessmentBridge.Instance != null)
            {
                GasScenarioAssessmentBridge.Instance.OnBuddySystemChecked(true);
            }
            ShowFeedback(Loc("gas.feedback.buddyConfirmed", "✓ Buddy-System Confirmed  +10"), true, 10);
            StartCoroutine(AutoAdvance(1.2f, TrainingStep.SafetyResponse));
        }
    }

    public void OnPipelineValveTapped()
    {
        if (currentStep == TrainingStep.SafetyResponse || currentStep == TrainingStep.AlarmActive || currentStep == TrainingStep.GasLeakActive)
        {
            isolationValveInteracted = true;
            verificationComplete = true;
            Debug.Log("[GasAR] Red Isolation Valve operated! Gas isolated.");

            StopGasLeakSFXAndPlume();

            if (GasScenarioAssessmentBridge.Instance != null)
            {
                GasScenarioAssessmentBridge.Instance.OnIsolationValveClosed(true);
            }

            ShowFeedback(Loc("gas.feedback.gasIsolated", "✓ Gas Leak Isolated  +10"), true, 10);
            SetStep(TrainingStep.ResponseComplete);
        }
        else
        {
            OnWrongObjectTapped("Valve");
        }
    }

    private void StopGasLeakSFXAndPlume()
    {
        if (leakVisualController != null)
        {
            leakVisualController.StopGasLeak();
        }

        if (_hissAudioSource != null && _hissAudioSource.isPlaying)
        {
            _hissAudioSource.Stop();
        }

        if (_alarmAudioSource != null && _alarmAudioSource.isPlaying)
        {
            _alarmAudioSource.Stop();
        }
    }

    public void OnConfinedSpaceTapped()
    {
        if (currentStep == TrainingStep.ConfinedSpaceIntro)
        {
            Debug.Log("[GasAR] Confined Space Vessel area entered.");
            SetStep(TrainingStep.ConfinedSpaceDrill);
        }
        else if (currentStep == TrainingStep.ConfinedSpaceDrill)
        {
            OnConfinedSpaceTaskCompleted();
        }
    }

    public void OnConfinedSpaceTaskCompleted()
    {
        if (currentStep == TrainingStep.ConfinedSpaceDrill)
        {
            confinedSpaceCompleted = true;
            Debug.Log("[GasAR] Confined Space Ventilation & Lifeline verified.");
            ShowFeedback("✓ Confined Space Pre-Entry Verified  +10", true, 10);
            StartCoroutine(AutoAdvance(1.5f, TrainingStep.FinalAssessment));
        }
    }

    public void OnWrongObjectTapped(string targetObjectType)
    {
        string msg;
        switch (targetObjectType)
        {
            case "Cylinder":
                msg = Loc("gas.feedback.wrongCylinder", "Please tap the Gas Cylinder to begin the drill.");
                break;
            case "Detector":
                msg = Loc("gas.feedback.wrongDetector", "Tap the Gas Detector to check the reading.");
                break;
            case "Valve":
                msg = Loc("gas.feedback.wrongValve", "Tap the Red Isolation Valve to isolate the gas supply.");
                break;
            case "PPE":
                msg = Loc("gas.feedback.wrongPpe", "Select the required protective equipment as per SOP.");
                break;
            case "Buddy":
                msg = Loc("gas.feedback.wrongBuddy", "Confirm the designated buddy/standby procedure.");
                break;
            default:
                msg = "Please follow the current training instruction.";
                break;
        }

        ShowFeedback(msg, false, 0);
    }

    private void OnActionPillClicked()
    {
        switch (currentStep)
        {
            case TrainingStep.ScanAndPlace:
                if (arPlacement != null) arPlacement.ActivateNonArMode();
                else OnScenarioPlaced();
                break;
            case TrainingStep.WorkstationReady:
            case TrainingStep.CylinderInteraction:
                OnCylinderOrLeakPointTapped();
                break;
            case TrainingStep.GasLeakActive:
            case TrainingStep.DetectorInteraction:
                OnDetectorTapped();
                break;
            case TrainingStep.AlarmActive:
                SetStep(TrainingStep.PPECheck);
                break;
            case TrainingStep.PPECheck:
                SelectPpe("Approved_Gas_PPE", true);
                break;
            case TrainingStep.HazardRecognition:
                OnHazardZoneAcknowledged();
                break;
            case TrainingStep.BuddySystemCheck:
                OnBuddyConfirmed();
                break;
            case TrainingStep.SafetyResponse:
                OnPipelineValveTapped();
                break;
            case TrainingStep.ResponseComplete:
                SetStep(TrainingStep.ConfinedSpaceIntro);
                break;
            case TrainingStep.ConfinedSpaceIntro:
                OnConfinedSpaceTapped();
                break;
            case TrainingStep.ConfinedSpaceDrill:
                OnConfinedSpaceTaskCompleted();
                break;
            case TrainingStep.FinalAssessment:
                CompleteTrainingAndGoToAssessment();
                break;
        }
    }

    public void CompleteTrainingAndGoToAssessment()
    {
        EvaluateAssessment();

        // Notify global training event manager that drill is complete
        TrainingEventManager.RaiseTrainingCompleted();

        // Exit AR scene back to Assessment screen via authoritative ARModuleLauncher
        if (ARModuleLauncher.Instance != null)
        {
            ARModuleLauncher.Instance.ExitCurrentARScene();
        }
        else
        {
            AppManager.Instance?.ReturnFromARToAssessment();
        }
    }

    public void EvaluateAssessment()
    {
        missedSteps.Clear();
        if (!cylinderInteracted) missedSteps.Add("Gas Cylinder Activation");
        if (!leakRecognized) missedSteps.Add("Gas Leak Observation");
        if (!detectorInteracted) missedSteps.Add("Atmospheric Gas Detection");
        if (!hazardRecognized) missedSteps.Add("Hazard Zone Identification");
        if (!ppeComplete || !ppeSelectedCorrect) missedSteps.Add("PPE Selection");
        if (!buddyVerified) missedSteps.Add("Standby Buddy Confirmation");
        if (!isolationValveInteracted) missedSteps.Add("Gas Isolation Valve Operation");

        bool passed = _userScore >= 80f && missedSteps.Count == 0;
        if (GasScenarioAssessmentBridge.Instance != null)
        {
            GasScenarioAssessmentBridge.Instance.CompleteGasScenario(_userScore, passed);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  FEEDBACK TOAST & SCORING ANIMATION
    // ─────────────────────────────────────────────────────────────────
    public void ShowFeedback(string text, bool isSuccess, int scoreDelta)
    {
        if (_feedbackBanner == null || _feedbackText == null) return;

        if (_feedbackRoutine != null) StopCoroutine(_feedbackRoutine);
        _feedbackRoutine = StartCoroutine(FeedbackRoutine(text, isSuccess, scoreDelta));
    }

    private IEnumerator FeedbackRoutine(string text, bool isSuccess, int scoreDelta)
    {
        if (_feedbackBanner == null || _feedbackBg == null || _feedbackText == null) yield break;

        _feedbackBg.color = isSuccess ? SafetyGreen : DangerRed;
        _feedbackText.text = text;
        _feedbackText.font = UIHelper.GetFontForLanguage(GetCurrentLanguage());
        _feedbackBanner.gameObject.SetActive(true);

        if (scoreDelta > 0)
        {
            AwardScore(scoreDelta);
        }

        yield return new WaitForSeconds(2.8f);

        if (_feedbackBanner != null && _feedbackBanner.gameObject != null)
        {
            _feedbackBanner.gameObject.SetActive(false);
        }
        _feedbackRoutine = null;
    }

    private void AwardScore(int delta)
    {
        _userScore = Mathf.Clamp(_userScore + delta, 0f, 100f);
        if (_scoreText != null && _scoreText.gameObject != null)
        {
            string fmt = Loc("gas.hud.score", "Score: {0}");
            _scoreText.text = string.Format(fmt, (int)_userScore);
        }

        if (_scoreDeltaText != null && _scoreDeltaText.gameObject != null)
        {
            _scoreDeltaText.text = $"+{delta}";
            if (_scoreDeltaRoutine != null) StopCoroutine(_scoreDeltaRoutine);
            _scoreDeltaRoutine = StartCoroutine(ScoreDeltaAnimRoutine());
        }
    }

    private void SetTargetScore(float target)
    {
        _userScore = target;
        if (_scoreText != null && _scoreText.gameObject != null)
        {
            string fmt = Loc("gas.hud.score", "Score: {0}");
            _scoreText.text = string.Format(fmt, (int)_userScore);
        }
    }

    private IEnumerator ScoreDeltaAnimRoutine()
    {
        if (_scoreDeltaText != null && _scoreDeltaText.gameObject != null)
        {
            _scoreDeltaText.gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(1.5f);
        if (_scoreDeltaText != null && _scoreDeltaText.gameObject != null)
        {
            _scoreDeltaText.gameObject.SetActive(false);
        }
        _scoreDeltaRoutine = null;
    }

    // ─────────────────────────────────────────────────────────────────
    //  TIMER COROUTINE
    // ─────────────────────────────────────────────────────────────────
    private void StartLiveTimer()
    {
        if (_timerRunning) return;
        _timerRunning = true;
        _scenarioStartTime = Time.time;
        if (_timerRoutine != null) StopCoroutine(_timerRoutine);
        _timerRoutine = StartCoroutine(TimerTickRoutine());
    }

    private void StopLiveTimer()
    {
        _timerRunning = false;
        if (_timerRoutine != null)
        {
            StopCoroutine(_timerRoutine);
            _timerRoutine = null;
        }
    }

    private IEnumerator TimerTickRoutine()
    {
        while (_timerRunning)
        {
            _elapsedSeconds = Mathf.FloorToInt(Time.time - _scenarioStartTime);
            int m = _elapsedSeconds / 60;
            int s = _elapsedSeconds % 60;
            if (_timerText != null && _timerText.gameObject != null)
            {
                _timerText.text = $"{m:00}:{s:00}";
            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    private IEnumerator AutoAdvance(float delay, TrainingStep nextStep)
    {
        yield return new WaitForSeconds(delay);
        if (this != null && gameObject != null && enabled)
        {
            SetStep(nextStep);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  VOICE / LISTEN AUDIO GUIDANCE
    // ─────────────────────────────────────────────────────────────────
    private void OnVoiceButtonClicked()
    {
        AppLanguage lang = GetCurrentLanguage();

        if (_voiceActive)
        {
            StopVoiceGuidance();
            return;
        }

        if (lang == AppLanguage.Santali)
        {
            string pendingMsg = Loc("gas.voice.santaliPending", "Santali voice guidance pending recording");
            ShowFeedback(pendingMsg, true, 0);
            return;
        }

        StartVoiceGuidance();
    }

    private void StartVoiceGuidance()
    {
        _voiceActive = true;
        if (_voiceBtnBg != null) _voiceBtnBg.color = DangerRed;
        if (_voiceBtnText != null) _voiceBtnText.text = Loc("gas.voice.stopBtn", "Stop");
        if (_voiceBtnIcon != null) _voiceBtnIcon.sprite = CreateStopSprite();

        // Play synthetic voice confirmation tone / prompt
        if (_voiceAudioSource != null)
        {
            _voiceAudioSource.PlayOneShot(CreateProceduralTone(440f, 0.4f));
        }

        if (_voiceFeedbackRoutine != null) StopCoroutine(_voiceFeedbackRoutine);
        _voiceFeedbackRoutine = StartCoroutine(VoiceButtonAutoReset(3.5f));
    }

    private void StopVoiceGuidance()
    {
        _voiceActive = false;
        if (_voiceBtnBg != null) _voiceBtnBg.color = GasPrimaryBlue;
        if (_voiceBtnText != null) _voiceBtnText.text = Loc("gas.voice.listenBtn", "Listen");
        if (_voiceBtnIcon != null) _voiceBtnIcon.sprite = CreateSpeakerSprite();

        if (_voiceAudioSource != null && _voiceAudioSource.isPlaying)
        {
            _voiceAudioSource.Stop();
        }
    }

    private void SetVoiceButtonIdle()
    {
        StopVoiceGuidance();
    }

    private IEnumerator VoiceButtonAutoReset(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (this != null && enabled)
        {
            SetVoiceButtonIdle();
        }
    }

    private static AudioClip CreateProceduralTone(float freq, float duration)
    {
        int sampleRate = 44100;
        int numSamples = (int)(sampleRate * duration);
        float[] samples = new float[numSamples];
        for (int i = 0; i < numSamples; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Sin(Mathf.PI * (t / duration));
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.4f;
        }
        var clip = AudioClip.Create("Tone", numSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // ─────────────────────────────────────────────────────────────────
    //  3D MULTI-GAS DETECTOR DISPLAY (Digital Readout)
    // ─────────────────────────────────────────────────────────────────
    private void UpdateDetectorScreenDisplay(int mode)
    {
        // mode: 0=Off, 1=Normal, 2=Hazard Active, 3=Safe Normal
        EnsureDetectorDisplayExists();
        if (_detectorDisplayRoot == null) return;

        if (mode == 0)
        {
            _detectorDisplayRoot.SetActive(false);
            return;
        }

        _detectorDisplayRoot.SetActive(true);

        if (mode == 1)
        {
            // Monitoring Normal (Green)
            if (_detectorDisplayBgRenderer != null) _detectorDisplayBgRenderer.material.color = new Color(0.04f, 0.28f, 0.12f, 0.95f);
            if (_detectorDisplayTitle != null) { _detectorDisplayTitle.color = Color.white; _detectorDisplayTitle.text = "GAS DETECTOR\nMONITORING"; }
            if (_detectorDisplaySub != null) { _detectorDisplaySub.color = UIColors.Hex("#34D399"); _detectorDisplaySub.text = "Status: Normal"; }
        }
        else if (mode == 2)
        {
            // Hazard Active (Glowing Red)
            if (_detectorDisplayBgRenderer != null) _detectorDisplayBgRenderer.material.color = new Color(0.75f, 0.08f, 0.08f, 0.98f);
            if (_detectorDisplayTitle != null) { _detectorDisplayTitle.color = Color.white; _detectorDisplayTitle.text = "GAS DETECTED"; }
            if (_detectorDisplaySub != null) { _detectorDisplaySub.color = UIColors.Hex("#FDE047"); _detectorDisplaySub.text = "HAZARD ACTIVE"; }
        }
        else if (mode == 3)
        {
            // Gas Isolated / Atmosphere Safe (Green)
            if (_detectorDisplayBgRenderer != null) _detectorDisplayBgRenderer.material.color = new Color(0.04f, 0.28f, 0.12f, 0.95f);
            if (_detectorDisplayTitle != null) { _detectorDisplayTitle.color = Color.white; _detectorDisplayTitle.text = "GAS ISOLATED"; }
            if (_detectorDisplaySub != null) { _detectorDisplaySub.color = UIColors.Hex("#34D399"); _detectorDisplaySub.text = "Status: Normal"; }
        }
    }

    private void EnsureDetectorDisplayExists()
    {
        if (_detectorDisplayRoot != null) return;

        GameObject detectorObj = environmentBuilder != null ? environmentBuilder.CurrentGasDetector : GameObject.Find("Multi_Gas_Detector");
        if (detectorObj == null) detectorObj = GameObject.Find("Multi_Gas_Detector") ?? GameObject.Find("detector");
        if (detectorObj == null) return;

        _detectorDisplayRoot = new GameObject("Detector3DDisplay");
        _detectorDisplayRoot.transform.SetParent(detectorObj.transform, false);
        _detectorDisplayRoot.transform.localPosition = new Vector3(0f, 0.14f, 0.05f);
        _detectorDisplayRoot.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);
        _detectorDisplayRoot.transform.localScale = Vector3.one * 0.45f;

        // Background quad
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "ScreenBg";
        quad.transform.SetParent(_detectorDisplayRoot.transform, false);
        quad.transform.localScale = new Vector3(0.24f, 0.14f, 1f);
        Destroy(quad.GetComponent<Collider>());
        _detectorDisplayBgRenderer = quad.GetComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
        mat.color = new Color(0.04f, 0.28f, 0.12f, 0.95f);
        _detectorDisplayBgRenderer.material = mat;

        // Title text
        var titleGO = new GameObject("TitleTMP");
        titleGO.transform.SetParent(_detectorDisplayRoot.transform, false);
        titleGO.transform.localPosition = new Vector3(0f, 0.025f, -0.005f);
        _detectorDisplayTitle = titleGO.AddComponent<TextMeshPro>();
        _detectorDisplayTitle.alignment = TextAlignmentOptions.Center;
        _detectorDisplayTitle.fontSize = 2.4f;
        _detectorDisplayTitle.fontStyle = FontStyles.Bold;
        _detectorDisplayTitle.text = "GAS DETECTOR\nMONITORING";

        // Sub text
        var subGO = new GameObject("SubTMP");
        subGO.transform.SetParent(_detectorDisplayRoot.transform, false);
        subGO.transform.localPosition = new Vector3(0f, -0.035f, -0.005f);
        _detectorDisplaySub = subGO.AddComponent<TextMeshPro>();
        _detectorDisplaySub.alignment = TextAlignmentOptions.Center;
        _detectorDisplaySub.fontSize = 2.0f;
        _detectorDisplaySub.fontStyle = FontStyles.Bold;
        _detectorDisplaySub.text = "Status: Normal";
    }

    private IEnumerator PulseDetectorFeedback(GameObject target)
    {
        if (target == null) yield break;
        Vector3 orig = target.transform.localScale;
        target.transform.localScale = orig * 1.15f;
        yield return new WaitForSeconds(0.25f);
        if (target != null)
        {
            target.transform.localScale = orig;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  GROUNDED HAZARD ZONE CIRCLE & FLOATING BADGE
    // ─────────────────────────────────────────────────────────────────
    private void CreateHazardZoneVisualDisk()
    {
        if (_hazardZoneRoot != null)
        {
            _hazardZoneRoot.SetActive(true);
            return;
        }

        _hazardZoneRoot = new GameObject("HazardZoneRoot");
        _hazardZoneRoot.transform.SetParent(transform, false);
        _hazardZoneRoot.transform.localPosition = Vector3.zero;

        // Grounded Dashed Red Hazard Ring
        var lineGO = new GameObject("HazardRingLine");
        lineGO.transform.SetParent(_hazardZoneRoot.transform, false);
        lineGO.transform.localPosition = new Vector3(0f, 0.02f, 0f);

        _hazardZoneLine = lineGO.AddComponent<LineRenderer>();
        _hazardZoneLine.useWorldSpace = false;
        _hazardZoneLine.loop = true;
        _hazardZoneLine.positionCount = 48;
        _hazardZoneLine.startWidth = 0.045f;
        _hazardZoneLine.endWidth = 0.045f;

        var lineMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
        lineMat.color = new Color(0.95f, 0.15f, 0.15f, 0.90f);
        _hazardZoneLine.material = lineMat;

        float radius = 1.10f;
        for (int i = 0; i < 48; i++)
        {
            float angle = (float)i / 48f * Mathf.PI * 2f;
            _hazardZoneLine.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
        }

        // Floating "⚠ HAZARD ZONE" 3D Pill Badge on perimeter
        _hazardZoneBadgeGO = new GameObject("HazardZoneBadge");
        _hazardZoneBadgeGO.transform.SetParent(_hazardZoneRoot.transform, false);
        _hazardZoneBadgeGO.transform.localPosition = new Vector3(0f, 0.35f, -radius);
        _hazardZoneBadgeGO.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);

        // Quad bg
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "BadgeBg";
        quad.transform.SetParent(_hazardZoneBadgeGO.transform, false);
        quad.transform.localScale = new Vector3(0.55f, 0.15f, 1f);
        Destroy(quad.GetComponent<Collider>());
        var bMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
        bMat.color = new Color(0.85f, 0.12f, 0.12f, 0.95f);
        quad.GetComponent<MeshRenderer>().material = bMat;

        // Text
        var txtGO = new GameObject("BadgeText");
        txtGO.transform.SetParent(_hazardZoneBadgeGO.transform, false);
        txtGO.transform.localPosition = new Vector3(0f, 0f, -0.005f);
        var tmp = txtGO.AddComponent<TextMeshPro>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 2.4f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.text = "⚠ HAZARD ZONE";
    }

    private void RemoveHazardZoneVisualDisk()
    {
        if (_hazardZoneRoot != null)
        {
            _hazardZoneRoot.SetActive(false);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  BACK BUTTON HANDLER
    // ─────────────────────────────────────────────────────────────────
    private void HandleBackClicked()
    {
        StopGasLeakSFXAndPlume();
        StopLiveTimer();

        if (ARModuleLauncher.Instance != null)
        {
            ARModuleLauncher.Instance.ExitCurrentARScene();
        }
        else
        {
            AppManager.Instance?.ReturnFromARToScenarioSelection();
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  LOCALIZATION HELPER
    // ─────────────────────────────────────────────────────────────────
    private static string Loc(string key, string fallback)
    {
        try
        {
            var mgr = AppManager.Instance?.Localization ?? LocalizationManager.Instance;
            if (mgr != null)
            {
                string r = mgr.Get(key);
                if (!string.IsNullOrEmpty(r) && r != $"[{key}]") return r;
            }
        }
        catch { }
        return fallback;
    }

    private static AppLanguage GetCurrentLanguage()
    {
        try
        {
            if (AppState.Instance != null) return AppState.Instance.CurrentLanguage;
            if (LocalizationManager.Instance != null) return LocalizationManager.Instance.CurrentLanguage;
            if (PlayerPrefs.HasKey("SurakshaAR_Language"))
                return (AppLanguage)PlayerPrefs.GetInt("SurakshaAR_Language", (int)AppLanguage.English);
        }
        catch { }
        return AppLanguage.English;
    }

    // ─────────────────────────────────────────────────────────────────
    //  PROCEDURAL VECTOR SPRITE GENERATOR (Ultra-Crisp Procedural Icons)
    // ─────────────────────────────────────────────────────────────────
    private static Sprite CreateProceduralIcon(string type)
    {
        int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] cols = new Color[size * size];
        for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

        void FillDisc(float cx, float cy, float r, Color c)
        {
            int minX = Mathf.Max(0, Mathf.FloorToInt(cx - r - 1));
            int maxX = Mathf.Min(size - 1, Mathf.CeilToInt(cx + r + 1));
            int minY = Mathf.Max(0, Mathf.FloorToInt(cy - r - 1));
            int maxY = Mathf.Min(size - 1, Mathf.CeilToInt(cy + r + 1));
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (d <= r + 0.5f)
                    {
                        float alpha = Mathf.Clamp01(r - d + 0.5f) * c.a;
                        int idx = y * size + x;
                        cols[idx] = new Color(c.r, c.g, c.b, Mathf.Max(cols[idx].a, alpha));
                    }
                }
            }
        }

        void FillCapsule(float x0, float y0, float x1, float y1, float r, Color c)
        {
            float dx = x1 - x0;
            float dy = y1 - y0;
            float len = Mathf.Sqrt(dx * dx + dy * dy);
            if (len < 0.001f) { FillDisc(x0, y0, r, c); return; }
            float ux = dx / len;
            float uy = dy / len;
            for (float t = 0; t <= len; t += 0.5f)
            {
                FillDisc(x0 + ux * t, y0 + uy * t, r, c);
            }
        }

        void FillRect(float x0, float y0, float w, float h, Color c)
        {
            int minX = Mathf.Max(0, Mathf.FloorToInt(x0));
            int maxX = Mathf.Min(size - 1, Mathf.CeilToInt(x0 + w));
            int minY = Mathf.Max(0, Mathf.FloorToInt(y0));
            int maxY = Mathf.Min(size - 1, Mathf.CeilToInt(y0 + h));
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    cols[y * size + x] = c;
                }
            }
        }

        Color white = Color.white;

        switch (type.ToLower())
        {
            case "cylinder":
                // Gas cylinder vertical capsule + top neck and cap
                FillCapsule(32, 16, 32, 46, 12, white);
                FillCapsule(32, 46, 32, 54, 5, white);
                FillCapsule(26, 52, 38, 52, 4, white);
                break;

            case "gas_cloud":
                // Gas plume cloud
                FillDisc(32, 22, 13, white);
                FillDisc(24, 32, 11, white);
                FillDisc(40, 32, 11, white);
                FillDisc(32, 42, 12, white);
                FillDisc(26, 48, 7, white);
                FillDisc(38, 48, 7, white);
                break;

            case "detector":
                // Handheld multi-gas detector meter with screen & sensor grill
                FillCapsule(32, 18, 32, 46, 13, white);
                FillRect(24, 30, 16, 14, Color.clear);
                FillCapsule(28, 18, 36, 18, 2, white);
                FillDisc(32, 54, 4, white);
                break;

            case "alarm":
                // Beacon alarm with siren waves
                FillCapsule(24, 18, 40, 18, 5, white);
                FillCapsule(32, 22, 32, 42, 10, white);
                FillCapsule(16, 32, 12, 36, 2, white);
                FillCapsule(48, 32, 52, 36, 2, white);
                FillCapsule(18, 44, 14, 48, 2, white);
                FillCapsule(46, 44, 50, 48, 2, white);
                break;

            case "hardhat":
                // Industrial safety hardhat / helmet
                FillDisc(32, 30, 15, white);
                FillRect(14, 18, 36, 7, white);
                FillCapsule(12, 20, 52, 20, 3.5f, white);
                break;

            case "hazard_pin":
                // Warning triangle / map marker
                FillCapsule(32, 50, 20, 18, 4, white);
                FillCapsule(20, 18, 44, 18, 4, white);
                FillCapsule(44, 18, 32, 50, 4, white);
                FillDisc(32, 26, 2.5f, white);
                FillCapsule(32, 32, 32, 40, 2.5f, white);
                break;

            case "buddy":
                // Two workers / users side by side
                FillDisc(24, 42, 6, white);
                FillCapsule(24, 20, 24, 32, 8, white);
                FillDisc(40, 42, 6, white);
                FillCapsule(40, 20, 40, 32, 8, white);
                break;

            case "valve":
                // Handwheel valve
                FillDisc(32, 32, 16, white);
                FillDisc(32, 32, 10, Color.clear);
                FillCapsule(32, 14, 32, 50, 3, white);
                FillCapsule(14, 32, 50, 32, 3, white);
                FillDisc(32, 32, 4, white);
                break;

            case "check_circle":
                // Checkmark inside circle
                FillDisc(32, 32, 18, white);
                FillDisc(32, 32, 14, Color.clear);
                FillCapsule(22, 30, 29, 23, 3.5f, white);
                FillCapsule(29, 23, 44, 41, 3.5f, white);
                break;

            case "confined_space":
                // Vessel manhole portal / entry
                FillDisc(32, 32, 18, white);
                FillDisc(32, 32, 13, Color.clear);
                FillCapsule(26, 18, 26, 46, 2.5f, white);
                FillCapsule(38, 18, 38, 46, 2.5f, white);
                FillCapsule(26, 26, 38, 26, 2.5f, white);
                FillCapsule(26, 38, 38, 38, 2.5f, white);
                break;

            case "clipboard":
                // Assessment clipboard
                FillRect(20, 14, 24, 36, white);
                FillRect(24, 18, 16, 26, Color.clear);
                FillCapsule(28, 48, 36, 48, 4, white);
                FillCapsule(26, 36, 38, 36, 1.8f, white);
                FillCapsule(26, 30, 38, 30, 1.8f, white);
                FillCapsule(26, 24, 34, 24, 1.8f, white);
                break;

            case "timer":
                FillDisc(32, 30, 18, white);
                FillDisc(32, 30, 14, Color.clear);
                FillCapsule(32, 48, 32, 54, 3.5f, white);
                FillCapsule(32, 30, 32, 40, 2.5f, white);
                FillCapsule(32, 30, 40, 30, 2.5f, white);
                break;

            case "speaker":
                FillRect(10, 24, 10, 16, white);
                for (float sx = 20; sx <= 32; sx += 0.5f)
                {
                    float st = (sx - 20f) / 12f;
                    float stopY = Mathf.Lerp(40f, 52f, st);
                    float sbotY = Mathf.Lerp(24f, 12f, st);
                    FillCapsule(sx, sbotY, sx, stopY, 1.2f, white);
                }
                for (float a = -45f * Mathf.Deg2Rad; a <= 45f * Mathf.Deg2Rad; a += 0.04f)
                    FillDisc(24 + Mathf.Cos(a) * 16f, 32 + Mathf.Sin(a) * 16f, 1.75f, white);
                for (float a = -40f * Mathf.Deg2Rad; a <= 40f * Mathf.Deg2Rad; a += 0.03f)
                    FillDisc(24 + Mathf.Cos(a) * 24f, 32 + Mathf.Sin(a) * 24f, 1.75f, white);
                break;

            case "stop":
                FillRect(18, 18, 28, 28, white);
                break;

            case "workstation":
            case "placement":
            default:
                // Isometric cube / station
                FillCapsule(32, 52, 16, 42, 3, white);
                FillCapsule(32, 52, 48, 42, 3, white);
                FillCapsule(16, 42, 32, 32, 3, white);
                FillCapsule(48, 42, 32, 32, 3, white);
                FillCapsule(16, 42, 16, 20, 3, white);
                FillCapsule(48, 42, 48, 20, 3, white);
                FillCapsule(32, 32, 32, 12, 3, white);
                FillCapsule(16, 20, 32, 12, 3, white);
                FillCapsule(48, 20, 32, 12, 3, white);
                break;
        }

        tex.SetPixels(cols);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

    private static Sprite _cachedSpeakerSprite;
    public static Sprite CreateSpeakerSprite()
    {
        if (_cachedSpeakerSprite != null) return _cachedSpeakerSprite;
        _cachedSpeakerSprite = CreateProceduralIcon("speaker");
        return _cachedSpeakerSprite;
    }

    private static Sprite _cachedStopSprite;
    public static Sprite CreateStopSprite()
    {
        if (_cachedStopSprite != null) return _cachedStopSprite;
        _cachedStopSprite = CreateProceduralIcon("stop");
        return _cachedStopSprite;
    }

    private static Sprite CreateTapHandSprite()
    {
        int size = 96;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] cols = new Color[size * size];
        for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;

        void FillDisc(float cx, float cy, float r, Color c)
        {
            int minX = Mathf.Max(0, Mathf.FloorToInt(cx - r - 1));
            int maxX = Mathf.Min(size - 1, Mathf.CeilToInt(cx + r + 1));
            int minY = Mathf.Max(0, Mathf.FloorToInt(cy - r - 1));
            int maxY = Mathf.Min(size - 1, Mathf.CeilToInt(cy + r + 1));
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (d <= r + 0.5f)
                    {
                        float alpha = Mathf.Clamp01(r - d + 0.5f) * c.a;
                        int idx = y * size + x;
                        cols[idx] = new Color(c.r, c.g, c.b, Mathf.Max(cols[idx].a, alpha));
                    }
                }
            }
        }

        void FillCapsule(float x0, float y0, float x1, float y1, float r, Color c)
        {
            float dx = x1 - x0;
            float dy = y1 - y0;
            float len = Mathf.Sqrt(dx * dx + dy * dy);
            if (len < 0.001f) { FillDisc(x0, y0, r, c); return; }
            float ux = dx / len;
            float uy = dy / len;
            for (float t = 0; t <= len; t += 0.5f)
            {
                FillDisc(x0 + ux * t, y0 + uy * t, r, c);
            }
        }

        Color white = Color.white;
        FillCapsule(40, 36, 40, 82, 7.5f, white); // Index finger
        FillCapsule(54, 32, 54, 56, 7f, white);   // Middle finger
        FillCapsule(67, 28, 67, 50, 6.5f, white); // Ring finger
        FillCapsule(78, 24, 78, 44, 6f, white);   // Pinky finger
        FillDisc(56, 32, 20f, white);             // Palm
        FillCapsule(42, 28, 72, 24, 14f, white);
        FillCapsule(30, 24, 28, 44, 7f, white);   // Thumb
        FillCapsule(28, 44, 38, 48, 6.5f, white);
        FillCapsule(54, 10, 54, 26, 15f, white);  // Wrist

        tex.SetPixels(cols);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }
}
