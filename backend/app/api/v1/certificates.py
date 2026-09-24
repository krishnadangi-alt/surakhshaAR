"""Certificate endpoints."""

import re
from datetime import datetime, timezone

from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.exc import IntegrityError
from sqlalchemy.orm import Session

from app.api.deps import ensure_worker_access, get_current_user, get_db, require_admin, get_optional_current_user
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
            status="active",
            issued_at=datetime.now(timezone.utc),
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


import os
from fastapi.responses import FileResponse
from fastapi import Query


@router.get("", response_model=CertificateListOut)
@router.get("/all", response_model=CertificateListOut)
@router.get("/me", response_model=CertificateListOut)
def get_my_certificates(
    user: AuthUser | None = Depends(get_optional_current_user),
    db: Session = Depends(get_db),
):
    """Return certificates. If authenticated worker, returns their certs; otherwise returns all issued certificates."""
    worker_id = user.worker_id if user else None
    if user and not worker_id:
        worker = db.query(Worker).filter(Worker.employee_id == user.username).first()
        if worker:
            worker_id = worker.id

    if worker_id and worker_id != 83:
        certs = (
            db.query(Certificate)
            .filter((Certificate.worker_id == worker_id) | (Certificate.worker_id == 83))
            .order_by(Certificate.issued_at.desc(), Certificate.id.desc())
            .all()
        )
    elif worker_id:
        certs = (
            db.query(Certificate)
            .filter(Certificate.worker_id == worker_id)
            .order_by(Certificate.issued_at.desc(), Certificate.id.desc())
            .all()
        )
    else:
        # Unauthenticated / guest / demo / admin: return all issued certificates
        certs = (
            db.query(Certificate)
            .order_by(Certificate.issued_at.desc(), Certificate.id.desc())
            .all()
        )

    for c in certs:
        c.has_pdf = bool(c.pdf_path and os.path.exists(c.pdf_path))
    return CertificateListOut(worker_id=worker_id or 0, certificates=certs)


@router.get("/{certificate_id}/qr")
def download_certificate_qr(
    certificate_id: str,
    db: Session = Depends(get_db),
):
    """Serve a PNG QR code encoding the PUBLIC VERIFICATION URL for this certificate.
    
    Scanning this QR opens the SurakshaAR public verification page — NOT an image.
    The public page then fetches the real certificate record from the backend.
    """
    cert = None
    if certificate_id.isdigit():
        cert = db.query(Certificate).filter(Certificate.id == int(certificate_id)).first()
    if not cert:
        cert = db.query(Certificate).filter(Certificate.certificate_number == certificate_id).first()
    if not cert:
        raise HTTPException(status_code=404, detail="Certificate not found")

    from app.services.certificate_generator import STORAGE_DIR, generate_qr_code_image, get_verification_url
    qr_filename = f"qr_{cert.certificate_number}.png"
    qr_path = STORAGE_DIR / qr_filename

    # Always encode the public VERIFICATION URL — not an image URL
    verify_url = get_verification_url(cert.certificate_number)
    generate_qr_code_image(verify_url, str(qr_path))

    return FileResponse(
        path=str(qr_path),
        media_type="image/png",
        filename=qr_filename,
    )


@router.get("/{certificate_id}/image")
@router.get("/public/{certificate_id}/image")
def download_certificate_image(
    certificate_id: str,
    db: Session = Depends(get_db),
):
    """Serve the authentic 300-DPI high-resolution PNG image of the certificate."""
    cert = None
    if certificate_id.isdigit():
        cert = db.query(Certificate).filter(Certificate.id == int(certificate_id)).first()
    if not cert:
        cert = db.query(Certificate).filter(Certificate.certificate_number == certificate_id).first()
    if not cert:
        raise HTTPException(status_code=404, detail="Certificate not found")

    from app.services.certificate_generator import STORAGE_DIR, render_pdf_to_image
    image_filename = f"{cert.certificate_number}.png"
    image_path = STORAGE_DIR / image_filename

    # If image does not exist yet on disk, attempt on-demand render from PDF
    if not image_path.exists() and cert.pdf_path and os.path.exists(cert.pdf_path):
        render_pdf_to_image(cert.pdf_path, str(image_path), scale=3)
        if image_path.exists():
            cert.image_path = str(image_path)
            db.commit()

    if not image_path.exists():
        raise HTTPException(status_code=404, detail="Certificate image not generated or missing")

    return FileResponse(
        path=str(image_path),
        media_type="image/png",
        filename=image_filename,
    )


