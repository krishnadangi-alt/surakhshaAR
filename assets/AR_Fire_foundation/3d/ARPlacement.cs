using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ARPlacement : MonoBehaviour
{
    [Header("Fire Scenario")]
    [SerializeField] private GameObject fireScenario;

    [Header("Scenario Settings")]
    [SerializeField] private float scenarioScale = 1f;
    [SerializeField] private bool usePrefabRotation = true;

    [Header("Placement Indicator")]
    [SerializeField] private float indicatorRadius = 0.12f;
    [SerializeField] private float indicatorWidth = 0.01f;
    [SerializeField] private float indicatorHeight = 0.003f;

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;

    private readonly List<ARRaycastHit> raycastHits =
        new List<ARRaycastHit>();

    private GameObject spawnedScenario;

    private GameObject placementIndicator;
    private LineRenderer indicatorLine;

    private Pose currentPose;
    private bool hasValidPose;
    private bool scenarioPlaced;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnScenarioPlaced = new UnityEngine.Events.UnityEvent();

    public bool IsScenarioPlaced => scenarioPlaced;
    public bool PlacementActive { get; private set; } = true;

    public void SetPlacementActive(bool active)
    {
        PlacementActive = active;
        if (!active && placementIndicator != null)
            placementIndicator.SetActive(false);
    }

    public void SimulatePlacement()
    {
        if (scenarioPlaced) return;
        Pose p = hasValidPose ? currentPose : new Pose(
            Camera.main != null ? Camera.main.transform.position + Camera.main.transform.forward * 1.8f : Vector3.forward * 1.8f,
            Quaternion.identity
        );
        PlaceFireScenario(p);
    }


    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();

        if (raycastManager == null)
        {
            Debug.LogError(
                "ARPlacement: ARRaycastManager is missing from XR Origin."
            );
        }

        if (planeManager == null)
        {
            Debug.LogError(
                "ARPlacement: ARPlaneManager is missing from XR Origin."
            );
        }

        if (fireScenario == null)
        {
            Debug.LogError(
                "ARPlacement: Fire Scenario is not assigned."
            );
        }

        // Hide scenario 3D visuals until placed, but keep FireScenario and FlowManager active so UI initializes
        if (fireScenario != null)
        {
            SetScenarioVisualsActive(false);
        }

        CreatePlacementIndicator();
    }

    public void SetScenarioVisualsActive(bool active)
    {
        if (fireScenario == null) return;
        foreach (Transform child in fireScenario.transform)
        {
            // Do not disable UI or Canvas objects
            if (child.name.Contains("UGUI") || child.name.Contains("UI") || child.GetComponent<FireScenarioUIController>() != null)
                continue;
            child.gameObject.SetActive(active);
        }

        if (active)
        {
            EnsureOperationalExtinguisherHidden();
        }
    }

    public void EnsureOperationalExtinguisherHidden()
    {
        if (fireScenario == null) return;
        var pickups = fireScenario.GetComponentsInChildren<ExtinguisherPickup>(true);
        foreach (var pickup in pickups)
        {
            if (pickup != null && !pickup.IsHeld())
            {
                pickup.gameObject.SetActive(false);
            }
        }

        var displays = fireScenario.GetComponentsInChildren<ExtinguisherDisplayPickup>(true);
        foreach (var disp in displays)
        {
            if (disp != null && !disp.IsPickedUp)
            {
                disp.gameObject.SetActive(true);
            }
        }
    }


    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }


    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }


    private void Update()
    {
        if (scenarioPlaced)
            return;

        if (!PlacementActive)
        {
            if (placementIndicator != null && placementIndicator.activeSelf)
                placementIndicator.SetActive(false);
            return;
        }

        UpdatePlacementPosition();

        CheckForTap();
    }


    // ---------------------------------------------------------
    // AR RAYCAST
    // ---------------------------------------------------------

    private void UpdatePlacementPosition()
    {
        hasValidPose = false;

        if (raycastManager == null)
            return;

        Camera arCamera = Camera.main;

        if (arCamera == null)
            return;

        Vector2 screenCenter = new Vector2(
            Screen.width * 0.5f,
            Screen.height * 0.5f
        );

        raycastHits.Clear();

        bool hit = raycastManager.Raycast(
            screenCenter,
            raycastHits,
            TrackableType.PlaneWithinPolygon
        );

        if (!hit || raycastHits.Count == 0)
        {
            if (placementIndicator != null)
                placementIndicator.SetActive(false);

            return;
        }

        currentPose = raycastHits[0].pose;
        hasValidPose = true;

        UpdateIndicator();
    }


    // ---------------------------------------------------------
    // BLUE CIRCLE
    // ---------------------------------------------------------

    private void CreatePlacementIndicator()
    {
        placementIndicator = new GameObject(
            "RuntimePlacementIndicator"
        );

        placementIndicator.transform.SetParent(
            transform,
            false
        );

        indicatorLine =
            placementIndicator.AddComponent<LineRenderer>();

        indicatorLine.useWorldSpace = false;

        indicatorLine.loop = true;

        indicatorLine.positionCount = 65;

        indicatorLine.startWidth = indicatorWidth;
        indicatorLine.endWidth = indicatorWidth;

        indicatorLine.numCapVertices = 4;

        indicatorLine.numCornerVertices = 4;

        // Try URP first.
        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Unlit"
            );

        // Fallback.
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        if (shader != null)
        {
            Material material =
                new Material(shader);

            material.color =
                new Color(
                    0.0f,
                    0.65f,
                    1.0f,
                    1.0f
                );

            indicatorLine.material = material;
        }

        CreateCircleVertices();

        placementIndicator.SetActive(false);
    }


    private void CreateCircleVertices()
    {
        if (indicatorLine == null)
            return;

        int segments = 64;

        indicatorLine.positionCount =
            segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float angle =
                (float)i /
                segments *
                Mathf.PI *
                2f;

            float x =
                Mathf.Cos(angle) *
                indicatorRadius;

            float z =
                Mathf.Sin(angle) *
                indicatorRadius;

            indicatorLine.SetPosition(
                i,
                new Vector3(
                    x,
                    0f,
                    z
                )
            );
        }
    }


    private void UpdateIndicator()
    {
        if (placementIndicator == null)
            return;

        if (!hasValidPose)
        {
            placementIndicator.SetActive(false);
            return;
        }

        placementIndicator.SetActive(true);

        placementIndicator.transform.position =
            currentPose.position +
            currentPose.rotation *
            Vector3.up *
            indicatorHeight;

        placementIndicator.transform.rotation =
            currentPose.rotation;
    }


    // ---------------------------------------------------------
    // TOUCH
    // ---------------------------------------------------------

    private void CheckForTap()
    {
        Vector2 touchPosition = Vector2.zero;
        bool hasTap = false;

        // 1. Direct Touchscreen input (primary New Input System on Android)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            hasTap = true;
        }
        // 2. EnhancedTouch fallback
        else if (Touch.activeTouches.Count > 0 && Touch.activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            touchPosition = Touch.activeTouches[0].screenPosition;
            hasTap = true;
        }
