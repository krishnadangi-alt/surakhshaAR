using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;

namespace SurakshaAR.Editor
{
    [InitializeOnLoad]
    public static class ARCameraBuildFixer
    {
        static ARCameraBuildFixer()
        {
            EditorApplication.delayCall += EnsureARCoreAndGraphicsConfigured;
        }

        [MenuItem("SurakshaAR/Configure ARCore & OpenGLES3")]
        public static void EnsureARCoreAndGraphicsConfigured()
        {
            bool modified = false;

            // 1. Force Android Graphics API to OpenGLES3 only (ARCore camera black screen fix)
            bool autoGfx = PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android);
            var currentApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
            bool needsGfxFix = autoGfx || currentApis == null || currentApis.Length == 0 ||
                               currentApis[0] != GraphicsDeviceType.OpenGLES3 ||
                               currentApis.Contains(GraphicsDeviceType.Vulkan);

            if (needsGfxFix)
            {
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
                PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
                Debug.Log("[ARCameraBuildFixer] Configured Android Graphics API: OpenGLES3 (Vulkan disabled).");
                modified = true;
            }

            // 2. Configure XR Plug-in Management for Android with ARCore
            XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.settingsKey, out buildTargetSettings);
            if (buildTargetSettings == null)
            {
                buildTargetSettings = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>("Assets/XR/XRGeneralSettingsPerBuildTarget.asset");
            }
            if (buildTargetSettings == null)
            {
                var guids = AssetDatabase.FindAssets("t:XRGeneralSettingsPerBuildTarget");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    buildTargetSettings = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(path);
                }
            }

            if (buildTargetSettings != null)
            {
                var androidSettings = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.Android);
                if (androidSettings == null)
                {
                    buildTargetSettings.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Android);
                    androidSettings = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.Android);
                    modified = true;
                }

                if (androidSettings != null)
                {
                    androidSettings.InitManagerOnStart = true;

                    if (androidSettings.Manager == null)
                    {
                        buildTargetSettings.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
                        modified = true;
                    }

                    var mgr = androidSettings.Manager;
                    if (mgr != null)
                    {
                        mgr.automaticLoading = true;
                        mgr.automaticRunning = true;

                        // Find ARCoreLoader asset
                        var arcoreLoader = AssetDatabase.LoadAssetAtPath<XRLoader>("Assets/XR/Loaders/ARCoreLoader.asset");
                        if (arcoreLoader == null)
                        {
                            var guids = AssetDatabase.FindAssets("t:ARCoreLoader");
                            if (guids.Length > 0)
                            {
                                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                                arcoreLoader = AssetDatabase.LoadAssetAtPath<XRLoader>(path);
                            }
                        }

                        if (arcoreLoader != null)
                        {
                            if (!mgr.activeLoaders.Contains(arcoreLoader))
                            {
                                mgr.TryAddLoader(arcoreLoader);
                                Debug.Log("[ARCameraBuildFixer] Registered ARCoreLoader into Android XRManagerSettings.");
                                modified = true;
                            }
                        }
                    }
                }

                EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.settingsKey, out XRGeneralSettingsPerBuildTarget current);
                if (current == null)
                {
                    EditorBuildSettings.AddConfigObject(XRGeneralSettings.settingsKey, buildTargetSettings, true);
                    modified = true;
                }
            }

            if (modified)
            {
                AssetDatabase.SaveAssets();
                Debug.Log("[ARCameraBuildFixer] Successfully saved ARCore XR settings and OpenGLES3 graphics configuration.");
            }
        }
    }
}
