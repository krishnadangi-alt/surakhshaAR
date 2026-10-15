using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;

public static class MachineryInspectTree
{
    [MenuItem("Machinery/Inspect Tree")]
    public static void Inspect()
    {
        string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== SCENE ROOT OBJECTS AND DRIVE MOTOR CHILDREN ===");
        
        foreach (var rootGo in scene.GetRootGameObjects())
        {
            sb.AppendLine($"ROOT: '{rootGo.name}' (Pos: {rootGo.transform.position.ToString("F4")}, Rot: {rootGo.transform.eulerAngles.ToString("F4")}, Scale: {rootGo.transform.localScale.ToString("F4")})");
            if (rootGo.name == "DriveMotor" || rootGo.name == "MachineryScenario" || rootGo.name == "ConveyorAssembly")
            {
                PrintChildren(rootGo.transform, sb, 1);
            }
        }
        
        File.WriteAllText("scene_tree_report.txt", sb.ToString());
        Debug.Log("Scene tree report written.");
    }
    
    private static void PrintChildren(Transform t, StringBuilder sb, int indent)
    {
        string ind = new string(' ', indent * 2);
        for (int i = 0; i < t.childCount; i++)
        {
            Transform c = t.GetChild(i);
            sb.AppendLine($"{ind}Child: '{c.name}' (Pos: {c.position.ToString("F4")}, Rot: {c.eulerAngles.ToString("F4")}, LocalPos: {c.localPosition.ToString("F4")}, LocalRot: {c.localEulerAngles.ToString("F4")})");
            if (c.childCount > 0 && indent < 3)
            {
                PrintChildren(c, sb, indent + 1);
            }
        }
    }
}
