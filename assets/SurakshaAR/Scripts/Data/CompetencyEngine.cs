using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurakshaAR.Data
{
    /// <summary>
    /// Deterministic offline competency scoring engine for SurakshaAR.
    /// Pure C# mirror of the team's Python ML competency architecture in ml/competency/.
    /// Evaluates raw AR / quiz events, computes per-competency percentages,
    /// enforces automatic FAIL on critical errors, detects weaknesses,
    /// and generates targeted retraining recommendations.
    /// </summary>
    public static class CompetencyEngine
    {
        public const float OVERALL_PASS_THRESHOLD = 70.0f;
        public const float SEVERE_WEAKNESS_THRESHOLD = 50.0f;
        public const float MODERATE_WEAKNESS_THRESHOLD = 60.0f;

        public class CompetencyDef
        {
            public string Name;
            public string DisplayName;
            public float PassThreshold;
            public string Description;

            public CompetencyDef(string name, string displayName, float threshold, string desc)
            {
                Name = name;
                DisplayName = displayName;
                PassThreshold = threshold;
                Description = desc;
            }
        }

        private static readonly Dictionary<string, CompetencyDef> FireCompetencies = new Dictionary<string, CompetencyDef>
        {
            { "hazard_identification", new CompetencyDef("hazard_identification", "Hazard Recognition", 75.0f, "Ability to identify fire/explosion hazards") },
            { "ppe_selection",         new CompetencyDef("ppe_selection", "PPE Selection", 80.0f, "Correct selection and donning of personal protective equipment") },
            { "procedure_compliance",  new CompetencyDef("procedure_compliance", "Procedure Compliance", 75.0f, "Following official mining fire response SOP") },
            { "equipment_use",         new CompetencyDef("equipment_use", "Equipment Operation", 75.0f, "Correct fire safety equipment & extinguisher technique") },
            { "decision_making",       new CompetencyDef("decision_making", "Decision Making", 45.0f, "Sound emergency safety decisions under pressure") }
        };

        private static readonly Dictionary<string, CompetencyDef> GasCompetencies = new Dictionary<string, CompetencyDef>
        {
            { "hazard_identification", new CompetencyDef("hazard_identification", "Gas Hazard Detection", 75.0f, "Ability to detect gas leaks and toxic atmospheres") },
            { "ppe_selection",         new CompetencyDef("ppe_selection", "Respiratory PPE", 80.0f, "Correct respirator and breathing apparatus selection") },
            { "evacuation",            new CompetencyDef("evacuation", "Safe Evacuation", 75.0f, "Safe upwind evacuation from contaminated area") },
            { "equipment_use",         new CompetencyDef("equipment_use", "Detector Operation", 75.0f, "Multi-gas detector and ventilation operation") },
            { "emergency_response",    new CompetencyDef("emergency_response", "Emergency Protocol", 70.0f, "Proper emergency response and buddy system procedure") }
        };

        private static readonly Dictionary<string, CompetencyDef> MachineryCompetencies = new Dictionary<string, CompetencyDef>
        {
            { "hazard_identification", new CompetencyDef("hazard_identification", "Machine Hazard Recognition", 75.0f, "Ability to detect pinch points, unshielded gears, and mechanical hazards") },
            { "ppe_selection",         new CompetencyDef("ppe_selection", "Mechanical Safety PPE", 80.0f, "Correct eye protection, hearing protection, and loose clothing restraint") },
            { "loto_procedure",        new CompetencyDef("loto_procedure", "Lockout / Tagout (LOTO)", 80.0f, "Execution of zero energy state lockout and tagout procedures") },
            { "equipment_use",         new CompetencyDef("equipment_use", "Machine Guard Operation", 75.0f, "Proper operation of fixed and interlocked safety guards") },
            { "emergency_response",    new CompetencyDef("emergency_response", "E-Stop Crisis Reaction", 70.0f, "Immediate emergency stop activation and entrapment response") }
        };

        public static Dictionary<string, CompetencyDef> GetDefinitions(string scenarioType)
        {
            if (string.IsNullOrEmpty(scenarioType)) return FireCompetencies;
            string st = scenarioType.ToLower();
            if (st.Contains("gas")) return GasCompetencies;
            if (st.Contains("machin")) return MachineryCompetencies;
            return FireCompetencies;
        }

        /// <summary>
        /// Evaluates a collection of behavioural assessment events and returns a comprehensive AssessmentResultData.
        /// </summary>
        public static AssessmentResultData Evaluate(string scenarioType, List<AssessmentEvent> events, float durationSeconds = 0f)
        {
            scenarioType = string.IsNullOrEmpty(scenarioType) ? "fire" : scenarioType.ToLower();
            var defs = GetDefinitions(scenarioType);

            // Baseline neutral score of 50 for all competencies (mirroring Python engine)
            var scores = new Dictionary<string, float>();
            foreach (var key in defs.Keys)
            {
                scores[key] = 50.0f;
            }

            var criticalErrors = new List<string>();

            if (events != null)
            {
                foreach (var ev in events)
                {
                    if (ev == null) continue;
                    string normalizedType = ev.event_type != null ? ev.event_type.Trim() : "";
                    switch (normalizedType)
                    {
                        case CommonAssessmentEvents.HAZARD_IDENTIFIED:
                        case "hazard_identified":
                            if (ev.correct)
                                scores["hazard_identification"] = Mathf.Min(100.0f, scores["hazard_identification"] + 50.0f);
                            else
                                scores["hazard_identification"] = Mathf.Max(0.0f, scores["hazard_identification"] - 25.0f);
                            break;

                        case CommonAssessmentEvents.PPE_SELECTED:
                        case "ppe_selected":
                            if (ev.correct)
                                scores["ppe_selection"] = Mathf.Min(100.0f, scores["ppe_selection"] + 60.0f);
                            else
                                scores["ppe_selection"] = Mathf.Max(0.0f, scores["ppe_selection"] - 30.0f);
                            break;

                        case CommonAssessmentEvents.EQUIPMENT_SELECTED:
                        case CommonAssessmentEvents.OBJECT_INTERACTION:
                        case "equipment_selected":
                            if (scores.ContainsKey("equipment_use"))
                            {
                                if (ev.correct)
                                    scores["equipment_use"] = Mathf.Min(100.0f, scores["equipment_use"] + 50.0f);
                                else
                                    scores["equipment_use"] = Mathf.Max(0.0f, scores["equipment_use"] - 25.0f);
                            }
                            break;

                        case CommonAssessmentEvents.CORRECT_ACTION:
                        case "correct_action":
                            string correctProcKey = (scenarioType == "gas" || scenarioType == "machinery") ? "emergency_response" : "procedure_compliance";
                            if (scores.ContainsKey(correctProcKey))
                                scores[correctProcKey] = Mathf.Min(100.0f, scores[correctProcKey] + 15.0f);
                            if (ev.action != null && ev.action.StartsWith("loto_") && scores.ContainsKey("loto_procedure"))
                                scores["loto_procedure"] = Mathf.Min(100.0f, scores["loto_procedure"] + 25.0f);
                            break;

                        case CommonAssessmentEvents.WRONG_ACTION:
                        case CommonAssessmentEvents.SEQUENCE_ERROR:
                        case "wrong_action":
                            string procKey = (scenarioType == "gas" || scenarioType == "machinery") ? "emergency_response" : "procedure_compliance";
                            string decKey = (scenarioType == "gas") ? "hazard_identification" : ((scenarioType == "machinery") ? "loto_procedure" : "decision_making");
                            bool isMajor = ev.severity == "major" || normalizedType == CommonAssessmentEvents.SEQUENCE_ERROR;

                            if (scores.ContainsKey(procKey))
                                scores[procKey] = Mathf.Max(0.0f, scores[procKey] - (isMajor ? 30.0f : 5.0f));
                            if (scores.ContainsKey(decKey))
                                scores[decKey] = Mathf.Max(0.0f, scores[decKey] - (isMajor ? 25.0f : 3.0f));
                            if (normalizedType == CommonAssessmentEvents.SEQUENCE_ERROR && scores.ContainsKey("loto_procedure"))
                                scores["loto_procedure"] = Mathf.Max(0.0f, scores["loto_procedure"] - 30.0f);
                            break;

                        case CommonAssessmentEvents.RESPONSE_TIME:
                        case "response_time":
                            // Advanced Day 4 timing metrics
                            string speedKey = scores.ContainsKey("emergency_response") ? "emergency_response" : "decision_making";
                            if (ev.duration_seconds > 0f)
                            {
                                if (ev.duration_seconds <= 3.0f && scores.ContainsKey(speedKey))
                                    scores[speedKey] = Mathf.Min(100.0f, scores[speedKey] + 5.0f); // Rapid reaction bonus
                                else if (ev.duration_seconds > 15.0f && scores.ContainsKey(speedKey))
                                    scores[speedKey] = Mathf.Max(0.0f, scores[speedKey] - 10.0f); // Excessive latency deduction
                            }
                            break;

                        case CommonAssessmentEvents.UNSAFE_ACTION:
                        case "unsafe_action":
                            string unsafeProc = (scenarioType == "gas" || scenarioType == "machinery") ? "emergency_response" : "procedure_compliance";
                            string unsafeDec = (scenarioType == "gas") ? "hazard_identification" : "decision_making";
                            if (scores.ContainsKey(unsafeProc))
                                scores[unsafeProc] = Mathf.Max(0.0f, scores[unsafeProc] - 25.0f);
                            if (scores.ContainsKey(unsafeDec))
                                scores[unsafeDec] = Mathf.Max(0.0f, scores[unsafeDec] - 20.0f);
                            break;

                        case CommonAssessmentEvents.CRITICAL_ACTION:
                        case "critical_action":
                            string reason = !string.IsNullOrEmpty(ev.reason) ? ev.reason : (!string.IsNullOrEmpty(ev.action) ? ev.action : "Critical safety violation");
                            criticalErrors.Add(reason);
                            break;

                        case "evacuation_started":
                            string evacKey = (scenarioType == "gas") ? "evacuation" : "procedure_compliance";
                            if (scores.ContainsKey(evacKey))
                            {
                                if (ev.safe)
                                    scores[evacKey] = Mathf.Min(100.0f, scores[evacKey] + 50.0f);
                                else
                                    scores[evacKey] = Mathf.Max(0.0f, scores[evacKey] - 30.0f);
                            }
                            break;
                    }
                }
            }

            // Clamp and construct competency score items
            float totalScore = 0f;
            var compScoresList = new List<CompetencyScoreData>();
            var weaknesses = new List<WeaknessData>();
            bool allCompetenciesPassed = true;

            foreach (var kvp in defs)
            {
                string key = kvp.Key;
                var def = kvp.Value;
                float finalVal = Mathf.Clamp(scores[key], 0.0f, 100.0f);
                totalScore += finalVal;
                bool passedComp = finalVal >= def.PassThreshold;

                if (!passedComp)
                {
                    allCompetenciesPassed = false;
                    string severity = (finalVal < SEVERE_WEAKNESS_THRESHOLD) ? "severe" :
                                      (finalVal < MODERATE_WEAKNESS_THRESHOLD ? "moderate" : "mild");
                    weaknesses.Add(new WeaknessData
                    {
                        competency_name = key,
                        score = finalVal,
                        threshold = def.PassThreshold,
                        severity = severity,
                        reason = $"{def.DisplayName} score ({finalVal:F0}%) below required pass threshold ({def.PassThreshold:F0}%)"
                    });
                }

                compScoresList.Add(new CompetencyScoreData
                {
                    name = key,
                    score = finalVal,
                    passed = passedComp,
                    pass_threshold = def.PassThreshold
                });
            }

            float overallScore = defs.Count > 0 ? (totalScore / defs.Count) : 0f;

            // Enforce Pass/Fail Decision Logic
            bool passed = true;
            string passReason = "";

            if (criticalErrors.Count > 0)
            {
                passed = false;
                passReason = "CRITICAL SAFETY VIOLATION: " + string.Join("; ", criticalErrors);
            }
            else if (overallScore < OVERALL_PASS_THRESHOLD)
            {
                passed = false;
                passReason = $"Insufficient overall score: {overallScore:F1}% (required: {OVERALL_PASS_THRESHOLD:F0}%)";
            }
            else if (!allCompetenciesPassed)
            {
                passed = false;
                var failedNames = new List<string>();
                foreach (var cs in compScoresList)
                {
                    if (!cs.passed) failedNames.Add(defs[cs.name].DisplayName);
                }
                passReason = "Failed competencies: " + string.Join(", ", failedNames);
            }
            else
            {
                passed = true;
                passReason = $"Assessment passed with validated competency ({overallScore:F1}%)";
            }

            // Generate targeted retraining recommendations
            var retraining = GenerateRetrainingPlan(scenarioType, weaknesses);

            return new AssessmentResultData
            {
                scenario_type = scenarioType,
                overall_score = Mathf.Round(overallScore * 10f) / 10f,
                passed = passed,
                pass_reason = passReason,
                critical_errors = criticalErrors,
                competency_scores = compScoresList,
                weaknesses = weaknesses,
                recommended_retraining = retraining,
                duration_seconds = durationSeconds,
                recorded_at = DateTime.UtcNow.ToString("o")
            };
        }

        private static List<RetrainingModuleData> GenerateRetrainingPlan(string scenarioType, List<WeaknessData> weaknesses)
        {
            var plan = new List<RetrainingModuleData>();
            if (weaknesses == null || weaknesses.Count == 0) return plan;

            foreach (var w in weaknesses)
            {
                switch (w.competency_name)
                {
                    case "hazard_identification":
                        plan.Add(new RetrainingModuleData
                        {
                            module_id = "retrain_hazard_01",
                            name = "Hazard Identification & Threat Triaging",
                            description = "Intensive walkthrough on recognizing industrial fire and gas triggers.",
                            estimated_duration_minutes = 15,
                            difficulty_level = "Intermediate",
                            competencies_addressed = new List<string> { "hazard_identification" },
                            reason = "Address hazard recognition gap"
                        });
                        break;
                    case "ppe_selection":
                        plan.Add(new RetrainingModuleData
                        {
                            module_id = "retrain_ppe_01",
                            name = "Mining PPE & Respiratory Protocol",
                            description = "Standard operating procedures for PPE inspection, fit-testing, and donning.",
                            estimated_duration_minutes = 10,
                            difficulty_level = "Beginner",
                            competencies_addressed = new List<string> { "ppe_selection" },
                            reason = "Reinforce proper protective equipment standards"
                        });
                        break;
                    case "procedure_compliance":
                    case "evacuation":
                        plan.Add(new RetrainingModuleData
                        {
                            module_id = "retrain_sop_01",
                            name = "Emergency Evacuation & Alarm Sequencing",
                            description = "Step-by-step guidance through emergency exit identification and alarm activation.",
                            estimated_duration_minutes = 12,
                            difficulty_level = "Beginner",
                            competencies_addressed = new List<string> { "procedure_compliance", "evacuation" },
                            reason = "Rectify procedural response omissions"
                        });
                        break;
                    case "equipment_use":
                        plan.Add(new RetrainingModuleData
                        {
                            module_id = "retrain_equip_01",
                            name = "Extinguisher Operation (PASS Protocol)",
                            description = "Hands-on virtual drill for Pull, Aim, Squeeze, and Sweep techniques.",
                            estimated_duration_minutes = 15,
                            difficulty_level = "Intermediate",
                            competencies_addressed = new List<string> { "equipment_use" },
                            reason = "Improve practical fire equipment operation"
                        });
                        break;
                    case "decision_making":
                    case "emergency_response":
                        plan.Add(new RetrainingModuleData
                        {
                            module_id = "retrain_decision_01",
                            name = "Crisis Decision-Making Under Smoke & Pressure",
                            description = "Scenario branching on when to fight vs evacuate in industrial hazards.",
                            estimated_duration_minutes = 20,
                            difficulty_level = "Advanced",
                            competencies_addressed = new List<string> { "decision_making", "emergency_response" },
                            reason = "Sharpen real-time emergency decision judgment"
                        });
                        break;
                    case "loto_procedure":
                        plan.Add(new RetrainingModuleData
                        {
                            module_id = "retrain_loto_01",
                            name = "Zero Energy Lockout / Tagout (LOTO) Drill",
                            description = "Step-by-step master drill for electrical isolation, padlock application, and zero-energy verification.",
                            estimated_duration_minutes = 20,
                            difficulty_level = "Intermediate",
                            competencies_addressed = new List<string> { "loto_procedure" },
                            reason = "Rectify life-critical lockout / tagout protocol error"
                        });
                        break;
                }
            }

            return plan;
        }
    }
}
