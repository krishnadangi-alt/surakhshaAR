"""Worker progress model."""

from datetime import datetime, timezone

from sqlalchemy import Column, DateTime, ForeignKey, Integer, String, UniqueConstraint

from app.database.connection import Base


def utcnow():
    return datetime.now(timezone.utc)


class WorkerProgress(Base):
    __tablename__ = "worker_progress"
    __table_args__ = (UniqueConstraint("worker_id", "module_id", name="uq_worker_module"),)

    id = Column(Integer, primary_key=True, index=True)
    worker_id = Column(Integer, ForeignKey("workers.id"), nullable=False, index=True)
    module_id = Column(Integer, ForeignKey("modules.id"), nullable=False, index=True)
    stage = Column(String, nullable=False)
    status = Column(String, nullable=False)
    updated_at = Column(DateTime, default=utcnow, onupdate=utcnow, nullable=False)