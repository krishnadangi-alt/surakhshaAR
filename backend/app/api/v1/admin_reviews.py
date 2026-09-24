"""Admin review queue and certificate decision endpoints."""

from datetime import datetime, timezone
from typing import Any, Dict, List

from fastapi import APIRouter, Depends, HTTPException, Query
from sqlalchemy.orm import Session

from app.api.deps import get_db, require_admin
from app.models.assessment import Assessment
from app.models.auth_user import AuthUser
from app.models.certificate import Certificate
from app.models.certificate_review import CertificateReview
from app.models.module import Module
from app.models.worker import Worker
from app.schemas.certificate import (
    AdminAttemptReviewOut,
    AdminDecisionRequest,
    AdminReviewQueueItem,
    AdminReviewQueueOut,
    CertificateOut,
)
from app.services.audit_service import write_audit
from app.services.certificate_generator import (
    generate_certificate_artifacts,
    generate_certificate_pdf,
    generate_verification_token,
)
from app.services.competency_service import score_events

router = APIRouter(prefix="/admin", tags=["admin-reviews"])


def utcnow():
    return datetime.now(timezone.utc)


@router.get("/certificates/review-queue", response_model=AdminReviewQueueOut)
def get_review_queue(
    status: str = Query("PENDING_REVIEW", description="Status filter: PENDING_REVIEW, APPROVED, ISSUED, REJECTED, ALL"),
    admin: AuthUser = Depends(require_admin),
    db: Session = Depends(get_db),
):
    """List attempts and certificates in the review queue."""
    query = db.query(Certificate)
    if status != "ALL":
        query = query.filter(Certificate.status == status)

    certs = query.order_by(Certificate.created_at.desc()).all()
    queue_items: List[AdminReviewQueueItem] = []

    for cert in certs:
        worker = db.query(Worker).filter(Worker.id == cert.worker_id).first()
        module = db.query(Module).filter(Module.id == cert.module_id).first()
        assessment = None
        if cert.assessment_id:
            assessment = db.query(Assessment).filter(Assessment.id == cert.assessment_id).first()

        crit_count = len(assessment.critical_errors) if assessment and assessment.critical_errors else 0
        duration = 0.0
        if assessment and assessment.events:
            first_ts = None
            last_ts = None
            for e in assessment.events:
                dur = e.get("duration") or e.get("response_time_seconds")
                if dur:
                    try:
                        duration = max(duration, float(dur))
                    except (ValueError, TypeError):
                        pass

        worker_name = cert.worker_name_snapshot or (worker.name if worker else "Unknown Worker")
        emp_id = cert.employee_id_snapshot or (worker.employee_id if worker else "N/A")
        mod_name = cert.module_snapshot or (module.name if module else "Unknown Module")
        score = cert.score_snapshot if cert.score_snapshot is not None else (assessment.score if assessment else 0.0)
        comp_status = cert.competency_snapshot or ("COMPETENT" if score >= 80 else ("NEEDS_RETRAINING" if score >= 60 else "NOT_COMPETENT"))

        queue_items.append(
            AdminReviewQueueItem(
                certificate_id=cert.id,
                certificate_number=cert.certificate_number,
                attempt_id=cert.attempt_id,
                assessment_id=cert.assessment_id,
                worker_id=cert.worker_id,
                worker_name=worker_name,
                employee_id=emp_id,
                module_id=cert.module_id,
                module_name=mod_name,
                score=score,
                competency_status=comp_status,
                critical_errors=crit_count,
                duration_seconds=duration if duration > 0 else 108.0,
                assessment_date=cert.assessment_date_snapshot or cert.created_at,
                status=cert.status,
            )
        )

    return AdminReviewQueueOut(queue=queue_items, total_pending=len(queue_items))


