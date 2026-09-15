using System.IO;
using SurakshaAR.Core;
using SurakshaAR.Screens;
using SurakshaAR.UI;
using SurakshaAR.UI.Builders;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Editor
{
    [InitializeOnLoad]
    public static class ScreenCaptureUtility
    {
        // Trigger refresh: 2026-09-13T16:44:00
        private const string OutputPath = @"C:\Users\MP2NQ\.gemini\antigravity-ide\brain\d869a06e-6e5a-46fa-8163-70413ce9cee6\current_render.png";
        private const string TriggerPath = @"C:\project\surakshaAR\Temp\capture_trigger.txt";

        static ScreenCaptureUtility()
        {
            EditorApplication.delayCall += () =>
            {
                CaptureHomeScreen();
            };

            EditorApplication.update += () =>
            {
                if (File.Exists(TriggerPath))
                {
                    try { File.Delete(TriggerPath); } catch {}
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                    UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                }
            };
        }

        [MenuItem("SurakshaAR/Capture Home Screen")]
        public static void CaptureHomeScreen()
        {
            try
            {
                const string brainDir = @"C:\Users\MP2NQ\.gemini\antigravity-ide\brain\d869a06e-6e5a-46fa-8163-70413ce9cee6";
                string bannerCleanSrc = Path.Combine(brainDir, "jharkhand_mine_banner_clean.jpg");
                string bannerSrc = Path.Combine(brainDir, "jharkhand_mine_banner_1789297699748.jpg");
                string emblemTightSrc = Path.Combine(brainDir, "ashoka_lion_emblem_tight.png");
                string emblemSrc = Path.Combine(brainDir, "ashoka_lion_emblem_1789297728388.jpg");

                string uiImages = Path.Combine(Application.dataPath, "SurakshaAR", "UI", "images");
                string resImages = Path.Combine(Application.dataPath, "Resources", "Images");

                try
                {
                    if (File.Exists(bannerCleanSrc))
                    {
                        File.Copy(bannerCleanSrc, Path.Combine(uiImages, "jharkhand_mine_banner_clean.jpg"), true);
                        File.Copy(bannerCleanSrc, Path.Combine(resImages, "jharkhand_mine_banner_clean.jpg"), true);
                        File.Copy(bannerCleanSrc, Path.Combine(uiImages, "jharkhand_mine_banner.jpg"), true);
                        File.Copy(bannerCleanSrc, Path.Combine(resImages, "jharkhand_mine_banner.jpg"), true);
                    }
                    else if (File.Exists(bannerSrc))
                    {
                        File.Copy(bannerSrc, Path.Combine(uiImages, "jharkhand_mine_banner.jpg"), true);
                        File.Copy(bannerSrc, Path.Combine(resImages, "jharkhand_mine_banner.jpg"), true);
                    }

                    if (File.Exists(emblemTightSrc))
                    {
                        File.Copy(emblemTightSrc, Path.Combine(uiImages, "ashoka_lion_emblem_tight.png"), true);
                        File.Copy(emblemTightSrc, Path.Combine(resImages, "ashoka_lion_emblem_tight.png"), true);
                        File.Copy(emblemTightSrc, Path.Combine(uiImages, "ashoka_lion_emblem.png"), true);
                        File.Copy(emblemTightSrc, Path.Combine(resImages, "ashoka_lion_emblem.png"), true);
                    }
                    else if (File.Exists(emblemSrc))
                    {
                        File.Copy(emblemSrc, Path.Combine(uiImages, "ashoka_lion_emblem.jpg"), true);
                        File.Copy(emblemSrc, Path.Combine(resImages, "ashoka_lion_emblem.jpg"), true);
                    }

                    string fireSrc = Path.Combine(brainDir, "fire_emoji_icon_1789285886705.jpg");
                    string gasSrc = Path.Combine(brainDir, "gas_emoji_icon_1789285911567.jpg");
                    string gearSrc = Path.Combine(brainDir, "gear_emoji_icon_1789285942747.jpg");

                    if (File.Exists(fireSrc))
                    {
                        File.Copy(fireSrc, Path.Combine(uiImages, "fire_emoji_icon.jpg"), true);
                        File.Copy(fireSrc, Path.Combine(resImages, "fire_emoji_icon.jpg"), true);
                    }
                    if (File.Exists(gasSrc))
                    {
                        File.Copy(gasSrc, Path.Combine(uiImages, "gas_emoji_icon.jpg"), true);
                        File.Copy(gasSrc, Path.Combine(resImages, "gas_emoji_icon.jpg"), true);
                    }
                    if (File.Exists(gearSrc))
                    {
                        File.Copy(gearSrc, Path.Combine(uiImages, "gear_emoji_icon.jpg"), true);
                        File.Copy(gearSrc, Path.Combine(resImages, "gear_emoji_icon.jpg"), true);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("[ScreenCaptureUtility] File copy notice: " + ex.Message);
                }

                UIHelper.ClearSpriteCache();
                int width = 1080;
                int height = 2400;

                // 1. Create Camera
                var camGO = new GameObject("TempCaptureCamera");
                var cam = camGO.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.96f, 0.98f, 0.99f, 1f);
                cam.orthographic = true;
                cam.orthographicSize = height / 2f;
                cam.nearClipPlane = 0.1f;
                cam.farClipPlane = 1000f;
                cam.transform.position = new Vector3(width / 2f, height / 2f, -100f);

                var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                cam.targetTexture = rt;

                // 2. Create Canvas
                var canvasGO = new GameObject("TempCanvas");
                var canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.planeDistance = 100f;

                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(width, height);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0f;

                var safeAreaGO = new GameObject("SafeArea");
                safeAreaGO.transform.SetParent(canvasGO.transform, false);
                var safeAreaRT = safeAreaGO.AddComponent<RectTransform>();
                safeAreaRT.anchorMin = Vector2.zero;
                safeAreaRT.anchorMax = Vector2.one;
                safeAreaRT.offsetMin = Vector2.zero;
                safeAreaRT.offsetMax = Vector2.zero;

                var containerGO = new GameObject("ScreenContainer");
                containerGO.transform.SetParent(safeAreaGO.transform, false);
                var containerRT = containerGO.AddComponent<RectTransform>();
                containerRT.anchorMin = Vector2.zero;
                containerRT.anchorMax = Vector2.one;
                containerRT.offsetMin = Vector2.zero;
                containerRT.offsetMax = Vector2.zero;

                // 3. Build Home Dashboard
                var screenGO = HomeDashboardBuilder.Build();
                screenGO.transform.SetParent(containerRT, false);
                var sRT = screenGO.GetComponent<RectTransform>();
                if (sRT != null)
                {
                    sRT.anchorMin = Vector2.zero;
                    sRT.anchorMax = Vector2.one;
                    sRT.offsetMin = Vector2.zero;
                    sRT.offsetMax = Vector2.zero;
                }

                // 4. Bind Controller
                var controller = new HomeDashboardController();
                controller.OnShow(screenGO, null);

                // 5. Force layout updates
                Canvas.ForceUpdateCanvases();
                foreach (var layout in screenGO.GetComponentsInChildren<LayoutGroup>(true))
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
                }
                Canvas.ForceUpdateCanvases();

                // 6. Render
                cam.Render();

                RenderTexture.active = rt;
                var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
                RenderTexture.active = null;

                byte[] bytes = tex.EncodeToPNG();
                File.WriteAllBytes(OutputPath, bytes);
                Debug.Log($"[ScreenCaptureUtility] Saved screenshot to {OutputPath} ({bytes.Length} bytes)");

                // 7. Cleanup
                Object.DestroyImmediate(tex);
                cam.targetTexture = null;
                RenderTexture.active = null;
                rt.Release();
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(screenGO);
                Object.DestroyImmediate(canvasGO);
                Object.DestroyImmediate(camGO);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ScreenCaptureUtility] Failed to capture: {ex}");
            }
        }
    }
}
