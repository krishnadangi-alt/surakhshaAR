#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public class GasRuntimeCapture
{
    [MenuItem("SurakshaAR/Capture Gas Runtime Proof Screenshots")]
    public static void RenderRuntimeProof()
    {
        string outputDir = "C:/SurakhshaAR/ReviewScreenshots/RUNTIME_REPAIR";
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject root = GameObject.Find("GasScenarioRoot");
        if (root == null)
        {
            Debug.LogError("[CAPTURE ERROR] GasScenarioRoot NOT found in scene!");
            return;
        }

        GasARPlacement arPlacement = root.GetComponent<GasARPlacement>() ?? Object.FindFirstObjectByType<GasARPlacement>();
        GasEnvironmentBuilder builder = root.GetComponent<GasEnvironmentBuilder>() ?? Object.FindFirstObjectByType<GasEnvironmentBuilder>();
        GasLeakVisualController leakVisual = root.GetComponent<GasLeakVisualController>() ?? Object.FindFirstObjectByType<GasLeakVisualController>();
        GasTrainingStepController stepController = root.GetComponent<GasTrainingStepController>() ?? Object.FindFirstObjectByType<GasTrainingStepController>();

        // Activate Non-AR Mode & Build Assembly
        if (arPlacement != null) arPlacement.ActivateNonArMode();
        if (builder != null) builder.BuildEnvironment();

        // Ensure LineRenderers are disabled in training runtime
        LineRenderer[] lines = Object.FindObjectsByType<LineRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var l in lines)
        {
            l.enabled = false;
            l.gameObject.SetActive(false);
        }

        Camera cam = Camera.main ?? Object.FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            Debug.LogError("[CAPTURE ERROR] Main Camera NOT found!");
            return;
        }

        // Configure Camera for Portrait Mobile (1080 x 1920)
        cam.aspect = 1080f / 1920f;
        cam.fieldOfView = 55f;
        cam.nearClipPlane = 0.05f;

        RenderTexture rt = new RenderTexture(1080, 1920, 24, RenderTextureFormat.ARGB32);
        Texture2D screenTex = new Texture2D(1080, 1920, TextureFormat.RGBA32, false);

        // Shot 1: FULL WORKSTATION OVERVIEW
        cam.transform.position = new Vector3(0.00f, 1.35f, -2.80f);
        cam.transform.LookAt(new Vector3(0.00f, 0.65f, 0.10f));
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "01_FULL_WORKSTATION.png"));

        // Shot 2: CYLINDER + REGULATOR + GAUGE CLOSEUP
        cam.transform.position = new Vector3(-0.035f, 1.15f, -1.05f);
        cam.transform.LookAt(new Vector3(-0.035f, 0.90f, 0.00f));
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "02_CYLINDER_REGULATOR_GAUGE.png"));

        // Shot 3: PIPELINE + ISOLATION VALVE CLOSEUP
        cam.transform.position = new Vector3(0.35f, 0.85f, -0.95f);
        cam.transform.LookAt(new Vector3(0.20f, 0.55f, 0.00f));
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "03_PIPE_VALVE.png"));

        // Shot 4: MULTI-GAS DETECTOR CLOSEUP
        cam.transform.position = new Vector3(-0.45f, 0.55f, -0.65f);
        cam.transform.LookAt(new Vector3(-0.45f, 0.05f, 0.25f));
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "04_DETECTOR.png"));

        // Shot 5: CONFINED SPACE VESSEL
        cam.transform.position = new Vector3(1.20f, 0.95f, -1.65f);
        cam.transform.LookAt(new Vector3(1.20f, 0.50f, 0.30f));
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "05_CONFINED_SPACE.png"));

        // Shot 6: PPE PREPARATION STATION
        cam.transform.position = new Vector3(-1.10f, 0.95f, -1.45f);
        cam.transform.LookAt(new Vector3(-1.10f, 0.45f, 0.10f));
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "06_PPE_STATION.png"));

        // Reset Camera to Main Inspection Angle
        cam.transform.position = new Vector3(0.00f, 1.35f, -2.80f);
        cam.transform.LookAt(new Vector3(0.00f, 0.65f, 0.10f));

        // Shot 7: NORMAL OPERATING STATE
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "07_NORMAL_STATE.png"));

        // Shot 8: GAS LEAK ACTIVE
        if (leakVisual != null && builder != null && builder.CurrentGasCylinder != null)
        {
            leakVisual.StartGasLeak(builder.CurrentGasCylinder);
        }
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "08_LEAK_ACTIVE.png"));

        // Shot 9: HAZARD ZONE ACTIVE
        GameObject hazardZoneDisk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hazardZoneDisk.name = "HazardZoneVisualDisk";
        hazardZoneDisk.transform.SetParent(root.transform, false);
        hazardZoneDisk.transform.localPosition = new Vector3(0f, 0.01f, 0f);
        hazardZoneDisk.transform.localScale = new Vector3(2.2f, 0.002f, 2.2f);
        Material hazardMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
        hazardMat.color = new Color(1.0f, 0.25f, 0.1f, 0.35f);
        hazardZoneDisk.GetComponent<Renderer>().material = hazardMat;
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "09_HAZARD_ZONE.png"));

        // Shot 10: TRAINING UI SCREEN
        if (stepController != null)
        {
            stepController.SetStep(GasTrainingStepController.TrainingStep.Intro);
        }
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "10_TRAINING_UI.png"));

        // Cleanup temporary render objects
        if (leakVisual != null) leakVisual.StopGasLeak();
        if (hazardZoneDisk != null) Object.DestroyImmediate(hazardZoneDisk);

        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(screenTex);

        Debug.Log($"[GAS RUNTIME CAPTURE COMPLETE] 10 Portrait Proof Screenshots rendered to: {outputDir}");
    }

    private static void CaptureCamera(Camera cam, RenderTexture rt, Texture2D screenTex, string savePath)
    {
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;

        screenTex.ReadPixels(new Rect(0, 0, 1080, 1920), 0, 0);
        screenTex.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;

        byte[] bytes = screenTex.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);
        Debug.Log($"Rendered screenshot: {savePath} ({bytes.Length} bytes)");
    }
}
#endif
