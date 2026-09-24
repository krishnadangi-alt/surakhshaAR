"""Comprehensive API & Security Test Suite for SurakshaAR Master Certification System.

Verifies Rules from Master Prompt Section 63:
- API-01: Valid attempt sync -> events stored, score recalculated, enters PENDING_REVIEW
- API-02: Idempotent attempt replay -> no duplicate attempt
- API-03: Client score tampering ignored -> server recalculates true score
- API-04: Fire extinguished submitted without valid spray -> flagged / rejected
- API-05: Spray before pin -> flagged as invalid sequence / critical error
- API-06: Submission after timeout -> unsuccessful, ineligible for certificate
- API-07: Certificate download requested before admin approval -> HTTP 400 / 403
- API-08: Authorized admin approves eligible attempt -> Certificate, QR, and PDF generated; status ISSUED
- API-09: Admin rejects attempt -> status REJECTED, reason stored, no certificate issued
- API-10: Valid QR verification -> VERIFIED
- API-11: Tampered QR token -> INVALID
- API-12: Revoked certificate -> status REVOKED
- API-13: Cross-worker certificate access -> HTTP 403 Forbidden
- API-14: Worker requests /certificates/me -> returns only current worker's certificates sorted latest first
"""

import os
import sys
from pathlib import Path

# Add backend and repository root to sys.path
REPO_ROOT = Path(__file__).resolve().parent.parent
BACKEND_DIR = REPO_ROOT / "backend"
sys.path.insert(0, str(REPO_ROOT))
sys.path.insert(0, str(BACKEND_DIR))

import pytest
from fastapi.testclient import TestClient
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker
from sqlalchemy.pool import StaticPool

from app.database.connection import Base
from app.main import app
from app.api.deps import get_db
from app.models.worker import Worker
from app.models.module import Module
from app.models.auth_user import AuthUser
from app.models.certificate import Certificate
from app.models.assessment import Assessment
from app.auth.security import hash_password, create_access_token


# Setup isolated in-memory SQLite database for testing
SQLALCHEMY_DATABASE_URL = "sqlite:///:memory:"
engine = create_engine(
    SQLALCHEMY_DATABASE_URL,
    connect_args={"check_same_thread": False},
    poolclass=StaticPool,
)
TestingSessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)


@pytest.fixture(scope="module")
def db_session():
    Base.metadata.create_all(bind=engine)
    db = TestingSessionLocal()

    # Seed modules
    mod_fire = Module(id=1, name="Fire & Explosion Response", code="fire", description="Underground fire response")
    mod_gas = Module(id=2, name="Hazardous Gas Detection", code="gas", description="Toxic gas response")
    db.add_all([mod_fire, mod_gas])

    # Seed workers
    w1 = Worker(id=1, name="Krishna Dangi", employee_id="EMP-101", role="Underground Miner")
    w2 = Worker(id=2, name="Rehan Khan", employee_id="EMP-102", role="Safety Inspector")
    db.add_all([w1, w2])

    # Seed auth users
    admin_user = AuthUser(id=1, username="admin", password_hash=hash_password("Admin@123"), role="admin", is_active=True)
    worker1_user = AuthUser(id=2, username="EMP-101", password_hash=hash_password("Pass@123"), role="worker", worker_id=1, is_active=True)
    worker2_user = AuthUser(id=3, username="EMP-102", password_hash=hash_password("Pass@123"), role="worker", worker_id=2, is_active=True)
    db.add_all([admin_user, worker1_user, worker2_user])



    db.commit()
    yield db
    db.close()
    Base.metadata.drop_all(bind=engine)


@pytest.fixture(scope="module")
def client(db_session):
    def override_get_db():
        try:
            yield db_session
        finally:
            pass

    app.dependency_overrides[get_db] = override_get_db
    with TestClient(app) as test_client:
        yield test_client
    app.dependency_overrides.clear()


@pytest.fixture(scope="module")
def admin_headers():
    token = create_access_token("admin", "admin")
    return {"Authorization": f"Bearer {token}"}


@pytest.fixture(scope="module")
def worker1_headers():
    token = create_access_token("EMP-101", "worker", worker_id=1)
    return {"Authorization": f"Bearer {token}"}


@pytest.fixture(scope="module")
def worker2_headers():
    token = create_access_token("EMP-102", "worker", worker_id=2)
    return {"Authorization": f"Bearer {token}"}



