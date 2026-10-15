using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.EnhancedTouch;
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
    /// MachineryIsolatedTestController
    /// ==============================
    /// Dedicated controller for the isolated Android AR test build:
    /// - Manages Android camera permission.
    /// - Tracks real-time AR camera frame reception.
    /// - Detects horizontal floor planes.
    /// - Renders blue placement indicator on detected floor.
    /// - Tap-to-place: Positions MachineryARPlacementRoot at tapped pose and activates it.
    /// - Machinery content stays 100% hidden until placement occurs.
    /// - Simple Reset button to clear anchor and re-scan.
    /// </summary>
    public class MachineryIsolatedTestController : MonoBehaviour
    {
        [Header("Target Root")]
        [Tooltip("Root GameObject containing complete machinery setup.")]
        [SerializeField] private GameObject machineryPlacementRoot;

        [Header("AR Component References")]
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private ARSession arSession;
        [SerializeField] private ARCameraManager cameraManager;
        [SerializeField] private Camera mainCamera;

        [Header("Simple UI References")]
        [SerializeField] private Text statusText;
        [SerializeField] private Button resetButton;
        [SerializeField] private GameObject diagnosticPanel;
        [SerializeField] private Text diagPermissionText;
        [SerializeField] private Text diagSessionText;
        [SerializeField] private Text diagFrameText;
        [SerializeField] private Text diagPlaneText;
        [SerializeField] private Text diagStatusText;

        [Header("Placement Indicator Settings")]
        [SerializeField] private float indicatorRadius = 0.15f;
        [SerializeField] private float indicatorWidth = 0.015f;
        [SerializeField] private Color indicatorColor = new Color(0.0f, 0.7f, 1.0f, 1.0f); // Electric Blue

        private GameObject placementIndicator;
        private LineRenderer indicatorLine;
        private Pose currentPose;
        private bool hasValidPose;
        private bool isPlaced;
        private bool cameraPermissionGranted;
        private bool frameReceived;
        private int frameCount;

        private static readonly List<ARRaycastHit> RaycastHits = new List<ARRaycastHit>();

        private void Awake()
        {
            if (raycastManager == null) raycastManager = GetComponent<ARRaycastManager>();
            if (planeManager == null) planeManager = GetComponent<ARPlaneManager>();
            if (arSession == null) arSession = FindAnyObjectByType<ARSession>();
            if (cameraManager == null) cameraManager = FindAnyObjectByType<ARCameraManager>();
            if (mainCamera == null) mainCamera = Camera.main;

            // MANDATORY REQUIREMENT: Machinery content MUST BE HIDDEN on app startup
            if (machineryPlacementRoot != null)
            {
                machineryPlacementRoot.SetActive(false);
            }

            if (resetButton != null)
            {
                resetButton.onClick.AddListener(ResetPlacement);
            }

            CreatePlacementIndicator();
            RequestCameraPermission();
        }

        private void Start()
        {
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
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
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

        public void RequestCameraPermission()
        {
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
            }
            else
            {
                cameraPermissionGranted = true;
            }
#else
            cameraPermissionGranted = true;
#endif
        }

        private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
        {
            frameReceived = true;
            frameCount++;
        }

        private void Update()
        {
#if UNITY_ANDROID
            cameraPermissionGranted = Permission.HasUserAuthorizedPermission(Permission.Camera);
#endif

            UpdateDiagnosticsUI();

            if (isPlaced) return;

            UpdatePlacementPosition();
            CheckForTap();
        }

        private void UpdatePlacementPosition()
        {
            hasValidPose = false;

            if (raycastManager == null) return;

            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            RaycastHits.Clear();

            bool hit = raycastManager.Raycast(screenCenter, RaycastHits, TrackableType.PlaneWithinPolygon);
            if (!hit || RaycastHits.Count == 0)
            {
                if (placementIndicator != null) placementIndicator.SetActive(false);
                if (statusText != null) statusText.text = "MACHINERY AR\nMove camera to scan floor plane...";
                return;
            }

            currentPose = RaycastHits[0].pose;
            hasValidPose = true;

            if (statusText != null) statusText.text = "FLOOR DETECTED!\nTap the blue circle to place Machinery.";

            UpdateIndicator();
        }

        private void CreatePlacementIndicator()
        {
            placementIndicator = new GameObject("MachineryTestPlacementIndicator");
            placementIndicator.transform.SetParent(transform, false);

            indicatorLine = placementIndicator.AddComponent<LineRenderer>();
            indicatorLine.useWorldSpace = false;
            indicatorLine.loop = true;
            indicatorLine.startWidth = indicatorWidth;
            indicatorLine.endWidth = indicatorWidth;
            indicatorLine.numCapVertices = 4;
            indicatorLine.numCornerVertices = 4;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                Material mat = new Material(shader);
                mat.color = indicatorColor;
                indicatorLine.material = mat;
            }

            int segments = 48;
            indicatorLine.positionCount = segments + 1;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * indicatorRadius;
                float z = Mathf.Sin(angle) * indicatorRadius;
                indicatorLine.SetPosition(i, new Vector3(x, 0.002f, z));
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
            placementIndicator.transform.rotation = currentPose.rotation;
        }

        private void CheckForTap()
        {
            if (Touch.activeTouches.Count == 0) return;

            Touch touch = Touch.activeTouches[0];
            if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began) return;

            if (raycastManager == null) return;

            Vector2 touchPosition = touch.screenPosition;
            RaycastHits.Clear();

            bool hit = raycastManager.Raycast(touchPosition, RaycastHits, TrackableType.PlaneWithinPolygon);
            if (!hit || RaycastHits.Count == 0) return;

            Pose hitPose = RaycastHits[0].pose;
            PlaceMachineryRoot(hitPose);
        }

        private void PlaceMachineryRoot(Pose hitPose)
        {
            if (machineryPlacementRoot == null) return;

            machineryPlacementRoot.transform.position = hitPose.position;
            machineryPlacementRoot.transform.rotation = Quaternion.Euler(0f, hitPose.rotation.eulerAngles.y, 0f);
            machineryPlacementRoot.SetActive(true);

            var anchor = machineryPlacementRoot.GetComponent<ARAnchor>();
            if (anchor == null)
            {
                anchor = machineryPlacementRoot.AddComponent<ARAnchor>();
            }

            isPlaced = true;

            if (placementIndicator != null) placementIndicator.SetActive(false);
            if (statusText != null) statusText.text = "MACHINERY PLACED IN AR!\nAll models anchored to surface.";

            // Hide planes after placement
            if (planeManager != null)
            {
                foreach (ARPlane plane in planeManager.trackables)
                {
                    if (plane != null) plane.gameObject.SetActive(false);
                }
                planeManager.enabled = false;
            }
        }

        public void ResetPlacement()
        {
            if (machineryPlacementRoot != null)
            {
                var anchor = machineryPlacementRoot.GetComponent<ARAnchor>();
                if (anchor != null) Destroy(anchor);
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

            if (statusText != null) statusText.text = "PLACEMENT RESET\nScan floor to detect new surface.";
        }

        private void UpdateDiagnosticsUI()
        {
            string diagString = $"Cam Perm: {(cameraPermissionGranted ? "GRANTED" : "DENIED")}\n" +
                                $"ARSession: {(arSession != null ? ARSession.state.ToString() : "MISSING")}\n" +
                                $"Camera Frames: {(frameReceived ? $"RECEIVING ({frameCount})" : "WAITING...")}\n" +
                                $"Planes Detected: {(planeManager != null ? planeManager.trackables.count : 0)}\n" +
                                $"Placement Status: {(isPlaced ? "PLACED" : (hasValidPose ? "READY" : "SCANNING"))}";

            if (diagStatusText != null)
            {
                diagStatusText.text = diagString;
            }
            else if (diagPermissionText != null)
            {
                diagPermissionText.text = diagString;
            }
        }

        private void OnDestroy()
        {
            if (placementIndicator != null) Destroy(placementIndicator);
        }
    }
}
