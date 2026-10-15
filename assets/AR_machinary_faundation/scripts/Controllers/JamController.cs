using System;
using UnityEngine;
using SurakshaAR.Data;

namespace SurakshaAR.Machinery
{
    /// <summary>
    /// JamController
    /// =============
    /// Handles coal jam clearing interaction and strictly enforces LOTO safety sequence:
    /// - PREVENT jam clearing if machine is energized or not locked out (calls OnServiceWithoutLockout).
    /// - ALLOW jam clearing once Zero Energy State is verified.
    /// - Clears coal blockage visuals upon safe execution.
    /// </summary>
    public class JamController : MonoBehaviour
    {
        public static JamController Instance { get; private set; }

        [Header("Jam Visuals")]
        [SerializeField] private GameObject jamCoalRoot;

        [Header("State")]
        [SerializeField] private bool isJamCleared = false;

        public bool IsJamCleared => isJamCleared;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void AttemptClearJam()
        {
            if (isJamCleared) return;

            // Check if zero energy / safe state has been verified
            bool isSafe = SafeStateController.Instance != null && SafeStateController.Instance.IsSafeStateVerified;

            if (!isSafe)
            {
                // CRITICAL SAFETY VIOLATION: Reaching into active machine without LOTO
                Debug.LogError("[JAM CONTROLLER] CRITICAL SAFETY FAILURE: Reached into conveyor belt without Zero Energy LOTO isolation!");
                
                if (MachineryScenarioAssessmentBridge.Instance != null)
                {
                    MachineryScenarioAssessmentBridge.Instance.OnServiceWithoutLockout();
                }

                var ui = FindAnyObjectByType<MachineryScenarioUIController>();
                if (ui != null)
                {
                    ui.SetStatusMessage("CRITICAL SAFETY FAILURE! Servicing energized machinery is prohibited. Isolate & LOTO first!");
                }
                return;
            }

            // Safe state verified: perform jam clearing
            isJamCleared = true;

            if (jamCoalRoot != null)
            {
                jamCoalRoot.SetActive(false);
            }

            TrainingEventManager.RaiseCorrectAction("jam_cleared_safely", 2.0f);
            Debug.Log("[JAM CONTROLLER] Coal blockage successfully and safely removed from de-energized conveyor belt.");

            if (ConveyorScenarioController.Instance != null &&
                ConveyorScenarioController.Instance.CurrentStep == ConveyorScenarioController.ScenarioStep.ClearJam)
            {
                ConveyorScenarioController.Instance.SetStep(ConveyorScenarioController.ScenarioStep.RestoreOperation);
            }
        }
    }
}
