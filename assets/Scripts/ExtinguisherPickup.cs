using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExtinguisherPickup : MonoBehaviour
{
    [Header("AR Camera")]
    public Transform arCamera;

    [Header("Position While Holding (Camera-relative, metres)")]
    public Vector3 holdPosition = new Vector3(0.0f, -0.25f, 0.6f);

    [Header("Rotation While Holding (Euler)")]
    public Vector3 holdRotation = new Vector3(0f, 0f, 0f);

    [Header("Hose References")]
    public Transform hose;
    public Transform hosePivot;
    public Transform sprayPoint;

    [Header("Events")]
    public UnityEvent OnAttachedToCamera = new UnityEvent();

    // ─── private state ────────────────────────────────────────────────────────
    private Vector3  sprayPointLocalPosition;
    private bool     isHeld                 = false;
    private bool     runtimeVisible         = true;
    private bool     hasInitializedRuntimeVisibility = false;

    // cached initial scene transform (for clean reset / detach)
    private Transform initialParent;
    private Vector3   initialLocalPosition;
    private Quaternion initialLocalRotation;
    private Vector3   initialLocalScale     = Vector3.one;
    private bool      initialTransformCached = false;


    // =========================================================================
    // UNITY LIFECYCLE
    // =========================================================================

    private void Awake()
    {
        CacheInitialTransform();
        ResolveARCamera();
        FindChildReferences();
        CacheSprayPointPosition();
    }

    private void Start()
    {
        if (!Application.isPlaying)
        {
            SetRuntimeVisibility(true);
            return;
        }

        // Guard: If we are already on Step 4 or beyond, or if held/revealed, do NOT hide or detach!
        var flow = FireScenarioFlowManager.Instance ?? FindAnyObjectByType<FireScenarioFlowManager>(FindObjectsInactive.Include);
        if (flow != null && flow.CurrentStage >= FireScenarioFlowManager.Stage.Step4_RemovePin)
        {
            hasInitializedRuntimeVisibility = true;
            return;
        }

        if (!hasInitializedRuntimeVisibility && !isHeld)
        {
            hasInitializedRuntimeVisibility = true;
            SetRuntimeVisibility(false);
        }
    }

    /// <summary>
    /// Called every late update. When held, positions FireExt directly in front
    /// of the AR camera in WORLD SPACE every frame.
    /// This replaces camera parenting — it is immune to XR Origin / Camera Offset
    /// hierarchy scale/offset distortions that made the extinguisher invisible.
    /// </summary>
    private void LateUpdate()
    {
        if (!isHeld) return;

        // Re-resolve camera if it somehow went null
        if (arCamera == null) ResolveARCamera();
        if (arCamera == null) return;

        // World-space position = camera position + camera-local holdPosition
        transform.position = arCamera.TransformPoint(holdPosition);
        transform.rotation = arCamera.rotation * Quaternion.Euler(holdRotation);
    }


    // ─── editor helpers ───────────────────────────────────────────────────────

    private void OnValidate()
    {
#if UNITY_EDITOR
        // Defer to avoid "SendMessage cannot be called during OnValidate" errors
        // that Unity throws when ParticleSystem.Simulate() internally sends
        // OnParticleUpdateJobScheduled / OnParticleCollision to child objects.
        if (!Application.isPlaying)
            UnityEditor.EditorApplication.delayCall += ForceEditorVisibility;
#endif
    }

    private void Reset()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += ForceEditorVisibility;
#endif
    }

    private void ForceEditorVisibility()
    {
#if UNITY_EDITOR
        if (Application.isPlaying) return;
        // Guard against the object being destroyed before the deferred call fires.
        if (this == null) return;

        foreach (var r in GetComponentsInChildren<Renderer>(true))
            if (r != null) r.enabled = true;

        foreach (var c in GetComponentsInChildren<Collider>(true))
            if (c != null) c.enabled = true;

        foreach (var p in GetComponentsInChildren<FirePinInteraction>(true))
            if (p != null) p.enabled = true;

        foreach (var g in GetComponentsInChildren<ExtinguisherGripInteraction>(true))
            if (g != null) g.enabled = true;

        foreach (var ps in GetComponentsInChildren<ParticleSystem>(true))
        {
            if (ps == null) continue;
            var psRenderer = ps.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null) psRenderer.enabled = true;
            var em = ps.emission;
            em.enabled = true;
            // Simulate without withChildren=true to avoid SendMessage on siblings;
            // each ParticleSystem in the loop handles itself individually.
            ps.Simulate(1.0f, false, true);
        }

        runtimeVisible = true;
