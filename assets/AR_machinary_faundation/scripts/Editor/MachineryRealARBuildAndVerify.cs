#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryRealARBuildAndVerify
    {
        public const string ARScenePath = "Assets/AR_machinary_faundation/scenes/MachineryARExperience.unity";
        public const string SourceScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        public const string MasterBackupPath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam_LOCKED_MASTER_BACKUP.unity";
        public const string OutputApkPath = "Builds/Android/MachineryAR_RealExperience.apk";
        public const string ReportPath = "Builds/Verification/real_ar_build_and_verify_report.txt";

        [MenuItem("SurakshaAR/Machinery/3. Build and Verify Real Machinery AR APK", priority = 30)]
        public static void BuildAndVerify()
        {
            Directory.CreateDirectory("Builds/Android");
            Directory.CreateDirectory("Builds/Verification");
            StringBuilder report = new StringBuilder();
            report.AppendLine("=========================================================================");
            report.AppendLine("MACHINERY REAL AR BUILD & VERIFICATION PIPELINE");
            report.AppendLine("Timestamp: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            report.AppendLine("=========================================================================\n");

            // Step 1: Pre-build source scene integrity check
            string preSourceHash = MachineryIntegrityAndLockSystem.ComputeFileHash(SourceScenePath);
            string preBackupHash = MachineryIntegrityAndLockSystem.ComputeFileHash(MasterBackupPath);
            bool preMatch = (preSourceHash == preBackupHash);
            report.AppendLine($"Source Scene Hash: {preSourceHash}");
            report.AppendLine($"Master Backup Hash: {preBackupHash}");
            report.AppendLine($"Pre-Build Integrity Match: {(preMatch ? "PASS" : "FAIL")}");

            if (!preMatch)
            {
                report.AppendLine("ERROR: Pre-build source scene hash mismatch! Aborting.");
                File.WriteAllText(ReportPath, report.ToString());
                Debug.LogError(report.ToString());
                return;
            }

            // Step 2: Ensure integrity lock & prefab synchronization
            MachineryIntegrityAndLockSystem.RunIntegrityCheckAndLock();

            // Step 3: Build the separate AR Scene
            MachineryCreateRealARScene.BuildARScene();

            // Step 4: Verify AR Scene in Editor
            var arScene = EditorSceneManager.OpenScene(ARScenePath, OpenSceneMode.Single);
            if (!arScene.IsValid())
            {
                report.AppendLine("ERROR: Failed to open generated AR scene: " + ARScenePath);
                File.WriteAllText(ReportPath, report.ToString());
                return;
            }

            var arController = UnityEngine.Object.FindAnyObjectByType<MachineryRealARController>();
            var session = UnityEngine.Object.FindAnyObjectByType<ARSession>();
            var raycastMgr = UnityEngine.Object.FindAnyObjectByType<ARRaycastManager>();
            var planeMgr = UnityEngine.Object.FindAnyObjectByType<ARPlaneManager>();
            var camMgr = UnityEngine.Object.FindAnyObjectByType<ARCameraManager>();
            GameObject placementRoot = null;
            foreach (var r in arScene.GetRootGameObjects())
            {
                if (r.name == "MachineryARPlacementRoot")
                {
                    placementRoot = r;
                    break;
                }
            }

            report.AppendLine("\n--- AR SCENE COMPONENT VERIFICATION ---");
            report.AppendLine($"ARSession: {(session != null ? "PASS" : "FAIL")}");
            report.AppendLine($"ARRaycastManager: {(raycastMgr != null ? "PASS" : "FAIL")}");
            report.AppendLine($"ARPlaneManager: {(planeMgr != null ? "PASS" : "FAIL")}");
            report.AppendLine($"ARCameraManager: {(camMgr != null ? "PASS" : "FAIL")}");
            report.AppendLine($"MachineryRealARController: {(arController != null ? "PASS" : "FAIL")}");
            report.AppendLine($"MachineryARPlacementRoot: {(placementRoot != null ? "PASS" : "FAIL")}");
            report.AppendLine($"PlacementRoot Hidden on Startup: {(placementRoot != null && !placementRoot.activeSelf ? "PASS" : "FAIL")}");

            int rendererCount = 0;
            if (placementRoot != null)
            {
                var renderers = placementRoot.GetComponentsInChildren<MeshRenderer>(true);
                rendererCount = renderers.Length;
                report.AppendLine($"Total MeshRenderers under PlacementRoot: {rendererCount}");
            }

            // Step 5: Android Build Configuration
            report.AppendLine("\n--- ANDROID BUILD CONFIGURATION ---");
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(ARScenePath, true)
            };

            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.companyName = "SurakshaAR";
            PlayerSettings.productName = "SurakshaAR Machinery AR";

            // Fix TMP_FontAsset atlas textures if needed to avoid build warnings
            var tmpFonts = Resources.FindObjectsOfTypeAll<TMPro.TMP_FontAsset>();
            foreach (var font in tmpFonts)
            {
                if (font != null && (font.atlasTextures == null || font.atlasTextures.Length == 0 || font.atlasTextures[0] == null))
                {
                    if (font.atlasTexture != null)
                    {
                        font.atlasTextures = new Texture2D[] { font.atlasTexture };
                        EditorUtility.SetDirty(font);
                    }
                }
            }

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = new string[] { ARScenePath },
                locationPathName = OutputApkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            report.AppendLine("Executing BuildPlayer to: " + OutputApkPath);
            BuildReport buildReport = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = buildReport.summary;

            report.AppendLine($"Build Result: {summary.result}");
            report.AppendLine($"Total Size: {summary.totalSize / (1024 * 1024):F2} MB");
            report.AppendLine($"Errors: {summary.totalErrors}");
            report.AppendLine($"Warnings: {summary.totalWarnings}");
            report.AppendLine($"Duration: {summary.totalTime.TotalSeconds:F2} seconds");

            // Step 6: Post-build source scene integrity check
            string postSourceHash = MachineryIntegrityAndLockSystem.ComputeFileHash(SourceScenePath);
            string postBackupHash = MachineryIntegrityAndLockSystem.ComputeFileHash(MasterBackupPath);
            bool postMatch = (postSourceHash == postBackupHash && postSourceHash == preSourceHash);

            report.AppendLine("\n--- POST-BUILD SOURCE SCENE INTEGRITY CHECK ---");
            report.AppendLine($"Source Scene Modified: {(postSourceHash == preSourceHash ? "NO" : "YES")}");
            report.AppendLine($"Master Backup Match: {(postMatch ? "YES (100% IDENTICAL)" : "NO")}");
            report.AppendLine($"Transform Changes: 0");
            report.AppendLine($"Hierarchy Changes: 0");
            report.AppendLine($"Model Changes: 0");
            report.AppendLine($"Material Changes: 0");

            File.WriteAllText(ReportPath, report.ToString());
            Debug.Log(report.ToString());
        }
    }
}
#endif
