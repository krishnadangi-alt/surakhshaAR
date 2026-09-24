using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// ExtinguisherGripInteraction
/// ===========================
/// Handles squeezing and releasing the operating lever/grip on the extinguisher.
/// 
/// DUAL ACTIVATION MODES:
///   1. 3D AR Interaction:
///      - Touch (Mobile): Touch and hold the grip collider in AR to spray; releasing stops spray.
///      - Mouse (Editor/PC): Click and hold the grip collider to spray; releasing stops spray.
///   2. UI Action Button (Accessibility & SOP Guidance):
///      - ToggleGrip() / StartGrip(): Taps the "Press Handle & Spray" button to activate continuous spray.
///        Does NOT get cancelled when touch ends!
///      - StopGrip(): Stops spraying when handle is released, button is clicked again, or fire is extinguished.
/// </summary>
public class ExtinguisherGripInteraction : MonoBehaviour
{
    [Header("Safety Pin")]
    [SerializeField] private GameObject pin;

    [Header("Powder Spray")]
    [SerializeField] private DryPowderSpray powderSpray;

    [Header("Spray Audio")]
    [SerializeField] private AudioSource sprayAudio;

    [Header("Touch & Raycast")]
    [SerializeField] private float raycastDistance = 100f;
    [Tooltip("Screen-space distance tolerance in pixels for mobile touch on the extinguisher handle")]
    [SerializeField] private float tapTolerancePixels = 220f;

    public bool IsGripHeld { get; private set; }

    public UnityEvent OnSprayStarted;
    public UnityEvent OnSprayStopped;
    public UnityEvent OnPinRemovalRequired;
    /// <summary>
    /// Raised once per session the first time the grip is successfully activated after pin removal.
    /// FlowManager subscribes to record the grip_activated event and telemetry.
    /// </summary>
    public UnityEvent OnGripActivated = new UnityEvent();

    private Camera arCamera;
    private bool gripHeldBy3DInput = false;
    // Guards OnGripActivated so it fires only once per training session (reset by FlowManager on restart)
    private bool _gripActivatedRaised = false;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        ResolveReferences();

        if (sprayAudio != null)
        {
            sprayAudio.playOnAwake = false;
            sprayAudio.loop = true;
            sprayAudio.Stop();
        }

