"""Public interface for the ML competency engine.

Provides a single `assess()` function that converts Fire/Gas assessment
events into:
  - overall score (mean of category scores)
  - pass/fail decision
  - competency status (competent / not_competent)
  - weakness identification (low-scoring categories)
  - targeted retraining recommendations (1:1 with weaknesses)

This is the one clean entry point for application/backend integration.
The engine computes the result itself; client-supplied passed/score values
are never trusted.
"""

from typing import Any, Dict, List

from .scoring.engine import CompetencyScorer
from .weakness_detection.detector import WeaknessDetector


def assess(
    events: List[Dict[str, Any]],
    scenario_type: str = "fire",
) -> Dict[str, Any]:
    """Assess worker competency from a list of behavioural events.

    Args:
        events: Ordered list of assessment event dicts. Each dict must
            contain at least an ``event_type`` key. Unknown/missing
            fields are handled safely (treated as incorrect/empty).
        scenario_type: ``"fire"`` or ``"gas"``. Defaults to ``"fire"``.

    Returns:
        A dict with the canonical public shape::

            {
                "score":            <float>,   # overall score 0-100
                "passed":           <bool>,    # True if competent
                "competency_status": <str>,    # "competent" | "not_competent"
                "weaknesses":       [<str>],   # low-scoring category names
                "retraining":       [<str>],   # matching retraining categories
            }

    Notes:
        - An empty / non-list event list returns a zero-score FAIL.
        - Non-dict events in the list are skipped.
        - A ``critical_action`` event triggers automatic FAIL regardless
          of the numerical score.
    """
    # Guard against invalid input
    if not events or not isinstance(events, list):
        return {
            "score": 0.0,
            "passed": False,
            "competency_status": "not_competent",
            "weaknesses": [],
            "retraining": [],
        }

    # 1. Score the events
    scorer = CompetencyScorer(scenario_type)
    for event in events:
        if isinstance(event, dict):
            scorer.process_event(event)
    result = scorer.get_result()

    # 2. Detect weaknesses from below-threshold categories
    weaknesses = WeaknessDetector().detect_weaknesses(result)
    weakness_names: List[str] = [w.competency_name for w in weaknesses]

    # 3. Targeted retraining: deterministic 1:1 mapping.
    #    Each weakness maps to its own category for retraining.
    #    Example: weakness ppe_selection -> retraining ppe_selection
    retraining_names: List[str] = list(weakness_names)

    return {
        "score": result.overall_score,
        "passed": result.passed,
        "competency_status": "competent" if result.passed else "not_competent",
        "weaknesses": weakness_names,
        "retraining": retraining_names,
    }
