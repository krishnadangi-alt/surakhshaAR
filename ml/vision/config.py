"""Configuration for the ML/Vision module.

Prototype values only. Required-PPE lists, confidence thresholds and model paths
must be validated with safety professionals before production deployment.
"""

from typing import List

# ---------------------------------------------------------------------------
# PPE detection
# ---------------------------------------------------------------------------

# PPE the detector looks for. The MVP treats helmet and safety vest as the
# primary (must-have) items; other items are advisory.
DEFAULT_REQUIRED_PPE: List[str] = ["helmet", "safety_vest"]

# For the gross `ppe_ok` decision: must the *primary* items all be detected?
PRIMARY_PPE = {"helmet", "safety_vest"}

# Confidence below this is reported as low_confidence instead of a hard miss.
LOW_CONFIDENCE_THRESHOLD = 0.5

# Detection statuses returned to callers (mirrors the README failure surface).
STATUS_OK = "ok"
STATUS_LOW_CONFIDENCE = "low_confidence"
STATUS_MODEL_ERROR = "model_error"
STATUS_DISABLED = "disabled"

# -- Mock fallback engine ---------------------------------------------------
#
# When no model checkpoint is configured, the detector falls back to this
# deterministic result so the integration can be demonstrated end-to-end
# (per the README fallback strategy). The result can be overridden per call
# for tests, demo scripts and the "AI off" demo mode.

MOCK_DEFAULT_RESULT = {
    "ppe_ok": True,
    "detections": [
        {"item": "helmet", "confidence": 0.98},
        {"item": "safety_vest", "confidence": 0.97},
    ],
    "missing_items": [],
    "confidence": 0.975,
    "message": "Mock PPE detector: helmet and safety vest detected (no model configured).",
}

MOCK_FAIL_RESULT = {
    "ppe_ok": False,
    "detections": [
        {"item": "helmet", "confidence": 0.98},
    ],
    "missing_items": ["safety_vest"],
    "confidence": 0.98,
    "message": "Mock PPE detector: safety_vest missing (no model configured).",
}

# -- Model backend ----------------------------------------------------------
#
# Set to an exported ONNX/TFLite path to route detection through a real model.
# The detector interface is defined in ml/vision/inference/ppe_detector.py.
MODEL_PATH = ""

# Maximum accepted payload for a single detection request (bytes).
MAX_IMAGE_BYTES = 10 * 1024 * 1024  # 10 MB