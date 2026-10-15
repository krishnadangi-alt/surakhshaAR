#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public class GasFlowProofCapturer
{
    [MenuItem("SurakshaAR/Render 14 Proof Flow Screenshots")]
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

        GasARPlacement arPlacement = root.GetComponent<GasARPlacement>() ?? Object.FindAnyObjectByType<GasARPlacement>();
        GasEnvironmentBuilder builder = root.GetComponent<GasEnvironmentBuilder>() ?? Object.FindAnyObjectByType<GasEnvironmentBuilder>();
        GasLeakVisualController leakVisual = root.GetComponent<GasLeakVisualController>() ?? Object.FindAnyObjectByType<GasLeakVisualController>();
        GasTrainingStepController stepController = root.GetComponent<GasTrainingStepController>() ?? Object.FindAnyObjectByType<GasTrainingStepController>();

        if (arPlacement != null) arPlacement.ActivateNonArMode();
        if (builder != null) builder.BuildEnvironment();

        // Ensure LineRenderers disabled
        LineRenderer[] lines = Object.FindObjectsByType<LineRenderer>(FindObjectsInactive.Include);
        foreach (var l in lines)
        {
            l.enabled = false;
            l.gameObject.SetActive(false);
        }

        Camera cam = Camera.main ?? Object.FindAnyObjectByType<Camera>();
        if (cam == null)
        {
            Debug.LogError("[CAPTURE ERROR] Main Camera NOT found!");
            return;
        }

        cam.aspect = 1080f / 1920f;
        cam.fieldOfView = 55f;
        cam.nearClipPlane = 0.05f;

        // Ensure proper lighting for PBR 3D models
        GameObject tempKeyLightGO = new GameObject("TempCaptureKeyLight");
        var keyLight = tempKeyLightGO.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.intensity = 1.7f;
        keyLight.color = new Color(1.0f, 0.98f, 0.95f);
        tempKeyLightGO.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

        GameObject tempFillLightGO = new GameObject("TempCaptureFillLight");
        var fillLight = tempFillLightGO.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.intensity = 0.9f;
        fillLight.color = new Color(0.85f, 0.92f, 1.0f);
        tempFillLightGO.transform.rotation = Quaternion.Euler(20f, 145f, 0f);

        RenderTexture rt = new RenderTexture(1080, 1920, 24, RenderTextureFormat.ARGB32);
        Texture2D screenTex = new Texture2D(1080, 1920, TextureFormat.RGBA32, false);

        if (stepController != null)
        {
            stepController.EnsureUIInitialized();
            stepController.PrepareForCameraCapture(cam);
        }

        GameObject cylinderObj = builder != null ? builder.CurrentGasCylinder : GameObject.Find("gas_cylinder");

        // ── 01. STEP 1: AR PLACEMENT ──────────────────────────────
        cam.transform.position = new Vector3(0.00f, 1.45f, -2.50f);
        cam.transform.LookAt(new Vector3(0.00f, 0.10f, 0.50f));
        stepController.SetStep(GasTrainingStepController.TrainingStep.ScanAndPlace);
        stepController.PrepareForCameraCapture(cam);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "01_AR_PLACEMENT.png"));

        // ── 02. STEP 2: WORKSTATION READY ─────────────────────────
        cam.transform.position = new Vector3(0.00f, 1.35f, -2.60f);
        cam.transform.LookAt(new Vector3(0.00f, 0.65f, 0.10f));
        stepController.SetStep(GasTrainingStepController.TrainingStep.WorkstationReady);
        stepController.PrepareForCameraCapture(cam);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "02_WORKSTATION_READY.png"));

        // ── 03. STEP 3: TAP GAS CYLINDER ──────────────────────────
        cam.transform.position = new Vector3(-0.05f, 1.10f, -1.25f);
        cam.transform.LookAt(new Vector3(-0.05f, 0.85f, 0.00f));
        stepController.SetStep(GasTrainingStepController.TrainingStep.CylinderInteraction);
        stepController.PrepareForCameraCapture(cam);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "03_TAP_GAS_CYLINDER.png"));

        // ── 04. STEP 4: GAS LEAK STARTED ──────────────────────────
        cam.transform.position = new Vector3(0.00f, 1.20f, -1.75f);
        cam.transform.LookAt(new Vector3(0.00f, 0.80f, 0.00f));
        if (leakVisual != null && cylinderObj != null)
        {
            leakVisual.StartGasLeak(cylinderObj);
        }
        stepController.SetStep(GasTrainingStepController.TrainingStep.GasLeakActive);
        stepController.PrepareForCameraCapture(cam);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "04_GAS_LEAK_STARTED.png"));

        // ── 05. STEP 5: CHECK GAS DETECTOR ────────────────────────
        cam.transform.position = new Vector3(0.35f, 0.60f, -0.65f);
        cam.transform.LookAt(new Vector3(0.35f, 0.15f, 0.10f));
        stepController.SetStep(GasTrainingStepController.TrainingStep.DetectorInteraction);
        stepController.PrepareForCameraCapture(cam);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "05_CHECK_GAS_DETECTOR.png"));

        // ── 06. STEP 6: ALARM ACTIVATED ───────────────────────────
        cam.transform.position = new Vector3(0.35f, 0.65f, -0.65f);
        cam.transform.LookAt(new Vector3(0.35f, 0.20f, 0.10f));
        stepController.SetStep(GasTrainingStepController.TrainingStep.AlarmActive);
        stepController.PrepareForCameraCapture(cam);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "06_ALARM_ACTIVATED.png"));

        // ── 07. STEP 7: SELECT REQUIRED PPE ───────────────────────
        cam.transform.position = new Vector3(-1.10f, 0.95f, -1.35f);
        cam.transform.LookAt(new Vector3(-1.10f, 0.55f, 0.00f));
        stepController.SetStep(GasTrainingStepController.TrainingStep.PPECheck);
        stepController.PrepareForCameraCapture(cam);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "07_SELECT_REQUIRED_PPE.png"));

        // ── 08. STEP 8: IDENTIFY HAZARD ZONE ──────────────────────
        cam.transform.position = new Vector3(0.00f, 1.50f, -2.70f);
        cam.transform.LookAt(new Vector3(0.00f, 0.40f, 0.00f));
        stepController.SetStep(GasTrainingStepController.TrainingStep.HazardRecognition);
        stepController.PrepareForCameraCapture(cam);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "08_IDENTIFY_HAZARD_ZONE.png"));

        // ── 09. STEP 9: CONFIRM BUDDY SYSTEM ──────────────────────
        cam.transform.position = new Vector3(-0.80f, 1.25f, -2.10f);
        cam.transform.LookAt(new Vector3(-0.70f, 0.80f, 0.00f));
        stepController.SetStep(GasTrainingStepController.TrainingStep.BuddySystemCheck);
        stepController.PrepareForCameraCapture(cam);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "09_CONFIRM_BUDDY_SYSTEM.png"));

        // ── 10. STEP 10: OPERATE ISOLATION VALVE ──────────────────
        cam.transform.position = new Vector3(0.20f, 0.85f, -0.90f);
        cam.transform.LookAt(new Vector3(0.20f, 0.65f, 0.00f));
        stepController.SetStep(GasTrainingStepController.TrainingStep.SafetyResponse);
        stepController.PrepareForCameraCapture(cam);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "10_OPERATE_ISOLATION_VALVE.png"));

        // ── 11. STEP 11: LEAK RESPONSE COMPLETE ───────────────────
        cam.transform.position = new Vector3(0.00f, 1.30f, -2.40f);
        cam.transform.LookAt(new Vector3(0.00f, 0.70f, 0.00f));
        if (leakVisual != null) leakVisual.StopGasLeak();
        stepController.SetStep(GasTrainingStepController.TrainingStep.ResponseComplete);
        stepController.PrepareForCameraCapture(cam);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "11_LEAK_RESPONSE_COMPLETE.png"));

        // ── 12. STEP 12: PROCEED TO CONFINED SPACE ────────────────
        cam.transform.position = new Vector3(1.10f, 1.15f, -1.75f);
        cam.transform.LookAt(new Vector3(1.10f, 0.75f, 0.10f));
        stepController.SetStep(GasTrainingStepController.TrainingStep.ConfinedSpaceIntro);
        stepController.PrepareForCameraCapture(cam);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "12_PROCEED_TO_CONFINED_SPACE.png"));

        // ── 13. STEP 13: COMPLETE CONFINED SPACE TASK ─────────────
        cam.transform.position = new Vector3(1.20f, 0.95f, -1.15f);
        cam.transform.LookAt(new Vector3(1.20f, 0.75f, 0.20f));
        stepController.SetStep(GasTrainingStepController.TrainingStep.ConfinedSpaceDrill);
        stepController.PrepareForCameraCapture(cam);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "13_COMPLETE_CONFINED_SPACE_TASK.png"));

        // ── 14. STEP 14: FINAL ASSESSMENT ─────────────────────────
        cam.transform.position = new Vector3(0.00f, 1.35f, -2.60f);
        cam.transform.LookAt(new Vector3(0.00f, 0.65f, 0.10f));
        stepController.SetStep(GasTrainingStepController.TrainingStep.FinalAssessment);
        stepController.PrepareForCameraCapture(cam);
        CaptureCamera(cam, rt, screenTex, Path.Combine(outputDir, "14_FINAL_ASSESSMENT.png"));

        // Cleanup
        if (stepController != null)
        {
            stepController.RestoreAfterCameraCapture();
        }
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(screenTex);
        Object.DestroyImmediate(tempKeyLightGO);
        Object.DestroyImmediate(tempFillLightGO);

        Debug.Log($"[PROOF CAPTURE SUCCESS] All 14 Authoritative Gas Steps rendered with lighting to: {outputDir}");
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
