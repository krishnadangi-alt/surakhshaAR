#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryStep1ConveyorFixer
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string LogPath = "scratch/step1_conveyor_fix_log.txt";

        [MenuItem("SurakshaAR/Machinery/Step 1 — Restore Conveyor Assembly Only")]
        public static void RestoreConveyorOnly()
        {
            Directory.CreateDirectory("scratch");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("SurakshaAR — STEP 1: CONVEYOR ASSEMBLY VISIBILITY FIX");
            sb.AppendLine("Scene: " + ScenePath);
            sb.AppendLine("Timestamp: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("=========================================================================\n");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                sb.AppendLine("ERROR: Failed to open scene at " + ScenePath);
                File.WriteAllText(LogPath, sb.ToString());
                return;
            }

            GameObject masterRoot = GameObject.Find("MachineryScenario");
            if (masterRoot == null)
            {
                masterRoot = new GameObject("MachineryScenario");
            }
            masterRoot.SetActive(true);
            masterRoot.transform.position = Vector3.zero;
            masterRoot.transform.rotation = Quaternion.identity;
            masterRoot.transform.localScale = Vector3.one;

            // Remove any existing broken/old ConveyorAssembly nodes
            List<GameObject> toDestroy = new List<GameObject>();
            foreach (Transform child in masterRoot.transform)
            {
                if (child.name.Equals("ConveyorAssembly", System.StringComparison.OrdinalIgnoreCase))
                {
                    toDestroy.Add(child.gameObject);
                }
            }
            foreach (var oldGo in toDestroy)
            {
                string oldName = oldGo.name;
                Object.DestroyImmediate(oldGo);
                sb.AppendLine($"[Cleanup] Destroyed old Conveyor node: {oldName}");
            }

            // Load and instantiate raw GLB fresh
            string glbPath = "Assets/AR_machinary_faundation/3d/Raw/industrial_conveyor_belt.glb";
            GameObject conveyorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
            if (conveyorPrefab == null)
            {
                sb.AppendLine("CRITICAL ERROR: industrial_conveyor_belt.glb missing at " + glbPath);
                File.WriteAllText(LogPath, sb.ToString());
                return;
            }

            GameObject conveyorInst = (GameObject)PrefabUtility.InstantiatePrefab(conveyorPrefab, masterRoot.transform);
            conveyorInst.name = "ConveyorAssembly";
            PrefabUtility.UnpackPrefabInstance(conveyorInst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            sb.AppendLine("[Instantiated & Unpacked] ConveyorAssembly from " + glbPath);

            conveyorInst.SetActive(true);
            conveyorInst.transform.localPosition = Vector3.zero;
            conveyorInst.transform.localRotation = Quaternion.Euler(-90f, 0f, 90f);
            conveyorInst.transform.localScale = new Vector3(0.0125f, 0.0125f, 0.0125f);

            // Enable all physical conveyor renderers (hiding only room floor planes)
            MeshRenderer[] renderers = conveyorInst.GetComponentsInChildren<MeshRenderer>(true);
            sb.AppendLine($"Total MeshRenderers under ConveyorAssembly: {renderers.Length}");

            int enabledCount = 0;
            Shader standardShader = Shader.Find("Standard");
            if (standardShader == null) standardShader = Shader.Find("Mobile/Diffuse");

            foreach (var r in renderers)
            {
                r.gameObject.SetActive(true);

                string path = r.transform.name.ToLower();
                Transform p = r.transform.parent;
                while (p != null && p != conveyorInst.transform)
                {
                    path = p.name.ToLower() + "/" + path;
                    p = p.parent;
                }

                if (path.Contains("floor") || path.Contains("vinyl"))
                {
                    r.enabled = false;
                    sb.AppendLine($"  Disabled Background Floor Submesh: '{path}'");
                    continue;
                }

                r.enabled = true;
                enabledCount++;

                // Ensure materials use valid shader
                Material[] mats = r.sharedMaterials;
                for (int m = 0; m < mats.Length; m++)
                {
                    if (mats[m] == null || mats[m].shader == null || mats[m].shader.name.Contains("Error"))
                    {
                        Material validMat = new Material(standardShader);
                        validMat.name = "Conveyor_Mat_" + m;
                        validMat.color = new Color(0.25f, 0.40f, 0.65f, 1.0f);
                        mats[m] = validMat;
                    }
                }
                r.sharedMaterials = mats;

                sb.AppendLine($"  Enabled Submesh: '{r.name}' | Path='{path}' | BoundsCenter={r.bounds.center} | BoundsSize={r.bounds.size}");
            }

            sb.AppendLine($"\nTotal Active Conveyor Renderers Enabled: {enabledCount} / {renderers.Length}");

            // Calculate physical conveyor bounds at origin
            Bounds convBounds = new Bounds();
            bool hasFirst = false;
            foreach (var r in renderers)
            {
                if (r.enabled && r.gameObject.activeInHierarchy)
                {
                    if (!hasFirst) { convBounds = r.bounds; hasFirst = true; }
                    else { convBounds.Encapsulate(r.bounds); }
                }
            }

            // Offset conveyorInst so ground base is at Y=0 and center X=0, Z=0
            Vector3 offset = new Vector3(-convBounds.center.x, -convBounds.min.y, -convBounds.center.z);
            conveyorInst.transform.position += offset;

            // Recalculate final world bounds
            hasFirst = false;
            foreach (var r in renderers)
            {
                if (r.enabled && r.gameObject.activeInHierarchy)
                {
                    if (!hasFirst) { convBounds = r.bounds; hasFirst = true; }
                    else { convBounds.Encapsulate(r.bounds); }
                }
            }

            sb.AppendLine($"\n[CONVEYOR FINAL PHYSICAL BOUNDS]");
            sb.AppendLine($"  World Bounds Center: {convBounds.center}");
            sb.AppendLine($"  World Bounds Size: {convBounds.size}");
            sb.AppendLine($"  World Bounds Min: {convBounds.min}");
            sb.AppendLine($"  World Bounds Max: {convBounds.max}");

            // Save Scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[Step1ConveyorFixer] Conveyor restored and saved to scene. Report written to " + LogPath);

            // Render views
            MachinerySceneCameraRenderer.RenderAllViews();
        }
    }
}
#endif
