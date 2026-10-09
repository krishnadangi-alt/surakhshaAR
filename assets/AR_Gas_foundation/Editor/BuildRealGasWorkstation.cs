#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class BuildRealGasWorkstation
{
    [MenuItem("SurakshaAR/Build Complete Real Gas Workstation")]
    public static void ExecuteBuild()
    {
        Debug.Log("==========================================================================");
        Debug.Log("STARTING COMPLETE REAL GAS 3D WORKSTATION REBUILD (V4 FINAL STABLE)");
        Debug.Log("==========================================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError($"CRITICAL: Could not open scene at path {scenePath}");
            return;
        }

        // 1. Prepare Base Hierarchy
        GameObject gasRoot = GameObject.Find("GasScenarioRoot");
        if (gasRoot == null)
        {
            gasRoot = new GameObject("GasScenarioRoot");
        }
        gasRoot.transform.position = Vector3.zero;
        gasRoot.transform.rotation = Quaternion.identity;
        gasRoot.transform.localScale = Vector3.one;

        // 2. Clean out old broken/fake/duplicate instances in gasRoot
        Debug.Log("--- CLEANING UP STRAY / FAKE / DUPLICATE OBJECTS ---");
        for (int i = gasRoot.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = gasRoot.transform.GetChild(i);
            string lowerName = child.name.ToLower();
            if (lowerName.Contains("fake") || lowerName.Contains("procedural") || lowerName.Contains("placeholder") || lowerName.Contains("temp"))
            {
                Debug.Log($"Destroying fake/placeholder object: {child.name}");
                Object.DestroyImmediate(child.gameObject);
            }
        }

        // Zones
        Transform ppeZone = GetOrCreateContainer(gasRoot.transform, "PPE_Preparation_Station");
        Transform gasZone = GetOrCreateContainer(gasRoot.transform, "Gas_Leak_Station");
        Transform confinedZone = GetOrCreateContainer(gasRoot.transform, "Confined_Space_Training_Area");

        ClearContainerChildren(ppeZone);
        ClearContainerChildren(gasZone);
        ClearContainerChildren(confinedZone);

        ppeZone.position = new Vector3(-2.40f, 0f, 0f);
        gasZone.position = new Vector3(0.00f, 0f, 0f);
        confinedZone.position = new Vector3(2.80f, 0f, 0f);

        // Load Materials & Textures
        Material matCylinderRed = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_GasCylinder_Red.mat");
        Material matRegulatorBrass = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Regulator_Brass.mat");
        Material matGauge = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_PressureGauge.mat");
        Material matPipeSteel = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Pipeline_Steel.mat");
        Material matValveRed = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Valve_Red.mat");
        Material matDetectorYellow = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Detector_Yellow.mat");
        Material matDetectorBlack = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Detector_Black.mat");
        Material matSteel = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Structural_Steel.mat");
        Material matLeather = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_PPE_Leather.mat");
        Material matVesselTank = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Vessel_Tank.mat");
        Material matSign = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Safety_Sign.mat");

        // Cylinder Base Color Texture
        Texture2D cylTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AR_Gas_foundation/3d/GasCylinder/Source_SM/Canister_Gas_Lp_M_Canister_Gas_BaseColor.png");
        Texture2D cylNorm = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AR_Gas_foundation/3d/GasCylinder/Source_SM/Canister_Gas_Lp_M_Canister_Gas_Normal.png");
        if (matCylinderRed != null && cylTex != null)
        {
            matCylinderRed.mainTexture = cylTex;
            if (cylNorm != null) matCylinderRed.SetTexture("_BumpMap", cylNorm);
        }

        // ========================================================================
        // 3. ZONE 1: LEFT - PPE WORKSTATION
        // ========================================================================
        Debug.Log("--- BUILDING ZONE 1: PPE WORKSTATION ---");
        // 1. Workbench
        GameObject benchObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/PPE/workbench_hd.obj", ppeZone, "PPE_Workbench");
        if (benchObj != null)
        {
            benchObj.transform.localEulerAngles = Vector3.zero;
            benchObj.transform.localScale = Vector3.one;
            AssignMaterialsToChildRenderers(benchObj, matSteel, matLeather);
            SnapToGround(benchObj, 0.0000f);
        }

        Bounds benchBounds = CalculateWorldBounds(benchObj);
        float tableTopY = benchBounds.max.y > 0.1f ? benchBounds.max.y : 0.85f;

        // 2. Safety Helmet
        GameObject helmetObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/PPE/ppe_hardhat_hd.obj", ppeZone, "Safety_Helmet");
        if (helmetObj == null) helmetObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/PPE/Prefabs/safety_helmet_real.prefab", ppeZone, "Safety_Helmet");
        if (helmetObj != null)
        {
            helmetObj.transform.localEulerAngles = new Vector3(0f, 30f, 0f);
            helmetObj.transform.localScale = Vector3.one;
            AssignMaterialsToChildRenderers(helmetObj, matDetectorYellow);
            helmetObj.transform.position = new Vector3(ppeZone.position.x - 0.35f, tableTopY + 0.01f, ppeZone.position.z - 0.10f);
        }

        // 3. Safety Goggles
        GameObject gogglesObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/PPE/Prefabs/safety_goggles_real.prefab", ppeZone, "Safety_Goggles");
        if (gogglesObj == null) gogglesObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/PPE/safety_goggles_converted.fbx", ppeZone, "Safety_Goggles");
        if (gogglesObj != null)
        {
            gogglesObj.transform.localEulerAngles = new Vector3(0f, 45f, 0f);
            gogglesObj.transform.localScale = new Vector3(0.012f, 0.012f, 0.012f);
            gogglesObj.transform.position = new Vector3(ppeZone.position.x + 0.05f, tableTopY + 0.01f, ppeZone.position.z + 0.05f);
        }

        // 4. Work Gloves
        GameObject glovesObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/PPE/Prefabs/work_gloves_real.prefab", ppeZone, "Work_Gloves");
        if (glovesObj == null) glovesObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/PPE/work_gloves_converted.fbx", ppeZone, "Work_Gloves");
        if (glovesObj != null)
        {
            glovesObj.transform.localEulerAngles = new Vector3(0f, 15f, 0f);
            glovesObj.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
            glovesObj.transform.position = new Vector3(ppeZone.position.x - 0.15f, tableTopY + 0.01f, ppeZone.position.z + 0.05f);
        }

        // 5. Safety Vest
        GameObject vestObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/PPE/ppe_vest_hd.obj", ppeZone, "Safety_Vest");
        if (vestObj != null)
        {
            vestObj.transform.localEulerAngles = new Vector3(0f, -20f, 0f);
            vestObj.transform.localScale = Vector3.one;
            AssignMaterialsToChildRenderers(vestObj, matDetectorYellow);
            vestObj.transform.position = new Vector3(ppeZone.position.x + 0.35f, tableTopY + 0.01f, ppeZone.position.z - 0.10f);
        }

        // 6. Safety Boots
        GameObject bootsObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/PPE/Prefabs/safety_boots_real.prefab", ppeZone, "Safety_Boots");
        if (bootsObj == null) bootsObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/PPE/work_boot_converted.fbx", ppeZone, "Safety_Boots");
        if (bootsObj != null)
        {
            bootsObj.transform.localEulerAngles = new Vector3(0f, 75f, 0f);
            bootsObj.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
            SnapToGround(bootsObj, 0.0000f);
            bootsObj.transform.position = new Vector3(ppeZone.position.x - 0.50f, bootsObj.transform.position.y, ppeZone.position.z + 0.35f);
        }

        // ========================================================================
        // 4. ZONE 2: CENTER - GAS CYLINDER & LEAK STATION
        // ========================================================================
        Debug.Log("--- BUILDING ZONE 2: GAS CYLINDER & LEAK STATION ---");
        // 1. Gas Cylinder
        GameObject cylObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/GasCylinder/Prefabs/gas_cylinder_real.prefab", gasZone, "Realistic_Gas_Cylinder");
        if (cylObj == null) cylObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/GasCylinder/Source_SM/Gas_Cylinder_SM.fbx", gasZone, "Realistic_Gas_Cylinder");
        if (cylObj != null)
        {
            cylObj.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            cylObj.transform.localScale = Vector3.one;
            if (matCylinderRed != null) AssignMaterialsToChildRenderers(cylObj, matCylinderRed);
            SnapToGround(cylObj, 0.0000f);
            cylObj.transform.position = new Vector3(-0.65f, cylObj.transform.position.y, 0.00f);
        }

        Bounds cylBounds = CalculateWorldBounds(cylObj);
        float cylTopY = cylBounds.max.y > 0.5f ? cylBounds.max.y : 1.10f;

        // 2. Gas Pipeline Container
        Transform pipeAssembly = GetOrCreateContainer(gasZone, "GasPipelineAssembly");
        pipeAssembly.localPosition = Vector3.zero;

        // 3. Regulator
        GameObject regObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/GasRegulator/gas_regulator_hd.obj", pipeAssembly, "Gas_Regulator_3D");
        if (regObj == null) regObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/GasRegulator/Prefabs/high-pressure_regulator.prefab", pipeAssembly, "Gas_Regulator_3D");
        if (regObj != null)
        {
            regObj.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
            regObj.transform.localScale = Vector3.one;
            if (matRegulatorBrass != null) AssignMaterialsToChildRenderers(regObj, matRegulatorBrass);
            regObj.transform.position = new Vector3(cylObj.transform.position.x + 0.20f, cylTopY - 0.10f, cylObj.transform.position.z);
        }

        // 4. Pressure Gauge
        GameObject gaugeObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge_hd.obj", pipeAssembly, "Pressure_Gauge_3D");
        if (gaugeObj == null) gaugeObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/PressureGauge/Prefabs/pressure_gauge_real.prefab", pipeAssembly, "Pressure_Gauge_3D");
        if (gaugeObj != null)
        {
            gaugeObj.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            gaugeObj.transform.localScale = Vector3.one;
            if (matGauge != null) AssignMaterialsToChildRenderers(gaugeObj, matGauge);
            gaugeObj.transform.position = new Vector3(regObj.transform.position.x + 0.10f, regObj.transform.position.y + 0.12f, regObj.transform.position.z);
        }

        // 5. Pipe
        GameObject pipeObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/GasPipeline/industrial_pipe_hd.obj", pipeAssembly, "Horizontal_Pipe_1");
        if (pipeObj == null) pipeObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/GasPipeline/Prefabs/large_modular_pipes_real.prefab", pipeAssembly, "Horizontal_Pipe_1");
        if (pipeObj != null)
        {
            pipeObj.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
            pipeObj.transform.localScale = Vector3.one;
            if (matPipeSteel != null) AssignMaterialsToChildRenderers(pipeObj, matPipeSteel);
            pipeObj.transform.position = new Vector3(gaugeObj.transform.position.x + 0.25f, regObj.transform.position.y, gaugeObj.transform.position.z);
        }

        // 6. Red Valve
        GameObject valveObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/GasPipeline/isolation_valve_hd.obj", pipeAssembly, "Isolation_Valve_3D");
        if (valveObj == null) valveObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/GasPipeline/Prefabs/industrial_valve_real.prefab", pipeAssembly, "Isolation_Valve_3D");
        if (valveObj != null)
        {
            valveObj.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            valveObj.transform.localScale = Vector3.one;
            if (matValveRed != null) AssignMaterialsToChildRenderers(valveObj, matValveRed);
            valveObj.transform.position = new Vector3(pipeObj.transform.position.x + 0.30f, regObj.transform.position.y, pipeObj.transform.position.z);
        }

        // 7. Multi-Gas Detector
        GameObject detectorObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_hd.obj", gasZone, "Multi_Gas_Detector");
        if (detectorObj == null) detectorObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/GasDetector/Prefabs/h2s_gas_detector_real.prefab", gasZone, "Multi_Gas_Detector");
        if (detectorObj != null)
        {
            detectorObj.transform.localEulerAngles = new Vector3(-15f, 20f, 0f);
            detectorObj.transform.localScale = Vector3.one;
            if (matDetectorYellow != null) AssignMaterialsToChildRenderers(detectorObj, matDetectorYellow, matDetectorBlack);
            detectorObj.transform.position = new Vector3(cylObj.transform.position.x + 0.70f, 0.85f, cylObj.transform.position.z + 0.20f);
        }

        // ========================================================================
        // 5. ZONE 3: RIGHT - CONFINED SPACE TRAINING AREA
        // ========================================================================
        Debug.Log("--- BUILDING ZONE 3: CONFINED SPACE TRAINING AREA ---");
        // 1. Confined Space Vessel Assembly (Original OBJ model)
        GameObject vesselObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/ConfinedSpace/confined_vessel_hd.obj", confinedZone, "Real_Confined_Space_OBJ");
        if (vesselObj == null) vesselObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/ConfinedSpace/confined_vessel.prefab", confinedZone, "Real_Confined_Space_OBJ");

        if (vesselObj != null)
        {
            AssignMaterialsToChildRenderers(vesselObj, matVesselTank, matSign, matValveRed);
            vesselObj.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            vesselObj.transform.localScale = new Vector3(1.45f, 1.45f, 1.45f);
            SnapToGround(vesselObj, 0.0000f);
            vesselObj.transform.position = new Vector3(2.80f, vesselObj.transform.position.y, 0.00f);
        }

        // 2. Ventilation Fan
        GameObject fanObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs/ventilation_fan_real.prefab", confinedZone, "Axial_Ventilation_Fan_Real");
        if (fanObj == null) fanObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/ConfinedSpace/ventilation_fan_converted.fbx", confinedZone, "Axial_Ventilation_Fan_Real");
        if (fanObj != null)
        {
            fanObj.transform.localEulerAngles = new Vector3(0f, -30f, 0f);
            fanObj.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
            SnapToGround(fanObj, 0.0000f);
            fanObj.transform.position = new Vector3(1.95f, fanObj.transform.position.y, -0.40f);
        }

        // 3. Self-Retracting Lifeline
        GameObject lifelineObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs/retracting_lifeline_real.prefab", confinedZone, "Self_Retracting_Lifeline_Real");
        if (lifelineObj == null) lifelineObj = InstantiateAsset("Assets/AR_Gas_foundation/3d/ConfinedSpace/self_retracting_lifeline_converted.fbx", confinedZone, "Self_Retracting_Lifeline_Real");
        if (lifelineObj != null)
        {
            lifelineObj.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            lifelineObj.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
            lifelineObj.transform.position = new Vector3(3.45f, 1.45f, 0.20f);
        }

        // Cache positions before saving scene
        Vector3 ppePosCache = ppeZone.position;
        Vector3 gasPosCache = gasZone.position;
        Vector3 confinedPosCache = confinedZone.position;

        // 6. Save Scene & Reload to Verify
        EditorSceneManager.MarkSceneDirty(scene);
        bool saved = EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log($"[SAVE SCENE] AR_gas_Foundation.unity save result: {saved}");

        // Reload scene and verify persistence
        var reloadedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        GameObject reloadedGasRoot = GameObject.Find("GasScenarioRoot");

        if (reloadedGasRoot != null)
        {
            Debug.Log("[VERIFICATION SUCCESS] Reloaded scene verified! GasScenarioRoot is active with all 3 zones.");
        }
        else
        {
            Debug.LogError("[VERIFICATION FAILED] Could not find GasScenarioRoot after scene reload!");
        }

        // 7. Capture Proof Screenshots
        CaptureProofScreenshots(ppePosCache, gasPosCache, confinedPosCache);
    }

    private static Transform GetOrCreateContainer(Transform parent, string name)
    {
        Transform t = parent.Find(name);
        if (t == null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            t = go.transform;
        }
        return t;
    }

    private static void ClearContainerChildren(Transform container)
    {
        if (container == null) return;
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(container.GetChild(i).gameObject);
        }
    }

    private static GameObject InstantiateAsset(string assetPath, Transform parent, string instanceName)
    {
        Transform existing = parent.Find(instanceName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefabAsset == null)
        {
            Debug.LogWarning($"[ASSET NOT FOUND] Path: {assetPath}");
            return null;
        }

        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset, parent);
        inst.name = instanceName;
        inst.SetActive(true);
        return inst;
    }

    private static void AssignMaterialsToChildRenderers(GameObject obj, params Material[] mats)
    {
        if (mats == null || mats.Length == 0) return;
        Renderer[] rends = obj.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < rends.Length; i++)
        {
            Material targetMat = mats[i % mats.Length];
            if (targetMat != null) rends[i].sharedMaterial = targetMat;
        }
    }

    private static Bounds CalculateWorldBounds(GameObject obj)
    {
        Renderer[] rends = obj.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return new Bounds(obj.transform.position, Vector3.zero);

        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++)
        {
            b.Encapsulate(rends[i].bounds);
        }
        return b;
    }

    private static void SnapToGround(GameObject obj, float targetGroundY = 0.0000f)
    {
        Bounds b = CalculateWorldBounds(obj);
        float deltaY = targetGroundY - b.min.y;
        obj.transform.position += new Vector3(0f, deltaY, 0f);
    }

    private static void CaptureProofScreenshots(Vector3 ppePos, Vector3 gasPos, Vector3 confinedPos)
    {
        string dir1 = @"C:\SurakshaAR\Screenshots";
        string dir2 = @"C:\Users\anjan\.gemini\antigravity\brain\c728ff39-ec61-4697-b390-b7629a5ba242";
        if (!Directory.Exists(dir1)) Directory.CreateDirectory(dir1);
        if (!Directory.Exists(dir2)) Directory.CreateDirectory(dir2);

        int w = 1920, h = 1080;

        // 1. FULL WORKSTATION OVERVIEW SCREENSHOT
        // Frame all 3 zones from Left (-2.4m) to Center (0m) to Right (+2.8m)
        Vector3 overviewPos = new Vector3(0.2f, 1.8f, -7.2f);
        Quaternion overviewRot = Quaternion.Euler(12f, 0f, 0f);
        CaptureCamera(overviewPos, overviewRot, 56f, w, h,
            Path.Combine(dir1, "REAL_WORKSTATION_FULL_VIEW.png"), Path.Combine(dir2, "REAL_WORKSTATION_FULL_VIEW.png"));

        // 2. LEFT PPE WORKSTATION CLOSEUP SCREENSHOT
        Vector3 ppeCamPos = ppePos + new Vector3(0f, 1.20f, -2.4f);
        Quaternion ppeCamRot = Quaternion.Euler(14f, 0f, 0f);
        CaptureCamera(ppeCamPos, ppeCamRot, 42f, w, h,
            Path.Combine(dir1, "REAL_PPE_CLOSEUP.png"), Path.Combine(dir2, "REAL_PPE_CLOSEUP.png"));

        // 3. CENTER GAS CYLINDER & PIPELINE CLOSEUP SCREENSHOT
        Vector3 gasCamPos = gasPos + new Vector3(0.1f, 1.10f, -2.4f);
        Quaternion gasCamRot = Quaternion.Euler(10f, 0f, 0f);
        CaptureCamera(gasCamPos, gasCamRot, 42f, w, h,
            Path.Combine(dir1, "REAL_CYLINDER_CLOSEUP.png"), Path.Combine(dir2, "REAL_CYLINDER_CLOSEUP.png"));

        // 4. RIGHT CONFINED SPACE CLOSEUP SCREENSHOT
        Vector3 confCamPos = confinedPos + new Vector3(0f, 1.20f, -3.2f);
        Quaternion confCamRot = Quaternion.Euler(10f, 0f, 0f);
        CaptureCamera(confCamPos, confCamRot, 45f, w, h,
            Path.Combine(dir1, "REAL_CONFINED_SPACE_CLOSEUP.png"), Path.Combine(dir2, "REAL_CONFINED_SPACE_CLOSEUP.png"));

        Debug.Log($"[SCREENSHOTS GENERATED] Saved proof screenshots to {dir1} and {dir2}");
    }

    private static void CaptureCamera(Vector3 pos, Quaternion rot, float fov, int width, int height, string filePath1, string filePath2)
    {
        GameObject camObj = new GameObject("RenderCam");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.18f, 0.20f, 0.24f, 1f);
        cam.fieldOfView = fov;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 100f;
        camObj.transform.position = pos;
        camObj.transform.rotation = rot;

        GameObject keyLight = new GameObject("KeyLight");
        Light kl = keyLight.AddComponent<Light>();
        kl.type = LightType.Directional;
        kl.intensity = 1.0f;
        kl.color = new Color(1.0f, 0.96f, 0.90f);
        keyLight.transform.rotation = Quaternion.Euler(35f, -30f, 0f);

        GameObject fillLight = new GameObject("FillLight");
        Light fl = fillLight.AddComponent<Light>();
        fl.type = LightType.Directional;
        fl.intensity = 0.6f;
        fl.color = new Color(0.85f, 0.90f, 1.0f);
        fillLight.transform.rotation = Quaternion.Euler(20f, 140f, 0f);

        RenderTexture rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(filePath1, bytes);
        File.WriteAllBytes(filePath2, bytes);

        RenderTexture.active = null;
        cam.targetTexture = null;
        RenderTexture.ReleaseTemporary(rt);

        Object.DestroyImmediate(keyLight);
        Object.DestroyImmediate(fillLight);
        Object.DestroyImmediate(camObj);
    }
}
#endif