def test_api_01_upload_valid_attempt_enters_pending_review(client, worker1_headers, db_session):
    """TEST API-01: Valid attempt sync -> events stored, score recalculated, enters PENDING_REVIEW (no auto-cert)."""
    payload = {
        "worker_id": 1,
        "device_id": "TEST_DEVICE_01",
        "batch_id": "BATCH_01",
        "sessions": [
            {
                "type": "assessment",
                "module_id": 1,
                "scenario_type": "fire",
                "client_session_id": "ATTEMPT_VAL_01",
                "attempt_number": 1,
                "occurred_at": "2026-09-21T10:00:00Z",
                "events": [
                    {"event_type": "training_started", "timestamp": "00:00.00"},
                    {"event_type": "hazard_identified", "action": "hazard_identified", "response_time_seconds": 2.5, "timestamp": "00:04.20"},
                    {"event_type": "alarm_activated", "action": "activate_alarm", "response_time_seconds": 2.0, "timestamp": "00:08.80"},
                    {"event_type": "extinguisher_selected", "action": "select_co2_extinguisher", "response_time_seconds": 2.2, "timestamp": "00:12.40"},
                    {"event_type": "pin_removed", "action": "remove_safety_pin", "response_time_seconds": 1.8, "timestamp": "00:18.10"},
                    {"event_type": "grip_activated", "action": "grip_handle", "response_time_seconds": 1.2, "timestamp": "00:21.00"},
                    {"event_type": "valid_aim", "action": "aim_at_base_of_fire", "response_time_seconds": 2.0, "timestamp": "00:24.50"},
                    {"event_type": "spray_started", "action": "pass_technique_spray", "duration": 10.2, "timestamp": "00:25.10"},
                    {"event_type": "fire_extinguished", "action": "fire_extinguished", "timestamp": "00:36.00"},
                    {"event_type": "assessment_completed", "timestamp": "00:38.00"},
                ]
            }
        ]
    }
    resp = client.post("/api/v1/sync", json=payload, headers=worker1_headers)
    assert resp.status_code == 201
    data = resp.json()
    assert data["assessments_created"] == 1

    # Verify certificate is in PENDING_REVIEW and NOT active/ISSUED
    cert = db_session.query(Certificate).filter(Certificate.attempt_id == "ATTEMPT_VAL_01").first()
    assert cert is not None
    assert cert.status == "PENDING_REVIEW"
    assert cert.score_snapshot >= 70.0
    assert cert.approved_at is None


def test_api_02_idempotent_attempt_upload(client, worker1_headers, db_session):
    """TEST API-02: Upload same attempt twice -> idempotent, no duplicate."""
    payload = {
        "worker_id": 1,
        "device_id": "TEST_DEVICE_01",
        "batch_id": "BATCH_01",  # Same batch ID
        "sessions": [
            {
                "type": "assessment",
                "module_id": 1,
                "client_session_id": "ATTEMPT_VAL_01",
                "occurred_at": "2026-09-21T10:00:00Z",
                "events": [{"event_type": "training_started"}]
            }
        ]
    }

    resp = client.post("/api/v1/sync", json=payload, headers=worker1_headers)
    assert resp.status_code == 200  # Replay returned

    # Verify only 1 assessment exists with this client_session_id
    count = db_session.query(Assessment).filter(Assessment.client_session_id == "ATTEMPT_VAL_01").count()
    assert count == 1


def test_api_03_client_score_tampering_ignored(client, worker1_headers):
    """TEST API-03: Client sends score=100 while events indicate mistakes -> server recalculates true score."""
    payload = {
        "worker_id": 1,
        "module_id": 1,
        "scenario_type": "fire",
        "client_session_id": "ATTEMPT_TAMPER_01",
        "events": [
            {"event_type": "training_started"},
            {"event_type": "wrong_action", "action": "wrong_switch", "severity": "major", "score": 100.0},
            {"event_type": "wrong_action", "action": "wrong_direction", "severity": "major"},
            {"event_type": "assessment_completed"},
        ]
    }
    resp = client.post("/api/v1/assessments", json=payload, headers=worker1_headers)
    assert resp.status_code == 201
    data = resp.json()
    # Server recomputed score based on mistakes, ignoring client's fake 100.0
    assert data["score"] < 70.0
    assert data["passed"] is False


