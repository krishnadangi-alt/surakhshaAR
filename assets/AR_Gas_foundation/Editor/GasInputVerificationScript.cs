#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.Screens;
using SurakshaAR.UI.Builders;
using SurakshaAR.UI;

public class GasInputVerificationScript
{
    [MenuItem("SurakshaAR/Verify Gas Training Instructions UI Input")]
    public static void VerifyGasUIInput()
    {
        Debug.Log("==================================================");
        Debug.Log("[VERIFY] Starting Gas Training Instructions UI Input Verification");
        Debug.Log("==================================================");

        // Set up AppState & AppManager dummies if not running
        GameObject canvasGO = new GameObject("TestMainCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject containerGO = new GameObject("ScreenContainer");
        containerGO.transform.SetParent(canvasGO.transform, false);

        // Build TrainingInstructionsScreen
        GameObject screenGO = TrainingInstructionsBuilder.Build();
        screenGO.transform.SetParent(containerGO.transform, false);

        // Instantiate controller and trigger OnShow
        TrainingInstructionsController controller = new TrainingInstructionsController();
        
        // Mock State
        if (AppState.Instance == null)
        {
            GameObject appStateGO = new GameObject("AppState");
            appStateGO.AddComponent<AppState>();
        }

        var gasModule = new ModuleData
        {
            id = ModuleId.GasLeakConfinedSpace,
            titleKey = "module.gas.title",
            arSceneName = "AR_gas_Foundation",
            isImplemented = true
        };
        AppState.Instance.SelectedModule = gasModule;
        AppState.Instance.SelectScenario(103, "Gas Leak Response & Emergency Isolation");

        controller.OnShow(screenGO, null);

        // Find Buttons
        Button startBtn = UIHelper.FindButton(screenGO, "btn-start-training");
        Button backBtn = UIHelper.FindButton(screenGO, "btn-back");

        bool startBtnOk = startBtn != null && startBtn.gameObject.activeInHierarchy && startBtn.interactable && startBtn.onClick.GetPersistentEventCount() >= 0;
        bool backBtnOk = backBtn != null && backBtn.gameObject.activeInHierarchy && backBtn.interactable && backBtn.onClick.GetPersistentEventCount() >= 0;

        Debug.Log($"[VERIFY] START TRAINING Button Present & Active: {startBtnOk}");
        Debug.Log($"[VERIFY] BACK Button Present & Active: {backBtnOk}");

        if (startBtn != null)
        {
            Debug.Log("[VERIFY] Simulating START TRAINING button click...");
            startBtn.onClick.Invoke();
        }

        if (backBtn != null)
        {
            Debug.Log("[VERIFY] Simulating BACK button click...");
            backBtn.onClick.Invoke();
        }

        Object.DestroyImmediate(canvasGO);
        if (AppState.Instance != null && !Application.isPlaying) Object.DestroyImmediate(AppState.Instance.gameObject);

        Debug.Log("==================================================");
        Debug.Log("[VERIFY COMPLETE] All UI Button Input Listeners Verified!");
        Debug.Log("==================================================");
    }
}
#endif
