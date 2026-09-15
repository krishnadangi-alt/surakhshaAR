"""Event request and response schemas."""

from datetime import datetime, timezone
from typing import Any

from pydantic import BaseModel, ConfigDict, Field, field_validator

VALID_EVENT_SEVERITIES = {"info", "minor", "major", "critical"}


def utcnow():
    return datetime.now(timezone.utc)


class EventItemCreate(BaseModel):
    model_config = ConfigDict(extra="allow")

    event_type: str = Field(
        ...,
        min_length=1,
        max_length=64,
        description="Event type e.g. hazard_identified, pin_removed, spray_started, wrong_action, critical_action",
    )
    session_id: str | None = Field(None, max_length=64, description="Unique session ID; defaults from batch if omitted")
    worker_id: int | None = Field(None, ge=1, description="Worker ID")
    module_id: int | None = Field(None, ge=1, description="Training module ID")
    scenario_type: str | None = Field("fire", max_length=32, description="Scenario type: fire, gas, machinery")
    severity: str | None = Field("info", description="Severity: info, minor, major, critical")
    payload: dict[str, Any] = Field(default_factory=dict, description="Arbitrary telemetry data")
    device_id: str | None = Field(None, max_length=128)
    timestamp: datetime | None = Field(default_factory=utcnow)

    @field_validator("severity")
    @classmethod
    def _validate_severity(cls, v: str | None) -> str | None:
        if v is not None and v not in VALID_EVENT_SEVERITIES:
            raise ValueError(
                f"Invalid severity '{v}'. Must be one of {sorted(VALID_EVENT_SEVERITIES)}"
            )
        return v


class EventBatchCreate(BaseModel):
    worker_id: int | None = Field(None, ge=1)
    device_id: str | None = Field(None, max_length=128)
    session_id: str | None = Field(None, max_length=64)
    events: list[EventItemCreate] = Field(..., min_length=1, max_length=2000)


class EventOut(BaseModel):
    id: int
    session_id: str
    worker_id: int | None
    module_id: int | None
    scenario_type: str
    event_type: str
    severity: str | None
    payload: dict[str, Any]
    device_id: str | None
    timestamp: datetime
    created_at: datetime


class EventBatchOut(BaseModel):
    received: int
    stored: int
    session_id: str | None
    status: str = "success"


class EventStatsOut(BaseModel):
    total_events: int
    critical_events_count: int
    hazards_identified_count: int
    events_by_type: dict[str, int]
    events_by_scenario: dict[str, int]
