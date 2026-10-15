using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public static class MachineryAndroidBuildAndRun
{
    private const string ApkOutputPath = "Builds/Android/SurakshaAR.apk";

    [MenuItem("Machinery/Build and Run Android APK")]
    public static void BuildAndRun()
    {
        string dir = Path.GetDirectoryName(ApkOutputPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        Debug.Log("[Build] Starting Android Development Build and Run...");

        var options = new BuildPlayerOptions
        {
            scenes = new string[]
            {
                "Assets/SurakshaAR/Scenes/Main.unity",
                "Assets/AR_Fire_foundation/scenes/FireTraining.unity",
                "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity"
            },
            locationPathName = ApkOutputPath,
            target           = BuildTarget.Android,
            // Development build preserves test assemblies (nunit) needed by glTFast AnimationPlayableComponent
            options          = BuildOptions.AutoRunPlayer | BuildOptions.Development
        };

        BuildReport  report  = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[Build] SUCCESS — APK: {ApkOutputPath} ({summary.totalSize / 1024 / 1024} MB) — Deploying to device...");
        }
        else
        {
            Debug.LogError($"[Build] FAILED — Result: {summary.result} | Errors: {summary.totalErrors}");
        }
    }

    /// <summary>Build-only (no auto-run).</summary>
    [MenuItem("Machinery/Build Android APK Only")]
    public static void BuildOnly()
    {
        string dir = Path.GetDirectoryName(ApkOutputPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        Debug.Log("[Build] Starting Android Build-Only (Development)...");

        var options = new BuildPlayerOptions
        {
            scenes = new string[]
            {
                "Assets/SurakshaAR/Scenes/Main.unity",
                "Assets/AR_Fire_foundation/scenes/FireTraining.unity",
                "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity"
            },
            locationPathName = ApkOutputPath,
            target           = BuildTarget.Android,
            options          = BuildOptions.Development
        };

        BuildReport  report  = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[Build] SUCCESS — APK built: {ApkOutputPath} ({summary.totalSize / 1024 / 1024} MB)");
        }
        else
        {
            Debug.LogError($"[Build] FAILED — Result: {summary.result} | Errors: {summary.totalErrors}");
        }
    }
}
