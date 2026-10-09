#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GasMasterRealAssetRestorerV3
{
    [MenuItem("SurakshaAR/Restore Master Real Gas Workstation V3")]
    public static void PerformRestoration()
    {
        Debug.Log("==================================================");
        Debug.Log("[START] Master Real Gas Workstation Restoration V3");
        Debug.Log("==================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Clean & Prepare Hierarchy
        GameObject gasRoot = GameObject.Find("GasScenarioRoot");
        if (gasRoot == null)
        {
            gasRoot = new GameObject("GasScenarioRoot");
        }

        for (int i = gasRoot.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(gasRoot.transform.GetChild(i).gameObject);
        }

        Transform ppeStation = new GameObject("PPE_Preparation_Station").transform;
        ppeStation.SetParent(gasRoot.transform, false);
        ppeStation.localPosition = new Vector3(-2.2f, 0f, 0f);

        Transform gasLeakStation = new GameObject("Gas_Leak_Station").transform;
        gasLeakStation.SetParent(gasRoot.transform, false);
        gasLeakStation.localPosition = Vector3.zero;

        Transform confinedStation = new GameObject("Confined_Space_Training_Area").transform;
        confinedStation.SetParent(gasRoot.transform, false);
        confinedStation.localPosition = new Vector3(2.5f, 0f, 0f);

        // 2. Load Real Project Materials
        Material matCylinderRed = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_GasCylinder_Red.mat");
        Material matRegulatorBrass = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Regulator_Brass.mat");
        Material matGauge = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_PressureGauge.mat");
        Material matPipeSteel = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Pipeline_Steel.mat");
        Material matValveRed = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Valve_Red.mat");
        Material matDetectorYellow = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Detector_Yellow.mat");
        Material matDetectorBlack = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Detector_Black.mat");
        Material matSteel = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Structural_Steel.mat");
        Material matLeather = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_PPE_Leather.mat");
        Material matVessel = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Vessel_Tank.mat");

        // Cylinder Base Color & Normal Textures
        Texture2D cylTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AR_Gas_foundation/3d/GasCylinder/Source_SM/Canister_Gas_Lp_M_Canister_Gas_BaseColor.png");
        Texture2D cylNorm = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AR_Gas_foundation/3d/GasCylinder/Source_SM/Canister_Gas_Lp_M_Canister_Gas_Normal.png");

        if (matCylinderRed != null && cylTex != null)
        {
            matCylinderRed.mainTexture = cylTex;
            if (cylNorm != null) matCylinderRed.SetTexture("_BumpMap", cylNorm);
        }

        // 3. Instantiate REAL Assets

        // --- LEFT: PPE PREPARATION STATION ---
        // Real Workbench
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/PPE/workbench_hd.obj", ppeStation, "PPE_Workbench", new Vector3(0f, 0.4f, 0f), new Vector3(0f, 0f, 0f), Vector3.one, matSteel, matLeather);
        // Real Helmet
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/PPE/ppe_hardhat_hd.obj", ppeStation, "Safety_Helmet", new Vector3(-0.35f, 0.88f, -0.1f), new Vector3(0f, 30f, 0f), Vector3.one, matDetectorYellow);
        // Real Vest
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/PPE/ppe_vest_hd.obj", ppeStation, "Safety_Vest", new Vector3(0.35f, 0.88f, -0.1f), new Vector3(0f, -20f, 0f), Vector3.one, matDetectorYellow);
        // Real Goggles
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/PPE/safety_goggles_converted.fbx", ppeStation, "Safety_Goggles", new Vector3(0f, 0.87f, 0.05f), new Vector3(0f, 45f, 0f), new Vector3(0.012f, 0.012f, 0.012f));
        // Real Gloves
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/PPE/work_gloves_converted.fbx", ppeStation, "Work_Gloves", new Vector3(-0.15f, 0.87f, 0.05f), new Vector3(0f, 15f, 0f), new Vector3(0.015f, 0.015f, 0.015f));
        // Real Boots
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/PPE/work_boot_converted.fbx", ppeStation, "Safety_Boots", new Vector3(-0.45f, 0.02f, 0.35f), new Vector3(0f, 75f, 0f), new Vector3(0.015f, 0.015f, 0.015f));

        // --- CENTER: GAS LEAK STATION ---
        // Real Industrial Gas Cylinder (Reference Scale Object standing on floor)
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/GasCylinder/Source_SM/Gas_Cylinder_SM.fbx", gasLeakStation, "Realistic_Gas_Cylinder", new Vector3(-0.50f, 0f, 0f), new Vector3(0f, 180f, 0f), Vector3.one, matCylinderRed);

        // Real Regulator physically sitting directly on top outlet valve of cylinder
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/GasRegulator/gas_regulator_hd.obj", gasLeakStation, "Gas_Regulator_3D", new Vector3(-0.50f, 0.35f, 0f), new Vector3(0f, 90f, 0f), new Vector3(0.5f, 0.5f, 0.5f), matRegulatorBrass);

        // Real Pressure Gauge sitting directly on top port of regulator
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge_hd.obj", gasLeakStation, "Pressure_Gauge_3D", new Vector3(-0.50f, 0.45f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0.4f, 0.4f, 0.4f), matGauge);

        // Short Straight Pipe extending horizontally from regulator to valve
        string pipeAsset = "Assets/AR_Gas_foundation/3d/GasPipeline/industrial_pipe_hd.obj";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(pipeAsset) == null)
        {
            pipeAsset = "Assets/AR_Gas_foundation/3d/GasPipeline/industrial_pipe.obj";
        }
        InstantiateRealAsset(pipeAsset, gasLeakStation, "Short_Pipe", new Vector3(-0.38f, 0.35f, 0f), new Vector3(0f, 90f, 0f), new Vector3(0.08f, 0.08f, 0.15f), matPipeSteel);

        // Real Red Isolation Valve physically aligned inline on short pipe
        InstantiateRealAsset("Assets/industrial-valve/source/VALVE.fbx", gasLeakStation, "Isolation_Valve_3D", new Vector3(-0.30f, 0.35f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0.04f, 0.04f, 0.04f), matValveRed);

        // Real Multi-Gas Detector placed resting flat on floor beside gas cylinder
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_hd.obj", gasLeakStation, "Multi_Gas_Detector", new Vector3(-0.25f, 0.02f, 0.25f), new Vector3(-90f, 30f, 0f), new Vector3(0.6f, 0.6f, 0.6f), matDetectorYellow, matDetectorBlack);

        // --- RIGHT: CONFINED SPACE TRAINING AREA ---
        // Real Downloaded Confined Space Model (industrial_tanks_and_equipment_converted.fbx offset to center tank at Z=0)
        string confinedModel = "Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment_converted.fbx";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(confinedModel) == null)
        {
            confinedModel = "Assets/AR_Gas_foundation/3d/ConfinedSpace/confined_vessel_hd.obj";
        }
        InstantiateRealAsset(confinedModel, confinedStation, "REAL_Confined_Space_Model", new Vector3(0f, 0f, -4.5f), new Vector3(0f, 0f, 0f), Vector3.one, matVessel);

        // Axial Ventilation Fan
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/ConfinedSpace/ventilation_fan_converted.fbx", confinedStation, "Ventilation_Fan", new Vector3(-0.85f, 0.02f, -0.4f), new Vector3(0f, -30f, 0f), new Vector3(0.015f, 0.015f, 0.015f));

        // Lifeline
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/ConfinedSpace/self_retracting_lifeline_converted.fbx", confinedStation, "Lifeline", new Vector3(0.65f, 1.45f, 0.2f), new Vector3(0f, 0f, 0f), new Vector3(0.015f, 0.015f, 0.015f));

        // Save Scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("[SCENE SAVED SUCCESS V3] Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity updated with tight physical connections!");

        // Capture proof screenshots V3
        GasWorkstationScreenshotCapturerV3.CaptureAllScreenshots();

        Debug.Log("==================================================");
        Debug.Log("[RESTORATION V3 COMPLETE]");
        Debug.Log("==================================================");
    }

    private static GameObject InstantiateRealAsset(string assetPath, Transform parent, string objName, Vector3 localPos, Vector3 localRot, Vector3 localScale, params Material[] customMats)
    {
        GameObject sourceAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (sourceAsset == null)
        {
            Debug.LogError($"[ERROR] Real asset not found at {assetPath}");
            return null;
        }

        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(sourceAsset, parent);
        inst.name = objName;
        inst.transform.localPosition = localPos;
        inst.transform.localRotation = Quaternion.Euler(localRot);
        inst.transform.localScale = localScale;

        // Assign project materials if specified
        if (customMats != null && customMats.Length > 0 && customMats[0] != null)
        {
            Renderer[] renderers = inst.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                Material[] newMats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < newMats.Length; i++)
                {
                    int matIdx = Mathf.Min(i, customMats.Length - 1);
                    newMats[i] = customMats[matIdx] != null ? customMats[matIdx] : r.sharedMaterials[i];
                }
                r.sharedMaterials = newMats;
            }
        }

        Debug.Log($"[INSTANTIATED REAL MODEL V3] {objName} from {assetPath}");
        return inst;
    }
}
#endif
