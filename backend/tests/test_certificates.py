"""Tests for certificate endpoints (competency gate)."""

from datetime import datetime, timedelta, timezone

from sqlalchemy.exc import IntegrityError

from conftest import TestingSessionLocal

from app.models.assessment import Assessment
from app.models.certificate import Certificate
from events import (
    BAD_FIRE_EVENTS,
    CRITICAL_FIRE_EVENTS,
    GOOD_FIRE_EVENTS,
    GOOD_GAS_EVENTS,
)


def _create_worker(client):
    return client.post(
        "/api/v1/workers",
        json={"name": "Ramesh Kumar", "employee_id": "EMP001", "role": "Fire Safety Worker"},
    ).json()


def _pass_assessment(client, worker_id, module_id=1):
    """Submit a passing assessment so a certificate can be issued."""
    events = GOOD_FIRE_EVENTS if module_id == 1 else GOOD_GAS_EVENTS
    return client.post(
        "/api/v1/assessments",
        json={"worker_id": worker_id, "module_id": module_id, "events": events},
    ).json()


def test_issue_certificate(client):
    worker = _create_worker(client)
    _pass_assessment(client, worker["id"], module_id=1)
    response = client.post(
        "/api/v1/certificates",
        json={"worker_id": worker["id"], "module_id": 1},
    )
    assert response.status_code == 201
    data = response.json()
    assert data["id"] == 1
    assert data["certificate_number"] == "SUR-2026-0001"
    assert data["worker_id"] == worker["id"]
    assert data["module_id"] == 1
    assert data["status"] == "active"
    assert "issued_at" in data
    assert "valid_until" in data


def test_issue_certificate_duplicate(client):
    worker = _create_worker(client)
    _pass_assessment(client, worker["id"], module_id=1)
    payload = {"worker_id": worker["id"], "module_id": 1}
    assert client.post("/api/v1/certificates", json=payload).status_code == 201
    response = client.post("/api/v1/certificates", json=payload)
    assert response.status_code == 409
    assert response.json() == {"detail": "Certificate already issued for this worker and module"}


def test_issue_certificate_without_passing_assessment(client):
    """Certification is gated on demonstrated competency (passing assessment)."""
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/certificates",
        json={"worker_id": worker["id"], "module_id": 1},
    )
    assert response.status_code == 409
    assert response.json() == {
        "detail": "Certificate requires a passing assessment for this module"
    }


def test_issue_certificate_worker_not_found(client):
    response = client.post(
        "/api/v1/certificates",
        json={"worker_id": 999, "module_id": 1},
    )
    assert response.status_code == 404
    assert response.json() == {"detail": "Worker not found"}


def test_issue_certificate_module_not_found(client):
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/certificates",
        json={"worker_id": worker["id"], "module_id": 999},
    )
    assert response.status_code == 404
    assert response.json() == {"detail": "Module not found"}


def test_get_worker_certificates(client):
    worker = _create_worker(client)
    _pass_assessment(client, worker["id"], module_id=1)
    client.post(
        "/api/v1/certificates",
        json={"worker_id": worker["id"], "module_id": 1},
    )
    response = client.get(f"/api/v1/certificates/{worker['id']}")
    assert response.status_code == 200
    data = response.json()
    assert data["worker_id"] == worker["id"]
    assert len(data["certificates"]) == 1
    assert data["certificates"][0]["certificate_number"] == "SUR-2026-0001"


def test_get_worker_certificates_not_found(client):
    response = client.get("/api/v1/certificates/999")
    assert response.status_code == 404
    assert response.json() == {"detail": "Worker not found"}


def test_verify_certificate(client):
    worker = _create_worker(client)
    _pass_assessment(client, worker["id"], module_id=1)
    cert = client.post(
        "/api/v1/certificates",
        json={"worker_id": worker["id"], "module_id": 1},
    ).json()
    response = client.get(f"/api/v1/certificates/verify/{cert['certificate_number']}")
    assert response.status_code == 200
    data = response.json()
    assert data["certificate_number"] == cert["certificate_number"]
    assert data["valid"] is True
    assert data["worker_name"] == "Ramesh Kumar"
    assert data["module_name"] == "Fire & Explosion Response"
    assert data["status"] == "active"


def test_verify_certificate_not_found(client):
    response = client.get("/api/v1/certificates/verify/SUR-2026-9999")
    assert response.status_code == 404
    assert response.json() == {"detail": "Certificate not found"}


