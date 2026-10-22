using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;

public static class MachineryMotorAlignmentMaster
{
    [MenuItem("Machinery/Diagnose and Align Motor Only")]
    public static void RunDiagnosticAndAlign()
    {
        string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("==========================================================");
        sb.AppendLine("MACHINERY DRIVE MOTOR ALIGNMENT DIAGNOSTIC & EXECUTION");
        sb.AppendLine("==========================================================");
        
        GameObject motor = GameObject.Find("DriveMotor");
        GameObject basePlate = GameObject.Find("MotorBasePlate");
        GameObject coupling = GameObject.Find("Coupling");
        GameObject conveyor = GameObject.Find("ConveyorAssembly");

        sb.AppendLine($"DriveMotor found: {motor != null}");
        sb.AppendLine($"MotorBasePlate found: {basePlate != null}");
        sb.AppendLine($"Coupling found: {coupling != null}");
        sb.AppendLine($"ConveyorAssembly found: {conveyor != null}");

        if (motor == null || basePlate == null || coupling == null)
        {
            sb.AppendLine("ERROR: Required objects missing!");
            File.WriteAllText("motor_align_diagnostic.txt", sb.ToString());
            return;
        }

        // BasePlate Bounds & Top Surface
        Bounds baseBounds = GetBounds(basePlate);
        sb.AppendLine($"\n--- MOTOR BASE PLATE ---");
        sb.AppendLine($"  Pos: {basePlate.transform.position.ToString("F4")}, Rot: {basePlate.transform.eulerAngles.ToString("F4")}, Scale: {basePlate.transform.lossyScale.ToString("F4")}");
        sb.AppendLine($"  Bounds Center: {baseBounds.center.ToString("F4")}, Size: {baseBounds.size.ToString("F4")}");
        sb.AppendLine($"  Top Surface Y: {baseBounds.max.y:F4}");

        // Coupling Bounds & Center Axis
        Bounds couplingBounds = GetBounds(coupling);
        sb.AppendLine($"\n--- COUPLING ---");
        sb.AppendLine($"  Pos: {coupling.transform.position.ToString("F4")}, Rot: {coupling.transform.eulerAngles.ToString("F4")}, Scale: {coupling.transform.lossyScale.ToString("F4")}");
        sb.AppendLine($"  Bounds Center: {couplingBounds.center.ToString("F4")}, Size: {couplingBounds.size.ToString("F4")}");
        sb.AppendLine($"  Coupling Shaft Center Axis Y: {couplingBounds.center.y:F4}, Z: {couplingBounds.center.z:F4}, X: {couplingBounds.center.x:F4}");

        // DriveMotor details before change
        sb.AppendLine($"\n--- DRIVEMOTOR BEFORE CHANGE ---");
        sb.AppendLine($"  Root Pos: {motor.transform.position.ToString("F4")}, Rot: {motor.transform.eulerAngles.ToString("F4")}, Scale: {motor.transform.localScale.ToString("F4")}");
        Bounds initialMotorBounds = GetBounds(motor);
        sb.AppendLine($"  Bounds Center: {initialMotorBounds.center.ToString("F4")}, Size: {initialMotorBounds.size.ToString("F4")}");

        // Inspect DriveMotor sub-mesh structure
        sb.AppendLine($"\n--- DRIVEMOTOR SUBMESHES ---");
        MeshRenderer[] renderers = motor.GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in renderers)
        {
            sb.AppendLine($"  Child: '{mr.gameObject.name}', Mesh: '{(mr.GetComponent<MeshFilter>() != null ? mr.GetComponent<MeshFilter>().sharedMesh.name : "null")}'");
            sb.AppendLine($"    World Bounds Center: {mr.bounds.center.ToString("F4")}, Extents: {mr.bounds.extents.ToString("F4")}, Size: {mr.bounds.size.ToString("F4")}");
        }

        File.WriteAllText("motor_align_diagnostic.txt", sb.ToString());
        Debug.Log("Motor diagnostic saved to motor_align_diagnostic.txt");
    }

    private static Bounds GetBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            b.Encapsulate(renderers[i].bounds);
        }
        return b;
    }
}
