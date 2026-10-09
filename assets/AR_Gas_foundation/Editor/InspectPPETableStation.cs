using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class InspectPPETableStation
{
    [MenuItem("Tools/Inspect PPE Table Station")]
    public static void Execute()
    {
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject ppeStation = GameObject.Find("PPE_Preparation_Station");
        Debug.Log("=== PPE PREPARATION STATION INSPECTION ===");

        if (ppeStation != null)
        {
            DumpHierarchy(ppeStation.transform, 0);
        }
        else
        {
            Debug.LogError("COULD NOT FIND 'PPE_Preparation_Station'!");
        }

        // Search project for any wood materials, workbench models, or vest models
        Debug.Log("=== SEARCHING PROJECT ASSETS FOR WOOD MATERIALS, WORKBENCHES & VESTS ===");
        string[] guids = AssetDatabase.FindAssets("t:Material t:Model t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string lower = path.ToLower();
            if (lower.Contains("wood") || lower.Contains("brown") || lower.Contains("workbench") || lower.Contains("table") || lower.Contains("vest") || lower.Contains("ppe") || lower.Contains("shelf"))
            {
                Debug.Log($"ASSET: {path}");
            }
        }
    }

    private static void DumpHierarchy(Transform t, int indent)
    {
        string prefix = new string(' ', indent * 2);
        Renderer r = t.GetComponent<Renderer>();
        string rInfo = "";
        if (r != null)
        {
            string mats = "";
            foreach (Material m in r.sharedMaterials)
            {
                if (m != null) mats += m.name + " ";
            }
            rInfo = $" [Renderer Mat: {mats}, Center: {r.bounds.center.ToString("F4")}, Size: {r.bounds.size.ToString("F4")}, MinY: {r.bounds.min.y:F4}, MaxY: {r.bounds.max.y:F4}]";
        }

        Debug.Log($"{prefix}- '{t.name}' (LocalPos: {t.localPosition.ToString("F4")}, WorldPos: {t.position.ToString("F4")}, Rot: {t.eulerAngles.ToString("F4")}, Scale: {t.localScale.ToString("F4")}){rInfo}");

        foreach (Transform child in t)
        {
            DumpHierarchy(child, indent + 1);
        }
    }
}
