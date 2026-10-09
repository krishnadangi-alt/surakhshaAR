using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class PrecisionGasWorkstationAlignment
{
    private static readonly string ArtifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\333cbd97-2829-4724-af91-85a64f05469f";
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";

    [MenuItem("Tools/Precision Gas Workstation Alignment")]
    public static void AlignAndVerify()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Debug.Log("Opened scene for Precision Alignment: " + scene.path);

        // 1. PPE WORKBENCH & REAL PPE ASSETS (LEFT)
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

        // 2. REAL GAS CYLINDER (CENTER)
        GameObject cylinderGo = GameObject.Find("Realistic_Gas_Cylinder");
        if (cylinderGo != null)
        {
            cylinderGo.transform.position = new Vector3(-0.25f, 0.60f, 0.20f);
            cylinderGo.transform.rotation = Quaternion.Euler(270f, 0f, 0f); // Vertically upright
            cylinderGo.transform.localScale = new Vector3(0.017f, 0.017f, 0.017f);
        }

        // 3. ORIGINAL REAL PRESSURE REGULATOR (.glb)
        GameObject regGo = GameObject.Find("Real_Gas_Regulator");
        if (regGo == null) regGo = GameObject.Find("high-pressure_regulator");
        if (regGo == null) regGo = GameObject.Find("Gas_Regulator_3D");
        if (regGo != null)
        {
            regGo.transform.position = new Vector3(-0.25f, 1.05f, 0.20f);
            regGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            regGo.transform.localScale = new Vector3(0.016f, 0.016f, 0.016f);
        }

        // 4. ORIGINAL REAL PRESSURE GAUGE (.glb)
        GameObject gaugeGo = GameObject.Find("Real_Pressure_Gauge");
        if (gaugeGo == null) gaugeGo = GameObject.Find("pressure_gauge");
        if (gaugeGo == null) gaugeGo = GameObject.Find("Pressure_Gauge_3D");
        if (gaugeGo != null)
        {
            gaugeGo.transform.position = new Vector3(-0.25f, 1.16f, 0.20f);
            gaugeGo.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            gaugeGo.transform.localScale = new Vector3(0.016f, 0.016f, 0.016f);
        }

        // 5. PIPE 1 (REGULATOR OUTLET TO VALVE)
        GameObject pipe1 = GameObject.Find("Horizontal_Pipe_1");
        if (pipe1 != null)
        {
            pipe1.transform.position = new Vector3(-0.20f, 1.05f, 0.20f);
            pipe1.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            pipe1.transform.localScale = new Vector3(0.007f, 0.007f, 0.007f);
        }

        // 6. RED ISOLATION VALVE
        GameObject valveGo = GameObject.Find("Isolation_Valve_3D");
        if (valveGo != null)
        {
            valveGo.transform.position = new Vector3(-0.14f, 1.05f, 0.20f);
            valveGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            valveGo.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        }

        // 7. PIPE 2 & 90-DEGREE ELBOW
        GameObject pipe2 = GameObject.Find("Horizontal_Pipe_2");
        if (pipe2 != null)
        {
            pipe2.transform.position = new Vector3(-0.06f, 1.05f, 0.20f);
            pipe2.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            pipe2.transform.localScale = new Vector3(0.007f, 0.007f, 0.007f);
        }

        GameObject elbowGo = GameObject.Find("Pipe_Elbow_3D");
        if (elbowGo != null)
        {
            elbowGo.transform.position = new Vector3(0.03f, 1.05f, 0.20f);
            elbowGo.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            elbowGo.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);
        }

        // 8. DOWNSTREAM PIPE TO CONFINED SPACE
        GameObject downstreamPipe = GameObject.Find("Downstream_Pipe_3D");
        if (downstreamPipe != null)
        {
            downstreamPipe.transform.position = new Vector3(0.03f, 0.85f, 0.20f);
            downstreamPipe.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            downstreamPipe.transform.localScale = new Vector3(0.009f, 0.009f, 0.009f);
        }

        // 9. CONFINED SPACE TRAINING AREA (PROPORTIONALLY LARGER)
        GameObject vesselGo = GameObject.Find("Confined_Space_Vessel_Tank");
        if (vesselGo == null) vesselGo = GameObject.Find("Confined_Space_Training_Area");
        if (vesselGo != null)
        {
            vesselGo.transform.position = new Vector3(0.85f, 0.45f, 0.20f);
            vesselGo.transform.rotation = Quaternion.Euler(270f, 0f, 0f);
            vesselGo.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f); // Proportionally larger as requested
        }

        // 10. REAL SELF-RETRACTING LIFELINE (MOUNTED ON CONFINED SPACE OVERHEAD FRAME)
        GameObject lifelineGo = GameObject.Find("Self_Retracting_Lifeline");
        if (lifelineGo == null) lifelineGo = GameObject.Find("Lifeline");
        if (lifelineGo != null)
        {
            // Mount directly on top structural bar of confined space setup
            lifelineGo.transform.position = new Vector3(0.85f, 1.35f, 0.20f);
            lifelineGo.transform.rotation = Quaternion.identity;
            lifelineGo.transform.localScale = new Vector3(0.086f, 0.086f, 0.086f);
        }

        // 11. REAL VENTILATION EQUIPMENT
        GameObject fanGo = GameObject.Find("Axial_Ventilation_Fan_Real");
        if (fanGo == null) fanGo = GameObject.Find("Axial_Ventilation_Fan");
        if (fanGo != null)
        {
            fanGo.transform.position = new Vector3(0.35f, 0.15f, 0.20f);
            fanGo.transform.rotation = Quaternion.identity;
        }

        // 12. REAL GAS DETECTOR (ON GROUND NEAR CYLINDER)
        GameObject detectorGo = GameObject.Find("Multi_Gas_Detector");
        if (detectorGo == null) detectorGo = GameObject.Find("Gas_Detector");
        if (detectorGo != null)
        {
            detectorGo.transform.position = new Vector3(-0.05f, 0.02f, 0.20f);
            detectorGo.transform.rotation = Quaternion.identity;
            if (detectorGo.transform.localScale == Vector3.zero)
            {
                detectorGo.transform.localScale = Vector3.one;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Precision Gas Workstation Alignment complete and saved.");

        // Render proof images
        RenderView("PRECISION_GAS_FRONT.png", new Vector3(-0.15f, 0.65f, -2.8f), Quaternion.Euler(2f, 0f, 0f), 52f);
        RenderView("PRECISION_GAS_ASSEMBLY_CLOSEUP.png", new Vector3(-0.10f, 1.05f, -0.9f), Quaternion.Euler(2f, 0f, 0f), 45f);
        RenderView("PRECISION_GAS_LIFELINE_CLOSEUP.png", new Vector3(0.70f, 1.0f, -1.2f), Quaternion.Euler(5f, -15f, 0f), 45f);
        RenderView("PRECISION_GAS_FULL.png", new Vector3(-0.15f, 0.65f, -3.2f), Quaternion.Euler(2f, 0f, 0f), 55f);

        Debug.Log("Rendered all 4 precision verification proof images.");
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
