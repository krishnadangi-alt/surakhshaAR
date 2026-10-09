using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class ApplyProjectBackupTransformsToMainScene
{
    struct TransformData
    {
        public string name;
        public string parentName;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
        public bool active;
    }

    [MenuItem("Tools/Apply ProjectBackup Transforms To Main Scene")]
    public static void Execute()
    {
        string mainScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        string backupScenePath = "Assets/AR_Gas_foundation/scenes/ProjectBackup_Temp.unity";

        Debug.Log("Starting transform copy from ProjectBackup/AR_gas_Foundation_Backup.unity to AR_gas_Foundation.unity...");

        // Step 1: Load ProjectBackup scene to capture transforms
        Scene backupScene = EditorSceneManager.OpenScene(backupScenePath, OpenSceneMode.Single);
        
        Dictionary<string, TransformData> backupMap = new Dictionary<string, TransformData>();

        foreach (GameObject rootObj in backupScene.GetRootGameObjects())
        {
            RecordTransformsRecursive(rootObj, "", backupMap);
        }

        Debug.Log($"Recorded {backupMap.Count} object transforms from ProjectBackup.");

        // Step 2: Load Main Scene
        Scene mainScene = EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);

        // Apply transforms to all objects
        foreach (GameObject rootObj in mainScene.GetRootGameObjects())
        {
            ApplyTransformsRecursive(rootObj, "", backupMap);
        }

        // Specifically ensure main workstation objects are active and positioned correctly
        EnsureWorkstationActive(mainScene, backupMap);

        // Mark scene dirty and save
        EditorSceneManager.MarkSceneDirty(mainScene);
        EditorSceneManager.SaveScene(mainScene);
        Debug.Log($"Successfully updated and saved {mainScenePath}");

        // Delete temporary scene asset
        AssetDatabase.DeleteAsset(backupScenePath);

        // Step 3: Render preview screenshot
        RenderScreenshot();
    }

    private static void RecordTransformsRecursive(GameObject go, string path, Dictionary<string, TransformData> backupMap)
    {
        string fullPath = string.IsNullOrEmpty(path) ? go.name : path + "/" + go.name;
        
        TransformData td = new TransformData
        {
            name = go.name,
            parentName = go.transform.parent != null ? go.transform.parent.name : "",
            localPosition = go.transform.localPosition,
            localRotation = go.transform.localRotation,
            localScale = go.transform.localScale,
            active = go.activeSelf
        };

        if (!backupMap.ContainsKey(go.name))
        {
            backupMap[go.name] = td;
        }
        backupMap[fullPath] = td;

        for (int i = 0; i < go.transform.childCount; i++)
        {
            RecordTransformsRecursive(go.transform.GetChild(i).gameObject, fullPath, backupMap);
        }
    }

    private static void ApplyTransformsRecursive(GameObject go, string path, Dictionary<string, TransformData> backupMap)
    {
        string fullPath = string.IsNullOrEmpty(path) ? go.name : path + "/" + go.name;

        TransformData td;
        if (backupMap.TryGetValue(fullPath, out td) || backupMap.TryGetValue(go.name, out td))
        {
            go.SetActive(true); // Ensure object is active
            go.transform.localPosition = td.localPosition;
            go.transform.localRotation = td.localRotation;
            go.transform.localScale = td.localScale;
        }

        for (int i = 0; i < go.transform.childCount; i++)
        {
            ApplyTransformsRecursive(go.transform.GetChild(i).gameObject, fullPath, backupMap);
        }
    }

    private static void EnsureWorkstationActive(Scene scene, Dictionary<string, TransformData> backupMap)
    {
        List<string> workstationNames = new List<string>
        {
            "PPE_Preparation_Station", "Real_PPE_Workbench", "PPE_Workbench", "Safety_Helmet", "Safety_Goggles",
            "Work_Gloves_Left", "Work_Gloves_Right", "Safety_Boots_Left", "Safety_Boots_Right",
            "Gas_Leak_Station", "Realistic_Gas_Cylinder", "Real_Gas_Regulator", "Gas_Regulator_3D",
            "Real_Pressure_Gauge", "Pressure_Gauge_3D", "Horizontal_Pipe_1", "Coupler_Pipe1_Valve",
            "Isolation_Valve_3D", "Horizontal_Pipe_2", "Coupler_Pipe2_Elbow", "Pipe_Elbow_3D",
            "Downstream_Pipe_3D", "Multi_Gas_Detector", "H2S_Gas_Detector",
            "Confined_Space_Training_Area", "Confined_Space_Vessel_Tank", "Real_Confined_Space_Model",
            "Self_Retracting_Lifeline", "Axial_Ventilation_Fan"
        };

        foreach (GameObject rootObj in scene.GetRootGameObjects())
        {
            Transform[] allTransforms = rootObj.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allTransforms)
            {
                if (workstationNames.Contains(t.name))
                {
                    t.gameObject.SetActive(true);
                    TransformData td;
                    if (backupMap.TryGetValue(t.name, out td))
                    {
                        t.localPosition = td.localPosition;
                        t.localRotation = td.localRotation;
                        t.localScale = td.localScale;
                    }
                }
            }
        }
    }

    private static void RenderScreenshot()
    {
        // Disable XR origin during screenshot
        GameObject xrOrigin = GameObject.Find("XR Origin") ?? GameObject.Find("XRRig");
        bool wasXrActive = false;
        if (xrOrigin != null)
        {
            wasXrActive = xrOrigin.activeSelf;
            xrOrigin.SetActive(false);
        }

        // Create render camera
        GameObject camObj = new GameObject("ScreenshotRenderCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.15f, 0.20f);
        cam.fieldOfView = 50f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 1000f;

        // Position camera to frame full workstation (Left: PPE, Center: Cylinder & Pipeline, Right: Vessel)
        camObj.transform.position = new Vector3(-0.35f, 0.85f, -2.8f);
        camObj.transform.rotation = Quaternion.Euler(0, 0, 0);

        int width = 1920;
        int height = 1080;
        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);

        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(camObj);

        if (xrOrigin != null)
        {
            xrOrigin.SetActive(wasXrActive);
        }

        byte[] bytes = screenShot.EncodeToPNG();
        Object.DestroyImmediate(screenShot);

        string p1 = @"C:\SurakshaAR\Gas_Recovery_FINAL_PREVIEW.png";
        string p2 = @"C:\Users\anjan\.gemini\antigravity\brain\333cbd97-2829-4724-af91-85a64f05469f\Gas_Recovery_FINAL_PREVIEW.png";

        File.WriteAllBytes(p1, bytes);
        File.WriteAllBytes(p2, bytes);

        Debug.Log($"Rendered final preview screenshot to {p1} and {p2}");
    }
}
