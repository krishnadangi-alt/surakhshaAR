"""Certificate review audit log model."""

from datetime import datetime, timezone

from sqlalchemy import Column, DateTime, ForeignKey, Integer, String

from app.database.connection import Base


def utcnow():
    return datetime.now(timezone.utc)


class CertificateReview(Base):
    __tablename__ = "certificate_reviews"

    id = Column(Integer, primary_key=True, index=True)
    certificate_id = Column(Integer, ForeignKey("certificates.id", ondelete="CASCADE"), nullable=True, index=True)
    attempt_id = Column(String(64), nullable=True, index=True)
    assessment_id = Column(Integer, ForeignKey("assessments.id", ondelete="SET NULL"), nullable=True, index=True)
    admin_id = Column(Integer, ForeignKey("auth_users.id", ondelete="SET NULL"), nullable=True, index=True)
    # Decision: APPROVED, REJECTED, REVOKED
    decision = Column(String(32), nullable=False, index=True)
    reason = Column(String(1024), nullable=True)
    reviewed_at = Column(DateTime, default=utcnow, nullable=False)
