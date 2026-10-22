#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryDriveMotorFixer
    {
        [MenuItem("SurakshaAR/Machinery/Execute Motor-Only Precision Repair")]
        public static void ExecuteDriveMotorPrecisionRepair()
        {
            string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
            string preFixBackupPath = "Assets/AR_machinary_faundation/scenes/Backups/MachineryConveyorJam_PRE_MOTOR_ONLY_FIX.unity";

            // 1. Verify / Create Pre-Motor-Fix Backup Scene
            if (File.Exists(scenePath))
            {
                string backupDir = Path.GetDirectoryName(preFixBackupPath);
                if (!Directory.Exists(backupDir)) Directory.CreateDirectory(backupDir);
                if (!File.Exists(preFixBackupPath))
                {
                    File.Copy(scenePath, preFixBackupPath, true);
                    Debug.Log($"[MotorFixer] Created pre-fix backup at '{preFixBackupPath}'.");
                }
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }

            var scene = EditorSceneManager.GetActiveScene();
            Debug.Log($"[MotorFixer] Executing Motor-Only Precision Repair on scene: {scene.path}");

            // Locate ConveyorAssembly (Master Reference - DO NOT MODIFY CONVEYOR OR ANY OTHER OBJECT)
            GameObject conveyor = GameObject.Find("ConveyorAssembly");
            if (conveyor == null) conveyor = GameObject.Find("industrial_conveyor_belt");

            if (conveyor == null)
            {
                Debug.LogError("[MotorFixer] ConveyorAssembly missing! Cannot align motor.");
                return;
            }

            // Calculate Conveyor Head / Drive Pulley Centerline from conveyor renderers
            var conveyorRenderers = conveyor.GetComponentsInChildren<Renderer>();
            Bounds conveyorBounds = new Bounds();
            bool firstConv = true;
            foreach (var r in conveyorRenderers)
            {
                if (firstConv) { conveyorBounds = r.bounds; firstConv = false; }
                else { conveyorBounds.Encapsulate(r.bounds); }
            }

            float conveyorMaxX = conveyorBounds.max.x; // Discharge head end (~ +2.24m)
            float driveShaftY = 0.92f;                  // Head shaft center height (~ 0.92m)
            float driveShaftZ = conveyorBounds.center.z;// Centerline Z (~ 0.01m)

            Debug.Log($"[MotorFixer] Conveyor Drive End: MaxX={conveyorMaxX:F2}, ShaftY={driveShaftY:F2}, ShaftZ={driveShaftZ:F2}");

            // Locate / Prepare DriveMotor
            GameObject scenarioRoot = conveyor.transform.parent != null ? conveyor.transform.parent.gameObject : GameObject.Find("MachineryScenario");
            GameObject motor = GameObject.Find("DriveMotor");
            if (motor == null)
            {
                string glbPath = "Assets/AR_machinary_faundation/3d/Raw/electric_motor.glb";
                GameObject glbPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
                if (glbPrefab != null)
                {
                    motor = (GameObject)PrefabUtility.InstantiatePrefab(glbPrefab, scenarioRoot.transform);
                    motor.name = "DriveMotor";
                }
            }
            if (motor == null)
            {
                Debug.LogError("[MotorFixer] DriveMotor asset missing!");
                return;
            }

            motor.SetActive(true);
            motor.transform.SetParent(scenarioRoot.transform, true);

            // Clean obsolete duplicate child mounts under motor ONLY
            for (int i = motor.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = motor.transform.GetChild(i);
                if (child.name.Contains("Mount") || child.name.Contains("Frame") || child.name.Contains("Slab"))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            // 2. Set Motor Transform to align motor shaft centerline with conveyor drive shaft
            motor.transform.localScale = new Vector3(0.46f, 0.46f, 0.46f);
            motor.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            motor.transform.position = new Vector3(conveyorMaxX + 0.28f, driveShaftY, driveShaftZ);

            var motorRenderers = motor.GetComponentsInChildren<Renderer>();
            Bounds motorBounds = new Bounds();
            bool firstM = true;
            foreach (var r in motorRenderers)
            {
                if (firstM) { motorBounds = r.bounds; firstM = false; }
                else { motorBounds.Encapsulate(r.bounds); }
            }

            // Apply rich industrial electric blue material matching reference
            Color electricBlue = new Color(0.12f, 0.35f, 0.68f, 1.0f);
            foreach (var r in motorRenderers)
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

            // 3. Configure MotorBasePlate directly under motor body
            GameObject basePlate = GameObject.Find("MotorBasePlate");
            if (basePlate == null)
            {
                basePlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                basePlate.name = "MotorBasePlate";
            }
            basePlate.SetActive(true);
            basePlate.transform.SetParent(motor.transform, true);
            basePlate.transform.position = new Vector3(motorBounds.center.x, 0.02f, motorBounds.center.z);
            basePlate.transform.rotation = Quaternion.identity;
            basePlate.transform.localScale = new Vector3(0.85f, 0.04f, 0.75f);

            Material darkSteelMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            Color darkSteel = new Color(0.15f, 0.18f, 0.22f, 1.0f);
            if (darkSteelMat.HasProperty("_BaseColor")) darkSteelMat.SetColor("_BaseColor", darkSteel);
            if (darkSteelMat.HasProperty("_Color")) darkSteelMat.SetColor("_Color", darkSteel);
            var bpRenderer = basePlate.GetComponent<Renderer>();
            if (bpRenderer != null) bpRenderer.sharedMaterial = darkSteelMat;

            // 4. Configure Coupling connecting motor shaft directly to conveyor drive shaft
            GameObject coupling = GameObject.Find("Coupling");
            if (coupling == null)
            {
                coupling = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                coupling.name = "Coupling";
            }
            coupling.SetActive(true);
            coupling.transform.SetParent(motor.transform, true);
            coupling.transform.position = new Vector3(conveyorMaxX + 0.05f, driveShaftY, driveShaftZ);
            coupling.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            coupling.transform.localScale = new Vector3(0.14f, 0.16f, 0.14f);

            Material metallicCouplingMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            Color metallicSilver = new Color(0.75f, 0.78f, 0.82f, 1.0f);
            if (metallicCouplingMat.HasProperty("_BaseColor")) metallicCouplingMat.SetColor("_BaseColor", metallicSilver);
            if (metallicCouplingMat.HasProperty("_Color")) metallicCouplingMat.SetColor("_Color", metallicSilver);
            var cpRenderer = coupling.GetComponent<Renderer>();
            if (cpRenderer != null) cpRenderer.sharedMaterial = metallicCouplingMat;

            // 5. Save Scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[MotorFixer] Motor subsystem successfully aligned, coupled, grounded, and saved.");

            // 6. Render 5 Mandatory Verification Views
            RenderMotorVerificationViews(conveyor, motor);
        }

        private static void RenderMotorVerificationViews(GameObject conveyor, GameObject motor)
        {
            string outDir = "Builds/Verification/UnityScene";
            string brainDir = "C:/Users/jainh/.gemini/antigravity-ide/brain/89459e12-a559-4036-8ffc-ee9eb81b1fda";
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
            if (!Directory.Exists(brainDir)) Directory.CreateDirectory(brainDir);

            GameObject camObj = new GameObject("MotorVerificationCam");
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.18f, 0.20f, 0.23f, 1.0f);
            cam.fieldOfView = 45f;

            // VIEW 1 — OVERALL: Conveyor and motor together
            cam.orthographic = false;
            cam.transform.position = new Vector3(0.5f, 1.7f, -3.8f);
            cam.transform.LookAt(new Vector3(0.6f, 0.85f, 0.0f));
            RenderCamToPNG(cam, Path.Combine(outDir, "VIEW1_Overall.png"));
            RenderCamToPNG(cam, Path.Combine(brainDir, "VIEW1_Overall.png"));

            // VIEW 2 — MOTOR CLOSE-UP: DriveMotor + conveyor head + coupling
            cam.transform.position = new Vector3(3.6f, 1.45f, -1.6f);
            cam.transform.LookAt(new Vector3(2.4f, 0.85f, 0.0f));
            RenderCamToPNG(cam, Path.Combine(outDir, "VIEW2_Motor_Closeup.png"));
            RenderCamToPNG(cam, Path.Combine(brainDir, "VIEW2_Motor_Closeup.png"));

            // VIEW 3 — SIDE VIEW: Motor shaft, coupling and conveyor shaft on the same axis
            cam.orthographic = true;
            cam.orthographicSize = 0.75f;
            cam.transform.position = new Vector3(2.4f, 0.92f, -2.5f);
            cam.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            RenderCamToPNG(cam, Path.Combine(outDir, "VIEW3_Side_Shaft_Alignment.png"));
            RenderCamToPNG(cam, Path.Combine(brainDir, "VIEW3_Side_Shaft_Alignment.png"));

            // VIEW 4 — TOP/ANGLED VIEW: Motor direction relative to conveyor
            cam.orthographic = true;
            cam.orthographicSize = 0.9f;
            cam.transform.position = new Vector3(2.4f, 3.5f, 0.01f);
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            RenderCamToPNG(cam, Path.Combine(outDir, "VIEW4_Top_Shaft_Direction.png"));
            RenderCamToPNG(cam, Path.Combine(brainDir, "VIEW4_Top_Shaft_Direction.png"));

            // VIEW 5 — BASE VIEW: DriveMotor sitting correctly on MotorBasePlate and base on ground
            cam.orthographic = false;
            cam.transform.position = new Vector3(3.1f, 0.6f, -1.2f);
            cam.transform.LookAt(new Vector3(2.52f, 0.25f, 0.01f));
            RenderCamToPNG(cam, Path.Combine(outDir, "VIEW5_Base_Grounding.png"));
            RenderCamToPNG(cam, Path.Combine(brainDir, "VIEW5_Base_Grounding.png"));

            Object.DestroyImmediate(camObj);
            Debug.Log("[MotorFixer] All 5 mandatory motor verification views rendered successfully.");
        }

        private static void RenderCamToPNG(Camera cam, string path)
        {
            int width = 1920;
            int height = 1080;
            RenderTexture rt = new RenderTexture(width, height, 24);
            cam.targetTexture = rt;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
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
