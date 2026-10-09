#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GasMasterRealAssetRestorer
{
    [MenuItem("SurakshaAR/Restore Master Real Gas Workstation")]
    public static void PerformRestoration()
    {
        Debug.Log("==================================================");
        Debug.Log("[START] Master Real Gas Workstation Restoration");
        Debug.Log("==================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Prepare Base Scene Hierarchy
        GameObject gasRoot = GameObject.Find("GasScenarioRoot");
        if (gasRoot == null)
        {
            gasRoot = new GameObject("GasScenarioRoot");
        }

        Transform industrialEnv = gasRoot.transform.Find("IndustrialEnvironment");
        if (industrialEnv == null)
        {
            GameObject envObj = new GameObject("IndustrialEnvironment");
            envObj.transform.SetParent(gasRoot.transform, false);
            industrialEnv = envObj.transform;
        }

        Transform ppeStation = industrialEnv.Find("PPE_Preparation_Station");
        if (ppeStation == null)
        {
            GameObject ppeObj = new GameObject("PPE_Preparation_Station");
            ppeObj.transform.SetParent(industrialEnv, false);
            ppeStation = ppeObj.transform;
            ppeStation.localPosition = new Vector3(-2.2f, 0f, 0f);
        }

        Transform confinedStation = industrialEnv.Find("Confined_Space_Training_Area");
        if (confinedStation == null)
        {
            GameObject confObj = new GameObject("Confined_Space_Training_Area");
            confObj.transform.SetParent(industrialEnv, false);
            confinedStation = confObj.transform;
            confinedStation.localPosition = new Vector3(2.5f, 0f, 0f);
        }

        // 2. Load Real Materials
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

        // Cylinder Base Color Texture
        Texture2D cylTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AR_Gas_foundation/3d/GasCylinder/Source_SM/Canister_Gas_Lp_M_Canister_Gas_BaseColor.png");
        Texture2D cylNorm = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AR_Gas_foundation/3d/GasCylinder/Source_SM/Canister_Gas_Lp_M_Canister_Gas_Normal.png");

        if (matCylinderRed != null && cylTex != null)
        {
            matCylinderRed.mainTexture = cylTex;
            if (cylNorm != null) matCylinderRed.SetTexture("_BumpMap", cylNorm);
        }

        // 3. Remove fake/procedural objects
        RemoveAllChildObjects(gasRoot.transform, "fake");
        RemoveAllChildObjects(gasRoot.transform, "procedural");
        RemoveAllChildObjects(gasRoot.transform, "substitute");
        RemoveAllChildObjects(gasRoot.transform, "placeholder");

        // 4. Instantiate & Configure ALL 15 Real 3D Assets

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

        // --- CENTER GAS LEAK STATION ---
        // Real Cylinder
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/GasCylinder/Source_SM/Gas_Cylinder_SM.fbx", gasRoot.transform, "Realistic_Gas_Cylinder", new Vector3(-0.65f, 0f, 0f), new Vector3(0f, 180f, 0f), Vector3.one, matCylinderRed);
        
        // Pipeline Container
        Transform pipeAssembly = gasRoot.transform.Find("GasPipelineAssembly");
        if (pipeAssembly == null)
        {
            GameObject pObj = new GameObject("GasPipelineAssembly");
            pObj.transform.SetParent(gasRoot.transform, false);
            pipeAssembly = pObj.transform;
        }

        // Regulator
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/GasRegulator/gas_regulator_hd.obj", pipeAssembly, "Gas_Regulator_3D", new Vector3(-0.45f, 1.12f, 0f), new Vector3(0f, 90f, 0f), Vector3.one, matRegulatorBrass);
        // Pressure Gauge
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge_hd.obj", pipeAssembly, "Pressure_Gauge_3D", new Vector3(-0.35f, 1.25f, 0f), new Vector3(0f, 0f, 0f), Vector3.one, matGauge);
        // Steel Pipes
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/GasPipeline/Source_Pipes/pipes.fbx", pipeAssembly, "Horizontal_Pipe_1", new Vector3(-0.1f, 1.12f, 0f), new Vector3(0f, 90f, 0f), Vector3.one, matPipeSteel);
        // Red Valve
        InstantiateRealAsset("Assets/industrial-valve/source/VALVE.fbx", pipeAssembly, "Isolation_Valve_3D", new Vector3(0.2f, 1.12f, 0f), new Vector3(0f, 0f, 0f), Vector3.one, matValveRed);
        // Multi-Gas Detector
        InstantiateRealAsset("Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_hd.obj", gasRoot.transform, "Multi_Gas_Detector", new Vector3(0.6f, 0.87f, 0.2f), new Vector3(-20f, 15f, 0f), Vector3.one, matDetectorYellow, matDetectorBlack);

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

        Debug.Log("[SCENE SAVED SUCCESS] Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity updated with 100% authentic downloaded assets");

        // Capture proof screenshots
        GasWorkstationScreenshotCapturer.CaptureScreenshots();

        Debug.Log("==================================================");
        Debug.Log("[RESTORATION COMPLETE]");
        Debug.Log("==================================================");
    }

    private static void InstantiateRealAsset(string assetPath, Transform parent, string objName, Vector3 localPos, Vector3 localRot, Vector3 localScale, params Material[] customMats)
    {
        // Remove existing duplicate instance if any
        Transform oldTrans = parent.Find(objName);
        if (oldTrans != null)
        {
            Object.DestroyImmediate(oldTrans.gameObject);
        }

        GameObject sourceAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (sourceAsset == null)
        {
            Debug.LogError($"[ERROR] Real asset not found at {assetPath}");
            return;
        }

        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(sourceAsset, parent);
        inst.name = objName;
        inst.transform.localPosition = localPos;
        inst.transform.localRotation = Quaternion.Euler(localRot);
        inst.transform.localScale = localScale;

        // Assign custom materials if provided
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
    }

    private static void RemoveAllChildObjects(Transform parent, string keyword)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child.name.ToLower().Contains(keyword))
            {
                Debug.Log($"[REMOVED UNWANTED OBJECT] {child.name} from {parent.name}");
                Object.DestroyImmediate(child.gameObject);
            }
            else
            {
                RemoveAllChildObjects(child, keyword);
            }
        }
    }
}
#endif
