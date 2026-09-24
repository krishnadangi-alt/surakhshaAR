import os
from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session

from app.api.deps import get_db, require_admin
from app.models.assessment import Assessment
from app.models.auth_user import AuthUser
from app.models.certificate import Certificate
from app.models.module import Module
from app.models.progress import WorkerProgress
from app.models.worker import Worker
from app.schemas.assessment import AssessmentOut, CompetencyScoreOut, WeaknessOut
from app.schemas.certificate import CertificateOut
from app.schemas.dashboard import (
    CommonWeaknessOut,
    DashboardAssessmentItemOut,
    DashboardCertificateItemOut,
    DashboardSummaryOut,
    DashboardWorkerDetailOut,
    DashboardWorkerListOut,
    DashboardWorkerOut,
    ModuleStatOut,
    WorkerCompetencyProfileOut,
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


def _common_weaknesses(db: Session, limit: int = 5) -> list[CommonWeaknessOut]:
    """Aggregate the most common weaknesses across all assessments."""
    aggregated: dict[str, dict] = {}
    for assessment in db.query(Assessment).all():
        for weakness in assessment.weaknesses or []:
            if isinstance(weakness, dict):
                name = weakness.get("competency_name", "unknown")
                score = weakness.get("score")
            else:  # legacy rows storing plain strings
                name, score = str(weakness), None
            entry = aggregated.setdefault(
                name, {"count": 0, "total": 0.0, "scored": 0}
            )
            entry["count"] += 1
            if score is not None:
                entry["total"] += float(score)
                entry["scored"] += 1

    items = [
        CommonWeaknessOut(
            competency_name=name,
            count=entry["count"],
            average_score=(
                round(entry["total"] / entry["scored"], 1)
                if entry["scored"]
                else None
            ),
        )
        for name, entry in aggregated.items()
    ]
    items.sort(key=lambda item: (-item.count, item.competency_name))
    return items[:limit]


def _competency_profile(
    db: Session, worker_id: int
) -> list[WorkerCompetencyProfileOut]:
    """Latest competency profile per module for a worker."""
    rows = (
        db.query(Assessment, Module)
        .join(Module, Assessment.module_id == Module.id)
        .filter(Assessment.worker_id == worker_id)
        .order_by(Assessment.created_at.desc(), Assessment.id.desc())
        .all()
    )
    latest_by_module: dict[int, tuple[Assessment, Module]] = {}
    for assessment, module in rows:
        latest_by_module.setdefault(module.id, (assessment, module))

    profile = []
    for module_id in sorted(latest_by_module):
        assessment, module = latest_by_module[module_id]
        profile.append(
            WorkerCompetencyProfileOut(
                module_id=module.id,
                module_code=module.code,
                module_name=module.name,
                attempt_number=assessment.attempt_number,
                overall_score=assessment.score,
                passed=assessment.passed,
                competencies={
                    name: CompetencyScoreOut(**values)
                    for name, values in (assessment.competency_scores or {}).items()
                },
                weaknesses=[
                    WeaknessOut(**weakness)
                    for weakness in (assessment.weaknesses or [])
                    if isinstance(weakness, dict)
                ],
            )
        )
    return profile


@router.get("/summary", response_model=DashboardSummaryOut)
def dashboard_summary(
    admin: AuthUser = Depends(require_admin),
    db: Session = Depends(get_db),
):
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
        common_weaknesses=_common_weaknesses(db),
    )


@router.get("/workers", response_model=DashboardWorkerListOut)
def dashboard_worker_list(
    admin: AuthUser = Depends(require_admin),
    db: Session = Depends(get_db),
):
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
def dashboard_worker_detail(
    worker_id: int,
    admin: AuthUser = Depends(require_admin),
    db: Session = Depends(get_db),
):
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
        competency_profile=_competency_profile(db, worker_id),
    )


@router.get("/assessments", response_model=list[DashboardAssessmentItemOut])
def dashboard_assessments_list(
    admin: AuthUser = Depends(require_admin),
    db: Session = Depends(get_db),
):
    rows = (
        db.query(Assessment, Worker, Module)
        .join(Worker, Assessment.worker_id == Worker.id)
        .join(Module, Assessment.module_id == Module.id)
        .order_by(Assessment.created_at.desc())
        .all()
    )
    result = []
    for assessment, worker, module in rows:
        events = assessment.events or []
        correct_count = sum(
            1
            for e in events
            if e.get("correct") is True
            or e.get("result") == "correct"
            or str(e.get("event_type", "")).lower() == "correct_action"
        )
        wrong_count = sum(
            1
            for e in events
            if e.get("correct") is False
            or str(e.get("event_type", "")).lower()
            in ("wrong_action", "sequence_error", "unsafe_action", "aim_off_target")
        )
        crit_count = len(assessment.critical_errors or [])
        crit_details = (
            ", ".join(str(c) for c in assessment.critical_errors)
            if assessment.critical_errors
            else None
        )

        dur = 0.0
        if events:
            elapsed_list = [
                float(e.get("elapsed_seconds", 0.0))
                for e in events
                if e.get("elapsed_seconds") is not None
            ]
            if elapsed_list:
                dur = max(elapsed_list)

        result.append(
            DashboardAssessmentItemOut(
                id=assessment.id,
                worker_id=worker.id,
                worker_name=worker.name,
                employee_id=worker.employee_id,
                module_id=module.id,
                module_code=module.code,
                module_name=module.name,
                scenario_type=assessment.scenario_type,
                client_session_id=assessment.client_session_id,
                attempt_number=assessment.attempt_number,
                score=assessment.score,
                passed=assessment.passed,
                pass_reason=assessment.pass_reason,
                correct_actions=correct_count,
                wrong_actions=wrong_count,
                critical_errors=crit_count,
                critical_error_details=crit_details,
                duration_seconds=dur,
                created_at=assessment.created_at.isoformat() if assessment.created_at else "",
                weaknesses=assessment.weaknesses or [],
                competency_scores=assessment.competency_scores or {},
                events=events,
            )
        )
    return result


@router.get("/certificates", response_model=list[DashboardCertificateItemOut])
def dashboard_certificates_list(
    admin: AuthUser = Depends(require_admin),
    db: Session = Depends(get_db),
):
    rows = (
        db.query(Certificate, Worker, Module)
        .join(Worker, Certificate.worker_id == Worker.id)
        .join(Module, Certificate.module_id == Module.id)
        .order_by(Certificate.issued_at.desc())
        .all()
    )
    return [
        DashboardCertificateItemOut(
            id=c.id,
            certificate_number=c.certificate_number,
            worker_id=w.id,
            worker_name=w.name,
            employee_id=w.employee_id,
            module_id=m.id,
            module_name=m.name,
            module_code=m.code,
            issued_at=c.issued_at.isoformat() if c.issued_at else "",
            valid_until=c.valid_until.isoformat() if c.valid_until else "",
            status=c.status,
            score=c.score_snapshot if c.score_snapshot is not None else 90.0,
            competency_status=c.competency_snapshot or "Grade A (Competent)",
            public_image_url=c.public_image_url,
            has_image=bool(c.image_path and os.path.exists(c.image_path)),
            has_pdf=bool(c.pdf_path and os.path.exists(c.pdf_path)),
        )
        for c, w, m in rows
    ]