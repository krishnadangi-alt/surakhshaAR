"""Bridge between the FastAPI backend and the ML/vision package.

Mirrors ``competency_service``: the vision package lives in the repository-level
``ml`` package and is framework-free. This service adapts it to the backend
domain and keeps the API layer thin.
"""

from __future__ import annotations

import base64
from typing import Any, Dict, List, Optional

import app.ml_path  # noqa: F401  (adds repo-root `ml` package to sys.path)
from ml.vision import service as vision_service  # noqa: E402


def run_ppe_check(
    image_base64: str,
    required_ppe: Optional[List[str]] = None,
    mode: Optional[str] = None,
) -> Dict[str, Any]:
    """Decode the request payload and run the PPE detection pipeline.

    Returns:
        A JSON-safe dict matching ``PPECheckOut`` (via the ml/vision service).
        Never raises: invalid images degrade to ``model_error``.
    """
    image_bytes = base64.b64decode(image_base64)
    return vision_service.run_ppe_check(
        image_bytes,
        required_ppe=required_ppe,
        mode=mode,
    )


def get_vision_status() -> Dict[str, Any]:
    """Return the current vision stack configuration for the status endpoint."""
    return vision_service.vision_capabilities()