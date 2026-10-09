#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GasMasterRealAssetRestorerV2
{
    [MenuItem("SurakshaAR/Restore Master Real Gas Workstation V2")]
    public static void PerformRestoration()
    {
        Debug.Log("==================================================");
        Debug.Log("[START] Master Real Gas Workstation Restoration V2");
        Debug.Log("==================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Base Hierarchy Setup
        GameObject gasRoot = GameObject.Find("GasScenarioRoot");
        if (gasRoot == null)
        {
            gasRoot = new GameObject("GasScenarioRoot");
        }

        // Clean out any existing children in gasRoot to guarantee zero legacy/fake objects
        for (int i = gasRoot.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(gasRoot.transform.GetChild(i).gameObject);
        }

        Transform industrialEnv = new GameObject("IndustrialEnvironment").transform;
        industrialEnv.SetParent(gasRoot.transform, false);

        Transform ppeStation = new GameObject("PPE_Preparation_Station").transform;
        ppeStation.SetParent(industrialEnv, false);
        ppeStation.localPosition = new Vector3(-2.2f, 0f, 0f);

        Transform confinedStation = new GameObject("Confined_Space_Training_Area").transform;
        confinedStation.SetParent(industrialEnv, false);
        confinedStation.localPosition = new Vector3(2.5f, 0f, 0f);

        Transform pipeAssembly = new GameObject("GasPipelineAssembly").transform;
        pipeAssembly.SetParent(gasRoot.transform, false);

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

        // 3. Instantiate REAL Assets with Proportional Scale & Seamless Physical Alignment

        // --- CENTER GAS LEAK STATION ---
        // REAL Industrial Gas Cylinder (Reference Object for Scale, standing on floor at y=0)
        Vector3 cylPos = new Vector3(-0.65f, 0f, 0f);
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/GasCylinder/Source_SM/Gas_Cylinder_SM.fbx", gasRoot.transform, "Realistic_Gas_Cylinder", cylPos, new Vector3(0f, 180f, 0f), Vector3.one, matCylinderRed);

        pipeAssembly.localPosition = new Vector3(0f, 0f, 0f);

        // REAL Regulator physically sitting on top outlet valve of cylinder
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/GasRegulator/gas_regulator_hd.obj", pipeAssembly, "Gas_Regulator_3D", new Vector3(-0.65f, 0.40f, 0f), new Vector3(0f, 90f, 0f), new Vector3(0.5f, 0.5f, 0.5f), matRegulatorBrass);

        // REAL Pressure Gauge sitting directly on top port of regulator
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge_hd.obj", pipeAssembly, "Pressure_Gauge_3D", new Vector3(-0.65f, 0.47f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0.4f, 0.4f, 0.4f), matGauge);

        // REAL Steel Connected Pipes (Scaled down to match regulator outlet ~8cm outer pipe diameter)
        Vector3 pipeScale = new Vector3(0.08f, 0.08f, 0.08f);
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/GasPipeline/Source_Pipes/pipes.fbx", pipeAssembly, "Horizontal_Pipe_1", new Vector3(-0.65f, 0.20f, 0f), new Vector3(0f, 90f, 0f), pipeScale, matPipeSteel);

        // REAL Red Isolation Valve (Proportionally scaled inline on the pipe network)
        Vector3 valveScale = new Vector3(0.04f, 0.04f, 0.04f);
        InstantiateRealAsset("Assets/industrial-valve/source/VALVE.fbx", pipeAssembly, "Isolation_Valve_3D", new Vector3(-0.40f, 0.20f, 0f), new Vector3(0f, 0f, 0f), valveScale, matValveRed);

        // REAL Multi-Gas Detector placed resting flat on floor beside gas cylinder
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_hd.obj", gasRoot.transform, "Multi_Gas_Detector", new Vector3(-0.35f, 0.02f, 0.25f), new Vector3(-90f, 30f, 0f), new Vector3(0.6f, 0.6f, 0.6f), matDetectorYellow, matDetectorBlack);

        // --- LEFT PPE STATION ---
        // Workbench
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/PPE/workbench_hd.obj", ppeStation, "PPE_Workbench", new Vector3(0f, 0.4f, 0f), new Vector3(0f, 0f, 0f), Vector3.one, matSteel, matLeather);
        // Helmet
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/PPE/ppe_hardhat_hd.obj", ppeStation, "Safety_Helmet", new Vector3(-0.35f, 0.88f, -0.1f), new Vector3(0f, 30f, 0f), Vector3.one, matDetectorYellow);
        // Vest
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/PPE/ppe_vest_hd.obj", ppeStation, "Safety_Vest", new Vector3(0.35f, 0.88f, -0.1f), new Vector3(0f, -20f, 0f), Vector3.one, matDetectorYellow);
        // Goggles
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/PPE/safety_goggles_converted.fbx", ppeStation, "Safety_Goggles_Real", new Vector3(0f, 0.87f, 0.05f), new Vector3(0f, 45f, 0f), new Vector3(0.012f, 0.012f, 0.012f));
        // Gloves
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/PPE/work_gloves_converted.fbx", ppeStation, "Work_Gloves_Real", new Vector3(-0.15f, 0.87f, 0.05f), new Vector3(0f, 15f, 0f), new Vector3(0.015f, 0.015f, 0.015f));
        // Boots
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/PPE/work_boot_converted.fbx", ppeStation, "Safety_Boots_Real", new Vector3(-0.45f, 0.02f, 0.35f), new Vector3(0f, 75f, 0f), new Vector3(0.015f, 0.015f, 0.015f));

        // --- RIGHT CONFINED SPACE STATION ---
        // Confined Space Vessel
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/ConfinedSpace/confined_vessel_hd.obj", confinedStation, "Industrial_Tanks_And_Equipment", new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), Vector3.one, matVessel);
        // Ventilation Fan
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/ConfinedSpace/ventilation_fan_converted.fbx", confinedStation, "Axial_Ventilation_Fan_Real", new Vector3(-0.85f, 0.02f, -0.4f), new Vector3(0f, -30f, 0f), new Vector3(0.015f, 0.015f, 0.015f));
        // Lifeline
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/ConfinedSpace/self_retracting_lifeline_converted.fbx", confinedStation, "Self_Retracting_Lifeline_Real", new Vector3(0.65f, 1.45f, 0.2f), new Vector3(0f, 0f, 0f), new Vector3(0.015f, 0.015f, 0.015f));

        // Save scene & assets
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("[SCENE SAVED SUCCESS] Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity updated with seamless physical connections!");

        // Capture proof screenshots V2
        GasWorkstationScreenshotCapturerV2.CaptureAllScreenshots();

        Debug.Log("==================================================");
        Debug.Log("[RESTORATION V2 COMPLETE]");
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

        Debug.Log($"[INSTANTIATED REAL MODEL] {objName} from {assetPath}");
        return inst;
    }
}
#endif
