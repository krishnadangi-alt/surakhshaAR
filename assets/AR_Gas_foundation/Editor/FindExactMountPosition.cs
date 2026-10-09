using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class FindExactMountPosition
{
    [MenuItem("Tools/Find Exact Mount Position")]
    public static void Execute()
    {
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject yellowObj = GameObject.Find("Self_Retracting_Lifeline");
        if (yellowObj == null)
        {
            Debug.LogError("COULD NOT FIND Self_Retracting_Lifeline!");
            return;
        }

        string artifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\a1a848aa-7529-4b66-b451-f359d5eb909f";

        // Let's test different Y positions and X alignment
        // Current Pos: X=2.2475, Y=1.0850, Z=0.2000
        float[] testYValues = new float[] { 0.85f, 0.90f, 0.95f, 0.98f, 1.00f, 1.02f, 1.05f, 1.085f, 1.12f, 1.15f, 1.20f };

        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();

        foreach (float y in testYValues)
        {
            yellowObj.transform.position = new Vector3(2.2475f, y, 0.2000f);

            // Front View (Camera facing yellow model and top of equipment)
            RenderView(cam, Path.Combine(artifactDir, $"MOUNT_FRONT_Y_{y:F3}.png"), new Vector3(2.2475f, 1.05f, -1.20f), Quaternion.Euler(0, 0, 0), 40f);

            // Side/Perspective View
            RenderView(cam, Path.Combine(artifactDir, $"MOUNT_PERSP_Y_{y:F3}.png"), new Vector3(1.60f, 1.10f, -1.00f), Quaternion.Euler(15, 30, 0), 40f);
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

        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(outputPath, bytes);

        cam.transform.position = origPos;
        cam.transform.rotation = origRot;
        cam.fieldOfView = origFov;
    }
}
