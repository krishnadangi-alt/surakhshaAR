using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ExtinguisherSprayCollision
/// ==========================
/// Responsible for collision and contact detection between the extinguisher spray and the fire.
/// 
/// ARCHITECTURE: VISIBLE SPRAY != COLLISION DETECTION
///   1. Visual Spray (SprayVisual):
///      - Collision = OFF, Renderer = ON.
///      - Originates at the nozzle, shoots straight forward along nozzle forward axis.
///      - Never distorted, bounced, or culled by colliders or AR planes.
///
///   2. Collision Detector (SprayCollision / Aim Cone):
///      - Collision = ON, Renderer = OFF (invisible probe).
///      - Offset slightly forward along nozzle to avoid self-collision with extinguisher body/hose.
///      - Physics hit triggers OnParticleCollision(GameObject other).
///
///   3. Directional Nozzle Cone Check (Tier 2 Robust AR Validation):
///      - Origin: actual nozzle tip (spraypoint).
///      - Direction: nozzle.forward.
///      - Target: fireExtinguishable.FireWorldPosition (actual base of the fire).
///      - Range <= 5.0m, Cone Angle <= 35 degrees (with Camera fallback).
///
/// 10-SECOND CONTINUOUS SPRAY LOGIC:
///   - 10 seconds of valid continuous contact is required to extinguish the fire.
///   - If aim is broken or handle is released, timer resets to 0.
///   - When 10s is reached -> ExtinguishAll() is called -> fire is extinguished.
/// </summary>
public class ExtinguisherSprayCollision : MonoBehaviour
{
    [Header("Particle System References")]
    [Tooltip("Visual particle system - collision is strictly disabled")]
    [SerializeField] private ParticleSystem visualParticles;
    [Tooltip("Invisible collision probe - collision is enabled")]
    [SerializeField] private ParticleSystem collisionParticles;

    [Header("Nozzle Origin")]
    [SerializeField] private Transform sprayOrigin;

    [Header("Fire Targets")]
    [SerializeField] private FireExtinguishable fireExtinguishable;
    [SerializeField] private FireExtinguishSystem fireSystem;

    [Header("Extinguishing Settings")]
    [SerializeField] private float requiredTime = 10f;
    [Tooltip("Grace period in seconds for momentary single-frame optical tracking jitter.")]
    [SerializeField] private float collisionGracePeriod = 0.35f;

    [Header("Aim Detection (Nozzle to FireBase Cone)")]
    [SerializeField] private float aimMaxDistance = 5.0f;
    [SerializeField] private float aimMaxAngleDeg = 35.0f;

    // Runtime state
    private float contactTimer = 0f;
    private float lastHitTime = -999f;
    private bool isTouchingFire = false;
    private bool isExtinguished = false;

    private Transform fireTargetTransform;
    private Transform cameraTransform;
    private FirePinInteraction pinInteraction;
    private ExtinguisherGripInteraction gripInteraction;

    // Public accessors
    public float ContactTimer => contactTimer;
    public float RequiredTime => requiredTime;
    public float RemainingTime => Mathf.Max(0f, requiredTime - contactTimer);
    public float Progress01 => requiredTime > 0f ? Mathf.Clamp01(contactTimer / requiredTime) : 1f;
    public bool IsTouchingFire => isTouchingFire;
    public bool IsExtinguished => isExtinguished;

    private void Awake()
    {
        ResolveParticlesAndOrigin();
        ConfigureCollisionDetector();
    }

    private void Start()
    {
        ResolveReferences();
        if (fireExtinguishable != null)
            fireExtinguishable.RegisterSprayCollision(this);
    }

    private void OnEnable()
    {
        isExtinguished = false;
        contactTimer = 0f;
        isTouchingFire = false;
        lastHitTime = -999f;
    }

