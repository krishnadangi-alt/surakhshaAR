#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public class GasAssetStagingBuilder
{
    [MenuItem("SurakshaAR/Build Temporary Asset Staging")]
    public static void BuildAssetStaging()
    {
        Debug.Log("==================================================");
        Debug.Log("[GAS REAL ASSET IMPORT & STAGING PASS]");
        Debug.Log("==================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Ensure Prefab Directories Exist
        EnsureDirectory("Assets/AR_Gas_foundation/3d/GasCylinder/Prefabs");
        EnsureDirectory("Assets/AR_Gas_foundation/3d/GasRegulator/Prefabs");
        EnsureDirectory("Assets/AR_Gas_foundation/3d/PressureGauge/Prefabs");
        EnsureDirectory("Assets/AR_Gas_foundation/3d/GasPipeline/Prefabs");
        EnsureDirectory("Assets/AR_Gas_foundation/3d/GasDetector/Prefabs");
        EnsureDirectory("Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs");
        EnsureDirectory("Assets/AR_Gas_foundation/3d/PPE/Prefabs");

        AssetDatabase.Refresh();

        // 2. Create Clean Prefabs from Model Assets
        CreatePrefabFromAsset("Assets/AR_Gas_foundation/3d/GasCylinder/gas_cylinder.glb", "Assets/AR_Gas_foundation/3d/GasCylinder/Prefabs/gas_cylinder_real.prefab");
        CreatePrefabFromAsset("Assets/AR_Gas_foundation/3d/GasRegulator/high-pressure_regulator.glb", "Assets/AR_Gas_foundation/3d/GasRegulator/Prefabs/high-pressure_regulator.prefab");
        CreatePrefabFromAsset("Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge.glb", "Assets/AR_Gas_foundation/3d/PressureGauge/Prefabs/pressure_gauge_real.prefab");
        CreatePrefabFromAsset("Assets/AR_Gas_foundation/3d/GasPipeline/industrial_valve.glb", "Assets/AR_Gas_foundation/3d/GasPipeline/Prefabs/industrial_valve_real.prefab");
        CreatePrefabFromAsset("Assets/AR_Gas_foundation/3d/GasPipeline/large_modular_pipes_metal.glb", "Assets/AR_Gas_foundation/3d/GasPipeline/Prefabs/large_modular_pipes_real.prefab");
        CreatePrefabFromAsset("Assets/AR_Gas_foundation/3d/GasDetector/h2s_gas_detector.glb", "Assets/AR_Gas_foundation/3d/GasDetector/Prefabs/h2s_gas_detector_real.prefab");
        CreatePrefabFromAsset("Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment.glb", "Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs/confined_space_tank_real.prefab");
        CreatePrefabFromAsset("Assets/AR_Gas_foundation/3d/ConfinedSpace/ventilation_fan.glb", "Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs/ventilation_fan_real.prefab");
        CreatePrefabFromAsset("Assets/AR_Gas_foundation/3d/ConfinedSpace/self_retracting_lifeline.glb", "Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs/retracting_lifeline_real.prefab");
        CreatePrefabFromAsset("Assets/AR_Gas_foundation/3d/PPE/safety_helmet.glb", "Assets/AR_Gas_foundation/3d/PPE/Prefabs/safety_helmet_real.prefab");
        CreatePrefabFromAsset("Assets/AR_Gas_foundation/3d/PPE/safety_goggles.glb", "Assets/AR_Gas_foundation/3d/PPE/Prefabs/safety_goggles_real.prefab");
        CreatePrefabFromAsset("Assets/AR_Gas_foundation/3d/PPE/work_gloves.glb", "Assets/AR_Gas_foundation/3d/PPE/Prefabs/work_gloves_real.prefab");
        CreatePrefabFromAsset("Assets/AR_Gas_foundation/3d/PPE/work_boot.glb", "Assets/AR_Gas_foundation/3d/PPE/Prefabs/safety_boots_real.prefab");

        AssetDatabase.Refresh();

        // 3. Create or Reset GasAssetStaging_TEMP Parent Object
        GameObject existingStaging = GameObject.Find("GasAssetStaging_TEMP");
        if (existingStaging != null)
        {
            Object.DestroyImmediate(existingStaging);
        }

        GameObject stagingRoot = new GameObject("GasAssetStaging_TEMP");
        stagingRoot.transform.position = Vector3.zero;

        // 4. Instantiate and Space Staged Prefabs for Inspection
        string[] prefabPaths = new string[]
        {
            "Assets/AR_Gas_foundation/3d/GasCylinder/Prefabs/gas_cylinder_real.prefab",
            "Assets/AR_Gas_foundation/3d/GasRegulator/Prefabs/high-pressure_regulator.prefab",
            "Assets/AR_Gas_foundation/3d/PressureGauge/Prefabs/pressure_gauge_real.prefab",
            "Assets/AR_Gas_foundation/3d/GasPipeline/Prefabs/industrial_valve_real.prefab",
            "Assets/AR_Gas_foundation/3d/GasPipeline/Prefabs/large_modular_pipes_real.prefab",
            "Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.prefab",
            "Assets/AR_Gas_foundation/3d/GasDetector/Prefabs/h2s_gas_detector_real.prefab",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs/confined_space_tank_real.prefab",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs/ventilation_fan_real.prefab",
            "Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs/retracting_lifeline_real.prefab",
            "Assets/AR_Gas_foundation/3d/PPE/Prefabs/safety_helmet_real.prefab",
            "Assets/AR_Gas_foundation/3d/PPE/Prefabs/safety_goggles_real.prefab",
            "Assets/AR_Gas_foundation/3d/PPE/Prefabs/work_gloves_real.prefab",
            "Assets/AR_Gas_foundation/3d/PPE/Prefabs/safety_boots_real.prefab"
        };

        float currentX = -4.5f;
        float currentZ = 0.0f;
        int itemsPerRow = 5;
        int count = 0;

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.SetParent(stagingRoot.transform, false);

                float xPos = currentX + (count % itemsPerRow) * 1.8f;
                float zPos = currentZ + (count / itemsPerRow) * 2.2f;
                instance.transform.localPosition = new Vector3(xPos, 0.0f, zPos);

                Debug.Log($"[STAGED ASSET] {prefab.name} -> Position ({xPos:F2}, 0.0, {zPos:F2})");
                count++;
            }
            else
            {
                Debug.LogWarning($"[STAGING WARNING] Could not load prefab at: {path}");
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log($"[STAGING COMPLETE] Staged {count} real 3D asset prefabs under GasAssetStaging_TEMP.");
        Debug.Log("==================================================");
    }

    private static void CreatePrefabFromAsset(string modelPath, string prefabPath)
    {
        if (File.Exists(prefabPath)) return;

        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (model == null)
        {
            Debug.LogError($"[PREFAB BUILD ERROR] Model not found at: {modelPath}");
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        Object.DestroyImmediate(instance);
        Debug.Log($"[CREATED PREFAB] {prefabPath}");
    }

    private static void EnsureDirectory(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
    }
}
#endif
