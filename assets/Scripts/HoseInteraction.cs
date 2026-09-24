using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class HoseInteraction : MonoBehaviour
{
    [Header("References")]
    public Transform hosePivot;
    public Transform sprayPoint;

    [Header("Movement")]
    public bool enableHoseMovement = true;

    [Header("Touch Settings")]
    public float rotationSpeed = 0.35f;

    private Camera arCamera;

    private bool dragging = false;
    private Vector2 lastTouchPosition;

    private Quaternion startPivotRotation;
    private Quaternion startSprayRotation;

    private void Start()
    {
        arCamera = Camera.main;

        if (hosePivot != null)
            startPivotRotation = hosePivot.rotation;

        if (sprayPoint != null)
            startSprayRotation = sprayPoint.rotation;

        if (hosePivot == null)
        {
            Debug.LogWarning(
                "HoseInteraction: Hose Pivot is not assigned."
            );
        }

        if (sprayPoint == null)
        {
            Debug.LogWarning(
                "HoseInteraction: Spray Point is not assigned."
            );
        }
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        if (!enableHoseMovement)
            return;

        if (arCamera == null || hosePivot == null)
            return;

        if (Touch.activeTouches.Count == 0)
        {
            dragging = false;
            return;
        }

        Touch touch = Touch.activeTouches[0];

        // Finger touches the hose
        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            Ray ray = arCamera.ScreenPointToRay(
                touch.screenPosition
            );

            RaycastHit[] hits = Physics.RaycastAll(ray);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.transform == transform ||
                    hit.collider.transform.IsChildOf(transform))
                {
                    dragging = true;
                    lastTouchPosition = touch.screenPosition;
                    break;
                }
            }
        }

        // Finger moves
        if (dragging &&
            touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            Vector2 currentPosition = touch.screenPosition;

            Vector2 delta =
                currentPosition - lastTouchPosition;

            lastTouchPosition = currentPosition;

            RotateNozzle(delta);
        }

        // Finger released
        if (touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Ended ||
            touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Canceled)
        {
            dragging = false;
        }
    }

    private void RotateNozzle(Vector2 delta)
    {
        float horizontal =
            delta.x * rotationSpeed;

        float vertical =
            -delta.y * rotationSpeed;

        hosePivot.Rotate(
            Vector3.up,
            horizontal,
            Space.World
        );

        hosePivot.Rotate(
            Vector3.right,
            vertical,
            Space.Self
        );

        // Make spray direction follow nozzle
        if (sprayPoint != null)
        {
            sprayPoint.rotation = hosePivot.rotation;
        }
    }

    public void ResetHose()
    {
        if (hosePivot != null)
            hosePivot.rotation = startPivotRotation;

        if (sprayPoint != null)
            sprayPoint.rotation = startSprayRotation;

        dragging = false;
    }
}
