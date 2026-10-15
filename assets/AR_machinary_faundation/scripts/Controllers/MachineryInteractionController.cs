using System;
using UnityEngine;
using SurakshaAR.Data;

namespace SurakshaAR.Machinery
{
    /// <summary>
    /// MachineryInteractionController
    /// ==============================
    /// Handles user touch/raycast interactions with 3D machinery elements:
    /// - Emergency Stop button
    /// - Main Power Isolator switch
    /// - Lockout Hasp & Padlock
    /// - Control Panel Start/Stop controls
    /// - Conveyor Belt & Coal Jam Area
    /// Routes interactions to the respective sub-controllers based on current scenario step.
    /// </summary>
    public class MachineryInteractionController : MonoBehaviour
    {
        public static MachineryInteractionController Instance { get; private set; }

        [Header("Target References")]
        [SerializeField] private GameObject conveyorAssemblyObject;
        [SerializeField] private GameObject emergencyStopObject;
        [SerializeField] private GameObject mainIsolatorObject;
        [SerializeField] private GameObject lotoStationObject;
        [SerializeField] private GameObject controlPanelObject;
        [SerializeField] private GameObject jamCoalAreaObject;

        [Header("Raycast Config")]
        [SerializeField] private Camera arCamera;
        [SerializeField] private LayerMask interactableLayerMask = ~0;

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
            if (arCamera == null)
            {
                arCamera = Camera.main;
            }
        }

        private void Update()
        {
            // Process mouse click or touch tap
            if (Input.GetMouseButtonDown(0))
            {
                HandlePointerDown(Input.mousePosition);
            }
            else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                HandlePointerDown(Input.GetTouch(0).position);
            }
        }

        public void HandlePointerDown(Vector3 screenPosition)
        {
            if (arCamera == null) arCamera = Camera.main;
            if (arCamera == null) return;

            Ray ray = arCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 50f, interactableLayerMask))
            {
                ProcessHitObject(hit.collider.gameObject);
            }
        }

        public void ProcessHitObject(GameObject hitObj)
        {
            if (hitObj == null) return;

            string name = hitObj.name.ToLower();
            var step = ConveyorScenarioController.Instance != null 
                ? ConveyorScenarioController.Instance.CurrentStep 
                : ConveyorScenarioController.ScenarioStep.IdentifyHazard;

            Debug.Log($"[MACHINERY INTERACTION] Tapped object: {hitObj.name}, Current Step: {step}");

            // 1. Hazard Identification Step
            if (step == ConveyorScenarioController.ScenarioStep.IdentifyHazard || step == ConveyorScenarioController.ScenarioStep.ConveyorJamActive)
            {
                if (name.Contains("conveyor") || name.Contains("coal") || name.Contains("hopper") || name.Contains("motor"))
                {
                    if (MachineryScenarioAssessmentBridge.Instance != null)
                    {
                        MachineryScenarioAssessmentBridge.Instance.OnMechanicalHazardIdentified("conveyor_nip_point_jam");
                    }
                    if (ConveyorScenarioController.Instance != null)
                    {
                        ConveyorScenarioController.Instance.SetStep(ConveyorScenarioController.ScenarioStep.PressEmergencyStop);
                    }
                    return;
                }
            }

            // 2. Emergency Stop Interaction
            if (name.Contains("emergency") || name.Contains("estop") || name.Contains("e_stop") || name.Contains("stop"))
            {
                if (EmergencyStopController.Instance != null)
                {
                    EmergencyStopController.Instance.PressEmergencyStop();
                }
                return;
            }

            // 3. Main Power Isolator Interaction
            if (name.Contains("isolator") || name.Contains("breaker") || name.Contains("disconnect") || name.Contains("switch"))
            {
                if (step == ConveyorScenarioController.ScenarioStep.RestoreOperation)
                {
                    if (LOTOController.Instance != null && LOTOController.Instance.IsLOTOApplied)
                    {
                        LOTOController.Instance.RemoveLOTO();
                    }
                    else if (IsolationController.Instance != null)
                    {
                        IsolationController.Instance.SwitchIsolatorOn();
                        if (ConveyorScenarioController.Instance != null)
                        {
                            ConveyorScenarioController.Instance.SetStep(ConveyorScenarioController.ScenarioStep.SafeOperationCheck);
                        }
                    }
                }
                else if (IsolationController.Instance != null)
                {
                    IsolationController.Instance.SwitchIsolatorOff();
                }
                return;
            }

            // 4. LOTO Hardware & Table Interaction
            if (name.Contains("hasp") || name.Contains("padlock") || name.Contains("tag") || name.Contains("table") || name.Contains("workstation"))
            {
                if (step == ConveyorScenarioController.ScenarioStep.RestoreOperation)
                {
                    if (LOTOController.Instance != null)
                    {
                        LOTOController.Instance.RemoveLOTO();
                    }
                }
                else if (LOTOController.Instance != null)
                {
                    LOTOController.Instance.ApplyLOTO();
                }
                return;
            }

            // 5. Control Panel / Zero Energy Test
            if (name.Contains("panel") || name.Contains("button") || name.Contains("start") || name.Contains("control"))
            {
                if (step == ConveyorScenarioController.ScenarioStep.VerifySafeState)
                {
                    if (SafeStateController.Instance != null)
                    {
                        SafeStateController.Instance.VerifyZeroEnergyState();
                    }
                }
                else if (step == ConveyorScenarioController.ScenarioStep.SafeOperationCheck)
                {
                    Debug.Log("[MACHINERY INTERACTION] Start button pressed. Conveyor system re-energized safely!");
                    if (EmergencyStopController.Instance != null && EmergencyStopController.Instance.IsPressed)
                    {
                        EmergencyStopController.Instance.ResetEmergencyStop();
                    }
                    if (ConveyorScenarioController.Instance != null)
                    {
                        ConveyorScenarioController.Instance.SetStep(ConveyorScenarioController.ScenarioStep.Completed);
                    }
                }
                return;
            }

            // 6. Coal Jam Clearing
            if (name.Contains("coal") || name.Contains("jam") || name.Contains("conveyor") || name.Contains("belt"))
            {
                if (JamController.Instance != null)
                {
                    JamController.Instance.AttemptClearJam();
                }
                return;
            }
        }
    }
}
