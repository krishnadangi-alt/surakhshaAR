using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// AlarmInteraction
/// ================
/// Attached to the fire alarm 3-D prop in the FireTraining scene.
/// Detects a tap / mouse-click on the alarm collider (with a generous
/// screen-space fallback so the small prop is easy to tap on mobile),
/// plays the alarm audio, and raises OnAlarmActivated once.
///
/// Wire-up in the Unity Inspector:
///   • Assign this component to the alarm GameObject (e.g. "FireAlarm")
///   • Assign alarmAudio (optional AudioSource on the same GO)
///   • tapTolerancePixels (default 120) – screen pixel radius forgiveness
/// </summary>
public class AlarmInteraction : MonoBehaviour
{
    [Header("Tap Settings")]
    [Tooltip("Screen pixel radius used as a forgiveness zone when the raycast misses the small alarm collider.")]
    [SerializeField] private float tapTolerancePixels = 120f;
    [SerializeField] private float raycastDistance    = 100f;

    [Header("Audio (optional)")]
    [SerializeField] private AudioSource alarmAudio;
    [SerializeField] private float alarmAudioDuration = 1.5f;

    [Header("Visual Feedback (optional)")]
    [Tooltip("GameObject to show briefly when the alarm is activated (e.g. a flash effect).")]
    [SerializeField] private GameObject activationEffect;

    // ----------------------------------------------------------------
    // Public State
    // ----------------------------------------------------------------
    public bool IsActivated { get; private set; }

    // ----------------------------------------------------------------
    // Events
    // ----------------------------------------------------------------
    /// <summary>Raised once when the worker successfully taps the fire alarm.</summary>
    public UnityEvent OnAlarmActivated = new UnityEvent();

    // ----------------------------------------------------------------
    // Private
    // ----------------------------------------------------------------
    private Camera _arCamera;

    // ----------------------------------------------------------------
    // Initialization
    // ----------------------------------------------------------------
    private void Awake()
    {
        ResolveAudioAndColliders();
    }

    private void Start()
    {
        ResolveAudioAndColliders();
    }

    private void ResolveAudioAndColliders()
    {
        // 1. Resolve AudioSource dynamically from this object or children (e.g. EUFFireAlarm)
        if (alarmAudio == null)
        {
            alarmAudio = GetComponent<AudioSource>()
                      ?? GetComponentInChildren<AudioSource>(true)
                      ?? GetComponentInParent<AudioSource>();
        }

        // 2. CRITICAL: Silence any play-on-awake audio immediately on scene load!
        if (alarmAudio != null)
        {
            alarmAudio.playOnAwake = false;
            if (alarmAudio.isPlaying && !IsActivated)
            {
                alarmAudio.Stop();
            }
        }

        // 3. Ensure a physical collider exists on this transform so raycasts work
        if (GetComponent<Collider>() == null)
        {
            var col = gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(0.5f, 0.5f, 0.5f);
        }

        // Also ensure any child model (like EUFFireAlarm) has a collider
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Collider>() == null && child.GetComponentInChildren<Renderer>() != null)
            {
                var cCol = child.gameObject.AddComponent<BoxCollider>();
                cCol.size = new Vector3(0.4f, 0.4f, 0.4f);
            }
        }
    }

    public void SetAudioSource(AudioSource src)
    {
        alarmAudio = src;
        if (alarmAudio != null)
        {
            alarmAudio.playOnAwake = false;
            if (alarmAudio.isPlaying && !IsActivated)
            {
                alarmAudio.Stop();
            }
        }
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        ResolveAudioAndColliders();
    }

    private void OnDisable()
    {
        // NOTE: intentionally NOT calling EnhancedTouchSupport.Disable() here —
        // other interaction scripts share the support (same pattern as FirePinInteraction).
    }

    private void Update()
    {
        if (IsActivated) return;

        if (_arCamera == null)
            _arCamera = Camera.main;
        if (_arCamera == null) return;

        // 1. New Input System – Touchscreen (primary for Android/AR)
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            CheckAlarmTap(Touchscreen.current.primaryTouch.position.ReadValue());
            return;
        }

        // 2. New Input System – Mouse (Unity Editor / Device Simulator)
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckAlarmTap(Mouse.current.position.ReadValue());
            return;
        }

        // 3. EnhancedTouch fallback
        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                CheckAlarmTap(touch.screenPosition);
            }
        }
    }

    private void CheckAlarmTap(Vector2 screenPosition)
    {
        // Ignore taps that land on a UI element (e.g. guidance card buttons)
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Ray ray = _arCamera.ScreenPointToRay(screenPosition);
        RaycastHit[] hits = Physics.RaycastAll(
            ray, raycastDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Collide);

        foreach (RaycastHit hit in hits)
        {
            Transform t = hit.collider.transform;
            if (t == transform || t.IsChildOf(transform) || transform.IsChildOf(t))
            {
                Debug.Log("[AlarmInteraction] Alarm tapped via raycast.");
                ActivateAlarm();
                return;
            }
        }

        // Screen-space tolerance fallback — very useful on mobile when the
        // alarm model is small and the raycast barely misses the collider.
        if (IsTapNearAlarm(screenPosition))
        {
            Debug.Log("[AlarmInteraction] Alarm tapped via screen-space tolerance.");
            ActivateAlarm();
        }
    }

    private bool IsTapNearAlarm(Vector2 screenPosition)
    {
        Vector3 worldPos = transform.position;

        Collider col = GetComponent<Collider>();
        if (col != null)
            worldPos = col.bounds.center;
        else
        {
            Renderer rend = GetComponentInChildren<Renderer>();
            if (rend != null) worldPos = rend.bounds.center;
        }

        Vector3 screenPoint = _arCamera.WorldToScreenPoint(worldPos);
        if (screenPoint.z <= 0f) return false; // behind camera

        return Vector2.Distance(screenPoint, screenPosition) <= tapTolerancePixels;
    }

    /// <summary>
    /// Activates the fire alarm: plays audio, shows effect, fires the event.
    /// Safe to call multiple times — only activates once.
    /// </summary>
    public void ActivateAlarm()
    {
        if (IsActivated) return;
        IsActivated = true;

        Debug.Log("[AlarmInteraction] ALARM ACTIVATED");

        // Play audio
        if (alarmAudio != null && !alarmAudio.isPlaying)
        {
            alarmAudio.Play();
            if (alarmAudioDuration > 0f)
                Invoke(nameof(StopAlarmAudio), alarmAudioDuration);
        }

        // Show flash / visual effect
        if (activationEffect != null)
        {
            activationEffect.SetActive(true);
        }

        // Notify all listeners (primarily FireScenarioFlowManager)
        OnAlarmActivated?.Invoke();

        // Raise the global event bus as well
        TrainingEventManager.RaiseAlarmActivated();
    }

    private void StopAlarmAudio()
    {
        if (alarmAudio != null && alarmAudio.isPlaying)
            alarmAudio.Stop();
    }

    /// <summary>Resets the alarm state so the scenario can be restarted.</summary>
    public void ResetAlarm()
    {
        IsActivated = false;
        CancelInvoke(nameof(StopAlarmAudio));

        if (alarmAudio != null)
            alarmAudio.Stop();

        if (activationEffect != null)
            activationEffect.SetActive(false);
    }
}
