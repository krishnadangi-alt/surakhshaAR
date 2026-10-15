using System;
using UnityEngine;
using SurakshaAR.Data;

namespace SurakshaAR.Machinery
{
    /// <summary>
    /// LOTOController
    /// ==============
    /// Manages Lockout/Tagout attachment on the Main Power Isolator:
    /// - Moves Hasp and Padlock/Tag from Workstation Table to Main Isolator locking tab.
    /// - Verifies that Main Power Isolator is OFF before lock application.
    /// - Raises assessment event via MachineryScenarioAssessmentBridge.
    /// </summary>
    public class LOTOController : MonoBehaviour
    {
        public static LOTOController Instance { get; private set; }

        [Header("LOTO Hardware References")]
        [SerializeField] private GameObject haspOnTable;
        [SerializeField] private GameObject padlockOnTable;
        [SerializeField] private GameObject haspOnIsolator;
        [SerializeField] private GameObject padlockOnIsolator;

        [Header("State")]
        [SerializeField] private bool isHaspApplied = false;
        [SerializeField] private bool isPadlockApplied = false;

        public bool IsHaspApplied => isHaspApplied;
        public bool IsPadlockApplied => isPadlockApplied;
        public bool IsLOTOApplied => isHaspApplied && isPadlockApplied;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void ApplyLOTO()
        {
            if (IsLOTOApplied) return;

            // Check if isolator is OFF
            bool isolatorOff = IsolationController.Instance != null && IsolationController.Instance.IsIsolated;
            if (!isolatorOff)
            {
                Debug.LogWarning("[LOTO CONTROLLER] Attempted to apply LOTO while Main Isolator is still ON!");
                if (MachineryScenarioAssessmentBridge.Instance != null)
                {
                    MachineryScenarioAssessmentBridge.Instance.OnPadlockApplied(haspApplied: false);
                }
                var ui = FindAnyObjectByType<MachineryScenarioUIController>();
                if (ui != null)
                {
                    ui.SetStatusMessage("SEQUENCE ERROR: Switch Main Isolator OFF before attaching Lockout Hasp!");
                }
                return;
            }

            isHaspApplied = true;
            isPadlockApplied = true;

            // Toggle visual representations
            if (haspOnTable != null) haspOnTable.SetActive(false);
            if (padlockOnTable != null) padlockOnTable.SetActive(false);

            if (haspOnIsolator != null) haspOnIsolator.SetActive(true);
            if (padlockOnIsolator != null) padlockOnIsolator.SetActive(true);

            if (MachineryScenarioAssessmentBridge.Instance != null)
            {
                MachineryScenarioAssessmentBridge.Instance.OnPadlockApplied(haspApplied: true);
            }

            Debug.Log("[LOTO CONTROLLER] Safety Hasp and Padlock/Tag successfully attached to Main Power Isolator.");

            if (ConveyorScenarioController.Instance != null &&
                ConveyorScenarioController.Instance.CurrentStep == ConveyorScenarioController.ScenarioStep.ApplyLOTO)
            {
                ConveyorScenarioController.Instance.SetStep(ConveyorScenarioController.ScenarioStep.VerifySafeState);
            }
        }

        public void RemoveLOTO()
        {
            if (!IsLOTOApplied) return;

            isHaspApplied = false;
            isPadlockApplied = false;

            if (haspOnTable != null) haspOnTable.SetActive(true);
            if (padlockOnTable != null) padlockOnTable.SetActive(true);

            if (haspOnIsolator != null) haspOnIsolator.SetActive(false);
            if (padlockOnIsolator != null) padlockOnIsolator.SetActive(false);

            Debug.Log("[LOTO CONTROLLER] LOTO hardware removed from Main Isolator.");
        }
    }
}
