using System;
using UnityEngine;
using SurakshaAR.Data;

namespace SurakshaAR.Data
{
    /// <summary>
    /// MachineryScenarioAssessmentBridge
    /// ================================
    /// Bridge connecting Harshita's Industrial Machinery & LOTO AR scenario to Rehan's Assessment Engine.
    /// Maps mechanical hazard identification, pinch point checks, LOTO isolation steps,
    /// machine guard verification, and emergency stop actuation to the 12 Common Assessment Events.
    /// 
    /// Enforces:
    ///   - Automatic FAIL on servicing energized machinery without LOTO.
    ///   - Sub-second reaction time tracking on emergency stop (E-stop).
    /// </summary>
    public class MachineryScenarioAssessmentBridge : MonoBehaviour
    {
        public static MachineryScenarioAssessmentBridge Instance { get; private set; }

        [Header("Scenario State")]
        public string activeScenarioId = "machinery_loto_drill_01";
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

        public void StartMachineryScenario(string scenarioId = "machinery_loto_drill_01")
        {
            activeScenarioId = scenarioId;
            scenarioStartTime = Time.time;
            _lastActionTimestamp = scenarioStartTime;
            isSessionActive = true;

            TrainingEventManager.RaiseScenarioStarted("machinery", scenarioId);
            Debug.Log($"[MACHINERY ASSESSMENT] Machinery Scenario Started: {scenarioId}");
        }

        /// <summary>
        /// Worker identifies an in-running nip point or unshielded drive belt.
        /// </summary>
        public void OnMechanicalHazardIdentified(string hazardType = "exposed_drive_pinch_point")
        {
            float responseTime = GetDeltaTime();
            TrainingEventManager.RaiseHazardIdentified(hazardType, responseTime);
            Debug.Log($"[MACHINERY ASSESSMENT] Mechanical hazard identified ({hazardType}) in {responseTime:F2}s");
        }

        /// <summary>
        /// Worker selects mechanical safety PPE (safety goggles, hearing protection, tucked hair/clothing).
        /// </summary>
        public void OnPpeSelected(string ppeType, bool isSafeForMachinery)
        {
            float responseTime = GetDeltaTime();
            TrainingEventManager.RaisePpeSelected(ppeType, isSafeForMachinery, responseTime);

            if (!isSafeForMachinery)
            {
                // Loose gloves or dangling lanyards near rotating parts is an extreme entanglement hazard
                TrainingEventManager.RaiseUnsafeAction("entanglement_risk_ppe", 
                    $"Selected {ppeType} poses an entanglement danger around rotating machinery");
            }
        }

        /// <summary>
        /// Worker executes LOTO step 1: Isolates the primary electrical breaker.
        /// </summary>
        public void OnBreakerSwitchedOff()
        {
            float responseTime = GetDeltaTime();
            TrainingEventManager.RaiseCorrectAction("loto_isolate_power_breaker", responseTime);
            TrainingEventManager.RaiseObjectInteraction("main_breaker", "switch_off", responseTime);
        }

        /// <summary>
        /// Worker executes LOTO step 2: Applies safety lockout hasp and personal padlock.
        /// </summary>
        public void OnPadlockApplied(bool haspApplied)
        {
            float responseTime = GetDeltaTime();
            if (haspApplied)
            {
                TrainingEventManager.RaiseCorrectAction("loto_apply_padlock_and_hasp", responseTime);
            }
            else
            {
                TrainingEventManager.RaiseSequenceError("apply_lockout_hasp_first", "applied_tag_without_lock");
            }
        }

        /// <summary>
        /// Worker executes LOTO step 3: Affixes danger tag and verifies zero energy state.
        /// </summary>
        public void OnZeroEnergyVerified(bool testedControls)
        {
            float responseTime = GetDeltaTime();
            if (testedControls)
            {
                TrainingEventManager.RaiseCorrectAction("loto_verify_zero_energy", responseTime);
            }
            else
            {
                TrainingEventManager.RaiseWrongAction("skipped_zero_energy_test", "major", 
                    "Worker failed to test start button to verify machine was fully de-energized");
            }
        }

        /// <summary>
        /// Worker attempts to clear a jam or service machine while energized.
        /// CRITICAL SAFETY VIOLATION: Instant Automatic FAIL.
        /// </summary>
        public void OnServiceWithoutLockout()
        {
            TrainingEventManager.RaiseCriticalAction("servicing_energized_machinery", 
                "Worker reached into active machine drive to clear jam without de-energizing and locking out power");
        }

        /// <summary>
        /// Worker hits the Emergency Stop button during an unexpected jam or entanglement.
        /// </summary>
        public void OnEmergencyStopPressed(float reactionSeconds)
        {
            TrainingEventManager.RaiseResponseTime("e_stop_actuation", reactionSeconds);
            if (reactionSeconds <= 2.5f)
            {
                TrainingEventManager.RaiseCorrectAction("rapid_e_stop_reaction", reactionSeconds);
            }
            else
            {
                TrainingEventManager.RaiseWrongAction("delayed_e_stop_reaction", "minor", 
                    $"E-stop reaction took {reactionSeconds:F2}s (standard threshold is < 2.5s)");
            }
        }

        /// <summary>
        /// Worker safely restores machine guards and completes drill.
        /// </summary>
        public void CompleteMachineryScenario(float finalScore, bool isPassing)
        {
            isSessionActive = false;
            float totalDuration = Time.time - scenarioStartTime;
            TrainingEventManager.RaiseScenarioCompleted("machinery", finalScore, isPassing);
            TrainingEventManager.RaiseAssessmentCompleted(totalDuration);
            Debug.Log($"[MACHINERY ASSESSMENT] Machinery Scenario Completed: Score={finalScore:F1}%, Passed={isPassing}, Duration={totalDuration:F1}s");
        }
    }
}
