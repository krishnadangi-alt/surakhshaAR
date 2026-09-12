using UnityEngine;

public class DryPowderSpray : MonoBehaviour
{
    [Header("Spray Point")]
    public Transform sprayPoint;

    [Header("Spray Particle System")]
    public ParticleSystem sprayParticles;

    private void Awake()
    {
        if (sprayParticles == null)
        {
            sprayParticles =
                GetComponent<ParticleSystem>();
        }

        // This component sits directly on the "spray" particle system
        // object, so it is its own spray origin if nothing was assigned.
        if (sprayPoint == null)
        {
            sprayPoint = transform;
        }
    }

    private void Start()
    {
        StopSpray();
    }

    public void StartSpray()
    {
        if (sprayParticles == null)
            return;

        if (!sprayParticles.isPlaying)
        {
            sprayParticles.Play();
        }
    }

    public void StopSpray()
    {
        if (sprayParticles == null)
            return;

        sprayParticles.Stop(
            true,
            ParticleSystemStopBehavior.StopEmitting
        );
    }

    public bool IsSpraying()
    {
        return sprayParticles != null &&
               sprayParticles.isPlaying;
    }

    public ParticleSystem GetParticleSystem()
    {
        return sprayParticles;
    }

    public Transform GetSprayPoint()
    {
        // Never return null - the fire-detection code silently skips
        // extinguishing when the spray point is missing. This component
        // sits on the "spray" object, so it is a valid origin itself.
        if (sprayPoint == null)
            sprayPoint = transform;

        return sprayPoint;
    }
}