using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;

public static class MachineryLotoTableInspector
{
    [MenuItem("Machinery/Inspect LOTO Table Geometry")]
    public static void InspectLotoTable()
    {
        string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== LOTO TABLE & OBJECT GEOMETRY INSPECTION ===");

        GameObject table = GameObject.Find("SteelWorkstationTable");
        GameObject hasp = GameObject.Find("Hasp");
        GameObject padlock = GameObject.Find("PadlockAndTag");

        sb.AppendLine($"SteelWorkstationTable Found: {table != null}");
        sb.AppendLine($"Hasp Found: {hasp != null}");
        sb.AppendLine($"PadlockAndTag Found: {padlock != null}");

        if (table != null)
        {
            sb.AppendLine($"\n--- STEEL WORKSTATION TABLE ---");
            sb.AppendLine($"  Path: {GetFullPath(table)}");
            sb.AppendLine($"  Active Self/Hierarchy: {table.activeSelf}/{table.activeInHierarchy}");
            sb.AppendLine($"  World Pos: {table.transform.position.ToString("F4")}, Rot: {table.transform.eulerAngles.ToString("F4")}, LossyScale: {table.transform.lossyScale.ToString("F4")}");

            MeshRenderer[] mrs = table.GetComponentsInChildren<MeshRenderer>(true);
            sb.AppendLine($"  Child Renderers: {mrs.Length}");
            Bounds b = new Bounds();
            bool hasFirst = false;
            foreach (var mr in mrs)
            {
                sb.AppendLine($"    Renderer: '{mr.gameObject.name}' | Active: {mr.enabled}/{mr.gameObject.activeInHierarchy} | World Center: {mr.bounds.center.ToString("F4")}, Extents: {mr.bounds.extents.ToString("F4")}, Size: {mr.bounds.size.ToString("F4")}");
                if (mr.enabled && mr.gameObject.activeInHierarchy)
                {
                    if (!hasFirst) { b = mr.bounds; hasFirst = true; }
                    else { b.Encapsulate(mr.bounds); }
                }
            }

            if (hasFirst)
            {
                sb.AppendLine($"  COMBINED ACTIVE TABLE BOUNDS:");
                sb.AppendLine($"    Center: {b.center.ToString("F4")}");
                sb.AppendLine($"    Size:   {b.size.ToString("F4")}");
                sb.AppendLine($"    Min:    {b.min.ToString("F4")}");
                sb.AppendLine($"    Max:    {b.max.ToString("F4")}");
                sb.AppendLine($"    TABLETOP TOP SURFACE Y = {b.max.y:F4}");
            }
        }

        if (hasp != null)
        {
            sb.AppendLine($"\n--- HASP ---");
            sb.AppendLine($"  Path: {GetFullPath(hasp)}");
            sb.AppendLine($"  Active Self/Hierarchy: {hasp.activeSelf}/{hasp.activeInHierarchy}");
            sb.AppendLine($"  World Pos: {hasp.transform.position.ToString("F4")}, Rot: {hasp.transform.eulerAngles.ToString("F4")}, LossyScale: {hasp.transform.lossyScale.ToString("F4")}");

            MeshRenderer[] mrs = hasp.GetComponentsInChildren<MeshRenderer>(true);
            sb.AppendLine($"  Child Renderers: {mrs.Length}");
            Bounds b = new Bounds();
            bool hasFirst = false;
            foreach (var mr in mrs)
            {
                sb.AppendLine($"    Renderer: '{mr.gameObject.name}' | Active: {mr.enabled}/{mr.gameObject.activeInHierarchy} | World Center: {mr.bounds.center.ToString("F4")}, Extents: {mr.bounds.extents.ToString("F4")}, Size: {mr.bounds.size.ToString("F4")}");
                if (mr.enabled && mr.gameObject.activeInHierarchy)
                {
                    if (!hasFirst) { b = mr.bounds; hasFirst = true; }
                    else { b.Encapsulate(mr.bounds); }
                }
            }
            if (hasFirst)
            {
                sb.AppendLine($"  COMBINED HASP BOUNDS: Center={b.center.ToString("F4")}, Size={b.size.ToString("F4")}, MinY={b.min.y:F4}, MaxY={b.max.y:F4}");
            }
        }

        if (padlock != null)
        {
            sb.AppendLine($"\n--- PADLOCK AND TAG ---");
            sb.AppendLine($"  Path: {GetFullPath(padlock)}");
            sb.AppendLine($"  Active Self/Hierarchy: {padlock.activeSelf}/{padlock.activeInHierarchy}");
            sb.AppendLine($"  World Pos: {padlock.transform.position.ToString("F4")}, Rot: {padlock.transform.eulerAngles.ToString("F4")}, LossyScale: {padlock.transform.lossyScale.ToString("F4")}");

            MeshRenderer[] mrs = padlock.GetComponentsInChildren<MeshRenderer>(true);
            sb.AppendLine($"  Child Renderers: {mrs.Length}");
            Bounds b = new Bounds();
            bool hasFirst = false;
            foreach (var mr in mrs)
            {
                sb.AppendLine($"    Renderer: '{mr.gameObject.name}' | Active: {mr.enabled}/{mr.gameObject.activeInHierarchy} | World Center: {mr.bounds.center.ToString("F4")}, Extents: {mr.bounds.extents.ToString("F4")}, Size: {mr.bounds.size.ToString("F4")}");
                if (mr.enabled && mr.gameObject.activeInHierarchy)
                {
                    if (!hasFirst) { b = mr.bounds; hasFirst = true; }
                    else { b.Encapsulate(mr.bounds); }
                }
            }
            if (hasFirst)
            {
                sb.AppendLine($"  COMBINED PADLOCK BOUNDS: Center={b.center.ToString("F4")}, Size={b.size.ToString("F4")}, MinY={b.min.y:F4}, MaxY={b.max.y:F4}");
            }
        }

        File.WriteAllText("loto_table_geometry_report.txt", sb.ToString());
        Debug.Log("LOTO Table geometry inspection complete.");
    }

    private static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform t = go.transform.parent;
        while (t != null)
        {
            path = t.name + "/" + path;
            t = t.parent;
        }
        return path;
    }
}
