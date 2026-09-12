using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using SurakshaAR.Core;
using SurakshaAR.UI;

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
        Success,
        Complete
    }

    [Header("Cross References (Preserved for Unity Scene Serializations)")]
    public GameObject fireScenario;
    public FireScenarioARPlacement placement;
    public ExtinguisherDisplayPickup displayPickup;
    public ExtinguisherPickup originalPickup;
    public FirePinInteraction pinInteraction;
    public ExtinguisherGripInteraction gripInteraction;
    public FireExtinguishable fire;
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
    private bool isTimerRunning = false;

    public Stage CurrentStage => stage;

    private Stage stage = Stage.Intro;
    private bool subscribed;
    private Coroutine messageRoutine;
    private float hazardLookTimer = 0f;
    private float aimBaseLookTimer = 0f;

    // =====================================================
    // LIFECYCLE
    // =====================================================

    private void Start()
    {
        ResolveReferences();
        BuildUI();
        SubscribeEvents();
        BeginIntro();
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
            ui.SetTimer(scenarioTimer);
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
            unsafeActions++;
        else
            wrongActions++;

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

        if (fireScenario != null)
        {
            if (displayPickup == null)
                displayPickup = fireScenario.GetComponentInChildren<ExtinguisherDisplayPickup>(true);

            if (originalPickup == null)
                originalPickup = fireScenario.GetComponentInChildren<ExtinguisherPickup>(true);

            if (fire == null)
                fire = fireScenario.GetComponentInChildren<FireExtinguishable>(true);

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

    private void SubscribeEvents()
    {
        if (subscribed) return;

        if (placement != null)
            placement.OnScenarioPlaced.AddListener(HandleScenarioPlaced);

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
        currentScore = 70;
        correctActions = 0;
        wrongActions = 0;
        unsafeActions = 0;
        criticalErrors = 0;

        if (placement != null)
            placement.SetPlacementActive(false);

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
                actionBtnText: "Start AR Placement",
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
                    if (placement != null)
                    {
                        // Simulate placement for editor/testing or fallback
                        HandleScenarioPlaced();
                    }
                }
            );
        }

        if (placement != null)
            placement.SetPlacementActive(true);
    }

    private void HandleScenarioPlaced()
    {
        if (stage != Stage.Scanning && stage != Stage.Intro) return;

        if (placement != null)
            placement.SetPlacementActive(false);

        isTimerRunning = true;
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

        // 1. Check Camera Look / Aim at Fire
        Vector3 toFire = fire.transform.position - cam.transform.position;
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
                if (hit.transform == fire.transform || hit.transform.IsChildOf(fire.transform) || fire.transform.IsChildOf(hit.transform))
                {
                    OnHazardIdentified();
                    return;
                }
            }

            Vector3 fireScreen = cam.WorldToScreenPoint(fire.transform.position);
            if (fireScreen.z > 0 && Vector2.Distance(fireScreen, screenPos) <= 140f)
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

        Vector3 toFire = fire.transform.position - cam.transform.position;
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
        if (ui == null) return;

        ui.SetModuleInfo("Fire & Explosion Response", 1, 6);
        ui.HideProgress();

        ui.ShowGuidance(
            stepTag: "🔥 STEP 1 OF 6",
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
        TrainingEventManager.RaiseHazardIdentified();

        if (ui != null)
        {
            ui.ShowTransientToast(
                title: "Hazard Identified!",
                subtitle: "Now locate the fire extinguisher",
                duration: 2.0f,
                onDismiss: () =>
                {
                    TransitionToStep2_SelectExtinguisher();
                }
            );
        }
        else
        {
            TransitionToStep2_SelectExtinguisher();
        }
    }

    // --- STEP 2: SELECT EXTINGUISHER ---
    public void TransitionToStep2_SelectExtinguisher()
    {
        stage = Stage.Step3_SelectExtinguisher;
        if (ui == null) return;

        ui.SetModuleInfo("Fire & Explosion Response", 2, 6);

        ui.ShowGuidance(
            stepTag: "🧯 STEP 2 OF 6",
            title: "Select Correct Extinguisher",
            description: "Examine the burning equipment. Tap on the CO2 Extinguisher (Black band) in your surroundings to equip it.",
            hint: "DANGER: Never use Water or Foam on live electrical panels! Electrocution hazard.",
            actionBtnText: "Equip CO2 Extinguisher",
            onActionClicked: () =>
            {
                if (displayPickup != null)
                {
                    displayPickup.Pickup();
                }
                else
                {
                    HandleExtinguisherPickedUp();
                }
            }
        );
    }

    // Legacy alias
    public void TransitionToStep2() => TransitionToStep2_SelectExtinguisher();
    public void TransitionToStep3() => TransitionToStep2_SelectExtinguisher();

    private void HandleExtinguisherPickedUp()
    {
        if (stage != Stage.Step3_SelectExtinguisher && stage != Stage.Step2_ActivateAlarm) return;

        AddScore(10, "CO2 Extinguisher Selected");
        TrainingEventManager.RaiseExtinguisherPickedUp();

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

    // --- STEP 3: REMOVE SAFETY PIN ---
    public void TransitionToStep3_RemovePin()
    {
        stage = Stage.Step4_RemovePin;
        if (ui == null) return;

        ui.SetModuleInfo("Fire & Explosion Response", 3, 6);

        ui.ShowGuidance(
            stepTag: "📌 STEP 3 OF 6",
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

    // Legacy alias
    public void TransitionToStep4() => TransitionToStep3_RemovePin();

    private void HandlePinRemoved()
    {
        if (stage != Stage.Step4_RemovePin) return;

        AddScore(10, "Safety Pin Removed");
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
    }

    // --- STEP 4: AIM AT BASE OF FIRE ---
    public void TransitionToStep4_AimBase()
    {
        stage = Stage.Step5_AimBase;
        aimBaseLookTimer = 0f;
        if (ui == null) return;

        ui.SetModuleInfo("Fire & Explosion Response", 4, 6);

        ui.ShowGuidance(
            stepTag: "🎯 STEP 4 OF 6",
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

    // --- STEP 5: EXTINGUISH (SWEEP & SPRAY) ---
    public void TransitionToStep5_Extinguish()
    {
        stage = Stage.Step6_Extinguish;
        if (ui == null) return;

        ui.SetModuleInfo("Fire & Explosion Response", 5, 6);

        ui.ShowGuidance(
            stepTag: "🔥 STEP 5 OF 6",
            title: "Extinguish the Fire",
            description: "Squeeze the operating lever and sweep side-to-side across the base until the fire is completely out.",
            hint: "Maintain continuous discharge until all embers and smoke cease.",
            actionBtnText: "Press Handle & Spray",
            onActionClicked: () =>
            {
                if (gripInteraction != null)
                {
                    gripInteraction.StartGrip();
                }
                else if (fire != null)
                {
                    fire.ExtinguishFire();
                }
            }
        );

        ui.ShowProgress(0f, "Spraying fire base... 10.0s remaining");
    }

    // Legacy alias
    public void TransitionToStep6() => TransitionToStep5_Extinguish();

    private void HandleSprayStarted()
    {
        if (stage == Stage.Step6_Extinguish)
        {
            TrainingEventManager.RaiseExtinguisherUsed();
        }
    }

    private void HandleSprayStopped()
    {
    }

    private void UpdateSprayProgress()
    {
        if (stage != Stage.Step6_Extinguish || ui == null || fire == null)
            return;

        float progress = fire.SprayProgress01;
        float total = fire.extinguishTime > 0 ? fire.extinguishTime : 10f;
        float remaining = Mathf.Clamp(total - (progress * total), 0f, total);

        if (fire.IsBeingSprayed)
        {
            // Continuously colliding: live countdown from 10.0s to 0.0s!
            ui.ShowProgress(progress, string.Format("Extinguishing Fire: {0:F1}s / {1:F0}s (Keep Spraying!)", remaining, total));
        }
        else if (gripInteraction != null && gripInteraction.IsGripHeld)
        {
            // Spray is held but particles moved away / off-target: timer restarts!
            ui.ShowProgress(0f, string.Format("Off Target! Timer Restarted: {0:F1}s / {1:F0}s", total, total));
            ui.ShowFeedback(FireScenarioUIController.FeedbackType.Wrong, "Off Target - Timer Reset!", "Spray particles must continuously hit the fire for 10s!", 1.2f);
        }
        else
        {
            // Extinguisher ready to spray
            ui.ShowProgress(0f, string.Format("Aim Nozzle & Press Handle ({0:F0}s Continuous Spray)", total));
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
        TrainingEventManager.RaiseFireExtinguished();

        if (ui != null) ui.HideProgress();

        if (ui != null)
        {
            ui.ShowTransientToast(
                title: "Fire Fully Extinguished!",
                subtitle: "Hazard neutralized. Prepare for evacuation.",
                duration: 2.0f,
                onDismiss: () =>
                {
                    TransitionToStep6_Evacuate();
                }
            );
        }
        else
        {
            TransitionToStep6_Evacuate();
        }
    }

    // --- STEP 6: EVACUATION & EXIT ---
    public void TransitionToStep6_Evacuate()
    {
        stage = Stage.Step7_Evacuate;
        if (ui == null) return;

        ui.SetModuleInfo("Fire & Explosion Response", 6, 6);
        ui.HideProgress();

        ui.ShowGuidance(
            stepTag: "🚪 STEP 6 OF 6",
            title: "Safe Evacuation",
            description: "The fire is suppressed. Back away slowly while keeping visual contact. Follow the emergency EXIT signs to the assembly point.",
            hint: "Never turn your back on a suppressed fire due to re-ignition risk.",
            actionBtnText: "Proceed to Emergency Exit",
            onActionClicked: CompleteScenario
        );
    }

    // Legacy alias
    public void TransitionToStep7() => TransitionToStep6_Evacuate();

    public void CompleteScenario()
    {
        stage = Stage.Complete;
        isTimerRunning = false;

        // Record metrics into AppState and AppSession
        if (AppState.Instance != null)
        {
            AppState.Instance.RecordAssessmentResult(currentScore, 7);
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
            ui.ShowCompletion(
                title: "TRAINING COMPLETED",
                message: "Excellent performance! You successfully executed all 7 fire safety SOP steps in accordance with Ministry of Mines safety guidelines.",
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

        // Return to Step 1
        currentScore = 70;
        scenarioTimer = 0f;
        isTimerRunning = true;
        correctActions = 0;
        wrongActions = 0;
        unsafeActions = 0;
        criticalErrors = 0;

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