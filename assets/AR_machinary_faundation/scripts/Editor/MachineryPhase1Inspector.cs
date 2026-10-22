#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Text;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryPhase1Inspector
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string Raw3DFolder = "Assets/AR_machinary_faundation/3d/Raw";
        private const string OutputFolder = "Builds/Verification/UnityScene";

        [MenuItem("SurakshaAR/Machinery/Phase 1 - Build Model Inspection Grid")]
        public static void BuildPhase1InspectionGrid()
        {
            Debug.Log("[Phase 1] Setting up 12-model raw inspection grid in Unity scene...");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[Phase 1] Cannot open scene at {ScenePath}");
                return;
            }

            // Create/Clean parent container MachineryModelInspection
            GameObject root = GameObject.Find("MachineryModelInspection");
            if (root != null)
            {
                Object.DestroyImmediate(root);
            }
            root = new GameObject("MachineryModelInspection");
            root.SetActive(true);

            // Hide old scenario object if present
            GameObject oldScenario = GameObject.Find("MachineryScenario");
            if (oldScenario != null) oldScenario.SetActive(false);

            // Definitions for 12 GLB assets
            var modelSpecs = new (string nodeName, string glbFile, Vector3 gridPos, Vector3 targetScale, Vector3 rotEuler)[]
            {
                // ROW 1 (Z = +3.5m)
                ("Model_01_industrial_conveyor_belt", "industrial_conveyor_belt", new Vector3(-4.5f, 0f, 3.5f), new Vector3(0.0035f, 0.0035f, 0.0035f), new Vector3(0f, 30f, 0f)),
                ("Model_02_conveyor_belt", "conveyor_belt", new Vector3(-1.5f, 0f, 3.5f), new Vector3(0.0045f, 0.0045f, 0.0045f), new Vector3(0f, 30f, 0f)),
                ("Model_03_bulk_material_handling_conveyors_frame", "bulk_material_handling_conveyors_frame", new Vector3(1.5f, 0f, 3.5f), new Vector3(0.015f, 0.015f, 0.015f), new Vector3(0f, 30f, 0f)),
                ("Model_12_hopper", "hopper", new Vector3(4.5f, 0f, 3.5f), new Vector3(0.00035f, 0.00035f, 0.00035f), new Vector3(0f, 30f, 0f)),

                // ROW 2 (Z = 0.0m)
                ("Model_04_coal", "coal", new Vector3(-4.5f, 0f, 0f), new Vector3(0.04f, 0.04f, 0.04f), new Vector3(15f, 45f, 0f)),
                ("Model_05_electric_motor", "electric_motor", new Vector3(-1.5f, 0f, 0f), new Vector3(0.65f, 0.65f, 0.65f), new Vector3(0f, 30f, 0f)),
                ("Model_10_industrial_control_panel_box", "industrial_control_panel_box", new Vector3(1.5f, 0f, 0f), new Vector3(0.0008f, 0.0008f, 0.0008f), new Vector3(0f, 180f, 0f)),
                ("Model_11_industrial_steel_table", "industrial_steel_table", new Vector3(4.5f, 0f, 0f), new Vector3(0.006f, 0.006f, 0.006f), new Vector3(0f, 30f, 0f)),

                // ROW 3 (Z = -3.5m)
                ("Model_06_emergency_stop_button", "emergency_stop_button", new Vector3(-4.5f, 0f, -3.5f), new Vector3(0.005f, 0.005f, 0.005f), new Vector3(0f, 0f, 0f)),
                ("Model_07_switch", "switch", new Vector3(-1.5f, 0f, -3.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0f, 180f, 0f)),
                ("Model_08_lockout_tagout_locks_with_key_and_tag", "lockout_tagout_locks_with_key_and_tag", new Vector3(1.5f, 0f, -3.5f), new Vector3(5.0f, 5.0f, 5.0f), new Vector3(0f, 30f, 0f)),
                ("Model_09_hasp_and_staple", "hasp_and_staple", new Vector3(4.5f, 0f, -3.5f), new Vector3(0.005f, 0.005f, 0.005f), new Vector3(90f, 0f, 0f))
            };

            foreach (var spec in modelSpecs)
            {
                string glbPath = $"{Raw3DFolder}/{spec.glbFile}.glb";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
                if (prefab == null)
                {
                    Debug.LogError($"[Phase 1] Cannot load asset at: {glbPath}");
                    continue;
                }

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
                instance.name = spec.nodeName;
                instance.transform.position = spec.gridPos;
                instance.transform.eulerAngles = spec.rotEuler;
                instance.transform.localScale = spec.targetScale;

                // Clean up white wall & environment room planes in instance
                CleanInstanceRenderers(instance, spec.glbFile);

                // Ground model on Y = 0.0
                GroundModel(instance, 0.0f);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Phase 1] Inspection Grid built, cleaned, grounded, and saved successfully!");

            // Render inspection views
            RenderPhase1PNGs();
        }

        private static void CleanInstanceRenderers(GameObject instance, string glbFile)
        {
            MeshRenderer[] renderers = instance.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var r in renderers)
            {
                MeshFilter mf = r.GetComponent<MeshFilter>();
                Vector3 size = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.bounds.size : Vector3.zero;
                string matName = (r.sharedMaterial != null) ? r.sharedMaterial.name : "";

                // Disable giant room/wall/floor planes
                if (size.x > 150f || size.y > 150f || size.z > 150f || 
                    r.name.Contains("Floor") || r.name.Contains("Plane") || 
                    matName.Contains("material_7") || matName.Contains("material_6") || matName.Contains("material_8"))
                {
                    r.enabled = false;
                }
                else
                {
                    r.enabled = true;
                }
            }
        }

        private static void GroundModel(GameObject go, float groundY)
        {
            MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>(true);
            if (renderers.Length == 0) return;

            Bounds b = new Bounds();
            bool first = false;
            foreach (var r in renderers)
            {
                if (!r.enabled) continue;
                if (!first) { b = r.bounds; first = true; }
                else b.Encapsulate(r.bounds);
            }

            if (first)
            {
                float yOffset = groundY - b.min.y;
                go.transform.position += new Vector3(0f, yOffset, 0f);
            }
        }

        private static void RenderPhase1PNGs()
        {
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            GameObject tempCamGo = new GameObject("TempPhase1Cam");
            Camera tempCam = tempCamGo.AddComponent<Camera>();
            tempCam.clearFlags = CameraClearFlags.SolidColor;
            tempCam.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1f); // Dark blueprint background
            tempCam.nearClipPlane = 0.1f;
            tempCam.farClipPlane = 200f;

            int width = 1280;
            int height = 720;
            RenderTexture rt = new RenderTexture(width, height, 24);
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            void RenderView(Vector3 eyePos, Vector3 targetLook, string filename)
            {
                tempCam.transform.position = eyePos;
                tempCam.transform.LookAt(targetLook);
                tempCam.targetTexture = rt;
                tempCam.Render();

                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
                RenderTexture.active = null;

                byte[] bytes = tex.EncodeToPNG();
                string path = Path.Combine(OutputFolder, filename);
                File.WriteAllBytes(path, bytes);
                Debug.Log($"[Phase 1 Render] Saved {filename} ({bytes.Length} bytes)");
            }

            // Render main inspection overview (All 12 models in grid)
            RenderView(new Vector3(0f, 7.5f, -9.5f), new Vector3(0f, 0.8f, 0.2f), "REAL_3D_MODELS_INSPECTION.png");

            // Close-up view 1 (Row 1 & 2)
            RenderView(new Vector3(0f, 5.0f, -4.5f), new Vector3(0f, 0.8f, 1.5f), "Inspection_View_01.png");

            // Close-up view 2 (Row 2 & 3)
            RenderView(new Vector3(0f, 5.0f, -8.0f), new Vector3(0f, 0.5f, -2.0f), "Inspection_View_02.png");

            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(tempCamGo);

            Debug.Log("[Phase 1] ALL PHASE 1 INSPECTION PNG RENDERS GENERATED SUCCESSFULLY!");
        }
    }
}
#endif
