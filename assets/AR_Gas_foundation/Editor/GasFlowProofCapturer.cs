#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public class GasFlowProofCapturer
{
    [MenuItem("SurakshaAR/Render 11 Proof Flow Screenshots")]
    public static void RenderProofFlow()
    {
        string outputDir = "C:/SurakhshaAR/ReviewScreenshots/PROOF_FLOW";
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

        if (arPlacement != null) arPlacement.ActivateNonArMode();
        if (builder != null) builder.BuildEnvironment();

        // Ensure LineRenderers disabled
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

        cam.aspect = 1080f / 1920f;
        cam.fieldOfView = 55f;
        cam.nearClipPlane = 0.05f;

        RenderTexture rt = new RenderTexture(1080, 1920, 24, RenderTextureFormat.ARGB32);
        Texture2D screenTex = new Texture2D(1080, 1920, TextureFormat.RGBA32, false);

        // Position camera for workstation overview view
        cam.transform.position = new Vector3(0.00f, 1.35f, -2.80f);
        cam.transform.LookAt(new Vector3(0.00f, 0.65f, 0.10f));

        // Shot 01: NORMAL WORKSTATION
        stepController.SetStep(GasTrainingStepController.TrainingStep.WorkstationReady);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "01_NORMAL_WORKSTATION.png"));

        // Shot 02: CYLINDER INSTRUCTION
        stepController.SetStep(GasTrainingStepController.TrainingStep.CylinderInteraction);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "02_CYLINDER_INSTRUCTION.png"));

        // Shot 03: LEAK ACTIVE
        GameObject cylinderObj = builder != null ? builder.CurrentGasCylinder : GameObject.Find("gas_cylinder");
        if (leakVisual != null && cylinderObj != null)
        {
            leakVisual.StartGasLeak(cylinderObj);
        }
        stepController.SetStep(GasTrainingStepController.TrainingStep.GasLeakActive);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "03_LEAK_ACTIVE.png"));

        // Shot 04: DETECTOR INSTRUCTION
        stepController.SetStep(GasTrainingStepController.TrainingStep.DetectorInteraction);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "04_DETECTOR_INSTRUCTION.png"));

        // Shot 05: DETECTOR SELECTED & READING
        stepController.SetStep(GasTrainingStepController.TrainingStep.DetectorChecked);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "05_DETECTOR_SELECTED_AND_READING.png"));

        // Shot 06: HAZARD RECOGNITION
        stepController.SetStep(GasTrainingStepController.TrainingStep.HazardRecognition);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "06_HAZARD_RECOGNITION.png"));

        // Shot 07: PPE STEP
        stepController.SetStep(GasTrainingStepController.TrainingStep.PPECheck);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "07_PPE_STEP.png"));

        // Shot 08: BUDDY STEP
        stepController.SetStep(GasTrainingStepController.TrainingStep.BuddySystemCheck);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "08_BUDDY_STEP.png"));

        // Shot 09: VALVE INTERACTION
        stepController.SetStep(GasTrainingStepController.TrainingStep.SafetyResponse);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "09_VALVE_INTERACTION.png"));

        // Shot 10: LEAK ISOLATED
        if (leakVisual != null) leakVisual.StopGasLeak();
        stepController.SetStep(GasTrainingStepController.TrainingStep.IncidentResolved);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "10_LEAK_ISOLATED.png"));

        // Shot 11: ASSESSMENT RESULT
        stepController.ForceCompleteInteractions();
        stepController.SetStep(GasTrainingStepController.TrainingStep.PassResult);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "11_ASSESSMENT_RESULT.png"));

        // Cleanup
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(screenTex);

        Debug.Log($"[PROOF CAPTURE SUCCESS] 11 Proof Flow Screenshots rendered to: {outputDir}");
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
