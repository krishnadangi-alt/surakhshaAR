#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    [InitializeOnLoad]
    public static class MachineryBuildProfileManager
    {
        private const string MachineryScenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";

        static MachineryBuildProfileManager()
        {
            EditorApplication.delayCall += EnsureAndroidPlatformActive;
        }

        [MenuItem("SurakshaAR/Machinery/Switch Active Build Target to Android")]
        public static void EnsureAndroidPlatformActive()
        {
            // 1. Ensure MachineryConveyorJam scene is registered in EditorBuildSettings
            EnsureSceneInBuildSettings(MachineryScenePath);

            // 2. Switch Active Build Target to Android if not already active
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.Log("[MachineryBuildProfileManager] Switching Active Build Platform to Android...");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }

            // 3. Configure Android Player Settings
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel26)
            {
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            }

            Debug.Log("[MachineryBuildProfileManager] SUCCESS: Android Build Platform is ACTIVE and configured for ARM64 / IL2CPP.");
        }

        public static void EnsureSceneInBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            bool exists = scenes.Any(s => s.path == scenePath);

            if (!exists && File.Exists(scenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
                Debug.Log($"[MachineryBuildProfileManager] Added {scenePath} to EditorBuildSettings.");
            }
            else if (exists)
            {
                foreach (var s in scenes)
                {
                    if (s.path == scenePath)
                    {
                        s.enabled = true;
                    }
                }
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}
#endif
