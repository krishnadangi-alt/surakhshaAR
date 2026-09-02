"""Vision endpoints — PPE detection and vision-stack status.

Follows the ml/vision design rules (AI is never on the critical path):
* Invalid *payloads* (bad base64) return 422.
* Invalid *images* / missing models return a graceful ``model_error`` result so
  the Unity app can fall back to manual tap-to-select interaction.
"""

from fastapi import APIRouter

from app.schemas.vision import PPECheckOut, PPECheckRequest, VisionStatusOut
from app.services.vision_service import get_vision_status, run_ppe_check

router = APIRouter(prefix="/vision", tags=["vision"])


@router.post("/ppe-check", response_model=PPECheckOut)
def ppe_check(payload: PPECheckRequest):
    """Detect whether the required PPE is present in the submitted frame."""
    return run_ppe_check(
        payload.image_base64,
        required_ppe=payload.required_ppe,
        mode=payload.mode,
    )


@router.get("/status", response_model=VisionStatusOut)
def vision_status():
    """Report how the vision stack is configured (mock vs model backend)."""
    return {
        "status": "ok",
        **get_vision_status(),
    }