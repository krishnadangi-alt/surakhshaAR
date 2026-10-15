using System;
using UnityEngine;
using SurakshaAR.Data;

namespace SurakshaAR.Machinery
{
    /// <summary>
    /// MachinerySession
    /// ================
    /// Foundation session controller for the Industrial Machinery & LOTO AR module.
    /// Manages session lifecycle, mode selection (Training vs. Assessment), and hooks into
    /// the central event manager (TrainingEventManager) and assessment bridge (MachineryScenarioAssessmentBridge).
    /// </summary>
    public class MachinerySession : MonoBehaviour
    {
        public enum SessionState
        {
            Uninitialized,
            SearchingForPlanes,
            PlacementReady,
            Placed,
            InProgress,
            Completed,
            Failed
        }

        public enum TrainingMode
        {
            Training,   // Guided with step hints, feedback, retry
            Assessment  // Independent worker evaluation
        }

        [Header("Session Configuration")]
        [SerializeField] private string scenarioId = "machinery_loto_drill_01";
        [SerializeField] private TrainingMode currentMode = TrainingMode.Training;

        [Header("Session State")]
        [SerializeField] private SessionState currentState = SessionState.Uninitialized;
        [SerializeField] private float sessionStartTime;

        public static MachinerySession Instance { get; private set; }

        public SessionState CurrentState => currentState;
        public TrainingMode CurrentMode => currentMode;
        public string ScenarioId => scenarioId;

        public event Action<SessionState> OnStateChanged;

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
            InitializeSession();
        }

        /// <summary>
        /// Initializes the Machinery AR session and notifies system listeners.
        /// </summary>
        public void InitializeSession()
        {
            sessionStartTime = Time.time;
            SetState(SessionState.SearchingForPlanes);

            // Signal training event manager
            TrainingEventManager.RaiseTrainingSelected("MachinerySafety");
            TrainingEventManager.RaiseTrainingStarted();

            // Connect to existing Assessment Bridge if available
            if (MachineryScenarioAssessmentBridge.Instance != null)
            {
                MachineryScenarioAssessmentBridge.Instance.StartMachineryScenario(scenarioId);
            }

            Debug.Log($"[MACHINERY SESSION] Session initialized. Scenario: {scenarioId}, Mode: {currentMode}");
        }

        /// <summary>
        /// Updates the current session state and triggers delegates.
        /// </summary>
        public void SetState(SessionState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            Debug.Log($"[MACHINERY SESSION] State changed to: {newState}");
            OnStateChanged?.Invoke(newState);
        }

        /// <summary>
        /// Sets training or assessment mode.
        /// </summary>
        public void SetMode(TrainingMode mode)
        {
            currentMode = mode;
            Debug.Log($"[MACHINERY SESSION] Mode switched to: {mode}");
        }

        /// <summary>
        /// Resets the session state for retry.
        /// </summary>
        public void ResetSession()
        {
            InitializeSession();
        }
    }
}
