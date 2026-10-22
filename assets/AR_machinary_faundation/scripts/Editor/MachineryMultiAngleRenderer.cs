#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryMultiAngleRenderer
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string OutputFolder = "Builds/Verification/Machinery";

        [MenuItem("SurakshaAR/Machinery/Render 4 Multi-Angle Layout Views")]
        public static void RenderMultiAngleViews()
        {
            Debug.Log("[MachineryMultiAngleRenderer] Rendering 4 multi-angle layout verification images...");

            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[MachineryMultiAngleRenderer] Could not open scene at {ScenePath}");
                return;
            }

            GameObject camGo = GameObject.FindWithTag("MainCamera");
            if (camGo == null) camGo = GameObject.Find("Main Camera");
            
            Camera mainCam = camGo != null ? camGo.GetComponent<Camera>() : Camera.main;

            GameObject tempCamGo = new GameObject("TempMultiAngleCam");
            Camera tempCam = tempCamGo.AddComponent<Camera>();
            if (mainCam != null) tempCam.CopyFrom(mainCam);

            tempCam.clearFlags = CameraClearFlags.SolidColor;
            tempCam.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1f); // Sleek dark blueprint background
            tempCam.nearClipPlane = 0.05f;
            tempCam.farClipPlane = 100f;

            int width = 1280;
            int height = 720;
            RenderTexture rt = new RenderTexture(width, height, 24);
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            void CaptureAngle(Vector3 eyePos, Vector3 targetLook, string filename)
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
                Debug.Log($"[MachineryMultiAngleRenderer] Saved: {savePath} ({bytes.Length} bytes)");
            }

            Vector3 sceneCenter = new Vector3(0f, 0.5f, -0.2f);

            // VIEW 1: Front / Worker View (facing operator control side)
            CaptureAngle(new Vector3(0f, 1.2f, -3.2f), sceneCenter, "Layout_Front.png");

            // VIEW 2: 45-Degree Perspective View (isometric overview)
            CaptureAngle(new Vector3(3.2f, 2.5f, -3.2f), sceneCenter, "Layout_45deg.png");

            // VIEW 3: Opposite Side View (rear view of hopper and motor)
            CaptureAngle(new Vector3(0f, 1.5f, 3.2f), sceneCenter, "Layout_Opposite.png");

            // VIEW 4: Top Overview (plan view looking down)
            CaptureAngle(new Vector3(0f, 4.5f, -0.2f), sceneCenter, "Layout_Top.png");

            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(tempCamGo);

            Debug.Log("[MachineryMultiAngleRenderer] All 4 layout verification images rendered successfully!");
        }
    }
}
#endif
