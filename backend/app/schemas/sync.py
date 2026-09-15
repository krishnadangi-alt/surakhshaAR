"""Sync request/response schemas.

Offline sessions that include behavioural ``events`` (assessments recorded
offline) are scored server-side by the ML competency engine on sync and stored
as real Assessment records. Sessions without events are logged as-is
(backward compatible with clients that report their own scores - those scores
are never treated as authoritative).
"""

from datetime import datetime

from pydantic import BaseModel, ConfigDict, Field

from app.schemas.assessment import AssessmentEvent


class SyncSession(BaseModel):
    model_config = ConfigDict(extra="allow")

    type: str = Field(..., min_length=1, max_length=32)
    module_id: int = Field(..., ge=1)
    score: float | None = Field(None, ge=0, le=100)
    passed: bool | None = None
    weaknesses: list[str] = Field(default_factory=list, max_length=50)
    occurred_at: datetime
    scenario_type: str | None = Field(None, max_length=32)
    attempt_number: int | None = Field(None, ge=1)
    client_session_id: str | None = Field(
        None,
        max_length=64,
        description=(
            "Optional per-session idempotency key; an assessment already scored for "
            "the same worker+module+key is skipped on re-sync."
        ),
    )
    events: list[AssessmentEvent] = Field(default_factory=list, max_length=2000)


class SyncCreate(BaseModel):
    worker_id: int = Field(..., ge=1)
    device_id: str = Field(..., min_length=1, max_length=128)
    batch_id: str | None = Field(
        None,
        max_length=64,
        description=(
            "Optional client-generated batch idempotency key; re-sending the same "
            "batch_id for a worker returns the original sync result (200) and "
            "creates no duplicate log or assessment rows."
        ),
    )
    pending_sessions: int | None = Field(
        None,
        ge=0,
        le=1_000_000,
        description=(
            "Optional client-reported count of sessions still queued offline; "
            "GET /sync/status/{worker_id} echoes the most recent value."
        ),
    )
    sessions: list[SyncSession] = Field(default_factory=list, max_length=1000)


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
