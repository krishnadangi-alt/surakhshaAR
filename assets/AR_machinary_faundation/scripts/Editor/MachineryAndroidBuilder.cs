#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryAndroidBuilder
    {
        private const string ScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
        private const string OutputDirectory = "Builds";
        private const string ApkName = "SurakshaAR_Machinery_Placement.apk";

        [MenuItem("SurakshaAR/Machinery/Build Android APK")]
        public static void BuildAndroidApk()
        {
            Debug.Log("[MachineryAndroidBuilder] Starting Android APK Build for Machinery AR Module...");

            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }

            string apkPath = Path.Combine(OutputDirectory, ApkName);

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[MachineryAndroidBuilder] SUCCESS! Android APK generated successfully at: {Path.GetFullPath(apkPath)} (Size: {summary.totalSize / (1024 * 1024)} MB)");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"[MachineryAndroidBuilder] BUILD FAILED! Total errors: {summary.totalErrors}");
            }
        }
    }
}
#endif
