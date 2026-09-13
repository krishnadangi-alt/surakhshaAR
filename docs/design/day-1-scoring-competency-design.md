# Day 1 — Scoring & Competency Design (with Rehan)

> **Owner:** Harshita (with Rehan) · **Branch:** `feature/ml-competency`
> **Deliverable:** "Scoring & Competency Design (with Rehan)" — Day 1 of the member execution plan.
> **Rule followed:** no event names, scoring values, thresholds or competency rules were invented.
> Every rule below already exists in the codebase and is cited with file:line. Anything genuinely
> missing is explicitly marked **NEEDS CONFIRMATION** (Rehan / team decision required).

---

## 1. Day 1 Requirements

**Scoring inputs (in):** correct action · wrong action · unsafe action · response time ·
critical error · completion · attempts

**Scoring outputs (out):** scoring logic · pass/fail interpretation · competency indicators ·
weakness indicators

---

## 2. Existing Implementation Map (Step 1)

There is **no symbol named "Action Logger" or "Rehan"** in the repository. Rehan's Action Logger
contract is represented on the Unity side by the event factories and on the backend by the ML
competency engine's event schema:

| Layer | File / location | Purpose |
|---|---|---|
| Unity event factories ("Action Logger" contract) | `assets/Scripts/Networking/AssessmentEvents.cs` | Builds the exact behavioural events the engine understands (`HazardIdentified`, `PpeSelected`, `EquipmentSelected`, `EvacuationStarted`, `EmergencyProcedure`, `WrongAction`, `CriticalAction`, `Timestamped`) |
| Event schema (wire contract) | `backend/app/schemas/assessment.py` — `AssessmentEvent` | `event_type` required; event-specific fields + unknown fields preserved (`ConfigDict(extra="allow")`) |
| Event routing / scoring | `ml/competency/scoring/engine.py` — `CompetencyScorer.process_event` (lines 90-128) | Routes each `event_type` to its scoring method |
| Event contract (single source of truth) | `docs/api/API.md` — "Assessment Events (ML Competency Engine)" (lines 51-157) | Documents every event, field and scoring effect |
| Backend endpoint | `backend/app/api/v1/assessments.py` | `POST /api/v1/assessments` — server-side scoring, never trusts client scores |
| Service bridge | `backend/app/services/competency_service.py` | `CompetencyScorer → WeaknessDetector → RetrainingRecommender` pipeline |
| Attempt tracking | `backend/app/services/competency_service.py` — `next_attempt_number` (lines 101-112) | Auto-increments `attempt_number` per worker+module |

### 2.1 Existing event vocabulary and scoring effects (all from `docs/api/API.md` + `engine.py`)

| event_type | fields | scoring effect (existing, unchanged) |
|---|---|---|
| `hazard_identified` | `correct`, `hazard_type` | `hazard_identification` +50 / −25 |
| `ppe_selected` | `correct`, `items` | `ppe_selection` +60 (correct **and** non-empty items) / −30 |
| `equipment_selected` | `correct` | `equipment_use` +50 / −25 |
| `evacuation_started` | `correct` | fire: `procedure_compliance` +50/−30 · gas: `evacuation` +50/−30 |
| `emergency_procedure` | `correct`, `action` | gas only: `emergency_response` +50/−25 (ignored for fire) |
| `wrong_action` | `severity` (`minor`/`major`) | minor −5/−3 · major −30/−25 on procedure & decision competencies (fire: `procedure_compliance`+`decision_making` · gas: `emergency_response`+`hazard_identification`) — `engine.py:179-213` |
| `critical_action` | `action`, `reason` | **Automatic FAIL** regardless of all scores — `engine.py:215-229` |
| `training_started` / `assessment_started` / `assessment_completed` | — | Logged for audit; no score change — `engine.py:127-128` |

Scoring fundamentals (`ml/competency/scoring/engine.py`):
- Every competency starts at a **baseline of 50.0** (lines 81-85); scores are clamped to [0, 100] (lines 285-288).
- Overall score = **mean** of all competency scores (lines 291-292).
- Scenario competencies & per-competency pass thresholds (`ml/competency/scoring/config.py`):
  - **fire:** `hazard_identification` 75, `ppe_selection` 80, `procedure_compliance` 75, `equipment_use` 75, `decision_making` 45
  - **gas:** `hazard_identification` 75, `ppe_selection` 80, `evacuation` 75, `equipment_use` 75, `emergency_response` 70
  - Declared-but-unused prototype constants: `OVERALL_SCORE_WEIGHT`, `CRITICAL_ERROR_WEIGHT`, `RETRAINING_SEVERITY_THRESHOLDS` (defined in `config.py`, not referenced by engine/detector/recommender).

