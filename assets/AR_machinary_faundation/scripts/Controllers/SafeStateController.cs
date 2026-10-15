using System;
using UnityEngine;
using SurakshaAR.Data;

namespace SurakshaAR.Machinery
{
    /// <summary>
    /// SafeStateController
    /// ==================
    /// Handles Zero Energy State verification:
    /// - Worker tests Control Panel start button after LOTO application to verify 0V / de-energized state.
    /// - Raises zero energy verification event via MachineryScenarioAssessmentBridge.
    /// </summary>
    public class SafeStateController : MonoBehaviour
    {
        public static SafeStateController Instance { get; private set; }

        [Header("State")]
        [SerializeField] private bool isSafeStateVerified = false;

        public bool IsSafeStateVerified => isSafeStateVerified;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void VerifyZeroEnergyState()
        {
            bool isIsolated = IsolationController.Instance != null && IsolationController.Instance.IsIsolated;
            bool isLotoApplied = LOTOController.Instance != null && LOTOController.Instance.IsLOTOApplied;

            if (isIsolated && isLotoApplied)
            {
                isSafeStateVerified = true;

                if (MachineryScenarioAssessmentBridge.Instance != null)
                {
                    MachineryScenarioAssessmentBridge.Instance.OnZeroEnergyVerified(testedControls: true);
                }

                Debug.Log("[SAFE STATE CONTROLLER] Zero Energy State verified! Control panel start test returned zero motion.");

                if (ConveyorScenarioController.Instance != null &&
                    ConveyorScenarioController.Instance.CurrentStep == ConveyorScenarioController.ScenarioStep.VerifySafeState)
                {
                    ConveyorScenarioController.Instance.SetStep(ConveyorScenarioController.ScenarioStep.ClearJam);
                }
            }
            else
            {
                if (MachineryScenarioAssessmentBridge.Instance != null)
                {
                    MachineryScenarioAssessmentBridge.Instance.OnZeroEnergyVerified(testedControls: false);
                }

                var ui = FindAnyObjectByType<MachineryScenarioUIController>();
                if (ui != null)
                {
                    ui.SetStatusMessage("UNSAFE TEST: Main isolator must be switched OFF and LOTO applied before verifying zero energy!");
                }

                Debug.LogWarning("[SAFE STATE CONTROLLER] Zero energy test failed: Machinery is not properly isolated and locked out.");
            }
        }
    }
}