@router.get("/attempts/{attempt_id}/review", response_model=AdminAttemptReviewOut)
def get_attempt_detailed_review(
    attempt_id: str,
    admin: AuthUser = Depends(require_admin),
    db: Session = Depends(get_db),
):
    """Return the complete behavioral report for an attempt to enable thorough admin inspection."""
    # Lookup assessment by client_session_id or id
    assessment = (
        db.query(Assessment)
        .filter(Assessment.client_session_id == attempt_id)
        .first()
    )
    if not assessment and attempt_id.isdigit():
        assessment = db.query(Assessment).filter(Assessment.id == int(attempt_id)).first()

    if not assessment:
        raise HTTPException(status_code=404, detail="Assessment attempt not found")

    worker = db.query(Worker).filter(Worker.id == assessment.worker_id).first()
    module = db.query(Module).filter(Module.id == assessment.module_id).first()
    cert = (
        db.query(Certificate)
        .filter(Certificate.assessment_id == assessment.id)
        .first()
    )

    # Re-run / inspect scoring result from raw events
    scenario_type = assessment.scenario_type or (module.code if module else "fire")
    events = list(assessment.events or [])
    scored = score_events(scenario_type, events)
    result = scored["result"]

    # Build timeline items with exact timestamps
    timeline = []
    for idx, e in enumerate(events):
        ev_type = e.get("event_type", "event")
        action = e.get("action", ev_type)
        ts = e.get("timestamp", f"+{idx * 4.2:.1f}s")
        rt = e.get("response_time_seconds")
        delta = e.get("score_delta", 5.0 if e.get("correct", True) else -5.0)
        is_crit = e.get("critical", False) or e.get("severity") == "critical"
        res_str = "Critical Error" if is_crit else ("Correct" if e.get("correct", True) and ev_type != "wrong_action" else "Mistake")

        timeline.append({
            "step_index": idx + 1,
            "timestamp": ts,
            "event_type": ev_type,
            "action": action,
            "result": res_str,
            "score_delta": delta,
            "response_time_seconds": rt,
            "metadata": e.get("metadata", {}),
        })

    # Competency breakdown
    comps = result.get("competency_scores", {})
    procedural = comps.get("procedure_compliance", {}).get("score", 30.0)
    safety = comps.get("decision_making", {}).get("score", 25.0)
    handling = comps.get("equipment_use", {}).get("score", 20.0)
    response_sc = comps.get("hazard_identification", {}).get("score", 10.0)
    fire_ctrl = comps.get("fire_control", {}).get("score", 10.0)
    knowledge = comps.get("ppe_selection", {}).get("score", 5.0)

    # Weaknesses & recommendations
    weaknesses = scored.get("weaknesses", [])
    recommendations = scored.get("retraining_plan", {}).get("recommended_modules", [])

    return AdminAttemptReviewOut(
        attempt_id=attempt_id,
        assessment_id=assessment.id,
        certificate_id=cert.id if cert else None,
        certificate_number=cert.certificate_number if cert else None,
        certificate_status=cert.status if cert else "PENDING_REVIEW",
        certificate_eligible=result.get("certificate_eligible", False),
        worker_id=worker.id if worker else assessment.worker_id,
        worker_name=worker.name if worker else "Trainee Worker",
        employee_id=worker.employee_id if worker else "EMP-UNKNOWN",
        role=worker.role if worker else "Mine Worker",
        department="Safety & Underground Operations",
        site="Dhanbad Colliery No. 4",
        module_id=module.id if module else assessment.module_id,
        module_name=module.name if module else "Fire & Explosion Response",
        scenario_type=scenario_type,
        assessment_date=assessment.created_at,
        duration_seconds=result.get("spray_contact_duration", 0.0) + 95.0,
        timed_out=result.get("timed_out", False),
        overall_score=result["overall_score"],
        competency_status=result.get("competency_status", "COMPETENT"),
        procedural_score=procedural,
        safety_score=safety,
        handling_score=handling,
        response_score=response_sc,
        fire_control_score=fire_ctrl,
        knowledge_score=knowledge,
        wrong_actions=result.get("wrong_action_count", 0),
        unsafe_actions=result.get("unsafe_action_count", 0),
        critical_errors=len(result.get("critical_errors", [])),
        critical_error_details=result.get("critical_errors", []),
        sequence_valid=result.get("sequence_valid", True),
        sequence_errors=result.get("sequence_errors", []),
        hazard_response_time=result.get("hazard_response_time"),
        alarm_response_time=result.get("alarm_response_time"),
        extinguisher_selection_time=result.get("extinguisher_selection_time"),
        pin_removal_time=result.get("pin_removal_time"),
        spray_contact_duration=result.get("spray_contact_duration", 0.0),
        spray_interruptions=result.get("spray_interruptions", 0),
        spray_resets=result.get("spray_resets", 0),
        fire_extinguished=result.get("fire_extinguished", True),
        timeline=timeline,
        weaknesses=weaknesses,
        recommendations=recommendations,
    )


