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
    public int currentScore = 70;
    public int correctActions = 0;
    public int wrongActions = 0;
    public int unsafeActions = 0;
    public int criticalErrors = 0;

    [Header("Timing")]
    public float scenarioTimer = 0f;
    [Tooltip("Overall training time limit in seconds (420 = 7 minutes). Worker is guided to exit after this.")] 
    public float maxTrainingTimeSeconds = 420f;
    private bool isTimerRunning = false;
    private bool timeoutTriggered = false;

    public Stage CurrentStage => stage;

    private Stage stage = Stage.Intro;
    private bool subscribed;
    private Coroutine messageRoutine;
    private float hazardLookTimer = 0f;
    private float aimBaseLookTimer = 0f;
    private bool _wasSprayingOffTarget = false;

    private FireAssessmentAdapter EnsureFireAdapter()
    {
        if (FireAssessmentAdapter.Instance != null) return FireAssessmentAdapter.Instance;
        var go = new GameObject("FireAssessmentAdapter");
        return go.AddComponent<FireAssessmentAdapter>();
    }

    // =====================================================
    // LIFECYCLE
    // =====================================================

    private void Start()
    {
        ResolveReferences();
        BuildUI();
        SubscribeEvents();

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

    public void AddScore(int points, string reason = null)
    {
        currentScore = Mathf.Clamp(currentScore + points, 0, 100);
        correctActions++;
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
        if (fire == null)
            fire = FindAnyObjectByType<FireExtinguishable>();

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
        if (ui != null) return;

        GameObject uiGO = new GameObject("FireScenarioUGUI");
        uiGO.transform.SetParent(transform, false);
        ui = uiGO.AddComponent<FireScenarioUIController>();

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

        TrainingEventManager.OnScenarioPlaced += HandleScenarioPlacedFromEvent;

        if (alarmInteraction != null)
            alarmInteraction.OnAlarmActivated.AddListener(HandleAlarmActivated);

        if (displayPickup != null)
            displayPickup.OnPickedUp.AddListener(HandleExtinguisherPickedUp);

        if (pinInteraction != null)
            pinInteraction.OnPinRemoved.AddListener(HandlePinRemoved);

        if (gripInteraction != null)
        {
            gripInteraction.OnSprayStarted.AddListener(HandleSprayStarted);
            gripInteraction.OnSprayStopped.AddListener(HandleSprayStopped);
            gripInteraction.OnPinRemovalRequired.AddListener(HandlePinRemovalRequired);
        }

        if (fire != null)
            fire.OnExtinguished.AddListener(HandleFireExtinguished);

        subscribed = true;
    }

    private void UnsubscribeEvents()
    {
        if (!subscribed) return;

        if (placement != null)
            placement.OnScenarioPlaced.RemoveListener(HandleScenarioPlaced);

        if (arPlacement != null)
            arPlacement.OnScenarioPlaced.RemoveListener(HandleScenarioPlaced);

        TrainingEventManager.OnScenarioPlaced -= HandleScenarioPlacedFromEvent;

        if (alarmInteraction != null)
            alarmInteraction.OnAlarmActivated.RemoveListener(HandleAlarmActivated);

        if (displayPickup != null)
            displayPickup.OnPickedUp.RemoveListener(HandleExtinguisherPickedUp);

        if (pinInteraction != null)
            pinInteraction.OnPinRemoved.RemoveListener(HandlePinRemoved);

        if (gripInteraction != null)
        {
            gripInteraction.OnSprayStarted.RemoveListener(HandleSprayStarted);
            gripInteraction.OnSprayStopped.RemoveListener(HandleSprayStopped);
            gripInteraction.OnPinRemovalRequired.RemoveListener(HandlePinRemovalRequired);
        }

        if (fire != null)
            fire.OnExtinguished.RemoveListener(HandleFireExtinguished);

        subscribed = false;
    }

    // =====================================================
    // 7-STEP SOP TRAINING FLOW
    // =====================================================

    private void BeginIntro()
    {
        stage = Stage.Intro;
        isTimerRunning = false;
        scenarioTimer = 0f;
        currentScore = 40;
        correctActions = 0;
        wrongActions = 0;
        unsafeActions = 0;
        criticalErrors = 0;

        if (placement != null)
            placement.SetPlacementActive(false);
        if (arPlacement != null)
            arPlacement.SetPlacementActive(false);

        if (ui != null)
        {
            ui.SetModuleInfo("Fire & Explosion Response", 0, 6);
            ui.SetScore(currentScore, 0);
            ui.SetTimer(0f);
            ui.HideProgress();

            ui.ShowGuidance(
                stepTag: "SURAKSHAAR AR",
                title: "Industrial Fire Response Training",
                description: "In this scenario, an electrical equipment fire breaks out in a mining facility.\nFollow standard operating procedures (SOP) to safely respond and evacuate.",
                hint: "Scan the floor and tap the reticle to anchor the 3D training scenario.",
                actionBtnText: "Tap to Start AR Training",
                onActionClicked: BeginScanning
            );
        }
    }

    private void BeginScanning()
    {
        stage = Stage.Scanning;

        if (ui != null)
        {
            ui.SetModuleInfo("Fire & Explosion Response", 0, 6);
            ui.ShowGuidance(
                stepTag: "SURFACE SCAN",
                title: "Find a Flat Surface",
                description: "Move your phone slowly to scan the ground.\nWhen the placement reticle appears, tap anywhere to place the industrial scenario.",
                hint: "Ensure adequate ambient lighting for optical feature tracking.",
                actionBtnText: "Simulate Placement",
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
        }

        if (placement != null)
            placement.SetPlacementActive(true);
        if (arPlacement != null)
            arPlacement.SetPlacementActive(true);
    }

    private void HandleScenarioPlaced()
    {
        if (stage != Stage.Scanning && stage != Stage.Intro) return;

        if (placement != null)
            placement.SetPlacementActive(false);
        if (arPlacement != null)
            arPlacement.SetPlacementActive(false);

        isTimerRunning = true;
        scenarioTimer = 0f;
        TrainingEventManager.RaiseScenarioPlaced();

        // Step 1: Identify Fire Hazard
        TransitionToStep1();
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
        if (fire == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 firePos = fire.FireWorldPosition;
        Vector3 toFire = firePos - cam.transform.position;
        float dist = toFire.magnitude;
        if (dist > 0.1f && dist < 6f)
        {
            float angle = Vector3.Angle(cam.transform.forward, toFire.normalized);
            if (angle < 25f)
            {
                aimBaseLookTimer += Time.deltaTime;
                if (aimBaseLookTimer >= 1.2f)
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
        stage = Stage.Step1_IdentifyHazard;
        hazardLookTimer = 0f;
        EnsureFireAdapter().StartScenario("fire_drill_01");
        if (fire != null)
        {
            if (MovementTelemetryCollector.Instance == null)
            {
                var mgo = new GameObject("MovementTelemetryCollector");
                mgo.AddComponent<MovementTelemetryCollector>();
            }
            MovementTelemetryCollector.Instance?.StartCollection(fire.transform);
        }
        if (ui == null) return;

        ui.SetModuleInfo("Fire & Explosion Response", 1, 6);
        ui.HideProgress();

        ui.ShowGuidance(
            stepTag: "STEP 1 OF 6",
            title: "Identify Fire Hazard",
            description: "Look around your surroundings to locate the electrical fire. Aim your camera at the flames or tap directly on the fire in AR.",
            hint: "Look for sparks, dark smoke, and electrical panel indicators.",
            actionBtnText: "I Have Identified Fire Source",
            onActionClicked: OnHazardIdentified
        );
    }

    public void OnHazardIdentified()
    {
        if (stage != Stage.Step1_IdentifyHazard) return;

        AddScore(10, "Fire Hazard Identified");
        EnsureFireAdapter().RecordHazardIdentified("electrical_fire", true);
        TrainingEventManager.RaiseHazardIdentified();

        if (ui != null)
        {
            ui.ShowTransientToast(
                title: "Hazard Identified!",
                subtitle: "Activate the fire alarm immediately!",
                duration: 2.0f,
                onDismiss: () =>
                {
                    // SOP Step 2: Activate Alarm BEFORE reaching for extinguisher
                    TransitionToStep2_ActivateAlarm();
                }
            );
        }
        else
        {
            TransitionToStep2_ActivateAlarm();
        }
    }

    // --- STEP 2: ACTIVATE FIRE ALARM ---
    public void TransitionToStep2_ActivateAlarm()
    {
        stage = Stage.Step2_ActivateAlarm;
        if (ui == null) return;

        ui.SetModuleInfo("Fire & Explosion Response", 2, 6);

        ui.ShowGuidance(
            stepTag: "STEP 2 OF 6",
            title: "Activate Fire Alarm",
            description: "Locate the red fire alarm pull station on the wall and tap it to alert all personnel in the facility.",
            hint: "Always alert others before attempting to fight a fire alone. Never skip the alarm.",
            actionBtnText: "Activate Fire Alarm",
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
        if (stage != Stage.Step2_ActivateAlarm) return;

        AddScore(10, "Fire Alarm Activated");
        EnsureFireAdapter().RecordAlarmActivated();

        if (ui != null)
        {
            ui.ShowTransientToast(
                title: "Alarm Activated!",
                subtitle: "Personnel alerted. Now locate the extinguisher.",
                duration: 2.0f,
                onDismiss: () =>
                {
                    TransitionToStep3_SelectExtinguisher();
                }
            );
        }
        else
        {
            TransitionToStep3_SelectExtinguisher();
        }
    }

    // --- STEP 3: SELECT EXTINGUISHER ---
    public void TransitionToStep3_SelectExtinguisher()
    {
        stage = Stage.Step3_SelectExtinguisher;
        if (ui == null) return;

        ui.SetModuleInfo("Fire & Explosion Response", 3, 6);

        ui.ShowGuidance(
            stepTag: "STEP 3 OF 6",
            title: "Select Correct Extinguisher",
            description: "Examine the burning equipment. Tap on the CO2 Extinguisher (Black band) in your surroundings to equip it.",
            hint: "DANGER: Never use Water or Foam on live electrical panels! Electrocution hazard.",
            actionBtnText: "Equip CO2 Extinguisher",
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

    private void HandleExtinguisherPickedUp()
    {
        if (stage != Stage.Step3_SelectExtinguisher) return;

        AddScore(10, "CO2 Extinguisher Selected");
        EnsureFireAdapter().RecordEquipmentSelected("co2_extinguisher", true);
        TrainingEventManager.RaiseExtinguisherPickedUp();

        // Dynamically re-bind components to the active functional extinguisher
        if (originalPickup != null)
        {
            pinInteraction = originalPickup.GetComponentInChildren<FirePinInteraction>(true) ?? pinInteraction;
            gripInteraction = originalPickup.GetComponentInChildren<ExtinguisherGripInteraction>(true) ?? gripInteraction;
        }

        if (ui != null)
        {
            ui.ShowTransientToast(
                title: "CO2 Extinguisher Equipped!",
                subtitle: "Prepare extinguisher for operation",
                duration: 2.0f,
                onDismiss: () =>
                {
                    TransitionToStep3_RemovePin();
                }
            );
        }
        else
        {
            TransitionToStep3_RemovePin();
        }
    }

    // --- STEP 4: REMOVE SAFETY PIN ---
    public void TransitionToStep3_RemovePin()
    {
        stage = Stage.Step4_RemovePin;
        if (ui == null) return;

        ui.SetModuleInfo("Fire & Explosion Response", 4, 6);

        ui.ShowGuidance(
            stepTag: "STEP 4 OF 6",
            title: "Remove Safety Pin",
            description: "Tap the safety pin on the extinguisher handle to break the tamper seal and unlock the lever.",
            hint: "Twist slightly and pull firmly. Do not squeeze the lever while pulling.",
            actionBtnText: "Pull Safety Pin",
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
        if (stage != Stage.Step4_RemovePin) return;

        AddScore(10, "Safety Pin Removed");
        EnsureFireAdapter().RecordPinRemoved();
        TrainingEventManager.RaisePinRemoved();

        if (ui != null)
        {
            ui.ShowTransientToast(
                title: "Safety Pin Removed!",
                subtitle: "Handle unlocked. Extinguisher is armed and ready.",
                duration: 2.0f,
                onDismiss: () =>
                {
                    TransitionToStep4_AimBase();
                }
            );
        }
        else
        {
            TransitionToStep4_AimBase();
        }
    }

    private void HandlePinRemovalRequired()
    {
        DeductScore(5, "Safety pin must be removed before operating the lever!", isUnsafe: false);
        EnsureFireAdapter().RecordWrongAction("lever_squeezed_before_pin", "Safety pin must be removed before operating the lever", "minor");
    }

    // --- STEP 5: AIM AT BASE OF FIRE ---
    public void TransitionToStep4_AimBase()
    {
        stage = Stage.Step5_AimBase;
        aimBaseLookTimer = 0f;
        if (ui == null) return;

        ui.SetModuleInfo("Fire & Explosion Response", 5, 6);

        ui.ShowGuidance(
            stepTag: "STEP 5 OF 6",
            title: "Aim at Fire Base",
            description: "Hold the insulated discharge horn. Aim directly at the fuel base of the fire, not at the high flames.",
            hint: "Aiming at the flames allows the fire to continue feeding from the combustible base.",
            actionBtnText: "Nozzle Aimed at Base",
            onActionClicked: OnAimConfirmed
        );
    }

    // Legacy alias
    public void TransitionToStep5() => TransitionToStep4_AimBase();

    public void OnAimConfirmed()
    {
        if (stage != Stage.Step5_AimBase) return;

        AddScore(10, "Aimed at Base");
        EnsureFireAdapter().RecordAimAtBase(true);

        if (ui != null)
        {
            ui.ShowTransientToast(
                title: "Nozzle Aimed at Base!",
                subtitle: "Ready for sweep discharge",
                duration: 2.0f,
                onDismiss: () =>
                {
                    TransitionToStep5_Extinguish();
                }
            );
        }
        else
        {
            TransitionToStep5_Extinguish();
        }
    }

    // --- STEP 6: EXTINGUISH (SWEEP & SPRAY) ---
    public void TransitionToStep5_Extinguish()
    {
        stage = Stage.Step6_Extinguish;
        if (ui == null) return;

        ui.SetModuleInfo("Fire & Explosion Response", 6, 6);

        ui.ShowGuidance(
            stepTag: "STEP 6 OF 6",
            title: "Extinguish the Fire",
            description: "Squeeze the operating lever or tap the button below to discharge spray. Sweep side-to-side across the fuel base until the fire is completely out.",
            hint: "Maintain continuous discharge for 10 seconds until all flames and smoke cease.",
            actionBtnText: gripInteraction != null && gripInteraction.IsGripHeld ? "Release Handle (Stop Spray)" : "Press Handle & Spray",
            onActionClicked: () =>
            {
                if (gripInteraction != null)
                {
                    gripInteraction.ToggleGrip();
                }
            }
        );

        ui.ShowProgress(0f, 0f, 10f, "Ready");
    }

    // Legacy alias
    public void TransitionToStep6() => TransitionToStep5_Extinguish();

    private void HandleSprayStarted()
    {
        if (stage == Stage.Step6_Extinguish)
        {
            EnsureFireAdapter().RecordSprayAction(true, 0f);
            TrainingEventManager.RaiseExtinguisherUsed();
            if (ui != null)
            {
                ui.ShowGuidance(
                    stepTag: "🔥 STEP 6 OF 6",
                    title: "Extinguish the Fire",
                    description: "Spraying active! Sweep side-to-side across the fuel base. Keep particles directly on the fire.",
                    hint: "Maintain continuous discharge for 10 seconds until all flames cease.",
                    actionBtnText: "Release Handle (Stop Spray)",
                    onActionClicked: () =>
                    {
                        if (gripInteraction != null) gripInteraction.ToggleGrip();
                    }
                );
            }
        }
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
                    title: "Extinguish the Fire",
                    description: "Squeeze the operating lever or tap the button below to discharge spray. Sweep side-to-side across the fuel base until the fire is completely out.",
                    hint: "Maintain continuous discharge for 10 seconds until all flames and smoke cease.",
                    actionBtnText: "Press Handle & Spray",
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
        if (stage != Stage.Step6_Extinguish || ui == null || fire == null)
            return;

        float progress = fire.SprayProgress01;
        float total = fire.extinguishTime > 0 ? fire.extinguishTime : 10f;
        float elapsed = fire.CurrentContactTimer;

        if (fire.IsBeingSprayed)
        {
            _wasSprayingOffTarget = false;
            // Continuously colliding: live timer counts up to 10.0s!
            ui.ShowProgress(progress, elapsed, total, "Spraying...");
        }
        else if (gripInteraction != null && gripInteraction.IsGripHeld)
        {
            // Spray is active but particles are off-target: timer reset!
            ui.ShowProgress(0f, 0f, total, "Off Target");
            if (!_wasSprayingOffTarget)
            {
                _wasSprayingOffTarget = true;
                ui.ShowFeedback(FireScenarioUIController.FeedbackType.Wrong, "Off Target - Timer Reset!", "Spray particles must continuously hit the fire for 10s!", 1.5f);
            }
        }
        else
        {
            _wasSprayingOffTarget = false;
            // Extinguisher ready to spray
            ui.ShowProgress(0f, 0f, total, "Ready to Spray");
        }
    }

    private void HandleFireExtinguished()
    {
        if (stage != Stage.Step6_Extinguish) return;

        if (gripInteraction != null)
        {
            gripInteraction.StopGrip();
        }

        AddScore(10, "Fire Fully Extinguished!");
        EnsureFireAdapter().RecordFireExtinguished(scenarioTimer);
        TrainingEventManager.RaiseFireExtinguished();

        if (ui != null) ui.HideProgress();

        if (ui != null)
        {
            ui.ShowTransientToast(
                title: "Fire Fully Extinguished!",
                subtitle: "Hazard neutralized. Training complete!",
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
        stage = Stage.Step7_Evacuate;
        if (ui == null) return;

        ui.SetModuleInfo("Fire & Explosion Response", 6, 6);
        ui.HideProgress();

        ui.ShowGuidance(
            stepTag: "🚪 SAFE EVACUATION",
            title: "Safe Evacuation",
            description: "The fire is suppressed. Back away slowly while keeping visual contact. Follow the emergency EXIT signs to the assembly point.",
            hint: "Never turn your back on a suppressed fire due to re-ignition risk.",
            actionBtnText: "Proceed to Emergency Exit",
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
        EnsureFireAdapter().RecordCriticalAction("scenario_timeout", "7-minute time limit expired before fire was extinguished");

        if (ui != null)
        {
            ui.HideProgress();
            ui.ShowGuidance(
                stepTag: "⏰ TIME LIMIT REACHED",
                title: "Training Time Expired",
                description: "The 7-minute time limit has been reached. The fire was not suppressed in time. You must now evacuate via the emergency exit.",
                hint: "Safety first: Never remain in a hazard zone once the emergency timeout is reached.",
                actionBtnText: "Proceed to Emergency Exit",
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
            title: "Evacuate the Hazard Zone",
            description: "Follow the green EXIT signs to the emergency assembly point. Do not attempt to re-enter.",
            hint: "Inform emergency services of fire location, fuel type, and any personnel still inside.",
            actionBtnText: "Exit Reached — Complete Evacuation",
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
        stage = isSuccess ? Stage.Complete : Stage.Timeout;
        isTimerRunning = false;
        MovementTelemetryCollector.Instance?.StopCollection();

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
        }

        // Reset alarm interaction for re-play
        if (alarmInteraction != null)
        {
            alarmInteraction.ResetAlarm();
        }

        // Return to Step 1 — reset all counters
        currentScore    = 40;
        scenarioTimer   = 0f;
        isTimerRunning  = true;
        timeoutTriggered = false;
        correctActions  = 0;
        wrongActions    = 0;
        unsafeActions   = 0;
        criticalErrors  = 0;

        if (ui != null)
        {
            ui.SetScore(currentScore, 0);
            ui.SetTimer(maxTrainingTimeSeconds); // show full countdown time at restart
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