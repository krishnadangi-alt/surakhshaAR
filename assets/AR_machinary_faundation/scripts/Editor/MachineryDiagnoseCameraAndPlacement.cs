using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Text;
using System.IO;

public static class MachineryDiagnoseCameraAndPlacement
{
    [MenuItem("Machinery/Diagnose Camera and Placement Setup")]
    public static void Diagnose()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== MACHINERY CAMERA AND PLACEMENT DIAGNOSIS ===");

        EditorSceneManager.OpenScene("Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity", OpenSceneMode.Single);

        var scenario = GameObject.Find("MachineryScenario");
        sb.AppendLine($"MachineryScenario GameObject: {(scenario != null ? "FOUND (active=" + scenario.activeSelf + ")" : "MISSING")}");

        var placementCtrl = Object.FindFirstObjectByType<SurakshaAR.Machinery.MachineryARPlacementController>();
        if (placementCtrl != null)
        {
            var so = new SerializedObject(placementCtrl);
            var targetProp = so.FindProperty("machineryScenarioTarget");
            var targetObj = targetProp?.objectReferenceValue;
            sb.AppendLine($"MachineryARPlacementController: FOUND on {placementCtrl.gameObject.name}");
            sb.AppendLine($"  - machineryScenarioTarget: {(targetObj != null ? targetObj.name : "NULL / UNASSIGNED!")}");
        }
        else
        {
            sb.AppendLine("MachineryARPlacementController: MISSING!");
        }

        // Check Graphics Settings / URP Asset
        var graphicsAsset = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
        sb.AppendLine($"Default Render Pipeline Asset: {(graphicsAsset != null ? graphicsAsset.name + " (" + graphicsAsset.GetType().Name + ")" : "Built-in Render Pipeline (NULL)")}");

        // Check Main Camera setup
        var cam = Camera.main;
        if (cam != null)
        {
            sb.AppendLine($"Main Camera: FOUND ({cam.name})");
            sb.AppendLine($"  - Clear Flags: {cam.clearFlags}");
            sb.AppendLine($"  - Background Color: {cam.backgroundColor}");
            sb.AppendLine($"  - Culling Mask: {cam.cullingMask}");
            sb.AppendLine($"  - Far Clip Plane: {cam.farClipPlane}");
            sb.AppendLine($"  - Near Clip Plane: {cam.nearClipPlane}");
            var camBg = cam.GetComponent<ARCameraBackground>();
            sb.AppendLine($"  - ARCameraBackground: {(camBg != null ? "FOUND (enabled=" + camBg.enabled + ")" : "MISSING!")}");
            var camMgr = cam.GetComponent<ARCameraManager>();
            sb.AppendLine($"  - ARCameraManager: {(camMgr != null ? "FOUND (enabled=" + camMgr.enabled + ")" : "MISSING!")}");
        }
        else
        {
            sb.AppendLine("Main Camera: MISSING!");
        }

        File.WriteAllText("camera_placement_diag_report.txt", sb.ToString());
        Debug.Log(sb.ToString());
    }
}
