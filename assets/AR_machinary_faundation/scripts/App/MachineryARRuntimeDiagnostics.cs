using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace SurakshaAR.Machinery
{
    /// <summary>
    /// MachineryARRuntimeDiagnostics
    /// =============================
    /// Provides real-time Android runtime camera & ARCore diagnostics:
    /// - Requests Android CAMERA permission on startup.
    /// - Tracks ARCameraManager frameReceived events.
    /// - Monitors ARSession tracking state and plane detection.
    /// - Displays non-blocking runtime status overlay.
    /// </summary>
    public class MachineryARRuntimeDiagnostics : MonoBehaviour
    {
        [Header("AR Component References")]
        [SerializeField] private ARCameraManager cameraManager;
        [SerializeField] private ARSession arSession;
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private Camera mainCamera;

        private bool cameraPermissionGranted = false;
        private bool frameReceived = false;
        private int frameCount = 0;

        private void Awake()
        {
            if (cameraManager == null) cameraManager = FindAnyObjectByType<ARCameraManager>();
            if (arSession == null) arSession = FindAnyObjectByType<ARSession>();
            if (planeManager == null) planeManager = FindAnyObjectByType<ARPlaneManager>();
            if (mainCamera == null) mainCamera = Camera.main;

            RequestCameraPermission();
        }

        private void OnEnable()
        {
            if (cameraManager != null)
            {
                cameraManager.frameReceived += OnCameraFrameReceived;
            }
        }

        private void OnDisable()
        {
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
                Debug.Log("[MachineryDiagnostics] Requesting Android Camera Permission...");
                Permission.RequestUserPermission(Permission.Camera);
            }
            else
            {
                cameraPermissionGranted = true;
                Debug.Log("[MachineryDiagnostics] Camera Permission already granted.");
            }
#else
            cameraPermissionGranted = true;
#endif
        }

        private void Update()
        {
#if UNITY_ANDROID
            cameraPermissionGranted = Permission.HasUserAuthorizedPermission(Permission.Camera);
#endif
        }

        private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
        {
            frameReceived = true;
            frameCount++;
        }

        private void OnGUI()
        {
            float w = Screen.width > 800 ? 420f : Screen.width * 0.85f;
            float h = 260f;
            Rect rect = new Rect(15, 15, w, h);

            GUI.Box(rect, "Machinery AR Runtime Diagnostics");

            GUILayout.BeginArea(new Rect(25, 45, w - 20, h - 45));

            string permStatus = cameraPermissionGranted ? "GRANTED" : "DENIED (Tap button below)";
            GUILayout.Label($"Camera Permission: {permStatus}");

            string sessionState = arSession != null ? ARSession.state.ToString() : "MISSING";
            GUILayout.Label($"ARSession State: {sessionState}");

            string frameStatus = frameReceived ? $"RECEIVING FRAMES ({frameCount})" : "NO FRAMES RECEIVED YET";
            GUILayout.Label($"Camera Feed: {frameStatus}");

            int planeCount = planeManager != null ? planeManager.trackables.count : 0;
            GUILayout.Label($"Planes Detected: {planeCount}");

            string camEnabled = (mainCamera != null && mainCamera.enabled) ? "ENABLED" : "DISABLED";
            GUILayout.Label($"Main Camera: {camEnabled} ({Screen.width}x{Screen.height})");

            GUILayout.Label($"Graphics API: {SystemInfo.graphicsDeviceType}");

            if (!cameraPermissionGranted)
            {
                if (GUILayout.Button("GRANT CAMERA PERMISSION", GUILayout.Height(35)))
                {
                    RequestCameraPermission();
                }
            }

            GUILayout.EndArea();
        }
    }
}
