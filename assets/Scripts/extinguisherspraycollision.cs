using UnityEngine;

/// <summary>
/// Attached to the 'spray' Particle System GameObject (child of FireExt -> spraypoint).
/// Listens for Unity particle collisions against fire/hazard colliders.
/// Requires 10 seconds of continuous particle collision to extinguish the fire.
/// If particles move off-target or cease colliding, the 10-second timer instantly restarts.
/// </summary>
public class ExtinguisherSprayCollision : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem sprayParticles;
    [SerializeField] private FireExtinguishable fireExtinguishable;
    [SerializeField] private FireExtinguishSystem fireSystem;

    [Header("Extinguishing Settings")]
    [SerializeField] private float requiredTime = 10f;
    [SerializeField] private float collisionGracePeriod = 0.25f;

    private float contactTimer = 0f;
    private float lastCollisionTime = -999f;
    private bool isTouchingFire = false;
    private bool isExtinguished = false;

    public float ContactTimer => contactTimer;
    public float RequiredTime => requiredTime;
    public float RemainingTime => Mathf.Max(0f, requiredTime - contactTimer);
    public float Progress01 => requiredTime > 0f ? Mathf.Clamp01(contactTimer / requiredTime) : 1f;
    public bool IsTouchingFire => isTouchingFire;

    private void Awake()
    {
        enabled = true;
    }

    private void Start()
    {
        if (sprayParticles == null)
        {
            sprayParticles = GetComponent<ParticleSystem>();
        }

        if (fireExtinguishable == null)
        {
            fireExtinguishable = FindAnyObjectByType<FireExtinguishable>();
        }

        if (fireSystem == null)
        {
            fireSystem = FindAnyObjectByType<FireExtinguishSystem>();
        }
    }

    private void Update()
    {
        if (isExtinguished)
            return;

        // Lazy resolution in case objects were enabled/instantiated dynamically
        if (fireExtinguishable == null)
            fireExtinguishable = FindAnyObjectByType<FireExtinguishable>();

        if (fireSystem == null)
            fireSystem = FindAnyObjectByType<FireExtinguishSystem>();

        // Verify if particles are currently actively spraying
        bool isSpraying = sprayParticles != null && sprayParticles.isPlaying;

        // Collision is active if we received an OnParticleCollision event within the grace period
        isTouchingFire = isSpraying && (Time.time - lastCollisionTime <= collisionGracePeriod);

        if (isTouchingFire)
        {
            // Spray particles are continuously colliding with the fire:
            // Count up towards 10.0s (timer counts down to 0.0s)
            contactTimer += Time.deltaTime;

            if (contactTimer >= requiredTime)
            {
                contactTimer = requiredTime;
                ExtinguishAll();
            }
        }
        else
        {
            // Spray moved somewhere else, aimed away, or stopped:
            // RESTART TIMER! Fire will not turn off until 10s continuous collision!
            contactTimer = 0f;
        }

        // Notify FireExtinguishable & FireExtinguishSystem of current state
        if (fireExtinguishable != null)
        {
            fireExtinguishable.NotifyParticleCollision(isTouchingFire, contactTimer, requiredTime);
        }

        if (fireSystem != null)
        {
            fireSystem.SetSprayHittingFire(isTouchingFire);
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (isExtinguished || other == null)
            return;

        if (IsFireObject(other))
        {
            lastCollisionTime = Time.time;
        }
    }

    private bool IsFireObject(GameObject other)
    {
        if (other == null)
            return false;

        string lower = other.name.ToLower();
        if (lower.Contains("fire") || lower.Contains("flame") || lower.Contains("hazard") ||
            lower.Contains("target") || lower.Contains("electric") || lower.Contains("box"))
        {
            return true;
        }

        if (other.GetComponentInParent<FireExtinguishSystem>() != null)
            return true;

        if (other.GetComponentInParent<FireExtinguishable>() != null)
            return true;

        Transform t = other.transform;
        while (t != null)
        {
            string pName = t.name.ToLower();
            if (pName.Contains("hazard") || pName.Contains("fire") || pName.Contains("electric box"))
                return true;
            t = t.parent;
        }

        return false;
    }

    private void ExtinguishAll()
    {
        if (isExtinguished) return;
        isExtinguished = true;
        isTouchingFire = false;

        if (fireExtinguishable != null && !fireExtinguishable.IsExtinguished)
        {
            fireExtinguishable.ExtinguishFire();
        }

        if (fireSystem != null && !fireSystem.IsExtinguished())
        {
            var method = fireSystem.GetType().GetMethod("ExtinguishFire",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public);
            method?.Invoke(fireSystem, null);
        }

        Debug.Log("=========================================");
        Debug.Log("SPRAY COLLISION: 10s CONTINUOUS COLLISION REACHED -> FIRE EXTINGUISHED");
        Debug.Log("=========================================");
    }

    private void OnParticleSystemStopped()
    {
        isTouchingFire = false;
        contactTimer = 0f;

        if (fireExtinguishable != null)
        {
            fireExtinguishable.NotifyParticleCollision(false, 0f, requiredTime);
        }

        if (fireSystem != null)
        {
            fireSystem.SetSprayHittingFire(false);
        }
    }

    public void ResetCollisionTimer()
    {
        isExtinguished = false;
        isTouchingFire = false;
        contactTimer = 0f;
        lastCollisionTime = -999f;
    }
}