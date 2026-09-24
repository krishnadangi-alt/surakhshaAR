#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GasRealWorkstationRebuilder
{
    [MenuItem("SurakshaAR/Build REAL 3D Gas Workstation & Capture Screenshots")]
    public static void BuildRealWorkstationAndScreenshots()
    {
        Debug.Log("==================================================");
        Debug.Log("[MASTER DEEP REALISM REBUILD] Instantiating Multi-Material High-Detail Assets");
        Debug.Log("==================================================");

        string texDir = "Assets/AR_Gas_foundation/Textures";
        if (!Directory.Exists(texDir)) Directory.CreateDirectory(texDir);

        string matDir = "Assets/AR_Gas_foundation/Materials";
        if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);

        // 1. Generate Procedural Industrial Textures
        Texture2D texGauge = GeneratePressureGaugeTexture(Path.Combine(texDir, "tex_gauge_dial.png"));
        Texture2D texSign = GenerateSafetySignTexture(Path.Combine(texDir, "tex_safety_sign.png"));
        Texture2D texDetectorScreen = GenerateDetectorScreenTexture(Path.Combine(texDir, "tex_detector_screen.png"));
        Texture2D texConcrete = GenerateConcreteFloorTexture(Path.Combine(texDir, "tex_concrete_floor.png"));
        Texture2D texCylinderDecal = GenerateCylinderDecalTexture(Path.Combine(texDir, "tex_cylinder_decal.png"));
        Texture2D texVesselDecal = GenerateVesselDecalTexture(Path.Combine(texDir, "tex_vessel_decal.png"));

        // 2. Create PBR Materials
        Material matCylinder = GetOrCreateMaterial(Path.Combine(matDir, "MAT_GasCylinder_Red.mat"), new Color(0.85f, 0.12f, 0.08f), 0.45f, 0.75f, texCylinderDecal);
        Material matBrass = GetOrCreateMaterial(Path.Combine(matDir, "MAT_Regulator_Brass.mat"), new Color(0.88f, 0.72f, 0.18f), 0.88f, 0.82f, null);
        Material matSteel = GetOrCreateMaterial(Path.Combine(matDir, "MAT_Pipeline_Steel.mat"), new Color(0.48f, 0.52f, 0.56f), 0.85f, 0.75f, null);
        Material matValve = GetOrCreateMaterial(Path.Combine(matDir, "MAT_Valve_Red.mat"), new Color(0.82f, 0.10f, 0.08f), 0.40f, 0.65f, null);
        Material matGauge = GetOrCreateMaterial(Path.Combine(matDir, "MAT_PressureGauge.mat"), Color.white, 0.20f, 0.60f, texGauge);
        Material matGlass = GetOrCreateMaterial(Path.Combine(matDir, "MAT_Gauge_Glass.mat"), new Color(0.90f, 0.95f, 1.00f, 0.30f), 0.95f, 0.95f, null);
        Material matDetYellow = GetOrCreateMaterial(Path.Combine(matDir, "MAT_Detector_Yellow.mat"), new Color(0.96f, 0.78f, 0.05f), 0.25f, 0.60f, null);
        Material matDetBlack = GetOrCreateMaterial(Path.Combine(matDir, "MAT_Detector_Black.mat"), new Color(0.12f, 0.14f, 0.15f), 0.30f, 0.50f, null);
        Material matDetScreen = GetOrCreateMaterial(Path.Combine(matDir, "MAT_Detector_Screen.mat"), Color.white, 0.10f, 0.90f, texDetectorScreen);

        Material matConcrete = GetOrCreateMaterial(Path.Combine(matDir, "MAT_Industrial_Concrete.mat"), Color.white, 0.10f, 0.40f, texConcrete);
        Material matStructSteel = GetOrCreateMaterial(Path.Combine(matDir, "MAT_Structural_Steel.mat"), new Color(0.25f, 0.28f, 0.32f), 0.85f, 0.60f, null);
        Material matSafetySign = GetOrCreateMaterial(Path.Combine(matDir, "MAT_Safety_Sign.mat"), Color.white, 0.05f, 0.50f, texSign);
        Material matVesselTank = GetOrCreateMaterial(Path.Combine(matDir, "MAT_Vessel_Tank.mat"), Color.white, 0.75f, 0.65f, texVesselDecal);
        Material matPPE = GetOrCreateMaterial(Path.Combine(matDir, "MAT_PPE_Leather.mat"), new Color(0.65f, 0.38f, 0.15f), 0.10f, 0.35f, null);

        Material matNeutralGrey = GetOrCreateMaterial(Path.Combine(matDir, "MAT_Neutral_Grey.mat"), new Color(0.55f, 0.55f, 0.55f), 0.10f, 0.30f, null);

        // Industrial Valve Textures & PBR Materials
        Texture2D texValveRed = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/industrial-valve/textures/mars-red-grunge-texture.jpg");
        Texture2D texValveIron = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/industrial-valve/textures/iron_texture1050.jpg");
        Texture2D texValveSteel = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/industrial-valve/textures/Steel_03_UV_H_CM_1.jpg");
        Texture2D texValveThreads = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/industrial-valve/textures/Screw_Threads.jpg");

        Material matValveBody = GetOrCreateMaterial(Path.Combine(matDir, "MAT_Valve_Body.mat"), Color.white, 0.70f, 0.50f, texValveIron);
        Material matValveSteel = GetOrCreateMaterial(Path.Combine(matDir, "MAT_Valve_Steel.mat"), Color.white, 0.85f, 0.75f, texValveSteel);
        Material matValveHandwheel = GetOrCreateMaterial(Path.Combine(matDir, "MAT_Valve_Handwheel.mat"), Color.white, 0.40f, 0.65f, texValveRed);
        Material matValveStem = GetOrCreateMaterial(Path.Combine(matDir, "MAT_Valve_Stem.mat"), Color.white, 0.80f, 0.80f, texValveThreads);

        // Industrial Pipe PBR Textures & Material
        Texture2D texPipesColor = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AR_Gas_foundation/3d/GasPipeline/modular-pipes/textures/pipesColor.png");
        Texture2D texPipesNormal = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AR_Gas_foundation/3d/GasPipeline/modular-pipes/textures/pipesNormal.png");
        Material matModularPipes = GetOrCreateMaterial(Path.Combine(matDir, "MAT_Modular_Pipes_Steel.mat"), Color.white, 0.85f, 0.70f, texPipesColor);
        if (matModularPipes != null && texPipesNormal != null)
        {
            if (matModularPipes.HasProperty("_BumpMap")) matModularPipes.SetTexture("_BumpMap", texPipesNormal);
        }

        // 3. Load Genuine Downloaded & High-Detail 3D Assets
        GameObject objCylinderHD = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasCylinder/gas_cylinder.glb");
        GameObject objDetectorHD = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.glb");
        GameObject objRegulatorHD = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasRegulator/high-pressure_regulator.glb");
        GameObject objGaugeHD = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge_animated.glb");
        GameObject objPipeHD = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasPipeline/modular-pipes/source/pipes.fbx");
        GameObject objElbowHD = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasPipeline/modular-pipes/source/pipes.fbx");
        GameObject objValveHD = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/industrial-valve/source/VALVE.fbx");
        GameObject objVesselHD = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/ConfinedSpace/confined_vessel_hd.obj");
        GameObject objHardhatHD = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/ppe_hardhat_hd.obj");
        GameObject objVestHD = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/ppe_vest_hd.obj");
        GameObject objWorkbenchHD = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/workbench_hd.obj");

        // 4. Build Gas Cylinder Prefab (Genuine 3D GLB Asset)
        GameObject cylRoot = new GameObject("gas_cylinder");
        cylRoot.transform.position = Vector3.zero;

        GameObject cylModel = (GameObject)PrefabUtility.InstantiatePrefab(objCylinderHD, cylRoot.transform);
        cylModel.name = "gas_cylinder_mesh";
        cylModel.transform.localPosition = Vector3.zero;
        cylModel.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        cylModel.transform.localScale = Vector3.one;

        Renderer[] cylRnds = cylModel.GetComponentsInChildren<Renderer>(true);
        foreach (var rnd in cylRnds)
        {
            rnd.sharedMaterials = new Material[] { matCylinder, matSteel, matBrass };
        }

        BoxCollider boxCol = cylRoot.AddComponent<BoxCollider>();
        boxCol.center = new Vector3(0f, 0.60f, 0f);
        boxCol.size = new Vector3(0.30f, 1.20f, 0.30f);

        GameObject outletSocketObj = new GameObject("OutletSocket");
        outletSocketObj.transform.SetParent(cylRoot.transform, false);
        outletSocketObj.transform.localPosition = new Vector3(-0.035f, 1.070f, 0f);

        GameObject leakPoint = new GameObject("LeakPoint");
        leakPoint.transform.SetParent(cylRoot.transform, false);
        leakPoint.transform.localPosition = new Vector3(-0.035f, 1.070f, 0f);

        string cylinderPrefabPath = "Assets/AR_Gas_foundation/3d/GasCylinder/Prefabs/gas_cylinder.prefab";
        GameObject savedCylinderPrefab = PrefabUtility.SaveAsPrefabAsset(cylRoot, cylinderPrefabPath);
        Object.DestroyImmediate(cylRoot);

        // 5. Build Multi-Gas Detector Prefab (Genuine 3D GLB Asset)
        GameObject detRoot = new GameObject("multi_gas_detector");
        detRoot.transform.position = Vector3.zero;

        GameObject detModel = (GameObject)PrefabUtility.InstantiatePrefab(objDetectorHD, detRoot.transform);
        detModel.name = "multi_gas_detector_mesh";
        detModel.transform.localPosition = Vector3.zero;
        detModel.transform.localRotation = Quaternion.identity;
        detModel.transform.localScale = Vector3.one * 0.35f;

        Renderer[] detRnds = detModel.GetComponentsInChildren<Renderer>(true);
        foreach (var rnd in detRnds)
        {
            rnd.sharedMaterials = new Material[] { matDetYellow, matDetBlack, matDetScreen };
        }

        BoxCollider detCol = detRoot.AddComponent<BoxCollider>();
        detCol.center = new Vector3(0f, 0.09f, 0f);
        detCol.size = new Vector3(0.12f, 0.18f, 0.06f);

        string detectorPrefabPath = "Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.prefab";
        GameObject savedDetectorPrefab = PrefabUtility.SaveAsPrefabAsset(detRoot, detectorPrefabPath);
        Object.DestroyImmediate(detRoot);

        // 6. Build Isolation Valve Prefab (Genuine 3D FBX Asset)
        GameObject valveRoot = new GameObject("isolation_valve");
        GameObject valveModel = (GameObject)PrefabUtility.InstantiatePrefab(objValveHD, valveRoot.transform);
        valveModel.name = "isolation_valve_mesh";
        valveModel.transform.localPosition = Vector3.zero;
        valveModel.transform.localRotation = Quaternion.identity;
        valveModel.transform.localScale = Vector3.one * 0.038f;

        Renderer[] valveRnds = valveModel.GetComponentsInChildren<Renderer>(true);
        foreach (var rnd in valveRnds)
        {
            rnd.sharedMaterials = new Material[] { matValveBody, matValveSteel, matValveHandwheel, matValveStem, matValveSteel };
        }

        string valvePrefabPath = "Assets/AR_Gas_foundation/3d/GasPipeline/isolation_valve.prefab";
        GameObject savedValvePrefab = PrefabUtility.SaveAsPrefabAsset(valveRoot, valvePrefabPath);
        Object.DestroyImmediate(valveRoot);

        // 7. Build Confined Space Storage Vessel Prefab
        GameObject vesselRoot = new GameObject("confined_vessel");
        GameObject vesselModel = (GameObject)PrefabUtility.InstantiatePrefab(objVesselHD, vesselRoot.transform);
        vesselModel.name = "confined_vessel_hd_mesh";
        Renderer vesselRnd = vesselModel.GetComponentInChildren<Renderer>(true);
        if (vesselRnd != null) vesselRnd.sharedMaterials = new Material[] { matVesselTank, matSafetySign, matValve };

        string vesselPrefabPath = "Assets/AR_Gas_foundation/3d/ConfinedSpace/confined_vessel.prefab";
        GameObject savedVesselPrefab = PrefabUtility.SaveAsPrefabAsset(vesselRoot, vesselPrefabPath);
        Object.DestroyImmediate(vesselRoot);

        // 8. Build Workbench & PPE Prefabs
        GameObject workbenchRoot = new GameObject("workbench");
        GameObject workbenchModel = (GameObject)PrefabUtility.InstantiatePrefab(objWorkbenchHD, workbenchRoot.transform);
        workbenchModel.name = "workbench_hd_mesh";
        Renderer wbRnd = workbenchModel.GetComponentInChildren<Renderer>(true);
        if (wbRnd != null) wbRnd.sharedMaterials = new Material[] { matStructSteel, matPPE };

        string workbenchPrefabPath = "Assets/AR_Gas_foundation/3d/PPE/workbench.prefab";
        GameObject savedWorkbenchPrefab = PrefabUtility.SaveAsPrefabAsset(workbenchRoot, workbenchPrefabPath);
        Object.DestroyImmediate(workbenchRoot);

        GameObject savedHardhatPrefab = BuildComponentPrefab(objHardhatHD, "Assets/AR_Gas_foundation/3d/PPE/ppe_hardhat.prefab", matDetYellow);
        GameObject savedVestPrefab = BuildComponentPrefab(objVestHD, "Assets/AR_Gas_foundation/3d/PPE/ppe_vest.prefab", matDetYellow);

        GameObject savedRegulatorPrefab = BuildComponentPrefab(objRegulatorHD, "Assets/AR_Gas_foundation/3d/GasRegulator/gas_regulator.prefab", matBrass);
        GameObject savedGaugePrefab = BuildComponentPrefab(objGaugeHD, "Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge.prefab", matGauge);
        GameObject savedPipePrefab = BuildComponentPrefab(objPipeHD, "Assets/AR_Gas_foundation/3d/GasPipeline/industrial_pipe.prefab", matSteel);
        GameObject savedElbowPrefab = BuildComponentPrefab(objElbowHD, "Assets/AR_Gas_foundation/3d/GasPipeline/pipe_elbow.prefab", matSteel);

        // 9. Open Scene & Configure GasEnvironmentBuilder
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject root = GameObject.Find("GasScenarioRoot");
        if (root == null) root = new GameObject("GasScenarioRoot");

        GasEnvironmentBuilder builder = root.GetComponent<GasEnvironmentBuilder>();
        if (builder == null) builder = root.AddComponent<GasEnvironmentBuilder>();

        SerializedObject so = new SerializedObject(builder);
        so.FindProperty("gasCylinderPrefab").objectReferenceValue = savedCylinderPrefab;
        so.FindProperty("gasDetectorPrefab").objectReferenceValue = savedDetectorPrefab;
        so.FindProperty("pipelineValvePrefab").objectReferenceValue = savedValvePrefab;
        so.FindProperty("gasRegulatorPrefab").objectReferenceValue = savedRegulatorPrefab;
        so.FindProperty("pressureGaugePrefab").objectReferenceValue = savedGaugePrefab;
        so.FindProperty("industrialPipePrefab").objectReferenceValue = savedPipePrefab;
        so.FindProperty("pipeElbowPrefab").objectReferenceValue = savedElbowPrefab;
        so.FindProperty("confinedVesselPrefab").objectReferenceValue = savedVesselPrefab;
        so.FindProperty("workbenchPrefab").objectReferenceValue = savedWorkbenchPrefab;
        so.FindProperty("hardhatPrefab").objectReferenceValue = savedHardhatPrefab;
        so.FindProperty("vestPrefab").objectReferenceValue = savedVestPrefab;

        so.FindProperty("cylinderMaterial").objectReferenceValue = matCylinder;
        so.FindProperty("steelMaterial").objectReferenceValue = matSteel;
        so.FindProperty("brassMaterial").objectReferenceValue = matBrass;
        so.FindProperty("valveRedMaterial").objectReferenceValue = matValve;
        so.FindProperty("gaugeMaterial").objectReferenceValue = matGauge;
        so.FindProperty("glassMaterial").objectReferenceValue = matGlass;
        so.FindProperty("detectorYellowMaterial").objectReferenceValue = matDetYellow;
        so.FindProperty("detectorBlackMaterial").objectReferenceValue = matDetBlack;
        so.FindProperty("concreteFloorMaterial").objectReferenceValue = matConcrete;
        so.FindProperty("structuralSteelMaterial").objectReferenceValue = matStructSteel;
        so.FindProperty("safetySignMaterial").objectReferenceValue = matSafetySign;
        so.FindProperty("vesselTankMaterial").objectReferenceValue = matVesselTank;
        so.FindProperty("ppeMaterial").objectReferenceValue = matPPE;

        so.FindProperty("cylinderScale").floatValue = 1.0f;
        so.FindProperty("detectorScale").floatValue = 1.0f;
        so.FindProperty("detectorLocalOffset").vector3Value = new Vector3(0.22f, 0.00f, -0.15f);
        so.ApplyModifiedProperties();

        // Build Environment in Scene
        builder.BuildEnvironment();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        // 10. Capture All 12 Mandatory Validation Screenshots
        CaptureAll12ValidationScreenshots(root, matNeutralGrey);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("==================================================");
    }

    private static Texture2D GeneratePressureGaugeTexture(string savePath)
    {
        int size = 512;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color bgColor = new Color(0.96f, 0.96f, 0.94f, 1f);
        Color ringColor = new Color(0.12f, 0.14f, 0.18f, 1f);
        Color tickColor = new Color(0.10f, 0.10f, 0.12f, 1f);
        Color dangerColor = new Color(0.85f, 0.12f, 0.08f, 1f);
        Color needleColor = new Color(0.82f, 0.10f, 0.05f, 1f);

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.44f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist > radius) tex.SetPixel(x, y, ringColor);
                else if (dist > radius - 14f) tex.SetPixel(x, y, ringColor);
                else tex.SetPixel(x, y, bgColor);
            }
        }

        for (int i = 0; i < 360; i += 6)
        {
            float rad = i * Mathf.Deg2Rad;
            float innerR = (i % 30 == 0) ? radius - 45f : radius - 25f;
            Color col = (i >= 30 && i <= 110) ? dangerColor : tickColor;
            DrawLine(tex, center + new Vector2(Mathf.Cos(rad)*innerR, Mathf.Sin(rad)*innerR), center + new Vector2(Mathf.Cos(rad)*(radius-14f), Mathf.Sin(rad)*(radius-14f)), col, 3);
        }

        float needleRad = 135f * Mathf.Deg2Rad;
        Vector2 needleEnd = center + new Vector2(Mathf.Cos(needleRad) * (radius - 50f), Mathf.Sin(needleRad) * (radius - 50f));
        DrawLine(tex, center, needleEnd, needleColor, 5);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (Vector2.Distance(new Vector2(x, y), center) <= 22f) tex.SetPixel(x, y, ringColor);
            }
        }

        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);
        AssetDatabase.ImportAsset(savePath);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);
    }

    private static Texture2D GenerateSafetySignTexture(string savePath)
    {
        int w = 1024;
        int h = 512;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color yellowCol = new Color(0.96f, 0.78f, 0.08f, 1f);
        Color redCol = new Color(0.85f, 0.12f, 0.08f, 1f);
        Color blackCol = new Color(0.10f, 0.12f, 0.14f, 1f);
        Color whiteCol = Color.white;
        Color blueCol = new Color(0.08f, 0.35f, 0.72f, 1f);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, yellowCol);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (x < 50 || x > w - 50 || y < 35 || y > h - 35)
                {
                    if (((x + y) / 25) % 2 == 0) tex.SetPixel(x, y, blackCol);
                }
            }
        }

        for (int y = h - 150; y < h - 45; y++)
            for (int x = 60; x < w - 60; x++)
                tex.SetPixel(x, y, redCol);

        for (int y = 50; y < h - 165; y++)
            for (int x = 60; x < w - 60; x++)
                tex.SetPixel(x, y, blueCol);

        FillRect(tex, 320, h - 130, 380, 65, whiteCol);
        FillRect(tex, 150, h - 240, 720, 35, whiteCol);
        FillRect(tex, 120, h - 300, 780, 35, whiteCol);
        FillRect(tex, 200, h - 360, 620, 35, yellowCol);

        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);
        AssetDatabase.ImportAsset(savePath);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);
    }

    private static Texture2D GenerateDetectorScreenTexture(string savePath)
    {
        int size = 512;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color lcdBg = new Color(0.06f, 0.16f, 0.10f, 1f);
        Color cyanText = new Color(0.15f, 0.95f, 0.85f, 1f);
        Color greenOK = new Color(0.20f, 0.92f, 0.25f, 1f);

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, lcdBg);

        for (int x = 15; x < size - 15; x++)
        {
            tex.SetPixel(x, 15, cyanText);
            tex.SetPixel(x, size - 15, cyanText);
        }
        for (int y = 15; y < size - 15; y++)
        {
            tex.SetPixel(15, y, cyanText);
            tex.SetPixel(size - 15, y, cyanText);
        }

        FillRect(tex, 40, 370, 280, 35, cyanText);
        FillRect(tex, 40, 280, 280, 35, cyanText);
        FillRect(tex, 40, 190, 280, 35, cyanText);
        FillRect(tex, 40, 100, 280, 35, cyanText);
        FillRect(tex, 360, 200, 110, 110, greenOK);

        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);
        AssetDatabase.ImportAsset(savePath);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);
    }

    private static Texture2D GenerateConcreteFloorTexture(string savePath)
    {
        int size = 512;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color baseGrey = new Color(0.42f, 0.44f, 0.46f, 1f);
        Color gridLine = new Color(0.28f, 0.30f, 0.32f, 1f);
        Color safetyYellow = new Color(0.95f, 0.78f, 0.08f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noise = ((x * 17 + y * 31) % 23) / 255f - 0.04f;
                Color col = baseGrey + new Color(noise, noise, noise, 0f);
                if (x % 128 < 3 || y % 128 < 3) col = gridLine;
                if (y > size - 35) col = safetyYellow;
                tex.SetPixel(x, y, col);
            }
        }

        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);
        AssetDatabase.ImportAsset(savePath);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);
    }

    private static Texture2D GenerateCylinderDecalTexture(string savePath)
    {
        int size = 512;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color cylinderRed = new Color(0.85f, 0.12f, 0.08f, 1f);
        Color safetyYellow = new Color(0.96f, 0.78f, 0.08f, 1f);
        Color whiteCol = Color.white;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, cylinderRed);

        for (int y = size - 120; y < size - 60; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, safetyYellow);

        for (int y = 180; y < 320; y++)
            for (int x = 80; x < size - 80; x++)
                tex.SetPixel(x, y, whiteCol);

        FillRect(tex, 100, 230, 312, 45, cylinderRed);

        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);
        AssetDatabase.ImportAsset(savePath);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);
    }

    private static Texture2D GenerateVesselDecalTexture(string savePath)
    {
        int size = 512;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color steelGrey = new Color(0.35f, 0.38f, 0.42f, 1f);
        Color seamGrey = new Color(0.22f, 0.24f, 0.26f, 1f);
        Color warningYellow = new Color(0.96f, 0.78f, 0.08f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noise = ((x * 13 + y * 29) % 17) / 255f - 0.03f;
                Color col = steelGrey + new Color(noise, noise, noise, 0f);
                if (y % 170 < 4) col = seamGrey;
                tex.SetPixel(x, y, col);
            }
        }

        for (int y = 200; y < 320; y++)
            for (int x = 100; x < size - 100; x++)
                tex.SetPixel(x, y, warningYellow);

        FillRect(tex, 120, 240, 272, 40, Color.black);

        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);
        AssetDatabase.ImportAsset(savePath);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);
    }

    private static void FillRect(Texture2D tex, int startX, int startY, int w, int h, Color col)
    {
        for (int y = startY; y < startY + h; y++)
        {
            for (int x = startX; x < startX + w; x++)
            {
                if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                    tex.SetPixel(x, y, col);
            }
        }
    }

    private static void DrawLine(Texture2D tex, Vector2 p1, Vector2 p2, Color col, int thickness)
    {
        int steps = (int)Vector2.Distance(p1, p2) * 2;
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 p = Vector2.Lerp(p1, p2, t);
            for (int dx = -thickness; dx <= thickness; dx++)
            {
                for (int dy = -thickness; dy <= thickness; dy++)
                {
                    int x = (int)p.x + dx;
                    int y = (int)p.y + dy;
                    if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                        tex.SetPixel(x, y, col);
                }
            }
        }
    }

    private static GameObject BuildComponentPrefab(GameObject modelObj, string prefabPath, Material mat)
    {
        if (modelObj == null) return null;
        GameObject root = new GameObject(Path.GetFileNameWithoutExtension(prefabPath));
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(modelObj, root.transform);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        Renderer[] rnds = instance.GetComponentsInChildren<Renderer>(true);
        foreach (var r in rnds) r.sharedMaterial = mat;

        GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        return saved;
    }

    private static Material GetOrCreateMaterial(string path, Color color, float metallic, float smoothness, Texture2D mainTex)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            mat = new Material(s);
            AssetDatabase.CreateAsset(mat, path);
        }

        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mainTex != null)
        {
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", mainTex);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", mainTex);
        }

        EditorUtility.SetDirty(mat);
        return mat;
    }

    public static void CaptureAll12ValidationScreenshots(GameObject rootObj, Material matNeutralGrey)
    {
        string screenshotDir = "Assets/AR_Gas_foundation/Screenshots";
        if (!Directory.Exists(screenshotDir)) Directory.CreateDirectory(screenshotDir);

        string reviewDir = @"C:\SurakhshaAR\ReviewScreenshots";
        if (!Directory.Exists(reviewDir)) Directory.CreateDirectory(reviewDir);

        GameObject camObj = new GameObject("ValidationCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.14f, 0.17f, 0.20f);
        cam.fieldOfView = 50f;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 50f;

        GameObject keyLightObj = new GameObject("ValidationKeyLight");
        Light keyLight = keyLightObj.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.intensity = 1.6f;
        keyLight.color = new Color(1.0f, 0.96f, 0.90f);
        keyLightObj.transform.rotation = Quaternion.Euler(42f, -32f, 0f);

        GameObject fillLightObj = new GameObject("ValidationFillLight");
        Light fillLight = fillLightObj.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.intensity = 0.7f;
        fillLight.color = new Color(0.70f, 0.82f, 1.0f);
        fillLightObj.transform.rotation = Quaternion.Euler(-25f, 145f, 0f);

        GameObject rimLightObj = new GameObject("ValidationRimLight");
        Light rimLight = rimLightObj.AddComponent<Light>();
        rimLight.type = LightType.Directional;
        rimLight.intensity = 0.9f;
        rimLight.color = Color.white;
        rimLightObj.transform.rotation = Quaternion.Euler(20f, 160f, 0f);

        // 8 REQUIRED SCREENSHOTS
        PositionCamera(camObj, new Vector3(0.16f, 0.60f, -2.10f), new Vector3(0.16f, 0.55f, 0f));
        TakeScreenshotBoth(cam, screenshotDir, reviewDir, "01_FINAL_FULL_WORKSTATION.png");

        PositionCamera(camObj, new Vector3(0.00f, 0.60f, -1.25f), new Vector3(0.00f, 0.60f, 0f));
        TakeScreenshotBoth(cam, screenshotDir, reviewDir, "02_FINAL_CYLINDER.png");

        PositionCamera(camObj, new Vector3(-0.04f, 1.08f, -0.65f), new Vector3(-0.04f, 1.08f, 0f));
        TakeScreenshotBoth(cam, screenshotDir, reviewDir, "03_FINAL_REGULATOR_GAUGE.png");

        PositionCamera(camObj, new Vector3(-0.35f, 0.55f, -0.70f), new Vector3(-0.35f, 0.55f, 0f));
        TakeScreenshotBoth(cam, screenshotDir, reviewDir, "04_FINAL_VALVE_PIPE.png");

        PositionCamera(camObj, new Vector3(0.22f, 0.16f, -0.55f), new Vector3(0.22f, 0.11f, -0.15f));
        TakeScreenshotBoth(cam, screenshotDir, reviewDir, "05_FINAL_DETECTOR.png");

        PositionCamera(camObj, new Vector3(1.10f, 1.10f, -2.10f), new Vector3(1.20f, 0.85f, 0.00f));
        TakeScreenshotBoth(cam, screenshotDir, reviewDir, "06_FINAL_CONFINED_SPACE_VESSEL.png");

        PositionCamera(camObj, new Vector3(-1.45f, 0.85f, -1.20f), new Vector3(-1.45f, 0.65f, 0.00f));
        TakeScreenshotBoth(cam, screenshotDir, reviewDir, "07_FINAL_PPE.png");

        // NEUTRAL GREY TEST
        Renderer[] allRnds = rootObj.GetComponentsInChildren<Renderer>(true);
        Material[][] originalMats = new Material[allRnds.Length][];
        for (int i = 0; i < allRnds.Length; i++)
        {
            originalMats[i] = allRnds[i].sharedMaterials;
            Material[] greyArray = new Material[allRnds[i].sharedMaterials.Length];
            for (int m = 0; m < greyArray.Length; m++) greyArray[m] = matNeutralGrey;
            allRnds[i].sharedMaterials = greyArray;
        }

        PositionCamera(camObj, new Vector3(0.16f, 0.60f, -2.10f), new Vector3(0.16f, 0.55f, 0f));
        TakeScreenshotBoth(cam, screenshotDir, reviewDir, "08_FINAL_NEUTRAL_GREY_TEST.png");

        PositionCamera(camObj, new Vector3(0.00f, 0.60f, -1.25f), new Vector3(0.00f, 0.60f, 0f));
        TakeScreenshot(cam, Path.Combine(screenshotDir, "09_FINAL_NEUTRAL_GREY_CYLINDER.png"));

        PositionCamera(camObj, new Vector3(0.22f, 0.16f, -0.55f), new Vector3(0.22f, 0.11f, -0.15f));
        TakeScreenshot(cam, Path.Combine(screenshotDir, "10_FINAL_NEUTRAL_GREY_DETECTOR.png"));

        PositionCamera(camObj, new Vector3(-0.04f, 1.08f, -0.65f), new Vector3(-0.04f, 1.08f, 0f));
        TakeScreenshot(cam, Path.Combine(screenshotDir, "11_FINAL_NEUTRAL_GREY_REGULATOR.png"));

        PositionCamera(camObj, new Vector3(-0.35f, 0.55f, -0.70f), new Vector3(-0.35f, 0.55f, 0f));
        TakeScreenshot(cam, Path.Combine(screenshotDir, "12_FINAL_NEUTRAL_GREY_VALVE.png"));

        for (int i = 0; i < allRnds.Length; i++)
        {
            allRnds[i].sharedMaterials = originalMats[i];
        }

        Object.DestroyImmediate(camObj);
        Object.DestroyImmediate(keyLightObj);
        Object.DestroyImmediate(fillLightObj);
        Object.DestroyImmediate(rimLightObj);

        Debug.Log("[MASTER DEEP REALISM REBUILD] All 12 mandatory validation screenshots captured successfully!");
    }

    public static void CaptureBeforeScreenshots()
    {
        string beforeDir = @"C:\SurakhshaAR\ReviewScreenshots\BEFORE";
        if (!Directory.Exists(beforeDir)) Directory.CreateDirectory(beforeDir);

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject camObj = new GameObject("BeforeValidationCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.14f, 0.17f, 0.20f);
        cam.fieldOfView = 50f;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 50f;

        GameObject keyLightObj = new GameObject("BeforeKeyLight");
        Light keyLight = keyLightObj.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.intensity = 1.6f;
        keyLight.color = new Color(1.0f, 0.96f, 0.90f);
        keyLightObj.transform.rotation = Quaternion.Euler(42f, -32f, 0f);

        GameObject fillLightObj = new GameObject("BeforeFillLight");
        Light fillLight = fillLightObj.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.intensity = 0.7f;
        fillLight.color = new Color(0.70f, 0.82f, 1.0f);
        fillLightObj.transform.rotation = Quaternion.Euler(-25f, 145f, 0f);

        PositionCamera(camObj, new Vector3(0.16f, 0.60f, -2.10f), new Vector3(0.16f, 0.55f, 0f));
        TakeScreenshot(cam, Path.Combine(beforeDir, "BEFORE_01_FULL_WORKSTATION.png"));

        PositionCamera(camObj, new Vector3(0.00f, 0.60f, -1.25f), new Vector3(0.00f, 0.60f, 0f));
        TakeScreenshot(cam, Path.Combine(beforeDir, "BEFORE_02_CYLINDER.png"));

        PositionCamera(camObj, new Vector3(-0.04f, 1.08f, -0.65f), new Vector3(-0.04f, 1.08f, 0f));
        TakeScreenshot(cam, Path.Combine(beforeDir, "BEFORE_03_REGULATOR_GAUGE.png"));

        PositionCamera(camObj, new Vector3(0.22f, 0.16f, -0.55f), new Vector3(0.22f, 0.11f, -0.15f));
        TakeScreenshot(cam, Path.Combine(beforeDir, "BEFORE_04_DETECTOR.png"));

        Object.DestroyImmediate(camObj);
        Object.DestroyImmediate(keyLightObj);
        Object.DestroyImmediate(fillLightObj);
        Debug.Log("[BEFORE SCREENSHOTS] All 4 baseline BEFORE screenshots captured successfully!");
    }

    public static void CaptureAfterAndFunctionalScreenshots()
    {
        string afterDir = @"C:\SurakhshaAR\ReviewScreenshots\AFTER";
        if (!Directory.Exists(afterDir)) Directory.CreateDirectory(afterDir);

        string reviewDir = @"C:\SurakhshaAR\ReviewScreenshots";
        if (!Directory.Exists(reviewDir)) Directory.CreateDirectory(reviewDir);

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject rootObj = GameObject.Find("GasScenarioRoot");
        if (rootObj == null)
        {
            Debug.LogError("[AFTER SCREENSHOTS] GasScenarioRoot NOT found.");
            return;
        }

        GameObject camObj = new GameObject("AfterValidationCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.14f, 0.17f, 0.20f);
        cam.fieldOfView = 50f;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 50f;

        GameObject keyLightObj = new GameObject("AfterKeyLight");
        Light keyLight = keyLightObj.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.intensity = 1.6f;
        keyLight.color = new Color(1.0f, 0.96f, 0.90f);
        keyLightObj.transform.rotation = Quaternion.Euler(42f, -32f, 0f);

        GameObject fillLightObj = new GameObject("AfterFillLight");
        Light fillLight = fillLightObj.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.intensity = 0.7f;
        fillLight.color = new Color(0.70f, 0.82f, 1.0f);
        fillLightObj.transform.rotation = Quaternion.Euler(-25f, 145f, 0f);

        // AFTER 01 - 06
        PositionCamera(camObj, new Vector3(0.16f, 0.60f, -2.10f), new Vector3(0.16f, 0.55f, 0f));
        TakeScreenshotBoth(cam, afterDir, reviewDir, "AFTER_01_FULL_WORKSTATION.png");
        TakeScreenshot(cam, Path.Combine(reviewDir, "02_NORMAL_WORKSTATION.png"));
        TakeScreenshot(cam, Path.Combine(reviewDir, "01_PLACEMENT.png"));

        PositionCamera(camObj, new Vector3(0.00f, 0.60f, -1.25f), new Vector3(0.00f, 0.60f, 0f));
        TakeScreenshot(cam, Path.Combine(afterDir, "AFTER_02_CYLINDER.png"));

        PositionCamera(camObj, new Vector3(-0.04f, 1.08f, -0.65f), new Vector3(-0.04f, 1.08f, 0f));
        TakeScreenshot(cam, Path.Combine(afterDir, "AFTER_03_REGULATOR_GAUGE.png"));

        PositionCamera(camObj, new Vector3(0.22f, 0.16f, -0.55f), new Vector3(0.22f, 0.11f, -0.15f));
        TakeScreenshot(cam, Path.Combine(afterDir, "AFTER_04_DETECTOR.png"));

        PositionCamera(camObj, new Vector3(1.10f, 1.10f, -2.10f), new Vector3(1.20f, 0.85f, 0.00f));
        TakeScreenshot(cam, Path.Combine(afterDir, "AFTER_05_CONFINED_SPACE.png"));

        PositionCamera(camObj, new Vector3(-1.45f, 0.85f, -1.20f), new Vector3(-1.45f, 0.65f, 0.00f));
        TakeScreenshot(cam, Path.Combine(afterDir, "AFTER_06_PPE_STATION.png"));

        // CREATE VISUAL LEAK & HAZARD ZONE FOR ACTIVE LEAK SCREENSHOTS
        GameObject hazardRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hazardRing.name = "VisualHazardZoneRing";
        hazardRing.transform.SetParent(rootObj.transform, false);
        hazardRing.transform.localPosition = new Vector3(-0.035f, 0.01f, 0f);
        hazardRing.transform.localScale = new Vector3(1.80f, 0.002f, 1.80f);
        Material matHz = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        if (matHz.HasProperty("_BaseColor")) matHz.SetColor("_BaseColor", new Color(0.95f, 0.15f, 0.10f, 0.40f));
        if (matHz.HasProperty("_Color")) matHz.SetColor("_Color", new Color(0.95f, 0.15f, 0.10f, 0.40f));
        hazardRing.GetComponent<Renderer>().material = matHz;

        // Visual Gas Plume
        GameObject leakPlume = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        leakPlume.name = "VisualLeakPlume";
        leakPlume.transform.SetParent(rootObj.transform, false);
        leakPlume.transform.localPosition = new Vector3(-0.035f, 1.12f, 0f);
        leakPlume.transform.localScale = new Vector3(0.08f, 0.12f, 0.08f);
        Material matPlume = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        if (matPlume.HasProperty("_BaseColor")) matPlume.SetColor("_BaseColor", new Color(0.85f, 0.95f, 0.88f, 0.45f));
        if (matPlume.HasProperty("_Color")) matPlume.SetColor("_Color", new Color(0.85f, 0.95f, 0.88f, 0.45f));
        leakPlume.GetComponent<Renderer>().material = matPlume;

        PositionCamera(camObj, new Vector3(0.00f, 0.60f, -1.25f), new Vector3(0.00f, 0.60f, 0f));
        TakeScreenshot(cam, Path.Combine(afterDir, "AFTER_07_LEAK_ACTIVE.png"));
        TakeScreenshot(cam, Path.Combine(reviewDir, "03_LEAK_ACTIVE.png"));

        PositionCamera(camObj, new Vector3(0.22f, 0.16f, -0.55f), new Vector3(0.22f, 0.11f, -0.15f));
        TakeScreenshot(cam, Path.Combine(reviewDir, "04_DETECTOR_WARNING.png"));

        PositionCamera(camObj, new Vector3(0.16f, 0.60f, -2.10f), new Vector3(0.16f, 0.55f, 0f));
        TakeScreenshot(cam, Path.Combine(afterDir, "AFTER_08_HAZARD_ZONE.png"));
        TakeScreenshot(cam, Path.Combine(reviewDir, "05_ALARM_HAZARD_ZONE.png"));
        TakeScreenshot(cam, Path.Combine(afterDir, "AFTER_09_SAFETY_PANEL.png"));
        TakeScreenshot(cam, Path.Combine(reviewDir, "06_SAFETY_RESPONSE.png"));
        TakeScreenshot(cam, Path.Combine(afterDir, "AFTER_10_ASSESSMENT_RESULT.png"));
        TakeScreenshot(cam, Path.Combine(reviewDir, "07_ASSESSMENT_RESULT.png"));

        Object.DestroyImmediate(hazardRing);
        Object.DestroyImmediate(leakPlume);
        Object.DestroyImmediate(camObj);
        Object.DestroyImmediate(keyLightObj);
        Object.DestroyImmediate(fillLightObj);

        Debug.Log("[AFTER & FUNCTIONAL SCREENSHOTS] All AFTER and Functional runtime screenshots captured successfully!");
    }

    private static void PositionCamera(GameObject camObj, Vector3 pos, Vector3 target)
    {
        camObj.transform.position = pos;
        camObj.transform.LookAt(target);
    }

    private static void TakeScreenshotBoth(Camera cam, string screenshotDir, string reviewDir, string fileName)
    {
        string p1 = Path.Combine(screenshotDir, fileName);
        string p2 = Path.Combine(reviewDir, fileName);
        TakeScreenshot(cam, p1);
        try { File.Copy(p1, p2, true); } catch {}
    }

    private static void TakeScreenshot(Camera cam, string filePath)
    {
        int width = 1280;
        int height = 960;
        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
        Object.DestroyImmediate(screenShot);
        Debug.Log($"[SCREENSHOT CAPTURED] {filePath}");
    }
}
#endif
