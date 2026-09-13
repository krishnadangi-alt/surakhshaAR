using System;

namespace SurakshaAR.Networking
{
    /// <summary>
    /// Factories for well-formed assessment events that the backend ML competency
    /// engine (ml/competency) understands. Build an assessment by collecting these
    /// in chronological order and submitting them with SubmissionAssessment.
    ///
    /// Event naming and field contracts follow docs/api/API.md -> "Assessment
    /// Events (ML Competency Engine)" and ml/competency/scoring/engine.py.
    /// </summary>
    public static class AssessmentEvents
    {
        /// <summary>hazard_identified - correct = worker spotted the hazard.</summary>
        public static ApiContracts.AssessmentEvent HazardIdentified(bool correct, string hazardType)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "hazard_identified",
                hazard_type = hazardType,
                correct = correct,
            };
        }

        /// <summary>ppe_selected - correct + the chosen items.</summary>
        public static ApiContracts.AssessmentEvent PpeSelected(bool correct, params string[] items)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "ppe_selected",
                items = items ?? new string[0],
                correct = correct,
            };
        }

        /// <summary>equipment_selected - correct choice of tool / equipment.</summary>
        public static ApiContracts.AssessmentEvent EquipmentSelected(bool correct, string action)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "equipment_selected",
                action = action,
                correct = correct,
            };
        }

        /// <summary>evacuation_started - fire: route; gas: upwind direction.</summary>
        public static ApiContracts.AssessmentEvent EvacuationStarted(bool correct, string routeOrDirection)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "evacuation_started",
                route = routeOrDirection,
                direction = routeOrDirection,
                correct = correct,
            };
        }

        /// <summary>emergency_procedure (gas only) - e.g. alert_supervisor.</summary>
        public static ApiContracts.AssessmentEvent EmergencyProcedure(bool correct, string action)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "emergency_procedure",
                action = action,
                correct = correct,
            };
        }

        /// <summary>wrong_action - minor or major mistake (no hint mode in assessment).</summary>
        public static ApiContracts.AssessmentEvent WrongAction(string severity)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "wrong_action",
                severity = severity,
            };
        }

        /// <summary>
        /// unsafe_action - Day 1: increases hazard exposure but is NOT immediately
        /// fatal. Large penalty (-20..-25 band) on the server; does NOT auto-FAIL.
        /// </summary>
        public static ApiContracts.AssessmentEvent UnsafeAction(string action)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "unsafe_action",
                action = action,
            };
        }

        /// <summary>
        /// assessment_completed - Day 1: completion is mandatory for PASS. Send as
        /// the final event of every assessment submission.
        /// </summary>
        public static ApiContracts.AssessmentEvent AssessmentCompleted(string completionStatus)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "assessment_completed",
                completion_status = completionStatus,
            };
        }

        /// <summary>scenario_completed - alternative Day 1 completion marker.</summary>
        public static ApiContracts.AssessmentEvent ScenarioCompleted(string completionStatus)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "scenario_completed",
                completion_status = completionStatus,
            };
        }

        /// <summary>
        /// Day 1: stamps an event with the worker's reaction time in seconds.
        /// Rules: &lt; 3s +5% bonus | 3-15s baseline | &gt; 15s -10% latency penalty |
        /// Machinery E-Stop benchmark &lt; 2.5s (late reaction recorded server-side).
        /// </summary>
        public static ApiContracts.AssessmentEvent WithResponseTime(
            ApiContracts.AssessmentEvent e, float seconds)
        {
            e.response_time_seconds = seconds;
            return e;
        }

        /// <summary>
        /// critical_action - a safety violation. Triggers automatic FAIL on the
        /// server regardless of all other scores.
        /// </summary>
        public static ApiContracts.AssessmentEvent CriticalAction(string action, string reason)
        {
            return new ApiContracts.AssessmentEvent
            {
                event_type = "critical_action",
                action = action,
                reason = reason,
            };
        }

        /// <summary>Stamps an event with the current UTC time (ISO 8601).</summary>
        public static ApiContracts.AssessmentEvent Timestamped(ApiContracts.AssessmentEvent e)
        {
            e.timestamp = DateTime.UtcNow.ToString("o");
            return e;
        }
    }
}