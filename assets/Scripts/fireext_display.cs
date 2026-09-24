using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// ExtinguisherDisplayPickup
/// =========================
/// Controls the visual display extinguisher prop in the 3D fire scenario.
///
/// Requirement:
/// 1. When the scenario is placed, ONLY this display model is visible alongside
///    the other scenario models; the functional 'FireExt' remains hidden.
/// 2. When the worker taps this display extinguisher (at Step 3 or upon direct tap):
///    - This display extinguisher disappears.
///    - The functional 'FireExt' activates and attaches to the AR Camera.
///    - All further training steps (pull pin, aim, spray) run on 'FireExt'.
/// </summary>
public class ExtinguisherDisplayPickup : MonoBehaviour
{
    [Header("Original Functional Extinguisher (FireExt)")]
    public GameObject originalExtinguisher;

    [Header("AR Camera")]
    public Transform arCamera;

    [Header("Optional Flow Manager Reference")]
    public FireScenarioFlowManager flowManager;

    private bool pickedUp = false;

    /// <summary>
    /// Raised the moment the worker taps the display (fake) extinguisher.
    /// The hidden original is then attached to the AR camera.
    /// </summary>
    public UnityEvent OnPickedUp = new UnityEvent();

    public bool IsPickedUp => pickedUp;

    private void Awake()
    {
        FindReferences();
        EnsureCollider();
    }

    private void OnEnable()
    {
        try { UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable(); } catch {}
        TrainingEventManager.OnScenarioPlaced += HandleScenarioPlaced;
        TrainingEventManager.OnTrainingStarted += HandleScenarioPlaced;
        if (!pickedUp)
        {
            EnsureInitialVisibility();
        }
    }

    private void OnDisable()
    {
        TrainingEventManager.OnScenarioPlaced -= HandleScenarioPlaced;
        TrainingEventManager.OnTrainingStarted -= HandleScenarioPlaced;
    }

    private void Start()
    {
        FindReferences();
        EnsureInitialVisibility();
    }

    public void FindReferences()
    {
        // 1. AR Camera — robust multi-fallback for XR/AR builds
        if (arCamera == null)
        {
            // Try Camera.main first
            if (Camera.main != null)
            {
                arCamera = Camera.main.transform;
            }
            else
            {
                // Fallback: any active camera in scene
                var cam = FindAnyObjectByType<Camera>();
                if (cam != null) arCamera = cam.transform;
            }

            // XR Origin fallback for AR Foundation builds
            if (arCamera == null)
            {
                var xrOrigin = FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
                if (xrOrigin != null && xrOrigin.Camera != null)
                    arCamera = xrOrigin.Camera.transform;
            }
        }

        // 2. Functional Original Extinguisher (FireExt)
        if (originalExtinguisher == null)
        {
            var pickupComp = FindAnyObjectByType<ExtinguisherPickup>(FindObjectsInactive.Include);
            if (pickupComp != null)
            {
                originalExtinguisher = pickupComp.gameObject;
            }
            else
            {
                var go = GameObject.Find("FireExt");
                if (go != null) originalExtinguisher = go;
            }
        }

        // 3. Flow Manager
        if (flowManager == null)
        {
            flowManager = FindAnyObjectByType<FireScenarioFlowManager>(FindObjectsInactive.Include);
        }

        if (arCamera != null)
            Debug.Log($"[ExtinguisherDisplayPickup] AR Camera resolved: '{arCamera.name}'");
        else
            Debug.LogError("[ExtinguisherDisplayPickup] AR Camera could NOT be found! FireExt pickup will fail.");
    }

    private void EnsureCollider()
    {
        if (GetComponent<Collider>() == null && GetComponentInChildren<Collider>() == null)
        {
            BoxCollider col = gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(0.35f, 0.7f, 0.35f);
            col.center = new Vector3(0f, 0.35f, 0f);
        }
    }

    public void EnsureInitialVisibility()
    {
        FindReferences();

        // Display prop is visible
        gameObject.SetActive(true);
        pickedUp = false;

        // Original functional extinguisher is hidden until picked up
        if (originalExtinguisher != null)
        {
            var pickup = originalExtinguisher.GetComponent<ExtinguisherPickup>();
            if (pickup != null)
            {
                pickup.SetRuntimeVisibility(false);
            }
            else
            {
                originalExtinguisher.SetActive(false);
            }
        }
    }

    private void HandleScenarioPlaced()
    {
        EnsureInitialVisibility();
    }

    private void Update()
    {
        if (pickedUp)
            return;

        // =================================================
        // 1. NEW INPUT SYSTEM - TOUCH (Mobile AR)
        // =================================================
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPosition =
                Touchscreen.current.primaryTouch.position.ReadValue();

            TryPickup(touchPosition);
            return;
        }

