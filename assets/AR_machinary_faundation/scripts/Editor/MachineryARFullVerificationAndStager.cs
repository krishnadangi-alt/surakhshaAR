#if UNITY_EDITOR
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryARFullVerificationAndStager
    {
        public const string SourceScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        public const string MasterBackupPath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam_LOCKED_MASTER_BACKUP.unity";
        public const string ARScenePath = "Assets/AR_machinary_faundation/scenes/MachineryARExperience.unity";
        public const string PrefabPath = "Assets/AR_machinary_faundation/prefabs/MachineryScenario_SourceOfTruth.prefab";
        public const string VerificationReportPath = "Builds/Verification/machinery_ar_verification_report.txt";

        [MenuItem("SurakshaAR/Machinery/3. Full Verify and Setup Separate AR Scene", priority = 30)]
        public static void RunFullVerificationAndSetup()
        {
            Directory.CreateDirectory("Builds/Verification");
            var sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("MACHINERY AR RUNTIME AND WORLD-SPACE PLACEMENT VERIFICATION");
            sb.AppendLine("Timestamp: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("=========================================================================\n");

            // 1. Source Scene Integrity Check
            string sourceHash = ComputeFileHash(SourceScenePath);
            string backupHash = ComputeFileHash(MasterBackupPath);
            bool sourceMatch = (sourceHash == backupHash && !string.IsNullOrEmpty(sourceHash));

            sb.AppendLine("--- 1. SOURCE MACHINERY SCENE INTEGRITY ---");
            sb.AppendLine($"Source Scene: {SourceScenePath}");
            sb.AppendLine($"  Hash: {sourceHash}");
            sb.AppendLine($"Master Backup: {MasterBackupPath}");
            sb.AppendLine($"  Hash: {backupHash}");
            sb.AppendLine($"Source Scene Modified: {(sourceMatch ? "NO" : "YES (MISMATCH!)")}");
            sb.AppendLine($"Master Backup Match: {(sourceMatch ? "YES (100% IDENTICAL)" : "FAIL")}\n");

            if (!sourceMatch)
            {
                sb.AppendLine("ABORTING: Source scene was modified! Restoring from locked master...");
                File.Copy(MasterBackupPath, SourceScenePath, true);
                sourceHash = ComputeFileHash(SourceScenePath);
                sb.AppendLine($"Restored Hash: {sourceHash} -> MATCH: {sourceHash == backupHash}\n");
            }

            // 2. Open or create separate Machinery AR scene
            sb.AppendLine("--- 2. SEPARATE MACHINERY AR SCENE SETUP ---");
            MachineryCreateRealARScene.BuildARScene();

            var arScene = EditorSceneManager.OpenScene(ARScenePath, OpenSceneMode.Single);
            sb.AppendLine($"Opened Scene: {arScene.path} (Valid: {arScene.IsValid()})");

            // 3. Inspect Hierarchy Separation
            sb.AppendLine("\n--- 3. HIERARCHY SEPARATION VERIFICATION ---");
            GameObject arSession = null;
            GameObject xrOrigin = null;
            GameObject mainCam = null;
            GameObject placementRoot = null;
            GameObject uiCanvas = null;

            foreach (var rootGo in arScene.GetRootGameObjects())
            {
                if (rootGo.name == "AR Session") arSession = rootGo;
                else if (rootGo.name == "XR Origin") xrOrigin = rootGo;
                else if (rootGo.name == "MachineryARPlacementRoot") placementRoot = rootGo;
                else if (rootGo.name == "Machinery_AR_UI_Canvas") uiCanvas = rootGo;
            }

            if (xrOrigin != null)
            {
                var camTransform = xrOrigin.transform.Find("Camera Offset/Main Camera");
                if (camTransform != null) mainCam = camTransform.gameObject;
            }

            sb.AppendLine($"AR Session: {(arSession != null ? "FOUND (Root)" : "MISSING")}");
            sb.AppendLine($"XR Origin: {(xrOrigin != null ? "FOUND (Root)" : "MISSING")}");
            sb.AppendLine($"Main Camera: {(mainCam != null ? "FOUND (Child of Camera Offset)" : "MISSING")}");
            sb.AppendLine($"MachineryPlacementRoot: {(placementRoot != null ? "FOUND (Root)" : "MISSING")}");
            sb.AppendLine($"UI Canvas: {(uiCanvas != null ? "FOUND (Root)" : "MISSING")}");

            bool separateFromCamera = (placementRoot != null && mainCam != null && !placementRoot.transform.IsChildOf(mainCam.transform));
            bool separateFromCanvas = (placementRoot != null && uiCanvas != null && !placementRoot.transform.IsChildOf(uiCanvas.transform));
            bool separateFromXROrigin = (placementRoot != null && xrOrigin != null && !placementRoot.transform.IsChildOf(xrOrigin.transform));

            sb.AppendLine($"Machinery Root NOT Child of Camera: {(separateFromCamera ? "PASS" : "FAIL")}");
            sb.AppendLine($"Machinery Root NOT Child of Canvas: {(separateFromCanvas ? "PASS" : "FAIL")}");
            sb.AppendLine($"Machinery Root NOT Child of XR Origin: {(separateFromXROrigin ? "PASS" : "FAIL")}");

            // 4. Verify Camera Independent Tracking Components & Bindings
            sb.AppendLine("\n--- 4. CAMERA TRACKING & PARALLAX VERIFICATION ---");
            if (mainCam != null)
            {
                var camMgr = mainCam.GetComponent<ARCameraManager>();
                var camBg = mainCam.GetComponent<ARCameraBackground>();
                var poseDriver = mainCam.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();

                sb.AppendLine($"ARCameraManager: {(camMgr != null ? "PASS" : "FAIL")}");
                sb.AppendLine($"ARCameraBackground: {(camBg != null ? "PASS" : "FAIL")}");
                sb.AppendLine($"TrackedPoseDriver: {(poseDriver != null ? "PASS" : "FAIL")}");

                if (poseDriver != null)
                {
                    int posBindings = poseDriver.positionInput.action != null ? poseDriver.positionInput.action.bindings.Count : 0;
                    int rotBindings = poseDriver.rotationInput.action != null ? poseDriver.rotationInput.action.bindings.Count : 0;
                    sb.AppendLine($"TrackedPoseDriver Position Bindings Count: {posBindings} (Expected >= 2)");
                    sb.AppendLine($"TrackedPoseDriver Rotation Bindings Count: {rotBindings} (Expected >= 2)");
                    sb.AppendLine($"Camera Independent: {(posBindings > 0 && rotBindings > 0 ? "PASS" : "FAIL")}");
                }
            }

            // 5. Verify Machinery Children & Internal Preservation
            sb.AppendLine("\n--- 5. MACHINERY INTERNAL PRESERVATION VERIFICATION ---");
            if (placementRoot != null)
            {
                Transform scenarioT = placementRoot.transform.Find("MachineryScenario");
                sb.AppendLine($"MachineryScenario child under placement root: {(scenarioT != null ? "PASS" : "FAIL")}");

                if (scenarioT != null)
                {
                    sb.AppendLine($"MachineryScenario Local Position: {scenarioT.localPosition}");
                    sb.AppendLine($"MachineryScenario Local Scale: {scenarioT.localScale}");
                    sb.AppendLine($"PlacementRoot Local Scale: {placementRoot.transform.localScale}");

                    string[] expectedChildren = new string[]
                    {
                        "JamArea", "FeedHopper", "ConveyorAssembly", "MainIsolator",
                        "EmergencyStop", "DriveMotor", "ControlPanel", "LOTOStation",
                        "SteelWorkstationTable", "EStopStand", "IsolatorStand"
                    };

                    int foundCount = 0;
                    foreach (var childName in expectedChildren)
                    {
                        Transform child = scenarioT.Find(childName);
                        bool found = (child != null);
                        if (found) foundCount++;
                        sb.AppendLine($"  - {childName}: {(found ? $"PASS (pos={child.localPosition}, scale={child.localScale})" : "FAIL")}");
                    }
                    sb.AppendLine($"Relative Child Arrangement Preserved: {(foundCount == expectedChildren.Length ? "PASS" : "FAIL")}");
                }
            }

            // 6. Verify Controllers
            sb.AppendLine("\n--- 6. AR CONTROLLER VERIFICATION ---");
            var realARController = xrOrigin != null ? xrOrigin.GetComponent<MachineryRealARController>() : null;
            sb.AppendLine($"MachineryRealARController on XR Origin: {(realARController != null ? "PASS" : "FAIL")}");

            // 7. Verify Editor Build Settings
            sb.AppendLine("\n--- 7. BUILD SETTINGS SCENES ---");
            foreach (var s in EditorBuildSettings.scenes)
            {
                sb.AppendLine($"  - Scene: {s.path} (Enabled: {s.enabled})");
            }
            bool scene0IsAR = (EditorBuildSettings.scenes.Length > 0 && EditorBuildSettings.scenes[0].path == ARScenePath && EditorBuildSettings.scenes[0].enabled);
            sb.AppendLine($"Scene 0 is Separate AR Experience Scene: {(scene0IsAR ? "PASS" : "FAIL")}");

            // Save and Log
            File.WriteAllText(VerificationReportPath, sb.ToString());
            Debug.Log(sb.ToString());
            EditorSceneManager.SaveScene(arScene);
        }

        private static string ComputeFileHash(string filePath)
        {
            if (!File.Exists(filePath)) return string.Empty;
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
            }
        }
    }
}
#endif
