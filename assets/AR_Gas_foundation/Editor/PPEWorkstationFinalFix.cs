using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class PPEWorkstationFinalFix
{
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
    private static readonly string LogPath   = @"C:\SurakhshaAR\ppe_final_fix_log.txt";
    private static readonly string ShotPath  = @"C:\Users\anjan\.gemini\antigravity-ide\brain\264d3be8-9397-4468-9849-c5bf2ed82521\PPE_FINAL_FIX.png";
    private static readonly string ShotBak   = @"C:\SurakhshaAR\Assets\AR_Gas_foundation\Screenshots\PPE_FINAL_FIX.png";

    public static void Run()
    {
        using (var sw = new StreamWriter(LogPath, false))
        {
            sw.WriteLine("=== PPE WORKSTATION FINAL FIX ===");
            sw.WriteLine("Time: " + DateTime.Now.ToString("u"));

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid()) { sw.WriteLine("ERROR: Scene not found"); return; }

            GameObject detector = GameObject.Find("Multi_Gas_Detector");
            GameObject cylinder = GameObject.Find("Realistic_Gas_Cylinder");
            GameObject pipeline = GameObject.Find("GasPipelineAssembly");
            GameObject confined = GameObject.Find("Confined_Space_Training_Area");

            Vector3 preDet  = detector  != null ? detector.transform.position  : Vector3.zero;
            Vector3 preCyl  = cylinder  != null ? cylinder.transform.position  : Vector3.zero;
            Vector3 prePipe = pipeline  != null ? pipeline.transform.position  : Vector3.zero;
            Vector3 preConf = confined  != null ? confined.transform.position  : Vector3.zero;

            sw.WriteLine($"PRE det:{preDet:F3} cyl:{preCyl:F3} pipe:{prePipe:F3} conf:{preConf:F3}");

            GameObject station = GameObject.Find("PPE_Preparation_Station");
            if (station == null) { sw.WriteLine("ERROR: PPE_Preparation_Station not found"); return; }
            sw.WriteLine($"Station world pos: {station.transform.position:F3}");

            // -- Measure workbench actual surfaces ------------------------------
            Transform wb = station.transform.Find("PPE_Workbench");
            float tabletopY   = 0.750f;
            float lowerShelfY = 0.220f;

            if (wb != null)
            {
                Renderer[] wbRends = wb.GetComponentsInChildren<Renderer>();
                float maxY = float.MinValue;
                foreach (var r in wbRends) if (r.bounds.max.y > maxY) maxY = r.bounds.max.y;
                tabletopY = maxY;

                float midY = float.MinValue;
                foreach (var r in wbRends)
                {
                    float top = r.bounds.max.y;
                    if (top < tabletopY - 0.20f && top > midY) midY = top;
                }
                if (midY > float.MinValue) lowerShelfY = midY;

                sw.WriteLine($"Workbench tabletopY={tabletopY:F4} lowerShelfY={lowerShelfY:F4}");

                // Fix material tint so iron/steel textures show properly
                Renderer wbRend = wb.GetComponentInChildren<Renderer>();
                if (wbRend != null && wbRend.sharedMaterials.Length >= 1)
                {
                    var frameMat = wbRend.sharedMaterials[0];
                    if (frameMat != null)
                    {
                        frameMat.color = new Color(0.88f, 0.88f, 0.88f, 1f);
                        frameMat.SetFloat("_Metallic", 0.30f);
                        frameMat.SetFloat("_Glossiness", 0.36f);
                        EditorUtility.SetDirty(frameMat);
                    }
                    if (wbRend.sharedMaterials.Length >= 2)
                    {
                        var topMat = wbRend.sharedMaterials[1];
                        if (topMat != null)
                        {
                            topMat.color = new Color(0.92f, 0.93f, 0.95f, 1f);
                            topMat.SetFloat("_Metallic", 0.45f);
                            topMat.SetFloat("_Glossiness", 0.42f);
                            EditorUtility.SetDirty(topMat);
                        }
                    }
                }
            }

            // -- Helper: land bottommost bounds point on targetY -----------------
            Action<Transform, float, string> LandOn = (t, targetY, label) =>
            {
                if (t == null) { sw.WriteLine($"  {label}: NOT FOUND"); return; }
                t.gameObject.SetActive(true);
                Renderer[] rends = t.GetComponentsInChildren<Renderer>(true);
                if (rends.Length == 0) { sw.WriteLine($"  {label}: no renderer"); return; }
                float minY = float.MaxValue;
                foreach (var r in rends) if (r.bounds.min.y < minY) minY = r.bounds.min.y;
                float dy = targetY - minY;
                t.position += new Vector3(0f, dy, 0f);
                float newMin = float.MaxValue;
                foreach (var r in rends) if (r.bounds.min.y < newMin) newMin = r.bounds.min.y;
                sw.WriteLine($"  {label}: shifted +{dy:F4} -> min.y={newMin:F4} (target={targetY:F4})");
            };

            // -- 1. HELMET ------------------------------------------------------
            {
                Transform helmet = station.transform.Find("Safety_Helmet");
                if (helmet != null)
                {
                    helmet.localPosition = new Vector3(-0.420f, 0.88f, 0.020f);
                    helmet.localRotation = Quaternion.Euler(-90f, 15f, 0f);
                    helmet.localScale    = new Vector3(0.0826f, 0.0826f, 0.0826f);
                    LandOn(helmet, tabletopY, "Helmet");
                }
            }

            // -- 2. GOGGLES -----------------------------------------------------
            {
                Transform goggles = station.transform.Find("Safety_Goggles");
                if (goggles != null)
                {
                    goggles.localPosition = new Vector3(-0.160f, 0.80f, 0.020f);
                    goggles.localRotation = Quaternion.Euler(-90f, 90f, 0f);
                    goggles.localScale    = new Vector3(0.1725f, 0.1725f, 0.1725f);
                    LandOn(goggles, tabletopY, "Goggles");
                }
            }

            // -- 3. GLOVES ------------------------------------------------------
            {
                for (int i = station.transform.childCount - 1; i >= 0; i--)
                {
                    Transform c = station.transform.GetChild(i);
                    if (c.name.Contains("Glove") && c.name != "Work_Gloves_Left" && c.name != "Work_Gloves_Right")
                    { sw.WriteLine($"  Remove dupe: {c.name}"); UnityEngine.Object.DestroyImmediate(c.gameObject); }
                }
                Transform gL = station.transform.Find("Work_Gloves_Left");
                Transform gR = station.transform.Find("Work_Gloves_Right");
                if (gL != null)
                {
                    gL.localPosition = new Vector3(0.060f, 0.80f, 0.020f);
                    gL.localRotation = Quaternion.Euler(0f, 82f, 0f);
                    gL.localScale    = new Vector3(0.0038f, 0.0038f, 0.0038f);
                    LandOn(gL, tabletopY, "GloveLeft");
                }
                if (gR != null)
                {
                    gR.localPosition = new Vector3(0.185f, 0.80f, 0.035f);
                    gR.localRotation = Quaternion.Euler(0f, 98f, 5f);
                    gR.localScale    = new Vector3(0.0038f, 0.0038f, 0.0038f);
                    LandOn(gR, tabletopY, "GloveRight");
                }
            }

            // -- 4. VEST --------------------------------------------------------
            {
                Transform vest = station.transform.Find("Safety_Vest");
                if (vest != null)
                {
                    vest.localPosition = new Vector3(0.400f, 0.760f, 0.020f);
                    vest.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    vest.localScale    = Vector3.one;
                    LandOn(vest, tabletopY, "Vest");
                }
            }

            // -- 5. BOOTS on LOWER SHELF ----------------------------------------
            {
                for (int i = station.transform.childCount - 1; i >= 0; i--)
                {
                    Transform c = station.transform.GetChild(i);
                    if (c.name.Contains("Boot") && c.name != "Safety_Boots_Left" && c.name != "Safety_Boots_Right")
                    { sw.WriteLine($"  Remove dupe boot: {c.name}"); UnityEngine.Object.DestroyImmediate(c.gameObject); }
                }
                Transform bL = station.transform.Find("Safety_Boots_Left");
                Transform bR = station.transform.Find("Safety_Boots_Right");
                if (bL != null)
                {
                    bL.gameObject.SetActive(true);
                    bL.localPosition = new Vector3(-0.130f, 0.350f, 0.020f);
                    bL.localRotation = Quaternion.Euler(-90f, 180f, 0f);
                    bL.localScale    = new Vector3(0.0016f, 0.0016f, 0.0016f);
                    LandOn(bL, lowerShelfY, "BootLeft");
                }
                if (bR != null)
                {
                    bR.gameObject.SetActive(true);
                    bR.localPosition = new Vector3(0.130f, 0.350f, 0.020f);
                    bR.localRotation = Quaternion.Euler(-90f, 178f, 0f);
                    bR.localScale    = new Vector3(0.0016f, 0.0016f, 0.0016f);
                    LandOn(bR, lowerShelfY, "BootRight");
                }
            }

            // -- POST-CHECK -----------------------------------------------------
            Vector3 postDet  = detector  != null ? detector.transform.position  : Vector3.zero;
            Vector3 postCyl  = cylinder  != null ? cylinder.transform.position  : Vector3.zero;
            Vector3 postPipe = pipeline  != null ? pipeline.transform.position  : Vector3.zero;
            Vector3 postConf = confined  != null ? confined.transform.position  : Vector3.zero;
            sw.WriteLine("POST-CHECK:");
            sw.WriteLine($"  Det  unchanged={preDet  == postDet}");
            sw.WriteLine($"  Cyl  unchanged={preCyl  == postCyl}");
            sw.WriteLine($"  Pipe unchanged={prePipe == postPipe}");
            sw.WriteLine($"  Conf unchanged={preConf == postConf}");

            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            sw.WriteLine($"Scene saved: {(saved ? "SUCCESS" : "FAILED")}");

            // -- Screenshot -----------------------------------------------------
            CaptureShot(sw);
            sw.WriteLine("=== DONE ===");
        }
    }

    private static void CaptureShot(StreamWriter sw)
    {
        GameObject keyGo = new GameObject("__FIX_Key");
        Light key = keyGo.AddComponent<Light>();
        key.type = LightType.Directional;
        key.intensity = 1.28f;
        key.color = new Color(1f, 0.97f, 0.92f);
        key.shadows = LightShadows.Soft;
        key.shadowStrength = 0.65f;
        keyGo.transform.rotation = Quaternion.Euler(38f, -30f, 0f);

        GameObject fillGo = new GameObject("__FIX_Fill");
        Light fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.intensity = 0.40f;
        fill.color = new Color(0.88f, 0.92f, 1f);
        fillGo.transform.rotation = Quaternion.Euler(-12f, 148f, 0f);

        GameObject camGo = new GameObject("__FIX_Cam");
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.11f, 0.12f, 0.14f);
        cam.fieldOfView = 38f;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 50f;
        cam.transform.position = new Vector3(-1.65f, 1.05f, -1.50f);
        cam.transform.LookAt(new Vector3(-1.65f, 0.46f, 0.15f));

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
        UnityEngine.Object.DestroyImmediate(keyGo);
        UnityEngine.Object.DestroyImmediate(fillGo);

        byte[] png = tex.EncodeToPNG();
        UnityEngine.Object.DestroyImmediate(tex);

        Directory.CreateDirectory(Path.GetDirectoryName(ShotPath));
        File.WriteAllBytes(ShotPath, png);
        Directory.CreateDirectory(Path.GetDirectoryName(ShotBak));
        File.WriteAllBytes(ShotBak, png);
        sw.WriteLine($"Shot: {png.Length} bytes");
    }
}
