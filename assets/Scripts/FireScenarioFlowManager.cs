using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using SurakshaAR.Core;
using SurakshaAR.UI;
using SurakshaAR.Data;

/// <summary>
/// FireScenarioFlowManager
/// ========================
/// Upgraded lead flow controller for SurakshaAR Fire & Explosion Response module.
/// 
/// Implements the official 7-Step SOP training & assessment sequence:
///   1. Identify Fire Hazard
///   2. Activate Fire Alarm
///   3. Select Correct Extinguisher (CO2 / Electrical safe)
///   4. Remove Safety Pin
///   5. Aim at Base of Fire
///   6. Extinguish the Fire (Sweep & Spray)
///   7. Safe Evacuation / Emergency Exit
///
/// Dual-input architecture:
///   - Direct 3D AR interactions (tapping extinguisher, pin, fire, alarm)
///   - High-contrast accessible UGUI CTA buttons on guidance cards
///
/// Features:
///   - Real-time Score HUD with dynamic deltas (+10 correct, -5 wrong, -10 unsafe)
///   - Real-time timer in mm:ss format
///   - Dynamic UGUI feedback banners for Correct / Wrong / Unsafe / Critical
///   - Clean scenario reset without breaking AR tracking
///   - Seamless transition into Assessment & Certification
/// </summary>
public class FireScenarioFlowManager : MonoBehaviour
{
    public enum Stage
    {
        Intro,
        Scanning,
        Step1_IdentifyHazard,
        Step2_ActivateAlarm,
        Step3_SelectExtinguisher,
        Step4_RemovePin,
        Step5_AimBase,
        Step6_Extinguish,
        Step7_Evacuate,
        Timeout,
        MoveToExit,
        Success,
        Complete
    }

    [Header("Cross References (Preserved for Unity Scene Serializations)")]
    public GameObject fireScenario;
    public FireScenarioARPlacement placement;
    public ARPlacement arPlacement;
    public ExtinguisherDisplayPickup displayPickup;
    public ExtinguisherPickup originalPickup;
    public FirePinInteraction pinInteraction;
    public ExtinguisherGripInteraction gripInteraction;
    public FireExtinguishable fire;
    public AlarmInteraction alarmInteraction;
    public FireScenarioUIController ui;

    [Header("Mode Configuration")]
    public bool isAssessmentMode = false;

    [Header("Scoring")]
    public int currentScore = 0;
    public int correctActions = 0;
    public int wrongActions = 0;
    public int unsafeActions = 0;
    public int criticalErrors = 0;

    public int CurrentScore => currentScore;
    public int CorrectActionsCount => correctActions;
    public int WrongActionsCount => wrongActions;
    public int UnsafeActionsCount => unsafeActions;
    public int CriticalErrorsCount => criticalErrors;

    [Header("Timing")]
    public float scenarioTimer = 0f;
    [Tooltip("Overall training time limit in seconds (420 = 7 minutes). Worker is guided to exit after this.")] 
    public float maxTrainingTimeSeconds = 420f;
    private bool isTimerRunning = false;
    private bool timeoutTriggered = false;

    public Stage CurrentStage => stage;

    public event Action<Stage> OnStageChanged;

    private void SetStage(Stage newStage)
    {
        stage = newStage;
        Debug.Log($"[UI-STEP] Current Stage = {newStage}");
        OnStageChanged?.Invoke(newStage);
    }

    private Stage stage = Stage.Intro;
    private bool subscribed;
    private bool _scenarioPlacedHandled = false;
    private Coroutine messageRoutine;
    private float hazardLookTimer = 0f;
    private float aimBaseLookTimer = 0f;
    private bool _wasSprayingOffTarget = false;

    private FireAssessmentAdapter EnsureFireAdapter()
    {
        if (FireAssessmentAdapter.Instance != null) return FireAssessmentAdapter.Instance;
        var existing = FindAnyObjectByType<FireAssessmentAdapter>(FindObjectsInactive.Include);
        if (existing != null) return existing;
        var go = new GameObject("FireAssessmentAdapter");
        if (!Application.isPlaying) go.hideFlags = HideFlags.DontSave;
        return go.AddComponent<FireAssessmentAdapter>();
    }

    /// <summary>
    /// Public wrapper so ExtinguisherSprayCollision (which has no FlowManager reference) can
    /// access the FireAssessmentAdapter without creating a second instance.
    /// </summary>
    public FireAssessmentAdapter EnsureFireAdapterPublic() => EnsureFireAdapter();

    public static FireScenarioFlowManager Instance { get; private set; }

    // =====================================================
    // LIFECYCLE
    // =====================================================

    private void Awake()
    {
        Instance = this;
        ResolveReferences();
        ResetScenarioState();
        EnsureInitialExtinguisherVisibility();
    }

    public void EnsureInitialExtinguisherVisibility()
    {
        // If we are in active operational steps (Step 4 to Step 7) and original extinguisher is held, do not detach or hide it
        if (stage >= Stage.Step4_RemovePin && stage <= Stage.Step7_Evacuate && originalPickup != null && originalPickup.IsHeld())
        {
            return;
        }

        if (displayPickup == null)
            displayPickup = FindAnyObjectByType<ExtinguisherDisplayPickup>(FindObjectsInactive.Include);

        if (originalPickup == null)
            originalPickup = FindAnyObjectByType<ExtinguisherPickup>(FindObjectsInactive.Include);

        // Ensure display extinguisher is active, visible, and reset
        if (displayPickup != null)
        {
            displayPickup.gameObject.SetActive(true);
            displayPickup.ResetDisplay();
        }

        // Ensure original operational FireExt is hidden and not interactable
        if (originalPickup != null)
        {
            if (originalPickup.IsHeld())
            {
                originalPickup.DetachFromCamera();
            }
            originalPickup.SetRuntimeVisibility(false);
        }
    }

    public void ResetScenarioState()
    {
        currentScore = 0;
        correctActions = 0;
        wrongActions = 0;
        unsafeActions = 0;
        criticalErrors = 0;
        scenarioTimer = 0f;
        isTimerRunning = false;
        timeoutTriggered = false;
        _lastPenaltyTimes.Clear();
    }

    public void InitializeForTraining()
    {
        if (Instance == null) Instance = this;
        ResolveReferences();
        SubscribeEvents();
        ResetScenarioState();
        EnsureInitialExtinguisherVisibility();
    }

    private void Start()
    {
        if (Instance == null) Instance = this;
        ResolveReferences();
        BuildUI();
        SubscribeEvents();
        EnsureInitialExtinguisherVisibility();

#if UNITY_EDITOR
        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = new Color(0.10f, 0.14f, 0.20f, 1f);
        }
#endif

        bool alreadyPlaced = (arPlacement != null && arPlacement.IsScenarioPlaced) ||
                             (placement != null && placement.IsPlaced);

        if (alreadyPlaced)
        {
            HandleScenarioPlaced();
        }
        else
        {
            BeginIntro();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        UnsubscribeEvents();
    }

    private void Update()
    {
        UpdateTimer();
        UpdateHazardDetection();
        UpdateAimBaseDetection();
        UpdateSprayProgress();
        CheckARTrackingStatus();
    }

    // =====================================================
    // TIMER & SCORING
    // =====================================================

    private void UpdateTimer()
    {
        if (!isTimerRunning) return;

        scenarioTimer += Time.deltaTime;

        if (ui != null)
        {
            // Overall training stopwatch timer (00:00 to 07:00 / 420s)
            ui.SetTimer(scenarioTimer);
        }

        // Trigger timeout once when time runs out (420 seconds)
        if (!timeoutTriggered && scenarioTimer >= maxTrainingTimeSeconds)
        {
            timeoutTriggered = true;
            TransitionToTimeout();
        }
    }

    /// <param name="isCorrectAction">
    /// When true (default) increments correctActions counter.
    /// Pass false for score-only adjustments (e.g. grip aim bonus) that should not inflate the correct-action count.
    /// </param>
    public void AddScore(int points, string reason = null, bool isCorrectAction = true)
    {
        currentScore = Mathf.Clamp(currentScore + points, 0, 100);
        if (isCorrectAction) correctActions++;
        if (ui != null)
        {
            ui.SetScore(currentScore, points);
            if (!string.IsNullOrEmpty(reason))
            {
                ui.ShowFeedback(FireScenarioUIController.FeedbackType.Correct, "✓ " + reason, "+" + points + " Points");
            }
        }
    }

