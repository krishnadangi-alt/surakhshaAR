"""Workers endpoints."""

from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session

from app.api.deps import ensure_worker_access, get_current_user, get_db, require_admin
from app.api.v1.progress import build_worker_progress
from app.auth.service import create_auth_user
from app.models.auth_user import AuthUser
from app.models.worker import Worker
from app.schemas.progress import WorkerProgressListOut
from app.schemas.worker import WorkerCreate, WorkerOut
from app.services.audit_service import write_audit

router = APIRouter(prefix="/workers", tags=["workers"])


@router.post("", response_model=WorkerOut, status_code=201)
def create_worker(
    payload: WorkerCreate,
    admin: AuthUser = Depends(require_admin),
    db: Session = Depends(get_db),
):
    """Register a worker (admin only).

    When optional ``username``/``password`` are supplied, a worker login
    account is provisioned in the same transaction (Day 4).
    """
    existing = db.query(Worker).filter(Worker.employee_id == payload.employee_id).first()
    if existing:
        raise HTTPException(
            status_code=409,
            detail=f"Worker with employee_id {payload.employee_id} already exists",
        )

    if payload.username is not None:
        username_taken = (
            db.query(AuthUser).filter(AuthUser.username == payload.username).first()
        )
        if username_taken:
            raise HTTPException(
                status_code=409,
                detail=f"Username {payload.username} already exists",
            )

    worker = Worker(
        name=payload.name,
        employee_id=payload.employee_id,
        role=payload.role,
    )
    db.add(worker)
    db.flush()

    if payload.username is not None:
        create_auth_user(
            db,
            username=payload.username,
            password=payload.password,
            role="worker",
            worker_id=worker.id,
        )

    write_audit(
        db,
        action="worker.create",
        user=admin,
        resource_type="worker",
        resource_id=worker.id,
        detail={"employee_id": payload.employee_id, "name": payload.name},
    )
    db.commit()
    db.refresh(worker)
    return worker


@router.get("/{worker_id}", response_model=WorkerOut)
def get_worker(
    worker_id: int,
    user: AuthUser = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    ensure_worker_access(user, worker_id)
    return _get_worker_or_404(db, worker_id)


def _get_worker_or_404(db: Session, worker_id: int) -> Worker:
    worker = db.query(Worker).filter(Worker.id == worker_id).first()
    if not worker:
        raise HTTPException(status_code=404, detail="Worker not found")
    return worker


@router.get("/{worker_id}/progress", response_model=WorkerProgressListOut)
def get_worker_progress(
    worker_id: int,
    user: AuthUser = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """Return the worker's per-module progress merged with stored assessment stats."""
    ensure_worker_access(user, worker_id)
    _get_worker_or_404(db, worker_id)
    return WorkerProgressListOut(
        worker_id=worker_id, progress=build_worker_progress(db, worker_id)
    )