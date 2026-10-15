using System;
using UnityEngine;
using SurakshaAR.Data;

namespace SurakshaAR.Machinery
{
    /// <summary>
    /// ConveyorScenarioController
    /// ==========================
    /// Manages the 12-step Machinery AR Training & Assessment sequence:
    /// Step 1: Scan & Detect Plane
    /// Step 2: Place Conveyor
    /// Step 3: Conveyor Jam / Blockage Initialized
    /// Step 4: Identify Mechanical Hazard
    /// Step 5: Press Emergency Stop
    /// Step 6: Select Main Power Isolation Point
    /// Step 7: Switch Main Isolator OFF
    /// Step 8: Apply LOTO (Hasp + Padlock/Tag)
    /// Step 9: Verify Safe Zero Energy State
    /// Step 10: Clear Coal Jam
    /// Step 11: Restore Operation
    /// Step 12: Safe Operation Check & Scenario Completion
    /// </summary>
    public class ConveyorScenarioController : MonoBehaviour
    {
        public enum ScenarioStep
        {
            ScanAndDetectPlane = 1,
            PlaceConveyor = 2,
            ConveyorJamActive = 3,
            IdentifyHazard = 4,
            PressEmergencyStop = 5,
            SelectIsolationPoint = 6,
            SwitchIsolatorOff = 7,
            ApplyLOTO = 8,
            VerifySafeState = 9,
            ClearJam = 10,
            RestoreOperation = 11,
            SafeOperationCheck = 12,
            Completed = 13
        }

        public static ConveyorScenarioController Instance { get; private set; }

        [Header("Scenario Progression")]
        [SerializeField] private ScenarioStep currentStep = ScenarioStep.ScanAndDetectPlane;
        [SerializeField] private float stepStartTime;

        public ScenarioStep CurrentStep => currentStep;

        public event Action<ScenarioStep> OnStepChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            stepStartTime = Time.time;
            if (MachinerySession.Instance != null)
            {
                MachinerySession.Instance.OnStateChanged += HandleSessionStateChanged;
            }
            UpdateUIForCurrentStep();
        }

        private void OnDestroy()
        {
            if (MachinerySession.Instance != null)
            {
                MachinerySession.Instance.OnStateChanged -= HandleSessionStateChanged;
            }
        }

        private void HandleSessionStateChanged(MachinerySession.SessionState state)
        {
            if (state == MachinerySession.SessionState.Placed && currentStep < ScenarioStep.ConveyorJamActive)
            {
                SetStep(ScenarioStep.ConveyorJamActive);
            }
        }

        public void SetStep(ScenarioStep nextStep)
        {
            currentStep = nextStep;
            stepStartTime = Time.time;
            Debug.Log($"[CONVEYOR SCENARIO] Advanced to Step {(int)currentStep}: {currentStep}");
            
            UpdateUIForCurrentStep();
            OnStepChanged?.Invoke(currentStep);

            if (currentStep == ScenarioStep.Completed)
            {
                if (MachineryScenarioAssessmentBridge.Instance != null)
                {
                    MachineryScenarioAssessmentBridge.Instance.CompleteMachineryScenario(100f, true);
                }
                if (MachinerySession.Instance != null)
                {
                    MachinerySession.Instance.SetState(MachinerySession.SessionState.Completed);
                }
            }
        }

        public void UpdateUIForCurrentStep()
        {
            var ui = FindAnyObjectByType<MachineryScenarioUIController>();
            if (ui == null) return;

            bool isTraining = MachinerySession.Instance == null || MachinerySession.Instance.CurrentMode == MachinerySession.TrainingMode.Training;

            switch (currentStep)
            {
                case ScenarioStep.ScanAndDetectPlane:
                    ui.UpdateInstruction("Step 1: Scan Area", "Point camera at a flat floor to detect AR placement plane.");
                    break;
                case ScenarioStep.PlaceConveyor:
                    ui.UpdateInstruction("Step 2: Place Conveyor", "Tap blue placement ring to anchor conveyor setup.");
                    break;
                case ScenarioStep.ConveyorJamActive:
                    ui.UpdateInstruction("Step 3: Conveyor Jammed", isTraining ? "Conveyor has jammed! Inspect the machine safely." : "Machinery Jam Active.");
                    break;
                case ScenarioStep.IdentifyHazard:
                    ui.UpdateInstruction("Step 4: Identify Hazard", isTraining ? "Tap the jammed conveyor belt area to identify the mechanical hazard." : "Inspect setup.");
                    break;
                case ScenarioStep.PressEmergencyStop:
                    ui.UpdateInstruction("Step 5: Emergency Stop", isTraining ? "Press the red Emergency Stop button to halt conveyor motion." : "Halt equipment.");
                    break;
                case ScenarioStep.SelectIsolationPoint:
                    ui.UpdateInstruction("Step 6: Isolation Point", isTraining ? "Locate and tap the Main Power Isolator switch." : "Select isolation point.");
                    break;
                case ScenarioStep.SwitchIsolatorOff:
                    ui.UpdateInstruction("Step 7: Power Isolation", isTraining ? "Switch Main Isolator to OFF (0) to de-energize primary power." : "Isolate power.");
                    break;
                case ScenarioStep.ApplyLOTO:
                    ui.UpdateInstruction("Step 8: Apply LOTO", isTraining ? "Pick up Hasp & Padlock/Tag from workstation and attach to Main Isolator." : "Apply Lockout/Tagout.");
                    break;
                case ScenarioStep.VerifySafeState:
                    ui.UpdateInstruction("Step 9: Verify Zero Energy", isTraining ? "Test control panel start button to verify machine is fully de-energized." : "Verify zero energy state.");
                    break;
                case ScenarioStep.ClearJam:
                    ui.UpdateInstruction("Step 10: Clear Coal Jam", isTraining ? "Safely remove the coal blockage from the de-energized conveyor belt." : "Clear blockage.");
                    break;
                case ScenarioStep.RestoreOperation:
                    ui.UpdateInstruction("Step 11: Restore Operation", isTraining ? "Remove LOTO devices, switch Main Isolator ON, and reset E-Stop." : "Restore equipment.");
                    break;
                case ScenarioStep.SafeOperationCheck:
                    ui.UpdateInstruction("Step 12: Operation Check", "Verify clear workspace and restart conveyor safely.");
                    break;
                case ScenarioStep.Completed:
                    ui.UpdateInstruction("Drill Completed", "Conveyor Jam & LOTO Training Drill successfully completed!");
                    break;
            }
        }
    }
}
