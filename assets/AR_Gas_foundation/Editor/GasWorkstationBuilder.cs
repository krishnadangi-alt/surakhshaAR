#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public class GasWorkstationBuilder
{
    [MenuItem("SurakshaAR/Build Complete 3D Industrial Gas Workstation")]
    public static void BuildCompleteWorkstation()
    {
        Debug.Log("==================================================");
        Debug.Log("[WORKSTATION BUILDER] Rebuilding Clean 3D Gas Workstation");
        Debug.Log("==================================================");

        string matDir = "Assets/AR_Gas_foundation/Materials";
        if (!Directory.Exists(matDir)) Directory.CreateDirectory(matDir);

        // 1. Create High-Quality Mobile/URP Compatible Materials
        Material matCylinder = CreateOrGetMaterial(Path.Combine(matDir, "MAT_GasCylinder_Red.mat"), new Color(0.82f, 0.14f, 0.10f), 0.35f, 0.68f);
        Material matBrass = CreateOrGetMaterial(Path.Combine(matDir, "MAT_Regulator_Brass.mat"), new Color(0.84f, 0.68f, 0.22f), 0.85f, 0.80f);
        Material matSteel = CreateOrGetMaterial(Path.Combine(matDir, "MAT_Pipeline_Steel.mat"), new Color(0.40f, 0.42f, 0.45f), 0.70f, 0.60f);
        Material matValve = CreateOrGetMaterial(Path.Combine(matDir, "MAT_Valve_Red.mat"), new Color(0.80f, 0.12f, 0.10f), 0.30f, 0.60f);
        Material matGauge = CreateOrGetMaterial(Path.Combine(matDir, "MAT_PressureGauge.mat"), new Color(0.95f, 0.95f, 0.92f), 0.10f, 0.30f);
        Material matDetectorYellow = CreateOrGetMaterial(Path.Combine(matDir, "MAT_Detector_Yellow.mat"), new Color(0.95f, 0.75f, 0.05f), 0.20f, 0.50f);
        Material matDetectorBlack = CreateOrGetMaterial(Path.Combine(matDir, "MAT_Detector_Black.mat"), new Color(0.12f, 0.12f, 0.14f), 0.15f, 0.40f);
        Material matGlass = CreateOrGetMaterial(Path.Combine(matDir, "MAT_Gauge_Glass.mat"), new Color(0.90f, 0.95f, 1.00f, 0.25f), 0.90f, 0.95f);

        // Load 3D assets
        GameObject glbDetector = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.glb");
        GameObject fbxValve = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/industrial-valve/source/VALVE.fbx");

        // 2. Build Complete Gas Cylinder Assembly Prefab
        GameObject cylinderRoot = new GameObject("gas_cylinder");
        cylinderRoot.transform.position = Vector3.zero;
        cylinderRoot.transform.rotation = Quaternion.identity;
        cylinderRoot.transform.localScale = Vector3.one;

        // Grounded Base Ring
        GameObject cylBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylBase.name = "Cylinder_Base";
        cylBase.transform.SetParent(cylinderRoot.transform, false);
        cylBase.transform.localPosition = new Vector3(0f, 0.015f, 0f);
        cylBase.transform.localScale = new Vector3(0.34f, 0.015f, 0.34f);
        cylBase.GetComponent<Renderer>().material = matDetectorBlack;
        Object.DestroyImmediate(cylBase.GetComponent<Collider>());

        // Solid Pressurized Vessel Body (0.36m Diameter, 0.72m Height)
        GameObject cylBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylBody.name = "Cylinder_Body_Mesh";
        cylBody.transform.SetParent(cylinderRoot.transform, false);
        cylBody.transform.localPosition = new Vector3(0f, 0.38f, 0f);
        cylBody.transform.localScale = new Vector3(0.36f, 0.36f, 0.36f);
        cylBody.GetComponent<Renderer>().material = matCylinder;
        Object.DestroyImmediate(cylBody.GetComponent<Collider>());

        // Curved Top Shoulder Dome
        GameObject cylShoulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cylShoulder.name = "Cylinder_Shoulder";
        cylShoulder.transform.SetParent(cylinderRoot.transform, false);
        cylShoulder.transform.localPosition = new Vector3(0f, 0.74f, 0f);
        cylShoulder.transform.localScale = new Vector3(0.358f, 0.26f, 0.358f);
        cylShoulder.GetComponent<Renderer>().material = matCylinder;
        Object.DestroyImmediate(cylShoulder.GetComponent<Collider>());

        // Brass Neck Collar
        GameObject cylNeck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylNeck.name = "Cylinder_Neck";
        cylNeck.transform.SetParent(cylinderRoot.transform, false);
        cylNeck.transform.localPosition = new Vector3(0f, 0.88f, 0f);
        cylNeck.transform.localScale = new Vector3(0.12f, 0.02f, 0.12f);
        cylNeck.GetComponent<Renderer>().material = matBrass;
        Object.DestroyImmediate(cylNeck.GetComponent<Collider>());

        // Top Valve Stem & Handwheel
        GameObject cylValveStem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylValveStem.name = "Cylinder_Valve_Stem";
        cylValveStem.transform.SetParent(cylinderRoot.transform, false);
        cylValveStem.transform.localPosition = new Vector3(0f, 0.92f, 0f);
        cylValveStem.transform.localScale = new Vector3(0.045f, 0.02f, 0.045f);
        cylValveStem.GetComponent<Renderer>().material = matBrass;
        Object.DestroyImmediate(cylValveStem.GetComponent<Collider>());

        GameObject cylValveWheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylValveWheel.name = "Cylinder_Valve_Handwheel";
        cylValveWheel.transform.SetParent(cylinderRoot.transform, false);
        cylValveWheel.transform.localPosition = new Vector3(0f, 0.95f, 0f);
        cylValveWheel.transform.localScale = new Vector3(0.08f, 0.01f, 0.08f);
        cylValveWheel.GetComponent<Renderer>().material = matValve;
        Object.DestroyImmediate(cylValveWheel.GetComponent<Collider>());

        // Root BoxCollider (Overall Cylinder Bounds)
        BoxCollider boxCol = cylinderRoot.AddComponent<BoxCollider>();
        boxCol.center = new Vector3(0f, 0.525f, 0f);
        boxCol.size = new Vector3(0.38f, 1.05f, 0.38f);

        // 3. Brass Regulator Assembly
        GameObject regulatorRoot = new GameObject("Brass_Regulator");
        regulatorRoot.transform.SetParent(cylinderRoot.transform, false);
        regulatorRoot.transform.localPosition = Vector3.zero;

        // Inlet Coupling Nipple (Valve -> Regulator)
        GameObject regInlet = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        regInlet.name = "Inlet_Coupling";
        regInlet.transform.SetParent(regulatorRoot.transform, false);
        regInlet.transform.localPosition = new Vector3(0.045f, 0.95f, 0f);
        regInlet.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        regInlet.transform.localScale = new Vector3(0.035f, 0.03f, 0.035f);
        regInlet.GetComponent<Renderer>().material = matBrass;
        Object.DestroyImmediate(regInlet.GetComponent<Collider>());

        // Regulator Main Body
        GameObject regBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        regBody.name = "Regulator_Body";
        regBody.transform.SetParent(regulatorRoot.transform, false);
        regBody.transform.localPosition = new Vector3(0.10f, 0.95f, 0f);
        regBody.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        regBody.transform.localScale = new Vector3(0.065f, 0.05f, 0.065f);
        regBody.GetComponent<Renderer>().material = matBrass;
        Object.DestroyImmediate(regBody.GetComponent<Collider>());

        // Relief Knob
        GameObject regKnob = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        regKnob.name = "Relief_Knob";
        regKnob.transform.SetParent(regulatorRoot.transform, false);
        regKnob.transform.localPosition = new Vector3(0.10f, 0.905f, 0f);
        regKnob.transform.localScale = new Vector3(0.025f, 0.015f, 0.025f);
        regKnob.GetComponent<Renderer>().material = matDetectorBlack;
        Object.DestroyImmediate(regKnob.GetComponent<Collider>());

        // Outlet Coupling
        GameObject regOutlet = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        regOutlet.name = "Outlet_Coupling";
        regOutlet.transform.SetParent(regulatorRoot.transform, false);
        regOutlet.transform.localPosition = new Vector3(0.155f, 0.95f, 0f);
        regOutlet.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        regOutlet.transform.localScale = new Vector3(0.045f, 0.015f, 0.045f);
        regOutlet.GetComponent<Renderer>().material = matBrass;
        Object.DestroyImmediate(regOutlet.GetComponent<Collider>());

        // 4. Analog Pressure Gauge Assembly
        GameObject gaugeRoot = new GameObject("Pressure_Gauge");
        gaugeRoot.transform.SetParent(cylinderRoot.transform, false);
        gaugeRoot.transform.localPosition = new Vector3(0.06f, 1.02f, 0.03f);
        gaugeRoot.transform.localRotation = Quaternion.identity;

        // Gauge Nipple Stem
        GameObject gaugeStem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        gaugeStem.name = "Mounting_Nipple";
        gaugeStem.transform.SetParent(gaugeRoot.transform, false);
        gaugeStem.transform.localPosition = new Vector3(0f, -0.03f, -0.015f);
        gaugeStem.transform.localScale = new Vector3(0.015f, 0.025f, 0.015f);
        gaugeStem.GetComponent<Renderer>().material = matBrass;
        Object.DestroyImmediate(gaugeStem.GetComponent<Collider>());

        // Gauge Housing
        GameObject gaugeHousing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        gaugeHousing.name = "Gauge_Housing";
        gaugeHousing.transform.SetParent(gaugeRoot.transform, false);
        gaugeHousing.transform.localPosition = Vector3.zero;
        gaugeHousing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        gaugeHousing.transform.localScale = new Vector3(0.085f, 0.015f, 0.085f);
        gaugeHousing.GetComponent<Renderer>().material = matDetectorBlack;
        Object.DestroyImmediate(gaugeHousing.GetComponent<Collider>());

        // Gauge Face
        GameObject gaugeFace = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        gaugeFace.name = "Gauge_Face";
        gaugeFace.transform.SetParent(gaugeRoot.transform, false);
        gaugeFace.transform.localPosition = new Vector3(0f, 0f, -0.008f);
        gaugeFace.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        gaugeFace.transform.localScale = new Vector3(0.078f, 0.001f, 0.078f);
        gaugeFace.GetComponent<Renderer>().material = matGauge;
        Object.DestroyImmediate(gaugeFace.GetComponent<Collider>());

        // Gauge Needle
        GameObject gaugeNeedle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gaugeNeedle.name = "Gauge_Needle";
        gaugeNeedle.transform.SetParent(gaugeRoot.transform, false);
        gaugeNeedle.transform.localPosition = new Vector3(0.008f, 0.008f, -0.011f);
        gaugeNeedle.transform.localRotation = Quaternion.Euler(0f, 0f, -45f);
        gaugeNeedle.transform.localScale = new Vector3(0.004f, 0.032f, 0.002f);
        gaugeNeedle.GetComponent<Renderer>().material = matValve;
        Object.DestroyImmediate(gaugeNeedle.GetComponent<Collider>());

        // Gauge Glass
        GameObject gaugeGlass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        gaugeGlass.name = "Gauge_Glass";
        gaugeGlass.transform.SetParent(gaugeRoot.transform, false);
        gaugeGlass.transform.localPosition = new Vector3(0f, 0f, -0.012f);
        gaugeGlass.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        gaugeGlass.transform.localScale = new Vector3(0.080f, 0.001f, 0.080f);
        gaugeGlass.GetComponent<Renderer>().material = matGlass;
        Object.DestroyImmediate(gaugeGlass.GetComponent<Collider>());

        // 5. LeakPoint at Regulator Connection
        GameObject leakPoint = new GameObject("LeakPoint");
        leakPoint.transform.SetParent(cylinderRoot.transform, false);
        leakPoint.transform.localPosition = new Vector3(0.12f, 0.95f, 0f);
        leakPoint.transform.localRotation = Quaternion.identity;

        // Save Clean Cylinder Prefab
        string cylinderPrefabPath = "Assets/AR_Gas_foundation/3d/GasCylinder/Prefabs/gas_cylinder.prefab";
        GameObject savedCylinderPrefab = PrefabUtility.SaveAsPrefabAsset(cylinderRoot, cylinderPrefabPath);
        Object.DestroyImmediate(cylinderRoot);
        Debug.Log($"[WORKSTATION BUILDER] Gas Cylinder Prefab saved to {cylinderPrefabPath}");

        // 6. Build Handheld Multi-Gas Detector Prefab using REAL 3D Mesh multi_gas_detector_-_low_poly.glb (Upright Orientation)
        GameObject detectorRoot = new GameObject("multi_gas_detector");
        detectorRoot.transform.position = Vector3.zero;
        detectorRoot.transform.rotation = Quaternion.identity;

        float targetDetectorHeight = 0.20f; // 0.20m height handheld detector
        if (glbDetector != null)
        {
            GameObject detModel = (GameObject)PrefabUtility.InstantiatePrefab(glbDetector, detectorRoot.transform);
            detModel.name = "multi_gas_detector_3d_mesh";
            
            // Rotate X -90 deg to orient detector main body upright vertically along Y axis
            detModel.transform.localRotation = Quaternion.Euler(-90f, 180f, 0f);

            Renderer[] rnds = detModel.GetComponentsInChildren<Renderer>(true);
            float rawHeight = 4.6667f;
            float detScale = targetDetectorHeight / rawHeight;
            detModel.transform.localScale = Vector3.one * detScale;

            // Ground base of upright detector at Y = 0.0000m flat on floor
            detModel.transform.localPosition = new Vector3(0f, (rawHeight * 0.5f) * detScale, 0f);

            foreach (var r in rnds)
            {
                if (r.name == "Object_4") r.material = matDetectorYellow;
                else if (r.name == "Object_6" || r.name == "Object_8" || r.name == "Object_10") r.material = matDetectorBlack;
                else r.material = matDetectorYellow;
            }
        }
        else
        {
            Debug.LogError("[WORKSTATION BUILDER] multi_gas_detector_-_low_poly.glb NOT found!");
        }

        BoxCollider detCol = detectorRoot.AddComponent<BoxCollider>();
        detCol.center = new Vector3(0f, 0.10f, 0f);
        detCol.size = new Vector3(0.12f, 0.20f, 0.06f);

        string detectorPrefabPath = "Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.prefab";
        GameObject savedDetectorPrefab = PrefabUtility.SaveAsPrefabAsset(detectorRoot, detectorPrefabPath);
        Object.DestroyImmediate(detectorRoot);
        Debug.Log($"[WORKSTATION BUILDER] Multi-Gas Detector Prefab saved to {detectorPrefabPath}");

        // 7. Bind Builder Component References
        GasEnvironmentBuilder builder = Object.FindFirstObjectByType<GasEnvironmentBuilder>();
        if (builder != null)
        {
            SerializedObject so = new SerializedObject(builder);
            so.FindProperty("gasCylinderPrefab").objectReferenceValue = savedCylinderPrefab;
            so.FindProperty("gasDetectorPrefab").objectReferenceValue = savedDetectorPrefab;
            so.FindProperty("pipelineValvePrefab").objectReferenceValue = fbxValve;
            so.FindProperty("pipelineSteelMaterial").objectReferenceValue = matSteel;
            so.FindProperty("valveRedMaterial").objectReferenceValue = matValve;
            so.FindProperty("cylinderScale").floatValue = 1.0f;
            so.FindProperty("detectorScale").floatValue = 1.0f;
            so.FindProperty("detectorLocalOffset").vector3Value = new Vector3(0.38f, 0.00f, 0.08f);
            so.ApplyModifiedProperties();

            // Execute scene build
            builder.BuildEnvironment();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[WORKSTATION BUILDER] COMPLETE 3D INDUSTRIAL GAS WORKSTATION REBUILT SUCCESSFULLY!");
        Debug.Log("==================================================");
    }

    [MenuItem("SurakshaAR/Build Workstation + Capture Screenshots")]
    public static void BuildAndCaptureScreenshots()
    {
        BuildCompleteWorkstation();
        GasScreenshotCapturer.CaptureScreenshots();
    }

    [MenuItem("SurakshaAR/Build Workstation + Capture Screenshots + Build APK")]
    public static void BuildAllCombined()
    {
        BuildCompleteWorkstation();
        GasScreenshotCapturer.CaptureScreenshots();
        GasAndroidBuildScript.BuildAndroidAPK();
    }

    private static Material CreateOrGetMaterial(string matPath, Color albedoColor, float metallic, float smoothness)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Mobile/Bumped Specular");
            if (shader == null) shader = Shader.Find("Unlit/Color");

            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, matPath);
        }

        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", albedoColor);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", albedoColor);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

        EditorUtility.SetDirty(mat);
        return mat;
    }
}
#endif
