"""Sync endpoints — securely ingest offline sessions.

Offline assessment sessions that carry raw behavioural events are scored
server-side with the ML competency engine and stored as real Assessment
records, so scores produced on untrusted devices never enter the database.
Sessions without events are logged as-is (legacy clients).
"""

from fastapi import APIRouter, Depends, HTTPException, Response
from sqlalchemy.exc import IntegrityError
from sqlalchemy.orm import Session

from app.api.deps import get_db
from app.models.assessment import Assessment
from app.models.module import Module
from app.models.sync_log import SyncLog
from app.models.worker import Worker
from app.schemas.sync import SyncCreate, SyncOut, SyncStatusOut
from app.services.competency_service import (
    UnsupportedScenarioError,
    next_attempt_number,
    score_events,
)

router = APIRouter(prefix="/sync", tags=["sync"])


def _get_worker_or_404(db: Session, worker_id: int) -> Worker:
    worker = db.query(Worker).filter(Worker.id == worker_id).first()
    if not worker:
        raise HTTPException(status_code=404, detail="Worker not found")
    return worker


@router.post("", response_model=SyncOut, status_code=201)
def sync_sessions(payload: SyncCreate, response: Response, db: Session = Depends(get_db)):
    _get_worker_or_404(db, payload.worker_id)

    # Idempotent replay: a previously synced batch_id returns the stored sync result.

    
    if payload.batch_id:
        replay = (
            db.query(SyncLog)
            .filter(
                SyncLog.worker_id == payload.worker_id,
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
                    Assessment.worker_id == payload.worker_id,
                    Assessment.module_id == module.id,
                    Assessment.client_session_id == session.client_session_id,
                )
                .first()
            )
            if already:
                continue

        scenario_type = session.scenario_type or module.code
        events = [event.model_dump() for event in session.events]
        try:
            scored = score_events(scenario_type, events)
        except UnsupportedScenarioError as exc:
            raise HTTPException(status_code=422, detail=str(exc))

        result = scored["result"]
        assessment = Assessment(
            worker_id=payload.worker_id,
            module_id=module.id,
            attempt_number=session.attempt_number
            or next_attempt_number(db, payload.worker_id, module.id),
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

    log = SyncLog(
        worker_id=payload.worker_id,
        device_id=payload.device_id,
        batch_id=payload.batch_id,
        sessions_synced=len(payload.sessions),
        assessments_created=assessments_created,
        payload=payload.model_dump(mode="json"),
    )
    db.add(log)
    try:
        db.commit()
    except IntegrityError:
        db.rollback()
        if payload.batch_id:
            replay = (
                db.query(SyncLog)
                .filter(
                    SyncLog.worker_id == payload.worker_id,
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
def get_sync_status(worker_id: int, db: Session = Depends(get_db)):
    _get_worker_or_404(db, worker_id)
    last = (
        db.query(SyncLog)
        .filter(SyncLog.worker_id == worker_id)
        .order_by(SyncLog.synced_at.desc())
        .first()
    )
    return SyncStatusOut(
        worker_id=worker_id,
        last_synced_at=last.synced_at if last else None,
        pending_sessions=0,
    )