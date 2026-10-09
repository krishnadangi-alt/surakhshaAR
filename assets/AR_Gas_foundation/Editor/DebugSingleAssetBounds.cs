#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class DebugSingleAssetBounds
{
    [MenuItem("SurakshaAR/Debug Single Asset Bounds")]
    public static void DebugBounds()
    {
        Debug.Log("==========================================================================");
        Debug.Log("DEBUGGING EXACT UNMOLLESTED ASSET BOUNDS");
        Debug.Log("==========================================================================");

        string[] paths = new string[]
        {
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/confined_vessel_hd.obj",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment_converted.fbx",
            "Assets/AR_Gas_foundation/3d/PPE/workbench_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasCylinder/Prefabs/gas_cylinder_real.prefab"
        };

        foreach (var p in paths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (prefab == null) continue;
            GameObject temp = (GameObject)Object.Instantiate(prefab);
            temp.transform.position = Vector3.zero;
            temp.transform.rotation = Quaternion.identity;
            temp.transform.localScale = Vector3.one;

            Renderer[] rends = temp.GetComponentsInChildren<Renderer>(true);
            if (rends.Length > 0)
            {
                Bounds b = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
                Debug.Log($"Path: {p} | Name: {temp.name} | Bounds Size: ({b.size.x:F3}, {b.size.y:F3}, {b.size.z:F3}) | Bounds Center: ({b.center.x:F3}, {b.center.y:F3}, {b.center.z:F3})");
            }
            Object.DestroyImmediate(temp);
        }

        Debug.Log("==========================================================================");
    }
}
#endif
