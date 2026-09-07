using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// FireScenarioFlowManager
/// ========================
/// The brain of the AR fire-safety training flow.
///
/// It drives the worker through the complete milestone:
///
///   1. INTRO card          -> tap anywhere
///   2. SCANNING hint       -> move phone, blue circle appears on floor
///   3. TAP ON circle       -> FireScenario placed + world-locked
///   4. Mission message     -> find fire extinguisher
///   5. Pick-up guide       -> tap display extinguisher (the fake)
///   6. Pin guide           -> remove safety pin
///   7. Aim + spray guide   -> hold handle, spray the fire
///   8. Spray timer         -> keep powder on fire until it goes out
///   9. SUCCESS card
///  10. COMPLETE card
///
/// It communicates only through public events raised by the existing
/// interaction scripts. It never moves/rotates/parents the scenario,
/// so the scenario stays fixed in the real world.
///
/// Attach this to the XR Origin (or any always-active object).
/// </summary>
public class FireScenarioFlowManager : MonoBehaviour
{
    public enum Stage
    {
        Intro,
        Scanning,
        MissionIntro,
        PickupGuide,
        PinGuide,
        AimGuide,
        Spraying,
        Success,
        Complete
    }

    [Header("Cross References")]
    public GameObject fireScenario;
    public FireScenarioARPlacement placement;
    public ExtinguisherDisplayPickup displayPickup;
    public ExtinguisherPickup originalPickup;
    public FirePinInteraction pinInteraction;
    public ExtinguisherGripInteraction gripInteraction;
    public FireExtinguishable fire;
    public FireScenarioUIController ui;

    [Header("Intro Card")]
    public string introTitle = "SURAKSHAAR";
    public string introBody =
        "AR FIRE-SAFETY TRAINING\n\n" +
        "In this experience a real floor is detected and an AR scenario is placed on it.\n" +
        "An ELECTRICAL FIRE breaks out.\nYour job: act safely and put the fire out.\n" +
        "\nFollow the cards at the bottom of the screen.";
    public string introFooter = "TAP ANYWHERE TO CONTINUE";

    [Header("Scanning")]
    public string scanHint =
        "Move your phone slowly to scan the floor.\n" +
        "When the BLUE circle appears, TAP it to place the scenario.";

    [Header("Mission")]
    public string missionHint =
        "ELECTRICAL FIRE DETECTED\n" +
        "An electrical box is on fire. Find the fire extinguisher and put the fire out.";
    public string pickupHint =
        "Walk to the FIRE EXTINGUISHER (on your right) and TAP it to pick it up.";

    [Header("Extinguisher")]
    public string pinHint =
        "STEP 1: Remove the SAFETY PIN from the extinguisher.";
    public string aimHint =
        "STEP 2: Aim the hose at the BASE of the fire.\n" +
        "STEP 3: PRESS AND HOLD the handle to spray.";
    public string pinBlockedHint = "Wrong order! Remove the safety pin first.";
    public string sprayHint =
        "KEEP SPRAYING!\nAim the powder at the base of the fire.";
    public string sprayMissHint =
        "The powder is not hitting the fire.\nAim the handle toward the fire.";

    [Header("Result")]
    public string successTitle = "FIRE EXTINGUISHED!";
    public string successBody =
        "Excellent work!\n" +
        "You:\n1. Identified the fire\n2. Picked up the extinguisher\n" +
        "3. Removed the safety pin\n4. Aimed and kept spraying\n" +
        "5. Continued until the fire went out.";
    public string successFooter = "TAP ANYWHERE TO CONTINUE";

    public string completeTitle = "TRAINING COMPLETE";
    public string completeBody =
        "The fire is out and the area is safe.\n" +
        "Now walk calmly toward the EXIT sign and leave the area.\n\nWell done!";
    public string completeFooter = "TAP TO CLOSE";

