using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class InspectAllBackupScenes
{
    private static readonly string[] BackupPaths = new string[]
    {
        "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation_SafetyBackup.unity",
        "ProjectBackup/AR_gas_Foundation_Backup.unity",
        "backup_gas_pipe_workstation/AR_gas_Foundation.unity"
    };

    [MenuItem("Tools/Inspect All Backup Scenes")]
    public static void Execute()
    {
        foreach (var path in BackupPaths)
        {
            if (!File.Exists(path))
            {
                Debug.Log("Path does not exist: " + path);
                continue;
            }

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            Debug.Log($"=== SCENE: {path} ===");

            Transform[] allTransforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            foreach (var t in allTransforms)
            {
                if (t.name.Contains("Regulator") || t.name.Contains("Gauge") || t.name.Contains("Cylinder") || t.name.Contains("Workbench") || t.name.Contains("Vessel") || t.name.Contains("Confined") || t.name.Contains("Valve") || t.name.Contains("Pipe") || t.name.Contains("Lifeline"))
                {
                    Debug.Log($"[{path}] Object: '{t.name}' | Active: {t.gameObject.activeInHierarchy} | Parent: '{(t.parent != null ? t.parent.name : "ROOT")}' | Pos: {t.position} | Rot: {t.eulerAngles} | Scale: {t.lossyScale}");
                }
            }
        }
    }
}
