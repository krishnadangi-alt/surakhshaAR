"""Vision request/response schemas.

The vision endpoint follows the ml/vision design rules: detection never blocks
the core app and every call reports a status (ok / low_confidence /
model_error / disabled). Invalid *payloads* (bad base64) are 422 client errors;
invalid *images* are graceful ``model_error`` responses so the app can fall back
to manual (tap) interaction.
"""

from pydantic import BaseModel, Field, field_validator

import app.ml_path  # noqa: F401  (adds repo-root `ml` package to sys.path)
from ml.vision.config import DEFAULT_REQUIRED_PPE


class PPECheckRequest(BaseModel):
    """A single PPE detection request (base64-encoded camera frame)."""

    image_base64: str = Field(..., description="Base64-encoded image (PNG/JPEG).")
    required_ppe: list[str] = Field(
        default_factory=lambda: list(DEFAULT_REQUIRED_PPE),
        description="PPE items to look for (default: helmet, safety_vest).",
    )
    mode: str | None = Field(
        None,
        description="Detector override: 'auto' (default), 'mock', or 'model'.",
    )

    @field_validator("image_base64")
    @classmethod
    def _valid_base64(cls, v: str) -> str:
        import base64

        if not v.strip():
            raise ValueError("image_base64 must not be empty")
        try:
            base64.b64decode(v, validate=True)
        except Exception as exc:  # noqa: BLE001 - converted to a 422 detail
            raise ValueError("image_base64 is not valid base64") from exc
        return v

    @field_validator("required_ppe")
    @classmethod
    def _non_empty(cls, v: list[str]) -> list[str]:
        if not v:
            raise ValueError("required_ppe must not be empty")
        return v


class PPEDetectionOut(BaseModel):
    """A single detected PPE item with its confidence."""

    item: str
    confidence: float


class PPECheckOut(BaseModel):
    """Result of a PPE detection call."""

    status: str
    ppe_ok: bool
    detections: list[PPEDetectionOut]
    missing_items: list[str]
    confidence: float
    fallback_used: bool
    message: str
    required_ppe: list[str]


class VisionStatusOut(BaseModel):
    """How the vision stack is currently configured."""

    status: str
    module: str
    version: str
    mode: str
    model_path: str | None
    model_loaded: bool
    fallback_enabled: bool
    supported_items: list[str]