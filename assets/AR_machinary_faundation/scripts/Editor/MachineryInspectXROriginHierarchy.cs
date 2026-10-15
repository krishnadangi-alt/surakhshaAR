using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;

public static class MachineryInspectXROriginHierarchy
{
    [MenuItem("Machinery/Inspect XR Origin Hierarchy")]
    public static void Inspect()
    {
        var sb = new StringBuilder();
        EditorSceneManager.OpenScene("Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity", OpenSceneMode.Single);
        
        var xrOrigin = Object.FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null)
        {
            sb.AppendLine($"XROrigin GameObject: {xrOrigin.gameObject.name}");
            PrintTree(xrOrigin.transform, sb, 1);
        }
        else
        {
            sb.AppendLine("XROrigin NOT FOUND in scene!");
        }

        File.WriteAllText("xr_origin_hierarchy_report.txt", sb.ToString());
        Debug.Log(sb.ToString());
    }

    private static void PrintTree(Transform t, StringBuilder sb, int indent)
    {
        string ind = new string(' ', indent * 2);
        var cam = t.GetComponent<Camera>();
        string camTag = t.tag;
        sb.AppendLine($"{ind}- {t.name} (Tag: {camTag}, Active: {t.gameObject.activeSelf})");
        
        foreach (var c in t.GetComponents<Component>())
        {
            if (c != null)
                sb.AppendLine($"{ind}   [Component] {c.GetType().FullName} (Enabled={(c is Behaviour b ? b.enabled.ToString() : "N/A")})");
        }

        for (int i = 0; i < t.childCount; i++)
        {
            PrintTree(t.GetChild(i), sb, indent + 1);
        }
    }
}
