# Day 1 — Scoring & Competency Design (with Rehan) — FINAL

> **Owner:** Harshita (with Rehan) · **Branch:** `feature/ml-competency`
> **Status:** **FINAL — Day 1 rules confirmed by Rehan and IMPLEMENTED + TESTED** (2026-09-13).
> The scoring inputs below map 1:1 onto the single, authoritative scoring pipeline
> (`ml/competency` engine → `backend/app/services/competency_service.py`). No duplicate scoring
> system exists. Every confirmed rule is implemented in code and covered by automated tests
> (`ml/competency/tests/test_day1_rules.py`, backend `tests/test_day1_rules.py`).
> Values marked **OPEN** in §7 still need project-level confirmation; nothing else is provisional
> except the standing prototype/SOP validation disclaimer.

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
| `wrong_action` | `severity` (`minor`/`major`) | minor −5/−3 · major −30/−25 on procedure & decision competencies (fire: `procedure_compliance`+`decision_making` · gas: `emergency_response`+`hazard_identification`) — **Day 1:** `severity="critical"` is intercepted as a Critical Safety Error (auto-FAIL), never scored as minor |
| `unsafe_action` (**Day 1 — new, Rehan-confirmed**) | `action` | larger penalty in the **−20..−25 band** (project default −22 procedure / −20 decision, `config.py UNSAFE_ACTION_PENALTY`) — does **NOT** auto-FAIL |
| `critical_action` | `action`, `reason` | **Automatic FAIL** regardless of all scores — `engine.py _score_critical_action` |
| `critical=true` / `severity="critical"` (any event type) | — | **Automatic FAIL** (Day 1: `_is_critical_event` central check) |
| `response_time_seconds` (optional, any event) | seconds | **Day 1 (Rehan):** < 3.0s → +5% bonus on the event's delta · 3.0–15.0s → baseline · > 15.0s → −10% procedural latency penalty · E-Stop benchmark < 2.5s (late → `delayed_estop_reactions` counter) |
| `training_started` / `assessment_started` | — | Logged for audit; no score change |
| `assessment_completed` / `scenario_completed` | optional `completion_status` | **Day 1 (Rehan):** completion is **mandatory for PASS**; absence → FAIL ("Incomplete assessment") |

Scoring fundamentals (`ml/competency/scoring/engine.py`):
- Every competency starts at a **baseline of 50.0** (lines 81-85); scores are clamped to [0, 100] (lines 285-288).
- Overall score = **mean** of all competency scores (lines 291-292).
- Scenario competencies & per-competency pass thresholds (`ml/competency/scoring/config.py`):
  - **fire:** `hazard_identification` 75, `ppe_selection` 80, `procedure_compliance` 75, `equipment_use` 75, `decision_making` 45
  - **gas:** `hazard_identification` 75, `ppe_selection` 80, `evacuation` 75, `equipment_use` 75, `emergency_response` 70
  - Declared-but-unused prototype constants: `OVERALL_SCORE_WEIGHT`, `CRITICAL_ERROR_WEIGHT`, `RETRAINING_SEVERITY_THRESHOLDS` (defined in `config.py`, not referenced by engine/detector/recommender).

### 2.2 Pass / Fail interpretation (final, `engine.py get_result`)

An assessment **FAILS** if **any** of these hold, otherwise it **PASSES** (in precedence order):

1. Any Critical Safety Error (`critical_action` event, `critical: true`, or `severity: "critical"`) → automatic FAIL with reason `CRITICAL ERRORS: ...` — **Day 1 (Rehan rule 2)**.
2. `overall_score < 70.0` (`OVERALL_PASS_THRESHOLD`) → "Insufficient overall competency".
3. Any competency score **below its per-competency pass threshold** → "Failed competencies: ...".
4. **No completion event observed** (`assessment_completed` / `scenario_completed`) → FAIL "Incomplete assessment: SCENARIO_COMPLETED / required completion not observed" — **Day 1 (Rehan rule 4)**.

### 2.3 Competency indicators (existing)

