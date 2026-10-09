using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class TestPerfectGasPipeline
{
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
    private static readonly string ArtifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";

    [MenuItem("Tools/Test Perfect Gas Pipeline")]
    public static void Execute()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject scenarioRoot = GameObject.Find("GasScenarioRoot");
        Transform pipelineAssemblyTr = scenarioRoot.transform.Find("GasPipelineAssembly");
        if (pipelineAssemblyTr != null)
        {
            for (int i = pipelineAssemblyTr.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(pipelineAssemblyTr.GetChild(i).gameObject);
            }
        }
        else
        {
            GameObject paGo = new GameObject("GasPipelineAssembly");
            paGo.transform.SetParent(scenarioRoot.transform, false);
            pipelineAssemblyTr = paGo.transform;
        }

        // Materials
        Material matPipelineSteel = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Pipeline_Steel.mat");
        Material matValveRed = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Valve_Red.mat");
        Material matPressureGauge = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_PressureGauge.mat");

        // 1. CYLINDER TOP & REGULATOR
        // Existing cylinder position is (-0.25, 0.60, 0.20), top outlet is at (-0.25, 1.20, 0.20)
        Vector3 cylinderTopOutlet = new Vector3(-0.25f, 1.20f, 0.20f);

        GameObject regPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasRegulator/gas_regulator_hd.obj");
        GameObject regInst = UnityEngine.Object.Instantiate(regPrefab, pipelineAssemblyTr);
        regInst.name = "Real_Gas_Regulator";
        regInst.transform.position = cylinderTopOutlet + new Vector3(0.04f, 0.02f, 0f);
        regInst.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        regInst.transform.localScale = Vector3.one;

        if (matPipelineSteel != null)
        {
            foreach (var mr in regInst.GetComponentsInChildren<MeshRenderer>())
                mr.material = matPipelineSteel;
        }

        // 2. PRESSURE GAUGE
        // Attached directly to regulator top port
        Vector3 gaugePos = new Vector3(-0.21f, 1.28f, 0.20f);
        GameObject gaugePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge_hd.obj");
        GameObject gaugeInst = UnityEngine.Object.Instantiate(gaugePrefab, pipelineAssemblyTr);
        gaugeInst.name = "Real_Pressure_Gauge";
        gaugeInst.transform.position = gaugePos;
        gaugeInst.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        gaugeInst.transform.localScale = Vector3.one;

        if (matPressureGauge != null)
        {
            foreach (var mr in gaugeInst.GetComponentsInChildren<MeshRenderer>())
                mr.material = matPressureGauge;
        }

        // 3. RED INLINE VALVE
        // Center position X = 0.12, Y = 1.22, Z = 0.20
        Vector3 valvePos = new Vector3(0.12f, 1.22f, 0.20f);
        GameObject valvePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasPipeline/isolation_valve_hd.obj");
        GameObject valveInst = UnityEngine.Object.Instantiate(valvePrefab, pipelineAssemblyTr);
        valveInst.name = "Real_Red_Inline_Valve";
        valveInst.transform.position = valvePos;
        valveInst.transform.rotation = Quaternion.Euler(0f, 90f, 0f); // Handwheel UP, flanges facing -X and +X
        valveInst.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);

        if (matValveRed != null)
        {
            foreach (var mr in valveInst.GetComponentsInChildren<MeshRenderer>())
                mr.material = matValveRed;
        }

        // Valve flanges under scale 0.85:
        // Left flange: X = 0.052
        // Right flange: X = 0.188

        // 4. 90-DEGREE ELBOW
        // Target top center of equipment vertical dome vessel is X = 0.68, Y = 1.22, Z = 0.20
        Vector3 elbowPos = new Vector3(0.68f, 1.22f, 0.20f);
        GameObject elbowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasPipeline/pipe_elbow_hd.obj");
        GameObject elbowInst = UnityEngine.Object.Instantiate(elbowPrefab, pipelineAssemblyTr);
        elbowInst.name = "Real_Pipe_Elbow_90";
        elbowInst.transform.position = elbowPos;
        elbowInst.transform.rotation = Quaternion.Euler(90f, 90f, 0f); // Curved 90 bend from -X to -Y
        elbowInst.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);

        if (matPipelineSteel != null)
        {
            foreach (var mr in elbowInst.GetComponentsInChildren<MeshRenderer>())
                mr.material = matPipelineSteel;
        }

        // 5. PIPES (Round cylindrical pipes, radius 0.02 = 4cm diameter, MAT_Pipeline_Steel)
        float pipeRadius = 0.02f;

        // Pipe 1: Regulator Outlet -> Red Valve Left Flange (COMPLETELY CONTINUOUS, ZERO GAP)
        Vector3 regOutlet = new Vector3(-0.10f, 1.22f, 0.20f);
        Vector3 valveLeftFlange = new Vector3(0.065f, 1.22f, 0.20f);
        BuildCylindricalPipe("Pipe_Regulator_To_Valve", pipelineAssemblyTr, regOutlet, valveLeftFlange, pipeRadius, matPipelineSteel);

        // Pipe 2: Red Valve Right Flange -> Elbow Left Inlet (COMPLETELY CONTINUOUS, ZERO GAP)
        Vector3 valveRightFlange = new Vector3(0.175f, 1.22f, 0.20f);
        Vector3 elbowLeftInlet = new Vector3(0.64f, 1.22f, 0.20f);
        BuildCylindricalPipe("Pipe_Valve_To_Elbow", pipelineAssemblyTr, valveRightFlange, elbowLeftInlet, pipeRadius, matPipelineSteel);

        // Pipe 3: Vertical Downstream: Elbow Bottom Outlet -> Top Center of Equipment Vertical Dome Vessel
        Vector3 elbowBottomOutlet = new Vector3(0.68f, 1.16f, 0.20f);
        Vector3 equipmentDomeVesselTopPort = new Vector3(0.68f, 0.58f, 0.20f);
        BuildCylindricalPipe("Pipe_Vertical_Downstream", pipelineAssemblyTr, elbowBottomOutlet, equipmentDomeVesselTopPort, pipeRadius, matPipelineSteel);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        RenderScreenshots();
    }

    private static void BuildCylindricalPipe(string name, Transform parent, Vector3 start, Vector3 end, float radius, Material mat)
    {
        GameObject pipeGo = new GameObject(name);
        pipeGo.transform.SetParent(parent, false);

        Vector3 dir = end - start;
        float length = dir.magnitude;
        if (length < 0.001f) return;

        pipeGo.transform.position = (start + end) / 2f;
        pipeGo.transform.rotation = Quaternion.FromToRotation(Vector3.up, dir.normalized);

        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = "PipeMesh";
        cylinder.transform.SetParent(pipeGo.transform, false);
        cylinder.transform.localPosition = Vector3.zero;
        cylinder.transform.localRotation = Quaternion.identity;
        cylinder.transform.localScale = new Vector3(radius * 2f, length / 2f, radius * 2f);

        if (mat != null)
        {
            cylinder.GetComponent<MeshRenderer>().material = mat;
        }
    }

    private static void RenderScreenshots()
    {
        GameObject xrOrigin = GameObject.Find("XR Origin");
        bool wasXrActive = true;
        if (xrOrigin != null)
        {
            wasXrActive = xrOrigin.activeSelf;
            xrOrigin.SetActive(false);
        }

        EnsureDirectionalLight();

        RenderView(Path.Combine(ArtifactDir, "PERFECT_FULL_WORKSTATION.png"), new Vector3(0.0f, 1.10f, -4.5f), Quaternion.Euler(0f, 0f, 0f), 52f);
        RenderView(Path.Combine(ArtifactDir, "PERFECT_CYLINDER_VALVE_CLOSEUP.png"), new Vector3(-0.05f, 1.15f, -1.6f), Quaternion.Euler(0f, 0f, 0f), 38f);
        RenderView(Path.Combine(ArtifactDir, "PERFECT_ELBOW_EQUIPMENT_CLOSEUP.png"), new Vector3(0.40f, 0.95f, -1.6f), Quaternion.Euler(0f, 0f, 0f), 40f);

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
        GameObject camGo = new GameObject("RenderCameraPerfectTest");
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
