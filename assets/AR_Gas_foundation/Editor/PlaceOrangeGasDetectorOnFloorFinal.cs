using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class PlaceOrangeGasDetectorOnFloorFinal
{
    [MenuItem("SurakshaAR/Fix Gas Detector Only")]
    [MenuItem("Tools/Place Orange Gas Detector On Floor Final")]
    [MenuItem("Tools/Place Orange Gas Detector On Floor")]
    public static void Execute()
    {
        string mainScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";

        // Open scene
        var scene = EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);

        // Find GasScenarioRoot
        GameObject scenarioRoot = GameObject.Find("GasScenarioRoot");
        Transform parentTransform = scenarioRoot != null ? scenarioRoot.transform : null;

        // Clean up any extra or fake detector instances
        string[] staleNames = new[] { "Orange_Gas_Detector_Floor", "PPE_Gas_Detector", "GLB_Multi_Gas_Detector", "Gas_Detector" };
        foreach (var sName in staleNames)
        {
            GameObject staleGo = GameObject.Find(sName);
            if (staleGo != null)
            {
                Object.DestroyImmediate(staleGo);
            }
        }

        // Find or instantiate Multi_Gas_Detector using multi_gas_detector_-_low_poly.prefab
        GameObject orangeDetector = GameObject.Find("Multi_Gas_Detector");
        string prefabPath = "Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.prefab";
        GameObject detectorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (detectorPrefab == null)
        {
            Debug.LogError("Could not find orange gas detector prefab at: " + prefabPath);
            return;
        }

        if (orangeDetector == null)
        {
            orangeDetector = (GameObject)PrefabUtility.InstantiatePrefab(detectorPrefab, parentTransform);
            orangeDetector.name = "Multi_Gas_Detector";
        }

        orangeDetector.SetActive(true);

        // Exact placement: on the floor, lower-left of the red cylinder (cylinder is at X=-0.25, Z=0.20)
        // Position: X = -0.48, Z = 0.10
        // Orientation: Upright, rotated 180 degrees around Y to face the front view directly
        // Scale: Vector3(0.05, 0.05, 0.05) gives realistic ~23cm height in AR
        orangeDetector.transform.parent = parentTransform;
        orangeDetector.transform.localPosition = new Vector3(-0.4800f, 0.1193f, 0.1000f);
        orangeDetector.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        orangeDetector.transform.localScale = new Vector3(0.0500f, 0.0500f, 0.0500f);

        // Align min.y precisely to floor level (Y = 0.0000)
        Renderer r = orangeDetector.GetComponentInChildren<Renderer>();
        if (r != null)
        {
            float deltaY = 0.0000f - r.bounds.min.y;
            orangeDetector.transform.position += new Vector3(0, deltaY, 0);
            Debug.Log($"Orange Gas Detector aligned to floor: Bounds Center={r.bounds.center.ToString("F4")}, min.y={r.bounds.min.y.ToString("F4")}, size={r.bounds.size.ToString("F4")}");
        }

        // Update GasEnvironmentBuilder reference if present
        GasEnvironmentBuilder builder = Object.FindFirstObjectByType<GasEnvironmentBuilder>();
        if (builder != null)
        {
            SerializedObject builderSo = new SerializedObject(builder);
            SerializedProperty detProp = builderSo.FindProperty("gasDetectorPrefab");
            if (detProp != null)
            {
                detProp.objectReferenceValue = detectorPrefab;
                builderSo.ApplyModifiedProperties();
            }
        }

        // Save modified scene
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"SAVED SCENE TO: {mainScenePath}");

        // Render verification screenshots
        string artifactDir = @"C:\Users\anjan\.gemini\antigravity-ide\brain\264d3be8-9397-4468-9849-c5bf2ed82521";
        string projectScreenshotDir = @"C:\SurakhshaAR\Assets\AR_Gas_foundation\Screenshots";

        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            RenderView(cam, Path.Combine(artifactDir, "FINAL_FRONT_VIEW_ORANGE_DETECTOR.png"), new Vector3(-0.40f, 0.60f, -1.80f), Quaternion.Euler(5f, 0f, 0f), 52f);
            RenderView(cam, Path.Combine(projectScreenshotDir, "FINAL_FRONT_VIEW_ORANGE_DETECTOR.png"), new Vector3(-0.40f, 0.60f, -1.80f), Quaternion.Euler(5f, 0f, 0f), 52f);
        }
    }

    private static void RenderView(Camera cam, string outputPath, Vector3 pos, Quaternion rot, float fov)
    {
        Vector3 origPos = cam.transform.position;
        Quaternion origRot = cam.transform.rotation;
        float origFov = cam.fieldOfView;

        cam.transform.position = pos;
        cam.transform.rotation = rot;
        cam.fieldOfView = fov;

        RenderTexture rt = new RenderTexture(1920, 1080, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        screenShot.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);

        string dir = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllBytes(outputPath, screenShot.EncodeToPNG());
        Debug.Log($"Saved verification screenshot: {outputPath}");

        cam.transform.position = origPos;
        cam.transform.rotation = origRot;
        cam.fieldOfView = origFov;
    }
}