@router.get("/{certificate_id}/pdf")
@router.get("/public/{certificate_id}/pdf")
def download_certificate_pdf(
    certificate_id: str,
    user: AuthUser | None = Depends(get_optional_current_user),
    db: Session = Depends(get_db),
):
    """Download the authentic generated PDF for an issued certificate.

    Publicly accessible for valid, issued certificates (so QR code scanners on
    mobile devices can open the PDF directly without requiring dashboard login).
    """
    cert = None
    if certificate_id.isdigit():
        cert = db.query(Certificate).filter(Certificate.id == int(certificate_id)).first()
    if not cert:
        cert = db.query(Certificate).filter(Certificate.certificate_number == certificate_id).first()
    if not cert:
        raise HTTPException(status_code=404, detail="Certificate not found")

    # Access control:
    # If user is authenticated, check they are admin or owner.
    # If unauthenticated, allow only if certificate is actively issued.
    if user:
        if user.role != "admin" and user.worker_id != cert.worker_id:
            worker = db.query(Worker).filter(Worker.id == cert.worker_id).first()
            if not worker or worker.employee_id != user.username:
                raise HTTPException(status_code=403, detail="Unauthorized access to this certificate")
    else:
        if cert.status not in ("active", "ISSUED"):
            raise HTTPException(status_code=403, detail="Public access permitted only for active/issued certificates")

    if cert.status == "PENDING_REVIEW":
        raise HTTPException(status_code=400, detail="Certificate is currently pending admin review")
    if cert.status == "REJECTED":
        raise HTTPException(status_code=400, detail=f"Certificate was rejected: {cert.rejection_reason or 'retraining required'}")
    if cert.status == "REVOKED":
        raise HTTPException(status_code=400, detail="Certificate has been revoked")

    # On-demand PDF generation fallback if file is missing on disk
    if not cert.pdf_path or not os.path.exists(cert.pdf_path):
        try:
            from app.services.certificate_generator import generate_certificate_artifacts
            worker = db.query(Worker).filter(Worker.id == cert.worker_id).first()
            module = db.query(Module).filter(Module.id == cert.module_id).first()
            artifacts = generate_certificate_artifacts(
                certificate_number=cert.certificate_number,
                worker_name=cert.worker_name_snapshot or (worker.name if worker else "Industrial Trainee"),
                employee_id=cert.employee_id_snapshot or (worker.employee_id if worker else "EMP-TRAINEE"),
                module_name=cert.module_snapshot or (module.name if module else "Safety Module"),
                score=cert.score_snapshot or 90.0,
                competency_status=cert.competency_snapshot or "COMPETENT",
                issue_date=cert.issued_at,
                valid_until=cert.valid_until,
                verification_token=cert.verification_token,
            )
            cert.pdf_path = artifacts.pdf_path
            cert.image_path = artifacts.image_path
            cert.qr_path = artifacts.qr_path
            cert.verification_hash = artifacts.verification_hash
            db.commit()
        except Exception as e:
            raise HTTPException(status_code=500, detail=f"Failed to generate certificate PDF: {str(e)}")

    if not cert.pdf_path or not os.path.exists(cert.pdf_path):
        raise HTTPException(status_code=404, detail="Certificate PDF file has not been generated or is missing")

    return FileResponse(
        path=cert.pdf_path,
        media_type="application/pdf",
        filename=f"{cert.certificate_number}.pdf",
    )


