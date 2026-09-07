using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class FireScenarioARPlacement : MonoBehaviour
{
    [Header("AR")]
    public ARRaycastManager raycastManager;
    public Camera arCamera;

    [Header("Fire Scenario")]
    public Transform fireScenario;

    [Header("Placement Indicator")]
    public float circleSize = 0.35f;
    public float circleHeight = 0.015f;
    public float circleThickness = 0.025f;
    public int circleSegments = 64;

    [Header("Circle Appearance")]
    public Color circleColor = new Color(0f, 0.5f, 1f, 0.9f);

    [Header("Behaviour")]
    public bool useARAnchor = true;
    public bool hidePlanesAfterPlacement = true;

    // =====================================================
    // EVENTS
    // =====================================================

    /// <summary>Raised once, the moment the scenario is placed & locked.</summary>
    public UnityEvent OnScenarioPlaced = new UnityEvent();

    /// <summary>Raised when SetPlacementActive is called.</summary>
    public UnityEvent<bool> OnPlacementActiveChanged = new UnityEvent<bool>();

    public bool IsPlaced { get; private set; }

    public bool PlacementActive { get; private set; } = true;

    private GameObject placementCircle;
    private LineRenderer circleRenderer;
    private ARPlaneManager planeManager;

    private static List<ARRaycastHit> hits =
        new List<ARRaycastHit>();

    /// <summary>
    /// Enable/disable the "blue circle + tap to place" system.
    /// The FlowManager keeps it disabled during the intro card and
    /// disables it permanently after the scenario is placed.
    /// </summary>
    public void SetPlacementActive(bool active)
    {
        PlacementActive = active;

        if (!active && placementCircle != null)
        {
            placementCircle.SetActive(false);
        }

        if (OnPlacementActiveChanged != null)
        {
            OnPlacementActiveChanged.Invoke(active);
        }
    }


    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        // Find AR Camera automatically
        if (arCamera == null && Camera.main != null)
        {
            arCamera = Camera.main;
        }

        // Find Raycast Manager automatically
        if (raycastManager == null)
        {
            raycastManager =
                FindFirstObjectByType<ARRaycastManager>();
        }

        // Plane manager lives on the same XR Origin
        if (planeManager == null)
        {
            planeManager =
                GetComponent<ARPlaneManager>();
        }

        if (raycastManager == null)
        {
            Debug.LogError(
                "ARRaycastManager not found!"
            );

            return;
        }

        if (arCamera == null)
        {
            Debug.LogError(
                "AR Camera not assigned!"
            );

            return;
        }

        // Fire scenario can also be found by name (works when inactive).
        if (fireScenario == null)
        {
            GameObject found = GameObject.Find("FireScenario");
            if (found != null)
            {
                fireScenario = found.transform;
            }
        }

        if (fireScenario == null)
        {
            Debug.LogError(
                "Fire Scenario is not assigned!"
            );

            return;
        }

        // Scenario must NOT follow camera
        fireScenario.gameObject.SetActive(false);

        // Create blue placement circle
        CreatePlacementCircle();
    }


    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        // Once placed, do absolutely nothing.
        //
        // The scenario remains fixed in the real world.

        if (IsPlaced)
            return;

        if (!PlacementActive)
            return;

        UpdatePlacementCircle();

        HandleTouch();
    }


    // =====================================================
    // CREATE BLUE CIRCLE
    // =====================================================

    private void CreatePlacementCircle()
    {
        placementCircle =
            new GameObject("PlacementCircle");

        circleRenderer =
            placementCircle.AddComponent<LineRenderer>();

        circleRenderer.loop = true;

        circleRenderer.positionCount =
            circleSegments;

        circleRenderer.startWidth =
            circleThickness;

        circleRenderer.endWidth =
            circleThickness;

        circleRenderer.useWorldSpace = true;

        circleRenderer.material =
            CreateCircleMaterial();

        circleRenderer.startColor =
            circleColor;

        circleRenderer.endColor =
            circleColor;

        placementCircle.SetActive(false);
    }


    // =====================================================
    // CREATE SIMPLE MATERIAL FOR CIRCLE
    // =====================================================

    private Material CreateCircleMaterial()
    {
        Shader shader =
            Shader.Find("Sprites/Default");

        if (shader == null)
        {
            shader =
                Shader.Find("Unlit/Color");
        }

        Material material =
            new Material(shader);

        material.color =
            circleColor;

        return material;
    }


    // =====================================================
    // UPDATE FLOOR CIRCLE
    // =====================================================

    private void UpdatePlacementCircle()
    {
        if (arCamera == null ||
            raycastManager == null)
        {
            return;
        }

        // Screen center of phone
        Vector2 screenCenter =
            new Vector2(
                Screen.width * 0.5f,
                Screen.height * 0.5f
            );

        // Raycast only against detected horizontal planes
        if (raycastManager.Raycast(
            screenCenter,
            hits,
            TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose =
                hits[0].pose;

            placementCircle.SetActive(true);

            DrawCircle(hitPose);
        }
        else
        {
            placementCircle.SetActive(false);
        }
    }


    // =====================================================
    // DRAW CIRCLE ON FLOOR
    // =====================================================

    private void DrawCircle(Pose pose)
    {
        Vector3 center =
            pose.position +
            pose.up * circleHeight;

        for (int i = 0; i < circleSegments; i++)
        {
            float angle =
                ((float)i / circleSegments) *
                Mathf.PI * 2f;

            float x =
                Mathf.Cos(angle) *
                circleSize;

            float z =
                Mathf.Sin(angle) *
                circleSize;

            Vector3 point =
                center +
                pose.right * x +
                pose.forward * z;

            circleRenderer.SetPosition(
                i,
                point
            );
        }
    }


    // =====================================================
    // NEW INPUT SYSTEM - TOUCH
    // =====================================================

    private void HandleTouch()
    {
        // ---------------- Android touch ----------------
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press
                .wasPressedThisFrame)
        {
            Vector2 touchPosition =
                Touchscreen.current.primaryTouch.position
                    .ReadValue();

            TryPlaceScenario(touchPosition);
            return;
        }

        // ---------------- Editor mouse ----------------
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition =
                Mouse.current.position.ReadValue();

            TryPlaceScenario(mousePosition);
        }
    }


    // =====================================================
    // PLACE SCENARIO
    // =====================================================

    private void TryPlaceScenario(
        Vector2 screenPosition
    )
    {
        if (raycastManager == null)
            return;

        // Check whether the worker tapped a detected floor
        if (!raycastManager.Raycast(
            screenPosition,
            hits,
            TrackableType.PlaneWithinPolygon))
        {
            return;
        }

        Pose placementPose =
            hits[0].pose;

        PlaceScenario(
            placementPose
        );
    }


    // =====================================================
    // FINAL SCENARIO PLACEMENT
    // =====================================================

    private void PlaceScenario(
        Pose placementPose
    )
    {
        if (fireScenario == null)
            return;

        // =================================================
        // POSITION
        // =================================================

        fireScenario.position =
            placementPose.position;


        // =================================================
        // CAPTURE CAMERA FORWARD DIRECTION
        // ONLY AT THIS MOMENT
        // =================================================

        Vector3 cameraForward =
            arCamera.transform.forward;

        // Keep direction parallel to the floor.
        cameraForward.y = 0f;

        if (cameraForward.sqrMagnitude >
            0.001f)
        {
            cameraForward.Normalize();

            fireScenario.rotation =
                Quaternion.LookRotation(
                    cameraForward,
                    placementPose.up
                );
        }
        else
        {
            // Fallback if camera direction
            // cannot be calculated.
            fireScenario.rotation =
                Quaternion.LookRotation(
                    placementPose.forward,
                    placementPose.up
                );
        }


        // =================================================
        // SHOW COMPLETE SCENARIO
        // =================================================

        fireScenario.gameObject.SetActive(true);


        // =================================================
        // HIDE PLACEMENT CIRCLE
        // =================================================

        placementCircle.SetActive(false);


        // =================================================
        // WORLD LOCK - REAL-WORLD ANCHOR
        // =================================================

        if (useARAnchor)
        {
            AddAnchorToScenario();
        }

        // =================================================
        // HIDE DETECTED PLANES (optional)
        // =================================================

        if (hidePlanesAfterPlacement)
        {
            HidePlanes();
        }


        // =================================================
        // LOCK PLACEMENT FOREVER
        // =================================================

        IsPlaced = true;

        Debug.Log(
            "FIRE SCENARIO PLACED AND LOCKED."
        );

        if (OnScenarioPlaced != null)
        {
            OnScenarioPlaced.Invoke();
        }
    }


    // =====================================================
    // ADD AR ANCHOR (world locking)
    // =====================================================

    private void AddAnchorToScenario()
    {
        if (fireScenario == null)
            return;

        ARAnchor anchor =
            fireScenario.GetComponent<ARAnchor>();

        if (anchor == null)
        {
            try
            {
                anchor =
                    fireScenario.gameObject.AddComponent<ARAnchor>();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning(
                    "FireScenarioARPlacement: could not add ARAnchor - " +
                    ex.Message
                );
            }
        }
    }


    // =====================================================
    // HIDE PLANES (after placement)
    // =====================================================

    private void HidePlanes()
    {
        if (planeManager == null)
            return;

        if (planeManager.trackables == null)
            return;

        foreach (ARPlane plane in planeManager.trackables)
        {
            if (plane != null)
            {
                plane.gameObject.SetActive(false);
            }
        }

        planeManager.enabled = false;
    }
}