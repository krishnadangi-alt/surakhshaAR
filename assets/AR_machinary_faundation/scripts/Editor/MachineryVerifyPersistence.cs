using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;

public static class MachineryVerifyPersistence
{
    [MenuItem("Machinery/Verify Saved Scene Persistence")]
    public static void VerifyPersistence()
    {
        string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== SAVED SCENE PERSISTENCE VERIFICATION ===");
        sb.AppendLine($"Opened scene: {scene.path}");

        GameObject motor = GameObject.Find("DriveMotor");
        GameObject basePlate = GameObject.Find("MotorBasePlate");
        GameObject coupling = GameObject.Find("Coupling");

        sb.AppendLine($"DriveMotor World Position: {motor.transform.position.ToString("F4")}");
        sb.AppendLine($"DriveMotor World Rotation: {motor.transform.eulerAngles.ToString("F4")}");
        sb.AppendLine($"MotorBasePlate World Position: {basePlate.transform.position.ToString("F4")}");
        sb.AppendLine($"Coupling World Position: {coupling.transform.position.ToString("F4")}");

        // Check against target collinear alignment coordinates
        bool motorPosOk = Vector3.Distance(motor.transform.position, new Vector3(2.5000f, 1.0096f, 0.0104f)) < 0.01f;
        bool motorRotOk = Vector3.Distance(motor.transform.eulerAngles, new Vector3(0f, 90f, 0f)) < 0.1f;
        bool couplingAxisOk = Mathf.Abs(coupling.transform.position.y - 1.0096f) < 0.01f && Mathf.Abs(coupling.transform.position.z - 0.0104f) < 0.01f;

        sb.AppendLine($"\nMotor Position Target Match: {motorPosOk}");
        sb.AppendLine($"Motor Rotation Target Match: {motorRotOk}");
        sb.AppendLine($"Coupling Shaft Axis Match: {couplingAxisOk}");

        File.WriteAllText("persistence_verification_report.txt", sb.ToString());
        Debug.Log("Persistence verification finished.");
    }
}
