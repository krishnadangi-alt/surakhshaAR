using UnityEngine;

public class FireScenarioAlignment : MonoBehaviour
{
    [Header("SCENARIO ROOT")]
    public Transform scenarioRoot;

    [Header("OBJECT REFERENCES")]

    public Transform electricalBox;

    public Transform fireExtinguisher;

    public Transform fireAlarm;

    public Transform exitSign;

    public Transform extinguisherDisplay;


    // =====================================================
    // ANCHOR POINTS (optional, for visual alignment)
    // =====================================================

    [Header("ANCHOR POINTS (optional)")]

    public Transform electricalBoxPoint;

    public Transform extinguisherPoint;

    public Transform fireAlarmPoint;

    public Transform exitPoint;


    // =====================================================
    // ELECTRICAL BOX
    // =====================================================

    [Header("ELECTRICAL BOX")]

    public Vector3 electricalBoxPosition =
        new Vector3(0f, 0f, 3f);

    public Vector3 electricalBoxRotation =
        new Vector3(0f, 0f, 0f);


    // =====================================================
    // FIRE EXTINGUISHER (display - the one that is picked up)
    // =====================================================

    [Header("FIRE EXTINGUISHER")]

    // Right side + slightly behind the electrical box
    public Vector3 fireExtinguisherPosition =
        new Vector3(2.5f, 1.0f, 1.8f);

    public Vector3 fireExtinguisherRotation =
        new Vector3(0f, 0f, 0f);


    // =====================================================
    // FIRE ALARM
    // =====================================================

    [Header("FIRE ALARM")]

    // Near the extinguisher
    public Vector3 fireAlarmPosition =
        new Vector3(0f, 1.2f, 0.5f);

    public Vector3 fireAlarmRotation =
        new Vector3(0f, 0f, 0f);


    // =====================================================
    // EXIT
    // =====================================================

    [Header("EXIT")]

    // Left side on the floor (exact opposite of previous position)
    public Vector3 exitSignPosition =
        new Vector3(-4.5f, 0.0f, 6.0f);

    public Vector3 exitSignRotation =
        new Vector3(0f, 0f, 0f);


    // =====================================================
    // DISPLAY EXTINGUISHER
    // =====================================================

    [Header("EXTINGUISHER DISPLAY")]

    public Vector3 extinguisherDisplayPosition =
        new Vector3(2.5f, 1.0f, 1.8f);

    public Vector3 extinguisherDisplayRotation =
        new Vector3(0f, 0f, 0f);


    [Header("BEHAVIOUR")]

    public bool applyOnEnable = true;

    public bool syncAnchors = true;

    private bool appliedFromStart;

    private void OnEnable()
    {
        // Robust against execution-order issues:
        // alignment is applied whenever the scenario becomes active.
        if (applyOnEnable)
        {
            ApplyAlignment();
        }
    }

    private void Start()
    {
        if (!appliedFromStart)
        {
            ApplyAlignment();

            appliedFromStart = true;
        }
    }


    // =====================================================
    // APPLY ALIGNMENT
    // =====================================================

