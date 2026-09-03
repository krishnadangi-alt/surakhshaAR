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
    client_session_id: str | None = Field(
        None,
        max_length=64,
        description=(
            "Optional per-session idempotency key; an assessment already scored for "
            "the same worker+module+key is skipped on re-sync."
        ),
    )
    events: list[AssessmentEvent] = Field(default_factory=list)


class SyncCreate(BaseModel):
    worker_id: int
    device_id: str
    batch_id: str | None = Field(
        None,
        max_length=64,
        description=(
            "Optional client-generated batch idempotency key; re-sending the same "
            "batch_id for a worker returns the original sync result (200) and "
            "creates no duplicate log or assessment rows."
        ),
    )
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