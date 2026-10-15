#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryFinalMotorInspector
    {
        [MenuItem("SurakshaAR/Machinery/Execute Final Motor Visual Renders")]
        public static void ExecuteFinalMotorRenders()
        {
            string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
            if (!File.Exists(scenePath))
            {
                Debug.LogError($"[FinalInspector] Scene file not found at '{scenePath}'.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Debug.Log($"[FinalInspector] Opened target scene: {scene.path}");

            // Locate objects for coordinate auditing
            GameObject motor = GameObject.Find("DriveMotor");
            GameObject coupling = GameObject.Find("Coupling");
            GameObject basePlate = GameObject.Find("MotorBasePlate");
            GameObject conveyor = GameObject.Find("ConveyorAssembly") ?? GameObject.Find("industrial_conveyor_belt");
            GameObject hopper = GameObject.Find("FeedHopper");
            GameObject controlPanel = GameObject.Find("ControlPanel");
            GameObject eStop = GameObject.Find("EmergencyStop");
            GameObject isolator = GameObject.Find("MainIsolator");
            GameObject table = GameObject.Find("SteelWorkstationTable");
            GameObject coalGroup = GameObject.Find("CoalGroup") ?? GameObject.Find("JamArea");

            Debug.Log("================ SCENE TRANSFORM AUDIT ================");
            if (motor != null) Debug.Log($"DriveMotor Pos: {motor.transform.position.ToString("F3")}, Rot: {motor.transform.eulerAngles.ToString("F1")}, Scale: {motor.transform.localScale.ToString("F2")}");
            if (coupling != null) Debug.Log($"Coupling Pos: {coupling.transform.position.ToString("F3")}, Rot: {coupling.transform.eulerAngles.ToString("F1")}, Scale: {coupling.transform.localScale.ToString("F2")}");
            if (basePlate != null) Debug.Log($"MotorBasePlate Pos: {basePlate.transform.position.ToString("F3")}, Rot: {basePlate.transform.eulerAngles.ToString("F1")}, Scale: {basePlate.transform.localScale.ToString("F2")}");
            if (conveyor != null) Debug.Log($"ConveyorAssembly Pos: {conveyor.transform.position.ToString("F3")}");
            if (hopper != null) Debug.Log($"FeedHopper Pos: {hopper.transform.position.ToString("F3")}");
            if (controlPanel != null) Debug.Log($"ControlPanel Pos: {controlPanel.transform.position.ToString("F3")}");
            if (eStop != null) Debug.Log($"EmergencyStop Pos: {eStop.transform.position.ToString("F3")}");
            if (isolator != null) Debug.Log($"MainIsolator Pos: {isolator.transform.position.ToString("F3")}");
            if (table != null) Debug.Log($"SteelWorkstationTable Pos: {table.transform.position.ToString("F3")}");
            if (coalGroup != null) Debug.Log($"CoalGroup/JamArea Pos: {coalGroup.transform.position.ToString("F3")}");
            Debug.Log("=======================================================");

            // Directories
            string buildDir = "Builds/Verification/UnityScene";
            string artifactDir = "C:/Users/jainh/.gemini/antigravity-ide/brain/89459e12-a559-4036-8ffc-ee9eb81b1fda";

            if (!Directory.Exists(buildDir)) Directory.CreateDirectory(buildDir);
            if (!Directory.Exists(artifactDir)) Directory.CreateDirectory(artifactDir);

            // Render 5 mandatory views
            GameObject camObj = new GameObject("FinalVerificationCam");
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.18f, 0.20f, 0.23f, 1.0f);
            cam.fieldOfView = 45f;

            // View 1: Overall worker-facing view
            cam.orthographic = false;
            cam.transform.position = new Vector3(0.5f, 1.7f, -3.8f);
            cam.transform.LookAt(new Vector3(0.6f, 0.85f, 0.0f));
            SaveCamRender(cam, Path.Combine(buildDir, "01_Overall_Worker_Facing_View.png"));
            SaveCamRender(cam, Path.Combine(artifactDir, "01_Overall_Worker_Facing_View.png"));

            // View 2: Motor 3/4 close-up
            cam.transform.position = new Vector3(3.6f, 1.45f, -1.6f);
            cam.transform.LookAt(new Vector3(2.4f, 0.85f, 0.0f));
            SaveCamRender(cam, Path.Combine(buildDir, "02_Motor_3_4_Closeup.png"));
            SaveCamRender(cam, Path.Combine(artifactDir, "02_Motor_3_4_Closeup.png"));

            // View 3: Side view showing shaft alignment (Orthographic for exact axial visual test)
            cam.orthographic = true;
            cam.orthographicSize = 0.75f;
            cam.transform.position = new Vector3(2.4f, 0.92f, -2.5f);
            cam.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            SaveCamRender(cam, Path.Combine(buildDir, "03_Side_Shaft_Alignment.png"));
            SaveCamRender(cam, Path.Combine(artifactDir, "03_Side_Shaft_Alignment.png"));

            // View 4: Top view showing shaft direction (Orthographic plan view)
            cam.orthographic = true;
            cam.orthographicSize = 0.9f;
            cam.transform.position = new Vector3(2.4f, 3.5f, 0.01f);
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            SaveCamRender(cam, Path.Combine(buildDir, "04_Top_Shaft_Direction.png"));
            SaveCamRender(cam, Path.Combine(artifactDir, "04_Top_Shaft_Direction.png"));

            // View 5: Close-up showing motor + coupling + conveyor drive
            cam.orthographic = false;
            cam.transform.position = new Vector3(2.95f, 1.25f, -1.05f);
            cam.transform.LookAt(new Vector3(2.35f, 0.92f, 0.01f));
            SaveCamRender(cam, Path.Combine(buildDir, "05_Motor_Coupling_Drive_Closeup.png"));
            SaveCamRender(cam, Path.Combine(artifactDir, "05_Motor_Coupling_Drive_Closeup.png"));

            Object.DestroyImmediate(camObj);
            Debug.Log("[FinalInspector] Successfully rendered all 5 mandatory views.");
        }

        private static void SaveCamRender(Camera cam, string outPath)
        {
            int w = 1920;
            int h = 1080;
            RenderTexture rt = new RenderTexture(w, h, 24);
            cam.targetTexture = rt;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(outPath, bytes);

            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Debug.Log($"[FinalInspector] Saved render to: {outPath}");
        }
    }
}
#endif
