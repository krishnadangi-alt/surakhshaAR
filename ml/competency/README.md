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

This is a **prototype implementation** and uses example thresholds and scoring rules. 
**All scoring thresholds, pass/fail criteria, and competency definitions must be validated against official industrial safety procedures and domain experts before production deployment.**

## Structure

| Path | Purpose |
|---|---|
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
- Scores range from 0–100
- Correct actions add points; mistakes subtract points
- Scores are bounded to [0, 100]

**Scoring Rules:**
- Correct hazard identification: +25 points
- Correct PPE: +30 points
- Correct equipment use: +25 points
- Correct evacuation: +35 points
- Wrong action (minor): -10 procedure, -5 decision-making
- Wrong action (major): -25 procedure, -20 decision-making

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

Three fire scenario samples for testing:

1. **Good Assessment** (`FIRE_ASSESSMENT_GOOD`)
   - Expected result: PASS
   - Overall score: ~85%
   - Events: Correct hazard ID, appropriate PPE, minor procedural issue

2. **Poor Assessment** (`FIRE_ASSESSMENT_POOR`)
   - Expected result: FAIL
   - Overall score: ~35%
   - Events: Missed hazards, wrong PPE, major procedural violations

3. **Critical Error** (`FIRE_ASSESSMENT_CRITICAL_ERROR`)
   - Expected result: FAIL (critical error override)
   - Events: Good initial choices, then critical violation (re-entered unsafe area)

**Usage:**
```python
from ml.competency.sample_data import get_sample_fire_assessment

assessment = get_sample_fire_assessment("good")  # "good", "poor", or "critical"
```

## Tests (`tests/`)

Comprehensive test suite with 20+ tests covering:

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

**Run Tests:**
```bash
pytest ml/competency/tests/ -v
```

## Usage Example

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