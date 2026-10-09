using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FineTunePPERealism
{
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
    private static readonly string SteelMatPath = "Assets/AR_Gas_foundation/materials/MAT_Pipeline_Steel.mat";
    private static readonly string ExecutionReportPath = @"C:\SurakhshaAR\ppe_finetune_report.txt";

    public static void Run()
    {
        using (var sw = new StreamWriter(ExecutionReportPath, false))
        {
            sw.WriteLine("=== PPE FINAL PERFECTION SURGICAL EXECUTION ===");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                sw.WriteLine("ERROR: Could not open scene: " + ScenePath);
                return;
            }

            GameObject ppeStation = GameObject.Find("PPE_Preparation_Station");
            if (ppeStation == null)
            {
                sw.WriteLine("ERROR: PPE_Preparation_Station not found!");
                return;
            }

            // 1. WORKBENCH: Assign clean realistic industrial steel material MAT_Pipeline_Steel
            Transform wb = ppeStation.transform.Find("PPE_Workbench");
            Renderer wbRend = wb.GetComponentInChildren<Renderer>();
            Material steelMat = AssetDatabase.LoadAssetAtPath<Material>(SteelMatPath);
            if (steelMat != null)
            {
                wbRend.sharedMaterials = new Material[] { steelMat, steelMat };
                sw.WriteLine($"Assigned MAT_Pipeline_Steel to workbench: {steelMat.name}");
            }

            float topSurfaceY = wbRend.bounds.max.y; // 0.7500
            float lowerShelfY = ppeStation.transform.position.y + 0.2200f; // 0.2200
            sw.WriteLine($"Top Surface Y = {topSurfaceY:F4}, Lower Shelf Y = {lowerShelfY:F4}");

            // 2. SAFETY BOOTS: Rotate 180 around Y so front toes face the camera, soles flat on shelf
            Transform bootLeft = ppeStation.transform.Find("Safety_Boots_Left");
            Transform bootRight = ppeStation.transform.Find("Safety_Boots_Right");

            if (bootLeft != null)
            {
                bootLeft.gameObject.SetActive(true);
                bootLeft.localPosition = new Vector3(-0.1300f, 0.3500f, 0.0200f);
                bootLeft.localRotation = Quaternion.Euler(-90f, 180f, 0f); // Upright, toes forward
                bootLeft.localScale = new Vector3(0.0016f, 0.0016f, 0.0016f);

                Renderer r = bootLeft.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    float dy = lowerShelfY - r.bounds.min.y;
                    bootLeft.position += new Vector3(0f, dy, 0f);
                    sw.WriteLine($"Boot Left: final pos={bootLeft.localPosition.ToString("F4")}, bounds.min.y={r.bounds.min.y:F4}");
                }
            }

            if (bootRight != null)
            {
                bootRight.gameObject.SetActive(true);
                bootRight.localPosition = new Vector3(0.1300f, 0.3500f, 0.0200f);
                bootRight.localRotation = Quaternion.Euler(-90f, 180f, 0f); // Upright, toes forward
                bootRight.localScale = new Vector3(0.0016f, 0.0016f, 0.0016f);

                Renderer r = bootRight.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    float dy = lowerShelfY - r.bounds.min.y;
                    bootRight.position += new Vector3(0f, dy, 0f);
                    sw.WriteLine($"Boot Right: final pos={bootRight.localPosition.ToString("F4")}, bounds.min.y={r.bounds.min.y:F4}");
                }
            }

            // 3. SAFETY HELMET: Upright dome, visor forward
            Transform helmet = ppeStation.transform.Find("Safety_Helmet");
            if (helmet != null)
            {
                helmet.gameObject.SetActive(true);
                helmet.localPosition = new Vector3(-0.4200f, 0.8800f, 0.0000f);
                helmet.localRotation = Quaternion.Euler(-90f, 20f, 0f);
                helmet.localScale = new Vector3(0.0826f, 0.0826f, 0.0826f);

                Renderer r = helmet.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    float dy = topSurfaceY - r.bounds.min.y;
                    helmet.position += new Vector3(0f, dy, 0f);
                    sw.WriteLine($"Helmet: final pos={helmet.localPosition.ToString("F4")}, bounds.min.y={r.bounds.min.y:F4}");
                }
            }

            // 4. SAFETY GOGGLES: Front lenses facing camera
            Transform goggles = ppeStation.transform.Find("Safety_Goggles");
            if (goggles != null)
            {
                goggles.gameObject.SetActive(true);
                goggles.localPosition = new Vector3(-0.1600f, 0.8000f, 0.0000f);
                goggles.localRotation = Quaternion.Euler(-90f, 90f, 0f); // Lenses forward
                goggles.localScale = new Vector3(0.1725f, 0.1725f, 0.1725f);

                Renderer r = goggles.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    float dy = topSurfaceY - r.bounds.min.y;
                    goggles.position += new Vector3(0f, dy, 0f);
                    sw.WriteLine($"Goggles: final pos={goggles.localPosition.ToString("F4")}, bounds.min.y={r.bounds.min.y:F4}");
                }
            }

            // 5. WORK GLOVES: Both lying naturally flat on table
            Transform glovesLeft = ppeStation.transform.Find("Work_Gloves_Left");
            if (glovesLeft != null)
            {
                glovesLeft.gameObject.SetActive(true);
                glovesLeft.localPosition = new Vector3(0.0500f, 0.8000f, 0.0000f);
                glovesLeft.localRotation = Quaternion.Euler(0f, 90f, 0f); // Flat, fingers forward
                glovesLeft.localScale = new Vector3(0.0038f, 0.0038f, 0.0038f);

                Renderer r = glovesLeft.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    float dy = topSurfaceY - r.bounds.min.y;
                    glovesLeft.position += new Vector3(0f, dy, 0f);
                    sw.WriteLine($"Gloves Left: final pos={glovesLeft.localPosition.ToString("F4")}, bounds.min.y={r.bounds.min.y:F4}");
                }
            }

            Transform glovesRight = ppeStation.transform.Find("Work_Gloves_Right");
            if (glovesRight != null)
            {
                glovesRight.gameObject.SetActive(true);
                glovesRight.localPosition = new Vector3(0.1700f, 0.8000f, 0.0000f);
                glovesRight.localRotation = Quaternion.Euler(0f, 90f, 0f); // Flat, fingers forward
                glovesRight.localScale = new Vector3(0.0038f, 0.0038f, 0.0038f);

                Renderer r = glovesRight.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    float dy = topSurfaceY - r.bounds.min.y;
                    glovesRight.position += new Vector3(0f, dy, 0f);
                    sw.WriteLine($"Gloves Right: final pos={glovesRight.localPosition.ToString("F4")}, bounds.min.y={r.bounds.min.y:F4}");
                }
            }

            // 6. SAFETY VEST: Lying flat on tabletop at right side
            Transform vest = ppeStation.transform.Find("Safety_Vest");
            if (vest != null)
            {
                vest.gameObject.SetActive(true);
                vest.localPosition = new Vector3(0.4000f, 0.8000f, 0.0000f);
                vest.localRotation = Quaternion.Euler(0f, 0f, 0f);
                vest.localScale = Vector3.one;

                Renderer r = vest.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    float dy = topSurfaceY - r.bounds.min.y;
                    vest.position += new Vector3(0f, dy, 0f);
                    sw.WriteLine($"Safety Vest: final pos={vest.localPosition.ToString("F4")}, bounds.min.y={r.bounds.min.y:F4}");
                }
            }

            // 7. Verify non-PPE objects are strictly untouched
            GameObject detector = GameObject.Find("Multi_Gas_Detector");
            GameObject cylinder = GameObject.Find("Realistic_Gas_Cylinder");
            GameObject pipeline = GameObject.Find("GasPipelineAssembly");
            GameObject confined = GameObject.Find("Confined_Space_Training_Area");

            sw.WriteLine($"Lockdown Audit: Detector pos = {detector?.transform.position.ToString("F4")}");
            sw.WriteLine($"Lockdown Audit: Cylinder pos = {cylinder?.transform.position.ToString("F4")}");
            sw.WriteLine($"Lockdown Audit: Pipeline pos = {pipeline?.transform.position.ToString("F4")}");
            sw.WriteLine($"Lockdown Audit: Confined pos = {confined?.transform.position.ToString("F4")}");

            // 8. Save the scene
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            sw.WriteLine($"Saved scene: {saved}");

            // 9. Capture Front-View Screenshot
            string artifactImg = @"C:\Users\anjan\.gemini\antigravity-ide\brain\264d3be8-9397-4468-9849-c5bf2ed82521\FINAL_PPE_WORKSTATION_VERIFICATION.png";
            string projectImg = @"C:\SurakhshaAR\Assets\AR_Gas_foundation\Screenshots\FINAL_PPE_WORKSTATION_VERIFICATION.png";

            CaptureScreenshot(artifactImg, projectImg, sw);
        }
    }

    private static void CaptureScreenshot(string artifactPath, string projectPath, StreamWriter sw)
    {
        GameObject lightGo = new GameObject("__TempPPELight");
        Light light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.35f;
        light.color = new Color(1.0f, 0.98f, 0.95f);
        lightGo.transform.rotation = Quaternion.Euler(35f, -20f, 0f);

        GameObject fillGo = new GameObject("__TempPPEFill");
        Light fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.intensity = 0.55f;
        fill.color = new Color(0.92f, 0.94f, 1.0f);
        fillGo.transform.rotation = Quaternion.Euler(-25f, 160f, 0f);

        GameObject camGo = new GameObject("__TempPPECam");
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.18f, 0.20f, 0.24f);
        cam.fieldOfView = 42f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;

        // Position camera to see complete PPE workstation in clean front view
        cam.transform.position = new Vector3(-1.65f, 0.98f, -1.75f);
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

        UnityEngine.Object.DestroyImmediate(rt);
        UnityEngine.Object.DestroyImmediate(camGo);
        UnityEngine.Object.DestroyImmediate(lightGo);
        UnityEngine.Object.DestroyImmediate(fillGo);

        byte[] bytes = screenShot.EncodeToPNG();
        UnityEngine.Object.DestroyImmediate(screenShot);

        Directory.CreateDirectory(Path.GetDirectoryName(artifactPath));
        File.WriteAllBytes(artifactPath, bytes);

        Directory.CreateDirectory(Path.GetDirectoryName(projectPath));
        File.WriteAllBytes(projectPath, bytes);
        sw.WriteLine($"Screenshot captured successfully: {bytes.Length} bytes");
    }
}
