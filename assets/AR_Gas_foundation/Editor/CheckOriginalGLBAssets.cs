#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class CheckOriginalGLBAssets
{
    [MenuItem("SurakshaAR/Check Original GLB Assets")]
    public static void CheckAssets()
    {
        Debug.Log("========================================================");
        Debug.Log("DIAGNOSTIC INVENTORY OF ALL 3D ASSETS IN PROJECT");
        Debug.Log("========================================================");

        string[] modelPaths = new string[]
        {
            "Assets/AR_Gas_foundation/3d/GasCylinder/Source_SM/Gas_Cylinder_SM.fbx",
            "Assets/AR_Gas_foundation/3d/GasCylinder/Prefabs/gas_cylinder.prefab",
            "Assets/AR_Gas_foundation/3d/GasRegulator/gas_regulator.prefab",
            "Assets/AR_Gas_foundation/3d/GasRegulator/gas_regulator_hd.obj",
            "Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge.prefab",
            "Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge_hd.obj",
            "Assets/AR_Gas_foundation/3d/GasPipeline/Source_Valve/VALVE.fbx",
            "Assets/AR_Gas_foundation/3d/GasPipeline/industrial_pipe.prefab",
            "Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_hd.obj",
            "Assets/AR_Gas_foundation/3d/PPE/workbench_hd.obj",
            "Assets/AR_Gas_foundation/3d/PPE/workbench.prefab",
            "Assets/AR_Gas_foundation/3d/PPE/ppe_hardhat.prefab",
            "Assets/AR_Gas_foundation/3d/PPE/ppe_hardhat_hd.obj",
            "Assets/AR_Gas_foundation/3d/PPE/ppe_vest.prefab",
            "Assets/AR_Gas_foundation/3d/PPE/ppe_vest_hd.obj",
            "Assets/AR_Gas_foundation/3d/PPE/safety_goggles_converted.fbx",
            "Assets/AR_Gas_foundation/3d/PPE/work_gloves_converted.fbx",
            "Assets/AR_Gas_foundation/3d/PPE/work_boot_converted.fbx",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment_converted.fbx",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/confined_vessel_hd.obj",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/confined_vessel.prefab",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/ventilation_fan_converted.fbx",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/self_retracting_lifeline_converted.fbx"
        };

        foreach (string path in modelPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError("[ASSET FAIL] Could not load GameObject at: " + path);
                continue;
            }

            Renderer[] rends = prefab.GetComponentsInChildren<Renderer>(true);
            MeshFilter[] filters = prefab.GetComponentsInChildren<MeshFilter>(true);
            Bounds combined = new Bounds(Vector3.zero, Vector3.zero);
            if (rends.Length > 0)
            {
                combined = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++) combined.Encapsulate(rends[i].bounds);
            }

            Debug.Log(string.Format("[ASSET OK] {0} | Renderers: {1} | Meshes: {2} | Bounds Size: {3}", Path.GetFileName(path), rends.Length, filters.Length, combined.size));

            foreach (var r in rends)
            {
                string matNames = "";
                foreach (var m in r.sharedMaterials)
                {
                    if (m != null) matNames += m.name + " (Shader: " + m.shader.name + "), ";
                    else matNames += "NULL, ";
                }
                Debug.Log(string.Format("    Sub-Node: '{0}' | Materials: {1}", r.gameObject.name, matNames));
            }
        }
        Debug.Log("========================================================");
    }
}
#endif
