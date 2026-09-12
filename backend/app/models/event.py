"""Event model for raw behavioural telemetry ingested from SurakshaAR clients."""

from datetime import datetime, timezone

from sqlalchemy import JSON, Column, DateTime, ForeignKey, Integer, String

from app.database.connection import Base


def utcnow():
    return datetime.now(timezone.utc)


class EventModel(Base):
    __tablename__ = "events"

    id = Column(Integer, primary_key=True, index=True)
    worker_id = Column(Integer, ForeignKey("workers.id", ondelete="SET NULL"), nullable=True, index=True)
    session_id = Column(String(64), nullable=False, index=True)
    module_id = Column(Integer, nullable=True, index=True)
    scenario_type = Column(String(32), nullable=False, default="fire", index=True)
    event_type = Column(String(64), nullable=False, index=True)
    severity = Column(String(16), nullable=True, default="info")
    payload = Column(JSON, nullable=False, default=dict)
    device_id = Column(String(64), nullable=True)
    timestamp = Column(DateTime, default=utcnow, nullable=False, index=True)
    created_at = Column(DateTime, default=utcnow, nullable=False)
