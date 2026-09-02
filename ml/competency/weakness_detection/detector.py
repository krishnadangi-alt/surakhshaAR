"""
Weakness detection engine.

Identifies performance weaknesses from competency scores and assessment results.
"""

from dataclasses import dataclass
from typing import List, Dict
from ..scoring.config import WEAKNESS_THRESHOLD, SEVERE_WEAKNESS_THRESHOLD


@dataclass
class Weakness:
    """A detected performance weakness."""
    competency_name: str
    score: float
    threshold: float
    severity: str  # "severe", "moderate", "mild"
    reason: str  # Why this is a weakness
    affected_aspects: List[str]  # Sub-competencies affected


class WeaknessDetector:
    """Detects performance weaknesses from assessment results."""
    
    def __init__(self):
        """Initialize weakness detector."""
        pass
    
    def detect_weaknesses(self, scoring_result) -> List[Weakness]:
        """
        Identify weaknesses from a scoring result.
        
        A weakness is identified when:
        - Competency score falls below threshold
        - Severity varies with how far below threshold
        
        Args:
            scoring_result: ScoringResult object from scorer
            
        Returns:
            List of Weakness objects
        """
        weaknesses = []
        
        for competency_name, comp_score in scoring_result.competency_scores.items():
            score = comp_score.score
            threshold = comp_score.pass_threshold
            
            # Detect weakness if score below any threshold
            if score < threshold:
                severity = self._determine_severity(score)
                weakness = Weakness(
                    competency_name=competency_name,
                    score=score,
                    threshold=threshold,
                    severity=severity,
                    reason=f"Score {score:.1f} below pass threshold {threshold:.1f}",
                    affected_aspects=[]  # Would be populated from competency config
                )
                weaknesses.append(weakness)
        
        # Sort by severity (severe first) then by score (lowest first)
        severity_order = {"severe": 0, "moderate": 1, "mild": 2}
        weaknesses.sort(
            key=lambda w: (severity_order.get(w.severity, 3), w.score)
        )
        
        return weaknesses
    
    def _determine_severity(self, score: float) -> str:
        """Determine weakness severity based on score."""
        if score < SEVERE_WEAKNESS_THRESHOLD:
            return "severe"
        elif score < WEAKNESS_THRESHOLD:
            return "moderate"
        else:
            return "mild"
