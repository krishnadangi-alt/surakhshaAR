using UnityEngine;

/// <summary>
/// DryPowderSpray
/// ==============
/// Manages the extinguisher spray particle systems.
/// 
/// ARCHITECTURE:
/// - sprayParticles (Visual): Collision = OFF, Renderer = ON.
///   Originates at the nozzle, shoots straight forward along nozzle forward axis.
///   Never culled, distorted, or scattered by extinguisher colliders or AR planes.
/// - collisionParticles (Collision Probe): Collision = ON, Renderer = OFF.
///   Invisible lightweight probe particles for physical contact detection.
/// </summary>
public class DryPowderSpray : MonoBehaviour
{
    [Header("Spray Origin (Nozzle)")]
    public Transform sprayPoint;

    [Header("Visual Spray (Collision = OFF)")]
    public ParticleSystem sprayParticles;

    [Header("Collision Probe (Collision = ON, Invisible)")]
    public ParticleSystem collisionParticles;

    [Header("Spray Transform Scale")]
    public Vector3 sprayScale = new Vector3(0.05f, 0.25f, 0.6f);

    private void Awake()
    {
        transform.localScale = sprayScale;
        InitializeComponents();
    }

    private void LateUpdate()
    {
        // Dynamically maintain spray scale at runtime
        if (transform.localScale != sprayScale)
        {
            transform.localScale = sprayScale;
        }
    }

    private void InitializeComponents()
    {
        // 1. Resolve visual particle system
        if (sprayParticles == null)
        {
            sprayParticles = GetComponent<ParticleSystem>()
                          ?? GetComponentInChildren<ParticleSystem>(true);
        }

        // 2. Resolve spray origin (nozzle)
        if (sprayPoint == null)
        {
            sprayPoint = transform.parent != null && transform.parent.name.ToLower().Contains("spraypoint")
                ? transform.parent
                : transform;
        }

        // 3. Set local scale dynamically to desired spray size (0.05, 0.25, 0.6)
        transform.localScale = sprayScale;

        // 4. Configure Visual Spray: Dense, billowy white chemical powder stream
        if (sprayParticles != null)
        {
            var main = sprayParticles.main;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            main.scalingMode = ParticleSystemScalingMode.Shape;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startSpeed = Mathf.Max(6.0f, main.startSpeed.constant);
            main.startSize = Mathf.Max(0.40f, main.startSize.constant);
            main.startLifetime = Mathf.Max(1.0f, main.startLifetime.constant);
            main.playOnAwake = false;
            main.loop = true;

            var emission = sprayParticles.emission;
            if (emission.rateOverTime.constant < 150f)
            {
                emission.rateOverTime = 300f;
            }

            var shape = sprayParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = Mathf.Clamp(shape.angle, 4.0f, 10.0f);

            // Keep visual collision OFF so the extinguisher itself doesn't eat/scatter particles
            var col = sprayParticles.collision;
            col.enabled = false;

            var rend = sprayParticles.GetComponent<ParticleSystemRenderer>();
            if (rend != null)
            {
                rend.enabled = true;
            }
        }

        // 5. Resolve or create Collision Probe if not assigned
        if (collisionParticles == null && sprayPoint != null)
        {
            var colChild = sprayPoint.Find("SprayCollision");
            if (colChild != null)
            {
                colChild.gameObject.SetActive(true);
                collisionParticles = colChild.GetComponent<ParticleSystem>();
            }
        }

        if (collisionParticles != null)
        {
            collisionParticles.gameObject.SetActive(true);
            var colMain = collisionParticles.main;
            colMain.simulationSpace = ParticleSystemSimulationSpace.World;
            colMain.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            colMain.playOnAwake = false;
            colMain.loop = true;

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

            var colRend = collisionParticles.GetComponent<ParticleSystemRenderer>();
            if (colRend != null)
            {
                colRend.enabled = false; // Invisible probe
            }
        }
    }

    private void Start()
    {
        StopSpray();
    }

    public void StartSpray()
    {
        transform.localScale = sprayScale;

        // Always re-initialize to ensure we have fresh references
        if (sprayParticles == null)
        {
            InitializeComponents();
        }

        if (sprayParticles != null)
        {
            // Force renderer ON before playing so particles are always visible
            var rend = sprayParticles.GetComponent<ParticleSystemRenderer>();
            if (rend != null) rend.enabled = true;

            var main = sprayParticles.main;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;

            var em = sprayParticles.emission;
            em.enabled = true;
            if (!sprayParticles.isPlaying)
                sprayParticles.Play(true);
        }

        if (collisionParticles != null)
        {
            collisionParticles.gameObject.SetActive(true);
            var colEm = collisionParticles.emission;
            colEm.enabled = true;
            if (!collisionParticles.isPlaying)
                collisionParticles.Play(true);
        }

        // Also activate any child particle systems under sprayPoint (catches nested VFX)
        if (sprayPoint != null)
        {
            var allPS = sprayPoint.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in allPS)
            {
                if (ps == null || ps == collisionParticles) continue;
                var rend = ps.GetComponent<ParticleSystemRenderer>();
                if (rend != null) rend.enabled = true;
                var em = ps.emission;
                em.enabled = true;
                if (!ps.isPlaying) ps.Play(true);
            }
        }

        Debug.Log("[DryPowderSpray] StartSpray called. sprayParticles=" + (sprayParticles != null) + " isPlaying=" + (sprayParticles != null && sprayParticles.isPlaying));
    }

    public void StopSpray()
    {
        if (sprayParticles != null)
        {
            var em = sprayParticles.emission;
            em.enabled = false;
            sprayParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (collisionParticles != null)
        {
            var colEm = collisionParticles.emission;
            colEm.enabled = false;
            collisionParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (sprayPoint != null)
        {
            var allPS = sprayPoint.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in allPS)
            {
                if (ps != null)
                {
                    var em = ps.emission;
                    em.enabled = false;
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
        }
    }

    public bool IsSpraying()
    {
        return (sprayParticles != null && (sprayParticles.isPlaying || sprayParticles.emission.enabled)) ||
               (collisionParticles != null && (collisionParticles.isPlaying || collisionParticles.emission.enabled));
    }

    public ParticleSystem GetParticleSystem()
    {
        return sprayParticles;
    }

    public ParticleSystem GetCollisionParticleSystem()
    {
        return collisionParticles;
    }

    public Transform GetSprayPoint()
    {
        if (sprayPoint == null)
            sprayPoint = transform.parent != null ? transform.parent : transform;

        return sprayPoint;
    }
}