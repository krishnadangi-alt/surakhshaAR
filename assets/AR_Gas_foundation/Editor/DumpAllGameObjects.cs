using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Text;
using System.IO;

public class DumpAllGameObjects
{
    [MenuItem("Tools/Dump All GameObjects")]
    public static void Execute()
    {
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject[] rootObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();
        StringBuilder sb = new StringBuilder();

        foreach (GameObject root in rootObjects)
        {
            DumpRecursively(root.transform, 0, sb);
        }

        string outputPath = Path.Combine(Application.dataPath, "../scene_hierarchy_dump.txt");
        File.WriteAllText(outputPath, sb.ToString());
        Debug.Log($"=== DUMPED {sb.Length} CHARS TO {outputPath} ===");
    }

    private static void DumpRecursively(Transform t, int indent, StringBuilder sb)
    {
        string prefix = new string(' ', indent * 2);
        string posStr = t.position.ToString("F4");
        string rotStr = t.eulerAngles.ToString("F4");
        string scaleStr = t.localScale.ToString("F4");
        
        Renderer r = t.GetComponent<Renderer>();
        string matInfo = "";
        if (r != null && r.sharedMaterial != null)
        {
            matInfo = $" [Mat: {r.sharedMaterial.name}, BoundsCenter: {r.bounds.center.ToString("F4")}, BoundsSize: {r.bounds.size.ToString("F4")}]";
        }

        sb.AppendLine($"{prefix}- '{t.name}' (Pos: {posStr}, Rot: {rotStr}, Scale: {scaleStr}, Active: {t.gameObject.activeSelf}){matInfo}");

        foreach (Transform child in t)
        {
            DumpRecursively(child, indent + 1, sb);
        }
    }
}
