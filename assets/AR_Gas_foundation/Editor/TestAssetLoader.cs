#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class TestAssetLoader
{
    [MenuItem("SurakshaAR/Test Native Asset Loader")]
    public static void TestLoading()
    {
        string[] candidatePaths = new string[] {
            "Assets/AR_Gas_foundation/3d/GasCylinder/Source_SM/Gas_Cylinder_SM.fbx",
            "Assets/AR_Gas_foundation/3d/GasCylinder/gas_cylinder_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasPipeline/Source_Valve/VALVE.fbx",
            "Assets/AR_Gas_foundation/3d/GasPipeline/isolation_valve_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasPipeline/industrial_pipe_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasPipeline/pipe_elbow_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasRegulator/gas_regulator_hd.obj",
            "Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge_hd.obj",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/confined_vessel_hd.obj",
            "Assets/AR_Gas_foundation/3d/PPE/workbench_hd.obj",
            "Assets/AR_Gas_foundation/3d/PPE/ppe_hardhat_hd.obj",
            "Assets/AR_Gas_foundation/3d/PPE/ppe_vest_hd.obj"
        };

        Debug.Log("=== NATIVE 3D ASSET LOAD TEST ===");
        foreach (var p in candidatePaths)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            bool success = go != null;
            Debug.Log($"  {(success ? "SUCCESS" : "FAILED")}: {p}");
        }
        Debug.Log("=================================");
    }
}
#endif
