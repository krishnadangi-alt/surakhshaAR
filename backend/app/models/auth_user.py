"""AuthUser model - backend login accounts for RBAC (worker / admin roles).

Day 4: authorization is derived from these server-side accounts, never from
any role/worker_id sent by the client. A worker account links to exactly one
``workers`` row via ``worker_id``; admin accounts keep ``worker_id`` NULL.
"""

from datetime import datetime, timezone

from sqlalchemy import Boolean, Column, DateTime, ForeignKey, Integer, String

from app.database.connection import Base


def utcnow():
    return datetime.now(timezone.utc)


class AuthUser(Base):
    __tablename__ = "auth_users"

    id = Column(Integer, primary_key=True, index=True)
    username = Column(String(64), unique=True, nullable=False, index=True)
    password_hash = Column(String(256), nullable=False)
    role = Column(String(16), nullable=False, default="worker")  # worker | admin
    worker_id = Column(Integer, ForeignKey("workers.id"), nullable=True, index=True)
    is_active = Column(Boolean, nullable=False, default=True)
    created_at = Column(DateTime, default=utcnow, nullable=False)
    last_login_at = Column(DateTime, nullable=True)
