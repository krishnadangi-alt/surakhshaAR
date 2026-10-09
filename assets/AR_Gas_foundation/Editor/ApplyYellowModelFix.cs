using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class ApplyYellowModelFix
{
    [MenuItem("Tools/Apply Yellow Model Fix")]
    public static void Execute()
    {
        string mainScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";

        // 1. BACKUP CURRENT SCENE FIRST (Timestamped backup)
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmm");
        string backupPath = $"Assets/AR_Gas_foundation/scenes/AR_gas_Foundation_Backup_{timestamp}_YellowModelFix.unity";
        AssetDatabase.CopyAsset(mainScenePath, backupPath);
        AssetDatabase.Refresh();
        Debug.Log($"=== BACKUP CREATED AT: {backupPath} ===");

        // Open main scene
        EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);

        // 2. FIND YELLOW MODEL
        GameObject yellowModel = GameObject.Find("Self_Retracting_Lifeline");
        if (yellowModel == null)
        {
            Debug.LogError("COULD NOT FIND 'Self_Retracting_Lifeline'!");
            return;
        }

        Vector3 oldPos = yellowModel.transform.position;
        Debug.Log($"PREVIOUS YELLOW MODEL POS: {oldPos.ToString("F4")}");

        // 3. UPDATE POSITION SURGICALLY
        // Target position: X=2.2475, Y=0.9520, Z=0.2000
        // Mounts the bottom hook latch perfectly on top of the equipment structure mounting bar
        Vector3 newPos = new Vector3(2.2475f, 0.9520f, 0.2000f);
        yellowModel.transform.position = newPos;

        Debug.Log($"NEW YELLOW MODEL POS: {yellowModel.transform.position.ToString("F4")}");

        // 4. SAVE SCENE
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log($"SAVED SCENE: {mainScenePath}");

        // 5. RENDER MANDATORY VERIFICATION SCREENSHOTS
        string artifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";

        Camera mainCam = Camera.main;
        if (mainCam == null) mainCam = Object.FindFirstObjectByType<Camera>();

        // Render Full Workstation View
        RenderView(mainCam, Path.Combine(artifactDir, "YELLOW_FIX_FULL_WORKSTATION.png"), new Vector3(0.50f, 1.00f, -3.20f), Quaternion.Euler(0, 0, 0), 45f);

        // Render Close-up View of Yellow Model + Confined Space Top
        RenderView(mainCam, Path.Combine(artifactDir, "YELLOW_FIX_CLOSEUP.png"), new Vector3(2.2475f, 0.9520f, -0.90f), Quaternion.Euler(0, 0, 0), 40f);
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
