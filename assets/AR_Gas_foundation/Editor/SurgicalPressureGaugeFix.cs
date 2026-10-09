using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SurgicalPressureGaugeFix
{
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
    private static readonly string ArtifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";

    [MenuItem("Tools/Surgical Pressure Gauge Fix Precision")]
    public static void Execute()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Debug.Log("Opened scene for Surgical Pressure Gauge Precision Fix: " + scene.path);

        GameObject scenarioRoot = GameObject.Find("GasScenarioRoot");
        if (scenarioRoot == null)
        {
            Debug.LogError("GasScenarioRoot not found!");
            return;
        }

        Transform pipelineAssemblyTr = scenarioRoot.transform.Find("GasPipelineAssembly");
        if (pipelineAssemblyTr == null)
        {
            Debug.LogError("GasPipelineAssembly not found!");
            return;
        }

        Transform gaugeTr = pipelineAssemblyTr.Find("Real_Pressure_Gauge");
        if (gaugeTr == null) gaugeTr = pipelineAssemblyTr.Find("Pressure_Gauge_3D");
        if (gaugeTr == null)
        {
            Debug.LogError("Pressure gauge object not found!");
            return;
        }

        Vector3 oldPos = gaugeTr.position;

        // SURGICAL FIX: Position gauge at Y = 1.30 so its bottom mounting stem (Y = 1.25..1.28)
        // touches the top port of the regulator (Y = 1.28) with ZERO gap and ZERO overlap.
        Vector3 newPos = new Vector3(-0.21f, 1.30f, 0.20f);
        gaugeTr.position = newPos;

        Debug.Log($"SURGICAL PRESSURE GAUGE ADJUSTMENT SUCCESSFUL:");
        Debug.Log($"  Target Object: {GetHierarchyPath(gaugeTr.gameObject)}");
        Debug.Log($"  Old Position: {oldPos}");
        Debug.Log($"  New Position: {newPos}");
        Debug.Log($"  Rotation Changed: FALSE (Kept {gaugeTr.eulerAngles})");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Saved scene cleanly.");

        RenderProofScreenshots();
    }

    private static string GetHierarchyPath(GameObject go)
    {
        string path = go.name;
        Transform p = go.transform.parent;
        while (p != null)
        {
            path = p.name + "/" + path;
            p = p.parent;
        }
        return path;
    }

    private static void RenderProofScreenshots()
    {
        GameObject xrOrigin = GameObject.Find("XR Origin");
        bool wasXrActive = true;
        if (xrOrigin != null)
        {
            wasXrActive = xrOrigin.activeSelf;
            xrOrigin.SetActive(false);
        }

        EnsureDirectionalLight();

        // 1. Cylinder Top Area View
        RenderView(Path.Combine(ArtifactDir, "GAUGE_FIX_CYLINDER_TOP.png"), new Vector3(-0.21f, 1.28f, -1.2f), Quaternion.Euler(0f, 0f, 0f), 32f);

        // 2. Close-up: Cylinder + Regulator + Gauge + Start of Pipe
        RenderView(Path.Combine(ArtifactDir, "GAUGE_FIX_CLOSEUP.png"), new Vector3(-0.15f, 1.22f, -1.4f), Quaternion.Euler(0f, 0f, 0f), 35f);

        if (xrOrigin != null)
        {
            xrOrigin.SetActive(wasXrActive);
        }
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

    private static void RenderView(string outPath, Vector3 pos, Quaternion rot, float fov)
    {
        GameObject camGo = new GameObject("RenderCameraSurgicalGaugeFix");
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
        string dir = Path.GetDirectoryName(outPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllBytes(outPath, bytes);
        Debug.Log("Saved screenshot to " + outPath);
    }
}
