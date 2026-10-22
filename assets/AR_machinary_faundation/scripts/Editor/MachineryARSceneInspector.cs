#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryARSceneInspector
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string LogPath = "scratch/ar_scene_inspection_log.txt";

        [MenuItem("SurakshaAR/Machinery/Inspect AR Scene Setup (Read-Only)")]
        public static void InspectARScene()
        {
            Directory.CreateDirectory("scratch");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("SurakshaAR — READ-ONLY AR SCENE SETUP INSPECTION");
            sb.AppendLine("Scene: " + ScenePath);
            sb.AppendLine("Timestamp: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("=========================================================================\n");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                sb.AppendLine("CRITICAL ERROR: Failed to open scene at " + ScenePath);
                File.WriteAllText(LogPath, sb.ToString());
                return;
            }

            // Inspect AR components
            ARSession session = Object.FindFirstObjectByType<ARSession>();
            sb.AppendLine($"ARSession: {(session != null ? "PRESENT (" + session.name + ")" : "MISSING")}");

            XROrigin xrOrigin = Object.FindFirstObjectByType<XROrigin>();
            sb.AppendLine($"XROrigin: {(xrOrigin != null ? "PRESENT (" + xrOrigin.name + ")" : "MISSING")}");

            if (xrOrigin != null)
            {
                var cameraManager = xrOrigin.GetComponentInChildren<ARCameraManager>(true);
                sb.AppendLine($"  ARCameraManager: {(cameraManager != null ? "PRESENT" : "MISSING")}");

                var cameraBackground = xrOrigin.GetComponentInChildren<ARCameraBackground>(true);
                sb.AppendLine($"  ARCameraBackground: {(cameraBackground != null ? "PRESENT" : "MISSING")}");

                var planeManager = xrOrigin.GetComponentInChildren<ARPlaneManager>(true);
                sb.AppendLine($"  ARPlaneManager: {(planeManager != null ? "PRESENT" : "MISSING")}");

                var raycastManager = xrOrigin.GetComponentInChildren<ARRaycastManager>(true);
                sb.AppendLine($"  ARRaycastManager: {(raycastManager != null ? "PRESENT" : "MISSING")}");
            }

            // Inspect MachineryScenario root
            GameObject masterRoot = GameObject.Find("MachineryScenario");
            sb.AppendLine($"MachineryScenario Root: {(masterRoot != null ? "PRESENT" : "MISSING")}");
            if (masterRoot != null)
            {
                sb.AppendLine($"  Active Self: {masterRoot.activeSelf}");
                sb.AppendLine($"  Transform Pos: {masterRoot.transform.position}");
                sb.AppendLine($"  Transform Rot: {masterRoot.transform.eulerAngles}");
                sb.AppendLine($"  Transform Scale: {masterRoot.transform.localScale}");
                sb.AppendLine($"  Child Count: {masterRoot.transform.childCount}");

                foreach (Transform child in masterRoot.transform)
                {
                    sb.AppendLine($"    - {child.name} (Pos={child.localPosition}, Rot={child.localEulerAngles}, Scale={child.localScale})");
                }
            }

            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[MachineryARSceneInspector] Read-Only Inspection Complete! Written to " + LogPath);
        }
    }
}
#endif
