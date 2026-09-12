"""Sync log model."""

from datetime import datetime, timezone

from sqlalchemy import JSON, Column, DateTime, ForeignKey, Integer, String, UniqueConstraint

from app.database.connection import Base


def utcnow():
    return datetime.now(timezone.utc)


class SyncLog(Base):
    __tablename__ = "sync_logs"
    __table_args__ = (UniqueConstraint("worker_id", "batch_id", name="uq_synclog_batch"),)

    id = Column(Integer, primary_key=True, index=True)
    worker_id = Column(Integer, ForeignKey("workers.id"), nullable=False, index=True)
    device_id = Column(String, nullable=False)
    batch_id = Column(String(64), nullable=True, index=True)
    synced_at = Column(DateTime, default=utcnow, nullable=False)
    sessions_synced = Column(Integer, nullable=False, default=0)
    assessments_created = Column(Integer, nullable=False, default=0)
    payload = Column(JSON, nullable=False, default=dict)