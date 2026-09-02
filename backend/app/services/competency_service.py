"""Bridge between the FastAPI backend and the ML competency engine.

The ML engine lives in the repository-level ``ml`` package and is intentionally
framework-free. This service adapts it to the backend domain (workers, modules,
assessments) and converts the engine output into JSON-serialisable structures
that can be stored on the Assessment model and returned by the API.

Pipeline applied to every assessment:

1. ``CompetencyScorer`` - scores the raw behavioural events.
2. ``WeaknessDetector`` - derives weaknesses from below-threshold scores.
3. ``RetrainingRecommender`` - builds a targeted retraining plan.
"""

from __future__ import annotations

import sys
from pathlib import Path
from typing import Any, Dict, List

# Make the repository-level ``ml`` package importable no matter how the backend
# is started (uvicorn via run.py, pytest, or as an imported module).
_REPO_ROOT = Path(__file__).resolve().parents[3]
if str(_REPO_ROOT) not in sys.path:
    sys.path.insert(0, str(_REPO_ROOT))

from ml.competency.retraining.recommender import RetrainingRecommender  # noqa: E402
from ml.competency.scoring.engine import CompetencyScorer  # noqa: E402
from ml.competency.weakness_detection.detector import WeaknessDetector  # noqa: E402

from app.models.assessment import Assessment  # noqa: E402
from sqlalchemy.orm import Session  # noqa: E402

# Scenario types supported by the competency engine. They map 1:1 to the
# training module codes seeded in the backend (fire / gas).
SUPPORTED_SCENARIOS = ("fire", "gas")

# Default time budget handed to the retraining recommender.
DEFAULT_RETRAINING_TIME_LIMIT_MINUTES = 120


class UnsupportedScenarioError(ValueError):
    """Raised when a module/scenario cannot be scored by the competency engine."""


def weakness_to_dict(weakness) -> Dict[str, Any]:
    """Serialise a Weakness dataclass into a JSON-safe dict."""
    return {
        "competency_name": weakness.competency_name,
        "score": weakness.score,
        "threshold": weakness.threshold,
        "severity": weakness.severity,
        "reason": weakness.reason,
        "affected_aspects": list(weakness.affected_aspects),
    }


def score_events(
    scenario_type: str,
    events: List[Dict[str, Any]],
    *,
    retraining_time_limit_minutes: int = DEFAULT_RETRAINING_TIME_LIMIT_MINUTES,
) -> Dict[str, Any]:
    """Run behavioural events through the full competency pipeline.

    Args:
        scenario_type: ``fire`` or ``gas``.
        events: Raw assessment events as produced by the VR worker app.
        retraining_time_limit_minutes: Time budget for the retraining plan.

    Returns:
        Dict with ``result`` (scoring output), ``weaknesses`` (detected
        weaknesses) and ``retraining_plan`` (targeted modules).

    Raises:
        UnsupportedScenarioError: If the scenario is unknown or no events given.
    """
    if scenario_type not in SUPPORTED_SCENARIOS:
        raise UnsupportedScenarioError(
            f"Unsupported scenario type '{scenario_type}'; "
            f"expected one of {list(SUPPORTED_SCENARIOS)}"
        )
    if not events:
        raise UnsupportedScenarioError(
            "At least one assessment event is required to score competency"
        )

    scorer = CompetencyScorer(scenario_type)
    for event in events:
        scorer.process_event(event)
    result = scorer.get_result()

    weaknesses = WeaknessDetector().detect_weaknesses(result)

    retraining_plan = RetrainingRecommender(scenario_type).get_retraining_plan(
        weaknesses,
        total_time_limit_minutes=retraining_time_limit_minutes,
    )

    return {
        "result": result.to_dict(),
        "weaknesses": [weakness_to_dict(w) for w in weaknesses],
        "retraining_plan": retraining_plan,
    }


def next_attempt_number(db: Session, worker_id: int, module_id: int) -> int:
    """Return the next assessment attempt number for a worker + module pair."""
    latest = (
        db.query(Assessment)
        .filter(
            Assessment.worker_id == worker_id,
            Assessment.module_id == module_id,
        )
        .order_by(Assessment.attempt_number.desc())
        .first()
    )
    return latest.attempt_number + 1 if latest else 1