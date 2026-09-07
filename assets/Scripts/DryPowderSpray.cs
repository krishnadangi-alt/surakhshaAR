using UnityEngine;

public class DryPowderSpray : MonoBehaviour
{
    [Header("Spray Point")]
    [SerializeField] private Transform sprayPoint;

    [Header("Spray Particle System")]
    [SerializeField] private ParticleSystem sprayParticles;

    private void Awake()
    {
        if (sprayParticles == null)
            sprayParticles = GetComponent<ParticleSystem>();
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
            sprayParticles.Play();
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
        return sprayPoint;
    }
}