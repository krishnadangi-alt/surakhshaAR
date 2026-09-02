"""Tests for certificate endpoints (competency gate)."""

from events import GOOD_FIRE_EVENTS, GOOD_GAS_EVENTS


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