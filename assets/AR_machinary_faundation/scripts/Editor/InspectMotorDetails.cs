using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;

public static class InspectMotorDetails
{
    [MenuItem("Machinery/Inspect Motor Details")]
    public static void Inspect()
    {
        string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== DETAILED MOTOR INSPECTION ===");
        
        GameObject driveMotor = GameObject.Find("DriveMotor");
        sb.AppendLine($"DriveMotor: {GetFullPath(driveMotor)}");
        if (driveMotor != null)
        {
            sb.AppendLine($"  World Pos: {driveMotor.transform.position.ToString("F4")}, Rot: {driveMotor.transform.eulerAngles.ToString("F4")}, LossyScale: {driveMotor.transform.lossyScale.ToString("F4")}");
            sb.AppendLine($"  Local Pos: {driveMotor.transform.localPosition.ToString("F4")}, Rot: {driveMotor.transform.localEulerAngles.ToString("F4")}, LocalScale: {driveMotor.transform.localScale.ToString("F4")}");
        }

        GameObject[] allBasePlates = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var go in allBasePlates)
        {
            if (go.name == "MotorBasePlate" || go.name == "Coupling" || go.name.Contains("DriveShaftCoupling") || go.name.Contains("MotorShelfPlate"))
            {
                sb.AppendLine($"\nMatching Object: '{go.name}' | Path: {GetFullPath(go)} | ActiveInHierarchy: {go.activeInHierarchy}");
                sb.AppendLine($"  World Pos: {go.transform.position.ToString("F4")}, Rot: {go.transform.eulerAngles.ToString("F4")}, Scale: {go.transform.lossyScale.ToString("F4")}");
                var r = go.GetComponent<Renderer>();
                if (r != null)
                {
                    sb.AppendLine($"  Renderer Bounds Center: {r.bounds.center.ToString("F4")}, Size: {r.bounds.size.ToString("F4")}, MinY: {r.bounds.min.y:F4}, MaxY: {r.bounds.max.y:F4}");
                }
            }
        }

        File.WriteAllText("detailed_motor_inspection.txt", sb.ToString());
        Debug.Log("Detailed motor inspection finished.");
    }

    private static string GetFullPath(GameObject go)
    {
        if (go == null) return "null";
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
