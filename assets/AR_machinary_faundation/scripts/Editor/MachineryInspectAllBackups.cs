using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;

public static class MachineryInspectAllBackups
{
    [MenuItem("Machinery/Inspect All Backups Structure")]
    public static void InspectBackups()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("==========================================================");
        sb.AppendLine("MACHINERY BACKUP CANDIDATE AUDIT REPORT");
        sb.AppendLine("==========================================================");

        List<string> candidatePaths = new List<string>();
        
        string backupDir = "Assets/AR_machinary_faundation/scenes/Backups";
        if (Directory.Exists(backupDir))
        {
            foreach (var f in Directory.GetFiles(backupDir, "*.unity"))
            {
                candidatePaths.Add(f.Replace('\\', '/'));
            }
        }

        string sceneDir = "Assets/AR_machinary_faundation/scenes";
        foreach (var f in Directory.GetFiles(sceneDir, "*.unity"))
        {
            string p = f.Replace('\\', '/');
            if (p.Contains("_BACKUP") || p.Contains("_BEFORE_"))
            {
                candidatePaths.Add(p);
            }
        }

        sb.AppendLine($"Found {candidatePaths.Count} candidate backup scenes.\n");

        foreach (var path in candidatePaths)
        {
            FileInfo fi = new FileInfo(path);
            sb.AppendLine($"----------------------------------------------------------");
            sb.AppendLine($"BACKUP: {fi.Name}");
            sb.AppendLine($"Path: {path} | Size: {fi.Length} bytes | Modified: {fi.LastWriteTime:yyyy-MM-dd HH:mm:ss}");

            try
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                if (!scene.IsValid())
                {
                    sb.AppendLine("  [ERROR] Could not open scene!");
                    continue;
                }

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
                    // Search all objects for coal
                    foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                    {
                        if (go.name.ToLower().Contains("coal")) coalCount++;
                    }
                }

                sb.AppendLine($"  Presence Check:");
                sb.AppendLine($"    FeedHopper: {hopper != null} | ConveyorAssembly: {conveyor != null} | CoalGroup/Count: {coalGroup != null} ({coalCount} items)");
                sb.AppendLine($"    DriveMotor: {motor != null} | MotorBasePlate: {motorBase != null} | Coupling: {coupling != null}");
                sb.AppendLine($"    ControlPanel: {panel != null} | EmergencyStop: {eStop != null} | MainIsolator: {isolator != null}");
                sb.AppendLine($"    SteelWorkstationTable: {table != null} | Hasp: {hasp != null} | PadlockAndTag: {padlock != null}");

                if (motor != null)
                {
                    sb.AppendLine($"  DriveMotor World Pos: {motor.transform.position.ToString("F4")}, Rot: {motor.transform.eulerAngles.ToString("F4")}, LossyScale: {motor.transform.lossyScale.ToString("F4")}");
                    if (motorBase != null)
                        sb.AppendLine($"    MotorBasePlate World Pos: {motorBase.transform.position.ToString("F4")}, Scale: {motorBase.transform.lossyScale.ToString("F4")}");
                    if (coupling != null)
                        sb.AppendLine($"    Coupling World Pos: {coupling.transform.position.ToString("F4")}");
                }

                if (conveyor != null)
                {
                    sb.AppendLine($"  ConveyorAssembly World Pos: {conveyor.transform.position.ToString("F4")}, Rot: {conveyor.transform.eulerAngles.ToString("F4")}");
                }

                if (table != null)
                {
                    sb.AppendLine($"  SteelWorkstationTable World Pos: {table.transform.position.ToString("F4")}");
                    if (hasp != null) sb.AppendLine($"    Hasp World Pos: {hasp.transform.position.ToString("F4")}");
                    if (padlock != null) sb.AppendLine($"    PadlockAndTag World Pos: {padlock.transform.position.ToString("F4")}");
                }
            }
            catch (System.Exception ex)
            {
                sb.AppendLine($"  [EXCEPTION] {ex.Message}");
            }
        }

        File.WriteAllText("all_backups_audit_report.txt", sb.ToString());
        Debug.Log("Backup inspection report generated.");
    }
}
