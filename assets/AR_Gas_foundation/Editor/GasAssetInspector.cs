using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class GasAssetInspector
{
    private static readonly string ArtifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";

    [MenuItem("Tools/Inspect Pipeline Assets")]
    public static void InspectAssets()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== PIPELINE ASSETS AUDIT ===");

        string[] assetPaths = new string[]
        {
            "Assets/AR_Gas_foundation/3d/GasCylinder/gas_cylinder_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasCylinder/gas_cylinder.glb",
            "Assets/AR_Gas_foundation/3d/GasCylinder/Prefabs/gas_cylinder_real.prefab",

            "Assets/AR_Gas_foundation/3d/GasRegulator/high-pressure_regulator.glb",
            "Assets/AR_Gas_foundation/3d/GasRegulator/gas_regulator_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasRegulator/Prefabs/high-pressure_regulator.prefab",

            "Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge.glb",
            "Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge_hd.obj",
            "Assets/AR_Gas_foundation/3d/PressureGauge/Prefabs/pressure_gauge_real.prefab",

            "Assets/AR_Gas_foundation/3d/GasPipeline/Source_Valve/VALVE.fbx",
            "Assets/AR_Gas_foundation/3d/GasPipeline/industrial_valve.glb",
            "Assets/AR_Gas_foundation/3d/GasPipeline/isolation_valve_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasPipeline/Prefabs/industrial_valve_real.prefab",

            "Assets/AR_Gas_foundation/3d/GasPipeline/Source_Pipes/pipes.fbx",
            "Assets/AR_Gas_foundation/3d/GasPipeline/industrial_pipe_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasPipeline/pipe_elbow_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasPipeline/large_modular_pipes_metal.glb",

            "Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment.glb",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/confined_vessel_hd.obj"
        };

        foreach (string path in assetPaths)
        {
            sb.AppendLine($"\n--- ASSET: {path} ---");
            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (asset == null)
            {
                sb.AppendLine("  NOT FOUND!");
                continue;
            }

            GameObject go = null;
            bool isPrefabOrModel = false;
            if (asset is GameObject prefabGo)
            {
                go = UnityEngine.Object.Instantiate(prefabGo);
                isPrefabOrModel = true;
            }

            if (go != null)
            {
                InspectGameObject(go, sb, 1);
                UnityEngine.Object.DestroyImmediate(go);
            }
            else
            {
                sb.AppendLine($"  Type: {asset.GetType().Name}");
            }
        }

        string logPath = Path.Combine(ArtifactDir, "pipeline_assets_audit.txt");
        File.WriteAllText(logPath, sb.ToString());
        Debug.Log("Pipeline assets audit written to " + logPath);
    }

    private static void InspectGameObject(GameObject go, StringBuilder sb, int indent)
    {
        string prefix = new string(' ', indent * 2);
        MeshFilter mf = go.GetComponent<MeshFilter>();
        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        string meshInfo = "none";
        if (mf != null && mf.sharedMesh != null)
        {
            Mesh m = mf.sharedMesh;
            meshInfo = $"Mesh '{m.name}' Verts: {m.vertexCount}, LocalBounds: Min={m.bounds.min}, Max={m.bounds.max}, Size={m.bounds.size}";
        }
        string matInfo = "none";
        if (mr != null && mr.sharedMaterials != null)
        {
            string[] mats = new string[mr.sharedMaterials.Length];
            for (int i = 0; i < mr.sharedMaterials.Length; i++)
                mats[i] = mr.sharedMaterials[i] != null ? mr.sharedMaterials[i].name : "null";
            matInfo = string.Join(", ", mats);
        }

        sb.AppendLine($"{prefix}- {go.name} | Mesh: [{meshInfo}] | Mats: [{matInfo}] | Pos: {go.transform.localPosition} | Rot: {go.transform.localEulerAngles} | Scale: {go.transform.localScale}");

        for (int i = 0; i < go.transform.childCount; i++)
        {
            InspectGameObject(go.transform.GetChild(i).gameObject, sb, indent + 1);
        }
    }
}
