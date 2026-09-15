# Machinery Module — Action & Competency Specification

**SurakshaAR | SIH 2026 — PS 26041**  
**Author**: Harshita (Intelligence Layer Lead)  
**Scope**: Machinery Safety Module (In-running nip points, unshielded drives, Lockout/Tagout, Emergency Stop)

---

## 1. Executive Summary & Safety Baseline

This document specifies the authoritative action-to-competency mapping for Module 3 (Machinery Safety). All scoring and competency evaluation adheres strictly to the Day 1 safety rules baseline:

- **Critical Errors**: `critical_action` events, `critical=True`, or `severity="critical"` payload fields trigger instant **automatic FAIL** regardless of score.
- **Unsafe Actions**: `unsafe_action` events represent elevated hazard exposure without immediate fatality. They carry a score penalty in the **-20 to -25 range** (default `-22.0`), but do *not* force automatic fail.
- **Wrong Actions**: Minor (`-5.0` procedure / `-3.0` decision) or Major (`-30.0` procedure / `-25.0` decision) procedural mistakes.
- **Response-Time Scaling**:
  - `< 3.0s`: +5% speed bonus on score delta
  - `3.0s – 15.0s`: Baseline score delta
  - `> 15.0s`: -10% procedural latency penalty
  - `E-Stop Reaction`: Benchmark `< 2.5s` (delayed E-Stop recorded as audit penalty).
- **Completion Enforcement**: Assessment must include `assessment_completed` or `scenario_completed` to PASS.
- **Overall PASS Gate**: Overall score $\ge 70.0\%$, all individual competency thresholds met, 0 critical errors, completed.

---

## 2. Machinery Action-to-Competency Mapping Matrix

| Event / Action ID | Action Type | Competency Area | Safety Classification | Score Effect | Competency Effect | Weakness Effect |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `hazard_identified` (exposed_drive_nip) | Hazard Detection | `hazard_identification` | Safe (Correct) | $+50.0$ | Boosts Hazard Recognition | None |
| `hazard_identified` (incorrect) | Hazard Detection | `hazard_identification` | Wrong Action | $-25.0$ | Reduces Hazard Recognition | Weakness: Hazard Recognition |
| `ppe_selected` (safety_glasses_ear_plugs) | Equipment Selection | `ppe_selection` | Safe (Correct) | $+60.0$ | Boosts PPE Selection | None |
| `ppe_selected` (improper/missing) | Equipment Selection | `ppe_selection` | Wrong Action | $-30.0$ | Reduces PPE Selection | Weakness: PPE Selection |
| `loto_switch_breaker_off` | Energy Isolation | `loto_procedure` | Safe (Correct) | $+25.0$ | Boosts LOTO Execution | None |
| `loto_apply_hasp_padlock` | Energy Isolation | `loto_procedure` | Safe (Correct) | $+25.0$ | Boosts LOTO Execution | None |
| `loto_verify_zero_energy` | Energy Isolation | `loto_procedure` | Safe (Correct) | $+25.0$ | Boosts LOTO Execution | None |
| `equipment_selected` (fixed_machine_guard)| Guarding | `equipment_use` | Safe (Correct) | $+50.0$ | Boosts Equipment Use | None |
| `bypass_safety_interlock` | Operational Mistake | `loto_procedure` | Unsafe Action | $-22.0$ | Reduces LOTO & Procedure | Weakness: LOTO Procedure |
| `reach_into_live_gear` | Critical Violation | `emergency_response` | Critical Action | Instant FAIL | Forces NOT_COMPETENT | Critical Weakness Flag |
| `e_stop_actuation` (< 2.5s) | Emergency Actuation | `emergency_response` | Safe (Correct) | $+50.0$ | Boosts Emergency Response| None |
| `e_stop_actuation` (> 2.5s) | Emergency Actuation | `emergency_response` | Late Action | $+50.0$ (Delayed penalty flag) | Recorded in audit log | Latency Audit Penalty |

---

## 3. Integration & Handoff Interfaces

- **Krishna (AR Scene Developer)**: Implements visual feedback and interactive triggers in Unity matching the Event/Action IDs above.
- **Rehan (Action Logger / Assessment Lead)**: Transmits standardized JSON telemetry payloads for each logged action to `CompetencyScorer.process_event()`.
- **Omesh (Backend Lead)**: Receives the computed `competency_scores`, `weaknesses`, `retraining`, and `reassessment` payload for database persistence.
- **Kanishka (Admin Dashboard Lead)**: Consumes structured competency output for compliance monitoring.