### 2.2 Pass / Fail interpretation (existing, `engine.py get_result` lines 304-329)

An assessment **FAILS** if **any** of these hold, otherwise it **PASSES**:

1. One or more `critical_action` events recorded (`critical_errors` non-empty) → automatic FAIL with reason `CRITICAL ERRORS: ...`.
2. `overall_score < 70.0` (`OVERALL_PASS_THRESHOLD`) → "Insufficient overall competency".
3. Any competency score **below its per-competency pass threshold** → "Failed competencies: ...".

### 2.3 Competency indicators (existing)

- `ScoringResult.competency_scores`: per-competency `{name, score, passed, pass_threshold}` (`engine.py:42-61`).
- Public `assess()` returns `score`, `passed`, `competency_status` (`"competent"`/`"not_competent"`), `weaknesses`, `retraining` (`ml/competency/assess.py:22-83`).
- Dashboard surfaces the latest per-module competency profile: `GET /api/v1/dashboard/workers/{worker_id}` → `competency_profile` (`backend/app/api/v1/dashboard.py:100-137`).

### 2.4 Weakness indicators (existing)

- `WeaknessDetector.detect_weaknesses` (`ml/competency/weakness_detection/detector.py`): a competency is a weakness when `score < its pass_threshold`; severity tiers `severe` (<50), `moderate` (<60), `mild` (below threshold but ≥60); sorted severe-first, lowest score first.
- Each weakness → a 1:1 targeted retraining recommendation (`retraining/recommender.py`; `assess.py:72-82`).
- `affected_aspects` (the "skill" level) is **always empty** in current output — see §3 item 11 (NEEDS CONFIRMATION).

---

## 3. Day-1 Gap Analysis (Step 2)

