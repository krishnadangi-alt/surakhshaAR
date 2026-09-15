using UnityEngine;
using UnityEngine.Events;

public class ExtinguisherPickup : MonoBehaviour
{
    [Header("AR Camera")]
    public Transform arCamera;

    [Header("Position While Holding")]
    public Vector3 holdPosition =
        new Vector3(0f, -0.70f, 0.80f);

    [Header("Rotation While Holding")]
    public Vector3 holdRotation =
        new Vector3(0f, 20f, 0f);

    [Header("Hose References")]
    public Transform hose;

    public Transform hosePivot;

    public Transform sprayPoint;

    [Header("Events")]
    public UnityEvent OnAttachedToCamera =
        new UnityEvent();

    // Spray point position relative to hose.
    private Vector3 sprayPointLocalPosition;

    private bool isHeld = false;


    // =====================================================
    // UNITY LIFECYCLE
    // =====================================================

    private void Awake()
    {
        FindReferences();
        CacheSprayPointPosition();
    }


    // =====================================================
    // FIND REFERENCES
    // =====================================================

    private void FindReferences()
    {
        // AR Camera
        if (arCamera == null && Camera.main != null)
        {
            arCamera = Camera.main.transform;
        }

        // Hose
        if (hose == null)
        {
            hose = transform.Find("Hose");

            if (hose == null)
            {
                hose = transform.Find("hose");
            }
        }

        // Hose Pivot
        if (hosePivot == null)
        {
            hosePivot = transform.Find("hosepivot");

            if (hosePivot == null)
            {
                hosePivot = transform.Find("HosePivot");
            }
        }

        // Spray Point
        if (sprayPoint == null)
        {
            sprayPoint = transform.Find("spraypoint");

            if (sprayPoint == null)
            {
                sprayPoint = transform.Find("SprayPoint");
            }
        }
    }


    // =====================================================
    // CACHE SPRAY POINT
    // =====================================================

    private void CacheSprayPointPosition()
    {
        if (hose != null && sprayPoint != null)
        {
            sprayPointLocalPosition =
                hose.InverseTransformPoint(
                    sprayPoint.position
                );
        }
        else
        {
            Debug.LogWarning(
                "[ExtinguisherPickup] " +
                "Hose or Spray Point is missing."
            );
        }
    }


    // =====================================================
    // ATTACH TO AR CAMERA
    // =====================================================

    public void AttachToCamera()
    {
        if (isHeld)
            return;

        // Find camera again if necessary
        if (arCamera == null && Camera.main != null)
        {
            arCamera = Camera.main.transform;
        }

        if (arCamera == null)
        {
            Debug.LogError(
                "[ExtinguisherPickup] " +
                "AR Camera not found."
            );

            return;
        }

        if (hose == null)
        {
            Debug.LogError(
                "[ExtinguisherPickup] " +
                "Hose reference is missing."
            );

            return;
        }

        if (hosePivot == null)
        {
            Debug.LogError(
                "[ExtinguisherPickup] " +
                "Hose Pivot reference is missing."
            );

            return;
        }

        if (sprayPoint == null)
        {
            Debug.LogError(
                "[ExtinguisherPickup] " +
                "Spray Point reference is missing."
            );

            return;
        }

        isHeld = true;

        // Attach extinguisher to AR camera
        transform.SetParent(
            arCamera,
            true
        );

        // Position
        transform.localPosition =
            holdPosition;

        // Rotation
        transform.localRotation =
            Quaternion.Euler(
                holdRotation
            );

        UpdateSprayPoint();

        Debug.Log(
            "[ExtinguisherPickup] " +
            "Extinguisher attached to AR camera."
        );

        OnAttachedToCamera?.Invoke();
    }


    // =====================================================
    // DETACH
    // =====================================================

    public void DetachFromCamera()
    {
        if (!isHeld)
            return;

        transform.SetParent(
            null,
            true
        );

        isHeld = false;

        Debug.Log(
            "[ExtinguisherPickup] " +
            "Extinguisher detached."
        );
    }


    // =====================================================
    // SPRAY POINT
    // =====================================================

    public void UpdateSprayPoint()
    {
        if (hose == null ||
            sprayPoint == null)
        {
            return;
        }

        sprayPoint.position =
            hose.TransformPoint(
                sprayPointLocalPosition
            );

        sprayPoint.rotation = hose.rotation * Quaternion.Euler(0, 180, 0);
    }


    public void RecalculateSprayPoint()
    {
        CacheSprayPointPosition();
        UpdateSprayPoint();
    }


    // =====================================================
    // HOSE ROTATION
    // IMPORTANT:
    // HouseAdjustUI uses Vector3/Euler angles.
    // =====================================================

    public Vector3 CurrentHoseRotation
    {
        get
        {
            if (hosePivot == null)
            {
                return Vector3.zero;
            }

            return hosePivot.localEulerAngles;
        }
    }


    public void SetHoseRotation(Vector3 rotation)
    {
        if (hosePivot == null)
        {
            Debug.LogWarning(
                "[ExtinguisherPickup] " +
                "Hose Pivot is missing."
            );

            return;
        }

        hosePivot.localEulerAngles =
            rotation;

        UpdateSprayPoint();
    }


    // =====================================================
    // RESET HOSE
    // =====================================================

    public void ResetHoseRotation()
    {
        if (hosePivot == null)
            return;

        hosePivot.localEulerAngles =
            Vector3.zero;

        UpdateSprayPoint();
    }


    // =====================================================
    // STATE
    // =====================================================

    public bool IsHeld()
    {
        return isHeld;
    }


    // =====================================================
    // REFERENCES
    // =====================================================

    public Transform GetHose()
    {
        return hose;
    }


    public Transform GetHosePivot()
    {
        return hosePivot;
    }


    public Transform GetSprayPoint()
    {
        return sprayPoint;
    }
}