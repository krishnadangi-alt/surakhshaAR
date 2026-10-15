#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryMasterV5Fixer
    {
        [MenuItem("SurakshaAR/Machinery/Execute Master V5 Realistic Scene Fix")]
        public static void ExecuteV5MasterFix()
        {
            // 1. Create Pre-Fix Backup Scene: MachineryConveyorJam_BEFORE_REALISTIC_FINAL_FIX_BACKUP.unity
            string workingScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
            string preFixBackupPath = "Assets/AR_machinary_faundation/scenes/Backups/MachineryConveyorJam_BEFORE_REALISTIC_FINAL_FIX_BACKUP.unity";

            if (File.Exists(workingScenePath))
            {
                string backupDir = Path.GetDirectoryName(preFixBackupPath);
                if (!Directory.Exists(backupDir)) Directory.CreateDirectory(backupDir);
                File.Copy(workingScenePath, preFixBackupPath, true);
                Debug.Log($"[SurakshaAR V5Fixer] Created pre-fix backup at '{preFixBackupPath}'.");

                EditorSceneManager.OpenScene(workingScenePath, OpenSceneMode.Single);
            }

            var scene = EditorSceneManager.GetActiveScene();
            Debug.Log($"[SurakshaAR V5Fixer] Executing Master V5 Fix on scene: {scene.path}");

            // Ensure parent root MachineryScenario exists with identity transform (0,0,0, rot 0, scale 1,1,1)
            GameObject scenarioRoot = GameObject.Find("MachineryScenario");
            if (scenarioRoot == null)
            {
                scenarioRoot = GameObject.Find("MachineryScenario_REFERENCE_ALIGNED_FINAL") ?? new GameObject("MachineryScenario");
                scenarioRoot.name = "MachineryScenario";
            }

            scenarioRoot.transform.position = Vector3.zero;
            scenarioRoot.transform.rotation = Quaternion.identity;
            scenarioRoot.transform.localScale = Vector3.one;

            // 2. Locate / Organize Primary Machinery GameObjects
            GameObject conveyor = FindOrCreateGameObject("ConveyorAssembly", "industrial_conveyor_belt", scenarioRoot);
            GameObject hopper = FindOrCreateGameObject("FeedHopper", "feed_hopper", scenarioRoot);
            GameObject motor = FindOrCreateGameObject("DriveMotor", "electric_motor", scenarioRoot);
            GameObject jamArea = GameObject.Find("JamArea") ?? new GameObject("JamArea");
            jamArea.transform.SetParent(scenarioRoot.transform, true);

            GameObject controlPanel = FindOrCreateGameObject("ControlPanel", "control_panel", scenarioRoot);
            GameObject eStop = FindOrCreateGameObject("EmergencyStop", "emergency_stop", scenarioRoot);
            GameObject isolator = FindOrCreateGameObject("MainIsolator", "main_isolator", scenarioRoot);
            GameObject table = FindOrCreateGameObject("SteelWorkstationTable", "steel_table", scenarioRoot);
            GameObject hasp = FindOrCreateGameObject("Hasp", "hasp", scenarioRoot);
            GameObject padlockTag = FindOrCreateGameObject("PadlockAndTag", "padlock", scenarioRoot);

            CleanDuplicates(scenarioRoot);

            // 3. MASTER OBJECT GEOMETRY ANALYSIS — CONVEYOR ASSEMBLY
            var conveyorRenderers = conveyor.GetComponentsInChildren<Renderer>();
            Bounds conveyorBounds = CalculateGroupBounds(conveyorRenderers);

            float minX = conveyorBounds.min.x; // Feed end (~ -1.80m)
            float maxX = conveyorBounds.max.x; // Discharge head (~ +2.24m)
            float beltTopY = conveyorBounds.max.y - 0.04f; // Belt surface height (~ 1.10m)
            float driveShaftY = conveyorBounds.min.y + conveyorBounds.size.y * 0.85f; // Head shaft height (~ 1.01m)
            float driveShaftZ = conveyorBounds.center.z; // Centerline Z (~ 0.01m)

            Debug.Log($"[SurakshaAR V5Fixer] Conveyor Bounds: MinX={minX:F2}, MaxX={maxX:F2}, BeltTopY={beltTopY:F2}, ShaftY={driveShaftY:F2}");

            // 4. DRIVE MOTOR FIX — MECHANICAL SHAFT ALIGNMENT & EXTENSION FRAME
            Transform oldMount = motor.transform.Find("MotorMountFrame") ?? motor.transform.Find("MotorMountExtensionFrame");
            if (oldMount != null) Object.DestroyImmediate(oldMount.gameObject);

            motor.transform.localScale = new Vector3(0.46f, 0.46f, 0.46f);
            motor.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            motor.transform.position = new Vector3(maxX + 0.28f, 0.92f, driveShaftZ);

            var motorRenderers = motor.GetComponentsInChildren<Renderer>();
            Bounds motorBounds = CalculateGroupBounds(motorRenderers);

            Color electricBlue = new Color(0.12f, 0.35f, 0.68f, 1.0f);
            ApplyColorToRenderers(motorRenderers, electricBlue, "Industrial_Electric_Blue_Motor");

            // Integrated steel extension shelf frame attached directly to conveyor end frame
            GameObject mountFrame = new GameObject("MotorMountExtensionFrame");
            mountFrame.transform.SetParent(conveyor.transform, true);

            GameObject shelfPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelfPlate.name = "MotorShelfPlate";
            shelfPlate.transform.SetParent(mountFrame.transform, false);
            shelfPlate.transform.position = new Vector3(maxX + 0.22f, motorBounds.min.y - 0.02f, driveShaftZ);
            shelfPlate.transform.localScale = new Vector3(0.55f, 0.04f, motorBounds.size.z + 0.10f);

            GameObject shaftCoupling = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaftCoupling.name = "DriveShaftCoupling";
            shaftCoupling.transform.SetParent(mountFrame.transform, false);
            shaftCoupling.transform.position = new Vector3(maxX + 0.02f, driveShaftY - 0.02f, driveShaftZ);
            shaftCoupling.transform.localScale = new Vector3(0.12f, 0.16f, 0.12f);
            shaftCoupling.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            Shader steelShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material steelMat = new Material(steelShader != null ? steelShader : motorRenderers[0].sharedMaterial.shader);
            Color conveyorSteelColor = new Color(0.18f, 0.32f, 0.55f, 1.0f);
            if (steelMat.HasProperty("_BaseColor")) steelMat.SetColor("_BaseColor", conveyorSteelColor);
            if (steelMat.HasProperty("_Color")) steelMat.SetColor("_Color", conveyorSteelColor);
            foreach (var r in mountFrame.GetComponentsInChildren<Renderer>()) r.sharedMaterial = steelMat;

            // 5. FEED HOPPER FIX — Centered over feed end, galvanized finish
            hopper.transform.position = new Vector3(minX + 0.60f, 0.85f, driveShaftZ);
            var hopperRenderers = hopper.GetComponentsInChildren<Renderer>();
            Color galvanizedSteel = new Color(0.72f, 0.74f, 0.76f, 1.0f);
            ApplyColorToRenderers(hopperRenderers, galvanizedSteel, "Galvanized_Hopper_Steel");

            // 6. COAL ON THE BELT FIX — Sitting physically ON top of belt surface
            List<GameObject> coalPieces = new List<GameObject>();
            foreach (Transform child in jamArea.transform) coalPieces.Add(child.gameObject);
            Renderer[] directCoal = jamArea.GetComponentsInChildren<Renderer>();
            foreach (var r in directCoal) if (!coalPieces.Contains(r.gameObject)) coalPieces.Add(r.gameObject);

            float startX = minX + 0.50f;
            float endX = maxX - 0.30f;
            int count = coalPieces.Count;

            for (int i = 0; i < count; i++)
            {
                GameObject coal = coalPieces[i];
                float t = (float)i / Mathf.Max(1, count - 1);
                float posX = Mathf.Lerp(startX, endX, t);

                // Jam accumulation density towards 3/4 point of conveyor
                if (i > count * 0.6f && i < count * 0.9f)
                {
                    posX += Random.Range(-0.15f, 0.15f);
                }

                float posZ = driveShaftZ + Random.Range(-0.25f, 0.25f);

                var coalR = coal.GetComponent<Renderer>() ?? coal.GetComponentInChildren<Renderer>();
                float coalHeight = coalR != null ? coalR.bounds.size.y : 0.08f;
                float posY = beltTopY + (coalHeight * 0.45f) + 0.01f;

                coal.transform.position = new Vector3(posX, posY, posZ);
                coal.transform.rotation = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0f, 360f), Random.Range(-15f, 15f));
            }

            Debug.Log($"[SurakshaAR V5Fixer] Positioned {count} coal pieces sitting physically ON the belt surface.");

            // 7. CONTROL PANEL — STANDALONE INDUSTRIAL CABINET
            foreach (Transform child in controlPanel.transform)
            {
                if (child.name.Contains("Conveyor") || child.name.Contains("Motor") || child.name.Contains("Slab"))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            controlPanel.transform.localScale = new Vector3(0.42f, 0.60f, 0.85f);
            controlPanel.transform.position = new Vector3(1.95f, 0.85f, +1.35f);
            controlPanel.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            Color industrialBeige = new Color(0.92f, 0.90f, 0.86f, 1.0f);
            ApplyColorToRenderers(controlPanel.GetComponentsInChildren<Renderer>(), industrialBeige, "Industrial_Beige_Cabinet");

            // 8. EMERGENCY STOP & MAIN ISOLATOR — Coherent Pedestal Safety Station
            eStop.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            eStop.transform.position = new Vector3(3.10f, 1.15f, +1.35f);
            eStop.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            EnsurePedestal(eStop, 1.15f);

            isolator.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            isolator.transform.position = new Vector3(3.90f, 1.15f, +1.35f);
            isolator.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            EnsurePedestal(isolator, 1.15f);

            // 9. STEEL WORKSTATION TABLE & LOTO EQUIPMENT ON TABLETOP SURFACE
            table.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            table.transform.position = new Vector3(3.90f, 0.00f, -1.35f);
            table.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            var tableRenderers = table.GetComponentsInChildren<Renderer>();
            Bounds tableBounds = CalculateGroupBounds(tableRenderers);
            float tableTopY = tableBounds.max.y > 0.1f ? tableBounds.max.y : 0.78f;

            hasp.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
            var haspR = hasp.GetComponent<Renderer>() ?? hasp.GetComponentInChildren<Renderer>();
            float haspHeight = haspR != null ? haspR.bounds.size.y : 0.04f;
            hasp.transform.position = new Vector3(3.70f, tableTopY + (haspHeight * 0.5f) + 0.01f, -1.30f);
            hasp.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            Color brightRed = new Color(0.85f, 0.12f, 0.12f, 1.0f);
            ApplyColorToRenderers(hasp.GetComponentsInChildren<Renderer>(), brightRed, "Safety_Red_Hasp");

            padlockTag.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
            var padR = padlockTag.GetComponent<Renderer>() ?? padlockTag.GetComponentInChildren<Renderer>();
            float padHeight = padR != null ? padR.bounds.size.y : 0.04f;
            padlockTag.transform.position = new Vector3(4.10f, tableTopY + (padHeight * 0.5f) + 0.01f, -1.35f);
            padlockTag.transform.rotation = Quaternion.Euler(0f, 45f, 0f);

            Debug.Log($"[SurakshaAR V5Fixer] Hasp and PadlockAndTag positioned physically ON top of tabletop surface (Y={tableTopY:F2}).");

            // 10. Save Active Scene & Create V5 Approved Scene/Prefab Backups
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            string v5BackupScene = "Assets/AR_machinary_faundation/scenes/Backups/MachineryConveyorJam_REFERENCE_ALIGNED_FINAL_V5.unity";
            string v5BackupPrefab = "Assets/AR_machinary_faundation/prefabs/MachineryScenario_REFERENCE_ALIGNED_FINAL_V5.prefab";

            File.Copy(workingScenePath, v5BackupScene, true);
            Debug.Log($"[SurakshaAR V5Fixer] Saved V5 backup scene to '{v5BackupScene}'.");

            PrefabUtility.SaveAsPrefabAsset(scenarioRoot, v5BackupPrefab);
            Debug.Log($"[SurakshaAR V5Fixer] Saved V5 backup prefab to '{v5BackupPrefab}'.");

            AssetDatabase.Refresh();

            // 11. Render 6 Required Verification Views
            RenderV5VerificationViews(scenarioRoot, conveyor, motor, controlPanel, table);
        }

        private static GameObject FindOrCreateGameObject(string name, string altSearch, GameObject parent)
        {
            GameObject obj = GameObject.Find(name);
            if (obj == null) obj = GameObject.Find(altSearch);
            if (obj == null) obj = new GameObject(name);
            obj.name = name;
            obj.transform.SetParent(parent.transform, true);
            return obj;
        }

        private static Bounds CalculateGroupBounds(Renderer[] renderers)
        {
            Bounds b = new Bounds();
            bool first = true;
            foreach (var r in renderers)
            {
                if (first) { b = r.bounds; first = false; }
                else { b.Encapsulate(r.bounds); }
            }
            return b;
        }

        private static void ApplyColorToRenderers(Renderer[] renderers, Color col, string matName)
        {
            foreach (var r in renderers)
            {
                if (r.sharedMaterial != null)
                {
                    Material mat = new Material(r.sharedMaterial);
                    mat.name = matName;
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);
                    if (mat.HasProperty("_Color")) mat.SetColor("_Color", col);
                    r.sharedMaterial = mat;
                }
            }
        }

        private static void EnsurePedestal(GameObject target, float height)
        {
            Transform pedestal = target.transform.Find("SafetyPedestal");
            if (pedestal == null)
            {
                GameObject pedObj = new GameObject("SafetyPedestal");
                pedObj.transform.SetParent(target.transform, false);

                GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.transform.SetParent(pedObj.transform, false);
                pole.transform.localPosition = new Vector3(0f, -height * 0.5f, 0f);
                pole.transform.localScale = new Vector3(0.06f, height * 0.5f, 0.06f);

                GameObject baseFlange = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                baseFlange.transform.SetParent(pedObj.transform, false);
                baseFlange.transform.localPosition = new Vector3(0f, -height + 0.02f, 0f);
                baseFlange.transform.localScale = new Vector3(0.25f, 0.02f, 0.25f);

                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                Material mat = new Material(shader != null ? shader : target.GetComponentInChildren<Renderer>().sharedMaterial.shader);
                Color darkSteel = new Color(0.25f, 0.28f, 0.30f, 1.0f);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", darkSteel);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", darkSteel);

                pole.GetComponent<Renderer>().sharedMaterial = mat;
                baseFlange.GetComponent<Renderer>().sharedMaterial = mat;
            }
        }

        private static void CleanDuplicates(GameObject root)
        {
            var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            HashSet<string> seenNames = new HashSet<string>();

            foreach (var go in allObjects)
            {
                if (go.transform.parent == root.transform)
                {
                    if (seenNames.Contains(go.name))
                    {
                        Debug.Log($"[SurakshaAR V5Fixer] Removing duplicate GameObject '{go.name}'");
                        Object.DestroyImmediate(go);
                    }
                    else
                    {
                        seenNames.Add(go.name);
                    }
                }
            }
        }

        private static void RenderV5VerificationViews(GameObject root, GameObject conveyor, GameObject motor, GameObject controlPanel, GameObject table)
        {
            string outDir = "C:/Users/jainh/.gemini/antigravity-ide/brain/89459e12-a559-4036-8ffc-ee9eb81b1fda";
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

            GameObject camObj = new GameObject("V5VerificationCam");
            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.18f, 0.20f, 0.23f, 1.0f);
            cam.fieldOfView = 45f;

            // VIEW 1: Overall 3/4 scene
            cam.transform.position = new Vector3(1.2f, 3.2f, -5.5f);
            cam.transform.LookAt(new Vector3(1.2f, 0.8f, 0.0f));
            RenderCamToPNG(cam, Path.Combine(outDir, "V5_VIEW1_Overall_3_4_Scene.png"), 1280, 720);

            // VIEW 2: Conveyor + Hopper + Coal close-up
            cam.transform.position = new Vector3(-0.2f, 1.8f, -3.2f);
            cam.transform.LookAt(new Vector3(-0.2f, 0.95f, 0.0f));
            RenderCamToPNG(cam, Path.Combine(outDir, "V5_VIEW2_Conveyor_Hopper_Coal_CloseUp.png"), 1280, 720);

            // VIEW 3: DriveMotor + Conveyor drive-end close-up
            cam.transform.position = new Vector3(3.2f, 1.4f, -1.8f);
            cam.transform.LookAt(motor.transform.position);
            RenderCamToPNG(cam, Path.Combine(outDir, "V5_VIEW3_DriveMotor_ConveyorDrive_CloseUp.png"), 1280, 720);

            // VIEW 4: ControlPanel + EmergencyStop + MainIsolator (Worker front view)
            cam.transform.position = new Vector3(2.9f, 1.4f, -1.5f);
            cam.transform.LookAt(new Vector3(2.9f, 1.15f, 1.35f));
            RenderCamToPNG(cam, Path.Combine(outDir, "V5_VIEW4_SafetyStation_ControlPanel_EStop_Isolator.png"), 1280, 720);

            // VIEW 5: SteelWorkstationTable + Hasp + Padlock + Tag
            cam.transform.position = new Vector3(3.9f, 1.4f, -2.8f);
            cam.transform.LookAt(table.transform.position + new Vector3(0f, 0.78f, 0f));
            RenderCamToPNG(cam, Path.Combine(outDir, "V5_VIEW5_SteelWorkstationTable_LOTO.png"), 1280, 720);

            // VIEW 6: Full worker-facing mobile view
            cam.transform.position = new Vector3(1.5f, 1.4f, -4.8f);
            cam.transform.LookAt(new Vector3(1.5f, 0.8f, 0.0f));
            RenderCamToPNG(cam, Path.Combine(outDir, "V5_VIEW6_Full_Worker_Facing_View.png"), 1280, 720);

            Object.DestroyImmediate(camObj);
            Debug.Log("[SurakshaAR V5Fixer] All 6 V5 verification views rendered successfully.");
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
