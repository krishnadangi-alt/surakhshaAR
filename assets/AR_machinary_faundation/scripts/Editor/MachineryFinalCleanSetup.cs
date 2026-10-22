#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryFinalCleanSetup
    {
        [MenuItem("SurakshaAR/Machinery/Execute Final Clean Physical Setup")]
        public static void ExecuteFinalCleanPhysicalSetup()
        {
            string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
            if (!File.Exists(scenePath))
            {
                Debug.LogError($"[FinalSetup] Scene file missing at '{scenePath}'.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Debug.Log($"[FinalSetup] Cleaning duplicate instances & restoring approved layout in scene: {scene.path}");

            // 1. Remove all inactive duplicate main machinery instances
            var allGameObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var go in allGameObjects)
            {
                if (!go.activeSelf && (go.name == "FeedHopper" || go.name == "ConveyorAssembly" || go.name == "DriveMotor" || go.name == "ControlPanel" || go.name == "EmergencyStop" || go.name == "MainIsolator" || go.name == "SteelWorkstationTable" || go.name == "MotorBasePlate" || go.name == "Coupling"))
                {
                    Debug.Log($"[FinalSetup] Removing inactive duplicate node: {go.name}");
                    Object.DestroyImmediate(go);
                }
            }

            // 2. Ensure active single instances under MachineryScenario
            GameObject scenarioRoot = GameObject.Find("MachineryScenario");
            if (scenarioRoot == null)
            {
                scenarioRoot = new GameObject("MachineryScenario");
            }

            // Locate / Align FeedHopper
            GameObject hopper = GameObject.Find("FeedHopper");
            if (hopper != null)
            {
                hopper.SetActive(true);
                hopper.transform.SetParent(scenarioRoot.transform, true);
                hopper.transform.position = new Vector3(-1.85f, 1.57f, 1.56f);
                hopper.transform.rotation = Quaternion.identity;
                hopper.transform.localScale = Vector3.one;
            }

            // Locate / Align ConveyorAssembly
            GameObject conveyor = GameObject.Find("ConveyorAssembly") ?? GameObject.Find("industrial_conveyor_belt");
            if (conveyor != null)
            {
                conveyor.SetActive(true);
                conveyor.transform.SetParent(scenarioRoot.transform, true);
                conveyor.transform.position = new Vector3(-1.50f, 0.00f, 0.00f);
                conveyor.transform.rotation = Quaternion.identity;
                conveyor.transform.localScale = Vector3.one;
            }

            // Locate / Align DriveMotor
            GameObject motor = GameObject.Find("DriveMotor");
            if (motor != null)
            {
                motor.SetActive(true);
                motor.transform.SetParent(scenarioRoot.transform, true);
                motor.transform.position = new Vector3(2.608f, 0.920f, 0.010f);
                motor.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                motor.transform.localScale = new Vector3(0.46f, 0.46f, 0.46f);

                // Apply rich electric blue material
                Color electricBlue = new Color(0.12f, 0.35f, 0.68f, 1.0f);
                foreach (var r in motor.GetComponentsInChildren<Renderer>())
                {
                    if (r.sharedMaterial != null)
                    {
                        Material mat = new Material(r.sharedMaterial);
                        mat.name = "Industrial_Electric_Blue_Motor";
                        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", electricBlue);
                        if (mat.HasProperty("_Color")) mat.SetColor("_Color", electricBlue);
                        mat.SetFloat("_Metallic", 0.85f);
                        mat.SetFloat("_Smoothness", 0.65f);
                        r.sharedMaterial = mat;
                    }
                }
            }

            // Locate / Align MotorBasePlate
            GameObject basePlate = GameObject.Find("MotorBasePlate");
            if (basePlate == null && motor != null)
            {
                basePlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                basePlate.name = "MotorBasePlate";
            }
            if (basePlate != null)
            {
                basePlate.SetActive(true);
                if (motor != null) basePlate.transform.SetParent(motor.transform, true);
                basePlate.transform.position = new Vector3(2.608f, 0.020f, 0.010f);
                basePlate.transform.rotation = Quaternion.identity;
                basePlate.transform.localScale = new Vector3(0.85f, 0.04f, 0.75f);
            }

            // Locate / Align Coupling
            GameObject coupling = GameObject.Find("Coupling");
            if (coupling == null && motor != null)
            {
                coupling = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                coupling.name = "Coupling";
            }
            if (coupling != null)
            {
                coupling.SetActive(true);
                if (motor != null) coupling.transform.SetParent(motor.transform, true);
                coupling.transform.position = new Vector3(2.378f, 0.920f, 0.010f);
                coupling.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                coupling.transform.localScale = new Vector3(0.14f, 0.16f, 0.14f);
            }

            // Locate / Align Safety Station Components
            GameObject panel = GameObject.Find("ControlPanel");
            if (panel != null)
            {
                panel.SetActive(true);
                panel.transform.SetParent(scenarioRoot.transform, true);
                panel.transform.position = new Vector3(1.95f, 1.30f, 1.35f);
                panel.transform.rotation = Quaternion.identity;
            }

            GameObject estop = GameObject.Find("EmergencyStop");
            if (estop != null)
            {
                estop.SetActive(true);
                estop.transform.SetParent(scenarioRoot.transform, true);
                estop.transform.position = new Vector3(3.10f, 1.15f, 1.35f);
                estop.transform.rotation = Quaternion.identity;
            }

            GameObject isolator = GameObject.Find("MainIsolator");
            if (isolator != null)
            {
                isolator.SetActive(true);
                isolator.transform.SetParent(scenarioRoot.transform, true);
                isolator.transform.position = new Vector3(3.90f, 1.15f, 1.35f);
                isolator.transform.rotation = Quaternion.identity;
            }

            // Locate / Align Steel Workstation Table & LOTO Items
            GameObject table = GameObject.Find("SteelWorkstationTable");
            if (table != null)
            {
                table.SetActive(true);
                table.transform.SetParent(scenarioRoot.transform, true);
                table.transform.position = new Vector3(3.90f, 0.00f, -1.35f);
                table.transform.rotation = Quaternion.identity;
            }

            GameObject hasp = GameObject.Find("Hasp");
            if (hasp != null)
            {
                hasp.SetActive(true);
                if (table != null) hasp.transform.SetParent(table.transform, true);
                hasp.transform.position = new Vector3(3.70f, 0.82f, -1.25f);
            }

            GameObject padlockTag = GameObject.Find("PadlockAndTag");
            if (padlockTag != null)
            {
                padlockTag.SetActive(true);
                if (table != null) padlockTag.transform.SetParent(table.transform, true);
                padlockTag.transform.position = new Vector3(4.10f, 0.82f, -1.25f);
            }

            // Mark Scene Dirty and Save
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[FinalSetup] Scene cleaned, aligned to approved setup, and saved successfully.");

            // 3. Render Camera Verification Views
            RenderVerificationViews();
        }

        private static void RenderVerificationViews()
        {
            string outDir = "Builds/Verification/UnityScene";
            string brainDir = "C:/Users/jainh/.gemini/antigravity-ide/brain/89459e12-a559-4036-8ffc-ee9eb81b1fda";
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
            if (!Directory.Exists(brainDir)) Directory.CreateDirectory(brainDir);

            GameObject camObj = new GameObject("FinalCleanCam");
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.18f, 0.20f, 0.23f, 1.0f);
            cam.fieldOfView = 45f;

            // View 1: Complete Scene Overview
            cam.transform.position = new Vector3(0.5f, 1.7f, -3.8f);
            cam.transform.LookAt(new Vector3(0.6f, 0.85f, 0.0f));
            SaveCam(cam, Path.Combine(outDir, "Machinery_Final_Clean_Setup.png"));
            SaveCam(cam, Path.Combine(brainDir, "Machinery_Final_Clean_Setup.png"));

            Object.DestroyImmediate(camObj);
            Debug.Log("[FinalSetup] Verification renders completed.");
        }

        private static void SaveCam(Camera cam, string path)
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
            File.WriteAllBytes(path, bytes);

            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
        }
    }
}
#endif
