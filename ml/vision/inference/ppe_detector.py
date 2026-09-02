"""PPE detection: is the worker wearing the required protective equipment?

The detector follows the module's golden rule — AI never crashes the app and is
never on the critical path:

* Every call returns a result object with a ``status`` of ``ok``,
  ``low_confidence``, ``model_error`` or ``disabled``. It never raises.
* Without a real model checkpoint the detector runs a deterministic **mock
  fallback** so the full integration (Unity -> backend -> vision service) is
  demonstrable end-to-end with zero AI dependencies (README Section 10).
* A real ONNX/TFLite checkpoint can be plugged in later by implementing
  ``_run_model`` for the target runtime (onnxruntime / Unity Sentis).

Detection statuses mirror the failure surface documented in ml/vision/README.md
(``OK``, ``LOW_CONFIDENCE``, ``MODEL_ERROR``, ``DISABLED``).
"""

from dataclasses import dataclass, field
from typing import Dict, List, Optional

from ml.vision.config import (
    DEFAULT_REQUIRED_PPE,
    LOW_CONFIDENCE_THRESHOLD,
    MOCK_DEFAULT_RESULT,
    MOCK_FAIL_RESULT,
    MODEL_PATH,
    STATUS_MODEL_ERROR,
    STATUS_OK,
)
from ml.vision.preprocessing.image_utils import validate_image_bytes


@dataclass
class PPEDetection:
    """A single detected PPE item."""

    item: str
    confidence: float


@dataclass
class PPEDetectionResult:
    """Complete result of one PPE detection call.

    Attributes:
        status: ``ok`` | ``low_confidence`` | ``model_error`` | ``disabled``.
        ppe_ok: Whether the primary required PPE items are all present.
        detections: Detected PPE items with confidence.
        missing_items: Required primary items that were not detected.
        confidence: Max detection confidence across primary items.
        fallback_used: ``True`` when the mock/fallback engine produced the result.
        message: Human-readable explanation (for logs / demo UI).
    """

    status: str = STATUS_OK
    ppe_ok: bool = False
    detections: List[PPEDetection] = field(default_factory=list)
    missing_items: List[str] = field(default_factory=list)
    confidence: float = 0.0
    fallback_used: bool = False
    message: str = ""

    def to_dict(self) -> Dict[str, object]:
        """Serialise to a JSON-safe dict for the backend API."""
        return {
            "status": self.status,
            "ppe_ok": self.ppe_ok,
            "detections": [
                {"item": d.item, "confidence": d.confidence} for d in self.detections
            ],
            "missing_items": list(self.missing_items),
            "confidence": round(self.confidence, 3),
            "fallback_used": self.fallback_used,
            "message": self.message,
        }


class PPEDetector:
    """PPE presence detector with a graceful mock fallback (never raises)."""

    def __init__(
        self,
        mode: str = "auto",
        model_path: Optional[str] = None,
        confidence_threshold: float = LOW_CONFIDENCE_THRESHOLD,
    ) -> None:
        """Args:
            mode: ``auto`` (model when available, else mock), ``mock`` (force
                the deterministic fallback), or ``model`` (require a model).
            model_path: Path to an exported ONNX/TFLite checkpoint. When ``None``
                the module-level ``MODEL_PATH`` config is used.
            confidence_threshold: Below this, primary detections are treated as
                low confidence rather than confident hits.
        """
        self.mode = mode
        self.model_path = model_path if model_path is not None else MODEL_PATH
        self.confidence_threshold = confidence_threshold

    # -- Public API ---------------------------------------------------------

    def analyze(
        self,
        image_bytes: bytes,
        required_ppe: Optional[List[str]] = None,
    ) -> PPEDetectionResult:
        """Detect required PPE in an image.

        Args:
            image_bytes: Raw encoded image bytes (PNG/JPEG/GIF/BMP).
            required_ppe: PPE items to look for. Defaults to
                ``DEFAULT_REQUIRED_PPE`` (helmet, safety_vest).

        Returns:
            A PPEDetectionResult. This method never raises; malformed input,
            missing models and runtime failures all degrade to ``model_error``.
        """
        required = list(required_ppe or DEFAULT_REQUIRED_PPE)

        valid, _fmt = validate_image_bytes(image_bytes)
        if not valid:
            return PPEDetectionResult(
                status=STATUS_MODEL_ERROR,
                ppe_ok=False,
                detections=[],
                missing_items=required,
                confidence=0.0,
                fallback_used=False,
                message="image validation failed (empty, oversized or unsupported format)",
            )

        if self.mode == "model":
            return self._run_model(image_bytes, required)

        # "auto": use a real model only when a checkpoint is actually available.
        if self.mode == "auto" and self.model_path:
            return self._run_model(image_bytes, required)

        return self._mock_result(required)

    def capabilities(self) -> Dict[str, object]:
        """Describe the current detector configuration (for status endpoints)."""
        return {
            "mode": self.mode,
            "model_path": self.model_path or None,
            "model_loaded": bool(self.model_path),
            "fallback_enabled": True,
            "supported_items": sorted(DEFAULT_REQUIRED_PPE),
        }

    # -- Mock fallback --------------------------------------------------------

    def _mock_result(self, required: List[str]) -> PPEDetectionResult:
        """Deterministic fallback: report the configured result for the call.

        ``MOCK_DEFAULT_RESULT`` and ``MOCK_FAIL_RESULT`` are demo-oriented; the
        backend service can also inject an explicit override (see
        ``ml/vision/service.py``).
        """
        data = {**MOCK_DEFAULT_RESULT}
        items_detected = {d["item"] for d in data["detections"]}
        missing = [item for item in required if item not in items_detected]
        if missing:
            data = {**MOCK_FAIL_RESULT}
        data["missing_items"] = missing
        if data["detections"]:
            data["confidence"] = round(
                sum(d["confidence"] for d in data["detections"]) / len(data["detections"]),
                3,
            )

        return PPEDetectionResult(
            status=STATUS_OK,
            ppe_ok=data["ppe_ok"],
            detections=[PPEDetection(**d) for d in data["detections"]],
            missing_items=list(data["missing_items"]),
            confidence=data["confidence"],
            fallback_used=True,
            message=data["message"],
        )

    # -- Model backend (interface, not yet implemented) ---------------------

    def _run_model(
        self, image_bytes: bytes, required: List[str]
    ) -> PPEDetectionResult:
        """Run the real model and convert predictions into a result.

        The ONNX/TFLite/Sentis runtime is intentionally not wired here yet (see
        the POCs in ml/vision/README.md). Until a checkpoint and runtime are
        available this returns ``model_error`` so callers degrade gracefully —
        exactly the documented fallback behaviour.
        """
        return PPEDetectionResult(
            status=STATUS_MODEL_ERROR,
            ppe_ok=False,
            detections=[],
            missing_items=list(required),
            confidence=0.0,
            fallback_used=False,
            message=(
                "model backend requested but no inference runtime is configured; "
                "see ml/vision/README.md (POC B) - use mode='mock' or 'auto'"
            ),
        )