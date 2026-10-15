using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;

public static class MachineryVerifyRestoredScene
{
    [MenuItem("Machinery/Verify Restored Pre-LOTO Scene")]
    public static void VerifyRestoredScene()
    {
        string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== PRE-LOTO RESTORED SCENE INTEGRITY CHECK ===");
        sb.AppendLine($"Loaded Scene: {scene.path}");
        sb.AppendLine($"Scene Is Valid: {scene.IsValid()}");
        sb.AppendLine($"Scene Root Count: {scene.rootCount}");

        GameObject motor = GameObject.Find("DriveMotor");
        GameObject basePlate = GameObject.Find("MotorBasePlate");
        GameObject coupling = GameObject.Find("Coupling");
        GameObject table = GameObject.Find("SteelWorkstationTable");
        GameObject hasp = GameObject.Find("Hasp");
        GameObject padlock = GameObject.Find("PadlockAndTag");

        if (motor != null)
        {
            sb.AppendLine($"DriveMotor Pos: {motor.transform.position.ToString("F4")}, Rot: {motor.transform.eulerAngles.ToString("F4")}");
        }
        if (basePlate != null)
        {
            sb.AppendLine($"MotorBasePlate Pos: {basePlate.transform.position.ToString("F4")}, Rot: {basePlate.transform.eulerAngles.ToString("F4")}");
        }
        if (coupling != null)
        {
            sb.AppendLine($"Coupling Pos: {coupling.transform.position.ToString("F4")}, Rot: {coupling.transform.eulerAngles.ToString("F4")}");
        }
        if (hasp != null)
        {
            sb.AppendLine($"Hasp Pos: {hasp.transform.position.ToString("F4")}, Rot: {hasp.transform.eulerAngles.ToString("F4")}");
        }
        if (padlock != null)
        {
            sb.AppendLine($"PadlockAndTag Pos: {padlock.transform.position.ToString("F4")}, Rot: {padlock.transform.eulerAngles.ToString("F4")}");
        }

        File.WriteAllText("restored_scene_integrity_report.txt", sb.ToString());
        Debug.Log("Restored scene integrity check finished.");
    }
}