    public void ApplyAlignment()
    {
        if (scenarioRoot == null)
            scenarioRoot = transform;

        if (extinguisherDisplay == null)
        {
            var dispComp = FindAnyObjectByType<ExtinguisherDisplayPickup>(FindObjectsInactive.Include);
            if (dispComp != null)
            {
                extinguisherDisplay = dispComp.transform;
            }
            else
            {
                var disp = GameObject.Find("FireExt_display");
                if (disp != null) extinguisherDisplay = disp.transform;
            }
        }

        if (fireExtinguisher == null)
        {
            var pickupComp = FindAnyObjectByType<ExtinguisherPickup>(FindObjectsInactive.Include);
            if (pickupComp != null)
            {
                fireExtinguisher = pickupComp.transform;
            }
            else
            {
                var ext = GameObject.Find("FireExt");
                if (ext != null) fireExtinguisher = ext.transform;
            }
        }

        // CRITICAL REQUIREMENT: Original FireExt MUST remain hidden before selection!
        if (fireExtinguisher != null)
        {
            var pickup = fireExtinguisher.GetComponent<ExtinguisherPickup>();
            if (pickup == null || !pickup.IsHeld())
            {
                if (pickup != null)
                {
                    pickup.SetRuntimeVisibility(false);
                }
                else
                {
                    fireExtinguisher.gameObject.SetActive(false);
                }
            }
        }

        // Display extinguisher MUST be active and visible initially
        if (extinguisherDisplay != null)
        {
            var disp = extinguisherDisplay.GetComponent<ExtinguisherDisplayPickup>();
            if (disp == null || !disp.IsPickedUp)
            {
                extinguisherDisplay.gameObject.SetActive(true);
            }
        }

        SetTransform(
            electricalBox,
            electricalBoxPosition,
            electricalBoxRotation
        );

        // NOTE: Preserve original FireExt scene transform! Do NOT overwrite fireExtinguisher transform.

        SetTransform(
            fireAlarm,
            fireAlarmPosition,
            fireAlarmRotation
        );

        SetTransform(
            exitSign,
            exitSignPosition,
            exitSignRotation
        );

        SetTransform(
            extinguisherDisplay,
            extinguisherDisplayPosition,
            extinguisherDisplayRotation
        );

        if (syncAnchors)
        {
            SyncAnchor(electricalBoxPoint, electricalBox);
            SyncAnchor(extinguisherPoint, fireExtinguisher);
            SyncAnchor(fireAlarmPoint, fireAlarm);
            SyncAnchor(exitPoint, exitSign);
        }

        // Ensure fire particles on the hazard are active and playing
        var fireExt = FindAnyObjectByType<FireExtinguishable>(FindObjectsInactive.Include);
        if (fireExt != null)
        {
            fireExt.EnsureFireVisualsActive();
        }
        else
        {
            var hazard = electricalBox != null ? electricalBox.gameObject : (GameObject.Find("Hazard") ?? GameObject.Find("Electric Box"));
            if (hazard != null)
            {
                var vfx = hazard.transform.Find("Electric Box/VFX_Fire_01_Small") ?? hazard.transform.Find("VFX_Fire_01_Small");
                if (vfx != null)
                {
                    vfx.gameObject.SetActive(true);
                    foreach (var ps in vfx.GetComponentsInChildren<ParticleSystem>(true))
                    {
                        ps.gameObject.SetActive(true);
                        if (!ps.isPlaying) ps.Play();
                    }
                }
            }
        }
    }


    // =====================================================
    // SET OBJECT
    // =====================================================

    private void SetTransform(
        Transform target,
        Vector3 position,
        Vector3 rotation
    )
    {
        if (target == null)
        {
            return;
        }

        // CRITICAL PROTECTION: If this target is an extinguisher currently held by the worker,
        // do NOT overwrite its held position or parent relative to the camera!
        var pickup = target.GetComponent<ExtinguisherPickup>();
        if (pickup != null && pickup.IsHeld())
        {
            return;
        }

        target.localPosition =
            position;

        target.localRotation =
            Quaternion.Euler(
                rotation
            );
    }


    // =====================================================
    // SYNC ANCHOR (optional visual helpers)
    // =====================================================

    private void SyncAnchor(
        Transform anchor,
        Transform target
    )
    {
        if (anchor == null || target == null)
            return;

        anchor.position = target.position;
        anchor.rotation = target.rotation;
    }


    // =====================================================
    // UNITY INSPECTOR MENU
    // =====================================================

    [ContextMenu("Apply Alignment")]
    private void ApplyAlignmentFromInspector()
    {
        ApplyAlignment();
    }

    [ContextMenu("Apply Alignment + Anchors")]
    private void ApplyAlignmentAndAnchorsFromInspector()
    {
        syncAnchors = true;
        ApplyAlignment();
    }
}