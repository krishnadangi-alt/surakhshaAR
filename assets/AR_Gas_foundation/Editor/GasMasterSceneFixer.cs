#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GasMasterSceneFixer
{
    [MenuItem("SurakshaAR/Fix Gas Scene & Capture Wide Workstation Screenshot")]
    public static void FixGasSceneAndCapture()
    {
        Debug.Log("==================================================");
        Debug.Log("[GAS MASTER SCENE FIXER] Rebuilding Real Workstation in Scene & Capturing Verification Screenshot");
        Debug.Log("==================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject root = GameObject.Find("GasScenarioRoot");
        if (root == null)
        {
            root = new GameObject("GasScenarioRoot");
        }
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;

        // Clear all existing children from root before building
        for (int i = root.transform.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
        }

        GasEnvironmentBuilder builder = root.GetComponent<GasEnvironmentBuilder>();
        if (builder == null) builder = root.AddComponent<GasEnvironmentBuilder>();

        // Load original real 3D assets (GLB / FBX / OBJ)
        GameObject cylinderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasCylinder/gas_cylinder.glb");
        if (cylinderPrefab == null) cylinderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasCylinder/Prefabs/gas_cylinder.prefab");

        GameObject detectorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.glb");
        if (detectorPrefab == null) detectorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasDetector/Prefabs/multi_gas_detector_-_low_poly.prefab");

        GameObject regulatorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasRegulator/high-pressure_regulator.glb");
        GameObject gaugePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge_animated.glb");
        GameObject pipePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasPipeline/modular-pipes/source/pipes.fbx");
        GameObject valvePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/industrial-valve/source/VALVE.fbx");
        GameObject vesselPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/ConfinedSpace/confined_vessel_hd.obj");
        GameObject fanPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/ConfinedSpace/ventilation_fan.glb");
        GameObject lifelinePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/ConfinedSpace/self_retracting_lifeline.glb");
        GameObject workbenchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/workbench_hd.obj");
        GameObject hardhatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/ppe_hardhat_hd.obj");
        GameObject gogglesPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/goggles.glb");
        GameObject glovesPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/work_gloves.glb");
        GameObject bootsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/safety_boots.glb");

        SerializedObject soBuilder = new SerializedObject(builder);
        soBuilder.FindProperty("gasCylinderPrefab").objectReferenceValue = cylinderPrefab;
        soBuilder.FindProperty("gasDetectorPrefab").objectReferenceValue = detectorPrefab;
        soBuilder.FindProperty("pipelineValvePrefab").objectReferenceValue = valvePrefab;
        soBuilder.FindProperty("gasRegulatorPrefab").objectReferenceValue = regulatorPrefab;
        soBuilder.FindProperty("pressureGaugePrefab").objectReferenceValue = gaugePrefab;
        soBuilder.FindProperty("industrialPipePrefab").objectReferenceValue = pipePrefab;
        soBuilder.FindProperty("pipeElbowPrefab").objectReferenceValue = pipePrefab;
        soBuilder.FindProperty("confinedVesselPrefab").objectReferenceValue = vesselPrefab;
        soBuilder.FindProperty("ventilationFanPrefab").objectReferenceValue = fanPrefab;
        soBuilder.FindProperty("lifelinePrefab").objectReferenceValue = lifelinePrefab;
        soBuilder.FindProperty("workbenchPrefab").objectReferenceValue = workbenchPrefab;
        soBuilder.FindProperty("hardhatPrefab").objectReferenceValue = hardhatPrefab;
        if (soBuilder.FindProperty("gogglesPrefab") != null) soBuilder.FindProperty("gogglesPrefab").objectReferenceValue = gogglesPrefab;
        if (soBuilder.FindProperty("glovesPrefab") != null) soBuilder.FindProperty("glovesPrefab").objectReferenceValue = glovesPrefab;
        if (soBuilder.FindProperty("bootsPrefab") != null) soBuilder.FindProperty("bootsPrefab").objectReferenceValue = bootsPrefab;
        soBuilder.ApplyModifiedProperties();

        // Build restored environment using exact real source assets
        builder.BuildEnvironment();

        // Mark scene dirty and save to persist scene hierarchy changes
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        string outputDir = Path.Combine(Application.dataPath, "AR_Gas_foundation/Validation");
        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

        string screenshotsDir = Path.Combine(Application.dataPath, "AR_Gas_foundation/Screenshots");
        if (!Directory.Exists(screenshotsDir)) Directory.CreateDirectory(screenshotsDir);

        // Setup rendering camera
        GameObject camObj = new GameObject("MasterValidationCam");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 50f;
        cam.fieldOfView = 54f;

        // Add Directional Light
        GameObject lightObj = new GameObject("StudioLight");
        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = 1.25f;
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Render Wide Overview Screenshot (PPE Station + Cylinder & Pipeline & Detector + Confined Space Vessel Area)
        camObj.transform.position = new Vector3(-0.15f, 1.15f, -3.30f);
        camObj.transform.LookAt(new Vector3(-0.15f, 0.85f, 0.20f));

        RenderCameraToPNG(cam, Path.Combine(outputDir, "RESTORED_FULL_WORKSTATION.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "WORKSTATION_MASTER_REALISM.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(screenshotsDir, "WORKSTATION_MASTER_REALISM.png"), 1280, 720);

        // Render Closeup Connection Screenshot (Cylinder -> Regulator -> Gauge -> Pipe -> Valve -> Elbow -> Downstream)
        cam.fieldOfView = 38f;
        camObj.transform.position = new Vector3(-0.10f, 0.85f, -1.50f);
        camObj.transform.LookAt(new Vector3(-0.10f, 0.85f, 0.20f));
        RenderCameraToPNG(cam, Path.Combine(outputDir, "WORKSTATION_PIPE_CHAIN_CLOSEUP.png"), 1280, 720);

        // Cleanup temporary render tools
        Object.DestroyImmediate(camObj);
        Object.DestroyImmediate(lightObj);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log($"[GAS MASTER SCENE FIXER] Workstation rebuilt, scene saved, and screenshots written to: {outputDir}");
        Debug.Log("==================================================");
    }

    private static void RenderCameraToPNG(Camera cam, string filePath, int width, int height)
    {
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
        Debug.Log($"Captured screenshot: {filePath}");
    }
}
#endif
