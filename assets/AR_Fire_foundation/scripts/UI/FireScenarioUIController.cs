using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.Localization;
using SurakshaAR.UI;

/// <summary>
/// Pure Unity uGUI AR Fire Training HUD.
/// Implements the exact visual hierarchy, compactness, and screen composition from the
/// official Ministry of Mines / SurakshaAR reference image:
/// - Top Header: Back button, Module Title, Step X of 6, Stopwatch Timer, Score, and thin progress line.
/// - In-World Target Overlays: Dynamic world-to-screen indicators with floating instruction pills,
///   mint corner brackets [ ], hand tapping icons 👆, circular target rings, concentric crosshairs ⌖,
///   and checkmark badges ✓.
/// - Bottom Action Cards: Compact white rounded cards with red icon badges and chevrons,
///   two-tier spray progress box with live timer and green bar, and dark completion card.
/// - Responsive: Fully adapts across all Android portrait resolutions (320x568 to 1440x3200).
/// </summary>
[DisallowMultipleComponent]
[ExecuteAlways]
public class FireScenarioUIController : MonoBehaviour
{
    public enum FeedbackType
    {
        Correct,
        Wrong,
        Unsafe,
        Critical
    }

    public static FireScenarioUIController Instance { get; private set; }

    // ── Public Properties for Compatibility ───────────────────────────
    public bool HasCard { get; private set; }
    public bool HasCompletionPanel { get; set; }
    public bool IsAssessmentMode { get; private set; }

    public System.Action OnBackClicked;
    public System.Action OnResetClicked;

    // ── UI Hierarchy Elements ─────────────────────────────────────────
    private Canvas _canvas;
    private RectTransform _safeArea;

    public Canvas Canvas => _canvas;
    public RectTransform SafeArea => _safeArea;

    // Top Bar
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

    // Center Notification Pill & Reticle (Placement Mode)
    private RectTransform _placementPill;
    private TextMeshProUGUI _placementPillText;
    private RectTransform _placementReticle;

    // World Target Indicator Container & Elements
    private RectTransform _targetOverlayRoot;
    private RectTransform _targetIndicatorBox;
    private TextMeshProUGUI _targetFloatingPillText;
    private GameObject _targetFloatingPillGO;
    private GameObject _targetBracketsGO;
    private GameObject _targetHandIconGO;
    private GameObject _targetRingGO;
    private GameObject _targetCrosshairGO;
    private GameObject _targetCheckmarkGO;

    // Target Transforms in 3D Scene
    private Transform _fireTransform;
    private Transform _alarmTransform;
    private Transform _displayExtinguisherTransform;
    private Transform _actualExtinguisherTransform;
    private Transform _pinTransform;
    private Transform _handleTransform;
    private Transform _nozzleTransform;
    private Transform _activeTargetTransform;

    // Feedback Toast Banner
    private RectTransform _feedbackBanner;
    private Image _feedbackBg;
    private TextMeshProUGUI _feedbackText;
    private Coroutine _feedbackRoutine;

    // Single Authoritative Contextual Bottom Guidance Card (Steps 1 - 6)
    private RectTransform _guidanceCard;
    private Image _guidanceCardIconImg;
    private Image _guidanceCardIconBadge;
    private TextMeshProUGUI _guidanceCardTitle;
    private TextMeshProUGUI _guidanceCardBody;
    private Button _voiceBtn;
    private Image _voiceBtnBg;
    private Image _voiceBtnIcon;
    private TextMeshProUGUI _voiceBtnText;
    private bool _voiceActive = false;
    private Coroutine _voiceFeedbackRoutine;
    private UnityAction _currentActionCallback;

    // Integrated Step 6 Spray Progress Elements (Inside Single Guidance Card)
    private RectTransform _sprayProgressSection;
    private TextMeshProUGUI _sprayStatusText;
    private TextMeshProUGUI _sprayTimerText;
    private RectTransform _sprayProgressFill;

    // Start Guidance Card (Screen 0: Tap to Start AR Training)
    private RectTransform _startCard;
    private Button _btnStartTraining;
    private TextMeshProUGUI _startCardBtnText;

    // Placement Bottom Pill
    private RectTransform _placementBottomCard;
    private TextMeshProUGUI _placementStatusText;
    private Button _btnPlaceScenario;
    private UnityAction _placementCallback;

    // Completion Card (Screen 7: Fire Extinguished!)
    private RectTransform _completionCard;
    private TextMeshProUGUI _compTitle;
    private TextMeshProUGUI _compSubtitle;
    private TextMeshProUGUI _compTimeText;
    private TextMeshProUGUI _compScoreText;
    private Button _btnCompContinue;
    private UnityAction _homeCallback;
    private UnityAction _retryCallback;

    // Fullscreen Message Card (Legacy fallback)
    private RectTransform _messageCard;
    private TextMeshProUGUI _messageTitle;
    private TextMeshProUGUI _messageBody;
    private TextMeshProUGUI _messageFooter;

    // Tracking Lost Modal
    private RectTransform _trackingLostModal;
    private Button _btnTrackingRetry;
    private UnityAction _trackingRetryCallback;

    // Scenario Reset Modal
    private RectTransform _resetModal;
    private Button _btnResetConfirm;
    private Button _btnResetCancel;
    private UnityAction _resetConfirmCallback;

    // Transient Toast Banner
    private RectTransform _toastBox;
    private TextMeshProUGUI _toastTitle;
    private TextMeshProUGUI _toastSubtitle;
    private Coroutine _toastRoutine;

    // Active Step State
    private int _currentStepIndex = 1;
    private int _totalSteps = 6;
    private bool _isPlacementMode = false;

    private void Awake()
    {
        if (Application.isPlaying && Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        EnsureInitialized();
    }

    public void EnsureInitialized()
    {
        if (_canvas == null)
        {
            BuildUGUIHierarchy();
        }
    }

    public static void ResetInstance()
    {
        Instance = null;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        UnsubscribeFlowAndEvents();
    }

    // ─────────────────────────────────────────────────────────────────
    //  LOCALIZATION HELPER
    // ─────────────────────────────────────────────────────────────────
    /// <summary>
    /// Fetch a localized string. Safely returns key placeholder if manager is unavailable.
    /// Never returns empty string — caller always gets displayable text.
    /// </summary>
    private static string Loc(string key, string fallback = null)
    {
        try
        {
            var mgr = AppManager.Instance?.Localization;
            if (mgr != null)
            {
                string result = mgr.Get(key);
                if (!string.IsNullOrEmpty(result) && result != $"[{key}]")
                    return result;
            }
        }
        catch { }
        return !string.IsNullOrEmpty(fallback) ? fallback : key;
    }

    private static AppLanguage GetCurrentLanguage()
    {
        try
        {
            if (LocalizationManager.Instance != null)
                return LocalizationManager.Instance.CurrentLanguage;
            if (AppManager.Instance?.Localization != null)
                return AppManager.Instance.Localization.CurrentLanguage;
            if (PlayerPrefs.HasKey("SurakshaAR_Language"))
                return (AppLanguage)PlayerPrefs.GetInt("SurakshaAR_Language", (int)AppLanguage.English);
        }
        catch { }
        return AppLanguage.English;
    }

    private void OnEnable()
    {
        SubscribeFlowAndEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFlowAndEvents();
    }

    private void Start()
    {
        ResolveSceneTargets();
        SubscribeFlowAndEvents();

        // Check if FlowManager is already active in an operational stage
        if (FireScenarioFlowManager.Instance != null &&
            FireScenarioFlowManager.Instance.CurrentStage >= FireScenarioFlowManager.Stage.Step1_IdentifyHazard &&
            FireScenarioFlowManager.Instance.CurrentStage <= FireScenarioFlowManager.Stage.Step6_Extinguish)
        {
            HandleFlowStageChanged(FireScenarioFlowManager.Instance.CurrentStage);
        }
    }

    private bool _flowSubscribed = false;
    private void SubscribeFlowAndEvents()
    {
        if (_flowSubscribed) return;
        _flowSubscribed = true;

        if (FireScenarioFlowManager.Instance != null)
        {
            FireScenarioFlowManager.Instance.OnStageChanged -= HandleFlowStageChanged;
            FireScenarioFlowManager.Instance.OnStageChanged += HandleFlowStageChanged;
        }

        TrainingEventManager.OnScenarioPlaced       -= HandleEventScenarioPlaced;
        TrainingEventManager.OnScenarioPlaced       += HandleEventScenarioPlaced;
        TrainingEventManager.OnHazardIdentified     -= HandleEventHazardIdentified;
        TrainingEventManager.OnHazardIdentified     += HandleEventHazardIdentified;
        TrainingEventManager.OnAlarmActivated      -= HandleEventAlarmActivated;
        TrainingEventManager.OnAlarmActivated      += HandleEventAlarmActivated;
        TrainingEventManager.OnExtinguisherPickedUp -= HandleEventExtinguisherPickedUp;
        TrainingEventManager.OnExtinguisherPickedUp += HandleEventExtinguisherPickedUp;
        TrainingEventManager.OnPinRemoved          -= HandleEventPinRemoved;
        TrainingEventManager.OnPinRemoved          += HandleEventPinRemoved;
    }

    private void UnsubscribeFlowAndEvents()
    {
        if (!_flowSubscribed) return;
        _flowSubscribed = false;

        if (FireScenarioFlowManager.Instance != null)
        {
            FireScenarioFlowManager.Instance.OnStageChanged -= HandleFlowStageChanged;
        }

        TrainingEventManager.OnScenarioPlaced       -= HandleEventScenarioPlaced;
        TrainingEventManager.OnHazardIdentified     -= HandleEventHazardIdentified;
        TrainingEventManager.OnAlarmActivated      -= HandleEventAlarmActivated;
        TrainingEventManager.OnExtinguisherPickedUp -= HandleEventExtinguisherPickedUp;
        TrainingEventManager.OnPinRemoved          -= HandleEventPinRemoved;
    }

    private void HandleEventScenarioPlaced()       => RefreshStepGuidance(1);
    private void HandleEventHazardIdentified()     => RefreshStepGuidance(2);
    private void HandleEventAlarmActivated()      => RefreshStepGuidance(3);
    private void HandleEventExtinguisherPickedUp() => RefreshStepGuidance(4);
    private void HandleEventPinRemoved()          => RefreshStepGuidance(5);

    public void HandleFlowStageChanged(FireScenarioFlowManager.Stage stage)
    {
        Debug.Log($"[UI-STEP] UI received stage change: {stage}");
        switch (stage)
        {
            case FireScenarioFlowManager.Stage.Step1_IdentifyHazard:
                RefreshStepGuidance(1);
                break;
            case FireScenarioFlowManager.Stage.Step2_ActivateAlarm:
                RefreshStepGuidance(2);
                break;
            case FireScenarioFlowManager.Stage.Step3_SelectExtinguisher:
                RefreshStepGuidance(3);
                break;
            case FireScenarioFlowManager.Stage.Step4_RemovePin:
                RefreshStepGuidance(4);
                break;
            case FireScenarioFlowManager.Stage.Step5_AimBase:
                RefreshStepGuidance(5);
                break;
            case FireScenarioFlowManager.Stage.Step6_Extinguish:
                RefreshStepGuidance(6);
                break;
        }
    }

    public void RefreshStepGuidance(int step)
    {
        SetModuleInfo(Loc("module.fire.title", "Fire & Explosion Response"), step, 6);
        Debug.Log($"[UI-GUIDANCE] guidanceKey = fire.step{step}.guidance");
    }

    private void LateUpdate()
    {
        UpdateWorldTargetPosition();
    }

    // =================================================================
    //  UGUI HIERARCHY BUILDER
    // =================================================================
    private void BuildUGUIHierarchy()
    {
        // 0. Clean up any existing children to prevent duplicate UI layers
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            UIHelper.SafeDestroy(transform.GetChild(i).gameObject);
        }

        // 1. Canvas
        var canvasGO = new GameObject("AR_UGUI_Canvas");
        canvasGO.transform.SetParent(transform, false);

        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 2400);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f; // Width-first for portrait mobile

        canvasGO.AddComponent<GraphicRaycaster>();

        var canvasRT = canvasGO.GetComponent<RectTransform>();
        if (canvasRT != null) canvasRT.sizeDelta = new Vector2(1080, 2400);

        // 2. Safe Area Root
        var safeGO = new GameObject("SafeArea");
        safeGO.transform.SetParent(canvasGO.transform, false);
        _safeArea = safeGO.AddComponent<RectTransform>();
        UIHelper.Stretch(_safeArea, 0, 0, 0, 0);

        safeGO.AddComponent<SafeAreaDriver>();

