"""
ML Competency Module
====================

Components for scoring worker competency, detecting weaknesses, and recommending retraining.

Modules:
- scoring: Competency scoring engine
- weakness_detection: Performance weakness identification
- retraining: Targeted retraining recommendations
- assess: Public ``assess(events)`` interface

Public interface:
    from ml.competency import assess
    result = assess(events)
"""

from .assess import assess

__version__ = "0.1.0"

__all__ = ["assess"]
