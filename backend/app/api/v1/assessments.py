"""Assessment endpoints."""

from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session

from app.api.deps import get_db
from app.models.assessment import Assessment
from app.models.module import Module
from app.models.worker import Worker
from app.schemas.assessment import (
    AssessmentCreate,
    AssessmentHistoryOut,
    AssessmentOut,
)

router = APIRouter(prefix="/assessments", tags=["assessments"])


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


@router.post("", response_model=AssessmentOut, status_code=201)
def submit_assessment(payload: AssessmentCreate, db: Session = Depends(get_db)):
    _get_worker_or_404(db, payload.worker_id)
    _get_module_or_404(db, payload.module_id)

    assessment = Assessment(
        worker_id=payload.worker_id,
        module_id=payload.module_id,
        attempt_number=payload.attempt_number,
        score=payload.score,
        passed=payload.passed,
        weaknesses=payload.weaknesses,
    )
    db.add(assessment)
    db.commit()
    db.refresh(assessment)
    return assessment


@router.get("/{worker_id}", response_model=AssessmentHistoryOut)
def get_assessment_history(worker_id: int, db: Session = Depends(get_db)):
    _get_worker_or_404(db, worker_id)
    assessments = (
        db.query(Assessment)
        .filter(Assessment.worker_id == worker_id)
        .order_by(Assessment.created_at.desc())
        .all()
    )
    return AssessmentHistoryOut(worker_id=worker_id, assessments=assessments)


@router.get("/{worker_id}/latest", response_model=AssessmentOut)
def get_latest_assessment(worker_id: int, db: Session = Depends(get_db)):
    _get_worker_or_404(db, worker_id)
    assessment = (
        db.query(Assessment)
        .filter(Assessment.worker_id == worker_id)
        .order_by(Assessment.created_at.desc())
        .first()
    )
    if not assessment:
        raise HTTPException(status_code=404, detail="No assessments found for worker")
    return assessment