using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class InspectBackupsDetail
{
    private static readonly string ArtifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";

    [MenuItem("Tools/Inspect Backups Detail")]
    public static void Inspect()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== BACKUPS DETAILED INSPECTION ===");

        string[] backupPaths = new string[]
        {
            "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation_Backup_20260928_1235_PressureGaugeFix.unity",
            "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation_Backup_PipelineFix.unity",
            "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation_Backup_20260928_1145.unity",
            "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation_Backup_20260928_MasterStep0.unity"
        };

        foreach (var path in backupPaths)
        {
            sb.AppendLine($"\n--- SCENE: {path} ---");
            if (!File.Exists(path))
            {
                sb.AppendLine("  File does not exist!");
                continue;
            }

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            GameObject scenarioRoot = GameObject.Find("GasScenarioRoot");
            if (scenarioRoot == null)
            {
                sb.AppendLine("  GasScenarioRoot not found!");
                continue;
            }

            Transform pipelineAssemblyTr = scenarioRoot.transform.Find("GasPipelineAssembly");
            if (pipelineAssemblyTr == null)
            {
                sb.AppendLine("  GasPipelineAssembly not found!");
                continue;
            }

            sb.AppendLine($"  GasPipelineAssembly children ({pipelineAssemblyTr.childCount}):");
            for (int i = 0; i < pipelineAssemblyTr.childCount; i++)
            {
                Transform child = pipelineAssemblyTr.GetChild(i);
                MeshFilter mf = child.GetComponentInChildren<MeshFilter>();
                MeshRenderer mr = child.GetComponentInChildren<MeshRenderer>();
                string meshName = mf != null && mf.sharedMesh != null ? mf.sharedMesh.name : "none";
                string matName = mr != null && mr.sharedMaterial != null ? mr.sharedMaterial.name : "none";
                sb.AppendLine($"    - '{child.name}' | Active: {child.gameObject.activeSelf} | Pos: {child.position} | Rot: {child.eulerAngles} | Scale: {child.localScale} | Mesh: {meshName} | Mat: {matName}");
            }
        }

        string logPath = Path.Combine(ArtifactDir, "backups_detailed_inspection.txt");
        File.WriteAllText(logPath, sb.ToString());
        Debug.Log("Backups detailed inspection log written to " + logPath);
    }
}
