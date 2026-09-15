"""Dashboard endpoints."""

from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session

from app.api.deps import get_db
from app.models.assessment import Assessment
from app.models.certificate import Certificate
from app.models.module import Module
from app.models.progress import WorkerProgress
from app.models.worker import Worker
from app.schemas.assessment import AssessmentOut
from app.schemas.certificate import CertificateOut
from app.schemas.dashboard import (
    DashboardSummaryOut,
    DashboardWorkerDetailOut,
    DashboardWorkerListOut,
    DashboardWorkerOut,
    ModuleStatOut,
)
from app.schemas.progress import ProgressItemOut

router = APIRouter(prefix="/dashboard", tags=["dashboard"])


def _get_worker_or_404(db: Session, worker_id: int) -> Worker:
    worker = db.query(Worker).filter(Worker.id == worker_id).first()
    if not worker:
        raise HTTPException(status_code=404, detail="Worker not found")
    return worker


def _progress_items(db: Session, worker_id: int) -> list[ProgressItemOut]:
    rows = (
        db.query(WorkerProgress, Module)
        .join(Module, WorkerProgress.module_id == Module.id)
        .filter(WorkerProgress.worker_id == worker_id)
        .order_by(Module.id)
        .all()
    )
    return [
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


def _certified_modules(db: Session, worker_id: int) -> list[str]:
    certificates = (
        db.query(Certificate, Module)
        .join(Module, Certificate.module_id == Module.id)
        .filter(Certificate.worker_id == worker_id)
        .all()
    )
    return [module.code for _, module in certificates]


@router.get("/summary", response_model=DashboardSummaryOut)
def dashboard_summary(db: Session = Depends(get_db)):
    total_workers = db.query(Worker).count()
    total_assessments = db.query(Assessment).count()

    certified_worker_ids = {
        c.worker_id for c in db.query(Certificate.worker_id).distinct().all()
    }
    certified_workers = len(certified_worker_ids)
    workers_in_training = total_workers - certified_workers

    passed = db.query(Assessment).filter(Assessment.passed.is_(True)).count()
    pass_rate = (passed / total_assessments * 100.0) if total_assessments else 0.0

    module_stats = []
    for module in db.query(Module).order_by(Module.id).all():
        enrolled = (
            db.query(WorkerProgress)
            .filter(WorkerProgress.module_id == module.id)
            .count()
        )
        certified = (
            db.query(Certificate)
            .filter(Certificate.module_id == module.id)
            .count()
        )
        module_stats.append(
            ModuleStatOut(
                module_id=module.id,
                module_name=module.name,
                workers_enrolled=enrolled,
                certified=certified,
            )
        )

    return DashboardSummaryOut(
        total_workers=total_workers,
        workers_in_training=workers_in_training,
        certified_workers=certified_workers,
        total_assessments=total_assessments,
        pass_rate=round(pass_rate, 1),
        module_stats=module_stats,
    )


@router.get("/workers", response_model=DashboardWorkerListOut)
def dashboard_worker_list(db: Session = Depends(get_db)):
    workers = db.query(Worker).order_by(Worker.id).all()
    result = [
        DashboardWorkerOut(
            id=worker.id,
            name=worker.name,
            employee_id=worker.employee_id,
            role=worker.role,
            progress=_progress_items(db, worker.id),
            certified_modules=_certified_modules(db, worker.id),
        )
        for worker in workers
    ]
    return DashboardWorkerListOut(workers=result)


@router.get("/workers/{worker_id}", response_model=DashboardWorkerDetailOut)
def dashboard_worker_detail(worker_id: int, db: Session = Depends(get_db)):
    worker = _get_worker_or_404(db, worker_id)
    assessments = (
        db.query(Assessment)
        .filter(Assessment.worker_id == worker_id)
        .order_by(Assessment.created_at.desc())
        .all()
    )
    certificates = (
        db.query(Certificate)
        .filter(Certificate.worker_id == worker_id)
        .order_by(Certificate.issued_at.desc())
        .all()
    )
    return DashboardWorkerDetailOut(
        id=worker.id,
        name=worker.name,
        employee_id=worker.employee_id,
        role=worker.role,
        progress=_progress_items(db, worker_id),
        assessments=[AssessmentOut.model_validate(a) for a in assessments],
        certificates=[CertificateOut.model_validate(c) for c in certificates],
    )