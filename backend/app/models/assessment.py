"""Assessment model.

Assessments are behaviour-based: the raw VR session events are stored alongside
the scores computed server-side by the ML competency engine.
"""

from datetime import datetime, timezone

from sqlalchemy import JSON, Boolean, Column, DateTime, Float, ForeignKey, Integer, String, UniqueConstraint

from app.database.connection import Base


def utcnow():
    return datetime.now(timezone.utc)


class Assessment(Base):
    __tablename__ = "assessments"
    __table_args__ = (UniqueConstraint("worker_id", "module_id", "client_session_id", name="uq_assessment_dedup"),)

    id = Column(Integer, primary_key=True, index=True)
    worker_id = Column(Integer, ForeignKey("workers.id"), nullable=False, index=True)
    module_id = Column(Integer, ForeignKey("modules.id"), nullable=False, index=True)
    client_session_id = Column(String(64), nullable=True, index=True)
    attempt_number = Column(Integer, nullable=False)
    scenario_type = Column(String(16), nullable=False)
    score = Column(Float, nullable=False)
    passed = Column(Boolean, nullable=False)
    pass_reason = Column(String(512), nullable=False, default="")
    weaknesses = Column(JSON, nullable=False, default=list)
    competency_scores = Column(JSON, nullable=False, default=dict)
    critical_errors = Column(JSON, nullable=False, default=list)
    events = Column(JSON, nullable=False, default=list)
    created_at = Column(DateTime, default=utcnow, nullable=False)