- `ScoringResult.competency_scores`: per-competency `{name, score, passed, pass_threshold}` (`engine.py:42-61`).
- Public `assess()` returns `score`, `passed`, `competency_status` (`"competent"`/`"not_competent"`), `weaknesses`, `retraining` (`ml/competency/assess.py:22-83`).
- Dashboard surfaces the latest per-module competency profile: `GET /api/v1/dashboard/workers/{worker_id}` → `competency_profile` (`backend/app/api/v1/dashboard.py:100-137`).

### 2.4 Weakness indicators (existing)

- `WeaknessDetector.detect_weaknesses` (`ml/competency/weakness_detection/detector.py`): a competency is a weakness when `score < its pass_threshold`; severity tiers `severe` (<50), `moderate` (<60), `mild` (below threshold but ≥60); sorted severe-first, lowest score first.
- Each weakness → a 1:1 targeted retraining recommendation (`retraining/recommender.py`; `assess.py:72-82`).
- **Day 1 (Rehan rule 6):** `affected_aspects` is now **populated from the competency's
  `CompetencyDefinition.aspects`** (`detector.py _aspects_for_competency`; definitions flow from the
  scorer via `ScoringResult.competency_definitions`) and is serialised to the API/backend
  (`competency_service.weakness_to_dict` → `WeaknessOut.affected_aspects`). It is `[]` only when a
  competency definition has no aspects (none today).

---

## 3. Day-1 Gap Analysis (Step 2)

| # | Day-1 requirement | Classification | Evidence (existing code) |
|---|---|---|---|
| 1 | Correct action | **COMPLETE** | `hazard_identified`/`ppe_selected`/`equipment_selected`/`evacuation_started`/`emergency_procedure` with `correct: true` (`engine.py:130-277`; factories in `AssessmentEvents.cs`) |
| 2 | Wrong action | **COMPLETE** (minor/major) | `wrong_action` event, minor (−5/−3) / major (−30/−25) (`engine.py:179-213`, `API.md:63`) |
| 3 | Unsafe action | **IMPLEMENTED (Day 1, Rehan-confirmed)** | New `unsafe_action` event: distinct from WRONG_ACTION and CRITICAL_ACTION; penalty in the −20..−25 band (default `UNSAFE_ACTION_PENALTY=22.0` procedure / 20 decision in `config.py`); does **NOT** auto-FAIL (`engine.py _score_unsafe_action`) |
| 4 | Response time | **IMPLEMENTED (Day 1, Rehan-confirmed)** | `response_time_seconds` now scored: <3s +5%, 3–15s baseline, >15s −10%, E-Stop benchmark 2.5s (`engine.py _apply_response_time` / `_check_estop_timing`; counters on `ScoringResult`) |
| 5 | Critical error | **COMPLETE + EXTENDED (Day 1)** | `critical_action` → automatic FAIL (existing); **plus** `critical=true` and `severity="critical"` on any event now also auto-FAIL (`engine.py _is_critical_event`) |
| 6 | Completion | **IMPLEMENTED (Day 1, Rehan-confirmed)** | Completion is now **mandatory for PASS**: no `assessment_completed`/`scenario_completed` → FAIL "Incomplete assessment" (`engine.py get_result` completion gate) |
| 7 | Attempts | **COMPLETE (tracking-only, Rehan-confirmed)** | `attempt_number` auto-increment per worker+module (`competency_service.py next_attempt_number`) and stored (`models/assessment.py`); **no score effect** — each attempt is scored on its own performance; direct reassessment blocking after FAIL is a workflow/progress concern (progress stage `retrain` → `reassess`), not a scoring rule |
| 8 | Scoring logic (out) | **COMPLETE** | `ml/competency/scoring/engine.py` + `config.py` |
| 9 | Pass/Fail interpretation (out) | **COMPLETE** | `engine.py get_result` (§2.2, incl. Day-1 completion gate) |
| 10 | Competency indicators (out) | **COMPLETE** | `ScoringResult` + `assess()` + dashboard `competency_profile` |
| 11 | Weakness indicators (out) | **IMPLEMENTED (Day 1, Rehan-confirmed)** | `affected_aspects` is populated from `CompetencyDefinition.aspects` (`detector.py _aspects_for_competency`) and flows to the API (`WeaknessOut.affected_aspects`) |

