using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class TestXandYPositions
{
    [MenuItem("Tools/Test X and Y Positions")]
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

        Camera mainCam = Camera.main;
        if (mainCam == null) mainCam = Object.FindFirstObjectByType<Camera>();

        // Test 1: X = 1.55 (Centered on top of the main vessel dome dome top)
        // Dome top Y is around 1.05..1.10
        float[] testY_Dome = new float[] { 1.05f, 1.10f, 1.15f, 1.20f, 1.25f, 1.30f };

        foreach (float y in testY_Dome)
        {
            yellowObj.transform.position = new Vector3(1.5500f, y, 0.2000f);

            RenderFullWorkstation(mainCam, Path.Combine(artifactDir, $"TEST_DOME_FULL_Y_{y:F2}.png"));
            RenderCloseup(mainCam, Path.Combine(artifactDir, $"TEST_DOME_CLOSEUP_Y_{y:F2}.png"), new Vector3(1.55f, y, 0.20f));
        }

        // Test 2: X = 2.2475 (On the right frame/gantry top)
        float[] testY_Frame = new float[] { 0.70f, 0.75f, 0.80f, 0.85f, 0.90f };

        foreach (float y in testY_Frame)
        {
            yellowObj.transform.position = new Vector3(2.2475f, y, 0.2000f);

            RenderFullWorkstation(mainCam, Path.Combine(artifactDir, $"TEST_FRAME_FULL_Y_{y:F2}.png"));
            RenderCloseup(mainCam, Path.Combine(artifactDir, $"TEST_FRAME_CLOSEUP_Y_{y:F2}.png"), new Vector3(2.2475f, y, 0.20f));
        }
    }

    private static void RenderFullWorkstation(Camera cam, string outputPath)
    {
        Vector3 origPos = cam.transform.position;
        Quaternion origRot = cam.transform.rotation;
        float origFov = cam.fieldOfView;

        cam.transform.position = new Vector3(0.0000f, 1.1500f, -1.3500f);
        cam.transform.rotation = Quaternion.Euler(3.6914f, 0.0000f, 0.0000f);
        cam.fieldOfView = 60f;

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

        cam.transform.position = origPos;
        cam.transform.rotation = origRot;
        cam.fieldOfView = origFov;
    }

    private static void RenderCloseup(Camera cam, string outputPath, Vector3 focusPos)
    {
        Vector3 origPos = cam.transform.position;
        Quaternion origRot = cam.transform.rotation;
        float origFov = cam.fieldOfView;

        cam.transform.position = new Vector3(focusPos.x, focusPos.y, focusPos.z - 1.20f);
        cam.transform.rotation = Quaternion.Euler(0, 0, 0);
        cam.fieldOfView = 45f;

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

        cam.transform.position = origPos;
        cam.transform.rotation = origRot;
        cam.fieldOfView = origFov;
    }
}
