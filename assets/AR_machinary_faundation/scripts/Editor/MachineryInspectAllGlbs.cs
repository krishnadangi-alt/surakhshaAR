#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryInspectAllGlbs
    {
        [MenuItem("SurakshaAR/Machinery/Inspect All GLBs")]
        public static void InspectAll()
        {
            Directory.CreateDirectory("scratch");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("GLB MODEL RENDERER INSPECTION");
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

            foreach (string glbPath in rawGlbs)
            {
                string glbName = Path.GetFileName(glbPath);
                sb.AppendLine($"\n=================================================");
                sb.AppendLine($"MODEL: {glbName}");
                sb.AppendLine($"=================================================");

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
                if (prefab == null)
                {
                    sb.AppendLine("ERROR: Asset not found!");
                    continue;
                }

                GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                MeshRenderer[] renderers = inst.GetComponentsInChildren<MeshRenderer>(true);
                sb.AppendLine($"Total Renderers: {renderers.Length}");

                foreach (var r in renderers)
                {
                    MeshFilter mf = r.GetComponent<MeshFilter>();
                    string meshName = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.name : "NULL";
                    Vector3 size = r.bounds.size;
                    string fullPath = GetHierarchyPath(r.transform, inst.transform);
                    sb.AppendLine($"  Node: '{fullPath}' | Mat: '{(r.sharedMaterial != null ? r.sharedMaterial.name : "NULL")}' | MeshSize: ({size.x:F3}, {size.y:F3}, {size.z:F3})");
                }

                Object.DestroyImmediate(inst);
            }

            File.WriteAllText("scratch/glb_renderers_summary.txt", sb.ToString());
            Debug.Log("[InspectAllGLBs] Summary written to scratch/glb_renderers_summary.txt");
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
