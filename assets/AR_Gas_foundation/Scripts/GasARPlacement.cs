using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class GasARPlacement : MonoBehaviour
{
    [Header("AR References")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;

    [Header("Gas Environment")]
    [SerializeField] private GasEnvironmentBuilder environmentBuilder;
    [SerializeField] private Transform gasEnvironmentRoot;
    [SerializeField] private GasTrainingStepController stepController;

    [Header("Placement Reticle Settings")]
    [SerializeField] private Material reticleMaterial;
    [SerializeField] private float indicatorRadius = 0.12f;
    [SerializeField] private float indicatorWidth = 0.01f;
    [SerializeField] private float indicatorHeight = 0.003f;

    private readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private GameObject placementIndicator;
    private LineRenderer indicatorLine;
    private Pose currentPose;
    private bool hasValidPose;
    private bool placed = false;
    private bool loggedPose = false;

    private bool isNonArMode = false;

    private void Awake()
    {
        Debug.Log("[GasAR] Placement script started");

        if (raycastManager == null) raycastManager = GetComponent<ARRaycastManager>();
        if (planeManager == null) planeManager = GetComponent<ARPlaneManager>();

        if (planeManager != null)
        {
            planeManager.trackablesChanged.AddListener(OnPlanesChanged);
        }

        if (environmentBuilder == null) environmentBuilder = FindFirstObjectByType<GasEnvironmentBuilder>();
        if (stepController == null) stepController = FindFirstObjectByType<GasTrainingStepController>();

        if (gasEnvironmentRoot == null)
        {
            GameObject root = GameObject.Find("GasScenarioRoot");
            if (root != null) gasEnvironmentRoot = root.transform;
        }

        CreatePlacementIndicator();
    }

    private void Start()
    {
        StartCoroutine(CheckArSupportAndInitialize());
    }

    private System.Collections.IEnumerator CheckArSupportAndInitialize()
    {
        if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
        {
            yield return ARSession.CheckAvailability();
        }

        if (ARSession.state == ARSessionState.Unsupported || raycastManager == null)
        {
            Debug.Log("[GasAR] ARCore unavailable on device. Activating Non-AR 3D Training Mode Fallback.");
            ActivateNonArMode();
        }
        else
        {
            Debug.Log($"[GasAR] ARCore supported on device. State: {ARSession.state}");
        }
    }

    public void ActivateNonArMode()
    {
        if (isNonArMode) return;
        isNonArMode = true;

        if (placementIndicator != null) placementIndicator.SetActive(false);
        if (indicatorLine != null) indicatorLine.enabled = false;
        if (planeManager != null) planeManager.enabled = false;
        HidePlanes();

        Camera arCamera = Camera.main;
        if (arCamera == null) arCamera = FindFirstObjectByType<Camera>();
        if (arCamera != null)
        {
            arCamera.transform.position = new Vector3(0.00f, 1.35f, -2.80f);
            arCamera.transform.LookAt(new Vector3(0.00f, 0.65f, 0.10f));
            arCamera.fieldOfView = 55f;
            arCamera.nearClipPlane = 0.05f;
        }

        if (gasEnvironmentRoot != null)
        {
            gasEnvironmentRoot.position = Vector3.zero;
            gasEnvironmentRoot.rotation = Quaternion.identity;
        }

        if (environmentBuilder != null && !placed)
        {
            environmentBuilder.BuildEnvironment();
            placed = true;
        }

        if (stepController != null)
        {
            stepController.OnScenarioPlaced();
        }

        Debug.Log("[GasAR] 3D Training Mode (Non-AR Fallback) activated with optimal mobile camera framing!");
    }

    private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> eventArgs)
    {
        if (placed || isNonArMode)
        {
            HidePlanes();
        }
    }

    private void OnEnable()
    {
        try { EnhancedTouchSupport.Enable(); } catch { }
    }

    private void OnDisable()
    {
        if (planeManager != null) planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
        try { EnhancedTouchSupport.Disable(); } catch { }
    }

    private void Update()
    {
        if (placed || isNonArMode)
        {
            if (placementIndicator != null && placementIndicator.activeSelf) placementIndicator.SetActive(false);
            if (indicatorLine != null && indicatorLine.enabled) indicatorLine.enabled = false;
            return;
        }

        UpdatePlacementIndicatorPosition();
        CheckForTap();
    }

    private void UpdatePlacementIndicatorPosition()
    {
        hasValidPose = false;

        if (raycastManager == null) return;

        Camera arCamera = Camera.main;
        if (arCamera == null) arCamera = FindFirstObjectByType<Camera>();
        if (arCamera == null) return;

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        hits.Clear();

        bool hit = raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon)
                || raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinBounds)
                || raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinInfinity);

        if (!hit || hits.Count == 0)
        {
            if (placementIndicator != null) placementIndicator.SetActive(false);
            return;
        }

        currentPose = hits[0].pose;
        hasValidPose = true;

        if (!loggedPose)
        {
            loggedPose = true;
            Debug.Log($"[GasAR] Valid placement pose found: {currentPose.position}");
        }

        UpdateIndicatorVisual();
    }

    private void CreatePlacementIndicator()
    {
        placementIndicator = new GameObject("RuntimePlacementIndicator");
        placementIndicator.transform.SetParent(transform, false);

        indicatorLine = placementIndicator.AddComponent<LineRenderer>();
        indicatorLine.useWorldSpace = false;
        indicatorLine.loop = true;
        indicatorLine.startWidth = indicatorWidth;
        indicatorLine.endWidth = indicatorWidth;
        indicatorLine.numCapVertices = 4;
        indicatorLine.numCornerVertices = 4;

        Material matToUse = null;
        if (reticleMaterial != null)
        {
            matToUse = new Material(reticleMaterial);
        }
        else
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Sprites/Default")
                         ?? Shader.Find("Unlit/Color");
            if (shader != null)
            {
                matToUse = new Material(shader);
                matToUse.color = new Color(0.0f, 0.65f, 1.0f, 0.85f);
            }
        }

        if (matToUse != null) indicatorLine.material = matToUse;

        CreateCircleVertices();
        placementIndicator.SetActive(false);
    }

    private void CreateCircleVertices()
    {
        if (indicatorLine == null) return;
        int segments = 64;
        indicatorLine.positionCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * indicatorRadius;
            float z = Mathf.Sin(angle) * indicatorRadius;
            indicatorLine.SetPosition(i, new Vector3(x, 0f, z));
        }
    }

    private void UpdateIndicatorVisual()
    {
        if (placementIndicator == null) return;

        if (!hasValidPose || placed || isNonArMode)
        {
            placementIndicator.SetActive(false);
            if (indicatorLine != null) indicatorLine.enabled = false;
            return;
        }

        placementIndicator.SetActive(true);
        if (indicatorLine != null) indicatorLine.enabled = true;
        placementIndicator.transform.position = currentPose.position + currentPose.rotation * Vector3.up * indicatorHeight;
        placementIndicator.transform.rotation = currentPose.rotation;
    }

    private void CheckForTap()
    {
        Vector2 tapPosition = Vector2.zero;
        bool hasTap = false;

        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                tapPosition = touch.screenPosition;
                hasTap = true;
            }
        }

        if (!hasTap && Input.touchCount > 0)
        {
            UnityEngine.Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                tapPosition = touch.position;
                hasTap = true;
            }
        }

        if (!hasTap && Input.GetMouseButtonDown(0))
        {
            tapPosition = Input.mousePosition;
            hasTap = true;
        }

        if (hasTap)
        {
            TryPlaceAtTap(tapPosition);
        }
    }

    private void TryPlaceAtTap(Vector2 screenPosition)
    {
        if (raycastManager == null || environmentBuilder == null || gasEnvironmentRoot == null) return;

        hits.Clear();
        bool hit = raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon)
                || raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinBounds)
                || raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinInfinity);

        if (!hit || hits.Count == 0) return;

        Pose hitPose = hits[0].pose;
        gasEnvironmentRoot.position = hitPose.position;

        Camera arCamera = Camera.main;
        if (arCamera == null) arCamera = FindFirstObjectByType<Camera>();
        if (arCamera != null)
        {
            Vector3 lookDir = hitPose.position - arCamera.transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                gasEnvironmentRoot.rotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
            }
            else
            {
                gasEnvironmentRoot.rotation = Quaternion.Euler(0f, hitPose.rotation.eulerAngles.y, 0f);
            }
        }
        else
        {
            gasEnvironmentRoot.rotation = Quaternion.Euler(0f, hitPose.rotation.eulerAngles.y, 0f);
        }

        gasEnvironmentRoot.localScale = Vector3.one;

        environmentBuilder.BuildEnvironment();

        ARAnchor existingAnchor = gasEnvironmentRoot.GetComponent<ARAnchor>();
        if (existingAnchor != null)
        {
            if (Application.isPlaying) Destroy(existingAnchor);
            else DestroyImmediate(existingAnchor);
        }

        if (placementIndicator != null) placementIndicator.SetActive(false);
        if (indicatorLine != null) indicatorLine.enabled = false;
        placed = true;

        HidePlanes();

        if (stepController != null)
        {
            stepController.OnScenarioPlaced();
        }
    }

    public void HidePlanes()
    {
        if (planeManager != null)
        {
            planeManager.requestedDetectionMode = PlaneDetectionMode.None;
            foreach (ARPlane plane in planeManager.trackables)
            {
                if (plane != null)
                {
                    Renderer[] rends = plane.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in rends) r.enabled = false;
                    LineRenderer[] lines = plane.GetComponentsInChildren<LineRenderer>(true);
                    foreach (var l in lines) l.enabled = false;
                    plane.gameObject.SetActive(false);
                }
            }
            planeManager.enabled = false;
        }

        ARPlane[] allPlanes = FindObjectsByType<ARPlane>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in allPlanes)
        {
            if (p != null)
            {
                Renderer[] rends = p.GetComponentsInChildren<Renderer>(true);
                foreach (var r in rends) r.enabled = false;
                LineRenderer[] lines = p.GetComponentsInChildren<LineRenderer>(true);
                foreach (var l in lines) l.enabled = false;
                p.gameObject.SetActive(false);
            }
        }
    }

    public void ResetPlacement()
    {
        placed = false;
        loggedPose = false;

        if (environmentBuilder != null) environmentBuilder.ClearEnvironment();
        if (planeManager != null) planeManager.enabled = true;
        if (placementIndicator != null) placementIndicator.SetActive(false);
        if (indicatorLine != null) indicatorLine.enabled = false;
    }

    private void OnDestroy()
    {
        if (placementIndicator != null) Destroy(placementIndicator);
    }
}