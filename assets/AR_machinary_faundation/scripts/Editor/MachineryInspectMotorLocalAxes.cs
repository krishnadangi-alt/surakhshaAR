using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;

public static class MachineryInspectMotorLocalAxes
{
    [MenuItem("Machinery/Inspect Motor Mesh Local Axes")]
    public static void InspectAxes()
    {
        string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== MOTOR MESH LOCAL AXES DIAGNOSTIC ===");

        GameObject motor = GameObject.Find("DriveMotor");
        if (motor == null)
        {
            sb.AppendLine("DriveMotor not found!");
            File.WriteAllText("motor_axes_report.txt", sb.ToString());
            return;
        }

        Transform visualGroup = motor.transform.Find("Collada visual scene group");
        sb.AppendLine($"DriveMotor Root: Pos={motor.transform.position.ToString("F4")}, Rot={motor.transform.eulerAngles.ToString("F4")}, Scale={motor.transform.localScale.ToString("F4")}");
        sb.AppendLine($"VisualGroup: Pos={visualGroup.localPosition.ToString("F4")}, Rot={visualGroup.localEulerAngles.ToString("F4")}, Scale={visualGroup.localScale.ToString("F4")}");

        // Find body mesh
        Transform bodyTr = visualGroup.Find("body/defaultMaterial");
        if (bodyTr != null)
        {
            MeshFilter mf = bodyTr.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                Mesh mesh = mf.sharedMesh;
                sb.AppendLine($"Body Mesh Name: '{mesh.name}', Vertices: {mesh.vertexCount}");
                sb.AppendLine($"Local Mesh Bounds Center: {mesh.bounds.center.ToString("F4")}, Extents: {mesh.bounds.extents.ToString("F4")}, Size: {mesh.bounds.size.ToString("F4")}");
            }
        }

        // Test rotations of DriveMotor to find HORIZONTAL motor orientation
        // Test rotations: (0,0,0), (0,90,0), (0,0,90), (90,0,0), (0,90,90), (-90,0,0), etc.
        Vector3[] testRots = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 90, 0),
            new Vector3(0, 180, 0),
            new Vector3(0, 270, 0),
            new Vector3(90, 0, 0),
            new Vector3(-90, 0, 0),
            new Vector3(0, 0, 90),
            new Vector3(0, 0, -90),
            new Vector3(90, 90, 0),
            new Vector3(0, 90, 90),
            new Vector3(90, 0, 90)
        };

        sb.AppendLine("\n--- TESTING ROOT ROTATION IMPACT ON BODY BOUNDS SIZE ---");
        Vector3 origRot = motor.transform.eulerAngles;
        foreach (var rot in testRots)
        {
            motor.transform.eulerAngles = rot;
            MeshRenderer[] mrs = visualGroup.GetComponentsInChildren<MeshRenderer>();
            Bounds b = new Bounds();
            bool first = true;
            foreach (var mr in mrs)
            {
                if (first) { b = mr.bounds; first = false; }
                else { b.Encapsulate(mr.bounds); }
            }
            sb.AppendLine($"Rot {rot.ToString("F0")}: World Size = {b.size.ToString("F4")} | X Size={b.size.x:F4}, Y Size={b.size.y:F4}, Z Size={b.size.z:F4} | MinY={b.min.y:F4}, MaxY={b.max.y:F4}");
        }

        motor.transform.eulerAngles = origRot;

        File.WriteAllText("motor_axes_report.txt", sb.ToString());
        Debug.Log("Motor axes inspection finished.");
    }
}
