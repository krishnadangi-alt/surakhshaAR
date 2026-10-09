#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class TestWorkbenchPrefab
{
    [MenuItem("SurakhshaAR/Test Workbench Prefab")]
    public static void Test()
    {
        string path = "Assets/AR_Gas_foundation/3d/PPE/workbench.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab != null)
        {
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Renderer[] rends = inst.GetComponentsInChildren<Renderer>(true);
            if (rends.Length > 0)
            {
                Bounds b = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
                File.WriteAllText(@"C:\SurakhshaAR\WorkbenchBounds.txt", $"Prefab Bounds: Center={b.center}, Size={b.size}, Scale={inst.transform.localScale}");
            }
            Object.DestroyImmediate(inst);
        }
        else
        {
            File.WriteAllText(@"C:\SurakhshaAR\WorkbenchBounds.txt", "Prefab NOT found");
        }
    }
}
#endif
