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
///        Does NOT get cancelled if touch count is 0!
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

    public bool IsGripHeld { get; private set; }

    public UnityEvent OnSprayStarted;
    public UnityEvent OnSprayStopped;
    public UnityEvent OnPinRemovalRequired;

    private Camera arCamera;
    private bool gripHeldBy3DInput = false;

    private void Awake()
    {
    }

    private void Start()
    {
        if (sprayAudio != null)
        {
            sprayAudio.playOnAwake = false;
            sprayAudio.loop = true;
            sprayAudio.Stop();
        }

        if (powderSpray == null)
        {
            powderSpray = GetComponentInChildren<DryPowderSpray>(true)
                       ?? FindAnyObjectByType<DryPowderSpray>();
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

        if (pin == null)
        {
            var pinComp = GetComponentInChildren<FirePinInteraction>(true)
                       ?? FindAnyObjectByType<FirePinInteraction>();
            if (pinComp != null)
            {
                pin = pinComp.gameObject;
            }
        }

        if (powderSpray != null)
        {
            powderSpray.StopSpray();
        }
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        StopGrip();
    }

    private void Update()
    {
        if (arCamera == null)
            arCamera = Camera.main;

        if (arCamera == null)
            return;

        // ── 1. Mobile Touch Input (3D AR Grip) ────────────────────────────────
        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                // Check if touch is on 3D grip (and not on UI)
                if (UnityEngine.EventSystems.EventSystem.current == null ||
                    !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
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

        // ── 2. Mouse Input (Editor & Desktop Testing) ─────────────────────────
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (UnityEngine.EventSystems.EventSystem.current == null ||
                    !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    Vector2 mousePos = Mouse.current.position.ReadValue();
                    if (CheckGripRaycast(mousePos))
                    {
                        gripHeldBy3DInput = true;
                        StartGrip();
                    }
                }
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (gripHeldBy3DInput)
                {
                    gripHeldBy3DInput = false;
                    StopGrip();
                }
            }
        }
    }

    private bool CheckGripRaycast(Vector2 screenPosition)
    {
        if (arCamera == null) return false;

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

            // Grip collider itself or any child collider of Grip
            if (hitTransform == transform || hitTransform.IsChildOf(transform))
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
            StartGrip();
        }
    }

    public void StartGrip()
    {
        if (IsGripHeld)
            return;

        // Safety pin must be removed first
        FirePinInteraction pinComp = null;
        if (pin != null) pinComp = pin.GetComponent<FirePinInteraction>();
        if (pinComp == null)
        {
            pinComp = GetComponentInChildren<FirePinInteraction>(true)
                   ?? FindAnyObjectByType<FirePinInteraction>(FindObjectsInactive.Include);
            if (pinComp != null) pin = pinComp.gameObject;
        }

        bool isPinStillInserted = (pinComp != null && !pinComp.IsPinRemoved()) || (pin != null && pin.activeInHierarchy);
        if (isPinStillInserted)
        {
            Debug.Log("[ExtinguisherGripInteraction] Safety pin is still inserted! Cannot spray.");
            OnPinRemovalRequired?.Invoke();
            return;
        }

        IsGripHeld = true;

        Debug.Log("[ExtinguisherGripInteraction] Grip pressed -> Spray Started");

        if (powderSpray != null)
        {
            powderSpray.StartSpray();
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

        if (sprayAudio != null && sprayAudio.isPlaying)
        {
            sprayAudio.Stop();
        }

        Debug.Log("[ExtinguisherGripInteraction] Grip released -> Spray Stopped");

        OnSprayStopped?.Invoke();
    }
}