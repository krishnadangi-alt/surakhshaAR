using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class InspectFBXContents
{
    [MenuItem("Tools/Inspect FBX Contents")]
    public static void Execute()
    {
        string[] assetPaths = new string[]
        {
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment_converted.fbx",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/confined_vessel_hd.obj",
            "Assets/3d/Floor_Stand.fbx",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs/confined_space_tank_real.prefab",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs/retracting_lifeline_real.prefab"
        };

        foreach (string path in assetPaths)
        {
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (obj != null)
            {
                Debug.Log($"=== FBX/PREFAB: {path} ===");
                DumpTree(obj.transform, 0);
            }
            else
            {
                Debug.LogWarning($"Could not load asset at: {path}");
            }
        }
    }

    private static void DumpTree(Transform t, int indent)
    {
        string prefix = new string(' ', indent * 2);
        Renderer r = t.GetComponent<Renderer>();
        string rInfo = (r != null) ? $" [Renderer: {r.sharedMaterial?.name}, BoundsSize: {r.bounds.size.ToString("F4")}]" : "";
        Debug.Log($"{prefix}- '{t.name}'{rInfo}");

        foreach (Transform child in t)
        {
            DumpTree(child, indent + 1);
        }
    }
}
