#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace SurakshaAR.Machinery.Editor
{
    public static class InspectSubmeshDetails
    {
        [MenuItem("SurakshaAR/Machinery/Inspect Submesh Details")]
        public static void InspectDetails()
        {
            Directory.CreateDirectory("scratch");
            using (StreamWriter sw = new StreamWriter("scratch/submesh_details.txt", false))
            {
                InspectPrefab(sw, "hopper", "Assets/AR_machinary_faundation/3d/Raw/hopper.glb");
                InspectPrefab(sw, "conveyor", "Assets/AR_machinary_faundation/3d/Raw/industrial_conveyor_belt.glb");
            }
            Debug.Log("[InspectSubmeshDetails] Log written to scratch/submesh_details.txt");
        }

        private static void InspectPrefab(StreamWriter sw, string label, string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) return;

            sw.WriteLine($"=================================================");
            sw.WriteLine($"PREFAB: {label} ({path})");
            sw.WriteLine($"=================================================");

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            
            MeshRenderer[] renderers = instance.GetComponentsInChildren<MeshRenderer>(true);
            sw.WriteLine($"Total MeshRenderers: {renderers.Length}");

            foreach (var r in renderers)
            {
                MeshFilter mf = r.GetComponent<MeshFilter>();
                Vector3 sz = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.bounds.size : Vector3.zero;
                string matName = (r.sharedMaterial != null) ? r.sharedMaterial.name : "None";
                
                // Get full path in hierarchy
                string fullPath = r.name;
                Transform current = r.transform.parent;
                while (current != null && current != instance.transform)
                {
                    fullPath = current.name + "/" + fullPath;
                    current = current.parent;
                }

                sw.WriteLine($"Renderer: '{fullPath}' | Mat: '{matName}' | MeshSize: ({sz.x:F1}, {sz.y:F1}, {sz.z:F1}) | Color: {(r.sharedMaterial != null && r.sharedMaterial.HasProperty("_Color") ? r.sharedMaterial.color.ToString() : "N/A")}");
            }

            Object.DestroyImmediate(instance);
        }
    }
}
#endif