    public void DeductScore(int points, string reason, bool isUnsafe = false)
    {
        currentScore = Mathf.Clamp(currentScore - points, 0, 100);
        if (isUnsafe)
        {
            unsafeActions++;
            EnsureFireAdapter().RecordUnsafeAction("unsafe_action", reason);
        }
        else
        {
            wrongActions++;
            EnsureFireAdapter().RecordWrongAction("wrong_action", reason, "minor");
        }

        if (ui != null)
        {
            ui.SetScore(currentScore, -points);
            FireScenarioUIController.FeedbackType fType = isUnsafe
                ? FireScenarioUIController.FeedbackType.Unsafe
                : FireScenarioUIController.FeedbackType.Wrong;

            string title = isUnsafe ? "⚠ Unsafe Action (-" + points + ")" : "❌ Incorrect Action (-" + points + ")";
            ui.ShowFeedback(fType, title, reason, 3.0f);
        }
    }

    // --- DETERMINISTIC PENALTY RATE LIMITING ---
    private readonly System.Collections.Generic.Dictionary<string, float> _lastPenaltyTimes =
        new System.Collections.Generic.Dictionary<string, float>();
    private const float PenaltyCooldownSeconds = 1.2f;

    public bool CanApplyPenalty(string penaltyKey)
    {
        float now = Time.time;
        if (_lastPenaltyTimes.TryGetValue(penaltyKey, out float lastTime))
        {
            if (now - lastTime < PenaltyCooldownSeconds)
                return false;
        }
        _lastPenaltyTimes[penaltyKey] = now;
        return true;
    }

    public void ResetPenaltyCooldowns()
    {
        _lastPenaltyTimes.Clear();
    }

    public void HandlePrematureAlarmAttempt()
    {
        if (stage == Stage.Step2_ActivateAlarm || stage == Stage.Complete || stage == Stage.Timeout) return;
        if (!CanApplyPenalty("premature_alarm")) return;

        string msg = GetLoc("fire.feedback.identifyHazardFirst", "Identify the fire hazard first.");
        DeductScore(5, msg, isUnsafe: false);
        EnsureFireAdapter().RecordSequenceError("identify_hazard", "activate_alarm");
    }

    public void HandlePrematureExtinguisherAttempt()
    {
        EnsureInitialExtinguisherVisibility();
        if (stage == Stage.Step3_SelectExtinguisher || stage == Stage.Complete || stage == Stage.Timeout) return;
        if (!CanApplyPenalty("premature_extinguisher")) return;

        if (stage == Stage.Step1_IdentifyHazard || stage == Stage.Intro || stage == Stage.Scanning)
        {
            string msg = GetLoc("fire.feedback.identifyHazardFirst", "Identify the fire hazard first.");
            DeductScore(5, msg, isUnsafe: false);
            EnsureFireAdapter().RecordSequenceError("identify_hazard", "select_extinguisher");
        }
        else if (stage == Stage.Step2_ActivateAlarm)
        {
            // CRITICAL RULE (Section 4): tapping extinguisher before alarm is an UNSAFE action (-10 marks)
            string msg = GetLoc("fire.feedback.activateAlarmFirst", "Activate the fire alarm before selecting the extinguisher.");
            DeductScore(10, msg, isUnsafe: true);
            EnsureFireAdapter().RecordUnsafeAction("premature_extinguisher_attempt", msg);
            EnsureFireAdapter().RecordSequenceError("activate_alarm", "select_extinguisher");
        }
    }

    public void HandlePrematurePinAttempt()
    {
        if (stage == Stage.Step4_RemovePin || stage == Stage.Complete || stage == Stage.Timeout) return;
        if (!CanApplyPenalty("premature_pin")) return;

        string msg = stage == Stage.Step1_IdentifyHazard
            ? GetLoc("fire.feedback.identifyHazardFirst", "Identify the fire hazard first.")
            : (stage == Stage.Step2_ActivateAlarm
                ? GetLoc("fire.feedback.activateAlarmFirst", "Activate the fire alarm before selecting the extinguisher.")
                : GetLoc("fire.feedback.selectExtinguisherFirst", "Select the CO2 extinguisher first."));

        DeductScore(5, msg, isUnsafe: false);
        EnsureFireAdapter().RecordSequenceError("select_extinguisher", "remove_pin");
    }

    public void HandlePrematureGripAttempt()
    {
        if (stage == Stage.Complete || stage == Stage.Timeout) return;
        if (!CanApplyPenalty("premature_grip")) return;

        // CRITICAL RULE (Section 6): Attempting grip before pin removal is an UNSAFE action (-10 marks)
        string msg = GetLoc("fire.feedback.removePinBeforeHandle", "Remove the safety pin before using the handle.");
        DeductScore(10, msg, isUnsafe: true);
        EnsureFireAdapter().RecordUnsafeAction("lever_squeezed_before_pin", msg);
        EnsureFireAdapter().RecordSequenceError("remove_pin", "grip_handle");
    }

    public void HandleInvalidAim()
    {
        if (!CanApplyPenalty("invalid_aim")) return;

        string msg = GetLoc("fire.feedback.aimAtBase", "Aim the nozzle at the base of the fire.");
        DeductScore(5, msg, isUnsafe: false);
        EnsureFireAdapter().RecordAimAtBase(false);
        // Raise named telemetry event so assessment pipeline captures invalid_aim
        EnsureFireAdapter().RecordInvalidAim();
    }

    /// <summary>
    /// Handles a premature spray attempt (spray before safety pin is removed).
    /// Records as unsafe action, deducts score, raises telemetry. No spray proceeds.
    /// </summary>
    public void HandlePrematureSprayAttempt()
    {
        if (stage == Stage.Step6_Extinguish || stage == Stage.Complete || stage == Stage.Timeout) return;
        if (!CanApplyPenalty("premature_spray")) return;

        string msg = GetLoc("fire.feedback.prematureSpray", "Remove the safety pin before spraying.");
        DeductScore(10, msg, isUnsafe: true);
        EnsureFireAdapter().RecordPrematureSprayAttempt();
    }


    // =====================================================
    // REFERENCE RESOLUTION
    // =====================================================

