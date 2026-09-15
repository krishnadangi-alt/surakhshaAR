"""Workers endpoints."""

from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session

from app.api.deps import get_db
from app.models.worker import Worker
from app.schemas.worker import WorkerCreate, WorkerOut

router = APIRouter(prefix="/workers", tags=["workers"])


@router.post("", response_model=WorkerOut, status_code=201)
def create_worker(payload: WorkerCreate, db: Session = Depends(get_db)):
    existing = db.query(Worker).filter(Worker.employee_id == payload.employee_id).first()
    if existing:
        raise HTTPException(
            status_code=409,
            detail=f"Worker with employee_id {payload.employee_id} already exists",
        )
    worker = Worker(
        name=payload.name,
        employee_id=payload.employee_id,
        role=payload.role,
    )
    db.add(worker)
    db.commit()
    db.refresh(worker)
    return worker


@router.get("/{worker_id}", response_model=WorkerOut)
def get_worker(worker_id: int, db: Session = Depends(get_db)):
    worker = db.query(Worker).filter(Worker.id == worker_id).first()
    if not worker:
        raise HTTPException(status_code=404, detail="Worker not found")
    return worker