#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class GasModuleApkBuilder
{
    [MenuItem("SurakshaAR/Build Android APK (Gas Module)")]
    public static void BuildAndroidApk()
    {
        Debug.Log("==================================================");
        Debug.Log("[ANDROID APK BUILD START]");
        Debug.Log("==================================================");

        string outputDir = @"C:\SurakhshaAR\Builds";
        if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

        string apkPath = Path.Combine(outputDir, "AR_Gas_Module_FINAL.apk");

        string[] scenes = new string[]
        {
            "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity"
        };

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = apkPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[APK BUILD SUCCESS] Path: {apkPath} ({summary.totalSize} bytes)");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError($"[APK BUILD FAILED] Total errors: {summary.totalErrors}");
        }
    }
}
#endif
