"""Worker request/response schemas."""

from datetime import datetime

from pydantic import BaseModel, ConfigDict


class WorkerCreate(BaseModel):
    name: str
    employee_id: str
    role: str


class WorkerOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: int
    name: str
    employee_id: str
    role: str
    created_at: datetime