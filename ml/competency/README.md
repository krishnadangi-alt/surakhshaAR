# ML — Competency

Machine-learning components that score worker competency during and after assessment scenarios, and that
detect performance weaknesses to drive targeted retraining.

> **Status:** P0 Member 2 implementation complete. Core scoring, weakness detection, and retraining modules are functional and tested.

## Overview

The competency engine provides:
1. **Deterministic scoring** of worker performance across defined competencies
2. **Critical error handling** with automatic FAIL override for safety violations
3. **Weakness detection** to identify performance gaps
4. **Targeted retraining** recommendations mapped to specific training modules
5. **Public `assess()` interface** — a single entry point that converts raw events into a complete competency verdict

This is a **prototype implementation** and uses example thresholds and scoring rules. 
**All scoring thresholds, pass/fail criteria, and competency definitions must be validated against official industrial safety procedures and domain experts before production deployment.**

## Structure

| Path | Purpose |
|---|---|
| `assess.py` | Public ``assess(events)`` interface — single entry point for the whole pipeline |
| `scoring/` | Competency scoring engine, configuration, and competency definitions |
| `weakness_detection/` | Detection of skill/performance weaknesses from assessment results |
| `retraining/` | Targeted retraining recommendations based on detected weaknesses |
| `sample_data/` | Sample assessment event data for testing and development |
| `tests/` | Automated test suite for all modules |

## Architecture

### Scoring Engine (`scoring/`)

The scoring engine processes assessment events and calculates competency-based scores.

**Key Classes:**
- `CompetencyScorer` — Main scoring engine
  - `process_event(event)` — Process a single assessment event
  - `get_result()` — Finalize and return ScoringResult
- `ScoringResult` — Complete assessment result with scores and PASS/FAIL decision

**Key Files:**
- `engine.py` — Core scoring logic and event routing
- `config.py` — Competency definitions, thresholds, and scoring parameters

### Event Types

The scorer handles events from the AR worker application (API standard):
- `training_started` — Training session initiated
- `assessment_started` — Assessment scenario started
- `hazard_identified` — Worker identified a hazard (correct/incorrect)
- `ppe_selected` — Worker selected PPE equipment
- `equipment_selected` — Worker selected tools/equipment
- `wrong_action` — Incorrect action (severity: minor/major/critical)
- `critical_action` — Safety-critical violation (triggers automatic FAIL)
- `evacuation_started` — Evacuation procedure initiated
- `assessment_completed` — Assessment concluded

### Competency Definitions

#### Fire & Explosion Response

Competencies for fire safety scenarios:

| Competency | Description | Pass Threshold |
|---|---|---|
| **hazard_identification** | Identify fire/explosion hazards and triggers | 75% |
| **ppe_selection** | Correct personal protective equipment selection | 80% |
| **procedure_compliance** | Follow fire response procedures | 75% |
| **equipment_use** | Correct fire safety equipment operation | 75% |
| **decision_making** | Sound safety decisions under pressure | 70% |

#### Gas Hazard Response

Competencies for gas hazard scenarios:

| Competency | Description | Pass Threshold |
|---|---|---|
| **hazard_identification** | Detect gas leaks and toxic atmosphere hazards | 75% |
| **ppe_selection** | Correct respiratory protection selection | 80% |
| **evacuation** | Safe evacuation from contaminated area | 75% |
| **equipment_use** | Detection and ventilation equipment operation | 75% |
| **emergency_response** | Proper emergency procedures for gas incidents | 70% |

### Scoring Approach

**Deterministic Scoring:**
- Each event updates competency scores directly (no machine learning)
- Scores range from 0–100, bounded to [0, 100]
- Correct actions add points; mistakes subtract points
- All competency scores start at a neutral baseline of 50

**Scoring Rules** (per `docs/api/API.md`):
- Correct hazard identification: +50 / incorrect: −25 → `hazard_identification`
- Correct PPE (non-empty items): +60 / incorrect: −30 → `ppe_selection`
- Correct equipment: +50 / incorrect: −25 → `equipment_use`
- Correct evacuation: +50 / incorrect: −30 → `procedure_compliance` (fire) or `evacuation` (gas)
- Wrong action (minor): −5 procedure, −3 decision-making
- Wrong action (major): −30 procedure, −25 decision-making
  - fire: affects `procedure_compliance` + `decision_making`
  - gas: affects `emergency_response` + `hazard_identification`
- Emergency procedure (gas only): +50 / −25 → `emergency_response`
- `critical_action`: automatic FAIL (logged, no score change)
- `training_started`, `assessment_started`, `assessment_completed`: logged for audit, no score change

### Pass/Fail Logic

