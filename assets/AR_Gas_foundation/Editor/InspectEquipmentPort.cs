using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class InspectEquipmentPort
{
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
    private static readonly string ArtifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";

    [MenuItem("Tools/Inspect Equipment Port")]
    public static void Inspect()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        StringBuilder sb = new StringBuilder();

        GameObject equipment = GameObject.Find("Confined_Space_Vessel_Tank");
        if (equipment != null)
        {
            sb.AppendLine($"Equipment Root: {equipment.name} Pos: {equipment.transform.position}");
            MeshRenderer[] renderers = equipment.GetComponentsInChildren<MeshRenderer>();
            foreach (var mr in renderers)
            {
                sb.AppendLine($"  Child: {mr.gameObject.name} | Bounds Center: {mr.bounds.center} | Bounds Extents: {mr.bounds.extents} | Min: {mr.bounds.min} | Max: {mr.bounds.max}");
            }
        }
        else
        {
            sb.AppendLine("Confined_Space_Vessel_Tank NOT FOUND!");
        }

        GameObject cylinder = GameObject.Find("Realistic_Gas_Cylinder");
        if (cylinder != null)
        {
            sb.AppendLine($"\nCylinder Root: {cylinder.name} Pos: {cylinder.transform.position}");
            MeshRenderer[] renderers = cylinder.GetComponentsInChildren<MeshRenderer>();
            foreach (var mr in renderers)
            {
                sb.AppendLine($"  Child: {mr.gameObject.name} | Bounds Center: {mr.bounds.center} | Bounds Extents: {mr.bounds.extents} | Min: {mr.bounds.min} | Max: {mr.bounds.max}");
            }
        }

        string logPath = Path.Combine(ArtifactDir, "equipment_cylinder_bounds.txt");
        File.WriteAllText(logPath, sb.ToString());
        Debug.Log("Dumped equipment & cylinder bounds to " + logPath);
    }
}
