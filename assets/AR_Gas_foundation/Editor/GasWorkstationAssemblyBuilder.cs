#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public class GasWorkstationAssemblyBuilder
{
    [MenuItem("SurakshaAR/Build & Capture Final Real Workstation")]
    public static void BuildAndCaptureFinalWorkstation()
    {
        Debug.Log("==================================================");
        Debug.Log("[FINAL REALISTIC WORKSTATION ASSEMBLY & CAPTURE]");
        Debug.Log("==================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Remove Temporary Staging Nodes or Duplicate Scene Instances outside GasScenarioRoot
        GameObject stagingTemp = GameObject.Find("GasAssetStaging_TEMP");
        if (stagingTemp != null) Object.DestroyImmediate(stagingTemp);

        // Clean up any stray non-root scene objects that overlap
        GameObject[] rootObjs = scene.GetRootGameObjects();
        foreach (var obj in rootObjs)
        {
            if (obj != null && obj.name != "GasScenarioRoot" && obj.name != "Main Camera" && obj.name != "Directional Light" && obj.name != "AR Session" && obj.name != "AR Session Origin" && obj.name != "SurakshaAR_GasTrainingCanvas")
            {
                if (obj.name.Contains("Boot") || obj.name.Contains("clove") || obj.name.Contains("pipe") || obj.name.Contains("valve") || obj.name.Contains("Cylinder") || obj.name.Contains("vessel") || obj.name.Contains("Canvas"))
                {
                    Object.DestroyImmediate(obj);
                    Debug.Log($"[CLEANUP] Destroyed stray scene instance: {obj.name}");
                }
            }
        }

        // Clean up any old duplicate canvases safely
        Canvas[] existingCanvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in existingCanvases)
        {
            if (c != null && c.gameObject != null && c.gameObject.name != "SurakshaAR_GasTrainingCanvas")
            {
                Object.DestroyImmediate(c.gameObject);
                Debug.Log("[CLEANUP] Destroyed duplicate scene canvas");
            }
        }

        // 2. Find GasScenarioRoot and GasEnvironmentBuilder
        GameObject root = GameObject.Find("GasScenarioRoot");
        if (root == null)
        {
            Debug.LogError("[ASSEMBLY FAILED] GasScenarioRoot NOT found in scene!");
            return;
        }

        GasEnvironmentBuilder builder = root.GetComponent<GasEnvironmentBuilder>();
        if (builder == null)
        {
            builder = root.AddComponent<GasEnvironmentBuilder>();
        }

        GasARPlacement placement = root.GetComponent<GasARPlacement>();
        if (placement != null)
        {
            placement.ActivateNonArMode();
        }

        // 3. Assign Pristine HD 1:1 Scale Assets to Builder Serialized Properties
        SerializedObject so = new SerializedObject(builder);

        SetPrefabProperty(so, "gasCylinderPrefab", "Assets/AR_Gas_foundation/3d/GasCylinder/Prefabs/gas_cylinder_real.prefab");
        SetPrefabProperty(so, "gasDetectorPrefab", "Assets/AR_Gas_foundation/3d/GasDetector/Prefabs/h2s_gas_detector_real.prefab");
        SetPrefabProperty(so, "pipelineValvePrefab", "Assets/AR_Gas_foundation/3d/GasPipeline/Prefabs/industrial_valve_real.prefab");
        SetPrefabProperty(so, "gasRegulatorPrefab", "Assets/AR_Gas_foundation/3d/GasRegulator/Prefabs/high-pressure_regulator.prefab");
        SetPrefabProperty(so, "pressureGaugePrefab", "Assets/AR_Gas_foundation/3d/PressureGauge/Prefabs/pressure_gauge_real.prefab");
        SetPrefabProperty(so, "industrialPipePrefab", "Assets/AR_Gas_foundation/3d/GasPipeline/industrial_pipe.prefab");
        SetPrefabProperty(so, "pipeElbowPrefab", "Assets/AR_Gas_foundation/3d/GasPipeline/pipe_elbow.prefab");
        SetPrefabProperty(so, "confinedVesselPrefab", "Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs/confined_space_tank_real.prefab");
        SetPrefabProperty(so, "ventilationFanPrefab", "Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs/ventilation_fan_real.prefab");
        SetPrefabProperty(so, "lifelinePrefab", "Assets/AR_Gas_foundation/3d/ConfinedSpace/Prefabs/retracting_lifeline_real.prefab");
        SetPrefabProperty(so, "workbenchPrefab", "Assets/AR_Gas_foundation/3d/PPE/workbench.prefab");
        SetPrefabProperty(so, "hardhatPrefab", "Assets/AR_Gas_foundation/3d/PPE/Prefabs/safety_helmet_real.prefab");
        SetPrefabProperty(so, "gogglesPrefab", "Assets/AR_Gas_foundation/3d/PPE/Prefabs/safety_goggles_real.prefab");
        SetPrefabProperty(so, "glovesPrefab", "Assets/AR_Gas_foundation/3d/PPE/Prefabs/work_gloves_real.prefab");
        SetPrefabProperty(so, "bootsPrefab", "Assets/AR_Gas_foundation/3d/PPE/Prefabs/safety_boots_real.prefab");

        SetMaterialProperty(so, "vesselTankMaterial", "Assets/AR_Gas_foundation/Materials/MAT_Vessel_Tank.mat");

        bool applied = so.ApplyModifiedProperties();
        Debug.Log($"[SERIALIZATION] ApplyModifiedProperties result: {applied}");

        // Inspect GLB assets details
        InspectAsset("Regulator", "Assets/AR_Gas_foundation/3d/GasRegulator/high-pressure_regulator.glb");
        InspectAsset("Gauge", "Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge.glb");
        InspectAsset("Valve", "Assets/AR_Gas_foundation/3d/GasPipeline/industrial_valve.glb");
        InspectAsset("Pipe", "Assets/AR_Gas_foundation/3d/GasPipeline/large_modular_pipes_metal.glb");

        // 4. Build Environment
        builder.BuildEnvironment();

        GasTrainingStepController stepCtrl = root.GetComponent<GasTrainingStepController>();
        if (stepCtrl != null)
        {
            stepCtrl.SetStep(GasTrainingStepController.TrainingStep.Intro);
        }

        // 5. Ensure Reticle LineRenderers and AR planes are Disabled for Clean Rendering
        GasARPlacement placementScript = root.GetComponent<GasARPlacement>();
        if (placementScript != null) placementScript.HidePlanes();

        LineRenderer[] lines = Object.FindObjectsByType<LineRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var l in lines) if (l != null) l.enabled = false;

        // 6. Camera Setup & Capture (Portrait 1080x1920 & Landscape 1920x1080)
        Camera cam = Camera.main ?? Object.FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null) canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            string screenshotDir = "C:/SurakhshaAR/ReviewScreenshots";
            if (!Directory.Exists(screenshotDir)) Directory.CreateDirectory(screenshotDir);

            // A. Portrait 1080 x 1920 Capture
            cam.aspect = 1080f / 1920f;
            cam.fieldOfView = 56f;
            cam.nearClipPlane = 0.05f;
            cam.transform.position = new Vector3(-0.15f, 0.82f, -3.85f);
            cam.transform.LookAt(new Vector3(-0.15f, 0.65f, 0.10f));

            string savePath = Path.Combine(screenshotDir, "GAS_FINAL_VISUAL_REVIEW.png");
            RenderTexture rt = new RenderTexture(1080, 1920, 24, RenderTextureFormat.ARGB32);
            Texture2D screenTex = new Texture2D(1080, 1920, TextureFormat.RGBA32, false);

            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;

            screenTex.ReadPixels(new Rect(0, 0, 1080, 1920), 0, 0);
            screenTex.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;

            byte[] bytes = screenTex.EncodeToPNG();
            File.WriteAllBytes(savePath, bytes);
            File.WriteAllBytes("C:/SurakhshaAR/GAS_FINAL_VISUAL_REVIEW_SHARE.png", bytes);

            string artifactDir = "C:/Users/anjan/.gemini/antigravity/brain/6a0ab234-4e28-423a-9f0f-f4bae4b8c729";
            if (Directory.Exists(artifactDir))
            {
                File.WriteAllBytes(Path.Combine(artifactDir, "GAS_FINAL_VISUAL_REVIEW_SHARE.png"), bytes);
            }

            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(screenTex);

            Debug.Log($"[SCREENSHOT CAPTURED PORTRAIT] saved to: {savePath} ({bytes.Length} bytes)");

            // B. Landscape 1920 x 1080 Capture
            cam.aspect = 1920f / 1080f;
            cam.fieldOfView = 52f;
            cam.transform.position = new Vector3(-0.15f, 0.82f, -2.85f);
            cam.transform.LookAt(new Vector3(-0.15f, 0.65f, 0.10f));

            string savePathLand = Path.Combine(screenshotDir, "GAS_LANDSCAPE_VISUAL_REVIEW.png");
            RenderTexture rtLand = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            Texture2D screenTexLand = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);

            cam.targetTexture = rtLand;
            cam.Render();
            RenderTexture.active = rtLand;

            screenTexLand.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            screenTexLand.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;

            byte[] bytesLand = screenTexLand.EncodeToPNG();
            File.WriteAllBytes(savePathLand, bytesLand);
            File.WriteAllBytes("C:/SurakhshaAR/GAS_LANDSCAPE_VISUAL_REVIEW_SHARE.png", bytesLand);

            if (Directory.Exists(artifactDir))
            {
                File.WriteAllBytes(Path.Combine(artifactDir, "GAS_LANDSCAPE_VISUAL_REVIEW_SHARE.png"), bytesLand);
            }

            Object.DestroyImmediate(rtLand);
            Object.DestroyImmediate(screenTexLand);

            Debug.Log($"[SCREENSHOT CAPTURED LANDSCAPE] saved to: {savePathLand} ({bytesLand.Length} bytes)");

            // C. Gas System Close-up 1920 x 1080 Capture
            cam.aspect = 1920f / 1080f;
            cam.fieldOfView = 38f;
            cam.transform.position = new Vector3(0.00f, 1.15f, -1.35f);
            cam.transform.LookAt(new Vector3(0.00f, 1.05f, 0.20f));

            string savePathClose = Path.Combine(screenshotDir, "GAS_SYSTEM_CLOSEUP.png");
            RenderTexture rtClose = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            Texture2D screenTexClose = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);

            cam.targetTexture = rtClose;
            cam.Render();
            RenderTexture.active = rtClose;

            screenTexClose.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            screenTexClose.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;

            byte[] bytesClose = screenTexClose.EncodeToPNG();
            File.WriteAllBytes(savePathClose, bytesClose);
            File.WriteAllBytes("C:/SurakhshaAR/GAS_SYSTEM_CLOSEUP_SHARE.png", bytesClose);

            if (Directory.Exists(artifactDir))
            {
                File.WriteAllBytes(Path.Combine(artifactDir, "GAS_SYSTEM_CLOSEUP_SHARE.png"), bytesClose);
            }

            Object.DestroyImmediate(rtClose);
            Object.DestroyImmediate(screenTexClose);

            Debug.Log($"[SCREENSHOT CAPTURED CLOSEUP] saved to: {savePathClose} ({bytesClose.Length} bytes)");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[WORKSTATION ASSEMBLY COMPLETE] Scene saved successfully!");
        Debug.Log("==================================================");
    }

    private static void InspectAsset(string label, string assetPath)
    {
        GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (go == null)
        {
            Debug.LogError($"[INSPECT] {label} asset not found at {assetPath}");
            return;
        }

        GameObject inst = Object.Instantiate(go);
        Renderer[] rends = inst.GetComponentsInChildren<Renderer>();
        Debug.Log($"=== [INSPECT {label}] ===");
        Debug.Log($"Root Name: {inst.name}, Child Count: {inst.transform.childCount}, Renderers Count: {rends.Length}");
        foreach (var r in rends)
        {
            Debug.Log($"  Renderer: {r.name}, Bounds Center: {r.bounds.center.ToString("F4")}, Bounds Size: {r.bounds.size.ToString("F4")}");
        }
        Object.DestroyImmediate(inst);
    }

    private static void SetPrefabProperty(SerializedObject so, string propName, string assetPath)
    {
        SerializedProperty prop = so.FindProperty(propName);
        if (prop != null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null)
            {
                prop.objectReferenceValue = prefab;
                Debug.Log($"[ASSIGNED ASSET] {propName} -> {assetPath}");
            }
            else
            {
                Debug.LogWarning($"[ASSIGN WARNING] Asset NOT found at: {assetPath}");
            }
        }
    }

    private static void SetMaterialProperty(SerializedObject so, string propName, string assetPath)
    {
        SerializedProperty prop = so.FindProperty(propName);
        if (prop != null)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (mat != null)
            {
                prop.objectReferenceValue = mat;
                Debug.Log($"[ASSIGNED MATERIAL] {propName} -> {assetPath}");
            }
            else
            {
                Debug.LogWarning($"[ASSIGN WARNING] Material asset NOT found at: {assetPath}");
            }
        }
    }
}
#endif
