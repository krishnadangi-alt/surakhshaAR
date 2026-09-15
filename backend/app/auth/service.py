"""Authentication service: account lookups, verification and token minting."""

from datetime import datetime, timezone

from sqlalchemy.orm import Session

from app.auth.security import create_access_token, hash_password, verify_password
from app.models.auth_user import AuthUser


def utcnow():
    return datetime.now(timezone.utc)


def get_user_by_username(db: Session, username: str) -> AuthUser | None:
    return db.query(AuthUser).filter(AuthUser.username == username).first()


def create_auth_user(
    db: Session,
    *,
    username: str,
    password: str,
    role: str = "worker",
    worker_id: int | None = None,
) -> AuthUser:
    """Create an active login account; caller commits."""
    user = AuthUser(
        username=username,
        password_hash=hash_password(password),
        role=role,
        worker_id=worker_id,
    )
    db.add(user)
    db.flush()
    return user


def authenticate_user(db: Session, username: str, password: str) -> AuthUser | None:
    """Return the user when credentials are valid, else None."""
    user = get_user_by_username(db, username)
    if user is None or not user.is_active:
        return None
    if not verify_password(password, user.password_hash):
        return None
    return user


def issue_token(user: AuthUser) -> dict:
    """Mint an access token for the user and stamp last_login_at (caller commits)."""
    user.last_login_at = utcnow()
    token = create_access_token(
        subject=user.username,
        role=user.role,
        worker_id=user.worker_id,
    )
    return {
        "access_token": token,
        "role": user.role,
        "worker_id": user.worker_id,
    }