    private void Update()
    {
        if (isExtinguished)
            return;

        if (fireExtinguishable == null || fireTargetTransform == null || pinInteraction == null || gripInteraction == null)
            ResolveReferences();

        // 5-Point Continuous Contact Rule:
        // 1. Safety pin removed
        bool pinRemoved = pinInteraction == null || pinInteraction.IsPinRemoved();

        // 2. Extinguisher handle pressed/held
        bool handleHeld = gripInteraction == null || gripInteraction.IsGripHeld;

        // 3. Spray particles actively emitting
        bool isSpraying = (visualParticles != null && visualParticles.isPlaying) ||
                          (collisionParticles != null && collisionParticles.isPlaying);

        // 4. Physical particle collision within grace period (0.35s)
        bool physicsHit = isSpraying && (Time.time - lastHitTime <= collisionGracePeriod);

        // 5. Directional aim cone hit from actual nozzle to fire base
        bool aimHit = isSpraying && CheckAimAtFire();

        // VALID EXTINGUISHING = PIN REMOVED + HANDLE HELD + SPRAY ACTIVE + (PHYSICS HIT OR CORRECT AIM)
        bool validContact = pinRemoved && handleHeld && isSpraying && (physicsHit || aimHit);
        isTouchingFire = validContact;

        // 10-Second Continuous contact accumulation & reset
        if (validContact)
        {
            contactTimer += Time.deltaTime;
            if (contactTimer >= requiredTime)
            {
                contactTimer = requiredTime;
                ExtinguishAll();
                return;
            }
        }
        else
        {
            // Contact broken: timer resets to 0 according to continuous extinguishing requirement
            contactTimer = 0f;
        }

        // Notify FireExtinguishable so HUD displays live continuous timer & progress
        if (fireExtinguishable != null)
            fireExtinguishable.NotifyParticleCollision(isTouchingFire, contactTimer, requiredTime);

        if (fireSystem != null)
            fireSystem.SetSprayHittingFire(isTouchingFire);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Unity Particle Physics Collision Callbacks
    // ─────────────────────────────────────────────────────────────────────────
    private void OnParticleCollision(GameObject other)
    {
        HandleParticleCollision(other);
    }

    public void HandleParticleCollision(GameObject other)
    {
        if (isExtinguished || other == null)
            return;

        if (IsFireOrHazardObject(other))
        {
            lastHitTime = Time.time;
        }
    }

    public void ReceiveCollisionFromFire()
    {
        if (!isExtinguished)
            lastHitTime = Time.time;
    }

    private bool IsFireOrHazardObject(GameObject other)
    {
        if (other == null) return false;

        if (fireTargetTransform != null)
        {
            if (other.transform == fireTargetTransform ||
                other.transform.IsChildOf(fireTargetTransform) ||
                fireTargetTransform.IsChildOf(other.transform))
            {
                return true;
            }
        }

        if (fireExtinguishable != null)
        {
            if (other.transform == fireExtinguishable.transform ||
                other.transform.IsChildOf(fireExtinguishable.transform) ||
                other.GetComponentInParent<FireExtinguishable>() != null)
            {
                return true;
            }
        }

        if (other.GetComponentInParent<FireExtinguishSystem>() != null)
            return true;

        Transform t = other.transform;
        while (t != null)
        {
            string n = t.name.ToLower();
            if (n.Contains("fire") || n.Contains("flame") || n.Contains("hazard") ||
                n.Contains("target") || n.Contains("electric") || n.Contains("box"))
            {
                return true;
            }
            t = t.parent;
        }

        return false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Aim Cone Check: Actual Nozzle to Fire Base
    // ─────────────────────────────────────────────────────────────────────────
    private bool CheckAimAtFire()
    {
        Vector3 targetPos = Vector3.zero;
        if (fireTargetTransform != null)
            targetPos = fireTargetTransform.position;
        else if (fireExtinguishable != null)
            targetPos = fireExtinguishable.FireWorldPosition;
        else
            return false;

        // Nozzle Transform (actual spray origin)
        Transform nozzle = sprayOrigin != null ? sprayOrigin : transform;
        Vector3 nozzlePos = nozzle.position;
        Vector3 toTarget = targetPos - nozzlePos;
        float dist = toTarget.magnitude;

        // Realistic distance check: fire must be within 0.3m and 5.0m
        if (dist < 0.3f || dist > aimMaxDistance)
            return false;

        Vector3 toTargetDir = toTarget.normalized;
        float nozzleAngle = Vector3.Angle(nozzle.forward, toTargetDir);
        float nozzleReverseAngle = Vector3.Angle(-nozzle.forward, toTargetDir);
        float minNozzleAngle = Mathf.Min(nozzleAngle, nozzleReverseAngle);

        if (minNozzleAngle <= aimMaxAngleDeg)
            return true;

        // Camera look aim towards fire as AR fallback
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform != null)
        {
            Vector3 camToTarget = (targetPos - cameraTransform.position).normalized;
            float camAngle = Vector3.Angle(cameraTransform.forward, camToTarget);
            if (camAngle <= (aimMaxAngleDeg + 5f) && dist <= aimMaxDistance)
                return true;
        }

        return false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Extinguish All
    // ─────────────────────────────────────────────────────────────────────────
    private void ExtinguishAll()
    {
        if (isExtinguished) return;
        isExtinguished = true;
        isTouchingFire = false;
        contactTimer = requiredTime;

        if (fireExtinguishable == null)
            ResolveReferences();

        if (fireExtinguishable != null && !fireExtinguishable.IsExtinguished)
            fireExtinguishable.ExtinguishFire();

        if (fireSystem != null && !fireSystem.IsExtinguished())
        {
            var method = fireSystem.GetType().GetMethod(
                "ExtinguishFire",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public);
            method?.Invoke(fireSystem, null);
        }

        Debug.Log("[SurakshaAR] ExtinguisherSprayCollision: 10s continuous spray complete -> FIRE EXTINGUISHED");
    }

    public void ResetCollisionTimer()
    {
        isExtinguished = false;
        isTouchingFire = false;
        contactTimer = 0f;
        lastHitTime = -999f;
    }

    private void OnParticleSystemStopped()
    {
        isTouchingFire = false;
        contactTimer = 0f;
        if (fireExtinguishable != null)
            fireExtinguishable.NotifyParticleCollision(false, 0f, requiredTime);
        if (fireSystem != null)
            fireSystem.SetSprayHittingFire(false);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Setup: Separation of Visual Spray & Collision Probe
    // ─────────────────────────────────────────────────────────────────────────
    private void ResolveParticlesAndOrigin()
    {
        // Check if this GameObject is the visual spray or the probe
        var thisPS = GetComponent<ParticleSystem>();
        var thisRenderer = GetComponent<ParticleSystemRenderer>();

        if (thisRenderer != null && thisRenderer.enabled)
        {
            // This is the VISUAL spray
            visualParticles = thisPS;
        }
        else if (thisPS != null)
        {
            // This is the collision probe
            collisionParticles = thisPS;
        }

        // Auto-find nozzle origin
        if (sprayOrigin == null)
        {
            if (transform.parent != null && transform.parent.name.ToLower().Contains("spraypoint"))
                sprayOrigin = transform.parent;
            else
                sprayOrigin = transform;
        }

        // If collisionParticles is null, look for child or sibling "SprayCollision"
        if (collisionParticles == null && sprayOrigin != null)
        {
            var probeChild = sprayOrigin.Find("SprayCollision");
            if (probeChild != null)
            {
                collisionParticles = probeChild.GetComponent<ParticleSystem>();
            }
        }

        // If visualParticles is null, look for child or sibling "spray"
        if (visualParticles == null && sprayOrigin != null)
        {
            var sprayChild = sprayOrigin.Find("spray");
            if (sprayChild != null)
            {
                visualParticles = sprayChild.GetComponent<ParticleSystem>();
            }
        }
    }

    private void ConfigureCollisionDetector()
    {
        // 1. Strictly enforce: VISUAL SPRAY COLLISION IS OFF!
        if (visualParticles != null)
        {
            var visCol = visualParticles.collision;
            visCol.enabled = false;

            var visMain = visualParticles.main;
            visMain.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
        }

        // 2. Configure the invisible collision probe
        if (collisionParticles != null)
        {
            var col = collisionParticles.collision;
            col.enabled = true;
            col.type = ParticleSystemCollisionType.World;
            col.mode = ParticleSystemCollisionMode.Collision3D;
            col.collidesWith = ~0; // All layers for world detection
            col.sendCollisionMessages = true;
            col.quality = ParticleSystemCollisionQuality.High;
            col.dampen = 0.5f;
            col.bounce = 0f; // ZERO bounce: prevents particles from ricocheting back!
            col.radiusScale = 0.25f;

            // Ensure probe is completely invisible
            var r = collisionParticles.GetComponent<ParticleSystemRenderer>();
            if (r != null)
                r.enabled = false;

            // Ensure probe forwards collision events to this script if on a different GameObject
            if (collisionParticles.gameObject != this.gameObject)
            {
                var forwarder = collisionParticles.GetComponent<SprayCollisionProbe>();
                if (forwarder == null)
                    forwarder = collisionParticles.gameObject.AddComponent<SprayCollisionProbe>();
                forwarder.master = this;
            }
        }
    }

    private void ResolveReferences()
    {
        if (visualParticles == null || collisionParticles == null || sprayOrigin == null)
            ResolveParticlesAndOrigin();

        if (fireExtinguishable == null)
        {
            fireExtinguishable = FindAnyObjectByType<FireExtinguishable>();
            if (fireExtinguishable != null)
                fireExtinguishable.RegisterSprayCollision(this);
        }

        if (fireSystem == null)
            fireSystem = FindAnyObjectByType<FireExtinguishSystem>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (pinInteraction == null)
            pinInteraction = GetComponentInParent<FirePinInteraction>() ?? FindAnyObjectByType<FirePinInteraction>(FindObjectsInactive.Include);

        if (gripInteraction == null)
            gripInteraction = GetComponentInParent<ExtinguisherGripInteraction>() ?? FindAnyObjectByType<ExtinguisherGripInteraction>(FindObjectsInactive.Include);

        if (fireTargetTransform == null)
            fireTargetTransform = FindFireTarget();

        if (fireTargetTransform != null)
            EnsureColliderOnTarget(fireTargetTransform.gameObject);
    }

    private Transform FindFireTarget()
    {
        var go = GameObject.Find("VFX_Fire_01_Small")
              ?? GameObject.Find("ExtinguisherTarget")
              ?? GameObject.Find("Flames");

        if (go == null)
        {
            var hazard = GameObject.Find("Hazard") ?? GameObject.Find("Electric Box");
            if (hazard != null)
            {
                go = hazard.transform.Find("Electric Box/VFX_Fire_01_Small")?.gameObject
                  ?? hazard.transform.Find("VFX_Fire_01_Small")?.gameObject
                  ?? hazard.transform.Find("Electric Box/ExtinguisherTarget")?.gameObject
                  ?? hazard;
            }
        }

        if (go == null && fireExtinguishable != null)
            go = fireExtinguishable.gameObject;

        return go != null ? go.transform : null;
    }

    private static void EnsureColliderOnTarget(GameObject go)
    {
        if (go == null) return;
        if (go.GetComponent<Collider>() != null) return;

        var sc = go.AddComponent<SphereCollider>();
        sc.radius = 0.85f;
        sc.isTrigger = false;
    }
}

/// <summary>
/// Helper probe component attached to the invisible SprayCollision GameObject to forward
/// OnParticleCollision events directly to ExtinguisherSprayCollision.
/// </summary>
public class SprayCollisionProbe : MonoBehaviour
{
    public ExtinguisherSprayCollision master;

    private void OnParticleCollision(GameObject other)
    {
        if (master != null)
        {
            master.HandleParticleCollision(other);
        }
    }
}