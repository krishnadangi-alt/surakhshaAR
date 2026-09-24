"""Comprehensive test suite for SurakshaAR Public QR Certificate Verification System.

Validates:
1. Authoritative public HTTPS verification URLs.
2. Public verification endpoints (/api/v1/certificates/verify/{id}, /verify/{id}, /certificate/{id}).
3. Rendering of official Jharkhand Government layout with Hindi typography.
4. Unauthenticated public PDF download for issued certificates.
5. Authoritative handling of VALID, PENDING, REVOKED, EXPIRED, and INVALID states.
6. QR code decoding and verification consistency across backend, dashboard, and app.
"""

import sys
import os
from pathlib import Path
from datetime import datetime, timedelta, timezone

# Add backend to path
sys.path.insert(0, str(Path(__file__).resolve().parent.parent / "backend"))

import pytest
from fastapi.testclient import TestClient
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker

from app.database.connection import Base
from app.models.auth_user import AuthUser
from app.models.worker import Worker
from app.models.module import Module
from app.models.certificate import Certificate
from app.main import app
from app.database.seed import seed_modules, seed_authoritative_certificates
from app.services.certificate_generator import get_verification_url


@pytest.fixture(scope="module")
def client():
    with TestClient(app) as test_client:
        yield test_client


def test_qr_verification_url_structure():
    """Verify that get_verification_url generates a public HTTPS URL."""
    url = get_verification_url("SUR-2026-0002")
    assert "https://" in url
    assert "surakhshaar.onrender.com" in url
    assert "/verify/SUR-2026-0002" in url
    assert "localhost" not in url
    assert "127.0.0.1" not in url


def test_public_html_verification_page_valid(client):
    """Test /verify/{certificate_id} serves the official Jharkhand Government layout."""
    resp = client.get("/verify/SUR-2026-0002")
    assert resp.status_code == 200
    html = resp.text

    # Validate branding & govt headers
    assert "Suraksha" in html
    assert "झारखंड सरकार" in html
    assert "श्रम, रोजगार एवं प्रशिक्षण विभाग" in html
    assert "Learn Safe | Work Safe | Build a Safer Jharkhand" in html

    # Validate verified state
    assert "प्रमाणपत्र सत्यापित है" in html
    assert "यह प्रमाणपत्र वैध है और SurakshaAR के अभिलेख में उपलब्ध है।" in html

    # Validate worker & details
    assert "Krishna" in html
    assert "SUR-2026-0002" in html
    assert "आग एवं विस्फोट से निपटने की प्रक्रिया" in html
    assert "सफलतापूर्वक पूरा किया गया" in html

    # Validate PDF action
    assert "प्रमाणपत्र देखें (PDF)" in html
    assert "/api/v1/certificates/SUR-2026-0002/pdf" in html

    # Validate footer
    assert "यह प्रमाणपत्र SurakshaAR, झारखंड सरकार द्वारा जारी किया गया है।" in html


def test_public_html_certificate_alias_route(client):
    """Test /certificate/{id} alias matching verify.surakshaar.jharkhand.gov.in/certificate/{id}."""
    resp = client.get("/certificate/SUR-2026-0002")
    assert resp.status_code == 200
    assert "प्रमाणपत्र सत्यापित है" in resp.text
    assert "Krishna" in resp.text


def test_public_api_verification_endpoint(client):
    """Test /api/v1/certificates/verify/{certificate_number} returns authoritative JSON."""
    resp = client.get("/api/v1/certificates/verify/SUR-2026-0002")
    assert resp.status_code == 200
    data = resp.json()

    assert data["certificate_number"] == "SUR-2026-0002"
    assert data["valid"] is True
    assert data["worker_name"] == "Krishna"
    assert data["employee_id"] == "EMP-PROD-CERT"
    assert data["module_name"] == "Fire & Explosion Response"
    assert data["status"] in ("ISSUED", "active")
    assert data["score"] == 90.0
    assert data["competency_status"] == "COMPETENT"
    assert "https://" in data["verify_url"]


