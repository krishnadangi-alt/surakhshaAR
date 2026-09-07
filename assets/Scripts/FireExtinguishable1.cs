using UnityEngine;
using UnityEngine.Events;

public class FireExtinguishable : MonoBehaviour
{
    [Header("Fire")]
    [SerializeField] public ParticleSystem fireParticle;

    [Header("Extinguishing")]
    [SerializeField] public float extinguishTime = 10f;

    [Header("Powder Spray")]
    [SerializeField] private DryPowderSpray powderSpray;

    [Header("Detection")]
    [SerializeField] private float sprayRange = 5f;

    [SerializeField] private float maxAimAngle = 35f;

    [Header("Events")]
    public UnityEvent OnExtinguished;

    public bool IsExtinguished { get; private set; }

    private float extinguishTimer = 0f;

    // =========================================================
    // PUBLIC PROPERTIES
    // Used by FireScenarioFlowManager
    // =========================================================

    public bool IsBeingSprayed
    {
        get
        {
            if (IsExtinguished)
                return false;

            if (powderSpray == null)
                return false;

            if (!powderSpray.IsSpraying())
                return false;

            return IsSprayAimedAtFire();
        }
    }

    public float SprayProgress01
    {
        get
        {
            if (extinguishTime <= 0f)
                return 1f;

            return Mathf.Clamp01(
                extinguishTimer / extinguishTime
            );
        }
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        IsExtinguished = false;
        extinguishTimer = 0f;

        // Start fire.
        if (fireParticle != null)
        {
            fireParticle.gameObject.SetActive(true);
            fireParticle.Clear();
            fireParticle.Play();
        }

        // Find spray automatically if not assigned.
        if (powderSpray == null)
        {
            powderSpray =
                FindFirstObjectByType<DryPowderSpray>();
        }
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (IsExtinguished)
            return;

        // Find spray if reference is missing.
        if (powderSpray == null)
        {
            powderSpray =
                FindFirstObjectByType<DryPowderSpray>();

            if (powderSpray == null)
                return;
        }

        // Spray is currently aimed at the fire.
        if (IsBeingSprayed)
        {
            extinguishTimer += Time.deltaTime;

            // 10 continuous seconds reached.
            if (extinguishTimer >= extinguishTime)
            {
                ExtinguishFire();
            }
        }
        else
        {
            // Spray is not reaching fire.
            // Reset the timer.
            extinguishTimer = 0f;
        }
    }

    // =========================================================
    // SPRAY DETECTION
    // =========================================================

    private bool IsSprayAimedAtFire()
    {
        Transform sprayPoint =
            powderSpray.GetSprayPoint();

        if (sprayPoint == null)
            return false;

        Vector3 firePosition;

        if (fireParticle != null)
        {
            firePosition =
                fireParticle.transform.position;
        }
        else
        {
            firePosition =
                transform.position;
        }

        Vector3 directionToFire =
            firePosition - sprayPoint.position;

        float distance =
            directionToFire.magnitude;

        // Too far away.
        if (distance > sprayRange)
            return false;

        // Prevent invalid angle calculation.
        if (distance < 0.001f)
            return true;

        directionToFire.Normalize();

        // Normal spray direction.
        float forwardAngle =
            Vector3.Angle(
                sprayPoint.forward,
                directionToFire
            );

        // Support spray pointing backwards.
        float backwardAngle =
            Vector3.Angle(
                -sprayPoint.forward,
                directionToFire
            );

        float finalAngle =
            Mathf.Min(
                forwardAngle,
                backwardAngle
            );

        return finalAngle <= maxAimAngle;
    }

    // =========================================================
    // EXTINGUISH
    // =========================================================

    private void ExtinguishFire()
    {
        if (IsExtinguished)
            return;

        IsExtinguished = true;

        extinguishTimer = extinguishTime;

        // Stop fire immediately.
        if (fireParticle != null)
        {
            fireParticle.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            fireParticle.gameObject.SetActive(false);
        }

        Debug.Log("FIRE_EXTINGUISHED");
        Debug.Log("Fire extinguished after 10 seconds.");

        OnExtinguished?.Invoke();
    }

    // =========================================================
    // PROGRESS
    // =========================================================

    public float GetProgress()
    {
        if (extinguishTime <= 0f)
            return 1f;

        return Mathf.Clamp01(
            extinguishTimer / extinguishTime
        );
    }

    // =========================================================
    // RESET
    // =========================================================

    public void ResetFire()
    {
        IsExtinguished = false;
        extinguishTimer = 0f;

        if (fireParticle != null)
        {
            fireParticle.gameObject.SetActive(true);

            fireParticle.Clear();
            fireParticle.Play();
        }
    }
}