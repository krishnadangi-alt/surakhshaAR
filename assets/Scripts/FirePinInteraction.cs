using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class FirePinInteraction : MonoBehaviour
{
    [Header("Extinguisher Movement")]
    public ExtinguisherAutoMove extinguisherAutoMove;

    private Camera arCamera;
    private bool pinRemoved = false;

    /// <summary>
    /// Raised the moment the safety pin is removed.
    /// </summary>
    public UnityEvent OnPinRemoved = new UnityEvent();

    private void Start()
    {
        arCamera = Camera.main;

        if (arCamera == null)
        {
            Debug.LogError("FirePinInteraction: AR Camera not found.");
        }

        if (extinguisherAutoMove == null)
        {
            Debug.LogWarning(
                "FirePinInteraction: Extinguisher Auto Move is not assigned."
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
        if (pinRemoved)
            return;

        if (arCamera == null)
            return;

        if (Touch.activeTouches.Count == 0)
            return;

        Touch touch = Touch.activeTouches[0];

        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
            return;

        Ray ray = arCamera.ScreenPointToRay(
            touch.screenPosition
        );

        RaycastHit[] hits = Physics.RaycastAll(ray);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == gameObject)
            {
                RemovePin();
                break;
            }
        }
    }

    private void RemovePin()
    {
        if (pinRemoved)
            return;

        pinRemoved = true;

        // Remove the visible safety pin
        gameObject.SetActive(false);

        Debug.Log("equipment_selected");
        Debug.Log("Safety pin removed. Extinguisher is READY.");

        if (OnPinRemoved != null)
        {
            OnPinRemoved.Invoke();
        }

        // Automatically move extinguisher
        if (extinguisherAutoMove != null)
        {
            extinguisherAutoMove.MoveToTarget();
        }
    }
}