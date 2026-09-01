"""Sync endpoints."""

from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session

from app.api.deps import get_db
from app.models.sync_log import SyncLog
from app.models.worker import Worker
from app.schemas.sync import SyncCreate, SyncOut, SyncStatusOut

router = APIRouter(prefix="/sync", tags=["sync"])


def _get_worker_or_404(db: Session, worker_id: int) -> Worker:
    worker = db.query(Worker).filter(Worker.id == worker_id).first()
    if not worker:
        raise HTTPException(status_code=404, detail="Worker not found")
    return worker


@router.post("", response_model=SyncOut, status_code=201)
def sync_sessions(payload: SyncCreate, db: Session = Depends(get_db)):
    _get_worker_or_404(db, payload.worker_id)

    log = SyncLog(
        worker_id=payload.worker_id,
        device_id=payload.device_id,
        payload=payload.model_dump(mode="json"),
    )
    db.add(log)
    db.commit()
    db.refresh(log)
    return SyncOut(
        sync_id=log.id,
        worker_id=log.worker_id,
        synced_at=log.synced_at,
        sessions_synced=len(payload.sessions),
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