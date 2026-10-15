using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace SurakshaAR.Machinery
{
    /// <summary>
    /// MachineryRealARController
    /// =========================
    /// Authentic Real-World 3D AR Controller for the complete Machinery setup:
    /// - Ensures AR Camera tracks real world movements independently via AR Foundation & TrackedPoseDriver.
    /// - Starts with real phone camera immediately (Machinery hidden on startup).
    /// - Detects horizontal surfaces using real ARCore Plane Detection.
    /// - Renders a real world-space blue placement indicator on detected floor.
    /// - On user tap: places the COMPLETE Machinery setup anchored in real world space.
    /// - Complete Machinery has one consistent root-level runtime scale (~2.0m human-scale span).
    /// - All child objects maintain 100% of their exact relative arrangements and scales.
    /// - Bottom of equipment rests cleanly on the detected horizontal plane.
    /// - Real world-space: once placed, root is pinned by ARAnchor and its position/rotation freeze permanently.
    /// - Camera moves completely independently, enabling full physical 360-degree walk-around and perspective parallax.
    /// </summary>
    public class MachineryRealARController : MonoBehaviour
    {
        [Header("Machinery Target")]
        [Tooltip("The placement root containing the complete MachineryScenario.")]
        [SerializeField] private GameObject machineryPlacementRoot;

        [Header("AR Foundation Components")]
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private ARAnchorManager anchorManager;
        [SerializeField] private ARSession arSession;
        [SerializeField] private ARCameraManager cameraManager;
        [SerializeField] private Camera mainCamera;

        [Header("Runtime Scale & Alignment")]
        [Tooltip("Consistent root-level scale factor for the machinery (0.35 scales 5.75m wide equipment to ~2.0m wide).")]
        [SerializeField] [Range(0.2f, 1.0f)] private float placementScale = 0.35f;

        [Header("Minimal UI")]
        [SerializeField] private Text statusText;
        [SerializeField] private Button resetButton;

        [Header("Placement Indicator Settings")]
        [SerializeField] private float indicatorRadius = 0.35f;
        [SerializeField] private float indicatorWidth = 0.03f;
        [SerializeField] private Material indicatorMaterial;

        private GameObject placementIndicator;
        private LineRenderer indicatorLine;
        private Pose currentPose;
        private ARPlane currentPlane;
        private bool hasValidPose;
        private bool isPlaced;
        private bool frameReceived;
        private int frameCount;

        private static readonly List<ARRaycastHit> RaycastHits = new List<ARRaycastHit>();

        public bool IsPlaced => isPlaced;
        public bool HasValidPose => hasValidPose;

        private void Awake()
        {
            if (raycastManager == null) raycastManager = GetComponent<ARRaycastManager>() ?? FindAnyObjectByType<ARRaycastManager>();
            if (planeManager == null) planeManager = GetComponent<ARPlaneManager>() ?? FindAnyObjectByType<ARPlaneManager>();
            if (anchorManager == null) anchorManager = GetComponent<ARAnchorManager>() ?? FindAnyObjectByType<ARAnchorManager>();
            if (arSession == null) arSession = FindAnyObjectByType<ARSession>();
            if (cameraManager == null) cameraManager = FindAnyObjectByType<ARCameraManager>();
            if (mainCamera == null) mainCamera = Camera.main ?? FindAnyObjectByType<Camera>();

            // Ensure camera tracking inputs are firmly bound so camera moves independently in world space
            EnsureCameraTracking();

            // MANDATORY: Complete machinery is HIDDEN at app startup
            if (machineryPlacementRoot != null)
            {
                machineryPlacementRoot.SetActive(false);
            }

            if (resetButton != null)
            {
                resetButton.onClick.AddListener(ResetPlacement);
                resetButton.gameObject.SetActive(false);
            }

            CreatePlacementIndicator();
            RequestCameraPermission();

            if (statusText != null)
            {
                if (statusText.font == null)
                {
                    statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                statusText.text = "Scan the floor to place the machinery";
            }
        }

        private void Start()
        {
            EnsureCameraTracking();

            if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
            {
                if (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
                {
                    XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
                }
                if (XRGeneralSettings.Instance.Manager.activeLoader != null)
                {
                    XRGeneralSettings.Instance.Manager.StartSubsystems();
                }
            }

            if (planeManager != null)
            {
                planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
                planeManager.enabled = true;
            }
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            EnsureCameraTracking();
            if (cameraManager != null)
            {
                cameraManager.frameReceived += OnCameraFrameReceived;
            }
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
            if (cameraManager != null)
            {
                cameraManager.frameReceived -= OnCameraFrameReceived;
            }
        }

        /// <summary>
        /// Guarantees that TrackedPoseDriver on the main camera has active bindings for handheld AR devices,
        /// ensuring the Unity virtual camera moves and rotates freely in 3D world space matching the real phone.
        /// </summary>
        public void EnsureCameraTracking()
        {
            if (mainCamera == null) mainCamera = Camera.main ?? FindAnyObjectByType<Camera>();
            if (mainCamera == null) return;

            var poseDriver = mainCamera.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
            if (poseDriver == null)
            {
                poseDriver = mainCamera.gameObject.AddComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
            }

            if (poseDriver != null)
            {
                // Verify or add position input bindings
                var posAction = poseDriver.positionInput.action;
                if (posAction != null)
                {
                    if (posAction.bindings.Count == 0)
                    {
                        posAction.AddBinding("<HandheldARInputDevice>/devicePosition");
                        posAction.AddBinding("<XRHMD>/centerEyePosition");
                    }
                    if (!posAction.enabled) posAction.Enable();
                }

                // Verify or add rotation input bindings
                var rotAction = poseDriver.rotationInput.action;
                if (rotAction != null)
                {
                    if (rotAction.bindings.Count == 0)
                    {
                        rotAction.AddBinding("<HandheldARInputDevice>/deviceRotation");
                        rotAction.AddBinding("<XRHMD>/centerEyeRotation");
                    }
                    if (!rotAction.enabled) rotAction.Enable();
                }

                // Verify tracking state input bindings
                var trackAction = poseDriver.trackingStateInput.action;
                if (trackAction != null)
                {
                    if (trackAction.bindings.Count == 0)
                    {
                        trackAction.AddBinding("<HandheldARInputDevice>/trackingState");
                        trackAction.AddBinding("<XRHMD>/trackingState");
                    }
                    if (!trackAction.enabled) trackAction.Enable();
                }
            }
        }

        private void RequestCameraPermission()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
            }
#endif
        }

        private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
        {
            frameReceived = true;
            frameCount++;
        }

        private void Update()
        {
            // CRITICAL: Once placed, world position and rotation are permanently frozen!
            if (isPlaced) return;

            UpdatePlacementPosition();
            CheckForTap();
        }

        private void UpdatePlacementPosition()
        {
            hasValidPose = false;
            currentPlane = null;

            if (mainCamera == null) mainCamera = Camera.main;

            // 1. Raycast against real detected horizontal planes from center/lower screen
            if (raycastManager != null)
            {
                Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                RaycastHits.Clear();

                bool hit = raycastManager.Raycast(screenCenter, RaycastHits, TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds);
                if (!hit)
                {
                    Vector2 screenLower = new Vector2(Screen.width * 0.5f, Screen.height * 0.65f);
                    hit = raycastManager.Raycast(screenLower, RaycastHits, TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds);
                }

                if (hit && RaycastHits.Count > 0)
                {
                    var hitResult = RaycastHits[0];
                    currentPose = hitResult.pose;
                    if (planeManager != null)
                    {
                        currentPlane = planeManager.GetPlane(hitResult.trackableId);
                    }
                    hasValidPose = true;
                }
            }

            // 2. Editor simulation fallback so developers can test placement on ground plane without device
#if UNITY_EDITOR
            if (!hasValidPose && mainCamera != null)
            {
                Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                Plane floorPlane = new Plane(Vector3.up, Vector3.zero);
                if (floorPlane.Raycast(ray, out float enter))
                {
                    currentPose = new Pose(ray.GetPoint(enter), Quaternion.identity);
                    hasValidPose = true;
                }
            }
#endif

            // 3. Update Indicator & UI text
            if (hasValidPose)
            {
                UpdateIndicator();
                if (statusText != null)
                {
                    statusText.text = "Floor detected!\nTap to place Machinery on the floor.";
                }
            }
            else
            {
                if (placementIndicator != null) placementIndicator.SetActive(false);
                if (statusText != null)
                {
                    statusText.text = "Scan the floor to place the machinery";
                }
            }
        }

        private void CreatePlacementIndicator()
        {
            placementIndicator = new GameObject("RealARPlacementIndicator");
            placementIndicator.transform.SetParent(transform, false);

            indicatorLine = placementIndicator.AddComponent<LineRenderer>();
            indicatorLine.useWorldSpace = false;
            indicatorLine.loop = true;
            indicatorLine.startWidth = indicatorWidth;
            indicatorLine.endWidth = indicatorWidth;
            indicatorLine.numCapVertices = 8;
            indicatorLine.numCornerVertices = 8;

            Material mat = indicatorMaterial;
            if (mat == null)
            {
                Shader shader = Shader.Find("Unlit/Color") ??
                               Shader.Find("Universal Render Pipeline/Unlit") ??
                               Shader.Find("Sprites/Default") ??
                               Shader.Find("Mobile/Unlit (Supports Lightmap)") ??
                               Shader.Find("Standard");

                if (shader != null)
                {
                    mat = new Material(shader);
                    Color blue = new Color(0.0f, 0.72f, 1.0f, 0.95f);
                    mat.color = blue;
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", blue);
                    if (mat.HasProperty("_Color")) mat.SetColor("_Color", blue);
                }
            }

            if (mat != null)
            {
                indicatorLine.material = mat;
            }

            // Build circular ring
            int segments = 64;
            indicatorLine.positionCount = segments + 1;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * indicatorRadius;
                float z = Mathf.Sin(angle) * indicatorRadius;
                indicatorLine.SetPosition(i, new Vector3(x, 0.005f, z));
            }

            // Center crosshair / disc lying flat on XZ floor plane
            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Quad);
            disc.name = "IndicatorFlatDisc";
            disc.transform.SetParent(placementIndicator.transform, false);
            disc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            disc.transform.localPosition = new Vector3(0f, 0.004f, 0f);
            disc.transform.localScale = Vector3.one * (indicatorRadius * 1.5f);
            Destroy(disc.GetComponent<Collider>());
            if (mat != null)
            {
                var r = disc.GetComponent<Renderer>();
                if (r != null) r.material = mat;
            }

            placementIndicator.SetActive(false);
        }

        private void UpdateIndicator()
        {
            if (placementIndicator == null) return;
            if (!hasValidPose)
            {
                placementIndicator.SetActive(false);
                return;
            }

            placementIndicator.SetActive(true);
            placementIndicator.transform.position = currentPose.position;
            // Floor planes are horizontal: align indicator upright with the plane normal
            placementIndicator.transform.rotation = currentPose.rotation;
        }

        private void CheckForTap()
        {
            bool tapBegan = false;
            Vector2 tapPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            // New Input System touch handling
            if (Touch.activeTouches.Count > 0)
            {
                Touch touch = Touch.activeTouches[0];
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    if (UnityEngine.EventSystems.EventSystem.current != null &&
                        UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.touchId))
                    {
                        return; // Ignore UI clicks
                    }
                    tapBegan = true;
                    tapPos = touch.screenPosition;
                }
            }
            // Mouse handling for Editor testing
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (UnityEngine.EventSystems.EventSystem.current != null &&
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    return; // Ignore UI clicks
                }
                tapBegan = true;
                tapPos = Mouse.current.position.ReadValue();
            }
            else if (Input.GetMouseButtonDown(0))
            {
                if (UnityEngine.EventSystems.EventSystem.current != null &&
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    return; // Ignore UI clicks
                }
                tapBegan = true;
                tapPos = Input.mousePosition;
            }

            if (!tapBegan) return;

            // 1. Attempt raycast at tap position against real detected horizontal planes
            if (raycastManager != null)
            {
                RaycastHits.Clear();
                bool hit = raycastManager.Raycast(tapPos, RaycastHits, TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds);
                if (hit && RaycastHits.Count > 0)
                {
                    var hitResult = RaycastHits[0];
                    ARPlane hitPlane = planeManager != null ? planeManager.GetPlane(hitResult.trackableId) : null;
                    PlaceMachinery(hitResult.pose, hitPlane);
                    return;
                }
            }

            // 2. If tap did not directly hit plane polygon, but placement indicator currently has a valid floor pose:
            if (hasValidPose)
            {
                PlaceMachinery(currentPose, currentPlane);
                return;
            }

            // MANDATORY RULE 4 & 6:
            // DO NOT fall back to camera-forward or fixed camera distance!
            // Placement MUST ONLY occur on a real detected floor plane.
            Debug.Log("[MachineryRealARController] Tap ignored: Please point camera at detected floor to place.");
        }

        private void PlaceMachinery(Pose hitPose, ARPlane hitPlane)
        {
            if (machineryPlacementRoot == null) return;

            // Ensure camera tracking is active
            EnsureCameraTracking();

            // Center MachineryScenario relative to placement root
            // In local space, MachineryScenario's components span:
            // X from -1.85 (FeedHopper) to +3.90 (MainIsolator/Table) -> Center X = +1.025m
            // Z from -1.35 (SteelWorkstationTable) to +1.56 (FeedHopper) -> Center Z = +0.105m
            // Y base is at 0.0m (bottom of conveyor legs and table legs)
            // By offsetting MachineryScenario by (-1.025, 0, -0.105), the center of the machine
            // sits directly on the placement indicator, and its feet rest on the floor!
            Transform scenarioChild = machineryPlacementRoot.transform.Find("MachineryScenario");
            if (scenarioChild != null)
            {
                scenarioChild.localPosition = new Vector3(-1.025f, 0f, -0.105f);
                scenarioChild.localRotation = Quaternion.identity;
                scenarioChild.localScale = Vector3.one; // child scale untouched
            }

            // Orientation: strictly from the real AR raycast hit pose on the detected horizontal floor plane
            Quaternion initialRotation = hitPose.rotation;

            // Apply world-space placement pose ONLY to the root
            machineryPlacementRoot.transform.position = hitPose.position;
            machineryPlacementRoot.transform.rotation = initialRotation;
            machineryPlacementRoot.transform.localScale = Vector3.one * placementScale;

            // Attach ARAnchor to firmly pin root in real world space
            if (anchorManager != null && hitPlane != null)
            {
                var anchor = anchorManager.AttachAnchor(hitPlane, hitPose);
                if (anchor != null)
                {
                    machineryPlacementRoot.transform.SetParent(anchor.transform, true);
                }
            }
            else
            {
                var anchor = machineryPlacementRoot.GetComponent<ARAnchor>();
                if (anchor == null)
                {
                    machineryPlacementRoot.AddComponent<ARAnchor>();
                }
            }

            // Activate complete machinery assembly
            machineryPlacementRoot.SetActive(true);

            // Ensure all child renderers are active and visible
            Renderer[] renderers = machineryPlacementRoot.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r != null)
                {
                    r.enabled = true;
                    if (!r.gameObject.activeSelf) r.gameObject.SetActive(true);
                }
            }

            isPlaced = true;

            // Hide placement indicator
            if (placementIndicator != null) placementIndicator.SetActive(false);

            // Hide plane visualizations & stop scanning to optimize runtime
            if (planeManager != null)
            {
                foreach (ARPlane plane in planeManager.trackables)
                {
                    if (plane != null) plane.gameObject.SetActive(false);
                }
                planeManager.enabled = false;
            }

            // Update UI
            if (statusText != null)
            {
                statusText.text = "Machinery Placed\nWalk around the machinery and inspect the equipment.";
            }

            if (resetButton != null)
            {
                resetButton.gameObject.SetActive(true);
            }

            Debug.Log($"[MachineryRealARController] COMPLETE Machinery anchored in world space at {hitPose.position} with scale {placementScale:F2} and {renderers.Length} active renderers.");
        }

        public void ResetPlacement()
        {
            if (machineryPlacementRoot != null)
            {
                var anchor = machineryPlacementRoot.GetComponent<ARAnchor>();
                if (anchor != null) Destroy(anchor);
                machineryPlacementRoot.transform.SetParent(null, true);
                machineryPlacementRoot.SetActive(false);
            }

            isPlaced = false;
            hasValidPose = false;

            if (planeManager != null)
            {
                planeManager.enabled = true;
                foreach (ARPlane plane in planeManager.trackables)
                {
                    if (plane != null) plane.gameObject.SetActive(true);
                }
            }

            if (statusText != null)
            {
                statusText.text = "Scan the floor to place the machinery";
            }

            if (resetButton != null)
            {
                resetButton.gameObject.SetActive(false);
            }

            Debug.Log("[MachineryRealARController] AR Placement Reset.");
        }

        private void OnDestroy()
        {
            if (placementIndicator != null) Destroy(placementIndicator);
        }
    }
}
