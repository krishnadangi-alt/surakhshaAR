using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class InspectMaterialsAndPPE
{
    [MenuItem("Tools/Inspect Materials And PPE")]
    public static void Execute()
    {
        string[] matGuids = AssetDatabase.FindAssets("t:Material");
        Debug.Log($"=== TOTAL MATERIALS FOUND: {matGuids.Length} ===");

        foreach (string guid in matGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                Color color = Color.white;
                if (mat.HasProperty("_Color")) color = mat.color;
                else if (mat.HasProperty("_BaseColor")) color = mat.GetColor("_BaseColor");

                string lower = path.ToLower();
                if (lower.Contains("wood") || lower.Contains("brown") || lower.Contains("table") || lower.Contains("workbench") || lower.Contains("leather") || lower.Contains("steel") || lower.Contains("mat_"))
                {
                    Debug.Log($"MAT: {path} | Color: {color.ToString()} | Shader: {mat.shader.name}");
                }
            }
        }
    }
}
