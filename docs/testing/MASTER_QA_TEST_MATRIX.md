# SurakshaAR — Master QA Test Matrix
**Author / QA Lead:** Rehan  
**Date:** Wednesday, 16 September 2026  
**Modules Covered:** Fire & Explosion (Krishna), Gas & Confined Space (Anjani), Industrial Machinery (Harshita)

---

## 1. Module Test Matrix

| Test ID | Module | Scenario Event Trigger | Expected Event Logged | Expected Score Delta | Pass/Fail Expected | Critical Error Expected |
|---|---|---|---|---|---|---|
| **QA-FIRE-01** | Fire | Identify electrical fire | `HAZARD_IDENTIFIED` | $+50$ (hazard_id) | Progress | No |
| **QA-FIRE-02** | Fire | Pick CO2 Extinguisher | `EQUIPMENT_SELECTED` | $+50$ (equipment_use) | Progress | No |
| **QA-FIRE-03** | Fire | Squeeze lever before pulling pin | `SEQUENCE_ERROR` | $-30$ (procedure_comp) | Progress | No |
| **QA-FIRE-04** | Fire | Stand 0.5m directly into flames | `CRITICAL_ACTION` | Numerical penalty | **FAIL** | **YES** |
| **QA-FIRE-05** | Fire | Full PASS execution $< 45$s | `SCENARIO_COMPLETED` | Score $\ge 85\%$ | **PASS** | No |
| **QA-GAS-01** | Gas | Detector sounds at 30 PPM | `HAZARD_IDENTIFIED` | $+50$ (hazard_id) | Progress | No |
| **QA-GAS-02** | Gas | Select SCBA respirator | `PPE_SELECTED` | $+60$ (ppe_selection) | Progress | No |
| **QA-GAS-03** | Gas | Isolate gas valve | `CORRECT_ACTION` | $+15$ (emergency_resp) | Progress | No |
| **QA-GAS-04** | Gas | Walk downwind into plume | `CRITICAL_ACTION` | Score capped | **FAIL** | **YES** |
| **QA-GAS-05** | Gas | Enter confined space without buddy | `CRITICAL_ACTION` | Score capped | **FAIL** | **YES** |
| **QA-MACH-01** | Machinery | Spot exposed drive belt | `HAZARD_IDENTIFIED` | $+50$ (hazard_id) | Progress | No |
| **QA-MACH-02** | Machinery | Apply lockout hasp and padlock | `CORRECT_ACTION` | $+25$ (loto_procedure) | Progress | No |
| **QA-MACH-03** | Machinery | Apply tag without padlock | `SEQUENCE_ERROR` | $-30$ (loto_procedure) | Progress | No |
| **QA-MACH-04** | Machinery | Reach into live gear to clear jam | `CRITICAL_ACTION` | Score capped | **FAIL** | **YES** |
| **QA-MACH-05** | Machinery | Hit E-stop in $< 2.0$s | `RESPONSE_TIME` | $+5$ speed bonus | **PASS** | No |

---

## 2. Platform & System Integration Matrix

| Test ID | Area | Test Condition | Expected Behavior | Status |
|---|---|---|---|---|
| **QA-SYS-01** | Readiness Gate | Worker attempts assessment without practice | `CanAccessAssessment() == false`; blocks attempt | Verified |
| **QA-SYS-02** | Critical Error | Worker scores 94% but committed 1 critical action | Assessment result: **FAIL**; reason listed | Verified |
| **QA-SYS-03** | Retraining | Worker fails `ppe_selection` | Recommends `retrain_ppe_01`; blocks reassessment until done | Verified |
| **QA-SYS-04** | Certificate | Worker attempts certificate generation on failed drill | Generation rejected; eligibility returns false | Verified |
| **QA-SYS-05** | Certificate QR | Scan QR code on valid certificate | Opens verification endpoint `SUR-YYYY-NNNN`; valid = true | Verified |
| **QA-SYS-06** | Retention | Worker passes drill today | Sets Day 1, Day 7, Day 30 milestones with dates | Verified |
| **QA-SYS-07** | Offline Storage | Disconnect network $\rightarrow$ complete drill | Saves to local PlayerPrefs / OfflineStore | Verified |
| **QA-SYS-08** | Sync Bridge | Reconnect network $\rightarrow$ trigger sync | Batches offline sessions to `/api/v1/sync` | Verified |
| **QA-SYS-09** | Live Telemetry | Perform action in Unity | Live broadcast over `/events/live` WebSocket to Dashboard | Verified |

---

## 3. Bug Classification Guidelines for QA Lead

- **Critical (P0):** App crash, AR tracking loss with crash, critical error not triggering FAIL, data loss during offline drill.
- **High (P1):** Incorrect score calculation, wrong competency recommendation, certificate issued on failed attempt, sync failure.
- **Medium (P2):** UI alignment defect, delayed guidance text, minor timer drift.
- **Low (P3):** Text font inconsistency, non-blocking visual artifact.