        // =================================================
        // 2. NEW INPUT SYSTEM - MOUSE (Editor & Simulator)
        // =================================================
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition =
                Mouse.current.position.ReadValue();

            TryPickup(mousePosition);
            return;
        }

        // =================================================
        // 3. EnhancedTouch Fallback (Mobile AR)
        // =================================================
        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
        {
            var touch = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0];
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                TryPickup(touch.screenPosition);
            }
        }
    }

    private void TryPickup(Vector2 screenPosition)
    {
        // Don't intercept UI button taps
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Camera cam = null;
        if (arCamera != null)
            cam = arCamera.GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;
        if (cam == null)
            cam = FindAnyObjectByType<Camera>();

        if (cam == null)
        {
            Debug.LogError("[ExtinguisherDisplayPickup] Camera not found!");
            return;
        }

        Ray ray = cam.ScreenPointToRay(screenPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

        foreach (RaycastHit hit in hits)
        {
            Transform hitObj = hit.transform;

            if (hitObj == transform ||
                hitObj.IsChildOf(transform) ||
                transform.IsChildOf(hitObj) ||
                hitObj.name.ToLower().Contains("fireext") ||
                hitObj.name.ToLower().Contains("display"))
            {
                Pickup();
                return;
            }
        }

        // Screen-space proximity fallback for mobile AR tapping tolerance
        Vector3 extPos = transform.position;
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            extPos = col.bounds.center;
        }
        else
        {
            Renderer rend = GetComponentInChildren<Renderer>();
            if (rend != null) extPos = rend.bounds.center;
        }

        Vector3 extScreen = cam.WorldToScreenPoint(extPos);
        if (extScreen.z > 0f && Vector2.Distance(extScreen, screenPosition) <= 220f)
        {
            Pickup();
        }
    }

    public void ResetDisplay()
    {
        pickedUp = false;
        gameObject.SetActive(true);

        if (originalExtinguisher != null)
        {
            var pickup = originalExtinguisher.GetComponent<ExtinguisherPickup>();
            if (pickup != null)
            {
                if (pickup.IsHeld())
                {
                    pickup.DetachFromCamera();
                }
                pickup.SetRuntimeVisibility(false);
            }
            else
            {
                originalExtinguisher.SetActive(false);
            }
        }
    }

    public void Pickup()
    {
        if (pickedUp)
            return;

        // CRITICAL RULE: Extinguisher MUST NOT be pickable before the fire alarm is activated!
        var flow = flowManager ?? FireScenarioFlowManager.Instance ?? FindAnyObjectByType<FireScenarioFlowManager>(FindObjectsInactive.Include);
        if (flow != null)
        {
            if (flow.CurrentStage != FireScenarioFlowManager.Stage.Step3_SelectExtinguisher)
            {
                flow.HandlePrematureExtinguisherAttempt();
                return;
            }
        }

        FindReferences();

        if (originalExtinguisher == null)
        {
            Debug.LogError("[ExtinguisherDisplayPickup] Original Extinguisher is not assigned or found in scene!");
            return;
        }

        if (arCamera == null && Camera.main != null)
        {
            arCamera = Camera.main.transform;
        }

        if (arCamera == null)
        {
            var cam = FindAnyObjectByType<Camera>();
            if (cam != null) arCamera = cam.transform;
        }

        if (arCamera == null)
        {
            Debug.LogError("[ExtinguisherDisplayPickup] AR Camera is missing!");
            return;
        }

        ExtinguisherPickup pickup =
            originalExtinguisher.GetComponent<ExtinguisherPickup>();

        if (pickup == null)
        {
            Debug.LogError("[ExtinguisherDisplayPickup] ExtinguisherPickup component is missing from originalExtinguisher!");
            return;
        }

        pickedUp = true;

        // Provide AR camera to original
        pickup.arCamera = arCamera;

        // Deactivate display model immediately
        gameObject.SetActive(false);

        Debug.Log("[ExtinguisherDisplayPickup] Display extinguisher tapped -> Valid selection event fired");

        // The authoritative owner for the real extinguisher handoff is FlowManager
        if (flow != null)
        {
            flow.HandleExtinguisherPickedUp();
        }
        else
        {
            // Standalone fallback (e.g. isolated scene without FlowManager):
            pickup.SetRuntimeVisibility(true);
            if (!originalExtinguisher.activeSelf)
            {
                originalExtinguisher.SetActive(true);
            }
            pickup.AttachToCamera();
            TrainingEventManager.RaiseExtinguisherPickedUp();
        }

        // Trigger events
        if (OnPickedUp != null)
        {
            OnPickedUp.Invoke();
        }
    }
}
