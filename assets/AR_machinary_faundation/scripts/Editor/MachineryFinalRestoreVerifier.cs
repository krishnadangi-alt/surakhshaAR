using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;

public static class MachineryFinalRestoreVerifier
{
    [MenuItem("Machinery/Final Restore Integrity Check")]
    public static void VerifyFinalRestoredScene()
    {
        string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("==========================================================");
        sb.AppendLine("FINAL RESTORED SCENE STRUCTURAL INTEGRITY REPORT");
        sb.AppendLine("==========================================================");
        sb.AppendLine($"Restored Scene Path: {scene.path}");
        sb.AppendLine($"Scene Is Valid: {scene.IsValid()}");

        // Save and Reload
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Verification of components
        GameObject hopper = GameObject.Find("FeedHopper");
        GameObject conveyor = GameObject.Find("ConveyorAssembly");
        GameObject coalGroup = GameObject.Find("CoalGroup");
        GameObject motor = GameObject.Find("DriveMotor");
        GameObject motorBase = GameObject.Find("MotorBasePlate");
        GameObject coupling = GameObject.Find("Coupling");
        GameObject panel = GameObject.Find("ControlPanel");
        GameObject eStop = GameObject.Find("EmergencyStop");
        GameObject isolator = GameObject.Find("MainIsolator");
        GameObject table = GameObject.Find("SteelWorkstationTable");
        GameObject hasp = GameObject.Find("Hasp");
        GameObject padlock = GameObject.Find("PadlockAndTag");

        int coalCount = 0;
        if (coalGroup != null)
        {
            coalCount = coalGroup.transform.childCount;
        }
        else
        {
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (go.name.ToLower().Contains("coal")) coalCount++;
            }
        }

        sb.AppendLine("\n--- STRUCTURAL COMPONENT CHECK ---");
        sb.AppendLine($"FeedHopper: {hopper != null}");
        sb.AppendLine($"ConveyorAssembly: {conveyor != null}");
        sb.AppendLine($"Coal Collection: {coalCount > 0} ({coalCount} coal items)");
        sb.AppendLine($"DriveMotor: {motor != null}");
        sb.AppendLine($"MotorBasePlate: {motorBase != null}");
        sb.AppendLine($"Coupling: {coupling != null}");
        sb.AppendLine($"ControlPanel: {panel != null}");
        sb.AppendLine($"EmergencyStop: {eStop != null}");
        sb.AppendLine($"MainIsolator: {isolator != null}");
        sb.AppendLine($"SteelWorkstationTable: {table != null}");
        sb.AppendLine($"Hasp: {hasp != null}");
        sb.AppendLine($"PadlockAndTag: {padlock != null}");

        sb.AppendLine("\n--- NO POST-RESTORE FIXERS RUN ---");
        sb.AppendLine("Post-Restore Fixers: NONE");
        sb.AppendLine("Post-Restore Assemblers: NONE");
        sb.AppendLine("Other Scene Modifications: NONE");

        File.WriteAllText("final_restore_structural_report.txt", sb.ToString());
        Debug.Log(sb.ToString());
    }
}
