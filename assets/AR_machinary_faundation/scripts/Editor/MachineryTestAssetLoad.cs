#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryTestAssetLoad
    {
        [MenuItem("SurakshaAR/Machinery/Test Asset Load")]
        public static void TestLoad()
        {
            string logPath = "scratch/asset_load_test.log";
            Directory.CreateDirectory("scratch");

            using (StreamWriter writer = new StreamWriter(logPath, false))
            {
                writer.WriteLine("=== TESTING GLB ASSET LOADING ===");
                string rawDir = "Assets/AR_machinary_faundation/3d/Raw";
                string[] files = Directory.GetFiles(rawDir, "*.glb");
                writer.WriteLine($"Found {files.Length} GLB files in {rawDir}:");

                foreach (var file in files)
                {
                    string assetPath = file.Replace("\\", "/");
                    Object loadedObj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    GameObject loadedGo = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    writer.WriteLine($"File: {assetPath}");
                    writer.WriteLine($"  Loaded Object: {(loadedObj != null ? loadedObj.GetType().Name : "NULL")}");
                    writer.WriteLine($"  Loaded GameObject: {(loadedGo != null ? loadedGo.name : "NULL")}");
                    if (loadedGo != null)
                    {
                        MeshFilter[] filters = loadedGo.GetComponentsInChildren<MeshFilter>(true);
                        MeshRenderer[] renderers = loadedGo.GetComponentsInChildren<MeshRenderer>(true);
                        writer.WriteLine($"  MeshFilters: {filters.Length}, MeshRenderers: {renderers.Length}");
                    }
                }
            }
            Debug.Log("[MachineryTestAssetLoad] Test log written to scratch/asset_load_test.log");
        }
    }
}
#endif
