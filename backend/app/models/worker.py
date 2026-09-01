"""Worker model."""

from datetime import datetime, timezone

from sqlalchemy import Column, DateTime, Integer, String

from app.database.connection import Base


def utcnow():
    return datetime.now(timezone.utc)


class Worker(Base):
    __tablename__ = "workers"

    id = Column(Integer, primary_key=True, index=True)
    name = Column(String, nullable=False)
    employee_id = Column(String, unique=True, nullable=False, index=True)
    role = Column(String, nullable=False)
    created_at = Column(DateTime, default=utcnow, nullable=False)