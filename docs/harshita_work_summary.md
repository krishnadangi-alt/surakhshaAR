# HARSHITA — Intelligence Layer Deliverables Summary (Days 2 – 5)

**SurakshaAR | SIH 2026 — Problem Statement PS 26041**  
**Role**: Intelligence Layer Lead (Machinery Module, ML Pipeline, Competency Engine, Weakness Detection, Adaptive Retraining, Reassessment)

---

## 1. Executive Summary

This document summarizes the intelligence layer deliverables completed for Days 2 through 5. The core pipeline connects raw VR/AR assessment telemetry to structured competency evaluations, weakness detection with repeated mistake tracking, targeted retraining recommendations, reassessment improvement metrics, and ML prediction comparisons.

```
Worker VR/AR Session
        │
        ▼
Rehan Assessment Logger
        │
        ▼
Harshita Intelligence Layer (Rule-Based Baseline & ML Model)
   ├── Score + Competency Evaluation
   ├── Weakness Detection (with Repeated Mistake Tracking & affected_aspects)
   ├── Targeted Retraining Recommendation (Practice & Reassessment Conditions)
   └── Reassessment Improvement Tracking (Pre- vs Post-Retraining)
        │
        ├──► Omesh Backend / DB Storage Payload
        ├──► Kanishka Admin Dashboard Analytics Feed
        └──► Rehan Training Engine Handshake
```

---

## 2. Day 2 Deliverables: ML Dataset & Pipeline

- **Dataset**: Created clearly labeled synthetic prototype dataset [`ml/dataset/worker_competency_dataset.csv`](file:///e:/SurakshaAR/surakhshaAR/ml/dataset/worker_competency_dataset.csv) (500 records) with fields: `worker_id`, `module`, `scenario`, `correct_actions`, `wrong_actions`, `unsafe_actions`, `critical_errors`, `response_time`, `completion`, `score`, `attempts`, `competency`.
- **Feature Engineering**:
  - `correct_action_rate`
  - `wrong_action_rate`
  - `unsafe_action_rate`
  - `avg_response_time`
  - `completion_rate`
  - `critical_error_flag`
- **Model Comparison Results (Measured empirical test results)**:
  | Model Name | Accuracy | Precision | Recall | F1-Score | Status |
  | :--- | :--- | :--- | :--- | :--- | :--- |
  | Logistic Regression | $82.0\%$ | $82.5\%$ | $88.1\%$ | $85.2\%$ | Evaluated |
  | **Decision Tree (Selected)** | $\mathbf{88.0\%}$ | $\mathbf{91.2\%}$ | $\mathbf{88.1\%}$ | $\mathbf{89.7\%}$ | **Selected & Saved** |
  | Random Forest | $86.0\%$ | $89.5\%$ | $86.4\%$ | $87.9\%$ | Evaluated |

- **Saved Artifact**: Exported `DecisionTree` model to [`ml/models/competency_model.joblib`](file:///e:/SurakshaAR/surakhshaAR/ml/models/competency_model.joblib).
- **Rule-Based vs. ML Safety Guarantee**: The ML model provides secondary predictions. Critical safety errors (`critical_action`, `critical=True`, `severity="critical"`) trigger an automatic rule-based FAIL that **cannot be overridden** by ML predictions.

---

## 3. Day 3 Deliverables: Machinery Competency Mapping

- Documented in [`docs/machinery_competency_mapping.md`](file:///e:/SurakshaAR/surakhshaAR/docs/machinery_competency_mapping.md).
- Competencies: `hazard_identification`, `ppe_selection`, `loto_procedure`, `equipment_use`, `emergency_response`.
- Safety Rules:
  - Critical Error: Automatic FAIL (`reach_into_live_gear`).
  - Unsafe Action: Penalty `-22.0` (`bypass_safety_interlock`).
  - Response Time: E-Stop reaction benchmark `< 2.5s`.

---

## 4. Day 4 Deliverables: Weakness Detection & Repeated Mistake Tracking

- **Detector**: [`ml/competency/weakness_detection/detector.py`](file:///e:/SurakshaAR/surakhshaAR/ml/competency/weakness_detection/detector.py).
- **Repeated Mistake Analysis**: Analyzes worker attempt history to mark persistent weaknesses (`is_persistent=True`) when a worker repeatedly fails the same competency across sessions.
- **Structured Output**:
  - `competency_status`: `"competent"` / `"not_competent"`
  - `strong_areas`: Passed competencies
  - `weak_areas`: Below-threshold competencies
  - `affected_aspects`: Populated sub-aspects (e.g. `["select_respirator", "proper_fit_test"]`)
  - `repeated_mistakes`: List of persistent weak areas
  - `weakness_reasons`: Detailed evidence strings per weakness

---

## 5. Day 5 Deliverables: Adaptive Retraining, Reassessment & Integration Contracts

- **Recommender**: [`ml/competency/retraining/recommender.py`](file:///e:/SurakshaAR/surakhshaAR/ml/competency/retraining/recommender.py) maps weaknesses 1:1 to training modules with explicit `required_practice` and `reassessment_condition`.
- **Reassessment Evaluation**: [`ml/competency/reassessment/reassessment.py`](file:///e:/SurakshaAR/surakhshaAR/ml/competency/reassessment/reassessment.py) measures pre- vs. post-retraining scores:
  - `score_gain`: Numerical score improvement
  - `resolved_weaknesses`: Weaknesses cleared after retraining
  - `persistent_weaknesses`: Weaknesses remaining below threshold
  - `learning_loop_status`: `"COMPLETED_PASS"` / `"NEEDS_FURTHER_PRACTICE"`
- **Integration Contracts**: [`ml/competency/integration_contracts.py`](file:///e:/SurakshaAR/surakhshaAR/ml/competency/integration_contracts.py) provides formatted JSON payloads for:
  - **Rehan**: Action Logger & Training Engine handshake
  - **Omesh**: Backend DB/API storage payload
  - **Kanishka**: Admin Dashboard analytics feed

---

## 6. Sample Output Evidence (For SIH Demo/PPT)

```json
{
  "worker_id": 101,
  "scenario": "fire",
  "baseline_score": 58.5,
  "reassessment_score": 86.0,
  "score_gain": 27.5,
  "competency_status": "competent",
  "strong_areas": ["hazard_identification", "ppe_selection", "equipment_use", "procedure_compliance"],
  "weak_areas": [],
  "resolved_weaknesses": ["ppe_selection"],
  "persistent_weaknesses": [],
  "ml_prediction": {
    "predicted_competency": "competent",
    "confidence": 0.9123
  }
}
```
