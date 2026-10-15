using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;

public static class MachineryMotorMeshDetail
{
    [MenuItem("Machinery/Inspect Motor Mesh Detail")]
    public static void InspectMotor()
    {
        string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== MOTOR DETAILED MESH & ORIENTATION DIAGNOSTIC ===");
        
        GameObject motor = GameObject.Find("DriveMotor");
        if (motor == null)
        {
            sb.AppendLine("DriveMotor not found!");
            File.WriteAllText("motor_mesh_detail_report.txt", sb.ToString());
            return;
        }

        sb.AppendLine($"DriveMotor Root Transform:");
        sb.AppendLine($"  World Pos: {motor.transform.position.ToString("F4")}");
        sb.AppendLine($"  World Rot (Euler): {motor.transform.eulerAngles.ToString("F4")}");
        sb.AppendLine($"  Local Scale: {motor.transform.localScale.ToString("F4")}");
        sb.AppendLine($"  Lossy Scale: {motor.transform.lossyScale.ToString("F4")}");

        // Inspect all child mesh renderers
        var renderers = motor.GetComponentsInChildren<MeshRenderer>(true);
        sb.AppendLine($"\nDriveMotor Child Renderers ({renderers.Length}):");

        Bounds overallMotorBounds = new Bounds();
        bool hasBounds = false;

        foreach (var mr in renderers)
        {
            if (!mr.enabled || !mr.gameObject.activeInHierarchy) continue;
            
            // Skip MotorBasePlate and Coupling when computing motor body bounds
            if (mr.gameObject.name == "MotorBasePlate" || mr.gameObject.name == "Coupling")
                continue;

            if (!hasBounds)
            {
                overallMotorBounds = mr.bounds;
                hasBounds = true;
            }
            else
            {
                overallMotorBounds.Encapsulate(mr.bounds);
            }

            sb.AppendLine($"  Renderer '{mr.gameObject.name}' | Path: {GetFullPath(mr.gameObject)}");
            sb.AppendLine($"    World Center: {mr.bounds.center.ToString("F4")}");
            sb.AppendLine($"    World Extents: {mr.bounds.extents.ToString("F4")}");
            sb.AppendLine($"    World Size: {mr.bounds.size.ToString("F4")}");
            
            var mf = mr.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                Mesh m = mf.sharedMesh;
                sb.AppendLine($"    Mesh Name: '{m.name}' | Vertex Count: {m.vertexCount}");
                sb.AppendLine($"    Mesh Local Bounds Center: {m.bounds.center.ToString("F4")}, Extents: {m.bounds.extents.ToString("F4")}");
            }
        }

        sb.AppendLine($"\n=== OVERALL MOTOR BODY RENDERER BOUNDS (Excl. Base & Coupling) ===");
        sb.AppendLine($"Center: {overallMotorBounds.center.ToString("F4")}");
        sb.AppendLine($"Extents: {overallMotorBounds.extents.ToString("F4")}");
        sb.AppendLine($"Size: {overallMotorBounds.size.ToString("F4")}");
        sb.AppendLine($"Min: {overallMotorBounds.min.ToString("F4")}");
        sb.AppendLine($"Max: {overallMotorBounds.max.ToString("F4")}");

        // Inspect MotorBasePlate
        Transform baseTr = motor.transform.Find("MotorBasePlate");
        if (baseTr != null)
        {
            sb.AppendLine($"\n=== MOTOR BASE PLATE ===");
            sb.AppendLine($"Local Pos: {baseTr.localPosition.ToString("F4")} | Rot: {baseTr.localEulerAngles.ToString("F4")} | Scale: {baseTr.localScale.ToString("F4")}");
            sb.AppendLine($"World Pos: {baseTr.position.ToString("F4")} | Rot: {baseTr.eulerAngles.ToString("F4")} | Scale: {baseTr.lossyScale.ToString("F4")}");
            var mr = baseTr.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                sb.AppendLine($"World Bounds Center: {mr.bounds.center.ToString("F4")}, Size: {mr.bounds.size.ToString("F4")}");
            }
        }

        // Inspect Coupling
        Transform couplingTr = motor.transform.Find("Coupling");
        if (couplingTr != null)
        {
            sb.AppendLine($"\n=== COUPLING ===");
            sb.AppendLine($"Local Pos: {couplingTr.localPosition.ToString("F4")} | Rot: {couplingTr.localEulerAngles.ToString("F4")} | Scale: {couplingTr.localScale.ToString("F4")}");
            sb.AppendLine($"World Pos: {couplingTr.position.ToString("F4")} | Rot: {couplingTr.eulerAngles.ToString("F4")} | Scale: {couplingTr.lossyScale.ToString("F4")}");
            var mr = couplingTr.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                sb.AppendLine($"World Bounds Center: {mr.bounds.center.ToString("F4")}, Size: {mr.bounds.size.ToString("F4")}");
            }
        }

        File.WriteAllText("motor_mesh_detail_report.txt", sb.ToString());
        Debug.Log("Motor mesh detail inspection finished.");
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