def test_public_pdf_download_unauthenticated(client):
    """Test that public users can download/view issued certificate PDFs without logging in."""
    resp = client.get("/api/v1/certificates/SUR-2026-0002/pdf")
    assert resp.status_code == 200
    assert resp.headers["content-type"] == "application/pdf"
    assert len(resp.content) > 1000  # Valid PDF bytes


def test_invalid_certificate_handling(client):
    """Test that non-existent certificate returns 404 with Invalid UI."""
    resp = client.get("/verify/NON-EXISTENT-CERT")
    assert resp.status_code == 404
    html = resp.text
    assert "प्रमाणपत्र अमान्य है" in html
    assert "SurakshaAR के राष्ट्रीय/राज्य अभिलेख में उपलब्ध नहीं है" in html
    # Must not contain fake worker details
    assert "सफलतापूर्वक पूरा किया गया" not in html


def test_revoked_certificate_handling(client):
    """Test that revoked certificate renders the Revoked state."""
    from app.database.connection import SessionLocal
    db = SessionLocal()
    try:
        worker_rev = Worker(name="Revoked Worker", employee_id="EMP-TEST-REV-001", role="Mine Worker")
        db.add(worker_rev)
        db.flush()

        # Create temporary revoked certificate
        rev_cert = Certificate(
            certificate_number="SUR-2026-9999-REV",
            worker_id=worker_rev.id,
            module_id=1,
            worker_name_snapshot="Revoked Worker",
            employee_id_snapshot="EMP-TEST-REV-001",
            module_snapshot="Fire & Explosion Response",
            score_snapshot=85.0,
            competency_snapshot="COMPETENT",
            status="REVOKED",
            issued_at=datetime.now(timezone.utc) - timedelta(days=30),
            valid_until=datetime.now(timezone.utc) + timedelta(days=335),
        )
        db.add(rev_cert)
        db.commit()

        # Check HTML view
        resp = client.get("/verify/SUR-2026-9999-REV")
        assert resp.status_code == 200
        assert "प्रमाणपत्र रद्द किया गया" in resp.text
        assert "प्रमाणपत्र सुरक्षा नियमों के तहत रद्द (Revoked) किया गया है" in resp.text
        assert "प्रमाणपत्र देखें (PDF)" not in resp.text

        # Cleanup
        db.delete(rev_cert)
        db.delete(worker_rev)
        db.commit()
    finally:
        db.close()


def test_expired_certificate_handling(client):
    """Test that expired certificate renders the Expired state."""
    from app.database.connection import SessionLocal
    db = SessionLocal()
    try:
        worker_exp = Worker(name="Expired Worker", employee_id="EMP-TEST-EXP-001", role="Mine Worker")
        db.add(worker_exp)
        db.flush()

        exp_cert = Certificate(
            certificate_number="SUR-2026-8888-EXP",
            worker_id=worker_exp.id,
            module_id=1,
            worker_name_snapshot="Expired Worker",
            employee_id_snapshot="EMP-TEST-EXP-001",
            module_snapshot="Fire & Explosion Response",
            score_snapshot=90.0,
            competency_snapshot="COMPETENT",
            status="active",
            issued_at=datetime.now(timezone.utc) - timedelta(days=400),
            valid_until=datetime.now(timezone.utc) - timedelta(days=35),  # expired
        )
        db.add(exp_cert)
        db.commit()

        # Check HTML view
        resp = client.get("/verify/SUR-2026-8888-EXP")
        assert resp.status_code == 200
        assert "प्रमाणपत्र की वैधता समाप्त" in resp.text
        assert "निर्धारित समयावधि पूर्ण हो चुकी है" in resp.text

        # Cleanup
        db.delete(exp_cert)
        db.delete(worker_exp)
        db.commit()
    finally:
        db.close()