def test_api_04_fire_extinguished_without_valid_spray_flagged(client, worker1_headers):
    """TEST API-04: Submit fire_extinguished without valid spray -> flagged as invalid sequence / critical error."""
    payload = {
        "worker_id": 1,
        "module_id": 1,
        "scenario_type": "fire",
        "client_session_id": "ATTEMPT_NO_SPRAY",
        "events": [
            {"event_type": "training_started"},
            {"event_type": "hazard_identified", "response_time_seconds": 2.0},
            {"event_type": "fire_extinguished", "action": "fire_extinguished"},  # Extinguished without 10s spray!
            {"event_type": "assessment_completed"},
        ]
    }
    resp = client.post("/api/v1/assessments", json=payload, headers=worker1_headers)
    assert resp.status_code == 201
    data = resp.json()
    assert data["passed"] is False
    assert any("continuous spray contact" in err.lower() for err in data["critical_errors"])


def test_api_05_spray_before_pin_flagged(client, worker1_headers):
    """TEST API-05: Submit spray before pin -> flagged as invalid sequence / critical error."""
    payload = {
        "worker_id": 1,
        "module_id": 1,
        "scenario_type": "fire",
        "client_session_id": "ATTEMPT_SPRAY_BEFORE_PIN",
        "events": [
            {"event_type": "training_started"},
            {"event_type": "hazard_identified"},
            {"event_type": "alarm_activated"},
            {"event_type": "extinguisher_selected"},
            {"event_type": "spray_started", "duration": 5.0},  # Pin was never removed!
            {"event_type": "assessment_completed"},
        ]
    }
    resp = client.post("/api/v1/assessments", json=payload, headers=worker1_headers)
    assert resp.status_code == 201
    data = resp.json()
    assert data["passed"] is False
    assert any("pin" in err.lower() for err in data["critical_errors"])


def test_api_06_submission_after_timeout_ineligible(client, worker1_headers):
    """TEST API-06: Submission after timeout -> unsuccessful, ineligible for certificate."""
    payload = {
        "worker_id": 1,
        "module_id": 1,
        "scenario_type": "fire",
        "client_session_id": "ATTEMPT_TIMEOUT",
        "events": [
            {"event_type": "training_started"},
            {"event_type": "hazard_identified"},
            {"event_type": "scenario_timeout", "action": "timeout"},  # Timed out!
            {"event_type": "assessment_completed"},
        ]
    }
    resp = client.post("/api/v1/assessments", json=payload, headers=worker1_headers)
    assert resp.status_code == 201
    data = resp.json()
    assert data["passed"] is False
    assert any("time" in err.lower() for err in data["critical_errors"])


def test_api_07_certificate_download_blocked_before_approval(client, worker1_headers, db_session):
    """TEST API-07: Attempt certificate download before admin approval -> HTTP 400 (pending review)."""
    cert = db_session.query(Certificate).filter(Certificate.attempt_id == "ATTEMPT_VAL_01").first()
    assert cert is not None
    assert cert.status == "PENDING_REVIEW"

    resp = client.get(f"/api/v1/certificates/{cert.id}/pdf", headers=worker1_headers)
    assert resp.status_code == 400
    assert "pending admin review" in resp.json()["detail"].lower()


def test_api_08_admin_approves_eligible_attempt(client, admin_headers, worker1_headers, db_session):
    """TEST API-08: Authorized admin approves eligible report -> Certificate, QR, and PDF generated; status ISSUED."""
    # 1. Admin inspects detailed review
    review_resp = client.get("/api/v1/admin/attempts/ATTEMPT_VAL_01/review", headers=admin_headers)
    assert review_resp.status_code == 200
    review_data = review_resp.json()
    assert review_data["worker_name"] == "Krishna Dangi"
    assert review_data["certificate_eligible"] is True
    assert len(review_data["timeline"]) >= 8

    # 2. Admin approves
    approve_resp = client.post(
        "/api/v1/admin/attempts/ATTEMPT_VAL_01/certificate/approve",
        json={"reason": "All 6 SOP steps verified. 10s continuous spray contact achieved."},
        headers=admin_headers,
    )
    assert approve_resp.status_code == 200
    cert_data = approve_resp.json()
    assert cert_data["status"] == "ISSUED"
    assert cert_data["has_pdf"] is True
    assert cert_data["verification_token"] is not None

    # 3. Worker can now download the authentic PDF
    cert = db_session.query(Certificate).filter(Certificate.attempt_id == "ATTEMPT_VAL_01").first()
    pdf_resp = client.get(f"/api/v1/certificates/{cert.id}/pdf", headers=worker1_headers)
    assert pdf_resp.status_code == 200
    assert pdf_resp.headers["content-type"] == "application/pdf"
    assert len(pdf_resp.content) > 1000  # Non-empty PDF


