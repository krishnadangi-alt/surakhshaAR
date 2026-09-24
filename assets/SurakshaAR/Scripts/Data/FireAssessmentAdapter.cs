using System;
using System.Collections.Generic;
using UnityEngine;
using SurakshaAR.Core;

namespace SurakshaAR.Data
{
    /// <summary>
    /// FireAssessmentAdapter
    /// =====================
    /// Official Fire & Explosion Response module adapter.
    /// Bridges the 3D AR Fire scenario mechanics into the canonical 12 Common Assessment Events.
    /// Feeds CompetencyEngine, OfflineDataStore, and backend synchronization.
    ///
    /// Implements the standard 6-Step SOP:
    ///   1. Identify Hazard (Electrical Fire)
    ///   2. Activate Alarm
    ///   3. Select CO2 Extinguisher
    ///   4. Remove Safety Pin
    ///   5. Aim at Base of Fire
    ///   6. PASS Technique / Spray & Extinguish
    ///   (followed by safe evacuation)
    /// </summary>
    public class FireAssessmentAdapter : MonoBehaviour, IModuleAssessmentAdapter
    {
        public static FireAssessmentAdapter Instance { get; private set; }

        public string ModuleId => "1";
        public string ScenarioType => "fire";
        public string ActiveScenarioId => _activeScenarioId;

        public Dictionary<string, CompetencyEngine.CompetencyDef> SkillTaxonomy =>
            CompetencyEngine.GetDefinitions("fire");

        [Header("Runtime State")]
        [SerializeField] private string _activeScenarioId = "fire_drill_01";
        [SerializeField] private bool _isSessionActive = false;
        [SerializeField] private float _scenarioStartTime;
        [SerializeField] private float _lastActionTimestamp;
        [SerializeField] private int _workerId = 1;

        public bool IsSessionActive => _isSessionActive;
        public float ElapsedSeconds => _isSessionActive ? (Time.time - _scenarioStartTime) : 0f;

        private readonly List<AssessmentEvent> _recordedEvents = new List<AssessmentEvent>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (Application.isPlaying) DontDestroyOnLoad(gameObject);
        }

        private float GetDeltaTime()
        {
            float now = Time.time;
            float delta = _lastActionTimestamp > 0f ? (now - _lastActionTimestamp) : (now - _scenarioStartTime);
            _lastActionTimestamp = now;
            return Mathf.Max(0.1f, delta);
        }

        public void StartScenario(string scenarioId = "fire_drill_01")
        {
            _activeScenarioId = scenarioId;
            _scenarioStartTime = Time.time;
            _lastActionTimestamp = _scenarioStartTime;
            _isSessionActive = true;
            _recordedEvents.Clear();

            if (AuthSession.Instance != null && AuthSession.Instance.WorkerId > 0)
            {
                _workerId = AuthSession.Instance.WorkerId;
            }

            TrainingEventManager.RaiseScenarioStarted("fire", scenarioId);
            RecordEvent(AssessmentEvent.Create(CommonAssessmentEvents.SCENARIO_STARTED, "start_scenario", "info", false, 0f));
            Debug.Log($"[FIRE ADAPTER] Started Fire Scenario: {scenarioId} for Worker ID: {_workerId}");
        }

        public void CompleteScenario(float finalScore, bool passed)
        {
            if (!_isSessionActive) return;

            float duration = ElapsedSeconds;
            _isSessionActive = false;

            RecordEvent(AssessmentEvent.Create(CommonAssessmentEvents.SCENARIO_COMPLETED, "complete_scenario", passed ? "correct" : "wrong", false, duration));
            TrainingEventManager.RaiseScenarioCompleted("fire", finalScore, passed);
            TrainingEventManager.RaiseAssessmentCompleted(duration);

            // Run offline competency evaluation
            var result = CompetencyEngine.Evaluate("fire", _recordedEvents, duration);
            OfflineDataStore.Instance?.SaveAssessmentSession("fire", result, _recordedEvents);

            Debug.Log($"[FIRE ADAPTER] Completed scenario '{_activeScenarioId}' in {duration:F1}s. Provisional Score: {result.overall_score:F1}%, Passed: {result.passed}");
        }

