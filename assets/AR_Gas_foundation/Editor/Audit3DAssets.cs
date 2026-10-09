#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class Audit3DAssets
{
    [MenuItem("SurakshaAR/Audit 3D Assets")]
    public static void AuditAll()
    {
        Debug.Log("==========================================================================");
        Debug.Log("AUDITING ALL 3D ASSETS IN AR_GAS_FOUNDATION/3D");
        Debug.Log("==========================================================================");

        string[] assetPaths = new string[]
        {
            // GLB assets
            "Assets/AR_Gas_foundation/3d/GasCylinder/gas_cylinder.glb",
            "Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.glb",
            "Assets/AR_Gas_foundation/3d/GasDetector/h2s_gas_detector.glb",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment.glb",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/ventilation_fan.glb",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/self_retracting_lifeline.glb",
            "Assets/AR_Gas_foundation/3d/PPE/safety_goggles.glb",
            "Assets/AR_Gas_foundation/3d/PPE/safety_helmet.glb",
            "Assets/AR_Gas_foundation/3d/PPE/work_gloves.glb",
            "Assets/AR_Gas_foundation/3d/PPE/work_boot.glb",
            "Assets/AR_Gas_foundation/3d/GasRegulator/high-pressure_regulator.glb",
            "Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge.glb",

            // Converted FBX assets
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment_converted.fbx",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/ventilation_fan_converted.fbx",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/self_retracting_lifeline_converted.fbx",
            "Assets/AR_Gas_foundation/3d/PPE/safety_goggles_converted.fbx",
            "Assets/AR_Gas_foundation/3d/PPE/work_gloves_converted.fbx",
            "Assets/AR_Gas_foundation/3d/PPE/work_boot_converted.fbx",
            "Assets/AR_Gas_foundation/3d/GasPipeline/Source_Pipes/pipes.fbx",
            "Assets/AR_Gas_foundation/3d/GasPipeline/Source_Valve/VALVE.fbx",

            // OBJ assets
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/confined_vessel_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasCylinder/gas_cylinder_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasPipeline/industrial_pipe_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasPipeline/isolation_valve_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasRegulator/gas_regulator_hd.obj",
            "Assets/AR_Gas_foundation/3d/PPE/ppe_hardhat_hd.obj",
            "Assets/AR_Gas_foundation/3d/PPE/ppe_vest_hd.obj",
            "Assets/AR_Gas_foundation/3d/PPE/workbench_hd.obj",
            "Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge_hd.obj",

            // Prefabs
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs/confined_space_tank_real.prefab",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs/ventilation_fan_real.prefab",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs/retracting_lifeline_real.prefab",
            "Assets/AR_Gas_foundation/3d/GasCylinder/Prefabs/gas_cylinder_real.prefab",
            "Assets/AR_Gas_foundation/3d/GasDetector/Prefabs/h2s_gas_detector_real.prefab",
            "Assets/AR_Gas_foundation/3d/GasPipeline/Prefabs/large_modular_pipes_real.prefab",
            "Assets/AR_Gas_foundation/3d/GasPipeline/Prefabs/industrial_valve_real.prefab",
            "Assets/AR_Gas_foundation/3d/GasRegulator/Prefabs/high-pressure_regulator.prefab",
            "Assets/AR_Gas_foundation/3d/PPE/Prefabs/safety_boots_real.prefab",
            "Assets/AR_Gas_foundation/3d/PPE/Prefabs/safety_goggles_real.prefab",
            "Assets/AR_Gas_foundation/3d/PPE/Prefabs/safety_helmet_real.prefab",
            "Assets/AR_Gas_foundation/3d/PPE/Prefabs/work_gloves_real.prefab",
            "Assets/AR_Gas_foundation/3d/PPE/workbench.prefab",
            "Assets/AR_Gas_foundation/3d/PressureGauge/Prefabs/pressure_gauge_real.prefab"
        };

        foreach (var path in assetPaths)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null)
            {
                Debug.LogWarning($"[ASSET MISSING] Path: {path}");
                continue;
            }

            Renderer[] rends = go.GetComponentsInChildren<Renderer>(true);
            MeshFilter[] filters = go.GetComponentsInChildren<MeshFilter>(true);
            int vertCount = 0;
            foreach (var mf in filters)
            {
                if (mf.sharedMesh != null) vertCount += mf.sharedMesh.vertexCount;
            }

            Debug.Log($"[ASSET LOADED] Path: {path} | Name: {go.name} | Renderers: {rends.Length} | Vertices: {vertCount}");
        }

        Debug.Log("==========================================================================");
    }
}
#endif
