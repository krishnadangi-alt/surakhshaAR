#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class FixGasDetectorOnly
{
    [MenuItem("SurakshaAR/Fix Gas Detector Only")]
    public static void Execute()
    {
        Debug.Log("=== FIX GAS DETECTOR ONLY START ===");
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        
        // Open main scene
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError("Failed to open scene: " + scenePath);
            return;
        }

        // STEP 1: REMOVE THE WRONG DETECTOR FROM THE PPE TABLE
        // Find any detector object parented to PPE_Preparation_Station or sitting on the PPE table
        GameObject ppeTable = GameObject.Find("PPE_Preparation_Station");
        if (ppeTable != null)
        {
            for (int i = ppeTable.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = ppeTable.transform.GetChild(i);
                if (child.name.ToLower().Contains("detector"))
                {
                    Debug.Log("Removing wrong detector from PPE table: " + child.name);
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        // Remove any leftover PPE_Gas_Detector globally
        GameObject ppeDetector = GameObject.Find("PPE_Gas_Detector");
        if (ppeDetector != null)
        {
            Debug.Log("Removing leftover PPE_Gas_Detector: " + ppeDetector.name);
            Object.DestroyImmediate(ppeDetector);
        }

        // Remove any existing Multi_Gas_Detector or floor detector to place fresh copy of exact original orange GLB asset
        GameObject existingDetector = GameObject.Find("Multi_Gas_Detector");
        if (existingDetector != null)
        {
            Debug.Log("Removing existing detector object: " + existingDetector.name);
            Object.DestroyImmediate(existingDetector);
        }
        
        GameObject existingOrangeDetector = GameObject.Find("Orange_Gas_Detector_Floor");
        if (existingOrangeDetector != null)
        {
            Debug.Log("Removing existing Orange_Gas_Detector_Floor: " + existingOrangeDetector.name);
            Object.DestroyImmediate(existingOrangeDetector);
        }

        // STEP 2: FIND THE ORIGINAL ORANGE GLB GAS DETECTOR
        string orangePrefabPath = "Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.prefab";
        GameObject orangePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(orangePrefabPath);
        
        if (orangePrefab == null)
        {
            orangePrefabPath = "Assets/AR_Gas_foundation/3d/GasDetector/multi_gas_detector_-_low_poly.glb";
            orangePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(orangePrefabPath);
        }

        if (orangePrefab == null)
        {
            Debug.LogError("CRITICAL ERROR: Original Orange GLB Gas Detector asset not found!");
            return;
        }

        Debug.Log("Loaded original orange gas detector asset from: " + orangePrefabPath);

        // STEP 3: PLACE THE ORANGE DETECTOR ON THE FLOOR TO THE LEFT OF THE RED GAS CYLINDER
        GameObject workstationRoot = GameObject.Find("Workstation_Root");
        Transform parentTransform = workstationRoot != null ? workstationRoot.transform : null;

        GameObject orangeDetector = (GameObject)PrefabUtility.InstantiatePrefab(orangePrefab, parentTransform);
        orangeDetector.name = "Multi_Gas_Detector";

        // Position: X = -0.55 (left of cylinder at -0.25, right of PPE table at -1.65), Z = 0.10 (slightly in front of cylinder)
        orangeDetector.transform.localPosition = new Vector3(-0.55f, 0.0f, 0.10f);
        
        // STEP 5: ORIENTATION - Upright, front display facing worker (front view)
        orangeDetector.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        // STEP 4: SIZE - Make slightly larger than default so readable in AR (target height ~0.20m)
        Renderer r = orangeDetector.GetComponentInChildren<Renderer>();
        if (r != null)
        {
            float currentHeight = r.bounds.size.y;
            if (currentHeight > 0)
            {
                float targetHeight = 0.20f; // 20cm portable handheld detector height
                float scaleFactor = targetHeight / currentHeight;
                orangeDetector.transform.localScale = orangeDetector.transform.localScale * scaleFactor;
            }
        }
        else
        {
            orangeDetector.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }

        // STEP 6: FLOOR CONTACT - Ensure bottom touches floor (Y = 0.0000) exactly
        r = orangeDetector.GetComponentInChildren<Renderer>();
        if (r != null)
        {
            float minWorldY = r.bounds.min.y;
            float targetFloorY = 0.0000f;
            float deltaY = targetFloorY - minWorldY;
            orangeDetector.transform.position += new Vector3(0f, deltaY, 0f);
            Debug.Log($"Floor alignment done. Min Y: {r.bounds.min.y:F4}, Center: {r.bounds.center:F4}");
        }

        // Update GasEnvironmentBuilder component reference if present
        GasEnvironmentBuilder builder = Object.FindFirstObjectByType<GasEnvironmentBuilder>();
        if (builder != null)
        {
            SerializedObject builderSo = new SerializedObject(builder);
            SerializedProperty detProp = builderSo.FindProperty("gasDetectorPrefab");
            if (detProp != null)
            {
                detProp.objectReferenceValue = orangePrefab;
                builderSo.ApplyModifiedProperties();
            }
        }

        // Save modified scene
        EditorSceneManager.SaveScene(scene);
        Debug.Log("SCENE SAVED SUCCESSFULLY TO: " + scenePath);

        // FINAL VERIFICATION SCREENSHOT - Front View
        string artifactDir = @"C:\Users\anjan\.gemini\antigravity\brain\e89740c3-7119-440e-9701-7245a2fa60a9";
        string screenshotPath = Path.Combine(artifactDir, "FINAL_FRONT_VIEW_ORANGE_DETECTOR.png");

        Camera cam = Camera.main;
        if (cam == null) cam = Object.FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            Vector3 origPos = cam.transform.position;
            Quaternion origRot = cam.transform.rotation;
            float origFov = cam.fieldOfView;

            // Front view looking at workstation
            cam.transform.position = new Vector3(-0.40f, 0.65f, -1.80f);
            cam.transform.rotation = Quaternion.Euler(5f, 0f, 0f);
            cam.fieldOfView = 50f;

            RenderTexture rt = new RenderTexture(1920, 1080, 24);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            screenShot.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            File.WriteAllBytes(screenshotPath, screenShot.EncodeToPNG());
            Debug.Log("Verification screenshot saved to: " + screenshotPath);

            cam.transform.position = origPos;
            cam.transform.rotation = origRot;
            cam.fieldOfView = origFov;
        }

        Debug.Log("=== FIX GAS DETECTOR ONLY COMPLETE ===");
    }
}
#endif
