using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// ONE CHANGE ONLY: move PPE_Preparation_Station -0.50 on X axis.
/// Nothing else is touched.
public class PPEShiftLeft
{
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
    private static readonly string LogPath   = @"C:\SurakhshaAR\ppe_shift_left_log.txt";
    private static readonly float  ShiftX    = -0.50f;   // leftward shift (Unity left = negative X)

    public static void Run()
    {
        using (var sw = new StreamWriter(LogPath, false))
        {
            sw.WriteLine("=== PPE SHIFT LEFT ===");
            sw.WriteLine("Time: " + DateTime.Now.ToString("u"));

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid()) { sw.WriteLine("ERROR: scene not found"); return; }

            // Record protected objects BEFORE
            GameObject detector = GameObject.Find("Multi_Gas_Detector");
            GameObject cylinder = GameObject.Find("Realistic_Gas_Cylinder");
            GameObject pipeline = GameObject.Find("GasPipelineAssembly");
            GameObject confined = GameObject.Find("Confined_Space_Training_Area");
            Vector3 preDet  = detector  != null ? detector.transform.position  : Vector3.zero;
            Vector3 preCyl  = cylinder  != null ? cylinder.transform.position  : Vector3.zero;
            Vector3 prePipe = pipeline  != null ? pipeline.transform.position  : Vector3.zero;
            Vector3 preConf = confined  != null ? confined.transform.position  : Vector3.zero;
            sw.WriteLine($"PRE det:{preDet:F4} cyl:{preCyl:F4} pipe:{prePipe:F4} conf:{preConf:F4}");

            // Find PPE workstation root
            GameObject station = GameObject.Find("PPE_Preparation_Station");
            if (station == null) { sw.WriteLine("ERROR: PPE_Preparation_Station not found"); return; }

            Vector3 before = station.transform.position;
            sw.WriteLine($"PPE_Preparation_Station BEFORE: {before:F4}");

            // === ONE CHANGE ONLY: shift X by ShiftX ===
            station.transform.position = new Vector3(before.x + ShiftX, before.y, before.z);

            Vector3 after = station.transform.position;
            sw.WriteLine($"PPE_Preparation_Station AFTER:  {after:F4}");
            sw.WriteLine($"Delta: X={ShiftX:F4} Y=0.0000 Z=0.0000");

            // Verify protected objects AFTER (must be identical)
            Vector3 postDet  = detector  != null ? detector.transform.position  : Vector3.zero;
            Vector3 postCyl  = cylinder  != null ? cylinder.transform.position  : Vector3.zero;
            Vector3 postPipe = pipeline  != null ? pipeline.transform.position  : Vector3.zero;
            Vector3 postConf = confined  != null ? confined.transform.position  : Vector3.zero;
            sw.WriteLine("POST verification:");
            sw.WriteLine($"  Det  unchanged={preDet  == postDet}  pos={postDet:F4}");
            sw.WriteLine($"  Cyl  unchanged={preCyl  == postCyl}  pos={postCyl:F4}");
            sw.WriteLine($"  Pipe unchanged={prePipe == postPipe} pos={postPipe:F4}");
            sw.WriteLine($"  Conf unchanged={preConf == postConf} pos={postConf:F4}");

            // Count children of station — should NOT have changed
            sw.WriteLine($"  Station child count: {station.transform.childCount}");

            // Save
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            sw.WriteLine($"Scene saved: {(saved ? "SUCCESS" : "FAILED")}");

            // Quick screenshot
            TakeShot(before, after, sw);
            sw.WriteLine("=== DONE ===");
        }
    }

    private static void TakeShot(Vector3 before, Vector3 after, StreamWriter sw)
    {
        string outPath = @"C:\Users\anjan\.gemini\antigravity-ide\brain\264d3be8-9397-4468-9849-c5bf2ed82521\PPE_SHIFTED_LEFT.png";

        // Temporary camera looking at the full workstation area from front-left
        GameObject camGo = new GameObject("__ShiftCam");
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.11f, 0.12f, 0.14f);
        cam.fieldOfView = 46f;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 100f;

        // Position camera to capture the full layout: PPE station + gas side
        // PPE station now at ~X=-2.15, gas cylinder at X=-0.25 ? center X ˜ -1.2
        cam.transform.position = new Vector3(-1.20f, 1.40f, -2.40f);
        cam.transform.LookAt(new Vector3(-1.20f, 0.50f, 0.20f));

        // Temp key light
        GameObject lightGo = new GameObject("__ShiftLight");
        Light l = lightGo.AddComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = 1.25f;
        l.color = new Color(1f, 0.97f, 0.92f);
        l.shadows = LightShadows.Soft;
        lightGo.transform.rotation = Quaternion.Euler(38f, -28f, 0f);

        int W = 1920, H = 1080;
        RenderTexture rt = new RenderTexture(W, H, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        var tex = new Texture2D(W, H, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
        tex.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;

        UnityEngine.Object.DestroyImmediate(rt);
        UnityEngine.Object.DestroyImmediate(camGo);
        UnityEngine.Object.DestroyImmediate(lightGo);

        byte[] png = tex.EncodeToPNG();
        UnityEngine.Object.DestroyImmediate(tex);

        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outPath));
        System.IO.File.WriteAllBytes(outPath, png);
        sw.WriteLine($"Screenshot: {png.Length} bytes ? {outPath}");
    }
}
