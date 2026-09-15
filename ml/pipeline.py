"""
ML Pipeline for SurakshaAR Worker Competency Prediction.

PROTOTYPE DISCLAIMER:
This ML pipeline uses prototype synthetic worker telemetry data for hackathon
demonstration. The ML model provides a secondary prediction layer and DOES NOT
override rule-based safety controls or critical safety error fails.
"""

import os
import csv
import joblib
import numpy as np
from typing import Dict, List, Tuple, Any

from sklearn.model_selection import train_test_split
from sklearn.linear_model import LogisticRegression
from sklearn.tree import DecisionTreeClassifier
from sklearn.ensemble import RandomForestClassifier
from sklearn.metrics import accuracy_score, precision_score, recall_score, f1_score


FEATURE_NAMES = [
    "correct_action_rate",
    "wrong_action_rate",
    "unsafe_action_rate",
    "avg_response_time",
    "completion_rate",
    "critical_error_flag"
]

DEFAULT_DATASET_PATH = os.path.join(os.path.dirname(__file__), "dataset", "worker_competency_dataset.csv")
DEFAULT_MODEL_PATH = os.path.join(os.path.dirname(__file__), "models", "competency_model.joblib")


def load_and_clean_data(file_path: str = DEFAULT_DATASET_PATH) -> List[Dict]:
    """Load, validate, and clean raw dataset records."""
    if not os.path.exists(file_path):
        from .dataset.generate_dataset import save_dataset
        save_dataset(file_path)

    raw_records = []
    with open(file_path, "r", encoding="utf-8") as f:
        reader = csv.DictReader(f)
        for row in reader:
            raw_records.append(row)

    cleaned_records = []
    seen = set()

    for row in raw_records:
        # 1. Deduplication key
        dedup_key = (row.get("worker_id"), row.get("module"), row.get("attempts"), row.get("score"))
        if dedup_key in seen:
            continue
        seen.add(dedup_key)

        # 2. Missing & invalid value handling
        try:
            correct_actions = max(0, int(row.get("correct_actions", 0)))
            wrong_actions = max(0, int(row.get("wrong_actions", 0)))
            unsafe_actions = max(0, int(row.get("unsafe_actions", 0)))
            critical_errors = max(0, int(row.get("critical_errors", 0)))
            response_time = max(0.1, float(row.get("response_time", 5.0)))
            completion = 1 if int(row.get("completion", 0)) > 0 else 0
            score = float(row.get("score", 0.0))
            attempts = max(1, int(row.get("attempts", 1)))
            competency = 1 if int(row.get("competency", 0)) > 0 else 0
        except (ValueError, TypeError):
            continue  # Skip invalid rows

        # Label validation: critical error or incomplete implies non-competent
        if critical_errors > 0 or completion == 0:
            competency = 0

        cleaned_records.append({
            "worker_id": row.get("worker_id", "unknown"),
            "module": row.get("module", "fire"),
            "scenario": row.get("scenario", "default"),
            "correct_actions": correct_actions,
            "wrong_actions": wrong_actions,
            "unsafe_actions": unsafe_actions,
            "critical_errors": critical_errors,
            "response_time": response_time,
            "completion": completion,
            "score": score,
            "attempts": attempts,
            "competency": competency
        })

    return cleaned_records


def extract_features_and_targets(records: List[Dict]) -> Tuple[np.ndarray, np.ndarray]:
    """Perform feature engineering on dataset records.
    
    Features engineered:
    - correct_action_rate: correct / total_actions
    - wrong_action_rate: wrong / total_actions
    - unsafe_action_rate: unsafe / total_actions
    - avg_response_time: numeric seconds
    - completion_rate: 1.0 or 0.0
    - critical_error_flag: 1.0 if critical_errors > 0 else 0.0
    """
    X_list = []
    y_list = []

    for r in records:
        total_actions = r["correct_actions"] + r["wrong_actions"] + r["unsafe_actions"]
        if total_actions == 0:
            total_actions = 1

        c_rate = r["correct_actions"] / total_actions
        w_rate = r["wrong_actions"] / total_actions
        u_rate = r["unsafe_actions"] / total_actions
        rt = r["response_time"]
        comp = float(r["completion"])
        crit_flag = 1.0 if r["critical_errors"] > 0 else 0.0

        X_list.append([c_rate, w_rate, u_rate, rt, comp, crit_flag])
        y_list.append(r["competency"])

    return np.array(X_list), np.array(y_list)


