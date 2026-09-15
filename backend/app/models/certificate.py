"""Certificate model."""

from datetime import datetime, timedelta, timezone

from sqlalchemy import Column, DateTime, ForeignKey, Integer, String

from app.database.connection import Base


def utcnow():
    return datetime.now(timezone.utc)


def default_valid_until():
    return utcnow() + timedelta(days=365)


class Certificate(Base):
    __tablename__ = "certificates"

    id = Column(Integer, primary_key=True, index=True)
    certificate_number = Column(String, unique=True, nullable=False, index=True)
    worker_id = Column(Integer, ForeignKey("workers.id"), nullable=False, index=True)
    module_id = Column(Integer, ForeignKey("modules.id"), nullable=False, index=True)
    issued_at = Column(DateTime, default=utcnow, nullable=False)
    valid_until = Column(DateTime, default=default_valid_until, nullable=False)
    status = Column(String, nullable=False, default="active")