#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class ProjectAssetInventoryInspector
{
    [MenuItem("SurakshaAR/Print Full 3D Asset Inventory")]
    public static void PrintInventory()
    {
        Debug.Log("==================================================");
        Debug.Log("[FULL 3D ASSET INVENTORY AUDIT]");
        Debug.Log("==================================================");

        string targetDir = "Assets/AR_Gas_foundation/3d";
        string[] allFiles = Directory.GetFiles(targetDir, "*.*", SearchOption.AllDirectories);

        foreach (var file in allFiles)
        {
            string ext = Path.GetExtension(file).ToLower();
            if (ext == ".obj" || ext == ".fbx" || ext == ".glb" || ext == ".gltf" || ext == ".prefab")
            {
                string relativePath = file.Replace("\\", "/");
                InspectFile(relativePath);
            }
        }

        Debug.Log("==================================================");
        Debug.Log("[INVENTORY AUDIT COMPLETE]");
        Debug.Log("==================================================");
    }

    private static void InspectFile(string path)
    {
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (asset == null)
        {
            Debug.LogWarning($"[FILE UNLOADED/DEFAULT] {path}");
            return;
        }

        System.Type t = asset.GetType();
        long size = new FileInfo(path).Length;

        if (asset is GameObject go)
        {
            MeshFilter mf = go.GetComponentInChildren<MeshFilter>();
            MeshRenderer mr = go.GetComponentInChildren<MeshRenderer>();
            string meshName = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.name : "None";
            int vertCount = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.vertexCount : 0;

            string matNames = "";
            if (mr != null && mr.sharedMaterials != null)
            {
                foreach (var m in mr.sharedMaterials)
                {
                    if (m != null)
                    {
                        matNames += m.name + " (Shader: " + m.shader.name + "), ";
                    }
                }
            }

            Debug.Log($"[3D ASSET] {path} | Type: {t.Name} | Size: {size} bytes | Mesh: {meshName} (Verts: {vertCount}) | Mats: {matNames}");
        }
        else
        {
            Debug.Log($"[RAW ASSET] {path} | Type: {t.Name} | Size: {size} bytes");
        }
    }
}
#endif
