using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Text;
using System.IO;

public static class FireInspectARSetupDetails
{
    [MenuItem("Machinery/Inspect Fire AR Details")]
    public static void Inspect()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== FIRE SCENE DETAILED AR INSPECTION ===");

        EditorSceneManager.OpenScene("Assets/AR_Fire_foundation/scenes/FireTraining.unity", OpenSceneMode.Single);

        var firePlacement = Object.FindFirstObjectByType<FireScenarioUIController>();
        if (firePlacement != null)
        {
            sb.AppendLine($"FireScenarioUIController: FOUND on {firePlacement.gameObject.name}");
        }

        var xrOrigin = Object.FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null)
        {
            sb.AppendLine($"XR Origin: {xrOrigin.name}");
            foreach (var c in xrOrigin.GetComponents<Component>())
            {
                sb.AppendLine($"  - XR Origin Component: {c.GetType().FullName}");
            }
        }

        var cam = Camera.main;
        if (cam != null)
        {
            sb.AppendLine($"Main Camera: {cam.name}");
            sb.AppendLine($"  - Clear Flags: {cam.clearFlags}");
            sb.AppendLine($"  - Background Color: {cam.backgroundColor}");
            sb.AppendLine($"  - Culling Mask: {cam.cullingMask}");
            sb.AppendLine($"  - Depth: {cam.depth}");
            sb.AppendLine($"  - Near: {cam.nearClipPlane}, Far: {cam.farClipPlane}");
            foreach (var c in cam.GetComponents<Component>())
            {
                sb.AppendLine($"  - Camera Component: {c.GetType().FullName}");
            }
        }

        File.WriteAllText("fire_ar_details_report.txt", sb.ToString());
        Debug.Log(sb.ToString());
    }
}