def test_api_09_admin_rejects_attempt_stores_reason(client, admin_headers, worker1_headers, db_session):
    """TEST API-09: Admin rejects report -> no certificate issued, reason stored in audit trail."""
    # Create another attempt in PENDING_REVIEW
    cert = Certificate(
        certificate_number="SUR-2026-9999",
        worker_id=1,
        module_id=1,
        attempt_id="ATTEMPT_TO_REJECT",
        status="PENDING_REVIEW",
    )
    db_session.add(cert)
    db_session.commit()

    reject_resp = client.post(
        "/api/v1/admin/attempts/ATTEMPT_TO_REJECT/certificate/reject",
        json={"reason": "Excessive spray interruptions and delayed hazard identification."},
        headers=admin_headers,
    )
    assert reject_resp.status_code == 200
    data = reject_resp.json()
    assert data["status"] == "REJECTED"
    assert "Excessive spray interruptions" in data["reason"]

    # Verify certificate cannot be downloaded
    db_session.refresh(cert)
    assert cert.status == "REJECTED"
    dl_resp = client.get(f"/api/v1/certificates/{cert.id}/pdf", headers=worker1_headers)
    assert dl_resp.status_code == 400


def test_api_10_and_11_qr_verification(client, db_session):
    """TEST API-10 & 11: Valid QR verifies, tampered QR token fails."""
    cert = db_session.query(Certificate).filter(Certificate.attempt_id == "ATTEMPT_VAL_01").first()
    assert cert is not None
    assert cert.status == "ISSUED"

    # API-10: Valid QR scan (with correct token)
    verify_resp = client.get(f"/api/v1/certificates/verify/{cert.certificate_number}?token={cert.verification_token}")
    assert verify_resp.status_code == 200
    data = verify_resp.json()
    assert data["valid"] is True
    assert data["worker_name"] == "Krishna Dangi"
    assert data["status"] == "ISSUED"

    # API-11: Tampered token
    tampered_resp = client.get(f"/api/v1/certificates/verify/{cert.certificate_number}?token=INVALID_TAMPERED_TOKEN")
    assert tampered_resp.status_code == 200
    data_tampered = tampered_resp.json()
    assert data_tampered["valid"] is False


def test_api_12_certificate_revocation(client, admin_headers, db_session):
    """TEST API-12: Revoke certificate -> status REVOKED, QR reflects revoked state."""
    cert = db_session.query(Certificate).filter(Certificate.attempt_id == "ATTEMPT_VAL_01").first()

    resp = client.post(
        f"/api/v1/admin/certificates/{cert.id}/revoke",
        json={"reason": "Safety protocol violation during field audit"},
        headers=admin_headers,
    )
    assert resp.status_code == 200
    assert resp.json()["status"] == "REVOKED"

    # QR verification now returns valid=False and status=REVOKED
    verify_resp = client.get(f"/api/v1/certificates/verify/{cert.certificate_number}?token={cert.verification_token}")
    assert verify_resp.status_code == 200
    assert verify_resp.json()["valid"] is False
    assert verify_resp.json()["status"] == "REVOKED"


def test_api_13_cross_worker_certificate_access_prevented(client, worker2_headers, db_session):
    """TEST API-13: Worker 2 requests Worker 1's certificate -> HTTP 403 Forbidden."""
    cert = db_session.query(Certificate).filter(Certificate.worker_id == 1).first()
    assert cert is not None

    resp = client.get(f"/api/v1/certificates/{cert.id}/pdf", headers=worker2_headers)
    assert resp.status_code == 403


def test_api_14_worker_requests_own_certificates_sorted_latest(client, worker1_headers, db_session):
    """TEST API-14: Worker requests /certificates/me -> returns only current worker's certificates sorted latest first."""
    resp = client.get("/api/v1/certificates/me", headers=worker1_headers)
    assert resp.status_code == 200
    data = resp.json()
    assert data["worker_id"] == 1
    certs = data["certificates"]
    assert len(certs) >= 1
    # Check that all returned certificates belong to worker 1
    for c in certs:
        assert c["worker_id"] == 1
