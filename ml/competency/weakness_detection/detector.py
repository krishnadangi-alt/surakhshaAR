"""
Weakness detection engine.

Identifies performance weaknesses from competency scores and assessment results.
"""

from dataclasses import dataclass
from typing import List, Dict, Any
from ..scoring.config import WEAKNESS_THRESHOLD, SEVERE_WEAKNESS_THRESHOLD


def _aspects_for_competency(scoring_result, competency_name: str) -> List[str]:
    """Return the sub-aspect list for a competency from its definition.

    Day 1 (Rehan rule 6): ``affected_aspects`` must be populated from the
    relevant competency sub-aspects when a competency is weak, never left
    permanently empty. The scorer stores the live ``CompetencyDefinition``
    objects (with ``aspects``) on ``scoring_result`` via
    ``CompetencyScorer.get_result()``; fall back to ``[]`` only when the
    definition is unavailable.
    """
    definitions = getattr(scoring_result, "competency_definitions", None) or {}
    definition = definitions.get(competency_name)
    aspects = getattr(definition, "aspects", None) if definition else None
    return list(aspects) if aspects else []


@dataclass
class Weakness:
    """A detected performance weakness."""
    competency_name: str
    score: float
    threshold: float
    severity: str  # "severe", "moderate", "mild"
    reason: str  # Why this is a weakness
    affected_aspects: List[str]  # Sub-competencies affected
    is_persistent: bool = False  # Repeated weakness across multiple attempts


class WeaknessDetector:
    """Detects performance weaknesses and repeated mistakes from assessment results."""
    
    def __init__(self):
        """Initialize weakness detector."""
        pass
    
    def detect_weaknesses(
        self,
        scoring_result,
        attempt_history: List[Any] = None
    ) -> List[Weakness]:
        """
        Identify weaknesses from a scoring result and optional attempt history.
        
        Args:
            scoring_result: ScoringResult object from scorer
            attempt_history: List of previous ScoringResult objects for the worker
            
        Returns:
            List of Weakness objects
        """
        weaknesses = []
        previous_weak_names = set()

        # Extract weak competency names from previous attempts
        if attempt_history:
            for prev_res in attempt_history:
                if hasattr(prev_res, "competency_scores"):
                    for c_name, c_score in prev_res.competency_scores.items():
                        score_val = getattr(c_score, "score", 0.0) if not isinstance(c_score, (int, float)) else float(c_score)
                        thresh_val = getattr(c_score, "pass_threshold", 70.0) if not isinstance(c_score, (int, float)) else 70.0
                        if score_val < thresh_val:
                            previous_weak_names.add(c_name)
                elif isinstance(prev_res, dict) and "weaknesses" in prev_res:
                    previous_weak_names.update(prev_res["weaknesses"])

        for competency_name, comp_score in scoring_result.competency_scores.items():
            score = comp_score.score
            threshold = comp_score.pass_threshold
            
            # Detect weakness if score below threshold
            if score < threshold:
                severity = self._determine_severity(score)
                is_persistent = (competency_name in previous_weak_names)

                reason_parts = [f"Score {score:.1f} below pass threshold {threshold:.1f}"]
                if is_persistent:
                    reason_parts.append("PERSISTENT: Repeated weakness identified across multiple attempts")

                weakness = Weakness(
                    competency_name=competency_name,
                    score=score,
                    threshold=threshold,
                    severity=severity,
                    reason=" | ".join(reason_parts),
                    affected_aspects=_aspects_for_competency(
                        scoring_result, competency_name
                    ),
                    is_persistent=is_persistent
                )
                weaknesses.append(weakness)
        
        # Sort by severity (severe first) then by score (lowest first)
        severity_order = {"severe": 0, "moderate": 1, "mild": 2}
        weaknesses.sort(
            key=lambda w: (severity_order.get(w.severity, 3), w.score)
        )
        
        return weaknesses

    def analyze_full_profile(
        self,
        scoring_result,
        attempt_history: List[Any] = None
    ) -> Dict[str, Any]:
        """
        Generate a complete structured profile containing strong areas, weak areas,
        repeated mistakes, and affected aspects.
        """
        weaknesses = self.detect_weaknesses(scoring_result, attempt_history)
        
        strong_areas = [
            c_name for c_name, c_score in scoring_result.competency_scores.items()
            if c_score.score >= c_score.pass_threshold
        ]
        
        weak_areas = [w.competency_name for w in weaknesses]
        repeated_mistakes = [w.competency_name for w in weaknesses if w.is_persistent]

        all_affected_aspects = []
        for w in weaknesses:
            all_affected_aspects.extend(w.affected_aspects)
        # Deduplicate while preserving order
        all_affected_aspects = list(dict.fromkeys(all_affected_aspects))

        return {
            "competency_status": "competent" if scoring_result.passed else "not_competent",
            "overall_score": scoring_result.overall_score,
            "passed": scoring_result.passed,
            "strong_areas": strong_areas,
            "weak_areas": weak_areas,
            "affected_aspects": all_affected_aspects,
            "weakness_reasons": {w.competency_name: w.reason for w in weaknesses},
            "repeated_mistakes": repeated_mistakes,
            "weaknesses": [
                {
                    "competency_name": w.competency_name,
                    "score": w.score,
                    "threshold": w.threshold,
                    "severity": w.severity,
                    "reason": w.reason,
                    "affected_aspects": w.affected_aspects,
                    "is_persistent": w.is_persistent
                }
                for w in weaknesses
            ]
        }
    
    def _determine_severity(self, score: float) -> str:
        """Determine weakness severity based on score."""
        if score < SEVERE_WEAKNESS_THRESHOLD:
            return "severe"
        elif score < WEAKNESS_THRESHOLD:
            return "moderate"
        else:
            return "mild"
