"""Unit tests for ML Dataset & Pipeline (Day 2 Deliverable)."""

import os
import pytest
from ml.pipeline import (
    load_and_clean_data,
    extract_features_and_targets,
    compare_and_train_pipeline,
    predict_competency,
    compare_rule_vs_ml,
    DEFAULT_DATASET_PATH,
    DEFAULT_MODEL_PATH
)


def test_dataset_loading_and_cleaning():
    """Verify synthetic dataset loads and cleans without errors."""
    records = load_and_clean_data(DEFAULT_DATASET_PATH)
    assert len(records) > 0
    sample = records[0]
    assert "correct_actions" in sample
    assert "wrong_actions" in sample
    assert "unsafe_actions" in sample
    assert "critical_errors" in sample
    assert "competency" in sample


def test_feature_extraction():
    """Verify feature matrix and target vector shapes."""
    records = load_and_clean_data(DEFAULT_DATASET_PATH)
    X, y = extract_features_and_targets(records)
    assert len(X) == len(records)
    assert len(y) == len(records)
    assert X.shape[1] == 6  # 6 features


def test_model_training_and_export():
    """Verify training pipeline compares models and exports best artifact."""
    res = compare_and_train_pipeline(DEFAULT_DATASET_PATH, DEFAULT_MODEL_PATH)
    assert "selected_model" in res
    assert "best_f1" in res
    assert os.path.exists(DEFAULT_MODEL_PATH)


def test_prediction_and_explainability():
    """Verify competency prediction and explanation notes."""
    event_summary = {
        "correct_actions": 6,
        "wrong_actions": 0,
        "unsafe_actions": 0,
        "critical_errors": 0,
        "response_time": 3.5,
        "completion": True
    }
    pred = predict_competency(event_summary, DEFAULT_MODEL_PATH)
    assert pred["predicted_competency"] in ["competent", "not_competent"]
    assert len(pred["explanation"]) > 0


def test_rule_vs_ml_safety_guarantee():
    """Verify critical safety errors block ML override."""
    rule_result = {"passed": False, "score": 85.0, "critical_errors": ["Standing in fire"]}
    ml_pred = {"predicted_competency": "competent", "confidence": 0.92}

    cmp_res = compare_rule_vs_ml(rule_result, ml_pred)
    assert cmp_res["rule_based_verdict"] == "not_competent"
    assert cmp_res["final_authoritative_verdict"] == "not_competent"
    assert cmp_res["override_blocked"] is True
