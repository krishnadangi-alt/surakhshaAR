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