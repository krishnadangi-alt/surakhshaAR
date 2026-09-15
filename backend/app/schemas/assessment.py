"""Assessment request/response schemas."""

from datetime import datetime

from pydantic import BaseModel, ConfigDict


class AssessmentCreate(BaseModel):
    worker_id: int
    module_id: int
    attempt_number: int
    score: float
    passed: bool
    weaknesses: list[str] = []


class AssessmentOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: int
    worker_id: int
    module_id: int
    attempt_number: int
    score: float
    passed: bool
    weaknesses: list[str]
    created_at: datetime


class AssessmentHistoryOut(BaseModel):
    worker_id: int
    assessments: list[AssessmentOut]