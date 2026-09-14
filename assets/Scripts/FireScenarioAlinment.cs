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

    // Right side and farther away
    public Vector3 exitSignPosition =
        new Vector3(4.5f, 2.0f, 6.0f);

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
        SetTransform(
            electricalBox,
            electricalBoxPosition,
            electricalBoxRotation
        );

        SetTransform(
            fireExtinguisher,
            fireExtinguisherPosition,
            fireExtinguisherRotation
        );

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
            Debug.LogWarning(
                "FireScenarioAlignment: " +
                "An object is not assigned."
            );

            return;
        }

        // IMPORTANT:
        // Do NOT change the parent.
        //
        // This preserves your existing hierarchy,
        // including Hose, spraypoint, hosepivot,
        // Fire particles, etc.

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