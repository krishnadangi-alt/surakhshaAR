"""Certificate service: certificate number generation."""

from datetime import datetime, timezone

from sqlalchemy.orm import Session

from app.models.certificate import Certificate


def generate_certificate_number(db: Session) -> str:
    """Generate the next certificate number in the form SUR-YYYY-NNNN."""
    year = datetime.now(timezone.utc).year
    count = db.query(Certificate).count()
    return f"SUR-{year}-{count + 1:04d}"