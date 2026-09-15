using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using SurakshaAR.Core;
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

    // Normal Bottom Action Card (Steps 1 - 5)
    private RectTransform _normalCard;
    private Image _normalCardIconImg;
    private TextMeshProUGUI _normalCardTitle;
    private TextMeshProUGUI _normalCardSubtitle;
    private Button _normalCardBtn;
    private UnityAction _currentActionCallback;

    // Spray Progress Two-Tier Card (Step 6)
    private RectTransform _sprayCardContainer;
    private TextMeshProUGUI _sprayStatusText;
    private TextMeshProUGUI _sprayTimerText;
    private RectTransform _sprayProgressFill;
    private TextMeshProUGUI _sprayCardBottomTitle;
    private Image _sprayBadgeIconImg;
    private Button _sprayCardBottomBtn;

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
    }

    private void Start()
    {
        ResolveSceneTargets();
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
        BuildNormalBottomCard(_safeArea);
        BuildSprayTwoTierCard(_safeArea);
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

    // ─────────────────────────────────────────────────────────────────
    //  1. TOP BAR (Matches Reference Image exactly)
    // ─────────────────────────────────────────────────────────────────
    private void BuildTopBar(Transform parent)
    {
        _topBar = UIHelper.MakeRect("TopBar", parent);
        _topBar.anchorMin = new Vector2(0, 1);
        _topBar.anchorMax = new Vector2(1, 1);
        _topBar.pivot = new Vector2(0.5f, 1);
        _topBar.sizeDelta = new Vector2(0, 130);
        _topBar.anchoredPosition = new Vector2(0, -10);

        // A. Back Button (Left: 54x54px dark rounded button)
        var backBtn = UIHelper.MakeButton("btn-ar-back", _topBar, "‹", 32, new Color(0.06f, 0.10f, 0.18f, 0.75f), Color.white, 16);
        var backRT = backBtn.GetComponent<RectTransform>();
        backRT.anchorMin = new Vector2(0, 0.5f);
        backRT.anchorMax = new Vector2(0, 0.5f);
        backRT.pivot = new Vector2(0, 0.5f);
        backRT.sizeDelta = new Vector2(56, 56);
        backRT.anchoredPosition = new Vector2(24, 6);

        var backOutline = backBtn.gameObject.AddComponent<Outline>();
        backOutline.effectColor = new Color(1f, 1f, 1f, 0.15f);
        backOutline.effectDistance = new Vector2(1, -1);
        backBtn.onClick.AddListener(HandleBackClicked);

        // B. Module Title & Step Subtitle Stack (Center / Left)
        var titleStack = UIHelper.MakeRect("TitleStack", _topBar);
        titleStack.anchorMin = new Vector2(0, 0.5f);
        titleStack.anchorMax = new Vector2(0, 0.5f);
        titleStack.pivot = new Vector2(0, 0.5f);
        titleStack.anchoredPosition = new Vector2(96, 6);
        titleStack.sizeDelta = new Vector2(650, 78);

        var vlg = titleStack.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 2;
        vlg.childAlignment = TextAnchor.MiddleLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        _moduleTitleText = UIHelper.MakeLabel("ModuleTitle", titleStack, "Fire & Explosion Response", 32, Color.white, bold: true, wrap: false);
        _moduleTitleText.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(_moduleTitleText.gameObject, preferredWidth: 640, minWidth: 500, preferredHeight: 40);

        _stepCounterText = UIHelper.MakeLabel("StepCounter", titleStack, "Step 1 of 6", 26, Hex("#94A3B8"), bold: false, wrap: false);
        _stepCounterText.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(_stepCounterText.gameObject, preferredWidth: 640, minWidth: 500, preferredHeight: 34);

        // C. Right Cluster (Stopwatch Timer + Score)
        var rightCluster = UIHelper.MakeRect("RightCluster", _topBar);
        rightCluster.anchorMin = new Vector2(1, 0.5f);
        rightCluster.anchorMax = new Vector2(1, 0.5f);
        rightCluster.pivot = new Vector2(1, 0.5f);
        rightCluster.anchoredPosition = new Vector2(-24, 6);
        rightCluster.sizeDelta = new Vector2(340, 56);

        var hlg = rightCluster.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 12;
        hlg.childAlignment = TextAnchor.MiddleRight;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        // Score Label
        _scoreText = UIHelper.MakeLabel("ScoreLbl", rightCluster, "Score: 70", 26, Hex("#CBD5E1"), TextAlignmentOptions.Right, bold: true, wrap: false);
        _scoreText.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(_scoreText.gameObject, preferredWidth: 140, minWidth: 120, preferredHeight: 40);

        _scoreDeltaText = UIHelper.MakeLabel("DeltaLbl", rightCluster, "+10", 26, Hex("#4ADE80"), bold: true, wrap: false);
        _scoreDeltaText.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(_scoreDeltaText.gameObject, preferredWidth: 56, minWidth: 50, preferredHeight: 40);
        _scoreDeltaText.gameObject.SetActive(false);

        // Timer Pill (Stopwatch icon + time)
        _timerPill = UIHelper.MakeRect("TimerPill", rightCluster);
        _timerPill.sizeDelta = new Vector2(150, 48);
        UIHelper.SetLayout(_timerPill.gameObject, preferredWidth: 150, minWidth: 140, preferredHeight: 48);
        var timerImg = _timerPill.gameObject.AddComponent<Image>();
        timerImg.color = new Color(0.06f, 0.10f, 0.18f, 0.65f);
        timerImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(timerImg, 14);

        var timerRow = UIHelper.MakeHorizontal("Row", _timerPill, 6);
        UIHelper.Stretch(timerRow, 8, 8, 0, 0);

        var timerIconGO = UIHelper.MakeRect("TimerIcon", timerRow);
        timerIconGO.sizeDelta = new Vector2(26, 26);
        UIHelper.SetLayout(timerIconGO.gameObject, preferredWidth: 26, minWidth: 26, preferredHeight: 26);
        var timerIconImg = timerIconGO.gameObject.AddComponent<Image>();
        timerIconImg.sprite = CreateProceduralIcon("timer");
        timerIconImg.color = Hex("#94A3B8");
        timerIconImg.type = Image.Type.Simple;
        timerIconImg.preserveAspect = true;

        _timerText = UIHelper.MakeLabel("TimerText", timerRow, "06:58", 34, Color.white, bold: true, wrap: false);
        _timerText.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(_timerText.gameObject, preferredWidth: 98, minWidth: 90, preferredHeight: 40);

        // D. Sleek Thin Progress Bar Line directly below TopBar
        _topProgressBarTrack = UIHelper.MakeRect("TopProgressTrack", _topBar);
        _topProgressBarTrack.anchorMin = new Vector2(0.02f, 0f);
        _topProgressBarTrack.anchorMax = new Vector2(0.98f, 0f);
        _topProgressBarTrack.pivot = new Vector2(0.5f, 0f);
        _topProgressBarTrack.sizeDelta = new Vector2(0, 4);
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
        _placementPill.sizeDelta = new Vector2(480, 52);
        _placementPill.anchoredPosition = new Vector2(0, -165);

        var pillImg = _placementPill.gameObject.AddComponent<Image>();
        pillImg.color = new Color(0.06f, 0.10f, 0.18f, 0.85f);
        pillImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(pillImg, 22);

        var outline = _placementPill.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.15f);
        outline.effectDistance = new Vector2(1, -1);

        _placementPillText = UIHelper.MakeLabel("Text", _placementPill, "Move your phone to find a flat surface", 17, Color.white, TextAlignmentOptions.Center, bold: true, wrap: false);
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

        // Vertical Crosshair Line (extends through ring)
        var vLine = UIHelper.MakeRect("VLine", _placementReticle);
        vLine.anchorMin = new Vector2(0.5f, 0.5f);
        vLine.anchorMax = new Vector2(0.5f, 0.5f);
        vLine.pivot = new Vector2(0.5f, 0.5f);
        vLine.sizeDelta = new Vector2(3f, 130);
        vLine.anchoredPosition = Vector2.zero;
        var vImg = vLine.gameObject.AddComponent<Image>();
        vImg.color = new Color(1f, 1f, 1f, 0.85f);

        // Horizontal Crosshair Line
        var hLine = UIHelper.MakeRect("HLine", _placementReticle);
        hLine.anchorMin = new Vector2(0.5f, 0.5f);
        hLine.anchorMax = new Vector2(0.5f, 0.5f);
        hLine.pivot = new Vector2(0.5f, 0.5f);
        hLine.sizeDelta = new Vector2(50, 3f);
        hLine.anchoredPosition = Vector2.zero;
        var hImg = hLine.gameObject.AddComponent<Image>();
        hImg.color = new Color(1f, 1f, 1f, 0.85f);

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
        _targetIndicatorBox.sizeDelta = new Vector2(260, 260);
        _targetIndicatorBox.anchoredPosition = new Vector2(0, 60);

        // A. Floating Instruction Pill (e.g. "Look at the fire", "Tap to pull alarm", "Tap to pick up")
        _targetFloatingPillGO = new GameObject("FloatingPill");
        _targetFloatingPillGO.transform.SetParent(_targetIndicatorBox, false);
        var pillRT = _targetFloatingPillGO.AddComponent<RectTransform>();
        pillRT.anchorMin = new Vector2(0.5f, 1f);
        pillRT.anchorMax = new Vector2(0.5f, 1f);
        pillRT.pivot = new Vector2(0.5f, 0f);
        pillRT.sizeDelta = new Vector2(240, 42);
        pillRT.anchoredPosition = new Vector2(0, 16);

        var pillImg = _targetFloatingPillGO.AddComponent<Image>();
        pillImg.color = new Color(0.06f, 0.10f, 0.18f, 0.88f);
        pillImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(pillImg, 18);

        var pillOutline = _targetFloatingPillGO.AddComponent<Outline>();
        pillOutline.effectColor = new Color(0.20f, 0.83f, 0.60f, 0.45f); // subtle mint outline
        pillOutline.effectDistance = new Vector2(1, -1);

        _targetFloatingPillText = UIHelper.MakeLabel("PillText", _targetFloatingPillGO.transform, "Look at the fire", 16, Color.white, TextAlignmentOptions.Center, bold: true, wrap: false);
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

        var checkIconGO = UIHelper.MakeRect("CheckIcon", checkRT);
        UIHelper.Stretch(checkIconGO, 22, 22, 22, 22);
        var checkIconImg = checkIconGO.gameObject.AddComponent<Image>();
        checkIconImg.sprite = CreateProceduralIcon("checkmark");
        checkIconImg.color = Color.white;
        checkIconImg.type = Image.Type.Simple;
        checkIconImg.preserveAspect = true;

        _targetIndicatorBox.gameObject.SetActive(true);
    }

    private static void CreateCornerBracket(string name, Transform parent, Vector2 cornerAnchor, Color col, bool horizontalRight, bool verticalDown)
    {
        var cornerGO = new GameObject("Corner_" + name);
        cornerGO.transform.SetParent(parent, false);
        var rt = cornerGO.AddComponent<RectTransform>();
        rt.anchorMin = cornerAnchor;
        rt.anchorMax = cornerAnchor;
        rt.pivot = cornerAnchor;
        rt.sizeDelta = new Vector2(32, 32);

        // Horizontal line
        var hLine = UIHelper.MakeRect("H", rt);
        hLine.anchorMin = cornerAnchor;
        hLine.anchorMax = cornerAnchor;
        hLine.pivot = cornerAnchor;
        hLine.sizeDelta = new Vector2(32, 3.5f);
        hLine.anchoredPosition = Vector2.zero;
        var hImg = hLine.gameObject.AddComponent<Image>();
        hImg.color = col;

        // Vertical line
        var vLine = UIHelper.MakeRect("V", rt);
        vLine.anchorMin = cornerAnchor;
        vLine.anchorMax = cornerAnchor;
        vLine.pivot = cornerAnchor;
        vLine.sizeDelta = new Vector2(3.5f, 32);
        vLine.anchoredPosition = Vector2.zero;
        var vImg = vLine.gameObject.AddComponent<Image>();
        vImg.color = col;
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
    }

    // ─────────────────────────────────────────────────────────────────
    //  4. NORMAL BOTTOM CARD (White rounded card, Steps 1 - 5)
    // ─────────────────────────────────────────────────────────────────
    private void BuildNormalBottomCard(Transform parent)
    {
        _normalCard = UIHelper.MakeRect("NormalBottomCard", parent);
        _normalCard.anchorMin = new Vector2(0.5f, 0f);
        _normalCard.anchorMax = new Vector2(0.5f, 0f);
        _normalCard.pivot = new Vector2(0.5f, 0f);
        _normalCard.sizeDelta = new Vector2(980, 88);
        _normalCard.anchoredPosition = new Vector2(0, 32);

        // Pure white card background
        var cardImg = _normalCard.gameObject.AddComponent<Image>();
        cardImg.color = Color.white;
        cardImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(cardImg, 22);

        var shadow = _normalCard.gameObject.AddComponent<Outline>();
        shadow.effectColor = new Color(0, 0, 0, 0.20f);
        shadow.effectDistance = new Vector2(1, -2);

        _normalCardBtn = _normalCard.gameObject.AddComponent<Button>();
        _normalCardBtn.onClick.AddListener(() => _currentActionCallback?.Invoke());

        var hlg = _normalCard.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(20, 20, 14, 14);
        hlg.spacing = 18;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        // Left: Red Rounded Square Icon Badge
        var iconBadge = UIHelper.MakeRect("IconBadge", _normalCard);
        iconBadge.sizeDelta = new Vector2(52, 52);
        UIHelper.SetLayout(iconBadge.gameObject, preferredWidth: 52, minWidth: 52, preferredHeight: 52);
        var iconBg = iconBadge.gameObject.AddComponent<Image>();
        iconBg.color = Hex("#EF4444"); // Safety Red
        iconBg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(iconBg, 14);

        var iconInner = UIHelper.MakeRect("InnerIcon", iconBadge);
        UIHelper.Stretch(iconInner, 10, 10, 10, 10);
        _normalCardIconImg = iconInner.gameObject.AddComponent<Image>();
        _normalCardIconImg.sprite = CreateProceduralIcon("flame");
        _normalCardIconImg.color = Color.white;
        _normalCardIconImg.type = Image.Type.Simple;
        _normalCardIconImg.preserveAspect = true;

        // Middle: Title & Subtitle Stack (Flexible width to fill card)
        var textStack = UIHelper.MakeRect("TextStack", _normalCard);
        UIHelper.SetLayout(textStack.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 56);

        var vlg = textStack.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 2;
        vlg.childAlignment = TextAnchor.MiddleLeft;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        _normalCardTitle = UIHelper.MakeLabel("Title", textStack, "Identify the Hazard", 36, Hex("#0F172A"), bold: true, wrap: false);
        _normalCardTitle.overflowMode = TextOverflowModes.Ellipsis;
        UIHelper.SetLayout(_normalCardTitle.gameObject, preferredHeight: 44);

        _normalCardSubtitle = UIHelper.MakeLabel("Subtitle", textStack, "Find the electrical fire.", 26, Hex("#64748B"), wrap: false);
        _normalCardSubtitle.overflowMode = TextOverflowModes.Ellipsis;
        UIHelper.SetLayout(_normalCardSubtitle.gameObject, preferredHeight: 34);

        // Right: Chevron Arrow >
        var arrow = UIHelper.MakeLabel("Arrow", _normalCard, "›", 32, Hex("#94A3B8"), TextAlignmentOptions.Center, bold: true, wrap: false);
        arrow.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(arrow.gameObject, preferredWidth: 36, minWidth: 36, preferredHeight: 52);

        _normalCard.gameObject.SetActive(true);
    }

    // ─────────────────────────────────────────────────────────────────
    //  5. SPRAY TWO-TIER BOTTOM CARD (Step 6 in image)
    // ─────────────────────────────────────────────────────────────────
    private void BuildSprayTwoTierCard(Transform parent)
    {
        _sprayCardContainer = UIHelper.MakeRect("SprayTwoTierCard", parent);
        _sprayCardContainer.anchorMin = new Vector2(0.5f, 0f);
        _sprayCardContainer.anchorMax = new Vector2(0.5f, 0f);
        _sprayCardContainer.pivot = new Vector2(0.5f, 0f);
        _sprayCardContainer.sizeDelta = new Vector2(980, 166);
        _sprayCardContainer.anchoredPosition = new Vector2(0, 32);

        var vlg = _sprayCardContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Top Tier: Dark Progress Box
        var topBox = UIHelper.MakeRect("TopProgressBox", _sprayCardContainer);
        topBox.sizeDelta = new Vector2(980, 74);
        UIHelper.SetLayout(topBox.gameObject, preferredWidth: 980, minWidth: 980, preferredHeight: 74);

        var topImg = topBox.gameObject.AddComponent<Image>();
        topImg.color = new Color(0.06f, 0.10f, 0.18f, 0.90f);
        topImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(topImg, 18);

        var topOutline = topBox.gameObject.AddComponent<Outline>();
        topOutline.effectColor = new Color(1f, 1f, 1f, 0.12f);
        topOutline.effectDistance = new Vector2(1, -1);

        var topVlg = topBox.gameObject.AddComponent<VerticalLayoutGroup>();
        topVlg.padding = new RectOffset(20, 20, 12, 12);
        topVlg.spacing = 8;
        topVlg.childControlWidth = true;
        topVlg.childControlHeight = true;
        topVlg.childForceExpandWidth = true;

        // Row with status and timer
        var statRow = UIHelper.MakeHorizontal("StatusRow", topBox, 10);
        statRow.sizeDelta = new Vector2(940, 26);
        UIHelper.SetLayout(statRow.gameObject, preferredWidth: 940, minWidth: 940, preferredHeight: 26);
        var statHlg = statRow.GetComponent<HorizontalLayoutGroup>();
        if (statHlg != null)
        {
            statHlg.childControlWidth = true;
            statHlg.childControlHeight = true;
            statHlg.childForceExpandWidth = false;
            statHlg.childForceExpandHeight = false;
        }

        _sprayStatusText = UIHelper.MakeLabel("Status", statRow, "Spraying...", 28, Color.white, bold: true, wrap: false);
        _sprayStatusText.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(_sprayStatusText.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 36);

        _sprayTimerText = UIHelper.MakeLabel("Timer", statRow, "6.5 / 10.0 s", 32, Hex("#E2E8F0"), TextAlignmentOptions.Right, bold: true, wrap: false);
        _sprayTimerText.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(_sprayTimerText.gameObject, preferredWidth: 260, minWidth: 240, preferredHeight: 36);

        // Progress Bar
        var progTrack = UIHelper.MakeRect("Track", topBox);
        UIHelper.SetLayout(progTrack.gameObject, preferredHeight: 12);

        var progTrackImg = progTrack.gameObject.AddComponent<Image>();
        progTrackImg.color = Hex("#334155");
        progTrackImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(progTrackImg, 6);

        var progFillGO = new GameObject("Fill");
        progFillGO.transform.SetParent(progTrack, false);
        _sprayProgressFill = progFillGO.AddComponent<RectTransform>();
        _sprayProgressFill.anchorMin = Vector2.zero;
        _sprayProgressFill.anchorMax = new Vector2(0.65f, 1f);
        _sprayProgressFill.offsetMin = Vector2.zero;
        _sprayProgressFill.offsetMax = Vector2.zero;

        var progFillImg = progFillGO.AddComponent<Image>();
        progFillImg.color = Hex("#22C55E");
        progFillImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(progFillImg, 6);

        // Bottom Tier: White Card
        var bottomCard = UIHelper.MakeRect("BottomCard", _sprayCardContainer);
        bottomCard.sizeDelta = new Vector2(980, 88);
        UIHelper.SetLayout(bottomCard.gameObject, preferredWidth: 980, minWidth: 980, preferredHeight: 88);

        var bottomImg = bottomCard.gameObject.AddComponent<Image>();
        bottomImg.color = Color.white;
        bottomImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(bottomImg, 20);

        _sprayCardBottomBtn = bottomCard.gameObject.AddComponent<Button>();
        _sprayCardBottomBtn.onClick.AddListener(() => _currentActionCallback?.Invoke());

        var bRow = bottomCard.gameObject.AddComponent<HorizontalLayoutGroup>();
        bRow.padding = new RectOffset(16, 16, 12, 12);
        bRow.spacing = 14;
        bRow.childAlignment = TextAnchor.MiddleLeft;
        bRow.childControlWidth = true;
        bRow.childControlHeight = true;
        bRow.childForceExpandWidth = false;
        bRow.childForceExpandHeight = false;

        var sBadge = UIHelper.MakeRect("Badge", bottomCard);
        sBadge.sizeDelta = new Vector2(56, 56);
        UIHelper.SetLayout(sBadge.gameObject, preferredWidth: 56, minWidth: 56, preferredHeight: 56);
        var sBadgeImg = sBadge.gameObject.AddComponent<Image>();
        sBadgeImg.color = Hex("#EF4444");
        sBadgeImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(sBadgeImg, 14);

        var sInner = UIHelper.MakeRect("Inner", sBadge);
        UIHelper.Stretch(sInner, 8, 8, 8, 8);
        _sprayBadgeIconImg = sInner.gameObject.AddComponent<Image>();
        _sprayBadgeIconImg.sprite = CreateProceduralIcon("extinguisher");
        _sprayBadgeIconImg.color = Color.white;
        _sprayBadgeIconImg.type = Image.Type.Simple;
        _sprayBadgeIconImg.preserveAspect = true;

        _sprayCardBottomTitle = UIHelper.MakeLabel("Title", bottomCard, "Keep spraying at the base", 34, Hex("#0F172A"), bold: true, wrap: false);
        _sprayCardBottomTitle.overflowMode = TextOverflowModes.Ellipsis;
        UIHelper.SetLayout(_sprayCardBottomTitle.gameObject, flexibleWidth: true, flexWidth: 1, preferredHeight: 42);

        var bArrow = UIHelper.MakeLabel("Arrow", bottomCard, "›", 30, Hex("#94A3B8"), TextAlignmentOptions.Center, bold: true, wrap: false);
        bArrow.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(bArrow.gameObject, preferredWidth: 32, minWidth: 32, preferredHeight: 44);

        _sprayCardContainer.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  6. PLACEMENT BOTTOM CARD (Screen 1 in image)
    // ─────────────────────────────────────────────────────────────────
    private void BuildPlacementBottomCard(Transform parent)
    {
        _placementBottomCard = UIHelper.MakeRect("PlacementBottomCard", parent);
        _placementBottomCard.anchorMin = new Vector2(0.5f, 0f);
        _placementBottomCard.anchorMax = new Vector2(0.5f, 0f);
        _placementBottomCard.pivot = new Vector2(0.5f, 0f);
        _placementBottomCard.sizeDelta = new Vector2(560, 68);
        _placementBottomCard.anchoredPosition = new Vector2(0, 36);

        // Scanning pill
        var pillImg = _placementBottomCard.gameObject.AddComponent<Image>();
        pillImg.color = new Color(0.06f, 0.10f, 0.18f, 0.85f);
        pillImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(pillImg, 24);

        var outline = _placementBottomCard.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.15f);
        outline.effectDistance = new Vector2(1, -1);

        var hlg = _placementBottomCard.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(20, 20, 10, 10);
        hlg.spacing = 12;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        // Radar Scan Icon
        var scanIconGO = UIHelper.MakeRect("ScanIcon", _placementBottomCard);
        scanIconGO.sizeDelta = new Vector2(28, 28);
        UIHelper.SetLayout(scanIconGO.gameObject, preferredWidth: 28, minWidth: 28, preferredHeight: 28);
        var scanImg = scanIconGO.gameObject.AddComponent<Image>();
        scanImg.sprite = CreateRingSprite(28, 4, Hex("#38BDF8"));
        scanImg.color = Color.white;
        scanImg.type = Image.Type.Simple;

        var scanDot = UIHelper.MakeRect("ScanDot", scanIconGO);
        scanDot.anchorMin = new Vector2(0.5f, 0.5f);
        scanDot.anchorMax = new Vector2(0.5f, 0.5f);
        scanDot.pivot = new Vector2(0.5f, 0.5f);
        scanDot.sizeDelta = new Vector2(8, 8);
        var scanDotImg = scanDot.gameObject.AddComponent<Image>();
        scanDotImg.sprite = UIHelper.GetCircleSprite();
        scanDotImg.color = Hex("#38BDF8");
        scanDotImg.type = Image.Type.Simple;

        _placementStatusText = UIHelper.MakeLabel("Status", _placementBottomCard, "Scanning for surface...", 18, Color.white, TextAlignmentOptions.Center, bold: true, wrap: false);
        _placementStatusText.overflowMode = TextOverflowModes.Overflow;
        UIHelper.SetLayout(_placementStatusText.gameObject, preferredWidth: 360, minWidth: 320, preferredHeight: 32);

        _btnPlaceScenario = UIHelper.MakeButton("btn-place", _placementBottomCard, "Place", 18, Hex("#22C55E"), Color.white, 14);
        _btnPlaceScenario.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 44);
        UIHelper.SetLayout(_btnPlaceScenario.gameObject, preferredWidth: 100, minWidth: 100, preferredHeight: 44);
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
        _startCard.anchorMin = new Vector2(0.5f, 0f);
        _startCard.anchorMax = new Vector2(0.5f, 0f);
        _startCard.pivot = new Vector2(0.5f, 0f);
        _startCard.sizeDelta = new Vector2(980, 84);
        _startCard.anchoredPosition = new Vector2(0, 36);

        _btnStartTraining = UIHelper.MakeButton("btn-start-training", _startCard, "TAP TO START AR TRAINING", 20,
            Hex("#1E3A8A"), Color.white, 20);
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
        _completionCard.sizeDelta = new Vector2(960, 380);
        _completionCard.anchoredPosition = new Vector2(0, 48);

        var img = _completionCard.gameObject.AddComponent<Image>();
        img.color = new Color(0.06f, 0.10f, 0.18f, 0.95f);
        img.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(img, 28);

        var outline = _completionCard.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.15f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        var vlg = _completionCard.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(36, 36, 28, 28);
        vlg.spacing = 14;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        _compTitle = UIHelper.MakeLabel("Title", _completionCard, "Fire Extinguished!", 28, Color.white, TextAlignmentOptions.Center, bold: true, wrap: false);
        _compTitle.rectTransform.sizeDelta = new Vector2(880, 38);
        UIHelper.SetLayout(_compTitle.gameObject, preferredWidth: 880, minWidth: 880, preferredHeight: 38);

        _compSubtitle = UIHelper.MakeLabel("Subtitle", _completionCard, "Well Done!", 20, Hex("#4ADE80"), TextAlignmentOptions.Center, bold: true, wrap: false);
        _compSubtitle.rectTransform.sizeDelta = new Vector2(880, 28);
        UIHelper.SetLayout(_compSubtitle.gameObject, preferredWidth: 880, minWidth: 880, preferredHeight: 28);

        // Stats Row (Time + Score)
        var statRow = UIHelper.MakeHorizontal("StatRow", _completionCard, 24);
        statRow.sizeDelta = new Vector2(500, 36);
        UIHelper.SetLayout(statRow.gameObject, preferredWidth: 500, minWidth: 500, preferredHeight: 36);
        var statHlg = statRow.GetComponent<HorizontalLayoutGroup>();
        if (statHlg != null)
        {
            statHlg.childControlWidth = false;
            statHlg.childControlHeight = false;
            statHlg.childAlignment = TextAnchor.MiddleCenter;
        }

        var timeLbl = UIHelper.MakeLabel("TimeLbl", statRow, "Time", 18, Hex("#94A3B8"), TextAlignmentOptions.Right, wrap: false);
        timeLbl.rectTransform.sizeDelta = new Vector2(60, 32);
        UIHelper.SetLayout(timeLbl.gameObject, preferredWidth: 60, minWidth: 60, preferredHeight: 32);

        _compTimeText = UIHelper.MakeLabel("TimeVal", statRow, "05:42", 20, Color.white, bold: true, wrap: false);
        _compTimeText.rectTransform.sizeDelta = new Vector2(90, 32);
        UIHelper.SetLayout(_compTimeText.gameObject, preferredWidth: 90, minWidth: 90, preferredHeight: 32);

        var scoreLbl = UIHelper.MakeLabel("ScoreLbl", statRow, "Score", 18, Hex("#94A3B8"), TextAlignmentOptions.Right, wrap: false);
        scoreLbl.rectTransform.sizeDelta = new Vector2(60, 32);
        UIHelper.SetLayout(scoreLbl.gameObject, preferredWidth: 60, minWidth: 60, preferredHeight: 32);

        _compScoreText = UIHelper.MakeLabel("ScoreVal", statRow, "100", 20, Color.white, bold: true, wrap: false);
        _compScoreText.rectTransform.sizeDelta = new Vector2(80, 32);
        UIHelper.SetLayout(_compScoreText.gameObject, preferredWidth: 80, minWidth: 80, preferredHeight: 32);

        // Primary Action CTA Button: "Continue to Assessment →"
        _btnCompContinue = UIHelper.MakeButton("btn-assessment", _completionCard, "Continue to Assessment →", 20,
            Hex("#1E3A8A"), Color.white, 16);
        _btnCompContinue.GetComponent<RectTransform>().sizeDelta = new Vector2(880, 56);
        UIHelper.SetLayout(_btnCompContinue.gameObject, preferredWidth: 880, minWidth: 880, preferredHeight: 56);
        _btnCompContinue.onClick.AddListener(() => _homeCallback?.Invoke());

        _completionCard.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  8. FEEDBACK TOAST BANNER
    // ─────────────────────────────────────────────────────────────────
    private void BuildFeedbackBanner(Transform parent)
    {
        _feedbackBanner = UIHelper.MakeRect("FeedbackBanner", parent);
        _feedbackBanner.anchorMin = new Vector2(0.08f, 1f);
        _feedbackBanner.anchorMax = new Vector2(0.92f, 1f);
        _feedbackBanner.pivot = new Vector2(0.5f, 1f);
        _feedbackBanner.sizeDelta = new Vector2(0, 64);
        _feedbackBanner.anchoredPosition = new Vector2(0, -145);

        _feedbackBg = _feedbackBanner.gameObject.AddComponent<Image>();
        _feedbackBg.color = UIColors.SafetyGreen;
        _feedbackBg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(_feedbackBg, 18);

        _feedbackText = UIHelper.MakeLabel("FeedbackText", _feedbackBanner, "✓  Hazard Identified  +10", 20, Color.white, TextAlignmentOptions.Center, bold: true);
        UIHelper.Stretch(_feedbackText.GetComponent<RectTransform>(), 16, 16, 0, 0);

        _feedbackBanner.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  9. TRANSIENT TOAST MODAL (Auto-dismissing step transitions)
    // ─────────────────────────────────────────────────────────────────
    private void BuildTransientToastModal(Transform parent)
    {
        _toastBox = UIHelper.MakeRect("TransientToastModal", parent);
        _toastBox.anchorMin = new Vector2(0.1f, 0.62f);
        _toastBox.anchorMax = new Vector2(0.9f, 0.62f);
        _toastBox.pivot = new Vector2(0.5f, 0.5f);
        _toastBox.sizeDelta = new Vector2(0, 130);

        var bg = _toastBox.gameObject.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.10f, 0.18f, 0.92f);
        bg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(bg, 22);

        var outline = _toastBox.gameObject.AddComponent<Outline>();
        outline.effectColor = Hex("#34D399");
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        var vlg = _toastBox.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(24, 24, 18, 18);
        vlg.spacing = 6;
        vlg.childAlignment = TextAnchor.MiddleCenter;

        _toastTitle = UIHelper.MakeLabel("ToastTitle", _toastBox, "Hazard Identified!", 24, Color.white, TextAlignmentOptions.Center, bold: true);
        UIHelper.SetLayout(_toastTitle.gameObject, preferredHeight: 34);

        _toastSubtitle = UIHelper.MakeLabel("ToastSub", _toastBox, "Now activate the fire alarm", 18, Hex("#A7F3D0"), TextAlignmentOptions.Center);
        UIHelper.SetLayout(_toastSubtitle.gameObject, preferredHeight: 26);

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
        vlg.padding = new RectOffset(28, 28, 24, 24);
        vlg.spacing = 14;
        vlg.childAlignment = TextAnchor.MiddleCenter;

        var title = UIHelper.MakeLabel("Title", box, "⚠️ AR Tracking Paused", 24, Hex("#FDE68A"), TextAlignmentOptions.Center, bold: true);
        UIHelper.SetLayout(title.gameObject, preferredHeight: 36);

        var desc = UIHelper.MakeLabel("Desc", box, "Move phone slowly toward a well-lit textured surface.", 18, Color.white, TextAlignmentOptions.Center, wrap: true);
        UIHelper.SetLayout(desc.gameObject, preferredHeight: 52);

        _btnTrackingRetry = UIHelper.MakeButton("btn-resume", box, "Resume Training", 20, Hex("#22C55E"), Color.white, 16);
        UIHelper.SetLayout(_btnTrackingRetry.gameObject, preferredHeight: 52);
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
        vlg.padding = new RectOffset(24, 24, 20, 20);
        vlg.spacing = 14;
        vlg.childAlignment = TextAnchor.MiddleCenter;

        var title = UIHelper.MakeLabel("Title", box, "Restart Scenario?", 24, Color.white, TextAlignmentOptions.Center, bold: true);
        UIHelper.SetLayout(title.gameObject, preferredHeight: 34);

        var desc = UIHelper.MakeLabel("Desc", box, "Your scenario placement and actions will reset to step 1.", 16, Hex("#94A3B8"), TextAlignmentOptions.Center, wrap: true);
        UIHelper.SetLayout(desc.gameObject, preferredHeight: 44);

        var bRow = UIHelper.MakeHorizontal("BtnRow", box, 12);
        UIHelper.SetLayout(bRow.gameObject, preferredHeight: 48);

        _btnResetCancel = UIHelper.MakeButton("btn-cancel", bRow, "Cancel", 18, new Color(1, 1, 1, 0.15f), Color.white, 14);
        UIHelper.SetLayout(_btnResetCancel.gameObject, flexibleWidth: true, flexWidth: 1);
        _btnResetCancel.onClick.AddListener(() => _resetModal.gameObject.SetActive(false));

        _btnResetConfirm = UIHelper.MakeButton("btn-restart", bRow, "Restart", 18, Hex("#EF4444"), Color.white, 14);
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
        vlg.padding = new RectOffset(32, 32, 32, 32);
        vlg.spacing = 16;
        vlg.childAlignment = TextAnchor.MiddleCenter;

        _messageTitle = UIHelper.MakeLabel("Title", _messageCard, "SurakshaAR", 32, Color.white, TextAlignmentOptions.Center, bold: true);
        UIHelper.SetLayout(_messageTitle.gameObject, preferredHeight: 44);

        _messageBody = UIHelper.MakeLabel("Body", _messageCard, "AR Fire Safety Module", 20, Hex("#CBD5E1"), TextAlignmentOptions.Center, wrap: true);
        UIHelper.SetLayout(_messageBody.gameObject, flexibleHeight: true, flexHeight: 1);

        _messageFooter = UIHelper.MakeLabel("Footer", _messageCard, "Tap anywhere to continue", 18, Hex("#4ADE80"), TextAlignmentOptions.Center, bold: true);
        UIHelper.SetLayout(_messageFooter.gameObject, preferredHeight: 32);

        _messageCard.gameObject.SetActive(false);
    }

    // =================================================================
    //  WORLD-TO-SCREEN TARGET TRACKING (3D AR Guidance)
    // =================================================================
    private void ResolveSceneTargets()
    {
        // 1. Fire Transform
        if (_fireTransform == null)
        {
            var go = GameObject.Find("VFX_Fire_01_Small") ?? GameObject.Find("Flames") ?? GameObject.Find("Hazard") ?? GameObject.Find("Electric Box");
            if (go != null)
            {
                _fireTransform = go.transform;
            }
            else
            {
                var fireExt = FindAnyObjectByType<FireExtinguishable>();
                if (fireExt != null)
                    _fireTransform = fireExt.transform;
            }
        }

        // 2. Alarm Transform
        if (_alarmTransform == null)
        {
            var alarm = FindAnyObjectByType<AlarmInteraction>();
            if (alarm != null)
                _alarmTransform = alarm.transform;
            else
            {
                var go = GameObject.Find("FireAlarm") ?? GameObject.Find("EUfFireAlarm") ?? GameObject.Find("Alarm");
                if (go != null) _alarmTransform = go.transform;
            }
        }

        // 3. Display Extinguisher Transform (for Step 3 selection ONLY)
        if (_displayExtinguisherTransform == null)
        {
            var dispGO = GameObject.Find("FireExt_display") ?? GameObject.Find("ExtinguisherDisplay");
            if (dispGO != null) _displayExtinguisherTransform = dispGO.transform;
            else
            {
                var dispComp = FindAnyObjectByType<ExtinguisherDisplayPickup>(FindObjectsInactive.Include);
                if (dispComp != null) _displayExtinguisherTransform = dispComp.transform;
            }
        }

        // 4. Actual Picked-Up Extinguisher (for Steps 4, 5, 6)
        if (_actualExtinguisherTransform == null || !_actualExtinguisherTransform.gameObject.activeInHierarchy)
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

        // 5. Actual Safety Pin on Picked-Up Extinguisher (Step 4)
        if (_actualExtinguisherTransform != null)
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
            var pinComp = FindAnyObjectByType<FirePinInteraction>();
            if (pinComp != null) _pinTransform = pinComp.transform;
        }

        // 6. Actual Operating Lever / Handle on Picked-Up Extinguisher (Step 6)
        if (_actualExtinguisherTransform != null)
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
            var gripComp = FindAnyObjectByType<ExtinguisherGripInteraction>();
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

    private void UpdateWorldTargetPosition()
    {
        if (_targetIndicatorBox == null || _safeArea == null) return;

        // Dynamically ensure targets are resolved on current active extinguisher
        if (_currentStepIndex >= 4 && (_pinTransform == null || _handleTransform == null))
            ResolveSceneTargets();

        // Resolve active target based on current step
        switch (_currentStepIndex)
        {
            case 1:
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
                // Step 5: Points to actual fire base
                _activeTargetTransform = _fireTransform;
                break;
            case 6:
                // Step 6: Points strictly to actual grip/handle on PICKED-UP extinguisher
                _activeTargetTransform = _handleTransform != null ? _handleTransform : _actualExtinguisherTransform;
                break;
            default:
                _activeTargetTransform = null;
                break;
        }

        // If target is pin in Step 4 and it has been removed / deactivated, hide indicator
        if (_currentStepIndex == 4 && _pinTransform != null && !_pinTransform.gameObject.activeInHierarchy)
        {
            _targetIndicatorBox.gameObject.SetActive(false);
            return;
        }

        Camera cam = Camera.main;
        if (cam != null && _activeTargetTransform != null && !cam.orthographic)
        {
            Vector3 worldPos = _activeTargetTransform.position;

            // Offset to center of visual object
            if (_currentStepIndex == 1 || _currentStepIndex == 5)
                worldPos += Vector3.up * 0.25f;
            else if (_currentStepIndex == 6 && _handleTransform != null)
                worldPos += Vector3.up * 0.05f;

            Vector3 screenPoint = cam.WorldToScreenPoint(worldPos);

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
        }

        // Mock / Editor preview fallback: anchor at reference center
        if (HasCompletionPanel)
        {
            _targetIndicatorBox.anchoredPosition = new Vector2(0, 180);
            _targetIndicatorBox.gameObject.SetActive(true);
        }
        else
        {
            _targetIndicatorBox.anchoredPosition = new Vector2(0, 60);
            _targetIndicatorBox.gameObject.SetActive(!_isPlacementMode);
        }
    }

    // =================================================================
    //  STEP GUIDANCE & STATE TRANSITIONS (Matches Reference Image)
    // =================================================================
    public void SetModuleInfo(string moduleTitle, int currentStep, int totalSteps)
    {
        _currentStepIndex = currentStep;
        _totalSteps = totalSteps > 0 ? totalSteps : 6;
        _isPlacementMode = (currentStep == 0);

        if (_moduleTitleText != null) _moduleTitleText.text = moduleTitle;

        if (_stepCounterText != null)
        {
            if (_isPlacementMode)
            {
                _stepCounterText.text = "";
            }
            else
            {
                _stepCounterText.text = $"Step {currentStep} of 6";
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
                if (_targetFloatingPillText != null) _targetFloatingPillText.text = "Tap to identify the fire";
                if (_targetBracketsGO != null) _targetBracketsGO.SetActive(true);
                if (_targetHandIconGO != null) _targetHandIconGO.SetActive(true);
                _targetIndicatorBox.sizeDelta = new Vector2(230, 230);
                break;

            case 2: // Activate Alarm
                if (_targetFloatingPillText != null) _targetFloatingPillText.text = "Tap to pull alarm";
                if (_targetBracketsGO != null) _targetBracketsGO.SetActive(true);
                if (_targetHandIconGO != null) _targetHandIconGO.SetActive(true);
                _targetIndicatorBox.sizeDelta = new Vector2(200, 220);
                break;

            case 3: // Select Extinguisher
                if (_targetFloatingPillText != null) _targetFloatingPillText.text = "Tap to pick up";
                if (_targetBracketsGO != null) _targetBracketsGO.SetActive(true);
                if (_targetHandIconGO != null) _targetHandIconGO.SetActive(true);
                _targetIndicatorBox.sizeDelta = new Vector2(160, 310);
                break;

            case 4: // Remove Safety Pin
                if (_targetFloatingPillText != null) _targetFloatingPillText.text = "Tap to remove pin";
                if (_targetRingGO != null) _targetRingGO.SetActive(true);
                if (_targetHandIconGO != null) _targetHandIconGO.SetActive(true);
                _targetIndicatorBox.sizeDelta = new Vector2(180, 180);
                break;

            case 5: // Aim at Base
                if (_targetFloatingPillText != null) _targetFloatingPillText.text = "Aim at the base";
                if (_targetCrosshairGO != null) _targetCrosshairGO.SetActive(true);
                _targetIndicatorBox.sizeDelta = new Vector2(180, 180);
                break;

            case 6: // Spray & Extinguish (Guidance pointing to handle)
                if (_targetFloatingPillText != null) _targetFloatingPillText.text = "Press and hold to spray";
                if (_targetHandIconGO != null) _targetHandIconGO.SetActive(true);
                if (_targetRingGO != null) _targetRingGO.SetActive(true);
                _targetIndicatorBox.sizeDelta = new Vector2(160, 160);
                break;
        }
    }

    public void ShowGuidance(string stepBadge, string title, string body, string hint,
        string ctaLabel, UnityAction onCtaClick, UnityAction onListenClick = null)
    {
        if (IsAssessmentMode) return;

        _currentActionCallback = onCtaClick;

        if (_isPlacementMode || _currentStepIndex == 0)
        {
            if (_normalCard != null) _normalCard.gameObject.SetActive(false);
            if (_sprayCardContainer != null) _sprayCardContainer.gameObject.SetActive(false);

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
                        _placementStatusText.text = "Scanning for surface...";
                }
            }
            return;
        }

        // Steps 1 to 6: ensure all placement and start elements are inactive
        if (_startCard != null) _startCard.gameObject.SetActive(false);
        if (_placementPill != null) _placementPill.gameObject.SetActive(false);
        if (_placementReticle != null) _placementReticle.gameObject.SetActive(false);
        if (_placementBottomCard != null) _placementBottomCard.gameObject.SetActive(false);

        if (_currentStepIndex == 6)
        {
            // Step 6: Two-Tier Spray Progress Card
            if (_normalCard != null) _normalCard.gameObject.SetActive(false);
            if (_sprayCardContainer != null)
            {
                _sprayCardContainer.gameObject.SetActive(true);
                if (_sprayCardBottomTitle != null) _sprayCardBottomTitle.text = "Keep spraying at the base.";
            }
        }
        else
        {
            // Steps 1 to 5: Compact White Rounded Card
            if (_sprayCardContainer != null) _sprayCardContainer.gameObject.SetActive(false);
            if (_normalCard != null)
            {
                _normalCard.gameObject.SetActive(true);

                // Configure Icon & Text matching reference image
                switch (_currentStepIndex)
                {
                    case 1:
                        if (_normalCardIconImg != null) _normalCardIconImg.sprite = CreateProceduralIcon("flame");
                        _normalCardTitle.text = "Identify the Hazard";
                        _normalCardSubtitle.text = "Find the electrical fire.";
                        break;
                    case 2:
                        if (_normalCardIconImg != null) _normalCardIconImg.sprite = CreateProceduralIcon("alarm");
                        _normalCardTitle.text = "Activate the Alarm";
                        _normalCardSubtitle.text = "Pull the emergency alarm.";
                        break;
                    case 3:
                        if (_normalCardIconImg != null) _normalCardIconImg.sprite = CreateProceduralIcon("extinguisher");
                        _normalCardTitle.text = "Select CO<sub>2</sub> Extinguisher";
                        _normalCardSubtitle.text = "Pick the correct CO<sub>2</sub> extinguisher.";
                        break;
                    case 4:
                        if (_normalCardIconImg != null) _normalCardIconImg.sprite = CreateProceduralIcon("pin");
                        _normalCardTitle.text = "Remove Safety Pin";
                        _normalCardSubtitle.text = "Pull the safety pin.";
                        break;
                    case 5:
                        if (_normalCardIconImg != null) _normalCardIconImg.sprite = CreateProceduralIcon("target");
                        _normalCardTitle.text = "Aim at Base";
                        _normalCardSubtitle.text = "Point the nozzle at the base of the fire.";
                        break;
                    default:
                        _normalCardTitle.text = title;
                        _normalCardSubtitle.text = body;
                        break;
                }
            }
        }

        HasCard = true;
    }

    public void HideGuidance()
    {
        if (_startCard != null) _startCard.gameObject.SetActive(false);
        if (_normalCard != null) _normalCard.gameObject.SetActive(false);
        if (_sprayCardContainer != null) _sprayCardContainer.gameObject.SetActive(false);
        if (_placementBottomCard != null) _placementBottomCard.gameObject.SetActive(false);
        if (_placementPill != null) _placementPill.gameObject.SetActive(false);
        if (_placementReticle != null) _placementReticle.gameObject.SetActive(false);
        HasCard = false;
    }

    public void ShowGuidance(string stepTag, string title, string description, string hint = null,
        string actionBtnText = null, UnityAction onActionClicked = null)
    {
        ShowGuidance(stepTag, title, description, hint, actionBtnText, onActionClicked, null);
    }

    // ─────────────────────────────────────────────────────────────────
    //  STEP 6 SPRAY PROGRESS CONTROL
    // ─────────────────────────────────────────────────────────────────
    public void ShowProgress(float value01, string label)
    {
        ShowProgress(value01, value01 * 10f, 10f, label);
    }

    public void ShowProgress(float value01, float elapsedSeconds, float totalSeconds, string label)
    {
        if (_sprayCardContainer != null)
        {
            _sprayCardContainer.gameObject.SetActive(true);
            if (_normalCard != null) _normalCard.gameObject.SetActive(false);

            float clamped = Mathf.Clamp01(value01);
            if (_sprayProgressFill != null)
                _sprayProgressFill.anchorMax = new Vector2(clamped, 1f);

            float total = totalSeconds > 0f ? totalSeconds : 10f;
            float elapsed = Mathf.Clamp(elapsedSeconds, 0f, total);
            if (_sprayTimerText != null)
                _sprayTimerText.text = $"{elapsed:F1} / {total:F1} s";

            if (_sprayStatusText != null)
            {
                if (label != null && label.ToLower().Contains("off target"))
                {
                    _sprayStatusText.text = "Off Target";
                    _sprayStatusText.color = Hex("#F59E0B"); // Amber
                }
                else if (elapsed > 0.05f)
                {
                    _sprayStatusText.text = "Spraying...";
                    _sprayStatusText.color = Hex("#4ADE80"); // Mint Green
                }
                else
                {
                    _sprayStatusText.text = "Ready to Spray";
                    _sprayStatusText.color = Color.white;
                }
            }
        }
    }

    public void HideProgress()
    {
        if (_sprayCardContainer != null)
            _sprayCardContainer.gameObject.SetActive(false);
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
            if (_compTitle != null) _compTitle.text = "Fire Extinguished!";
            if (_compSubtitle != null) _compSubtitle.text = "Well Done!";
            if (_compTimeText != null) _compTimeText.text = string.IsNullOrEmpty(timeTaken) ? "05:42" : timeTaken;
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
        ShowCompletion("Fire Extinguished!", body, 100, "05:42", home);
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
        HideGuidance();

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

        onDismiss?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────────
    //  SCORE & TIMER HUD
    // ─────────────────────────────────────────────────────────────────
    public void SetScore(int score, int delta = 0)
    {
        if (_scoreText != null) _scoreText.text = $"Score: {score}";

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
                if (btnTxt != null) btnTxt.text = "PLACE TRAINING SCENARIO";
                var btnRT = _btnPlaceScenario.GetComponent<RectTransform>();
                if (btnRT != null) btnRT.sizeDelta = new Vector2(230, 44);
            }
        }

        if (_targetIndicatorBox != null) _targetIndicatorBox.gameObject.SetActive(false);
        if (_normalCard != null) _normalCard.gameObject.SetActive(false);
        if (_sprayCardContainer != null) _sprayCardContainer.gameObject.SetActive(false);
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
