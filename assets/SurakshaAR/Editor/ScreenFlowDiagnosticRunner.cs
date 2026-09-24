using System;
using System.Collections;
using System.IO;
using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.Screens;
using SurakshaAR.UI.Builders;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Editor
{
    public static class ScreenFlowDiagnosticRunner
    {
        public static void RunDiagnostic()
        {
            string logPath = @"c:\project\surakshaAR\scratch\flow_diagnostic.txt";
            using (var sw = new StreamWriter(logPath, false, System.Text.Encoding.UTF8))
            {
                sw.WriteLine("=== SCREEN FLOW DIAGNOSTIC START ===");
                try
                {
                    // 1. Ensure AppRoot components exist
                    sw.WriteLine("[1] Checking AppManager / UIManager / AppState...");
                    if (AppManager.Instance == null)
                    {
                        var appMgrGO = new GameObject("AppRootDiagnostic");
                        appMgrGO.AddComponent<AppManager>();
                        AppManager.Instance.InitializeForTesting(AppLanguage.English);
                    }
                    var appMgr = AppManager.Instance;
                    var uiMgr = appMgr.UIManager ?? UIManager.Instance;
                    sw.WriteLine($"AppManager: {(appMgr != null ? "OK" : "NULL")}");
                    sw.WriteLine($"UIManager: {(uiMgr != null ? "OK" : "NULL")}");
                    sw.WriteLine($"AppState: {(AppState.Instance != null ? "OK" : "NULL")}");

                    // 2. Test Splash Screen Build
                    sw.WriteLine("\n[2] Testing Splash Screen Build...");
                    var splashGO = SplashScreenBuilder.Build();
                    sw.WriteLine($"SplashGO: {(splashGO != null ? "Built successfully: " + splashGO.name : "NULL")}");
                    var splashCtrl = new SplashScreenController();
                    splashCtrl.OnShow(splashGO, null);
                    sw.WriteLine("SplashController.OnShow succeeded.");

                    // 3. Test ShowScreen(ScreenId.Splash) via UIManager
                    sw.WriteLine("\n[3] Testing UIManager.ShowScreen(ScreenId.Splash)...");
                    uiMgr.ShowScreen(ScreenId.Splash);
                    sw.WriteLine($"CurrentScreen after Splash: {uiMgr.CurrentScreen}");

                    // 4. Test Transition to LanguageSelection
                    sw.WriteLine("\n[4] Testing UIManager.ShowScreen(ScreenId.LanguageSelection)...");
                    uiMgr.ShowScreen(ScreenId.LanguageSelection);
                    sw.WriteLine($"CurrentScreen after LanguageSelection: {uiMgr.CurrentScreen}");
                    sw.WriteLine($"ActiveScreenGO: {(uiMgr.CurrentScreen == ScreenId.LanguageSelection ? "OK" : "MISMATCH")}");

                    // Check LanguageSelection components
                    var btnContinue = UI.UIHelper.FindButton(splashGO ?? GameObject.Find("LanguageSelectionScreen"), "btn-continue");
                    sw.WriteLine($"Continue button on LanguageSelection: {(btnContinue != null ? "FOUND" : "NOT FOUND on cached root")}");

                    // 5. Test Transition to Login
                    sw.WriteLine("\n[5] Testing UIManager.ShowScreen(ScreenId.Login)...");
                    uiMgr.ShowScreen(ScreenId.Login);
                    sw.WriteLine($"CurrentScreen after Login: {uiMgr.CurrentScreen}");

                    // 6. Test Transition to HomeDashboard
                    sw.WriteLine("\n[6] Testing UIManager.ShowScreen(ScreenId.HomeDashboard)...");
                    uiMgr.ShowScreen(ScreenId.HomeDashboard);
                    sw.WriteLine($"CurrentScreen after HomeDashboard: {uiMgr.CurrentScreen}");

                    sw.WriteLine("\n=== ALL DIRECT SCREEN TRANSITIONS SUCCEEDED WITHOUT EXCEPTION ===");
                }
                catch (Exception ex)
                {
                    sw.WriteLine($"\n[EXCEPTION ENCOUNTERED]:\n{ex}");
                }
            }
        }
    }
}
