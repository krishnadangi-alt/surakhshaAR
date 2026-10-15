#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryUnpackStep1Conveyor
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string ConveyorGlbPath = "Assets/AR_machinary_faundation/3d/Raw/industrial_conveyor_belt.glb";
        private const string LogPath = "scratch/step1_unpack_log.txt";

        [MenuItem("SurakshaAR/Machinery/Step 1 — Unpack And Restore Conveyor Only")]
        public static void UnpackAndRestoreConveyor()
        {
            Directory.CreateDirectory("scratch");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("SurakshaAR — STEP 1: UNPACK & RESTORE CONVEYOR ASSEMBLY ONLY");
            sb.AppendLine("Timestamp: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("=========================================================================\n");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                sb.AppendLine("CRITICAL ERROR: Failed to open scene at " + ScenePath);
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

            // Remove old broken ConveyorAssembly nodes if any
            List<GameObject> toDestroy = new List<GameObject>();
            foreach (Transform child in masterRoot.transform)
            {
                if (child.name == "ConveyorAssembly" || child.name.ToLower().Contains("conveyor"))
                {
                    toDestroy.Add(child.gameObject);
                }
            }
            foreach (var go in toDestroy)
            {
                Object.DestroyImmediate(go);
            }

            // Load raw GLB prefab
            GameObject conveyorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ConveyorGlbPath);
            if (conveyorPrefab == null)
            {
                sb.AppendLine("CRITICAL ERROR: Raw conveyor GLB not found at " + ConveyorGlbPath);
                File.WriteAllText(LogPath, sb.ToString());
                return;
            }

            // Instantiate prefab
            GameObject conveyorInst = (GameObject)PrefabUtility.InstantiatePrefab(conveyorPrefab, masterRoot.transform);
            conveyorInst.name = "ConveyorAssembly";

            // UNPACK COMPLETELY to convert from broken PrefabInstance to clean native scene GameObjects
            PrefabUtility.UnpackPrefabInstance(conveyorInst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            conveyorInst.SetActive(true);
            conveyorInst.transform.position = Vector3.zero;
            conveyorInst.transform.rotation = Quaternion.identity;
            conveyorInst.transform.localScale = Vector3.one;

            Shader standardShader = Shader.Find("Standard");
            MeshRenderer[] renderers = conveyorInst.GetComponentsInChildren<MeshRenderer>(true);
            sb.AppendLine($"Total Sub-Mesh Renderers in ConveyorAssembly: {renderers.Length}");

            int enabledCount = 0;
            foreach (var r in renderers)
            {
                r.gameObject.SetActive(true);

                // Disable distant non-physical helper nodes if any (>20m offset)
                MeshFilter mf = r.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    if (mf.sharedMesh.bounds.center.magnitude > 20.0f || r.transform.localPosition.magnitude > 20.0f)
                    {
                        r.enabled = false;
                        sb.AppendLine($"  Disabled Helper Node: '{r.name}'");
                        continue;
                    }
                }

                r.enabled = true;
                enabledCount++;

                // Ensure materials use valid shader
                Material[] mats = r.sharedMaterials;
                for (int m = 0; m < mats.Length; m++)
                {
                    if (mats[m] == null || mats[m].shader == null || mats[m].shader.name.Contains("Error"))
                    {
                        Material newMat = new Material(standardShader);
                        newMat.name = "Conveyor_Mat_" + m;
                        newMat.color = new Color(0.20f, 0.40f, 0.65f, 1.0f);
                        mats[m] = newMat;
                    }
                }
                r.sharedMaterials = mats;

                sb.AppendLine($"  Enabled Conveyor Submesh: '{r.name}' | LocalPos={r.transform.localPosition} | WorldBoundsCenter={r.bounds.center} | Size={r.bounds.size}");
            }

            // Compute bounds of physical conveyor
            Bounds b = new Bounds();
            bool hasFirst = false;
            foreach (var r in renderers)
            {
                if (r.enabled && r.gameObject.activeInHierarchy)
                {
                    if (!hasFirst) { b = r.bounds; hasFirst = true; }
                    else { b.Encapsulate(r.bounds); }
                }
            }

            sb.AppendLine($"\n[CONVEYOR PHYSICAL BOUNDS AT ORIGIN]");
            sb.AppendLine($"  Center={b.center}");
            sb.AppendLine($"  Size={b.size}");
            sb.AppendLine($"  Min={b.min}");
            sb.AppendLine($"  Max={b.max}");

            // Mark Scene Dirty and Save
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[UnpackStep1Conveyor] Conveyor unpacked and saved to scene. Log written to " + LogPath);

            // Render camera views
            MachinerySceneCameraRenderer.RenderAllViews();
        }
    }
}
#endif