#if UNITY_EDITOR || UNITY_STANDALONE
        // 3. Mouse input for Editor testing
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            touchPosition = Mouse.current.position.ReadValue();
            hasTap = true;
        }
#endif

        if (!hasTap) return;

        raycastHits.Clear();
        bool hit = raycastManager != null && raycastManager.Raycast(
            touchPosition,
            raycastHits,
            TrackableType.PlaneWithinPolygon
        );

        if (hit && raycastHits.Count > 0)
        {
            PlaceFireScenario(raycastHits[0].pose);
        }
        else
        {
            // Fallback for editor or non-detected surfaces: anchor in front of camera or at current pose
            Pose hitPose = hasValidPose ? currentPose : new Pose(
                Camera.main != null ? Camera.main.transform.position + Camera.main.transform.forward * 1.8f : Vector3.forward * 1.8f,
                Quaternion.identity
            );
            PlaceFireScenario(hitPose);
        }
    }


    // ---------------------------------------------------------
    // PLACE COMPLETE FIRE SCENARIO
    // ---------------------------------------------------------

    private void PlaceFireScenario(Pose hitPose)
    {
        if (scenarioPlaced)
            return;

        if (fireScenario == null)
            return;

        // Immediately lock placement to prevent duplicate triggers
        scenarioPlaced = true;
        PlacementActive = false;

        Quaternion rotation;

        if (usePrefabRotation)
        {
            rotation = fireScenario.transform.rotation;
        }
        else
        {
            rotation = Quaternion.Euler(
                0f,
                hitPose.rotation.eulerAngles.y,
                0f
            );
        }

        // Position existing in-scene scenario and activate 3D visuals
        fireScenario.transform.position = hitPose.position;
        fireScenario.transform.rotation = rotation;
        fireScenario.transform.localScale = Vector3.one * scenarioScale;
        SetScenarioVisualsActive(true);

        spawnedScenario = fireScenario;
        spawnedScenario.SetActive(true);

        // Hide blue circle
        if (placementIndicator != null)
        {
            placementIndicator.SetActive(false);
        }

        // Stop plane detection safely
        HidePlanes();

        Debug.Log("[ARPlacement] FireScenario placed successfully at: " + hitPose.position);

        // Notify flow manager
        if (spawnedScenario != null)
        {
            var flows = spawnedScenario.GetComponentsInChildren<FireScenarioFlowManager>(true);
            foreach (var flow in flows)
            {
                flow.NotifyScenarioPlacedByAR();
            }
        }

        OnScenarioPlaced?.Invoke();
    }


    // ---------------------------------------------------------
    // HIDE PLANES
    // ---------------------------------------------------------

    private void HidePlanes()
    {
        if (planeManager == null)
            return;

        try
        {
            foreach (ARPlane plane in planeManager.trackables)
            {
                if (plane != null && plane.gameObject != null)
                {
                    plane.gameObject.SetActive(false);
                }
            }
            planeManager.enabled = false;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[ARPlacement] HidePlanes exception handled: " + ex.Message);
        }
    }


    // ---------------------------------------------------------
    // CLEANUP
    // ---------------------------------------------------------

    private void OnDestroy()
    {
        if (placementIndicator != null)
        {
            Destroy(placementIndicator);
        }
    }
}