using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ExtinguisherGripInteraction : MonoBehaviour
{
    [Header("Safety Pin")]
    [SerializeField] private GameObject pin;

    [Header("Powder Spray")]
    [SerializeField] private DryPowderSpray powderSpray;

    [Header("Spray Audio")]
    [SerializeField] private AudioSource sprayAudio;

    [Header("Touch")]
    [SerializeField] private float raycastDistance = 100f;

    public bool IsGripHeld { get; private set; }

    public UnityEvent OnSprayStarted;
    public UnityEvent OnSprayStopped;
    public UnityEvent OnPinRemovalRequired;

    private Camera arCamera;

    private void Awake()
    {
        arCamera = Camera.main;

        if (arCamera == null)
        {
            Debug.LogError(
                "ExtinguisherGripInteraction: Main Camera not found."
            );
        }
    }

    private void Start()
    {
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
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        StopGrip();
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        if (arCamera == null)
            return;

        if (Touch.activeTouches.Count == 0)
        {
            if (IsGripHeld)
                StopGrip();

            return;
        }

        Touch touch = Touch.activeTouches[0];

        if (touch.phase ==
            UnityEngine.InputSystem.TouchPhase.Began)
        {
            CheckGripTap(touch.screenPosition);
        }

        if (touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Ended ||
            touch.phase ==
                UnityEngine.InputSystem.TouchPhase.Canceled)
        {
            StopGrip();
        }
    }

    private void CheckGripTap(Vector2 screenPosition)
    {
        Ray ray =
            arCamera.ScreenPointToRay(screenPosition);

        RaycastHit[] hits =
            Physics.RaycastAll(
                ray,
                raycastDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide
            );

        foreach (RaycastHit hit in hits)
        {
            Transform hitTransform =
                hit.collider.transform;

            // Grip collider itself.
            if (hitTransform == transform)
            {
                StartGrip();
                return;
            }

            // Any child collider of Grip.
            if (hitTransform.IsChildOf(transform))
            {
                StartGrip();
                return;
            }
        }
    }

    private void StartGrip()
    {
        if (IsGripHeld)
            return;

        // Safety pin must be removed first.
        if (pin != null && pin.activeInHierarchy)
        {
            Debug.Log(
                "SAFETY PIN STILL INSERTED"
            );

            OnPinRemovalRequired?.Invoke();

            return;
        }

        IsGripHeld = true;

        Debug.Log("GRIP PRESSED");
        Debug.Log("SPRAY STARTED");

        if (powderSpray != null)
        {
            powderSpray.StartSpray();
        }

        if (sprayAudio != null)
        {
            if (!sprayAudio.isPlaying)
                sprayAudio.Play();
        }

        OnSprayStarted?.Invoke();
    }

    private void StopGrip()
    {
        if (!IsGripHeld)
            return;

        IsGripHeld = false;

        if (powderSpray != null)
        {
            powderSpray.StopSpray();
        }

        if (sprayAudio != null)
        {
            sprayAudio.Stop();
        }

        Debug.Log("SPRAY STOPPED");

        OnSprayStopped?.Invoke();
    }
}