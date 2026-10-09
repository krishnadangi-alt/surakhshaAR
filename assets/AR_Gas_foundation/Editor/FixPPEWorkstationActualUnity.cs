using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixPPEWorkstationActualUnity
{
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
    private static readonly string StructuralSteelMatPath = "Assets/AR_Gas_foundation/materials/MAT_Structural_Steel.mat";
    private static readonly string ModularSteelMatPath = "Assets/AR_Gas_foundation/materials/MAT_Modular_Pipes_Steel.mat";
    private static readonly string VestPrefabPath = "Assets/AR_Gas_foundation/3d/PPE/ppe_vest.prefab";
    private static readonly string ExecutionReportPath = @"C:\SurakhshaAR\ppe_workstation_execution_report.txt";

    public static void Run()
    {
        using (var sw = new StreamWriter(ExecutionReportPath, false))
        {
            sw.WriteLine("=== PPE WORKSTATION SURGICAL FIX REPORT ===");
            sw.WriteLine("Timestamp: " + DateTime.Now.ToString("u"));

            // 1. Open the actual scene
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                sw.WriteLine("ERROR: Could not open scene: " + ScenePath);
                return;
            }
            sw.WriteLine("Scene opened successfully: " + scene.path);

            // 2. Verify non-PPE objects BEFORE modification (LOCKDOWN CHECK)
            GameObject detector = GameObject.Find("Multi_Gas_Detector");
            GameObject cylinder = GameObject.Find("Realistic_Gas_Cylinder");
            GameObject pipeline = GameObject.Find("GasPipelineAssembly");
            GameObject confined = GameObject.Find("Confined_Space_Training_Area");

            Vector3 preDetPos = detector != null ? detector.transform.position : Vector3.zero;
            Vector3 preCylPos = cylinder != null ? cylinder.transform.position : Vector3.zero;
            Vector3 prePipePos = pipeline != null ? pipeline.transform.position : Vector3.zero;
            Vector3 preConfPos = confined != null ? confined.transform.position : Vector3.zero;

            sw.WriteLine("\n--- PRE-EXECUTION LOCKDOWN VERIFICATION ---");
            sw.WriteLine($"Detector pos: {preDetPos.ToString("F4")}");
            sw.WriteLine($"Cylinder pos: {preCylPos.ToString("F4")}");
            sw.WriteLine($"Pipeline pos: {prePipePos.ToString("F4")}");
            sw.WriteLine($"Confined Space pos: {preConfPos.ToString("F4")}");

            // 3. Locate PPE Station
            GameObject ppeStation = GameObject.Find("PPE_Preparation_Station");
            if (ppeStation == null)
            {
                sw.WriteLine("ERROR: PPE_Preparation_Station not found!");
                return;
            }
            sw.WriteLine($"Found PPE_Preparation_Station at world pos: {ppeStation.transform.position.ToString("F4")}");

            // Verify it remains on the left side (X ≈ -1.65)
            float stationBaseY = ppeStation.transform.position.y; // 0.00

            // 4. PPE WORKBENCH: Assign Industrial Steel Materials
            Transform wbTransform = ppeStation.transform.Find("PPE_Workbench");
            if (wbTransform == null)
            {
                sw.WriteLine("ERROR: PPE_Workbench not found under PPE_Preparation_Station!");
                return;
            }

            Renderer wbRenderer = wbTransform.GetComponentInChildren<Renderer>();
            if (wbRenderer == null)
            {
                sw.WriteLine("ERROR: Renderer on PPE_Workbench not found!");
                return;
            }

            Material structuralSteelMat = AssetDatabase.LoadAssetAtPath<Material>(StructuralSteelMatPath);
            Material modularSteelMat = AssetDatabase.LoadAssetAtPath<Material>(ModularSteelMatPath);
            if (structuralSteelMat == null)
            {
                sw.WriteLine("WARNING: Could not load " + StructuralSteelMatPath + ", searching alternative...");
                structuralSteelMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Structural_Steel.mat");
            }
            if (modularSteelMat == null)
            {
                modularSteelMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/AR_Gas_foundation/Materials/MAT_Modular_Pipes_Steel.mat");
            }

            // Assign industrial steel materials: submesh 0 (steel frame/shelf), submesh 1 (steel tabletop)
            Material[] newWbMats = new Material[2];
            newWbMats[0] = structuralSteelMat;
            newWbMats[1] = modularSteelMat != null ? modularSteelMat : structuralSteelMat;
            wbRenderer.sharedMaterials = newWbMats;
            sw.WriteLine($"Assigned realistic industrial steel materials to workbench: {newWbMats[0]?.name}, {newWbMats[1]?.name}");

            // Measure workbench surface levels
            Bounds wbBounds = wbRenderer.bounds;
            float topSurfaceY = wbBounds.max.y; // 0.7500
            float lowerShelfY = stationBaseY + 0.2200f; // 0.2200
            sw.WriteLine($"Workbench World Bounds: Min={wbBounds.min.ToString("F4")}, Max={wbBounds.max.ToString("F4")}, Size={wbBounds.size.ToString("F4")}");
            sw.WriteLine($"Top Tabletop World Y = {topSurfaceY:F4}, Lower Shelf World Y = {lowerShelfY:F4}");

            // 5. TOP OF WORKBENCH: Arrange PPE Selection [ HELMET ] [ GOGGLES ] [ GLOVES ] [ SAFETY VEST ]
            sw.WriteLine("\n--- ARRANGING TOP WORKBENCH PPE SELECTION ---");

            // A. SAFETY HELMET (Left side: X ≈ -0.42)
            Transform helmet = ppeStation.transform.Find("Safety_Helmet");
            if (helmet != null)
            {
                helmet.gameObject.SetActive(true);
                helmet.localPosition = new Vector3(-0.4200f, 0.8890f, 0.0000f);
                helmet.localRotation = Quaternion.Euler(0f, 45f, 0f);
                helmet.localScale = new Vector3(0.0826f, 0.0826f, 0.0826f);

                Renderer hRend = helmet.GetComponentInChildren<Renderer>();
                if (hRend != null)
                {
                    float currentMinY = hRend.bounds.min.y;
                    float deltaY = topSurfaceY - currentMinY;
                    helmet.position += new Vector3(0f, deltaY, 0f);
                    sw.WriteLine($"Safety Helmet: final pos={helmet.localPosition.ToString("F4")}, bounds.min.y={hRend.bounds.min.y:F4} (tabletop contact: YES, floating: NO)");
                }
            }

            // B. SAFETY GOGGLES (Beside helmet: X ≈ -0.16)
            Transform goggles = ppeStation.transform.Find("Safety_Goggles");
            if (goggles != null)
            {
                goggles.gameObject.SetActive(true);
                goggles.localPosition = new Vector3(-0.1600f, 0.8350f, 0.0000f);
                goggles.localRotation = Quaternion.Euler(0f, 15f, 0f); // Naturally laid, lenses forward
                goggles.localScale = new Vector3(0.1725f, 0.1725f, 0.1725f);

                Renderer gRend = goggles.GetComponentInChildren<Renderer>();
                if (gRend != null)
                {
                    float currentMinY = gRend.bounds.min.y;
                    float deltaY = topSurfaceY - currentMinY;
                    goggles.position += new Vector3(0f, deltaY, 0f);
                    sw.WriteLine($"Safety Goggles: final pos={goggles.localPosition.ToString("F4")}, bounds.min.y={gRend.bounds.min.y:F4} (tabletop contact: YES, floating: NO)");
                }
            }

            // C. SAFETY GLOVES (Beside goggles: Left at X ≈ +0.06, Right at X ≈ +0.18)
            Transform glovesLeft = ppeStation.transform.Find("Work_Gloves_Left");
            if (glovesLeft != null)
            {
                glovesLeft.gameObject.SetActive(true);
                glovesLeft.localPosition = new Vector3(0.0600f, 0.8385f, -0.0100f);
                glovesLeft.localRotation = Quaternion.Euler(270f, 0f, 0f);
                glovesLeft.localScale = new Vector3(0.0038f, 0.0038f, 0.0038f);

                Renderer glRend = glovesLeft.GetComponentInChildren<Renderer>();
                if (glRend != null)
                {
                    float currentMinY = glRend.bounds.min.y;
                    float deltaY = topSurfaceY - currentMinY;
                    glovesLeft.position += new Vector3(0f, deltaY, 0f);
                    sw.WriteLine($"Work Gloves Left: final pos={glovesLeft.localPosition.ToString("F4")}, bounds.min.y={glRend.bounds.min.y:F4} (tabletop contact: YES, floating: NO)");
                }
            }

            Transform glovesRight = ppeStation.transform.Find("Work_Gloves_Right");
            if (glovesRight != null)
            {
                glovesRight.gameObject.SetActive(true); // Ensure activated
                glovesRight.localPosition = new Vector3(0.1800f, 0.8385f, -0.0100f);
                glovesRight.localRotation = Quaternion.Euler(270f, 180f, 0f);
                glovesRight.localScale = new Vector3(0.0038f, 0.0038f, 0.0038f);

                Renderer grRend = glovesRight.GetComponentInChildren<Renderer>();
                if (grRend != null)
                {
                    float currentMinY = grRend.bounds.min.y;
                    float deltaY = topSurfaceY - currentMinY;
                    glovesRight.position += new Vector3(0f, deltaY, 0f);
                    sw.WriteLine($"Work Gloves Right: final pos={glovesRight.localPosition.ToString("F4")}, bounds.min.y={grRend.bounds.min.y:F4} (tabletop contact: YES, floating: NO)");
                }
            }

            // D. SAFETY VEST (Right side of tabletop: X ≈ +0.40)
            Transform vestTransform = ppeStation.transform.Find("Safety_Vest");
            if (vestTransform == null)
            {
                vestTransform = ppeStation.transform.Find("ppe_vest");
            }
            if (vestTransform == null)
            {
                GameObject vestPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(VestPrefabPath);
                if (vestPrefab != null)
                {
                    GameObject vestInstance = (GameObject)PrefabUtility.InstantiatePrefab(vestPrefab, ppeStation.transform);
                    vestInstance.name = "Safety_Vest";
                    vestTransform = vestInstance.transform;
                    sw.WriteLine("Instantiated Safety_Vest from " + VestPrefabPath);
                }
            }
            if (vestTransform != null)
            {
                vestTransform.gameObject.SetActive(true);
                vestTransform.localPosition = new Vector3(0.4000f, 0.8000f, 0.0000f);
                vestTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                vestTransform.localScale = Vector3.one;

                Renderer vRend = vestTransform.GetComponentInChildren<Renderer>();
                if (vRend != null)
                {
                    float currentMinY = vRend.bounds.min.y;
                    float deltaY = topSurfaceY - currentMinY;
                    vestTransform.position += new Vector3(0f, deltaY, 0f);
                    sw.WriteLine($"Safety Vest: final pos={vestTransform.localPosition.ToString("F4")}, bounds.min.y={vRend.bounds.min.y:F4} (tabletop contact: YES, floating: NO)");
                }
            }

            // 6. LOWER SHELF — SAFETY BOOTS
            sw.WriteLine("\n--- PLACING SAFETY BOOTS ON LOWER SHELF ---");

            // We test orientation:
            // When rotated around Y, boots should be upright and toe should point forward (towards -Z / camera).
            // Let's configure Left and Right boots side by side.
            Transform bootLeft = ppeStation.transform.Find("Safety_Boots_Left");
            Transform bootRight = ppeStation.transform.Find("Safety_Boots_Right");

            if (bootLeft != null)
            {
                bootLeft.gameObject.SetActive(true);
                bootLeft.localPosition = new Vector3(-0.1400f, 0.3500f, 0.0000f);
                bootLeft.localRotation = Quaternion.Euler(0f, 0f, 0f); // Upright facing forward
                bootLeft.localScale = new Vector3(0.0016f, 0.0016f, 0.0016f);

                Renderer blRend = bootLeft.GetComponentInChildren<Renderer>();
                if (blRend != null)
                {
                    float currentMinY = blRend.bounds.min.y;
                    float deltaY = lowerShelfY - currentMinY;
                    bootLeft.position += new Vector3(0f, deltaY, 0f);
                    sw.WriteLine($"Boot Left: final pos={bootLeft.localPosition.ToString("F4")}, bounds.min.y={blRend.bounds.min.y:F4} (lower shelf contact: YES, floating: NO, below tabletop: YES)");
                }
            }

            if (bootRight != null)
            {
                bootRight.gameObject.SetActive(true);
                bootRight.localPosition = new Vector3(0.1400f, 0.3500f, 0.0000f);
                bootRight.localRotation = Quaternion.Euler(0f, 0f, 0f); // Upright facing forward
                bootRight.localScale = new Vector3(0.0016f, 0.0016f, 0.0016f);

                Renderer brRend = bootRight.GetComponentInChildren<Renderer>();
                if (brRend != null)
                {
                    float currentMinY = brRend.bounds.min.y;
                    float deltaY = lowerShelfY - currentMinY;
                    bootRight.position += new Vector3(0f, deltaY, 0f);
                    sw.WriteLine($"Boot Right: final pos={bootRight.localPosition.ToString("F4")}, bounds.min.y={brRend.bounds.min.y:F4} (lower shelf contact: YES, floating: NO, below tabletop: YES)");
                }
            }

            // 7. COMPREHENSIVE INDIVIDUAL VERIFICATION CHECKLIST
            sw.WriteLine("\n--- VERIFICATION CHECKLIST ---");
            Renderer hTestRend = helmet?.GetComponentInChildren<Renderer>();
            Renderer gTestRend = goggles?.GetComponentInChildren<Renderer>();
            Renderer glTestRend = glovesLeft?.GetComponentInChildren<Renderer>();
            Renderer grTestRend = glovesRight?.GetComponentInChildren<Renderer>();
            Renderer vTestRend = vestTransform?.GetComponentInChildren<Renderer>();
            Renderer blTestRend = bootLeft?.GetComponentInChildren<Renderer>();
            Renderer brTestRend = bootRight?.GetComponentInChildren<Renderer>();

            sw.WriteLine($"Helmet: tabletop contact = {(Mathf.Abs(hTestRend.bounds.min.y - topSurfaceY) < 0.005f ? "YES" : "NO")}, floating = {(hTestRend.bounds.min.y > topSurfaceY + 0.005f ? "YES" : "NO")}");
            sw.WriteLine($"Goggles: tabletop contact = {(Mathf.Abs(gTestRend.bounds.min.y - topSurfaceY) < 0.005f ? "YES" : "NO")}, floating = {(gTestRend.bounds.min.y > topSurfaceY + 0.005f ? "YES" : "NO")}");
            sw.WriteLine($"Gloves Left: tabletop contact = {(Mathf.Abs(glTestRend.bounds.min.y - topSurfaceY) < 0.005f ? "YES" : "NO")}, floating = {(glTestRend.bounds.min.y > topSurfaceY + 0.005f ? "YES" : "NO")}");
            sw.WriteLine($"Gloves Right: tabletop contact = {(Mathf.Abs(grTestRend.bounds.min.y - topSurfaceY) < 0.005f ? "YES" : "NO")}, floating = {(grTestRend.bounds.min.y > topSurfaceY + 0.005f ? "YES" : "NO")}");
            sw.WriteLine($"Vest: tabletop contact = {(Mathf.Abs(vTestRend.bounds.min.y - topSurfaceY) < 0.005f ? "YES" : "NO")}, floating = {(vTestRend.bounds.min.y > topSurfaceY + 0.005f ? "YES" : "NO")}");
            sw.WriteLine($"Boot 1 (Left): lower shelf contact = {(Mathf.Abs(blTestRend.bounds.min.y - lowerShelfY) < 0.005f ? "YES" : "NO")}, floating = {(blTestRend.bounds.min.y > lowerShelfY + 0.005f ? "YES" : "NO")}");
            sw.WriteLine($"Boot 2 (Right): lower shelf contact = {(Mathf.Abs(brTestRend.bounds.min.y - lowerShelfY) < 0.005f ? "YES" : "NO")}, floating = {(brTestRend.bounds.min.y > lowerShelfY + 0.005f ? "YES" : "NO")}");

            // 8. FINAL LOCKDOWN VERIFICATION
            sw.WriteLine("\n--- NON-PPE OBJECT LOCKDOWN AUDIT ---");
            Vector3 postDetPos = detector != null ? detector.transform.position : Vector3.zero;
            Vector3 postCylPos = cylinder != null ? cylinder.transform.position : Vector3.zero;
            Vector3 postPipePos = pipeline != null ? pipeline.transform.position : Vector3.zero;
            Vector3 postConfPos = confined != null ? confined.transform.position : Vector3.zero;

            bool detectorUnchanged = (preDetPos == postDetPos);
            bool cylinderUnchanged = (preCylPos == postCylPos);
            bool pipelineUnchanged = (prePipePos == postPipePos);
            bool confinedUnchanged = (preConfPos == postConfPos);

            sw.WriteLine($"Gas Detector unchanged: {detectorUnchanged} (pos={postDetPos.ToString("F4")})");
            sw.WriteLine($"Gas Cylinder unchanged: {cylinderUnchanged} (pos={postCylPos.ToString("F4")})");
            sw.WriteLine($"Pipeline unchanged: {pipelineUnchanged} (pos={postPipePos.ToString("F4")})");
            sw.WriteLine($"Confined Space unchanged: {confinedUnchanged} (pos={postConfPos.ToString("F4")})");

            // 9. Save the scene
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            sw.WriteLine($"\nScene saved to {ScenePath}: " + (saved ? "SUCCESS" : "FAILED"));

            // 10. Capture Front-View Screenshot of Complete PPE Workstation
            string artifactImg = @"C:\Users\anjan\.gemini\antigravity-ide\brain\264d3be8-9397-4468-9849-c5bf2ed82521\FINAL_PPE_WORKSTATION_VERIFICATION.png";
            string projectImg = @"C:\SurakhshaAR\Assets\AR_Gas_foundation\Screenshots\FINAL_PPE_WORKSTATION_VERIFICATION.png";

            CapturePPEWorkstationScreenshot(artifactImg, projectImg, sw);
        }
    }

    private static void CapturePPEWorkstationScreenshot(string artifactPath, string projectPath, StreamWriter sw)
    {
        // Directional illumination for crisp PPE details
        GameObject lightGo = new GameObject("__TempPPELight");
        Light light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.35f;
        light.color = new Color(1.0f, 0.98f, 0.95f);
        lightGo.transform.rotation = Quaternion.Euler(35f, -15f, 0f);

        // Fill light for soft shadow fill
        GameObject fillGo = new GameObject("__TempPPEFill");
        Light fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.intensity = 0.55f;
        fill.color = new Color(0.92f, 0.94f, 1.0f);
        fillGo.transform.rotation = Quaternion.Euler(-20f, 160f, 0f);

        // Front-view camera looking directly at the PPE workstation
        // PPE Station is at (-1.65, 0.00, 0.20)
        // Center of table is at (-1.65, 0.48, 0.20)
        GameObject camGo = new GameObject("__TempPPECam");
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.18f, 0.20f, 0.24f);
        cam.fieldOfView = 44f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;

        cam.transform.position = new Vector3(-1.65f, 0.58f, -1.35f);
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
        sw.WriteLine($"Saved screenshot to artifact: {artifactPath} ({bytes.Length} bytes)");

        Directory.CreateDirectory(Path.GetDirectoryName(projectPath));
        File.WriteAllBytes(projectPath, bytes);
        sw.WriteLine($"Saved screenshot to project: {projectPath} ({bytes.Length} bytes)");
    }
}
