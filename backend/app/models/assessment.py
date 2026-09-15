"""Assessment model."""

from datetime import datetime, timezone

from sqlalchemy import JSON, Boolean, Column, DateTime, Float, ForeignKey, Integer

from app.database.connection import Base


def utcnow():
    return datetime.now(timezone.utc)


class Assessment(Base):
    __tablename__ = "assessments"

    id = Column(Integer, primary_key=True, index=True)
    worker_id = Column(Integer, ForeignKey("workers.id"), nullable=False, index=True)
    module_id = Column(Integer, ForeignKey("modules.id"), nullable=False, index=True)
    attempt_number = Column(Integer, nullable=False)
    score = Column(Float, nullable=False)
    passed = Column(Boolean, nullable=False)
    weaknesses = Column(JSON, nullable=False, default=list)
    created_at = Column(DateTime, default=utcnow, nullable=False)