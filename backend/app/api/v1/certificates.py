"""Certificate endpoints."""

import re
from datetime import datetime, timezone

from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.exc import IntegrityError
from sqlalchemy.orm import Session

from app.api.deps import ensure_worker_access, get_current_user, get_db, require_admin
from app.models.assessment import Assessment
from app.models.auth_user import AuthUser
from app.models.certificate import Certificate
from app.models.module import Module
from app.models.worker import Worker
from app.schemas.certificate import (
    CertificateCreate,
    CertificateListOut,
    CertificateOut,
    CertificateVerifyOut,
)
from app.services.audit_service import write_audit
from app.services.certificate_service import generate_certificate_number

router = APIRouter(prefix="/certificates", tags=["certificates"])

CERTIFICATE_NUMBER_RE = re.compile(r"^SUR-\d{4}-\d{4,}$")


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


# Re-issue attempts before giving up on a certificate-number collision.
_MAX_NUMBER_RETRIES = 5


def _latest_passing_assessment(db: Session, worker_id: int, module_id: int):
    """Latest server-scored passing assessment for the worker + module."""
    return (
        db.query(Assessment)
        .filter(
            Assessment.worker_id == worker_id,
            Assessment.module_id == module_id,
            Assessment.passed.is_(True),
        )
        .order_by(Assessment.created_at.desc(), Assessment.id.desc())
        .first()
    )


def get_certificate_eligibility(
    db: Session, worker_id: int, module_id: int
) -> tuple[bool, str | None]:
    """Determine certificate eligibility for a worker + module, server-side.

    A worker is eligible only when they hold a **server-scored passing
    assessment** for the module (the authoritative, re-used signal that the
    module/scenario was completed successfully and every required competency
    passed) and that assessment recorded **no critical errors**. Client-supplied
    scores and pass/fail values are never consulted.

    Returns ``(eligible, reason)``; the endpoint turns a non-eligible outcome
    into a ``409`` with the conventional ``{"detail": ...}`` shape.
    """
    passing = _latest_passing_assessment(db, worker_id, module_id)
    if passing is None:
        return False, "Certificate requires a passing assessment for this module"
    if passing.critical_errors:
        return (
            False,
            "Certificate cannot be issued: critical errors present in the assessment",
        )
    return True, None


@router.post("", response_model=CertificateOut, status_code=201)
def issue_certificate(
    payload: CertificateCreate,
    admin: AuthUser = Depends(require_admin),
    db: Session = Depends(get_db),
):
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

    eligible, reason = get_certificate_eligibility(
        db, payload.worker_id, payload.module_id
    )
    if not eligible:
        raise HTTPException(status_code=409, detail=reason)

    # Create the certificate; the DB-level unique constraints on
    # (worker_id, module_id) and certificate_number make conflicting or
    # duplicate issuance impossible even under concurrent requests.
    for _ in range(_MAX_NUMBER_RETRIES):
        certificate = Certificate(
            certificate_number=generate_certificate_number(db),
            worker_id=payload.worker_id,
            module_id=payload.module_id,
        )
        db.add(certificate)
        write_audit(
            db,
            action="certificate.issue",
            user=admin,
            resource_type="certificate",
            resource_id=certificate.certificate_number,
            detail={
                "worker_id": payload.worker_id,
                "module_id": payload.module_id,
            },
        )
        try:
            db.commit()
            break
        except IntegrityError:
            db.rollback()
            # Another request already issued this worker + module certificate.
            if (
                db.query(Certificate)
                .filter(
                    Certificate.worker_id == payload.worker_id,
                    Certificate.module_id == payload.module_id,
                )
                .first()
            ):
                raise HTTPException(
                    status_code=409,
                    detail="Certificate already issued for this worker and module",
                )
            # Otherwise a certificate-number collision: regenerate and retry.
    else:
        raise HTTPException(
            status_code=500, detail="Certificate issuance failed; please retry"
        )

    db.refresh(certificate)
    return certificate


@router.get("/verify/{certificate_number}", response_model=CertificateVerifyOut)
def verify_certificate(certificate_number: str, db: Session = Depends(get_db)):
    """Public QR-style verification for a certificate number.

    A certificate verifies as VALID only while it is active *and* has not
    expired (``valid_until`` still in the future). The endpoint stays public so
    a scanned QR code can resolve without credentials (Day 4 contract).
    """
    if not CERTIFICATE_NUMBER_RE.fullmatch(certificate_number):
        raise HTTPException(
            status_code=422, detail="Invalid certificate number format"
        )
    certificate = (
        db.query(Certificate)
        .filter(Certificate.certificate_number == certificate_number)
        .first()
    )
    if not certificate:
        raise HTTPException(status_code=404, detail="Certificate not found")

    now = datetime.now(timezone.utc)
    valid_until = certificate.valid_until
    if valid_until is not None and valid_until.tzinfo is None:
        valid_until = valid_until.replace(tzinfo=timezone.utc)
    valid = certificate.status == "active" and (
        valid_until is None or valid_until >= now
    )

    worker = db.query(Worker).filter(Worker.id == certificate.worker_id).first()
    module = db.query(Module).filter(Module.id == certificate.module_id).first()
    return CertificateVerifyOut(
        certificate_number=certificate.certificate_number,
        valid=valid,
        worker_name=worker.name,
        module_name=module.name,
        issued_at=certificate.issued_at,
        valid_until=certificate.valid_until,
        status=certificate.status,
    )


@router.get("/{worker_id}", response_model=CertificateListOut)
def get_worker_certificates(
    worker_id: int,
    user: AuthUser = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    ensure_worker_access(user, worker_id)
    _get_worker_or_404(db, worker_id)
    certificates = (
        db.query(Certificate)
        .filter(Certificate.worker_id == worker_id)
        .order_by(Certificate.issued_at.desc())
        .all()
    )
    return CertificateListOut(worker_id=worker_id, certificates=certificates)
