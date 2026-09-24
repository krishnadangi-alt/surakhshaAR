#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public class GasAndroidBuildScript
{
    [MenuItem("SurakshaAR/Build Gas Android APK")]
    public static void BuildAndroidAPK()
    {
        Debug.Log("==================================================");
        Debug.Log("[BUILD] Starting Gas Module Android APK Build");
        Debug.Log("==================================================");

        string[] scenes = new string[] { "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity" };
        string buildDir = "C:/SurakhshaAR/Builds";
        if (!Directory.Exists(buildDir))
        {
            Directory.CreateDirectory(buildDir);
        }
        string apkPath = Path.Combine(buildDir, "AR_Gas_Module.apk");

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = apkPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BUILD SUCCESS] APK built successfully at: {apkPath} ({summary.totalSize} bytes)");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError($"[BUILD FAILED] Total Errors: {summary.totalErrors}");
        }
        Debug.Log("==================================================");
    }
}
#endif
