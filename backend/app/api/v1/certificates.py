"""Certificate endpoints."""

from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session

from app.api.deps import get_db
from app.models.assessment import Assessment
from app.models.certificate import Certificate
from app.models.module import Module
from app.models.worker import Worker
from app.schemas.certificate import (
    CertificateCreate,
    CertificateListOut,
    CertificateOut,
    CertificateVerifyOut,
)
from app.services.certificate_service import generate_certificate_number

router = APIRouter(prefix="/certificates", tags=["certificates"])


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


@router.post("", response_model=CertificateOut, status_code=201)
def issue_certificate(payload: CertificateCreate, db: Session = Depends(get_db)):
    _get_worker_or_404(db, payload.worker_id)
    _get_module_or_404(db, payload.module_id)

    existing = (
        db.query(Certificate)
        .filter(
            Certificate.worker_id == payload.worker_id,
            Certificate.module_id == payload.module_id,
        )
        .first()
    )
    if existing:
        raise HTTPException(
            status_code=409,
            detail="Certificate already issued for this worker and module",
        )

    # Competency gate: a certificate may only be issued after the worker has
    # demonstrated competency in this module (passing behavioural assessment
    # scored by the ML competency engine).
    passing_assessment = (
        db.query(Assessment)
        .filter(
            Assessment.worker_id == payload.worker_id,
            Assessment.module_id == payload.module_id,
            Assessment.passed.is_(True),
        )
        .first()
    )
    if not passing_assessment:
        raise HTTPException(
            status_code=409,
            detail="Certificate requires a passing assessment for this module",
        )

    certificate = Certificate(
        certificate_number=generate_certificate_number(db),
        worker_id=payload.worker_id,
        module_id=payload.module_id,
    )
    db.add(certificate)
    db.commit()
    db.refresh(certificate)
    return certificate


@router.get("/verify/{certificate_number}", response_model=CertificateVerifyOut)
def verify_certificate(certificate_number: str, db: Session = Depends(get_db)):
    certificate = (
        db.query(Certificate)
        .filter(Certificate.certificate_number == certificate_number)
        .first()
    )
    if not certificate:
        raise HTTPException(status_code=404, detail="Certificate not found")

    worker = db.query(Worker).filter(Worker.id == certificate.worker_id).first()
    module = db.query(Module).filter(Module.id == certificate.module_id).first()
    return CertificateVerifyOut(
        certificate_number=certificate.certificate_number,
        valid=certificate.status == "active",
        worker_name=worker.name,
        module_name=module.name,
        issued_at=certificate.issued_at,
        valid_until=certificate.valid_until,
        status=certificate.status,
    )


@router.get("/{worker_id}", response_model=CertificateListOut)
def get_worker_certificates(worker_id: int, db: Session = Depends(get_db)):
    _get_worker_or_404(db, worker_id)
    certificates = (
        db.query(Certificate)
        .filter(Certificate.worker_id == worker_id)
        .order_by(Certificate.issued_at.desc())
        .all()
    )
    return CertificateListOut(worker_id=worker_id, certificates=certificates)
