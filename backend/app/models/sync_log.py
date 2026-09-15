"""Sync log model."""

from datetime import datetime, timezone

from sqlalchemy import JSON, Column, DateTime, ForeignKey, Integer, String

from app.database.connection import Base


def utcnow():
    return datetime.now(timezone.utc)


class SyncLog(Base):
    __tablename__ = "sync_logs"

    id = Column(Integer, primary_key=True, index=True)
    worker_id = Column(Integer, ForeignKey("workers.id"), nullable=False, index=True)
    device_id = Column(String, nullable=False)
    synced_at = Column(DateTime, default=utcnow, nullable=False)
    payload = Column(JSON, nullable=False, default=dict)