using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class BuildMasterTargetWorkstation
{
    private static readonly string ArtifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\333cbd97-2829-4724-af91-85a64f05469f";

    [MenuItem("Tools/Build Master Target Workstation")]
    public static void Execute()
    {
        string srcScene = "C:/SurakhshaAR/ProjectBackup/AR_gas_Foundation_Backup.unity";
        string dstScene = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";

        File.Copy(srcScene, dstScene, true);
        AssetDatabase.Refresh();

        var scene = EditorSceneManager.OpenScene(dstScene, OpenSceneMode.Single);
        Debug.Log("Opened restored ProjectBackup scene: " + scene.path);

        // 1. Gas Cylinder
        GameObject cylinderGo = GameObject.Find("Realistic_Gas_Cylinder");
        if (cylinderGo != null)
        {
            cylinderGo.transform.position = new Vector3(-0.25f, 0.60f, 0.20f);
            cylinderGo.transform.rotation = Quaternion.Euler(270f, 0f, 0f);
            cylinderGo.transform.localScale = new Vector3(0.017f, 0.017f, 0.017f);
        }

        // Load Original Downloaded GLB Assets
        UnityEngine.Object regulatorGlb = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/AR_Gas_foundation/3d/GasRegulator/high-pressure_regulator.glb");
        UnityEngine.Object gaugeGlb = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge.glb");

        // 2. High Pressure Regulator (Original GLB)
        GameObject oldReg = GameObject.Find("Gas_Regulator_3D");
        if (oldReg == null) oldReg = GameObject.Find("high-pressure_regulator");
        if (oldReg == null) oldReg = GameObject.Find("Real_Gas_Regulator");

        GameObject regGo = null;
        if (regulatorGlb != null)
        {
            regGo = (GameObject)PrefabUtility.InstantiatePrefab(regulatorGlb);
            regGo.name = "Real_Gas_Regulator";
            regGo.transform.position = new Vector3(-0.25f, 1.05f, 0.20f);
            regGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            regGo.transform.localScale = new Vector3(0.016f, 0.016f, 0.016f);

            if (oldReg != null && oldReg != regGo)
            {
                UnityEngine.Object.DestroyImmediate(oldReg);
            }
        }

        // 3. Pressure Gauge (Original GLB)
        GameObject oldGauge = GameObject.Find("Pressure_Gauge_3D");
        if (oldGauge == null) oldGauge = GameObject.Find("pressure_gauge");
        if (oldGauge == null) oldGauge = GameObject.Find("Real_Pressure_Gauge");

        GameObject gaugeGo = null;
        if (gaugeGlb != null)
        {
            gaugeGo = (GameObject)PrefabUtility.InstantiatePrefab(gaugeGlb);
            gaugeGo.name = "Real_Pressure_Gauge";
            gaugeGo.transform.position = new Vector3(-0.25f, 1.16f, 0.20f);
            gaugeGo.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            gaugeGo.transform.localScale = new Vector3(0.016f, 0.016f, 0.016f);

            if (oldGauge != null && oldGauge != gaugeGo)
            {
                UnityEngine.Object.DestroyImmediate(oldGauge);
            }
        }

        // 4. Isolation Valve 3D
        GameObject valveGo = GameObject.Find("Isolation_Valve_3D");
        if (valveGo != null)
        {
            valveGo.transform.position = new Vector3(-0.07f, 1.05f, 0.20f);
            valveGo.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            valveGo.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        }

        // 5. Multi-Gas Detector
        GameObject detectorGo = GameObject.Find("Multi_Gas_Detector");
        if (detectorGo == null) detectorGo = GameObject.Find("Gas_Detector");
        if (detectorGo != null)
        {
            detectorGo.transform.position = new Vector3(-0.05f, 0.02f, 0.20f);
            detectorGo.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        // 6. PPE Workbench & Boots
        GameObject workbenchGo = GameObject.Find("PPE_Workbench");
        if (workbenchGo != null)
        {
            workbenchGo.transform.position = new Vector3(-1.15f, 0.00f, 0.20f);
            workbenchGo.transform.rotation = Quaternion.identity;
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

        // 7. Confined Space Industrial Vessel Tank
        GameObject vesselGo = GameObject.Find("Confined_Space_Vessel_Tank");
        if (vesselGo != null)
        {
            vesselGo.transform.position = new Vector3(0.85f, 0.361f, 0.20f);
            vesselGo.transform.rotation = Quaternion.Euler(270f, 0f, 0f);
            vesselGo.transform.localScale = new Vector3(0.903f, 0.903f, 0.903f);
        }

        // Hide or position floating extra items if any
        GameObject fanGo = GameObject.Find("Axial_Ventilation_Fan_Real");
        if (fanGo == null) fanGo = GameObject.Find("Axial_Ventilation_Fan");
        if (fanGo != null)
        {
            fanGo.transform.position = new Vector3(1.20f, 0.20f, 0.20f);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Master Target Workstation Scene Saved.");

        // Render proof images matching master target layout
        RenderView("GAS_MASTER_FRONT.png", new Vector3(-0.1f, 0.65f, -2.8f), Quaternion.Euler(2f, 0f, 0f), 50f);
        RenderView("GAS_MASTER_LEFT_3_4.png", new Vector3(-2.8f, 1.2f, -1.8f), Quaternion.Euler(12f, 45f, 0f), 50f);
        RenderView("GAS_MASTER_RIGHT_3_4.png", new Vector3(2.6f, 1.2f, -1.8f), Quaternion.Euler(12f, -45f, 0f), 50f);
        RenderView("GAS_MASTER_FULL.png", new Vector3(-0.15f, 0.65f, -3.2f), Quaternion.Euler(2f, 0f, 0f), 54f);

        Debug.Log("Master Target screenshots rendered successfully.");
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
