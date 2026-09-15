"""Day 4 - audit logging tests (login, assessments, certificates, admin, sync)."""

from conftest import TestingSessionLocal

from app.models.audit_log import AuditLog
from events import GOOD_FIRE_EVENTS


def _load_audit_actions():
    db = TestingSessionLocal()
    try:
        rows = db.query(AuditLog.action).order_by(AuditLog.id).all()
        return [r[0] for r in rows]
    finally:
        db.close()


def test_login_audited(client):
    client.post(
        "/api/v1/auth/login", json={"username": "admin", "password": "adminpass123"}
    )
    assert "auth.login" in _load_audit_actions()


def test_login_failure_audited(client):
    client.post("/api/v1/auth/login", json={"username": "admin", "password": "bad"})
    assert "auth.login_failed" in _load_audit_actions()


def test_assessment_submission_audited(client):
    worker = client.post(
        "/api/v1/workers",
        json={
            "name": "Ramesh Kumar",
            "employee_id": "EMP-AUD-1",
            "role": "Fire Safety Worker",
        },
    ).json()
    client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": GOOD_FIRE_EVENTS},
    )
    assert "assessment.create" in _load_audit_actions()


def test_sync_audited(client):
    worker = client.post(
        "/api/v1/workers",
        json={
            "name": "Ramesh Kumar",
            "employee_id": "EMP-AUD-2",
            "role": "Fire Safety Worker",
        },
    ).json()
    client.post(
        "/api/v1/sync",
        json={
            "worker_id": worker["id"],
            "device_id": "dev-aud",
            "sessions": [
                {
                    "type": "assessment",
                    "module_id": 1,
                    "occurred_at": "2026-09-01T10:00:00Z",
                }
            ],
        },
    )
    assert "sync.process" in _load_audit_actions()


def test_certificate_issuance_audited(client):
    worker = client.post(
        "/api/v1/workers",
        json={
            "name": "Ramesh Kumar",
            "employee_id": "EMP-AUD-3",
            "role": "Fire Safety Worker",
        },
    ).json()
    client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": GOOD_FIRE_EVENTS},
    )
    client.post("/api/v1/certificates", json={"worker_id": worker["id"], "module_id": 1})
    assert "certificate.issue" in _load_audit_actions()


def test_worker_creation_audited(client):
    client.post(
        "/api/v1/workers",
        json={
            "name": "Ramesh Kumar",
            "employee_id": "EMP-AUD-4",
            "role": "Fire Safety Worker",
        },
    )
    assert "worker.create" in _load_audit_actions()