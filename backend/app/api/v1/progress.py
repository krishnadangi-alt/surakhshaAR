"""Progress endpoints."""

from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session

from app.api.deps import get_db
from app.models.module import Module
from app.models.progress import WorkerProgress
from app.models.worker import Worker
from app.schemas.progress import (
    ProgressCreate,
    ProgressItemOut,
    ProgressListOut,
    ProgressOut,
)

router = APIRouter(prefix="/progress", tags=["progress"])


def _get_worker_or_404(db: Session, worker_id: int) -> Worker:
    worker = db.query(Worker).filter(Worker.id == worker_id).first()
    if not worker:
        raise HTTPException(status_code=404, detail="Worker not found")
    return worker


def _get_module_or_404(db: Session, module_id: int) -> Module:
    module = db.query(Module).filter(Module.id == module_id).first()
    if not module:
        raise HTTPException(status_code=404, detail="Module not found")
    return module


@router.get("/{worker_id}", response_model=ProgressListOut)
def get_progress(worker_id: int, db: Session = Depends(get_db)):
    _get_worker_or_404(db, worker_id)
    rows = (
        db.query(WorkerProgress, Module)
        .join(Module, WorkerProgress.module_id == Module.id)
        .filter(WorkerProgress.worker_id == worker_id)
        .order_by(Module.id)
        .all()
    )
    progress = [
        ProgressItemOut(
            module_id=module.id,
            module_code=module.code,
            module_name=module.name,
            stage=row.stage,
            status=row.status,
            last_updated=row.updated_at,
        )
        for row, module in rows
    ]
    return ProgressListOut(worker_id=worker_id, progress=progress)


@router.post("", response_model=ProgressOut)
def update_progress(payload: ProgressCreate, db: Session = Depends(get_db)):
    _get_worker_or_404(db, payload.worker_id)
    _get_module_or_404(db, payload.module_id)

    progress = (
        db.query(WorkerProgress)
        .filter(
            WorkerProgress.worker_id == payload.worker_id,
            WorkerProgress.module_id == payload.module_id,
        )
        .first()
    )
    if progress:
        progress.stage = payload.stage
        progress.status = payload.status
    else:
        progress = WorkerProgress(
            worker_id=payload.worker_id,
            module_id=payload.module_id,
            stage=payload.stage,
            status=payload.status,
        )
        db.add(progress)
    db.commit()
    db.refresh(progress)
    return ProgressOut(
        worker_id=progress.worker_id,
        module_id=progress.module_id,
        stage=progress.stage,
        status=progress.status,
        updated_at=progress.updated_at,
    )