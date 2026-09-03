"""Assessment endpoints — behaviour-based, scored by the ML competency engine.

The client submits raw VR session events; the backend runs them through
``app.services.competency_service`` (CompetencyScorer → WeaknessDetector →
RetrainingRecommender) and stores the authoritative result. Client-supplied
scores are never trusted.
"""

from fastapi import APIRouter, Depends, HTTPException, Response
from sqlalchemy.exc import IntegrityError
from sqlalchemy.orm import Session

from app.api.deps import get_db
from app.models.assessment import Assessment
from app.models.module import Module
from app.models.worker import Worker
from app.schemas.assessment import (
    AssessmentCreate,
    AssessmentHistoryOut,
    AssessmentOut,
    RetrainingPlanOut,
)
from app.services.competency_service import (
    UnsupportedScenarioError,
    next_attempt_number,
    score_events,
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
def submit_assessment(
    payload: AssessmentCreate, response: Response, db: Session = Depends(get_db)
):
    """Score the submitted behavioural events server-side and store the result."""
    _get_worker_or_404(db, payload.worker_id)
    module = _get_module_or_404(db, payload.module_id)

    # Idempotent replay: if the client key (worker+module+client_session_id) has
    # already been stored, return the existing assessment instead of creating a duplicate.
    
    if payload.client_session_id:
        existing = (
            db.query(Assessment)
            .filter(
                Assessment.worker_id == payload.worker_id,
                Assessment.module_id == module.id,
                Assessment.client_session_id == payload.client_session_id,
            )
            .first()
        )
        if existing:
            response.status_code = 200
            return existing

    scenario_type = payload.scenario_type or module.code
    events = [event.model_dump() for event in payload.events]
    try:
        scored = score_events(scenario_type, events)
    except UnsupportedScenarioError as exc:
        raise HTTPException(status_code=422, detail=str(exc))

    result = scored["result"]
    attempt_number = payload.attempt_number or next_attempt_number(
        db, payload.worker_id, module.id
    )

    assessment = Assessment(
        worker_id=payload.worker_id,
        module_id=module.id,
        attempt_number=attempt_number,
        scenario_type=result["scenario_type"],
        score=result["overall_score"],
        passed=result["passed"],
        pass_reason=result["pass_reason"],
        weaknesses=scored["weaknesses"],
        competency_scores=result["competency_scores"],
        critical_errors=result["critical_errors"],
        client_session_id=payload.client_session_id,
        events=events,
    )
    db.add(assessment)
    try:
        db.commit()
    except IntegrityError:
        db.rollback()
        if payload.client_session_id:
            existing = (
                db.query(Assessment)
                .filter(
                    Assessment.worker_id == payload.worker_id,
                    Assessment.module_id == module.id,
                    Assessment.client_session_id == payload.client_session_id,
                )
                .first()
            )
            if existing:
                response.status_code = 200
                return existing
        raise
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


@router.get("/{assessment_id}/retraining-plan", response_model=RetrainingPlanOut)
def get_retraining_plan(assessment_id: int, db: Session = Depends(get_db)):
    """Recompute the targeted retraining plan from the stored assessment events."""
    assessment = db.query(Assessment).filter(Assessment.id == assessment_id).first()
    if not assessment:
        raise HTTPException(status_code=404, detail="Assessment not found")
    try:
        scored = score_events(assessment.scenario_type, list(assessment.events or []))
    except UnsupportedScenarioError as exc:
        raise HTTPException(status_code=422, detail=str(exc))
    return scored["retraining_plan"]