"""Event request and response schemas."""

from datetime import datetime, timezone
from typing import Any

from pydantic import BaseModel, ConfigDict, Field


def utcnow():
    return datetime.now(timezone.utc)


class EventItemCreate(BaseModel):
    model_config = ConfigDict(extra="allow")

    event_type: str = Field(
        ...,
        description="Event type e.g. hazard_identified, pin_removed, spray_started, wrong_action, critical_action",
    )
    session_id: str | None = Field(None, description="Unique session ID; defaults from batch if omitted")
    worker_id: int | None = Field(None, description="Worker ID")
    module_id: int | None = Field(None, description="Training module ID")
    scenario_type: str | None = Field("fire", description="Scenario type: fire, gas, machinery")
    severity: str | None = Field("info", description="Severity: info, minor, major, critical")
    payload: dict[str, Any] = Field(default_factory=dict, description="Arbitrary telemetry data")
    device_id: str | None = None
    timestamp: datetime | None = Field(default_factory=utcnow)


class EventBatchCreate(BaseModel):
    worker_id: int | None = None
    device_id: str | None = None
    session_id: str | None = None
    events: list[EventItemCreate] = Field(..., min_length=1)


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
