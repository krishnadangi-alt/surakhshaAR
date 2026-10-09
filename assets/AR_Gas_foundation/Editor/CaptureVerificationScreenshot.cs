#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class CaptureVerificationScreenshot
{
    [MenuItem("SurakshaAR/Capture Verification Screenshot")]
    public static void Execute()
    {
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        string artifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\e89740c3-7119-440e-9701-7245a2fa60a9";
        string screenshotPath = Path.Combine(artifactDir, "FINAL_FRONT_VIEW_ORANGE_DETECTOR.png");

        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
        
        GameObject tempCamObj = null;
        if (cam == null)
        {
            tempCamObj = new GameObject("TempVerificationCam");
            cam = tempCamObj.AddComponent<Camera>();
        }

        Vector3 origPos = cam.transform.position;
        Quaternion origRot = cam.transform.rotation;
        float origFov = cam.fieldOfView;

        // Position camera to capture clear FRONT VIEW showing [PPE TABLE] [ORANGE DETECTOR] [RED CYLINDER] [PIPELINE]
        cam.transform.position = new Vector3(-0.50f, 0.60f, -1.80f);
        cam.transform.rotation = Quaternion.Euler(5f, 0f, 0f);
        cam.fieldOfView = 50f;

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

        File.WriteAllBytes(screenshotPath, screenShot.EncodeToPNG());
        Debug.Log("Saved verification screenshot to: " + screenshotPath);

        cam.transform.position = origPos;
        cam.transform.rotation = origRot;
        cam.fieldOfView = origFov;

        if (tempCamObj != null)
        {
            Object.DestroyImmediate(tempCamObj);
        }
    }
}
#endif
