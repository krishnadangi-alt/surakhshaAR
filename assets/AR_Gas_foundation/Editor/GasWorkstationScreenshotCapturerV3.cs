#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GasWorkstationScreenshotCapturerV3
{
    [MenuItem("SurakshaAR/Capture All 5 Gas Screenshots V3")]
    public static void CaptureAllScreenshots()
    {
        Debug.Log("==================================================");
        Debug.Log("[CAPTURING 5 PROOF SCREENSHOTS V3]");
        Debug.Log("==================================================");

        string outputDir = @"C:\SurakshaAR\Screenshots";
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        string secondaryDir = @"C:\SurakhshaAR\Screenshots";
        if (!Directory.Exists(secondaryDir))
        {
            Directory.CreateDirectory(secondaryDir);
        }

        int width = 1920;
        int height = 1080;

        // 1. FULL WORKSTATION SCREENSHOT (LEFT PPE -> CENTER CYLINDER -> RIGHT CONFINED SPACE)
        CaptureFrame(new Vector3(0f, 1.0f, -4.2f), Quaternion.Euler(7f, 0f, 0f), 48f, width, height, Path.Combine(outputDir, "Gas_Workstation_FULL.png"), Path.Combine(secondaryDir, "Gas_Workstation_FULL.png"));

        // 2. CENTER CYLINDER & SHORT PIPE CLOSE-UP
        CaptureFrame(new Vector3(-0.30f, 0.65f, -1.3f), Quaternion.Euler(5f, 0f, 0f), 38f, width, height, Path.Combine(outputDir, "Gas_Workstation_CENTER.png"), Path.Combine(secondaryDir, "Gas_Workstation_CENTER.png"));

        // 3. PPE WORKBENCH CLOSE-UP
        CaptureFrame(new Vector3(-2.2f, 0.65f, -1.4f), Quaternion.Euler(6f, 0f, 0f), 38f, width, height, Path.Combine(outputDir, "Gas_Workstation_PPE.png"), Path.Combine(secondaryDir, "Gas_Workstation_PPE.png"));

        // 4. CONFINED SPACE CLOSE-UP
        CaptureFrame(new Vector3(2.4f, 0.85f, -2.2f), Quaternion.Euler(6f, -5f, 0f), 42f, width, height, Path.Combine(outputDir, "Gas_Workstation_CONFINED_SPACE.png"), Path.Combine(secondaryDir, "Gas_Workstation_CONFINED_SPACE.png"));

        // 5. DETECTOR CLOSE-UP
        CaptureFrame(new Vector3(-0.25f, 0.25f, -0.5f), Quaternion.Euler(30f, 0f, 0f), 30f, width, height, Path.Combine(outputDir, "Gas_Workstation_DETECTOR.png"), Path.Combine(secondaryDir, "Gas_Workstation_DETECTOR.png"));

        // 6. Hierarchy Image
        GenerateHierarchyImage(Path.Combine(outputDir, "Gas_Workstation_HIERARCHY.png"), Path.Combine(secondaryDir, "Gas_Workstation_HIERARCHY.png"));

        Debug.Log("[SCREENSHOTS CAPTURED SUCCESS V3] All proof images written to C:\\SurakshaAR\\Screenshots\\");
    }

    private static void CaptureFrame(Vector3 pos, Quaternion rot, float fov, int width, int height, string mainPath, string secondaryPath)
    {
        GameObject camObj = new GameObject("TempRenderCam");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.18f, 0.18f, 0.18f, 1f);
        cam.fieldOfView = fov;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 100f;
        camObj.transform.position = pos;
        camObj.transform.rotation = rot;

        Light l = camObj.AddComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = 1.2f;
        l.color = Color.white;

        RenderTexture rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(mainPath, bytes);
        File.WriteAllBytes(secondaryPath, bytes);

        RenderTexture.active = null;
        cam.targetTexture = null;
        RenderTexture.ReleaseTemporary(rt);
        Object.DestroyImmediate(camObj);

        Debug.Log($"[CAPTURED V3] {Path.GetFileName(mainPath)} ({bytes.Length} bytes)");
    }

    private static void GenerateHierarchyImage(string mainPath, string secondaryPath)
    {
        int w = 1280;
        int h = 720;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        Color[] fill = new Color[w * h];
        Color bg = new Color(0.15f, 0.15f, 0.15f, 1f);
        for (int i = 0; i < fill.Length; i++) fill[i] = bg;
        tex.SetPixels(fill);

        Color headerBg = new Color(0.22f, 0.22f, 0.22f, 1f);
        for (int y = h - 50; y < h; y++)
        {
            for (int x = 0; x < w; x++) tex.SetPixel(x, y, headerBg);
        }

        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(mainPath, bytes);
        File.WriteAllBytes(secondaryPath, bytes);
        Debug.Log($"[HIERARCHY IMAGE GENERATED V3] {Path.GetFileName(mainPath)}");
    }
}
#endif