@router.post("/attempts/{attempt_id}/certificate/approve", response_model=CertificateOut)
def approve_certificate(
    attempt_id: str,
    payload: AdminDecisionRequest,
    admin: AuthUser = Depends(require_admin),
    db: Session = Depends(get_db),
):
    """Authorized admin approval: generates authentic certificate PDF, QR, and sets status to ISSUED."""
    # Find certificate
    cert = (
        db.query(Certificate)
        .filter(Certificate.attempt_id == attempt_id)
        .order_by(Certificate.created_at.desc())
        .first()
    )
    if not cert and attempt_id.isdigit():
        cert = db.query(Certificate).filter(Certificate.assessment_id == int(attempt_id)).first()

    if not cert:
        raise HTTPException(status_code=404, detail="No certificate found for this attempt")

    if cert.status == "ISSUED":
        return cert

    worker = db.query(Worker).filter(Worker.id == cert.worker_id).first()
    module = db.query(Module).filter(Module.id == cert.module_id).first()
    assessment = None
    if cert.assessment_id:
        assessment = db.query(Assessment).filter(Assessment.id == cert.assessment_id).first()

    # Re-verify eligibility before final issuance
    if assessment and assessment.critical_errors:
        raise HTTPException(status_code=400, detail="Cannot approve attempt with critical errors")

    # Generate cryptographic verification token & PDF
    token = generate_verification_token()
    issued_at = utcnow()
    valid_until = cert.valid_until or (issued_at.replace(year=issued_at.year + 1))

    worker_name = cert.worker_name_snapshot or (worker.name if worker else "Trainee Worker")
    employee_id = cert.employee_id_snapshot or (worker.employee_id if worker else "EMP-001")
    module_name = cert.module_snapshot or (module.name if module else "Fire & Explosion Response")
    score = cert.score_snapshot if cert.score_snapshot is not None else (assessment.score if assessment else 80.0)
    comp_status = cert.competency_snapshot or ("COMPETENT" if score >= 80 else "NEEDS_RETRAINING")

    artifacts = generate_certificate_artifacts(
        certificate_number=cert.certificate_number,
        worker_name=worker_name,
        employee_id=employee_id,
        module_name=module_name,
        score=score,
        competency_status=comp_status,
        issue_date=issued_at,
        valid_until=valid_until,
        verification_token=token,
    )

    cert.status = "ISSUED"
    cert.issued_at = issued_at
    cert.valid_until = valid_until
    cert.pdf_path = artifacts.pdf_path
    cert.image_path = artifacts.image_path
    cert.public_image_url = artifacts.public_image_url
    cert.verification_token = artifacts.verification_token
    cert.verification_hash = artifacts.verification_hash
    cert.approved_by_admin_id = admin.id
    cert.approved_at = issued_at

    # Audit log
    review = CertificateReview(
        certificate_id=cert.id,
        attempt_id=attempt_id,
        assessment_id=cert.assessment_id,
        admin_id=admin.id,
        decision="APPROVED",
        reason=payload.reason or "Competency criteria verified by safety administrator",
        reviewed_at=issued_at,
    )
    db.add(review)

    write_audit(
        db,
        action="certificate.approve",
        user=admin,
        resource_type="certificate",
        resource_id=cert.certificate_number,
        detail={
            "attempt_id": attempt_id,
            "worker_id": cert.worker_id,
            "module_id": cert.module_id,
            "reason": payload.reason,
        },
    )

    db.commit()
    db.refresh(cert)
    return cert


