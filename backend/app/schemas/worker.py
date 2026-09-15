"""Worker request/response schemas."""

from datetime import datetime

from pydantic import BaseModel, ConfigDict, Field, model_validator


class WorkerCreate(BaseModel):
    name: str = Field(..., min_length=2, max_length=120)
    employee_id: str = Field(..., min_length=2, max_length=64)
    role: str = Field(..., min_length=2, max_length=64)

    # Optional login provisioning (Day 4): when set, an admin registering a
    # worker may also create that worker's login account in the same call.
    username: str | None = Field(None, min_length=3, max_length=64)
    password: str | None = Field(None, min_length=6, max_length=128)

    @model_validator(mode="after")
    def _credentials_must_be_paired(self):
        if (self.username is None) != (self.password is None):
            raise ValueError("username and password must be provided together")
        return self


class WorkerOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: int
    name: str
    employee_id: str
    role: str
    created_at: datetime
