#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryFullAssetInspector
    {
        private const string Raw3DFolder = "Assets/AR_machinary_faundation/3d/Raw";
        private const string OutputLog = "Builds/Verification/UnityScene/full_asset_inventory.txt";

        [MenuItem("SurakshaAR/Machinery/Run Full Asset Inventory")]
        public static void RunInventory()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== FULL RAW 3D ASSET INVENTORY REPORT ===");
            sb.AppendLine($"Folder: {Raw3DFolder}\n");

            string[] files = Directory.GetFiles(Raw3DFolder, "*.glb");
            sb.AppendLine($"Total GLB Files Found: {files.Length}\n");

            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                sb.AppendLine($"==================================================");
                sb.AppendLine($"ASSET FILE: {fileName}");
                sb.AppendLine($"Path: {filePath}");

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
                if (prefab == null)
                {
                    sb.AppendLine("  ERROR: Could not load prefab asset!");
                    continue;
                }

                // Instantiate temporarily to measure
                GameObject tempObj = Object.Instantiate(prefab);
                tempObj.transform.position = Vector3.zero;
                tempObj.transform.rotation = Quaternion.identity;
                tempObj.transform.localScale = Vector3.one;

                MeshRenderer[] renderers = tempObj.GetComponentsInChildren<MeshRenderer>(true);
                sb.AppendLine($"  Total MeshRenderers: {renderers.Length}");

                if (renderers.Length > 0)
                {
                    Bounds combined = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++)
                    {
                        combined.Encapsulate(renderers[i].bounds);
                    }
                    sb.AppendLine($"  Unscaled Native Bounds Center: {combined.center}");
                    sb.AppendLine($"  Unscaled Native Bounds Size (WxHxD): {combined.size}");
                    sb.AppendLine($"  Unscaled Native Bounds Min: {combined.min}");
                    sb.AppendLine($"  Unscaled Native Bounds Max: {combined.max}");

                    sb.AppendLine("\n  Child Renderers Detail:");
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        MeshFilter mf = renderers[i].GetComponent<MeshFilter>();
                        string meshName = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.name : "N/A";
                        Vector3 meshSize = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.bounds.size : Vector3.zero;
                        string matName = (renderers[i].sharedMaterial != null) ? renderers[i].sharedMaterial.name : "N/A";
                        
                        // Highlight suspicious large/wall/floor geometry
                        bool isSuspicious = meshSize.x > 100f || meshSize.y > 100f || meshSize.z > 100f || 
                                            meshName.ToLower().Contains("wall") || meshName.ToLower().Contains("floor") || 
                                            meshName.ToLower().Contains("plane") || meshName.ToLower().Contains("room") || 
                                            meshName.ToLower().Contains("bg") || renderers[i].name.ToLower().Contains("wall") || 
                                            renderers[i].name.ToLower().Contains("plane");

                        string flag = isSuspicious ? " <<< [SUSPICIOUS ENVIRONMENT GEOMETRY]" : "";
                        sb.AppendLine($"    [{i}] Name: '{renderers[i].name}', Mesh: '{meshName}', Mat: '{matName}', LocalMeshBounds: {meshSize}{flag}");
                    }
                }
                else
                {
                    sb.AppendLine("  NO MESH RENDERERS FOUND!");
                }

                Object.DestroyImmediate(tempObj);
                sb.AppendLine();
            }

            Directory.CreateDirectory("Builds/Verification/UnityScene");
            File.WriteAllText(OutputLog, sb.ToString());
            Debug.Log($"[MachineryFullAssetInspector] Full Inventory written to {OutputLog}");
        }
    }
}
#endif
