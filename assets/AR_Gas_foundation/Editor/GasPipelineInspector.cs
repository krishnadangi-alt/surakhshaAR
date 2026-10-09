using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GasPipelineInspector
{
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
    private static readonly string ArtifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";

    [MenuItem("Tools/Inspect Gas Scene")]
    public static void InspectScene()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Debug.Log("Opened scene: " + scene.path);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== GAS SCENE HIERARCHY AUDIT ===");

        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (var root in rootObjects)
        {
            DumpGameObject(root, sb, 0);
        }

        string logPath = Path.Combine(ArtifactDir, "gas_scene_hierarchy_audit.txt");
        File.WriteAllText(logPath, sb.ToString());
        Debug.Log("Dumped hierarchy to " + logPath);

        RenderBaselineScreenshots();
    }

    private static void DumpGameObject(GameObject go, StringBuilder sb, int indent)
    {
        string prefix = new string(' ', indent * 2);
        Vector3 p = go.transform.position;
        Vector3 r = go.transform.eulerAngles;
        Vector3 s = go.transform.lossyScale;
        MeshFilter mf = go.GetComponent<MeshFilter>();
        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        string meshName = mf != null && mf.sharedMesh != null ? mf.sharedMesh.name : "none";
        string matName = mr != null && mr.sharedMaterial != null ? mr.sharedMaterial.name : "none";
        Bounds b = mr != null ? mr.bounds : new Bounds();

        sb.AppendLine($"{prefix}- {go.name} (Active: {go.activeSelf}) | Pos: {p} | Rot: {r} | Scale: {s} | Mesh: {meshName} | Mat: {matName} | BoundsCenter: {b.center} | BoundsExtents: {b.extents}");

        for (int i = 0; i < go.transform.childCount; i++)
        {
            DumpGameObject(go.transform.GetChild(i).gameObject, sb, indent + 1);
        }
    }

    private static void RenderBaselineScreenshots()
    {
        GameObject xrOrigin = GameObject.Find("XR Origin");
        bool wasXrActive = true;
        if (xrOrigin != null)
        {
            wasXrActive = xrOrigin.activeSelf;
            xrOrigin.SetActive(false);
        }

        EnsureDirectionalLight();

        // 1. Full Workstation View
        RenderView(Path.Combine(ArtifactDir, "BASELINE_FULL_WORKSTATION.png"), new Vector3(0.0f, 1.10f, -4.5f), Quaternion.Euler(0f, 0f, 0f), 52f);

        // 2. Cylinder + Regulator + Gauge + Valve close-up
        RenderView(Path.Combine(ArtifactDir, "BASELINE_CYLINDER_VALVE_CLOSEUP.png"), new Vector3(-0.15f, 1.15f, -1.6f), Quaternion.Euler(0f, 0f, 0f), 38f);

        // 3. Valve + Elbow + Downstream Pipe close-up
        RenderView(Path.Combine(ArtifactDir, "BASELINE_ELBOW_EQUIPMENT_CLOSEUP.png"), new Vector3(0.75f, 0.95f, -1.8f), Quaternion.Euler(0f, 0f, 0f), 42f);

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
        GameObject camGo = new GameObject("RenderCameraInspector");
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
