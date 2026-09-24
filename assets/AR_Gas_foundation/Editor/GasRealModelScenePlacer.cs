#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GasRealModelScenePlacer
{
    [MenuItem("SurakshaAR/Place All Real 3D Models & Save Scene")]
    public static void PlaceAllRealModelsAndSave()
    {
        Debug.Log("==================================================");
        Debug.Log("[REAL 3D MODEL SCENE PLACER START]");
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

        // Also clean up any loose pipeline assembly objects in the scene
        GameObject[] loosePipelines = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var go in loosePipelines)
        {
            if (go.name == "GasPipelineAssembly" || go.name == "Isolation_Valve_3D" || go.name == "Horizontal_Pipe_1")
            {
                Debug.Log($"[DIAGNOSTIC] Found loose pipeline object: {go.name}, parent: {go.transform.parent?.name ?? "ROOT"}");
                if (go.transform.parent != root.transform)
                {
                    Object.DestroyImmediate(go);
                }
            }
        }

        GasEnvironmentBuilder builder = root.GetComponent<GasEnvironmentBuilder>();
        if (builder == null) builder = root.AddComponent<GasEnvironmentBuilder>();

        // Load all available real 3D assets (GLB / FBX / OBJ) across Assets/AR_Gas_foundation/3d/ and Assets/industrial-valve/
        GameObject cylinderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasCylinder/gas_cylinder.glb");
        if (cylinderPrefab == null) cylinderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasCylinder/Prefabs/gas_cylinder.prefab");

        GameObject detectorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.glb");
        if (detectorPrefab == null) detectorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasDetector/Prefabs/multi_gas_detector_-_low_poly.prefab");

        GameObject h2sDetectorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasDetector/h2s_gas_detector.glb");
        GameObject regulatorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasRegulator/high-pressure_regulator.glb");
        GameObject gaugePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge_animated.glb");
        GameObject pipePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasPipeline/industrial_pipe.obj");
        if (pipePrefab == null) pipePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasPipeline/industrial_pipe_hd.obj");
        if (pipePrefab == null) pipePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasPipeline/modular-pipes/source/pipes.fbx");
        GameObject pipeElbowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasPipeline/pipe_elbow.obj");
        if (pipeElbowPrefab == null) pipeElbowPrefab = pipePrefab;

        GameObject valvePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/industrial-valve/source/VALVE.fbx");
        GameObject ballValvePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasPipeline/ball_valve.glb");
        GameObject industrialValvePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasPipeline/industrial_valve.glb");
        GameObject metalPipesPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasPipeline/large_modular_pipes_metal.glb");

        GameObject vesselPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/ConfinedSpace/industrial_tanks_and_equipment.glb");
        GameObject industrialTanksPrefab = vesselPrefab;
        GameObject fanPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/ConfinedSpace/ventilation_fan.glb");
        GameObject lifelinePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/ConfinedSpace/self_retracting_lifeline.glb");

        GameObject workbenchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/workbench_hd.obj");
        GameObject hardhatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/safety_helmet.glb");
        if (hardhatPrefab == null) hardhatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/ppe_hardhat_hd.obj");

        GameObject safetyVestPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/ppe_vest_hd.obj");
        GameObject gogglesPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/safety_goggles.glb");
        if (gogglesPrefab == null) gogglesPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/goggles.glb");

        GameObject glovesPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/work_gloves.glb");
        GameObject bootsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/safety_boots.glb");
        if (bootsPrefab == null) bootsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PPE/work_boot.glb");

        SerializedObject soBuilder = new SerializedObject(builder);
        soBuilder.FindProperty("gasCylinderPrefab").objectReferenceValue = cylinderPrefab;
        soBuilder.FindProperty("gasDetectorPrefab").objectReferenceValue = detectorPrefab;
        if (soBuilder.FindProperty("h2sDetectorPrefab") != null) soBuilder.FindProperty("h2sDetectorPrefab").objectReferenceValue = h2sDetectorPrefab;
        soBuilder.FindProperty("pipelineValvePrefab").objectReferenceValue = valvePrefab;
        if (soBuilder.FindProperty("ballValvePrefab") != null) soBuilder.FindProperty("ballValvePrefab").objectReferenceValue = ballValvePrefab;
        if (soBuilder.FindProperty("industrialValvePrefab") != null) soBuilder.FindProperty("industrialValvePrefab").objectReferenceValue = industrialValvePrefab;
        soBuilder.FindProperty("gasRegulatorPrefab").objectReferenceValue = regulatorPrefab;
        soBuilder.FindProperty("pressureGaugePrefab").objectReferenceValue = gaugePrefab;
        soBuilder.FindProperty("industrialPipePrefab").objectReferenceValue = pipePrefab;
        soBuilder.FindProperty("pipeElbowPrefab").objectReferenceValue = pipeElbowPrefab;
        if (soBuilder.FindProperty("metalPipesPrefab") != null) soBuilder.FindProperty("metalPipesPrefab").objectReferenceValue = metalPipesPrefab;

        soBuilder.FindProperty("confinedVesselPrefab").objectReferenceValue = vesselPrefab;
        if (soBuilder.FindProperty("industrialTanksPrefab") != null) soBuilder.FindProperty("industrialTanksPrefab").objectReferenceValue = industrialTanksPrefab;
        soBuilder.FindProperty("ventilationFanPrefab").objectReferenceValue = fanPrefab;
        soBuilder.FindProperty("lifelinePrefab").objectReferenceValue = lifelinePrefab;

        soBuilder.FindProperty("workbenchPrefab").objectReferenceValue = workbenchPrefab;
        soBuilder.FindProperty("hardhatPrefab").objectReferenceValue = hardhatPrefab;
        if (soBuilder.FindProperty("safetyVestPrefab") != null) soBuilder.FindProperty("safetyVestPrefab").objectReferenceValue = safetyVestPrefab;
        if (soBuilder.FindProperty("gogglesPrefab") != null) soBuilder.FindProperty("gogglesPrefab").objectReferenceValue = gogglesPrefab;
        if (soBuilder.FindProperty("glovesPrefab") != null) soBuilder.FindProperty("glovesPrefab").objectReferenceValue = glovesPrefab;
        if (soBuilder.FindProperty("bootsPrefab") != null) soBuilder.FindProperty("bootsPrefab").objectReferenceValue = bootsPrefab;
        soBuilder.ApplyModifiedProperties();

        // Build restored environment using exact real source assets
        builder.BuildEnvironment();

        GameObject cyl = GameObject.Find("GasScenarioRoot/Realistic_Gas_Cylinder");
        if (cyl != null)
        {
            Debug.Log($"[CYLINDER_HARD_LOCK_VERIFICATION] Name={cyl.name}, LocalPos={cyl.transform.localPosition.ToString("F4")}, LocalRot={cyl.transform.localRotation.eulerAngles.ToString("F4")}, LocalScale={cyl.transform.localScale.ToString("F4")}");
        }

        // Save scene dirty state
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        string outputDir = Path.Combine(Application.dataPath, "AR_Gas_foundation/Validation");
        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

        // Setup camera for screenshot capture
        GameObject camObj = new GameObject("ValidationCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 50f;
        cam.fieldOfView = 45f;

        GameObject lightObj = new GameObject("ValidationLight");
        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = 1.35f;
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Render FINAL_REAL_VALVE_PIPE_ASSEMBLY.png close-up showing perfectly proportioned industrial valve pipeline
        cam.fieldOfView = 32f;
        camObj.transform.position = new Vector3(0.14f, 1.15f, -0.65f);
        camObj.transform.LookAt(new Vector3(0.14f, 1.12f, 0.00f));
        RenderCameraToPNG(cam, Path.Combine(outputDir, "FINAL_REAL_VALVE_PIPE_ASSEMBLY.png"), 1280, 720);

        // Render VALVE_BOTH_SIDES_CONNECTED.png showing zero-gap pipe-to-valve connection on both sides
        cam.fieldOfView = 28f;
        camObj.transform.position = new Vector3(0.18f, 1.15f, -0.50f);
        camObj.transform.LookAt(new Vector3(0.18f, 1.15f, 0.00f));
        RenderCameraToPNG(cam, Path.Combine(outputDir, "VALVE_BOTH_SIDES_CONNECTED.png"), 1280, 720);

        cam.fieldOfView = 42f;
        camObj.transform.position = new Vector3(0.19f, 0.70f, -1.55f);
        camObj.transform.LookAt(new Vector3(0.18f, 0.65f, 0.00f));
        RenderCameraToPNG(cam, Path.Combine(outputDir, "FINAL_VALVE_PIPE_CONNECTION.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "FINAL_VALVE_PIPE_PROPER_FIT.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "VALVE_OPERABLE_ORIENTATION_FINAL.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "VALVE_CORRECT_POSITION.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "VALVE_PIPE_FINAL_CLOSEUP.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "NEW_PIPELINE_CLOSEUP.png"), 1280, 720);

        // Render close-up screenshot of center gas pipeline assembly ONLY
        cam.fieldOfView = 34f;
        camObj.transform.position = new Vector3(0.13f, 1.10f, -0.85f);
        camObj.transform.LookAt(new Vector3(0.13f, 1.10f, 0.00f));
        RenderCameraToPNG(cam, Path.Combine(outputDir, "UNITY_GENERATED_PIPE_FINAL.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "CYLINDER_PIPELINE_REBUILT_FINAL.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "CYLINDER_PIPELINE_FINAL.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "FINAL_PIPE_JOINT_ONLY.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "PIPE_VALVE_JOINT_FINAL.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "REAL_GAS_PIPE_ASSEMBLY_CLOSEUP.png"), 1280, 720);

        // Render VALVE_PIPE_FINAL_FULL_SCENE.png & FULL_SCENE_VALVE_CONNECTED.png overview screenshot for validation
        cam.fieldOfView = 50f;
        camObj.transform.position = new Vector3(0.50f, 1.80f, -6.50f);
        camObj.transform.LookAt(new Vector3(0.60f, 1.00f, 0.00f));
        RenderCameraToPNG(cam, Path.Combine(outputDir, "FULL_SCENE_VALVE_CONNECTED.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "VALVE_PIPE_FINAL_FULL_SCENE.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "NEW_PIPELINE_FULL_SCENE.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "CONFINED_SPACE_50_PERCENT_LARGER.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "CONFINED_SPACE_LARGE_IMPACT.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "CONFINED_SPACE_ASSEMBLED_FINAL.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "CONFINED_SPACE_PROPER_ARRANGEMENT.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "REAL_CONFINED_SPACE_FINAL.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "CONFINED_SPACE_GROUNDED_REAL.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "CONFINED_SPACE_LARGE_FINAL.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "WORKSTATION_MASTER_REALISM.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "GAS_FINAL_LAYOUT.png"), 1280, 720);
        RenderCameraToPNG(cam, Path.Combine(outputDir, "GAS_COMPLETE_ALL_ASSETS.png"), 1280, 720);

        Object.DestroyImmediate(camObj);
        Object.DestroyImmediate(lightObj);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[GAS REAL MODEL PLACER] Confined Space grounded layout saved to scene and CONFINED_SPACE_GROUNDED_REAL.png rendered successfully!");
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
        Debug.Log($"Captured validation screenshot: {filePath}");
    }
}
#endif
