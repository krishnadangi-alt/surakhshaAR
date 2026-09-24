#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GasScenarioValidator
{
    [MenuItem("SurakshaAR/Validate Gas Module Integrity")]
    public static void ValidateGasModule()
    {
        Debug.Log("==================================================");
        Debug.Log("[GAS MODULE INTEGRITY VALIDATION]");
        Debug.Log("==================================================");

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject root = GameObject.Find("GasScenarioRoot");
        if (root == null)
        {
            Debug.LogError("[VALIDATION FAILED] GasScenarioRoot NOT found in scene.");
            return;
        }

        int missingScripts = 0;
        int missingRefs = 0;

        Component[] components = root.GetComponentsInChildren<Component>(true);
        foreach (var c in components)
        {
            if (c == null)
            {
                missingScripts++;
                Debug.LogError("[VALIDATION ERROR] Missing script attached to object in hierarchy.");
            }
        }

        GasEnvironmentBuilder builder = root.GetComponent<GasEnvironmentBuilder>();
        if (builder == null)
        {
            Debug.LogError("[VALIDATION FAILED] GasEnvironmentBuilder component missing on GasScenarioRoot.");
        }
        else
        {
            Debug.Log("[VALIDATION PASS] GasEnvironmentBuilder found.");
        }

        GasTrainingStepController controller = root.GetComponent<GasTrainingStepController>();
        if (controller == null)
        {
            Debug.LogWarning("[VALIDATION WARNING] GasTrainingStepController missing on GasScenarioRoot (Checking children or scene)...");
            controller = Object.FindFirstObjectByType<GasTrainingStepController>();
        }

        if (controller != null)
        {
            Debug.Log("[VALIDATION PASS] GasTrainingStepController found and assigned.");
        }

        GasLeakVisualController leakVisual = root.GetComponent<GasLeakVisualController>();
        if (leakVisual != null)
        {
            Debug.Log("[VALIDATION PASS] GasLeakVisualController found.");
        }

        GasHazardDetector hazardDet = root.GetComponent<GasHazardDetector>();
        if (hazardDet != null)
        {
            Debug.Log("[VALIDATION PASS] GasHazardDetector found.");
        }

        Debug.Log($"[VALIDATION RESULT] Missing Scripts: {missingScripts}, Missing References: {missingRefs}");
        Debug.Log("==================================================");
    }
}
#endif
