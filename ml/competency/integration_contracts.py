"""Integration Contracts for Harshita's Intelligence Layer.

Provides structured, machine-readable data formatters for cross-member handoffs:
- Rehan: Action Logger & Training Engine handoff.
- Omesh: Backend DB / REST API payload contract.
- Kanishka: Admin Dashboard analytics data feed.
"""

from typing import Dict, List, Any


def format_rehan_handshake(
    scoring_result: Any,
    weakness_profile: Dict[str, Any],
    retraining_plan: Dict[str, Any]
) -> Dict[str, Any]:
    """Format intelligence output for Rehan's Training Engine & Action Logger."""
    return {
        "assessment_id": getattr(scoring_result, "assessment_id", "unknown"),
        "scenario_type": getattr(scoring_result, "scenario_type", "fire"),
        "competency_status": weakness_profile.get("competency_status", "not_competent"),
        "passed": getattr(scoring_result, "passed", False),
        "overall_score": getattr(scoring_result, "overall_score", 0.0),
        "critical_errors": getattr(scoring_result, "critical_errors", []),
        "weak_areas": weakness_profile.get("weak_areas", []),
        "affected_aspects": weakness_profile.get("affected_aspects", []),
        "repeated_mistakes": weakness_profile.get("repeated_mistakes", []),
        "recommended_training_modules": retraining_plan.get("recommended_modules", []),
        "required_practice": [
            m.get("required_practice", "") for m in retraining_plan.get("recommended_modules", [])
        ],
        "reassessment_condition": [
            m.get("reassessment_condition", "") for m in retraining_plan.get("recommended_modules", [])
        ]
    }


def format_omesh_backend_payload(
    worker_id: int,
    module_id: int,
    scoring_result: Any,
    weakness_profile: Dict[str, Any],
    retraining_plan: Dict[str, Any],
    reassessment_eval: Dict[str, Any] = None
) -> Dict[str, Any]:
    """Format payload for Omesh's backend assessment & progress DB endpoints."""
    return {
        "worker_id": worker_id,
        "module_id": module_id,
        "scenario_type": getattr(scoring_result, "scenario_type", "fire"),
        "score": getattr(scoring_result, "overall_score", 0.0),
        "passed": getattr(scoring_result, "passed", False),
        "pass_reason": getattr(scoring_result, "pass_reason", ""),
        "competency_scores": {
            k: {
                "name": getattr(v, "name", k) if not isinstance(v, dict) else v.get("name", k),
                "score": getattr(v, "score", 0.0) if not isinstance(v, dict) else v.get("score", 0.0),
                "passed": getattr(v, "passed", False) if not isinstance(v, dict) else v.get("passed", False)
            }
            for k, v in getattr(scoring_result, "competency_scores", {}).items()
        },
        "weaknesses": weakness_profile.get("weak_areas", []),
        "affected_aspects": weakness_profile.get("affected_aspects", []),
        "critical_errors": getattr(scoring_result, "critical_errors", []),
        "retraining_recommendations": retraining_plan.get("recommended_modules", []),
        "progress_status": "certified" if getattr(scoring_result, "passed", False) else "retraining_required",
        "reassessment_summary": reassessment_eval or {}
    }


def format_kanishka_dashboard_feed(
    worker_id: int,
    scoring_result: Any,
    weakness_profile: Dict[str, Any],
    retraining_plan: Dict[str, Any],
    reassessment_eval: Dict[str, Any] = None,
    ml_prediction: Dict[str, Any] = None
) -> Dict[str, Any]:
    """Format analytics feed for Kanishka's Admin Dashboard UI."""
    return {
        "worker_id": worker_id,
        "scenario": getattr(scoring_result, "scenario_type", "fire"),
        "score": getattr(scoring_result, "overall_score", 0.0),
        "competency_status": weakness_profile.get("competency_status", "not_competent"),
        "strong_areas": weakness_profile.get("strong_areas", []),
        "weak_areas": weakness_profile.get("weak_areas", []),
        "affected_aspects": weakness_profile.get("affected_aspects", []),
        "repeated_mistakes": weakness_profile.get("repeated_mistakes", []),
        "critical_errors": getattr(scoring_result, "critical_errors", []),
        "retraining_status": "assigned" if weakness_profile.get("weak_areas") else "none",
        "recommended_modules": retraining_plan.get("recommended_modules", []),
        "reassessment_result": reassessment_eval or {},
        "ml_model_prediction": ml_prediction or {}
    }