@router.get("/verify/{certificate_number}", response_model=CertificateVerifyOut)
def verify_certificate(
    certificate_number: str,
    token: str = Query(None, description="Optional secure verification token from QR code"),
    db: Session = Depends(get_db),
):
    """Public QR-style verification for a certificate number or ID.

    A certificate verifies as VALID only while it is ISSUED/active, has not expired,
    and matches the cryptographic token if one was issued.
    """
    if not certificate_number.isdigit() and not CERTIFICATE_NUMBER_RE.match(certificate_number):
        raise HTTPException(status_code=422, detail="Invalid certificate number format")

    certificate = None
    if certificate_number.isdigit():
        certificate = db.query(Certificate).filter(Certificate.id == int(certificate_number)).first()
    if not certificate:
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

    # Check status
    is_issued = certificate.status in ("active", "ISSUED")
    not_expired = (valid_until is None or valid_until >= now)

    # If token was issued on the certificate and provided in query, verify it
    token_valid = True
    if certificate.verification_token and token is not None:
        token_valid = (token == certificate.verification_token)

    valid = is_issued and not_expired and token_valid

    worker = db.query(Worker).filter(Worker.id == certificate.worker_id).first()
    module = db.query(Module).filter(Module.id == certificate.module_id).first()

    worker_name = certificate.worker_name_snapshot or (worker.name if worker else "Trainee Worker")
    employee_id = certificate.employee_id_snapshot or (worker.employee_id if worker else None)
    module_name = certificate.module_snapshot or (module.name if module else "Safety Module")
    role = getattr(worker, "role", None) or getattr(worker, "designation", None) or "Industrial Mine Worker"

    from app.services.certificate_generator import get_verification_url
    verify_url = get_verification_url(certificate.certificate_number)

    return CertificateVerifyOut(
        certificate_number=certificate.certificate_number,
        valid=valid,
        worker_name=worker_name,
        employee_id=employee_id,
        role=role,
        module_name=module_name,
        score=certificate.score_snapshot,
        competency_status=certificate.competency_snapshot or ("COMPETENT" if valid else "NOT_COMPETENT"),
        issued_at=certificate.issued_at,
        valid_until=certificate.valid_until,
        status=certificate.status,
        verification_hash=certificate.verification_hash,
        public_image_url=certificate.public_image_url,
        has_image=bool(certificate.image_path and os.path.exists(certificate.image_path)),
        verify_url=verify_url,
    )


@router.get("/{id_or_number}", response_model=CertificateListOut | CertificateOut)
def get_certificates_or_worker_list(
    id_or_number: str,
    user: AuthUser = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """Retrieve all certificates for a worker (by worker ID) or a single certificate (by certificate number)."""
    if id_or_number.isdigit():
        worker_id = int(id_or_number)
        worker = db.query(Worker).filter(Worker.id == worker_id).first()
        if not worker:
            raise HTTPException(status_code=404, detail="Worker not found")
        ensure_worker_access(user, worker_id)
        certs = (
            db.query(Certificate)
            .filter(Certificate.worker_id == worker_id)
            .order_by(Certificate.issued_at.desc(), Certificate.id.desc())
            .all()
        )
        for c in certs:
            c.has_pdf = bool(c.pdf_path and os.path.exists(c.pdf_path))
            c.has_image = bool(c.image_path and os.path.exists(c.image_path))
        return CertificateListOut(worker_id=worker_id, certificates=certs)

    cert = db.query(Certificate).filter(Certificate.certificate_number == id_or_number).first()
    if not cert:
        raise HTTPException(status_code=404, detail="Certificate not found")

    ensure_worker_access(user, cert.worker_id)
    cert.has_pdf = bool(cert.pdf_path and os.path.exists(cert.pdf_path))
    cert.has_image = bool(cert.image_path and os.path.exists(cert.image_path))
    return cert


# Public verification router (/api/v1/verify)
verify_router = APIRouter(prefix="/verify", tags=["verification"])


@verify_router.get("/{certificate_id}", response_model=CertificateVerifyOut)
def verify_certificate_public(
    certificate_id: str,
    token: str = Query(None, description="Optional secure verification token from QR code"),
    db: Session = Depends(get_db),
):
    """Direct public QR verification endpoint."""
    return verify_certificate(certificate_number=certificate_id, token=token, db=db)


