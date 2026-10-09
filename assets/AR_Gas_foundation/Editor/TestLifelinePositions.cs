using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class TestLifelinePositions
{
    [MenuItem("Tools/Test Lifeline Positions")]
    public static void Execute()
    {
        string mainScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        
        // 1. Create timestamped backup of current scene FIRST
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmm");
        string backupPath = $"Assets/AR_Gas_foundation/scenes/AR_gas_Foundation_Backup_{timestamp}_YellowModelFix.unity";
        AssetDatabase.CopyAsset(mainScenePath, backupPath);
        AssetDatabase.Refresh();
        Debug.Log($"BACKUP CREATED: {backupPath}");

        // Open main scene
        EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Single);

        // Find Self_Retracting_Lifeline
        GameObject lifelineObj = GameObject.Find("Self_Retracting_Lifeline");
        if (lifelineObj == null)
        {
            Debug.LogError("COULD NOT FIND 'Self_Retracting_Lifeline'!");
            return;
        }

        Debug.Log($"ORIGINAL LIFELINE POS: {lifelineObj.transform.position.ToString("F4")}, ROT: {lifelineObj.transform.eulerAngles.ToString("F4")}, SCALE: {lifelineObj.transform.localScale.ToString("F4")}");

        // Find renderers under lifeline to get bounds
        Renderer[] renderers = lifelineObj.GetComponentsInChildren<Renderer>();
        Bounds totalBounds = new Bounds();
        bool first = true;
        foreach (Renderer r in renderers)
        {
            if (first) { totalBounds = r.bounds; first = false; }
            else { totalBounds.Encapsulate(r.bounds); }
            Debug.Log($"  Sub-renderer: {r.gameObject.name} | BoundsCenter: {r.bounds.center.ToString("F4")} | BoundsSize: {r.bounds.size.ToString("F4")} | MinY: {r.bounds.min.y:F4}, MaxY: {r.bounds.max.y:F4}");
        }

        Debug.Log($"TOTAL LIFELINE BOUNDS: Center: {totalBounds.center.ToString("F4")}, Size: {totalBounds.size.ToString("F4")}, Min: {totalBounds.min.ToString("F4")}, Max: {totalBounds.max.ToString("F4")}");

        // Let's render a few test Y offsets and save preview screenshots to artifact folder
        float[] testYPositions = new float[] { 1.0850f, 1.1500f, 1.2000f, 1.2500f, 1.2800f, 1.3000f, 1.3200f };

        string artifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";

        for (int i = 0; i < testYPositions.Length; i++)
        {
            float posY = testYPositions[i];
            lifelineObj.transform.position = new Vector3(2.2475f, posY, 0.2000f);

            // Render close-up screenshot of Yellow Model + Confined Space Top
            RenderCloseUpScreen(Path.Combine(artifactDir, $"YELLOW_TEST_Y_{posY:F4}.png"), lifelineObj.transform.position);
        }
    }

    private static void RenderCloseUpScreen(string outputPath, Vector3 focusPos)
    {
        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
        if (cam == null) return;

        Vector3 origPos = cam.transform.position;
        Quaternion origRot = cam.transform.rotation;
        float origFov = cam.fieldOfView;

        // Position camera to look directly at the yellow model and top mounting structure of confined space equipment
        cam.transform.position = new Vector3(2.2475f, focusPos.y + 0.05f, -0.65f);
        cam.transform.rotation = Quaternion.Euler(0, 0, 0);
        cam.fieldOfView = 35f;

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

        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(outputPath, bytes);
        Debug.Log($"Saved preview screenshot to {outputPath}");

        cam.transform.position = origPos;
        cam.transform.rotation = origRot;
        cam.fieldOfView = origFov;
    }
}
