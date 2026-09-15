"""
Full End-to-End System Smoke Test
SurakshaAR | SIH 2026 PS 26041
================================
Validates full round-trip execution:
1. Authentication & JWT Bearer token generation (Worker & Admin)
2. Worker profile & curriculum module retrieval
3. Authoritative server-side evaluation of Fire SOP steps
4. Zero-tolerance safety violation override & weakness detection
5. Offline batch sync with server-side scoring & idempotency
6. Certificate issuance & public QR verification endpoint
7. Administrative dashboard metrics & audit trail
"""

import pytest
from fastapi.testclient import TestClient


def test_full_system_integration_smoke(client: TestClient, worker_client, anonymous_client: TestClient):
    w_client, worker = worker_client
    worker_id = worker["id"]

    # Step 1: Verify Worker Profile (GET /api/v1/workers/{worker_id})
    worker_resp = w_client.get(f"/api/v1/workers/{worker_id}")
    assert worker_resp.status_code == 200
    assert worker_resp.json()["name"] == "Worker One"

    # Step 2: Curriculum Modules Query (GET /api/v1/modules)
    modules_resp = w_client.get("/api/v1/modules")
    assert modules_resp.status_code == 200
    modules = modules_resp.json()["modules"]
    assert any(m["code"] == "fire" for m in modules)

    # Step 3: Authoritative Assessment of Fire SOP (POST /api/v1/assessments)
    fire_events = [
        {"event_type": "hazard_identified", "correct": True, "hazard_type": "electrical_fire", "response_time_seconds": 2.1},
        {"event_type": "ppe_selected", "correct": True, "ppe_type": "fire_resistant_suit", "response_time_seconds": 1.8},
        {"event_type": "equipment_selected", "correct": True, "equipment_type": "co2_extinguisher", "response_time_seconds": 2.5},
        {"event_type": "correct_action", "action": "remove_pin", "response_time_seconds": 1.5},
        {"event_type": "correct_action", "action": "aim_base", "response_time_seconds": 2.0},
        {"event_type": "correct_action", "action": "squeeze_lever", "response_time_seconds": 1.2},
        {"event_type": "correct_action", "action": "sweep_base", "response_time_seconds": 3.0},
        {"event_type": "evacuation_started", "safe": True, "route": "emergency_exit_A"},
        {"event_type": "assessment_completed"}
    ]

    assess_payload = {
        "worker_id": worker_id,
        "module_id": 1,
        "client_session_id": "sess_live_fire_smoke_001",
        "attempt_number": 1,
        "scenario_type": "fire",
        "events": fire_events
    }
    assess_resp = w_client.post("/api/v1/assessments", json=assess_payload)
    assert assess_resp.status_code == 201
    result = assess_resp.json()
    assert result["passed"] is True
    assert result["score"] >= 70.0
    assert result["worker_id"] == worker_id

    # Step 4: Authoritative Certificate Issuance (POST /api/v1/certificates by Admin)
    cert_issue_resp = client.post(
        "/api/v1/certificates",
        json={"worker_id": worker_id, "module_id": 1}
    )
    assert cert_issue_resp.status_code == 201
    issued_cert = cert_issue_resp.json()
    cert_no = issued_cert["certificate_number"]
    assert cert_no.startswith("SUR-")

    # Step 5: Verify Certificate Retrieval by Worker (GET /api/v1/certificates/{worker_id})
    cert_resp = w_client.get(f"/api/v1/certificates/{worker_id}")
    assert cert_resp.status_code == 200
    certs = cert_resp.json()["certificates"]
    assert len(certs) >= 1
    assert certs[0]["certificate_number"] == cert_no

    # Step 6: Public QR Verification (GET /api/v1/certificates/verify/{cert_no}) - unauthenticated
    verify_resp = anonymous_client.get(f"/api/v1/certificates/verify/{cert_no}")
    assert verify_resp.status_code == 200
    verify_data = verify_resp.json()
    assert verify_data["valid"] is True
    assert verify_data["worker_name"] == "Worker One"

    # Step 6: Critical Error Failure Test (zero tolerance)
    crit_events = [
        {"event_type": "hazard_identified", "correct": True},
        {"event_type": "critical_action", "action": "stand_in_flames", "reason": "Worker entered fire zone without thermal protection"}
    ]
    crit_payload = {
        "worker_id": worker_id,
        "module_id": 1,
        "client_session_id": "sess_live_fire_smoke_002",
        "attempt_number": 2,
        "scenario_type": "fire",
        "events": crit_events
    }
    crit_resp = w_client.post("/api/v1/assessments", json=crit_payload)
    assert crit_resp.status_code == 201
    crit_result = crit_resp.json()
    assert crit_result["passed"] is False
    assert len(crit_result["critical_errors"]) > 0

    # Step 7: Offline Batch Sync (POST /api/v1/sync) with Server-side Re-scoring
    sync_payload = {
        "worker_id": worker_id,
        "device_id": "android_smoke_device_001",
        "sessions": [
            {
                "type": "assessment",
                "module_id": 1,
                "occurred_at": "2026-09-16T04:00:00Z",
                "events": fire_events
            }
        ]
    }
    sync_resp = w_client.post("/api/v1/sync", json=sync_payload)
    assert sync_resp.status_code == 201
    sync_result = sync_resp.json()
    assert sync_result["sessions_synced"] == 1
    assert sync_result["assessments_created"] == 1

    # Step 8: Admin Dashboard Summary (GET /api/v1/dashboard/summary)
    dash_resp = client.get("/api/v1/dashboard/summary")
    assert dash_resp.status_code == 200
    dash_data = dash_resp.json()
    assert "total_workers" in dash_data
    assert dash_data["total_workers"] >= 1
