"""Progress endpoints."""

from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session

from app.api.deps import get_db
from app.models.assessment import Assessment
from app.models.module import Module
from app.models.progress import WorkerProgress
from app.models.worker import Worker
from app.schemas.progress import (
    ProgressCreate,
    ProgressItemOut,
    ProgressListOut,
    ProgressOut,
    WorkerProgressItemOut,
    WorkerProgressListOut,
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


def build_worker_progress(db: Session, worker_id: int) -> list[WorkerProgressItemOut]:
    """Merge per-module progress rows with the worker's stored assessment stats.

    Returns one item per module the worker has progress and/or assessments."""
    progress_by_module = {}
    rows = (
        db.query(WorkerProgress, Module)
        .join(Module, WorkerProgress.module_id == Module.id)
        .filter(WorkerProgress.worker_id == worker_id)
        .order_by(Module.id)
        .all()
    )
    for row, module in rows:
        progress_by_module[module.id] = (row.stage, row.status, row.updated_at)

    latest_by_module = {}
    counts = {}
    assessments = (
        db.query(Assessment)
        .filter(Assessment.worker_id == worker_id)
        .order_by(Assessment.created_at.desc(), Assessment.id.desc())
        .all()
    )
    for assessment in assessments:
        counts[assessment.module_id] = counts.get(assessment.module_id, 0) + 1
        latest_by_module.setdefault(assessment.module_id, assessment)

    progress = []
    for module in db.query(Module).order_by(Module.id).all():
        prog = progress_by_module.get(module.id)
        latest = latest_by_module.get(module.id)
        if prog is None and latest is None:
            continue
        progress.append(
            WorkerProgressItemOut(
                module_id=module.id,
                module_code=module.code,
                module_name=module.name,
                stage=prog[0] if prog else None,
                status=prog[1] if prog else None,
                last_updated=prog[2] if prog else (latest.created_at if latest else None),
                attempt_number=latest.attempt_number if latest else None,
                overall_score=latest.score if latest else None,
                passed=latest.passed if latest else None,
                assessments_count=counts.get(module.id, 0),
            )
        )
    return progress


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