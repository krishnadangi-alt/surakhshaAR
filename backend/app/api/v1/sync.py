"""Sync endpoints — securely ingest offline sessions.

Offline assessment sessions that carry raw behavioural events are scored
server-side with the ML competency engine and stored as real Assessment
records, so scores produced on untrusted devices never enter the database.
Sessions without events are logged as-is (legacy clients).
"""

import logging
from datetime import datetime, timedelta, timezone

from fastapi import APIRouter, Depends, HTTPException, Response
from sqlalchemy.exc import IntegrityError
from sqlalchemy.orm import Session

from app.api.deps import ensure_worker_access, get_current_user, get_db, get_optional_current_user
from app.models.assessment import Assessment
from app.models.auth_user import AuthUser
from app.models.certificate import Certificate
from app.models.module import Module
from app.models.progress import WorkerProgress
from app.models.sync_log import SyncLog
from app.models.worker import Worker
from app.schemas.sync import SyncCreate, SyncOut, SyncStatusOut
from app.services.audit_service import write_audit
from app.services.certificate_service import generate_certificate_number
from app.services.competency_service import (
    UnsupportedScenarioError,
    next_attempt_number,
    score_events,
)

logger = logging.getLogger("surakshaar.sync")

router = APIRouter(prefix="/sync", tags=["sync"])


def _get_worker_or_404(db: Session, worker_id: int) -> Worker:
    worker = db.query(Worker).filter(Worker.id == worker_id).first()
    if not worker:
        raise HTTPException(status_code=404, detail="Worker not found")
    return worker