---

## 4. Day-1 Logic Definition (Step 3) — FINAL CONFIRMED RULES (Rehan)

All rules below are confirmed by Rehan and implemented in `ml/competency/scoring/engine.py`
(single centralized scoring path — no duplicate scoring system).

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
| `unsafe_action` (**Day 1 — new**) | **Unsafe** | **−22 procedure / −20 decision** (confirmed −20..−25 band; exact split OPEN in §7) | fire: `procedure_compliance` + `decision_making`; gas: `emergency_response` + `hazard_identification` | procedure & decision skills | **NO automatic FAIL** — fails only via the overall/competency score rules |
| `wrong_action`, severity=`"critical"` | **Critical Safety Error (Day 1)** | no score delta; recorded in `critical_errors` | all competencies still scored for output | all (automatic fail) | **automatic FAIL** regardless of score |
| `critical=true` (any event type) | **Critical Safety Error (Day 1)** | no score delta; recorded in `critical_errors` | all competencies still scored for output | all (automatic fail) | **automatic FAIL** |
| `critical_action` | **Critical Safety Error** | no score delta; records `critical_errors` | all competencies still scored for output | all (automatic fail) | **automatic FAIL** regardless of score |
| `response_time_seconds` on any scored event | Response time | < 3.0s → delta × **1.05** (+5%) · 3.0–15.0s → delta × 1.0 (baseline) · > 15.0s → delta × **0.90** (−10%) | the competencies the event affects | procedural latency | affects score only (can cause PASS/FAIL via the numeric rules); 0/missing = no data |
| `response_time_seconds` on a Machinery E-Stop event | Response time — E-Stop benchmark | benchmark **< 2.5s**; ≥ 2.5s → `delayed_estop_reactions` counter incremented (delayed-reaction penalty marker; score effect via the same latency rule) | equipment_use / relevant competency | reaction speed | recorded on `ScoringResult` for audit/demo |
| `assessment_completed` / `scenario_completed` | Completion | no score change; marks the assessment completed | — | — | **required for PASS** (Rehan rule 4); absence → FAIL "Incomplete assessment" |
| `attempt_number` (assessment payload) | Attempts | **no score effect** | — | — | tracking/progression data only; recorded on every assessment row (Rehan rule 5) |

### 4.2 Response-time handling (final — Rehan rule 3)

