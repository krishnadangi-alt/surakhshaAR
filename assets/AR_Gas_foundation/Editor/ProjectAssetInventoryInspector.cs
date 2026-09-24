#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ProjectAssetInventoryInspector
{
    public static void InspectAllProjectAssets()
    {
        Debug.Log("==================================================");
        Debug.Log("[ASSET INVENTORY] STARTING FULL PROJECT ASSET AUDIT");
        Debug.Log("==================================================");

        string[] meshGuids = AssetDatabase.FindAssets("t:Mesh");
        Debug.Log($"TOTAL MESH ASSETS FOUND IN PROJECT: {meshGuids.Length}");

        foreach (string guid in meshGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh != null)
            {
                Debug.Log($"[MESH] Path: '{path}' | Name: '{mesh.name}' | Verts: {mesh.vertexCount} | Triangles: {mesh.triangles.Length / 3} | Submeshes: {mesh.subMeshCount} | Bounds: {mesh.bounds.size}");
            }
        }

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        Debug.Log($"TOTAL PREFAB ASSETS FOUND IN PROJECT: {prefabGuids.Length}");
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Debug.Log($"[PREFAB] Path: '{path}'");
        }

        string[] matGuids = AssetDatabase.FindAssets("t:Material");
        Debug.Log($"TOTAL MATERIAL ASSETS FOUND IN PROJECT: {matGuids.Length}");
        foreach (string guid in matGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                Debug.Log($"[MATERIAL] Path: '{path}' | Shader: '{mat.shader.name}' | Color: {mat.color}");
            }
        }

        string[] texGuids = AssetDatabase.FindAssets("t:Texture");
        Debug.Log($"TOTAL TEXTURE ASSETS FOUND IN PROJECT: {texGuids.Length}");
        foreach (string guid in texGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (tex != null)
            {
                Debug.Log($"[TEXTURE] Path: '{path}' | Size: {tex.width}x{tex.height}");
            }
        }

        Debug.Log("==================================================");
        Debug.Log("[ASSET INVENTORY] COMPLETED FULL PROJECT ASSET AUDIT");
        Debug.Log("==================================================");
    }
}
#endif
