using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;

public static class TestMotorOrientation
{
    [MenuItem("Machinery/Test Motor Orientations")]
    public static void TestOrientations()
    {
        string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== TEST MOTOR ORIENTATIONS AND SHAFT ALIGNMENT ===");
        
        GameObject motor = GameObject.Find("DriveMotor");
        if (motor == null) return;

        Vector3[] rotsToTest = new Vector3[]
        {
            new Vector3(0, 0, 90),
            new Vector3(0, 0, -90),
            new Vector3(0, 180, 90),
            new Vector3(0, 180, -90),
            new Vector3(90, 90, 0),
            new Vector3(-90, 90, 0)
        };

        Transform visual = motor.transform.Find("Collada visual scene group");
        Transform body = visual != null ? visual.Find("body") : null;
        Transform closingPart = visual != null ? visual.Find("closingPart") : null;
        Transform parts = visual != null ? visual.Find("parts") : null;

        foreach (var rot in rotsToTest)
        {
            motor.transform.rotation = Quaternion.Euler(rot);
            
            sb.AppendLine($"\n--- Candidate Euler {rot} ---");
            if (body != null && closingPart != null && parts != null)
            {
                var rBody = body.GetComponentInChildren<MeshRenderer>();
                var rClosing = closingPart.GetComponentInChildren<MeshRenderer>();
                var rParts = parts.GetComponentInChildren<MeshRenderer>();

                if (rBody != null && rClosing != null && rParts != null)
                {
                    Vector3 bodyCenter = rBody.bounds.center;
                    Vector3 closingCenter = rClosing.bounds.center;
                    Vector3 shaftVector = closingCenter - bodyCenter;

                    sb.AppendLine($"  Body Center: {bodyCenter.ToString("F4")}");
                    sb.AppendLine($"  Shaft Center (closingPart): {closingCenter.ToString("F4")}");
                    sb.AppendLine($"  Shaft Direction: {shaftVector.normalized.ToString("F4")}");
                    
                    Bounds fullMotorBounds = rBody.bounds;
                    fullMotorBounds.Encapsulate(rClosing.bounds);
                    fullMotorBounds.Encapsulate(rParts.bounds);
                    sb.AppendLine($"  Full Mesh Bounds Size: {fullMotorBounds.size.ToString("F4")}");
                    sb.AppendLine($"  Full Mesh Bounds MinY: {fullMotorBounds.min.y:F4}, MaxY: {fullMotorBounds.max.y:F4}");
                }
            }
        }

        File.WriteAllText("motor_orientation_results.txt", sb.ToString());
        Debug.Log("Orientation test results saved to motor_orientation_results.txt");
    }
}
