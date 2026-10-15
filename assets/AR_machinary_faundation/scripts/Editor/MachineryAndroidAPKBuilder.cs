#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryAndroidAPKBuilder
    {
        private const string MainScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string BackupScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam_LOCKED_BACKUP.unity";
        private const string OutputApkPath = "scratch/MachineryAR_Test.apk";
        private const string LogPath = "scratch/apk_build_log.txt";

        [MenuItem("SurakshaAR/Machinery/Build Machinery AR Android APK Direct")]
        public static void BuildMachineryAPK()
        {
            Directory.CreateDirectory("scratch");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("SurakshaAR — DEDICATED MACHINERY AR ANDROID APK BUILD");
            sb.AppendLine("Timestamp: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("=========================================================================\n");

            // 1. Ensure Backup Exists
            if (!File.Exists(BackupScenePath))
            {
                File.Copy(MainScenePath, BackupScenePath, true);
                sb.AppendLine($"[Backup Created] {BackupScenePath}");
            }
            else
            {
                sb.AppendLine($"[Backup Verified] {BackupScenePath}");
            }

            // 2. Run AR Scene Setup Fix (Camera Tagging & Plane Manager) & Open Primary Scene
            MachinerySetupARSceneFix.FixARSceneSetup();
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            }

            // Verify MachineryScenario root exists
            GameObject masterRoot = null;
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == "MachineryScenario")
                {
                    masterRoot = go;
                    break;
                }
            }

            if (masterRoot == null)
            {
                sb.AppendLine("CRITICAL ERROR: MachineryScenario root not found in scene!");
                File.WriteAllText(LogPath, sb.ToString());
                return;
            }

            sb.AppendLine($"[MachineryScenario Verified] Child Count: {masterRoot.transform.childCount}");
            sb.AppendLine($"  Root Position: {masterRoot.transform.position}");
            sb.AppendLine($"  Root Rotation: {masterRoot.transform.eulerAngles}");
            sb.AppendLine($"  Root Scale: {masterRoot.transform.localScale}");

            // Ensure MachineryScenario root is saved inactive so it never shows before placement
            masterRoot.SetActive(false);

            // Save scene cleanly
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            // 3. Configure Editor Build Settings for Direct Machinery AR Startup
            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(MainScenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes;
            sb.AppendLine($"[Build Settings Configured] Direct startup scene: {MainScenePath}");

            // 4. Configure Android Player Settings
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.companyName = "SurakshaAR";
            PlayerSettings.productName = "SurakshaAR Machinery AR";

            // Fix TMP_FontAsset atlas texture assignment if needed
            var tmpFonts = Resources.FindObjectsOfTypeAll<TMPro.TMP_FontAsset>();
            foreach (var font in tmpFonts)
            {
                if (font != null)
                {
                    if (font.atlasTextures == null || font.atlasTextures.Length == 0 || font.atlasTextures[0] == null)
                    {
                        if (font.atlasTexture != null)
                        {
                            font.atlasTextures = new Texture2D[] { font.atlasTexture };
                            EditorUtility.SetDirty(font);
                        }
                    }
                }
            }
            AssetDatabase.SaveAssets();

            // 5. Build Android APK
            BuildPlayerOptions options = new BuildPlayerOptions();
            options.scenes = new string[] { MainScenePath };
            options.locationPathName = OutputApkPath;
            options.target = BuildTarget.Android;
            options.options = BuildOptions.None;

            sb.AppendLine("\nStarting BuildPlayer for Android...");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            sb.AppendLine("\n=========================================================================");
            sb.AppendLine($"BUILD RESULT: {summary.result}");
            sb.AppendLine($"Output Path: {summary.outputPath}");
            sb.AppendLine($"Total Size: {summary.totalSize / (1024 * 1024):F2} MB");
            sb.AppendLine($"Total Errors: {summary.totalErrors}");
            sb.AppendLine($"Total Warnings: {summary.totalWarnings}");
            sb.AppendLine($"Duration: {summary.totalTime.TotalSeconds:F2} seconds");
            sb.AppendLine("=========================================================================");

            if (summary.result != BuildResult.Succeeded)
            {
                sb.AppendLine("\n[BUILD ERRORS]");
                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        {
                            sb.AppendLine($"  - {msg.content}");
                        }
                    }
                }
            }

            File.WriteAllText(LogPath, sb.ToString());
            Debug.Log("[MachineryAndroidAPKBuilder] Build process finished with result: " + summary.result);
        }
    }
}
#endif
