using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public static class MachineryAndroidTestBuildAndRun
{
    private const string ApkOutputPath = "Builds/Android/MachineryAndroidTest.apk";
    private const string TestScenePath = "Assets/AR_machinary_faundation/scenes/MachineryAndroidTest.unity";

    [MenuItem("Machinery/Build and Run Isolated Android Test APK")]
    public static void BuildAndRun()
    {
        string dir = Path.GetDirectoryName(ApkOutputPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        Debug.Log("[IsolatedTestBuild] Starting Android Isolated Test Build & Run...");

        var options = new BuildPlayerOptions
        {
            scenes = new string[] { TestScenePath },
            locationPathName = ApkOutputPath,
            target           = BuildTarget.Android,
            options          = BuildOptions.AutoRunPlayer | BuildOptions.Development
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[IsolatedTestBuild] SUCCESS — APK: {ApkOutputPath} ({summary.totalSize / 1024 / 1024} MB) — Deploying to phone...");
        }
        else
        {
            Debug.LogError($"[IsolatedTestBuild] FAILED — Result: {summary.result} | Errors: {summary.totalErrors}");
        }
    }
}
