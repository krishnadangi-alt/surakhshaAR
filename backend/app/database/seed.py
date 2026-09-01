"""Seed the two MVP training modules."""

from sqlalchemy.orm import Session

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