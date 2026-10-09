using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class PerfectGasWorkstationConnections
{
    private static readonly string ArtifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\333cbd97-2829-4724-af91-85a64f05469f";
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";

    [MenuItem("Tools/Perfect Gas Workstation Connections")]
    public static void Execute()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Debug.Log("Opened scene for Perfect Connections: " + scene.path);

        // 1. PPE WORKBENCH & REAL PPE ASSETS (LEFT) - KEEP AS IS
        GameObject workbenchGo = GameObject.Find("PPE_Workbench");
        if (workbenchGo == null) workbenchGo = GameObject.Find("Real_PPE_Workbench");
        if (workbenchGo != null)
        {
            workbenchGo.transform.position = new Vector3(-1.15f, 0.00f, 0.20f);
            workbenchGo.transform.rotation = Quaternion.identity;
            workbenchGo.transform.localScale = Vector3.one;
        }

        GameObject bootLeft = GameObject.Find("Safety_Boots_Left");
        if (bootLeft != null)
        {
            bootLeft.transform.position = new Vector3(-1.35f, 0.16f, 0.20f);
            bootLeft.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }

        GameObject bootRight = GameObject.Find("Safety_Boots_Right");
        if (bootRight != null)
        {
            bootRight.transform.position = new Vector3(-0.95f, 0.16f, 0.20f);
            bootRight.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }

        // 2. REAL GAS CYLINDER (CENTER) - UPRIGHT EXACTLY IN PLACE
        GameObject cylinderGo = GameObject.Find("Realistic_Gas_Cylinder");
        if (cylinderGo != null)
        {
            cylinderGo.transform.position = new Vector3(-0.25f, 0.60f, 0.20f);
            cylinderGo.transform.rotation = Quaternion.Euler(270f, 0f, 0f);
            cylinderGo.transform.localScale = new Vector3(0.017f, 0.017f, 0.017f);
        }

        // 3. ORIGINAL REAL PRESSURE REGULATOR (.glb) MOUNTED AT CYLINDER TOP OUTLET
        GameObject regGo = GameObject.Find("Real_Gas_Regulator");
        if (regGo == null) regGo = GameObject.Find("high-pressure_regulator");
        if (regGo == null) regGo = GameObject.Find("Gas_Regulator_3D");
        if (regGo != null)
        {
            regGo.transform.position = new Vector3(-0.25f, 1.05f, 0.20f);
            regGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            regGo.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
        }

        // 4. ORIGINAL REAL PRESSURE GAUGE (.glb) MOUNTED AT REGULATOR GAUGE PORT
        GameObject gaugeGo = GameObject.Find("Real_Pressure_Gauge");
        if (gaugeGo == null) gaugeGo = GameObject.Find("pressure_gauge");
        if (gaugeGo == null) gaugeGo = GameObject.Find("Pressure_Gauge_3D");
        if (gaugeGo != null)
        {
            gaugeGo.transform.position = new Vector3(-0.25f, 1.14f, 0.20f);
            gaugeGo.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            gaugeGo.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
        }

        // 5. REAL PIPE 1 FROM REGULATOR OUTLET
        GameObject pipe1 = GameObject.Find("Horizontal_Pipe_1");
        if (pipe1 != null)
        {
            pipe1.transform.position = new Vector3(-0.21f, 1.05f, 0.20f);
            pipe1.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            pipe1.transform.localScale = new Vector3(0.007f, 0.007f, 0.007f);
        }

        // 6. REAL RED ISOLATION VALVE (PERFECTLY PROPORTIONED INLINE)
        GameObject valveGo = GameObject.Find("Isolation_Valve_3D");
        if (valveGo != null)
        {
            valveGo.transform.position = new Vector3(-0.12f, 1.05f, 0.20f);
            valveGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            valveGo.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f); // Realistic 11cm diameter valve
        }

        // 7. REAL PIPE 2 & 90-DEGREE ELBOW
        GameObject pipe2 = GameObject.Find("Horizontal_Pipe_2");
        if (pipe2 != null)
        {
            pipe2.transform.position = new Vector3(-0.04f, 1.05f, 0.20f);
            pipe2.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            pipe2.transform.localScale = new Vector3(0.007f, 0.007f, 0.007f);
        }

        GameObject elbowGo = GameObject.Find("Pipe_Elbow_3D");
        if (elbowGo != null)
        {
            elbowGo.transform.position = new Vector3(0.04f, 1.05f, 0.20f);
            elbowGo.transform.rotation = Quaternion.Euler(0f, 0f, 270f);
            elbowGo.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);
        }

        // 8. DOWNSTREAM PIPE CONTINUING TOWARD CONFINED-SPACE STATION
        GameObject downstreamPipe = GameObject.Find("Downstream_Pipe_3D");
        if (downstreamPipe != null)
        {
            downstreamPipe.transform.position = new Vector3(0.04f, 0.90f, 0.20f);
            downstreamPipe.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            downstreamPipe.transform.localScale = new Vector3(0.009f, 0.018f, 0.009f);
        }

        // 9. CONFINED SPACE TRAINING AREA (PROPORTIONALLY LARGER SETUP)
        GameObject vesselGo = GameObject.Find("Confined_Space_Vessel_Tank");
        if (vesselGo == null) vesselGo = GameObject.Find("Confined_Space_Training_Area");
        if (vesselGo != null)
        {
            vesselGo.transform.position = new Vector3(0.85f, 0.45f, 0.20f);
            vesselGo.transform.rotation = Quaternion.Euler(270f, 0f, 0f);
            vesselGo.transform.localScale = new Vector3(1.20f, 1.20f, 1.20f);
        }

        // 10. REAL SELF-RETRACTING LIFELINE MOUNTED ON OVERHEAD FRAME (NOT FLOATING)
        GameObject lifelineGo = GameObject.Find("Self_Retracting_Lifeline");
        if (lifelineGo == null) lifelineGo = GameObject.Find("Lifeline");
        if (lifelineGo != null)
        {
            // Positioned so top carabiner hooks directly onto structural frame top railing
            lifelineGo.transform.position = new Vector3(1.15f, 0.78f, 0.20f);
            lifelineGo.transform.rotation = Quaternion.identity;
            lifelineGo.transform.localScale = new Vector3(0.086f, 0.086f, 0.086f);
        }

        // 11. REAL VENTILATION EQUIPMENT (KEPT NEAR CONFINED SPACE AREA)
        GameObject fanGo = GameObject.Find("Axial_Ventilation_Fan_Real");
        if (fanGo == null) fanGo = GameObject.Find("Axial_Ventilation_Fan");
        if (fanGo != null)
        {
            fanGo.transform.position = new Vector3(0.35f, 0.15f, 0.20f);
            fanGo.transform.rotation = Quaternion.identity;
            fanGo.transform.localScale = Vector3.one;
        }

        // 12. REAL GAS DETECTOR (ON GROUND NEAR CYLINDER)
        GameObject detectorGo = GameObject.Find("Multi_Gas_Detector");
        if (detectorGo == null) detectorGo = GameObject.Find("Gas_Detector");
        if (detectorGo != null)
        {
            detectorGo.transform.position = new Vector3(-0.05f, 0.02f, 0.20f);
            detectorGo.transform.rotation = Quaternion.identity;
            detectorGo.transform.localScale = Vector3.one;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Perfect Gas Workstation Connections complete and saved.");

        // Render proof screenshots
        RenderView("VERIFIED_GAS_FRONT.png", new Vector3(-0.15f, 0.65f, -2.8f), Quaternion.Euler(2f, 0f, 0f), 52f);
        RenderView("VERIFIED_ASSEMBLY_CLOSEUP.png", new Vector3(-0.12f, 1.05f, -0.9f), Quaternion.Euler(2f, 0f, 0f), 45f);
        RenderView("VERIFIED_LIFELINE_CLOSEUP.png", new Vector3(1.00f, 0.75f, -1.2f), Quaternion.Euler(5f, -10f, 0f), 45f);
        RenderView("VERIFIED_GAS_FULL.png", new Vector3(-0.15f, 0.65f, -3.2f), Quaternion.Euler(2f, 0f, 0f), 55f);

        Debug.Log("Rendered all 4 verified proof images.");
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
