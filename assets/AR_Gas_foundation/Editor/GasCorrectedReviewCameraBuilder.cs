#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GasCorrectedReviewCameraBuilder
{
    [MenuItem("SurakshaAR/Capture CORRECTED High-Quality Review Screenshots")]
    public static void BuildAndCaptureCorrectedScreenshots()
    {
        Debug.Log("==================================================");
        Debug.Log("[CORRECTED REVIEW CAMERA PASS] Setting up Proper Framing & Lighting");
        Debug.Log("==================================================");

        string correctedDir = @"C:\SurakhshaAR\ReviewScreenshots\CORRECTED";
        if (!Directory.Exists(correctedDir)) Directory.CreateDirectory(correctedDir);

        string reviewDir = @"C:\SurakhshaAR\ReviewScreenshots";
        if (!Directory.Exists(reviewDir)) Directory.CreateDirectory(reviewDir);

        // First rebuild workstation scene to ensure latest prefabs are instantiated
        GasRealWorkstationRebuilder.BuildRealWorkstationAndScreenshots();

        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject rootObj = GameObject.Find("GasScenarioRoot");
        if (rootObj == null)
        {
            Debug.LogError("[CORRECTED CAMERA] GasScenarioRoot NOT found.");
            return;
        }

        // Setup Review Camera with Far Near-Clip & Clean FOV
        GameObject camObj = new GameObject("CorrectedReviewCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.15f, 0.18f); // Professional dark industrial slate background
        cam.fieldOfView = 48f;
        cam.nearClipPlane = 0.10f;
        cam.farClipPlane = 100f;

        // Balanced Industrial 3-Point Lighting
        GameObject keyLightObj = new GameObject("ReviewKeyLight");
        Light keyLight = keyLightObj.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.intensity = 1.8f;
        keyLight.color = new Color(1.0f, 0.96f, 0.90f);
        keyLightObj.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

        GameObject fillLightObj = new GameObject("ReviewFillLight");
        Light fillLight = fillLightObj.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.intensity = 0.8f;
        fillLight.color = new Color(0.65f, 0.80f, 1.0f);
        fillLightObj.transform.rotation = Quaternion.Euler(-20f, 140f, 0f);

        GameObject rimLightObj = new GameObject("ReviewRimLight");
        Light rimLight = rimLightObj.AddComponent<Light>();
        rimLight.type = LightType.Directional;
        rimLight.intensity = 1.0f;
        rimLight.color = Color.white;
        rimLightObj.transform.rotation = Quaternion.Euler(25f, 165f, 0f);

        // 1. CAMERA A: FULL WORKSTATION (Wide unclipped framing of PPE + Cylinder + Detector + Vessel)
        PositionCamera(camObj, new Vector3(0.20f, 1.45f, -4.60f), new Vector3(0.20f, 0.90f, 0.20f));
        TakeScreenshotBoth(cam, correctedDir, reviewDir, "FULL_WORKSTATION.png");

        // 2. CAMERA B: CYLINDER CLOSE-UP (Positioned comfortably outside at Z=-1.85m)
        PositionCamera(camObj, new Vector3(0.00f, 0.85f, -1.85f), new Vector3(0.00f, 0.70f, 0.00f));
        TakeScreenshotBoth(cam, correctedDir, reviewDir, "CYLINDER_CLOSEUP.png");

        // 3. CAMERA C: DETECTOR (Positioned at Z=-0.95m looking down at yellow detector)
        PositionCamera(camObj, new Vector3(0.22f, 0.40f, -0.95f), new Vector3(0.22f, 0.12f, -0.15f));
        TakeScreenshotBoth(cam, correctedDir, reviewDir, "DETECTOR.png");

        // 4. CAMERA D: CONFINED SPACE (Positioned at Z=-2.75m framing vessel, ladder, hoist)
        PositionCamera(camObj, new Vector3(1.65f, 1.15f, -2.75f), new Vector3(1.65f, 0.85f, 0.10f));
        TakeScreenshotBoth(cam, correctedDir, reviewDir, "CONFINED_SPACE.png");

        // 5. ACTIVE LEAK & HAZARD ZONE SCREENSHOTS
        GameObject hazardRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hazardRing.name = "VisualHazardZoneRing";
        hazardRing.transform.SetParent(rootObj.transform, false);
        hazardRing.transform.localPosition = new Vector3(-0.035f, 0.01f, 0f);
        hazardRing.transform.localScale = new Vector3(1.80f, 0.002f, 1.80f);
        Material matHz = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        if (matHz.HasProperty("_BaseColor")) matHz.SetColor("_BaseColor", new Color(0.95f, 0.15f, 0.10f, 0.40f));
        if (matHz.HasProperty("_Color")) matHz.SetColor("_Color", new Color(0.95f, 0.15f, 0.10f, 0.40f));
        hazardRing.GetComponent<Renderer>().material = matHz;

        GameObject leakPlume = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        leakPlume.name = "VisualLeakPlume";
        leakPlume.transform.SetParent(rootObj.transform, false);
        leakPlume.transform.localPosition = new Vector3(-0.035f, 1.12f, 0f);
        leakPlume.transform.localScale = new Vector3(0.08f, 0.12f, 0.08f);
        Material matPlume = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        if (matPlume.HasProperty("_BaseColor")) matPlume.SetColor("_BaseColor", new Color(0.85f, 0.95f, 0.88f, 0.45f));
        if (matPlume.HasProperty("_Color")) matPlume.SetColor("_Color", new Color(0.85f, 0.95f, 0.88f, 0.45f));
        leakPlume.GetComponent<Renderer>().material = matPlume;

        PositionCamera(camObj, new Vector3(0.00f, 0.85f, -1.85f), new Vector3(0.00f, 0.70f, 0.00f));
        TakeScreenshotBoth(cam, correctedDir, reviewDir, "LEAK_ACTIVE.png");

        PositionCamera(camObj, new Vector3(0.20f, 1.45f, -4.60f), new Vector3(0.20f, 0.90f, 0.20f));
        TakeScreenshotBoth(cam, correctedDir, reviewDir, "HAZARD_ZONE.png");

        Object.DestroyImmediate(hazardRing);
        Object.DestroyImmediate(leakPlume);
        Object.DestroyImmediate(camObj);
        Object.DestroyImmediate(keyLightObj);
        Object.DestroyImmediate(fillLightObj);
        Object.DestroyImmediate(rimLightObj);

        Debug.Log("[CORRECTED REVIEW CAMERA PASS] All corrected review screenshots captured successfully!");
    }

    private static void PositionCamera(GameObject camObj, Vector3 pos, Vector3 target)
    {
        camObj.transform.position = pos;
        camObj.transform.LookAt(target);
    }

    private static void TakeScreenshotBoth(Camera cam, string primaryDir, string secondaryDir, string fileName)
    {
        string p1 = Path.Combine(primaryDir, fileName);
        string p2 = Path.Combine(secondaryDir, fileName);
        TakeScreenshot(cam, p1);
        try { File.Copy(p1, p2, true); } catch {}
    }

    private static void TakeScreenshot(Camera cam, string filePath)
    {
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

        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
        Object.DestroyImmediate(screenShot);
        Debug.Log($"[CORRECTED SCREENSHOT CAPTURED] {filePath}");
    }
}
#endif
