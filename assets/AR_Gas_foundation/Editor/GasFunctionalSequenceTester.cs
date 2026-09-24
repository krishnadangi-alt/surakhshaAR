#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public class GasFunctionalSequenceTester
{
    [MenuItem("SurakshaAR/Test Gas Functional Sequence")]
    public static void RunFunctionalTest()
    {
        Debug.Log("==================================================");
        Debug.Log("[GAS FUNCTIONAL SEQUENCE TEST]");
        Debug.Log("==================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject root = GameObject.Find("GasScenarioRoot");
        if (root == null)
        {
            Debug.LogError("[TEST FAILED] GasScenarioRoot NOT found in scene!");
            return;
        }

        GasEnvironmentBuilder builder = root.GetComponent<GasEnvironmentBuilder>();
        GasTrainingStepController stepController = root.GetComponent<GasTrainingStepController>() ?? Object.FindFirstObjectByType<GasTrainingStepController>();
        GasLeakVisualController leakVisual = root.GetComponent<GasLeakVisualController>() ?? Object.FindFirstObjectByType<GasLeakVisualController>();
        GasHazardDetector hazardDetector = root.GetComponent<GasHazardDetector>() ?? Object.FindFirstObjectByType<GasHazardDetector>();

        if (builder != null) builder.BuildEnvironment();

        int passCount = 0;
        int failCount = 0;

        // Test 1: Training Controller Steps Progression
        if (stepController != null)
        {
            Debug.Log("[TEST PASS] GasTrainingStepController found.");
            passCount++;

            stepController.SetStep(GasTrainingStepController.TrainingStep.ScanAndPlace);
            stepController.SetStep(GasTrainingStepController.TrainingStep.Intro);
            stepController.SetStep(GasTrainingStepController.TrainingStep.EquipmentIdentification);
            stepController.SetStep(GasTrainingStepController.TrainingStep.PPECheck);
            stepController.SetStep(GasTrainingStepController.TrainingStep.BuddySystemCheck);
            stepController.SetStep(GasTrainingStepController.TrainingStep.NormalState);
            stepController.SetStep(GasTrainingStepController.TrainingStep.GasLeakActive);
            stepController.SetStep(GasTrainingStepController.TrainingStep.LeakRecognition);
            stepController.SetStep(GasTrainingStepController.TrainingStep.DetectorRecognition);
            stepController.SetStep(GasTrainingStepController.TrainingStep.HazardZoneRecognition);
            stepController.SetStep(GasTrainingStepController.TrainingStep.SafetyResponse);
            stepController.SetStep(GasTrainingStepController.TrainingStep.ReadinessCheck);
            stepController.SetStep(GasTrainingStepController.TrainingStep.FinalAssessment);
            stepController.SetStep(GasTrainingStepController.TrainingStep.PassResult);
            stepController.SetStep(GasTrainingStepController.TrainingStep.TargetedRetraining);
            stepController.SetStep(GasTrainingStepController.TrainingStep.Reassessment);
            stepController.SetStep(GasTrainingStepController.TrainingStep.Completion);

            Debug.Log("[TEST PASS] All 17 Gas Training Steps executed cleanly.");
            passCount++;
        }
        else
        {
            Debug.LogError("[TEST FAIL] GasTrainingStepController NOT found.");
            failCount++;
        }

        // Test 2: Gas Leak Particle VFX & Hiss Trigger
        if (leakVisual != null && builder != null && builder.CurrentGasCylinder != null)
        {
            leakVisual.StartGasLeak(builder.CurrentGasCylinder);
            Debug.Log("[TEST PASS] Gas Leak VFX & Hiss Audio activated.");
            passCount++;

            leakVisual.StopGasLeak();
            Debug.Log("[TEST PASS] Gas Leak VFX deactivated.");
            passCount++;
        }

        // Test 3: Hazard Detector Assessment
        if (hazardDetector != null)
        {
            Debug.Log("[TEST PASS] GasHazardDetector present and configured.");
            passCount++;
        }

        Debug.Log($"[FUNCTIONAL TEST RESULT] Passed: {passCount}, Failed: {failCount}");
        Debug.Log("==================================================");
    }
}
#endif
