"""Seed the two MVP training modules and the bootstrap admin account."""

from sqlalchemy.orm import Session

from app.auth.security import hash_password
from app.config import ADMIN_PASSWORD, ADMIN_USERNAME
from app.models.auth_user import AuthUser
from app.models.module import Module

MODULES = [
    {
        "code": "fire",
        "name": "Fire & Explosion Response",
        "description": "Fire and explosion safety training and assessment.",
    },
    {
        "code": "gas",
        "name": "Gas Leak & Confined Space Protocol",
        "description": "Gas leak and confined space safety training and assessment.",
    },
]


def seed_modules(db: Session) -> None:
    """Insert the two MVP modules if they do not already exist."""
    for data in MODULES:
        exists = db.query(Module).filter(Module.code == data["code"]).first()
        if not exists:
            db.add(Module(**data))
    db.commit()


def seed_admin(db: Session) -> None:
    """Seed the bootstrap admin account from the environment (Day 4).

    No-op when ``SURAKHSHAAR_ADMIN_PASSWORD`` is not configured, so default
    deployments never receive a default/hardcoded password.
    """
    if not ADMIN_PASSWORD:
        return
    exists = db.query(AuthUser).filter(AuthUser.username == ADMIN_USERNAME).first()
    if not exists:
        db.add(
            AuthUser(
                username=ADMIN_USERNAME,
                password_hash=hash_password(ADMIN_PASSWORD),
                role="admin",
            )
        )
        db.commit()


def seed_authoritative_certificates(db: Session) -> None:
    """Seed the two authoritative demo/production certificates (Birsa Munda and Krishna).

    Ensures that verification scans on any device (local or live Render) will find
    the authoritative record and render the authentic government verification page.
    """
    from datetime import datetime, timezone
    from app.models.worker import Worker
    from app.models.certificate import Certificate

    # 1. Seed Birsa Munda
    worker1 = db.query(Worker).filter(Worker.employee_id == "EMP-JH-001").first()
    if not worker1:
        worker1 = Worker(
            name="Birsa Munda",
            employee_id="EMP-JH-001",
            role="Industrial Mine Worker",
        )
        db.add(worker1)
        db.flush()

    cert1 = db.query(Certificate).filter(Certificate.certificate_number == "SUR-2026-0001").first()
    if not cert1:
        cert1 = Certificate(
            certificate_number="SUR-2026-0001",
            worker_id=worker1.id,
            module_id=1,
            worker_name_snapshot="Birsa Munda",
            employee_id_snapshot="EMP-JH-001",
            module_snapshot="Fire & Explosion Response",
            score_snapshot=90.0,
            competency_snapshot="COMPETENT",
            status="active",
            issued_at=datetime(2026, 9, 16, 3, 51, 8, tzinfo=timezone.utc),
            valid_until=datetime(2027, 9, 16, 3, 51, 8, tzinfo=timezone.utc),
            verification_token="token_birsa_001_secure",
            verification_hash="6c78580f2e59289d0f646f7d0a987348b0227c073fc505cbd7be476edf43fdf5",
            public_image_url="https://files.catbox.moe/hge6s4.png",
        )
        db.add(cert1)

    # 2. Seed Krishna
    worker2 = db.query(Worker).filter(Worker.employee_id == "EMP-PROD-CERT").first()
    if not worker2:
        worker2 = Worker(
            name="Krishna",
            employee_id="EMP-PROD-CERT",
            role="Industrial Mine Worker",
        )
        db.add(worker2)
        db.flush()

    cert2 = db.query(Certificate).filter(Certificate.certificate_number == "SUR-2026-0002").first()
    if not cert2:
        cert2 = Certificate(
            certificate_number="SUR-2026-0002",
            worker_id=worker2.id,
            module_id=1,
            worker_name_snapshot="Krishna",
            employee_id_snapshot="EMP-PROD-CERT",
            module_snapshot="Fire & Explosion Response",
            score_snapshot=90.0,
            competency_snapshot="COMPETENT",
            status="ISSUED",
            issued_at=datetime(2026, 9, 21, 18, 36, 51, tzinfo=timezone.utc),
            valid_until=datetime(2027, 9, 21, 18, 36, 2, tzinfo=timezone.utc),
            verification_token="GvqWPJ_F8RYN4c_h-IAOEdwWAm2paYWIOOeF0lgwBH8",
            verification_hash="6c78580f2e59289d0f646f7d0a987348b0227c073fc505cbd7be476edf43fdf5",
            public_image_url="https://files.catbox.moe/t1l5lb.png",
        )
        db.add(cert2)

    db.commit()

