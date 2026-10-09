using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class MasterGasWorkstationAssembly
{
    private static readonly string ArtifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\333cbd97-2829-4724-af91-85a64f05469f";
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";

    [MenuItem("Tools/Assemble Master Gas Workstation")]
    public static void AssembleAndRender()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Debug.Log("Opened scene for Master Workstation Assembly: " + scene.path);

        // 1. Ensure Original GLB Assets are used for Regulator & Pressure Gauge
        GameObject regulatorGo = GameObject.Find("Real_Gas_Regulator");
        if (regulatorGo == null) regulatorGo = GameObject.Find("high-pressure_regulator");
        if (regulatorGo == null) regulatorGo = GameObject.Find("Gas_Regulator_3D");

        GameObject gaugeGo = GameObject.Find("Real_Pressure_Gauge");
        if (gaugeGo == null) gaugeGo = GameObject.Find("pressure_gauge");
        if (gaugeGo == null) gaugeGo = GameObject.Find("Pressure_Gauge_3D");

        // Load original GLB assets from project
        UnityEngine.Object regulatorAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/AR_Gas_foundation/3d/GasRegulator/high-pressure_regulator.glb");
        UnityEngine.Object gaugeAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/AR_Gas_foundation/3d/PressureGauge/pressure_gauge.glb");

        if (regulatorAsset != null)
        {
            if (regulatorGo != null)
            {
                // Replace mesh or replace object with instantiated original GLB if needed, or update prefab reference
                Transform parent = regulatorGo.transform.parent;
                Vector3 origPos = regulatorGo.transform.position;
                Quaternion origRot = regulatorGo.transform.rotation;
                Vector3 origScale = regulatorGo.transform.localScale;

                GameObject newReg = (GameObject)PrefabUtility.InstantiatePrefab(regulatorAsset);
                newReg.name = "Real_Gas_Regulator";
                if (parent != null) newReg.transform.SetParent(parent, false);

                // Position at top of cylinder neck
                newReg.transform.position = new Vector3(0.0f, 1.05f, 0.0f);
                newReg.transform.rotation = Quaternion.Euler(0, 90, 0);
                newReg.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f); // Normalize scale if GLB default scale is large

                UnityEngine.Object.DestroyImmediate(regulatorGo);
                regulatorGo = newReg;
            }
        }

        if (gaugeAsset != null)
        {
            if (gaugeGo != null)
            {
                Transform parent = gaugeGo.transform.parent;
                GameObject newGauge = (GameObject)PrefabUtility.InstantiatePrefab(gaugeAsset);
                newGauge.name = "Real_Pressure_Gauge";
                if (parent != null) newGauge.transform.SetParent(parent, false);

                // Position on top of regulator mounting port
                newGauge.transform.position = new Vector3(0.0f, 1.16f, 0.0f);
                newGauge.transform.rotation = Quaternion.Euler(0, 0, 0);
                newGauge.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

                UnityEngine.Object.DestroyImmediate(gaugeGo);
                gaugeGo = newGauge;
            }
        }

        // 2. Adjust Gas Cylinder Position
        GameObject cylinderGo = GameObject.Find("Realistic_Gas_Cylinder");
        if (cylinderGo != null)
        {
            cylinderGo.transform.position = new Vector3(0.0f, 0.60f, 0.0f);
            cylinderGo.transform.rotation = Quaternion.identity;
        }

        // 3. Align Isolation Valve & Pipeline
        GameObject valveGo = GameObject.Find("Isolation_Valve_3D");
        if (valveGo != null)
        {
            valveGo.transform.position = new Vector3(0.18f, 1.05f, 0.0f);
            valveGo.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        // 4. Align Gas Detector on Floor
        GameObject detectorGo = GameObject.Find("Multi_Gas_Detector");
        if (detectorGo == null) detectorGo = GameObject.Find("Gas_Detector");
        if (detectorGo != null)
        {
            detectorGo.transform.position = new Vector3(-0.18f, 0.02f, -0.15f);
            detectorGo.transform.rotation = Quaternion.identity;
        }

        // 5. Align Workbench & PPE
        GameObject workbenchGo = GameObject.Find("PPE_Workbench");
        if (workbenchGo == null) workbenchGo = GameObject.Find("Real_PPE_Workbench");
        if (workbenchGo != null)
        {
            workbenchGo.transform.position = new Vector3(-1.65f, 0.0f, 0.0f);
        }

        GameObject bootsLeft = GameObject.Find("Safety_Boots_Left");
        if (bootsLeft == null) bootsLeft = GameObject.Find("Real_Safety_Boots_Left");
        if (bootsLeft != null)
        {
            bootsLeft.transform.position = new Vector3(-1.80f, 0.16f, 0.0f);
        }

        GameObject bootsRight = GameObject.Find("Safety_Boots_Right");
        if (bootsRight == null) bootsRight = GameObject.Find("Real_Safety_Boots_Right");
        if (bootsRight != null)
        {
            bootsRight.transform.position = new Vector3(-1.50f, 0.16f, 0.0f);
        }

        // Save Scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Master Workstation Assembly saved cleanly.");

        // Render proof images
        RenderView("GAS_MASTER_FRONT.png", new Vector3(0.3f, 1.1f, -3.8f), Quaternion.Euler(5f, 0f, 0f), 52f);
        RenderView("GAS_MASTER_LEFT_3_4.png", new Vector3(-3.2f, 1.5f, -2.8f), Quaternion.Euler(15f, 45f, 0f), 52f);
        RenderView("GAS_MASTER_RIGHT_3_4.png", new Vector3(3.5f, 1.5f, -2.8f), Quaternion.Euler(15f, -45f, 0f), 52f);
        RenderView("GAS_MASTER_FULL.png", new Vector3(0.5f, 1.3f, -4.6f), Quaternion.Euler(6f, 0f, 0f), 55f);

        Debug.Log("Rendered all 4 proof images.");
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
