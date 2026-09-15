"""Sync request/response schemas."""

from datetime import datetime

from pydantic import BaseModel


class SyncSession(BaseModel):
    type: str
    module_id: int
    score: float
    passed: bool
    weaknesses: list[str] = []
    occurred_at: datetime


class SyncCreate(BaseModel):
    worker_id: int
    device_id: str
    sessions: list[SyncSession]


class SyncOut(BaseModel):
    sync_id: int
    worker_id: int
    synced_at: datetime
    sessions_synced: int


class SyncStatusOut(BaseModel):
    worker_id: int
    last_synced_at: datetime | None
    pending_sessions: int