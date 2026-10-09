using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class FixMeshCenterOffsets
{
    private static readonly string ArtifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\333cbd97-2829-4724-af91-85a64f05469f";
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";

    [MenuItem("Tools/Fix Mesh Center Offsets")]
    public static void Execute()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Debug.Log("Opened scene for Mesh Center Offset Alignment: " + scene.path);

        // Target Y line for pipeline assembly is 1.05m
        float pipeLineY = 1.05f;
        float pipeLineZ = 0.20f;

        // 1. Gas Cylinder
        GameObject cylinderGo = GameObject.Find("Realistic_Gas_Cylinder");
        if (cylinderGo != null)
        {
            cylinderGo.transform.position = new Vector3(-0.25f, 0.60f, pipeLineZ);
            cylinderGo.transform.rotation = Quaternion.Euler(270f, 0f, 0f);
            cylinderGo.transform.localScale = new Vector3(0.017f, 0.017f, 0.017f);
        }

        // 2. Real Regulator (.glb)
        GameObject regGo = GameObject.Find("Real_Gas_Regulator");
        if (regGo == null) regGo = GameObject.Find("high-pressure_regulator");
        if (regGo == null) regGo = GameObject.Find("Gas_Regulator_3D");
        if (regGo != null)
        {
            regGo.transform.position = new Vector3(-0.25f, 1.05f, pipeLineZ);
            regGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            regGo.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
            SetMeshCenterY(regGo, pipeLineY);
        }

        // 3. Real Pressure Gauge (.glb)
        GameObject gaugeGo = GameObject.Find("Real_Pressure_Gauge");
        if (gaugeGo == null) gaugeGo = GameObject.Find("pressure_gauge");
        if (gaugeGo == null) gaugeGo = GameObject.Find("Pressure_Gauge_3D");
        if (gaugeGo != null)
        {
            gaugeGo.transform.position = new Vector3(-0.25f, 1.14f, pipeLineZ);
            gaugeGo.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            gaugeGo.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
            SetMeshCenterY(gaugeGo, 1.14f);
        }

        // 4. Horizontal Pipe 1
        GameObject pipe1 = GameObject.Find("Horizontal_Pipe_1");
        if (pipe1 != null)
        {
            pipe1.transform.position = new Vector3(-0.21f, 1.05f, pipeLineZ);
            pipe1.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            pipe1.transform.localScale = new Vector3(0.007f, 0.007f, 0.007f);
            SetMeshCenterY(pipe1, pipeLineY);
            SetMeshCenterX(pipe1, -0.19f);
        }

        // 5. Red Isolation Valve (Scaled & Centered Inline)
        GameObject valveGo = GameObject.Find("Isolation_Valve_3D");
        if (valveGo != null)
        {
            valveGo.transform.position = new Vector3(-0.12f, 1.05f, pipeLineZ);
            valveGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            valveGo.transform.localScale = new Vector3(0.0006f, 0.0006f, 0.0006f);
            SetMeshCenterY(valveGo, pipeLineY);
            SetMeshCenterX(valveGo, -0.13f);
        }

        // 6. Horizontal Pipe 2
        GameObject pipe2 = GameObject.Find("Horizontal_Pipe_2");
        if (pipe2 != null)
        {
            pipe2.transform.position = new Vector3(-0.04f, 1.05f, pipeLineZ);
            pipe2.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            pipe2.transform.localScale = new Vector3(0.007f, 0.007f, 0.007f);
            SetMeshCenterY(pipe2, pipeLineY);
            SetMeshCenterX(pipe2, -0.05f);
        }

        // 7. Pipe Elbow 3D
        GameObject elbowGo = GameObject.Find("Pipe_Elbow_3D");
        if (elbowGo != null)
        {
            elbowGo.transform.position = new Vector3(0.04f, 1.05f, pipeLineZ);
            elbowGo.transform.rotation = Quaternion.Euler(0f, 0f, 270f);
            elbowGo.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);
            SetMeshCenterY(elbowGo, pipeLineY);
            SetMeshCenterX(elbowGo, 0.03f);
        }

        // 8. Downstream Pipe 3D
        GameObject downstreamPipe = GameObject.Find("Downstream_Pipe_3D");
        if (downstreamPipe != null)
        {
            downstreamPipe.transform.position = new Vector3(0.03f, 0.85f, pipeLineZ);
            downstreamPipe.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            downstreamPipe.transform.localScale = new Vector3(0.009f, 0.018f, 0.009f);
            SetMeshCenterX(downstreamPipe, 0.03f);
        }

        // 9. Confined Space Training Area (Expanded)
        GameObject vesselGo = GameObject.Find("Confined_Space_Vessel_Tank");
        if (vesselGo == null) vesselGo = GameObject.Find("Confined_Space_Training_Area");
        if (vesselGo != null)
        {
            vesselGo.transform.position = new Vector3(0.85f, 0.45f, pipeLineZ);
            vesselGo.transform.rotation = Quaternion.Euler(270f, 0f, 0f);
            vesselGo.transform.localScale = new Vector3(1.20f, 1.20f, 1.20f);
        }

        // 10. Self Retracting Lifeline (Mounted on Overhead Frame Bar)
        GameObject lifelineGo = GameObject.Find("Self_Retracting_Lifeline");
        if (lifelineGo == null) lifelineGo = GameObject.Find("Lifeline");
        if (lifelineGo != null)
        {
            lifelineGo.transform.position = new Vector3(1.15f, 0.70f, pipeLineZ);
            lifelineGo.transform.rotation = Quaternion.identity;
            lifelineGo.transform.localScale = new Vector3(0.086f, 0.086f, 0.086f);
        }

        // 11. Ventilation Fan
        GameObject fanGo = GameObject.Find("Axial_Ventilation_Fan_Real");
        if (fanGo == null) fanGo = GameObject.Find("Axial_Ventilation_Fan");
        if (fanGo != null)
        {
            fanGo.transform.position = new Vector3(0.35f, 0.15f, pipeLineZ);
            fanGo.transform.rotation = Quaternion.identity;
            fanGo.transform.localScale = Vector3.one;
        }

        // 12. Gas Detector
        GameObject detectorGo = GameObject.Find("Multi_Gas_Detector");
        if (detectorGo == null) detectorGo = GameObject.Find("Gas_Detector");
        if (detectorGo != null)
        {
            detectorGo.transform.position = new Vector3(-0.05f, 0.02f, pipeLineZ);
            detectorGo.transform.rotation = Quaternion.identity;
            detectorGo.transform.localScale = Vector3.one;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Mesh Center Offset Alignment complete and saved.");

        // Render final proof screenshots
        RenderView("PERFECT_GAS_FRONT.png", new Vector3(-0.15f, 0.65f, -2.8f), Quaternion.Euler(2f, 0f, 0f), 52f);
        RenderView("PERFECT_ASSEMBLY_CLOSEUP.png", new Vector3(-0.12f, 1.05f, -0.9f), Quaternion.Euler(2f, 0f, 0f), 45f);
        RenderView("PERFECT_LIFELINE_CLOSEUP.png", new Vector3(1.00f, 0.75f, -1.2f), Quaternion.Euler(5f, -10f, 0f), 45f);
        RenderView("PERFECT_GAS_FULL.png", new Vector3(-0.15f, 0.65f, -3.2f), Quaternion.Euler(2f, 0f, 0f), 55f);

        Debug.Log("Rendered all 4 perfect verification proof images.");
    }

    private static void SetMeshCenterY(GameObject go, float targetY)
    {
        var r = go.GetComponentInChildren<Renderer>();
        if (r != null)
        {
            float deltaY = targetY - r.bounds.center.y;
            go.transform.position += new Vector3(0f, deltaY, 0f);
        }
    }

    private static void SetMeshCenterX(GameObject go, float targetX)
    {
        var r = go.GetComponentInChildren<Renderer>();
        if (r != null)
        {
            float deltaX = targetX - r.bounds.center.x;
            go.transform.position += new Vector3(deltaX, 0f, 0f);
        }
    }

    private static void RenderView(string filename, Vector3 pos, Quaternion rot, float fov)
    {
        Camera cam = Camera.main;
        GameObject camGo = null;
        if (cam == null)
        {
            camGo = new GameObject("RenderCamera");
            cam = camGo.AddComponent<Camera>();
        }

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

        if (camGo != null)
        {
            UnityEngine.Object.DestroyImmediate(camGo);
        }

        byte[] bytes = tex.EncodeToPNG();
        string outPath = Path.Combine(ArtifactDir, filename);
        File.WriteAllBytes(outPath, bytes);
        UnityEngine.Object.DestroyImmediate(tex);
    }
}
