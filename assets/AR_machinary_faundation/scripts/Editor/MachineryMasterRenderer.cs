using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryMasterRenderer
    {
        [MenuItem("SurakshaAR/Render All Verification Views")]
        public static void RenderAllViews()
        {
            string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            string outputDir = Path.Combine(Application.dataPath, "../Builds/Verification/UnityScene");
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            // Ensure lighting and active camera setup
            Camera cam = CreateOrGetRenderCamera();

            // Render 1: Reference Perspective View (Perspective angle matching reference image)
            SetupCamera(cam, new Vector3(-0.5f, 2.5f, -4.5f), new Vector3(20f, 35f, 0f), 55f);
            RenderToPNG(cam, Path.Combine(outputDir, "01_Master_Reference_Match.png"));

            // Render 2: Full Front View
            SetupCamera(cam, new Vector3(0.5f, 1.8f, -5.0f), new Vector3(10f, 0f, 0f), 60f);
            RenderToPNG(cam, Path.Combine(outputDir, "02_Full_Front_View.png"));

            // Render 3: Safety & LOTO Station Close-Up
            SetupCamera(cam, new Vector3(2.8f, 1.4f, -3.0f), new Vector3(12f, -15f, 0f), 45f);
            RenderToPNG(cam, Path.Combine(outputDir, "03_Safety_LOTO_Station.png"));

            // Render 4: Feed Hopper & Conveyor Feed Close-Up
            SetupCamera(cam, new Vector3(-2.0f, 1.8f, -3.2f), new Vector3(15f, 25f, 0f), 45f);
            RenderToPNG(cam, Path.Combine(outputDir, "04_FeedHopper_FeedEnd.png"));

            // Render 5: Coal Jam Area Close-Up
            SetupCamera(cam, new Vector3(0.0f, 1.8f, -2.8f), new Vector3(25f, 10f, 0f), 40f);
            RenderToPNG(cam, Path.Combine(outputDir, "05_Coal_Jam_Area.png"));

            // Render 6: Drive Motor Close-Up
            SetupCamera(cam, new Vector3(2.2f, 1.5f, -2.5f), new Vector3(15f, 40f, 0f), 40f);
            RenderToPNG(cam, Path.Combine(outputDir, "06_DriveMotor_DriveEnd.png"));

            // Cleanup render camera if dynamically created
            UnityEngine.Object.DestroyImmediate(cam.gameObject);

            Debug.Log($"All 6 Verification Renders Saved to {outputDir}");
        }

        private static Camera CreateOrGetRenderCamera()
        {
            GameObject camGo = new GameObject("TempRenderCamera");
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f); // Professional dark neutral background
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 1000f;
            return cam;
        }

        private static void SetupCamera(Camera cam, Vector3 pos, Vector3 rot, float fov)
        {
            cam.transform.position = pos;
            cam.transform.eulerAngles = rot;
            cam.fieldOfView = fov;
        }

        private static void RenderToPNG(Camera cam, string filePath)
        {
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

            byte[] bytes = screenShot.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
            UnityEngine.Object.DestroyImmediate(screenShot);
        }
    }
}
