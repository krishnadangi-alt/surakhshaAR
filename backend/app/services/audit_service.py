"""Audit helper - records important backend actions in the audit_logs table.

The entry is added to the *current* transaction and committed together with
the action it describes (callers call ``db.commit()`` as usual).
"""

from sqlalchemy.orm import Session

from app.models.audit_log import AuditLog
from app.models.auth_user import AuthUser


def write_audit(
    db: Session,
    *,
    action: str,
    user: AuthUser | None = None,
    resource_type: str | None = None,
    resource_id: str | int | None = None,
    detail: dict | None = None,
    ip_address: str | None = None,
) -> AuditLog:
    """Append an audit row in the current transaction (no commit here)."""
    entry = AuditLog(
        user_id=user.id if user else None,
        username=user.username if user else None,
        role=user.role if user else None,
        action=action,
        resource_type=resource_type,
        resource_id=str(resource_id) if resource_id is not None else None,
        detail=detail or {},
        ip_address=ip_address,
    )
    db.add(entry)
    return entry
