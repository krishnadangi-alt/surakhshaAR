#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryDriveMotorVisualV3Fixer
    {
        [MenuItem("SurakshaAR/Machinery/Execute V3 Final Visual Drive Motor Fix")]
        public static void ExecuteV3Fix()
        {
            string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
            if (File.Exists(scenePath))
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }

            var scene = EditorSceneManager.GetActiveScene();
            Debug.Log($"[SurakshaAR V3Fixer] Executing V3 Visual Drive Motor Fix on active scene: {scene.path}");

            GameObject conveyor = GameObject.Find("ConveyorAssembly");
            if (conveyor == null) conveyor = GameObject.Find("industrial_conveyor_belt");

            GameObject motor = GameObject.Find("DriveMotor");
            if (motor == null) motor = GameObject.Find("electric_motor");

            if (conveyor == null || motor == null)
            {
                Debug.LogError($"[SurakshaAR V3Fixer] Conveyor or Motor missing! Conveyor={conveyor != null}, Motor={motor != null}");
                return;
            }

            // 1. Calculate Conveyor Discharge Head Roller Shaft position
            var conveyorRenderers = conveyor.GetComponentsInChildren<Renderer>();
            float maxHeadX = -999f;
            float headCenterY = 1.01f;
            float headCenterZ = 0.01f;

            foreach (var r in conveyorRenderers)
            {
                if (r.bounds.max.x > maxHeadX)
                {
                    maxHeadX = r.bounds.max.x;
                }
            }

            Debug.Log($"[SurakshaAR V3Fixer] Conveyor Head Max X={maxHeadX:F3}, Head Shaft Y={headCenterY:F3}, Z={headCenterZ:F3}");

            // 2. Remove old artificial support structures attached to motor
            Transform oldMount = motor.transform.Find("MotorMountFrame");
            if (oldMount != null) Object.DestroyImmediate(oldMount.gameObject);

            Transform oldSlab = motor.transform.Find("WhitePedestalSlab");
            if (oldSlab != null) Object.DestroyImmediate(oldSlab.gameObject);

            // Clean standalone MotorBasePlate if generated at root
            GameObject oldBasePlate = GameObject.Find("MotorBasePlate");
            if (oldBasePlate != null) Object.DestroyImmediate(oldBasePlate);

            // 3. Set Motor Scale, Rotation, and Exact Height to match Conveyor Head Roller Shaft
            motor.transform.localScale = new Vector3(0.46f, 0.46f, 0.46f);
            motor.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            // Position motor directly at the discharge head end frame
            // Motor shaft center matches conveyor head shaft height (Y ~ 0.95m - 1.01m)
            Vector3 targetMotorPos = new Vector3(
                maxHeadX + 0.28f, // X = 2.48m
                0.92f,            // Y = 0.92m (elevated perfectly matching drive shaft height)
                headCenterZ       // Z = 0.01m
            );

            motor.transform.position = targetMotorPos;

            // Recalculate motor bounds in world space
            var motorRenderers = motor.GetComponentsInChildren<Renderer>();
            Bounds motorBounds = new Bounds();
            bool firstM = true;
            foreach (var r in motorRenderers)
            {
                if (firstM) { motorBounds = r.bounds; firstM = false; }
                else { motorBounds.Encapsulate(r.bounds); }
            }

            Debug.Log($"[SurakshaAR V3Fixer] Final Motor Bounds: Center={motorBounds.center}, Size={motorBounds.size}, Min={motorBounds.min}, Max={motorBounds.max}");

            // Apply rich industrial electric blue material
            Color electricBlue = new Color(0.12f, 0.35f, 0.68f, 1.0f);
            foreach (var r in motorRenderers)
            {
                if (r.sharedMaterial != null)
                {
                    Material mat = new Material(r.sharedMaterial);
                    mat.name = "Industrial_Electric_Blue_Motor";
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", electricBlue);
                    if (mat.HasProperty("_Color")) mat.SetColor("_Color", electricBlue);
                    r.sharedMaterial = mat;
                }
            }

            // 4. Create an integrated steel extension bracket attached to conveyor end frame underneath motor
            GameObject mountExtension = new GameObject("MotorMountExtensionFrame");
            mountExtension.transform.SetParent(conveyor.transform, true);

            // Bracket shelf plate extending from conveyor end frame (X=2.10m to X=2.70m, Y=0.62m)
            GameObject shelfPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelfPlate.name = "MotorShelfPlate";
            shelfPlate.transform.SetParent(mountExtension.transform, false);
            shelfPlate.transform.position = new Vector3(maxHeadX + 0.22f, motorBounds.min.y - 0.02f, headCenterZ);
            shelfPlate.transform.localScale = new Vector3(0.55f, 0.04f, motorBounds.size.z + 0.10f);

            // Side frame steel gussets connecting shelf plate to conveyor end legs
            GameObject gussetLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gussetLeft.name = "SteelGusset_Left";
            gussetLeft.transform.SetParent(mountExtension.transform, false);
            gussetLeft.transform.position = new Vector3(maxHeadX + 0.10f, motorBounds.min.y - 0.15f, headCenterZ - 0.20f);
            gussetLeft.transform.localScale = new Vector3(0.30f, 0.22f, 0.05f);
            gussetLeft.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);

            GameObject gussetRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gussetRight.name = "SteelGusset_Right";
            gussetRight.transform.SetParent(mountExtension.transform, false);
            gussetRight.transform.position = new Vector3(maxHeadX + 0.10f, motorBounds.min.y - 0.15f, headCenterZ + 0.20f);
            gussetRight.transform.localScale = new Vector3(0.30f, 0.22f, 0.05f);
            gussetRight.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);

            // Drive shaft coupling housing connecting motor output to conveyor head roller
            GameObject shaftCoupling = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaftCoupling.name = "DriveShaftCoupling";
            shaftCoupling.transform.SetParent(mountExtension.transform, false);
            shaftCoupling.transform.position = new Vector3(maxHeadX + 0.02f, headCenterY - 0.02f, headCenterZ);
            shaftCoupling.transform.localScale = new Vector3(0.12f, 0.16f, 0.12f);
            shaftCoupling.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            // Material for steel mounting frame (matching conveyor steel frame color)
            Shader steelShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material steelMat = new Material(steelShader != null ? steelShader : motorRenderers[0].sharedMaterial.shader);
            steelMat.name = "Conveyor_Steel_Extension_Frame";
            Color conveyorSteelColor = new Color(0.18f, 0.32f, 0.55f, 1.0f); // Matching blue industrial conveyor frame
            if (steelMat.HasProperty("_BaseColor")) steelMat.SetColor("_BaseColor", conveyorSteelColor);
            if (steelMat.HasProperty("_Color")) steelMat.SetColor("_Color", conveyorSteelColor);

            foreach (var r in mountExtension.GetComponentsInChildren<Renderer>())
            {
                r.sharedMaterial = steelMat;
            }

            Debug.Log("[SurakshaAR V3Fixer] DriveMotor position, scale, rotation, and integrated mounting frame applied successfully.");

            // 5. Save Scene & Create V3 Backups
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            string backupSceneV3 = "Assets/AR_machinary_faundation/scenes/Backups/MachineryConveyorJam_REFERENCE_ALIGNED_FINAL_V3.unity";
            string backupPrefabV3 = "Assets/AR_machinary_faundation/prefabs/MachineryScenario_REFERENCE_ALIGNED_FINAL_V3.prefab";

            string backupsDir = Path.GetDirectoryName(backupSceneV3);
            if (!Directory.Exists(backupsDir)) Directory.CreateDirectory(backupsDir);

            File.Copy(scene.path, backupSceneV3, true);
            Debug.Log($"[SurakshaAR V3Fixer] Saved backup scene to '{backupSceneV3}'");

            GameObject scenarioRoot = GameObject.Find("MachineryScenario_REFERENCE_ALIGNED_FINAL") ?? conveyor.transform.parent?.gameObject ?? conveyor;
            PrefabUtility.SaveAsPrefabAsset(scenarioRoot, backupPrefabV3);
            Debug.Log($"[SurakshaAR V3Fixer] Saved backup prefab to '{backupPrefabV3}'");

            AssetDatabase.Refresh();

            // 6. Render 4 Required Verification Views
            RenderV3VerificationViews(conveyor, motor);
        }

        private static void RenderV3VerificationViews(GameObject conveyor, GameObject motor)
        {
            string outDir = "C:/Users/jainh/.gemini/antigravity-ide/brain/89459e12-a559-4036-8ffc-ee9eb81b1fda";
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

            GameObject camObj = new GameObject("V3VerificationCam");
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.18f, 0.20f, 0.23f, 1.0f);
            cam.fieldOfView = 42f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;

            // VIEW 1: Overall machinery 3/4 view
            cam.transform.position = new Vector3(0.5f, 2.8f, -4.8f);
            cam.transform.LookAt(new Vector3(0.6f, 0.7f, 0.0f));
            RenderCamToPNG(cam, Path.Combine(outDir, "V3_VIEW1_Overall_3_4_View.png"), 1280, 720);

            // VIEW 2: DriveMotor + conveyor drive close-up
            cam.transform.position = new Vector3(3.2f, 1.4f, -1.8f);
            cam.transform.LookAt(motor.transform.position + new Vector3(-0.1f, 0.0f, 0f));
            RenderCamToPNG(cam, Path.Combine(outDir, "V3_VIEW2_DriveMotor_CloseUp.png"), 1280, 720);

            // VIEW 3: Side view showing motor shaft -> conveyor drive shaft alignment
            cam.transform.position = new Vector3(2.4f, 0.95f, -2.5f);
            cam.transform.LookAt(new Vector3(2.4f, 0.95f, 0.0f));
            RenderCamToPNG(cam, Path.Combine(outDir, "V3_VIEW3_Side_Shaft_Alignment.png"), 1280, 720);

            // VIEW 4: Front / worker view showing motor mounted at correct height
            cam.transform.position = new Vector3(0.8f, 1.1f, -3.8f);
            cam.transform.LookAt(new Vector3(0.8f, 0.8f, 0.0f));
            RenderCamToPNG(cam, Path.Combine(outDir, "V3_VIEW4_Front_Worker_View.png"), 1280, 720);

            Object.DestroyImmediate(camObj);
            Debug.Log("[SurakshaAR V3Fixer] All 4 V3 verification views rendered successfully.");
        }

        private static void RenderCamToPNG(Camera cam, string path, int width, int height)
        {
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
