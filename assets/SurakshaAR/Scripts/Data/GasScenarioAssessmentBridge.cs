using System;
using UnityEngine;
using SurakshaAR.Data;

namespace SurakshaAR.Data
{
    /// <summary>
    /// GasScenarioAssessmentBridge
    /// ===========================
    /// Bridge connecting Anjani's Gas & Confined Space AR scenario to Rehan's Assessment Engine.
    /// Maps physical triggers (gas leak, detector alarms, hazard boundaries, SCBA selection,
    /// isolation valves, and buddy checks) to the 12 Common Assessment Events.
    /// 
    /// Enforces:
    ///   - Zero-tolerance critical error on downwind entry or solitary confined space entry.
    ///   - Response time tracking for toxic gas detection and isolation.
    /// </summary>
    public class GasScenarioAssessmentBridge : MonoBehaviour
    {
        public static GasScenarioAssessmentBridge Instance { get; private set; }

        [Header("Scenario State")]
        public string activeScenarioId = "gas_leak_drill_01";
        public float scenarioStartTime;
        public bool isSessionActive;

        private float _lastActionTimestamp;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private float GetDeltaTime()
        {
            float now = Time.time;
            float delta = _lastActionTimestamp > 0f ? (now - _lastActionTimestamp) : (now - scenarioStartTime);
            _lastActionTimestamp = now;
            return Mathf.Max(0.1f, delta);
        }

        public void StartGasScenario(string scenarioId = "gas_leak_drill_01")
        {
            activeScenarioId = scenarioId;
            scenarioStartTime = Time.time;
            _lastActionTimestamp = scenarioStartTime;
            isSessionActive = true;

            TrainingEventManager.RaiseScenarioStarted("gas", scenarioId);
            Debug.Log($"[GAS ASSESSMENT] Gas Scenario Started: {scenarioId}");
        }

        /// <summary>
        /// Worker detects visual gas cloud or audible hissing from cylinder/pipeline.
        /// </summary>
        public void OnGasLeakDetected(string gasType = "methane_toxic_gas")
        {
            float responseTime = GetDeltaTime();
            TrainingEventManager.RaiseHazardIdentified(gasType, responseTime);
            Debug.Log($"[GAS ASSESSMENT] Gas leak identified ({gasType}) in {responseTime:F2}s");
        }

        /// <summary>
        /// Worker identifies the high-risk hazard zone perimeter via gas detector reading.
        /// </summary>
        public void OnHazardZonePerimeterIdentified(float ppmReading)
        {
            float responseTime = GetDeltaTime();
            TrainingEventManager.RaiseObjectInteraction("danger_zone_perimeter", "perimeter_recognition", responseTime);
            Debug.Log($"[GAS ASSESSMENT] Hazard perimeter identified at {ppmReading:F1} PPM in {responseTime:F2}s");
        }

        /// <summary>
        /// Worker selects PPE (e.g. SCBA breathing apparatus, respirator, or chemical suit).
        /// </summary>
        public void OnPpeSelected(string ppeType, bool isAppropriateForGas)
        {
            float responseTime = GetDeltaTime();
            TrainingEventManager.RaisePpeSelected(ppeType, isAppropriateForGas, responseTime);

            if (!isAppropriateForGas)
            {
                TrainingEventManager.RaiseWrongAction("inadequate_gas_ppe", "major", $"Selected {ppeType} is not certified for toxic gas atmosphere");
            }
            Debug.Log($"[GAS ASSESSMENT] PPE Selected: {ppeType} (Valid: {isAppropriateForGas}) in {responseTime:F2}s");
        }

        /// <summary>
        /// Worker interacts with the isolation valve to stop the source of the leak.
        /// </summary>
        public void OnIsolationValveClosed(bool isCorrectSequence)
        {
            float responseTime = GetDeltaTime();
            if (isCorrectSequence)
            {
                TrainingEventManager.RaiseCorrectAction("close_gas_isolation_valve", responseTime);
                TrainingEventManager.RaiseObjectInteraction("isolation_valve", "valve_closed", responseTime);
            }
            else
            {
                TrainingEventManager.RaiseSequenceError("verify_detector_first", "closed_valve_prematurely");
            }
        }

        /// <summary>
        /// Worker activates positive pressure exhaust ventilation.
        /// </summary>
        public void OnVentilationActivated()
        {
            float responseTime = GetDeltaTime();
            TrainingEventManager.RaiseCorrectAction("activate_forced_ventilation", responseTime);
            TrainingEventManager.RaiseEquipmentSelected("exhaust_blower", true, responseTime);
        }

        /// <summary>
        /// Worker checks buddy system status before entering hazard area or confined space.
        /// </summary>
        public void OnBuddySystemChecked(bool buddyPresent)
        {
            float responseTime = GetDeltaTime();
            if (buddyPresent)
            {
                TrainingEventManager.RaiseCorrectAction("buddy_system_verified", responseTime);
            }
            else
            {
                // CRITICAL SAFETY VIOLATION: Solitary entry into confined space / gas area is instant FAIL
                TrainingEventManager.RaiseCriticalAction("confined_space_entry_without_buddy", 
                    "Worker entered toxic gas / confined space zone without a designated standby buddy");
            }
        }

        /// <summary>
        /// Worker performs an unsafe movement, such as walking downwind into the plume.
        /// </summary>
        public void OnUnsafeEvacuationDirection(string windDirection, string workerDirection)
        {
            // CRITICAL ERROR: Downwind movement guarantees toxic inhalation
            TrainingEventManager.RaiseCriticalAction("downwind_gas_evacuation", 
                $"Worker evacuated downwind towards {workerDirection}, entering dense toxic cloud");
        }

        /// <summary>
        /// Worker safely evacuates crosswind/upwind and completes the scenario drill.
        /// </summary>
        public void CompleteGasScenario(float finalScore, bool isPassing)
        {
            isSessionActive = false;
            float totalDuration = Time.time - scenarioStartTime;
            TrainingEventManager.RaiseScenarioCompleted("gas", finalScore, isPassing);
            TrainingEventManager.RaiseAssessmentCompleted(totalDuration);
            Debug.Log($"[GAS ASSESSMENT] Gas Scenario Completed: Score={finalScore:F1}%, Passed={isPassing}, Duration={totalDuration:F1}s");
        }
    }
}
