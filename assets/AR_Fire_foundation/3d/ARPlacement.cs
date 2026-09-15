using System.Collections.Generic;
using UnityEngine;
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
        Pose p = hasValidPose ? currentPose : new Pose(Vector3.forward * 2f, Quaternion.identity);
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

        // The template must NEVER be visible before placement.
        if (fireScenario != null)
        {
            fireScenario.SetActive(false);
        }

        CreatePlacementIndicator();
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
        if (Touch.activeTouches.Count == 0)
            return;

        Touch touch =
            Touch.activeTouches[0];

        if (touch.phase !=
            UnityEngine.InputSystem.TouchPhase.Began)
        {
            return;
        }

        Vector2 touchPosition =
            touch.screenPosition;

        raycastHits.Clear();

        bool hit =
            raycastManager.Raycast(
                touchPosition,
                raycastHits,
                TrackableType.PlaneWithinPolygon
            );

        if (!hit || raycastHits.Count == 0)
        {
            Debug.Log(
                "ARPlacement: Tap did not hit a detected floor."
            );

            return;
        }

        Pose hitPose =
            raycastHits[0].pose;

        PlaceFireScenario(hitPose);
    }


    // ---------------------------------------------------------
    // PLACE COMPLETE FIRE SCENARIO
    // ---------------------------------------------------------

    private void PlaceFireScenario(Pose hitPose)
    {
        if (fireScenario == null)
            return;

        Quaternion rotation;

        if (usePrefabRotation)
        {
            rotation =
                fireScenario.transform.rotation;
        }
        else
        {
            rotation =
                Quaternion.Euler(
                    0f,
                    hitPose.rotation.eulerAngles.y,
                    0f
                );
        }


        spawnedScenario =
            Instantiate(
                fireScenario,
                hitPose.position,
                rotation
            );


        spawnedScenario.SetActive(true);


        spawnedScenario.transform.localScale =
            Vector3.one * scenarioScale;


        // Add AR Anchor to the complete scenario.
        ARAnchor anchor =
            spawnedScenario.GetComponent<ARAnchor>();

        if (anchor == null)
        {
            anchor =
                spawnedScenario.AddComponent<ARAnchor>();
        }


        // Hide original template.
        fireScenario.SetActive(false);


        // Hide blue circle.
        if (placementIndicator != null)
        {
            placementIndicator.SetActive(false);
        }


        // Placement is permanently finished.
        scenarioPlaced = true;
        PlacementActive = false;


        // Stop plane detection.
        HidePlanes();


        Debug.Log(
            "training_started"
        );

        Debug.Log(
            "FireScenario placed and anchored."
        );

        if (spawnedScenario != null)
        {
            var flows = spawnedScenario.GetComponentsInChildren<FireScenarioFlowManager>(true);
            foreach (var flow in flows)
            {
                flow.NotifyScenarioPlacedByAR();
            }
        }

        OnScenarioPlaced?.Invoke();
        TrainingEventManager.RaiseScenarioPlaced();
    }


    // ---------------------------------------------------------
    // HIDE PLANES
    // ---------------------------------------------------------

    private void HidePlanes()
    {
        if (planeManager == null)
            return;

        foreach (ARPlane plane in
                 planeManager.trackables)
        {
            if (plane != null)
            {
                plane.gameObject.SetActive(false);
            }
        }

        planeManager.enabled = false;
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