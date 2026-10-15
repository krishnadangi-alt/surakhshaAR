#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryFullHierarchyInspector
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string OutputLogPath = "scratch/full_hierarchy_inspection.txt";

        [MenuItem("SurakshaAR/Machinery/Run Full Hierarchy Inspection")]
        public static void RunInspection()
        {
            Directory.CreateDirectory("scratch");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("SurakshaAR — FULL MACHINERY HIERARCHY & RENDERER INSPECTION");
            sb.AppendLine("Timestamp: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("=========================================================================\n");

            string[] rawGlbs = new string[]
            {
                "Assets/AR_machinary_faundation/3d/Raw/industrial_conveyor_belt.glb",
                "Assets/AR_machinary_faundation/3d/Raw/hopper.glb",
                "Assets/AR_machinary_faundation/3d/Raw/electric_motor.glb",
                "Assets/AR_machinary_faundation/3d/Raw/coal.glb",
                "Assets/AR_machinary_faundation/3d/Raw/emergency_stop_button.glb",
                "Assets/AR_machinary_faundation/3d/Raw/switch.glb",
                "Assets/AR_machinary_faundation/3d/Raw/industrial_control_panel_box.glb",
                "Assets/AR_machinary_faundation/3d/Raw/industrial_steel_table.glb",
                "Assets/AR_machinary_faundation/3d/Raw/hasp_and_staple.glb",
                "Assets/AR_machinary_faundation/3d/Raw/lockout_tagout_locks_with_key_and_tag.glb"
            };

            sb.AppendLine("-------------------------------------------------------------------------");
            sb.AppendLine("1. RAW GLB PREFAB INSPECTION");
            sb.AppendLine("-------------------------------------------------------------------------");

            foreach (string glbPath in rawGlbs)
            {
                sb.AppendLine($"\n[GLB]: {Path.GetFileName(glbPath)} ({glbPath})");
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
                if (prefab == null)
                {
                    sb.AppendLine("  -> ERROR: Asset not found!");
                    continue;
                }

                GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                MeshRenderer[] renderers = inst.GetComponentsInChildren<MeshRenderer>(true);
                sb.AppendLine($"  Total MeshRenderers = {renderers.Length}");

                foreach (var r in renderers)
                {
                    MeshFilter mf = r.GetComponent<MeshFilter>();
                    string meshName = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.name : "NULL";
                    Vector3 size = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.bounds.size : Vector3.zero;
                    string matNames = "";
                    if (r.sharedMaterials != null)
                    {
                        foreach (var m in r.sharedMaterials)
                        {
                            if (m != null) matNames += m.name + "; ";
                            else matNames += "NULL; ";
                        }
                    }

                    string pathInInst = GetHierarchyPath(r.transform, inst.transform);
                    sb.AppendLine($"  Renderer: '{pathInInst}' | Active={r.gameObject.activeSelf} | Enabled={r.enabled} | Mesh='{meshName}' | MeshSize=({size.x:F3},{size.y:F3},{size.z:F3}) | Materials='{matNames}'");
                }

                Object.DestroyImmediate(inst);
            }

            sb.AppendLine("\n-------------------------------------------------------------------------");
            sb.AppendLine("2. CURRENT SCENE HIERARCHY INSPECTION (" + ScenePath + ")");
            sb.AppendLine("-------------------------------------------------------------------------");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                sb.AppendLine("ERROR: Scene failed to open!");
                File.WriteAllText(OutputLogPath, sb.ToString());
                return;
            }

            GameObject masterRoot = GameObject.Find("MachineryScenario");
            if (masterRoot == null)
            {
                sb.AppendLine("ERROR: MachineryScenario root not found in scene!");
            }
            else
            {
                InspectTransformRecursive(masterRoot.transform, 0, sb);
            }

            File.WriteAllText(OutputLogPath, sb.ToString());
            Debug.Log("[FullHierarchyInspector] Report written to " + OutputLogPath);
        }

        private static void InspectTransformRecursive(Transform tr, int indent, StringBuilder sb)
        {
            string prefix = new string(' ', indent * 2);
            MeshRenderer r = tr.GetComponent<MeshRenderer>();
            MeshFilter mf = tr.GetComponent<MeshFilter>();
            string rInfo = "";
            if (r != null)
            {
                string meshName = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.name : "NULL";
                Vector3 size = r.bounds.size;
                string matNames = "";
                if (r.sharedMaterials != null)
                {
                    foreach (var m in r.sharedMaterials)
                    {
                        if (m != null) matNames += m.name + "; ";
                        else matNames += "NULL; ";
                    }
                }
                rInfo = $" [Renderer: Active={tr.gameObject.activeSelf}, Enabled={r.enabled}, Mesh='{meshName}', BoundsSize=({size.x:F2},{size.y:F2},{size.z:F2}), Mats='{matNames}']";
            }

            sb.AppendLine($"{prefix}- '{tr.name}' Pos=({tr.position.x:F2},{tr.position.y:F2},{tr.position.z:F2}) Rot=({tr.eulerAngles.x:F0},{tr.eulerAngles.y:F0},{tr.eulerAngles.z:F0}) Scale=({tr.localScale.x:F2},{tr.localScale.y:F2},{tr.localScale.z:F2}){rInfo}");

            foreach (Transform child in tr)
            {
                InspectTransformRecursive(child, indent + 1, sb);
            }
        }

        private static string GetHierarchyPath(Transform target, Transform root)
        {
            string path = target.name;
            Transform curr = target.parent;
            while (curr != null && curr != root)
            {
                path = curr.name + "/" + path;
                curr = curr.parent;
            }
            return path;
        }
    }
}
#endif
