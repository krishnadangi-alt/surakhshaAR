using UnityEngine;
using UnityEngine.Events;

public class FireExtinguishable : MonoBehaviour
{
    [Header("Fire")]
    [SerializeField] private ParticleSystem fireParticle;

    [Header("Spray")]
    [SerializeField] private DryPowderSpray powderSpray;
    [SerializeField] private ExtinguisherSprayCollision sprayCollision;

    [Header("Extinguishing")]
    [SerializeField] public float extinguishTime = 10f;
    [SerializeField] private float sprayRange = 4.5f;
    [SerializeField] private float maxAimAngle = 45f;

    [Header("Events")]
    public UnityEvent OnExtinguished;

    public bool IsExtinguished { get; private set; }

    private float timer = 0f;
    private bool particleColliding = false;

    // ---------------------------------------------------------
    // Properties used by FireScenarioFlowManager & UI
    // ---------------------------------------------------------

    public bool IsBeingSprayed
    {
        get
        {
            if (IsExtinguished)
                return false;

            return IsSprayHittingFire();
        }
    }

    public float SprayProgress01
    {
        get
        {
            if (extinguishTime <= 0f)
                return 1f;

            return Mathf.Clamp01(timer / extinguishTime);
        }
    }

    public float RemainingTime => Mathf.Max(0f, extinguishTime - timer);

    // ---------------------------------------------------------
    // INITIALIZATION
    // ---------------------------------------------------------

    private void Awake()
    {
        IsExtinguished = false;
        timer = 0f;
    }

    private void Start()
    {
        // 1. Find the spray collision script
        if (sprayCollision == null)
        {
            sprayCollision = FindAnyObjectByType<ExtinguisherSprayCollision>();
        }

        // 2. Find the spray script if not assigned
        if (powderSpray == null)
        {
            powderSpray = FindAnyObjectByType<DryPowderSpray>();
        }

        // 3. Find fireParticle automatically if not assigned
        if (fireParticle == null)
        {
            var hazard = GameObject.Find("Hazard") ?? GameObject.Find("Electric Box");
            if (hazard != null)
            {
                var pSystems = hazard.GetComponentsInChildren<ParticleSystem>(true);
                foreach (var ps in pSystems)
                {
                    string pName = ps.name.ToLower();
                    if (pName.Contains("fire") || pName.Contains("flame"))
                    {
                        fireParticle = ps;
                        break;
                    }
                }
            }
        }

        // Start fire particles if present
        if (fireParticle != null)
        {
            fireParticle.gameObject.SetActive(true);
            fireParticle.Clear();
            fireParticle.Play();
        }
    }

    // ---------------------------------------------------------
    // COLLISION NOTIFICATION FROM ExtinguisherSprayCollision
    // ---------------------------------------------------------

    public void NotifyParticleCollision(bool isColliding, float currentTimer, float maxTime)
    {
        particleColliding = isColliding;
        extinguishTime = maxTime > 0f ? maxTime : 10f;
        timer = currentTimer;

        if (timer >= extinguishTime && !IsExtinguished)
        {
            ExtinguishFire();
        }
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    private void Update()
    {
        if (IsExtinguished)
            return;

        // Lazy resolution if objects were enabled dynamically
        if (sprayCollision == null)
            sprayCollision = FindAnyObjectByType<ExtinguisherSprayCollision>();

        if (powderSpray == null)
            powderSpray = FindAnyObjectByType<DryPowderSpray>();

        // If ExtinguisherSprayCollision is active and handling timer, it calls NotifyParticleCollision.
        // If ExtinguisherSprayCollision is not directly calling, handle timing here:
        if (sprayCollision != null)
        {
            particleColliding = sprayCollision.IsTouchingFire;
            timer = sprayCollision.ContactTimer;
            if (timer >= extinguishTime)
            {
                ExtinguishFire();
            }
            return;
        }

        // Fallback timing if sprayCollision component is absent
        if (IsSprayHittingFire())
        {
            timer += Time.deltaTime;

            if (timer >= extinguishTime)
            {
                ExtinguishFire();
            }
        }
        else
        {
            // Continuous collision broken -> RESTART TIMER!
            timer = 0f;
        }
    }

    // ---------------------------------------------------------
    // SPRAY DETECTION
    // ---------------------------------------------------------

    private bool IsSprayHittingFire()
    {
        // 1. Direct particle collision check (highest priority)
        if (particleColliding)
            return true;

        if (sprayCollision != null && sprayCollision.IsTouchingFire)
            return true;

        // 2. Vector & angle check as secondary fallback
        if (powderSpray == null || !powderSpray.IsSpraying())
            return false;

        Transform sprayPoint = powderSpray.GetSprayPoint();
        if (sprayPoint == null)
            return false;

        Vector3 firePosition = fireParticle != null ? fireParticle.transform.position : transform.position;
        Vector3 fireBase = firePosition + Vector3.down * 0.5f;

        return IsAimedAt(sprayPoint, firePosition) || IsAimedAt(sprayPoint, fireBase);
    }

    private bool IsAimedAt(Transform sprayPoint, Vector3 targetPosition)
    {
        Vector3 toTarget = targetPosition - sprayPoint.position;
        float distance = toTarget.magnitude;

        if (distance > sprayRange)
            return false;

        if (distance < 0.01f)
            return true;

        Vector3 direction = toTarget.normalized;
        float forwardAngle = Vector3.Angle(sprayPoint.forward, direction);
        float backwardAngle = Vector3.Angle(-sprayPoint.forward, direction);
        float angle = Mathf.Min(forwardAngle, backwardAngle);

        return angle <= maxAimAngle;
    }

    // ---------------------------------------------------------
    // EXTINGUISH
    // ---------------------------------------------------------

    public void ExtinguishFire()
    {
        if (IsExtinguished)
            return;

        IsExtinguished = true;
        timer = extinguishTime;

        // 1. Stop primary fire particle system
        if (fireParticle != null)
        {
            fireParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            fireParticle.gameObject.SetActive(false);
        }

        // 2. Stop and deactivate all other fire particle systems under Hazard
        var hazard = GameObject.Find("Hazard") ?? GameObject.Find("Electric Box");
        if (hazard != null)
        {
            var allPS = hazard.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in allPS)
            {
                if (ps != null)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    ps.gameObject.SetActive(false);
                }
            }

            // Deactivate VFX_Fire_01_Small GameObject
            var vfx = hazard.transform.Find("Electric Box/VFX_Fire_01_Small")
                   ?? hazard.transform.Find("VFX_Fire_01_Small");
            if (vfx != null)
            {
                vfx.gameObject.SetActive(false);
            }
        }

        Debug.Log("================================");
        Debug.Log("FIRE EXTINGUISHED (10s Continuous Spray Complete)");
        Debug.Log("================================");

        OnExtinguished?.Invoke();
    }

    // ---------------------------------------------------------
    // PROGRESS
    // ---------------------------------------------------------

    public float GetProgress()
    {
        if (extinguishTime <= 0f)
            return 1f;

        return Mathf.Clamp01(timer / extinguishTime);
    }

    // ---------------------------------------------------------
    // RESET
    // ---------------------------------------------------------

    public void ResetFire()
    {
        IsExtinguished = false;
        timer = 0f;
        particleColliding = false;

        if (sprayCollision != null)
        {
            sprayCollision.ResetCollisionTimer();
        }

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
            }
        }
    }
}