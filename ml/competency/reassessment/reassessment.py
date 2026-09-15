"""Reassessment Evaluation Module.

Evaluates worker performance improvement after targeted retraining by comparing
pre-retraining baseline scoring results against post-retraining reassessment results.
"""

from typing import Dict, List, Any


def evaluate_reassessment(
    previous_result: Any,
    new_result: Any,
    target_weaknesses: List[str] = None
) -> Dict[str, Any]:
    """
    Compare post-retraining reassessment result against previous baseline result.
    
    Args:
        previous_result: Baseline ScoringResult (or dict) before retraining.
        new_result: Reassessment ScoringResult (or dict) after retraining.
        target_weaknesses: Optional list of weak competency names targeted for retraining.
        
    Returns:
        Structured reassessment evaluation dictionary.
    """
    # Extract overall scores
    prev_overall = getattr(previous_result, "overall_score", None)
    if prev_overall is None and isinstance(previous_result, dict):
        prev_overall = previous_result.get("score", previous_result.get("overall_score", 0.0))
    prev_overall = float(prev_overall or 0.0)

    new_overall = getattr(new_result, "overall_score", None)
    if new_overall is None and isinstance(new_result, dict):
        new_overall = new_result.get("score", new_result.get("overall_score", 0.0))
    new_overall = float(new_overall or 0.0)

    score_gain = round(new_overall - prev_overall, 2)
    overall_improved = score_gain > 0.0

    # Extract competency scores dicts
    prev_scores = _extract_scores_dict(previous_result)
    new_scores = _extract_scores_dict(new_result)

    # Determine target weak competencies to evaluate
    if not target_weaknesses:
        target_weaknesses = [
            name for name, sc in prev_scores.items()
            if sc.get("score", 0.0) < sc.get("threshold", 70.0)
        ]

    resolved_weaknesses = []
    persistent_weaknesses = []
    competency_gains = {}

    for comp in target_weaknesses:
        prev_c = prev_scores.get(comp, {"score": 0.0, "threshold": 70.0})
        new_c = new_scores.get(comp, {"score": 0.0, "threshold": 70.0})

        c_gain = round(new_c["score"] - prev_c["score"], 2)
        competency_gains[comp] = c_gain

        if new_c["score"] >= new_c["threshold"]:
            resolved_weaknesses.append(comp)
        else:
            persistent_weaknesses.append(comp)

    # Check overall pass status
    new_passed = getattr(new_result, "passed", None)
    if new_passed is None and isinstance(new_result, dict):
        new_passed = new_result.get("passed", False)

    return {
        "previous_score": prev_overall,
        "new_score": new_overall,
        "score_gain": score_gain,
        "overall_improved": overall_improved,
        "passed": bool(new_passed),
        "target_weaknesses_evaluated": target_weaknesses,
        "resolved_weaknesses": resolved_weaknesses,
        "persistent_weaknesses": persistent_weaknesses,
        "competency_gains": competency_gains,
        "learning_loop_status": "COMPLETED_PASS" if (new_passed and len(persistent_weaknesses) == 0) else "NEEDS_FURTHER_PRACTICE"
    }


def _extract_scores_dict(result: Any) -> Dict[str, Dict[str, float]]:
    """Helper to convert ScoringResult or dict into standardized comp -> {score, threshold}."""
    scores_dict = {}
    
    if hasattr(result, "competency_scores") and isinstance(result.competency_scores, dict):
        for name, comp_score in result.competency_scores.items():
            if hasattr(comp_score, "score"):
                s = comp_score.score
                t = getattr(comp_score, "pass_threshold", 70.0)
            elif isinstance(comp_score, dict):
                s = comp_score.get("score", 0.0)
                t = comp_score.get("pass_threshold", 70.0)
            else:
                s = float(comp_score)
                t = 70.0
            scores_dict[name] = {"score": float(s), "threshold": float(t)}

    elif isinstance(result, dict):
        comp_scores = result.get("competency_scores", {})
        if isinstance(comp_scores, dict):
            for name, item in comp_scores.items():
                if isinstance(item, dict):
                    scores_dict[name] = {
                        "score": float(item.get("score", 0.0)),
                        "threshold": float(item.get("pass_threshold", 70.0))
                    }
                else:
                    scores_dict[name] = {"score": float(item), "threshold": 70.0}

    return scores_dict