    private void ResolveReferences()
    {
        if (fireScenario == null)
        {
            GameObject found = GameObject.Find("FireScenario");
            if (found != null) fireScenario = found;
        }

        if (placement == null)
        {
            placement = FindAnyObjectByType<FireScenarioARPlacement>();
        }

        if (arPlacement == null)
        {
            arPlacement = FindAnyObjectByType<ARPlacement>();
        }

        if (fireScenario != null)
        {
            if (displayPickup == null)
                displayPickup = fireScenario.GetComponentInChildren<ExtinguisherDisplayPickup>(true);

            if (originalPickup == null)
                originalPickup = fireScenario.GetComponentInChildren<ExtinguisherPickup>(true);

            if (fire == null)
                fire = fireScenario.GetComponentInChildren<FireExtinguishable>(true);

            // Alarm interaction — looks for AlarmInteraction anywhere on the scenario root
            if (alarmInteraction == null)
                alarmInteraction = fireScenario.GetComponentInChildren<AlarmInteraction>(true);

            if (alarmInteraction == null)
            {
                var alignment = fireScenario.GetComponentInChildren<FireScenarioAlignment>(true);
                if (alignment != null && alignment.fireAlarm != null)
                {
                    if (alignment.fireAlarm.GetComponent<Collider>() == null)
                        alignment.fireAlarm.gameObject.AddComponent<BoxCollider>();

                    alarmInteraction = alignment.fireAlarm.GetComponent<AlarmInteraction>()
                                       ?? alignment.fireAlarm.gameObject.AddComponent<AlarmInteraction>();
                }
            }

            if (pinInteraction == null)
            {
                FirePinInteraction[] allPins = fireScenario.GetComponentsInChildren<FirePinInteraction>(true);
                foreach (FirePinInteraction pin in allPins)
                {
                    if (originalPickup != null && pin.transform.IsChildOf(originalPickup.transform))
                    {
                        pinInteraction = pin;
                        break;
                    }
                }
                if (pinInteraction == null && allPins.Length > 0)
                {
                    pinInteraction = allPins[0];
                }
            }

            if (gripInteraction == null)
            {
                ExtinguisherGripInteraction[] allGrips = fireScenario.GetComponentsInChildren<ExtinguisherGripInteraction>(true);
                foreach (ExtinguisherGripInteraction grip in allGrips)
                {
                    if (originalPickup != null && grip.transform.IsChildOf(originalPickup.transform))
                    {
                        gripInteraction = grip;
                        break;
                    }
                }
                if (gripInteraction == null && allGrips.Length > 0)
                {
                    gripInteraction = allGrips[0];
                }
            }
        }

        // Scene-wide fallback resolution
        if (displayPickup == null)
            displayPickup = FindAnyObjectByType<ExtinguisherDisplayPickup>(FindObjectsInactive.Include);

        if (originalPickup == null)
            originalPickup = FindAnyObjectByType<ExtinguisherPickup>(FindObjectsInactive.Include);

        if (fire == null)
            fire = FindAnyObjectByType<FireExtinguishable>();

        if (fire != null)
        {
            fire.OnExtinguished.RemoveListener(HandleFireExtinguished);
            fire.OnExtinguished.AddListener(HandleFireExtinguished);
        }

        if (gripInteraction == null)
            gripInteraction = FindAnyObjectByType<ExtinguisherGripInteraction>();

        if (pinInteraction == null)
            pinInteraction = FindAnyObjectByType<FirePinInteraction>();

        if (alarmInteraction == null)
            alarmInteraction = FindAnyObjectByType<AlarmInteraction>();

        if (alarmInteraction == null)
        {
            var alarmGO = GameObject.Find("FireAlarm") ?? GameObject.Find("Alarm");
            if (alarmGO != null)
            {
                alarmInteraction = alarmGO.GetComponent<AlarmInteraction>() ?? alarmGO.AddComponent<AlarmInteraction>();
            }
            else
            {
                var target = fireScenario != null ? fireScenario.transform : transform;
                var newAlarm = new GameObject("FireAlarm");
                newAlarm.transform.SetParent(target, false);
                newAlarm.transform.localPosition = new Vector3(0, 1.2f, 0.5f);
                var box = newAlarm.AddComponent<BoxCollider>();
                box.size = new Vector3(0.5f, 0.5f, 0.5f);
                alarmInteraction = newAlarm.AddComponent<AlarmInteraction>();
            }
        }
    }

    // =====================================================
    // UI CREATION & HOOKS
    // =====================================================

    private void BuildUI()
    {
        if (ui == null)
            ui = GetComponentInChildren<FireScenarioUIController>(true) ?? FindAnyObjectByType<FireScenarioUIController>(FindObjectsInactive.Include);

        if (ui != null)
        {
            WireUICallbacks();
            return;
        }

        GameObject uiGO = new GameObject("FireScenarioUGUI");
        uiGO.transform.SetParent(transform, false);
        ui = uiGO.AddComponent<FireScenarioUIController>();
        WireUICallbacks();
    }

    private void WireUICallbacks()
    {
        if (ui == null) return;

        // Wire top bar button callbacks
        ui.OnBackClicked = () =>
        {
            if (ARModuleLauncher.Instance != null)
            {
                ARModuleLauncher.Instance.ExitCurrentARScene();
            }
        };

        ui.OnResetClicked = () =>
        {
            ui.ShowResetDialog(
                onConfirm: RestartScenario,
                onCancel: null
            );
        };
    }

    // =====================================================
    // EVENT SUBSCRIPTIONS
    // =====================================================

    public void NotifyScenarioPlacedByAR()
    {
        HandleScenarioPlaced();
    }

    private void HandleScenarioPlacedFromEvent()
    {
        HandleScenarioPlaced();
    }

    private void SubscribeEvents()
    {
        if (subscribed) return;

        if (placement != null)
            placement.OnScenarioPlaced.AddListener(HandleScenarioPlaced);

        if (arPlacement != null)
            arPlacement.OnScenarioPlaced.AddListener(HandleScenarioPlaced);

        if (alarmInteraction != null)
            alarmInteraction.OnAlarmActivated.AddListener(HandleAlarmActivated);

        if (displayPickup != null)
            displayPickup.OnPickedUp.AddListener(HandleExtinguisherPickedUp);

        if (pinInteraction != null)
            pinInteraction.OnPinRemoved.AddListener(HandlePinRemoved);

        if (gripInteraction != null)
        {
            gripInteraction.OnGripActivated.AddListener(HandleGripActivated);
            gripInteraction.OnSprayStarted.AddListener(HandleSprayStarted);
            gripInteraction.OnSprayStopped.AddListener(HandleSprayStopped);
            gripInteraction.OnPinRemovalRequired.AddListener(HandlePinRemovalRequired);
        }

        if (fire != null)
            fire.OnExtinguished.AddListener(HandleFireExtinguished);

        if (SurakshaAR.Localization.LocalizationManager.Instance != null)
            SurakshaAR.Localization.LocalizationManager.Instance.OnLanguageChanged += HandleLanguageChanged;

        subscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!subscribed) return;

        if (SurakshaAR.Localization.LocalizationManager.Instance != null)
            SurakshaAR.Localization.LocalizationManager.Instance.OnLanguageChanged -= HandleLanguageChanged;

        if (placement != null)
            placement.OnScenarioPlaced.RemoveListener(HandleScenarioPlaced);

        if (arPlacement != null)
            arPlacement.OnScenarioPlaced.RemoveListener(HandleScenarioPlaced);

        if (alarmInteraction != null)
            alarmInteraction.OnAlarmActivated.RemoveListener(HandleAlarmActivated);

        if (displayPickup != null)
            displayPickup.OnPickedUp.RemoveListener(HandleExtinguisherPickedUp);

        if (pinInteraction != null)
            pinInteraction.OnPinRemoved.RemoveListener(HandlePinRemoved);

        if (gripInteraction != null)
        {
            gripInteraction.OnGripActivated.RemoveListener(HandleGripActivated);
            gripInteraction.OnSprayStarted.RemoveListener(HandleSprayStarted);
            gripInteraction.OnSprayStopped.RemoveListener(HandleSprayStopped);
            gripInteraction.OnPinRemovalRequired.RemoveListener(HandlePinRemovalRequired);
        }

        if (fire != null)
            fire.OnExtinguished.RemoveListener(HandleFireExtinguished);

