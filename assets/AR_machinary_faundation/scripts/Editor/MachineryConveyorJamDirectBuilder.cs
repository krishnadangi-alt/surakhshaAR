#if UNITY_EDITOR
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryConveyorJamDirectBuilder
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string BackupPath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam_LOCKED_MASTER_BACKUP.unity";
        private const string OutputApkPath = "Builds/Android/MachineryConveyorJam.apk";
        private const string SummaryReportPath = "direct_build_summary.txt";

        public static void BuildAPK()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=========================================================================");
            sb.AppendLine("SurakshaAR — DIRECT MACHINERY CONVEYOR JAM APK BUILD");
            sb.AppendLine("Timestamp: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("=========================================================================\n");

            // 1. Verify source scene integrity against backup before doing anything
            string hashBefore = ComputeSHA256(ScenePath);
            string backupHash = ComputeSHA256(BackupPath);
            sb.AppendLine($"Scene Path: {ScenePath}");
            sb.AppendLine($"Scene SHA256 Before: {hashBefore}");
            sb.AppendLine($"Backup SHA256:       {backupHash}");

            if (hashBefore != backupHash)
            {
                sb.AppendLine("CRITICAL ERROR: Scene hash does not match locked master backup!");
                File.WriteAllText(SummaryReportPath, sb.ToString());
                EditorApplication.Exit(1);
                return;
            }
            sb.AppendLine("Scene Integrity: VERIFIED MATCH WITH MASTER BACKUP\n");

            // 2. Open scene in Editor without modifying it
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                sb.AppendLine("CRITICAL ERROR: Failed to open scene " + ScenePath);
                File.WriteAllText(SummaryReportPath, sb.ToString());
                EditorApplication.Exit(1);
                return;
            }
            sb.AppendLine($"Scene Loaded Successfully: {scene.name} (Root Count: {scene.rootCount})");

            // Inspect roots
            GameObject scenarioRoot = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "MachineryScenario")
                {
                    scenarioRoot = root;
                    break;
                }
            }

            if (scenarioRoot == null)
            {
                sb.AppendLine("CRITICAL ERROR: MachineryScenario root not found in scene!");
                File.WriteAllText(SummaryReportPath, sb.ToString());
                EditorApplication.Exit(1);
                return;
            }
            sb.AppendLine($"MachineryScenario Root Found: Active={scenarioRoot.activeSelf}, ChildCount={scenarioRoot.transform.childCount}");

            // 3. Configure EditorBuildSettings without modifying scene
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            sb.AppendLine($"EditorBuildSettings configured with sole scene: {ScenePath}");

            // 4. Ensure output directory exists
            string outDir = Path.GetDirectoryName(OutputApkPath);
            if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            // 5. Build Player
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new string[] { ScenePath },
                locationPathName = OutputApkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            sb.AppendLine("\nStarting Android BuildPlayer...");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            sb.AppendLine("\n=========================================================================");
            sb.AppendLine($"BUILD RESULT: {summary.result}");
            sb.AppendLine($"Output Path: {summary.outputPath}");
            sb.AppendLine($"Total Size: {summary.totalSize / (1024.0 * 1024.0):F2} MB");
            sb.AppendLine($"Total Errors: {summary.totalErrors}");
            sb.AppendLine($"Total Warnings: {summary.totalWarnings}");
            sb.AppendLine($"Duration: {summary.totalTime.TotalSeconds:F2} seconds");
            sb.AppendLine("=========================================================================\n");

            if (summary.result != BuildResult.Succeeded)
            {
                sb.AppendLine("BUILD ERRORS:");
                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        {
                            sb.AppendLine($"  - [{msg.type}] {msg.content}");
                        }
                    }
                }
            }

            // 6. Verify post-build SHA256 of scene
            string hashAfter = ComputeSHA256(ScenePath);
            sb.AppendLine($"Scene SHA256 After:  {hashAfter}");
            bool untouched = (hashAfter == hashBefore);
            sb.AppendLine($"Scene Modified During Build: {(untouched ? "NO (100% UNCHANGED)" : "YES (VIOLATION)")}");

            File.WriteAllText(SummaryReportPath, sb.ToString());
            Debug.Log(sb.ToString());

            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        private static string ComputeSHA256(string path)
        {
            if (!File.Exists(path)) return "FILE_NOT_FOUND";
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
            {
                byte[] bytes = sha.ComputeHash(stream);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
#endif
