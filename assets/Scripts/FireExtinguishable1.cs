using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// FireExtinguishable
/// ==================
/// Attached to the fire/hazard GameObject (or child of FireExt pointing to the fire).
/// 
/// RESPONSIBILITIES:
///   1. Manages fire visual particles and audio.
///   2. Exposes IsBeingSprayed, SprayProgress01, and RemainingTime for HUD & FlowManager.
///   3. Coordinates with ExtinguisherSprayCollision to run the 10-second continuous spray timer.
///   4. ExtinguishFire() stops all fire particles, turns off fire visuals, and fires OnExtinguished.
///   5. ResetFire() restarts the fire for scenario replay.
/// </summary>
public class FireExtinguishable : MonoBehaviour
{
    [Header("Fire Particle System")]
    [SerializeField] private ParticleSystem fireParticle;

    [Header("Extinguishing Settings")]
    [SerializeField] public float extinguishTime = 10f;
    [SerializeField] public float sprayRange = 5f;
    [SerializeField] public float maxAimAngle = 35f;

    [Header("Spray Reference (Optional)")]
    [SerializeField] private DryPowderSpray powderSpray;

    [Header("Events")]
    public UnityEvent OnExtinguished;

    // Runtime state
    public bool IsExtinguished { get; private set; }

    private float _mirroredTimer = 0f;
    private bool _mirroredContact = false;
    private ExtinguisherSprayCollision _registeredSprayCollision;

    // Properties used by FireScenarioFlowManager & HUD
    public bool IsBeingSprayed => !IsExtinguished && _mirroredContact;

    public float SprayProgress01
    {
        get
        {
            if (extinguishTime <= 0f) return 1f;
            return Mathf.Clamp01(_mirroredTimer / extinguishTime);
        }
    }

    public float CurrentContactTimer => _mirroredTimer;

    public float RemainingTime => Mathf.Max(0f, extinguishTime - _mirroredTimer);

    public Vector3 FireWorldPosition
    {
        get
        {
            if (fireParticle != null && !fireParticle.transform.IsChildOf(transform))
                return fireParticle.transform.position;
            var hazard = GameObject.Find("VFX_Fire_01_Small") ?? GameObject.Find("Flames") ?? GameObject.Find("Hazard") ?? GameObject.Find("Electric Box");
            if (hazard != null) return hazard.transform.position;
            if (fireParticle != null) return fireParticle.transform.position;
            return transform.position;
        }
    }

    public ParticleSystem FireParticle => fireParticle;

    // ─────────────────────────────────────────────────────────────────────────
    //  Registration
    // ─────────────────────────────────────────────────────────────────────────
    public void RegisterSprayCollision(ExtinguisherSprayCollision sprayCollision)
    {
        _registeredSprayCollision = sprayCollision;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        IsExtinguished = false;
        _mirroredTimer = 0f;
        _mirroredContact = false;
    }

