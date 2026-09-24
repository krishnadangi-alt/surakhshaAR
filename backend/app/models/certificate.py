from datetime import datetime, timedelta, timezone

from sqlalchemy import Column, DateTime, Float, ForeignKey, Integer, String, UniqueConstraint

from app.database.connection import Base


def utcnow():
    return datetime.now(timezone.utc)


def default_valid_until():
    return utcnow() + timedelta(days=365)


class Certificate(Base):
    __tablename__ = "certificates"
    __table_args__ = (
        UniqueConstraint("worker_id", "module_id", name="uq_certificate_worker_module"),
    )

    id = Column(Integer, primary_key=True, index=True)
    certificate_number = Column(String, unique=True, nullable=False, index=True)
    worker_id = Column(Integer, ForeignKey("workers.id"), nullable=False, index=True)
    module_id = Column(Integer, ForeignKey("modules.id"), nullable=False, index=True)
    attempt_id = Column(String(64), nullable=True, index=True)
    assessment_id = Column(Integer, ForeignKey("assessments.id", ondelete="SET NULL"), nullable=True, index=True)

    # Immutable Snapshots at Issuance
    worker_name_snapshot = Column(String, nullable=True)
    employee_id_snapshot = Column(String, nullable=True)
    module_snapshot = Column(String, nullable=True)
    score_snapshot = Column(Float, nullable=True)
    competency_snapshot = Column(String, nullable=True)
    assessment_date_snapshot = Column(DateTime, nullable=True)

    # Cryptographic Verification & Artifacts
    verification_token = Column(String(64), nullable=True, index=True)
    verification_hash = Column(String(128), nullable=True)
    pdf_path = Column(String, nullable=True)
    image_path = Column(String, nullable=True)
    public_image_url = Column(String, nullable=True)

    # Lifecycle & Timestamps
    issued_at = Column(DateTime, nullable=True)
    valid_until = Column(DateTime, default=default_valid_until, nullable=True)
    # Statuses: PENDING_REVIEW, APPROVED, ISSUED, active, REJECTED, REVOKED, EXPIRED
    status = Column(String, nullable=False, default="active", index=True)

    # Admin Review Audit
    approved_by_admin_id = Column(Integer, ForeignKey("auth_users.id", ondelete="SET NULL"), nullable=True)
    approved_at = Column(DateTime, nullable=True)
    rejection_reason = Column(String, nullable=True)
    revoked_by_admin_id = Column(Integer, ForeignKey("auth_users.id", ondelete="SET NULL"), nullable=True)
    revoked_at = Column(DateTime, nullable=True)

    created_at = Column(DateTime, default=utcnow, nullable=False)
    updated_at = Column(DateTime, default=utcnow, onupdate=utcnow, nullable=False)