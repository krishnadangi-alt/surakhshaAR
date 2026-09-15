"""Certificate service: certificate number generation."""

from datetime import datetime, timezone

from sqlalchemy.orm import Session

from app.models.certificate import Certificate


def generate_certificate_number(db: Session) -> str:
    """Return the next certificate number in the form ``SUR-YYYY-NNNN``.

    The sequence continues from the highest number already used for the current
    year (per-year, collision-resistant against manual/migrated rows), so the
    first certificate of every year is numbered ``SUR-{year}-0001``.
    """
    year = datetime.now(timezone.utc).year
    prefix = f"SUR-{year}-"
    rows = (
        db.query(Certificate.certificate_number)
        .filter(Certificate.certificate_number.like(f"{prefix}%"))
        .all()
    )
    max_seq = 0
    for (number,) in rows:
        try:
            seq = int(number.rsplit("-", 1)[1])
        except (ValueError, IndexError):
            continue
        if seq > max_seq:
            max_seq = seq
    return f"{prefix}{max_seq + 1:04d}"