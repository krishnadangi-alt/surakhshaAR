using UnityEngine;

public class ExtinguisherSprayCollision : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem sprayParticles;
    [SerializeField] private FireExtinguishSystem fireSystem;

    [Header("Extinguishing")]
    [SerializeField] private float requiredTime = 10f;

    private float contactTimer = 0f;
    private bool touchingFire = false;

    private void Start()
    {
        if (sprayParticles == null)
        {
            sprayParticles = GetComponent<ParticleSystem>();
        }
    }

    private void Update()
    {
        if (fireSystem == null)
            return;

        if (touchingFire)
        {
            contactTimer += Time.deltaTime;

            if (contactTimer >= requiredTime)
            {
                fireSystem.SetSprayHittingFire(true);
            }
        }
        else
        {
            contactTimer = 0f;
            fireSystem.SetSprayHittingFire(false);
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (fireSystem == null)
            return;

        if (IsFireObject(other))
        {
            touchingFire = true;
        }
    }

    private bool IsFireObject(GameObject other)
    {
        if (other == null)
            return false;

        Transform otherTransform = other.transform;

        if (otherTransform.name.Contains("Fire"))
            return true;

        if (otherTransform.GetComponentInParent<FireExtinguishSystem>() != null)
            return true;

        return false;
    }

    private void OnParticleSystemStopped()
    {
        touchingFire = false;
        contactTimer = 0f;

        if (fireSystem != null)
        {
            fireSystem.SetSprayHittingFire(false);
        }
    }
}