using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class InspectPressureGaugeOnly
{
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
    private static readonly string ArtifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";

    [MenuItem("Tools/Inspect Pressure Gauge Only")]
    public static void Inspect()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("=== PRESSURE GAUGE SURGICAL AUDIT ===");

        // Find pressure gauge objects
        GameObject gaugeObj = GameObject.Find("Real_Pressure_Gauge");
        if (gaugeObj == null) gaugeObj = GameObject.Find("Pressure_Gauge_3D");
        if (gaugeObj == null) gaugeObj = GameObject.Find("pressure_gauge");

        if (gaugeObj != null)
        {
            sb.AppendLine($"Pressure Gauge Found: '{gaugeObj.name}'");
            sb.AppendLine($"  Path: {GetHierarchyPath(gaugeObj)}");
            sb.AppendLine($"  World Position: {gaugeObj.transform.position}");
            sb.AppendLine($"  Local Position: {gaugeObj.transform.localPosition}");
            sb.AppendLine($"  World Rotation (Euler): {gaugeObj.transform.eulerAngles}");
            sb.AppendLine($"  Local Rotation (Euler): {gaugeObj.transform.localEulerAngles}");
            sb.AppendLine($"  World Scale: {gaugeObj.transform.lossyScale}");
            sb.AppendLine($"  Local Scale: {gaugeObj.transform.localScale}");

            MeshRenderer[] mrs = gaugeObj.GetComponentsInChildren<MeshRenderer>();
            foreach (var mr in mrs)
            {
                sb.AppendLine($"  MeshRenderer '{mr.gameObject.name}': Bounds Center={mr.bounds.center}, Extents={mr.bounds.extents}, Min={mr.bounds.min}, Max={mr.bounds.max}");
            }
        }
        else
        {
            sb.AppendLine("No pressure gauge object found by standard names!");
        }

        // Find regulator
        GameObject regObj = GameObject.Find("Real_Gas_Regulator");
        if (regObj == null) regObj = GameObject.Find("Gas_Regulator_3D");
        if (regObj != null)
        {
            sb.AppendLine($"\nRegulator Found: '{regObj.name}'");
            sb.AppendLine($"  World Position: {regObj.transform.position}");
            MeshRenderer[] mrs = regObj.GetComponentsInChildren<MeshRenderer>();
            foreach (var mr in mrs)
            {
                sb.AppendLine($"  MeshRenderer '{mr.gameObject.name}': Bounds Center={mr.bounds.center}, Extents={mr.bounds.extents}, Min={mr.bounds.min}, Max={mr.bounds.max}");
            }
        }

        // Find pipe 1
        GameObject pipe1Obj = GameObject.Find("Pipe_Regulator_To_Valve");
        if (pipe1Obj == null) pipe1Obj = GameObject.Find("Horizontal_Pipe_1");
        if (pipe1Obj != null)
        {
            sb.AppendLine($"\nPipe 1 Found: '{pipe1Obj.name}'");
            sb.AppendLine($"  World Position: {pipe1Obj.transform.position}");
            MeshRenderer[] mrs = pipe1Obj.GetComponentsInChildren<MeshRenderer>();
            foreach (var mr in mrs)
            {
                sb.AppendLine($"  MeshRenderer '{mr.gameObject.name}': Bounds Center={mr.bounds.center}, Extents={mr.bounds.extents}, Min={mr.bounds.min}, Max={mr.bounds.max}");
            }
        }

        string logPath = Path.Combine(ArtifactDir, "gauge_surgical_audit.txt");
        File.WriteAllText(logPath, sb.ToString());
        Debug.Log("Gauge surgical audit log written to " + logPath);
    }

    private static string GetHierarchyPath(GameObject go)
    {
        string path = go.name;
        Transform p = go.transform.parent;
        while (p != null)
        {
            path = p.name + "/" + path;
            p = p.parent;
        }
        return path;
    }
}
