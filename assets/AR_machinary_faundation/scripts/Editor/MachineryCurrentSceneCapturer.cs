#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryCurrentSceneCapturer
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string OutputFolder = "Builds/Verification/UnityScene";

        [MenuItem("SurakshaAR/Machinery/Capture Current Scene 3 PNG Views")]
        public static void CaptureCurrentScene()
        {
            Debug.Log("[MachineryCurrentSceneCapturer] Capturing 3 PNG views from current un-modified scene...");

            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            // Open scene strictly as-is
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[MachineryCurrentSceneCapturer] Failed to open scene at {ScenePath}");
                return;
            }

            GameObject mainCamGo = GameObject.FindWithTag("MainCamera");
            if (mainCamGo == null) mainCamGo = GameObject.Find("Main Camera");
            Camera mainCam = mainCamGo != null ? mainCamGo.GetComponent<Camera>() : Camera.main;

            // Create temporary camera in memory for rendering
            GameObject tempCamGo = new GameObject("TempSceneRenderCam");
            Camera tempCam = tempCamGo.AddComponent<Camera>();
            if (mainCam != null)
            {
                tempCam.CopyFrom(mainCam);
            }

            tempCam.clearFlags = CameraClearFlags.SolidColor;
            tempCam.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1f); // Dark blueprint background
            tempCam.nearClipPlane = 0.05f;
            tempCam.farClipPlane = 100f;

            int width = 1280;
            int height = 720;
            RenderTexture rt = new RenderTexture(width, height, 24);
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            void RenderView(Vector3 eyePos, Vector3 targetLook, string filename)
            {
                tempCam.transform.position = eyePos;
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
                Debug.Log($"[MachineryCurrentSceneCapturer] Rendered {filename} ({bytes.Length} bytes)");
            }

            // PNG 1: Overall Machinery Scene
            RenderView(new Vector3(2.8f, 2.2f, -3.2f), new Vector3(0f, 0.4f, -0.3f), "Overall_Machinery_Scene.png");

            // PNG 2: Control & Safety Area
            RenderView(new Vector3(1.4f, 1.4f, -2.4f), new Vector3(0.8f, 0.5f, -0.6f), "Control_Safety_Area.png");

            // PNG 3: LOTO Area
            RenderView(new Vector3(-1.4f, 1.3f, -2.4f), new Vector3(-1.3f, 0.5f, -1.0f), "LOTO_Area.png");

            // Cleanup memory objects without modifying scene
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(tempCamGo);

            Debug.Log("[MachineryCurrentSceneCapturer] All 3 PNG renders successfully generated from the current scene!");
        }
    }
}
#endif
