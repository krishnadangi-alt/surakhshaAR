using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class FirePinInteraction : MonoBehaviour
{
    [Header("Extinguisher Movement")]
    [SerializeField] private ExtinguisherAutoMove extinguisherAutoMove;

    [Header("Pin Settings")]
    [SerializeField] private float raycastDistance = 100f;

    [Header("Tap Forgiveness")]
    [Tooltip("If the ray misses the small pin collider, a tap within this many screen pixels of the pin still counts.")]
    [SerializeField] private float tapTolerancePixels = 200f;

    private Camera arCamera;
    private bool pinRemoved = false;

    public UnityEvent OnPinRemoved = new UnityEvent();

    private void Awake()
    {
        if (extinguisherAutoMove == null)
        {
            extinguisherAutoMove = FindAnyObjectByType<ExtinguisherAutoMove>();
        }
    }

    private void Start()
    {
        if (extinguisherAutoMove == null)
        {
            Debug.LogWarning(
                "FirePinInteraction: ExtinguisherAutoMove is not assigned."
            );
        }
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
    }

    private void Update()
    {
        if (pinRemoved)
            return;

        if (arCamera == null)
            arCamera = Camera.main;

        if (arCamera == null)
            return;

        // 1. Touchscreen input
        if (UnityEngine.InputSystem.Touchscreen.current != null &&
            UnityEngine.InputSystem.Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPos = UnityEngine.InputSystem.Touchscreen.current.primaryTouch.position.ReadValue();
            CheckPinTap(touchPos);
            return;
        }

        // 2. Mouse input (Editor/Simulator testing)
        if (UnityEngine.InputSystem.Mouse.current != null &&
            UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            CheckPinTap(mousePos);
            return;
        }

        // 3. EnhancedTouch fallback
        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                CheckPinTap(touch.screenPosition);
            }
        }
    }

    private void CheckPinTap(Vector2 screenPosition)
    {
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Ray ray =
            arCamera.ScreenPointToRay(screenPosition);

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            raycastDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Collide
        );

        foreach (RaycastHit hit in hits)
        {
            Transform hitTransform =
                hit.collider.transform;

            // Accept:
            // 1. The pin itself
            // 2. Any child of the pin
            // 3. Any hit named "Pin"
            if (hitTransform == transform ||
                hitTransform.IsChildOf(transform) ||
                hitTransform.name.IndexOf("pin", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Debug.Log("PIN TAP DETECTED (raycast)");
                RemovePin();
                return;
            }
        }

        // Fallback: the ray missed the very small pin collider, but the
        // touch may still be close enough in screen space.
        if (IsTapNearPin(screenPosition))
        {
            Debug.Log("PIN TAP DETECTED (screen tolerance)");
            RemovePin();
        }
    }

    private bool IsTapNearPin(Vector2 screenPosition)
    {
        // 1. Check collider bounds center
        Vector3 pinWorld = transform.position;
        Collider pinCollider = GetComponent<Collider>();
        if (pinCollider != null && pinCollider.bounds.size.sqrMagnitude > 0.0001f)
        {
            pinWorld = pinCollider.bounds.center;
        }
        else
        {
            Renderer pinRenderer = GetComponentInChildren<Renderer>();
            if (pinRenderer != null && pinRenderer.bounds.size.sqrMagnitude > 0.0001f)
                pinWorld = pinRenderer.bounds.center;
            else
                pinWorld = transform.TransformPoint(new Vector3(0f, 0.01f, 0.45f));
        }

        Vector3 screenPoint = arCamera.WorldToScreenPoint(pinWorld);

        // Behind the camera - cannot be tapped.
        if (screenPoint.z <= 0f)
            return false;

        float distance = Vector2.Distance(screenPoint, screenPosition);
        if (distance <= tapTolerancePixels)
            return true;

        // Also check direct local pin head position
        Vector3 pinHeadWorld = transform.TransformPoint(new Vector3(0f, 0.01f, 0.45f));
        Vector3 headScreenPoint = arCamera.WorldToScreenPoint(pinHeadWorld);
        if (headScreenPoint.z > 0f)
        {
            if (Vector2.Distance(headScreenPoint, screenPosition) <= tapTolerancePixels)
                return true;
        }

        return false;
    }

    public void RemovePin()
    {
        if (pinRemoved)
            return;

        var flow = FireScenarioFlowManager.Instance ?? FindAnyObjectByType<FireScenarioFlowManager>(FindObjectsInactive.Include);
        if (flow != null)
        {
            if (flow.CurrentStage != FireScenarioFlowManager.Stage.Step4_RemovePin)
            {
                flow.HandlePrematurePinAttempt();
                return;
            }
        }

        pinRemoved = true;

        Debug.Log("PIN REMOVED");
        Debug.Log("equipment_selected");
        Debug.Log("Safety pin removed. Extinguisher is READY.");

        // Hide the pin GameObject
        gameObject.SetActive(false);

        // Also synchronize any sibling pin instances in the scene
        var allPins = Object.FindObjectsByType<FirePinInteraction>(FindObjectsInactive.Include);
        foreach (var p in allPins)
        {
            if (p != null && p != this)
            {
                p.pinRemoved = true;
                p.gameObject.SetActive(false);
            }
        }

        // Tell other systems.
        OnPinRemoved?.Invoke();

        // Move extinguisher if configured (only if not held by camera).
        if (extinguisherAutoMove != null)
        {
            var pickup = GetComponentInParent<ExtinguisherPickup>();
            if (pickup == null || !pickup.IsHeld())
            {
                extinguisherAutoMove.MoveToTarget();
            }
        }
    }

    public bool IsPinRemoved()
    {
        return pinRemoved;
    }

    public void ResetPin()
    {
        pinRemoved = false;
        gameObject.SetActive(true);
        var r = GetComponent<Renderer>();
        if (r != null) r.enabled = true;
        var c = GetComponent<Collider>();
        if (c != null) c.enabled = true;
        enabled = true;
    }
}