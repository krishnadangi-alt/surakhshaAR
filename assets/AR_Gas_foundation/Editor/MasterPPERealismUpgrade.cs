using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class MasterPPERealismUpgrade
{
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
    private static readonly string IronTexPath = "Assets/industrial-valve/textures/iron_texture1050.jpg";
    private static readonly string SteelTexPath = "Assets/industrial-valve/textures/Steel_03_UV_H_CM_1.jpg";
    private static readonly string VestTexPath = "Assets/AR_Gas_foundation/textures/tex_safety_vest_real.png";
    private static readonly string FabricNormPath = "Assets/industrial-valve/textures/FabricWrinkled006_NRM_3K.jpg";
    private static readonly string HelmetTexPath = "Assets/AR_Gas_foundation/3d/PPE/Textures_safety_helmet/tex_0.png";
    private static readonly string GogglesTexPath = "Assets/AR_Gas_foundation/3d/PPE/Textures_safety_goggles/tex_0.png";
    private static readonly string GogglesNormPath = "Assets/AR_Gas_foundation/3d/PPE/Textures_safety_goggles/tex_2.png";
    private static readonly string GlovesTexPath = "Assets/AR_Gas_foundation/3d/PPE/Textures_work_gloves/tex_1.png";
    private static readonly string GlovesNormPath = "Assets/AR_Gas_foundation/3d/PPE/Textures_work_gloves/tex_2.png";
    private static readonly string ReportPath = @"C:\SurakhshaAR\ppe_realism_upgrade_report.txt";

    public static void Run()
    {
        // Reimport assets to ensure fresh state
        AssetDatabase.ImportAsset("Assets/AR_Gas_foundation/3d/PPE/workbench_hd.obj", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset("Assets/AR_Gas_foundation/3d/PPE/ppe_vest_hd.obj", ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(VestTexPath, ImportAssetOptions.ForceUpdate);

        using (var sw = new StreamWriter(ReportPath, false))
        {
            sw.WriteLine("=== MASTER PPE WORKSTATION REALISM UPGRADE ===");
            sw.WriteLine("Timestamp: " + DateTime.Now.ToString("u"));

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                sw.WriteLine("ERROR: Could not open scene at " + ScenePath);
                return;
            }

            // 1. Strict Pre-Check Lockdown
            GameObject detector = GameObject.Find("Multi_Gas_Detector");
            GameObject cylinder = GameObject.Find("Realistic_Gas_Cylinder");
            GameObject pipeline = GameObject.Find("GasPipelineAssembly");
            GameObject confined = GameObject.Find("Confined_Space_Training_Area");

            Vector3 preDet = detector != null ? detector.transform.position : Vector3.zero;
            Vector3 preCyl = cylinder != null ? cylinder.transform.position : Vector3.zero;
            Vector3 prePipe = pipeline != null ? pipeline.transform.position : Vector3.zero;
            Vector3 preConf = confined != null ? confined.transform.position : Vector3.zero;

            sw.WriteLine($"Pre-check Lockdown: Detector={preDet:F4}, Cyl={preCyl:F4}, Pipe={prePipe:F4}, Conf={preConf:F4}");

            // 2. Locate PPE Station
            GameObject ppeStation = GameObject.Find("PPE_Preparation_Station");
            if (ppeStation == null)
            {
                sw.WriteLine("ERROR: PPE_Preparation_Station not found!");
                return;
            }

            Shader standardShader = Shader.Find("Standard");

            // 3. WORKBENCH: Gunmetal Frame + Industrial Steel Plates
            Texture2D ironTex = AssetDatabase.LoadAssetAtPath<Texture2D>(IronTexPath);
            Texture2D steelTex = AssetDatabase.LoadAssetAtPath<Texture2D>(SteelTexPath);

            string frameMatPath = "Assets/AR_Gas_foundation/materials/MAT_Workbench_Frame.mat";
            Material frameMat = AssetDatabase.LoadAssetAtPath<Material>(frameMatPath);
            if (frameMat == null)
            {
                frameMat = new Material(standardShader);
                frameMat.name = "MAT_Workbench_Frame";
                AssetDatabase.CreateAsset(frameMat, frameMatPath);
            }
            if (ironTex != null) frameMat.mainTexture = ironTex;
            frameMat.color = new Color(0.85f, 0.85f, 0.85f, 1.0f); // Preserve authentic dark iron texture
            frameMat.SetFloat("_Metallic", 0.32f); // Structural steel with realistic oxide coat
            frameMat.SetFloat("_Glossiness", 0.36f); // Matte industrial powdercoat
            EditorUtility.SetDirty(frameMat);

            string topMatPath = "Assets/AR_Gas_foundation/materials/MAT_Workbench_Top.mat";
            Material topMat = AssetDatabase.LoadAssetAtPath<Material>(topMatPath);
            if (topMat == null)
            {
                topMat = new Material(standardShader);
                topMat.name = "MAT_Workbench_Top";
                AssetDatabase.CreateAsset(topMat, topMatPath);
            }
            if (steelTex != null) topMat.mainTexture = steelTex;
            topMat.color = new Color(0.90f, 0.92f, 0.94f, 1.0f); // Crisp industrial steel plate
            topMat.SetFloat("_Metallic", 0.55f); // Realistic metallic plate sheen
            topMat.SetFloat("_Glossiness", 0.42f); // Semi-matte industrial plate with scratches
            EditorUtility.SetDirty(topMat);

            Transform wb = ppeStation.transform.Find("PPE_Workbench");
            Renderer wbRend = wb.GetComponentInChildren<Renderer>();
            wbRend.sharedMaterials = new Material[] { frameMat, topMat };

            // Robust Mesh Loading for Workbench
            Mesh wbMesh = null;
            foreach (var sub in AssetDatabase.LoadAllAssetsAtPath("Assets/AR_Gas_foundation/3d/PPE/workbench_hd.obj"))
            {
                if (sub is Mesh m)
                {
                    wbMesh = m;
                    break;
                }
            }
            MeshFilter wbMf = wb.GetComponentInChildren<MeshFilter>();
            if (wbMesh != null && wbMf != null)
            {
                wbMf.sharedMesh = wbMesh;
                sw.WriteLine($"Workbench mesh assigned: {wbMesh.name}, submeshes={wbMesh.subMeshCount}, verts={wbMesh.vertexCount}");
            }

            float topSurfaceY = 0.7500f;
            float lowerShelfY = 0.2200f;
            sw.WriteLine($"Workbench: Surfaces: Top={topSurfaceY:F4}, Lower={lowerShelfY:F4}");

            // 4. HELMET: Standard PBR Material with authentic specular highlights
            Transform helmet = ppeStation.transform.Find("Safety_Helmet");
            if (helmet != null)
            {
                helmet.gameObject.SetActive(true);
                helmet.localPosition = new Vector3(-0.4200f, 0.8800f, 0.0000f);
                helmet.localRotation = Quaternion.Euler(-90f, 20f, 0f);
                helmet.localScale = new Vector3(0.0826f, 0.0826f, 0.0826f);

                string helmetMatPath = "Assets/AR_Gas_foundation/materials/MAT_PPE_Safety_Helmet.mat";
                Material helmetMat = AssetDatabase.LoadAssetAtPath<Material>(helmetMatPath);
                if (helmetMat == null)
                {
                    helmetMat = new Material(standardShader);
                    helmetMat.name = "MAT_PPE_Safety_Helmet";
                    AssetDatabase.CreateAsset(helmetMat, helmetMatPath);
                }
                Texture2D helmetTex = AssetDatabase.LoadAssetAtPath<Texture2D>(HelmetTexPath);
                if (helmetTex != null) helmetMat.mainTexture = helmetTex;
                helmetMat.color = Color.white;
                helmetMat.SetFloat("_Metallic", 0.05f);
                helmetMat.SetFloat("_Glossiness", 0.50f); // Smooth molded safety hardhat
                EditorUtility.SetDirty(helmetMat);

                Renderer hr = helmet.GetComponentInChildren<Renderer>();
                if (hr != null)
                {
                    hr.sharedMaterial = helmetMat;
                    float dy = topSurfaceY - hr.bounds.min.y;
                    helmet.position += new Vector3(0f, dy, 0f);
                    sw.WriteLine($"Helmet: Standard material applied, bounds.min.y={hr.bounds.min.y:F4}");
                }
            }

            // 5. SAFETY GOGGLES: Standard Material with lenses facing front
            Transform goggles = ppeStation.transform.Find("Safety_Goggles");
            if (goggles != null)
            {
                goggles.gameObject.SetActive(true);
                goggles.localPosition = new Vector3(-0.1600f, 0.8000f, 0.0000f);
                goggles.localRotation = Quaternion.Euler(-90f, 90f, 0f); // Lenses facing front
                goggles.localScale = new Vector3(0.1725f, 0.1725f, 0.1725f);

                string gogglesMatPath = "Assets/AR_Gas_foundation/materials/MAT_PPE_Safety_Goggles.mat";
                Material gogglesMat = AssetDatabase.LoadAssetAtPath<Material>(gogglesMatPath);
                if (gogglesMat == null)
                {
                    gogglesMat = new Material(standardShader);
                    gogglesMat.name = "MAT_PPE_Safety_Goggles";
                    AssetDatabase.CreateAsset(gogglesMat, gogglesMatPath);
                }
                Texture2D gogglesTex = AssetDatabase.LoadAssetAtPath<Texture2D>(GogglesTexPath);
                Texture2D gogglesNorm = AssetDatabase.LoadAssetAtPath<Texture2D>(GogglesNormPath);
                if (gogglesTex != null) gogglesMat.mainTexture = gogglesTex;
                if (gogglesNorm != null)
                {
                    gogglesMat.EnableKeyword("_NORMALMAP");
                    gogglesMat.SetTexture("_BumpMap", gogglesNorm);
                }
                gogglesMat.color = Color.white;
                gogglesMat.SetFloat("_Metallic", 0.05f);
                gogglesMat.SetFloat("_Glossiness", 0.65f); // Clear polycarbonate lenses
                EditorUtility.SetDirty(gogglesMat);

                Renderer gr = goggles.GetComponentInChildren<Renderer>();
                if (gr != null)
                {
                    gr.sharedMaterial = gogglesMat;
                    float dy = topSurfaceY - gr.bounds.min.y;
                    goggles.position += new Vector3(0f, dy, 0f);
                    sw.WriteLine($"Goggles: Standard material applied, bounds.min.y={gr.bounds.min.y:F4}");
                }
            }

            // 6. WORK GLOVES: Exactly TWO original models, Orange Leather Material
            Transform glovesLeft = ppeStation.transform.Find("Work_Gloves_Left");
            Transform glovesRight = ppeStation.transform.Find("Work_Gloves_Right");

            // Clean up any extraneous duplicate gloves
            for (int i = ppeStation.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = ppeStation.transform.GetChild(i);
                if (child.name.Contains("Glove") && child != glovesLeft && child != glovesRight)
                {
                    sw.WriteLine($"Removing duplicate glove object: {child.name}");
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }

            string glovesMatPath = "Assets/AR_Gas_foundation/materials/MAT_PPE_Work_Gloves.mat";
            Material glovesMat = AssetDatabase.LoadAssetAtPath<Material>(glovesMatPath);
            if (glovesMat == null)
            {
                glovesMat = new Material(standardShader);
                glovesMat.name = "MAT_PPE_Work_Gloves";
                AssetDatabase.CreateAsset(glovesMat, glovesMatPath);
            }
            Texture2D glovesTex = AssetDatabase.LoadAssetAtPath<Texture2D>(GlovesTexPath);
            Texture2D glovesNorm = AssetDatabase.LoadAssetAtPath<Texture2D>(GlovesNormPath);
            if (glovesTex != null) glovesMat.mainTexture = glovesTex;
            if (glovesNorm != null)
            {
                glovesMat.EnableKeyword("_NORMALMAP");
                glovesMat.SetTexture("_BumpMap", glovesNorm);
            }
            glovesMat.color = Color.white;
            glovesMat.SetFloat("_Metallic", 0.05f);
            glovesMat.SetFloat("_Glossiness", 0.28f); // Matte split-leather work gloves
            EditorUtility.SetDirty(glovesMat);

            if (glovesLeft != null)
            {
                glovesLeft.gameObject.SetActive(true);
                glovesLeft.localPosition = new Vector3(0.0650f, 0.8000f, 0.0100f);
                glovesLeft.localRotation = Quaternion.Euler(0f, 85f, 0f);
                glovesLeft.localScale = new Vector3(0.0038f, 0.0038f, 0.0038f);

                Renderer r = glovesLeft.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    r.sharedMaterial = glovesMat;
                    float dy = topSurfaceY - r.bounds.min.y;
                    glovesLeft.position += new Vector3(0f, dy, 0f);
                    sw.WriteLine($"Gloves Left: applied leather material, bounds.min.y={r.bounds.min.y:F4}");
                }
            }

            if (glovesRight != null)
            {
                glovesRight.gameObject.SetActive(true);
                glovesRight.localPosition = new Vector3(0.1800f, 0.8000f, 0.0100f);
                glovesRight.localRotation = Quaternion.Euler(0f, 95f, 0f);
                glovesRight.localScale = new Vector3(0.0038f, 0.0038f, 0.0038f);

                Renderer r = glovesRight.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    r.sharedMaterial = glovesMat;
                    float dy = topSurfaceY - r.bounds.min.y;
                    glovesRight.position += new Vector3(0f, dy, 0f);
                    sw.WriteLine($"Gloves Right: applied leather material, bounds.min.y={r.bounds.min.y:F4}");
                }
            }

            // 7. SAFETY VEST: Realistic Folded Silhouette & Fabric Material
            Transform vest = ppeStation.transform.Find("Safety_Vest");
            if (vest != null)
            {
                vest.gameObject.SetActive(true);
                vest.localPosition = new Vector3(0.4000f, 0.7500f, 0.0000f);
                vest.localRotation = Quaternion.Euler(0f, 0f, 0f); // Naturally laid out right-side up
                vest.localScale = Vector3.one;

                // Robust Mesh Loading for Vest
                Mesh vestMesh = null;
                foreach (var sub in AssetDatabase.LoadAllAssetsAtPath("Assets/AR_Gas_foundation/3d/PPE/ppe_vest_hd.obj"))
                {
                    if (sub is Mesh m)
                    {
                        vestMesh = m;
                        break;
                    }
                }
                MeshFilter vestMf = vest.GetComponentInChildren<MeshFilter>();
                if (vestMesh != null && vestMf != null)
                {
                    vestMf.sharedMesh = vestMesh;
                    sw.WriteLine($"Vest mesh assigned: {vestMesh.name}, verts={vestMesh.vertexCount}");
                }

                string vestMatPath = "Assets/AR_Gas_foundation/materials/MAT_PPE_Vest_Fabric.mat";
                Material vestMat = AssetDatabase.LoadAssetAtPath<Material>(vestMatPath);
                if (vestMat == null)
                {
                    vestMat = new Material(standardShader);
                    vestMat.name = "MAT_PPE_Vest_Fabric";
                    AssetDatabase.CreateAsset(vestMat, vestMatPath);
                }
                Texture2D vestTex = AssetDatabase.LoadAssetAtPath<Texture2D>(VestTexPath);
                Texture2D fabricNorm = AssetDatabase.LoadAssetAtPath<Texture2D>(FabricNormPath);
                if (vestTex != null) vestMat.mainTexture = vestTex;
                if (fabricNorm != null)
                {
                    vestMat.EnableKeyword("_NORMALMAP");
                    vestMat.SetTexture("_BumpMap", fabricNorm);
                    vestMat.SetFloat("_BumpScale", 0.6f);
                }
                vestMat.color = Color.white;
                vestMat.SetFloat("_Metallic", 0.02f);
                vestMat.SetFloat("_Glossiness", 0.12f); // Matte garment cloth
                EditorUtility.SetDirty(vestMat);

                Renderer vr = vest.GetComponentInChildren<Renderer>();
                if (vr != null)
                {
                    vr.sharedMaterial = vestMat;
                    float dy = topSurfaceY - vr.bounds.min.y;
                    vest.position += new Vector3(0f, dy, 0f);
                    sw.WriteLine($"Safety Vest: applied photographic fabric material, bounds.min.y={vr.bounds.min.y:F4}");
                }
            }

            // 8. SAFETY BOOTS: Exactly TWO boots on Lower Shelf
            Transform bootLeft = ppeStation.transform.Find("Safety_Boots_Left");
            Transform bootRight = ppeStation.transform.Find("Safety_Boots_Right");

            // Clean up any duplicate boots
            for (int i = ppeStation.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = ppeStation.transform.GetChild(i);
                if (child.name.Contains("Boot") && child != bootLeft && child != bootRight)
                {
                    sw.WriteLine($"Removing duplicate boot object: {child.name}");
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }

            if (bootLeft != null)
            {
                bootLeft.gameObject.SetActive(true);
                bootLeft.localPosition = new Vector3(-0.1300f, 0.3500f, 0.0200f);
                bootLeft.localRotation = Quaternion.Euler(-90f, 180f, 0f); // Upright, toes facing front
                bootLeft.localScale = new Vector3(0.0016f, 0.0016f, 0.0016f);

                Renderer r = bootLeft.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    float dy = lowerShelfY - r.bounds.min.y;
                    bootLeft.position += new Vector3(0f, dy, 0f);
                    sw.WriteLine($"Boot Left: bounds.min.y={r.bounds.min.y:F4} (resting on lower shelf)");
                }
            }

            if (bootRight != null)
            {
                bootRight.gameObject.SetActive(true);
                bootRight.localPosition = new Vector3(0.1300f, 0.3500f, 0.0200f);
                bootRight.localRotation = Quaternion.Euler(-90f, 180f, 0f); // Upright, toes facing front
                bootRight.localScale = new Vector3(0.0016f, 0.0016f, 0.0016f);

                Renderer r = bootRight.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    float dy = lowerShelfY - r.bounds.min.y;
                    bootRight.position += new Vector3(0f, dy, 0f);
                    sw.WriteLine($"Boot Right: bounds.min.y={r.bounds.min.y:F4} (resting on lower shelf)");
                }
            }

            // 9. Post-Check Strict Lockdown Audit
            Vector3 postDet = detector != null ? detector.transform.position : Vector3.zero;
            Vector3 postCyl = cylinder != null ? cylinder.transform.position : Vector3.zero;
            Vector3 postPipe = pipeline != null ? pipeline.transform.position : Vector3.zero;
            Vector3 postConf = confined != null ? confined.transform.position : Vector3.zero;

            sw.WriteLine("\n--- STRICT NON-PPE LOCKDOWN AUDIT ---");
            sw.WriteLine($"Gas Detector unchanged: {preDet == postDet} (pos={postDet:F4})");
            sw.WriteLine($"Gas Cylinder unchanged: {preCyl == postCyl} (pos={postCyl:F4})");
            sw.WriteLine($"Pipeline unchanged: {prePipe == postPipe} (pos={postPipe:F4})");
            sw.WriteLine($"Confined Space unchanged: {preConf == postConf} (pos={postConf:F4})");

            // 10. Save Scene
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            sw.WriteLine($"Scene saved to {ScenePath}: " + (saved ? "SUCCESS" : "FAILED"));

            // 11. Render Clean Front Screenshot
            string artifactImg = @"C:\Users\anjan\.gemini\antigravity-ide\brain\264d3be8-9397-4468-9849-c5bf2ed82521\FINAL_PPE_WORKSTATION_VERIFICATION.png";
            string projectImg = @"C:\SurakhshaAR\Assets\AR_Gas_foundation\Screenshots\FINAL_PPE_WORKSTATION_VERIFICATION.png";
            CaptureCleanFrontScreenshot(artifactImg, projectImg, sw);
        }
    }

    private static void CaptureCleanFrontScreenshot(string artifactPath, string projectPath, StreamWriter sw)
    {
        GameObject lightGo = new GameObject("__TempPPELight");
        Light light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.25f;
        light.color = new Color(1.0f, 0.97f, 0.92f);
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.70f;
        light.shadowBias = 0.02f;
        light.shadowNormalBias = 0.1f;
        lightGo.transform.rotation = Quaternion.Euler(36f, -28f, 0f);

        GameObject fillGo = new GameObject("__TempPPEFill");
        Light fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.intensity = 0.45f;
        fill.color = new Color(0.92f, 0.95f, 1.0f);
        fillGo.transform.rotation = Quaternion.Euler(-15f, 150f, 0f);

        GameObject camGo = new GameObject("__TempPPECam");
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.13f, 0.15f);
        cam.fieldOfView = 33f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;

        cam.transform.position = new Vector3(-1.65f, 1.18f, -1.45f);
        cam.transform.LookAt(new Vector3(-1.65f, 0.52f, 0.05f));

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
        sw.WriteLine($"Screenshot successfully captured: {bytes.Length} bytes");
    }
}
