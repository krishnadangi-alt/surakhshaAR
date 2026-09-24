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
[ExecuteAlways]
public class FireExtinguishable : MonoBehaviour
{
    [Header("Fire Particle System")]
    [SerializeField] private ParticleSystem fireParticle;

    [Header("Extinguishing Settings")]
    [SerializeField] public float extinguishTime = 10f;
    [SerializeField] public float sprayRange = 25f;
    [SerializeField] public float maxAimAngle = 65f;

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
        extinguishTime = 10f;
        sprayRange = Mathf.Max(sprayRange, 25f);
        maxAimAngle = Mathf.Max(maxAimAngle, 65f);
    }

    private void OnEnable()
    {
        if (!IsExtinguished)
        {
            EnsureFireVisualsActive();
        }
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

        // Start fire particles and make sure all flame VFX are active and emitting
        EnsureFireVisualsActive();
    }

    public void EnsureFireVisualsActive()
    {
        if (IsExtinguished) return;

        // 1. Find VFX_Fire_01_Small or Hazard
        var vfx = (gameObject.name == "VFX_Fire_01_Small") ? gameObject : GameObject.Find("VFX_Fire_01_Small");
        if (vfx == null)
        {
            var hazard = GameObject.Find("Hazard") ?? GameObject.Find("Electric Box");
            if (hazard != null)
            {
                var t = hazard.transform.Find("Electric Box/VFX_Fire_01_Small") ?? hazard.transform.Find("VFX_Fire_01_Small");
                if (t != null) vfx = t.gameObject;
            }
        }

        if (vfx != null)
        {
            vfx.SetActive(true);
            foreach (var ps in vfx.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (ps == null) continue;
                ps.gameObject.SetActive(true);
                var rend = ps.GetComponent<ParticleSystemRenderer>();
                if (rend != null && ps.name != "VFX_Fire_01_Small") rend.enabled = true;
                var em = ps.emission;
                em.enabled = true;
                var main = ps.main;
                main.loop = true;
                main.prewarm = true;
                main.playOnAwake = true;

                if (!ps.isPlaying) ps.Play(true);

#if UNITY_EDITOR
                if (!Application.isPlaying && ps.particleCount == 0)
                {
                    ps.Simulate(1.0f, false, true);
                }
#endif
            }
        }

        // 2. Play primary fire particle
        if (fireParticle != null)
        {
            fireParticle.gameObject.SetActive(true);
            var em = fireParticle.emission;
            em.enabled = true;
            if (!fireParticle.isPlaying) fireParticle.Play(true);
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
    //  Update — Dual Detection & Synchronization
    // ─────────────────────────────────────────────────────────────────────────
    private void Update()
    {
        if (IsExtinguished) return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EnsureFireVisualsActive();
            return;
        }
#endif

        // Stage validation: fire cannot be extinguished prior to Step 5 / Step 6
        var flow = FireScenarioFlowManager.Instance;
        if (flow != null)
        {
            int curOrdinal = (int)flow.CurrentStage;
            int step5Ordinal = (int)FireScenarioFlowManager.Stage.Step5_AimBase;
            if (curOrdinal < step5Ordinal)
            {
                _mirroredTimer = 0f;
                _mirroredContact = false;
                return;
            }
        }

        // Lazy resolution if references initialized late
        if (_registeredSprayCollision == null)
        {
            _registeredSprayCollision = FindAnyObjectByType<ExtinguisherSprayCollision>();
        }

        if (powderSpray == null)
        {
            powderSpray = FindAnyObjectByType<DryPowderSpray>();
        }

        // Check handle / grip
        bool handleHeld = false;
        var grip = FindAnyObjectByType<ExtinguisherGripInteraction>(FindObjectsInactive.Include);
        if (grip != null) handleHeld = grip.IsGripHeld;
        if (!handleHeld && powderSpray != null && powderSpray.IsSpraying()) handleHeld = true;

        // 1. Primary Sync: mirror ExtinguisherSprayCollision if active
        if (_registeredSprayCollision != null && _registeredSprayCollision.gameObject.activeInHierarchy)
        {
            if (_registeredSprayCollision.IsExtinguished && !IsExtinguished)
            {
                ExtinguishFire();
                return;
            }

            _mirroredContact = _registeredSprayCollision.IsTouchingFire;
            _mirroredTimer = _registeredSprayCollision.ContactTimer;

            if (_mirroredTimer >= extinguishTime && !IsExtinguished)
            {
                ExtinguishFire();
                return;
            }
        }
        else
        {
            // 2. Standalone fallback if ExtinguisherSprayCollision not active
            if (!handleHeld)
            {
                // Reset immediately on grip release before 10s
                _mirroredTimer = 0f;
                _mirroredContact = false;
            }
            else
            {
                bool hitting = CheckLocalAimAtFire();
                if (hitting)
                {
                    _mirroredContact = true;
                    _mirroredTimer += Time.deltaTime;

                    if (_mirroredTimer >= extinguishTime && !IsExtinguished)
                    {
                        ExtinguishFire();
                        return;
                    }
                }
                else
                {
                    _mirroredContact = false;
                }
            }
        }
    }

    private bool CheckLocalAimAtFire()
    {
        Camera cam = Camera.main ?? FindAnyObjectByType<Camera>();
        Vector3 targetPos = FireWorldPosition;

        // 1. Check Camera Aim towards fire in AR
        if (cam != null)
        {
            Vector3 camToTarget = targetPos - cam.transform.position;
            float camDist = camToTarget.magnitude;
            if (camDist <= sprayRange)
            {
                float camAngle = Vector3.Angle(cam.transform.forward, camToTarget.normalized);
                if (camAngle <= maxAimAngle)
                    return true;

                Vector3 vp = cam.WorldToViewportPoint(targetPos);
                if (vp.z > 0f && vp.x >= -0.35f && vp.x <= 1.35f && vp.y >= -0.35f && vp.y <= 1.35f)
                    return true;
            }

            if (camDist <= 2.5f)
                return true;
        }

        // 2. Check Spray Point / Nozzle aim towards fire
        Transform sprayPoint = (powderSpray != null) ? powderSpray.GetSprayPoint() : null;
        if (sprayPoint != null)
        {
            Vector3 toTarget = targetPos - sprayPoint.position;
            float dist = toTarget.magnitude;

            if (dist <= sprayRange)
            {
                Vector3 dir = toTarget.normalized;
                float forwardAngle = Vector3.Angle(sprayPoint.forward, dir);
                float backwardAngle = Vector3.Angle(-sprayPoint.forward, dir);
                float angle = Mathf.Min(forwardAngle, backwardAngle);

                if (angle <= maxAimAngle)
                    return true;
            }

            if (dist <= 2.5f)
                return true;
        }

        return false;
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

        // 4. Stop ALL fire / flame particle systems anywhere in scene
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

        // 5. Turn off fire lights and audio
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

        Debug.Log("[SurakshaAR] FireExtinguishable: Fire successfully extinguished!");

        OnExtinguished?.Invoke();

        var flow = FireScenarioFlowManager.Instance ?? FindAnyObjectByType<FireScenarioFlowManager>(FindObjectsInactive.Include);
        if (flow != null)
        {
            flow.HandleFireExtinguished();
        }
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
        EnsureSphereCollider(gameObject, 1.2f);

        if (fireParticle != null)
        {
            EnsureSphereCollider(fireParticle.gameObject, 1.2f);
        }

        var hazard = GameObject.Find("Hazard") ?? GameObject.Find("Electric Box");
        if (hazard != null)
        {
            EnsureSphereCollider(hazard, 1.5f);
            var box = hazard.transform.Find("Electric Box") ?? hazard.transform;
            EnsureSphereCollider(box.gameObject, 1.5f);
        }

        var vfx = GameObject.Find("VFX_Fire_01_Small");
        if (vfx != null)
        {
            EnsureSphereCollider(vfx, 1.5f);
        }
    }

    private static void EnsureSphereCollider(GameObject go, float radius)
    {
        if (go == null) return;
        var col = go.GetComponent<Collider>();
        if (col == null)
        {
            var sc = go.AddComponent<SphereCollider>();
            sc.radius = radius;
            sc.isTrigger = false;
        }
    }
}