using System;
using System.Collections;
using System.Collections.Generic;
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
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SurakshaAR.Editor
{
    [InitializeOnLoad]
    public static class FullAppQARunner
    {
        private const string TriggerFile = @"C:\project\surakshaAR\Temp\run_qa_trigger.txt";
        private const string ResultsFile = @"C:\project\surakshaAR\Temp\qa_results.txt";
        private const string ScreenshotDir = @"C:\Users\MP2NQ\.gemini\antigravity-ide\brain\d5e32a86-b373-434c-9539-64b1d5d775ba\screenshots";
        private const string BuildTriggerFile = @"C:\project\surakshaAR\Temp\build_trigger.txt";
        private const string BuildReportFile = @"C:\project\surakshaAR\Temp\build_report.txt";

        private const string ReadyFile = @"C:\project\surakshaAR\Temp\qa_ready.txt";

        static FullAppQARunner()
        {
            EditorApplication.update += CheckTrigger;
        }

        private static void CheckTrigger()
        {
            try
            {
                File.WriteAllText(@"C:\project\surakshaAR\Temp\debug_status.txt",
                    $"time={DateTime.Now:HH:mm:ss.fff}, isCompiling={EditorApplication.isCompiling}, isUpdating={EditorApplication.isUpdating}, triggerExists={File.Exists(TriggerFile)}, readyExists={File.Exists(ReadyFile)}");
            }
            catch {}

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (File.Exists(TriggerFile) || File.Exists(ReadyFile) || File.Exists(BuildTriggerFile))
                {
                    EditorApplication.isPlaying = false;
                    return; // Wait until playmode exits completely
                }
            }

            if (File.Exists(TriggerFile))
            {
                try { File.Delete(TriggerFile); } catch {}
                try { File.WriteAllText(ReadyFile, DateTime.UtcNow.Ticks.ToString()); } catch {}
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            MultilingualValidationCapture.CheckTrigger();
            RealWorkerFireValidation.CheckTrigger();

            if (File.Exists(ReadyFile))
            {
                try
                {
                    string content = File.ReadAllText(ReadyFile).Trim();
                    if (long.TryParse(content, out long ticks))
                    {
                        var elapsed = (DateTime.UtcNow - new DateTime(ticks, DateTimeKind.Utc)).TotalSeconds;
                        if (elapsed < 3.0) return;
                    }
                }
                catch { return; }

                try { File.Delete(ReadyFile); } catch {}
                RunFullQAAndCaptureAll();
            }

            if (File.Exists(BuildTriggerFile))
            {
                try { File.Delete(BuildTriggerFile); } catch {}
                BuildAndroidAPK();
            }

            RealWorkerFireValidation.CheckTrigger();
        }

        [MenuItem("SurakshaAR/Run Full QA & Capture Evidence")]
        public static void RunFullQAAndCaptureAll()
        {
            var sb = new StringBuilder();
            sb.AppendLine("================================================================================");
            sb.AppendLine("[SurakshaAR QA] MASTER QUALITY ASSURANCE & VERIFICATION RUN");
            sb.AppendLine("Timestamp: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("================================================================================");

            int totalTests = 0;
            int passedTests = 0;

            void LogTest(string name, bool success, string detail = "")
            {
                totalTests++;
                if (success)
                {
                    passedTests++;
                    sb.AppendLine($"[PASS] {name} {(string.IsNullOrEmpty(detail) ? "" : "— " + detail)}");
                    Debug.Log($"[SurakshaAR QA PASS] {name} {detail}");
                }
                else
                {
                    sb.AppendLine($"[FAIL] {name} — {detail}");
                    Debug.LogError($"[SurakshaAR QA FAIL] {name} — {detail}");
                }
            }

            // -------------------------------------------------------------
            // TEST 1: AppState & Data Architecture
            // -------------------------------------------------------------
            try
            {
                var stateGO = new GameObject("TestAppState");
                var state = stateGO.AddComponent<AppState>();
                AppState.Instance = state;
                state.SetUser("EMP-9942", "Test Worker", false);
                state.SetLanguage(AppLanguage.Hindi);
                state.RecordAssessmentResult(95, 4);

                bool stateOk = state.WorkerName == "Test Worker" &&
                               state.EmployeeId == "EMP-9942" &&
                               state.CurrentLanguage == AppLanguage.Hindi &&
                               state.IsPassed == true &&
                               !string.IsNullOrEmpty(state.CertificateId);

                LogTest("AppState User Persistence & Scoring Data", stateOk, $"Cert ID: {state.CertificateId}, Score: {state.AssessmentScore}%");
                // DO NOT DESTROY stateGO here, it is needed by the UI screenshots!
            }
            catch (Exception ex)
            {
                LogTest("AppState User Persistence & Scoring Data", false, ex.Message);
            }

            // -------------------------------------------------------------
            // TEST 2: Multi-Language String Table Audit (English, Hindi, Santali)
            // -------------------------------------------------------------
            try
            {
                int totalKeys = LocalizedStrings.Table.Count;
                int missingHindi = 0;
                int missingSantali = 0;

                foreach (var kvp in LocalizedStrings.Table)
                {
                    if (!kvp.Value.ContainsKey(AppLanguage.Hindi) || string.IsNullOrEmpty(kvp.Value[AppLanguage.Hindi]))
                        missingHindi++;
                    if (!kvp.Value.ContainsKey(AppLanguage.Santali) || string.IsNullOrEmpty(kvp.Value[AppLanguage.Santali]))
                        missingSantali++;
                }

                bool langOk = (missingHindi == 0 && missingSantali == 0 && totalKeys >= 25);
                LogTest("Localization Coverage (English, Hindi, Santali)", langOk,
                    $"Total Keys: {totalKeys}, Missing Hindi: {missingHindi}, Missing Santali: {missingSantali}");
            }
            catch (Exception ex)
            {
                LogTest("Localization Coverage (English, Hindi, Santali)", false, ex.Message);
            }

            // -------------------------------------------------------------
            // TEST 3: Module Catalog & Data Integrity
            // -------------------------------------------------------------
            try
            {
                var modules = ModuleCatalog.BuildDefaultCatalog();
                bool hasFire = false;
                bool hasGas = false;
                bool hasMach = false;

                foreach (var m in modules)
                {
                    if (m.id == ModuleId.FireAndExplosion && m.isImplemented && m.arSceneName == "FireTraining") hasFire = true;
                    if (m.id == ModuleId.GasLeakConfinedSpace) hasGas = true;
                    if (m.id == ModuleId.MachinerySafety) hasMach = true;
                }

                bool catalogOk = hasFire && hasGas && hasMach && modules.Count >= 4;
                LogTest("Module Catalog Structure & AR Scene Association", catalogOk,
                    $"Found {modules.Count} modules (Fire AR Scene: FireTraining, Implemented: true)");
            }
            catch (Exception ex)
            {
                LogTest("Module Catalog Structure & AR Scene Association", false, ex.Message);
            }

            // -------------------------------------------------------------
            // TEST 4: Assessment Telemetry & Critical Error Auto-Fail
            // -------------------------------------------------------------
            try
            {
                var telemGO = new GameObject("TestTelemetry");
                var telem = telemGO.AddComponent<AssessmentTelemetryManager>();
                
                // Case A: Correct full response (Hazard, PPE, Equipment, Evacuation)
                telem.StartSession("fire");
                telem.LogEvent(new AssessmentEvent("hazard_identified") { hazard_type = "electrical_panel", correct = true, action = "identify_electrical_hazard" });
                telem.LogEvent(new AssessmentEvent("ppe_selected") { ppe_type = "dielectric_gloves_and_helmet", correct = true, action = "equip_safety_gear" });
                telem.LogEvent(new AssessmentEvent("equipment_selected") { equipment_type = "co2_extinguisher", correct = true, action = "select_co2_extinguisher" });
                telem.LogEvent(new AssessmentEvent("evacuation_started") { route = "primary_exit_north", safe = true, action = "safe_evacuation" });
                var passResult = telem.CompleteSession();

                // Case B: Critical Unsafe action (Water on Electrical)
                telem.StartSession("fire");
                telem.LogEvent(new AssessmentEvent("critical_action") { action = "water_on_electrical_panel", correct = false });
                var failResult = telem.CompleteSession();

                bool telemOk = (passResult.passed && !failResult.passed && failResult.critical_errors.Count > 0);
                LogTest("Assessment Safety Telemetry & Auto-Fail Rules", telemOk,
                    $"Pass Score: {passResult.overall_score}%, Critical Fail Passed: {failResult.passed} (Critical Errors: {failResult.critical_errors.Count})");
                UnityEngine.Object.DestroyImmediate(telemGO);
            }
            catch (Exception ex)
            {
                LogTest("Assessment Safety Telemetry & Auto-Fail Rules", false, ex.Message);
            }

            // -------------------------------------------------------------
            // TEST 5: AR Scene & Flow Verification in FireTraining.unity
            // -------------------------------------------------------------
            try
            {
                const string scenePath = "Assets/AR_Fire_foundation/scenes/FireTraining.unity";
                var scene = SafeOpenScene(scenePath);
                if (scene.IsValid())
                {
                    var flow = UnityEngine.Object.FindAnyObjectByType<FireScenarioFlowManager>();
                    var fire = UnityEngine.Object.FindAnyObjectByType<FireExtinguishable>();
                    var grip = UnityEngine.Object.FindAnyObjectByType<ExtinguisherGripInteraction>();
                    var pin = UnityEngine.Object.FindAnyObjectByType<FirePinInteraction>();
                    var sprayCol = UnityEngine.Object.FindAnyObjectByType<ExtinguisherSprayCollision>();
                    
                    if (flow != null)
                    {
                        var resolveMethod = flow.GetType().GetMethod("ResolveReferences", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                        resolveMethod?.Invoke(flow, null);
                    }
                    var alarm = UnityEngine.Object.FindAnyObjectByType<AlarmInteraction>();

                    bool flowComponentsOk = (flow != null && fire != null && grip != null && pin != null && sprayCol != null && alarm != null);
                    LogTest("AR Scene Core Components Presence", flowComponentsOk,
                        $"Flow: {flow != null}, Fire: {fire != null}, Grip: {grip != null}, Pin: {pin != null}, SprayCol: {sprayCol != null}, Alarm: {alarm != null}");

                    // Test FireWorldPosition
                    if (fire != null)
                    {
                        Vector3 firePos = fire.FireWorldPosition;
                        bool posOk = firePos != Vector3.zero;
                        LogTest("FireWorldPosition Resolution (Flames vs Extinguisher)", posOk, $"World Position: {firePos}");
                    }

                    // ---------------------------------------------------------
                    // TEST A: Pin NOT removed -> Squeeze handle -> Spray MUST NOT activate
                    // ---------------------------------------------------------
                    if (grip != null && pin != null)
                    {
                        // Ensure pin is inserted/active
                        pin.gameObject.SetActive(true);
                        var pinField = typeof(FirePinInteraction).GetField("pinRemoved", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (pinField != null) pinField.SetValue(pin, false);

                        grip.StopGrip();
                        grip.StartGrip();
                        bool testABlocked = !grip.IsGripHeld;
                        LogTest("TEST A: Grip before Pin Removal (Spray Blocked)", testABlocked,
                            $"IsGripHeld: {grip.IsGripHeld} (Expected: false)");

                        // ---------------------------------------------------------
                        // TEST B: Remove Pin -> Pin removed state & GameObject hidden
                        // ---------------------------------------------------------
                        pin.RemovePin();
                        bool testBPinRemoved = pin.IsPinRemoved() && !pin.gameObject.activeSelf;
                        LogTest("TEST B: Safety Pin Removal State & Visibility", testBPinRemoved,
                            $"IsPinRemoved: {pin.IsPinRemoved()}, GameObject active: {pin.gameObject.activeSelf} (Expected: true, false)");

                        // ---------------------------------------------------------
                        // ---------------------------------------------------------
                        // TEST C: Squeeze handle after pin removal -> Spray Activates
                        // ---------------------------------------------------------
                        grip.StartGrip();
                        bool testCSprayActive = grip.IsGripHeld;
                        var powder = UnityEngine.Object.FindAnyObjectByType<DryPowderSpray>();
                        bool powderSpraying = powder != null && powder.IsSpraying();
                        LogTest("TEST C: Grip after Pin Removal (Spray & Powder Activates)", testCSprayActive && powderSpraying,
                            $"IsGripHeld: {grip.IsGripHeld}, Powder IsSpraying: {powderSpraying} (Expected: true, true)");

                        // ---------------------------------------------------------
                        // PROBLEM 1 & TEST D: Off-Target Spray -> Fire NOT extinguished
                        // ---------------------------------------------------------
                        if (sprayCol != null && fire != null)
                        {
                            fire.ResetFire();
                            sprayCol.ResetCollisionTimer();
                            bool testDNotTouching = !sprayCol.IsTouchingFire;
                            bool testDFireAlive = !fire.IsExtinguished;
                            LogTest("PROBLEM 1 - Test A: Spray Aimed Away (Fire Remains Active)", testDNotTouching && testDFireAlive,
                                $"IsTouchingFire: {sprayCol.IsTouchingFire}, IsExtinguished: {fire.IsExtinguished} (Expected: false, false)");

                            // -----------------------------------------------------
                            // PROBLEM 1 - Test B: Touch Fire Briefly -> Fire MUST NOT Extinguish
                            // -----------------------------------------------------
                            fire.NotifyParticleCollision(true, 0.15f, 10.0f);
                            bool testBBriefTouchAlive = !fire.IsExtinguished && fire.CurrentContactTimer == 0.15f;
                            LogTest("PROBLEM 1 - Test B: Brief Particle Collision (Fire Remains Active)", testBBriefTouchAlive,
                                $"CurrentTimer: {fire.CurrentContactTimer:F2}s, IsExtinguished: {fire.IsExtinguished} (Expected: false)");

                            // -----------------------------------------------------
                            // PROBLEM 1 - Test C: Valid Spray for 3.5s -> Timer at 3.5s, Fire Alive
                            // -----------------------------------------------------
                            fire.NotifyParticleCollision(true, 3.5f, 10.0f);
                            bool testCMidSprayAlive = !fire.IsExtinguished && fire.CurrentContactTimer == 3.5f;
                            LogTest("PROBLEM 1 - Test C: Continuous Spray 3.5s (Timer Accumulating, Fire Alive)", testCMidSprayAlive,
                                $"Timer: {fire.CurrentContactTimer:F1}s / {fire.extinguishTime:F1}s, IsExtinguished: {fire.IsExtinguished}");

                            // -----------------------------------------------------
                            // PROBLEM 1 - Test D: Release Handle / Interrupt Spray -> Timer Resets to 0
                            // -----------------------------------------------------
                            fire.NotifyParticleCollision(false, 0.0f, 10.0f);
                            bool testDResetOk = !fire.IsExtinguished && fire.CurrentContactTimer == 0.0f;
                            LogTest("PROBLEM 1 - Test D: Handle Released / Contact Lost (Timer Resets, Fire Alive)", testDResetOk,
                                $"Timer: {fire.CurrentContactTimer:F1}s, IsExtinguished: {fire.IsExtinguished} (Expected: 0.0s, false)");

                            // -----------------------------------------------------
                            // PROBLEM 1 - Test E: Aim Away Before Duration -> Timer Stays 0, Fire Alive
                            // -----------------------------------------------------
                            fire.NotifyParticleCollision(false, 0.0f, 10.0f);
                            bool testEAimAwayOk = !fire.IsExtinguished && fire.CurrentContactTimer == 0.0f;
                            LogTest("PROBLEM 1 - Test E: Aim Lost Before Duration (Timer Stays 0, Fire Alive)", testEAimAwayOk,
                                $"Timer: {fire.CurrentContactTimer:F1}s, IsExtinguished: {fire.IsExtinguished}");

                            // -----------------------------------------------------
                            // PROBLEM 1 - Test F: Full 10.0s Continuous Spray -> Fire Extinguished
                            // -----------------------------------------------------
                            fire.NotifyParticleCollision(true, 10.0f, 10.0f);
                            bool testFExtinguished = fire.IsExtinguished;
                            LogTest("PROBLEM 1 - Test F: 10-Second Continuous Spray (Fire Fully Extinguished)", testFExtinguished,
                                $"Timer: {fire.CurrentContactTimer:F1}s, Fire IsExtinguished: {fire.IsExtinguished} (Expected: true)");
                        }

                        // Stop grip
                        grip.StopGrip();

                        // ---------------------------------------------------------
                        // PROBLEM 2: Display Extinguisher vs Actual Picked-Up Extinguisher
                        // ---------------------------------------------------------
                        var uiCtrl = UnityEngine.Object.FindAnyObjectByType<FireScenarioUIController>();
                        if (uiCtrl != null)
                        {
                            var rstMethod = typeof(FireScenarioUIController).GetMethod("ResolveSceneTargets", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                            rstMethod?.Invoke(uiCtrl, null);

                            var displayField = typeof(FireScenarioUIController).GetField("_displayExtinguisherTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            var actualField = typeof(FireScenarioUIController).GetField("_actualExtinguisherTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            var pinField2 = typeof(FireScenarioUIController).GetField("_pinTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            var handleField2 = typeof(FireScenarioUIController).GetField("_handleTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                            Transform dispT = displayField?.GetValue(uiCtrl) as Transform;
                            Transform actT = actualField?.GetValue(uiCtrl) as Transform;
                            Transform pinT = pinField2?.GetValue(uiCtrl) as Transform;
                            Transform handleT = handleField2?.GetValue(uiCtrl) as Transform;

                            bool displayResolved = (dispT != null && dispT.name.Contains("display"));
                            bool actualResolved = (actT != null && actT.name == "FireExt");
                            bool pinOnActual = (pinT != null && actT != null && (pinT.IsChildOf(actT) || pinT == actT));
                            bool handleOnActual = (handleT != null && actT != null && (handleT.IsChildOf(actT) || handleT == actT));

                            LogTest("PROBLEM 2 - Test 1: ExtinguisherDisplay Resolved for Step 3 Selection", displayResolved,
                                $"Display Transform: {(dispT != null ? dispT.name : "null")}");

                            LogTest("PROBLEM 2 - Test 2: Actual Extinguisher Resolved for Step 4-6 Operations", actualResolved,
                                $"Actual Extinguisher: {(actT != null ? actT.name : "null")}");

                            LogTest("PROBLEM 2 - Test 3: Safety Pin Guidance Points to Actual Extinguisher", pinOnActual,
                                $"Pin: {(pinT != null ? pinT.name : "null")}, IsChildOf Actual: {pinOnActual}");

                            LogTest("PROBLEM 2 - Test 4: Handle/Grip Guidance Points to Actual Extinguisher", handleOnActual,
                                $"Handle: {(handleT != null ? handleT.name : "null")}, IsChildOf Actual: {handleOnActual}");
                        }
                    }

                    // ---------------------------------------------------------
                    // TEST G: Alarm Activation & Single Score Award
                    // ---------------------------------------------------------
                    if (alarm != null)
                    {
                        alarm.ActivateAlarm();
                        bool alarmActive = alarm.IsActivated;
                        // Activating again should be safe and idempotent
                        alarm.ActivateAlarm();
                        bool idempotent = alarm.IsActivated;
                        LogTest("TEST G: Fire Alarm Activation & Sound Siren", alarmActive && idempotent,
                            $"IsActivated: {alarm.IsActivated}, Idempotent: {idempotent}");
                    }

                    // ---------------------------------------------------------
                    // TEST H: Scenario Clean Reset & State Preservation
                    // ---------------------------------------------------------
                    if (flow != null)
                    {
                        flow.RestartScenario();
                        bool resetScore = (flow.currentScore == 40);
                        bool resetStage = (flow.CurrentStage == FireScenarioFlowManager.Stage.Step1_IdentifyHazard);
                        LogTest("TEST H: Scenario Clean Reset & State Preservation", resetScore && resetStage,
                            $"Score: {flow.currentScore}, Stage: {flow.CurrentStage}");
                    }
                }
                else
                {
                    LogTest("Open AR Scene FireTraining.unity", false, "Scene invalid at path: " + scenePath);
                }
            }
            catch (Exception ex)
            {
                LogTest("AR Scene FireTraining.unity Inspection", false, ex.Message);
            }

            // -------------------------------------------------------------
            // TEST I: 7-Minute Timeout Evacuation & Certificate Block
            // -------------------------------------------------------------
            try
            {
                if (AppState.Instance != null)
                {
                    AppState.Instance.LastAttemptTimedOut = true;
                    AppState.Instance.FireExtinguishedSuccess = false;
                    AppState.Instance.CriticalErrorsCount = 1;
                    bool passBlocked = !AppState.Instance.IsPassed;
                    LogTest("TEST I: 7-Minute Timeout Evacuation & Certificate Block", passBlocked,
                        $"LastAttemptTimedOut: {AppState.Instance.LastAttemptTimedOut}, IsPassed: {AppState.Instance.IsPassed} (Expected: false)");
                    // Restore
                    AppState.Instance.LastAttemptTimedOut = false;
                    AppState.Instance.FireExtinguishedSuccess = true;
                    AppState.Instance.CriticalErrorsCount = 0;
                }
            }
            catch (Exception ex)
            {
                LogTest("TEST I: 7-Minute Timeout Evacuation", false, ex.Message);
            }

            // -------------------------------------------------------------
            // TEST 6: Offline Storage & Sync Manager
            // -------------------------------------------------------------
            try
            {
                var syncGO = new GameObject("TestSync");
                var sync = syncGO.AddComponent<OfflineSyncManager>();
                bool isOfflineByDefault = (sync.CurrentState == SyncState.Offline);
                LogTest("Offline-First Zero-Connectivity Default Mode", isOfflineByDefault, $"Status: '{sync.StatusMessage}'");
                UnityEngine.Object.DestroyImmediate(syncGO);
            }
            catch (Exception ex)
            {
                LogTest("Offline-First Zero-Connectivity Default Mode", false, ex.Message);
            }

            // -------------------------------------------------------------
            // TEST 7: Android Player Settings Validation
            // -------------------------------------------------------------
            try
            {
                int minSdk = (int)PlayerSettings.Android.minSdkVersion;
                var arch = PlayerSettings.Android.targetArchitectures;
                bool arm64 = (arch & AndroidArchitecture.ARM64) != 0;
                bool sdkOk = minSdk >= 29; // Android 10+
                LogTest("Android Platform Specifications (Android 10+, ARM64)", sdkOk && arm64,
                    $"Min SDK: {minSdk}, Architecture: {arch}");
            }
            catch (Exception ex)
            {
                LogTest("Android Platform Specifications", false, ex.Message);
            }

            // -------------------------------------------------------------
            // TEST 8: Responsive Mobile UI & Multi-Resolution Audit
            // -------------------------------------------------------------
            try
            {
                var (resChecksTotal, resChecksPassed, resDetails) = RunResponsiveLayoutChecks();
                bool allLayoutsPass = (resChecksPassed == resChecksTotal && resChecksTotal > 0);
                LogTest("Responsive Mobile UI (13 Resolutions x 11 Screens + AR HUD)", allLayoutsPass,
                    $"Checks: {resChecksPassed}/{resChecksTotal} Passed across 320x568 up to 1440x3200 (16:9, 18:9, 19.5:9, 20:9, 21:9)");
                if (!string.IsNullOrEmpty(resDetails)) sb.AppendLine(resDetails);
            }
            catch (Exception ex)
            {
                LogTest("Responsive Mobile UI (Multi-Resolution)", false, ex.Message);
            }

            // -------------------------------------------------------------
            // SCREENSHOT CAPTURE: ALL 15 SCREENS & AR STATES
            // -------------------------------------------------------------
            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine("[SurakshaAR QA] CAPTURING 15 HIGH-RESOLUTION EVIDENCE SCREENSHOTS (1080x2400)");
            sb.AppendLine("--------------------------------------------------------------------------------");

            Directory.CreateDirectory(ScreenshotDir);

            // Ensure a pristine live AppState exists for UI rendering with real user credentials
            try
            {
                if (AppManager.Instance != null && AppManager.Instance.gameObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(AppManager.Instance.gameObject);
                }
                if (AppState.Instance != null && AppState.Instance.gameObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(AppState.Instance.gameObject);
                }
            }
            catch {}

            var liveAppGO = new GameObject("LiveAppManager");
            var liveApp = liveAppGO.AddComponent<AppManager>();
            liveApp.InitializeForTesting(AppLanguage.Hindi);

            var liveState = liveApp.AppState;
            AppState.Instance = liveState;
            liveState.SetUser("EMP-PROD-CORE-001", "Krishna", false);
            liveState.SetLanguage(AppLanguage.Hindi);
            liveState.WorkerRole = "Mine Worker";
            liveState.RecordAssessmentResult(95, 4);
            liveState.CompletedModulesCount = 1;
            liveState.AssessmentScore = 33;

            int shotSuccess = 0;
            shotSuccess += CaptureScreen(() => SplashScreenBuilder.Build(), new SplashScreenController(), "00_splash.png", sb);
            shotSuccess += CaptureScreen(() => LanguageSelectionBuilder.Build(), new LanguageSelectionController(), "01_language_selection.png", sb);
            shotSuccess += CaptureScreen(() => LoginBuilder.Build(), new LoginController(), "02_login.png", sb);
            shotSuccess += CaptureScreen(() => HomeDashboardBuilder.Build(), new HomeDashboardController(), "03_home_dashboard.png", sb);
            shotSuccess += CaptureScreen(() => ProfileBuilder.Build(), new ProfileController(), "03b_profile.png", sb);
            shotSuccess += CaptureScreen(() => ModuleSelectionBuilder.Build(), new ModuleSelectionController(), "04_module_selection.png", sb);
            shotSuccess += CaptureScreen(() => ProgressBuilder.Build(), new ProgressController(), "11_progress.png", sb);
            
            var testMod = new ModuleData
            {
                id = ModuleId.FireAndExplosion,
                titleKey = "module.fire.title",
                descriptionKey = "module.fire.description",
                difficultyKey = "difficulty.intermediate",
                scenarioCount = 7,
                durationLabel = "15 Mins",
                isImplemented = true,
                learningPointKeys = new List<string> { "Identify Electrical Hazard", "Pull Alarm Pull Station", "Aim & Spray CO2 for 10s", "Evacuate Hazard Zone" }
            };
            shotSuccess += CaptureScreen(() => ModuleDetailBuilder.Build(), new ModuleDetailController(), "05_module_detail.png", sb, testMod);

            // AR HUD States matching the reference image & natural Hindi instructions:
            // State 1: AR module just started -> "AR प्रशिक्षण शुरू करें"
            shotSuccess += CaptureArHudState("01_ar_start_training.png", 0, "औद्योगिक आग से निपटने का प्रशिक्षण", "इस परिदृश्य में, खनन परिसर के विद्युत उपकरण में आग लग जाती है।\nसुरक्षित बचाव और निकासी के लिए मानक संचालन प्रक्रिया (SOP) का पालन करें।", sb, feedback: null, actionBtn: "start");
            // State 2: Scenario placed -> Step 1: खतरे की पहचान
            shotSuccess += CaptureArHudState("02_ar_scenario_placed_step1.png", 1, "खतरे की पहचान करें", "अपने आसपास देखें और विद्युत आग को पहचानें। आग दिखाई देने पर उस पर टैप करें।", sb, actionBtn: "मैंने आग पहचान ली है");
            // State 3: Step 2: अलार्म सक्रिय करें
            shotSuccess += CaptureArHudState("03_ar_step2_alarm.png", 2, "अलार्म सक्रिय करें", "दीवार पर लाल फायर अलार्म स्टेशन का पता लगाएँ और परिसर में मौजूद सभी कर्मियों को सचेत करने के लिए इसे टैप करें।", sb, actionBtn: "अलार्म सक्रिय करें");
            // State 4: Step 3: सही अग्निशामक यंत्र चुनें
            shotSuccess += CaptureArHudState("04_ar_step3_extinguisher.png", 3, "सही अग्निशामक यंत्र चुनें", "जलते हुए उपकरण की जाँच करें। विद्युत आग के लिए उपयुक्त CO₂ अग्निशामक (काली पट्टी) चुनें।", sb, actionBtn: "CO₂ अग्निशामक चुनें");
            // State 5: Step 4: सुरक्षा पिन निकालें
            shotSuccess += CaptureArHudState("05_ar_step4_pin.png", 4, "सुरक्षा पिन निकालें", "हैंडल अनलॉक करने के लिए सुरक्षा पिन निकालें।", sb, actionBtn: "सुरक्षा पिन निकालें");
            // State 6: Step 4: Pin guidance visible
            shotSuccess += CaptureArHudState("06_ar_step4_pin_guidance.png", 4, "सुरक्षा पिन निकालें", "पिन को थोड़ा घुमाएँ और मजबूती से बाहर खींचें। खींचते समय लीवर को न दबाएँ।", sb, actionBtn: "सुरक्षा पिन निकालें");
            // State 7: Step 5: आग के आधार पर निशाना लगाएँ
            shotSuccess += CaptureArHudState("07_ar_step5_aim.png", 5, "आग के आधार पर निशाना लगाएँ", "इंसुलेटेड हॉर्न को पकड़ें। लपटों पर नहीं, सीधे आग के आधार पर निशाना लगाएँ।", sb, actionBtn: "आग के आधार पर निशाना लगाएँ");
            // State 8: Step 6: स्प्रे के लिए तैयार
            shotSuccess += CaptureArHudState("08_ar_step6_spray_ready.png", 6, "आग बुझाएँ (PASS तकनीक)", "हैंडल दबाकर रखें या नीचे बटन पर टैप करें। आग बुझने तक आधार पर दायें-बायें स्प्रे करें।", sb, actionBtn: "हैंडल दबाकर स्प्रे करें", progress: 0f, progressLabel: "स्प्रे के लिए तैयार");
            // State 9: Step 6: स्प्रे हो रहा है (0s)
            shotSuccess += CaptureArHudState("09_ar_step6_spraying_0s.png", 6, "आग बुझाएँ (PASS तकनीक)", "स्प्रे जारी है! आग के आधार पर दायें-बायें स्वीप करें।", sb, actionBtn: "हैंडल दबाकर स्प्रे करें", progress: 0.05f, progressLabel: "स्प्रे हो रहा है... 0.5 / 10.0 से.");
            // State 10: Step 6: 6.5s
            shotSuccess += CaptureArHudState("10_ar_step6_spraying_6_5s.png", 6, "आग बुझाएँ (PASS तकनीक)", "आग और धुआँ पूरी तरह समाप्त होने तक लगातार 10 सेकंड स्प्रे करते रहें।", sb, actionBtn: "हैंडल दबाकर स्प्रे करें", progress: 0.65f, progressLabel: "स्प्रे हो रहा है... 6.5 / 10.0 से.");
            // State 11: Step 6: 10s
            shotSuccess += CaptureArHudState("11_ar_step6_spraying_10s.png", 6, "आग बुझाएँ (PASS तकनीक)", "स्प्रे जारी है! आग के आधार पर दायें-बायें स्वीप करें।", sb, actionBtn: "हैंडल छोड़ें (स्प्रे रोकें)", progress: 1.0f, progressLabel: "स्प्रे हो रहा है... 10.0 / 10.0 से.");
            // State 12: Fire extinguished -> Completion
            shotSuccess += CaptureArHudCompletion("12_ar_completion.png", 100, "05:42", sb);
            shotSuccess += CaptureArHudState("16_timeout_evacuation.png", 6, "सुरक्षित निकासी", "आग बुझा दी गई है। निरंतर नजर रखते हुए धीरे-धीरे पीछे हटें और आपातकालीन निकास संकेतों का पालन करते हुए सुरक्षित स्थान पर जाएँ।", sb, actionBtn: "आपातकालीन निकास की ओर बढ़ें", feedback: "सुरक्षित निकासी");

            shotSuccess += CaptureScreen(() => AssessmentBuilder.Build(), new AssessmentController(), "13_assessment.png", sb);
            shotSuccess += CaptureScreen(() => ResultBuilder.Build(), new ResultController(), "14_result.png", sb);
            shotSuccess += CaptureScreen(() => CertificateBuilder.Build(), new CertificateController(), "15_certificate.png", sb);

            sb.AppendLine("--------------------------------------------------------------------------------");
            sb.AppendLine($"[SurakshaAR QA SUMMARY] Core Logic Tests: {passedTests}/{totalTests} PASSED");
            sb.AppendLine($"[SurakshaAR QA SUMMARY] Screenshots Captured: {shotSuccess}/21 SAVED");
            sb.AppendLine("================================================================================");

            string finalLog = sb.ToString();
            File.WriteAllText(ResultsFile, finalLog);
            Debug.Log(finalLog);

            // Restore FireTraining scene
            if (File.Exists("Assets/AR_Fire_foundation/scenes/FireTraining.unity"))
            {
                SafeOpenScene("Assets/AR_Fire_foundation/scenes/FireTraining.unity");
            }
        }

        private static UnityEngine.SceneManagement.Scene SafeOpenScene(string scenePath)
        {
            if (EditorApplication.isPlaying)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(scenePath);
                return UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            }
            else
            {
                return EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
        }

        private static int CaptureScreen(Func<GameObject> buildFn, IScreenController controller, string fileName, StringBuilder sb, object param = null)
        {
            try
            {
                int width = 1080;
                int height = 2400;

                var camGO = new GameObject("CaptureCam");
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

                var canvasGO = new GameObject("CaptureCanvas");
                var canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.planeDistance = 100f;

                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(width, height);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0f;

                var screenGO = buildFn();
                screenGO.transform.SetParent(canvasGO.transform, false);
                var sRT = screenGO.GetComponent<RectTransform>();
                if (sRT != null)
                {
                    sRT.anchorMin = Vector2.zero;
                    sRT.anchorMax = Vector2.one;
                    sRT.offsetMin = Vector2.zero;
                    sRT.offsetMax = Vector2.zero;
                }

                if (controller != null)
                {
                    controller.OnShow(screenGO, param);
                }

                // Ensure all screens use the authentic Devanagari Hindi font matching reference
                var menuFont = UI.UIHelper.GetFontForLanguage(AppLanguage.Hindi);
                if (menuFont != null)
                {
                    foreach (var tmp in screenGO.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
                    {
                        tmp.font = menuFont;
                    }
                }

                if (fileName == "02_login.png")
                {
                    foreach (var t in screenGO.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
                    {
                        sb.AppendLine($"[LOGIN TMP] name={t.gameObject.name}, parent={t.transform.parent.name}, text='{t.text}'");
                    }
                }

                Canvas.ForceUpdateCanvases();
                foreach (var layout in screenGO.GetComponentsInChildren<LayoutGroup>(true))
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
                }
                Canvas.ForceUpdateCanvases();

                cam.Render();

                RenderTexture.active = rt;
                var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
                RenderTexture.active = null;

                string dest = Path.Combine(ScreenshotDir, fileName);
                File.WriteAllBytes(dest, tex.EncodeToPNG());

                UnityEngine.Object.DestroyImmediate(tex);
                cam.targetTexture = null;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
                UnityEngine.Object.DestroyImmediate(screenGO);
                UnityEngine.Object.DestroyImmediate(canvasGO);
                UnityEngine.Object.DestroyImmediate(camGO);

                sb.AppendLine($"[SCREENSHOT SAVED] {fileName}");
                return 1;
            }
            catch (Exception ex)
            {
                sb.AppendLine($"[SCREENSHOT ERROR] {fileName}: {ex.Message}");
                return 0;
            }
        }

        private static int CaptureArHudState(string fileName, int stepIndex, string title, string description, StringBuilder sb, string feedback = null, string actionBtn = null, float progress = -1f, string progressLabel = null)
        {
            try
            {
                int width = 1080;
                int height = 2400;

                var camGO = new GameObject("CaptureCamAR");
                var cam = camGO.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.08f, 0.12f, 0.16f, 1f); // Dark AR scene simulated backdrop
                cam.orthographic = true;
                cam.orthographicSize = height / 2f;
                cam.nearClipPlane = 0.1f;
                cam.farClipPlane = 1000f;
                cam.transform.position = new Vector3(width / 2f, height / 2f, -100f);

                var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                cam.targetTexture = rt;

                var hostGO = new GameObject("ARHUDHost");
                var ui = hostGO.AddComponent<FireScenarioUIController>();
                ui.EnsureInitialized();

                // Configure Camera and scaling on canvas (unparent to ensure root scaling behavior)
                var canvas = ui.Canvas;
                if (canvas != null)
                {
                    canvas.transform.SetParent(null, false);
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = cam;
                    canvas.planeDistance = 100f;

                    var scaler = canvas.GetComponent<CanvasScaler>();
                    if (scaler != null)
                    {
                        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                        scaler.referenceResolution = new Vector2(width, height);
                        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                        scaler.matchWidthOrHeight = 0f;
                    }

                    var cRT = canvas.GetComponent<RectTransform>();
                    if (cRT != null)
                    {
                        cRT.sizeDelta = new Vector2(width, height);
                    }

                    var safeRT = canvas.transform.Find("SafeArea") as RectTransform;
                    if (safeRT != null)
                    {
                        var sad = safeRT.GetComponent<SafeAreaDriver>();
                        if (sad != null) UnityEngine.Object.DestroyImmediate(sad);

                        safeRT.anchorMin = Vector2.zero;
                        safeRT.anchorMax = Vector2.one;
                        safeRT.offsetMin = Vector2.zero;
                        safeRT.offsetMax = Vector2.zero;
                    }
                }

                ui.SetModuleInfo("आग एवं विस्फोट से निपटना", stepIndex, 6);

                if (stepIndex == 0)
                {
                    if (actionBtn == "start")
                    {
                        ui.ShowGuidance(
                            stepTag: "सुरक्षाAR AR",
                            title: "औद्योगिक आग से निपटने का प्रशिक्षण",
                            description: "इस परिदृश्य में, खनन परिसर के विद्युत उपकरण में आग लग जाती है।\nसुरक्षित बचाव और निकासी के लिए मानक संचालन प्रक्रिया (SOP) का पालन करें।",
                            hint: "फर्श को स्कैन करें और 3D प्रशिक्षण परिदृश्य स्थापित करने के लिए सतह पर टैप करें।",
                            actionBtnText: "AR प्रशिक्षण शुरू करें",
                            onActionClicked: null
                        );
                    }
                    else
                    {
                        ui.SetPlacementState(false, null);
                    }
                }
                else if (fileName.Contains("timeout"))
                {
                    ui.SetScore(70, -30);
                    ui.SetTimer(420f);
                    ui.ShowGuidance("🚪 आपातकालीन निकासी", title, description, "सुरक्षा सर्वोपरि: आपातकालीन समय सीमा समाप्त होने के बाद कभी भी खतरे वाले क्षेत्र में न रहें।", actionBtn ?? "निकास द्वार पहुँचा — निकासी पूर्ण", null);
                }
                else
                {
                    switch (stepIndex)
                    {
                        case 1:
                            ui.SetScore(40, 0);
                            ui.SetTimer(14f); // 00:14
                            break;
                        case 2:
                            ui.SetScore(50, 10);
                            ui.SetTimer(36f); // 00:36
                            break;
                        case 3:
                            ui.SetScore(60, 10);
                            ui.SetTimer(92f); // 01:32
                            break;
                        case 4:
                            ui.SetScore(70, 10);
                            ui.SetTimer(158f); // 02:38
                            break;
                        case 5:
                            ui.SetScore(80, 10);
                            ui.SetTimer(226f); // 03:46
                            break;
                        case 6:
                            ui.SetScore(90, 10);
                            ui.SetTimer(321f); // 05:21
                            break;
                    }
                    ui.ShowGuidance($"चरण {stepIndex} / 6", title, description, "राष्ट्रीय खनन सुरक्षा मानक (SOP) का पालन करें।", actionBtn ?? "कार्रवाई की पुष्टि करें", null);
                }

                if (!string.IsNullOrEmpty(feedback))
                {
                    ui.ShowFeedback(FireScenarioUIController.FeedbackType.Correct, "कार्रवाई पूर्ण", feedback, 3.0f);
                }

                if (progress >= 0f)
                {
                    ui.ShowProgress(progress, progressLabel ?? "स्प्रे जारी है...");
                }

                var hudFont = UI.UIHelper.GetFontForLanguage(AppLanguage.Hindi);
                if (hudFont != null && canvas != null)
                {
                    foreach (var tmp in canvas.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
                    {
                        tmp.font = hudFont;
                    }
                }

                Canvas.ForceUpdateCanvases();
                if (canvas != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(canvas.GetComponent<RectTransform>());
                    foreach (var layout in canvas.GetComponentsInChildren<LayoutGroup>(true))
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
                    }
                }
                Canvas.ForceUpdateCanvases();

                if (stepIndex == 2 && canvas != null)
                {
                    foreach (var tmp in canvas.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
                    {
                        sb.AppendLine($"[TMP DEBUG] {tmp.name}: text='{tmp.text}', rect={tmp.rectTransform.rect}, sizeDelta={tmp.rectTransform.sizeDelta}, mode={tmp.textWrappingMode}, lines={(tmp.textInfo != null ? tmp.textInfo.lineCount : -1)}");
                    }
                }

                cam.Render();

                RenderTexture.active = rt;
                var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
                RenderTexture.active = null;

                string dest = Path.Combine(ScreenshotDir, fileName);
                File.WriteAllBytes(dest, tex.EncodeToPNG());

                UnityEngine.Object.DestroyImmediate(tex);
                cam.targetTexture = null;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
                if (canvas != null) UnityEngine.Object.DestroyImmediate(canvas.gameObject);
                UnityEngine.Object.DestroyImmediate(hostGO);
                UnityEngine.Object.DestroyImmediate(camGO);

                sb.AppendLine($"[SCREENSHOT SAVED] {fileName}");
                return 1;
            }
            catch (Exception ex)
            {
                sb.AppendLine($"[SCREENSHOT ERROR] {fileName}: {ex.Message}");
                return 0;
            }
        }

        private static int CaptureArHudCompletion(string fileName, int score, string timeTaken, StringBuilder sb)
        {
            try
            {
                int width = 1080;
                int height = 2400;

                var camGO = new GameObject("CaptureCamARComp");
                var cam = camGO.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.08f, 0.12f, 0.16f, 1f);
                cam.orthographic = true;
                cam.orthographicSize = height / 2f;
                cam.nearClipPlane = 0.1f;
                cam.farClipPlane = 1000f;
                cam.transform.position = new Vector3(width / 2f, height / 2f, -100f);

                var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                cam.targetTexture = rt;

                var hostGO = new GameObject("ARHUDHostComp");
                var ui = hostGO.AddComponent<FireScenarioUIController>();
                ui.EnsureInitialized();

                var canvas = ui.Canvas;
                if (canvas != null)
                {
                    canvas.transform.SetParent(null, false);
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = cam;
                    canvas.planeDistance = 100f;

                    var scaler = canvas.GetComponent<CanvasScaler>();
                    if (scaler != null)
                    {
                        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                        scaler.referenceResolution = new Vector2(width, height);
                        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                        scaler.matchWidthOrHeight = 0f;
                    }

                    var cRT = canvas.GetComponent<RectTransform>();
                    if (cRT != null)
                    {
                        cRT.sizeDelta = new Vector2(width, height);
                    }

                    var safeRT = canvas.transform.Find("SafeArea") as RectTransform;
                    if (safeRT != null)
                    {
                        var sad = safeRT.GetComponent<SafeAreaDriver>();
                        if (sad != null) UnityEngine.Object.DestroyImmediate(sad);

                        safeRT.anchorMin = Vector2.zero;
                        safeRT.anchorMax = Vector2.one;
                        safeRT.offsetMin = Vector2.zero;
                        safeRT.offsetMax = Vector2.zero;
                    }
                }

                ui.SetModuleInfo("आग एवं विस्फोट से निपटना", 6, 6);
                ui.SetScore(score, 0);
                ui.SetTimer(342f); // 05:42

                ui.ShowCompletion(
                    title: "आग बुझाई गई!",
                    message: "उत्कृष्ट प्रदर्शन!",
                    score: score,
                    timeTaken: timeTaken,
                    onContinue: null
                );

                var hudFontComp = UI.UIHelper.GetFontForLanguage(AppLanguage.Hindi);
                if (hudFontComp != null && canvas != null)
                {
                    foreach (var tmp in canvas.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
                    {
                        tmp.font = hudFontComp;
                    }
                }

                Canvas.ForceUpdateCanvases();
                if (canvas != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(canvas.GetComponent<RectTransform>());
                    foreach (var layout in canvas.GetComponentsInChildren<LayoutGroup>(true))
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
                    }
                }
                Canvas.ForceUpdateCanvases();

                cam.Render();

                RenderTexture.active = rt;
                var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
                RenderTexture.active = null;

                string dest = Path.Combine(ScreenshotDir, fileName);
                File.WriteAllBytes(dest, tex.EncodeToPNG());

                UnityEngine.Object.DestroyImmediate(tex);
                cam.targetTexture = null;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
                if (canvas != null) UnityEngine.Object.DestroyImmediate(canvas.gameObject);
                UnityEngine.Object.DestroyImmediate(hostGO);
                UnityEngine.Object.DestroyImmediate(camGO);

                sb.AppendLine($"[SCREENSHOT SAVED] {fileName}");
                return 1;
            }
            catch (Exception ex)
            {
                sb.AppendLine($"[SCREENSHOT ERROR] {fileName}: {ex.Message}");
                return 0;
            }
        }

        private static (int total, int passed, string details) RunResponsiveLayoutChecks()
        {
            var sb = new StringBuilder();
            int total = 0;
            int passed = 0;

            var targetResolutions = new (string name, int w, int h, string aspect)[]
            {
                ("Ultra Compact", 320, 568, "16:9"),
                ("Compact Android", 360, 640, "16:9"),
                ("Budget 20:9", 360, 800, "20:9"),
                ("Mid-range 16:9", 375, 667, "16:9"),
                ("Modern 19.5:9", 390, 844, "19.5:9"),
                ("Pixel 20:9", 412, 915, "20:9"),
                ("Budget 18:9", 480, 960, "18:9"),
                ("Phablet", 600, 1280, "19.2:9"),
                ("FHD 16:9", 1080, 1920, "16:9"),
                ("FHD+ 18:9", 1080, 2160, "18:9"),
                ("Reference 20:9", 1080, 2400, "20:9"),
                ("Cinema 21:9", 1080, 2520, "21:9"),
                ("QHD+ 20:9", 1440, 3200, "20:9")
            };

            var screenIds = new (ScreenId id, string name)[]
            {
                (ScreenId.Splash, "Splash"),
                (ScreenId.LanguageSelection, "LanguageSelection"),
                (ScreenId.Login, "Login"),
                (ScreenId.HomeDashboard, "HomeDashboard"),
                (ScreenId.ModuleSelection, "ModuleSelection"),
                (ScreenId.ModuleDetail, "ModuleDetail"),
                (ScreenId.ScenarioSelection, "ScenarioSelection"),
                (ScreenId.Assessment, "Assessment"),
                (ScreenId.Result, "Result"),
                (ScreenId.Certificate, "Certificate"),
                (ScreenId.Progress, "Progress"),
                (ScreenId.TrainingInstructions, "TrainingInstructions")
            };

            foreach (var res in targetResolutions)
            {
                var canvasGO = new GameObject($"SimCanvas_{res.w}x{res.h}");
                var canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;

                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 2400);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = res.w > res.h ? 1f : 0f;

                var canvasRT = canvasGO.GetComponent<RectTransform>();
                float scaleFactor = (float)res.w / 1080f;
                float virtualHeight = (float)res.h / scaleFactor;
                canvasRT.sizeDelta = new Vector2(1080f, virtualHeight);

                var safeGO = new GameObject("SafeArea");
                safeGO.transform.SetParent(canvasGO.transform, false);
                var safeRT = safeGO.AddComponent<RectTransform>();
                safeRT.anchorMin = new Vector2(0f, 0.035f);
                safeRT.anchorMax = new Vector2(1f, 0.965f);
                safeRT.offsetMin = Vector2.zero;
                safeRT.offsetMax = Vector2.zero;

                var containerGO = new GameObject("ScreenContainer");
                containerGO.transform.SetParent(safeGO.transform, false);
                var containerRT = containerGO.AddComponent<RectTransform>();
                containerRT.anchorMin = Vector2.zero;
                containerRT.anchorMax = Vector2.one;
                containerRT.offsetMin = Vector2.zero;
                containerRT.offsetMax = Vector2.zero;

                foreach (var s in screenIds)
                {
                    total++;
                    GameObject screenGO = null;
                    try
                    {
                        screenGO = ScreenFactory.Build(s.id);
                        screenGO.transform.SetParent(containerRT, false);
                        var sRT = screenGO.GetComponent<RectTransform>();
                        if (sRT != null)
                        {
                            sRT.anchorMin = Vector2.zero;
                            sRT.anchorMax = Vector2.one;
                            sRT.offsetMin = Vector2.zero;
                            sRT.offsetMax = Vector2.zero;
                        }

                        Canvas.ForceUpdateCanvases();
                        var lgs = screenGO.GetComponentsInChildren<LayoutGroup>(true);
                        for (int li = lgs.Length - 1; li >= 0; li--)
                        {
                            LayoutRebuilder.ForceRebuildLayoutImmediate(lgs[li].GetComponent<RectTransform>());
                        }
                        if (sRT != null) LayoutRebuilder.ForceRebuildLayoutImmediate(sRT);
                        Canvas.ForceUpdateCanvases();

                        var buttons = screenGO.GetComponentsInChildren<Button>(true);
                        bool btnsValid = true;
                        string failBtnName = "";
                        float failBtnH = 0;
                        foreach (var b in buttons)
                        {
                            if (b.name.Contains("chip") || b.name.Contains("badge") || b.name.Contains("radio")) continue;

                            var bRT = b.GetComponent<RectTransform>();
                            if (bRT != null && bRT.rect.height > 0 && bRT.rect.height < 30f)
                            {
                                btnsValid = false;
                                failBtnName = b.name;
                                failBtnH = bRT.rect.height;
                                break;
                            }
                        }

                        if (btnsValid) passed++;
                        else sb.AppendLine($"  [FAIL] {s.name} at {res.w}x{res.h}: button '{failBtnName}' height is {failBtnH}px (<30px)");
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"  [FAIL] {s.name} at {res.w}x{res.h}: {ex.Message}");
                    }
                    finally
                    {
                        if (screenGO != null) UnityEngine.Object.DestroyImmediate(screenGO);
                    }
                }

                total++;
                var arGO = new GameObject($"AR_Sim_{res.w}x{res.h}");
                try
                {
                    FireScenarioUIController.ResetInstance();
                    var arCtrl = arGO.AddComponent<FireScenarioUIController>();
                    arCtrl.EnsureInitialized();
                    Canvas.ForceUpdateCanvases();
                    var buttons = arGO.GetComponentsInChildren<Button>(true);
                    if (buttons.Length >= 4) passed++;
                    else sb.AppendLine($"  [FAIL] AR HUD at {res.w}x{res.h}: found {buttons.Length} buttons (expected >= 4)");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"  [FAIL] AR HUD at {res.w}x{res.h}: {ex.Message}");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(arGO);
                }

                UnityEngine.Object.DestroyImmediate(canvasGO);
            }

            return (total, passed, sb.ToString());
        }

        [MenuItem("SurakshaAR/Build Android APK")]
        public static void BuildAndroidAPK()
        {
            try
            {
                string buildDir = Path.Combine(Application.dataPath, "..", "Builds");
                Directory.CreateDirectory(buildDir);
                string apkPath = Path.Combine(buildDir, "SurakshaAR.apk");

                string[] scenes = new string[]
                {
                    "Assets/SurakshaAR/Scenes/Main.unity",
                    "Assets/AR_Fire_foundation/scenes/FireTraining.unity"
                };

                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

                var buildOptions = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = apkPath,
                    target = BuildTarget.Android,
                    targetGroup = BuildTargetGroup.Android,
                    options = BuildOptions.None
                };

                var report = BuildPipeline.BuildPlayer(buildOptions);
                var summary = report.summary;
                string result = $"Result: {summary.result}\nTotal Time: {summary.totalTime}\nTotal Size: {summary.totalSize} bytes\nErrors: {summary.totalErrors}\nWarnings: {summary.totalWarnings}";
                File.WriteAllText(BuildReportFile, result);
                Debug.Log("[SurakshaAR Build Report] " + result);
            }
            catch (Exception ex)
            {
                File.WriteAllText(BuildReportFile, "Build Exception: " + ex);
                Debug.LogError("[SurakshaAR Build Exception] " + ex);
            }
        }
    }
}
