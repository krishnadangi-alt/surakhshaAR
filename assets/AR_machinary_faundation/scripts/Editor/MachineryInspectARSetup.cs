using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Text;
using System.IO;

public static class MachineryInspectARSetup
{
    [MenuItem("Machinery/Inspect AR Setup")]
    public static void Inspect()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== AR SETUP INSPECTION ===");

        // 1. Inspect MachineryConveyorJam.unity
        var scene = EditorSceneManager.OpenScene("Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity", OpenSceneMode.Single);
        sb.AppendLine("\n--- MachineryConveyorJam.unity ---");
        InspectSceneAR(sb);

        // 2. Inspect FireTraining.unity
        var fireScene = EditorSceneManager.OpenScene("Assets/AR_Fire_foundation/scenes/FireTraining.unity", OpenSceneMode.Single);
        sb.AppendLine("\n--- FireTraining.unity ---");
        InspectSceneAR(sb);

        File.WriteAllText("ar_setup_comparison_report.txt", sb.ToString());
        Debug.Log(sb.ToString());
    }

    private static void InspectSceneAR(StringBuilder sb)
    {
        var arSession = Object.FindFirstObjectByType<ARSession>();
        sb.AppendLine($"ARSession: {(arSession != null ? "FOUND (" + arSession.name + ", enabled=" + arSession.enabled + ")" : "MISSING")}");

        var xrOrigin = Object.FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
        sb.AppendLine($"XROrigin: {(xrOrigin != null ? "FOUND (" + xrOrigin.name + ", enabled=" + xrOrigin.enabled + ")" : "MISSING")}");

        var cam = Camera.main;
        if (cam != null)
        {
            sb.AppendLine($"Main Camera: FOUND ({cam.name}, clearFlags={cam.clearFlags}, bg={cam.backgroundColor})");
            var comps = cam.GetComponents<Component>();
            foreach (var c in comps)
            {
                if (c != null)
                    sb.AppendLine($"  - Camera Component: {c.GetType().Name} (enabled={(c is Behaviour b ? b.enabled.ToString() : "N/A")})");
            }
        }
        else
        {
            sb.AppendLine("Main Camera: MISSING");
        }

        var planeMgr = Object.FindFirstObjectByType<ARPlaneManager>();
        sb.AppendLine($"ARPlaneManager: {(planeMgr != null ? "FOUND (" + planeMgr.name + ", enabled=" + planeMgr.enabled + ")" : "MISSING")}");

        var raycastMgr = Object.FindFirstObjectByType<ARRaycastManager>();
        sb.AppendLine($"ARRaycastManager: {(raycastMgr != null ? "FOUND (" + raycastMgr.name + ", enabled=" + raycastMgr.enabled + ")" : "MISSING")}");
    }
}