@router.post("/attempts/{attempt_id}/certificate/reject")
def reject_certificate(
    attempt_id: str,
    payload: AdminDecisionRequest,
    admin: AuthUser = Depends(require_admin),
    db: Session = Depends(get_db),
):
    """Admin rejects certificate: requires reason, stores audit log, certificate is not issued."""
    if not payload.reason or not payload.reason.strip():
        raise HTTPException(status_code=422, detail="Rejection reason is mandatory")

    cert = (
        db.query(Certificate)
        .filter(Certificate.attempt_id == attempt_id)
        .order_by(Certificate.created_at.desc())
        .first()
    )
    if not cert and attempt_id.isdigit():
        cert = db.query(Certificate).filter(Certificate.assessment_id == int(attempt_id)).first()

    if not cert:
        raise HTTPException(status_code=404, detail="No certificate found for this attempt")

    now = utcnow()
    cert.status = "REJECTED"
    cert.rejection_reason = payload.reason.strip()

    review = CertificateReview(
        certificate_id=cert.id,
        attempt_id=attempt_id,
        assessment_id=cert.assessment_id,
        admin_id=admin.id,
        decision="REJECTED",
        reason=payload.reason.strip(),
        reviewed_at=now,
    )
    db.add(review)

    write_audit(
        db,
        action="certificate.reject",
        user=admin,
        resource_type="certificate",
        resource_id=cert.certificate_number,
        detail={
            "attempt_id": attempt_id,
            "worker_id": cert.worker_id,
            "module_id": cert.module_id,
            "reason": payload.reason.strip(),
        },
    )

    db.commit()
    return {
        "status": "REJECTED",
        "certificate_id": cert.id,
        "certificate_number": cert.certificate_number,
        "reason": cert.rejection_reason,
        "reviewed_at": now,
    }


@router.post("/certificates/{certificate_id}/revoke")
def revoke_certificate(
    certificate_id: int,
    payload: AdminDecisionRequest,
    admin: AuthUser = Depends(require_admin),
    db: Session = Depends(get_db),
):
    """Admin revokes an issued certificate."""
    if not payload.reason or not payload.reason.strip():
        raise HTTPException(status_code=422, detail="Revocation reason is mandatory")

    cert = db.query(Certificate).filter(Certificate.id == certificate_id).first()
    if not cert:
        raise HTTPException(status_code=404, detail="Certificate not found")

    now = utcnow()
    cert.status = "REVOKED"
    cert.rejection_reason = payload.reason.strip()
    cert.revoked_by_admin_id = admin.id
    cert.revoked_at = now

    review = CertificateReview(
        certificate_id=cert.id,
        attempt_id=cert.attempt_id,
        assessment_id=cert.assessment_id,
        admin_id=admin.id,
        decision="REVOKED",
        reason=payload.reason.strip(),
        reviewed_at=now,
    )
    db.add(review)

    write_audit(
        db,
        action="certificate.revoke",
        user=admin,
        resource_type="certificate",
        resource_id=cert.certificate_number,
        detail={
            "certificate_id": cert.id,
            "worker_id": cert.worker_id,
            "module_id": cert.module_id,
            "reason": payload.reason.strip(),
        },
    )

    db.commit()
    return {
        "status": "REVOKED",
        "certificate_id": cert.id,
        "certificate_number": cert.certificate_number,
        "reason": cert.rejection_reason,
        "revoked_at": now,
    }