An assessment **PASSES** only if:
1. **No critical errors** are recorded
2. **Overall score ≥ 70%** (average across competencies)
3. **All competencies pass** individually (score ≥ competency threshold)

An assessment **FAILS** if:
- **Critical error occurs** (overrides all other scores) → FAIL
- OR overall score < 70% → FAIL
- OR any competency score < pass threshold → FAIL

### Critical Error Handling

**Critical errors** are safety-critical violations that result in immediate FAIL:
- Re-entering unsafe area without clearance
- Ignoring evacuation orders
- Operating equipment without proper assessment
- Any other defined critical safety violation

Critical errors override numerical scores — a worker with 90% overall score still fails if they commit a critical error.

## Public Interface (`assess.py`)

The one clean entry point for application/backend integration. It wraps the full
pipeline (scoring → weakness detection → retraining) and returns a single canonical
dict. The engine computes the result itself; client-supplied ``passed``/``score``
values are never trusted.

```python
from ml.competency import assess

result = assess(events)               # scenario_type defaults to "fire"
result = assess(events, "gas")        # explicit gas scenario
```

**Input:** a list of event dicts (each with at least an ``event_type`` key; see
``docs/api/API.md``). Unknown/missing fields are handled safely. Empty/``None``
input returns a zero-score FAIL.

**Output:**

```python
{
    "score":            <float>,   # overall score 0–100 (mean of category scores)
    "passed":           <bool>,    # True if competent
    "competency_status": <str>,    # "competent" | "not_competent"
    "weaknesses":       [<str>],   # low-scoring category names
    "retraining":       [<str>],   # matching retraining categories (1:1)
}
```

**competency_status** is derived from ``passed``:
- ``True``  → ``"competent"``
- ``False`` → ``"not_competent"``

This interface is consumed by the backend service
(``backend/app/services/competency_service.py``) and can be called directly by any
application code.



## Weakness Detection (`weakness_detection/`)

The weakness detector identifies performance gaps from assessment results.

**Key Classes:**
- `WeaknessDetector` — Detects weak competencies
  - `detect_weaknesses(scoring_result)` — Identify weaknesses
- `Weakness` — A single weakness with score, threshold, and severity

**Weakness Thresholds:**
- **Severe**: Score < 50%
- **Moderate**: Score 50–60%
- **Mild**: Score 60–70%

Weaknesses are returned sorted by severity (severe first) and then by score (lowest first).

## Retraining Recommendations (`retraining/`)

The retraining recommender maps detected weaknesses to targeted training modules.

**Key Classes:**
- `RetrainingRecommender` — Generate recommendations
  - `recommend_retraining(weaknesses, max_modules)` — Suggest training modules
  - `get_retraining_plan(weaknesses, time_limit)` — Generate complete plan
- `TrainingModule` — A recommended training module

**Curriculum:**
- 5 modules per competency per scenario type
- Modules range from beginner to advanced
- Each module has estimated duration and competencies addressed
- Modules are selected based on weakness severity (severe → beginner, moderate → intermediate)

**Example Recommendations:**

| Weakness | Recommended Module | Duration |
|---|---|---|
| Poor PPE selection | Fire PPE Fundamentals | 12 min |
| Weak hazard identification | Fire Hazard Recognition | 15 min |
| Low procedure compliance | Fire Evacuation Procedures | 10 min |

**Principle:** Recommend only necessary, targeted training. Do not recommend full-course retraining for minor weaknesses.

## Sample Data (`sample_data/`)

Four fire scenario samples for testing:

1. **Good Assessment** (`FIRE_ASSESSMENT_GOOD`)
   - Expected result: PASS
   - Overall score: ~88%
   - Events: Correct hazard ID, appropriate PPE, minor procedural issue

2. **Poor Assessment** (`FIRE_ASSESSMENT_POOR`)
   - Expected result: FAIL
   - Overall score: ~19%
   - Events: Missed hazards, wrong PPE, major procedural violations

3. **Critical Error** (`FIRE_ASSESSMENT_CRITICAL_ERROR`)
   - Expected result: FAIL (critical error override)
   - Events: Good initial choices, then critical violation (re-entered unsafe area)

4. **Weak Area** (`FIRE_ASSESSMENT_WEAK_AREA`)
   - Expected result: FAIL (specific weak competency)
   - Overall score: ~74% (above 70% but fails on per-competency threshold)
   - Events: Correct hazard/equipment/evacuation, but failed PPE selection
   - Demonstrates weakness detection + targeted retraining

**Usage:**
```python
from ml.competency.sample_data import get_sample_fire_assessment

assessment = get_sample_fire_assessment("good")  # "good", "poor", "critical", "weak_area"
```

## Tests (`tests/`)

Comprehensive test suite with 36 tests covering:

**Scoring Engine Tests:**
- Scorer initialization
- Good assessment → PASS
- Poor assessment → FAIL
- Critical error → FAIL (override)
- Event processing and logging
- Result serialization

**Weakness Detection Tests:**
- Weakness detection for poor assessments
- No weaknesses for good assessments
- Severity classification (severe/moderate/mild)
- Weakness data structure validation

**Retraining Tests:**
- Module recommendations for weaknesses
- Minimal recommendations for good assessments
- Complete retraining plan generation
- Module data structure validation

**Integration Tests:**
- End-to-end good assessment workflow
- End-to-end poor assessment workflow
- End-to-end critical error workflow

**Public Interface Tests** (``assess()``):
- PASS case: score ≥ 70, no critical action, competent
- FAIL case: low score, not competent
- Critical action case: automatic FAIL regardless of score
- Weak area case: weakness detected, matching retraining returned
- Multiple weaknesses: all reported (not just one)
- Empty/None input: safe zero-score FAIL
- Non-dict events: safely skipped

**Run Tests:**
```bash
pytest ml/competency/tests/ -v
```

## Usage Example

### Simple: one-call assessment

```python
from ml.competency import assess
from ml.competency.sample_data import get_sample_fire_assessment

assessment = get_sample_fire_assessment("good")
result = assess(assessment["events"])

print(f"Score: {result['score']:.1f}")           # Score: 88.4
print(f"Passed: {result['passed']}")             # Passed: True
print(f"Status: {result['competency_status']}")  # Status: competent
print(f"Weaknesses: {result['weaknesses']}")     # Weaknesses: []
print(f"Retraining: {result['retraining']}")     # Retraining: []
```

### Advanced: direct pipeline access

```python
from ml.competency.scoring import CompetencyScorer
from ml.competency.weakness_detection import WeaknessDetector
from ml.competency.retraining import RetrainingRecommender
from ml.competency.sample_data import get_sample_fire_assessment

# 1. Score the assessment
scorer = CompetencyScorer(scenario_type="fire")
assessment = get_sample_fire_assessment("good")

for event in assessment["events"]:
    scorer.process_event(event)

result = scorer.get_result()
print(f"Passed: {result.passed}")
print(f"Overall Score: {result.overall_score:.1f}%")
print(f"Competencies: {list(result.competency_scores.keys())}")

# 2. Detect weaknesses
detector = WeaknessDetector()
weaknesses = detector.detect_weaknesses(result)
print(f"Weaknesses: {len(weaknesses)}")
for w in weaknesses:
    print(f"  - {w.competency_name}: {w.score:.1f}% ({w.severity})")

# 3. Get retraining recommendations
recommender = RetrainingRecommender(scenario_type="fire")
plan = recommender.get_retraining_plan(weaknesses)
print(f"Recommended Modules: {len(plan['recommended_modules'])}")
for module in plan["recommended_modules"]:
    print(f"  - {module['name']} ({module['estimated_duration_minutes']} min)")
```

## Configuration

All thresholds and scoring parameters are defined in `scoring/config.py`:

```python
OVERALL_PASS_THRESHOLD = 70.0          # Minimum overall score to pass
WEAKNESS_THRESHOLD = 60.0               # Below this = weakness
SEVERE_WEAKNESS_THRESHOLD = 50.0        # Below this = severe weakness
```

Per-competency pass thresholds are defined in `CompetencyDefinition`:
```python
FIRE_COMPETENCIES = {
    "hazard_identification": CompetencyDefinition(
        pass_threshold=75.0,  # Customize per competency
        ...
    ),
    ...
}
```

## Important Notes

⚠️ **PROTOTYPE STATUS**

This implementation uses **example thresholds and scoring rules** designed for demonstration and testing only.

**Before production deployment, you must:**
1. Validate all pass/fail thresholds with industrial safety experts
2. Verify competency definitions against official SOPs (Standard Operating Procedures)
3. Calibrate scoring point values to real-world scenarios
4. Define critical errors in consultation with safety stakeholders
5. Align retraining curriculum with authoritative training materials
6. Conduct pilot testing and gather empirical performance data

**Not Implemented:**
- No neural network or machine learning models
- No statistical analysis of performance trends
- No adaptive difficulty or personalization
- No integration with actual training content

**Assumptions Made:**
- Events arrive in chronological order
- Each event contains required fields (event_type, etc.)
- Assessments represent single, discrete scenarios
- Competencies are independent (no hierarchies or dependencies)

## Future Enhancements

- Statistical analysis of performance over time
- Adaptive competency thresholds based on role/experience level
- Machine learning models for improved scoring
- Integration with training content delivery
- Real-time feedback during assessments
- Peer/cohort comparison analytics