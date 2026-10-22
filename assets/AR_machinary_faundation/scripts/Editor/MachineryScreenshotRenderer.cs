#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryScreenshotRenderer
    {
        public const string SourceScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        public const string ARScenePath = "Assets/AR_machinary_faundation/scenes/MachineryARExperience.unity";
        public const string OutputDir = "Builds/Verification";

        [MenuItem("SurakshaAR/Machinery/Render Verification Screenshots 1 & 2", priority = 50)]
        public static void RenderScreenshots()
        {
            Directory.CreateDirectory(OutputDir);

            // --- SCREENSHOT 1: Unity Editor Complete Original Machinery ---
            Debug.Log("[ScreenshotRenderer] Rendering Screenshot 1: Complete Original Machinery...");
            Scene sourceScene = EditorSceneManager.OpenScene(SourceScenePath, OpenSceneMode.Single);

            GameObject scenario = null;
            foreach (var r in sourceScene.GetRootGameObjects())
            {
                if (r.name == "MachineryScenario")
                {
                    scenario = r;
                    break;
                }
            }
            if (scenario != null)
            {
                scenario.SetActive(true);
                foreach (var r in scenario.GetComponentsInChildren<Renderer>(true))
                {
                    r.enabled = true;
                    if (!r.gameObject.activeSelf) r.gameObject.SetActive(true);
                }

                GameObject camGo = new GameObject("RenderCamera_Screenshot1");
                Camera cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1.0f); // Dark studio neutral
                cam.nearClipPlane = 0.05f;
                cam.farClipPlane = 100f;
                cam.fieldOfView = 50f;

                // Frame overall machinery cleanly
                camGo.transform.position = new Vector3(1.0f, 2.8f, -6.5f);
                camGo.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

                // Add nice industrial key and fill lights
                GameObject keyLightGo = new GameObject("KeyLight");
                var keyLight = keyLightGo.AddComponent<Light>();
                keyLight.type = LightType.Directional;
                keyLight.intensity = 1.4f;
                keyLight.color = new Color(1f, 0.98f, 0.95f);
                keyLightGo.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

                GameObject fillLightGo = new GameObject("FillLight");
                var fillLight = fillLightGo.AddComponent<Light>();
                fillLight.type = LightType.Directional;
                fillLight.intensity = 0.6f;
                fillLight.color = new Color(0.85f, 0.9f, 1f);
                fillLightGo.transform.rotation = Quaternion.Euler(30f, 145f, 0f);

                RenderToPNG(cam, Path.Combine(OutputDir, "SCREENSHOT_1_Unity_Original_Machinery.png"), 1920, 1080);

                UnityEngine.Object.DestroyImmediate(camGo);
                UnityEngine.Object.DestroyImmediate(keyLightGo);
                UnityEngine.Object.DestroyImmediate(fillLightGo);
                Debug.Log("[ScreenshotRenderer] Saved SCREENSHOT_1_Unity_Original_Machinery.png");
            }

            // --- SCREENSHOT 2: Unity Editor Separate Machinery AR Scene Hierarchy ---
            Debug.Log("[ScreenshotRenderer] Rendering Screenshot 2: Separate Machinery AR Scene Hierarchy...");
            Scene arScene = EditorSceneManager.OpenScene(ARScenePath, OpenSceneMode.Single);

            // Re-activate placement root temporarily for screenshot rendering
            GameObject placementRoot = null;
            foreach (var r in arScene.GetRootGameObjects())
            {
                if (r.name == "MachineryARPlacementRoot")
                {
                    placementRoot = r;
                    break;
                }
            }

            if (placementRoot != null)
            {
                placementRoot.SetActive(true);
                foreach (var r in placementRoot.GetComponentsInChildren<Renderer>(true))
                {
                    r.enabled = true;
                    if (!r.gameObject.activeSelf) r.gameObject.SetActive(true);
                }

                GameObject camGo2 = new GameObject("RenderCamera_Screenshot2");
                Camera cam2 = camGo2.AddComponent<Camera>();
                cam2.clearFlags = CameraClearFlags.SolidColor;
                cam2.backgroundColor = new Color(0.10f, 0.12f, 0.16f, 1.0f);
                cam2.nearClipPlane = 0.05f;
                cam2.farClipPlane = 100f;
                cam2.fieldOfView = 55f;

                camGo2.transform.position = new Vector3(2.5f, 2.2f, -4.5f);
                camGo2.transform.rotation = Quaternion.Euler(15f, -25f, 0f);

                RenderToPNG(cam2, Path.Combine(OutputDir, "SCREENSHOT_2_Unity_AR_Scene_Setup.png"), 1920, 1080);

                UnityEngine.Object.DestroyImmediate(camGo2);

                // Re-deactivate placementRoot to maintain startup requirement
                placementRoot.SetActive(false);
                EditorSceneManager.SaveScene(arScene);
                Debug.Log("[ScreenshotRenderer] Saved SCREENSHOT_2_Unity_AR_Scene_Setup.png");
            }
        }

        private static void RenderToPNG(Camera cam, string filePath, int width, int height)
        {
            RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            cam.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);

            File.WriteAllBytes(filePath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }
    }
}
#endif