@router.post("", response_model=SyncOut, status_code=201)
def sync_sessions(
    payload: SyncCreate,
    response: Response,
    user: AuthUser | None = Depends(get_optional_current_user),
    db: Session = Depends(get_db),
):
    logger.info("[SYNC] Incoming sync request: employee_id=%s, guest_id=%s, worker_id=%s, device=%s, sessions=%d",
                payload.employee_id, payload.guest_id, payload.worker_id, payload.device_id, len(payload.sessions))

    # 1. Resolve effective worker ID
    effective_worker_id = None
    target_id = payload.employee_id or payload.guest_id
    if not target_id:
        for s in payload.sessions:
            if getattr(s, "guest_id", None):
                target_id = s.guest_id
                break

    if target_id:
        worker = db.query(Worker).filter(Worker.employee_id == target_id).first()
        if not worker:
            is_guest = target_id.upper().startswith("GUEST") or (payload.guest_id is not None and not payload.employee_id)
            name = payload.employee_name or (f"Guest {target_id}" if is_guest else f"Worker {target_id}")
            role = "Guest Trainee" if is_guest else "Mine Worker"
            worker = Worker(
                name=name,
                employee_id=target_id,
                role=role,
            )
            db.add(worker)
            db.commit()
            db.refresh(worker)
        effective_worker_id = worker.id
    elif payload.worker_id is not None and payload.worker_id > 0:
        worker = _get_worker_or_404(db, payload.worker_id)
        effective_worker_id = worker.id
    else:
        worker = db.query(Worker).first()
        if not worker:
            worker = Worker(name="Guest Trainee", employee_id="GUEST-001", role="Guest Trainee")
            db.add(worker)
            db.commit()
            db.refresh(worker)
        effective_worker_id = worker.id

    if user is not None and user.role != "admin":
        ensure_worker_access(user, effective_worker_id)

    # Idempotent replay: a previously synced batch_id returns the stored sync result.
    if payload.batch_id:
        replay = (
            db.query(SyncLog)
            .filter(
                SyncLog.worker_id == effective_worker_id,
                SyncLog.batch_id == payload.batch_id,
            )
            .first()
        )
        if replay:
            response.status_code = 200
            return SyncOut(
                sync_id=replay.id,
                worker_id=replay.worker_id,
                synced_at=replay.synced_at,
                sessions_synced=replay.sessions_synced,
                assessments_created=replay.assessments_created,
            )

    logger.info("[SYNC] Resolved effective_worker_id=%s for target_id=%s", effective_worker_id, target_id if target_id else "(none)")

    assessments_created = 0
    for session in payload.sessions:
        if session.type != "assessment" or not session.events:
            continue

        module = db.query(Module).filter(Module.id == session.module_id).first()
        if not module:
            raise HTTPException(
                status_code=404,
                detail=f"Module not found for synced session (module_id={session.module_id})",
            )

        if session.client_session_id:
            already = (
                db.query(Assessment)
                .filter(
                    Assessment.worker_id == effective_worker_id,
                    Assessment.module_id == module.id,
                    Assessment.client_session_id == session.client_session_id,
                )
                .first()
            )
            if already:
                continue

        scenario_type = session.scenario_type or module.code
        events = [event.model_dump() for event in session.events]
        logger.info("[SYNC] Scoring session: scenario=%s, events=%d, client_session=%s",
                    scenario_type, len(events), session.client_session_id)
        try:
            scored = score_events(scenario_type, events)
        except UnsupportedScenarioError as exc:
            logger.error("[SYNC] Scoring failed for scenario=%s: %s", scenario_type, exc)
            raise HTTPException(status_code=422, detail=str(exc))

        result = scored["result"]
        logger.info("[PROCESS] attempt_id=%s, events_loaded=%d, score=%.1f, passed=%s, competency=%s, critical_errors=%s",
                    session.client_session_id, len(events), result['overall_score'], result['passed'],
                    result.get('competency_status', 'N/A'), result.get('critical_errors', []))
        assessment = Assessment(
            worker_id=effective_worker_id,
            module_id=module.id,
            attempt_number=session.attempt_number
            or next_attempt_number(db, effective_worker_id, module.id),
            scenario_type=result["scenario_type"],
            score=result["overall_score"],
            passed=result["passed"],
            pass_reason=result["pass_reason"],
            weaknesses=scored["weaknesses"],
            competency_scores=result["competency_scores"],
            critical_errors=result["critical_errors"],
            client_session_id=session.client_session_id,
            events=events,
        )
        db.add(assessment)
        db.flush()  # visible to next_attempt_number within the same sync batch
        assessments_created += 1

        # Update or create WorkerProgress
        progress = (
            db.query(WorkerProgress)
            .filter(
                WorkerProgress.worker_id == effective_worker_id,
                WorkerProgress.module_id == module.id,
            )
            .first()
        )
        if not progress:
            progress = WorkerProgress(
                worker_id=effective_worker_id,
                module_id=module.id,
                stage="assessment" if not result["passed"] else "completed",
                status="retraining" if not result["passed"] else "completed",
            )
            db.add(progress)
        else:
            progress.stage = "assessment" if not result["passed"] else "completed"
            progress.status = "retraining" if not result["passed"] else "completed"

        # Eligible assessments enter PENDING_REVIEW queue — never auto-issue active certificates
        if result.get("certificate_eligible", False) or (result["passed"] and not result["critical_errors"] and not result.get("timed_out") and result.get("sequence_valid", True)):
            existing_cert = (
                db.query(Certificate)
                .filter(
                    Certificate.worker_id == effective_worker_id,
                    Certificate.module_id == module.id,
                    Certificate.assessment_id == assessment.id,
                )
                .first()
            )
            if not existing_cert:
                logger.info("[CERTIFICATE] Creating PENDING_REVIEW cert for worker=%s module=%s assessment=%s score=%.1f",
                            effective_worker_id, module.id, assessment.id, result['overall_score'])
                cert = Certificate(
                    certificate_number=generate_certificate_number(db),
                    worker_id=effective_worker_id,
                    module_id=module.id,
                    attempt_id=session.client_session_id or f"attempt_{assessment.id}",
                    assessment_id=assessment.id,
                    worker_name_snapshot=worker.name,
                    employee_id_snapshot=worker.employee_id,
                    module_snapshot=module.name,
                    score_snapshot=result["overall_score"],
                    competency_snapshot=result.get("competency_status", "COMPETENT"),
                    assessment_date_snapshot=assessment.created_at,
                    status="PENDING_REVIEW",
                    issued_at=datetime.now(timezone.utc),
                    valid_until=datetime.now(timezone.utc) + timedelta(days=365),
                )
                db.add(cert)



    log = SyncLog(
        worker_id=effective_worker_id,
        device_id=payload.device_id,
        batch_id=payload.batch_id,
        sessions_synced=len(payload.sessions),
        assessments_created=assessments_created,
        payload=payload.model_dump(mode="json"),
    )
    db.add(log)
    write_audit(
        db,
        action="sync.process",
        user=user,
        resource_type="worker",
        resource_id=effective_worker_id,
        detail={
            "batch_id": payload.batch_id,
            "sessions_synced": len(payload.sessions),
            "assessments_created": assessments_created,
            "pending_sessions": payload.pending_sessions,
        },
    )
    logger.info("[SYNC] Committing: sessions_synced=%d, assessments_created=%d, worker_id=%s, batch_id=%s",
                len(payload.sessions), assessments_created, effective_worker_id, payload.batch_id)
    try:
        db.commit()
    except IntegrityError:
        db.rollback()
        if payload.batch_id:
            replay = (
                db.query(SyncLog)
                .filter(
                    SyncLog.worker_id == effective_worker_id,
                    SyncLog.batch_id == payload.batch_id,
                )
                .first()
            )
            if replay:
                response.status_code = 200
                return SyncOut(
                    sync_id=replay.id,
                    worker_id=replay.worker_id,
                    synced_at=replay.synced_at,
                    sessions_synced=replay.sessions_synced,
                    assessments_created=replay.assessments_created,
                )
        raise
    db.refresh(log)
    return SyncOut(
        sync_id=log.id,
        worker_id=log.worker_id,
        synced_at=log.synced_at,
        sessions_synced=len(payload.sessions),
        assessments_created=assessments_created,
    )


@router.get("/status/{worker_id}", response_model=SyncStatusOut)
def get_sync_status(
    worker_id: int,
    user: AuthUser = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    ensure_worker_access(user, worker_id)
    _get_worker_or_404(db, worker_id)
    last = (
        db.query(SyncLog)
        .filter(SyncLog.worker_id == worker_id)
        .order_by(SyncLog.synced_at.desc())
        .first()
    )
    pending = 0
    if last and isinstance(last.payload, dict):
        reported = last.payload.get("pending_sessions")
        if isinstance(reported, int):
            pending = max(0, reported)
    return SyncStatusOut(
        worker_id=worker_id,
        last_synced_at=last.synced_at if last else None,
        pending_sessions=pending,
    )