        /// <summary>
        /// Combines AR scenario actions and knowledge assessment quiz into ONE unified assessment session.
        /// Evaluates all 5 competency dimensions and queues for background sync.
        /// </summary>
        public void FinalizeModuleAssessment(float finalScore, bool passed, AssessmentEvent quizEvent = null)
        {
            if (quizEvent != null)
            {
                RecordEvent(quizEvent);
            }
            float duration = Mathf.Max(15f, ElapsedSeconds);
            _isSessionActive = false;

            // Rule 10, 11, 20, 21: Evidence must come from REAL Fire AR interaction.
            // Synthetic event injection is disabled so only authentic worker actions are evaluated.

            var result = CompetencyEngine.Evaluate("fire", _recordedEvents, duration);
            result.overall_score = finalScore;
            result.passed = passed;

            OfflineDataStore.Instance?.SaveAssessmentSession("fire", result, _recordedEvents);
            OfflineSyncManager.Instance?.TriggerSync();

            Debug.Log($"[FIRE ADAPTER] Finalized Unified Assessment: Score={finalScore}%, Passed={passed}, Real Events={_recordedEvents.Count}");
        }

        private void EnsureComprehensiveFireEvents(bool passed)
        {
            // Deprecated: synthetic event injection disabled per SurakshaAR Rules 10, 11, 20, 21.
        }


        public void AbortScenario(string reason)
        {
            if (!_isSessionActive) return;
            _isSessionActive = false;
            RecordCriticalAction("abort_scenario", reason);
            TrainingEventManager.RaiseCriticalAction("abort_scenario", reason);
            Debug.LogWarning($"[FIRE ADAPTER] Scenario aborted: {reason}");
        }

        // ── Step 1: Identify Hazard ──
        public void RecordHazardIdentified(string hazardType, bool correct, float responseTime = 0f)
        {
            float rt = responseTime > 0f ? responseTime : GetDeltaTime();
            var ev = AssessmentEvent.CreateHazardIdentified(hazardType, rt);
            ev.correct = correct;
            ev.result = correct ? "correct" : "wrong";
            RecordEvent(ev);

            if (correct)
            {
                TrainingEventManager.RaiseHazardIdentified();
            }
            else
            {
                RecordWrongAction("wrong_hazard_selected", $"Selected incorrect hazard: {hazardType}", "minor");
            }
        }

        // ── Step 2: Alarm Activation ──
        public void RecordAlarmActivated(float responseTime = 0f)
        {
            float rt = responseTime > 0f ? responseTime : GetDeltaTime();
            RecordCorrectAction("activate_alarm", rt);
            TrainingEventManager.RaiseAlarmActivated();
        }

        // ── Step 3: Select Extinguisher ──
        public void RecordEquipmentSelected(string equipmentType, bool correct, float responseTime = 0f)
        {
            float rt = responseTime > 0f ? responseTime : GetDeltaTime();
            var ev = AssessmentEvent.CreateEquipmentSelected(equipmentType, correct, rt);
            RecordEvent(ev);

            if (correct)
            {
                TrainingEventManager.RaiseExtinguisherPickedUp();
            }
            else
            {
                RecordWrongAction("wrong_extinguisher_selected", $"Selected {equipmentType} for electrical fire", "major");
            }
        }

        // ── Step 4: Remove Safety Pin ──
        public void RecordPinRemoved(float responseTime = 0f)
        {
            float rt = responseTime > 0f ? responseTime : GetDeltaTime();
            RecordCorrectAction("remove_safety_pin", rt);
            TrainingEventManager.RaisePinRemoved();
        }

        // ── Step 5: Aim at Base ──
        public void RecordAimAtBase(bool correct, float responseTime = 0f)
        {
            float rt = responseTime > 0f ? responseTime : GetDeltaTime();
            if (correct)
            {
                RecordCorrectAction("aim_at_base_of_fire", rt);
            }
            else
            {
                RecordWrongAction("aim_off_target", "Aimed above or away from fire base", "minor");
            }
        }

        // ── Step 6: PASS Technique / Spray & Extinguish ──
        public void RecordSprayAction(bool validContact, float durationSeconds)
        {
            float rt = GetDeltaTime();
            if (validContact)
            {
                RecordCorrectAction("pass_technique_spray", rt);
                TrainingEventManager.RaiseExtinguisherUsed();
            }
            else
            {
                RecordWrongAction("spray_contact_interrupted", "Spray interrupted or aimed away from base", "minor");
            }
        }

        public void RecordFireExtinguished(float totalExtinguishTime)
        {
            RecordCorrectAction("fire_extinguished", totalExtinguishTime);
            TrainingEventManager.RaiseFireExtinguished();
        }

        // ── Step 7: Safe Evacuation ──
        public void RecordEvacuation(bool safe, string route = "emergency_exit_A")
        {
            var ev = new AssessmentEvent("evacuation_started");
            ev.safe = safe;
            ev.correct = safe;
            ev.route = route;
            ev.action = "evacuate";
            ev.result = safe ? "correct" : "unsafe";
            RecordEvent(ev);

            TrainingEventManager.RaiseEvacuationStarted(route, safe);
        }

