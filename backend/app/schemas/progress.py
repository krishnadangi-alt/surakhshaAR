"""Progress request/response schemas."""

from datetime import datetime

from pydantic import BaseModel, ConfigDict, field_validator

VALID_STAGES = {
    "learn",
    "practice",
    "assess",
    "diagnose",
    "retrain",
    "reassess",
    "certify",
    "retain",
}
VALID_STATUSES = {"in_progress", "completed"}


class ProgressCreate(BaseModel):
    worker_id: int
    module_id: int
    stage: str
    status: str

    @field_validator("stage")
    @classmethod
    def validate_stage(cls, v: str) -> str:
        if v not in VALID_STAGES:
            raise ValueError(f"Invalid stage '{v}'. Must be one of {sorted(VALID_STAGES)}")
        return v

    @field_validator("status")
    @classmethod
    def validate_status(cls, v: str) -> str:
        if v not in VALID_STATUSES:
            raise ValueError(f"Invalid status '{v}'. Must be one of {sorted(VALID_STATUSES)}")
        return v


class ProgressOut(BaseModel):
    worker_id: int
    module_id: int
    stage: str
    status: str
    updated_at: datetime


class ProgressItemOut(BaseModel):
    module_id: int
    module_code: str
    module_name: str
    stage: str
    status: str
    last_updated: datetime


class ProgressListOut(BaseModel):
    worker_id: int
    progress: list[ProgressItemOut]