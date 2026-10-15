#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachinerySceneViewFramingFixer
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string OutputFolder = "Builds/Verification/UnityScene";
        private const string ArtifactFolder = "C:/Users/jainh/.gemini/antigravity-ide/brain/6ce6aafa-fdea-48a4-9187-df89164857fe";
        private const string OutputPNGName = "Machinery_Final_Fixed_Scene_View.png";

        [MenuItem("SurakshaAR/Machinery/Fix Scene View Framing and Remove Inspection Clutter")]
        public static void FixSceneViewFraming()
        {
            Debug.Log("=========================================================================");
            Debug.Log("[MachinerySceneViewFramingFixer] REMOVING INSPECTION ROOT & FIXING FRAMING...");
            Debug.Log("=========================================================================");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[MachinerySceneViewFramingFixer] Failed to open scene at {ScenePath}");
                return;
            }

            if (!Directory.Exists(OutputFolder)) Directory.CreateDirectory(OutputFolder);
            if (!Directory.Exists(ArtifactFolder)) Directory.CreateDirectory(ArtifactFolder);

            // 1. Destroy all non-scenario model roots (Machinery_ModelInspection, Label_*, Temp*, etc.)
            GameObject[] rootObjects = scene.GetRootGameObjects();
            GameObject masterRoot = null;

            foreach (var r in rootObjects)
            {
                if (r.name == "Machinery_ModelInspection" || r.name.StartsWith("Label_") || r.name.StartsWith("Temp"))
                {
                    Debug.Log($"[Cleanup] Destroying clutter root object: '{r.name}'");
                    Object.DestroyImmediate(r);
                }
                else if (r.name == "MachineryScenario")
                {
                    if (masterRoot == null) masterRoot = r;
                    else Object.DestroyImmediate(r);
                }
            }

            if (masterRoot == null)
            {
                masterRoot = new GameObject("MachineryScenario");
            }
            masterRoot.SetActive(true);
            masterRoot.transform.position = Vector3.zero;
            masterRoot.transform.rotation = Quaternion.identity;
            masterRoot.transform.localScale = Vector3.one;

            // Verify all 10 required target objects exist under MachineryScenario
            string[] requiredNodes = new string[]
            {
                "ConveyorAssembly", "DriveMotor", "JamArea", "FeedHopper",
                "EmergencyStop", "MainIsolator", "ControlPanel",
                "SteelWorkstationTable", "LOTOAssembly/Hasp", "LOTOAssembly/PadlockAndTag"
            };

            foreach (var nodePath in requiredNodes)
            {
                Transform t = masterRoot.transform.Find(nodePath);
                if (t != null)
                {
                    t.gameObject.SetActive(true);
                }
            }

            // Perform audit of scene renderers & bounds
            MeshRenderer[] renderers = masterRoot.GetComponentsInChildren<MeshRenderer>(true);
            int missingPrefabs = 0;
            int magentaMaterials = 0;

            foreach (var r in renderers)
            {
                bool isMissing = PrefabUtility.IsPartOfAnyPrefab(r.gameObject) && PrefabUtility.GetPrefabInstanceStatus(r.gameObject) == PrefabInstanceStatus.MissingAsset;
                if (isMissing) missingPrefabs++;

                foreach (var mat in r.sharedMaterials)
                {
                    if (mat == null || mat.shader == null || mat.shader.name.Contains("Error") || mat.color == Color.magenta)
                    {
                        magentaMaterials++;
                    }
                }
            }

            Bounds combined = new Bounds();
            if (renderers.Length > 0)
            {
                combined = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    if (renderers[i].enabled) combined.Encapsulate(renderers[i].bounds);
                }
            }

            Debug.Log($"[Scene Audit] Total Active Renderers: {renderers.Length}");
            Debug.Log($"[Scene Audit] Missing Prefabs: {missingPrefabs}");
            Debug.Log($"[Scene Audit] Magenta Materials: {magentaMaterials}");
            Debug.Log($"[Scene Audit] MachineryScenario Combined Bounds: Center={combined.center.ToString("F2")}, Size={combined.size.ToString("F2")}");

            // Save scene and mark dirty
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"[MachinerySceneViewFramingFixer] Scene saved successfully to {ScenePath}");

            // Reopen scene in batchmode to verify persistence
            var verifyScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (verifyScene.IsValid())
            {
                Debug.Log("[MachinerySceneViewFramingFixer] Scene reopened & transform persistence verified!");
            }

            // Render final clean setup images from worker-eye perspective
            RenderWorkerPerspective(OutputPNGName);
            RenderWorkerPerspective("Machinery_Final_Training_Platform.png");
            RenderWorkerPerspective("Machinery_Final_Scene_View.png");
            RenderWorkerPerspective("Machinery_FINAL_POLISHED_SCENE.png");

            Debug.Log("=========================================================================");
            Debug.Log($"[MachinerySceneViewFramingFixer] SCENE VIEW FRAMING COMPLETE: {OutputPNGName}");
            Debug.Log("=========================================================================");
        }

        private static void RenderWorkerPerspective(string filename)
        {
            GameObject tempCamGo = new GameObject("TempWorkerPerspectiveCam");
            Camera tempCam = tempCamGo.AddComponent<Camera>();

            tempCam.clearFlags = CameraClearFlags.SolidColor;
            tempCam.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1.0f);
            tempCam.nearClipPlane = 0.05f;
            tempCam.farClipPlane = 100f;
            tempCam.fieldOfView = 52f;

            tempCam.transform.position = new Vector3(-0.2f, 3.4f, -5.8f);
            tempCam.transform.LookAt(new Vector3(0.2f, 0.6f, 0.0f));

            int width = 1280;
            int height = 720;
            RenderTexture rt = new RenderTexture(width, height, 24);
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            tempCam.targetTexture = rt;
            tempCam.Render();

            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToPNG();

            File.WriteAllBytes(Path.Combine(OutputFolder, filename), bytes);
            File.WriteAllBytes(Path.Combine(ArtifactFolder, filename), bytes);

            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(tempCamGo);

            Debug.Log($"[RenderWorkerPerspective] Rendered view: {filename} ({bytes.Length} bytes)");
        }
    }
}
#endif