        // ── Standard Actions & Penalties ──
        public void RecordPpeSelected(string ppeType, bool correct, float responseTime = 0f)
        {
            float rt = responseTime > 0f ? responseTime : GetDeltaTime();
            var ev = AssessmentEvent.CreatePpeSelected(ppeType, correct, rt);
            RecordEvent(ev);
            TrainingEventManager.RaisePpeSelected(ppeType, correct, rt);
        }

        public void RecordCorrectAction(string action, float responseTime = 0f)
        {
            float rt = responseTime > 0f ? responseTime : GetDeltaTime();
            var ev = AssessmentEvent.CreateCorrectAction(action, rt);
            RecordEvent(ev);
            TrainingEventManager.RaiseCorrectAction(action, rt);
        }

        public void RecordWrongAction(string action, string reason, string severity = "minor")
        {
            var ev = AssessmentEvent.CreateWrongAction(action, reason, severity);
            RecordEvent(ev);
            TrainingEventManager.RaiseWrongAction(action, severity, reason);
        }

        public void RecordUnsafeAction(string action, string reason)
        {
            var ev = AssessmentEvent.CreateUnsafeAction(action, reason);
            RecordEvent(ev);
            TrainingEventManager.RaiseUnsafeAction(action, reason);
        }

        public void RecordCriticalAction(string action, string reason)
        {
            var ev = AssessmentEvent.CreateCriticalAction(action, reason);
            RecordEvent(ev);
            TrainingEventManager.RaiseCriticalAction(action, reason);
        }

        public void RecordSequenceError(string expectedAction, string actualAction)
        {
            var ev = AssessmentEvent.CreateSequenceError(expectedAction, actualAction);
            RecordEvent(ev);
            TrainingEventManager.RaiseSequenceError(expectedAction, actualAction);
        }

        public void RecordTimeout()
        {
            RecordCriticalAction("training_timed_out", "Exceeded 420-second (7-minute) training time limit");
        }

        // ── Fine-Grained Grip / Aim / Spray Events (per Master Implementation Rule) ──

        /// <summary>Records the first successful grip activation after pin removal.</summary>
        public void RecordGripActivated(float responseTime = 0f)
        {
            float rt = responseTime > 0f ? responseTime : GetDeltaTime();
            RecordCorrectAction("grip_activated", rt);
            TrainingEventManager.RaiseGripActivated();
        }

        /// <summary>Records a confirmed valid aim at the base of the fire.</summary>
        public void RecordValidAim(float responseTime = 0f)
        {
            float rt = responseTime > 0f ? responseTime : GetDeltaTime();
            RecordCorrectAction("valid_aim", rt);
            TrainingEventManager.RaiseValidAim();
        }

        /// <summary>Records an invalid aim event (rate-limited by FlowManager penalty cooldown).</summary>
        public void RecordInvalidAim()
        {
            RecordWrongAction("invalid_aim", "Nozzle aimed away from fire base", "minor");
            TrainingEventManager.RaiseInvalidAim();
        }

        /// <summary>Records the transition into valid spray contact with the fire.</summary>
        public void RecordSprayContactValid()
        {
            float rt = GetDeltaTime();
            var ev = AssessmentEvent.Create(CommonAssessmentEvents.OBJECT_INTERACTION, "spray_contact_valid", "correct", false, rt);
            RecordEvent(ev);
            TrainingEventManager.RaiseSprayContactValid();
        }

        /// <summary>Records the moment spray contact with the fire is first lost (grace period begins).</summary>
        public void RecordSprayInterrupted()
        {
            RecordWrongAction("spray_interrupted", "Spray contact with fire lost — grace period started", "minor");
            TrainingEventManager.RaiseSprayInterrupted();
        }

        /// <summary>Records a full contact timer reset after grace period expires.</summary>
        public void RecordSprayContactReset()
        {
            RecordWrongAction("spray_contact_reset", "Grace period exceeded — contact timer reset to 0", "minor");
            TrainingEventManager.RaiseSprayContactReset();
        }

        /// <summary>Records an attempt to spray before the safety pin has been removed.</summary>
        public void RecordPrematureSprayAttempt()
        {
            RecordUnsafeAction("premature_spray_attempt", "Attempted to spray before safety pin was removed");
            RecordSequenceError("remove_safety_pin", "spray_attempt");
            TrainingEventManager.RaisePrematureSprayAttempt();
        }


        private void RecordEvent(AssessmentEvent ev)
        {
            if (ev == null) return;
            if (AuthSession.Instance != null && AuthSession.Instance.WorkerId > 0)
            {
                _workerId = AuthSession.Instance.WorkerId;
            }
            ev.worker_id = _workerId;
            ev.module = "fire";
            ev.scenario = _activeScenarioId;
            _recordedEvents.Add(ev);
        }
    }
}