        // 3. Build UI Components matching the reference image
        BuildTopBar(_safeArea);
        BuildPlacementNotification(_safeArea);
        BuildPlacementReticle(_safeArea);
        BuildWorldTargetOverlays(_safeArea);
        BuildFeedbackBanner(_safeArea);
        BuildGuidanceCard(_safeArea); // Single Authoritative Contextual Bottom Guidance Card
        BuildPlacementBottomCard(_safeArea);
        BuildStartCard(_safeArea);
        BuildCompletionCard(_safeArea);
        BuildTransientToastModal(_safeArea);
        BuildTrackingLostModal(_safeArea);
        BuildResetModal(_safeArea);
        BuildLegacyMessageCard(_safeArea);
    }

    private static Sprite CreateRingSprite(int size, int thickness, Color col)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] cols = new Color[size * size];
        float center = (size - 1) / 2f;
        float outerR = center;
        float innerR = center - thickness;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist <= outerR && dist >= innerR)
                {
                    float edgeOuter = Mathf.Clamp01(outerR - dist + 0.5f);
                    float edgeInner = Mathf.Clamp01(dist - innerR + 0.5f);
                    float alpha = Mathf.Min(edgeOuter, edgeInner) * col.a;
                    cols[y * size + x] = new Color(col.r, col.g, col.b, alpha);
                }
                else
                {
                    cols[y * size + x] = Color.clear;
                }
            }
        }
        tex.SetPixels(cols);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
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
        // Pointing index finger
        FillCapsule(40, 36, 40, 82, 7.5f, white);
        // Middle finger (curled)
        FillCapsule(54, 32, 54, 56, 7f, white);
        // Ring finger (curled)
        FillCapsule(67, 28, 67, 50, 6.5f, white);
        // Pinky finger (curled)
        FillCapsule(78, 24, 78, 44, 6f, white);
        // Palm body
        FillDisc(56, 32, 20f, white);
        FillCapsule(42, 28, 72, 24, 14f, white);
        // Thumb (folded across side)
        FillCapsule(30, 24, 28, 44, 7f, white);
        FillCapsule(28, 44, 38, 48, 6.5f, white);
        // Wrist
        FillCapsule(54, 10, 54, 26, 15f, white);

        tex.SetPixels(cols);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

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
            case "flame":
                FillDisc(32, 22, 14, white);
                FillDisc(32, 34, 10, white);
                FillDisc(32, 46, 5, white);
                FillDisc(24, 26, 8, white);
                FillDisc(40, 26, 8, white);
                FillDisc(20, 36, 4, white);
                FillDisc(44, 36, 4, white);
                break;

            case "alarm":
                // Alarm box with pull handle and top dome
                FillCapsule(22, 16, 42, 16, 4, white);
                FillRect(18, 16, 28, 28, white);
                FillCapsule(22, 44, 42, 44, 4, white);
                FillDisc(32, 48, 8, white); // top dome
                // Pull handle cutout (inner clear rect)
                FillRect(26, 24, 12, 10, Color.clear);
                FillCapsule(28, 28, 36, 28, 2, white);
                break;

            case "extinguisher":
                // Cylinder body
                FillCapsule(32, 20, 32, 44, 11, white);
                // Base foot
                FillRect(22, 10, 20, 4, white);
                // Top neck and handle
                FillCapsule(32, 46, 32, 54, 4, white);
                FillCapsule(24, 52, 40, 52, 3, white);
                // Hose curving down right
                FillCapsule(38, 50, 46, 42, 2.5f, white);
                FillCapsule(46, 42, 46, 26, 2.5f, white);
                FillCapsule(46, 26, 44, 20, 3.5f, white); // nozzle horn
                break;

            case "pin":
                // Circular ring on left
                FillDisc(22, 32, 12, white);
                FillDisc(22, 32, 7, Color.clear);
                // Pin shaft
                FillCapsule(28, 32, 48, 32, 4, white);
                // Pin head ball
                FillDisc(50, 32, 6.5f, white);
                break;

            case "timer":
                FillDisc(32, 30, 18, white);
                FillDisc(32, 30, 14, Color.clear);
                // Top knob
                FillCapsule(32, 48, 32, 54, 3.5f, white);
                // Hands
                FillCapsule(32, 30, 32, 40, 2.5f, white);
                FillCapsule(32, 30, 40, 30, 2.5f, white);
                break;

            case "checkmark":
                FillCapsule(16, 30, 28, 18, 5f, white);
                FillCapsule(28, 18, 50, 46, 5f, white);
                break;

            case "spray":
                // Angled nozzle handle
                FillCapsule(18, 20, 28, 30, 4.5f, white);
                FillCapsule(26, 28, 32, 34, 5.5f, white);
                // Spray droplet rays expanding forward
                FillCapsule(34, 38, 50, 54, 2.5f, white);
                FillCapsule(36, 34, 54, 44, 2.5f, white);
                FillCapsule(36, 30, 54, 30, 2.5f, white);
                FillCapsule(36, 26, 52, 18, 2.5f, white);
                FillCapsule(32, 22, 44, 12, 2.0f, white);
                break;

            case "speaker":
                // Base rectangle
                FillRect(10, 24, 10, 16, white);
                // Cone trapezoid
                for (float sx = 20; sx <= 32; sx += 0.5f)
                {
                    float st = (sx - 20f) / 12f;
                    float stopY = Mathf.Lerp(40f, 52f, st);
                    float sbotY = Mathf.Lerp(24f, 12f, st);
                    FillCapsule(sx, sbotY, sx, stopY, 1.2f, white);
                }
                // Sound wave arcs
                for (float a = -45f * Mathf.Deg2Rad; a <= 45f * Mathf.Deg2Rad; a += 0.04f)
                {
                    FillDisc(24 + Mathf.Cos(a) * 16f, 32 + Mathf.Sin(a) * 16f, 1.75f, white);
                }
                for (float a = -40f * Mathf.Deg2Rad; a <= 40f * Mathf.Deg2Rad; a += 0.03f)
                {
                    FillDisc(24 + Mathf.Cos(a) * 24f, 32 + Mathf.Sin(a) * 24f, 1.75f, white);
                }
                break;

            case "stop":
                // Clean solid square stop icon with rounded corners
                FillRect(18, 18, 28, 28, white);
                break;

            case "target":
            default:
                FillDisc(32, 32, 20, white);
                FillDisc(32, 32, 15, Color.clear);
                FillDisc(32, 32, 6, white);
                FillCapsule(32, 10, 32, 16, 2.5f, white);
                FillCapsule(32, 48, 32, 54, 2.5f, white);
                FillCapsule(10, 32, 16, 32, 2.5f, white);
                FillCapsule(48, 32, 54, 32, 2.5f, white);
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

    // ─────────────────────────────────────────────────────────────────
    //  1. TOP BAR (Matches Reference Image exactly)
    // ─────────────────────────────────────────────────────────────────
    private void BuildTopBar(Transform parent)
    {
        _topBar = UIHelper.MakeRect("TopBar", parent);
        _topBar.anchorMin = new Vector2(0, 1);
        _topBar.anchorMax = new Vector2(1, 1);
        _topBar.pivot = new Vector2(0.5f, 1);
        _topBar.sizeDelta = new Vector2(0, 164); // taller for mobile readability
        _topBar.anchoredPosition = new Vector2(0, -10);

        // A. Back Button — 72x72 minimum touch target
        var backBtn = UIHelper.MakeButton("btn-ar-back", _topBar, "‹", 44, new Color(0.06f, 0.10f, 0.18f, 0.75f), Color.white, 18);
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

        // B. Module Title & Step Subtitle Stack (Expanded width to prevent any truncation)
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

        // Module title: 38px — clean, crisp white, full visibility
        _moduleTitleText = UIHelper.MakeLabel("ModuleTitle", titleStack, "Fire & Explosion Response", 38, Color.white, bold: true, wrap: false);
        _moduleTitleText.overflowMode = TextOverflowModes.Ellipsis;
        UIHelper.SetLayout(_moduleTitleText.gameObject, preferredWidth: 550, minWidth: 420, preferredHeight: 50);

        // Step counter: 32px — safety green sub-label matching reference image
        _stepCounterText = UIHelper.MakeLabel("StepCounter", titleStack, "Step 1 of 6", 32, Hex("#22C55E"), bold: true, wrap: false);
        _stepCounterText.overflowMode = TextOverflowModes.Ellipsis;
        UIHelper.SetLayout(_stepCounterText.gameObject, preferredWidth: 550, minWidth: 420, preferredHeight: 40);

        // C. Right Cluster (Score + Stopwatch Timer)
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

        // Score Label: 34px
        _scoreText = UIHelper.MakeLabel("ScoreLbl", rightCluster, "Score: 0", 34, Hex("#CBD5E1"), TextAlignmentOptions.Right, bold: true, wrap: false);
        _scoreText.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(_scoreText.gameObject, preferredWidth: 130, minWidth: 100, preferredHeight: 48);

        _scoreDeltaText = UIHelper.MakeLabel("DeltaLbl", rightCluster, "+10", 34, Hex("#4ADE80"), bold: true, wrap: false);
        _scoreDeltaText.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(_scoreDeltaText.gameObject, preferredWidth: 60, minWidth: 50, preferredHeight: 48);
        _scoreDeltaText.gameObject.SetActive(false);

        // Timer Pill (Stopwatch icon + time) — 136x54
        _timerPill = UIHelper.MakeRect("TimerPill", rightCluster);
        _timerPill.sizeDelta = new Vector2(136, 54);
        UIHelper.SetLayout(_timerPill.gameObject, preferredWidth: 136, minWidth: 120, preferredHeight: 54);
        var timerImg = _timerPill.gameObject.AddComponent<Image>();
        timerImg.color = new Color(0.06f, 0.10f, 0.18f, 0.50f);
        timerImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(timerImg, 14);

        var timerRow = UIHelper.MakeHorizontal("Row", _timerPill, 6);
        UIHelper.Stretch(timerRow, 8, 8, 0, 0);

        // Timer icon: 26x26
        var timerIconGO = UIHelper.MakeRect("TimerIcon", timerRow);
        timerIconGO.sizeDelta = new Vector2(26, 26);
        UIHelper.SetLayout(timerIconGO.gameObject, preferredWidth: 26, minWidth: 26, preferredHeight: 26);
        var timerIconImg = timerIconGO.gameObject.AddComponent<Image>();
        timerIconImg.sprite = CreateProceduralIcon("timer");
        timerIconImg.color = Hex("#CBD5E1");
        timerIconImg.type = Image.Type.Simple;
        timerIconImg.preserveAspect = true;

        // Timer text: 36px — ARTimer semantic target
        _timerText = UIHelper.MakeLabel("TimerText", timerRow, "00:00", 36, Color.white, bold: true, wrap: false);
        _timerText.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(_timerText.gameObject, preferredWidth: 92, minWidth: 80, preferredHeight: 48);

        // D. Progress bar — slightly thicker (5px, was 4)
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
        _topProgressFill.anchorMax = new Vector2(0.167f, 1f);
        _topProgressFill.offsetMin = Vector2.zero;
        _topProgressFill.offsetMax = Vector2.zero;

        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = Hex("#22C55E"); // Safety Green
        fillImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(fillImg, 2);
    }

    // ─────────────────────────────────────────────────────────────────
    //  2. PLACEMENT NOTIFICATION PILL (Screen 1 in image)
    // ─────────────────────────────────────────────────────────────────
    private void BuildPlacementNotification(Transform parent)
    {
        _placementPill = UIHelper.MakeRect("PlacementNotification", parent);
        _placementPill.anchorMin = new Vector2(0.5f, 1f);
        _placementPill.anchorMax = new Vector2(0.5f, 1f);
        _placementPill.pivot = new Vector2(0.5f, 1f);
        _placementPill.sizeDelta = new Vector2(680, 78);
        _placementPill.anchoredPosition = new Vector2(0, -172);

        var pillImg = _placementPill.gameObject.AddComponent<Image>();
        pillImg.color = new Color(0.06f, 0.10f, 0.18f, 0.85f);
        pillImg.sprite = UIHelper.GetWhiteSprite();
        pillImg.raycastTarget = false;
        UIHelper.SetImageRoundedSprite(pillImg, 24);

        var outline = _placementPill.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.15f);
        outline.effectDistance = new Vector2(1, -1);

        // Placement hint text: 32px — easily readable at arm's length
        _placementPillText = UIHelper.MakeLabel("Text", _placementPill, "Move your phone to find a flat surface", 32, Color.white, TextAlignmentOptions.Center, bold: true, wrap: false);
        _placementPillText.raycastTarget = false;
        UIHelper.Stretch(_placementPillText.GetComponent<RectTransform>(), 16, 16, 0, 0);

        _placementPill.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  2B. PLACEMENT FLOOR RETICLE (Screen 1 Center in image)
    // ─────────────────────────────────────────────────────────────────
    private void BuildPlacementReticle(Transform parent)
    {
        _placementReticle = UIHelper.MakeRect("PlacementReticle", parent);
        _placementReticle.anchorMin = new Vector2(0.5f, 0.5f);
        _placementReticle.anchorMax = new Vector2(0.5f, 0.5f);
        _placementReticle.pivot = new Vector2(0.5f, 0.5f);
        _placementReticle.sizeDelta = new Vector2(160, 160);
        _placementReticle.anchoredPosition = new Vector2(0, -60);

        // Vector Ring (FullRect smooth circular ring)
        var ringImg = _placementReticle.gameObject.AddComponent<Image>();
        ringImg.sprite = CreateRingSprite(160, 5, Color.white);
        ringImg.color = new Color(1f, 1f, 1f, 0.90f);
        ringImg.type = Image.Type.Simple;
        ringImg.raycastTarget = false;

        // Vertical Crosshair Line (extends through ring)
        var vLine = UIHelper.MakeRect("VLine", _placementReticle);
        vLine.anchorMin = new Vector2(0.5f, 0.5f);
        vLine.anchorMax = new Vector2(0.5f, 0.5f);
        vLine.pivot = new Vector2(0.5f, 0.5f);
        vLine.sizeDelta = new Vector2(3f, 130);
        vLine.anchoredPosition = Vector2.zero;
        var vImg = vLine.gameObject.AddComponent<Image>();
        vImg.color = new Color(1f, 1f, 1f, 0.85f);
        vImg.raycastTarget = false;

        // Horizontal Crosshair Line
        var hLine = UIHelper.MakeRect("HLine", _placementReticle);
        hLine.anchorMin = new Vector2(0.5f, 0.5f);
        hLine.anchorMax = new Vector2(0.5f, 0.5f);
        hLine.pivot = new Vector2(0.5f, 0.5f);
        hLine.sizeDelta = new Vector2(50, 3f);
        hLine.anchoredPosition = Vector2.zero;
        var hImg = hLine.gameObject.AddComponent<Image>();
        hImg.color = new Color(1f, 1f, 1f, 0.85f);
        hImg.raycastTarget = false;

        // Center Dot (mint)
        var dot = UIHelper.MakeRect("Dot", _placementReticle);
        dot.anchorMin = new Vector2(0.5f, 0.5f);
        dot.anchorMax = new Vector2(0.5f, 0.5f);
        dot.pivot = new Vector2(0.5f, 0.5f);
        dot.sizeDelta = new Vector2(14, 14);
        dot.anchoredPosition = Vector2.zero;
        var dotImg = dot.gameObject.AddComponent<Image>();
        dotImg.color = Hex("#34D399"); // Mint dot
        dotImg.sprite = UIHelper.GetCircleSprite();
        dotImg.type = Image.Type.Simple;
        dotImg.raycastTarget = false;

        _placementReticle.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  3. IN-WORLD TARGET OVERLAYS (Matches Screens 2 - 7 in image)
    // ─────────────────────────────────────────────────────────────────
    private void BuildWorldTargetOverlays(Transform parent)
    {
        _targetOverlayRoot = UIHelper.MakeRect("TargetOverlayRoot", parent);
        UIHelper.Stretch(_targetOverlayRoot, 0, 0, 0, 0);

        _targetIndicatorBox = UIHelper.MakeRect("TargetBox", _targetOverlayRoot);
        _targetIndicatorBox.anchorMin = new Vector2(0.5f, 0.5f);
        _targetIndicatorBox.anchorMax = new Vector2(0.5f, 0.5f);
        _targetIndicatorBox.pivot = new Vector2(0.5f, 0.5f);
        _targetIndicatorBox.sizeDelta = new Vector2(280, 280); // was 260x260
        _targetIndicatorBox.anchoredPosition = new Vector2(0, 60);

        // Interactive button on target box for instant first-tap responsiveness
        var boxImg = _targetIndicatorBox.gameObject.AddComponent<Image>();
        boxImg.color = Color.clear;
        boxImg.raycastTarget = true;

        var boxBtn = _targetIndicatorBox.gameObject.AddComponent<Button>();
        boxBtn.transition = Selectable.Transition.None;
        boxBtn.onClick.AddListener(() =>
        {
            // Immediate authoritative step handling for 1st tap response
            var flow = FireScenarioFlowManager.Instance;
            if (_currentStepIndex == 4)
            {
                if (flow != null && flow.pinInteraction != null)
                {
                    flow.pinInteraction.RemovePin();
                    return;
                }
            }
            else if (_currentStepIndex == 1)
            {
                if (flow != null)
                {
                    flow.OnHazardIdentified();
                    return;
                }
            }
            else if (_currentStepIndex == 2)
            {
                if (flow != null && flow.alarmInteraction != null)
                {
                    flow.alarmInteraction.ActivateAlarm();
                    return;
                }
            }
            else if (_currentStepIndex == 3)
            {
                if (flow != null && flow.displayPickup != null)
                {
                    flow.displayPickup.Pickup();
                    return;
                }
            }
            else if (_currentStepIndex == 5)
            {
                if (flow != null)
                {
                    flow.OnAimConfirmed();
                    return;
                }
            }

            _currentActionCallback?.Invoke();
        });

        // A. Floating Instruction Pill — 280x52 (was 240x42), font 20px (was 16)
        _targetFloatingPillGO = new GameObject("FloatingPill");
        _targetFloatingPillGO.transform.SetParent(_targetIndicatorBox, false);
        var pillRT = _targetFloatingPillGO.AddComponent<RectTransform>();
        pillRT.anchorMin = new Vector2(0.5f, 1f);
        pillRT.anchorMax = new Vector2(0.5f, 1f);
        pillRT.pivot = new Vector2(0.5f, 0f);
        pillRT.sizeDelta = new Vector2(360, 68); // was 340x64
        pillRT.anchoredPosition = new Vector2(0, 16);

        var pillImg = _targetFloatingPillGO.AddComponent<Image>();
        pillImg.color = new Color(0.04f, 0.35f, 0.25f, 0.95f); // Deep emerald green
        pillImg.sprite = UIHelper.GetWhiteSprite();
        pillImg.raycastTarget = false;
        UIHelper.SetImageRoundedSprite(pillImg, 20);

        var pillOutline = _targetFloatingPillGO.AddComponent<Outline>();
        pillOutline.effectColor = new Color(0.20f, 0.83f, 0.60f, 0.65f); // Crisp mint/emerald outline
        pillOutline.effectDistance = new Vector2(1, -1);

        // Guidance pill text: 30px — readable in world space
        _targetFloatingPillText = UIHelper.MakeLabel("PillText", _targetFloatingPillGO.transform, "Look at the fire", 30, Color.white, TextAlignmentOptions.Center, bold: true, wrap: false);
        _targetFloatingPillText.raycastTarget = false;
        UIHelper.Stretch(_targetFloatingPillText.GetComponent<RectTransform>(), 14, 14, 0, 0);

        // B. Corner Brackets [ ] in safety green (Screens 2, 3, 4)
        _targetBracketsGO = new GameObject("CornerBrackets");
        _targetBracketsGO.transform.SetParent(_targetIndicatorBox, false);
        var bracketsRT = _targetBracketsGO.AddComponent<RectTransform>();
        UIHelper.Stretch(bracketsRT, 0, 0, 0, 0);

        Color mintColor = Hex("#34D399");
        CreateCornerBracket("TL", bracketsRT, new Vector2(0, 1), mintColor, horizontalRight: true, verticalDown: true);
        CreateCornerBracket("TR", bracketsRT, new Vector2(1, 1), mintColor, horizontalRight: false, verticalDown: true);
        CreateCornerBracket("BL", bracketsRT, new Vector2(0, 0), mintColor, horizontalRight: true, verticalDown: false);
        CreateCornerBracket("BR", bracketsRT, new Vector2(1, 0), mintColor, horizontalRight: false, verticalDown: false);

        // C. Hand Tapping Icon (Screens 3, 5) - Procedural vector solid pointing hand
        _targetHandIconGO = new GameObject("HandTapIcon");
        _targetHandIconGO.transform.SetParent(_targetIndicatorBox, false);
        var handRT = _targetHandIconGO.AddComponent<RectTransform>();
        handRT.anchorMin = new Vector2(1f, 0.25f);
        handRT.anchorMax = new Vector2(1f, 0.25f);
        handRT.pivot = new Vector2(0f, 0.5f);
        handRT.sizeDelta = new Vector2(88, 88);
        handRT.anchoredPosition = new Vector2(12, 0);

        var handImg = _targetHandIconGO.AddComponent<Image>();
        handImg.sprite = CreateTapHandSprite();
        handImg.color = Color.white;
        handImg.type = Image.Type.Simple;
        handImg.preserveAspect = true;
        handImg.raycastTarget = false;

        // D. Circular Target Ring (Screen 5: Safety Pin) - Procedural vector ring
        _targetRingGO = new GameObject("TargetRing");
        _targetRingGO.transform.SetParent(_targetIndicatorBox, false);
        var ringRT = _targetRingGO.AddComponent<RectTransform>();
        ringRT.anchorMin = new Vector2(0.5f, 0.5f);
        ringRT.anchorMax = new Vector2(0.5f, 0.5f);
        ringRT.pivot = new Vector2(0.5f, 0.5f);
        ringRT.sizeDelta = new Vector2(96, 96);
        ringRT.anchoredPosition = Vector2.zero;

        var pinRingImg = _targetRingGO.AddComponent<Image>();
        pinRingImg.sprite = CreateRingSprite(96, 6, mintColor);
        pinRingImg.color = Color.white;
        pinRingImg.type = Image.Type.Simple;
        pinRingImg.raycastTarget = false;

        // E. Concentric Crosshair Reticle ⌖ (Screen 6: Aim at Base)
        _targetCrosshairGO = new GameObject("CrosshairReticle");
        _targetCrosshairGO.transform.SetParent(_targetIndicatorBox, false);
        var crossRT = _targetCrosshairGO.AddComponent<RectTransform>();
        crossRT.anchorMin = new Vector2(0.5f, 0.5f);
        crossRT.anchorMax = new Vector2(0.5f, 0.5f);
        crossRT.pivot = new Vector2(0.5f, 0.5f);
        crossRT.sizeDelta = new Vector2(92, 92);
        crossRT.anchoredPosition = Vector2.zero;

        // Outer ring
        var outerImg = _targetCrosshairGO.AddComponent<Image>();
        outerImg.sprite = CreateRingSprite(92, 5, Color.white);
        outerImg.color = Color.white;
        outerImg.type = Image.Type.Simple;
        outerImg.raycastTarget = false;

        // Center dot
        var dot = UIHelper.MakeRect("CenterDot", crossRT);
        dot.anchorMin = new Vector2(0.5f, 0.5f);
        dot.anchorMax = new Vector2(0.5f, 0.5f);
        dot.sizeDelta = new Vector2(16, 16);
        dot.anchoredPosition = Vector2.zero;
        var dotImg = dot.gameObject.AddComponent<Image>();
        dotImg.color = Hex("#F59E0B"); // Safety Orange dot
        dotImg.sprite = UIHelper.GetCircleSprite();
        dotImg.type = Image.Type.Simple;
        dotImg.raycastTarget = false;

        // Crosshair ticks
        CreateCrosshairTick("T_Top", crossRT, new Vector2(0, 36), new Vector2(2, 14));
        CreateCrosshairTick("T_Bottom", crossRT, new Vector2(0, -36), new Vector2(2, 14));
        CreateCrosshairTick("T_Left", crossRT, new Vector2(-36, 0), new Vector2(14, 2));
        CreateCrosshairTick("T_Right", crossRT, new Vector2(36, 0), new Vector2(14, 2));

        // F. Completion Checkmark Badge ✓ (Screen 7)
        _targetCheckmarkGO = new GameObject("CheckmarkBadge");
        _targetCheckmarkGO.transform.SetParent(_targetIndicatorBox, false);
        var checkRT = _targetCheckmarkGO.AddComponent<RectTransform>();
        checkRT.anchorMin = new Vector2(0.5f, 0.5f);
        checkRT.anchorMax = new Vector2(0.5f, 0.5f);
        checkRT.pivot = new Vector2(0.5f, 0.5f);
        checkRT.sizeDelta = new Vector2(96, 96);
        checkRT.anchoredPosition = Vector2.zero;

        var checkBg = _targetCheckmarkGO.AddComponent<Image>();
        checkBg.color = Hex("#10B981"); // Green circle
        checkBg.sprite = UIHelper.GetCircleSprite();
        checkBg.type = Image.Type.Simple;
        checkBg.raycastTarget = false;

        var checkIconGO = UIHelper.MakeRect("CheckIcon", checkRT);
        UIHelper.Stretch(checkIconGO, 22, 22, 22, 22);
        var checkIconImg = checkIconGO.gameObject.AddComponent<Image>();
        checkIconImg.sprite = CreateProceduralIcon("checkmark");
        checkIconImg.color = Color.white;
        checkIconImg.type = Image.Type.Simple;
        checkIconImg.preserveAspect = true;
        checkIconImg.raycastTarget = false;

        _targetIndicatorBox.gameObject.SetActive(false);
    }

    private static void CreateCornerBracket(string name, Transform parent, Vector2 cornerAnchor, Color col, bool horizontalRight, bool verticalDown)
    {
        var cornerGO = new GameObject("Corner_" + name);
        cornerGO.transform.SetParent(parent, false);
        var rt = cornerGO.AddComponent<RectTransform>();
        rt.anchorMin = cornerAnchor;
        rt.anchorMax = cornerAnchor;
        rt.pivot = cornerAnchor;
        rt.sizeDelta = new Vector2(36, 36); // was 32x32 — slightly larger indicator

        // Horizontal line — 4px thick (was 3.5px)
        var hLine = UIHelper.MakeRect("H", rt);
        hLine.anchorMin = cornerAnchor;
        hLine.anchorMax = cornerAnchor;
        hLine.pivot = cornerAnchor;
        hLine.sizeDelta = new Vector2(36, 4f);
        hLine.anchoredPosition = Vector2.zero;
        var hImg = hLine.gameObject.AddComponent<Image>();
        hImg.color = col;
        hImg.raycastTarget = false;

        // Vertical line — 4px thick (was 3.5px)
        var vLine = UIHelper.MakeRect("V", rt);
        vLine.anchorMin = cornerAnchor;
        vLine.anchorMax = cornerAnchor;
        vLine.pivot = cornerAnchor;
        vLine.sizeDelta = new Vector2(4f, 36);
        vLine.anchoredPosition = Vector2.zero;
        var vImg = vLine.gameObject.AddComponent<Image>();
        vImg.color = col;
        vImg.raycastTarget = false;
    }

    private static void CreateCrosshairTick(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        var tick = UIHelper.MakeRect(name, parent);
        tick.anchorMin = new Vector2(0.5f, 0.5f);
        tick.anchorMax = new Vector2(0.5f, 0.5f);
        tick.pivot = new Vector2(0.5f, 0.5f);
        tick.sizeDelta = size;
        tick.anchoredPosition = pos;
        var img = tick.gameObject.AddComponent<Image>();
        img.color = Color.white;
        img.raycastTarget = false;
    }

    // ─────────────────────────────────────────────────────────────────
    //  3B. COMPACT GUIDANCE PANEL (above bottom card — NEW)
    // ─────────────────────────────────────────────────────────────────
    //  4. SINGLE AUTHORITATIVE BOTTOM GUIDANCE CARD (Steps 1 - 6)
    // ─────────────────────────────────────────────────────────────────
    private void BuildGuidanceCard(Transform parent)
    {
        _guidanceCard = UIHelper.MakeRect("GuidanceCard", parent);
        _guidanceCard.anchorMin = new Vector2(0.04f, 0f);
        _guidanceCard.anchorMax = new Vector2(0.96f, 0f);
        _guidanceCard.pivot     = new Vector2(0.5f, 0f);
        _guidanceCard.offsetMin = new Vector2(0, 36);
        _guidanceCard.offsetMax = new Vector2(0, 250);

        // Crisp White Card Background — matches official SurakshaAR reference design
        var cardImg = _guidanceCard.gameObject.AddComponent<Image>();
        cardImg.color  = Color.white;
        cardImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(cardImg, 24);

        // Soft subtle border
        var outline = _guidanceCard.gameObject.AddComponent<Outline>();
        outline.effectColor    = new Color(0.85f, 0.88f, 0.92f, 1f); // #E2E8F0 subtle slate border
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

        // ── 1. Header Row (Icon + Title + Listen Button) ─────────────
        var headerRow = UIHelper.MakeRect("HeaderRow", _guidanceCard);
        var headerHlg = headerRow.gameObject.AddComponent<HorizontalLayoutGroup>();
        headerHlg.spacing              = 14;
        headerHlg.childAlignment       = TextAnchor.MiddleLeft;
        headerHlg.childControlWidth    = true;
        headerHlg.childControlHeight   = true;
        headerHlg.childForceExpandWidth  = false;
        headerHlg.childForceExpandHeight = false;
        UIHelper.SetLayout(headerRow.gameObject, preferredHeight: 60, minHeight: 52);

        // A. Left: Step Icon Badge (56x56 circular/rounded badge)
        var iconBadgeGO = UIHelper.MakeRect("IconBadge", headerRow);
        iconBadgeGO.sizeDelta = new Vector2(56, 56);
        UIHelper.SetLayout(iconBadgeGO.gameObject, preferredWidth: 56, minWidth: 56, preferredHeight: 56);
        _guidanceCardIconBadge = iconBadgeGO.gameObject.AddComponent<Image>();
        _guidanceCardIconBadge.color  = Hex("#FEE2E2"); // Soft red tint default
        _guidanceCardIconBadge.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(_guidanceCardIconBadge, 28);

        var iconInner = UIHelper.MakeRect("InnerIcon", iconBadgeGO);
        UIHelper.Stretch(iconInner, 12, 12, 12, 12);
        _guidanceCardIconImg = iconInner.gameObject.AddComponent<Image>();
        _guidanceCardIconImg.sprite         = CreateProceduralIcon("flame");
        _guidanceCardIconImg.color          = Hex("#EF4444"); // Red accent
        _guidanceCardIconImg.type           = Image.Type.Simple;
        _guidanceCardIconImg.preserveAspect = true;

        // B. Center: Step Title (Dark Navy, bold, auto-wrapping)
        _guidanceCardTitle = UIHelper.MakeLabel("StepTitle", headerRow,
            "Identify Hazard", 36, Hex("#0F172A"), bold: true, wrap: true);
        _guidanceCardTitle.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(_guidanceCardTitle.gameObject, flexibleWidth: true, flexWidth: 1, minHeight: 48);

        // C. Right: Listen Audio Button (148x52) with real procedural speaker icon
        var voicePillGO = new GameObject("VoiceGuidanceBtn");
        voicePillGO.transform.SetParent(headerRow, false);
        var voicePillRT = voicePillGO.AddComponent<RectTransform>();
        voicePillRT.sizeDelta = new Vector2(148, 52);
        UIHelper.SetLayout(voicePillRT.gameObject, preferredWidth: 148, minWidth: 130, preferredHeight: 52);

        _voiceBtnBg = voicePillGO.AddComponent<Image>();
        _voiceBtnBg.color  = Hex("#2563EB"); // Royal blue
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
        var voiceColors = ColorBlock.defaultColorBlock;
        voiceColors.normalColor      = Color.white;
        voiceColors.highlightedColor = new Color(0.90f, 0.90f, 0.90f, 1f);
        voiceColors.pressedColor     = new Color(0.75f, 0.75f, 0.75f, 1f);
        _voiceBtn.colors = voiceColors;
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
            Loc("fire.voice.listenBtn", "Listen"),
            24, Color.white, TextAlignmentOptions.Center, bold: true, wrap: false);
        _voiceBtnText.raycastTarget = false;
        UIHelper.SetLayout(_voiceBtnText.gameObject, flexibleWidth: true, flexWidth: 1, minHeight: 28);

        // ── 2. Body Instruction Text (Dark Slate, auto-wrapping) ───────
        _guidanceCardBody = UIHelper.MakeLabel("StepBody", _guidanceCard,
            "Look around your surroundings to locate the highlighted electrical fire and tap it in AR.",
            28, Hex("#334155"), wrap: true);
        _guidanceCardBody.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(_guidanceCardBody.gameObject, flexibleWidth: true, flexWidth: 1, minHeight: 44);

        // ── 3. Step 6 Integrated Spray Progress Section ──────────────
        _sprayProgressSection = UIHelper.MakeRect("SprayProgressSection", _guidanceCard);
        UIHelper.SetLayout(_sprayProgressSection.gameObject, preferredHeight: 74, minHeight: 64);

        var sprayBoxImg = _sprayProgressSection.gameObject.AddComponent<Image>();
        sprayBoxImg.color  = Hex("#F8FAFC"); // Clean light background
        sprayBoxImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(sprayBoxImg, 12);

        var sprayBoxOutline = _sprayProgressSection.gameObject.AddComponent<Outline>();
        sprayBoxOutline.effectColor = new Color(0.85f, 0.88f, 0.92f, 1f);
        sprayBoxOutline.effectDistance = new Vector2(1, -1);

        var sprayVlg = _sprayProgressSection.gameObject.AddComponent<VerticalLayoutGroup>();
        sprayVlg.padding            = new RectOffset(16, 16, 10, 10);
        sprayVlg.spacing            = 8;
        sprayVlg.childControlWidth  = true;
        sprayVlg.childControlHeight = true;
        sprayVlg.childForceExpandWidth = true;

        var sprayStatRow = UIHelper.MakeHorizontal("SprayStatRow", _sprayProgressSection, 8);
        UIHelper.SetLayout(sprayStatRow.gameObject, preferredHeight: 32);

        _sprayStatusText = UIHelper.MakeLabel("SprayStatus", sprayStatRow,
            "Spraying...", 28, Hex("#16A34A"), bold: true, wrap: false);
        _sprayStatusText.overflowMode = TextOverflowModes.Ellipsis;
        UIHelper.SetLayout(_sprayStatusText.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 32);

        _sprayTimerText = UIHelper.MakeLabel("SprayTimer", sprayStatRow,
            "10.0 / 10.0 s", 28, Hex("#475569"), TextAlignmentOptions.Right, bold: true, wrap: false);
        _sprayTimerText.overflowMode = TextOverflowModes.Ellipsis;
        UIHelper.SetLayout(_sprayTimerText.gameObject, preferredWidth: 260, minWidth: 200, preferredHeight: 32);

        // Progress track & fill
        var progTrack = UIHelper.MakeRect("SprayTrack", _sprayProgressSection);
        UIHelper.SetLayout(progTrack.gameObject, preferredHeight: 8);
        var progTrackImg = progTrack.gameObject.AddComponent<Image>();
        progTrackImg.color  = Hex("#E2E8F0");
        progTrackImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(progTrackImg, 4);

        var progFillGO = new GameObject("SprayFill");
        progFillGO.transform.SetParent(progTrack, false);
        _sprayProgressFill = progFillGO.AddComponent<RectTransform>();
        _sprayProgressFill.anchorMin = Vector2.zero;
        _sprayProgressFill.anchorMax = new Vector2(0f, 1f);
        _sprayProgressFill.offsetMin = Vector2.zero;
        _sprayProgressFill.offsetMax = Vector2.zero;

        var progFillImg = progFillGO.AddComponent<Image>();
        progFillImg.color  = Hex("#22C55E"); // Safety Green
        progFillImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(progFillImg, 4);

        _sprayProgressSection.gameObject.SetActive(false); // only enabled on step 6

        _guidanceCard.gameObject.SetActive(false); // hidden until step 1
    }

    // ─────────────────────────────────────────────────────────────────
    //  6. PLACEMENT BOTTOM CARD (Screen 1 in image)
    // ─────────────────────────────────────────────────────────────────
    private void BuildPlacementBottomCard(Transform parent)
    {
        _placementBottomCard = UIHelper.MakeRect("PlacementBottomCard", parent);
        _placementBottomCard.anchorMin = new Vector2(0.05f, 0f);
        _placementBottomCard.anchorMax = new Vector2(0.95f, 0f);
        _placementBottomCard.pivot = new Vector2(0.5f, 0f);
        _placementBottomCard.offsetMin = new Vector2(0, 60);
        _placementBottomCard.offsetMax = new Vector2(0, 160); // height 100

        // Scanning pill
        var pillImg = _placementBottomCard.gameObject.AddComponent<Image>();
        pillImg.color = new Color(0.06f, 0.10f, 0.18f, 0.90f);
        pillImg.sprite = UIHelper.GetWhiteSprite();
        pillImg.raycastTarget = false;
        UIHelper.SetImageRoundedSprite(pillImg, 26);

        var outline = _placementBottomCard.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.18f);
        outline.effectDistance = new Vector2(1, -1);

        var hlg = _placementBottomCard.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(20, 20, 14, 14);
        hlg.spacing = 16;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        // Radar Scan Icon — 34x34
        var scanIconGO = UIHelper.MakeRect("ScanIcon", _placementBottomCard);
        scanIconGO.sizeDelta = new Vector2(34, 34);
        UIHelper.SetLayout(scanIconGO.gameObject, preferredWidth: 34, minWidth: 34, preferredHeight: 34);
        var scanImg = scanIconGO.gameObject.AddComponent<Image>();
        scanImg.sprite = CreateRingSprite(34, 5, Hex("#38BDF8"));
        scanImg.color = Color.white;
        scanImg.type = Image.Type.Simple;
        scanImg.raycastTarget = false;

        var scanDot = UIHelper.MakeRect("ScanDot", scanIconGO);
        scanDot.anchorMin = new Vector2(0.5f, 0.5f);
        scanDot.anchorMax = new Vector2(0.5f, 0.5f);
        scanDot.pivot = new Vector2(0.5f, 0.5f);
        scanDot.sizeDelta = new Vector2(10, 10);
        var scanDotImg = scanDot.gameObject.AddComponent<Image>();
        scanDotImg.sprite = UIHelper.GetCircleSprite();
        scanDotImg.color = Hex("#38BDF8");
        scanDotImg.type = Image.Type.Simple;
        scanDotImg.raycastTarget = false;

        // Status text: 32px, flexible
        _placementStatusText = UIHelper.MakeLabel("Status", _placementBottomCard, "Scanning for surface...", 32, Color.white, TextAlignmentOptions.Left, bold: true, wrap: false);
        _placementStatusText.overflowMode = TextOverflowModes.Ellipsis;
        _placementStatusText.raycastTarget = false;
        UIHelper.SetLayout(_placementStatusText.gameObject, flexibleWidth: true, flexWidth: 1, minWidth: 200, preferredHeight: 50);

        // Place button: 30px text, 70px height
        _btnPlaceScenario = UIHelper.MakeButton("btn-place", _placementBottomCard, "Place", 30, Hex("#22C55E"), Color.white, 18);
        _btnPlaceScenario.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 70);
        UIHelper.SetLayout(_btnPlaceScenario.gameObject, preferredWidth: 250, minWidth: 220, preferredHeight: 70);
        _btnPlaceScenario.onClick.AddListener(() => _placementCallback?.Invoke());
        _btnPlaceScenario.gameObject.SetActive(false);

        _placementBottomCard.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  6B. START GUIDANCE CARD (Screen 0: "Tap to Start AR Training")
    // ─────────────────────────────────────────────────────────────────
    private void BuildStartCard(Transform parent)
    {
        _startCard = UIHelper.MakeRect("StartGuidanceCard", parent);
        _startCard.anchorMin = new Vector2(0.06f, 0f);
        _startCard.anchorMax = new Vector2(0.94f, 0f);
        _startCard.pivot = new Vector2(0.5f, 0f);
        _startCard.offsetMin = new Vector2(0, 60);
        _startCard.offsetMax = new Vector2(0, 188); // height 128

        // Start CTA: 34px — prominent call to action comfortably above screen bottom
        _btnStartTraining = UIHelper.MakeButton("btn-start-training", _startCard, "TAP TO START AR TRAINING", 34,
            Hex("#1E3A8A"), Color.white, 24);
        UIHelper.Stretch(_btnStartTraining.GetComponent<RectTransform>(), 0, 0, 0, 0);

        var outline = _btnStartTraining.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.25f);
        outline.effectDistance = new Vector2(1, -1);

        _startCardBtnText = _btnStartTraining.GetComponentInChildren<TextMeshProUGUI>();

        _btnStartTraining.onClick.AddListener(() =>
        {
            _startCard.gameObject.SetActive(false);
            _currentActionCallback?.Invoke();
        });

        _startCard.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  7. COMPLETION CARD (Screen 7 in image: "Fire Extinguished!")
    // ─────────────────────────────────────────────────────────────────
    private void BuildCompletionCard(Transform parent)
    {
        _completionCard = UIHelper.MakeRect("CompletionCard", parent);
        _completionCard.anchorMin = new Vector2(0.5f, 0f);
        _completionCard.anchorMax = new Vector2(0.5f, 0f);
        _completionCard.pivot = new Vector2(0.5f, 0f);
        _completionCard.sizeDelta = new Vector2(960, 480);
        _completionCard.anchoredPosition = new Vector2(0, 52);

        var img = _completionCard.gameObject.AddComponent<Image>();
        img.color = new Color(0.06f, 0.10f, 0.18f, 0.95f);
        img.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(img, 30);

        var outline = _completionCard.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.15f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        var vlg = _completionCard.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(36, 36, 34, 34);
        vlg.spacing = 18;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        // Completion title: 50px — clearly distinguishes completion state, wraps for localized titles
        _compTitle = UIHelper.MakeLabel("Title", _completionCard, "Fire Extinguished!", 50, Color.white, TextAlignmentOptions.Center, bold: true, wrap: true);
        _compTitle.overflowMode = TextOverflowModes.Overflow;
        _compTitle.rectTransform.sizeDelta = new Vector2(880, 68);
        UIHelper.SetLayout(_compTitle.gameObject, preferredWidth: 880, minWidth: 880, preferredHeight: 68);

        // Completion subtitle: 34px
        _compSubtitle = UIHelper.MakeLabel("Subtitle", _completionCard, "Well Done!", 34, Hex("#4ADE80"), TextAlignmentOptions.Center, bold: true, wrap: true);
        _compSubtitle.overflowMode = TextOverflowModes.Overflow;
        _compSubtitle.rectTransform.sizeDelta = new Vector2(880, 50);
        UIHelper.SetLayout(_compSubtitle.gameObject, preferredWidth: 880, minWidth: 880, preferredHeight: 50);

        // Stats Row (Time + Score)
        var statRow = UIHelper.MakeHorizontal("StatRow", _completionCard, 28);
        statRow.sizeDelta = new Vector2(620, 50);
        UIHelper.SetLayout(statRow.gameObject, preferredWidth: 620, minWidth: 500, preferredHeight: 50);
        var statHlg = statRow.GetComponent<HorizontalLayoutGroup>();
        if (statHlg != null)
        {
            statHlg.childControlWidth = false;
            statHlg.childControlHeight = false;
            statHlg.childAlignment = TextAnchor.MiddleCenter;
        }

        // Stat labels: 28px
        var timeLbl = UIHelper.MakeLabel("TimeLbl", statRow, "Time", 28, Hex("#94A3B8"), TextAlignmentOptions.Right, wrap: false);
        timeLbl.rectTransform.sizeDelta = new Vector2(90, 44);
        UIHelper.SetLayout(timeLbl.gameObject, preferredWidth: 90, minWidth: 72, preferredHeight: 44);

        // Stat values: 32px
        _compTimeText = UIHelper.MakeLabel("TimeVal", statRow, "05:42", 32, Color.white, bold: true, wrap: false);
        _compTimeText.rectTransform.sizeDelta = new Vector2(120, 44);
        UIHelper.SetLayout(_compTimeText.gameObject, preferredWidth: 120, minWidth: 100, preferredHeight: 44);

        var scoreLbl = UIHelper.MakeLabel("ScoreLbl", statRow, "Score", 28, Hex("#94A3B8"), TextAlignmentOptions.Right, wrap: false);
        scoreLbl.rectTransform.sizeDelta = new Vector2(90, 44);
        UIHelper.SetLayout(scoreLbl.gameObject, preferredWidth: 90, minWidth: 72, preferredHeight: 44);

        _compScoreText = UIHelper.MakeLabel("ScoreVal", statRow, "100", 32, Color.white, bold: true, wrap: false);
        _compScoreText.rectTransform.sizeDelta = new Vector2(100, 44);
        UIHelper.SetLayout(_compScoreText.gameObject, preferredWidth: 100, minWidth: 90, preferredHeight: 44);

        // Primary Action CTA Button — 34px text, 84px height
        _btnCompContinue = UIHelper.MakeButton("btn-assessment", _completionCard, "Continue to Assessment →", 34,
            Hex("#1E3A8A"), Color.white, 18);
        _btnCompContinue.GetComponent<RectTransform>().sizeDelta = new Vector2(880, 84);
        UIHelper.SetLayout(_btnCompContinue.gameObject, preferredWidth: 880, minWidth: 880, preferredHeight: 84);
        _btnCompContinue.onClick.AddListener(() => _homeCallback?.Invoke());

        _completionCard.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  8. FEEDBACK TOAST BANNER
    // ─────────────────────────────────────────────────────────────────
    private void BuildFeedbackBanner(Transform parent)
    {
        _feedbackBanner = UIHelper.MakeRect("FeedbackBanner", parent);
        _feedbackBanner.anchorMin = new Vector2(0.06f, 1f);
        _feedbackBanner.anchorMax = new Vector2(0.94f, 1f);
        _feedbackBanner.pivot = new Vector2(0.5f, 1f);
        _feedbackBanner.sizeDelta = new Vector2(0, 92); // was 80 — taller warning banner
        _feedbackBanner.anchoredPosition = new Vector2(0, -170); // offset increased for taller top bar

        _feedbackBg = _feedbackBanner.gameObject.AddComponent<Image>();
        _feedbackBg.color = UIColors.SafetyGreen;
        _feedbackBg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(_feedbackBg, 20);

        // Feedback/warning text: 36px — ARWarning semantic target, critical readability
        _feedbackText = UIHelper.MakeLabel("FeedbackText", _feedbackBanner, "✓  Hazard Identified  +10", 36, Color.white, TextAlignmentOptions.Center, bold: true);
        UIHelper.Stretch(_feedbackText.GetComponent<RectTransform>(), 16, 16, 0, 0);

        _feedbackBanner.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  9. TRANSIENT TOAST MODAL (Auto-dismissing step transitions)
    // ─────────────────────────────────────────────────────────────────
    private void BuildTransientToastModal(Transform parent)
    {
        _toastBox = UIHelper.MakeRect("TransientToastModal", parent);
        _toastBox.anchorMin = new Vector2(0.08f, 0.62f);
        _toastBox.anchorMax = new Vector2(0.92f, 0.62f);
        _toastBox.pivot = new Vector2(0.5f, 0.5f);
        _toastBox.sizeDelta = new Vector2(0, 172); // was 158

        var bg = _toastBox.gameObject.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.10f, 0.18f, 0.92f);
        bg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(bg, 24);

        var outline = _toastBox.gameObject.AddComponent<Outline>();
        outline.effectColor = Hex("#34D399");
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        var vlg = _toastBox.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(24, 24, 24, 24);
        vlg.spacing = 8;
        vlg.childAlignment = TextAnchor.MiddleCenter;

        // Toast step title: 36px
        _toastTitle = UIHelper.MakeLabel("ToastTitle", _toastBox, "Hazard Identified!", 36, Color.white, TextAlignmentOptions.Center, bold: true);
        UIHelper.SetLayout(_toastTitle.gameObject, preferredHeight: 48);

        // Toast subtitle: 30px
        _toastSubtitle = UIHelper.MakeLabel("ToastSub", _toastBox, "Now activate the fire alarm", 30, Hex("#A7F3D0"), TextAlignmentOptions.Center);
        UIHelper.SetLayout(_toastSubtitle.gameObject, preferredHeight: 42);

        _toastBox.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  10. TRACKING LOST, RESET, AND LEGACY MESSAGE MODALS
    // ─────────────────────────────────────────────────────────────────
    private void BuildTrackingLostModal(Transform parent)
    {
        _trackingLostModal = UIHelper.MakeRect("TrackingLostModal", parent);
        UIHelper.Stretch(_trackingLostModal, 0, 0, 0, 0);

        var blocker = _trackingLostModal.gameObject.AddComponent<Image>();
        blocker.color = new Color(0, 0, 0, 0.70f);

        var box = UIHelper.MakeRect("Box", _trackingLostModal);
        box.anchorMin = new Vector2(0.1f, 0.38f);
        box.anchorMax = new Vector2(0.9f, 0.62f);
        box.offsetMin = Vector2.zero;
        box.offsetMax = Vector2.zero;

        var boxImg = box.gameObject.AddComponent<Image>();
        boxImg.color = new Color(0.06f, 0.10f, 0.18f, 0.95f);
        boxImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(boxImg, 24);

        var vlg = box.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(28, 28, 28, 28);
        vlg.spacing = 16;
        vlg.childAlignment = TextAnchor.MiddleCenter;

        // Tracking lost title: 40px
        var title = UIHelper.MakeLabel("Title", box, "⚠️ AR Tracking Paused", 40, Hex("#FDE68A"), TextAlignmentOptions.Center, bold: true);
        UIHelper.SetLayout(title.gameObject, preferredHeight: 52);

        // Tracking lost desc: 32px
        var desc = UIHelper.MakeLabel("Desc", box, "Move phone slowly toward a well-lit textured surface.", 32, Color.white, TextAlignmentOptions.Center, wrap: true);
        UIHelper.SetLayout(desc.gameObject, preferredHeight: 80);

        // Resume button: 32px text, 74px height
        _btnTrackingRetry = UIHelper.MakeButton("btn-resume", box, "Resume Training", 32, Hex("#22C55E"), Color.white, 18);
        UIHelper.SetLayout(_btnTrackingRetry.gameObject, preferredHeight: 74);
        _btnTrackingRetry.onClick.AddListener(() =>
        {
            _trackingLostModal.gameObject.SetActive(false);
            _trackingRetryCallback?.Invoke();
        });

        _trackingLostModal.gameObject.SetActive(false);
    }

    private void BuildResetModal(Transform parent)
    {
        _resetModal = UIHelper.MakeRect("ResetModal", parent);
        UIHelper.Stretch(_resetModal, 0, 0, 0, 0);

        var blocker = _resetModal.gameObject.AddComponent<Image>();
        blocker.color = new Color(0, 0, 0, 0.70f);

        var box = UIHelper.MakeRect("Box", _resetModal);
        box.anchorMin = new Vector2(0.12f, 0.40f);
        box.anchorMax = new Vector2(0.88f, 0.60f);
        box.offsetMin = Vector2.zero;
        box.offsetMax = Vector2.zero;

        var boxImg = box.gameObject.AddComponent<Image>();
        boxImg.color = new Color(0.06f, 0.10f, 0.18f, 0.95f);
        boxImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(boxImg, 24);

        var vlg = box.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(24, 24, 24, 24);
        vlg.spacing = 16;
        vlg.childAlignment = TextAnchor.MiddleCenter;

        // Reset modal title: 34px
        var title = UIHelper.MakeLabel("Title", box, "Restart Scenario?", 34, Color.white, TextAlignmentOptions.Center, bold: true);
        UIHelper.SetLayout(title.gameObject, preferredHeight: 46);

        // Reset modal desc: 26px
        var desc = UIHelper.MakeLabel("Desc", box, "Your scenario placement and actions will reset to step 1.", 26, Hex("#94A3B8"), TextAlignmentOptions.Center, wrap: true);
        UIHelper.SetLayout(desc.gameObject, preferredHeight: 66);

        // Button row: 70px height
        var bRow = UIHelper.MakeHorizontal("BtnRow", box, 14);
        UIHelper.SetLayout(bRow.gameObject, preferredHeight: 70);

        // Modal buttons: 28px
        _btnResetCancel = UIHelper.MakeButton("btn-cancel", bRow, "Cancel", 28, new Color(1, 1, 1, 0.15f), Color.white, 16);
        UIHelper.SetLayout(_btnResetCancel.gameObject, flexibleWidth: true, flexWidth: 1);
        _btnResetCancel.onClick.AddListener(() => _resetModal.gameObject.SetActive(false));

        _btnResetConfirm = UIHelper.MakeButton("btn-restart", bRow, "Restart", 28, Hex("#EF4444"), Color.white, 16);
        UIHelper.SetLayout(_btnResetConfirm.gameObject, flexibleWidth: true, flexWidth: 1);
        _btnResetConfirm.onClick.AddListener(() =>
        {
            _resetModal.gameObject.SetActive(false);
            _resetConfirmCallback?.Invoke();
        });

        _resetModal.gameObject.SetActive(false);
    }

    private void BuildLegacyMessageCard(Transform parent)
    {
        _messageCard = UIHelper.MakeRect("MessageCard", parent);
        _messageCard.anchorMin = new Vector2(0.08f, 0.25f);
        _messageCard.anchorMax = new Vector2(0.92f, 0.75f);
        _messageCard.offsetMin = Vector2.zero;
        _messageCard.offsetMax = Vector2.zero;

        var img = _messageCard.gameObject.AddComponent<Image>();
        img.color = new Color(0.06f, 0.10f, 0.18f, 0.95f);
        img.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(img, 26);

        var vlg = _messageCard.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(32, 32, 36, 36);
        vlg.spacing = 18;
        vlg.childAlignment = TextAnchor.MiddleCenter;

        // Message title: 46px
        _messageTitle = UIHelper.MakeLabel("Title", _messageCard, "SurakshaAR", 46, Color.white, TextAlignmentOptions.Center, bold: true);
        UIHelper.SetLayout(_messageTitle.gameObject, preferredHeight: 60);

        // Message body: 34px
        _messageBody = UIHelper.MakeLabel("Body", _messageCard, "AR Fire Safety Module", 34, Hex("#CBD5E1"), TextAlignmentOptions.Center, wrap: true);
        UIHelper.SetLayout(_messageBody.gameObject, flexibleHeight: true, flexHeight: 1);

        // Message footer: 32px
        _messageFooter = UIHelper.MakeLabel("Footer", _messageCard, "Tap anywhere to continue", 32, Hex("#4ADE80"), TextAlignmentOptions.Center, bold: true);
        UIHelper.SetLayout(_messageFooter.gameObject, preferredHeight: 48);

        _messageCard.gameObject.SetActive(false);
    }

    // =================================================================
    //  WORLD-TO-SCREEN TARGET TRACKING (3D AR Guidance)
    // =================================================================
    private void ResolveSceneTargets()
    {
        var flow = FireScenarioFlowManager.Instance;

        // 1. Fire Transform
        if (_fireTransform == null || !_fireTransform.gameObject.activeInHierarchy)
        {
            if (flow != null && flow.fire != null) _fireTransform = flow.fire.transform;
            if (_fireTransform == null || !_fireTransform.gameObject.activeInHierarchy)
            {
                var fireExt = FindAnyObjectByType<FireExtinguishable>(FindObjectsInactive.Include);
                if (fireExt != null && fireExt.gameObject.activeInHierarchy)
                {
                    _fireTransform = fireExt.transform;
                }
                else
                {
                    var go = GameObject.Find("VFX_Fire_01_Small") ?? GameObject.Find("Flames") ?? GameObject.Find("Hazard") ?? GameObject.Find("Electric Box");
                    if (go != null) _fireTransform = go.transform;
                }
            }
        }

        // 2. Alarm Transform
        if (_alarmTransform == null || !_alarmTransform.gameObject.activeInHierarchy)
        {
            if (flow != null && flow.alarmInteraction != null) _alarmTransform = flow.alarmInteraction.transform;
            if (_alarmTransform == null || !_alarmTransform.gameObject.activeInHierarchy)
            {
                var alarm = FindAnyObjectByType<AlarmInteraction>(FindObjectsInactive.Include);
                if (alarm != null)
                    _alarmTransform = alarm.transform;
                else
                {
                    var go = GameObject.Find("FireAlarm") ?? GameObject.Find("EUfFireAlarm") ?? GameObject.Find("Alarm");
                    if (go != null) _alarmTransform = go.transform;
                }
            }
        }

        // 3. Display Extinguisher Transform (for Step 3 selection ONLY)
        if (_displayExtinguisherTransform == null || !_displayExtinguisherTransform.gameObject.activeInHierarchy)
        {
            if (flow != null && flow.displayPickup != null) _displayExtinguisherTransform = flow.displayPickup.transform;
            if (_displayExtinguisherTransform == null || !_displayExtinguisherTransform.gameObject.activeInHierarchy)
            {
                var dispComp = FindAnyObjectByType<ExtinguisherDisplayPickup>(FindObjectsInactive.Include);
                if (dispComp != null)
                    _displayExtinguisherTransform = dispComp.transform;
                else
                {
                    var dispGO = GameObject.Find("FireExt_display") ?? GameObject.Find("ExtinguisherDisplay");
                    if (dispGO != null) _displayExtinguisherTransform = dispGO.transform;
                }
            }
        }

        // 4. Actual Picked-Up Extinguisher (for Steps 4, 5, 6)
        if (_actualExtinguisherTransform == null || !_actualExtinguisherTransform.gameObject.activeInHierarchy)
        {
            if (flow != null && flow.originalPickup != null) _actualExtinguisherTransform = flow.originalPickup.transform;
            if (_actualExtinguisherTransform == null)
            {
                var pickupComp = FindAnyObjectByType<ExtinguisherPickup>(FindObjectsInactive.Include);
                if (pickupComp != null)
                    _actualExtinguisherTransform = pickupComp.transform;
                else
                {
                    var go = GameObject.Find("FireExt");
                    if (go != null) _actualExtinguisherTransform = go.transform;
                }
            }
        }

        // 5. Actual Safety Pin on Picked-Up Extinguisher (Step 4)
        if (flow != null && flow.pinInteraction != null)
        {
            _pinTransform = flow.pinInteraction.transform;
        }
        if (_pinTransform == null && _actualExtinguisherTransform != null)
        {
            var pin = _actualExtinguisherTransform.GetComponentInChildren<FirePinInteraction>(true);
            if (pin != null) _pinTransform = pin.transform;
            else
            {
                var p = _actualExtinguisherTransform.Find("Pin") ?? _actualExtinguisherTransform.Find("SafetyPin") ?? _actualExtinguisherTransform.Find("pin");
                if (p != null) _pinTransform = p;
            }
        }
        if (_pinTransform == null)
        {
            var pinComp = FindAnyObjectByType<FirePinInteraction>(FindObjectsInactive.Include);
            if (pinComp != null) _pinTransform = pinComp.transform;
        }

        // 6. Actual Operating Lever / Handle on Picked-Up Extinguisher (Step 5 & 6)
        if (flow != null && flow.gripInteraction != null)
        {
            _handleTransform = flow.gripInteraction.transform;
        }
        if (_handleTransform == null && _actualExtinguisherTransform != null)
        {
            var grip = _actualExtinguisherTransform.GetComponentInChildren<ExtinguisherGripInteraction>(true);
            if (grip != null) _handleTransform = grip.transform;
            else
            {
                var h = _actualExtinguisherTransform.Find("Grip") ?? _actualExtinguisherTransform.Find("Lever") ?? _actualExtinguisherTransform.Find("Handle");
                if (h != null) _handleTransform = h;
            }
        }
        if (_handleTransform == null)
        {
            var gripComp = FindAnyObjectByType<ExtinguisherGripInteraction>(FindObjectsInactive.Include);
            if (gripComp != null) _handleTransform = gripComp.transform;
        }

        // 7. Actual Nozzle on Picked-Up Extinguisher (Step 5 & 6)
        if (_actualExtinguisherTransform != null)
        {
            var sp = _actualExtinguisherTransform.Find("spraypoint") ?? _actualExtinguisherTransform.Find("SprayPoint");
            if (sp != null) _nozzleTransform = sp;
            else
            {
                var dryPowder = _actualExtinguisherTransform.GetComponentInChildren<DryPowderSpray>(true);
                if (dryPowder != null) _nozzleTransform = dryPowder.GetSprayPoint();
            }
        }
    }

    private Vector3 GetTargetWorldPosition(Transform target, int step)
    {
        if (target == null && step != 1) return Vector3.zero;

        // Step 1: Fire Hazard (Precise flame world position)
        if (step == 1)
        {
            var flow = FireScenarioFlowManager.Instance;
            if (flow != null && flow.fire != null)
            {
                return flow.fire.FireWorldPosition;
            }
            if (target != null)
            {
                var col = target.GetComponent<Collider>();
                if (col != null) return col.bounds.center + Vector3.up * 0.15f;
                return target.position + Vector3.up * 0.25f;
            }
            return Vector3.zero;
        }

        // Step 4: Safety Pin (Use pull-ring renderer center or BoxCollider center)
        if (step == 4)
        {
            Transform pullRing = target.Find("SafetyPullRing");
            if (pullRing != null && pullRing.gameObject.activeInHierarchy)
            {
                var r = pullRing.GetComponent<Renderer>();
                if (r != null) return r.bounds.center;
                return pullRing.position;
            }

            var col = target.GetComponent<Collider>();
            if (col != null) return col.bounds.center;

            var rend = target.GetComponentInChildren<Renderer>();
            if (rend != null) return rend.bounds.center;

            return target.position;
        }

        // Step 5: Operating Handle / Aim at Base of Fire
        if (step == 5)
        {
            var flow = FireScenarioFlowManager.Instance;
            if (flow != null && flow.fire != null)
            {
                return flow.fire.FireWorldPosition;
            }
            if (_fireTransform != null)
            {
                return _fireTransform.position;
            }
            var col = target.GetComponent<Collider>();
            if (col != null) return col.bounds.center;

            var rend = target.GetComponentInChildren<Renderer>();
            if (rend != null) return rend.bounds.center;

            return target.position;
        }

        // Step 2: Fire Alarm
        if (step == 2)
        {
            var col = target.GetComponent<Collider>();
            if (col != null) return col.bounds.center;
            return target.position;
        }

        // Step 3: Extinguisher Display Prop
        if (step == 3)
        {
            var col = target.GetComponent<Collider>();
            if (col != null) return col.bounds.center;
            return target.position + Vector3.up * 0.35f;
        }

        var generalCol = target.GetComponent<Collider>();
        if (generalCol != null) return generalCol.bounds.center;

        return target.position;
    }

    private void UpdateWorldTargetPosition()
    {
        if (_targetIndicatorBox == null || _safeArea == null) return;

        // Dynamically ensure targets are resolved for the active step
        if ((_currentStepIndex == 1 && (_fireTransform == null || !_fireTransform.gameObject.activeInHierarchy)) ||
            (_currentStepIndex == 2 && (_alarmTransform == null || !_alarmTransform.gameObject.activeInHierarchy)) ||
            (_currentStepIndex == 3 && (_displayExtinguisherTransform == null || !_displayExtinguisherTransform.gameObject.activeInHierarchy)) ||
            (_currentStepIndex >= 4 && (_pinTransform == null || _handleTransform == null)))
        {
            ResolveSceneTargets();
        }

        // Resolve active target based on current step
        switch (_currentStepIndex)
        {
            case 1:
                if (_fireTransform == null)
                {
                    var flow = FireScenarioFlowManager.Instance;
                    if (flow != null && flow.fire != null) _fireTransform = flow.fire.transform;
                }
                _activeTargetTransform = _fireTransform;
                break;
            case 2:
                _activeTargetTransform = _alarmTransform;
                break;
            case 3:
                // Step 3: Points to DISPLAY extinguisher on the floor
                _activeTargetTransform = _displayExtinguisherTransform != null ? _displayExtinguisherTransform : _actualExtinguisherTransform;
                break;
            case 4:
                // Step 4: Points strictly to actual pin on PICKED-UP extinguisher
                _activeTargetTransform = _pinTransform;
                break;
            case 5:
                // Step 5: Grip / Aim -> Points to fire base target (or extinguisher handle)
                if (_fireTransform == null)
                {
                    var flow = FireScenarioFlowManager.Instance;
                    if (flow != null && flow.fire != null) _fireTransform = flow.fire.transform;
                }
                _activeTargetTransform = _fireTransform != null ? _fireTransform : (_handleTransform != null ? _handleTransform : _actualExtinguisherTransform);
                break;
            case 6:
                // Step 6: Spray step -> target the fire so green pill floats above fire
                if (_fireTransform == null)
                {
                    var flow = FireScenarioFlowManager.Instance;
                    if (flow != null && flow.fire != null) _fireTransform = flow.fire.transform;
                }
                _activeTargetTransform = _fireTransform;
                break;
            default:
                _activeTargetTransform = null;
                break;
        }

        // If no active target or step 6, hide indicator box
        if (_activeTargetTransform == null || _currentStepIndex == 6 || _currentStepIndex == 0)
        {
            _targetIndicatorBox.gameObject.SetActive(false);
            return;
        }

        // If target is pin in Step 4 and it has been removed / deactivated, hide indicator
        if (_currentStepIndex == 4 && _pinTransform != null && !_pinTransform.gameObject.activeInHierarchy)
        {
            _targetIndicatorBox.gameObject.SetActive(false);
            return;
        }

        Camera cam = Camera.main;
        if (cam != null && !cam.orthographic)
        {
            Vector3 worldPos = GetTargetWorldPosition(_activeTargetTransform, _currentStepIndex);
            Vector3 screenPoint = cam.WorldToScreenPoint(worldPos);

            // In front of camera
            if (screenPoint.z > 0.1f)
            {
                // Convert screen point to SafeArea local coordinates
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_safeArea, screenPoint, null, out Vector2 localPoint))
                {
                    // Clamp within safe margins so indicator never clips screen boundaries
                    float halfW = _safeArea.rect.width * 0.5f - 140f;
                    float halfH = _safeArea.rect.height * 0.5f - 160f;
                    localPoint.x = Mathf.Clamp(localPoint.x, -halfW, halfW);
                    localPoint.y = Mathf.Clamp(localPoint.y, -halfH + 60f, halfH - 40f);

                    _targetIndicatorBox.anchoredPosition = localPoint;
                    _targetIndicatorBox.gameObject.SetActive(true);
                    return;
                }
            }
            else
            {
                // Behind camera: hide indicator rather than placing at screen center
                _targetIndicatorBox.gameObject.SetActive(false);
                return;
            }
        }

        // In Editor preview when not in AR:
        if (!Application.isPlaying)
        {
            _targetIndicatorBox.anchoredPosition = new Vector2(0, 60);
            _targetIndicatorBox.gameObject.SetActive(!_isPlacementMode);
        }
        else
        {
            _targetIndicatorBox.gameObject.SetActive(false);
        }
    }

    // =================================================================
    //  VOICE GUIDANCE AUDIO PLAYBACK
    // =================================================================
    private void OnVoiceButtonClicked()
    {
        AppLanguage lang = GetCurrentLanguage();

        if (VoiceGuidanceManager.Instance == null) return;

        if (VoiceGuidanceManager.Instance.IsSpeaking)
        {
            VoiceGuidanceManager.Instance.StopSpeaking();
            SetVoiceButtonIdle();
            return;
        }

        if (lang == AppLanguage.Santali && !VoiceGuidanceManager.Instance.IsAvailableFor(AppLanguage.Santali, _currentStepIndex))
        {
            // Genuine Santali handling: Do NOT substitute Hindi/English audio.
            if (_voiceFeedbackRoutine != null) StopCoroutine(_voiceFeedbackRoutine);
            string pendingMsg = Loc("fire.voice.santaliPending", "Santali voice guidance pending recording");
            _voiceFeedbackRoutine = StartCoroutine(VoiceButtonPulseRoutine(pendingMsg));
            ShowFeedback(pendingMsg, FeedbackType.Correct, 0);
            return;
        }

        SetVoiceButtonActive();
        VoiceGuidanceManager.Instance.PlayStepVoice(_currentStepIndex, lang);

        VoiceGuidanceManager.Instance.OnSpeakingChanged -= HandleVoiceSpeakingChanged;
        VoiceGuidanceManager.Instance.OnSpeakingChanged += HandleVoiceSpeakingChanged;
    }

    private void HandleVoiceSpeakingChanged(bool isSpeaking)
    {
        if (isSpeaking)
            SetVoiceButtonActive();
        else
            SetVoiceButtonIdle();
    }

    private void SetVoiceButtonActive()
    {
        _voiceActive = true;
        if (_voiceBtnBg   != null) _voiceBtnBg.color   = Hex("#DC2626");  // red active
        if (_voiceBtnText != null) _voiceBtnText.text   = Loc("fire.voice.stopBtn", "Stop");
        if (_voiceBtnIcon != null) _voiceBtnIcon.sprite = CreateStopSprite();
    }

    private void SetVoiceButtonIdle()
    {
        _voiceActive = false;
        if (_voiceBtnBg   != null) _voiceBtnBg.color   = Hex("#2563EB");  // royal blue idle
        if (_voiceBtnText != null) _voiceBtnText.text   = Loc("fire.voice.listenBtn", "Listen");
        if (_voiceBtnIcon != null) _voiceBtnIcon.sprite = CreateSpeakerSprite();
    }

    private System.Collections.IEnumerator VoiceButtonPulseRoutine(string pulseText = null)
    {
        if (_voiceBtnBg != null) _voiceBtnBg.color = Hex("#F59E0B"); // amber
        if (_voiceBtnText != null) _voiceBtnText.text = !string.IsNullOrEmpty(pulseText) ? pulseText : Loc("fire.voice.comingSoon", "Coming soon...");
        yield return new WaitForSeconds(1.8f);
        SetVoiceButtonIdle();
    }

    // =================================================================
    //  STEP GUIDANCE & STATE TRANSITIONS (Single Authoritative Card)
    // =================================================================
    public void SetModuleInfo(string moduleTitle, int currentStep, int totalSteps)
    {
        _currentStepIndex = currentStep;
        _totalSteps = totalSteps > 0 ? totalSteps : 6;
        _isPlacementMode = (currentStep == 0);

        // Always resolve scene targets on step transition
        ResolveSceneTargets();

        if (_moduleTitleText != null) _moduleTitleText.text = moduleTitle;

        if (_stepCounterText != null)
        {
            if (_isPlacementMode)
            {
                _stepCounterText.text = "";
            }
            else
            {
                // Localized: "Step X of 6"
                string template = Loc("fire.ar.stepOf", "Step {0} of {1}");
                _stepCounterText.text = string.Format(template, currentStep, _totalSteps);
            }
        }

        if (_timerPill != null)
        {
            _timerPill.gameObject.SetActive(!_isPlacementMode);
        }

        if (_scoreText != null)
        {
            _scoreText.gameObject.SetActive(!_isPlacementMode);
        }

        // Top progress line
        if (_topProgressFill != null)
        {
            float fillPct = _isPlacementMode ? 0f : Mathf.Clamp01((float)currentStep / 6f);
            _topProgressFill.anchorMax = new Vector2(fillPct, 1f);
        }

        // Update target overlay visual components
        ApplyTargetOverlayForStep(currentStep);

        // Update single guidance card
        UpdateGuidanceCardForStep(currentStep);
    }

    private void ApplyTargetOverlayForStep(int step)
    {
        if (_targetIndicatorBox == null) return;

        bool showOverlay = (step >= 1 && step <= 6) && !HasCompletionPanel;
        _targetIndicatorBox.gameObject.SetActive(showOverlay);

        if (!showOverlay) return;

        // Reset all target parts
        if (_targetBracketsGO != null) _targetBracketsGO.SetActive(false);
        if (_targetHandIconGO != null) _targetHandIconGO.SetActive(false);
        if (_targetRingGO != null) _targetRingGO.SetActive(false);
        if (_targetCrosshairGO != null) _targetCrosshairGO.SetActive(false);
        if (_targetCheckmarkGO != null) _targetCheckmarkGO.SetActive(false);

        switch (step)
        {
            case 1: // Identify Hazard
                if (_targetFloatingPillText != null) _targetFloatingPillText.text = Loc("fire.actionHint.step1", "Tap on Fire");
                if (_targetBracketsGO != null) _targetBracketsGO.SetActive(true);
                if (_targetHandIconGO != null) _targetHandIconGO.SetActive(true);
                _targetIndicatorBox.sizeDelta = new Vector2(250, 250);
                break;

            case 2: // Activate Alarm
                if (_targetFloatingPillText != null) _targetFloatingPillText.text = Loc("fire.actionHint.step2", "Tap Alarm");
                if (_targetBracketsGO != null) _targetBracketsGO.SetActive(true);
                if (_targetHandIconGO != null) _targetHandIconGO.SetActive(true);
                _targetIndicatorBox.sizeDelta = new Vector2(220, 240);
                break;

            case 3: // Select Extinguisher
                if (_targetFloatingPillText != null) _targetFloatingPillText.text = Loc("fire.actionHint.step3", "Select CO₂ Extinguisher");
                if (_targetBracketsGO != null) _targetBracketsGO.SetActive(true);
                if (_targetHandIconGO != null) _targetHandIconGO.SetActive(true);
                _targetIndicatorBox.sizeDelta = new Vector2(180, 330);
                break;

            case 4: // Remove Safety Pin
                if (_targetFloatingPillText != null) _targetFloatingPillText.text = Loc("fire.actionHint.step4", "Pull Safety Pin");
                if (_targetRingGO != null) _targetRingGO.SetActive(true);
                if (_targetHandIconGO != null) _targetHandIconGO.SetActive(true);
                _targetIndicatorBox.sizeDelta = new Vector2(200, 200);
                break;

            case 5: // Grip / Aim
                if (_targetFloatingPillText != null) _targetFloatingPillText.text = Loc("fire.actionHint.step5", "Aim Horn at Base");
                if (_targetCrosshairGO != null) _targetCrosshairGO.SetActive(true);
                if (_targetRingGO != null) _targetRingGO.SetActive(false);
                if (_targetHandIconGO != null) _targetHandIconGO.SetActive(false);
                _targetIndicatorBox.sizeDelta = new Vector2(200, 200);
                break;

            case 6: // Spray & Extinguish
                if (_targetFloatingPillText != null) _targetFloatingPillText.text = Loc("fire.actionHint.step6", "Press & Hold to Spray");
                if (_targetFloatingPillGO != null) _targetFloatingPillGO.SetActive(true);
                if (_targetRingGO != null) _targetRingGO.SetActive(false);
                if (_targetHandIconGO != null) _targetHandIconGO.SetActive(false);
                if (_targetCrosshairGO != null) _targetCrosshairGO.SetActive(false);
                if (_targetBracketsGO != null) _targetBracketsGO.SetActive(false);
                _targetIndicatorBox.sizeDelta = new Vector2(260, 60);
                break;
        }
    }

    private void UpdateGuidanceCardForStep(int step)
    {
        bool showCard = (step >= 1 && step <= 6) && !HasCompletionPanel && !_isPlacementMode;
        if (_guidanceCard != null) _guidanceCard.gameObject.SetActive(showCard);
        if (!showCard) return;

        string iconType = "flame";
        Color badgeColor = Hex("#FEE2E2");
        Color iconColor  = Hex("#EF4444");
        string title    = "";
        string body     = "";

        switch (step)
        {
            case 1:
                iconType   = "flame";
                badgeColor = Hex("#FEE2E2");
                iconColor  = Hex("#EF4444");
                title      = Loc("fire.sop.step1.title", "Identify Hazard");
                body       = Loc("fire.sop.step1.desc", "Look around your surroundings to locate the highlighted electrical fire and tap it in AR.");
                break;
            case 2:
                iconType   = "alarm";
                badgeColor = Hex("#FEE2E2");
                iconColor  = Hex("#EF4444");
                title      = Loc("fire.sop.step2.title", "Activate Fire Alarm");
                body       = Loc("fire.sop.step2.desc", "Locate the fire alarm and tap it to activate the alarm as instructed.");
                break;
            case 3:
                iconType   = "extinguisher";
                badgeColor = Hex("#FEE2E2");
                iconColor  = Hex("#EF4444");
                title      = Loc("fire.sop.step3.title", "Select Correct Extinguisher");
                body       = Loc("fire.sop.step3.desc", "Choose the correct extinguisher for the electrical fire.");
                break;
            case 4:
                iconType   = "pin";
                badgeColor = Hex("#FEE2E2");
                iconColor  = Hex("#EF4444");
                title      = Loc("fire.sop.step4.title", "Pull Safety Pin");
                body       = Loc("fire.sop.step4.desc", "Locate the extinguisher safety pin and pull it out before operating the extinguisher.");
                break;
            case 5:
                iconType   = "target";
                badgeColor = Hex("#DBEAFE");
                iconColor  = Hex("#2563EB");
                title      = Loc("fire.sop.step5.title", "Aim at Base of Fire");
                body       = Loc("fire.sop.step5.desc", "Hold the insulated discharge horn. Aim directly at the fuel base of the fire, not at the high flames.");
                break;
            case 6:
                iconType   = "spray";
                badgeColor = Hex("#E0F2FE");
                iconColor  = Hex("#0284C7");
                title      = Loc("fire.sop.step6.title", "Press Handle & Spray");
                body       = Loc("fire.sop.step6.desc", "Press and hold the handle and maintain valid spray on the base of the fire until the fire is extinguished.");
                break;
        }

        if (_guidanceCardIconBadge != null)
            _guidanceCardIconBadge.color = badgeColor;

        if (_guidanceCardIconImg != null)
        {
            _guidanceCardIconImg.sprite = CreateProceduralIcon(iconType);
            _guidanceCardIconImg.color  = iconColor;
        }

        if (_guidanceCardTitle != null) _guidanceCardTitle.text = title;
        if (_guidanceCardBody  != null) _guidanceCardBody.text  = body;

        // Step 6 progress section toggle
        if (_sprayProgressSection != null)
            _sprayProgressSection.gameObject.SetActive(step == 6);

        // Stop previous audio and reset button with proper speaker icon & localized label
        if (VoiceGuidanceManager.Instance != null && VoiceGuidanceManager.Instance.IsSpeaking)
        {
            VoiceGuidanceManager.Instance.StopSpeaking();
        }
        SetVoiceButtonIdle();
    }

    public void ShowGuidance(string stepBadge, string title, string body, string hint,
        string ctaLabel, UnityAction onCtaClick, UnityAction onListenClick = null)
    {
        if (IsAssessmentMode) return;

        _currentActionCallback = onCtaClick;

        if (_isPlacementMode || _currentStepIndex == 0)
        {
            if (_guidanceCard != null) _guidanceCard.gameObject.SetActive(false);

            bool isIntro = (ctaLabel != null && ctaLabel.ToLower().Contains("start")) || 
                           (stepBadge != null && stepBadge.ToLower().Contains("surakshaar"));
            if (isIntro)
            {
                // First guidance UI: "Tap to Start AR Training"
                if (_startCard != null)
                {
                    if (_startCardBtnText != null)
                        _startCardBtnText.text = !string.IsNullOrEmpty(ctaLabel) ? ctaLabel : "TAP TO START AR TRAINING";
                    _startCard.gameObject.SetActive(true);
                }
                if (_placementPill != null) _placementPill.gameObject.SetActive(false);
                if (_placementReticle != null) _placementReticle.gameObject.SetActive(false);
                if (_placementBottomCard != null) _placementBottomCard.gameObject.SetActive(false);
            }
            else
            {
                // Surface Scanning UI
                if (_startCard != null) _startCard.gameObject.SetActive(false);
                if (_placementPill != null) _placementPill.gameObject.SetActive(true);
                if (_placementReticle != null) _placementReticle.gameObject.SetActive(true);
                if (_placementBottomCard != null)
                {
                    _placementBottomCard.gameObject.SetActive(true);
                    if (_placementStatusText != null)
                        _placementStatusText.text = Loc("fire.ar.placementPrompt", "Scanning for surface...");
                }
            }
            return;
        }

        // Steps 1 to 6: ensure all placement and start elements are inactive
        if (_startCard != null) _startCard.gameObject.SetActive(false);
        if (_placementPill != null) _placementPill.gameObject.SetActive(false);
        if (_placementReticle != null) _placementReticle.gameObject.SetActive(false);
        if (_placementBottomCard != null) _placementBottomCard.gameObject.SetActive(false);

        // Show single guidance card
        if (_guidanceCard != null)
        {
            _guidanceCard.gameObject.SetActive(true);

            if (!string.IsNullOrEmpty(title) && _guidanceCardTitle != null)
                _guidanceCardTitle.text = title;

            if (!string.IsNullOrEmpty(body) && _guidanceCardBody != null)
                _guidanceCardBody.text = body;

            if (_sprayProgressSection != null)
                _sprayProgressSection.gameObject.SetActive(_currentStepIndex == 6);
        }

        HasCard = true;
    }

    public void HideGuidance()
    {
        if (_startCard != null) _startCard.gameObject.SetActive(false);
        if (_placementBottomCard != null) _placementBottomCard.gameObject.SetActive(false);
        if (_placementPill != null) _placementPill.gameObject.SetActive(false);
        if (_placementReticle != null) _placementReticle.gameObject.SetActive(false);
        if (_guidanceCard != null) _guidanceCard.gameObject.SetActive(false);
        if (VoiceGuidanceManager.Instance != null && VoiceGuidanceManager.Instance.IsSpeaking)
        {
            VoiceGuidanceManager.Instance.StopSpeaking();
        }
        HasCard = false;
    }

    public void ShowGuidance(string stepTag, string title, string description, string hint = null,
        string actionBtnText = null, UnityAction onActionClicked = null)
    {
        ShowGuidance(stepTag, title, description, hint, actionBtnText, onActionClicked, null);
    }

    // ─────────────────────────────────────────────────────────────────
    //  STEP 6 SPRAY PROGRESS CONTROL (Integrated inside Single Card)
    // ─────────────────────────────────────────────────────────────────
    public void ShowProgress(float value01, string label)
    {
        ShowProgress(value01, value01 * 10f, 10f, label);
    }

    public void ShowProgress(float value01, float elapsedSeconds, float totalSeconds, string label)
    {
        if (_guidanceCard != null && _sprayProgressSection != null)
        {
            _guidanceCard.gameObject.SetActive(true);
            _sprayProgressSection.gameObject.SetActive(true);

            float clamped = Mathf.Clamp01(value01);
            if (_sprayProgressFill != null)
                _sprayProgressFill.anchorMax = new Vector2(clamped, 1f);

            float total = totalSeconds > 0f ? totalSeconds : 10f;
            float elapsed = Mathf.Clamp(elapsedSeconds, 0f, total);
            float remaining = Mathf.Max(0f, total - elapsed);
            if (_sprayTimerText != null)
                _sprayTimerText.text = $"{remaining:F1}s left ({elapsed:F1}/{total:F1}s)";

            if (_sprayStatusText != null)
            {
                if (label != null && (label.ToLower().Contains("off target") || label.ToLower().Contains("paused")))
                {
                    _sprayStatusText.text  = Loc("fire.ar.sprayPaused", "Off Target (Paused)");
                    _sprayStatusText.color = Hex("#F59E0B"); // Amber
                }
                else if (label != null && label.ToLower().Contains("spraying"))
                {
                    _sprayStatusText.text  = Loc("fire.ar.spraying", "Spraying...");
                    _sprayStatusText.color = Hex("#4ADE80"); // Mint Green
                }
                else
                {
                    _sprayStatusText.text  = Loc("fire.ar.readyToSpray", "Ready to Spray");
                    _sprayStatusText.color = Color.white;
                }
            }
        }
    }

    public void HideProgress()
    {
        if (_sprayProgressSection != null)
            _sprayProgressSection.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  COMPLETION CARD (Screen 7: Fire Extinguished!)
    // ─────────────────────────────────────────────────────────────────
    public void ShowCompletion(string title, string message, int score, string timeTaken, UnityAction onContinue)
    {
        HideGuidance();
        HideProgress();

        if (_stepCounterText != null) _stepCounterText.text = "";
        if (_topProgressFill != null) _topProgressFill.anchorMax = new Vector2(1f, 1f);

        // Show green checkmark over fire in 3D center
        if (_targetIndicatorBox != null)
        {
            _targetIndicatorBox.gameObject.SetActive(true);
            if (_targetBracketsGO != null) _targetBracketsGO.SetActive(false);
            if (_targetHandIconGO != null) _targetHandIconGO.SetActive(false);
            if (_targetRingGO != null) _targetRingGO.SetActive(false);
            if (_targetCrosshairGO != null) _targetCrosshairGO.SetActive(false);
            if (_targetFloatingPillGO != null) _targetFloatingPillGO.SetActive(false);
            if (_targetCheckmarkGO != null) _targetCheckmarkGO.SetActive(true);
        }

        if (_completionCard != null)
        {
            if (_compTitle    != null) _compTitle.text    = Loc("fire.ar.completionTitle", "Fire Extinguished!");
            if (_compSubtitle != null) _compSubtitle.text = Loc("fire.ar.completionSub",   "Excellent work! Proceed to assessment.");
            if (_compTimeText  != null) _compTimeText.text  = string.IsNullOrEmpty(timeTaken) ? "05:42" : timeTaken;
            if (_compScoreText != null) _compScoreText.text = $"{score}";

            _homeCallback = onContinue;
            _completionCard.gameObject.SetActive(true);
            HasCompletionPanel = true;
        }

        if (SurakshaAR.Core.AudioManager.Instance != null)
        {
            SurakshaAR.Core.AudioManager.Instance.PlayCompletion();
        }
    }

    public void ShowCompletion(string body, UnityAction retry, UnityAction home)
    {
        ShowCompletion(Loc("fire.ar.completionTitle", "Fire Extinguished!"), body, 100, "05:42", home);
    }

    public void HideCompletion()
    {
        if (_completionCard != null) _completionCard.gameObject.SetActive(false);
        HasCompletionPanel = false;
    }

    // ─────────────────────────────────────────────────────────────────
    //  FEEDBACK & TOAST BANNERS
    // ─────────────────────────────────────────────────────────────────
    public void ShowFeedback(FeedbackType type, string title, string message, float duration = 2.5f)
    {
        string full = string.IsNullOrEmpty(message) ? title : $"{title}: {message}";
        ShowFeedback(full, type, 0);
    }

    public void ShowFeedback(string message, FeedbackType type, int scoreDelta = 0)
    {
        if (_feedbackBanner == null) return;

        if (SurakshaAR.Core.AudioManager.Instance != null)
        {
            switch (type)
            {
                case FeedbackType.Correct:
                    SurakshaAR.Core.AudioManager.Instance.PlayCorrect();
                    break;
                case FeedbackType.Wrong:
                    SurakshaAR.Core.AudioManager.Instance.PlayWrong();
                    break;
                case FeedbackType.Unsafe:
                case FeedbackType.Critical:
                    SurakshaAR.Core.AudioManager.Instance.PlayUnsafe();
                    break;
            }
        }

        switch (type)
        {
            case FeedbackType.Correct:
                _feedbackBg.color = UIColors.SafetyGreen;
                _feedbackText.text = $"✓  {message}  {(scoreDelta > 0 ? $"+{scoreDelta}" : "")}";
                break;
            case FeedbackType.Wrong:
                _feedbackBg.color = Hex("#EF4444");
                _feedbackText.text = $"✕  {message}  {(scoreDelta != 0 ? $"{scoreDelta}" : "")}";
                break;
            case FeedbackType.Unsafe:
                _feedbackBg.color = Hex("#F97316");
                _feedbackText.text = $"⚠️  {message}  {(scoreDelta != 0 ? $"{scoreDelta}" : "")}";
                break;
            case FeedbackType.Critical:
                _feedbackBg.color = Hex("#DC2626");
                _feedbackText.text = $"🚨  CRITICAL: {message}";
                break;
        }

        _feedbackBanner.gameObject.SetActive(true);

        if (_feedbackRoutine != null) StopCoroutine(_feedbackRoutine);
        _feedbackRoutine = StartCoroutine(AutoHideFeedback(2.5f));
    }

    private IEnumerator AutoHideFeedback(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_feedbackBanner != null) _feedbackBanner.gameObject.SetActive(false);
    }

    public void ShowTransientToast(string title, string subtitle, float duration = 2.0f, System.Action onDismiss = null)
    {
        if (_toastRoutine != null) StopCoroutine(_toastRoutine);
        _toastRoutine = StartCoroutine(DoTransientToast(title, subtitle, duration, onDismiss));
    }

    private IEnumerator DoTransientToast(string title, string subtitle, float duration, System.Action onDismiss)
    {
        // DO NOT hide guidance card: toast banner is displayed in the upper area (0.62f) while guidance is at the bottom (0.04f).
        if (SurakshaAR.Core.AudioManager.Instance != null)
            SurakshaAR.Core.AudioManager.Instance.PlayCorrect();

        if (_toastBox != null)
        {
            if (_toastTitle != null) _toastTitle.text = title;
            if (_toastSubtitle != null) _toastSubtitle.text = subtitle;
            _toastBox.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(duration);

        if (_toastBox != null)
            _toastBox.gameObject.SetActive(false);

        // Ensure current step guidance card remains visible and active after toast dismisses
        if (_currentStepIndex >= 1 && _currentStepIndex <= 6 && !HasCompletionPanel && !_isPlacementMode)
        {
            UpdateGuidanceCardForStep(_currentStepIndex);
        }

        onDismiss?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────────
    //  SCORE & TIMER HUD
    // ─────────────────────────────────────────────────────────────────
    public void SetScore(int score, int delta = 0)
    {
        if (_scoreText != null)
        {
            // Localized "Score:" label followed by numeric value
            string scoreLabel = Loc("fire.ar.score", "Score:");
            _scoreText.text = $"{scoreLabel} {score}";
        }

        if (delta != 0 && _scoreDeltaText != null)
        {
            _scoreDeltaText.text = delta > 0 ? $"+{delta}" : $"{delta}";
            _scoreDeltaText.color = delta > 0 ? Hex("#4ADE80") : Hex("#EF4444");
            _scoreDeltaText.gameObject.SetActive(true);

            if (_scoreDeltaRoutine != null) StopCoroutine(_scoreDeltaRoutine);
            _scoreDeltaRoutine = StartCoroutine(AutoHideScoreDelta(1.5f));
        }
    }

    private IEnumerator AutoHideScoreDelta(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_scoreDeltaText != null) _scoreDeltaText.gameObject.SetActive(false);
    }

    public void SetTimer(float elapsedSeconds)
    {
        if (_timerText == null) return;
        float clamped = Mathf.Max(0f, elapsedSeconds);
        int min = Mathf.FloorToInt(clamped / 60f);
        int sec = Mathf.FloorToInt(clamped % 60f);
        _timerText.text = $"{min:00}:{sec:00}";

        if (clamped >= 360f)
            _timerText.color = Hex("#EF4444");
        else if (clamped >= 300f)
            _timerText.color = Hex("#F97316");
        else
            _timerText.color = Color.white;
    }

    public void SetAssessmentMode(bool isAssessment)
    {
        IsAssessmentMode = isAssessment;
        if (isAssessment)
        {
            HideGuidance();
            if (_stepCounterText != null) _stepCounterText.text = "Assessment";
        }
    }

    // ─────────────────────────────────────────────────────────────────
    //  PLACEMENT INTERFACE (Screen 1 in image)
    // ─────────────────────────────────────────────────────────────────
    public void SetPlacementState(bool planeDetected, UnityAction onPlaceClicked)
    {
        _isPlacementMode = true;
        _placementCallback = onPlaceClicked;

        if (_startCard != null) _startCard.gameObject.SetActive(false);
        if (_placementPill != null) _placementPill.gameObject.SetActive(true);
        if (_placementReticle != null) _placementReticle.gameObject.SetActive(true);
        if (_placementBottomCard != null)
        {
            _placementBottomCard.gameObject.SetActive(true);
            if (_placementStatusText != null)
                _placementStatusText.text = planeDetected ? "Surface detected" : "Scanning for surface...";

            if (_btnPlaceScenario != null)
            {
                _btnPlaceScenario.gameObject.SetActive(planeDetected);
                var btnTxt = _btnPlaceScenario.GetComponentInChildren<TextMeshProUGUI>();
                if (btnTxt != null) btnTxt.text = "PLACE SCENARIO";
                var btnRT = _btnPlaceScenario.GetComponent<RectTransform>();
                if (btnRT != null) btnRT.sizeDelta = new Vector2(250, 70);
            }
        }

        if (_targetIndicatorBox != null) _targetIndicatorBox.gameObject.SetActive(false);
        if (_guidanceCard != null) _guidanceCard.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  MODAL DIALOGS
    // ─────────────────────────────────────────────────────────────────
    public void ShowTrackingLost(bool show, UnityAction onRetry = null)
    {
        _trackingRetryCallback = onRetry;
        if (_trackingLostModal != null) _trackingLostModal.gameObject.SetActive(show);
    }

    public void ShowResetConfirmation(UnityAction onConfirmReset)
    {
        _resetConfirmCallback = onConfirmReset;
        if (_resetModal != null) _resetModal.gameObject.SetActive(true);
    }

    public void ShowResetDialog(System.Action onConfirm, System.Action onCancel = null)
    {
        if (_resetModal == null) return;
        _resetConfirmCallback = () =>
        {
            _resetModal.gameObject.SetActive(false);
            onConfirm?.Invoke();
        };

        if (_btnResetCancel != null)
        {
            _btnResetCancel.onClick.RemoveAllListeners();
            _btnResetCancel.onClick.AddListener(() =>
            {
                _resetModal.gameObject.SetActive(false);
                onCancel?.Invoke();
            });
        }

        _resetModal.gameObject.SetActive(true);
    }

    public void ShowCard(string title, string body, string footer)
    {
        if (_messageCard == null) return;
        _messageTitle.text = title;
        _messageBody.text = body;
        _messageFooter.text = footer;
        _messageCard.gameObject.SetActive(true);
        HasCard = true;
    }

    public void HideCard()
    {
        if (_messageCard != null) _messageCard.gameObject.SetActive(false);
        HasCard = false;
    }

    public void ShowHint(string message)
    {
    }

    public void HideHint()
    {
    }

    public void ShowStep(int current, int total)
    {
        SetModuleInfo("Fire & Explosion Response", current, total);
    }

    public void HideStep()
    {
    }

    public void ShowActionButton(string label, UnityAction onClick)
    {
        _currentActionCallback = onClick;
    }

    public void HideActionButton()
    {
    }

    private void HandleBackClicked()
    {
        if (OnBackClicked != null)
        {
            OnBackClicked.Invoke();
        }
        else
        {
            ARModuleLauncher.Instance?.ExitCurrentARScene();
        }
    }

    private static Color Hex(string hex) => UIColors.Hex(hex);
}