    private void Start()
    {
        // Auto-resolve fire particle if not assigned in Inspector
        if (fireParticle == null)
        {
            fireParticle = GetComponentInChildren<ParticleSystem>(true);
        }

        if (fireParticle == null)
        {
            var hazard = GameObject.Find("Hazard") ?? GameObject.Find("Electric Box");
            if (hazard != null)
            {
                foreach (var ps in hazard.GetComponentsInChildren<ParticleSystem>(true))
                {
                    string n = ps.name.ToLower();
                    if (n.Contains("fire") || n.Contains("flame"))
                    {
                        fireParticle = ps;
                        break;
                    }
                }
            }
        }

        if (fireParticle == null)
        {
            var vfx = GameObject.Find("VFX_Fire_01_Small");
            if (vfx != null)
                fireParticle = vfx.GetComponentInChildren<ParticleSystem>(true);
        }

        // Auto-resolve powder spray if blank
        if (powderSpray == null)
        {
            powderSpray = FindAnyObjectByType<DryPowderSpray>();
        }

        // Register with ExtinguisherSprayCollision if present in scene
        if (_registeredSprayCollision == null)
        {
            _registeredSprayCollision = FindAnyObjectByType<ExtinguisherSprayCollision>();
        }

        // Ensure colliders exist on the fire target so Unity particle collision fires
        EnsureCollider();

        // Start fire particles
        if (fireParticle != null)
        {
            fireParticle.gameObject.SetActive(true);
            fireParticle.Clear();
            fireParticle.Play();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Unity Particle Collision Callback
    // ─────────────────────────────────────────────────────────────────────────
    private void OnParticleCollision(GameObject other)
    {
        if (IsExtinguished || other == null) return;

        if (_registeredSprayCollision != null)
        {
            _registeredSprayCollision.ReceiveCollisionFromFire();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Notification from ExtinguisherSprayCollision
    // ─────────────────────────────────────────────────────────────────────────
    public void NotifyParticleCollision(bool isColliding, float currentTimer, float maxTime)
    {
        _mirroredContact = isColliding;
        _mirroredTimer = currentTimer;

        if (maxTime > 0f)
            extinguishTime = maxTime;

        // Safety fallback: if timer completed, extinguish fire
        if (currentTimer >= extinguishTime && !IsExtinguished)
        {
            ExtinguishFire();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Update — Fallback aim/collision timer if ExtinguisherSprayCollision is missing
    // ─────────────────────────────────────────────────────────────────────────
    private void Update()
    {
        if (IsExtinguished) return;

        // Lazy resolution if ExtinguisherSprayCollision initialized late
        if (_registeredSprayCollision == null)
        {
            _registeredSprayCollision = FindAnyObjectByType<ExtinguisherSprayCollision>();
            if (_registeredSprayCollision != null)
            {
                _registeredSprayCollision.ResetCollisionTimer();
            }
        }

        // If ExtinguisherSprayCollision is active, it handles the timer via NotifyParticleCollision.
        // As a belt-and-braces fallback, if ExtinguisherSprayCollision is absent, run local detection:
        if (_registeredSprayCollision == null && powderSpray != null && powderSpray.IsSpraying())
        {
            bool hitting = CheckLocalAimAtFire();
            _mirroredContact = hitting;

            if (hitting)
            {
                _mirroredTimer += Time.deltaTime;
                if (_mirroredTimer >= extinguishTime)
                {
                    ExtinguishFire();
                }
            }
            else
            {
                _mirroredTimer = 0f;
            }
        }
    }

    private bool CheckLocalAimAtFire()
    {
        if (powderSpray == null || !powderSpray.IsSpraying())
            return false;

        Transform sprayPoint = powderSpray.GetSprayPoint();
        if (sprayPoint == null)
            return false;

        Vector3 targetPos = FireWorldPosition;
        Vector3 toTarget = targetPos - sprayPoint.position;
        float dist = toTarget.magnitude;

        if (dist < 0.3f || dist > sprayRange)
            return false;

        Vector3 dir = toTarget.normalized;
        float forwardAngle = Vector3.Angle(sprayPoint.forward, dir);
        float backwardAngle = Vector3.Angle(-sprayPoint.forward, dir);
        float angle = Mathf.Min(forwardAngle, backwardAngle);

        return angle <= maxAimAngle;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Extinguish — stops ALL fire particles and deactivates flame visuals
    // ─────────────────────────────────────────────────────────────────────────
    public void ExtinguishFire()
    {
        if (IsExtinguished) return;

        IsExtinguished = true;
        _mirroredContact = false;
        _mirroredTimer = extinguishTime;

        // 1. Stop primary fire particle system
        if (fireParticle != null)
        {
            fireParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            fireParticle.gameObject.SetActive(false);
        }

        // 2. Stop and clear all particle systems under the Hazard hierarchy
        var hazard = GameObject.Find("Hazard") ?? GameObject.Find("Electric Box");
        if (hazard != null)
        {
            foreach (var ps in hazard.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (ps != null)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.gameObject.SetActive(false);
                }
            }

            var vfx = hazard.transform.Find("Electric Box/VFX_Fire_01_Small")
                   ?? hazard.transform.Find("VFX_Fire_01_Small");
            if (vfx != null)
                vfx.gameObject.SetActive(false);
        }

        // 3. Deactivate any scene-level VFX_Fire objects
        var sceneVfx = GameObject.Find("VFX_Fire_01_Small");
        if (sceneVfx != null)
        {
            foreach (var ps in sceneVfx.GetComponentsInChildren<ParticleSystem>(true))
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            sceneVfx.SetActive(false);
        }

        Debug.Log("[SurakshaAR] FireExtinguishable: Fire successfully extinguished!");

        OnExtinguished?.Invoke();
    }

    public float GetProgress() =>
        extinguishTime > 0f ? Mathf.Clamp01(_mirroredTimer / extinguishTime) : 1f;

    // ─────────────────────────────────────────────────────────────────────────
    //  Reset (for scenario replay)
    // ─────────────────────────────────────────────────────────────────────────
    public void ResetFire()
    {
        IsExtinguished = false;
        _mirroredTimer = 0f;
        _mirroredContact = false;

        if (_registeredSprayCollision != null)
            _registeredSprayCollision.ResetCollisionTimer();

        if (fireParticle != null)
        {
            fireParticle.gameObject.SetActive(true);
            fireParticle.Clear();
            fireParticle.Play();
        }

        var hazard = GameObject.Find("Hazard") ?? GameObject.Find("Electric Box");
        if (hazard != null)
        {
            var vfx = hazard.transform.Find("Electric Box/VFX_Fire_01_Small")
                   ?? hazard.transform.Find("VFX_Fire_01_Small");
            if (vfx != null)
            {
                vfx.gameObject.SetActive(true);
                foreach (var ps in vfx.GetComponentsInChildren<ParticleSystem>(true))
                {
                    ps.gameObject.SetActive(true);
                    ps.Clear();
                    ps.Play();
                }
            }
        }
    }

    private void EnsureCollider()
    {
        if (GetComponent<Collider>() == null)
        {
            var sc = gameObject.AddComponent<SphereCollider>();
            sc.radius = 0.9f;
            sc.isTrigger = false;
        }

        if (fireParticle != null && fireParticle.GetComponent<Collider>() == null)
        {
            var sc = fireParticle.gameObject.AddComponent<SphereCollider>();
            sc.radius = 0.9f;
            sc.isTrigger = false;
        }
    }
}