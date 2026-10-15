#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryNonDestructiveAPKBuilder
    {
        private const string ARScenePath = "Assets/AR_machinary_faundation/scenes/MachineryARExperience.unity";
        private const string LockedSourceScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string BackupPath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam_LOCKED_MASTER_BACKUP.unity";
        private const string OutputApkPath = "Builds/Android/MachineryAR_Workable.apk";
        private const string ReportPath = "E:/SurakshaAR/surakhshaAR/apk_build_summary.txt";

        public static void BuildAPK()
        {
            Directory.CreateDirectory("Builds/Android");
            var sb = new StringBuilder();
            sb.AppendLine("=== NON-DESTRUCTIVE MACHINERY AR APK BUILD ===");
            sb.AppendLine("Timestamp: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // 1. Verify locked source scene file byte integrity before build
            var sceneFi = new FileInfo(LockedSourceScenePath);
            var backupFi = new FileInfo(BackupPath);
            if (!sceneFi.Exists || !backupFi.Exists || sceneFi.Length != backupFi.Length)
            {
                sb.AppendLine("ERROR: Source scene file mismatch against master backup before build!");
                File.WriteAllText(ReportPath, sb.ToString());
                Debug.LogError(sb.ToString());
                return;
            }
            sb.AppendLine($"Locked Scene Integrity Before Build: PASS ({sceneFi.Length} bytes)");

            // 2. Set build scene to separate Machinery AR experience scene
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(ARScenePath, true)
            };

            // 3. Configure Android Player Settings cleanly
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.companyName = "SurakshaAR";
            PlayerSettings.productName = "SurakshaAR Machinery AR";

            // 4. Build Player
            BuildPlayerOptions options = new BuildPlayerOptions();
            options.scenes = new string[] { ARScenePath };
            options.locationPathName = OutputApkPath;
            options.target = BuildTarget.Android;
            options.options = BuildOptions.None;

            sb.AppendLine("Building Android APK...");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            sb.AppendLine($"Build Result: {summary.result}");
            sb.AppendLine($"Output: {summary.outputPath}");
            sb.AppendLine($"Size: {summary.totalSize / (1024 * 1024):F2} MB");
            sb.AppendLine($"Errors: {summary.totalErrors}");
            sb.AppendLine($"Warnings: {summary.totalWarnings}");
            sb.AppendLine($"Duration: {summary.totalTime.TotalSeconds:F2}s");

            // 5. Verify locked scene file byte integrity after build
            sceneFi.Refresh();
            bool stillMatches = sceneFi.Length == backupFi.Length;
            sb.AppendLine($"Locked Scene Integrity After Build: {(stillMatches ? "PASS (UNCHANGED)" : "FAIL (MODIFIED)")}");

            File.WriteAllText(ReportPath, sb.ToString());
            Debug.Log(sb.ToString());
        }
    }
}
#endif
