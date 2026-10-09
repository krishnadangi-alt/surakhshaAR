using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class InspectProjectAssetsForLifelineSupport
{
    [MenuItem("Tools/Inspect Assets For Lifeline Support")]
    public static void Execute()
    {
        string[] guids = AssetDatabase.FindAssets("t:Model t:Prefab");
        Debug.Log($"=== TOTAL MODEL/PREFAB ASSETS FOUND: {guids.Length} ===");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string lower = path.ToLower();
            if (lower.Contains("lifeline") || lower.Contains("davit") || lower.Contains("support") || lower.Contains("arm") || lower.Contains("post") || lower.Contains("stand") || lower.Contains("bracket") || lower.Contains("crane") || lower.Contains("gantry") || lower.Contains("chain") || lower.Contains("confined") || lower.Contains("vessel") || lower.Contains("structure"))
            {
                Debug.Log($"ASSET: {path}");
            }
        }
    }
}
