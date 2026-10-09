#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class RenderRestoredSceneScreenshots
{
    [MenuItem("SurakshaAR/Render Restored Screenshots")]
    public static void RenderScreenshots()
    {
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        Debug.Log("Rendering screenshots for scene: " + scene.path);

        GameObject camObj = new GameObject("RenderCam");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.18f, 0.20f, 0.24f, 1f);
        cam.fieldOfView = 50f;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 100f;

        GameObject lightObj = new GameObject("SunLight");
        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = 1.2f;
        l.color = new Color(1.0f, 0.96f, 0.90f);
        lightObj.transform.rotation = Quaternion.Euler(40f, -30f, 0f);

        GameObject fillObj = new GameObject("FillLight");
        Light fl = fillObj.AddComponent<Light>();
        fl.type = LightType.Directional;
        fl.intensity = 0.6f;
        fl.color = new Color(0.85f, 0.90f, 1.0f);
        fillObj.transform.rotation = Quaternion.Euler(20f, 140f, 0f);

        int width = 1920;
        int height = 1080;
        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;

        string outDir = @"C:\Users\anjan\.gemini\antigravity\brain\3ece2838-6cbc-471f-be3f-02201ace83cb";
        string outDir2 = @"C:\SurakshaAR\Screenshots";
        if (!Directory.Exists(outDir2)) Directory.CreateDirectory(outDir2);

        // View 1: Full Workstation Overview (PPE Table LEFT | Gas Cylinder CENTER | Confined Space RIGHT)
        cam.transform.position = new Vector3(0.4f, 1.6f, -4.8f);
        cam.transform.rotation = Quaternion.Euler(11f, 0f, 0f);
        cam.Render();
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        byte[] bytesFull = tex.EncodeToPNG();
        File.WriteAllBytes(Path.Combine(outDir, "RESTORED_FINAL_FULL.png"), bytesFull);
        File.WriteAllBytes(Path.Combine(outDir2, "RESTORED_FINAL_FULL.png"), bytesFull);

        // View 2: Confined Space Front Upright
        cam.transform.position = new Vector3(3.0f, 1.2f, -3.5f);
        cam.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
        cam.Render();
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        byte[] bytesFront = tex.EncodeToPNG();
        File.WriteAllBytes(Path.Combine(outDir, "RESTORED_FINAL_FRONT.png"), bytesFront);
        File.WriteAllBytes(Path.Combine(outDir2, "RESTORED_FINAL_FRONT.png"), bytesFront);

        // View 3: Confined Space Ground Contact
        cam.transform.position = new Vector3(3.0f, 0.4f, -2.5f);
        cam.transform.rotation = Quaternion.Euler(5f, 0f, 0f);
        cam.Render();
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        byte[] bytesGround = tex.EncodeToPNG();
        File.WriteAllBytes(Path.Combine(outDir, "RESTORED_FINAL_GROUND.png"), bytesGround);
        File.WriteAllBytes(Path.Combine(outDir2, "RESTORED_FINAL_GROUND.png"), bytesGround);

        RenderTexture.active = null;
        cam.targetTexture = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(camObj);
        Object.DestroyImmediate(lightObj);
        Object.DestroyImmediate(fillObj);

        Debug.Log("[RENDER SUCCESS] Restored scene screenshots saved to " + outDir);
    }
}
#endif
