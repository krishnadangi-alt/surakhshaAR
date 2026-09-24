using System;
using System.IO;
using System.Text;
using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.Localization;
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
    public static class MultilingualValidationCapture
    {
        private const string TriggerFile = @"C:\project\surakshaAR\Temp\run_multilingual_capture.txt";
        private const string ReportFile = @"C:\project\surakshaAR\Temp\multilingual_capture_report.txt";
        private const string BrainDir = @"C:\Users\MP2NQ\.gemini\antigravity-ide\brain\db2c9be4-c7d1-4f54-89e9-36decacdf0cd\screenshots";
        private const string ScratchDir = @"C:\project\surakshaAR\scratch\screenshots";

        static MultilingualValidationCapture()
        {
            EditorApplication.update += CheckTrigger;
        }

        public static void CheckTrigger()
        {
            if (File.Exists(TriggerFile))
            {
                try { File.Delete(TriggerFile); } catch { }
                RunCapture();
            }
        }

        [MenuItem("SurakshaAR/Run Multilingual Validation Capture")]
        public static void RunCapture()
        {
            var sb = new StringBuilder();
            sb.AppendLine("================================================================================");
            sb.AppendLine("[SurakshaAR] MULTILINGUAL VALIDATION CAPTURE (EN / HI / SAT)");
            sb.AppendLine("Timestamp: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("================================================================================");

            try
            {
                Directory.CreateDirectory(BrainDir);
                Directory.CreateDirectory(ScratchDir);

                // Setup AppState & AppManager
                if (AppState.Instance == null)
                {
                    var go = new GameObject("AppState");
                    AppState.Instance = go.AddComponent<AppState>();
                }
                AppState.Instance.SetUser("TEST-001", "Miner Operator", false, "Fire Safety Trainee");

                if (AppManager.Instance == null)
                {
                    var mgrGo = new GameObject("AppManager");
                    var mgr = mgrGo.AddComponent<AppManager>();
                    mgr.InitializeForTesting(AppLanguage.English);
                }

                if (UIManager.Instance == null)
                {
                    var uiGo = new GameObject("UIManager");
                    UIManager.Instance = uiGo.AddComponent<UIManager>();
                }

                // Ensure font fallback chains
                FontManager.Instance.EnsureFallbackChains();

                var languages = new (AppLanguage lang, string code)[]
                {
                    (AppLanguage.English, "en"),
                    (AppLanguage.Hindi, "hi"),
                    (AppLanguage.Santali, "sat")
                };

                int totalCaptured = 0;

                foreach (var (lang, code) in languages)
                {
                    sb.AppendLine($"\n--- Processing Language: {lang} ({code}) ---");
                    AppState.Instance.SetLanguage(lang);
                    if (AppManager.Instance != null)
                    {
                        AppManager.Instance.InitializeForTesting(lang);
                    }

                    // 0. Home Dashboard
                    CaptureScreen(
                        $"home_{code}.png",
                        () =>
                        {
                            var go = HomeDashboardBuilder.Build();
                            var ctrl = new HomeDashboardController();
                            ctrl.OnShow(go, null);
                            return go;
                        },
                        sb, ref totalCaptured);

                    // 1. Fire Scenario Selection
                    CaptureScreen(
                        $"fire_scenarios_{code}.png",
                        () =>
                        {
                            var go = ScenarioSelectionBuilder.Build();
                            var ctrl = new ScenarioSelectionController();
                            ctrl.OnShow(go, null);
                            return go;
                        },
                        sb, ref totalCaptured);

                    // 2. Fire Module Detail: Electrical Panel Fire
                    CaptureScreen(
                        $"fire_detail_electrical_{code}.png",
                        () =>
                        {
                            AppState.Instance.SelectedScenarioIndex = 1;
                            var go = ModuleDetailBuilder.Build();
                            var ctrl = new ModuleDetailController();
                            ctrl.OnShow(go, null);
                            return go;
                        },
                        sb, ref totalCaptured);

                    // 3. Fire Module Detail: Conveyor Belt Fire
                    CaptureScreen(
                        $"fire_detail_conveyor_{code}.png",
                        () =>
                        {
                            AppState.Instance.SelectedScenarioIndex = 2;
                            var go = ModuleDetailBuilder.Build();
                            var ctrl = new ModuleDetailController();
                            ctrl.OnShow(go, null);
                            return go;
                        },
                        sb, ref totalCaptured);

                    // 4. Gas Scenario Selection
                    CaptureScreen(
                        $"gas_scenarios_{code}.png",
                        () =>
                        {
                            var go = GasScenarioSelectionBuilder.Build();
                            var ctrl = new GasScenarioSelectionController();
                            ctrl.OnShow(go, null);
                            return go;
                        },
                        sb, ref totalCaptured);

                    // 5. Gas Module Detail: Underground Gas Release
                    CaptureScreen(
                        $"gas_detail_underground_{code}.png",
                        () =>
                        {
                            AppState.Instance.SelectedScenarioIndex = 101;
                            var go = GasModuleDetailBuilder.Build();
                            var ctrl = new GasModuleDetailController();
                            ctrl.OnShow(go, null);
                            return go;
                        },
                        sb, ref totalCaptured);

                    // 6. Training Instructions
                    CaptureScreen(
                        $"training_instructions_{code}.png",
                        () =>
                        {
                            var go = TrainingInstructionsBuilder.Build();
                            var ctrl = new TrainingInstructionsController();
                            ctrl.OnShow(go, null);
                            return go;
                        },
                        sb, ref totalCaptured);
                }

                sb.AppendLine("\n================================================================================");
                sb.AppendLine($"[SUMMARY] Total screenshots successfully rendered: {totalCaptured} / 21");
                sb.AppendLine("================================================================================");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"[FATAL ERROR] {ex}");
                Debug.LogError($"[MultilingualValidationCapture] Fatal: {ex}");
            }
            finally
            {
                File.WriteAllText(ReportFile, sb.ToString());
                Debug.Log($"[MultilingualValidationCapture] Finished! Log written to {ReportFile}");
            }
        }

        private static void CaptureScreen(string filename, Func<GameObject> factory, StringBuilder sb, ref int count, int width = 1080, int height = 2400)
        {
            var camGO = new GameObject("CaptureCam");
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.97f, 0.98f, 0.99f, 1f);
            cam.orthographic = true;
            cam.orthographicSize = height / 2f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 1000f;
            cam.transform.position = new Vector3(width / 2f, height / 2f, -100f);

            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;

            var canvasGO = new GameObject("CaptureCanvas");
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
                screenGO = factory();
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
                string brainPath = Path.Combine(BrainDir, filename);
                string scratchPath = Path.Combine(ScratchDir, filename);

                File.WriteAllBytes(brainPath, bytes);
                File.WriteAllBytes(scratchPath, bytes);

                count++;
                sb.AppendLine($"[SAVED] {filename} ({bytes.Length} bytes)");
                Debug.Log($"[MultilingualValidationCapture] Saved {filename} ({bytes.Length} bytes)");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"[ERROR] Failed {filename}: {ex.Message}");
                Debug.LogError($"[MultilingualValidationCapture] Failed {filename}: {ex}");
            }
            finally
            {
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
                if (cam != null) cam.targetTexture = null;
                RenderTexture.active = null;
                if (rt != null)
                {
                    rt.Release();
                    UnityEngine.Object.DestroyImmediate(rt);
                }
                if (screenGO != null) UnityEngine.Object.DestroyImmediate(screenGO);
                if (canvasGO != null) UnityEngine.Object.DestroyImmediate(canvasGO);
                if (camGO != null) UnityEngine.Object.DestroyImmediate(camGO);
            }
        }
    }
}
