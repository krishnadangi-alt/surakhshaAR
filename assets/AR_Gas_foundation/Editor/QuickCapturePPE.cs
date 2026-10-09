using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class QuickCapturePPE
{
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
    private static readonly string OutPath = @"C:\Users\anjan\.gemini\antigravity-ide\brain\264d3be8-9397-4468-9849-c5bf2ed82521\CURRENT_PPE_STATE.png";

    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject lightGo = new GameObject("__TempLight");
        Light light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.30f;
        light.color = new Color(1.0f, 0.98f, 0.95f);
        lightGo.transform.rotation = Quaternion.Euler(32f, -22f, 0f);

        GameObject fillGo = new GameObject("__TempFill");
        Light fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.intensity = 0.50f;
        fill.color = new Color(0.92f, 0.94f, 1.0f);
        fillGo.transform.rotation = Quaternion.Euler(-20f, 155f, 0f);

        GameObject camGo = new GameObject("__TempCam");
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.16f, 0.18f, 0.22f);
        cam.fieldOfView = 39f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;

        cam.transform.position = new Vector3(-1.65f, 0.95f, -1.80f);
        cam.transform.LookAt(new Vector3(-1.65f, 0.48f, 0.20f));

        int width = 1920;
        int height = 1080;
        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);

        cam.Render();

        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenShot.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;

        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(camGo);
        Object.DestroyImmediate(lightGo);
        Object.DestroyImmediate(fillGo);

        byte[] bytes = screenShot.EncodeToPNG();
        Object.DestroyImmediate(screenShot);

        File.WriteAllBytes(OutPath, bytes);
        Debug.Log("Saved current state to " + OutPath);
    }
}