- Every event may carry `response_time_seconds` (optional). Non-positive / missing values mean
  "no data" (Unity's JsonUtility serialises unset floats as 0.0; a real response time is > 0).
- `< 3.0s` → the event's score delta is multiplied by **1.05** (+5% speed bonus).
- `3.0–15.0s` (inclusive bounds) → **baseline** (no change).
- `> 15.0s` → the delta is multiplied by **0.90** (−10% procedural latency penalty).
- **Machinery E-Stop benchmark:** a correctly-acted E-Stop with `response_time_seconds >= 2.5`
  is a **late** reaction → recorded in `ScoringResult.delayed_estop_reactions` (delayed-reaction
  penalty marker; the numeric score effect flows through the same latency rule above).
- Counters `fast_responses` / `slow_responses` / `delayed_estop_reactions` are tracked once per
  event on `ScoringResult` and exposed via `result.to_dict()` for audit/demo.
- Implemented in `engine.py _apply_response_time`, `_track_response_time`, `_check_estop_timing`;
  thresholds in `config.py` (`RESPONSE_TIME_*`, `ESTOP_BENCHMARK_SECONDS`).

### 4.3 Completion handling (final — Rehan rule 4)

- `assessment_completed` and `scenario_completed` both count as completion events
  (`config.py COMPLETION_EVENT_TYPES`).
- An assessment **without** a completion event is **incomplete** and **must NOT PASS** — even with
  otherwise passing scores: `engine.get_result()` returns
  `passed=False`, reason "Incomplete assessment: SCENARIO_COMPLETED / required completion not observed".
- The gate propagates through the whole pipeline: public `assess()`, the backend
  `POST /api/v1/assessments` result, and sync scoring.

### 4.4 Attempts handling (final — Rehan rule 5)

- `attempt_number` is **tracking/audit data only**: auto-incremented per worker+module
  (`competency_service.next_attempt_number`), stored on every assessment row
  (`models/assessment.py`), and exposed by the history/progress endpoints.
- **No direct score penalty** from previous failed attempts — each attempt is scored purely on
  its own events (verified by `test_day1_rules.py::TestAttemptsRule`).
- **After FAIL, direct reassessment is blocked until targeted retraining is completed, then
  unlocked** — this is workflow/progression logic: the pipeline already produces the blocking
  inputs (FAIL verdict + per-weakness retraining plan via `RetrainingRecommender`, and the
  progress stage model `learn → ... → diagnose → retrain → reassess → ...`). The API-side
  enforcement of the gate itself (rejecting a new attempt while `retrain` is incomplete) is **not
  part of the scoring engine** and remains a Day 2+ backend-workflow item (§7).
---

## 5. Implementation Decision (Step 4) — what changed

All Day-1 changes are **inside the existing single scoring pipeline**; no duplicate scoring system
was added, and no existing Fire/Gas behaviour was broken (verified by the full test suites, §6).

| Change | File(s) | Notes |
|---|---|---|
| `unsafe_action` event scoring (−20..−25 band, no auto-FAIL) | `ml/competency/scoring/engine.py` (`_score_unsafe_action`, routing in `process_event`), `ml/competency/scoring/config.py` (`UNSAFE_ACTION_PENALTY`, `UNSAFE_ACTION_PENALTY_RANGE`) | Reuses the scenario-aware procedure/decision competency mapping — same architecture as `wrong_action` |
| Critical Safety Error centralisation (`critical_action` ∪ `critical=true` ∪ `severity="critical"`) | `engine.py _is_critical_event` / `_score_critical_action` | One check before event routing; audit `trigger` recorded per critical error |
| Response-time rules (<3s +5%, 3–15s baseline, >15s −10%, E-Stop 2.5s) | `engine.py _apply_response_time` / `_track_response_time` / `_check_estop_timing`, `config.py RESPONSE_TIME_*`, `ESTOP_BENCHMARK_SECONDS` | Non-positive `response_time_seconds` = no data (Unity JsonUtility unset-float compatibility) |
| Completion mandatory for PASS | `engine.py get_result`, `config.py COMPLETION_EVENT_TYPES = (assessment_completed, scenario_completed)` | Incomplete → FAIL even with passing scores |
| Attempts: tracking-only | `backend/app/services/competency_service.py` (unchanged), `backend/app/models/assessment.py` (unchanged) | No score effect; confirmed behaviour kept |
| `affected_aspects` population | `ml/competency/weakness_detection/detector.py` (`_aspects_for_competency`), `backend/app/services/competency_service.py` (`weakness_to_dict` — already forwarded) | Sourced from `CompetencyDefinition.aspects`; definitions attached to `ScoringResult` |
| New Day-1 constants + SIH baseline block | `ml/competency/scoring/config.py` | `WRONG_ACTION_*`, `UNSAFE_ACTION_*`, `RESPONSE_TIME_*`, `ESTOP_BENCHMARK_SECONDS`, `COMPLETION_EVENT_TYPES`, `SIH_BASELINE_THRESHOLDS` |
| Unity event contract (additive only) | `assets/Scripts/Networking/ApiContracts.cs` (DTO fields `response_time_seconds`, `critical`), `assets/Scripts/Networking/AssessmentEvents.cs` (factories `UnsafeAction`, `WithResponseTime`, `ScenarioCompleted`) | Pure additive; no existing factory changed. Unity-side compile verification is not possible in this environment (no Unity CLI) |
| Test fixtures updated for mandatory completion | `backend/tests/events.py`, `ml/competency/tests/test_integration_backend.py`, `ml/competency/tests/test_competency.py` | Good/bad fixtures now end with `assessment_completed` (clients send completion) |
| New tests | `ml/competency/tests/test_day1_rules.py` (25 tests), `backend/tests/test_day1_rules.py` (API-level Day-1 tests) | Cover all confirmed rules incl. negative paths |
| API contract doc | `docs/api/API.md` — Assessment Events section | `unsafe_action`, `response_time_seconds`, completion-required rule documented |

---

## 6. Verification (Step 5)

Final state after implementation:

| Suite | Command | Result |
|---|---|---|
| ML competency engine (incl. new Day-1 tests) | `python -m pytest ml/competency/tests -q` | **68 passed** |
| Backend API (incl. updated fixtures) | `cd backend && python -m pytest tests -q` | **74 passed** |

The backend suite covers: idempotent submissions, attempt auto-increment/explicit numbers,
weakness detection + `affected_aspects` through the API, retraining plans, certificates gating on
passing assessments, dashboard aggregation, progress stats, offline sync scoring, and the Day-1
event rules end-to-end. The ML suite adds `test_day1_rules.py` with the 18-point rule matrix
(correct/wrong/unsafe/critical/critical-flags, response-time tiers + E-Stop, completion,
attempts, affected aspects, SIH thresholds).

---

## 7. Remaining open items (explicitly unresolved — need Rehan / team decision)

1. **Exact unsafe-action penalty value (OPEN).** Rehan confirmed the −20..−25 range. The project
   default is −22 (`UNSAFE_ACTION_PENALTY` in `config.py`, applied to the procedure competency) with
   −20 on the decision competency. Pick exact per-competency values before production tuning.
2. **E-Stop event contract (OPEN).** The engine detects E-Stop events best-effort by matching
   "estop"/"e-stop"/"e_stop" in `event_type`/`action`/`hazard_type`/`description`/`reason`. If the VR
   team prefers a dedicated `estop_pressed` event type (or a `benchmark="estop"` field), wire that
   into `_is_estop_event` — the scoring path is already centralized.
3. **Late E-Stop score effect (OPEN).** Rehan confirmed "late E-Stop → delayed-reaction penalty".
   Currently the late reaction is recorded (`delayed_estop_reactions`) and the numeric effect flows
   through the shared latency rule (≥3s = baseline). If a dedicated penalty is wanted (e.g. flat
   −X on equipment_use), add it in one place: `engine._check_estop_timing`.
4. **Reassess-after-FAIL gate enforcement (OPEN, backend workflow).** The scoring engine produces
   the FAIL verdict + targeted retraining plan (the gate's inputs) and the progress stage model
   already contains `retrain → reassess`. Enforcing "block direct reassessment until retraining
   completed" at the API level (e.g. reject `POST /assessments` when the latest attempt failed and
   no `retrain` progress exists) is Day 2+ backend-workflow work — deliberately not invented here.
5. **`critical=true`/`severity="critical"` on positive events (OPEN).** Currently a correct action
   flagged `critical=true` also auto-FAILs (rule: "any event ... regardless of numerical score").
   If Rehan wants critical flags only on mistake events, restrict `_is_critical_event` to
   `correct != true` events.
6. **Production validation (standing).** All thresholds/penalties remain demo/SIH-baseline values
   (`SIH_BASELINE_THRESHOLDS`, "provisional for production") and must be validated against official
   industrial SOPs and safety experts before production deployment.

### 7.1 SIH baseline thresholds (locked for SIH/demo, provisional for production)

| Gate | Value | Where enforced |
|---|---|---|
| Overall PASS | ≥ 70% | `OVERALL_PASS_THRESHOLD` |
| Hazard Recognition | ≥ 75% | fire/gas `hazard_identification` threshold |
| PPE Selection & LOTO | ≥ 80% | fire/gas `ppe_selection` threshold |
| Emergency Response / Procedure | 70–75% | gas `emergency_response` 70 · fire `procedure_compliance` 75 |
| Decision Making | ≥ 45% | fire `decision_making` threshold |

All five are asserted against the live `CompetencyDefinition` thresholds in
`test_day1_rules.py::test_sih_baseline_thresholds_match_competency_definitions`.