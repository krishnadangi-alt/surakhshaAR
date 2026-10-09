using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class PlaceOrangeGasDetectorOnFloor
{
    [MenuItem("Tools/Place Orange Gas Detector On Floor")]
    public static void Execute()
    {
        string mainScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";

        // STEP 0: BACKUP FIRST
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmm");
        string backupPath = $"Assets/AR_Gas_foundation/scenes/AR_gas_Foundation_Backup_{timestamp}_GasDetectorPlacement.unity";
        AssetDatabase.CopyAsset(mainScenePath, backupPath);
        AssetDatabase.Refresh();
        Debug.Log($"=== BACKUP CREATED AT: {backupPath} ===");

        EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);

        // Find or instantiate the Orange Gas Detector
        GameObject orangeDetector = GameObject.Find("Orange_Gas_Detector_Floor");
        if (orangeDetector == null)
        {
            // Try loading existing prefabs
            GameObject detectorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasDetector/Prefabs/h2s_gas_detector_real.prefab");
            if (detectorPrefab == null) detectorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.prefab");
            if (detectorPrefab == null) detectorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AR_Gas_foundation/3d/GasDetector/h2s_gas_detector.prefab");

            if (detectorPrefab != null)
            {
                orangeDetector = (GameObject)PrefabUtility.InstantiatePrefab(detectorPrefab);
                orangeDetector.name = "Orange_Gas_Detector_Floor";
            }
            else
            {
                Debug.LogError("Could not find orange gas detector prefab!");
                return;
            }
        }

        orangeDetector.SetActive(true);

        // POSITION: On the floor at the lower-left side of the red gas cylinder (Cylinder is at X = -0.2500)
        // Detector Target World Position: X = -0.5500, Z = 0.1500 (between PPE table and cylinder)
        orangeDetector.transform.position = new Vector3(-0.5500f, 0.0000f, 0.1500f);
        orangeDetector.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        
        // Scale appropriately for real handheld gas detector
        orangeDetector.transform.localScale = new Vector3(0.0015f, 0.0015f, 0.0015f);

        // Align min.y to exact floor level (Y = 0.0000)
        Renderer r = orangeDetector.GetComponentInChildren<Renderer>();
        if (r != null)
        {
            float deltaY = 0.0000f - r.bounds.min.y;
            orangeDetector.transform.position += new Vector3(0, deltaY, 0);
            Debug.Log($"Orange Gas Detector aligned to floor. World Center: {r.bounds.center.ToString("F4")}, min.y: {r.bounds.min.y.ToString("F4")}");
        }

        // SAVE MODIFIED SCENE
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"SAVED SCENE TO: {mainScenePath}");

        // RENDER VERIFICATION SCREENSHOT (Front-view showing cylinder, detector, table, and workstation)
        string artifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindAnyObjectByType<Camera>();

        RenderView(cam, Path.Combine(artifactDir, "ORANGE_GAS_DETECTOR_PLACEMENT.png"), new Vector3(-0.3500f, 0.65f, -2.10f), Quaternion.Euler(0, 0, 0), 40f);
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

        File.WriteAllBytes(outputPath, screenShot.EncodeToPNG());
        Debug.Log($"Saved verification screenshot: {outputPath}");

        cam.transform.position = origPos;
        cam.transform.rotation = origRot;
        cam.fieldOfView = origFov;
    }
}
