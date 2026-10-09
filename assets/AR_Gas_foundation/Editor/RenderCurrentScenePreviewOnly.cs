using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class RenderCurrentScenePreviewOnly
{
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
    private static readonly string BrainDir = @"C:\Users\anjan\.gemini\antigravity\brain\333cbd97-2829-4724-af91-85a64f05469f";
    private static readonly string OutDir = @"C:\SurakshaAR";

    [MenuItem("Tools/Render Current Scene Preview Only")]
    public static void Execute()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Debug.Log("Opened scene for Preview Rendering ONLY (No Scene Edits): " + scene.path);

        GameObject xrOrigin = GameObject.Find("XR Origin");
        bool wasXrActive = true;
        if (xrOrigin != null)
        {
            wasXrActive = xrOrigin.activeSelf;
            xrOrigin.SetActive(false);
        }

        EnsureDirectionalLight();

        // 1. Full Workstation View
        RenderView(Path.Combine(OutDir, "Gas_Recovery_FINAL_PREVIEW.png"), Path.Combine(BrainDir, "Gas_Recovery_FINAL_PREVIEW.png"), new Vector3(0.0f, 1.10f, -4.5f), Quaternion.Euler(0f, 0f, 0f), 52f);
        RenderView(Path.Combine(OutDir, "Gas_Workstation_FULL.png"), Path.Combine(BrainDir, "Gas_Workstation_FULL.png"), new Vector3(0.0f, 1.10f, -4.5f), Quaternion.Euler(0f, 0f, 0f), 52f);

        // 2. Close-up 1: Cylinder + Regulator + Gauge + Valve + Pipe
        RenderView(Path.Combine(OutDir, "Gas_Workstation_CLOSEUP_CYLINDER_REGULATOR.png"), Path.Combine(BrainDir, "Gas_Workstation_CLOSEUP_CYLINDER_REGULATOR.png"), new Vector3(-0.15f, 1.15f, -1.6f), Quaternion.Euler(0f, 0f, 0f), 38f);
        RenderView(Path.Combine(OutDir, "Gas_Workstation_ASSEMBLY_CLOSEUP.png"), Path.Combine(BrainDir, "Gas_Workstation_ASSEMBLY_CLOSEUP.png"), new Vector3(-0.15f, 1.15f, -1.6f), Quaternion.Euler(0f, 0f, 0f), 38f);

        // 3. Close-up 2: Elbow + Downstream Pipe + Confined-Space Equipment
        RenderView(Path.Combine(OutDir, "Gas_Workstation_CLOSEUP_ELBOW_EQUIPMENT.png"), Path.Combine(BrainDir, "Gas_Workstation_CLOSEUP_ELBOW_EQUIPMENT.png"), new Vector3(0.75f, 0.95f, -1.8f), Quaternion.Euler(0f, 0f, 0f), 42f);

        // 4. Close-up 3: Support Frame + Yellow SRL + Confined-Space
        RenderView(Path.Combine(OutDir, "Gas_Workstation_CLOSEUP_FRAME_LIFELINE.png"), Path.Combine(BrainDir, "Gas_Workstation_CLOSEUP_FRAME_LIFELINE.png"), new Vector3(1.95f, 1.25f, -2.0f), Quaternion.Euler(0f, 0f, 0f), 45f);
        RenderView(Path.Combine(OutDir, "Gas_Workstation_LIFELINE_CLOSEUP.png"), Path.Combine(BrainDir, "Gas_Workstation_LIFELINE_CLOSEUP.png"), new Vector3(1.95f, 1.25f, -2.0f), Quaternion.Euler(0f, 0f, 0f), 45f);

        if (xrOrigin != null)
        {
            xrOrigin.SetActive(wasXrActive);
        }

        Debug.Log("Preview rendering complete. Scene was NOT modified.");
    }

    private static void EnsureDirectionalLight()
    {
        Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        if (lights == null || lights.Length == 0)
        {
            GameObject lightGo = new GameObject("Directional Light");
            Light lightComponent = lightGo.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }
    }

    private static void RenderView(string outPath1, string outPath2, Vector3 pos, Quaternion rot, float fov)
    {
        GameObject camGo = new GameObject("RenderCameraPreviewOnly");
        Camera cam = camGo.AddComponent<Camera>();

        cam.transform.position = pos;
        cam.transform.rotation = rot;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.15f, 0.20f);
        cam.fieldOfView = fov;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;

        RenderTexture rt = new RenderTexture(1920, 1080, 24);
        cam.targetTexture = rt;
        Texture2D tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);

        cam.Render();
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        tex.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        UnityEngine.Object.DestroyImmediate(rt);
        UnityEngine.Object.DestroyImmediate(camGo);

        byte[] bytes = tex.EncodeToPNG();
        
        string dir1 = Path.GetDirectoryName(outPath1);
        if (!Directory.Exists(dir1)) Directory.CreateDirectory(dir1);
        File.WriteAllBytes(outPath1, bytes);

        string dir2 = Path.GetDirectoryName(outPath2);
        if (!Directory.Exists(dir2)) Directory.CreateDirectory(dir2);
        File.WriteAllBytes(outPath2, bytes);

        UnityEngine.Object.DestroyImmediate(tex);
    }
}
