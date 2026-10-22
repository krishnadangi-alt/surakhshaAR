using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;

public static class MachineryFindHorizontalMotorOrientation
{
    [MenuItem("Machinery/Find Horizontal Motor Orientation")]
    public static void FindOrientation()
    {
        string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== HORIZONTAL MOTOR ORIENTATION & SHAFT/FEET DIAGNOSTIC ===");

        GameObject motor = GameObject.Find("DriveMotor");
        Transform visualGroup = motor.transform.Find("Collada visual scene group");
        Transform bodyTr = visualGroup.Find("body/defaultMaterial");
        Transform partsTr = visualGroup.Find("parts/defaultMaterial");
        Transform closingPartTr = visualGroup.Find("closingPart/defaultMaterial");

        sb.AppendLine($"Body bounds center local: {bodyTr.GetComponent<MeshFilter>().sharedMesh.bounds.center.ToString("F4")}");
        sb.AppendLine($"Parts bounds center local: {partsTr.GetComponent<MeshFilter>().sharedMesh.bounds.center.ToString("F4")}");
        sb.AppendLine($"ClosingPart bounds center local: {closingPartTr.GetComponent<MeshFilter>().sharedMesh.bounds.center.ToString("F4")}");

        // Test root rotations that produce X-axis length (0.84m)
        Vector3[] candidates = new Vector3[]
        {
            new Vector3(0, 0, 90),
            new Vector3(0, 0, -90),
            new Vector3(90, 90, 0),
            new Vector3(90, 270, 0),
            new Vector3(-90, 90, 0),
            new Vector3(-90, 270, 0),
            new Vector3(0, 90, 90),
            new Vector3(0, 270, 90)
        };

        foreach (var rot in candidates)
        {
            motor.transform.eulerAngles = rot;

            Vector3 bodyCenter = bodyTr.GetComponent<MeshRenderer>().bounds.center;
            Vector3 closingCenter = closingPartTr.GetComponent<MeshRenderer>().bounds.center;
            Vector3 partsCenter = partsTr.GetComponent<MeshRenderer>().bounds.center;

            // Shaft vector: from motor body center toward closingPart/shaft end
            Vector3 shaftVector = closingCenter - bodyCenter;
            
            sb.AppendLine($"\nCandidate Rot {rot.ToString("F0")}:");
            sb.AppendLine($"  Body Center World: {bodyCenter.ToString("F4")}");
            sb.AppendLine($"  Shaft Center World: {closingCenter.ToString("F4")}");
            sb.AppendLine($"  Shaft Direction Vector: {shaftVector.ToString("F4")} (Normalized: {shaftVector.normalized.ToString("F4")})");
            sb.AppendLine($"  Parts Center World: {partsCenter.ToString("F4")}");
            
            Bounds b = bodyTr.GetComponent<MeshRenderer>().bounds;
            b.Encapsulate(partsTr.GetComponent<MeshRenderer>().bounds);
            b.Encapsulate(closingPartTr.GetComponent<MeshRenderer>().bounds);
            sb.AppendLine($"  Overall Bounds World Size: {b.size.ToString("F4")} | MinY: {b.min.y:F4}, MaxY: {b.max.y:F4}");
        }

        File.WriteAllText("horizontal_orientation_report.txt", sb.ToString());
        Debug.Log("Horizontal motor orientation inspection finished.");
    }
}
