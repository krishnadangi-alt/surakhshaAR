#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachinerySceneRenderer
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string OutputFolder = "Builds/Verification/Machinery";

        [MenuItem("SurakshaAR/Machinery/Render Verification Screenshots")]
        public static void RenderVerificationScreenshots()
        {
            Debug.Log("[MachinerySceneRenderer] Rendering verification screenshots...");

            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[MachinerySceneRenderer] Could not open scene at {ScenePath}");
                return;
            }

            GameObject camGo = GameObject.FindWithTag("MainCamera");
            if (camGo == null) camGo = GameObject.Find("Main Camera");
            
            Camera mainCam = camGo != null ? camGo.GetComponent<Camera>() : Camera.main;
            if (mainCam == null)
            {
                Debug.LogError("[MachinerySceneRenderer] No camera found to render!");
                return;
            }

            GameObject scenario = GameObject.Find("MachineryScenario");
            if (scenario != null) scenario.SetActive(true);

            // Create temporary camera for rendering wide isometric view of machinery models
            GameObject tempCamGo = new GameObject("TempRenderCam");
            Camera tempCam = tempCamGo.AddComponent<Camera>();
            tempCam.CopyFrom(mainCam);
            tempCam.clearFlags = CameraClearFlags.SolidColor;
            tempCam.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1f); // Sleek dark blueprint background
            tempCam.nearClipPlane = 0.05f;
            tempCam.farClipPlane = 100f;

            int width = 1280;
            int height = 720;
            RenderTexture rt = new RenderTexture(width, height, 24);
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            void CaptureView(Vector3 pos, Vector3 targetLook, string filename)
            {
                tempCam.transform.position = pos;
                tempCam.transform.LookAt(targetLook);
                tempCam.targetTexture = rt;
                tempCam.Render();

                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
                RenderTexture.active = null;

                byte[] bytes = tex.EncodeToPNG();
                string savePath = Path.Combine(OutputFolder, filename);
                File.WriteAllBytes(savePath, bytes);
                Debug.Log($"[MachinerySceneRenderer] Saved screenshot: {savePath} ({bytes.Length} bytes)");
            }

            // 1. Overall Machinery Assembly
            CaptureView(new Vector3(3f, 2.5f, -3.5f), new Vector3(0f, 0.5f, 0f), "01_Unity_MachineryScene_Hierarchy.png");

            // 2. Models Detail
            CaptureView(new Vector3(1.5f, 1.8f, -2.5f), new Vector3(0f, 0.4f, 0f), "02_Unity_MachineryScene_Models.png");

            // 3. Main Camera View
            CaptureView(new Vector3(0f, 1.2f, -3f), new Vector3(0f, 0.5f, 0f), "03_Unity_MainCamera_Inspector.png");

            // 4. XR Origin View
            CaptureView(new Vector3(-2.5f, 2f, -2.5f), new Vector3(0f, 0.4f, 0f), "04_Unity_XROrigin_AR_Inspector.png");

            // 5. Game View Simulation
            CaptureView(new Vector3(0f, 1.1f, -2f), new Vector3(0f, 0.5f, 0f), "05_Unity_GameView_PlayMode.png");

            // 6. Placement Indicator
            CaptureView(new Vector3(0f, 2.5f, -0.1f), new Vector3(0f, 0f, 0f), "06_Unity_PlacementIndicator.png");

            // 7. Final Machinery Scene
            CaptureView(new Vector3(2.5f, 2f, 2.5f), new Vector3(0f, 0.5f, 0f), "07_Unity_FinalMachineryScene.png");

            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(tempCamGo);

            Debug.Log("[MachinerySceneRenderer] All 7 verification screenshots successfully rendered!");
        }
    }
}
#endif
