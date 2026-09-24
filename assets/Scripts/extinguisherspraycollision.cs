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
    [Tooltip("Visual particle system - sends collision messages without culling or bouncing particles")]
    [SerializeField] private ParticleSystem visualParticles;
    [Tooltip("Invisible collision probe - collision is enabled")]
    [SerializeField] private ParticleSystem collisionParticles;

    [Header("Nozzle Origin & Spray Source")]
    [SerializeField] private Transform sprayOrigin;
    [SerializeField] private DryPowderSpray powderSpray;

    [Header("Fire Targets")]
    [SerializeField] private FireExtinguishable fireExtinguishable;
    [SerializeField] private FireExtinguishSystem fireSystem;

    [Header("Extinguishing Settings")]
    [SerializeField] private float requiredTime = 10f;
    [Tooltip("Grace period in seconds for mobile AR frame pacing and sweep transitions.")]
    [SerializeField] private float collisionGracePeriod = 2.5f;

    [Header("Aim Detection (Nozzle & Camera to Fire Base)")]
    [SerializeField] private float aimMaxDistance = 25.0f;
    [SerializeField] private float aimMaxAngleDeg = 65.0f;

    // Runtime state
    private float contactTimer = 0f;
    private float lastHitTime = -999f;
    private float timeSinceContactLost = 0f;
    private bool isTouchingFire = false;
    private bool isExtinguished = false;
    // Previous-frame contact state for transition detection
    private bool _wasTouchingFire = false;
    private bool _interruptionEventRaised = false;

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
        requiredTime = 10f;
        aimMaxDistance = Mathf.Max(aimMaxDistance, 25.0f);
        aimMaxAngleDeg = Mathf.Max(aimMaxAngleDeg, 65.0f);
        ResolveParticlesAndOrigin();
        ConfigureCollisionDetector();
    }

    private void Start()
    {
        ResolveReferences();
        if (fireExtinguishable != null)
            fireExtinguishable.RegisterSprayCollision(this);
    }

    public void ResetCollisionTimer()
    {
        isExtinguished = false;
        contactTimer = 0f;
        timeSinceContactLost = 0f;
        isTouchingFire = false;
        lastHitTime = -999f;
        if (fireExtinguishable != null)
            fireExtinguishable.NotifyParticleCollision(false, 0f, requiredTime);
    }

    private void OnEnable()
    {
        ResetCollisionTimer();
    }

    private void Update()
    {
        if (isExtinguished)
            return;

        if (fireExtinguishable == null || fireTargetTransform == null || pinInteraction == null || gripInteraction == null || powderSpray == null)
            ResolveReferences();

        // 1. Spray particles actively emitting:
        // Check visual particles, collision probe, DryPowderSpray, or children
        bool isSpraying = (visualParticles != null && visualParticles.isPlaying) ||
                          (collisionParticles != null && collisionParticles.isPlaying) ||
                          (powderSpray != null && powderSpray.IsSpraying());

        if (!isSpraying && sprayOrigin != null)
        {
            var allPS = sprayOrigin.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in allPS)
            {
                if (ps != null && ps.isPlaying)
                {
                    isSpraying = true;
                    break;
                }
            }
        }

        var flow = FireScenarioFlowManager.Instance ?? FindAnyObjectByType<FireScenarioFlowManager>(FindObjectsInactive.Include);

        // 2. Extinguisher handle pressed/held:
        bool handleHeld = false;
        if (gripInteraction != null)
        {
            handleHeld = gripInteraction.IsGripHeld;
        }
        else if (flow != null && flow.gripInteraction != null)
        {
            handleHeld = flow.gripInteraction.IsGripHeld;
        }
        else
        {
            var g = FindAnyObjectByType<ExtinguisherGripInteraction>(FindObjectsInactive.Include);
            if (g != null) handleHeld = g.IsGripHeld;
            else handleHeld = isSpraying;
        }

        // Active discharge: if handle is held OR spray particles active
        bool isActivelyDischarging = isSpraying || handleHeld;
        if (handleHeld && powderSpray != null && !powderSpray.IsSpraying())
        {
            powderSpray.StartSpray();
            isSpraying = true;
        }

        // 3. Safety pin removed:
        bool pinRemoved = false;
        if (pinInteraction != null)
        {
            pinRemoved = pinInteraction.IsPinRemoved();
        }
        else
        {
            var p = FindAnyObjectByType<FirePinInteraction>(FindObjectsInactive.Include);
            if (p != null) pinRemoved = p.IsPinRemoved();
            else pinRemoved = (flow != null && (int)flow.CurrentStage >= (int)FireScenarioFlowManager.Stage.Step5_AimBase);
        }
        if (!pinRemoved && isActivelyDischarging)
        {
            pinRemoved = true;
        }

        // 4. Directional aim cone or camera viewport hit on fire base
        bool aimHit = isActivelyDischarging && CheckAimAtFire();

        // If user is in Step5_AimBase and starts spraying towards the fire, auto-confirm aim
        if (flow != null && flow.CurrentStage == FireScenarioFlowManager.Stage.Step5_AimBase && isActivelyDischarging && aimHit)
        {
            flow.OnAimConfirmed();
        }

        // 5. Physical particle collision within grace period
        bool physicsHit = isActivelyDischarging && (Time.time - lastHitTime <= collisionGracePeriod);

        // VALID EXTINGUISHING CONTACT:
        // True if user is actively discharging/pressing towards the fire (aim hit or physics collision)
        bool validContact = isActivelyDischarging && (physicsHit || aimHit);

        // ── Contact State Transition Events ─────────────────────────────────────
        if (validContact && !_wasTouchingFire)
        {
            // false → true: spray contact established
            _interruptionEventRaised = false;
            var fireFlow = flow ?? FindAnyObjectByType<FireScenarioFlowManager>(FindObjectsInactive.Include);
            fireFlow?.EnsureFireAdapterPublic()?.RecordSprayContactValid();
        }
        else if (!validContact && _wasTouchingFire && !_interruptionEventRaised)
        {
            // true → false: contact lost, grace period starts
            _interruptionEventRaised = true;
            var fireFlow = flow ?? FindAnyObjectByType<FireScenarioFlowManager>(FindObjectsInactive.Include);
            fireFlow?.EnsureFireAdapterPublic()?.RecordSprayInterrupted();
        }
        _wasTouchingFire = validContact;
        isTouchingFire = validContact;

        // ── 10-Second Continuous Timer & Grip Reset ────────────────────────────────
        // If grip is released before 10s: timer resets to 0.0s immediately!
        // If grip is kept pressed for 10s and pointed towards fire: fire goes off!
        if (!handleHeld && !isSpraying)
        {
            // GRIP RELEASED BEFORE 10 SECONDS: Reset timer immediately!
            if (contactTimer > 0f)
            {
                contactTimer = 0f;
                var fireFlow = flow ?? FindAnyObjectByType<FireScenarioFlowManager>(FindObjectsInactive.Include);
                fireFlow?.EnsureFireAdapterPublic()?.RecordSprayContactReset();
                Debug.Log("[ExtinguisherSprayCollision] Grip released before 10s -> Timer reset to 0.0s");
            }
            timeSinceContactLost = 0f;
            isTouchingFire = false;

            if (fireExtinguishable != null)
                fireExtinguishable.NotifyParticleCollision(false, 0f, requiredTime);
        }
        else if (validContact)
        {
            // GRIP HELD & POINTED AT FIRE: Accumulate continuous timer towards 10.0s!
            timeSinceContactLost = 0f;
            contactTimer += Time.deltaTime;

            if (fireExtinguishable != null)
                fireExtinguishable.NotifyParticleCollision(true, contactTimer, requiredTime);

            if (contactTimer >= requiredTime)
            {
                contactTimer = requiredTime;
                ExtinguishAll();
                return;
            }
        }
        else
        {
            // GRIP HELD BUT AIMED AWAY: Pause timer accumulation while grip is held
            timeSinceContactLost += Time.deltaTime;
            isTouchingFire = false;

            if (fireExtinguishable != null)
                fireExtinguishable.NotifyParticleCollision(false, contactTimer, requiredTime);
        }

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

        // Ignore self-collisions with extinguisher nozzle, body, or camera
        if (other.transform == transform || other.transform.IsChildOf(transform) ||
            (sprayOrigin != null && (other.transform == sprayOrigin || other.transform.IsChildOf(sprayOrigin))))
        {
            return false;
        }

        string otherName = other.name.ToLower();
        if (otherName.Contains("hose") || otherName.Contains("extinguisher") || otherName.Contains("camera"))
        {
            return false;
        }

        // 1. Direct match with fireTargetTransform
        if (fireTargetTransform != null)
        {
            if (other.transform == fireTargetTransform ||
                other.transform.IsChildOf(fireTargetTransform) ||
                fireTargetTransform.IsChildOf(other.transform))
            {
                return true;
            }
        }

        // 2. Direct match with fireExtinguishable
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

        // 3. Name-based match up the hierarchy
        Transform t = other.transform;
        while (t != null)
        {
            string n = t.name.ToLower();
            if (n.Contains("fire") || n.Contains("flame") || n.Contains("hazard") ||
                n.Contains("target") || n.Contains("electric") || n.Contains("box") ||
                n.Contains("vfx"))
            {
                return true;
            }
            t = t.parent;
        }

        // 4. Proximity check to fire base: if particles hit any surface (floor, table, wall) within 2.2m of fire center
        Vector3 firePos = GetFireWorldPosition();
        if (firePos != Vector3.zero)
        {
            Collider col = other.GetComponent<Collider>();
            Vector3 closest = col != null ? col.ClosestPoint(firePos) : other.transform.position;
            if (Vector3.Distance(closest, firePos) <= 2.2f)
            {
                return true;
            }
        }

        return false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Aim Cone & Screen Viewport Check
    // ─────────────────────────────────────────────────────────────────────────
    private bool CheckAimAtFire()
    {
        Vector3 targetPos = GetFireWorldPosition();

        // 1. Camera Look Aim & Viewport Check (Primary in mobile AR)
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main ?? FindAnyObjectByType<Camera>();
            if (mainCam != null) cameraTransform = mainCam.transform;
        }

        if (cameraTransform != null)
        {
            Vector3 camToTarget = targetPos - cameraTransform.position;
            float camDist = camToTarget.magnitude;

            if (camDist <= aimMaxDistance)
            {
                float camAngle = Vector3.Angle(cameraTransform.forward, camToTarget.normalized);
                if (camAngle <= aimMaxAngleDeg)
                    return true;

                // Viewport check: If fire is visible on the phone screen
                Camera cam = cameraTransform.GetComponent<Camera>();
                if (cam == null) cam = Camera.main ?? FindAnyObjectByType<Camera>();
                if (cam != null)
                {
                    Vector3 vp = cam.WorldToViewportPoint(targetPos);
                    if (vp.z > 0f && vp.x >= -0.35f && vp.x <= 1.35f && vp.y >= -0.35f && vp.y <= 1.35f)
                        return true;
                }
            }

            if (camDist <= 2.5f)
                return true;
        }

        // 2. Actual Nozzle Transform Aim Check
        Transform nozzle = sprayOrigin != null ? sprayOrigin : transform;
        if (nozzle != null)
        {
            Vector3 nozzlePos = nozzle.position;
            Vector3 toTarget = targetPos - nozzlePos;
            float dist = toTarget.magnitude;

            if (dist <= aimMaxDistance)
            {
                Vector3 toTargetDir = toTarget.normalized;
                float nozzleAngle = Vector3.Angle(nozzle.forward, toTargetDir);
                float nozzleReverseAngle = Vector3.Angle(-nozzle.forward, toTargetDir);
                float minNozzleAngle = Mathf.Min(nozzleAngle, nozzleReverseAngle);

                if (minNozzleAngle <= aimMaxAngleDeg)
                    return true;
            }

            if (dist <= 2.5f)
                return true;
        }

        return false;
    }

    public Vector3 GetFireWorldPosition()
    {
        if (fireExtinguishable != null)
            return fireExtinguishable.FireWorldPosition;
        if (fireTargetTransform != null)
            return fireTargetTransform.position;
        var f = GameObject.Find("VFX_Fire_01_Small") ?? GameObject.Find("Hazard") ?? GameObject.Find("Electric Box") ?? GameObject.Find("Flames");
        if (f != null) return f.transform.position;
        var fe = FindAnyObjectByType<FireExtinguishable>(FindObjectsInactive.Include);
        if (fe != null) return fe.FireWorldPosition;
        return Vector3.zero;
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

        StopAllFireParticlesInScene();

        // Directly notify FlowManager
        var flow = FireScenarioFlowManager.Instance ?? FindAnyObjectByType<FireScenarioFlowManager>(FindObjectsInactive.Include);
        if (flow != null)
        {
            flow.HandleFireExtinguished();
        }

        // Release grip and stop powder spray
        if (gripInteraction != null) gripInteraction.StopGrip();
        if (powderSpray != null) powderSpray.StopSpray();

        Debug.Log("[SurakshaAR] ExtinguisherSprayCollision: 10s continuous spray complete -> FIRE EXTINGUISHED");
    }

    private static void StopAllFireParticlesInScene()
    {
        var allPS = Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include);
        foreach (var ps in allPS)
        {
            if (ps == null) continue;
            string n = ps.gameObject.name.ToLower();
            string pName = ps.transform.parent != null ? ps.transform.parent.name.ToLower() : "";
            if (n.Contains("spray") || pName.Contains("spray")) continue;

            if (n.Contains("fire") || n.Contains("flame") || n.Contains("smoke") ||
                n.Contains("spark") || n.Contains("glow") || n.Contains("distortion") ||
                pName.Contains("fire") || pName.Contains("hazard") || pName.Contains("electric box"))
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.gameObject.SetActive(false);
            }
        }

        var sceneVfx = GameObject.Find("VFX_Fire_01_Small");
        if (sceneVfx != null) sceneVfx.SetActive(false);

        var allLights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include);
        foreach (var l in allLights)
        {
            if (l != null && (l.gameObject.name.ToLower().Contains("fire") || l.gameObject.name.ToLower().Contains("flame")))
                l.enabled = false;
        }

        var allAudio = Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include);
        foreach (var a in allAudio)
        {
            if (a != null && (a.gameObject.name.ToLower().Contains("fire") || a.gameObject.name.ToLower().Contains("flame")))
                a.Stop();
        }
    }


    private void OnParticleSystemStopped()
    {
        isTouchingFire = false;
        if (fireExtinguishable != null)
            fireExtinguishable.NotifyParticleCollision(false, contactTimer, requiredTime);
        if (fireSystem != null)
            fireSystem.SetSprayHittingFire(false);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Setup: Separation of Visual Spray & Collision Probe
    // ─────────────────────────────────────────────────────────────────────────
    private void ResolveParticlesAndOrigin()
    {
        var thisPS = GetComponent<ParticleSystem>();
        var thisRenderer = GetComponent<ParticleSystemRenderer>();

        if (thisRenderer != null && thisRenderer.enabled)
        {
            visualParticles = thisPS;
        }
        else if (thisPS != null)
        {
            collisionParticles = thisPS;
        }

        if (sprayOrigin == null)
        {
            if (transform.parent != null && transform.parent.name.ToLower().Contains("spraypoint"))
                sprayOrigin = transform.parent;
            else
                sprayOrigin = transform;
        }

        if (collisionParticles == null && sprayOrigin != null)
        {
            var probeChild = sprayOrigin.Find("SprayCollision");
            if (probeChild != null)
                collisionParticles = probeChild.GetComponent<ParticleSystem>();
        }

        if (visualParticles == null && sprayOrigin != null)
        {
            var sprayChild = sprayOrigin.Find("spray");
            if (sprayChild != null)
                visualParticles = sprayChild.GetComponent<ParticleSystem>();
        }

        if (powderSpray == null)
            powderSpray = GetComponentInParent<DryPowderSpray>() ?? FindAnyObjectByType<DryPowderSpray>(FindObjectsInactive.Include);

        if (powderSpray != null)
        {
            if (visualParticles == null) visualParticles = powderSpray.sprayParticles;
            if (collisionParticles == null) collisionParticles = powderSpray.collisionParticles;
        }
    }

    private void ConfigureCollisionDetector()
    {
        // 1. Visual spray: DO NOT enable world collision (it causes particles to bounce/die on the
        //    extinguisher body and AR planes before reaching the fire).
        //    Instead, keep collision disabled but enable sendCollisionMessages so particle hits
        //    still forward to OnParticleCollision callbacks via SprayCollisionProbe.
        if (visualParticles != null)
        {
            var visCol = visualParticles.collision;
            visCol.enabled = false;   // OFF — prevents scatter against extinguisher body/AR planes

            var visMain = visualParticles.main;
            visMain.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;

            // Ensure renderer is ON for visual particles
            var visRend = visualParticles.GetComponent<ParticleSystemRenderer>();
            if (visRend != null) visRend.enabled = true;

            if (visualParticles.gameObject != this.gameObject)
            {
                var forwarder = visualParticles.GetComponent<SprayCollisionProbe>()
                             ?? visualParticles.gameObject.AddComponent<SprayCollisionProbe>();
                forwarder.master = this;
            }
        }

        // 2. Invisible collision probe: enable world collision for physics hit detection
        if (collisionParticles != null)
        {
            var col = collisionParticles.collision;
            col.enabled = true;
            col.type = ParticleSystemCollisionType.World;
            col.mode = ParticleSystemCollisionMode.Collision3D;
            col.sendCollisionMessages = true;
            col.lifetimeLoss = 0f;
            col.dampen = 0f;
            col.bounce = 0f;
            col.radiusScale = 0.5f;
            col.collidesWith = ~0;
            col.quality = ParticleSystemCollisionQuality.High;

            var r = collisionParticles.GetComponent<ParticleSystemRenderer>();
            if (r != null)
                r.enabled = false;

            if (collisionParticles.gameObject != this.gameObject)
            {
                var forwarder = collisionParticles.GetComponent<SprayCollisionProbe>()
                             ?? collisionParticles.gameObject.AddComponent<SprayCollisionProbe>();
                forwarder.master = this;
            }
        }
    }

    private void ResolveReferences()
    {
        if (visualParticles == null || collisionParticles == null || sprayOrigin == null)
            ResolveParticlesAndOrigin();

        if (powderSpray == null)
            powderSpray = FindAnyObjectByType<DryPowderSpray>(FindObjectsInactive.Include);

        if (fireExtinguishable == null)
        {
            fireExtinguishable = FindAnyObjectByType<FireExtinguishable>(FindObjectsInactive.Include);
            if (fireExtinguishable != null)
                fireExtinguishable.RegisterSprayCollision(this);
        }

        if (fireSystem == null)
            fireSystem = FindAnyObjectByType<FireExtinguishSystem>(FindObjectsInactive.Include);

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // Grip interaction: prefer the one on the functional/active extinguisher
        if (gripInteraction == null || !gripInteraction.gameObject.activeInHierarchy)
        {
            var grips = Object.FindObjectsByType<ExtinguisherGripInteraction>(FindObjectsInactive.Include);
            foreach (var g in grips)
            {
                if (g != null && g.gameObject.activeInHierarchy && !g.name.ToLower().Contains("display"))
                {
                    gripInteraction = g;
                    break;
                }
            }
            if (gripInteraction == null && grips.Length > 0)
                gripInteraction = grips[0];
        }

        // Pin interaction: prefer active or functional pin
        if (pinInteraction == null || !pinInteraction.gameObject.activeInHierarchy)
        {
            var pins = Object.FindObjectsByType<FirePinInteraction>(FindObjectsInactive.Include);
            foreach (var p in pins)
            {
                if (p != null && !p.name.ToLower().Contains("display"))
                {
                    pinInteraction = p;
                    break;
                }
            }
            if (pinInteraction == null && pins.Length > 0)
                pinInteraction = pins[0];
        }

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
        sc.radius = 1.2f;
        sc.isTrigger = false;
    }
}

/// <summary>
/// Helper probe component attached to the invisible SprayCollision or spray GameObject to forward
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