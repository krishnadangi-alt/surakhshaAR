using UnityEngine;
using UnityEditor;

public class InspectWorkbenchPrefab
{
    [MenuItem("Tools/Inspect Workbench Prefab")]
    public static void Execute()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/workbench.prefab");
        if (prefab != null)
        {
            Debug.Log($"=== PREFAB 'workbench.prefab' loaded. Name: {prefab.name} ===");
            PrintHierarchy(prefab.transform, 0);
        }
        else
        {
            Debug.LogError("workbench.prefab NOT FOUND!");
        }

        Object objAsset = AssetDatabase.LoadAssetAtPath<Object>("Assets/AR_Gas_foundation/3d/PPE/workbench_hd.obj");
        if (objAsset != null)
        {
            Debug.Log($"=== OBJ ASSET 'workbench_hd.obj' loaded type: {objAsset.GetType().Name} ===");
        }
    }

    private static void PrintHierarchy(Transform t, int indent)
    {
        string prefix = new string('-', indent * 2);
        Renderer r = t.GetComponent<Renderer>();
        MeshFilter mf = t.GetComponent<MeshFilter>();
        
        string info = $"{prefix} '{t.name}' | Pos: {t.localPosition.ToString("F4")}, Rot: {t.localRotation.eulerAngles.ToString("F4")}, Scale: {t.localScale.ToString("F4")}";
        if (mf != null && mf.sharedMesh != null)
        {
            info += $" | Mesh: {mf.sharedMesh.name}";
        }
        if (r != null)
        {
            info += $" | Materials ({r.sharedMaterials.Length}): ";
            foreach (var mat in r.sharedMaterials)
            {
                info += mat != null ? $"{mat.name}, " : "null, ";
            }
        }
        Debug.Log(info);

        for (int i = 0; i < t.childCount; i++)
        {
            PrintHierarchy(t.GetChild(i), indent + 1);
        }
    }
}
