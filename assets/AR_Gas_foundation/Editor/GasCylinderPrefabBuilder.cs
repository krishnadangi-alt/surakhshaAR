#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class GasCylinderPrefabBuilder
{
    [MenuItem("SurakshaAR/Rebuild Clean Standalone Gas Cylinder Prefab")]
    public static void RebuildCylinderPrefab()
    {
        Debug.Log("==================================================");
        Debug.Log("[CYLINDER REBUILD] Creating Clean Standalone Prefab");
        Debug.Log("==================================================");

        string glbPath = "Assets/AR_Gas_foundation/3d/GasCylinder/gas_cylinder.glb";
        GameObject glbModel = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);

        if (glbModel == null)
        {
            Debug.LogError($"[CYLINDER REBUILD] Failed to load GLB model at {glbPath}");
            return;
        }

        // Instantiate clean model in memory
        GameObject rootObj = new GameObject("gas_cylinder");
        rootObj.transform.position = Vector3.zero;
        rootObj.transform.rotation = Quaternion.identity;
        rootObj.transform.localScale = Vector3.one;

        // Add BoxCollider
        BoxCollider boxCol = rootObj.AddComponent<BoxCollider>();

        // Instantiate imported GLB child
        GameObject modelChild = (GameObject)PrefabUtility.InstantiatePrefab(glbModel, rootObj.transform);
        modelChild.name = "gas_cylinder_model";
        modelChild.transform.localPosition = Vector3.zero;
        modelChild.transform.localRotation = Quaternion.identity;
        modelChild.transform.localScale = Vector3.one;

        // Measure raw bounds
        Renderer[] renderers = modelChild.GetComponentsInChildren<Renderer>(true);
        Debug.Log($"[CYLINDER REBUILD] Found {renderers.Length} renderer(s) in GLB model.");

        Bounds totalBounds = new Bounds();
        bool first = true;
        foreach (var r in renderers)
        {
            Debug.Log($"[CYLINDER REBUILD] Renderer '{r.name}': bounds center={r.bounds.center}, bounds size={r.bounds.size}");
            if (first)
            {
                totalBounds = r.bounds;
                first = false;
            }
            else
            {
                totalBounds.Encapsulate(r.bounds);
            }
        }

        Debug.Log($"[CYLINDER REBUILD] Total GLB raw size: {totalBounds.size}, center: {totalBounds.center}");

        // Setup BoxCollider based on exact raw bounds
        boxCol.center = totalBounds.center;
        boxCol.size = totalBounds.size;

        // Create LeakPoint at top valve area
        GameObject leakPointObj = new GameObject("LeakPoint");
        leakPointObj.transform.SetParent(rootObj.transform, false);
        // Set LeakPoint at top of bounds
        float topY = totalBounds.max.y > 0.1f ? totalBounds.max.y : 1.05f;
        leakPointObj.transform.localPosition = new Vector3(0f, topY, 0f);
        leakPointObj.transform.localRotation = Quaternion.identity;
        Debug.Log($"[CYLINDER REBUILD] LeakPoint placed at local pos: {leakPointObj.transform.localPosition}");

        // Save as clean standalone prefab
        string prefabPath = "Assets/AR_Gas_foundation/3d/GasCylinder/Prefabs/gas_cylinder.prefab";
        
        // Ensure directory exists
        string dir = Path.GetDirectoryName(prefabPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(rootObj, prefabPath);
        Object.DestroyImmediate(rootObj);

        if (savedPrefab != null)
        {
            Debug.Log($"[CYLINDER REBUILD] SUCCESS! Clean standalone prefab saved to {prefabPath}");
        }
        else
        {
            Debug.LogError($"[CYLINDER REBUILD] FAILED to save prefab at {prefabPath}");
        }

        Debug.Log("==================================================");
    }
}
#endif
