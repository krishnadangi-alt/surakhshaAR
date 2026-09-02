"""High-level vision service: the boundary the backend calls.

Keeps the detector usage simple and uniform: ``run_ppe_check`` accepts raw image
bytes (already validated at the API layer) and returns a plain dict that the
FastAPI response schema serialises directly. The service never raises.
"""

from typing import Dict, List, Optional

from ml.vision.inference.ppe_detector import PPEDetector, PPEDetectionResult


def run_ppe_check(
    image_bytes: bytes,
    required_ppe: Optional[List[str]] = None,
    mode: Optional[str] = None,
    model_path: Optional[str] = None,
) -> Dict[str, object]:
    """Detect required PPE in ``image_bytes``.

    Args:
        image_bytes: Raw encoded image bytes.
        required_ppe: PPE items to look for (defaults to helmet + safety_vest).
        mode: Detector mode — ``auto`` (default), ``mock`` or ``model``.
        model_path: Optional real-model checkpoint path.

    Returns:
        A JSON-safe result dict (see PPEDetectionResult.to_dict) enriched with
        the ``required_ppe`` that was requested. Never raises.
    """
    detector = PPEDetector(mode=mode or "auto", model_path=model_path)

    result: PPEDetectionResult = detector.analyze(image_bytes, required_ppe)
    payload = result.to_dict()
    payload["required_ppe"] = list(required_ppe) if required_ppe else ["helmet", "safety_vest"]
    return payload


def vision_capabilities() -> Dict[str, object]:
    """Describe the configured vision stack (for the backend status endpoint)."""
    detector = PPEDetector(mode="auto")
    caps = detector.capabilities()
    caps["module"] = "ml.vision"
    caps["version"] = "0.1.0"
    return caps