        subscribed = false;
    }

    private void HandleLanguageChanged(AppLanguage newLang)
    {
        if (ui != null)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ApplyLanguageFonts(ui.gameObject, newLang);
            }
            else
            {
                var font = UIHelper.GetFontForLanguage(newLang);
                if (font != null)
                {
                    foreach (var tmp in ui.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
                    {
                        tmp.font = font;
                    }
                }
            }
        }
        RefreshCurrentStageGuidance();
    }

    public void RefreshCurrentStageGuidance()
    {
        if (ui == null) return;
        switch (stage)
        {
            case Stage.Intro:
                ui.SetModuleInfo(GetLoc("module.fire.title", "Fire & Explosion Response"), 0, 6);
                ui.ShowGuidance(
                    stepTag: "SURAKSHAAR AR",
                    title: GetLoc("fire.intro.title", "Industrial Fire Response Training"),
                    description: GetLoc("fire.intro.desc", "In this scenario, an electrical equipment fire breaks out in a mining facility.\nFollow standard operating procedures (SOP) to safely respond and evacuate."),
                    hint: GetLoc("fire.intro.hint", "Scan the floor and tap the reticle to anchor the 3D training scenario."),
                    actionBtnText: GetLoc("fire.intro.action", "Tap to Start AR Training"),
                    onActionClicked: BeginScanning
                );
                break;
            case Stage.Scanning:
                ui.SetModuleInfo(GetLoc("module.fire.title", "Fire & Explosion Response"), 0, 6);
                ui.ShowGuidance(
                    stepTag: "SURFACE SCAN",
                    title: GetLoc("fire.scan.title", "Find a Flat Surface"),
                    description: GetLoc("fire.scan.desc", "Move your phone slowly to scan the ground.\nWhen the placement reticle appears, tap anywhere to place the industrial scenario."),
                    hint: GetLoc("fire.scan.hint", "Ensure adequate ambient lighting for optical feature tracking."),
                    actionBtnText: GetLoc("fire.scan.action", "Simulate Placement"),
                    onActionClicked: () =>
                    {
                        if (arPlacement != null)
                            arPlacement.SimulatePlacement();
                        else
                            HandleScenarioPlaced();
                    }
                );
                break;
            case Stage.Step1_IdentifyHazard:
                TransitionToStep1();
                break;
            case Stage.Step2_ActivateAlarm:
                TransitionToStep2_ActivateAlarm();
                break;
            case Stage.Step3_SelectExtinguisher:
                TransitionToStep3_SelectExtinguisher();
                break;
            case Stage.Step4_RemovePin:
                TransitionToStep3_RemovePin();
                break;
            case Stage.Step5_AimBase:
                TransitionToStep4_AimBase();
                break;
            case Stage.Step6_Extinguish:
                TransitionToStep5_Extinguish();
                break;
            case Stage.Step7_Evacuate:
                TransitionToStep6_Evacuate();
                break;
            case Stage.Timeout:
                TransitionToTimeout();
                break;
        }
    }

    // =====================================================
    // 7-STEP SOP TRAINING FLOW
    // =====================================================

    private string GetLoc(string key, string fallback)
    {
        return SurakshaAR.Localization.LocalizationManager.Instance != null
            ? SurakshaAR.Localization.LocalizationManager.Instance.Get(key)
            : fallback;
    }

    private void BeginIntro()
    {
        SetStage(Stage.Intro);
        isTimerRunning = false;
        scenarioTimer = 0f;
        currentScore = 0;
        correctActions = 0;
        wrongActions = 0;
        unsafeActions = 0;
        criticalErrors = 0;
        _lastPenaltyTimes.Clear();

        if (placement != null)
            placement.SetPlacementActive(false);
        if (arPlacement != null)
            arPlacement.SetPlacementActive(false);

        if (ui != null)
        {
            ui.SetModuleInfo(GetLoc("module.fire.title", "Fire & Explosion Response"), 0, 6);
            ui.SetScore(currentScore, 0);
            ui.SetTimer(0f);
            ui.HideProgress();

            ui.ShowGuidance(
                stepTag: "SURAKSHAAR AR",
                title: GetLoc("fire.intro.title", "Industrial Fire Response Training"),
                description: GetLoc("fire.intro.desc", "In this scenario, an electrical equipment fire breaks out in a mining facility.\nFollow standard operating procedures (SOP) to safely respond and evacuate."),
                hint: GetLoc("fire.intro.hint", "Scan the floor and tap the reticle to anchor the 3D training scenario."),
                actionBtnText: GetLoc("fire.intro.action", "Tap to Start AR Training"),
                onActionClicked: BeginScanning
            );
        }
    }

    private void BeginScanning()
    {
        SetStage(Stage.Scanning);

        if (ui != null)
        {
            ui.SetModuleInfo(GetLoc("module.fire.title", "Fire & Explosion Response"), 0, 6);
            ui.ShowGuidance(
                stepTag: "SURFACE SCAN",
                title: GetLoc("fire.scan.title", "Find a Flat Surface"),
                description: GetLoc("fire.scan.desc", "Move your phone slowly to scan the ground.\nWhen the placement reticle appears, tap anywhere to place the industrial scenario."),
                hint: GetLoc("fire.scan.hint", "Ensure adequate ambient lighting for optical feature tracking."),
                actionBtnText: GetLoc("fire.scan.action", "Simulate Placement"),
                onActionClicked: () =>
                {
                    if (arPlacement != null)
                        arPlacement.SimulatePlacement();
                    else if (placement != null)
                        HandleScenarioPlaced();
                    else
                        HandleScenarioPlaced();
                }
            );

            // Connect bottom bar "Place" button callback
            ui.SetPlacementState(true, () =>
            {
                if (arPlacement != null)
                    arPlacement.SimulatePlacement();
                else
                    HandleScenarioPlaced();
            });
        }

        if (placement != null)
            placement.SetPlacementActive(true);
        if (arPlacement != null)
            arPlacement.SetPlacementActive(true);
    }

    private void HandleScenarioPlaced()
    {
        if (_scenarioPlacedHandled) return;
        if (stage != Stage.Scanning && stage != Stage.Intro) return;
        _scenarioPlacedHandled = true;

        EnsureInitialExtinguisherVisibility();

        if (placement != null)
            placement.SetPlacementActive(false);
        if (arPlacement != null)
            arPlacement.SetPlacementActive(false);

        isTimerRunning = true;
        scenarioTimer = 0f;

        // Step 1: Identify Fire Hazard (sets stage = Stage.Step1_IdentifyHazard)
        TransitionToStep1();

        // Broadcast event safely after stage transition has completed
        TrainingEventManager.RaiseScenarioPlaced();
    }

    // =====================================================
    // 3D AR INTERACTIVE DETECTION
    // =====================================================

    private void UpdateHazardDetection()
    {
        if (stage != Stage.Step1_IdentifyHazard) return;
        if (fire == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 firePos = fire.FireWorldPosition;

        // 1. Check Camera Look / Aim at Fire
        Vector3 toFire = firePos - cam.transform.position;
        float dist = toFire.magnitude;
        if (dist > 0.1f && dist < 7f)
        {
            float angle = Vector3.Angle(cam.transform.forward, toFire.normalized);
            if (angle < 28f)
            {
                hazardLookTimer += Time.deltaTime;
                if (hazardLookTimer >= 1.2f)
                {
                    hazardLookTimer = 0f;
                    OnHazardIdentified();
                    return;
                }
            }
            else
            {
                hazardLookTimer = Mathf.Max(0f, hazardLookTimer - Time.deltaTime);
            }
        }

        // 2. Check Screen Tap on Fire in 3D AR
        bool pressed = false;
        Vector2 screenPos = Vector2.zero;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            pressed = true;
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPos = Mouse.current.position.ReadValue();
            pressed = true;
        }

        if (pressed)
        {
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = cam.ScreenPointToRay(screenPos);
            RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
            foreach (var hit in hits)
            {
                Transform ht = hit.transform;
                string n = ht.name.ToLower();
                if (ht == fire.transform || ht.IsChildOf(fire.transform) ||
                    (fire.FireParticle != null && (ht == fire.FireParticle.transform || ht.IsChildOf(fire.FireParticle.transform))) ||
                    n.Contains("fire") || n.Contains("flame") || n.Contains("hazard") || n.Contains("electric"))
                {
                    OnHazardIdentified();
                    return;
                }
            }

            Vector3 fireScreen = cam.WorldToScreenPoint(firePos);
            if (fireScreen.z > 0 && Vector2.Distance(fireScreen, screenPos) <= 160f)
            {
                OnHazardIdentified();
                return;
            }
        }
    }

    private void UpdateAimBaseDetection()
    {
        if (stage != Stage.Step5_AimBase) return;
        if (fire == null)
        {
            fire = FindAnyObjectByType<FireExtinguishable>(FindObjectsInactive.Include);
            if (fire == null) return;
        }

        bool isSprayingNow = (gripInteraction != null && gripInteraction.IsGripHeld);
        if (!isSprayingNow)
        {
            var p = FindAnyObjectByType<DryPowderSpray>();
            if (p != null && p.IsSpraying()) isSprayingNow = true;
        }

        Camera cam = Camera.main ?? FindAnyObjectByType<Camera>();
        if (cam == null) return;

        Vector3 firePos = fire.FireWorldPosition;
        Vector3 toFire = firePos - cam.transform.position;
        float dist = toFire.magnitude;

        // Also check actual nozzle forward direction if available
        Transform nozzle = gripInteraction != null ? gripInteraction.transform : (originalPickup != null ? originalPickup.transform : null);
        bool nozzleAimed = false;
        if (nozzle != null)
        {
            Vector3 nozzleToFire = firePos - nozzle.position;
            if (nozzleToFire.magnitude < 25f)
            {
                float nozzleAngle = Vector3.Angle(nozzle.forward, nozzleToFire.normalized);
                float revAngle = Vector3.Angle(-nozzle.forward, nozzleToFire.normalized);
                if (Mathf.Min(nozzleAngle, revAngle) < 65f)
                    nozzleAimed = true;
            }
        }

        if (dist > 0.1f && dist < 25f)
        {
            float angle = Vector3.Angle(cam.transform.forward, toFire.normalized);
            Vector3 vp = cam.WorldToViewportPoint(firePos);
            bool inView = (vp.z > 0f && vp.x >= -0.3f && vp.x <= 1.3f && vp.y >= -0.3f && vp.y <= 1.3f);

            if (angle < 55f || inView || nozzleAimed)
            {
                // If user is already pressing handle / spraying towards fire, advance immediately to Step 6
                if (isSprayingNow)
                {
                    aimBaseLookTimer = 0f;
                    OnAimConfirmed();
                    return;
                }

                aimBaseLookTimer += Time.deltaTime;
                if (aimBaseLookTimer >= 0.35f)
                {
                    aimBaseLookTimer = 0f;
                    OnAimConfirmed();
                }
            }
            else
            {
                aimBaseLookTimer = Mathf.Max(0f, aimBaseLookTimer - Time.deltaTime);
            }
        }
    }

    // =====================================================
    // STEP FLOW (Interactive with Timed Toast Transitions)
    // =====================================================

    // --- STEP 1: IDENTIFY HAZARD ---
    public void TransitionToStep1()
    {
        SetStage(Stage.Step1_IdentifyHazard);
        EnsureInitialExtinguisherVisibility();
        hazardLookTimer = 0f;
        EnsureFireAdapter().StartScenario("fire_drill_01");
        if (fire != null)
        {
            var collector = MovementTelemetryCollector.Instance;
            if (collector == null)
            {
                var mgo = new GameObject("MovementTelemetryCollector");
                collector = mgo.AddComponent<MovementTelemetryCollector>();
            }
            if (collector != null)
            {
                collector.StartCollection(fire.transform);
            }
        }
        if (ui == null) return;

        ui.SetModuleInfo(GetLoc("module.fire.title", "Fire & Explosion Response"), 1, 6);
        ui.HideProgress();

        ui.ShowGuidance(
            stepTag: "STEP 1 OF 6",
            title: GetLoc("fire.sop.step1.title", "Identify Fire Hazard"),
            description: GetLoc("fire.sop.step1.desc", "Look around your surroundings to locate the electrical fire. Aim your camera at the flames or tap directly on the fire in AR."),
            hint: GetLoc("fire.sop.step1.hint", "Look for sparks, dark smoke, and electrical panel indicators."),
            actionBtnText: GetLoc("fire.sop.step1.action", "I Have Identified Fire Source"),
            onActionClicked: OnHazardIdentified
        );
    }

    public void OnHazardIdentified()
    {
        if (stage != Stage.Step1_IdentifyHazard) return;

        AddScore(10, "Fire Hazard Identified");
        EnsureFireAdapter().RecordHazardIdentified("electrical_fire", true);
        TrainingEventManager.RaiseHazardIdentified();

        // SOP Step 2: Activate Alarm (transition immediately so workflow state is authoritative)
        TransitionToStep2_ActivateAlarm();

        if (ui != null)
        {
            ui.ShowTransientToast(
                title: GetLoc("fire.toast.hazard.title", "Hazard Identified!"),
                subtitle: GetLoc("fire.toast.hazard.sub", "Activate the fire alarm immediately!"),
                duration: 2.0f
            );
        }
    }

    // --- STEP 2: ACTIVATE FIRE ALARM ---
    public void TransitionToStep2_ActivateAlarm()
    {
        SetStage(Stage.Step2_ActivateAlarm);
        EnsureInitialExtinguisherVisibility();
        if (ui == null) return;

        ui.SetModuleInfo(GetLoc("module.fire.title", "Fire & Explosion Response"), 2, 6);

        ui.ShowGuidance(
            stepTag: "STEP 2 OF 6",
            title: GetLoc("fire.sop.step2.title", "Activate Fire Alarm"),
            description: GetLoc("fire.sop.step2.desc", "Locate the red fire alarm pull station on the wall and tap it to alert all personnel in the facility."),
            hint: GetLoc("fire.sop.step2.hint", "Always alert others before attempting to fight a fire alone. Never skip the alarm."),
            actionBtnText: GetLoc("fire.sop.step2.action", "Activate Fire Alarm"),
            onActionClicked: () =>
            {
                if (alarmInteraction != null)
                    alarmInteraction.ActivateAlarm();
                else
                    HandleAlarmActivated(); // fallback if no 3D alarm in scene
            }
        );
    }

    private void HandleAlarmActivated()
    {
        if (stage != Stage.Step2_ActivateAlarm)
        {
            if (stage == Stage.Step1_IdentifyHazard || stage == Stage.Intro || stage == Stage.Scanning)
            {
                HandlePrematureAlarmAttempt();
            }
            return;
        }

        AddScore(10, "Fire Alarm Activated");
        EnsureFireAdapter().RecordAlarmActivated();

        // SOP Step 3: Select Extinguisher (transition immediately so workflow state is authoritative)
        TransitionToStep3_SelectExtinguisher();

        if (ui != null)
        {
            ui.ShowTransientToast(
                title: GetLoc("fire.toast.alarm.title", "Alarm Activated!"),
                subtitle: GetLoc("fire.toast.alarm.sub", "Personnel alerted. Now locate the extinguisher."),
                duration: 2.0f
            );
        }
    }

    // --- STEP 3: SELECT EXTINGUISHER ---
    public void TransitionToStep3_SelectExtinguisher()
    {
        SetStage(Stage.Step3_SelectExtinguisher);
        EnsureInitialExtinguisherVisibility();
        if (ui == null) return;

        ui.SetModuleInfo(GetLoc("module.fire.title", "Fire & Explosion Response"), 3, 6);

        ui.ShowGuidance(
            stepTag: "STEP 3 OF 6",
            title: GetLoc("fire.sop.step3.title", "Select Correct Extinguisher"),
            description: GetLoc("fire.sop.step3.desc", "Examine the burning equipment. Tap on the CO2 Extinguisher (Black band) in your surroundings to equip it."),
            hint: GetLoc("fire.sop.step3.hint", "DANGER: Never use Water or Foam on live electrical panels! Electrocution hazard."),
            actionBtnText: GetLoc("fire.sop.step3.action", "Equip CO2 Extinguisher"),
            onActionClicked: () =>
            {
                if (displayPickup != null)
                    displayPickup.Pickup();
                else
                    HandleExtinguisherPickedUp();
            }
        );
    }

    // Legacy aliases (backward compatibility for any external callers)
    public void TransitionToStep2_SelectExtinguisher() => TransitionToStep3_SelectExtinguisher();
    public void TransitionToStep2() => TransitionToStep2_ActivateAlarm();
    public void TransitionToStep3() => TransitionToStep3_SelectExtinguisher();

    public void HandleExtinguisherPickedUp()
    {
        if (stage != Stage.Step3_SelectExtinguisher)
        {
            if (stage == Stage.Step4_RemovePin) return; // Cleanly ignore redundant invocation in same frame
            HandlePrematureExtinguisherAttempt();
            return;
        }

        Debug.Log("[FireAR] DISPLAY TAP DETECTED");
        Debug.Log($"[FireAR] CURRENT STAGE = {stage}");

        // 1. FireExt_display MUST DISAPPEAR immediately
        if (displayPickup != null && displayPickup.gameObject.activeSelf)
        {
            displayPickup.gameObject.SetActive(false);
        }

        // 2. The EXISTING ORIGINAL FireExt MUST become active and attach to the worker's camera/hand position
        if (originalPickup == null)
        {
            originalPickup = FindAnyObjectByType<ExtinguisherPickup>(FindObjectsInactive.Include);
        }

        if (originalPickup != null)
        {
            Debug.Log($"[FireAR] ORIGINAL FIRES EXT REFERENCE = {originalPickup.gameObject.name}");

            // Ensure runtime visibility is enabled BEFORE SetActive so Start() knows handoff occurred
            originalPickup.SetRuntimeVisibility(true);

            if (!originalPickup.gameObject.activeSelf)
            {
                originalPickup.gameObject.SetActive(true);
            }
            Debug.Log($"[FireAR] ORIGINAL ACTIVE SELF = {originalPickup.gameObject.activeSelf.ToString().ToLower()}");
            Debug.Log($"[FireAR] ORIGINAL ACTIVE IN HIERARCHY = {originalPickup.gameObject.activeInHierarchy.ToString().ToLower()}");
            Debug.Log($"[FireAR] RUNTIME VISIBILITY = {originalPickup.IsRuntimeVisible().ToString().ToLower()}");
            Debug.Log("[FireAR] PICKUP COMPONENT = FOUND");

            if (!originalPickup.IsHeld())
            {
                originalPickup.AttachToCamera();
            }
            Debug.Log($"[FireAR] AR CAMERA = {(originalPickup.arCamera != null ? originalPickup.arCamera.name : "None")}");
            Debug.Log("[FireAR] ATTACH TO CAMERA = COMPLETE");
            Debug.Log($"[FireAR] IS HELD = {originalPickup.IsHeld().ToString().ToLower()}");
        }
        else
        {
            Debug.LogError("[FireAR] ORIGINAL FIRES EXT REFERENCE = null");
        }

        Debug.Log("[FireAR] DISPLAY = INACTIVE");

        // 3. Immediately transition to Step 4 so stage is authoritative
        SetStage(Stage.Step4_RemovePin);
        Debug.Log("[FireAR] NEXT STATE = Step4_RemovePin");

        AddScore(15, "CO2 Extinguisher Selected");
        EnsureFireAdapter().RecordEquipmentSelected("co2_extinguisher", true);
        TrainingEventManager.RaiseExtinguisherPickedUp();

        // Dynamically re-bind components to the active functional extinguisher
        if (originalPickup != null)
        {
            var newPin = originalPickup.GetComponentInChildren<FirePinInteraction>(true);
            if (newPin != null)
            {
                if (pinInteraction != null && pinInteraction != newPin)
                    pinInteraction.OnPinRemoved.RemoveListener(HandlePinRemoved);
                pinInteraction = newPin;
                pinInteraction.OnPinRemoved.AddListener(HandlePinRemoved);
            }

            var newGrip = originalPickup.GetComponentInChildren<ExtinguisherGripInteraction>(true);
            if (newGrip != null)
            {
                if (gripInteraction != null && gripInteraction != newGrip)
                {
                    gripInteraction.OnGripActivated.RemoveListener(HandleGripActivated);
                    gripInteraction.OnSprayStarted.RemoveListener(HandleSprayStarted);
                    gripInteraction.OnSprayStopped.RemoveListener(HandleSprayStopped);
                    gripInteraction.OnPinRemovalRequired.RemoveListener(HandlePinRemovalRequired);
                }
                gripInteraction = newGrip;
                gripInteraction.OnGripActivated.AddListener(HandleGripActivated);
                gripInteraction.OnSprayStarted.AddListener(HandleSprayStarted);
                gripInteraction.OnSprayStopped.AddListener(HandleSprayStopped);
                gripInteraction.OnPinRemovalRequired.AddListener(HandlePinRemovalRequired);
            }
        }

        if (ui != null)
        {
            ui.SetModuleInfo(GetLoc("module.fire.title", "Fire & Explosion Response"), 4, 6);
            ui.ShowGuidance(
                stepTag: "STEP 4 OF 6",
                title: GetLoc("fire.sop.step4.title", "Remove Safety Pin"),
                description: GetLoc("fire.sop.step4.desc", "Tap the safety pin on the extinguisher handle to break the tamper seal and unlock the lever."),
                hint: GetLoc("fire.sop.step4.hint", "Twist slightly and pull firmly. Do not squeeze the lever while pulling."),
                actionBtnText: GetLoc("fire.sop.step4.action", "Pull Safety Pin"),
                onActionClicked: () =>
                {
                    if (pinInteraction != null)
                    {
                        pinInteraction.RemovePin();
                    }
                    else
                    {
                        HandlePinRemoved();
                    }
                }
            );

            ui.ShowTransientToast(
                title: GetLoc("fire.toast.ext.title", "CO2 Extinguisher Equipped!"),
                subtitle: GetLoc("fire.toast.ext.sub", "Prepare extinguisher for operation"),
                duration: 2.0f
            );
        }
    }

    // --- STEP 4: REMOVE SAFETY PIN ---
    public void TransitionToStep3_RemovePin()
    {
        SetStage(Stage.Step4_RemovePin);
        if (ui == null) return;

        ui.SetModuleInfo(GetLoc("module.fire.title", "Fire & Explosion Response"), 4, 6);

        ui.ShowGuidance(
            stepTag: "STEP 4 OF 6",
            title: GetLoc("fire.sop.step4.title", "Remove Safety Pin"),
            description: GetLoc("fire.sop.step4.desc", "Tap the safety pin on the extinguisher handle to break the tamper seal and unlock the lever."),
            hint: GetLoc("fire.sop.step4.hint", "Twist slightly and pull firmly. Do not squeeze the lever while pulling."),
            actionBtnText: GetLoc("fire.sop.step4.action", "Pull Safety Pin"),
            onActionClicked: () =>
            {
                if (pinInteraction != null)
                {
                    pinInteraction.RemovePin();
                }
                else
                {
                    HandlePinRemoved();
                }
            }
        );
    }

    // Legacy aliases
    public void TransitionToStep4() => TransitionToStep3_RemovePin();
    public void TransitionToStep4_RemovePin() => TransitionToStep3_RemovePin();

    private void HandlePinRemoved()
    {
        if (stage != Stage.Step4_RemovePin)
        {
            HandlePrematurePinAttempt();
            return;
        }

        AddScore(15, "Safety Pin Removed");
        EnsureFireAdapter().RecordPinRemoved();
        TrainingEventManager.RaisePinRemoved();

        // Immediately transition to AimBase so user is armed without delay
        TransitionToStep4_AimBase();

        if (ui != null)
        {
            ui.ShowTransientToast(
                title: GetLoc("fire.toast.pin.title", "Safety Pin Removed!"),
                subtitle: GetLoc("fire.toast.pin.sub", "Handle unlocked. Extinguisher is armed and ready."),
                duration: 2.0f
            );
        }
    }

    private void HandlePinRemovalRequired()
    {
        HandlePrematureGripAttempt();
    }

    // --- STEP 5: AIM AT BASE OF FIRE ---
    public void TransitionToStep4_AimBase()
    {
        SetStage(Stage.Step5_AimBase);
        aimBaseLookTimer = 0f;
        if (ui == null) return;

        ui.SetModuleInfo(GetLoc("module.fire.title", "Fire & Explosion Response"), 5, 6);

        ui.ShowGuidance(
            stepTag: "STEP 5 OF 6",
            title: GetLoc("fire.sop.step5.title", "Aim at Fire Base"),
            description: GetLoc("fire.sop.step5.desc", "Hold the insulated discharge horn. Aim directly at the fuel base of the fire, not at the high flames."),
            hint: GetLoc("fire.sop.step5.hint", "Aiming at the flames allows the fire to continue feeding from the combustible base."),
            actionBtnText: GetLoc("fire.sop.step5.action", "Nozzle Aimed at Base"),
            onActionClicked: OnAimConfirmed
        );
    }

    // Legacy alias
    public void TransitionToStep5() => TransitionToStep4_AimBase();

    public void OnAimConfirmed()
    {
        if (stage != Stage.Step5_AimBase) return;

        AddScore(15, "Aimed at Base");
        EnsureFireAdapter().RecordAimAtBase(true);
        // Raise valid_aim event — distinct from the broader aim_at_base_of_fire telemetry
        EnsureFireAdapter().RecordValidAim();

        // Immediately transition to Extinguish so countdown UI card appears right away
        TransitionToStep5_Extinguish();

        if (ui != null)
        {
            ui.ShowTransientToast(
                title: GetLoc("fire.toast.aim.title", "Nozzle Aimed at Base!"),
                subtitle: GetLoc("fire.toast.aim.sub", "Squeeze handle to discharge spray"),
                duration: 1.5f
            );
        }
    }

    // --- STEP 6: EXTINGUISH (SWEEP & SPRAY) ---
    public void TransitionToStep5_Extinguish()
    {
        SetStage(Stage.Step6_Extinguish);

        // Ensure grip interaction is refreshed on the active held extinguisher
        if (originalPickup != null)
        {
            var newGrip = originalPickup.GetComponentInChildren<ExtinguisherGripInteraction>(true);
            if (newGrip != null) gripInteraction = newGrip;
        }
        if (gripInteraction == null)
        {
            gripInteraction = FindAnyObjectByType<ExtinguisherGripInteraction>(FindObjectsInactive.Include);
        }
        if (gripInteraction != null)
        {
            gripInteraction.OnGripActivated.RemoveListener(HandleGripActivated);
            gripInteraction.OnSprayStarted.RemoveListener(HandleSprayStarted);
            gripInteraction.OnSprayStopped.RemoveListener(HandleSprayStopped);
            gripInteraction.OnPinRemovalRequired.RemoveListener(HandlePinRemovalRequired);
            gripInteraction.OnGripActivated.AddListener(HandleGripActivated);
            gripInteraction.OnSprayStarted.AddListener(HandleSprayStarted);
            gripInteraction.OnSprayStopped.AddListener(HandleSprayStopped);
            gripInteraction.OnPinRemovalRequired.AddListener(HandlePinRemovalRequired);
        }

        if (ui == null) return;

        ui.SetModuleInfo(GetLoc("module.fire.title", "Fire & Explosion Response"), 6, 6);

        ui.ShowGuidance(
            stepTag: "STEP 6 OF 6",
            title: GetLoc("fire.sop.step6.title", "Extinguish the Fire (PASS)"),
            description: GetLoc("fire.sop.step6.desc", "Squeeze the operating lever or tap the button below to discharge spray. Sweep side-to-side across the fuel base until the fire is completely out."),
            hint: GetLoc("fire.sop.step6.hint", "Maintain continuous discharge for 10 seconds until all flames and smoke cease."),
            actionBtnText: (gripInteraction != null && gripInteraction.IsGripHeld)
                ? GetLoc("fire.sop.step6.actionStop", "Release Handle (Stop Spray)")
                : GetLoc("fire.sop.step6.action", "Press Handle & Spray"),
            onActionClicked: () =>
            {
                if (gripInteraction != null)
                {
                    gripInteraction.ToggleGrip();
                }
                else
                {
                    var g = FindAnyObjectByType<ExtinguisherGripInteraction>(FindObjectsInactive.Include);
                    g?.ToggleGrip();
                }
            }
        );

        ui.ShowProgress(0f, "Ready");
    }

    // Legacy alias
    public void TransitionToStep6() => TransitionToStep5_Extinguish();

    private void HandleSprayStarted()
    {
        // Guard: if pin is not yet removed, this is a premature spray attempt
        if (stage < Stage.Step5_AimBase)
        {
            HandlePrematureSprayAttempt();
            return;
        }

        if (stage == Stage.Step6_Extinguish)
        {
            EnsureFireAdapter().RecordSprayAction(true, 0f);
            TrainingEventManager.RaiseExtinguisherUsed();
            if (ui != null)
            {
                ui.ShowGuidance(
                    stepTag: "🔥 STEP 6 OF 6",
                    title: GetLoc("fire.sop.step6.title", "Extinguish the Fire (PASS)"),
                    description: GetLoc("fire.sop.step6.sprayingDesc", "Spraying active! Sweep side-to-side across the fuel base. Keep particles directly on the fire."),
                    hint: GetLoc("fire.sop.step6.hint", "Maintain continuous discharge for 10 seconds until all flames cease."),
                    actionBtnText: GetLoc("fire.sop.step6.actionStop", "Release Handle (Stop Spray)"),
                    onActionClicked: () =>
                    {
                        if (gripInteraction != null) gripInteraction.ToggleGrip();
                    }
                );
            }
        }
    }

    /// <summary>Handles the first successful grip activation (distinct from spray_started).</summary>
    private void HandleGripActivated()
    {
        if (stage < Stage.Step5_AimBase) return; // safety guard
        EnsureFireAdapter().RecordGripActivated();
    }

    private void HandleSprayStopped()
    {
        _wasSprayingOffTarget = false;
        if (stage == Stage.Step6_Extinguish)
        {
            if (ui != null)
            {
                ui.ShowGuidance(
                    stepTag: "🔥 STEP 6 OF 6",
                    title: GetLoc("fire.sop.step6.title", "Extinguish the Fire (PASS)"),
                    description: GetLoc("fire.sop.step6.desc", "Squeeze the operating lever or tap the button below to discharge spray. Sweep side-to-side across the fuel base until the fire is completely out."),
                    hint: GetLoc("fire.sop.step6.hint", "Maintain continuous discharge for 10 seconds until all flames and smoke cease."),
                    actionBtnText: GetLoc("fire.sop.step6.action", "Press Handle & Spray"),
                    onActionClicked: () =>
                    {
                        if (gripInteraction != null) gripInteraction.ToggleGrip();
                    }
                );
            }
        }
    }

    private void UpdateSprayProgress()
    {
        if (stage != Stage.Step6_Extinguish || ui == null)
            return;

        if (fire == null)
        {
            fire = FindAnyObjectByType<FireExtinguishable>(FindObjectsInactive.Include);
            if (fire == null) return;
        }

        float progress = fire.SprayProgress01;
        float total = fire.extinguishTime > 0 ? fire.extinguishTime : 10f;
        float elapsed = fire.CurrentContactTimer;

        bool isSprayingActive = (gripInteraction != null && gripInteraction.IsGripHeld);
        if (!isSprayingActive)
        {
            var ps = FindAnyObjectByType<DryPowderSpray>();
            if (ps != null && ps.IsSpraying()) isSprayingActive = true;
        }

        if (isSprayingActive && fire.IsBeingSprayed)
        {
            _wasSprayingOffTarget = false;
            // Particles actively colliding with fire: countdown advances!
            ui.ShowProgress(progress, elapsed, total, $"Spraying... ({elapsed:F1}s / {total:F0}s)");
        }
        else if (isSprayingActive)
        {
            string offTargetMsg = GetLoc("fire.feedback.aimAtBase", "Aim the nozzle at the base of the fire.");
            ui.ShowProgress(progress, elapsed, total, $"Aim at Fire Base ({elapsed:F1}s / {total:F0}s)");
            if (!_wasSprayingOffTarget)
            {
                _wasSprayingOffTarget = true;
                ui.ShowFeedback(FireScenarioUIController.FeedbackType.Wrong, "Off Target", offTargetMsg, 1.5f);
                if (CanApplyPenalty("spray_off_target"))
                {
                    EnsureFireAdapter().RecordInvalidAim();
                }
            }
        }
        else
        {
            _wasSprayingOffTarget = false;
            // Grip released: progress is 0, timer is 0
            ui.ShowProgress(0f, 0f, total, "Hold Grip to Spray (10s)");
        }
    }

    public void HandleFireExtinguished()
    {
        if (stage == Stage.Complete || stage == Stage.Success || stage == Stage.Timeout || stage == Stage.MoveToExit)
            return;

        if (stage < Stage.Step6_Extinguish)
        {
            SetStage(Stage.Step6_Extinguish);
        }

        if (gripInteraction != null)
        {
            gripInteraction.StopGrip();
        }

        AddScore(25, "Fire Fully Extinguished!");
        EnsureFireAdapter().RecordFireExtinguished(scenarioTimer);
        TrainingEventManager.RaiseFireExtinguished();

        if (ui != null) ui.HideProgress();

        if (ui != null)
        {
            ui.ShowTransientToast(
                title: GetLoc("fire.toast.extinguished.title", "Fire Fully Extinguished!"),
                subtitle: GetLoc("fire.toast.extinguished.sub", "Hazard neutralized. Training complete!"),
                duration: 2.0f,
                onDismiss: () =>
                {
                    CompleteScenario(true);
                }
            );
        }
        else
        {
            CompleteScenario(true);
        }
    }

    // --- OPTIONAL / LEGACY EVACUATION STEP ---
    public void TransitionToStep6_Evacuate()
    {
        SetStage(Stage.Step7_Evacuate);
        if (ui == null) return;

        ui.SetModuleInfo(GetLoc("module.fire.title", "Fire & Explosion Response"), 6, 6);
        ui.HideProgress();

        ui.ShowGuidance(
            stepTag: "🚪 SAFE EVACUATION",
            title: GetLoc("fire.sop.step7.title", "Safe Evacuation"),
            description: GetLoc("fire.sop.step7.desc", "The fire is suppressed. Back away slowly while keeping visual contact. Follow the emergency EXIT signs to the assembly point."),
            hint: GetLoc("fire.sop.step7.hint", "Never turn your back on a suppressed fire due to re-ignition risk."),
            actionBtnText: GetLoc("fire.sop.step7.action", "Proceed to Emergency Exit"),
            onActionClicked: () => CompleteScenario(true)
        );
    }

    // Legacy alias
    public void TransitionToStep7() => TransitionToStep6_Evacuate();

    // =====================================================
    // TIMEOUT & EXIT STATES (SEPARATE BRANCH)
    // =====================================================

    private void TransitionToTimeout()
    {
        // Don't override completion or already-exited states
        if (stage == Stage.Complete || stage == Stage.Success || stage == Stage.Timeout || stage == Stage.MoveToExit) return;

        stage = Stage.Timeout;
        isTimerRunning = false;
        // Use the named RecordTimeout() method (not bare RecordCriticalAction) for consistent telemetry
        EnsureFireAdapter().RecordTimeout();
        TrainingEventManager.RaiseScenarioTimeout();

        if (ui != null)
        {
            ui.HideProgress();
            ui.ShowGuidance(
                stepTag: "⏰ TIME LIMIT REACHED",
                title: GetLoc("fire.timeout.title", "Training Time Expired"),
                description: GetLoc("fire.timeout.desc", "The 7-minute time limit has been reached. The fire was not suppressed in time. You must now evacuate via the emergency exit."),
                hint: GetLoc("fire.timeout.hint", "Safety first: Never remain in a hazard zone once the emergency timeout is reached."),
                actionBtnText: GetLoc("fire.timeout.action", "Proceed to Emergency Exit"),
                onActionClicked: TransitionToMoveToExit
            );
        }
    }

    private void TransitionToMoveToExit()
    {
        stage = Stage.MoveToExit;
        TrainingEventManager.RaiseEvacuationStarted("emergency_exit_timeout", true);
        if (ui == null) return;

        ui.ShowGuidance(
            stepTag: "🚪 EMERGENCY EVACUATION",
            title: GetLoc("fire.evac.title", "Evacuate the Hazard Zone"),
            description: GetLoc("fire.evac.desc", "Follow the green EXIT signs to the emergency assembly point. Do not attempt to re-enter."),
            hint: GetLoc("fire.evac.hint", "Inform emergency services of fire location, fuel type, and any personnel still inside."),
            actionBtnText: GetLoc("fire.evac.action", "Exit Reached — Complete Evacuation"),
            onActionClicked: () => CompleteScenario(false)
        );
    }

    // =====================================================
    // SCENARIO COMPLETION (SUCCESS VS TIMEOUT BRANCHES)
    // =====================================================

    public void CompleteScenario()
    {
        CompleteScenario(stage != Stage.Timeout && stage != Stage.MoveToExit);
    }

    public void CompleteScenario(bool isSuccess)
    {
        SetStage(isSuccess ? Stage.Complete : Stage.Timeout);
        isTimerRunning = false;
        var collector = MovementTelemetryCollector.Instance;
        if (collector != null)
        {
            collector.StopCollection();
        }

        EnsureFireAdapter().RecordEvacuation(isSuccess, isSuccess ? "emergency_exit_A" : "emergency_exit_timeout");
        EnsureFireAdapter().CompleteScenario(currentScore, isSuccess);

        // Record metrics into AppState for Result and Certificate screens
        if (AppState.Instance != null)
        {
            AppState.Instance.LastARTimerSeconds   = scenarioTimer;
            AppState.Instance.CorrectActionsCount  = correctActions;
            AppState.Instance.WrongActionsCount    = wrongActions;
            AppState.Instance.UnsafeActionsCount   = unsafeActions;
            AppState.Instance.LastAttemptTimedOut  = !isSuccess;
            AppState.Instance.FireExtinguishedSuccess = isSuccess;
            if (!isSuccess)
            {
                // Timeout penalty: flag critical error so it does NOT count as successful fire extinguishing
                criticalErrors = Mathf.Max(1, criticalErrors);
                currentScore = Mathf.Clamp(currentScore - 30, 0, 100);
            }
            AppState.Instance.CriticalErrorsCount  = criticalErrors;
            AppState.Instance.RecordAssessmentResult(currentScore, 6); // 6 SOP steps
        }

        if (AppSession.Instance != null)
        {
            AppSession.Instance.SetState(AppSession.TrainingState.Completed);
        }

        TrainingEventManager.RaiseTrainingCompleted();

        int mins = Mathf.FloorToInt(scenarioTimer / 60f);
        int secs = Mathf.FloorToInt(scenarioTimer % 60f);
        string timeStr = string.Format("{0:00}:{1:00}", mins, secs);

        if (ui != null)
        {
            ui.HideGuidance();
            ui.HideProgress();
            if (isSuccess)
            {
                ui.ShowCompletion(
                    title: "TRAINING COMPLETED",
                    message: "Excellent performance! You successfully executed all 6 fire safety SOP steps in accordance with Ministry of Mines safety guidelines.",
                    score: currentScore,
                    timeTaken: timeStr,
                    onContinue: () =>
                    {
                        if (ARModuleLauncher.Instance != null)
                        {
                            ARModuleLauncher.Instance.ExitCurrentARScene();
                        }
                    }
                );
            }
            else
            {
                ui.ShowCompletion(
                    title: "EVACUATION COMPLETE (TIMEOUT)",
                    message: "You safely evacuated via the emergency exit. The 7-minute limit expired before the fire was extinguished. Retraining is recommended.",
                    score: currentScore,
                    timeTaken: timeStr,
                    onContinue: () =>
                    {
                        if (ARModuleLauncher.Instance != null)
                        {
                            ARModuleLauncher.Instance.ExitCurrentARScene();
                        }
                    }
                );
            }
        }
    }

    // =====================================================
    // SCENARIO RESET
    // =====================================================

    public void RestartScenario()
    {
        if (fire != null)
        {
            fire.ResetFire();
        }

        if (gripInteraction != null)
        {
            gripInteraction.StopGrip();
            gripInteraction.ResetGripSession();
        }

        // Reset alarm interaction for re-play
        if (alarmInteraction != null)
        {
            alarmInteraction.ResetAlarm();
        }

        // Reset display extinguisher and return real extinguisher to unheld state
        if (displayPickup != null)
        {
            displayPickup.ResetDisplay();
        }

        if (originalPickup != null)
        {
            if (originalPickup.IsHeld())
            {
                originalPickup.DetachFromCamera();
            }
            originalPickup.gameObject.SetActive(false);
        }

        if (pinInteraction != null)
        {
            pinInteraction.ResetPin();
        }

        // Return to Step 1 — reset all counters
        currentScore    = 0;
        scenarioTimer   = 0f;
        isTimerRunning  = true;
        timeoutTriggered = false;
        correctActions  = 0;
        wrongActions    = 0;
        unsafeActions   = 0;
        criticalErrors  = 0;
        _lastPenaltyTimes.Clear();

        if (ui != null)
        {
            ui.SetScore(currentScore, 0);
            ui.SetTimer(0f);
            ui.HideProgress();
        }

        TransitionToStep1();
    }

    // =====================================================
    // TRACKING LOSS HANDLING
    // =====================================================

    private void CheckARTrackingStatus()
    {
        // AR camera or session tracking check can be invoked here
    }
}