| # | Day-1 requirement | Classification | Evidence (existing code) |
|---|---|---|---|
| 1 | Correct action | **COMPLETE** | `hazard_identified`/`ppe_selected`/`equipment_selected`/`evacuation_started`/`emergency_procedure` with `correct: true` (`engine.py:130-277`; factories in `AssessmentEvents.cs`) |
| 2 | Wrong action | **COMPLETE** (minor/major) | `wrong_action` event, minor (−5/−3) / major (−30/−25) (`engine.py:179-213`, `API.md:63`) |
| 3 | Unsafe action | **MISSING as a distinct event type — NEEDS CONFIRMATION** | No `unsafe_action` event exists anywhere. Closest existing: `critical_action` (auto-FAIL) and `wrong_action` severity documented (minor/major/**critical** in `ml/competency/README.md:55`) but the engine only branches major-vs-minor (`engine.py:196-213`), so severity `"critical"` currently behaves as **minor** |
| 4 | Response time | **PARTIALLY IMPLEMENTED — NEEDS CONFIRMATION** | `response_time_seconds` is an accepted optional field (passed through) but **not scored**; existing test `test_assess_handles_response_time_seconds_gracefully` (`ml/competency/tests/test_integration_backend.py:50-76`) documents it is safely ignored |
| 5 | Critical error | **COMPLETE** | `critical_action` → automatic FAIL (`engine.py:215-229`, `304-314`; tests in `test_competency.py`, `backend/tests/test_assessments.py`) |
| 6 | Completion | **PARTIALLY IMPLEMENTED — NEEDS CONFIRMATION** | `assessment_completed` event exists and is logged for audit with no score change (`engine.py:127-128`, `API.md:65`); scoring does not require it |
| 7 | Attempts | **COMPLETE (tracking); scoring impact — NEEDS CONFIRMATION** | `attempt_number` auto-increment per worker+module (`competency_service.py:101-112`; tests `test_assessments.py:47-67`) and stored (`models/assessment.py`); attempts do **not** affect scoring/pass-fail |
| 8 | Scoring logic (out) | **COMPLETE** | `ml/competency/scoring/engine.py` + `config.py` |
| 9 | Pass/Fail interpretation (out) | **COMPLETE** | `engine.py:304-329` (§2.2) |
| 10 | Competency indicators (out) | **COMPLETE** | `ScoringResult` + `assess()` + dashboard `competency_profile` |
| 11 | Weakness indicators (out) | **PARTIALLY IMPLEMENTED** | Detection works, but `affected_aspects` is always `[]` (`detector.py:59` — comment: "Would be populated from competency config") while `CompetencyDefinition.aspects` are already defined per competency in `config.py` |

---

## 4. Day-1 Logic Definition (Step 3)

All rules below are the **existing** rules — nothing was invented. "NEEDS CONFIRMATION" rows have
**no existing rule**, so no value is proposed.

### 4.1 Action/Event → Category → Score effect → Competency affected → Weakness/skill affected → Pass/Fail impact

| Action / event (existing vocabulary) | Category | Score effect (existing) | Competency affected (fire / gas) | Weakness / skill affected | Pass/Fail impact |
|---|---|---|---|---|---|
| `hazard_identified`, correct=true | Correct | +50 | `hazard_identification` | aspects: spot_fire_sources, identify_fuel, identify_ignition_sources, assess_hazard_level (gas: detect_gas_signs, identify_gas_type, assess_concentration, recognize_symptoms) | positive (raises score toward threshold) |
| `ppe_selected`, correct=true + non-empty items | Correct | +60 | `ppe_selection` (threshold 80) | aspects: select_correct_ppe, proper_donning, ppe_completeness, ppe_inspection (gas: select_respirator, proper_fit_test, donning_procedure, seal_verification) | positive |
| `equipment_selected`, correct=true | Correct | +50 | `equipment_use` | aspects: fire_extinguisher_type / equipment_operation / targeting / technique (gas: gas_detector_operation / ventilation_setup / monitoring / equipment_maintenance) | positive |
| `evacuation_started`, correct=true | Correct | +50 | `procedure_compliance` (fire) / `evacuation` (gas) | aspects: evacuation_steps, alarm_activation, communication, safe_exit_route (gas: upwind_movement, emergency_exit, assist_others, decontamination) | positive |
| `emergency_procedure`, correct=true (gas only) | Correct | +50 | `emergency_response` | aspects: alert_procedures, rescue_coordination, first_aid, incident_reporting | positive (ignored for fire) |
| `wrong_action`, severity=minor | Wrong | −5 procedure, −3 decision | fire: `procedure_compliance` + `decision_making`; gas: `emergency_response` + `hazard_identification` | procedure & decision skills | may still pass |
| `wrong_action`, severity=major | Wrong | −30 procedure, −25 decision | same mapping as minor | procedure & decision skills | can fail via overall/competency rule |
| `wrong_action`, severity=`"critical"` | **Unsafe — NEEDS CONFIRMATION** | currently **−5/−3** (falls into minor branch, `engine.py:205`) | same as minor | — | — |
| `critical_action` | Critical error / unsafe-critical | no score delta; records `critical_errors` | all competencies still scored for output | all (automatic fail) | **automatic FAIL** regardless of score |
| `response_time_seconds` (any event) | Response time — **NEEDS CONFIRMATION** | **no scoring rule exists** | — | — | — |
| `assessment_completed` (+ optional `completion_status`) | Completion — **NEEDS CONFIRMATION** | audit only; no score change; not required | — | — | — |
| `attempt_number` (assessment payload) | Attempts — **tracking COMPLETE; scoring NEEDS CONFIRMATION** | no score effect | — | — | — |

### 4.2 Response-time handling (verified)

- Accepted: every event may carry `response_time_seconds` (optional, preserved by the schema).
- Existing behaviour: **ignored by scoring** — confirmed by `ml/competency/tests/test_integration_backend.py:50-76` ("The existing scoring rules do not define response-time-based scoring, so this field should be safely ignored").
- No threshold or score-delta exists → **NEEDS CONFIRMATION** (Rehan/team must define the rule; not invented here).

### 4.3 Completion handling (verified)

- `assessment_completed` is a valid event type (`API.md:65`, `engine.py:127-128`); `completion_status` appears in sample data (`sample_data/fire_samples.py`).
- Existing behaviour: logged for audit, **no score change**, and an assessment is scored even without it.
- Whether an incomplete assessment (no `assessment_completed`) should be scored/failed/blocked is **NEEDS CONFIRMATION**.

### 4.4 Attempts handling (verified)

- Tracking: `next_attempt_number()` auto-increments `attempt_number` per worker+module; explicit `attempt_number` in the payload is honoured (`assessments.py:79-81`; tests `test_assessments.py:47-67`).
- Existing behaviour: attempts **do not** affect scoring or pass/fail.
- Whether attempts should drive pass rules (e.g. retrain → reassess gate, max attempts, attempt-based thresholds) is **NEEDS CONFIRMATION**.
---

## 5. Implementation Decision (Step 4)

- **No code changes are required for Day 1.** Every Day-1 input/output that has a defined rule is
  already implemented by the existing ML competency pipeline (`ml/competency/scoring/engine.py`),
  the backend integration (`app/services/competency_service.py`, `app/api/v1/assessments.py`) and
  the Unity event factories (`assets/Scripts/Networking/AssessmentEvents.cs`).
- **No duplicate scoring system was added.** The `ml/competency` engine remains the single,
  server-side, authoritative scoring system (client scores are never trusted).
- **No event names, thresholds or scoring values were changed or invented.** The genuinely missing
  rules (unsafe-action event, response-time scoring, completion gating, attempt-based scoring,
  `affected_aspects` wiring, `wrong_action severity="critical"` handling) are listed in §7 as
  **NEEDS CONFIRMATION** for Rehan / the team.
- Existing Fire/Gas functionality is untouched (verified by the unchanged test suites, §6).

---

## 6. Verification (Step 5)

Baseline before this work:

| Suite | Command (repo root) | Result |
|---|---|---|
| ML competency engine | `python -m pytest ml/competency/tests -q` | **43 passed** |
| Backend API | `cd backend && python -m pytest tests -q` | **74 passed** |

Day-1 12-point verification (Expected → Actual → PASS/FAIL) — executed 2026-09-13 against the
unchanged engine + backend (temporary script, no repo files touched):

| # | Check | Expected | Actual | Result |
|---|---|---|---|---|
| 1 | Correct action | hazard_identified correct → `hazard_identification` +50 | hazard_identification=100.0 | PASS |
| 2 | Wrong action | wrong_action major → procedure −30, decision −25 | procedure_compliance=20.0, decision_making=25.0 | PASS |
| 3 | Unsafe action | no distinct event; critical_action → auto FAIL; severity "critical" behaves as minor | critical_action passed=False; severity "critical" → procedure=45.0, decision=47.0 | PASS (behaviour documented; mapping NEEDS CONFIRMATION) |
| 4 | Critical error | critical_action → automatic FAIL + `critical_errors` recorded | passed=False, critical_errors=1, reason='CRITICAL ERRORS: Fed oxygen' | PASS |
| 5 | Response time | `response_time_seconds` accepted, not scored | identical=True (score=90.0, passed=True) | PASS (neutral by design; rule NEEDS CONFIRMATION) |
| 6 | Completion | `assessment_completed` accepted, no score effect | identical=True (score=90.0) | PASS (neutral by design; rule NEEDS CONFIRMATION) |
| 7 | Attempts | attempt_number auto-increments (backend) | attempt 1 = 1, attempt 2 = 2 | PASS |
| 8 | Score calculation | GOOD_FIRE_EVENTS → overall 90.0 | score=90.0 | PASS |
| 9 | PASS condition | good events → passed True, competent | passed=True, status=competent, score=90.0 | PASS |
| 10 | FAIL condition | poor/bad events → passed False, not_competent | fire passed=False, gas passed=False | PASS |
| 11 | Competency output | competency_scores with score/passed/pass_threshold; competency_status | 5 competencies; ppe_selection=100.0/80.0; all assess() keys present | PASS |
| 12 | Weakness output | weaknesses with severity + 1:1 retraining | 5 weaknesses (severe/moderate); retraining == weaknesses | PASS |

**Result: 12/12 PASS, 0 FAIL.**

---

## 7. Items requiring Rehan / team confirmation (Step 3/4)

1. **Unsafe action** — there is no `unsafe_action` event type. Should "unsafe action" map to the
   existing `critical_action` (auto-FAIL) or to a new/dedicated event? (Current code: no distinct handling.)
2. **`wrong_action` severity `"critical"`** — documented in `ml/competency/README.md:55` and
   `engine.py:187`, but handled by the minor branch (`engine.py:205`). Confirm intended severity tiers.
3. **Response time** — `response_time_seconds` captured but unscored. Need official thresholds and
   score deltas before it can be wired into the engine.
4. **Completion** — confirm whether an assessment without `assessment_completed` should be
   scored, blocked, or auto-failed.
5. **Attempts** — confirm whether `attempt_number` should affect pass/fail (e.g. reassess-after-fail
   gating, max attempts) or remain tracking-only.
6. **Weakness `affected_aspects`** — the `aspects` lists already exist in `config.py` per competency,
   but are not populated on weakness output (`detector.py:59`). Confirm whether to wire them.
7. **Prototype thresholds** — all thresholds remain labelled prototype/demo values pending
   validation against official industrial SOPs and domain experts (already declared in
   `ml/competency/README.md` and `scoring/config.py` header).