def compare_and_train_pipeline(
    file_path: str = DEFAULT_DATASET_PATH,
    model_export_path: str = DEFAULT_MODEL_PATH
) -> Dict[str, Any]:
    """Train and evaluate candidate models; save best model."""
    records = load_and_clean_data(file_path)
    X, y = extract_features_and_targets(records)

    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=0.2, random_state=42, stratify=y if len(np.unique(y)) > 1 else None
    )

    models = {
        "LogisticRegression": LogisticRegression(random_state=42),
        "DecisionTree": DecisionTreeClassifier(max_depth=5, random_state=42),
        "RandomForest": RandomForestClassifier(n_estimators=50, max_depth=5, random_state=42)
    }

    results = {}
    best_name = None
    best_f1 = -1.0
    best_model = None

    for name, model in models.items():
        model.fit(X_train, y_train)
        y_pred = model.predict(X_test)

        acc = float(accuracy_score(y_test, y_pred))
        prec = float(precision_score(y_test, y_pred, zero_division=0))
        rec = float(recall_score(y_test, y_pred, zero_division=0))
        f1 = float(f1_score(y_test, y_pred, zero_division=0))

        results[name] = {
            "accuracy": acc,
            "precision": prec,
            "recall": rec,
            "f1_score": f1,
            "model": model
        }

        if f1 > best_f1:
            best_f1 = f1
            best_name = name
            best_model = model

    # Save best model
    os.makedirs(os.path.dirname(model_export_path), exist_ok=True)
    joblib.dump(best_model, model_export_path)

    return {
        "selected_model": best_name,
        "best_f1": best_f1,
        "model_path": model_export_path,
        "comparison": {
            k: {m: v[m] for m in ["accuracy", "precision", "recall", "f1_score"]}
            for k, v in results.items()
        },
        "dataset_records": len(records),
        "train_size": len(X_train),
        "test_size": len(X_test)
    }


def predict_competency(
    event_summary: Dict[str, Any],
    model_path: str = DEFAULT_MODEL_PATH
) -> Dict[str, Any]:
    """Predict competency for a single session event summary using the saved model."""
    correct = event_summary.get("correct_actions", 0)
    wrong = event_summary.get("wrong_actions", 0)
    unsafe = event_summary.get("unsafe_actions", 0)
    total = max(1, correct + wrong + unsafe)

    c_rate = correct / total
    w_rate = wrong / total
    u_rate = unsafe / total
    rt = float(event_summary.get("response_time", 5.0))
    comp = 1.0 if event_summary.get("completion", True) else 0.0
    crit_flag = 1.0 if event_summary.get("critical_errors", 0) > 0 or event_summary.get("critical", False) else 0.0

    features = np.array([[c_rate, w_rate, u_rate, rt, comp, crit_flag]])

    if os.path.exists(model_path):
        model = joblib.load(model_path)
    else:
        # Fallback inline model
        model = RandomForestClassifier(n_estimators=20, max_depth=5, random_state=42)
        X_dummy = np.array([[1.0, 0.0, 0.0, 3.0, 1.0, 0.0], [0.0, 0.5, 0.5, 15.0, 0.0, 1.0]])
        y_dummy = np.array([1, 0])
        model.fit(X_dummy, y_dummy)

    prediction = int(model.predict(features)[0])
    prob = float(model.predict_proba(features)[0][1]) if hasattr(model, "predict_proba") else (1.0 if prediction == 1 else 0.0)

    explanation = explain_prediction(features[0], prediction, prob)

    return {
        "predicted_competency": "competent" if prediction == 1 else "not_competent",
        "confidence": round(prob, 4),
        "features": {
            "correct_action_rate": round(c_rate, 4),
            "wrong_action_rate": round(w_rate, 4),
            "unsafe_action_rate": round(u_rate, 4),
            "avg_response_time": round(rt, 2),
            "completion_rate": comp,
            "critical_error_flag": crit_flag
        },
        "explanation": explanation
    }


def explain_prediction(features: np.ndarray, prediction: int, prob: float) -> List[str]:
    """Generate human-readable explainability notes for model predictions."""
    c_rate, w_rate, u_rate, rt, comp, crit_flag = features
    reasons = []

    if crit_flag > 0:
        reasons.append("Critical safety error recorded (strong negative impact on competency)")
    if comp == 0:
        reasons.append("Incomplete assessment session (negative impact)")
    if u_rate > 0:
        reasons.append(f"Unsafe action rate is {u_rate*100:.1f}% (increased hazard exposure)")
    if w_rate > 0.3:
        reasons.append(f"High wrong-action rate ({w_rate*100:.1f}%)")
    if c_rate >= 0.7:
        reasons.append(f"High correct-action rate ({c_rate*100:.1f}%) contributed positively")
    if rt > 15.0:
        reasons.append(f"Procedural response latency of {rt:.1f}s incurred penalty")

    if not reasons:
        reasons.append("Balanced performance across standard evaluation metrics")

    return reasons


def compare_rule_vs_ml(rule_result: Dict[str, Any], ml_prediction: Dict[str, Any]) -> Dict[str, Any]:
    """Compare rule-based deterministic verdict against ML prediction.
    
    Safety Guarantee: Rule-based critical safety error automatic FAIL CANNOT
    be overridden by ML prediction.
    """
    rule_status = "competent" if rule_result.get("passed", False) else "not_competent"
    ml_status = ml_prediction.get("predicted_competency", "not_competent")

    agreement = (rule_status == ml_status)

    final_verdict = rule_status  # Rule-based system is authoritative for safety

    return {
        "rule_based_verdict": rule_status,
        "ml_predicted_verdict": ml_status,
        "agreement": agreement,
        "final_authoritative_verdict": final_verdict,
        "rule_passed": rule_result.get("passed", False),
        "rule_score": rule_result.get("score", 0.0),
        "ml_confidence": ml_prediction.get("confidence", 0.0),
        "override_blocked": (rule_status == "not_competent" and ml_status == "competent")
    }
