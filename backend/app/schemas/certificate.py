from datetime import datetime
from typing import Any, Dict, List, Optional

from pydantic import BaseModel, ConfigDict, Field, model_validator


class CertificateCreate(BaseModel):
    worker_id: int = Field(..., ge=1)
    module_id: int = Field(..., ge=1)


class CertificateOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: int
    certificate_number: str
    worker_id: int
    module_id: int
    attempt_id: Optional[str] = None
    worker_name_snapshot: Optional[str] = None
    employee_id_snapshot: Optional[str] = None
    module_snapshot: Optional[str] = None
    score_snapshot: Optional[float] = None
    competency_snapshot: Optional[str] = None
    issued_at: Optional[datetime] = None
    valid_until: Optional[datetime] = None
    status: str
    has_pdf: bool = False
    has_image: bool = False
    image_path: Optional[str] = None
    public_image_url: Optional[str] = None
    verification_token: Optional[str] = None

    @model_validator(mode="before")
    @classmethod
    def _compute_has_artifacts(cls, data: Any) -> Any:
        import os
        if hasattr(data, "pdf_path"):
            pdf_path = getattr(data, "pdf_path", None)
            setattr(data, "has_pdf", bool(pdf_path and os.path.exists(pdf_path)))
        elif isinstance(data, dict):
            pdf_path = data.get("pdf_path")
            data["has_pdf"] = bool(pdf_path and os.path.exists(pdf_path))

        if hasattr(data, "image_path"):
            image_path = getattr(data, "image_path", None)
            setattr(data, "has_image", bool(image_path and os.path.exists(image_path)))
        elif isinstance(data, dict):
            image_path = data.get("image_path")
            data["has_image"] = bool(image_path and os.path.exists(image_path))
        return data



class CertificateListOut(BaseModel):
    worker_id: int
    certificates: List[CertificateOut]


class CertificateVerifyOut(BaseModel):
    certificate_number: str
    valid: bool
    worker_name: str
    employee_id: Optional[str] = None
    role: Optional[str] = None
    module_name: str
    score: Optional[float] = None
    competency_status: Optional[str] = None
    issued_at: Optional[datetime] = None
    valid_until: Optional[datetime] = None
    status: str
    verification_hash: Optional[str] = None
    public_image_url: Optional[str] = None
    has_image: bool = False
    verify_url: Optional[str] = None


class AdminReviewQueueItem(BaseModel):
    certificate_id: int
    certificate_number: str
    attempt_id: Optional[str] = None
    assessment_id: Optional[int] = None
    worker_id: int
    worker_name: str
    employee_id: str
    module_id: int
    module_name: str
    score: float
    competency_status: str
    critical_errors: int
    duration_seconds: float
    assessment_date: Optional[datetime] = None
    status: str


class AdminReviewQueueOut(BaseModel):
    queue: List[AdminReviewQueueItem]
    total_pending: int


class AdminDecisionRequest(BaseModel):
    reason: Optional[str] = None


class AdminAttemptReviewOut(BaseModel):
    attempt_id: str
    assessment_id: Optional[int] = None
    certificate_id: Optional[int] = None
    certificate_number: Optional[str] = None
    certificate_status: str
    certificate_eligible: bool

    # Worker info
    worker_id: int
    worker_name: str
    employee_id: str
    role: str
    department: str
    site: str

    # Scenario & timing info
    module_id: int
    module_name: str
    scenario_type: str
    assessment_date: Optional[datetime] = None
    duration_seconds: float
    timed_out: bool

    # Scores
    overall_score: float
    competency_status: str
    procedural_score: float
    safety_score: float
    handling_score: float
    response_score: float
    fire_control_score: float
    knowledge_score: float

    # Mistakes & errors
    wrong_actions: int
    unsafe_actions: int
    critical_errors: int
    critical_error_details: List[str]
    sequence_valid: bool
    sequence_errors: List[str]

    # Spray & Aim behavior
    hazard_response_time: Optional[float] = None
    alarm_response_time: Optional[float] = None
    extinguisher_selection_time: Optional[float] = None
    pin_removal_time: Optional[float] = None
    spray_contact_duration: float
    spray_interruptions: int
    spray_resets: int
    fire_extinguished: bool

    # Timeline & Evidence
    timeline: List[Dict[str, Any]]
    weaknesses: List[Dict[str, Any]]
    recommendations: List[Dict[str, Any]]