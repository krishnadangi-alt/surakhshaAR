using System.IO;
using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.Screens;
using SurakshaAR.UI;
using SurakshaAR.UI.Builders;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Editor
{
    [InitializeOnLoad]
    public static class ScreenCaptureUtility
    {
        // Trigger refresh: 2026-09-13T16:44:00
        private const string CurrentBrainDir = @"C:\Users\MP2NQ\.gemini\antigravity-ide\brain\92289780-7780-4b08-97e9-9d96c9501899";
        private const string OutputPath = @"C:\Users\MP2NQ\.gemini\antigravity-ide\brain\92289780-7780-4b08-97e9-9d96c9501899\current_render.png";
        private const string TriggerPath = @"C:\project\surakshaAR\Temp\capture_trigger.txt";
        private const string QATriggerPath = @"C:\project\surakshaAR\Temp\run_qa_trigger.txt";

        static ScreenCaptureUtility()
        {
            EditorApplication.update += () =>
            {
                if (File.Exists(TriggerPath))
                {
                    try { File.Delete(TriggerPath); } catch {}
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                    DevanagariFontAssetCreator.EnsureAllFontAssets();
                    ScreenFlowDiagnosticRunner.RunDiagnostic();
                    CaptureHomeHindiScreen();
                    CaptureAllScreensBatch();
                }
                if (File.Exists(QATriggerPath))
                {
                    try { File.Delete(QATriggerPath); } catch {}
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                    MasterQATestSuite.RunAllTests();
                }
            };
        }

        [MenuItem("SurakshaAR/Capture Home Hindi Screen")]
        public static void CaptureHomeHindiScreen()
        {
            try
            {
                UIHelper.ClearSpriteCache();
                DevanagariFontAssetCreator.EnsureAllFontAssets();

                if (AppManager.Instance == null)
                {
                    var appMgrGO = new GameObject("AppManager");
                    var appMgr = appMgrGO.AddComponent<AppManager>();
                    appMgr.InitializeForTesting(AppLanguage.Hindi);
                }
                else
                {
                    AppManager.Instance.InitializeForTesting(AppLanguage.Hindi);
                }

                if (AppState.Instance != null)
                {
                    AppState.Instance.SetUser("EMP-PROD-CORE-001", "Krishna", false, "खान कार्यकर्ता");
                    AppState.Instance.SetLanguage(AppLanguage.Hindi);
                    AppState.Instance.CompletedModulesCount = 1;
                    AppState.Instance.TotalModulesCount = 3;
                }

                string outDir = Path.Combine(CurrentBrainDir, "screenshots");
                if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
                string scratchDir = @"c:\project\surakshaAR\scratch";
                if (!Directory.Exists(scratchDir)) Directory.CreateDirectory(scratchDir);

                CaptureSingleScreen(scratchDir, "home_hindi_render.png", () => {
                    var go = HomeDashboardBuilder.Build();
                    var ctrl = new HomeDashboardController();
                    ctrl.OnShow(go, null);
                    return go;
                }, 1080, 2400);

                string scratchFile = Path.Combine(scratchDir, "home_hindi_render.png");
                if (File.Exists(scratchFile))
                {
                    File.Copy(scratchFile, Path.Combine(CurrentBrainDir, "home_hindi_render.png"), true);
                    File.Copy(scratchFile, Path.Combine(outDir, "03_home_dashboard.png"), true);
                }

                Debug.Log("[ScreenCaptureUtility] Successfully captured Home Hindi screen!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ScreenCaptureUtility] Failed CaptureHomeHindiScreen: {ex}");
            }
        }

        [MenuItem("SurakshaAR/Capture All Polished Screens")]
        public static void CaptureAllScreensBatch()
        {
            try
            {
                string outDir = Path.Combine(CurrentBrainDir, "screenshots");
                if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

                // Initialize AppManager / AppState for Hindi
                if (AppState.Instance == null)
                {
                    var appStateGO = new GameObject("AppState");
                    appStateGO.AddComponent<AppState>();
                }
                if (AppManager.Instance == null)
                {
                    var appMgrGO = new GameObject("AppManager");
                    var appMgr = appMgrGO.AddComponent<AppManager>();
                    appMgr.InitializeForTesting(AppLanguage.Hindi);
                }
                else
                {
                    AppManager.Instance.InitializeForTesting(AppLanguage.Hindi);
                }

                if (AppState.Instance != null)
                {
                    AppState.Instance.SetUser("EMP-PROD-CORE-001", "Krishna", false, "खान कार्यकर्ता");
                    AppState.Instance.SetLanguage(AppLanguage.Hindi);
                    AppState.Instance.AssessmentScore = 92;
                    AppState.Instance.CriticalErrorsCount = 0;
                    AppState.Instance.LastAttemptTimedOut = false;
                    AppState.Instance.CertificateId = "IND-SAR-2026-0941";
                    AppState.Instance.CertificationDate = "12 Sept 2026";
                    AppState.Instance.CompletedModulesCount = 1;
                    AppState.Instance.TotalModulesCount = 3;
                }

                // 1. Language Selection
                CaptureSingleScreen(outDir, "01_language_selection.png", () => {
                    var go = LanguageSelectionBuilder.Build();
                    var ctrl = new LanguageSelectionController();
                    ctrl.OnShow(go, null);
                    return go;
                });

                // 2. Login Screen
                CaptureSingleScreen(outDir, "02_login.png", () => {
                    var go = LoginBuilder.Build();
                    var ctrl = new LoginController();
                    ctrl.OnShow(go, null);
                    return go;
                });

                // 3. Home Dashboard
                CaptureSingleScreen(outDir, "03_home_dashboard.png", () => {
                    var go = HomeDashboardBuilder.Build();
                    var ctrl = new HomeDashboardController();
                    ctrl.OnShow(go, null);
                    return go;
                });

                // 4. Module Selection (Learn)
                CaptureSingleScreen(outDir, "04_module_selection.png", () => {
                    var go = ModuleSelectionBuilder.Build();
                    var ctrl = new ModuleSelectionController();
                    ctrl.OnShow(go, null);
                    return go;
                });

                // 5. Training Progress
                CaptureSingleScreen(outDir, "11_progress.png", () => {
                    var go = ProgressBuilder.Build();
                    var ctrl = new ProgressController();
                    ctrl.OnShow(go, null);
                    return go;
                });

                // 6. Certificate
                CaptureSingleScreen(outDir, "15_certificate.png", () => {
                    var go = CertificateBuilder.Build();
                    var ctrl = new CertificateController();
                    ctrl.OnShow(go, null);
                    return go;
                });

                // 7. Profile Screen
                CaptureSingleScreen(outDir, "03b_profile.png", () => {
                    var go = ProfileBuilder.Build();
                    var ctrl = new ProfileController();
                    ctrl.OnShow(go, null);
                    return go;
                });

                // 8. Personal Info Modal
                CaptureSingleScreen(outDir, "08_modal_personal_info.png", () => {
                    var go = ProfileBuilder.Build();
                    var ctrl = new ProfileController();
                    ctrl.OnShow(go, null);
                    var modal = UIHelper.FindRect(go, "PersonalInfoModal");
                    if (modal != null) modal.gameObject.SetActive(true);
                    return go;
                });

                // 9. Safety Preferences Modal
                CaptureSingleScreen(outDir, "09_modal_safety_prefs.png", () => {
                    var go = ProfileBuilder.Build();
                    var ctrl = new ProfileController();
                    ctrl.OnShow(go, null);
                    var modal = UIHelper.FindRect(go, "SafetyPreferencesModal");
                    if (modal != null) modal.gameObject.SetActive(true);
                    return go;
                });

                // 10. Assessment Screen
                CaptureSingleScreen(outDir, "13_assessment.png", () => {
                    var go = AssessmentBuilder.Build();
                    var ctrl = new AssessmentController();
                    ctrl.OnShow(go, null);
                    return go;
                });

                // 11. Result Screen
                CaptureSingleScreen(outDir, "14_result.png", () => {
                    var go = ResultBuilder.Build();
                    var ctrl = new ResultController();
                    ctrl.OnShow(go, null);
                    return go;
                });

                // 12. Module Detail Screen (Hindi)
                var testMod = ScriptableObject.CreateInstance<ModuleData>();
                testMod.id = ModuleId.FireAndExplosion;
                testMod.titleKey = "module.fire.title";
                testMod.descriptionKey = "module.fire.description";
                testMod.difficultyKey = "difficulty.intermediate";
                testMod.scenarioCount = 7;
                testMod.durationLabel = "15 Mins";
                testMod.isImplemented = true;
                CaptureSingleScreen(outDir, "05_module_detail.png", () => {
                    var go = ModuleDetailBuilder.Build();
                    var ctrl = new ModuleDetailController();
                    ctrl.OnShow(go, testMod);
                    return go;
                });

                // 13. Fire AR Intro (Hindi)
                CaptureSingleScreen(outDir, "16_fire_ar_intro.png", () => {
                    var host = new GameObject("ARHUDHostTemp");
                    var ui = host.AddComponent<FireScenarioUIController>();
                    ui.EnsureInitialized();
                    ui.SetModuleInfo("आग एवं विस्फोट से निपटना", 0, 6);
                    ui.ShowGuidance(
                        stepTag: "सुरक्षाAR AR",
                        title: "औद्योगिक आग से निपटने का प्रशिक्षण",
                        description: "इस परिदृश्य में, खनन परिसर के विद्युत उपकरण में आग लग जाती है।\nसुरक्षित बचाव और निकासी के लिए मानक संचालन प्रक्रिया (SOP) का पालन करें।",
                        hint: "फर्श को स्कैन करें और 3D प्रशिक्षण परिदृश्य स्थापित करने के लिए सतह पर टैप करें।",
                        actionBtnText: "AR प्रशिक्षण शुरू करें",
                        onActionClicked: null
                    );
                    var hudFont = UIHelper.GetFontForLanguage(AppLanguage.Hindi);
                    if (hudFont != null && ui.Canvas != null)
                    {
                        foreach (var tmp in ui.Canvas.GetComponentsInChildren<TextMeshProUGUI>(true)) tmp.font = hudFont;
                    }
                    return ui.Canvas.gameObject;
                });

                Debug.Log("[ScreenCaptureUtility] Successfully captured all Hindi polished screens + Fire AR!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ScreenCaptureUtility] Failed CaptureAllScreensBatch: {ex}");
            }
        }

        private static void CaptureSingleScreen(string outDir, string filename, System.Func<GameObject> screenFactory, int width = 1080, int height = 2400)
        {
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

            var canvasGO = new GameObject("TempCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 100f;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = width / 1080f;

            var safeAreaGO = new GameObject("SafeArea");
            safeAreaGO.transform.SetParent(canvasGO.transform, false);
            var safeAreaRT = safeAreaGO.AddComponent<RectTransform>();
            safeAreaRT.anchorMin = Vector2.zero;
            safeAreaRT.anchorMax = Vector2.one;
            safeAreaRT.offsetMin = Vector2.zero;
            safeAreaRT.offsetMax = Vector2.zero;

            GameObject screenGO = null;
            Texture2D tex = null;
            try
            {
                screenGO = screenFactory();
                screenGO.transform.SetParent(safeAreaGO.transform, false);
                var sRT = screenGO.GetComponent<RectTransform>();
                if (sRT != null)
                {
                    sRT.anchorMin = Vector2.zero;
                    sRT.anchorMax = Vector2.one;
                    sRT.offsetMin = Vector2.zero;
                    sRT.offsetMax = Vector2.zero;
                }

                Canvas.ForceUpdateCanvases();
                foreach (var layout in screenGO.GetComponentsInChildren<LayoutGroup>(true))
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
                }
                Canvas.ForceUpdateCanvases();

                cam.Render();

                RenderTexture.active = rt;
                tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
                RenderTexture.active = null;

                byte[] bytes = tex.EncodeToPNG();
                string savePath = Path.Combine(outDir, filename);
                File.WriteAllBytes(savePath, bytes);
                Debug.Log($"[ScreenCaptureUtility] Captured {filename} ({bytes.Length} bytes)");
            }
            finally
            {
                if (tex != null) Object.DestroyImmediate(tex);
                if (cam != null) cam.targetTexture = null;
                RenderTexture.active = null;
                if (rt != null)
                {
                    rt.Release();
                    Object.DestroyImmediate(rt);
                }
                if (screenGO != null) Object.DestroyImmediate(screenGO);
                if (canvasGO != null) Object.DestroyImmediate(canvasGO);
                if (camGO != null) Object.DestroyImmediate(camGO);
            }
        }

        [MenuItem("SurakshaAR/Capture Home Screen")]
        public static void CaptureHomeScreen()
        {
            try
            {
                string brainDir = CurrentBrainDir;
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

        [MenuItem("SurakshaAR/Capture Multi-Resolution Screens")]
        public static void CaptureMultiResolutionBatch()
        {
            try
            {
                string resDir = Path.Combine(CurrentBrainDir, "screenshots", "resolutions");
                if (!Directory.Exists(resDir)) Directory.CreateDirectory(resDir);

                (int w, int h)[] resolutions = new[]
                {
                    (360, 640),
                    (390, 844),
                    (412, 915),
                    (480, 960),
                    (720, 1280),
                    (1080, 1920),
                    (1080, 2400)
                };

                // Initialize AppState
                if (AppState.Instance == null)
                {
                    var appStateGO = new GameObject("AppState");
                    appStateGO.AddComponent<AppState>();
                }
                if (AppState.Instance != null)
                {
                    AppState.Instance.SetUser("JH-MN-004821", "Sunil Marandi", false, "Mine Worker");
                    AppState.Instance.SetLanguage(AppLanguage.English);
                }

                foreach (var (w, h) in resolutions)
                {
                    string filename = $"home_{w}x{h}.png";
                    CaptureSingleScreen(resDir, filename, () => {
                        var go = HomeDashboardBuilder.Build();
                        var ctrl = new HomeDashboardController();
                        ctrl.OnShow(go, null);
                        return go;
                    }, w, h);
                    Debug.Log($"[ScreenCaptureUtility] Multi-Res Captured: {w}x{h}");
                }

                Debug.Log("[ScreenCaptureUtility] Successfully completed CaptureMultiResolutionBatch!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ScreenCaptureUtility] Failed CaptureMultiResolutionBatch: {ex}");
            }
        }
    }
}
