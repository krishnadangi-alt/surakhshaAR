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