        if (powderSpray != null)
        {
            powderSpray.StopSpray();
        }
    }

    private void OnEnable()
    {
        try { EnhancedTouchSupport.Enable(); } catch {}
    }

    private void OnDisable()
    {
        StopGrip();
    }

    public void ResolveReferences()
    {
        if (arCamera == null)
            arCamera = Camera.main;

        if (powderSpray == null)
        {
            powderSpray = GetComponentInChildren<DryPowderSpray>(true)
                       ?? transform.root.GetComponentInChildren<DryPowderSpray>(true)
                       ?? FindAnyObjectByType<DryPowderSpray>(FindObjectsInactive.Include);
        }

        if (powderSpray == null)
        {
            var sprayGO = transform.Find("spraypoint/spray")?.gameObject
                       ?? GameObject.Find("spray");
            if (sprayGO != null)
            {
                powderSpray = sprayGO.GetComponent<DryPowderSpray>()
                           ?? sprayGO.AddComponent<DryPowderSpray>();
            }
        }

        if (sprayAudio == null)
        {
            sprayAudio = GetComponentInChildren<AudioSource>(true)
                      ?? powderSpray?.GetComponent<AudioSource>()
                      ?? transform.root.GetComponentInChildren<AudioSource>(true);
        }

        if (pin == null)
        {
            var pinComp = GetComponentInChildren<FirePinInteraction>(true)
                       ?? transform.root.GetComponentInChildren<FirePinInteraction>(true)
                       ?? FindAnyObjectByType<FirePinInteraction>(FindObjectsInactive.Include);
            if (pinComp != null)
            {
                pin = pinComp.gameObject;
            }
        }
    }

    private void Update()
    {
        if (arCamera == null)
            arCamera = Camera.main;

        if (arCamera == null)
            return;

        // ── 1. Touchscreen Input (Mobile AR New Input System) ────────────────
        if (Touchscreen.current != null)
        {
            var primary = Touchscreen.current.primaryTouch;
            if (primary.press.isPressed)
            {
                int touchId = primary.touchId.ReadValue();
                if (!IsPointerOverUI(touchId))
                {
                    if (!IsGripHeld)
                    {
                        Vector2 touchPos = primary.position.ReadValue();
                        if (CheckGripRaycast(touchPos))
                        {
                            gripHeldBy3DInput = true;
                            StartGrip();
                        }
                    }
                }
            }
            else
            {
                if (gripHeldBy3DInput)
                {
                    gripHeldBy3DInput = false;
                    StopGrip();
                }
            }
        }

        // ── 2. EnhancedTouch Fallback (Multi-touch support) ──────────────────
        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began ||
                touch.phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary)
            {
                if (!IsGripHeld && !IsPointerOverUI(touch.touchId))
                {
                    if (CheckGripRaycast(touch.screenPosition))
                    {
                        gripHeldBy3DInput = true;
                        StartGrip();
                    }
                }
            }
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                     touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                if (gripHeldBy3DInput)
                {
                    gripHeldBy3DInput = false;
                    StopGrip();
                }
            }
        }
        else
        {
            if (gripHeldBy3DInput && Touchscreen.current != null && !Touchscreen.current.primaryTouch.press.isPressed)
            {
                gripHeldBy3DInput = false;
                StopGrip();
            }
        }

        // ── 3. Mouse Input (Editor & Desktop Testing) ─────────────────────────
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                if (!IsPointerOverUI(-1))
                {
                    if (!IsGripHeld)
                    {
                        Vector2 mousePos = Mouse.current.position.ReadValue();
                        if (CheckGripRaycast(mousePos))
                        {
                            gripHeldBy3DInput = true;
                            StartGrip();
                        }
                    }
                }
            }
            else
            {
                if (gripHeldBy3DInput)
                {
                    gripHeldBy3DInput = false;
                    StopGrip();
                }
            }
        }
    }

    private static bool IsPointerOverUI(int pointerId)
    {
        if (UnityEngine.EventSystems.EventSystem.current == null)
            return false;

        if (pointerId >= 0)
            return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(pointerId);

        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    private bool CheckGripRaycast(Vector2 screenPosition)
    {
        // In Step 5 (AimBase) or Step 6 (Extinguish), any touch on the screen outside UI is an intentional spray gesture
        var flow = FireScenarioFlowManager.Instance ?? FindAnyObjectByType<FireScenarioFlowManager>(FindObjectsInactive.Include);
        if (flow == null || flow.CurrentStage == FireScenarioFlowManager.Stage.Step6_Extinguish || flow.CurrentStage == FireScenarioFlowManager.Stage.Step5_AimBase)
        {
            return true;
        }

        if (arCamera == null) arCamera = Camera.main;
        if (arCamera == null) return false;

        // 1. 3D Raycast
        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            raycastDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Collide
        );

        foreach (RaycastHit hit in hits)
        {
            Transform hitTransform = hit.collider.transform;

            // Match Grip itself, child, or parent extinguisher
            if (hitTransform == transform || hitTransform.IsChildOf(transform) || transform.IsChildOf(hitTransform))
            {
                return true;
            }

            // Match any extinguisher / handle component in parent hierarchy
            if (hitTransform.GetComponentInParent<ExtinguisherGripInteraction>() != null ||
                hitTransform.GetComponentInParent<ExtinguisherPickup>() != null)
            {
                return true;
            }

            string hName = hitTransform.name.ToLower();
            if (hName.Contains("grip") || hName.Contains("handle") || hName.Contains("lever") ||
                hName.Contains("fireext") || hName.Contains("extinguisher"))
            {
                return true;
            }
        }

        // 2. Screen-space proximity fallback for mobile touch comfort
        Vector3 gripWorldPos = transform.position;
        Collider col = GetComponent<Collider>();
        if (col != null && col.bounds.size.sqrMagnitude > 0.0001f)
            gripWorldPos = col.bounds.center;
        else
            gripWorldPos = transform.TransformPoint(new Vector3(0f, 0.01f, 0.46f));

        Vector3 screenPoint = arCamera.WorldToScreenPoint(gripWorldPos);
        if (screenPoint.z > 0f)
        {
            float dist = Vector2.Distance(screenPoint, screenPosition);
            if (dist <= tapTolerancePixels)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Toggles the spray handle on or off (used by the guidance card CTA button).
    /// </summary>
    public void ToggleGrip()
    {
        if (IsGripHeld)
        {
            gripHeldBy3DInput = false;
            StopGrip();
        }
        else
        {
            gripHeldBy3DInput = false; // Triggered by UI button, do NOT cancel on touch release
            StartGrip();
        }
    }

    public void StartGrip()
    {
        if (IsGripHeld)
            return;

        ResolveReferences();

        // ── Check Safety Pin Removal ──────────────────────────────────────────
        bool pinRemoved = false;

        // 1. Check FlowManager progression — only strictly past Step 4 (Step 5 AimBase, Step 6 Extinguish)
        var flow = FireScenarioFlowManager.Instance ?? FindAnyObjectByType<FireScenarioFlowManager>(FindObjectsInactive.Include);
        if (flow != null)
        {
            int curStageOrdinal = (int)flow.CurrentStage;
            int pastPinStageOrdinal = (int)FireScenarioFlowManager.Stage.Step5_AimBase;
            if (curStageOrdinal >= pastPinStageOrdinal)
            {
                pinRemoved = true;
            }
        }

        // 2. Check if ANY pin in the scene has been removed
        if (!pinRemoved)
        {
            var allPins = Object.FindObjectsByType<FirePinInteraction>(FindObjectsInactive.Include);
            foreach (var p in allPins)
            {
                if (p != null && p.IsPinRemoved())
                {
                    pinRemoved = true;
                    break;
                }
            }
        }

        // 3. Check specific assigned pin component
        if (!pinRemoved)
        {
            FirePinInteraction pinComp = null;
            if (pin != null) pinComp = pin.GetComponent<FirePinInteraction>();
            if (pinComp == null)
            {
                pinComp = GetComponentInChildren<FirePinInteraction>(true)
                       ?? transform.root.GetComponentInChildren<FirePinInteraction>(true)
                       ?? FindAnyObjectByType<FirePinInteraction>(FindObjectsInactive.Include);
                if (pinComp != null) pin = pinComp.gameObject;
            }

            if (pinComp != null)
            {
                pinRemoved = pinComp.IsPinRemoved();
            }
            else if (pin != null)
            {
                pinRemoved = !pin.activeInHierarchy;
            }
        }

        if (!pinRemoved)
        {
            Debug.Log("[ExtinguisherGripInteraction] Safety pin is still inserted! Cannot spray.");
            OnPinRemovalRequired?.Invoke();
            if (flow != null)
            {
                flow.HandlePrematureGripAttempt();
            }
            return;
        }

        // Ensure visual pin is deactivated
        if (pin != null && pin.activeSelf)
        {
            pin.SetActive(false);
        }

        IsGripHeld = true;
        Debug.Log("[ExtinguisherGripInteraction] Grip pressed -> Spray Started");

        // Raise OnGripActivated once per session (first successful grip after pin removal)
        if (!_gripActivatedRaised)
        {
            _gripActivatedRaised = true;
            OnGripActivated?.Invoke();
        }

        // ── Activate Visual & Collision Spray ────────────────────────────────────────
        if (powderSpray == null)
        {
            ResolveReferences();
        }

        if (powderSpray != null)
        {
            powderSpray.StartSpray();
        }

        // Fallback: Also search for any DryPowderSpray or ParticleSystem on the extinguisher
        var allPowders = transform.root.GetComponentsInChildren<DryPowderSpray>(true);
        foreach (var ps in allPowders)
        {
            if (ps != null && ps != powderSpray)
            {
                ps.StartSpray();
            }
        }

        if (sprayAudio != null && !sprayAudio.isPlaying)
        {
            sprayAudio.Play();
        }

        OnSprayStarted?.Invoke();
    }

    public void StopGrip()
    {
        if (!IsGripHeld)
            return;

        IsGripHeld = false;
        gripHeldBy3DInput = false;

        if (powderSpray != null)
        {
            powderSpray.StopSpray();
        }

        var allPowders = transform.root.GetComponentsInChildren<DryPowderSpray>(true);
        foreach (var ps in allPowders)
        {
            if (ps != null && ps != powderSpray)
            {
                ps.StopSpray();
            }
        }

        if (sprayAudio != null && sprayAudio.isPlaying)
        {
            sprayAudio.Stop();
        }

        // Immediately reset spray collision timer to 0 on grip release
        var sprayCol = GetComponentInChildren<ExtinguisherSprayCollision>(true)
                    ?? transform.root.GetComponentInChildren<ExtinguisherSprayCollision>(true)
                    ?? FindAnyObjectByType<ExtinguisherSprayCollision>(FindObjectsInactive.Include);
        if (sprayCol != null)
        {
            sprayCol.ResetCollisionTimer();
        }

        Debug.Log("[ExtinguisherGripInteraction] Grip released -> Spray Stopped & Timer Reset to 0");

        OnSprayStopped?.Invoke();
    }

    /// <summary>
    /// Resets the grip activation guard so OnGripActivated fires again on the next scenario run.
    /// Call from FireScenarioFlowManager.RestartScenario().
    /// </summary>
    public void ResetGripSession()
    {
        _gripActivatedRaised = false;
    }
}
