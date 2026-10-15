using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;

public static class MachineryMotorDiagnostic
{
    [MenuItem("Machinery/Diagnose Motor")]
    public static void RunDiagnostic()
    {
        string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("==========================================================");
        sb.AppendLine("MACHINERY MOTOR DIAGNOSTIC REPORT");
        sb.AppendLine("==========================================================");
        sb.AppendLine($"Scene: {scene.path}");
        
        // Find DriveMotor, MotorBasePlate, Coupling, ConveyorAssembly
        GameObject driveMotor = GameObject.Find("DriveMotor");
        GameObject basePlate = GameObject.Find("MotorBasePlate");
        GameObject coupling = GameObject.Find("Coupling");
        GameObject conveyor = GameObject.Find("ConveyorAssembly");
        
        sb.AppendLine("\n--- ROOT OBJECT FIND RESULTS ---");
        sb.AppendLine($"DriveMotor found: {driveMotor != null}");
        sb.AppendLine($"MotorBasePlate found: {basePlate != null}");
        sb.AppendLine($"Coupling found: {coupling != null}");
        sb.AppendLine($"ConveyorAssembly found: {conveyor != null}");
        
        // Search scene for any object with 'motor' or 'Motor' or 'electric' in name
        sb.AppendLine("\n--- ALL MATCHING MOTOR/COUPLING/BASE OBJECTS IN SCENE ---");
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.name.ToLower().Contains("motor") || go.name.ToLower().Contains("coupling") || go.name.ToLower().Contains("baseplate") || go.name.ToLower().Contains("electric"))
            {
                sb.AppendLine($"  Object: '{go.name}' | Parent: '{(go.transform.parent != null ? go.transform.parent.name : "null")}' | Active: {go.activeSelf} | Path: {GetFullPath(go)}");
            }
        }
        
        if (driveMotor != null)
        {
            sb.AppendLine("\n--- DRIVEMOTOR DETAILED ANALYSIS ---");
            InspectHierarchy(driveMotor, sb, 0);
        }
        
        if (basePlate != null)
        {
            sb.AppendLine("\n--- MOTORBASEPLATE DETAILED ANALYSIS ---");
            InspectHierarchy(basePlate, sb, 0);
        }
        
        if (coupling != null)
        {
            sb.AppendLine("\n--- COUPLING DETAILED ANALYSIS ---");
            InspectHierarchy(coupling, sb, 0);
        }

        if (conveyor != null)
        {
            sb.AppendLine("\n--- CONVEYOR ASSEMBLY DETAILED ANALYSIS ---");
            InspectHierarchy(conveyor, sb, 0);
        }

        string reportPath = "motor_diagnostic_report.txt";
        File.WriteAllText(reportPath, sb.ToString());
        Debug.Log(sb.ToString());
    }

    private static void InspectHierarchy(GameObject go, StringBuilder sb, int depth)
    {
        string indent = new string(' ', depth * 2);
        Transform t = go.transform;
        
        sb.AppendLine($"{indent}GameObject: '{go.name}'");
        sb.AppendLine($"{indent}  Path: {GetFullPath(go)}");
        sb.AppendLine($"{indent}  Active (Self/Hierarchy): {go.activeSelf} / {go.activeInHierarchy}");
        sb.AppendLine($"{indent}  Prefab Utility Status: {PrefabUtility.GetPrefabInstanceStatus(go)} | Nearest Prefab Root: {(PrefabUtility.GetNearestPrefabInstanceRoot(go) != null ? PrefabUtility.GetNearestPrefabInstanceRoot(go).name : "null")}");
        sb.AppendLine($"{indent}  Local Pos: {t.localPosition.ToString("F4")} | Rot: {t.localEulerAngles.ToString("F4")} | Scale: {t.localScale.ToString("F4")}");
        sb.AppendLine($"{indent}  World Pos: {t.position.ToString("F4")} | Rot: {t.eulerAngles.ToString("F4")} | LossyScale: {t.lossyScale.ToString("F4")}");

        // Components
        var components = go.GetComponents<Component>();
        sb.AppendLine($"{indent}  Components ({components.Length}):");
        foreach (var c in components)
        {
            if (c == null)
            {
                sb.AppendLine($"{indent}    - NULL Component (Missing script!)");
                continue;
            }
            sb.AppendLine($"{indent}    - {c.GetType().FullName}");
            if (c is MeshFilter mf && mf.sharedMesh != null)
            {
                sb.AppendLine($"{indent}      Mesh Name: '{mf.sharedMesh.name}' | Vertices: {mf.sharedMesh.vertexCount} | Bounds Center: {mf.sharedMesh.bounds.center.ToString("F4")}, Extents: {mf.sharedMesh.bounds.extents.ToString("F4")}");
            }
            if (c is MeshRenderer mr)
            {
                sb.AppendLine($"{indent}      Renderer Enabled: {mr.enabled} | Bounds Center: {mr.bounds.center.ToString("F4")}, Extents: {mr.bounds.extents.ToString("F4")}, Size: {mr.bounds.size.ToString("F4")}");
                if (mr.sharedMaterials != null)
                {
                    for (int i = 0; i < mr.sharedMaterials.Length; i++)
                    {
                        var mat = mr.sharedMaterials[i];
                        sb.AppendLine($"{indent}      Material [{i}]: {(mat != null ? mat.name : "null")}");
                    }
                }
            }
        }

        for (int i = 0; i < t.childCount; i++)
        {
            InspectHierarchy(t.GetChild(i).gameObject, sb, depth + 1);
        }
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
