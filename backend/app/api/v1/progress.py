from datetime import datetime, timedelta, timezone

from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session

from app.api.deps import ensure_worker_access, get_current_user, get_db
from app.models.assessment import Assessment
from app.models.auth_user import AuthUser
from app.models.certificate import Certificate
from app.models.module import Module
from app.models.progress import WorkerProgress
from app.models.worker import Worker
from app.schemas.progress import (
    ProgressCreate,
    ProgressItemOut,
    ProgressListOut,
    ProgressOut,
    RetentionMilestoneOut,
    WorkerProgressItemOut,
    WorkerProgressListOut,
    WorkerRetentionListOut,
    WorkerRetentionOut,
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
def get_progress(
    worker_id: int,
    user: AuthUser = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    ensure_worker_access(user, worker_id)
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
def update_progress(
    payload: ProgressCreate,
    user: AuthUser = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    ensure_worker_access(user, payload.worker_id)
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


def _to_utc(dt):
    """Return ``dt`` as a timezone-aware UTC datetime.

    SQLite does not persist the offset, so naive values read back from the
    database are treated as UTC.
    """
    if dt is None:
        return None
    if dt.tzinfo is None:
        return dt.replace(tzinfo=timezone.utc)
    return dt


def _in_retention_window(assessment_dt, window_start, window_end) -> bool:
    """True when the assessment falls inside the given checkpoint window."""
    assessment_dt = _to_utc(assessment_dt)
    if assessment_dt is None or assessment_dt < window_start:
        return False
    if window_end is not None and assessment_dt >= window_end:
        return False
    return True


@router.get("/{worker_id}/retention", response_model=WorkerRetentionListOut)
def get_worker_retention(
    worker_id: int,
    user: AuthUser = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """Calculate Day 1, Day 7, Day 30 retention checkpoints for each module.

    The retention clock is anchored on the worker's **active certificate
    issue date** for the module (CERTIFY → RETAIN), falling back to the latest
    passing, server-scored assessment and then to any progress row. Each
    checkpoint uses a bounded window (12h grace before the due date until 12h
    before the next checkpoint), so a late retention assessment satisfies only
    its own checkpoint.
    """
    ensure_worker_access(user, worker_id)
    _get_worker_or_404(db, worker_id)

    now = datetime.now(timezone.utc)
    schedules = []

    for module in db.query(Module).order_by(Module.id).all():
        assessments = (
            db.query(Assessment)
            .filter(
                Assessment.worker_id == worker_id,
                Assessment.module_id == module.id,
            )
            .order_by(Assessment.created_at.desc(), Assessment.id.desc())
            .all()
        )

        # 1) CERTIFY -> RETAIN: an active certificate anchors the retention clock.
        certificate = (
            db.query(Certificate)
            .filter(
                Certificate.worker_id == worker_id,
                Certificate.module_id == module.id,
                Certificate.status == "active",
            )
            .order_by(Certificate.issued_at.desc())
            .first()
        )
        base_dt = _to_utc(certificate.issued_at) if certificate else None

        # 2) Otherwise anchor on the latest passing (server-scored) assessment.
        if base_dt is None:
            latest_pass = next((a for a in assessments if a.passed), None)
            if latest_pass is not None:
                base_dt = _to_utc(latest_pass.created_at)

        # 3) Fall back to the worker's progress row for this module.
        if base_dt is None:
            prog = (
                db.query(WorkerProgress)
                .filter(
                    WorkerProgress.worker_id == worker_id,
                    WorkerProgress.module_id == module.id,
                )
                .first()
            )
            if prog is not None:
                base_dt = _to_utc(prog.updated_at)

        if base_dt is None:
            continue

        checkpoints = [
            (1, "Day 1 Immediate Retention Check", 7),
            (7, "Day 7 Refresher Check", 30),
            (30, "Day 30 Competency Audit", None),
        ]
        milestones = []
        for day, title, next_day in checkpoints:
            due_date = base_dt + timedelta(days=day)
            window_start = due_date - timedelta(hours=12)
            window_end = (
                base_dt + timedelta(days=next_day) - timedelta(hours=12)
                if next_day is not None
                else None
            )

            ret_assessment = next(
                (
                    a
                    for a in assessments
                    if _in_retention_window(
                        a.created_at, window_start, window_end
                    )
                ),
                None,
            )

            if ret_assessment is not None:
                status = "completed"
                passed = ret_assessment.passed
                score = ret_assessment.score
            elif now >= due_date:
                status = "due"
                passed = None
                score = None
            else:
                status = "pending"
                passed = None
                score = None

            milestones.append(
                RetentionMilestoneOut(
                    day=day,
                    title=title,
                    due_date=due_date,
                    status=status,
                    passed=passed,
                    score=score,
                )
            )

        schedules.append(
            WorkerRetentionOut(
                worker_id=worker_id,
                module_id=module.id,
                module_code=module.code,
                module_name=module.name,
                base_date=base_dt,
                milestones=milestones,
            )
        )

    return WorkerRetentionListOut(
        worker_id=worker_id,
        retention_schedules=schedules,
    )