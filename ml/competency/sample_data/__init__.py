"""Sample data module."""

from .fire_samples import (
    get_sample_fire_assessment,
    get_all_fire_samples,
    FIRE_ASSESSMENT_GOOD,
    FIRE_ASSESSMENT_POOR,
    FIRE_ASSESSMENT_CRITICAL_ERROR,
)

__all__ = [
    "get_sample_fire_assessment",
    "get_all_fire_samples",
    "FIRE_ASSESSMENT_GOOD",
    "FIRE_ASSESSMENT_POOR",
    "FIRE_ASSESSMENT_CRITICAL_ERROR",
]
