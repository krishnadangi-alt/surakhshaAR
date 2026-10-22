#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryPrefabInspector
    {
        private const string PrefabDir = "Assets/AR_machinary_faundation/prefabs";
        private const string LogPath = "scratch/prefab_inspection_log.txt";

        [MenuItem("SurakshaAR/Machinery/Inspect Project Prefabs")]
        public static void InspectPrefabs()
        {
            Directory.CreateDirectory("scratch");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("SurakshaAR — PROJECT PREFAB INSPECTION");
            sb.AppendLine("Timestamp: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("=========================================================================\n");

            string[] prefabFiles = Directory.GetFiles(PrefabDir, "*.prefab");
            foreach (var file in prefabFiles)
            {
                string assetPath = file.Replace('\\', '/');
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null)
                {
                    sb.AppendLine($"ERROR: Could not load prefab at {assetPath}");
                    continue;
                }

                GameObject inst = (GameObject)Object.Instantiate(prefab);
                inst.name = prefab.name;

                MeshRenderer[] renderers = inst.GetComponentsInChildren<MeshRenderer>(true);
                Bounds b = new Bounds();
                bool hasFirst = false;
                int activeRenderers = 0;
                foreach (var r in renderers)
                {
                    if (r.enabled && r.gameObject.activeInHierarchy)
                    {
                        if (!hasFirst) { b = r.bounds; hasFirst = true; }
                        else { b.Encapsulate(r.bounds); }
                        activeRenderers++;
                    }
                }

                sb.AppendLine($"Prefab '{prefab.name}' (Path={assetPath}):");
                sb.AppendLine($"  Total MeshRenderers: {renderers.Length}, Active Renderers: {activeRenderers}");
                sb.AppendLine($"  Raw Bounds Center: {b.center}");
                sb.AppendLine($"  Raw Bounds Size: {b.size}");
                sb.AppendLine($"  Raw Bounds Magnitude: {b.size.magnitude}\n");

                Object.DestroyImmediate(inst);
            }

            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[MachineryPrefabInspector] COMPLETE! Written to " + LogPath);
        }
    }
}
#endif
