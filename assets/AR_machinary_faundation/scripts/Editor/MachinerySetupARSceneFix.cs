#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachinerySetupARSceneFix
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";

        [MenuItem("SurakshaAR/Machinery/Fix AR Scene Setup (Camera Tag & Plane Manager)")]
        public static void FixARSceneSetup()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError("[MachinerySetupARSceneFix] Failed to open scene: " + ScenePath);
                return;
            }

            // 1. Tag AR Camera as MainCamera
            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (Camera cam in cameras)
            {
                if (cam.name.Contains("AR Camera") || cam.name.Contains("Main Camera") || cam.name.Contains("Camera"))
                {
                    cam.gameObject.tag = "MainCamera";
                    Debug.Log($"[MachinerySetupARSceneFix] Tagged {cam.name} as MainCamera.");
                }
            }

            // 2. Configure ARPlaneManager
            ARPlaneManager planeManager = Object.FindFirstObjectByType<ARPlaneManager>();
            if (planeManager != null)
            {
                planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
                planeManager.enabled = true;
                EditorUtility.SetDirty(planeManager);
                Debug.Log("[MachinerySetupARSceneFix] Configured ARPlaneManager for Horizontal plane detection.");
            }

            // 3. Ensure MachineryScenario is inactive initially
            GameObject masterRoot = null;
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == "MachineryScenario")
                {
                    masterRoot = go;
                    break;
                }
            }

            if (masterRoot != null)
            {
                masterRoot.SetActive(false);
                EditorUtility.SetDirty(masterRoot);
                Debug.Log("[MachinerySetupARSceneFix] MachineryScenario root set to inactive for AR placement startup.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MachinerySetupARSceneFix] AR Scene Setup Fix completed cleanly!");
        }
    }
}
#endif
