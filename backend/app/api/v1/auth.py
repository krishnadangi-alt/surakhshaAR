"""Auth endpoints (DAY 4).

Only one endpoint is introduced: ``POST /api/v1/auth/login``. It verifies
worker/admin credentials and returns a short-lived bearer token which all
other v1 endpoints require.
"""

from fastapi import APIRouter, Depends, HTTPException, Request
from sqlalchemy.orm import Session

from app.api.deps import get_db
from app.auth.service import authenticate_user, issue_token
from app.config import ACCESS_TOKEN_EXPIRE_MINUTES
from app.schemas.auth import LoginRequest, TokenOut
from app.services.audit_service import write_audit

router = APIRouter(prefix="/auth", tags=["auth"])


@router.post("/login", response_model=TokenOut)
def login(payload: LoginRequest, request: Request, db: Session = Depends(get_db)):
    """Authenticate a worker or admin and return a short-lived bearer token."""
    ip = request.client.host if request.client else None
    user = authenticate_user(db, payload.username, payload.password)
    if user is None:
        write_audit(
            db,
            action="auth.login_failed",
            resource_type="auth_user",
            detail={"username": payload.username},
            ip_address=ip,
        )
        db.commit()
        raise HTTPException(status_code=401, detail="Invalid username or password")

    token_data = issue_token(user)
    write_audit(
        db,
        action="auth.login",
        resource_type="auth_user",
        resource_id=user.id,
        detail={"username": user.username},
        ip_address=ip,
    )
    db.commit()
    return TokenOut(
        access_token=token_data["access_token"],
        token_type="bearer",
        role=user.role,
        username=user.username,
        worker_id=user.worker_id,
        expires_in=ACCESS_TOKEN_EXPIRE_MINUTES * 60,
    )
