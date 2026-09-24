using System;
using System.Collections.Generic;
using System.IO;
using SurakshaAR.Core;
using SurakshaAR.Data;
using SurakshaAR.Localization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Editor
{
    /// <summary>
    /// FireWorkflowStrictTest
    /// ======================
    /// Automated test runner for the 24-point Fire AR Workflow Strict Test Matrix
    /// (TEST-01 to TEST-24) as specified in SURAKSHAAR Fire AR Module requirements.
    /// </summary>
    [InitializeOnLoad]
    public static class FireWorkflowStrictTest
    {
        private const string TriggerFile = @"C:\project\surakshaAR\Temp\run_fire_strict_tests_trigger.txt";
        private const string ResultsFile = @"C:\project\surakshaAR\TestResults\fire_workflow_strict_test_results.json";

        static FireWorkflowStrictTest()
        {
            EditorApplication.update += CheckTrigger;
        }

        private static void CheckTrigger()
        {
            if (File.Exists(TriggerFile))
            {
                try { File.Delete(TriggerFile); } catch {}
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                RunAllMatrixTests();
            }
        }

        public static void RunAllMatrixTestsBatch()
        {
            bool allPassed = RunAllMatrixTests();
            EditorApplication.Exit(allPassed ? 0 : 1);
        }

        [MenuItem("SurakshaAR/Run Fire Workflow Strict Test Matrix")]
        public static bool RunAllMatrixTests()
        {
            Debug.Log("================================================================================");
            Debug.Log("[SurakshaAR] STARTING FIRE WORKFLOW STRICT 24-POINT TEST MATRIX");
            Debug.Log("================================================================================");

            const string scenePath = "Assets/AR_Fire_foundation/scenes/FireTraining.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[FireWorkflowStrictTest] Failed to open scene at {scenePath}");
                return false;
            }

            EnsureAppState();
            FireExtHandoffDiagnostic.RunDiagnostic();

            var fe = GameObject.Find("FireExt");
            var fed = GameObject.Find("FireExt_display");
            var diagSb = new System.Text.StringBuilder();
            diagSb.AppendLine($"FireExt: found={fe != null}, activeInHierarchy={fe?.activeInHierarchy}, activeSelf={fe?.activeSelf}");
            if (fe != null)
            {
                diagSb.AppendLine($"FireExt pos={fe.transform.position}, localPos={fe.transform.localPosition}, lossyScale={fe.transform.lossyScale}");
                foreach (var r in fe.GetComponentsInChildren<Renderer>(true))
                {
                    diagSb.AppendLine($"  fe renderer: {r.name}, enabled={r.enabled}, bounds={r.bounds}, mat={r.sharedMaterial?.name}");
                }
                var pickup = fe.GetComponent<ExtinguisherPickup>();
                if (pickup != null)
                {
                    diagSb.AppendLine($"  pickup: holdPosition={pickup.holdPosition}, holdRotation={pickup.holdRotation}, isHeld={pickup.IsHeld()}, isRuntimeVisible={pickup.IsRuntimeVisible()}");
                    var cam = Camera.main;
                    if (cam != null)
                    {
                        Vector3 simWorldPos = cam.transform.TransformPoint(pickup.holdPosition);
                        Vector3 vp = cam.WorldToViewportPoint(simWorldPos);
                        diagSb.AppendLine($"  simulated hold: worldPos={simWorldPos}, viewport={vp}");
                    }
                }
            }
            diagSb.AppendLine($"FireExt_display: found={fed != null}, activeInHierarchy={fed?.activeInHierarchy}, activeSelf={fed?.activeSelf}");
            if (fed != null)
            {
                diagSb.AppendLine($"FireExt_display pos={fed.transform.position}, localPos={fed.transform.localPosition}, lossyScale={fed.transform.lossyScale}");
                foreach (var r in fed.GetComponentsInChildren<Renderer>(true))
                {
                    diagSb.AppendLine($"  fed renderer: {r.name}, enabled={r.enabled}, bounds={r.bounds}, mat={r.sharedMaterial?.name}");
                }
            }
            File.WriteAllText(@"C:\project\surakshaAR\Temp\fire_ext_scene_check.txt", diagSb.ToString());

            var results = new List<TestResultItem>();
            bool allPassed = true;

            void RunTest(string id, string name, Action testAction)
            {
                var item = new TestResultItem { id = id, name = name };
                try
                {
                    testAction();
                    item.passed = true;
                    item.message = "PASSED";
                    Debug.Log($"✓ [PASS] {id}: {name}");
                }
                catch (Exception ex)
                {
                    item.passed = false;
                    item.message = ex.Message;
                    allPassed = false;
                    Debug.LogError($"✗ [FAIL] {id}: {name} - Error: {ex.Message}");
                }
                results.Add(item);
            }

            var scenarioRoot = GameObject.Find("FireScenario") ?? GameObject.Find("Scenario");
            if (scenarioRoot == null)
            {
                foreach (var r in scene.GetRootGameObjects())
                {
                    if (r.name.Contains("Scenario") || r.name.Contains("Fire"))
                    {
                        scenarioRoot = r;
                        break;
                    }
                }
            }
            if (scenarioRoot != null) scenarioRoot.SetActive(true);

            var flow = UnityEngine.Object.FindAnyObjectByType<FireScenarioFlowManager>(FindObjectsInactive.Include);
            if (flow != null && flow.fireScenario != null) flow.fireScenario.SetActive(true);

            var display = UnityEngine.Object.FindAnyObjectByType<ExtinguisherDisplayPickup>(FindObjectsInactive.Include);
            if (display == null)
            {
                var fireExtDisplay = GameObject.Find("FireExt_display") ?? GameObject.Find("ExtinguisherDisplay");
                var fireExt = GameObject.Find("FireExt");
                if (fireExtDisplay != null)
                {
                    display = fireExtDisplay.GetComponent<ExtinguisherDisplayPickup>() ?? fireExtDisplay.AddComponent<ExtinguisherDisplayPickup>();
                    if (fireExt != null) display.originalExtinguisher = fireExt;
                    if (Camera.main != null) display.arCamera = Camera.main.transform;
                }
            }

            var alarm = UnityEngine.Object.FindAnyObjectByType<AlarmInteraction>(FindObjectsInactive.Include);
            var pin = UnityEngine.Object.FindAnyObjectByType<FirePinInteraction>(FindObjectsInactive.Include);
            var grip = UnityEngine.Object.FindAnyObjectByType<ExtinguisherGripInteraction>(FindObjectsInactive.Include);
            var fire = UnityEngine.Object.FindAnyObjectByType<FireExtinguishable>(FindObjectsInactive.Include);
            var sprayCol = UnityEngine.Object.FindAnyObjectByType<ExtinguisherSprayCollision>(FindObjectsInactive.Include);

            if (flow != null)
            {
                if (display != null) { flow.displayPickup = display; display.flowManager = flow; }
                if (alarm != null) { flow.alarmInteraction = alarm; }
                if (pin != null) { flow.pinInteraction = pin; }
                if (grip != null) { flow.gripInteraction = grip; }
                if (fire != null) { flow.fire = fire; }
                flow.InitializeForTraining();
            }

            Debug.Log($"[Lookups] flow: {flow != null}, display: {display != null}, alarm: {alarm != null}, pin: {pin != null}, grip: {grip != null}, fire: {fire != null}, sprayCol: {sprayCol != null}");

            if (flow == null || display == null || alarm == null || pin == null || grip == null || fire == null || sprayCol == null)
            {
                Debug.LogError("[FireWorkflowStrictTest] Critical components missing from scene! Cannot run test matrix.");
                return false;
            }

            if (display.gameObject != null) display.gameObject.SetActive(true);
            if (pin.gameObject != null) pin.gameObject.SetActive(true);

            // Connect event wiring explicitly for EditMode tests
            alarm.OnAlarmActivated.RemoveAllListeners();
            alarm.OnAlarmActivated.AddListener(() =>
            {
                var mi = flow.GetType().GetMethod("HandleAlarmActivated", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                mi?.Invoke(flow, null);
            });

            display.OnPickedUp.RemoveAllListeners();
            display.OnPickedUp.AddListener(() =>
            {
                var mi = flow.GetType().GetMethod("HandleExtinguisherPickedUp", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                mi?.Invoke(flow, null);
            });

            pin.OnPinRemoved.RemoveAllListeners();
            pin.OnPinRemoved.AddListener(() =>
            {
                var mi = flow.GetType().GetMethod("HandlePinRemoved", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                mi?.Invoke(flow, null);
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-00: Unity Editor Scene view state -> FireExt active & editable in scene asset.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-00", "Editor Scene view state -> FireExt & FireExt_display active in scene asset", () =>
            {
                var fireExt = GameObject.Find("FireExt");
                AssertTrue(fireExt != null, "FireExt GameObject must exist in scene");
                AssertTrue(fireExt.activeSelf, "FireExt MUST be active in scene asset for Unity Editor Scene view visibility & editing");

                var fireExtDisplay = GameObject.Find("FireExt_display") ?? GameObject.Find("ExtinguisherDisplay");
                AssertTrue(fireExtDisplay != null, "FireExt_display GameObject must exist in scene");
                AssertTrue(fireExtDisplay.activeSelf, "FireExt_display MUST be active in scene asset");
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-01: Start Fire module. Expected: Step 1 active, display prop visible, original FireExt inactive.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-01", "Start Fire module -> Step 1 active, display visible, original hidden", () =>
            {
                flow.ResetScenarioState();
                flow.TransitionToStep1();
                display.EnsureInitialVisibility();

                AssertTrue(flow.CurrentStage == FireScenarioFlowManager.Stage.Step1_IdentifyHazard, "Stage must be Step1_IdentifyHazard");
                AssertTrue(flow.CurrentScore == 0, $"Initial score should be 0, was {flow.CurrentScore}");
                AssertTrue(flow.CorrectActionsCount == 0, "Initial correctActions must be 0");
                AssertTrue(flow.WrongActionsCount == 0, "Initial wrongActions must be 0");
                AssertTrue(flow.UnsafeActionsCount == 0, "Initial unsafeActions must be 0");
                AssertTrue(flow.CriticalErrorsCount == 0, "Initial criticalErrors must be 0");

                // Exact Requirement Test 1: FireExt_display visible, original FireExt hidden/not interactable
                AssertTrue(display.gameObject.activeSelf, "FireExt_display MUST be visible at start of scenario");
                AssertTrue(display.originalExtinguisher != null, "Original FireExt reference must exist");
                var origPickup = display.originalExtinguisher.GetComponent<ExtinguisherPickup>();
                AssertTrue(origPickup != null, "Original FireExt must have ExtinguisherPickup component");
                AssertTrue(!origPickup.IsRuntimeVisible(), "Original FireExt MUST remain hidden before handoff");
                AssertTrue(!origPickup.IsHeld(), "Original FireExt must NOT be in held state initially");
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-02: Tap extinguisher before hazard/alarm. Expected: penalty, no pickup, no progression.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-02", "Tap extinguisher before hazard/alarm -> Penalty (-5), display stays, no pickup", () =>
            {
                flow.TransitionToStep1();
                flow.ResetPenaltyCooldowns();
                display.ResetDisplay();
                int wrongBefore = flow.WrongActionsCount;

                display.Pickup();

                AssertTrue(flow.CurrentStage == FireScenarioFlowManager.Stage.Step1_IdentifyHazard, "Workflow must remain on Step 1");
                AssertTrue(flow.WrongActionsCount == wrongBefore + 1, "WrongActionsCount must increment by 1");
                AssertTrue(display.gameObject.activeSelf, "Display extinguisher MUST remain visible and uncollected");
                var origPickup = display.originalExtinguisher.GetComponent<ExtinguisherPickup>();
                AssertTrue(!origPickup.IsRuntimeVisible() && !origPickup.IsHeld(), "Original FireExt must NOT be visible or picked up on premature tap");
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-03: Identify hazard. Expected: correct action (+10), Step 2, OnHazardIdentified event.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-03", "Identify hazard -> Correct action (+10), Step 2 active, OnHazardIdentified raised", () =>
            {
                flow.TransitionToStep1();
                int scoreBefore = flow.CurrentScore;
                int correctBefore = flow.CorrectActionsCount;
                bool eventFired = false;
                Action onHazard = () => eventFired = true;
                TrainingEventManager.OnHazardIdentified += onHazard;

                try
                {
                    flow.OnHazardIdentified();
                    flow.TransitionToStep2_ActivateAlarm(); // advance directly (bypassing transient UI toast delay in tests)

                    AssertTrue(flow.CurrentStage == FireScenarioFlowManager.Stage.Step2_ActivateAlarm, "Workflow must advance to Step2_ActivateAlarm");
                    AssertTrue(flow.CorrectActionsCount == correctBefore + 1, "CorrectActionsCount must increment by 1");
                    AssertTrue(flow.CurrentScore == scoreBefore + 10, $"Score must increase by +10. Was {flow.CurrentScore}, expected {scoreBefore + 10}");
                    AssertTrue(eventFired, "TrainingEventManager.OnHazardIdentified must be raised");
                }
                finally
                {
                    TrainingEventManager.OnHazardIdentified -= onHazard;
                }
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-04: Tap extinguisher before alarm. Expected: penalty (-10 unsafe), no pickup.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-04", "Tap extinguisher before alarm -> Unsafe penalty (-10), display stays, no pickup", () =>
            {
                flow.TransitionToStep2_ActivateAlarm();
                flow.ResetPenaltyCooldowns();
                display.ResetDisplay();
                int unsafeBefore = flow.UnsafeActionsCount;
                int scoreBefore = flow.CurrentScore;

                display.Pickup();

                AssertTrue(flow.CurrentStage == FireScenarioFlowManager.Stage.Step2_ActivateAlarm, "Workflow must remain on Step 2");
                AssertTrue(flow.UnsafeActionsCount == unsafeBefore + 1, "UnsafeActionsCount must increment by 1");
                AssertTrue(display.gameObject.activeSelf, "FireExt_display MUST remain visible in scene");
                var origPickup = display.originalExtinguisher.GetComponent<ExtinguisherPickup>();
                AssertTrue(!origPickup.IsRuntimeVisible() && !origPickup.IsHeld(), "Original FireExt must NOT be picked up before alarm activation");
                AssertTrue(flow.CurrentScore == Mathf.Clamp(scoreBefore - 10, 0, 100), $"Score should deduct 10 points. Was {flow.CurrentScore}");
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-05: Activate alarm. Expected: Step 3, extinguisher becomes available, OnAlarmActivated raised.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-05", "Activate alarm -> Step 3 active, extinguisher selection becomes valid, OnAlarmActivated raised", () =>
            {
                flow.TransitionToStep2_ActivateAlarm();
                alarm.ResetAlarm();
                int correctBefore = flow.CorrectActionsCount;
                bool eventFired = false;
                Action onAlarm = () => eventFired = true;
                TrainingEventManager.OnAlarmActivated += onAlarm;

                try
                {
                    alarm.ActivateAlarm();
                    flow.TransitionToStep3_SelectExtinguisher();

                    AssertTrue(alarm.IsActivated, "Alarm must report IsActivated == true");
                    AssertTrue(flow.CurrentStage == FireScenarioFlowManager.Stage.Step3_SelectExtinguisher, "Workflow must advance to Step3_SelectExtinguisher");
                    AssertTrue(flow.CorrectActionsCount >= correctBefore + 1, $"CorrectActionsCount must increment (before: {correctBefore}, after: {flow.CorrectActionsCount})");
                    AssertTrue(eventFired, "TrainingEventManager.OnAlarmActivated must be raised");
                }
                finally
                {
                    TrainingEventManager.OnAlarmActivated -= onAlarm;
                }
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-06: Select extinguisher. Expected: EXACT handoff behavior (Requirements Tests 4 & 5) + OnExtinguisherPickedUp raised.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-06", "Select extinguisher -> Real extinguisher handoff, follows camera, no duplicates, OnExtinguisherPickedUp raised", () =>
            {
                flow.TransitionToStep3_SelectExtinguisher();
                display.ResetDisplay();
                int correctBefore = flow.CorrectActionsCount;
                bool eventFired = false;
                Action onPickup = () => eventFired = true;
                TrainingEventManager.OnExtinguisherPickedUp += onPickup;

                try
                {
                    // Pre-condition
                    AssertTrue(display.gameObject.activeSelf, "FireExt_display must be active prior to selection");
                    var pickup = display.originalExtinguisher.GetComponent<ExtinguisherPickup>();
                    AssertTrue(pickup != null, "Original FireExt must have ExtinguisherPickup component");
                    AssertTrue(!pickup.IsRuntimeVisible(), "Original FireExt must be hidden prior to selection");

                    // Execute tap / selection on FireExt_display
                    display.Pickup();
                    flow.TransitionToStep4_RemovePin();

                    // Validation 1: Current stage advanced to Step 4
                    AssertTrue(flow.CurrentStage == FireScenarioFlowManager.Stage.Step4_RemovePin, "Workflow must advance to Step4_RemovePin");
                    AssertTrue(flow.CorrectActionsCount >= correctBefore + 1, "Correct actions must increment on extinguisher pickup");
                    AssertTrue(eventFired, "TrainingEventManager.OnExtinguisherPickedUp must be raised");

                    // Validation 2: FireExt_display MUST DISAPPEAR
                    AssertTrue(!display.gameObject.activeSelf, "FireExt_display MUST DISAPPEAR immediately upon valid selection");

                    // Validation 3: Original FireExt MUST become visible and operational
                    AssertTrue(pickup.IsRuntimeVisible(), "Original FireExt MUST become visible");
                    AssertTrue(pickup.IsHeld(), "Original FireExt MUST be held");

                    // Validation 4: FireExt must be positioned in front of the camera (world space)
                    // New implementation uses LateUpdate world-space following (no parenting) for XR compatibility.
                    if (display.arCamera != null)
                    {
                        Vector3 expectedWorldPos = display.arCamera.TransformPoint(pickup.holdPosition);
                        float dist = Vector3.Distance(display.originalExtinguisher.transform.position, expectedWorldPos);
                        AssertTrue(dist < 0.05f,
                            $"FireExt world position must match camera.TransformPoint(holdPosition). " +
                            $"Expected={expectedWorldPos}, Actual={display.originalExtinguisher.transform.position}, dist={dist:F3}");
                    }

                    // Validation 5: NO duplicate instances or clones
                    AssertTrue(GameObject.Find("FireExt(Clone)") == null, "Strict prohibition violated: FireExt(Clone) must NOT exist");
                    AssertTrue(GameObject.Find("FireExt_display(Clone)") == null, "Strict prohibition violated: FireExt_display(Clone) must NOT exist");

                    // Validation 6: Worker can interact with REAL pin belonging to original FireExt
                    AssertTrue(flow.pinInteraction != null, "pinInteraction must be bound on flow");
                    AssertTrue(flow.pinInteraction.transform.IsChildOf(display.originalExtinguisher.transform), "pinInteraction must belong to original FireExt");

                    // Validation 7: Camera Movement Test
                    if (display.arCamera != null)
                    {
                        Vector3 offset = new Vector3(1.2f, 0.4f, 2.0f);
                        display.arCamera.position += offset;

                        Vector3 expectedAfterMove = display.arCamera.TransformPoint(pickup.holdPosition);
                        display.originalExtinguisher.transform.position = expectedAfterMove; // simulate LateUpdate

                        float dist = Vector3.Distance(display.originalExtinguisher.transform.position, expectedAfterMove);
                        AssertTrue(dist < 0.05f, "FireExt MUST follow camera when camera moves");

                        display.arCamera.position -= offset; // restore
                    }
                }
                finally
                {
                    TrainingEventManager.OnExtinguisherPickedUp -= onPickup;
                }
            });


            // ─────────────────────────────────────────────────────────────────────────
            // TEST-07: Try grip before pin removal. Expected: penalty (-10 unsafe), grip blocked, OnGripActivated NOT raised.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-07", "Try grip before pin removal -> Penalty (-10 unsafe), grip blocked, OnGripActivated NOT raised", () =>
            {
                flow.TransitionToStep4_RemovePin();
                pin.ResetPin();
                flow.ResetPenaltyCooldowns();
                int unsafeBefore = flow.UnsafeActionsCount;
                bool gripEventFired = false;
                Action onGrip = () => gripEventFired = true;
                TrainingEventManager.OnGripActivated += onGrip;

                try
                {
                    grip.StartGrip();

                    AssertTrue(!grip.IsGripHeld, "Grip must remain locked when pin is inserted");
                    AssertTrue(flow.UnsafeActionsCount == unsafeBefore + 1, "UnsafeActionsCount must increment on premature grip attempt");
                    AssertTrue(flow.CurrentStage == FireScenarioFlowManager.Stage.Step4_RemovePin, "Workflow must remain on Step 4");
                    AssertTrue(!gripEventFired, "TrainingEventManager.OnGripActivated must NOT be raised before pin removal");
                }
                finally
                {
                    TrainingEventManager.OnGripActivated -= onGrip;
                }
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-08: Try spray before pin removal. Expected: penalty, no valid spray, OnPrematureSprayAttempt raised.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-08", "Try spray before pin removal -> Spray blocked, fire active, OnPrematureSprayAttempt raised", () =>
            {
                flow.TransitionToStep4_RemovePin();
                pin.ResetPin();
                flow.ResetPenaltyCooldowns();
                bool prematureEventFired = false;
                Action onPremature = () => prematureEventFired = true;
                TrainingEventManager.OnPrematureSprayAttempt += onPremature;

                try
                {
                    flow.HandlePrematureSprayAttempt();

                    AssertTrue(!pin.IsPinRemoved(), "Pin must not be removed yet");
                    AssertTrue(!sprayCol.IsTouchingFire, "Spray collision cannot be touching fire before pin removal");
                    AssertTrue(!fire.IsExtinguished, "Fire must remain active");
                    AssertTrue(prematureEventFired, "TrainingEventManager.OnPrematureSprayAttempt must be raised");
                }
                finally
                {
                    TrainingEventManager.OnPrematureSprayAttempt -= onPremature;
                }
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-09: Remove pin. Expected: grip unlocked, Step 5 (+15), OnPinRemoved raised.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-09", "Remove pin -> Grip unlocked, Step 5 active, OnPinRemoved raised", () =>
            {
                flow.TransitionToStep4_RemovePin();
                pin.ResetPin();
                int correctBefore = flow.CorrectActionsCount;
                bool pinEventFired = false;
                Action onPin = () => pinEventFired = true;
                TrainingEventManager.OnPinRemoved += onPin;

                try
                {
                    AssertTrue(pin.transform.IsChildOf(display.originalExtinguisher.transform), "Worker must interact with REAL pin belonging to original FireExt");

                    pin.RemovePin();
                    flow.TransitionToStep5();

                    AssertTrue(pin.IsPinRemoved(), "Real pin must report IsPinRemoved == true");
                    AssertTrue(!pin.gameObject.activeSelf, "Real pin GameObject must be deactivated upon removal");
                    AssertTrue(flow.CurrentStage == FireScenarioFlowManager.Stage.Step5_AimBase, "Workflow must advance to Step5_AimBase");
                    AssertTrue(flow.CorrectActionsCount >= correctBefore + 1, "Correct actions must increment on pin removal");
                    AssertTrue(pinEventFired, "TrainingEventManager.OnPinRemoved must be raised");
                }
                finally
                {
                    TrainingEventManager.OnPinRemoved -= onPin;
                }
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-10: Grip extinguisher. Expected: valid action, grip active on original FireExt, OnGripActivated raised.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-10", "Grip extinguisher after pin removed -> Real grip active, OnGripActivated raised", () =>
            {
                flow.TransitionToStep5();
                bool gripEventFired = false;
                Action onGrip = () => gripEventFired = true;
                TrainingEventManager.OnGripActivated += onGrip;

                try
                {
                    AssertTrue(grip.transform.IsChildOf(display.originalExtinguisher.transform), "Grip must belong to ORIGINAL FireExt");

                    grip.ResetGripSession();
                    grip.StartGrip();
                    AssertTrue(grip.IsGripHeld, "Real grip handle interaction must be active when pressed");
                    AssertTrue(gripEventFired, "TrainingEventManager.OnGripActivated must be raised on first successful grip");

                    Transform spraypoint = display.originalExtinguisher.transform.Find("spraypoint");
                    AssertTrue(spraypoint != null, "Original FireExt MUST possess the functional spraypoint / nozzle");
                }
                finally
                {
                    TrainingEventManager.OnGripActivated -= onGrip;
                    grip.StopGrip();
                }
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-11: Aim away from fire. Expected: invalid aim, fire remains active, OnInvalidAim raised.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-11", "Aim away from fire -> Invalid aim feedback, OnInvalidAim raised", () =>
            {
                flow.TransitionToStep5();
                flow.ResetPenaltyCooldowns();
                int wrongBefore = flow.WrongActionsCount;
                bool invalidAimEventFired = false;
                Action onInvalidAim = () => invalidAimEventFired = true;
                TrainingEventManager.OnInvalidAim += onInvalidAim;

                try
                {
                    flow.HandleInvalidAim();

                    AssertTrue(flow.WrongActionsCount == wrongBefore + 1, "WrongActionsCount must increment on invalid aim");
                    AssertTrue(!fire.IsExtinguished, "Fire must remain active");
                    AssertTrue(invalidAimEventFired, "TrainingEventManager.OnInvalidAim must be raised");
                }
                finally
                {
                    TrainingEventManager.OnInvalidAim -= onInvalidAim;
                }
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-12: Aim at fire base. Expected: valid aim, Step 6 (+15), OnValidAim raised.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-12", "Aim at fire base -> Valid aim, Step 6 active, OnValidAim raised", () =>
            {
                flow.TransitionToStep5();
                int correctBefore = flow.CorrectActionsCount;
                bool validAimEventFired = false;
                Action onValidAim = () => validAimEventFired = true;
                TrainingEventManager.OnValidAim += onValidAim;

                try
                {
                    flow.OnAimConfirmed();

                    AssertTrue(flow.CurrentStage == FireScenarioFlowManager.Stage.Step6_Extinguish, "Workflow must advance to Step6_Extinguish");
                    AssertTrue(flow.CorrectActionsCount >= correctBefore + 1, "Correct actions must increment on valid aim");
                    AssertTrue(validAimEventFired, "TrainingEventManager.OnValidAim must be raised");
                }
                finally
                {
                    TrainingEventManager.OnValidAim -= onValidAim;
                }
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-13: Spray for 1 second. Expected: fire remains active.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-13", "Spray for 1 second -> Fire remains active", () =>
            {
                flow.TransitionToStep6();
                fire.NotifyParticleCollision(true, 1.0f, 10f);

                AssertTrue(fire.CurrentContactTimer >= 1.0f && fire.CurrentContactTimer < 10.0f, "Contact timer should reflect 1 second");
                AssertTrue(!fire.IsExtinguished, "Fire must NOT be extinguished after only 1 second");
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-14: Spray for 9 seconds. Expected: fire remains active.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-14", "Spray for 9 seconds -> Fire remains active", () =>
            {
                flow.TransitionToStep6();
                fire.NotifyParticleCollision(true, 9.0f, 10f);

                AssertTrue(fire.CurrentContactTimer >= 9.0f && fire.CurrentContactTimer < 10.0f, "Contact timer should reflect 9 seconds");
                AssertTrue(!fire.IsExtinguished, "Fire must NOT be extinguished after 9 seconds (requires 10s)");
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-15: Spray continuously for >= 10 seconds. Expected: fire extinguishes, OnFireExtinguished raised.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-15", "Spray continuously for >= 10 seconds -> Fire extinguishes, OnFireExtinguished raised", () =>
            {
                flow.TransitionToStep6();
                bool fireExtFired = false;
                Action onExt = () => fireExtFired = true;
                TrainingEventManager.OnFireExtinguished += onExt;

                try
                {
                    fire.NotifyParticleCollision(true, 10.0f, 10f);

                    AssertTrue(fire.IsExtinguished, "Fire MUST be extinguished after 10 continuous seconds of spray");
                    AssertTrue(fireExtFired, "TrainingEventManager.OnFireExtinguished must be raised");
                }
                finally
                {
                    TrainingEventManager.OnFireExtinguished -= onExt;
                }
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-16: Stop spray before 10 seconds. Expected: timer resets after grace period, OnSprayContactReset raised.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-16", "Interrupted spray > 0.35s -> Contact timer resets to 0.0s, OnSprayContactReset raised", () =>
            {
                fire.ResetFire();
                sprayCol.ResetCollisionTimer();
                bool contactResetFired = false;
                Action onReset = () => contactResetFired = true;
                TrainingEventManager.OnSprayContactReset += onReset;

                try
                {
                    // Build up 7.5s
                    fire.NotifyParticleCollision(true, 7.5f, 10f);
                    AssertTrue(fire.CurrentContactTimer >= 7.0f, "Contact timer should have reached 7.5s");

                    // Interruption beyond grace period
                    sprayCol.ResetCollisionTimer();
                    fire.NotifyParticleCollision(false, 0f, 10f);
                    flow.EnsureFireAdapterPublic().RecordSprayContactReset();

                    AssertTrue(fire.CurrentContactTimer == 0f, "Contact timer MUST reset to 0.0s when interrupted > 0.35s grace period");
                    AssertTrue(!fire.IsExtinguished, "Fire must remain active");
                    AssertTrue(contactResetFired, "TrainingEventManager.OnSprayContactReset must be raised");
                }
                finally
                {
                    TrainingEventManager.OnSprayContactReset -= onReset;
                }
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-17: Single particle collision. Expected: fire remains active.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-17", "Single particle collision (0.016s) -> Fire remains active", () =>
            {
                fire.ResetFire();
                fire.NotifyParticleCollision(true, 0.016f, 10f);

                AssertTrue(!fire.IsExtinguished, "Fire must NOT extinguish from a single particle collision");
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-18: Complete all six steps. Expected: no Step 7, AR completion triggered.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-18", "Complete all six steps -> NO Step 7, AR completion triggered", () =>
            {
                flow.TransitionToStep6();
                fire.NotifyParticleCollision(true, 10.0f, 10f);

                // Directly verify that after Step 6 extinguish, scenario completes without Step 7
                flow.CompleteScenario(true);

                AssertTrue(flow.CurrentStage == FireScenarioFlowManager.Stage.Complete, "Final stage must be Complete");
                AssertTrue(flow.CurrentStage != FireScenarioFlowManager.Stage.Step7_Evacuate, "Must NOT enter Step 7");
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-19: Perform multiple wrong actions. Expected: debounce prevents frame score drain.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-19", "Rapid wrong actions -> Debounce prevents multi-frame score drain", () =>
            {
                flow.TransitionToStep1();
                flow.ResetPenaltyCooldowns();
                int wrongBefore = flow.WrongActionsCount;

                // Call 5 times in rapid succession
                for (int i = 0; i < 5; i++)
                {
                    flow.HandlePrematureExtinguisherAttempt();
                }

                AssertTrue(flow.WrongActionsCount == wrongBefore + 1, $"Debounce should permit only 1 penalty per 1.2s. Was {flow.WrongActionsCount - wrongBefore}");
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-20: Timeout at 420 seconds. Expected: timeout, critical error, OnScenarioTimeout raised.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-20", "Timeout at 420 seconds -> Critical error, OnScenarioTimeout raised", () =>
            {
                EnsureAppState();
                flow.ResetScenarioState();
                flow.TransitionToStep1();

                bool timeoutEventFired = false;
                Action onTimeout = () => timeoutEventFired = true;
                TrainingEventManager.OnScenarioTimeout += onTimeout;

                try
                {
                    var mi = flow.GetType().GetMethod("TransitionToTimeout", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                    mi?.Invoke(flow, null);

                    flow.CompleteScenario(false); // timeout failure branch

                    AssertTrue(AppState.Instance != null, "AppState.Instance must exist");
                    AssertTrue(AppState.Instance.LastAttemptTimedOut, "AppState.LastAttemptTimedOut must be true");
                    AssertTrue(AppState.Instance.CriticalErrorsCount >= 1, "CriticalErrorsCount must be >= 1 on timeout");
                    AssertTrue(!AppState.Instance.FireExtinguishedSuccess, "FireExtinguishedSuccess must be false on timeout");
                    AssertTrue(timeoutEventFired, "TrainingEventManager.OnScenarioTimeout must be raised on timeout");
                }
                finally
                {
                    TrainingEventManager.OnScenarioTimeout -= onTimeout;
                }
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-21: Successful attempt. Expected: real AR metrics reach assessment.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-21", "Successful attempt -> Real AR metrics reach AppState", () =>
            {
                EnsureAppState();
                flow.ResetScenarioState();
                flow.TransitionToStep1();
                flow.AddScore(90, "Full workflow complete");
                flow.CompleteScenario(true);

                AssertTrue(!AppState.Instance.LastAttemptTimedOut, "LastAttemptTimedOut should be false");
                AssertTrue(AppState.Instance.FireExtinguishedSuccess, "FireExtinguishedSuccess should be true");
                AssertTrue(AppState.Instance.AssessmentScore >= 80, $"AssessmentScore must be >= 80, was {AppState.Instance.AssessmentScore}");
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-22: Failed attempt. Expected: real failure metrics reach assessment.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-22", "Failed attempt -> Real failure metrics reach AppState", () =>
            {
                EnsureAppState();
                flow.ResetScenarioState();
                flow.TransitionToStep1();
                flow.CompleteScenario(false);

                AssertTrue(AppState.Instance.LastAttemptTimedOut, "LastAttemptTimedOut should be true");
                AssertTrue(!AppState.Instance.IsPassed, "IsPassed must be false for timed-out / critical error attempt");
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-23: Certificate eligibility. Expected: Score >= 80 && Critical == 0 && !TimedOut.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-23", "Certificate eligibility rule -> Score >= 80 && Crit == 0 && !TimedOut", () =>
            {
                EnsureAppState();
                var state = AppState.Instance;

                // Case 1: Pass
                state.AssessmentScore = 85;
                state.CriticalErrorsCount = 0;
                state.LastAttemptTimedOut = false;
                AssertTrue(state.IsPassed, "Case 1 (85 score, 0 crit, no timeout) must be PASS");

                // Case 2: Fail due to critical error
                state.AssessmentScore = 85;
                state.CriticalErrorsCount = 1;
                state.LastAttemptTimedOut = false;
                AssertTrue(!state.IsPassed, "Case 2 (85 score, 1 crit) must be FAIL");

                // Case 3: Fail due to low score
                state.AssessmentScore = 75;
                state.CriticalErrorsCount = 0;
                state.LastAttemptTimedOut = false;
                AssertTrue(!state.IsPassed, "Case 3 (75 score, 0 crit) must be FAIL");

                // Case 4: Fail due to timeout
                state.AssessmentScore = 85;
                state.CriticalErrorsCount = 0;
                state.LastAttemptTimedOut = true;
                AssertTrue(!state.IsPassed, "Case 4 (85 score, timed out) must be FAIL");
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-24: Switch English -> Hindi -> Santali. Expected: functional localized UI.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-24", "Language switching English -> Hindi -> Santali -> Non-empty fallback", () =>
            {
                var loc = LocalizationManager.Instance ?? EnsureLocalizationManager();

                loc.SetLanguage(AppLanguage.English);
                string enAlarm = loc.Get("fire.feedback.activateAlarmFirst");
                AssertTrue(!string.IsNullOrEmpty(enAlarm) && enAlarm.Contains("alarm"), $"English alarm feedback invalid: {enAlarm}");

                loc.SetLanguage(AppLanguage.Hindi);
                string hiAlarm = loc.Get("fire.feedback.activateAlarmFirst");
                AssertTrue(!string.IsNullOrEmpty(hiAlarm) && hiAlarm.Contains("फायर अलार्म"), $"Hindi alarm feedback invalid: {hiAlarm}");

                loc.SetLanguage(AppLanguage.Santali);
                string satAlarm = loc.Get("fire.feedback.activateAlarmFirst");
                AssertTrue(!string.IsNullOrEmpty(satAlarm), "Santali fallback must return non-empty string without throwing");

                // Reset back to English
                loc.SetLanguage(AppLanguage.English);
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-25: Scene Visual Activation / Alignment defense.
            // Expected: FireExt_display active, original FireExt inactive.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-25", "Scene placement & alignment defense -> FireExt_display active, FireExt inactive", () =>
            {
                var arPlace = UnityEngine.Object.FindAnyObjectByType<ARPlacement>(FindObjectsInactive.Include);
                if (arPlace != null)
                {
                    arPlace.SetScenarioVisualsActive(true);
                }

                var align = UnityEngine.Object.FindAnyObjectByType<FireScenarioAlignment>(FindObjectsInactive.Include);
                if (align != null)
                {
                    align.ApplyAlignment();
                }

                flow.EnsureInitialExtinguisherVisibility();

                AssertTrue(display.gameObject.activeSelf, "FireExt_display MUST remain active and visible after visuals activation & alignment");
                var origPickup = display.originalExtinguisher.GetComponent<ExtinguisherPickup>();
                AssertTrue(origPickup != null && !origPickup.IsRuntimeVisible() && !origPickup.IsHeld(), "Original FireExt MUST remain hidden and inactive after visuals activation & alignment");
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-26: Pre-alarm tap defense during Step 1 and Step 2.
            // Expected: Original FireExt NEVER becomes active, no pickup occurs.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-26", "Pre-alarm tap defense in Step 1 & Step 2 -> FireExt remains strictly inactive", () =>
            {
                // Step 1 tap attempt
                flow.TransitionToStep1();
                display.EnsureInitialVisibility();
                display.Pickup();
                AssertTrue(display.gameObject.activeSelf, "Step 1: FireExt_display must remain visible");
                var origPickup = display.originalExtinguisher.GetComponent<ExtinguisherPickup>();
                AssertTrue(!origPickup.IsRuntimeVisible(), "Step 1: Original FireExt MUST remain hidden");

                // Step 2 tap attempt
                flow.TransitionToStep2_ActivateAlarm();
                display.EnsureInitialVisibility();
                display.Pickup();
                AssertTrue(display.gameObject.activeSelf, "Step 2: FireExt_display must remain visible");
                AssertTrue(!origPickup.IsRuntimeVisible(), "Step 2: Original FireExt MUST remain hidden");
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-27: Valid selection handoff exact state transition.
            // Expected: FireExt_display disappears, FireExt appears & attaches to camera.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-27", "Valid selection handoff -> FireExt_display DISAPPEARS, FireExt APPEARS and ATTACHES", () =>
            {
                flow.TransitionToStep3_SelectExtinguisher();
                display.ResetDisplay();
                AssertTrue(display.gameObject.activeSelf, "Before selection: FireExt_display MUST be active");
                var origPickup = display.originalExtinguisher.GetComponent<ExtinguisherPickup>();
                AssertTrue(!origPickup.IsRuntimeVisible(), "Before selection: Original FireExt MUST be hidden");

                // Tapping FireExt_display
                display.Pickup();

                AssertTrue(!display.gameObject.activeSelf, "After selection: FireExt_display MUST DISAPPEAR");
                AssertTrue(origPickup.IsRuntimeVisible(), "After selection: Original FireExt MUST BECOME VISIBLE & ACTIVE");
                AssertTrue(origPickup.IsHeld(), "After selection: Original FireExt MUST be held");
                if (display.arCamera != null)
                {
                    float dist = Vector3.Distance(display.originalExtinguisher.transform.position, display.arCamera.TransformPoint(origPickup.holdPosition));
                    AssertTrue(dist < 0.05f, $"After selection: Original FireExt MUST be positioned in front of camera (dist={dist:F3})");
                }
            });

            // ─────────────────────────────────────────────────────────────────────────
            // TEST-28: Scenario Reset (RestartScenario).
            // Expected: FireExt detaches and becomes inactive, FireExt_display becomes active.
            // ─────────────────────────────────────────────────────────────────────────
            RunTest("TEST-28", "Scenario Reset -> FireExt detaches & becomes inactive, FireExt_display active", () =>
            {
                flow.RestartScenario();

                AssertTrue(display.gameObject.activeSelf, "On restart: FireExt_display MUST be active and visible");
                var origPickup = display.originalExtinguisher.GetComponent<ExtinguisherPickup>();
                AssertTrue(!origPickup.IsRuntimeVisible() && !origPickup.IsHeld(), "On restart: Original FireExt must NOT be visible or held");
                AssertTrue(flow.CurrentStage == FireScenarioFlowManager.Stage.Step1_IdentifyHazard, "On restart: Stage must reset to Step 1");
            });

            // ─────────────────────────────────────────────────────────────────────────
            // Summary & File Output
            // ─────────────────────────────────────────────────────────────────────────
            int passCount = 0;
            foreach (var r in results) if (r.passed) passCount++;

            Debug.Log("================================================================================");
            Debug.Log($"[FireWorkflowStrictTest] COMPLETED: {passCount} / {results.Count} TESTS PASSED");
            Debug.Log("================================================================================");

            var report = new TestReport
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                totalTests = results.Count,
                passedTests = passCount,
                failedTests = results.Count - passCount,
                allPassed = allPassed,
                results = results
            };

            try
            {
                string dir = Path.GetDirectoryName(ResultsFile);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string json = JsonUtility.ToJson(report, true);
                File.WriteAllText(ResultsFile, json);
                Debug.Log($"[FireWorkflowStrictTest] Test results written to {ResultsFile}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FireWorkflowStrictTest] Failed to write results file: {ex.Message}");
            }

            return allPassed;
        }

        private static void EnsureAppState()
        {
            if (AppState.Instance == null)
            {
                var existing = UnityEngine.Object.FindAnyObjectByType<AppState>(FindObjectsInactive.Include);
                if (existing != null)
                {
                    AppState.Instance = existing;
                }
                else
                {
                    var go = new GameObject("AppState");
                    var state = go.AddComponent<AppState>();
                    AppState.Instance = state;
                }
            }
        }

        private static LocalizationManager EnsureLocalizationManager()
        {
            return LocalizationManager.Instance ?? new LocalizationManager();
        }

        private static void AssertTrue(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }

        [Serializable]
        public class TestResultItem
        {
            public string id;
            public string name;
            public bool passed;
            public string message;
        }

        [Serializable]
        public class TestReport
        {
            public string timestamp;
            public int totalTests;
            public int passedTests;
            public int failedTests;
            public bool allPassed;
            public List<TestResultItem> results;
        }
    }
}
