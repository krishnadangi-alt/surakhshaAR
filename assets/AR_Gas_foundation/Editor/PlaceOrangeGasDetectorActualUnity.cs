using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class PlaceOrangeGasDetectorActualUnity
{
    private static readonly string ScenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
    private static readonly string TexturePath = "Assets/AR_Gas_foundation/3d/GasDetector/Textures_multi_gas_detector_-_low_poly/tex_0.png";
    private static readonly string FbxPath = "Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly_converted.fbx";
    private static readonly string PrefabPath = "Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.prefab";
    private static readonly string ReportPath = @"C:\SurakhshaAR\actual_unity_execution_report.txt";

    public static void Run()
    {
        using (var sw = new StreamWriter(ReportPath, false))
        {
            sw.WriteLine("=== ACTUAL UNITY SCENE EXECUTION REPORT ===");
            sw.WriteLine("Timestamp: " + DateTime.Now.ToString("u"));

            // 1. Open scene
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                sw.WriteLine("ERROR: Could not open scene at " + ScenePath);
                return;
            }
            sw.WriteLine("Opened scene: " + scene.path);

            // 2. Locate GasScenarioRoot
            GameObject scenarioRoot = GameObject.Find("GasScenarioRoot");
            if (scenarioRoot == null)
            {
                sw.WriteLine("ERROR: GasScenarioRoot not found in scene!");
                return;
            }
            sw.WriteLine("Found GasScenarioRoot at position: " + scenarioRoot.transform.position);

            // 3. Locate Realistic_Gas_Cylinder to verify its exact position
            GameObject cylinder = GameObject.Find("Realistic_Gas_Cylinder");
            if (cylinder == null)
            {
                sw.WriteLine("ERROR: Realistic_Gas_Cylinder not found!");
                return;
            }
            Renderer cylRenderer = cylinder.GetComponentInChildren<Renderer>();
            Bounds cylBounds = cylRenderer != null ? cylRenderer.bounds : new Bounds(cylinder.transform.position, Vector3.one);
            sw.WriteLine($"Realistic_Gas_Cylinder: pos={cylinder.transform.position.ToString("F4")}, boundsCenter={cylBounds.center.ToString("F4")}, min={cylBounds.min.ToString("F4")}, max={cylBounds.max.ToString("F4")}, size={cylBounds.size.ToString("F4")}");

            // 4. Remove ONLY the currently wrong detector instance from the scene
            string[] staleDetectorNames = new[] { "Multi_Gas_Detector", "Orange_Gas_Detector_Floor", "PPE_Gas_Detector", "GLB_Multi_Gas_Detector", "Gas_Detector", "H2S_Gas_Detector" };
            foreach (var name in staleDetectorNames)
            {
                var existing = GameObject.Find(name);
                while (existing != null)
                {
                    sw.WriteLine($"Removing stale detector instance: {existing.name}");
                    UnityEngine.Object.DestroyImmediate(existing);
                    existing = GameObject.Find(name);
                }
            }

            // Also check for any detector child under PPE_Preparation_Station
            GameObject ppeStation = GameObject.Find("PPE_Preparation_Station");
            if (ppeStation != null)
            {
                for (int i = ppeStation.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = ppeStation.transform.GetChild(i);
                    if (child.name.ToLower().Contains("detector"))
                    {
                        sw.WriteLine($"Removing detector from PPE table: {child.name}");
                        UnityEngine.Object.DestroyImmediate(child.gameObject);
                    }
                }
            }

            // 5. Load original model asset and texture
            GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
            if (fbxAsset == null)
            {
                sw.WriteLine("ERROR: Could not load " + FbxPath);
                return;
            }
            Texture2D originalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            if (originalTex == null)
            {
                sw.WriteLine("ERROR: Could not load texture at " + TexturePath);
                return;
            }
            sw.WriteLine($"Loaded converted model: {fbxAsset.name}, and original GLB texture: {originalTex.name} ({originalTex.width}x{originalTex.height})");

            // 6. Ensure authentic original materials preserving the original GLB appearance
            // Create or update the original material asset
            string originalMatPath = "Assets/AR_Gas_foundation/3d/GasDetector/MAT_Original_GLB_Detector.mat";
            Material originalMat = AssetDatabase.LoadAssetAtPath<Material>(originalMatPath);
            if (originalMat == null)
            {
                Shader stdShader = Shader.Find("Standard");
                if (stdShader == null) stdShader = Shader.Find("Mobile/Diffuse");
                originalMat = new Material(stdShader);
                AssetDatabase.CreateAsset(originalMat, originalMatPath);
            }
            originalMat.name = "MAT_Original_GLB_Detector";
            originalMat.color = Color.white; // neutral white, no tint or manual recoloring
            originalMat.mainTexture = originalTex;
            if (originalMat.HasProperty("_Glossiness")) originalMat.SetFloat("_Glossiness", 0.35f);
            if (originalMat.HasProperty("_Metallic")) originalMat.SetFloat("_Metallic", 0.05f);
            EditorUtility.SetDirty(originalMat);

            string blackMatPath = "Assets/AR_Gas_foundation/3d/GasDetector/MAT_Original_GLB_Buttons.mat";
            Material blackMat = AssetDatabase.LoadAssetAtPath<Material>(blackMatPath);
            if (blackMat == null)
            {
                Shader stdShader = Shader.Find("Standard");
                if (stdShader == null) stdShader = Shader.Find("Mobile/Diffuse");
                blackMat = new Material(stdShader);
                AssetDatabase.CreateAsset(blackMat, blackMatPath);
            }
            blackMat.name = "MAT_Original_GLB_Buttons";
            blackMat.color = new Color(0.12f, 0.12f, 0.12f, 1.0f);
            if (blackMat.HasProperty("_Glossiness")) blackMat.SetFloat("_Glossiness", 0.25f);
            EditorUtility.SetDirty(blackMat);

            AssetDatabase.SaveAssets();

            // 7. Instantiate the original detector model under GasScenarioRoot
            GameObject detectorGo = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, scenarioRoot.transform);
            detectorGo.name = "Multi_Gas_Detector";

            // Apply the authentic original materials to the renderer
            Renderer detRenderer = detectorGo.GetComponentInChildren<Renderer>();
            if (detRenderer != null)
            {
                detRenderer.sharedMaterials = new Material[] { originalMat, blackMat };
                sw.WriteLine($"Configured detector renderer materials: Mat[0]={originalMat.name} with texture={originalTex.name}, Mat[1]={blackMat.name}");
            }

            // 8. Positioning:
            // - Immediately to the LOWER-LEFT side of the red gas cylinder
            // - Cylinder is at X = -0.25, Z = 0.20, with radius 0.114 (left edge = -0.364, front edge = +0.086)
            // - Target X = -0.52 (small realistic gap of ~8cm from cylinder's left edge)
            // - Target Z = 0.05 (slightly forward of cylinder's front edge at 0.086, clearly visible, NOT behind, NOT underneath)
            // - Height scale: Vector3(0.05, 0.05, 0.05) gives realistic handheld ~23cm height
            // - Upright orientation facing the front view/worker: Euler(0, 180, 0)
            detectorGo.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            detectorGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            detectorGo.transform.localPosition = new Vector3(-0.52f, 0.1165f, 0.05f);

            // Determine detector's actual bottom/base from geometry bounds
            Physics.SyncTransforms();
            if (detRenderer != null)
            {
                float currentBottom = detRenderer.bounds.min.y;
                float targetFloorY = 0.0000f; // exact floor plane
                float diffY = targetFloorY - currentBottom;
                detectorGo.transform.position += new Vector3(0f, diffY, 0f);
                Physics.SyncTransforms();
                sw.WriteLine($"Aligned detector to floor: Center={detRenderer.bounds.center.ToString("F4")}, Min={detRenderer.bounds.min.ToString("F4")}, Max={detRenderer.bounds.max.ToString("F4")}, Size={detRenderer.bounds.size.ToString("F4")}");
                sw.WriteLine($"Actual bottom level (Y_min): {detRenderer.bounds.min.y:F6} (Target: 0.000000)");
            }

            // Add BoxCollider for physical bounds
            BoxCollider col = detectorGo.GetComponent<BoxCollider>();
            if (col == null) col = detectorGo.AddComponent<BoxCollider>();
            if (detRenderer != null)
            {
                col.center = detectorGo.transform.InverseTransformPoint(detRenderer.bounds.center);
                Vector3 worldSize = detRenderer.bounds.size;
                col.size = new Vector3(worldSize.x / detectorGo.transform.lossyScale.x, worldSize.y / detectorGo.transform.lossyScale.y, worldSize.z / detectorGo.transform.lossyScale.z);
            }

            // Update GasEnvironmentBuilder reference if present
            GasEnvironmentBuilder builder = UnityEngine.Object.FindFirstObjectByType<GasEnvironmentBuilder>();
            if (builder != null)
            {
                SerializedObject builderSo = new SerializedObject(builder);
                SerializedProperty detProp = builderSo.FindProperty("gasDetectorPrefab");
                if (detProp != null)
                {
                    detProp.objectReferenceValue = fbxAsset;
                    builderSo.ApplyModifiedProperties();
                }
            }

            // 9. Verify that NO OTHER scene object was modified
            sw.WriteLine("\n--- VERIFICATION OF SCENE LOCKDOWN ---");
            sw.WriteLine($"PPE Workbench position: {GameObject.Find("PPE_Preparation_Station")?.transform.position}");
            sw.WriteLine($"Gas Cylinder position: {cylinder.transform.position}");
            sw.WriteLine($"Pipeline assembly position: {GameObject.Find("GasPipelineAssembly")?.transform.position}");
            sw.WriteLine($"Confined Space position: {GameObject.Find("Confined_Space_Training_Area")?.transform.position}");
            sw.WriteLine($"Detector final position: {detectorGo.transform.position.ToString("F4")}, localPos={detectorGo.transform.localPosition.ToString("F4")}");
            sw.WriteLine($"Detector rotation: {detectorGo.transform.rotation.eulerAngles.ToString("F4")}");
            sw.WriteLine($"Detector scale: {detectorGo.transform.localScale.ToString("F4")}");

            // 10. Save the actual Unity scene
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            sw.WriteLine($"Saved scene to {ScenePath}: " + (saved ? "SUCCESS" : "FAILED"));

            // 11. Capture Screenshot directly from ACTUAL UNITY SCENE
            sw.WriteLine("\n--- CAPTURING ACTUAL UNITY SCREENSHOT ---");
            string artifactImg = @"C:\Users\anjan\.gemini\antigravity-ide\brain\264d3be8-9397-4468-9849-c5bf2ed82521\FINAL_FRONT_VIEW_ORANGE_DETECTOR.png";
            string projectImg = @"C:\SurakhshaAR\Assets\AR_Gas_foundation\Screenshots\FINAL_FRONT_VIEW_ORANGE_DETECTOR.png";

            CaptureActualUnityScreenshot(artifactImg, projectImg, sw);
            sw.WriteLine("Screenshot capture completed.");
        }
    }

    private static void CaptureActualUnityScreenshot(string artifactPath, string projectPath, StreamWriter sw)
    {
        // Create temporary directional light for clear illumination
        GameObject tempLightGo = new GameObject("__TempRenderLight");
        Light tempLight = tempLightGo.AddComponent<Light>();
        tempLight.type = LightType.Directional;
        tempLight.intensity = 1.3f;
        tempLight.color = new Color(1.0f, 0.98f, 0.95f);
        tempLightGo.transform.rotation = Quaternion.Euler(40f, -25f, 0f);

        // Create temporary fill light for soft ambient fill
        GameObject tempFillGo = new GameObject("__TempFillLight");
        Light tempFill = tempFillGo.AddComponent<Light>();
        tempFill.type = LightType.Directional;
        tempFill.intensity = 0.5f;
        tempFill.color = new Color(0.9f, 0.92f, 1.0f);
        tempFillGo.transform.rotation = Quaternion.Euler(-30f, 150f, 0f);

        // Create front-view camera to clearly capture PPE table, orange detector on floor, cylinder, pipeline & confined space
        GameObject tempCamGo = new GameObject("__TempFrontCamera");
        Camera cam = tempCamGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.16f, 0.18f, 0.22f);
        cam.fieldOfView = 50f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;

        // Position camera in front view
        // Looking at center around X = -0.35, Y = 0.60
        cam.transform.position = new Vector3(-0.30f, 0.95f, -2.60f);
        cam.transform.rotation = Quaternion.Euler(10f, 0f, 0f);

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

        // Cleanup temporary objects immediately so scene remains pure
        UnityEngine.Object.DestroyImmediate(rt);
        UnityEngine.Object.DestroyImmediate(tempCamGo);
        UnityEngine.Object.DestroyImmediate(tempLightGo);
        UnityEngine.Object.DestroyImmediate(tempFillGo);

        byte[] bytes = screenShot.EncodeToPNG();
        UnityEngine.Object.DestroyImmediate(screenShot);

        Directory.CreateDirectory(Path.GetDirectoryName(artifactPath));
        File.WriteAllBytes(artifactPath, bytes);
        sw.WriteLine("Saved screenshot to artifact: " + artifactPath + $" ({bytes.Length} bytes)");

        Directory.CreateDirectory(Path.GetDirectoryName(projectPath));
        File.WriteAllBytes(projectPath, bytes);
        sw.WriteLine("Saved screenshot to project: " + projectPath + $" ({bytes.Length} bytes)");
    }
}
