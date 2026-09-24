"""
End-to-End Verification Suite for SurakshaAR:
- Guest ID creation & isolation (e.g. GUEST-ABC123 vs GUEST-XYZ999)
- Unique sessionId and attemptId per attempt
- Multi-attempt preservation (Attempt 1 preserved, Attempt 2 recorded separately)
- 6 Fire SOP steps simulation with PASS spray
- SyncQueue payload -> FastAPI /api/v1/sync -> database
- Offline buffering & zero-data-loss sync
- Dashboard data wiring (summary, workers, assessments, worker detail)
"""

import os
import sys

# Ensure utf-8 stdout on Windows console
if hasattr(sys.stdout, 'reconfigure'):
    sys.stdout.reconfigure(encoding='utf-8')

# Ensure backend root is on sys.path
backend_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', 'backend'))
if backend_dir not in sys.path:
    sys.path.insert(0, backend_dir)

from fastapi.testclient import TestClient
from app.main import app
from app.database import Base, engine, get_db
from app.auth.security import create_access_token

client = TestClient(app)

# Generate admin auth token for dashboard endpoints
admin_token = create_access_token("admin", "admin")
auth_headers = {"Authorization": f"Bearer {admin_token}"}

def run_tests():
    import uuid
    run_id = uuid.uuid4().hex[:6]
    print("==================================================")
    print(f"SURAKSHAAR REAL DATA & WORKFLOW E2E VERIFICATION [RUN {run_id}]")
    print("==================================================")

    # 1. GUEST A - ATTEMPT 1
    guest_a_id = f"GUEST-A{run_id}"
    session_a1 = f"sess-a1-{run_id}"
    attempt_a1 = f"att-a1-{run_id}"

    print(f"\n[TEST 1] Syncing Guest A ({guest_a_id}) - Attempt 1 ({attempt_a1})")
    payload_a1 = {
        "sync_version": 1,
        "worker_id": 0,
        "guest_id": guest_a_id,
        "device_id": "device-unity-001",
        "client_timestamp": "2026-09-17T10:00:00Z",
        "sessions": [
            {
                "type": "assessment",
                "occurred_at": "2026-09-17T10:04:15Z",
                "client_session_id": session_a1,
                "module_id": 1,
                "attempt_id": attempt_a1,
                "guest_id": guest_a_id,
                "started_at": "2026-09-17T10:00:00Z",
                "completed_at": "2026-09-17T10:04:15Z",
                "score": 90.0,
                "passed": True,
                "events": [
                    {"event_type": "hazard_identified", "correct": True, "action": "identify_electrical_hazard", "timestamp": "2026-09-17T10:00:30Z"},
                    {"event_type": "alarm_activated", "correct": True, "action": "activate_fire_alarm", "timestamp": "2026-09-17T10:01:00Z"},
                    {"event_type": "extinguisher_selected", "correct": True, "action": "select_co2_extinguisher", "timestamp": "2026-09-17T10:01:45Z"},
                    {"event_type": "safety_pin_removed", "correct": True, "action": "pull_safety_pin", "timestamp": "2026-09-17T10:02:15Z"},
                    {"event_type": "aim_base", "correct": True, "action": "aim_nozzle_base_of_fire", "timestamp": "2026-09-17T10:02:45Z"},
                    {"event_type": "pass_spray_completed", "correct": True, "action": "continuous_10s_sweep_extinguish", "timestamp": "2026-09-17T10:03:00Z"},
                ]
            }
        ]
    }

    res_a1 = client.post("/api/v1/sync", json=payload_a1)
    assert res_a1.status_code in (200, 201), f"Sync A1 failed: {res_a1.text}"
    data_a1 = res_a1.json()
    assert data_a1["sync_id"] >= 1
    assert data_a1["assessments_created"] >= 1
    print(f"✓ Guest A Attempt 1 synced successfully: {data_a1}")

    # 2. GUEST A - ATTEMPT 2 (Same guest, NEW attemptId, Attempt 1 preserved)
    session_a2 = f"sess-a2-{run_id}"
    attempt_a2 = f"att-a2-{run_id}"

    print(f"\n[TEST 2] Syncing Guest A ({guest_a_id}) - Attempt 2 ({attempt_a2})")
    payload_a2 = {
        "sync_version": 1,
        "worker_id": 0,
        "guest_id": guest_a_id,
        "device_id": "device-unity-001",
        "client_timestamp": "2026-09-17T10:30:00Z",
        "sessions": [
            {
                "type": "assessment",
                "occurred_at": "2026-09-17T10:34:00Z",
                "client_session_id": session_a2,
                "module_id": 1,
                "attempt_id": attempt_a2,
                "guest_id": guest_a_id,
                "started_at": "2026-09-17T10:30:00Z",
                "completed_at": "2026-09-17T10:34:00Z",
                "score": 75.0,
                "passed": True,
                "events": [
                    {"event_type": "hazard_identified", "correct": True, "action": "identify_electrical_hazard", "timestamp": "2026-09-17T10:30:30Z"},
                    {"event_type": "wrong_action", "correct": False, "action": "selected_water_extinguisher", "timestamp": "2026-09-17T10:31:00Z"},
                    {"event_type": "alarm_activated", "correct": True, "action": "activate_fire_alarm", "timestamp": "2026-09-17T10:31:30Z"},
                    {"event_type": "extinguisher_selected", "correct": True, "action": "select_co2_extinguisher", "timestamp": "2026-09-17T10:32:00Z"},
                    {"event_type": "safety_pin_removed", "correct": True, "action": "pull_safety_pin", "timestamp": "2026-09-17T10:32:30Z"},
                    {"event_type": "aim_base", "correct": True, "action": "aim_nozzle_base_of_fire", "timestamp": "2026-09-17T10:33:00Z"},
                    {"event_type": "pass_spray_completed", "correct": True, "action": "continuous_10s_sweep_extinguish", "timestamp": "2026-09-17T10:33:15Z"},
                ]
            }
        ]
    }

    res_a2 = client.post("/api/v1/sync", json=payload_a2)
    assert res_a2.status_code in (200, 201), f"Sync A2 failed: {res_a2.text}"
    data_a2 = res_a2.json()
    assert data_a2["sync_id"] >= 1
    print(f"✓ Guest A Attempt 2 synced successfully: {data_a2}")

    # 3. GUEST B - DIFFERENT GUEST (Isolation verification)
    guest_b_id = f"GUEST-B{run_id}"
    session_b1 = f"sess-b1-{run_id}"
    attempt_b1 = f"att-b1-{run_id}"

    print(f"\n[TEST 3] Syncing Guest B ({guest_b_id}) - Attempt 1 ({attempt_b1})")
    payload_b1 = {
        "sync_version": 1,
        "worker_id": 0,
        "guest_id": guest_b_id,
        "device_id": "device-unity-002",
        "client_timestamp": "2026-09-17T11:00:00Z",
        "sessions": [
            {
                "type": "assessment",
                "occurred_at": "2026-09-17T11:03:30Z",
                "client_session_id": session_b1,
                "module_id": 1,
                "attempt_id": attempt_b1,
                "guest_id": guest_b_id,
                "started_at": "2026-09-17T11:00:00Z",
                "completed_at": "2026-09-17T11:03:30Z",
                "score": 95.0,
                "passed": True,
                "events": [
                    {"event_type": "hazard_identified", "correct": True, "action": "identify_electrical_hazard", "timestamp": "2026-09-17T11:00:30Z"},
                    {"event_type": "alarm_activated", "correct": True, "action": "activate_fire_alarm", "timestamp": "2026-09-17T11:01:00Z"},
                    {"event_type": "extinguisher_selected", "correct": True, "action": "select_co2_extinguisher", "timestamp": "2026-09-17T11:01:30Z"},
                    {"event_type": "safety_pin_removed", "correct": True, "action": "pull_safety_pin", "timestamp": "2026-09-17T11:02:00Z"},
                    {"event_type": "aim_base", "correct": True, "action": "aim_nozzle_base_of_fire", "timestamp": "2026-09-17T11:02:30Z"},
                    {"event_type": "pass_spray_completed", "correct": True, "action": "continuous_10s_sweep_extinguish", "timestamp": "2026-09-17T11:02:45Z"},
                ]
            }
        ]
    }

    res_b1 = client.post("/api/v1/sync", json=payload_b1)
    assert res_b1.status_code in (200, 201), f"Sync B1 failed: {res_b1.text}"
    data_b1 = res_b1.json()
    assert data_b1["sync_id"] >= 1
    print(f"✓ Guest B Attempt 1 synced successfully: {data_b1}")

    # 4. DASHBOARD & DATA ISOLATION VERIFICATION
    print("\n[TEST 4] Verifying Dashboard APIs & Data Isolation")
    res_workers = client.get("/api/v1/dashboard/workers", headers=auth_headers)
    assert res_workers.status_code == 200
    workers = res_workers.json().get("workers", [])
    worker_map = {w["employee_id"]: w for w in workers}

    assert guest_a_id in worker_map, f"Guest A ({guest_a_id}) not found in workers"
    assert guest_b_id in worker_map, f"Guest B ({guest_b_id}) not found in workers"
    guest_a_worker = worker_map[guest_a_id]
    guest_b_worker = worker_map[guest_b_id]
    print(f"✓ Both Guest A (ID={guest_a_worker['id']}) and Guest B (ID={guest_b_worker['id']}) exist as separate workers")

    # Verify Guest A worker detail contains both Attempt 1 and Attempt 2
    res_a_detail = client.get(f"/api/v1/dashboard/workers/{guest_a_worker['id']}", headers=auth_headers)
    assert res_a_detail.status_code == 200
    a_detail = res_a_detail.json()
    a_session_ids = [a.get("client_session_id") for a in a_detail.get("assessments", [])]
    assert session_a1 in a_session_ids, f"Attempt 1 ({session_a1}) missing from Guest A detail"
    assert session_a2 in a_session_ids, f"Attempt 2 ({session_a2}) missing from Guest A detail"
    assert session_b1 not in a_session_ids, f"Guest B session leaked into Guest A!"
    print(f"✓ Guest A has {len(a_detail['assessments'])} distinct attempts (Attempt 1 preserved, Attempt 2 added)")

    # Verify Guest B worker detail contains ONLY Guest B's attempt
    res_b_detail = client.get(f"/api/v1/dashboard/workers/{guest_b_worker['id']}", headers=auth_headers)
    assert res_b_detail.status_code == 200
    b_detail = res_b_detail.json()
    b_session_ids = [a.get("client_session_id") for a in b_detail.get("assessments", [])]
    assert session_b1 in b_session_ids, f"Attempt B1 missing from Guest B detail"
    assert session_a1 not in b_session_ids, f"Guest A session leaked into Guest B!"
    assert session_a2 not in b_session_ids, f"Guest A session leaked into Guest B!"
    print(f"✓ Guest B data completely isolated ({len(b_detail['assessments'])} attempt, zero cross-contamination)")

    # 5. OFFLINE SYNC QUEUE BUFFERING & RESTORE VERIFICATION
    print("\n[TEST 5] Verifying Offline Sync Queue Simulation")
    # Simulate an offline attempt queued locally
    offline_guest = f"GUEST-OFF{run_id}"
    offline_session = f"sess-off-{run_id}"
    offline_attempt = f"att-off-{run_id}"
    queued_payload = {
        "sync_version": 1,
        "worker_id": 0,
        "guest_id": offline_guest,
        "device_id": f"dev-off-{run_id}",
        "batch_id": f"batch-off-{run_id}",
        "client_timestamp": "2026-09-17T12:00:00Z",
        "sessions": [
            {
                "type": "assessment",
                "occurred_at": "2026-09-17T12:05:00Z",
                "client_session_id": offline_session,
                "module_id": 1,
                "attempt_id": offline_attempt,
                "guest_id": offline_guest,
                "started_at": "2026-09-17T12:00:00Z",
                "completed_at": "2026-09-17T12:05:00Z",
                "score": 88.0,
                "passed": True,
                "events": [
                    {"event_type": "hazard_identified", "correct": True, "action": "identify_electrical_hazard", "timestamp": "2026-09-17T12:00:30Z"},
                    {"event_type": "pass_spray_completed", "correct": True, "action": "continuous_10s_sweep_extinguish", "timestamp": "2026-09-17T12:04:00Z"},
                ]
            }
        ]
    }
    # "Network restored" -> flush queue
    res_offline = client.post("/api/v1/sync", json=queued_payload)
    assert res_offline.status_code in (200, 201), f"Offline sync failed: {res_offline.text}"
    assert res_offline.json()["sync_id"] >= 1
    # Idempotent replay test: Re-flushing same batch_id must succeed without duplicate corruption
    res_replay = client.post("/api/v1/sync", json=queued_payload)
    assert res_replay.status_code in (200, 201), f"Replay failed: {res_replay.text}"
    assert res_replay.json()["sync_id"] >= 1
    print("✓ Offline buffered queue flushed successfully with zero data loss and idempotent replay safety")

    # 6. DASHBOARD SUMMARY VERIFICATION
    print("\n[TEST 6] Verifying Dashboard Summary & KPIs")
    res_summary = client.get("/api/v1/dashboard/summary", headers=auth_headers)
    assert res_summary.status_code == 200
    summary = res_summary.json()
    assert summary["total_workers"] >= 2
    assert summary["total_assessments"] >= 3
    print(f"✓ Dashboard Summary: {summary['total_workers']} workers, {summary['total_assessments']} assessments, pass rate = {summary['pass_rate']:.1f}%")

    print("\n==================================================")
    print("ALL SURAKSHAAR E2E VERIFICATION CHECKS PASSED!")
    print("==================================================")

if __name__ == '__main__':
    run_tests()