def test_issue_certificate_for_gas_module(client):
    """End-to-end: passing gas assessment unlocks a gas-module certificate."""
    worker = _create_worker(client)
    _pass_assessment(client, worker["id"], module_id=2)
    response = client.post(
        "/api/v1/certificates",
        json={"worker_id": worker["id"], "module_id": 2},
    )
    assert response.status_code == 201
    data = response.json()
    assert data["module_id"] == 2
    assert data["status"] == "active"


# ---------------------------------------------------------------------------
# DAY 5 - eligibility, duplicate/conflict safety, verification & QR support
# ---------------------------------------------------------------------------


def test_issue_certificate_rejected_after_critical_error(client):
    """A critical action forces a server-side FAIL, so certification is refused."""
    worker = _create_worker(client)
    client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": CRITICAL_FIRE_EVENTS},
    )
    response = client.post(
        "/api/v1/certificates", json={"worker_id": worker["id"], "module_id": 1}
    )
    assert response.status_code == 409
    assert response.json() == {
        "detail": "Certificate requires a passing assessment for this module"
    }


def test_issue_certificate_rejected_when_passing_assessment_has_critical_errors(client):
    """Defense in depth: a passed assessment with critical errors never certifies.

    The ML engine can never produce this row itself (critical errors force a
    FAIL), but legacy/malformed data must not silently unlock a certificate.
    """
    worker = _create_worker(client)
    db = TestingSessionLocal()
    try:
        db.add(
            Assessment(
                worker_id=worker["id"],
                module_id=1,
                attempt_number=1,
                scenario_type="fire",
                score=95.0,
                passed=True,
                pass_reason="legacy row",
                weaknesses=[],
                competency_scores={},
                critical_errors=["CRITICAL: opened door during fire"],
                events=[],
            )
        )
        db.commit()
    finally:
        db.close()

    response = client.post(
        "/api/v1/certificates", json={"worker_id": worker["id"], "module_id": 1}
    )
    assert response.status_code == 409
    assert response.json()["detail"] == (
        "Certificate cannot be issued: critical errors present in the assessment"
    )


def test_certificate_worker_module_uniqueness_enforced_in_db(client):
    """The (worker_id, module_id) pair is unique at the database level."""
    worker = _create_worker(client)
    _pass_assessment(client, worker["id"], module_id=1)
    client.post(
        "/api/v1/certificates", json={"worker_id": worker["id"], "module_id": 1}
    ).json()

    db = TestingSessionLocal()
    try:
        db.add(
            Certificate(
                certificate_number="SUR-2099-0001",
                worker_id=worker["id"],
                module_id=1,
            )
        )
        try:
            db.commit()
        except IntegrityError:
            db.rollback()
        else:
            raise AssertionError(
                "Expected IntegrityError for duplicate worker+module certificate"
            )
    finally:
        db.close()


def test_certificate_number_sequence_continues_without_collision(client):
    """New numbers continue from the highest number already used for the year."""
    worker = _create_worker(client)
    db = TestingSessionLocal()
    try:
        db.add(
            Certificate(
                certificate_number="SUR-2026-0042",
                worker_id=worker["id"],
                module_id=1,
            )
        )
        db.commit()
    finally:
        db.close()

    _pass_assessment(client, worker["id"], module_id=2)
    response = client.post(
        "/api/v1/certificates", json={"worker_id": worker["id"], "module_id": 2}
    )
    assert response.status_code == 201
    assert response.json()["certificate_number"] == "SUR-2026-0043"


def test_issue_certificate_failed_assessment_not_eligible(client):
    """A stored FAIL (major wrong action) never unlocks certification."""
    worker = _create_worker(client)
    client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": BAD_FIRE_EVENTS},
    )
    response = client.post(
        "/api/v1/certificates", json={"worker_id": worker["id"], "module_id": 1}
    )
    assert response.status_code == 409
    assert response.json() == {
        "detail": "Certificate requires a passing assessment for this module"
    }


def test_issue_certificate_ignores_client_supplied_eligibility(client):
    """Client-supplied status/number/score/pass values can never steer issuance."""
    worker = _create_worker(client)
    _pass_assessment(client, worker["id"], module_id=1)
    response = client.post(
        "/api/v1/certificates",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "status": "revoked",
            "certificate_number": "SUR-1999-9999",
            "score": 0,
            "passed": False,
            "eligible": True,
        },
    )
    assert response.status_code == 201
    data = response.json()
    assert data["status"] == "active"  # server default, not the client claim
    assert data["certificate_number"] == "SUR-2026-0001"  # server-generated


