using UnityEngine;

/// <summary>
/// DryPowderSpray
/// ==============
/// Manages the extinguisher spray particle systems.
/// 
/// SEPARATION OF VISUAL SPRAY AND COLLISION DETECTION:
/// - sprayParticles (Visual): Collision = OFF, Renderer = ON.
///   Originates at the nozzle, shoots straight forward along nozzle forward axis.
///   Never culled, distorted, or scattered by colliders or AR planes.
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

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // 1. Resolve visual particle system
        if (sprayParticles == null)
        {
            sprayParticles = GetComponent<ParticleSystem>();
        }

        // 2. Resolve spray origin (nozzle)
        if (sprayPoint == null)
        {
            sprayPoint = transform.parent != null && transform.parent.name.ToLower().Contains("spraypoint")
                ? transform.parent
                : transform;
        }

        // 3. Ensure Visual Spray NEVER has collision enabled
        if (sprayParticles != null)
        {
            var col = sprayParticles.collision;
            col.enabled = false;

            var main = sprayParticles.main;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
        }

        // 4. Resolve or create Collision Probe if not assigned
        if (collisionParticles == null && sprayPoint != null)
        {
            var colChild = sprayPoint.Find("SprayCollision");
            if (colChild != null)
            {
                collisionParticles = colChild.GetComponent<ParticleSystem>();
            }
        }
    }

    private void Start()
    {
        StopSpray();
    }

    public void StartSpray()
    {
        if (sprayParticles != null && !sprayParticles.isPlaying)
        {
            sprayParticles.Play();
        }

        if (collisionParticles != null && !collisionParticles.isPlaying)
        {
            collisionParticles.Play();
        }
    }

    public void StopSpray()
    {
        if (sprayParticles != null)
        {
            sprayParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (collisionParticles != null)
        {
            collisionParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    public bool IsSpraying()
    {
        return sprayParticles != null && sprayParticles.isPlaying;
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