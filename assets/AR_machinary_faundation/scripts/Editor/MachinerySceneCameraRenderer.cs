#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachinerySceneCameraRenderer
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string ArtifactDir = @"C:\Users\jainh\.gemini\antigravity-ide\brain\6ce6aafa-fdea-48a4-9187-df89164857fe";

        [MenuItem("SurakshaAR/Machinery/Render All Scene Camera Views")]
        public static void RenderAllViews()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError("[CameraRenderer] Failed to open scene!");
                return;
            }

            Directory.CreateDirectory("scratch");
            if (Directory.Exists(ArtifactDir))
            {
                Directory.CreateDirectory(ArtifactDir);
            }

            // Create temporary rendering camera
            GameObject camGo = new GameObject("TempRenderCamera");
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Color;
            cam.backgroundColor = new Color(0.15f, 0.17f, 0.20f, 1.0f); // Sleek dark industrial background
            cam.fieldOfView = 55f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;

            // Add light if missing
            Light light = Object.FindFirstObjectByType<Light>();
            if (light == null)
            {
                GameObject lightGo = new GameObject("TempDirectionalLight");
                light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            // Define 6 target views matching IMAGE 1 layout (Feed on Left X=-2.0m, Table on Right X=+4.5m)
            ViewDef[] views = new ViewDef[]
            {
                new ViewDef("View1_Overall", new Vector3(1.2f, 4.5f, -7.5f), new Vector3(1.25f, 0.8f, 0.0f)),
                new ViewDef("View2_Feed", new Vector3(-1.95f, 2.5f, -3.5f), new Vector3(-1.95f, 1.2f, 0.0f)),
                new ViewDef("View3_ConveyorJam", new Vector3(0.8f, 2.5f, -3.2f), new Vector3(0.8f, 1.16f, 0.0f)),
                new ViewDef("View4_Drive", new Vector3(2.2f, 2.2f, -3.0f), new Vector3(2.2f, 0.5f, 0.3f)),
                new ViewDef("View5_SafetyStation", new Vector3(3.0f, 1.8f, -3.2f), new Vector3(3.0f, 1.0f, -1.35f)),
                new ViewDef("View6_LOTOStation", new Vector3(4.5f, 1.9f, -3.2f), new Vector3(4.5f, 0.8f, -1.45f))
            };

            int width = 1280;
            int height = 720;
            RenderTexture rt = new RenderTexture(width, height, 24);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);

            foreach (var v in views)
            {
                cam.transform.position = v.Position;
                cam.transform.LookAt(v.LookAtTarget);
                cam.Render();

                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                screenShot.Apply();
                RenderTexture.active = null;

                byte[] bytes = screenShot.EncodeToPNG();

                string scratchPath = Path.Combine("scratch", v.Name + ".png");
                File.WriteAllBytes(scratchPath, bytes);
                Debug.Log($"[CameraRenderer] Saved {scratchPath}");

                if (Directory.Exists(ArtifactDir))
                {
                    string artPath = Path.Combine(ArtifactDir, v.Name + ".png");
                    File.WriteAllBytes(artPath, bytes);
                    Debug.Log($"[CameraRenderer] Saved artifact {artPath}");
                }
            }

            cam.targetTexture = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(screenShot);
            Object.DestroyImmediate(camGo);

            Debug.Log("[CameraRenderer] All 6 views rendered successfully!");
        }

        private struct ViewDef
        {
            public string Name;
            public Vector3 Position;
            public Vector3 LookAtTarget;

            public ViewDef(string name, Vector3 pos, Vector3 lookAt)
            {
                Name = name;
                Position = pos;
                LookAtTarget = lookAt;
            }
        }
    }
}
#endif