def test_issue_certificate_invalid_payload_422(client):
    assert (
        client.post(
            "/api/v1/certificates", json={"worker_id": 0, "module_id": 1}
        ).status_code
        == 422
    )
    assert client.post("/api/v1/certificates", json={"module_id": 1}).status_code == 422
    assert client.post("/api/v1/certificates", json={"worker_id": 1}).status_code == 422


def test_issue_certificate_requires_authentication(anonymous_client):
    """Issuance is admin-only and is never reachable anonymously (Day 4 RBAC)."""
    response = anonymous_client.post(
        "/api/v1/certificates", json={"worker_id": 1, "module_id": 1}
    )
    assert response.status_code == 401


def test_verify_certificate_is_public_for_qr_scans(anonymous_client, client):
    """Verification stays public so a scanned QR code resolves without a token."""
    worker = _create_worker(client)
    _pass_assessment(client, worker["id"], module_id=1)
    cert = client.post(
        "/api/v1/certificates", json={"worker_id": worker["id"], "module_id": 1}
    ).json()
    response = anonymous_client.get(
        f"/api/v1/certificates/verify/{cert['certificate_number']}"
    )
    assert response.status_code == 200
    assert response.json()["valid"] is True


def test_verify_certificate_revoked_is_invalid(client):
    """A revoked certificate still resolves (200) but verifies as INVALID."""
    worker = _create_worker(client)
    db = TestingSessionLocal()
    try:
        db.add(
            Certificate(
                certificate_number="SUR-2026-0100",
                worker_id=worker["id"],
                module_id=1,
                status="revoked",
            )
        )
        db.commit()
    finally:
        db.close()

    response = client.get("/api/v1/certificates/verify/SUR-2026-0100")
    assert response.status_code == 200
    data = response.json()
    assert data["valid"] is False
    assert data["status"] == "revoked"
    assert data["worker_name"] == "Ramesh Kumar"


def test_verify_certificate_expired_is_invalid(client):
    """An active certificate past ``valid_until`` verifies as INVALID."""
    worker = _create_worker(client)
    now = datetime.now(timezone.utc)
    db = TestingSessionLocal()
    try:
        db.add(
            Certificate(
                certificate_number="SUR-2026-0101",
                worker_id=worker["id"],
                module_id=1,
                issued_at=now - timedelta(days=400),
                valid_until=now - timedelta(days=35),
            )
        )
        db.commit()
    finally:
        db.close()

    response = client.get("/api/v1/certificates/verify/SUR-2026-0101")
    assert response.status_code == 200
    data = response.json()
    assert data["valid"] is False
    assert data["status"] == "active"


def test_verify_certificate_number_validation(client):
    """Malformed numbers are rejected before lookup; unknown-but-valid 404s."""
    for malformed in (
        "NOT-A-CERT",
        "sur-2026-0001",
        "SUR-2026-ABC",
        "SUR-26-0001",
        "SUR-2026-0001-EXTRA",
        "SUR-2026-0001%20",
    ):
        response = client.get(f"/api/v1/certificates/verify/{malformed}")
        assert response.status_code == 422, malformed
        assert response.json() == {"detail": "Invalid certificate number format"}

    response = client.get("/api/v1/certificates/verify/SUR-2026-9999")
    assert response.status_code == 404
    assert response.json() == {"detail": "Certificate not found"}


def test_verify_certificate_is_read_only(client):
    """Repeat verification never mutates the stored certificate row."""
    worker = _create_worker(client)
    _pass_assessment(client, worker["id"], module_id=1)
    cert = client.post(
        "/api/v1/certificates", json={"worker_id": worker["id"], "module_id": 1}
    ).json()
    for _ in range(3):
        response = client.get(
            f"/api/v1/certificates/verify/{cert['certificate_number']}"
        )
        assert response.status_code == 200
        assert response.json()["valid"] is True

    listed = client.get(f"/api/v1/certificates/{worker['id']}").json()["certificates"]
    assert len(listed) == 1
    assert listed[0]["status"] == "active"
    assert listed[0]["certificate_number"] == cert["certificate_number"]