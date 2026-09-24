using UnityEngine;

public class FireExtinguishSystem : MonoBehaviour
{
    [Header("Fire")]
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private GameObject fireObject;

    [Header("Extinguishing")]
    [SerializeField] private float requiredTime = 10f;

    [Header("Audio")]
    [SerializeField] private AudioSource sprayAudio;

    private float timer;
    private bool sprayTouchingFire;
    private bool extinguished;

    private void Start()
    {
        timer = 0f;
        extinguished = false;

        if (fireObject != null)
        {
            fireObject.SetActive(true);
        }

        if (fireParticles != null)
        {
            fireParticles.Play();
        }
    }

    private void Update()
    {
        if (extinguished)
            return;

        if (sprayTouchingFire)
        {
            timer += Time.deltaTime;

            if (timer >= requiredTime)
            {
                ExtinguishFire();
            }
        }
        else
        {
            // Pause timer when spray moves off target (preserves accumulated progress)
        }
    }

    public void SetSprayHittingFire(bool hitting)
    {
        if (extinguished)
            return;

        sprayTouchingFire = hitting;

        if (hitting)
        {
            if (sprayAudio != null &&
                !sprayAudio.isPlaying)
            {
                sprayAudio.Play();
            }
        }
    }

    private void ExtinguishFire()
    {
        extinguished = true;
        sprayTouchingFire = false;

        if (fireParticles != null)
        {
            fireParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        if (fireObject != null)
        {
            fireObject.SetActive(false);
        }

        if (sprayAudio != null)
        {
            sprayAudio.Stop();
        }

        Debug.Log("FIRE_EXTINGUISHED");
    }

    public float GetProgress()
    {
        if (requiredTime <= 0f)
            return 1f;

        return Mathf.Clamp01(timer / requiredTime);
    }

    public bool IsExtinguished()
    {
        return extinguished;
    }
}