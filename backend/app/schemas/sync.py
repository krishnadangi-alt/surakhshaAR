"""Sync request/response schemas.

Offline sessions that include behavioural ``events`` (assessments recorded
offline) are scored server-side by the ML competency engine on sync and stored
as real Assessment records. Sessions without events are logged as-is
(backward compatible with clients that report their own scores).
"""

from datetime import datetime

from pydantic import BaseModel, ConfigDict, Field

from app.schemas.assessment import AssessmentEvent


class SyncSession(BaseModel):
    model_config = ConfigDict(extra="allow")

    type: str
    module_id: int
    score: float | None = None
    passed: bool | None = None
    weaknesses: list[str] = []
    occurred_at: datetime
    scenario_type: str | None = None
    attempt_number: int | None = None
    events: list[AssessmentEvent] = Field(default_factory=list)


class SyncCreate(BaseModel):
    worker_id: int
    device_id: str
    sessions: list[SyncSession]


class SyncOut(BaseModel):
    sync_id: int
    worker_id: int
    synced_at: datetime
    sessions_synced: int
    assessments_created: int = 0


class SyncStatusOut(BaseModel):
    worker_id: int
    last_synced_at: datetime | None
    pending_sessions: int