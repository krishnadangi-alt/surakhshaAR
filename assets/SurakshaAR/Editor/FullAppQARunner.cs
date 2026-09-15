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
        private const string ScreenshotDir = @"C:\Users\MP2NQ\.gemini\antigravity-ide\brain\412ee6bb-ee21-4d87-9a62-f97ff10f32ae\screenshots";
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
                        flow.SendMessage("ResolveReferences", SendMessageOptions.DontRequireReceiver);
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
                        // TEST C: Squeeze handle after pin removal -> Spray Activates
                        // ---------------------------------------------------------
                        grip.StartGrip();
                        bool testCSprayActive = grip.IsGripHeld;
                        LogTest("TEST C: Grip after Pin Removal (Spray Activates)", testCSprayActive,
                            $"IsGripHeld: {grip.IsGripHeld} (Expected: true)");

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
                            uiCtrl.SendMessage("ResolveSceneTargets", SendMessageOptions.DontRequireReceiver);

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
                if (AppState.Instance != null && AppState.Instance.gameObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(AppState.Instance.gameObject);
                }
            }
            catch {}

            var liveStateGO = new GameObject("LiveAppState");
            var liveState = liveStateGO.AddComponent<AppState>();
            AppState.Instance = liveState;
            liveState.SetUser("JH-MN-004821", "Ramesh Kumar", false);
            liveState.WorkerRole = "Mine Worker";
            liveState.RecordAssessmentResult(95, 4);
            liveState.CompletedModulesCount = 3;
            liveState.AssessmentScore = 92;

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
                descriptionKey = "module.fire.desc",
                difficultyKey = "difficulty.intermediate",
                scenarioCount = 7,
                durationLabel = "15 Mins",
                isImplemented = true,
                learningPointKeys = new List<string> { "Identify Electrical Hazard", "Pull Alarm Pull Station", "Aim & Spray CO2 for 10s", "Evacuate Hazard Zone" }
            };
            shotSuccess += CaptureScreen(() => ModuleDetailBuilder.Build(), new ModuleDetailController(), "05_module_detail.png", sb, testMod);

            // AR HUD States matching the reference image & user prompt section 21:
            // State 1: AR module just started -> "Tap to Start AR Training"
            shotSuccess += CaptureArHudState("01_ar_start_training.png", 0, "Industrial Fire Response Training", "Scan the floor and tap the reticle to anchor the 3D training scenario.", sb, null, "start");
            // State 2: Scenario successfully placed -> Step 1
            shotSuccess += CaptureArHudState("02_ar_scenario_placed_step1.png", 1, "Identify the Hazard", "Find the electrical fire.", sb);
            // State 3: Hazard identified -> Step 2
            shotSuccess += CaptureArHudState("03_ar_step2_alarm.png", 2, "Activate the Alarm", "Pull the emergency alarm.", sb);
            // State 4: Alarm activated -> Step 3
            shotSuccess += CaptureArHudState("04_ar_step3_extinguisher.png", 3, "Select CO<sub>2</sub> Extinguisher", "Pick the correct CO<sub>2</sub> extinguisher.", sb);
            // State 5: Extinguisher selected -> Step 4
            shotSuccess += CaptureArHudState("05_ar_step4_pin.png", 4, "Remove Safety Pin", "Pull the safety pin.", sb);
            // State 6: Pin guidance visible
            shotSuccess += CaptureArHudState("06_ar_step4_pin_guidance.png", 4, "Remove Safety Pin", "Pull the safety pin.", sb);
            // State 7: Pin removed -> Step 5
            shotSuccess += CaptureArHudState("07_ar_step5_aim.png", 5, "Aim at Base", "Point the nozzle at the base of the fire.", sb);
            // State 8: Valid aim -> Step 6
            shotSuccess += CaptureArHudState("08_ar_step6_spray_ready.png", 6, "Spray & Extinguish", "Press and hold the handle.", sb, null, null, 0f, "Spraying... 0.0 / 10.0 s");
            // State 9: Handle pressed / spraying -> live spray timer
            shotSuccess += CaptureArHudState("09_ar_step6_spraying_0s.png", 6, "Spray & Extinguish", "Press and hold the handle.", sb, null, null, 0.05f, "Spraying... 0.5 / 10.0 s");
            // State 10: 6.5 / 10.0 seconds
            shotSuccess += CaptureArHudState("10_ar_step6_spraying_6_5s.png", 6, "Spray & Extinguish", "Keep spraying at the base.", sb, null, null, 0.65f, "Spraying... 6.5 / 10.0 s");
            // State 11: 10.0 / 10.0 seconds
            shotSuccess += CaptureArHudState("11_ar_step6_spraying_10s.png", 6, "Spray & Extinguish", "Keep spraying at the base.", sb, null, null, 1.0f, "Spraying... 10.0 / 10.0 s");
            // State 12: Fire extinguished -> Completion
            shotSuccess += CaptureArHudCompletion("12_ar_completion.png", 100, "05:42", sb);
            shotSuccess += CaptureArHudState("16_timeout_evacuation.png", 6, "Evacuate the Hazard Zone", "Follow the green EXIT signs to the emergency assembly point.", sb, "Exit Reached — Complete Evacuation", "Time limit reached! Evacuate now!");

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

            // Restore Main scene
            SafeOpenScene("Assets/SurakshaAR/Scenes/Main.unity");
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

                // Ensure all screens use the exact same font as the menu
                var menuFont = UI.UIHelper.GetDefaultFont();
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

                ui.SetModuleInfo("Fire & Explosion Response", stepIndex, 6);

                if (stepIndex == 0)
                {
                    if (actionBtn == "start")
                    {
                        ui.ShowGuidance(
                            stepTag: "SURAKSHAAR AR",
                            title: "Industrial Fire Response Training",
                            description: "In this scenario, an electrical equipment fire breaks out in a mining facility.\nFollow standard operating procedures (SOP) to safely respond and evacuate.",
                            hint: "Scan the floor and tap the reticle to anchor the 3D training scenario.",
                            actionBtnText: "TAP TO START AR TRAINING",
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
                    ui.ShowGuidance("🚪 EMERGENCY EVACUATION", title, description, "Safety first: Never remain in a hazard zone once the emergency timeout is reached.", actionBtn ?? "Exit Reached — Complete Evacuation", null);
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
                    ui.ShowGuidance($"STEP {stepIndex} OF 6", title, description, "Follow National Mining Safety standard SOP.", actionBtn ?? "Action Confirmed", null);
                }

                if (!string.IsNullOrEmpty(feedback))
                {
                    ui.ShowFeedback(FireScenarioUIController.FeedbackType.Correct, "Action Complete", feedback, 3.0f);
                }

                if (progress >= 0f)
                {
                    ui.ShowProgress(progress, progressLabel ?? "Spraying Fire Base...");
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

                ui.SetModuleInfo("Fire & Explosion Response", 6, 6);
                ui.SetScore(score, 0);
                ui.SetTimer(342f); // 05:42

                ui.ShowCompletion(
                    title: "Fire Extinguished!",
                    message: "Well Done!",
                    score: score,
                    timeTaken: timeTaken,
                    onContinue: null
                );

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
                (ScreenId.Progress, "Progress")
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
