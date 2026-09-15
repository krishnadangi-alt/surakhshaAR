# SurakshaAR — Day 1 Deliverables: Assessment & Training Engine
**Author:** Rehan (Training Engine + Assessment + Evaluation + Certification + Retention + QA Lead)  
**Date:** Friday, 11 September 2026  
**Project:** SIH 2026 — PS 26041 (SurakshaAR)

---

## 1. Assessment & Training Engine Audit

All existing components and capabilities in the codebase have been reviewed and audited according to Rehan's Day 1 checklist:

| # | Assessment / Training Capability | Classification | Current State & Technical Notes |
|---|---|---|---|
| 1 | **Assessment Starts Correctly** | `Working (Keep)` | `AssessmentTelemetryManager.StartSession()` initializes UUID session and records `SCENARIO_STARTED` event. |
| 2 | **Worker Action Performance** | `Working (Keep)` | 3D AR taps and screen CTAs correctly invoke action triggers in `FireScenarioFlowManager`. |
| 3 | **Action Tracking & Telemetry** | `Working (Keep)` | Every worker action records timestamped `AssessmentEvent` objects into `_currentEvents` list. |
| 4 | **Correct / Wrong / Unsafe Action Detection** | `Working (Keep)` | Discrete actions categorized into `correct_action`, `wrong_action`, and `unsafe_action` with penalty weights. |
| 5 | **Critical Action / Error Detection** | `Working (Keep)` | `critical_action` flag triggers zero-tolerance immediate FAIL regardless of numerical score. |
| 6 | **Score Calculation** | `Needs improvement (Fix)` | Fixed: Added real-time response time and sequence error weighting into `CompetencyEngine.Evaluate()`. |
| 7 | **Pass / Fail Generation** | `Working (Keep)` | Evaluates both overall score $\ge 70\%$, all individual competencies passed, and zero critical errors. |
| 8 | **Assessment Reset** | `Working (Keep)` | Flow manager resets AR state and session telemetry without breaking surface tracking. |
| 9 | **Reassessment Flow** | `Needs improvement (Fix)` | Attempt number tracking connected; UI screen hookup scheduled for Day 5 with Harshita. |
| 10 | **Training Stage Structure** | `Working (Keep)` | Standardized into 5 stages: `Introduction -> Instruction -> Demonstration -> Guided Practice -> Free Practice`. |
| 11 | **Readiness Check Gate** | `Missing (Add)` | **Added in `TrainingEngine.cs`**: Blocks workers from entering final assessment until guided practice and readiness check ($\ge 80\%$) are completed. |
| 12 | **12 Common Assessment Events** | `Missing (Add)` | **Added in `AssessmentEvent.cs` and `TrainingEventManager.cs`**: Standardized across Unity and backend contracts. |

---

## 2. The 12 Common Assessment Events Specification

Standardized in `SurakshaAR.Data.CommonAssessmentEvents`:

```csharp
public static class CommonAssessmentEvents
{
    public const string SCENARIO_STARTED    = "SCENARIO_STARTED";
    public const string HAZARD_IDENTIFIED   = "HAZARD_IDENTIFIED";
    public const string PPE_SELECTED        = "PPE_SELECTED";
    public const string EQUIPMENT_SELECTED  = "EQUIPMENT_SELECTED";
    public const string OBJECT_INTERACTION  = "OBJECT_INTERACTION";
    public const string CORRECT_ACTION      = "CORRECT_ACTION";
    public const string WRONG_ACTION        = "WRONG_ACTION";
    public const string UNSAFE_ACTION       = "UNSAFE_ACTION";
    public const string CRITICAL_ACTION     = "CRITICAL_ACTION";
    public const string SEQUENCE_ERROR      = "SEQUENCE_ERROR";
    public const string RESPONSE_TIME       = "RESPONSE_TIME";
    public const string SCENARIO_COMPLETED  = "SCENARIO_COMPLETED";
}
```

### Event Data Fields (Schema for Omesh / Backend)
Every assessment event payload dispatched to `/api/v1/events` or buffered in `/api/v1/sync` adheres to this JSON schema:

```json
{
  "worker_id": 1,
  "module": "fire",
  "scenario": "fire_drill_01",
  "event_type": "CORRECT_ACTION",
  "action": "aim_at_base_of_fire",
  "result": "correct",
  "timestamp": "2026-09-12T18:15:00.000Z",
  "response_time": 3.45,
  "critical": false,
  "payload": {
    "equipment_type": "co2_extinguisher",
    "hazard_type": "electrical_fire",
    "severity": "info",
    "reason": "Targeted fuel source base correctly"
  }
}
```

---

## 3. Unity Event Integration Guide (For Krishna, Anjani, Harshita)

Teammates implementing AR scenario logic call these centralized methods on `TrainingEventManager`:

```csharp
// 1. Scenario start
TrainingEventManager.RaiseScenarioStarted("gas", "methane_leak_drill_01");

// 2. Hazard identified
TrainingEventManager.RaiseHazardIdentified("flammable_gas_cloud", responseTime: 2.1f);

// 3. PPE selection
TrainingEventManager.RaisePpeSelected("scba_respirator", correct: true, responseTime: 4.5f);

// 4. Equipment selection
TrainingEventManager.RaiseEquipmentSelected("multigas_detector", correct: true, responseTime: 1.8f);

// 5. Object interaction
TrainingEventManager.RaiseObjectInteraction("isolation_valve_A", "clockwise_turn", responseTime: 3.2f);

// 6. Correct action
TrainingEventManager.RaiseCorrectAction("activate_ventilation", responseTime: 2.7f);

// 7. Wrong action (procedural mistake, non-fatal)
TrainingEventManager.RaiseWrongAction("evacuate_downwind", severity: "major", reason: "Attempted evacuation along downwind smoke path");

// 8. Unsafe action
TrainingEventManager.RaiseUnsafeAction("unshielded_approach", reason: "Approached high pressure pipeline without visor down");

// 9. Critical error (AUTOMATIC FAIL)
TrainingEventManager.RaiseCriticalAction("enter_confined_space_without_buddy", reason: "Entered unventilated shaft alone without atmospheric test");

// 10. Sequence error
TrainingEventManager.RaiseSequenceError(expectedAction: "remove_pin", actualAction: "squeeze_lever");

// 11. Response time
TrainingEventManager.RaiseResponseTime("emergency_stop_press", seconds: 0.82f);

// 12. Scenario completed
TrainingEventManager.RaiseScenarioCompleted("fire", score: 85.0f, passed: true);
```

---

## 4. Complete Assessment Flow Diagram

```
                WORKER APP
                     │
                     ▼
          [1] Training Mode Starts
                     │
                     ▼
         5-Stage Instructional Flow:
         • Introduction (Safety briefing)
         • Instruction (SOP rules)
         • Demonstration (AR guided visual)
         • Guided Practice (With hints/prompts)
         • Free Practice (Independent retry)
                     │
                     ▼
             [2] READINESS CHECK
          (Physical drill & safety quiz)
                     │
          Score >= 80%? ───► NO ───► Return to Guided Practice
                     │
                    YES
                     │
                     ▼
             [3] ASSESSMENT MODE
             (Strict conditions:
              • NO hints
              • NO guidance arrows
              • NO answer reveals)
                     │
                     ▼
              Scenario Actions
                     │
                     ▼
           Live Action Tracking
          (12 Common Events logged)
                     │
                     ▼
             Score Calculation
       (Weighted by category & timing)
                     │
                     ▼
             CRITICAL ERROR CHECK
                     │
         Critical Error detected?
                /         \
          YES  /           \  NO
              ▼             ▼
       Automatic FAIL   Check Overall Score >= 70%
              │         & All Competencies Passed
              │                /       \
              │          FAIL /         \ PASS
              │              ▼           ▼
              └──────► [4] Harshita    [5] Competent
                       Weakness            │
                       Detection           ▼
                           │         Issue Certificate
                           ▼         (Unique ID + QR)
                       Targeted            │
                       Retraining          ▼
                           │          [6] Retention
                           ▼          Day 1 / 7 / 30
                      Reassessment         │
                           │               ▼
                           └────────► Omesh Backend
                                           │
                                           ▼
                                   Kanishka Dashboard
```
