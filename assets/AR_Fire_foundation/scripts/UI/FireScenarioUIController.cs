using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using SurakshaAR.Core;
using SurakshaAR.UI;

/// <summary>
/// Pure Unity UGUI AR Training HUD.
/// Replaces the legacy UI Toolkit implementation with a responsive,
/// glassmorphic government-grade AR HUD matching Figma references.
/// </summary>
[DisallowMultipleComponent]
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

    // Top Bar
    private RectTransform _topBar;
    private TextMeshProUGUI _stepCounterText;
    private TextMeshProUGUI _moduleTitleText;
    private RectTransform _topProgressFill;

    // Score & Timer HUD
    private RectTransform _scoreHud;
    private TextMeshProUGUI _scoreText;
    private TextMeshProUGUI _scoreDeltaText;
    private TextMeshProUGUI _timerText;
    private Coroutine _scoreDeltaRoutine;

    // Feedback Banner
    private RectTransform _feedbackBanner;
    private Image _feedbackBg;
    private TextMeshProUGUI _feedbackText;
    private Coroutine _feedbackRoutine;

    // Guidance Card
    private RectTransform _guidanceCard;
    private TextMeshProUGUI _guidanceStepBadge;
    private TextMeshProUGUI _guidanceTitle;
    private TextMeshProUGUI _guidanceBody;
    private TextMeshProUGUI _guidanceHintText;
    private GameObject _hintBoxGO;
    private Button _btnHint;
    private Button _btnListen;
    private Button _btnAction;
    private TextMeshProUGUI _actionBtnText;
    private UnityAction _currentActionCallback;
    private UnityAction _currentListenCallback;

    // Spray Progress Bar
    private RectTransform _sprayProgressBox;
    private TextMeshProUGUI _sprayProgressText;
    private RectTransform _sprayProgressFill;

    // Generic Message Card (for Intro & Outro)
    private RectTransform _messageCard;
    private TextMeshProUGUI _messageTitle;
    private TextMeshProUGUI _messageBody;
    private TextMeshProUGUI _messageFooter;

    // Tracking Lost Panel
    private RectTransform _trackingLostModal;
    private Button _btnTrackingRetry;
    private UnityAction _trackingRetryCallback;

    // Scenario Reset Modal
    private RectTransform _resetModal;
    private Button _btnResetConfirm;
    private Button _btnResetCancel;
    private UnityAction _resetConfirmCallback;

    // Legacy completion panel
    private RectTransform _completionPanel;
    private TextMeshProUGUI _completionBody;
    private Button _btnRetry;
    private Button _btnHome;
    private UnityAction _retryCallback;
    private UnityAction _homeCallback;

    // Hint banner (compact)
    private RectTransform _hintBanner;
    private TextMeshProUGUI _hintBannerText;

    // Transient Toast Banner (Timed guidance transition)
    private RectTransform _toastBox;
    private TextMeshProUGUI _toastTitle;
    private TextMeshProUGUI _toastSubtitle;
    private Coroutine _toastRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        BuildUGUIHierarchy();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // =================================================================
    //  UGUI HIERARCHY BUILDER
    // =================================================================
    private void BuildUGUIHierarchy()
    {
        // Canvas
        var canvasGO = new GameObject("AR_UGUI_Canvas");
        canvasGO.transform.SetParent(transform, false);

        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100; // Above 3D scene

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 2400);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f; // Width-first for portrait mobile

        canvasGO.AddComponent<GraphicRaycaster>();

        // Safe Area
        var safeGO = new GameObject("SafeArea");
        safeGO.transform.SetParent(canvasGO.transform, false);
        _safeArea = safeGO.AddComponent<RectTransform>();
        UIHelper.Stretch(_safeArea, 0, 0, 0, 0);

        var safeDriver = safeGO.AddComponent<SafeAreaDriver>();

        // ── 1. Top Bar ───────────────────────────────────────────────
        BuildTopBar(_safeArea);

        // ── 2. Score & Timer HUD ─────────────────────────────────────
        BuildScoreHUD(_safeArea);

        // ── 3. Feedback Toast ────────────────────────────────────────
        BuildFeedbackBanner(_safeArea);

        // ── 4. Guidance Card ─────────────────────────────────────────
        BuildGuidanceCard(_safeArea);

        // ── 5. Spray Progress Bar ────────────────────────────────────
        BuildSprayProgress(_safeArea);

        // ── 6. Fullscreen Message Card ───────────────────────────────
        BuildMessageCard(_safeArea);

        // ── 7. Compact Hint Banner ───────────────────────────────────
        BuildCompactHintBanner(_safeArea);

        // ── 7b. Transient Toast Modal (Timed Transitions) ────────────
        BuildTransientToastModal(_safeArea);

        // ── 8. Tracking Lost Modal ───────────────────────────────────
        BuildTrackingLostModal(_safeArea);

        // ── 9. Reset Modal ───────────────────────────────────────────
        BuildResetModal(_safeArea);

        // ── 10. Completion Panel ─────────────────────────────────────
        BuildCompletionPanel(_safeArea);
    }

    // ─────────────────────────────────────────────────────────────────
    //  1. TOP BAR (Figma AR Style)
    // ─────────────────────────────────────────────────────────────────
    private void BuildTopBar(Transform parent)
    {
        _topBar = UIHelper.MakeRect("TopBar", parent);
        _topBar.anchorMin = new Vector2(0, 1);
        _topBar.anchorMax = new Vector2(1, 1);
        _topBar.pivot = new Vector2(0.5f, 1);
        _topBar.sizeDelta = new Vector2(0, 140);
        _topBar.anchoredPosition = new Vector2(0, -20);

        // Back button (left)
        var backBtn = UIHelper.MakeButton("btn-ar-back", _topBar, "‹", 38, new Color(0, 0, 0, 0.55f), Color.white, 20);
        var backRT = backBtn.GetComponent<RectTransform>();
        backRT.anchorMin = new Vector2(0, 0.5f);
        backRT.anchorMax = new Vector2(0, 0.5f);
        backRT.pivot = new Vector2(0, 0.5f);
        backRT.sizeDelta = new Vector2(68, 68);
        backRT.anchoredPosition = new Vector2(28, 0);
        backBtn.onClick.AddListener(HandleBackClicked);

        // Center / Right pill banner
        var pillGO = new GameObject("PillBanner");
        pillGO.transform.SetParent(_topBar, false);
        var pillRT = pillGO.AddComponent<RectTransform>();
        pillRT.anchorMin = new Vector2(0.20f, 0.5f);
        pillRT.anchorMax = new Vector2(0.96f, 0.5f);
        pillRT.pivot = new Vector2(0.5f, 0.5f);
        pillRT.sizeDelta = new Vector2(0, 72);
        pillRT.anchoredPosition = Vector2.zero;

        var pillImg = pillGO.AddComponent<Image>();
        pillImg.color = UIColors.ARBarBg;
        pillImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(pillImg, 24);

        var pillOutline = pillGO.AddComponent<Outline>();
        pillOutline.effectColor = new Color(1f, 1f, 1f, 0.15f);
        pillOutline.effectDistance = new Vector2(1, -1);

        // Pill Inner Content Row
        var row = UIHelper.MakeHorizontal("InnerRow", pillGO.transform, 10, new RectOffset(20, 20, 0, 10));
        UIHelper.Stretch(row, 0, 0, 0, 0);

        var iconLbl = UIHelper.MakeLabel("Icon", row, "🔥", 24, Color.white);
        UIHelper.SetLayout(iconLbl.gameObject, preferredWidth: 32, preferredHeight: 32);

        _moduleTitleText = UIHelper.MakeLabel("ModuleTitle", row, "Fire Response", 22, Color.white, bold: true);
        UIHelper.SetLayout(_moduleTitleText.gameObject, flexibleWidth: true, flexWidth: 1);

        _stepCounterText = UIHelper.MakeLabel("StepCounter", row, "Step 1 / 4", 20, Hex("#A7F3D0"), TextAlignmentOptions.Right, bold: true);
        UIHelper.SetLayout(_stepCounterText.gameObject, preferredWidth: 140, preferredHeight: 30);

        // Progress Bar (anchored directly to bottom edge of pill)
        var progTrack = UIHelper.MakeRect("ProgressTrack", pillGO.transform);
        progTrack.anchorMin = new Vector2(0.04f, 0f);
        progTrack.anchorMax = new Vector2(0.96f, 0f);
        progTrack.pivot = new Vector2(0.5f, 0f);
        progTrack.sizeDelta = new Vector2(0, 6);
        progTrack.anchoredPosition = new Vector2(0, 6);

        var progTrackImg = progTrack.gameObject.AddComponent<Image>();
        progTrackImg.color = new Color(1f, 1f, 1f, 0.20f);
        progTrackImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(progTrackImg, 3);

        var progFillGO = new GameObject("ProgressFill");
        progFillGO.transform.SetParent(progTrack, false);
        _topProgressFill = progFillGO.AddComponent<RectTransform>();
        _topProgressFill.anchorMin = Vector2.zero;
        _topProgressFill.anchorMax = new Vector2(0.25f, 1f);
        _topProgressFill.offsetMin = Vector2.zero;
        _topProgressFill.offsetMax = Vector2.zero;

        var progFillImg = progFillGO.AddComponent<Image>();
        progFillImg.color = UIColors.ARProgressBarGreen;
        progFillImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(progFillImg, 3);
    }

    // ─────────────────────────────────────────────────────────────────
    //  2. SCORE & TIMER HUD
    // ─────────────────────────────────────────────────────────────────
    private void BuildScoreHUD(Transform parent)
    {
        _scoreHud = UIHelper.MakeRect("ScoreHUD", parent);
        _scoreHud.anchorMin = new Vector2(0.04f, 1f);
        _scoreHud.anchorMax = new Vector2(0.96f, 1f);
        _scoreHud.pivot = new Vector2(0.5f, 1f);
        _scoreHud.sizeDelta = new Vector2(0, 52);
        _scoreHud.anchoredPosition = new Vector2(0, -170);

        var row = UIHelper.MakeHorizontal("Row", _scoreHud, 14, new RectOffset(16, 16, 0, 0));
        UIHelper.Stretch(row, 0, 0, 0, 0);

        // Left: Score Box
        var scoreBox = UIHelper.MakeHorizontal("ScoreBox", row, 8);
        UIHelper.SetLayout(scoreBox.gameObject, preferredWidth: 260, preferredHeight: 46);

        _scoreText = UIHelper.MakeLabel("ScoreLbl", scoreBox, "Score: 100", 22, Color.white, bold: true);
        UIHelper.SetLayout(_scoreText.gameObject, preferredWidth: 150, preferredHeight: 32);

        _scoreDeltaText = UIHelper.MakeLabel("DeltaLbl", scoreBox, "+10", 20, Hex("#4ADE80"), bold: true);
        UIHelper.SetLayout(_scoreDeltaText.gameObject, preferredWidth: 80, preferredHeight: 32);
        _scoreDeltaText.gameObject.SetActive(false);

        // Center space
        var spacer = UIHelper.MakeRect("Spacer", row);
        UIHelper.SetLayout(spacer.gameObject, flexibleWidth: true, flexWidth: 1);

        // Reset Button (compact icon button)
        var resetBtn = UIHelper.MakeButton("btn-reset", row, "↺ Reset", 18, new Color(0, 0, 0, 0.45f), Color.white, 16);
        UIHelper.SetLayout(resetBtn.gameObject, preferredWidth: 110, preferredHeight: 44);
        resetBtn.onClick.AddListener(HandleResetScenarioClicked);

        // Right: Timer Box
        var timerBox = UIHelper.MakeHorizontal("TimerBox", row, 6);
        UIHelper.SetLayout(timerBox.gameObject, preferredWidth: 140, preferredHeight: 46);

        var timerIcon = UIHelper.MakeLabel("TimerIcon", timerBox, "⏱", 20, Color.white);
        UIHelper.SetLayout(timerIcon.gameObject, preferredWidth: 28, preferredHeight: 28);

        _timerText = UIHelper.MakeLabel("TimerLbl", timerBox, "00:00", 22, Hex("#E2E8F0"), bold: true);
        UIHelper.SetLayout(_timerText.gameObject, preferredWidth: 90, preferredHeight: 32);
    }

    // ─────────────────────────────────────────────────────────────────
    //  3. FEEDBACK BANNER (Animated Toast)
    // ─────────────────────────────────────────────────────────────────
    private void BuildFeedbackBanner(Transform parent)
    {
        _feedbackBanner = UIHelper.MakeRect("FeedbackBanner", parent);
        _feedbackBanner.anchorMin = new Vector2(0.08f, 1f);
        _feedbackBanner.anchorMax = new Vector2(0.92f, 1f);
        _feedbackBanner.pivot = new Vector2(0.5f, 1f);
        _feedbackBanner.sizeDelta = new Vector2(0, 76);
        _feedbackBanner.anchoredPosition = new Vector2(0, -230);

        _feedbackBg = _feedbackBanner.gameObject.AddComponent<Image>();
        _feedbackBg.color = UIColors.SafetyGreen;
        _feedbackBg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(_feedbackBg, 20);

        var outline = _feedbackBanner.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1, 1, 1, 0.25f);
        outline.effectDistance = new Vector2(1, -1);

        _feedbackText = UIHelper.MakeLabel("FeedbackText", _feedbackBanner, "✓  Correct  +10", 24, Color.white, TextAlignmentOptions.Center, bold: true);
        UIHelper.Stretch(_feedbackText.GetComponent<RectTransform>(), 16, 16, 0, 0);

        _feedbackBanner.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  4. GUIDANCE CARD (Figma Glassmorphism overlay)
    // ─────────────────────────────────────────────────────────────────
    private void BuildGuidanceCard(Transform parent)
    {
        _guidanceCard = UIHelper.MakeRect("GuidanceCard", parent);
        _guidanceCard.anchorMin = new Vector2(0.04f, 0f);
        _guidanceCard.anchorMax = new Vector2(0.96f, 0f);
        _guidanceCard.pivot = new Vector2(0.5f, 0f);
        _guidanceCard.sizeDelta = new Vector2(0, 480);
        _guidanceCard.anchoredPosition = new Vector2(0, 36);

        // Dark glass card background
        var cardImg = _guidanceCard.gameObject.AddComponent<Image>();
        cardImg.color = UIColors.AROverlayDark;
        cardImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(cardImg, 28);

        var cardOutline = _guidanceCard.gameObject.AddComponent<Outline>();
        cardOutline.effectColor = UIColors.AROverlayBorder;
        cardOutline.effectDistance = new Vector2(1.5f, -1.5f);

        var vlg = _guidanceCard.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(28, 28, 24, 24);
        vlg.spacing = 14;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        var csf = _guidanceCard.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Header Row: Step Badge + Hint Button
        var headRow = UIHelper.MakeHorizontal("HeadRow", _guidanceCard, 10);
        UIHelper.SetLayout(headRow.gameObject, preferredHeight: 44);

        _guidanceStepBadge = UIHelper.MakeLabel("StepBadge", headRow, "🔥  STEP 1 OF 4", 18, Hex("#4ADE80"), bold: true);
        UIHelper.SetLayout(_guidanceStepBadge.gameObject, flexibleWidth: true, flexWidth: 1);

        _btnHint = UIHelper.MakeButton("btn-hint", headRow, "💡 Hint", 18, new Color(1f, 1f, 1f, 0.14f), Hex("#FDE68A"), 16);
        UIHelper.SetLayout(_btnHint.gameObject, preferredWidth: 110, preferredHeight: 40);
        _btnHint.onClick.AddListener(OnHintClicked);

        // Title
        _guidanceTitle = UIHelper.MakeLabel("Title", _guidanceCard, "Identify the Fire Source", 28, Color.white, bold: true);
        UIHelper.SetLayout(_guidanceTitle.gameObject, preferredHeight: 38);

        // Body SOP text
        _guidanceBody = UIHelper.MakeLabel("Body", _guidanceCard,
            "Look around the environment. Locate the fire symbol marker. Do NOT approach without protective equipment.",
            20, Hex("#CBD5E1"), wrap: true);
        UIHelper.SetLayout(_guidanceBody.gameObject, preferredHeight: 76);

        // Collapsible Hint Box
        _hintBoxGO = new GameObject("HintBox");
        _hintBoxGO.transform.SetParent(_guidanceCard, false);
        UIHelper.SetLayout(_hintBoxGO, preferredHeight: 80);

        var hintImg = _hintBoxGO.AddComponent<Image>();
        hintImg.color = UIColors.AmberBg;
        hintImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(hintImg, 14);

        var hintBoxOutline = _hintBoxGO.AddComponent<Outline>();
        hintBoxOutline.effectColor = UIColors.AmberBorder;
        hintBoxOutline.effectDistance = new Vector2(1, -1);

        var hintInner = UIHelper.MakeVertical("HintInner", _hintBoxGO.transform, 4, new RectOffset(16, 16, 12, 12));
        UIHelper.Stretch(hintInner, 0, 0, 0, 0);

        _guidanceHintText = UIHelper.MakeLabel("HintText", hintInner,
            "Tip: Electrical fires require Class B or CO2 extinguishers. Do not use water under any circumstances.",
            18, UIColors.AmberTextDark, wrap: true);
        UIHelper.Stretch(_guidanceHintText.GetComponent<RectTransform>(), 0, 0, 0, 0);

        _hintBoxGO.SetActive(false); // Closed by default

        // Bottom Action Row: Listen Button + Big Primary CTA
        var actionRow = UIHelper.MakeHorizontal("ActionRow", _guidanceCard, 14);
        UIHelper.SetLayout(actionRow.gameObject, preferredHeight: 68);

        _btnListen = UIHelper.MakeButton("btn-listen", actionRow, "🔊 Listen", 20, new Color(1f, 1f, 1f, 0.12f), Color.white, 20);
        UIHelper.SetLayout(_btnListen.gameObject, preferredWidth: 150, preferredHeight: 68);
        _btnListen.onClick.AddListener(OnListenClicked);

        _btnAction = UIHelper.MakeButton("btn-action-primary", actionRow, "I have identified the fire source", 22,
            UIColors.SafetyGreen, Color.white, 20);
        UIHelper.SetLayout(_btnAction.gameObject, flexibleWidth: true, flexWidth: 1);
        _btnAction.onClick.AddListener(OnPrimaryActionClicked);
        _actionBtnText = _btnAction.GetComponentInChildren<TextMeshProUGUI>();
    }

    // ─────────────────────────────────────────────────────────────────
    //  5. SPRAY PROGRESS BAR
    // ─────────────────────────────────────────────────────────────────
    private void BuildSprayProgress(Transform parent)
    {
        _sprayProgressBox = UIHelper.MakeRect("SprayProgressBox", parent);
        _sprayProgressBox.anchorMin = new Vector2(0.06f, 0.28f);
        _sprayProgressBox.anchorMax = new Vector2(0.94f, 0.28f);
        _sprayProgressBox.pivot = new Vector2(0.5f, 0.5f);
        _sprayProgressBox.sizeDelta = new Vector2(0, 92);

        var bg = _sprayProgressBox.gameObject.AddComponent<Image>();
        bg.color = UIColors.ARBarBg;
        bg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(bg, 20);

        var vlg = _sprayProgressBox.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 14, 14);
        vlg.spacing = 8;
        vlg.childForceExpandWidth = true;

        _sprayProgressText = UIHelper.MakeLabel("SprayText", _sprayProgressBox, "Spraying the fire... 0.0s / 10s", 20, Color.white, bold: true);
        UIHelper.SetLayout(_sprayProgressText.gameObject, preferredHeight: 28);

        var track = UIHelper.MakeRect("Track", _sprayProgressBox);
        UIHelper.SetLayout(track.gameObject, preferredHeight: 16);
        var trackImg = track.gameObject.AddComponent<Image>();
        trackImg.color = new Color(1, 1, 1, 0.20f);
        trackImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(trackImg, 8);

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(track, false);
        _sprayProgressFill = fillGO.AddComponent<RectTransform>();
        _sprayProgressFill.anchorMin = Vector2.zero;
        _sprayProgressFill.anchorMax = new Vector2(0f, 1f);
        _sprayProgressFill.offsetMin = Vector2.zero;
        _sprayProgressFill.offsetMax = Vector2.zero;

        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = UIColors.ARProgressBarGreen;
        fillImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(fillImg, 8);

        _sprayProgressBox.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  6. FULLSCREEN MESSAGE CARD (Intro / Outro)
    // ─────────────────────────────────────────────────────────────────
    private void BuildMessageCard(Transform parent)
    {
        _messageCard = UIHelper.MakeRect("MessageCard", parent);
        _messageCard.anchorMin = new Vector2(0.06f, 0.15f);
        _messageCard.anchorMax = new Vector2(0.94f, 0.82f);
        _messageCard.offsetMin = Vector2.zero;
        _messageCard.offsetMax = Vector2.zero;

        var img = _messageCard.gameObject.AddComponent<Image>();
        img.color = UIColors.AROverlayDark;
        img.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(img, 30);

        var outline = _messageCard.gameObject.AddComponent<Outline>();
        outline.effectColor = UIColors.AROverlayBorder;
        outline.effectDistance = new Vector2(2, -2);

        var vlg = _messageCard.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(36, 36, 40, 36);
        vlg.spacing = 20;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        _messageTitle = UIHelper.MakeLabel("Title", _messageCard, "SURAKSHAAR", 36, Color.white, TextAlignmentOptions.Center, bold: true);
        UIHelper.SetLayout(_messageTitle.gameObject, preferredHeight: 48);

        _messageBody = UIHelper.MakeLabel("Body", _messageCard, "AR Fire-Safety Training Scenario", 22, Hex("#E2E8F0"), TextAlignmentOptions.Center, wrap: true);
        UIHelper.SetLayout(_messageBody.gameObject, flexibleHeight: true, flexHeight: 1);

        _messageFooter = UIHelper.MakeLabel("Footer", _messageCard, "TAP ANYWHERE TO CONTINUE", 20, Hex("#4ADE80"), TextAlignmentOptions.Center, bold: true);
        UIHelper.SetLayout(_messageFooter.gameObject, preferredHeight: 36);

        _messageCard.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  7. COMPACT HINT BANNER
    // ─────────────────────────────────────────────────────────────────
    private void BuildCompactHintBanner(Transform parent)
    {
        _hintBanner = UIHelper.MakeRect("CompactHintBanner", parent);
        _hintBanner.anchorMin = new Vector2(0.08f, 0.10f);
        _hintBanner.anchorMax = new Vector2(0.92f, 0.10f);
        _hintBanner.pivot = new Vector2(0.5f, 0f);
        _hintBanner.sizeDelta = new Vector2(0, 84);

        var img = _hintBanner.gameObject.AddComponent<Image>();
        img.color = UIColors.ARBarBg;
        img.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(img, 20);

        _hintBannerText = UIHelper.MakeLabel("Text", _hintBanner, "", 20, Color.white, TextAlignmentOptions.Center, wrap: true);
        UIHelper.Stretch(_hintBannerText.GetComponent<RectTransform>(), 16, 16, 8, 8);

        _hintBanner.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  7b. TRANSIENT TOAST MODAL (Auto-dismissing step transitions)
    // ─────────────────────────────────────────────────────────────────
    private void BuildTransientToastModal(Transform parent)
    {
        _toastBox = UIHelper.MakeRect("TransientToastModal", parent);
        _toastBox.anchorMin = new Vector2(0.06f, 0.65f);
        _toastBox.anchorMax = new Vector2(0.94f, 0.65f);
        _toastBox.pivot = new Vector2(0.5f, 0.5f);
        _toastBox.sizeDelta = new Vector2(0, 160);

        var bg = _toastBox.gameObject.AddComponent<Image>();
        bg.color = UIColors.AROverlayDark;
        bg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(bg, 28);

        var outline = _toastBox.gameObject.AddComponent<Outline>();
        outline.effectColor = UIColors.SafetyGreen;
        outline.effectDistance = new Vector2(2, -2);

        var vlg = _toastBox.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(28, 28, 20, 20);
        vlg.spacing = 8;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        _toastTitle = UIHelper.MakeLabel("ToastTitle", _toastBox, "Hazard Identified!", 32, Color.white, TextAlignmentOptions.Center, bold: true, wrap: true);
        UIHelper.SetLayout(_toastTitle.gameObject, preferredHeight: 46);

        _toastSubtitle = UIHelper.MakeLabel("ToastSub", _toastBox, "Now locate the fire extinguisher", 24, Hex("#A7F3D0"), TextAlignmentOptions.Center, wrap: true);
        UIHelper.SetLayout(_toastSubtitle.gameObject, preferredHeight: 38);

        _toastBox.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  8. AR TRACKING LOST MODAL
    // ─────────────────────────────────────────────────────────────────
    private void BuildTrackingLostModal(Transform parent)
    {
        _trackingLostModal = UIHelper.MakeRect("TrackingLostModal", parent);
        UIHelper.Stretch(_trackingLostModal, 0, 0, 0, 0);

        var blocker = _trackingLostModal.gameObject.AddComponent<Image>();
        blocker.color = new Color(0, 0, 0, 0.70f);

        var box = UIHelper.MakeRect("Box", _trackingLostModal);
        box.anchorMin = new Vector2(0.1f, 0.35f);
        box.anchorMax = new Vector2(0.9f, 0.65f);
        box.offsetMin = Vector2.zero;
        box.offsetMax = Vector2.zero;

        var boxImg = box.gameObject.AddComponent<Image>();
        boxImg.color = UIColors.AROverlayDark;
        boxImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(boxImg, 26);

        var vlg = box.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(32, 32, 28, 28);
        vlg.spacing = 16;
        vlg.childForceExpandWidth = true;

        var title = UIHelper.MakeLabel("Title", box, "⚠️ AR Tracking Paused", 28, Hex("#FDE68A"), TextAlignmentOptions.Center, bold: true);
        UIHelper.SetLayout(title.gameObject, preferredHeight: 40);

        var desc = UIHelper.MakeLabel("Desc", box,
            "Move your phone slowly and point it toward a well-lit surface with visible texture.",
            20, Color.white, TextAlignmentOptions.Center, wrap: true);
        UIHelper.SetLayout(desc.gameObject, preferredHeight: 64);

        _btnTrackingRetry = UIHelper.MakeButton("btn-retry-tracking", box, "Resume Training", 22,
            UIColors.SafetyGreen, Color.white, 18);
        UIHelper.SetLayout(_btnTrackingRetry.gameObject, preferredHeight: 60);
        _btnTrackingRetry.onClick.AddListener(() =>
        {
            _trackingLostModal.gameObject.SetActive(false);
            _trackingRetryCallback?.Invoke();
        });

        _trackingLostModal.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  9. RESET CONFIRMATION MODAL
    // ─────────────────────────────────────────────────────────────────
    private void BuildResetModal(Transform parent)
    {
        _resetModal = UIHelper.MakeRect("ResetModal", parent);
        UIHelper.Stretch(_resetModal, 0, 0, 0, 0);

        var blocker = _resetModal.gameObject.AddComponent<Image>();
        blocker.color = new Color(0, 0, 0, 0.70f);

        var box = UIHelper.MakeRect("Box", _resetModal);
        box.anchorMin = new Vector2(0.1f, 0.38f);
        box.anchorMax = new Vector2(0.9f, 0.62f);
        box.offsetMin = Vector2.zero;
        box.offsetMax = Vector2.zero;

        var boxImg = box.gameObject.AddComponent<Image>();
        boxImg.color = UIColors.AROverlayDark;
        boxImg.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(boxImg, 26);

        var vlg = box.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(32, 32, 28, 28);
        vlg.spacing = 18;
        vlg.childForceExpandWidth = true;

        var title = UIHelper.MakeLabel("Title", box, "Restart this scenario?", 28, Color.white, TextAlignmentOptions.Center, bold: true);
        UIHelper.SetLayout(title.gameObject, preferredHeight: 40);

        var desc = UIHelper.MakeLabel("Desc", box,
            "Your scenario placement and training actions will be reset to step 1.",
            18, Hex("#CBD5E1"), TextAlignmentOptions.Center, wrap: true);
        UIHelper.SetLayout(desc.gameObject, preferredHeight: 48);

        var btnRow = UIHelper.MakeHorizontal("BtnRow", box, 16);
        UIHelper.SetLayout(btnRow.gameObject, preferredHeight: 56);

        _btnResetCancel = UIHelper.MakeButton("btn-cancel", btnRow, "Cancel", 20, new Color(1, 1, 1, 0.16f), Color.white, 16);
        UIHelper.SetLayout(_btnResetCancel.gameObject, flexibleWidth: true, flexWidth: 1);
        _btnResetCancel.onClick.AddListener(() => _resetModal.gameObject.SetActive(false));

        _btnResetConfirm = UIHelper.MakeButton("btn-confirm", btnRow, "Restart", 20, UIColors.Danger, Color.white, 16);
        UIHelper.SetLayout(_btnResetConfirm.gameObject, flexibleWidth: true, flexWidth: 1);
        _btnResetConfirm.onClick.AddListener(() =>
        {
            _resetModal.gameObject.SetActive(false);
            _resetConfirmCallback?.Invoke();
        });

        _resetModal.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────
    //  10. COMPLETION PANEL (Outro)
    // ─────────────────────────────────────────────────────────────────
    private void BuildCompletionPanel(Transform parent)
    {
        _completionPanel = UIHelper.MakeRect("CompletionPanel", parent);
        _completionPanel.anchorMin = new Vector2(0.06f, 0.20f);
        _completionPanel.anchorMax = new Vector2(0.94f, 0.80f);
        _completionPanel.offsetMin = Vector2.zero;
        _completionPanel.offsetMax = Vector2.zero;

        var img = _completionPanel.gameObject.AddComponent<Image>();
        img.color = UIColors.AROverlayDark;
        img.sprite = UIHelper.GetWhiteSprite();
        UIHelper.SetImageRoundedSprite(img, 30);

        var vlg = _completionPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(32, 32, 36, 32);
        vlg.spacing = 18;
        vlg.childForceExpandWidth = true;

        var iconBox = UIHelper.MakeLabel("CheckIcon", _completionPanel, "COMPLETED", 36, UIColors.SafetyGreen, TextAlignmentOptions.Center, bold: true);
        UIHelper.SetLayout(iconBox.gameObject, preferredHeight: 52);

        var title = UIHelper.MakeLabel("Title", _completionPanel, "SCENARIO COMPLETED", 30, Color.white, TextAlignmentOptions.Center, bold: true);
        UIHelper.SetLayout(title.gameObject, preferredHeight: 40);

        _completionBody = UIHelper.MakeLabel("Body", _completionPanel, "You have successfully contained the fire and followed evacuation protocol.", 20, Hex("#E2E8F0"), TextAlignmentOptions.Center, wrap: true);
        UIHelper.SetLayout(_completionBody.gameObject, flexibleHeight: true, flexHeight: 1);

        var btnCol = UIHelper.MakeVertical("BtnCol", _completionPanel, 12);

        _btnHome = UIHelper.MakeButton("btn-continue-assessment", btnCol, "Proceed to Assessment →", 22, UIColors.SafetyGreen, Color.white, 18);
        UIHelper.SetLayout(_btnHome.gameObject, preferredHeight: 62);
        _btnHome.onClick.AddListener(() => _homeCallback?.Invoke());

        _btnRetry = UIHelper.MakeButton("btn-retry-training", btnCol, "Practice Again", 20, new Color(1, 1, 1, 0.16f), Color.white, 18);
        UIHelper.SetLayout(_btnRetry.gameObject, preferredHeight: 54);
        _btnRetry.onClick.AddListener(() => _retryCallback?.Invoke());

        _completionPanel.gameObject.SetActive(false);
    }

    // =================================================================
    //  PUBLIC API FOR SCENARIO & GUIDANCE CONTROL
    // =================================================================

    /// <summary>Sets dynamic training guidance matching Figma step cards.</summary>
    public void ShowGuidance(string stepBadge, string title, string body, string hint,
        string ctaLabel, UnityAction onCtaClick, UnityAction onListenClick = null)
    {
        if (IsAssessmentMode) return; // Suppress in assessment mode!

        if (_guidanceCard != null)
        {
            _guidanceStepBadge.text = stepBadge;
            _guidanceTitle.text = title;
            _guidanceBody.text = body;
            _guidanceHintText.text = hint;
            _actionBtnText.text = ctaLabel;

            _currentActionCallback = onCtaClick;
            _currentListenCallback = onListenClick;

            _hintBoxGO.SetActive(false); // Collapsed initially
            _guidanceCard.gameObject.SetActive(true);
        }

        if (_messageCard != null) _messageCard.gameObject.SetActive(false);
        if (_hintBanner != null) _hintBanner.gameObject.SetActive(false);
    }

    public void HideGuidance()
    {
        if (_guidanceCard != null) _guidanceCard.gameObject.SetActive(false);
    }

    /// <summary>Overload for dynamic training guidance with named parameters.</summary>
    public void ShowGuidance(string stepTag, string title, string description, string hint = null,
        string actionBtnText = null, UnityAction onActionClicked = null)
    {
        ShowGuidance(stepTag, title, description, hint, actionBtnText, onActionClicked, null);
    }

    /// <summary>Overload for feedback with type first.</summary>
    public void ShowFeedback(FeedbackType type, string title, string message, float duration = 2.5f)
    {
        string full = string.IsNullOrEmpty(message) ? title : $"{title}: {message}";
        ShowFeedback(full, type, 0);
    }

    /// <summary>Shows prominent feedback banner for Correct, Wrong, Unsafe, or Critical action.</summary>
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
                _feedbackBg.color = UIColors.Warning;
                _feedbackText.text = $"✕  {message}  {(scoreDelta != 0 ? $"{scoreDelta}" : "")}";
                break;
            case FeedbackType.Unsafe:
                _feedbackBg.color = UIColors.SafetyOrange;
                _feedbackText.text = $"⚠️  {message}  {(scoreDelta != 0 ? $"{scoreDelta}" : "")}";
                break;
            case FeedbackType.Critical:
                _feedbackBg.color = UIColors.Danger;
                _feedbackText.text = $"🚨  CRITICAL ERROR: {message}";
                break;
        }

        _feedbackBanner.gameObject.SetActive(true);

        if (_feedbackRoutine != null) StopCoroutine(_feedbackRoutine);
        _feedbackRoutine = StartCoroutine(AutoHideFeedback(2.6f));
    }

    /// <summary>
    /// Shows an auto-dismissing glassmorphic toast banner in AR view.
    /// Hides guidance card during display, waits for duration, then hides itself and triggers onDismiss.
    /// </summary>
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
        {
            _toastBox.gameObject.SetActive(false);
        }

        onDismiss?.Invoke();
    }

    private IEnumerator AutoHideFeedback(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_feedbackBanner != null) _feedbackBanner.gameObject.SetActive(false);
    }

    /// <summary>Updates live Score and shows floating delta.</summary>
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

    /// <summary>Updates timer in mm:ss format.</summary>
    public void SetTimer(float elapsedSeconds)
    {
        if (_timerText == null) return;
        int min = Mathf.FloorToInt(elapsedSeconds / 60f);
        int sec = Mathf.FloorToInt(elapsedSeconds % 60f);
        _timerText.text = $"{min:00}:{sec:00}";
    }

    /// <summary>Switches between Training Mode (guided) and Assessment Mode (independent test).</summary>
    public void SetAssessmentMode(bool isAssessment)
    {
        IsAssessmentMode = isAssessment;
        if (isAssessment)
        {
            HideGuidance();
            HideHint();
            if (_stepCounterText != null) _stepCounterText.text = "Assessment";
        }
    }

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

    // =================================================================
    //  BACKWARD-COMPATIBLE API (used by FireScenarioFlowManager)
    // =================================================================

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
        if (IsAssessmentMode) return;
        if (_hintBanner != null)
        {
            _hintBannerText.text = message;
            _hintBanner.gameObject.SetActive(true);
        }
    }

    public void HideHint()
    {
        if (_hintBanner != null) _hintBanner.gameObject.SetActive(false);
    }

    public void ShowStep(int current, int total)
    {
        if (_stepCounterText != null)
        {
            _stepCounterText.text = $"Step {current} / {total}";
        }

        if (_topProgressFill != null && total > 0)
        {
            float fillPct = Mathf.Clamp01((float)current / total);
            _topProgressFill.anchorMax = new Vector2(fillPct, 1f);
        }
    }

    public void HideStep()
    {
        // Keep top bar visible for continuity
    }

    public void ShowProgress(float value, string label)
    {
        if (_sprayProgressBox == null) return;
        _sprayProgressBox.gameObject.SetActive(true);
        _sprayProgressText.text = label;
        _sprayProgressFill.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
    }

    public void HideProgress()
    {
        if (_sprayProgressBox != null) _sprayProgressBox.gameObject.SetActive(false);
    }

    public void ShowActionButton(string label, UnityAction onClick)
    {
        if (_btnAction == null) return;
        _actionBtnText.text = label;
        _currentActionCallback = onClick;
        if (_guidanceCard != null) _guidanceCard.gameObject.SetActive(true);
    }

    public void HideActionButton()
    {
        // handled dynamically
    }

    public void ShowCompletion(string body, UnityAction retry, UnityAction home)
    {
        if (_completionPanel == null) return;
        _completionBody.text = body;
        _retryCallback = retry;
        _homeCallback = home;
        _completionPanel.gameObject.SetActive(true);
        HasCompletionPanel = true;
    }

    public void HideCompletion()
    {
        if (_completionPanel != null) _completionPanel.gameObject.SetActive(false);
        HasCompletionPanel = false;
    }

    // =================================================================
    //  INTERNAL EVENT LISTENERS
    // =================================================================
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

    private void HandleResetScenarioClicked()
    {
        if (OnResetClicked != null)
        {
            OnResetClicked.Invoke();
        }
        else
        {
            ShowResetConfirmation(() =>
            {
                var flow = FindAnyObjectByType<FireScenarioFlowManager>();
                if (flow != null)
                {
                    flow.RestartScenario();
                }
            });
        }
    }

    /// <summary>Displays the scenario reset confirmation modal.</summary>
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

    /// <summary>Updates title and step indicator on top bar.</summary>
    public void SetModuleInfo(string title, int currentStep, int totalSteps)
    {
        if (_moduleTitleText != null) _moduleTitleText.text = title;
        ShowStep(currentStep, totalSteps);
    }

    /// <summary>Overload for scenario completion dialog.</summary>
    public void ShowCompletion(string title, string message, int score, string timeTaken, System.Action onContinue)
    {
        if (_completionPanel == null) return;
        var titleTmp = _completionPanel.Find("Title")?.GetComponent<TextMeshProUGUI>();
        if (titleTmp != null) titleTmp.text = title;
        if (_completionBody != null)
        {
            _completionBody.text = $"{message}\n\nFinal Score: {score}%   •   Time: {timeTaken}";
        }
        _homeCallback = () => onContinue?.Invoke();
        _completionPanel.gameObject.SetActive(true);
        HasCompletionPanel = true;

        if (SurakshaAR.Core.AudioManager.Instance != null)
        {
            SurakshaAR.Core.AudioManager.Instance.PlayCompletion();
        }
    }

    private void OnHintClicked()
    {
        if (_hintBoxGO != null)
        {
            _hintBoxGO.SetActive(!_hintBoxGO.activeSelf);
        }
    }

    private void OnListenClicked()
    {
        _currentListenCallback?.Invoke();
        AudioManager.Instance?.PlayInstructionVoice(_guidanceTitle.text + ". " + _guidanceBody.text);
    }

    private void OnPrimaryActionClicked()
    {
        _currentActionCallback?.Invoke();
    }

    private static Color Hex(string hex) => UIColors.Hex(hex);
}
