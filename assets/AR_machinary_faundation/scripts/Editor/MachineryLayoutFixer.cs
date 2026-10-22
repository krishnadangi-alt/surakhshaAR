#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryLayoutFixer
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string OutputFolder = "Builds/Verification/UnityScene";
        private const string ArtifactFolder = "C:/Users/jainh/.gemini/antigravity-ide/brain/6ce6aafa-fdea-48a4-9187-df89164857fe";

        [MenuItem("SurakshaAR/Machinery/Fix Master Layout Exact Blueprint")]
        public static void FixMasterLayoutZeroOverlap()
        {
            Debug.Log("[MachineryLayoutFixer] Executing Machinery Training Platform Perfect Hopper Filtering...");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[MachineryLayoutFixer] Failed to open scene at {ScenePath}");
                return;
            }

            // =========================================================================
            // 0. CLEAN UP SCENE ROOTS — DESTROY EXTRA / DUPLICATE ROOT OBJECTS
            // =========================================================================
            GameObject[] rootObjects = scene.GetRootGameObjects();
            GameObject masterRoot = null;

            foreach (var r in rootObjects)
            {
                if (r.name == "MachineryModelInspection")
                {
                    Debug.Log("[Cleanup] Disabling/Destroying extra MachineryModelInspection root object.");
                    r.SetActive(false);
                    Object.DestroyImmediate(r);
                }
                else if (r.name == "MachineryScenario")
                {
                    if (masterRoot == null)
                    {
                        masterRoot = r;
                    }
                    else
                    {
                        Debug.Log("[Cleanup] Destroying duplicate MachineryScenario root object.");
                        Object.DestroyImmediate(r);
                    }
                }
            }

            if (masterRoot == null)
            {
                masterRoot = new GameObject("MachineryScenario");
            }
            masterRoot.SetActive(true);

            // Wipe all children of masterRoot to ensure 100% clean rebuild
            List<GameObject> existingChildren = new List<GameObject>();
            foreach (Transform child in masterRoot.transform)
            {
                existingChildren.Add(child.gameObject);
            }
            foreach (var child in existingChildren)
            {
                Object.DestroyImmediate(child);
            }

            // Create LOTO Assembly parent
            GameObject lotoParent = new GameObject("LOTOAssembly");
            lotoParent.transform.SetParent(masterRoot.transform, false);
            lotoParent.transform.localPosition = Vector3.zero;
            lotoParent.transform.localEulerAngles = Vector3.zero;
            lotoParent.transform.localScale = Vector3.one;

            // Instantiate fresh 10 target objects
            GameObject conveyor = CreateObject(masterRoot, "ConveyorAssembly", "industrial_conveyor_belt");
            GameObject motor = CreateObject(masterRoot, "DriveMotor", "electric_motor");
            GameObject jam = CreateObject(masterRoot, "JamArea", "coal");
            GameObject hopper = CreateObject(masterRoot, "FeedHopper", "hopper");
            GameObject eStop = CreateObject(masterRoot, "EmergencyStop", "emergency_stop_button");
            GameObject isolator = CreateObject(masterRoot, "MainIsolator", "switch");
            GameObject panel = CreateObject(masterRoot, "ControlPanel", "industrial_control_panel_box");
            GameObject table = CreateObject(masterRoot, "SteelWorkstationTable", "industrial_steel_table");
            
            GameObject hasp = CreateObject(lotoParent, "Hasp", "hasp_and_staple");
            GameObject padlock = CreateObject(lotoParent, "PadlockAndTag", "lockout_tagout_locks_with_key_and_tag");

            // =========================================================================
            // 1. SURGICAL GEOMETRY FILTERING
            // =========================================================================
            CleanupUnwantedGeometry(conveyor, hopper, panel, table);

            // =========================================================================
            // 2. MASTER CONVEYOR ASSEMBLY
            // Upright long axis along WORLD X: rotation = (-90, 90, 0)
            // Shift position so conveyor CENTER is strictly at X=0, Z=0.
            // =========================================================================
            if (conveyor != null)
            {
                conveyor.transform.localPosition = new Vector3(0f, 0f, 0f);
                conveyor.transform.localEulerAngles = new Vector3(-90f, 90f, 0f);
                conveyor.transform.localScale = new Vector3(0.010f, 0.010f, 0.010f);
                GroundObject(conveyor, 0.0f);

                // Recenter conveyor mesh so bounds center is strictly X=0, Z=0
                Bounds rawCBounds = GetBounds(conveyor);
                conveyor.transform.position -= new Vector3(rawCBounds.center.x, 0f, rawCBounds.center.z);
            }

            Bounds cBounds = GetBounds(conveyor);
            float beltTopY = cBounds.max.y;
            Debug.Log($"[Conveyor Centered Bounds] Center={cBounds.center}, Size={cBounds.size}, Min={cBounds.min}, Max={cBounds.max}");

            // =========================================================================
            // 3. FEED HOPPER — LEFT SIDE OF CONVEYOR (-X)
            // Positioned directly over the feed end of conveyor with spout discharging onto belt.
            // =========================================================================
            if (hopper != null)
            {
                hopper.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
                hopper.transform.localScale = new Vector3(0.00045f, 0.00045f, 0.00045f);
                
                float hopperX = cBounds.min.x + 0.10f;
                hopper.transform.localPosition = new Vector3(hopperX, 0f, cBounds.center.z);
                
                Bounds hBounds = GetBounds(hopper);
                float yOffset = (beltTopY + 0.05f) - hBounds.min.y;
                hopper.transform.localPosition += new Vector3(0f, yOffset, 0f);
            }

            // =========================================================================
            // 4. JAM / COAL — CENTER OF CONVEYOR (X=0, Z=0, Y ON BELT)
            // Must sit directly ON top of the conveyor belt in exact middle section.
            // =========================================================================
            if (jam != null)
            {
                jam.transform.localEulerAngles = new Vector3(0f, 45f, 0f);
                jam.transform.localScale = new Vector3(0.022f, 0.022f, 0.022f);
                jam.transform.localPosition = new Vector3(cBounds.center.x, 0f, cBounds.center.z);
                
                Bounds jBounds = GetBounds(jam);
                float yOffset = beltTopY - jBounds.min.y;
                jam.transform.localPosition += new Vector3(0f, yOffset, 0f);
            }

            // =========================================================================
            // 5. DRIVE MOTOR — RIGHT SIDE OF CONVEYOR (+X)
            // Grounded and connected to RIGHT drive end of conveyor.
            // =========================================================================
            if (motor != null)
            {
                motor.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
                motor.transform.localScale = new Vector3(0.38f, 0.38f, 0.38f);
                
                float motorX = cBounds.max.x + 0.35f;
                motor.transform.localPosition = new Vector3(motorX, 0f, cBounds.center.z);
                GroundObject(motor, 0.0f);
            }

            // =========================================================================
            // 6, 7, 8. CONTROL / SAFETY ZONE — RIGHT-BACK (Z ≈ +1.8m)
            // Face FRONT (-Z).
            // Layout: [ MAIN ISOLATOR ] [ CONTROL PANEL ] [ EMERGENCY STOP ]
            // =========================================================================
            float safetyZ = +1.80f;

            // Control Panel (X ≈ +2.20m, Z = +1.80m)
            if (panel != null)
            {
                panel.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
                panel.transform.localScale = new Vector3(0.0008f, 0.0008f, 0.0008f);
                panel.transform.localPosition = new Vector3(2.20f, 0f, safetyZ);
                GroundObject(panel, 0.0f);
            }

            Bounds pBounds = GetBounds(panel);

            // Main Isolator (Left of Control Panel: X ≈ +1.10m)
            if (isolator != null)
            {
                isolator.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
                isolator.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
                isolator.transform.localPosition = new Vector3(1.10f, 0.85f, safetyZ);
            }

            // Emergency Stop (Right of Control Panel: X ≈ +3.30m)
            if (eStop != null)
            {
                eStop.transform.localEulerAngles = new Vector3(-90f, 180f, 0f);
                eStop.transform.localScale = new Vector3(0.0018f, 0.0018f, 0.0018f);
                eStop.transform.localPosition = new Vector3(3.30f, 0.95f, safetyZ);
            }

            // =========================================================================
            // 9, 10, 11. STEEL WORKSTATION TABLE & LOTO EQUIPMENT — RIGHT-FRONT (Z ≈ -1.8m)
            // Rotation (-90, 90, 0) makes table upright on its 4 legs!
            // =========================================================================
            float lotoZ = -1.80f;

            if (table != null)
            {
                table.transform.localEulerAngles = new Vector3(-90f, 90f, 0f);
                table.transform.localScale = new Vector3(0.010f, 0.010f, 0.010f);
                table.transform.localPosition = new Vector3(2.20f, 0f, lotoZ);
                GroundObject(table, 0.0f);
            }

            Bounds tBounds = GetBounds(table);
            float tableTopY = tBounds.max.y;
            Debug.Log($"[Table] Clean Bounds: center={tBounds.center}, size={tBounds.size}, min={tBounds.min}, max={tBounds.max}");

            // Hasp on left side of table top
            if (hasp != null)
            {
                hasp.transform.localEulerAngles = new Vector3(-90f, 90f, 0f);
                hasp.transform.localScale = new Vector3(0.0009f, 0.0009f, 0.0009f);
                hasp.transform.localPosition = new Vector3(tBounds.center.x - 0.40f, 0f, tBounds.center.z);
                Bounds hBounds = GetBounds(hasp);
                hasp.transform.localPosition += new Vector3(0f, tableTopY - hBounds.min.y + 0.005f, 0f);
            }

            // Padlock + Tag on right side of table top
            if (padlock != null)
            {
                padlock.transform.localEulerAngles = new Vector3(0f, 45f, 0f);
                padlock.transform.localScale = new Vector3(1.20f, 1.20f, 1.20f);
                padlock.transform.localPosition = new Vector3(tBounds.center.x + 0.40f, 0f, tBounds.center.z);
                Bounds pbBounds = GetBounds(padlock);
                padlock.transform.localPosition += new Vector3(0f, tableTopY - pbBounds.min.y + 0.005f, 0f);
            }

            // =========================================================================
            // CLEARANCE AUDIT LOG
            // =========================================================================
            AuditPair(conveyor, motor, "Conveyor ↔ Motor");
            AuditPair(conveyor, panel, "Conveyor ↔ ControlPanel");
            AuditPair(conveyor, table, "Conveyor ↔ SteelTable");
            AuditPair(panel, eStop, "ControlPanel ↔ EmergencyStop");
            AuditPair(panel, isolator, "ControlPanel ↔ MainIsolator");
            AuditPair(isolator, table, "MainIsolator ↔ SteelTable");
            AuditPair(hasp, padlock, "Hasp ↔ Padlock");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[MachineryLayoutFixer] MASTER LAYOUT UPDATED AND SAVED.");

            RenderFinalPNGs();
        }

        private static void CleanupUnwantedGeometry(GameObject conveyor, GameObject hopper, GameObject panel, GameObject table)
        {
            // 1. Conveyor: Keep ONLY renderers under 'Conveyor_belt'. Disable Sorting_Machine cabinet and Floor.
            if (conveyor != null)
            {
                int disabled = 0;
                foreach (var r in conveyor.GetComponentsInChildren<MeshRenderer>(true))
                {
                    string fullPath = GetHierarchyPath(r.transform, conveyor.transform);
                    if (!fullPath.Contains("Conveyor_belt") || fullPath.Contains("Floor") || fullPath.Contains("Vinyl"))
                    {
                        r.enabled = false;
                        disabled++;
                    }
                    else
                    {
                        r.enabled = true;
                    }
                }
                Debug.Log($"[Cleanup Conveyor] Disabled {disabled} non-conveyor renderers (sorting machine / floor).");
            }

            // 2. FeedHopper: Disable ONLY yellow catwalks/platforms/railings (Object_112, material_4, etc.). Keep hopper funnel cone.
            if (hopper != null)
            {
                int disabled = 0;
                foreach (var r in hopper.GetComponentsInChildren<MeshRenderer>(true))
                {
                    string fullPath = GetHierarchyPath(r.transform, hopper.transform);
                    string matName = (r.sharedMaterial != null) ? r.sharedMaterial.name : "";

                    // Disable Object_112 (catwalks/railings) and yellow/room materials material_4, material_6, material_7, material_8
                    bool isCatwalkOrRailing = fullPath.Contains("Object_112") || matName.Contains("material_4") || matName.Contains("material_6") || matName.Contains("material_7") || matName.Contains("material_8");

                    if (isCatwalkOrRailing)
                    {
                        r.enabled = false;
                        disabled++;
                    }
                    else
                    {
                        r.enabled = true;
                    }
                }
                Debug.Log($"[Cleanup FeedHopper] Disabled {disabled} catwalk/railing/room renderers.");
            }
        }

        private static string GetHierarchyPath(Transform t, Transform root)
        {
            string path = t.name;
            Transform curr = t.parent;
            while (curr != null && curr != root)
            {
                path = curr.name + "/" + path;
                curr = curr.parent;
            }
            return path;
        }

        private static GameObject CreateObject(GameObject parent, string nodeName, string glbName)
        {
            string glbPath = $"Assets/AR_machinary_faundation/3d/Raw/{glbName}.glb";
            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
            if (modelPrefab == null)
            {
                Debug.LogError($"[MachineryLayoutFixer] Missing GLB asset: {glbPath}");
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, parent.transform);
            instance.name = nodeName;
            instance.SetActive(true);
            return instance;
        }

        private static Bounds GetBounds(GameObject go)
        {
            if (go == null) return new Bounds();
            MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>(true);
            Bounds b = new Bounds();
            bool hasFirst = false;
            foreach (var r in renderers)
            {
                if (!r.enabled) continue;
                if (!hasFirst)
                {
                    b = r.bounds;
                    hasFirst = true;
                }
                else
                {
                    b.Encapsulate(r.bounds);
                }
            }
            return b;
        }

        private static void GroundObject(GameObject go, float groundY)
        {
            Bounds b = GetBounds(go);
            float yOffset = groundY - b.min.y;
            go.transform.position += new Vector3(0f, yOffset, 0f);
        }

        private static void AuditPair(GameObject a, GameObject b, string pairName)
        {
            if (a == null || b == null) return;
            Bounds bA = GetBounds(a);
            Bounds bB = GetBounds(b);
            bool intersects = bA.Intersects(bB);
            float dist = Vector3.Distance(bA.center, bB.center);
            Debug.Log($"[Clearance Audit] {pairName}: Intersects={intersects}, CenterDistance={dist:F2}m");
        }

        private static void RenderFinalPNGs()
        {
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }
            if (!Directory.Exists(ArtifactFolder))
            {
                Directory.CreateDirectory(ArtifactFolder);
            }

            GameObject mainCamGo = GameObject.FindWithTag("MainCamera");
            if (mainCamGo == null) mainCamGo = GameObject.Find("Main Camera");
            Camera mainCam = mainCamGo != null ? mainCamGo.GetComponent<Camera>() : Camera.main;

            GameObject tempCamGo = new GameObject("TempFinalRenderCam");
            Camera tempCam = tempCamGo.AddComponent<Camera>();
            if (mainCam != null) tempCam.CopyFrom(mainCam);

            tempCam.clearFlags = CameraClearFlags.SolidColor;
            tempCam.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1f);
            tempCam.nearClipPlane = 0.05f;
            tempCam.farClipPlane = 100f;
            tempCam.fieldOfView = 52f;

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

                // Save to Builds verification output
                string path1 = Path.Combine(OutputFolder, filename);
                File.WriteAllBytes(path1, bytes);

                // Save to artifact folder for user viewing
                string artifactPath = Path.Combine(ArtifactFolder, filename);
                File.WriteAllBytes(artifactPath, bytes);

                // Save to Machinery_Final_Training_Platform.png specifically when rendering the main view
                if (filename == "Machinery_Final_Training_Platform.png")
                {
                    string platformPath = Path.Combine(OutputFolder, "Machinery_Final_Training_Platform.png");
                    File.WriteAllBytes(platformPath, bytes);
                    string platformArtifactPath = Path.Combine(ArtifactFolder, "Machinery_Final_Training_Platform.png");
                    File.WriteAllBytes(platformArtifactPath, bytes);
                }

                Debug.Log($"[MachineryLayoutFixer] Rendered view: {filename} ({bytes.Length} bytes)");
            }

            // Primary Training Platform Perspective View: FRONT + SLIGHTLY ABOVE + SLIGHTLY SIDE
            // Eye at X=-0.2, Y=3.4, Z=-5.8 looking at center (X=0.2, Y=0.6, Z=0.0)
            RenderView(new Vector3(-0.2f, 3.4f, -5.8f), new Vector3(0.2f, 0.6f, 0.0f), "Machinery_Final_Training_Platform.png");

            // Additional Verification Renders
            RenderView(new Vector3(-0.2f, 3.4f, -5.8f), new Vector3(0.2f, 0.6f, 0.0f), "Overall_Machinery_FINAL.png");
            RenderView(new Vector3(0.0f, 1.8f, -3.2f), new Vector3(0.0f, 0.6f, 0.0f), "Conveyor_Zone_FINAL.png");
            RenderView(new Vector3(2.2f, 1.8f, -2.5f), new Vector3(2.2f, 0.8f, +1.8f), "Control_Safety_Zone_FINAL.png");
            RenderView(new Vector3(2.2f, 1.8f, -4.0f), new Vector3(2.2f, 0.8f, -1.8f), "LOTO_Zone_FINAL.png");

            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(tempCamGo);

            Debug.Log("[MachineryLayoutFixer] ALL FINAL TRAINING PLATFORM RENDERS GENERATED SUCCESSFULLY!");
        }
    }
}
#endif