    [Header("Timing")]
    public float messageHoldTime = 3.5f;
    public float temporaryHintTime = 2.5f;

    public Stage CurrentStage => stage;

    private Stage stage = Stage.Intro;
    private bool subscribed;
    private Coroutine messageRoutine;

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
        HandleCardTaps();
        UpdateSprayProgress();
    }

    // =====================================================
    // REFERENCE RESOLUTION
    // =====================================================

    private void ResolveReferences()
    {
        // The scenario is inactive until placement, so we use
        // GameObject.Find (which finds inactive objects as well).
        if (fireScenario == null)
        {
            GameObject found = GameObject.Find("FireScenario");
            if (found != null)
            {
                fireScenario = found;
            }
            else
            {
                Debug.LogError(
                    "FireScenarioFlowManager: FireScenario not found in scene."
                );
            }
        }

        if (placement == null)
        {
            placement = FindFirstObjectByType<FireScenarioARPlacement>();
            if (placement == null)
            {
                Debug.LogError(
                    "FireScenarioFlowManager: FireScenarioARPlacement not found on XR Origin."
                );
            }
        }

        if (fireScenario != null)
        {
            if (displayPickup == null)
            {
                displayPickup = fireScenario.GetComponentInChildren<ExtinguisherDisplayPickup>(true);
            }

            if (originalPickup == null)
            {
                originalPickup = fireScenario.GetComponentInChildren<ExtinguisherPickup>(true);
            }

            if (fire == null)
            {
                fire = fireScenario.GetComponentInChildren<FireExtinguishable>(true);
            }

            if (pinInteraction == null)
            {
                FirePinInteraction[] allPins =
                    fireScenario.GetComponentsInChildren<FirePinInteraction>(true);

                foreach (FirePinInteraction pin in allPins)
                {
                    if (originalPickup != null &&
                        pin.transform.IsChildOf(originalPickup.transform))
                    {
                        pinInteraction = pin;
                        break;
                    }
                }
            }

            if (gripInteraction == null)
            {
                ExtinguisherGripInteraction[] allGrips =
                    fireScenario.GetComponentsInChildren<ExtinguisherGripInteraction>(true);

                foreach (ExtinguisherGripInteraction grip in allGrips)
                {
                    if (originalPickup != null &&
                        grip.transform.IsChildOf(originalPickup.transform))
                    {
                        gripInteraction = grip;
                        break;
                    }
                }
            }
        }

        if (fireScenario == null)
        {
            Debug.LogError(
                "FireScenarioFlowManager: could not resolve FireScenario references."
            );
        }
    }

    // =====================================================
    // UI
    // =====================================================

    private void BuildUI()
    {
        if (ui != null)
            return;

        GameObject uiGO = new GameObject("FireScenarioUI");
        uiGO.transform.SetParent(transform, false);
        ui = uiGO.AddComponent<FireScenarioUIController>();
    }

    // =====================================================
    // EVENT SUBSCRIPTIONS
    // =====================================================

    private void SubscribeEvents()
    {
        if (subscribed)
            return;

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
        if (!subscribed)
            return;

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
    // FLOW HANDLERS
    // =====================================================

    private void BeginIntro()
    {
        stage = Stage.Intro;

        if (placement != null)
            placement.SetPlacementActive(false);

        if (ui != null)
        {
            ui.HideHint();
            ui.HideProgress();
            ui.ShowCard(introTitle, introBody, introFooter);
        }
    }

    private void BeginScanning()
    {
        stage = Stage.Scanning;

        if (ui != null)
        {
            ui.HideCard();
            ui.ShowHint(scanHint);
        }

        // Enable the blue circle + floor placement.
        if (placement != null)
            placement.SetPlacementActive(true);
    }

    private void HandleScenarioPlaced()
    {
        if (stage == Stage.Complete)
            return;

        // Lock placement forever - scenario is anchored in the real world.
        if (placement != null)
            placement.SetPlacementActive(false);

        stage = Stage.MissionIntro;

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(ShowMissionSequence());
    }

    private IEnumerator ShowMissionSequence()
    {
        if (ui == null)
            yield break;

        ui.HideHint();

        // Let the fire appear, then explain the mission.
        yield return new WaitForSeconds(messageHoldTime);

        ui.ShowHint(missionHint);
        yield return new WaitForSeconds(messageHoldTime);

        ui.ShowHint(pickupHint);
        stage = Stage.PickupGuide;
    }

    private void HandleExtinguisherPickedUp()
    {
        if (stage == Stage.Complete)
            return;

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        stage = Stage.PinGuide;
        ui.ShowHint(pinHint);
    }

    private void HandlePinRemoved()
    {
        if (stage == Stage.Complete)
            return;

        stage = Stage.AimGuide;
        ui.ShowHint(aimHint);
    }

    private void HandleSprayStarted()
    {
        if (stage == Stage.Complete)
            return;

        stage = Stage.Spraying;

        if (ui != null)
        {
            ui.ShowHint(sprayHint);
            ui.ShowProgress(0f,
                "Spraying the fire... 0.0s / " +
                (int)RequiredSpraySeconds() + "s");
        }
    }

    private void HandleSprayStopped()
    {
        // Nothing to do right now - UpdateSprayProgress shows the
        // "not hitting the fire" hint automatically.
    }

    private void HandlePinRemovalRequired()
    {
        // The worker pressed the handle before removing the safety pin.
        if (stage == Stage.PickupGuide || stage == Stage.PinGuide)
        {
            if (messageRoutine != null)
                StopCoroutine(messageRoutine);

            messageRoutine = StartCoroutine(
                ShowTemporaryHint(pinBlockedHint, temporaryHintTime));
        }
    }

    private void HandleFireExtinguished()
    {
        if (stage == Stage.Complete)
            return;

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        if (ui != null)
        {
            ui.HideHint();
            ui.HideProgress();
            ui.ShowCard(successTitle, successBody, successFooter);
        }

        stage = Stage.Success;
    }

    private void UpdateSprayProgress()
    {
        if (stage != Stage.Spraying || ui == null || fire == null)
            return;

        float progress = fire.SprayProgress01;
        float total = fire.extinguishTime;
        float remaining = Mathf.Clamp(total - (progress * total), 0f, total);

        ui.ShowProgress(progress,
            string.Format(
                "Spraying the fire... {0:F1}s / {1:F0}s",
                remaining,
                total));

        if (fire.IsBeingSprayed)
        {
            ui.ShowHint(sprayHint);
        }
        else
        {
            ui.ShowHint(sprayMissHint);
        }
    }

    private float RequiredSpraySeconds()
    {
        return fire != null ? fire.extinguishTime : 10f;
    }

    private void HandleCardTaps()
    {
        if (ui == null || !ui.HasCard)
            return;

        if (!AnyTapPressedThisFrame())
            return;

        if (stage == Stage.Intro)
        {
            BeginScanning();
        }
        else if (stage == Stage.Success)
        {
            stage = Stage.Complete;
            ui.HideCard();
            ui.ShowCard(completeTitle, completeBody, completeFooter);
        }
        else if (stage == Stage.Complete)
        {
            ui.HideCard();
            Debug.Log("Fire training flow complete.");
        }
    }

    private IEnumerator ShowTemporaryHint(string text, float duration)
    {
        if (ui != null)
            ui.ShowHint(text);

        yield return new WaitForSeconds(duration);

        if (ui == null)
            yield break;

        switch (stage)
        {
            case Stage.PickupGuide:
                ui.ShowHint(pickupHint);
                break;
            case Stage.PinGuide:
                ui.ShowHint(pinHint);
                break;
            default:
                ui.HideHint();
                break;
        }
    }

    private bool AnyTapPressedThisFrame()
    {
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return true;
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }
}