using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ExtinguisherDisplayPickup : MonoBehaviour
{
    [Header("Original Functional Extinguisher")]
    public GameObject originalExtinguisher;

    [Header("AR Camera")]
    public Transform arCamera;

    private bool pickedUp = false;

    /// <summary>
    /// Raised the moment the worker taps the display (fake) extinguisher.
    /// The hidden original is then attached to the AR camera.
    /// </summary>
    public UnityEvent OnPickedUp = new UnityEvent();

    void Start()
    {
        // Find AR Camera automatically
        if (arCamera == null && Camera.main != null)
        {
            arCamera = Camera.main.transform;
        }

        // Original is hidden when the app starts
        if (originalExtinguisher != null)
        {
            originalExtinguisher.SetActive(false);
        }
        else
        {
            Debug.LogError(
                "Original Extinguisher is not assigned!"
            );
        }
    }

    void Update()
    {
        if (pickedUp)
            return;

        // =================================================
        // NEW INPUT SYSTEM - TOUCH
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
        // NEW INPUT SYSTEM - MOUSE
        // Editor testing only
        // =================================================

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition =
                Mouse.current.position.ReadValue();

            TryPickup(mousePosition);
        }
    }

    void TryPickup(Vector2 screenPosition)
    {
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Camera cam = null;

        if (arCamera != null)
        {
            cam = arCamera.GetComponent<Camera>();
        }

        if (cam == null)
        {
            cam = Camera.main;
        }

        if (cam == null)
        {
            Debug.LogError("Camera not found!");
            return;
        }

        Ray ray =
            cam.ScreenPointToRay(screenPosition);

        RaycastHit[] hits =
            Physics.RaycastAll(ray, 100f);

        foreach (RaycastHit hit in hits)
        {
            Transform hitObject = hit.transform;

            // Display object or any child or parent
            if (hitObject == transform ||
                hitObject.IsChildOf(transform) ||
                transform.IsChildOf(hitObject))
            {
                Pickup();

                return;
            }
        }

        // Fallback: screen-space tap tolerance (130px) for seamless mobile AR tapping
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
        if (extScreen.z > 0f && Vector2.Distance(extScreen, screenPosition) <= 130f)
        {
            Pickup();
        }
    }

    public void Pickup()
    {
        if (pickedUp)
            return;

        if (originalExtinguisher == null)
        {
            Debug.LogError(
                "Original Extinguisher is not assigned!"
            );
            return;
        }

        if (arCamera == null && Camera.main != null)
        {
            arCamera = Camera.main.transform;
        }

        if (arCamera == null)
        {
            Debug.LogError(
                "AR Camera is missing!"
            );
            return;
        }

        // Get the original's pickup script
        ExtinguisherPickup pickup =
            originalExtinguisher.GetComponent<ExtinguisherPickup>();

        if (pickup == null)
        {
            Debug.LogError(
                "ExtinguisherPickup is missing " +
                "from FireExt_Original!"
            );
            return;
        }

        pickedUp = true;

        // Give original the AR camera
        pickup.arCamera = arCamera;

        // Show original
        originalExtinguisher.SetActive(true);

        // Let original attach itself
        pickup.AttachToCamera();

        // Hide display AFTER original is activated
        gameObject.SetActive(false);

        Debug.Log(
            "DISPLAY TAPPED -> ORIGINAL EXTINGUISHER ACTIVATED"
        );

        if (OnPickedUp != null)
        {
            OnPickedUp.Invoke();
        }
    }
}