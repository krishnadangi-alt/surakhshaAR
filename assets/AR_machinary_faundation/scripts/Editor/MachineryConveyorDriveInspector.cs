using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;

public static class MachineryConveyorDriveInspector
{
    [MenuItem("Machinery/Inspect Conveyor Drive Geometry")]
    public static void InspectConveyor()
    {
        string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== CONVEYOR DRIVE GEOMETRY INSPECTION ===");
        
        GameObject conveyor = GameObject.Find("ConveyorAssembly");
        if (conveyor == null)
        {
            sb.AppendLine("ConveyorAssembly not found!");
            File.WriteAllText("conveyor_geometry_report.txt", sb.ToString());
            return;
        }

        sb.AppendLine($"Conveyor Root: {conveyor.name} | Pos: {conveyor.transform.position} | Rot: {conveyor.transform.eulerAngles} | Scale: {conveyor.transform.lossyScale}");

        var renderers = conveyor.GetComponentsInChildren<Renderer>(true);
        sb.AppendLine($"Total Renderers in ConveyorAssembly: {renderers.Length}");

        Bounds combinedActiveBounds = new Bounds();
        bool hasFirst = false;

        foreach (var r in renderers)
        {
            if (r.enabled && r.gameObject.activeInHierarchy)
            {
                if (!hasFirst)
                {
                    combinedActiveBounds = r.bounds;
                    hasFirst = true;
                }
                else
                {
                    combinedActiveBounds.Encapsulate(r.bounds);
                }
                sb.AppendLine($"  [ACTIVE RENDERER] '{r.gameObject.name}' | Path: {GetFullPath(r.gameObject)}");
                sb.AppendLine($"    World Center: {r.bounds.center.ToString("F4")}, Extents: {r.bounds.extents.ToString("F4")}, Size: {r.bounds.size.ToString("F4")}");
                
                var mf = r.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    sb.AppendLine($"    Mesh: '{mf.sharedMesh.name}', Vertices: {mf.sharedMesh.vertexCount}, Mesh Bounds Local Center: {mf.sharedMesh.bounds.center.ToString("F4")}, Local Extents: {mf.sharedMesh.bounds.extents.ToString("F4")}");
                }
            }
            else
            {
                sb.AppendLine($"  [INACTIVE RENDERER] '{r.gameObject.name}' | Path: {GetFullPath(r.gameObject)}");
            }
        }

        sb.AppendLine("\n=== COMBINED ACTIVE CONVEYOR BOUNDS ===");
        sb.AppendLine($"Center: {combinedActiveBounds.center.ToString("F4")}");
        sb.AppendLine($"Extents: {combinedActiveBounds.extents.ToString("F4")}");
        sb.AppendLine($"Size: {combinedActiveBounds.size.ToString("F4")}");
        sb.AppendLine($"Min: {combinedActiveBounds.min.ToString("F4")}");
        sb.AppendLine($"Max: {combinedActiveBounds.max.ToString("F4")}");

        File.WriteAllText("conveyor_geometry_report.txt", sb.ToString());
        Debug.Log("Conveyor inspection complete.");
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
