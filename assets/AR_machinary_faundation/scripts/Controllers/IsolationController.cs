using System;
using UnityEngine;
using SurakshaAR.Data;

namespace SurakshaAR.Machinery
{
    /// <summary>
    /// IsolationController
    /// ===================
    /// Manages the Main Power Isolator switch status and visual handle orientation:
    /// - Switch OFF: isolates primary power supply.
    /// - Switch ON: restores primary electrical connection (only if LOTO is removed).
    /// - Raises breaker isolated event via MachineryScenarioAssessmentBridge.
    /// </summary>
    public class IsolationController : MonoBehaviour
    {
        public static IsolationController Instance { get; private set; }

        [Header("Isolator Visuals")]
        [SerializeField] private Transform isolatorHandleMesh;
        [SerializeField] private Vector3 offRotationEuler = new Vector3(0f, 0f, 90f);
        [SerializeField] private Vector3 onRotationEuler = Vector3.zero;

        [Header("State")]
        [SerializeField] private bool isIsolated = false;

        public bool IsIsolated => isIsolated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SwitchIsolatorOff()
        {
            if (isIsolated) return;

            isIsolated = true;

            if (isolatorHandleMesh != null)
            {
                isolatorHandleMesh.localRotation = Quaternion.Euler(offRotationEuler);
            }

            if (MachineryScenarioAssessmentBridge.Instance != null)
            {
                MachineryScenarioAssessmentBridge.Instance.OnBreakerSwitchedOff();
            }

            Debug.Log("[ISOLATION CONTROLLER] Main Power Isolator switched to OFF (Isolated).");

            if (ConveyorScenarioController.Instance != null)
            {
                if (ConveyorScenarioController.Instance.CurrentStep == ConveyorScenarioController.ScenarioStep.SelectIsolationPoint ||
                    ConveyorScenarioController.Instance.CurrentStep == ConveyorScenarioController.ScenarioStep.SwitchIsolatorOff)
                {
                    ConveyorScenarioController.Instance.SetStep(ConveyorScenarioController.ScenarioStep.ApplyLOTO);
                }
            }
        }

        public void SwitchIsolatorOn()
        {
            if (!isIsolated) return;

            // Check if LOTO device is still attached
            if (LOTOController.Instance != null && LOTOController.Instance.IsLOTOApplied)
            {
                Debug.LogWarning("[ISOLATION CONTROLLER] Cannot switch ON Main Isolator while LOTO padlock is attached!");
                var ui = FindAnyObjectByType<MachineryScenarioUIController>();
                if (ui != null)
                {
                    ui.SetStatusMessage("SAFETY LOCKOUT: Remove Lockout/Tagout before energizing power!");
                }
                return;
            }

            isIsolated = false;

            if (isolatorHandleMesh != null)
            {
                isolatorHandleMesh.localRotation = Quaternion.Euler(onRotationEuler);
            }

            Debug.Log("[ISOLATION CONTROLLER] Main Power Isolator switched to ON (Energized).");
        }
    }
}