#endif
    }


    // =========================================================================
    // VISIBILITY
    // =========================================================================

    public bool IsRuntimeVisible() => runtimeVisible;

    public void SetRuntimeVisibility(bool visible)
    {
        hasInitializedRuntimeVisibility = true;
        runtimeVisible = visible;

        if (visible)
        {
            // Explicitly activate child GameObjects
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.name == "Pin")
                {
                    var p = child.GetComponent<FirePinInteraction>();
                    if (p == null || !p.IsPinRemoved())
                    {
                        child.gameObject.SetActive(true);
                    }
                }
                else
                {
                    child.gameObject.SetActive(true);
                }
            }

            // Ensure Pin has a proper collider if present
            var pin = GetComponentInChildren<FirePinInteraction>(true);
            if (pin != null)
            {
                var pinCol = pin.GetComponent<BoxCollider>();
                if (pinCol == null) pinCol = pin.gameObject.AddComponent<BoxCollider>();
                pinCol.center = new Vector3(0f, 0.01f, 0.45f);
                pinCol.size = new Vector3(0.18f, 0.18f, 0.18f);
                pinCol.isTrigger = false;
                pinCol.enabled = true;

                if (!pin.IsPinRemoved())
                {
                    pin.gameObject.SetActive(true);
                }
            }

            // Ensure Grip has a proper collider if present
            var grip = GetComponentInChildren<ExtinguisherGripInteraction>(true);
            if (grip != null)
            {
                var gripCol = grip.GetComponent<BoxCollider>();
                if (gripCol == null) gripCol = grip.gameObject.AddComponent<BoxCollider>();
                gripCol.center = new Vector3(0f, 0.01f, 0.46f);
                gripCol.size = new Vector3(0.20f, 0.18f, 0.20f);
                gripCol.isTrigger = false;
                gripCol.enabled = true;
                grip.gameObject.SetActive(true);
            }
        }

        // All renderers (MeshRenderer + ParticleSystemRenderer)
        foreach (var r in GetComponentsInChildren<Renderer>(true))
            if (r != null) r.enabled = visible;

        // Colliders
        foreach (var c in GetComponentsInChildren<Collider>(true))
            if (c != null) c.enabled = visible;

        // Interaction scripts
        foreach (var p in GetComponentsInChildren<FirePinInteraction>(true))
            if (p != null) p.enabled = visible;

        foreach (var g in GetComponentsInChildren<ExtinguisherGripInteraction>(true))
            if (g != null) g.enabled = visible;

        // Particle systems: stop when hiding
        if (!visible)
        {
            foreach (var ps in GetComponentsInChildren<ParticleSystem>(true))
            {
                if (ps == null) continue;
                var em = ps.emission;
                em.enabled = false;
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        if (!visible && isHeld)
            DetachFromCamera();
    }


    // =========================================================================
    // CAMERA RESOLUTION (robust multi-fallback for XR / AR Foundation)
    // =========================================================================

    public void ResolveARCamera()
    {
        if (arCamera != null) return;

        // 1. Camera.main (works when AR Camera is tagged MainCamera)
        if (Camera.main != null)
        {
            arCamera = Camera.main.transform;
            return;
        }

        // 2. Any active camera in scene
        var anyCam = FindAnyObjectByType<Camera>();
        if (anyCam != null)
        {
            arCamera = anyCam.transform;
            return;
        }

        // 3. XR Origin camera (AR Foundation)
        var xrOrigin = FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null && xrOrigin.Camera != null)
            arCamera = xrOrigin.Camera.transform;
    }


    // =========================================================================
    // ATTACH / DETACH
    // =========================================================================

    /// <summary>
    /// Marks the extinguisher as held and makes it visible.
    /// LateUpdate() takes over from here, positioning it in front of the
    /// camera every frame in world space — no parenting needed.
    /// </summary>
    public void AttachToCamera()
    {
        if (isHeld)
        {
            // Already held — just make sure it's visible
            SetRuntimeVisibility(true);
            return;
        }

        ResolveARCamera();

        if (arCamera == null)
        {
            Debug.LogError("[ExtinguisherPickup] AR Camera not found! " +
                           "Assign it to ExtinguisherPickup.arCamera in the Inspector.");
            // Still show the extinguisher even without a camera reference
            SetRuntimeVisibility(true);
            return;
        }

        if (!initialTransformCached) CacheInitialTransform();
        FindChildReferences();

        isHeld = true;

        // Make every renderer/collider visible immediately
        SetRuntimeVisibility(true);

        // Position immediately in this frame (LateUpdate will continue every frame)
        transform.position = arCamera.TransformPoint(holdPosition);
        transform.rotation = arCamera.rotation * Quaternion.Euler(holdRotation);

        // Keep original scale so the model looks correct
        transform.localScale = initialLocalScale != Vector3.zero
            ? initialLocalScale
            : Vector3.one;

        UpdateSprayPoint();

        Debug.Log($"[ExtinguisherPickup] ✅ FireExt equipped — following camera '{arCamera.name}' " +
                  $"at offset pos={holdPosition}, rot={holdRotation}");

        OnAttachedToCamera?.Invoke();
    }

    public void DetachFromCamera()
    {
        if (!isHeld) return;

        isHeld = false;

        // Restore to original scene position / hierarchy
        transform.SetParent(initialParent, false);
        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;
        transform.localScale    = initialLocalScale;

        SetRuntimeVisibility(false);

        Debug.Log("[ExtinguisherPickup] FireExt detached and returned to scene position.");
    }


    // =========================================================================
    // INITIAL TRANSFORM CACHE
    // =========================================================================

    public void CacheInitialTransform()
    {
        if (initialTransformCached) return;
        initialParent         = transform.parent;
        initialLocalPosition  = transform.localPosition;
        initialLocalRotation  = transform.localRotation;
        initialLocalScale     = transform.localScale;
        initialTransformCached = true;
    }


    // =========================================================================
    // CHILD REFERENCES
    // =========================================================================

    private void FindChildReferences()
    {
        if (hose == null)
        {
            hose = transform.Find("Hose") ?? transform.Find("hose");
        }

        if (hosePivot == null)
        {
            hosePivot = transform.Find("hosepivot") ?? transform.Find("HosePivot");
        }

        if (sprayPoint == null)
        {
            sprayPoint = transform.Find("spraypoint") ?? transform.Find("SprayPoint");
        }
    }


    // =========================================================================
    // SPRAY POINT
    // =========================================================================

    private void CacheSprayPointPosition()
    {
        if (hose != null && sprayPoint != null)
            sprayPointLocalPosition = hose.InverseTransformPoint(sprayPoint.position);
        else
            Debug.LogWarning("[ExtinguisherPickup] Hose or SprayPoint missing.");
    }

    public void UpdateSprayPoint()
    {
        if (hose == null || sprayPoint == null) return;
        sprayPoint.position = hose.TransformPoint(sprayPointLocalPosition);
    }

    public void RecalculateSprayPoint()
    {
        CacheSprayPointPosition();
        UpdateSprayPoint();
    }


    // =========================================================================
    // HOSE ROTATION
    // =========================================================================

    public Vector3 CurrentHoseRotation =>
        hosePivot != null ? hosePivot.localEulerAngles : Vector3.zero;

    public void SetHoseRotation(Vector3 rotation)
    {
        if (hosePivot == null)
        {
            Debug.LogWarning("[ExtinguisherPickup] HosePivot missing.");
            return;
        }
        hosePivot.localEulerAngles = rotation;
        UpdateSprayPoint();
    }

    public void ResetHoseRotation()
    {
        if (hosePivot == null) return;
        hosePivot.localEulerAngles = Vector3.zero;
        UpdateSprayPoint();
    }


    // =========================================================================
    // STATE QUERIES
    // =========================================================================

    public bool       IsHeld()           => isHeld;
    public Transform  GetHose()          => hose;
    public Transform  GetHosePivot()     => hosePivot;
    public Transform  GetSprayPoint()    => sprayPoint;
}