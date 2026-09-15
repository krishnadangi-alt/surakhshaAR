"""Shared API dependencies - DB session plus authentication / RBAC (Day 4)."""

from fastapi import Depends, HTTPException
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer
from sqlalchemy.orm import Session

from app.auth.security import decode_access_token
from app.database.connection import get_db
from app.models.auth_user import AuthUser

__all__ = [
    "get_db",
    "get_current_user",
    "require_admin",
    "ensure_worker_access",
]

bearer_scheme = HTTPBearer(auto_error=False)


def get_current_user(
    credentials: HTTPAuthorizationCredentials | None = Depends(bearer_scheme),
    db: Session = Depends(get_db),
) -> AuthUser:
    """Resolve the authenticated AuthUser from the Bearer token.

    Raises 401 when the token is missing, malformed, expired or unknown, and
    403-equivalent access decisions are enforced separately per endpoint.
    """
    if credentials is None or not credentials.credentials:
        raise HTTPException(
            status_code=401,
            detail="Not authenticated. Provide a valid Bearer token (see /api/v1/auth/login).",
            headers={"WWW-Authenticate": "Bearer"},
        )
    claims = decode_access_token(credentials.credentials)
    if claims is None:
        raise HTTPException(
            status_code=401,
            detail="Invalid or expired token",
            headers={"WWW-Authenticate": "Bearer"},
        )
    user = db.query(AuthUser).filter(AuthUser.username == claims.get("sub")).first()
    if user is None or not user.is_active:
        raise HTTPException(
            status_code=401,
            detail="Invalid or expired token",
            headers={"WWW-Authenticate": "Bearer"},
        )
    return user


def require_admin(user: AuthUser = Depends(get_current_user)) -> AuthUser:
    """RBAC gate: only users with the admin role may pass the dependency."""
    if user.role != "admin":
        raise HTTPException(status_code=403, detail="Admin privileges required")
    return user


def ensure_worker_access(user: AuthUser, worker_id: int) -> None:
    """RBAC ownership gate.

    Admins may read/manage any worker's data; worker accounts are restricted to
    their own ``worker_id``. Raises 403 otherwise. Called explicitly inside each
    endpoint so enforcement is server-side and never client-controlled.
    """
    if user.role != "admin":
        if user.worker_id is None or int(user.worker_id) != int(worker_id):
            raise HTTPException(
                status_code=403,
                detail="Not authorized to access this worker's data",
            )
