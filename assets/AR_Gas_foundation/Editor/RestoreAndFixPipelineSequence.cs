using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class RestoreAndFixPipelineSequence
{
    private static readonly string MainScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
    private static readonly string BackupToRestorePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation_Backup_20260928_1235_PressureGaugeFix.unity";
    private static readonly string SafetyBackupPath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation_Restored_State.unity";
    private static readonly string ArtifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";

    [MenuItem("Tools/Restore And Fix Pipeline Sequence")]
    public static void Execute()
    {
        Debug.Log("--- PHASE 1: RESTORING FROM BACKUP ---");
        Debug.Log("Restoring backup scene from: " + BackupToRestorePath);

        // 1. Open the backup scene first
        var backupScene = EditorSceneManager.OpenScene(BackupToRestorePath, OpenSceneMode.Single);

        // Save a copy as safety backup of restored state
        EditorSceneManager.SaveScene(backupScene, SafetyBackupPath);
        Debug.Log("Created safety backup of restored state: " + SafetyBackupPath);

        // Save into main scene file
        EditorSceneManager.SaveScene(backupScene, MainScenePath);
        Debug.Log("Restored main scene file: " + MainScenePath);

        // 2. Open Main Scene
        var scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);

        // --- PHASE 2: SURGICAL ALIGNMENT CORRECTION ---
        Debug.Log("--- PHASE 2: FINAL SURGICAL PIPELINE ALIGNMENT ---");

        GameObject scenarioRoot = GameObject.Find("GasScenarioRoot");
        if (scenarioRoot == null)
        {
            Debug.LogError("GasScenarioRoot not found!");
            return;
        }

        Transform pipelineAssemblyTr = scenarioRoot.transform.Find("GasPipelineAssembly");
        if (pipelineAssemblyTr == null)
        {
            GameObject paGo = new GameObject("GasPipelineAssembly");
            paGo.transform.SetParent(scenarioRoot.transform, false);
            pipelineAssemblyTr = paGo.transform;
        }

        // Clean existing children in GasPipelineAssembly
        for (int i = pipelineAssemblyTr.childCount - 1; i >= 0; i--)
        {
            UnityEngine.Object.DestroyImmediate(pipelineAssemblyTr.GetChild(i).gameObject);
        }

        // Load Materials
        Material matPipelineSteel = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Pipeline_Steel.mat");
        Material matValveRed = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Valve_Red.mat");
        Material matPressureGauge = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_PressureGauge.mat");

        float pipeRadius = 0.018f; // ~3.6cm realistic diameter

        // A) REGULATOR
        // Mounted directly on existing cylinder top outlet (-0.25, 1.20, 0.20)
        Vector3 cylinderTopOutlet = new Vector3(-0.25f, 1.20f, 0.20f);
        GameObject regPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasRegulator/gas_regulator_hd.obj");
        GameObject regInst = UnityEngine.Object.Instantiate(regPrefab, pipelineAssemblyTr);
        regInst.name = "Real_Gas_Regulator";
        regInst.transform.position = cylinderTopOutlet + new Vector3(0.04f, 0.01f, 0f);
        regInst.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        regInst.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);

        if (matPipelineSteel != null)
        {
            foreach (var mr in regInst.GetComponentsInChildren<MeshRenderer>())
                mr.material = matPipelineSteel;
        }

        // B) PRESSURE GAUGE
        // Mounted cleanly ABOVE regulator top gauge port (-0.21, 1.26, 0.20)
        Vector3 gaugePos = new Vector3(-0.21f, 1.30f, 0.20f);
        GameObject gaugePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge_hd.obj");
        GameObject gaugeInst = UnityEngine.Object.Instantiate(gaugePrefab, pipelineAssemblyTr);
        gaugeInst.name = "Real_Pressure_Gauge";
        gaugeInst.transform.position = gaugePos;
        gaugeInst.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // Dial facing front camera
        gaugeInst.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);

        if (matPressureGauge != null)
        {
            foreach (var mr in gaugeInst.GetComponentsInChildren<MeshRenderer>())
                mr.material = matPressureGauge;
        }

        // C) RED INLINE VALVE
        // Positioned shortly after regulator at X = 0.10
        Vector3 valvePos = new Vector3(0.10f, 1.21f, 0.20f);
        GameObject valvePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasPipeline/isolation_valve_hd.obj");
        GameObject valveInst = UnityEngine.Object.Instantiate(valvePrefab, pipelineAssemblyTr);
        valveInst.name = "Real_Red_Inline_Valve";
        valveInst.transform.position = valvePos;
        valveInst.transform.rotation = Quaternion.Euler(0f, 90f, 0f); // Red wheel UP, inline with X axis
        valveInst.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);

        if (matValveRed != null)
        {
            foreach (var mr in valveInst.GetComponentsInChildren<MeshRenderer>())
                mr.material = matValveRed;
        }

        // D) 90-DEGREE ELBOW
        // Positioned at X = 0.58 to align with equipment vertical dome vessel top port
        Vector3 elbowPos = new Vector3(0.58f, 1.21f, 0.20f);
        GameObject elbowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasPipeline/pipe_elbow_hd.obj");
        GameObject elbowInst = UnityEngine.Object.Instantiate(elbowPrefab, pipelineAssemblyTr);
        elbowInst.name = "Real_Pipe_Elbow_90";
        elbowInst.transform.position = elbowPos;
        elbowInst.transform.rotation = Quaternion.Euler(0f, 270f, 90f);
        elbowInst.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);

        if (matPipelineSteel != null)
        {
            foreach (var mr in elbowInst.GetComponentsInChildren<MeshRenderer>())
                mr.material = matPipelineSteel;
        }

        // Smooth elbow corner joint sphere
        GameObject elbowCornerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        elbowCornerSphere.name = "ElbowCornerJoint";
        elbowCornerSphere.transform.SetParent(pipelineAssemblyTr, false);
        elbowCornerSphere.transform.position = elbowPos;
        elbowCornerSphere.transform.localScale = new Vector3(pipeRadius * 2.2f, pipeRadius * 2.2f, pipeRadius * 2.2f);
        if (matPipelineSteel != null) elbowCornerSphere.GetComponent<MeshRenderer>().material = matPipelineSteel;

        // E) SHORT PIPE 1: Regulator Outlet -> Red Valve (Enters inside valve body, ZERO GAP)
        Vector3 regOutlet = new Vector3(-0.12f, 1.21f, 0.20f);
        Vector3 valveBodyLeft = new Vector3(0.10f, 1.21f, 0.20f);
        BuildCylindricalPipe("Pipe_Regulator_To_Valve", pipelineAssemblyTr, regOutlet, valveBodyLeft, pipeRadius, matPipelineSteel);

        // F) PIPE 2: Red Valve -> 90° Elbow (Starts inside valve body, ends at elbow, ZERO GAP)
        Vector3 valveBodyRight = new Vector3(0.10f, 1.21f, 0.20f);
        BuildCylindricalPipe("Pipe_Valve_To_Elbow", pipelineAssemblyTr, valveBodyRight, elbowPos, pipeRadius, matPipelineSteel);

        // G) VERTICAL DOWNSTREAM PIPE: Elbow -> Top Connection Port of Industrial Vessel
        Vector3 equipmentVesselTopPort = new Vector3(0.58f, 0.48f, 0.20f);
        BuildCylindricalPipe("Pipe_Vertical_Downstream", pipelineAssemblyTr, elbowPos, equipmentVesselTopPort, pipeRadius, matPipelineSteel);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Saved modified scene: " + scene.path);

        RenderMandatoryScreenshots();
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

    private static void RenderMandatoryScreenshots()
    {
        GameObject xrOrigin = GameObject.Find("XR Origin");
        bool wasXrActive = true;
        if (xrOrigin != null)
        {
            wasXrActive = xrOrigin.activeSelf;
            xrOrigin.SetActive(false);
        }

        EnsureDirectionalLight();

        // 1. FULL GAS WORKSTATION
        RenderView(Path.Combine(ArtifactDir, "RESTORED_FULL_WORKSTATION.png"), new Vector3(0.0f, 1.10f, -4.5f), Quaternion.Euler(0f, 0f, 0f), 52f);

        // 2. CLOSE-UP: CYLINDER + REGULATOR + PRESSURE GAUGE
        RenderView(Path.Combine(ArtifactDir, "RESTORED_CYLINDER_REGULATOR_GAUGE.png"), new Vector3(-0.21f, 1.25f, -1.2f), Quaternion.Euler(0f, 0f, 0f), 30f);

        // 3. CLOSE-UP: REGULATOR -> PIPE -> VALVE
        RenderView(Path.Combine(ArtifactDir, "RESTORED_REGULATOR_PIPE_VALVE.png"), new Vector3(-0.05f, 1.21f, -1.4f), Quaternion.Euler(0f, 0f, 0f), 35f);

        // 4. CLOSE-UP: VALVE -> PIPE -> 90° ELBOW -> INDUSTRIAL EQUIPMENT
        RenderView(Path.Combine(ArtifactDir, "RESTORED_VALVE_ELBOW_EQUIPMENT.png"), new Vector3(0.35f, 0.95f, -1.6f), Quaternion.Euler(0f, 0f, 0f), 40f);

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
        GameObject camGo = new GameObject("RenderCameraRestoreFix");
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
