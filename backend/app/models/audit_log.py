"""AuditLog model - server-side audit trail for important actions (Day 4).

Records authentication, assessment submissions, certificate issuance, admin
actions and sync operations. Written in the same transaction as the action it
describes, so every committed action has an audit row.
"""

from datetime import datetime, timezone

from sqlalchemy import JSON, Column, DateTime, Integer, String

from app.database.connection import Base


def utcnow():
    return datetime.now(timezone.utc)


class AuditLog(Base):
    __tablename__ = "audit_logs"

    id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, nullable=True, index=True)
    username = Column(String(64), nullable=True)
    role = Column(String(16), nullable=True)
    action = Column(String(64), nullable=False, index=True)
    resource_type = Column(String(64), nullable=True)
    resource_id = Column(String(64), nullable=True)
    detail = Column(JSON, nullable=False, default=dict)
    ip_address = Column(String(64), nullable=True)
    created_at = Column(DateTime, default=utcnow, nullable=False)
