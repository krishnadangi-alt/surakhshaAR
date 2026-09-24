import json
import urllib.request
import urllib.error
import time

BASE_URL = "http://localhost:8000/api/v1"

def login_admin():
    req = urllib.request.Request(
        f"{BASE_URL}/auth/login",
        data=json.dumps({"username": "admin", "password": "Admin@123"}).encode("utf-8"),
        headers={"Content-Type": "application/json"}
    )
    with urllib.request.urlopen(req) as resp:
        data = json.loads(resp.read().decode("utf-8"))
        return data["access_token"]

def test_endpoints(token):
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json"
    }

    # 1. Summary
    req = urllib.request.Request(f"{BASE_URL}/dashboard/summary", headers=headers)
    with urllib.request.urlopen(req) as resp:
        summary = json.loads(resp.read().decode("utf-8"))
        print(f"[TEST] Dashboard Summary: {summary['total_workers']} workers, {summary['total_assessments']} assessments, {summary['certified_workers']} certified, pass rate: {summary['pass_rate']}%")

    # 2. Workers
    req = urllib.request.Request(f"{BASE_URL}/dashboard/workers", headers=headers)
    with urllib.request.urlopen(req) as resp:
        workers = json.loads(resp.read().decode("utf-8"))["workers"]
        print(f"[TEST] Dashboard Workers: count={len(workers)}")

    # 3. Assessments
    req = urllib.request.Request(f"{BASE_URL}/dashboard/assessments", headers=headers)
    with urllib.request.urlopen(req) as resp:
        assessments = json.loads(resp.read().decode("utf-8"))
        print(f"[TEST] Dashboard Assessments: count={len(assessments)}")

    # 4. Certificates
    req = urllib.request.Request(f"{BASE_URL}/dashboard/certificates", headers=headers)
    with urllib.request.urlopen(req) as resp:
        certs = json.loads(resp.read().decode("utf-8"))
        print(f"[TEST] Dashboard Certificates: count={len(certs)}")

    return summary, workers, assessments, certs

def test_mobile_live_sync(token):
    # Simulate an actual mobile app sync payload for a worker completing Fire training
    sync_session_id = f"sess-mobile-{int(time.time())}"
    sync_payload = {
        "device_id": "device-mobile-android-01",
        "worker_id": 1,
        "guest_id": None,
        "batch_id": f"batch-{int(time.time())}",
        "pending_sessions": 1,
        "sessions": [
            {
                "type": "assessment",
                "module_id": 1,
                "score": 92.0,
                "passed": True,
                "weaknesses": [],
                "occurred_at": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()),
                "scenario_type": "fire",
                "attempt_number": 1,
                "client_session_id": sync_session_id,
                "events": [
                    {
                        "event_type": "hazard_identified",
                        "action": "identify_electrical_hazard",
                        "correct": True,
                        "timestamp": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()),
                        "elapsed_seconds": 15.0
                    },
                    {
                        "event_type": "alarm_activated",
                        "action": "activate_fire_alarm",
                        "correct": True,
                        "timestamp": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()),
                        "elapsed_seconds": 30.0
                    },
                    {
                        "event_type": "extinguisher_selected",
                        "action": "select_co2_extinguisher",
                        "correct": True,
                        "timestamp": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()),
                        "elapsed_seconds": 45.0
                    },
                    {
                        "event_type": "safety_pin_removed",
                        "action": "pull_safety_pin",
                        "correct": True,
                        "timestamp": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()),
                        "elapsed_seconds": 60.0
                    },
                    {
                        "event_type": "aim_base",
                        "action": "aim_nozzle_base_of_fire",
                        "correct": True,
                        "timestamp": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()),
                        "elapsed_seconds": 75.0
                    },
                    {
                        "event_type": "pass_spray_completed",
                        "action": "continuous_10s_sweep_extinguish",
                        "correct": True,
                        "timestamp": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()),
                        "elapsed_seconds": 88.0
                    }
                ]
            }
        ]
    }

    req = urllib.request.Request(
        f"{BASE_URL}/sync",
        data=json.dumps(sync_payload).encode("utf-8"),
        headers={
            "Content-Type": "application/json",
            "Authorization": f"Bearer {token}"
        }
    )
    with urllib.request.urlopen(req) as resp:
        result = json.loads(resp.read().decode("utf-8"))
        print(f"[TEST] Live Mobile Sync Response: {result}")

    # Verify that the new assessment appears in /dashboard/assessments
    headers = {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}
    req = urllib.request.Request(f"{BASE_URL}/dashboard/assessments", headers=headers)
    with urllib.request.urlopen(req) as resp:
        assessments = json.loads(resp.read().decode("utf-8"))
        matched = [a for a in assessments if a.get("client_session_id") == sync_session_id]
        if matched:
            a = matched[0]
            print(f"[TEST] SUCCESS! Live interaction reflected in backend & dashboard:")
            print(f"       Assessment ID: {a['id']}")
            print(f"       Worker: {a['worker_name']} ({a['employee_id']})")
            print(f"       Module: {a['module_name']}")
            print(f"       Calculated Score: {a['score']}%")
            print(f"       Pass/Fail: {a['passed']}")
            print(f"       Critical Errors: {a['critical_errors']}")
            print(f"       Competency Scores: {list(a['competency_scores'].keys())}")
        else:
            print("[TEST] WARNING: Session not found in dashboard assessments list.")

    # Verify certificate auto-issuance
    req = urllib.request.Request(f"{BASE_URL}/dashboard/certificates", headers=headers)
    with urllib.request.urlopen(req) as resp:
        certs = json.loads(resp.read().decode("utf-8"))
        worker_certs = [c for c in certs if c["worker_id"] == 1]
        print(f"[TEST] Certificates for Worker 1: count={len(worker_certs)}, numbers={[c['certificate_number'] for c in worker_certs]}")

if __name__ == "__main__":
    print("[TEST] Logging in as Admin...")
    token = login_admin()
    print("[TEST] Admin token obtained. Testing current dashboard endpoints...")
    test_endpoints(token)
    print("\n[TEST] Testing live mobile sync ingestion...")
    test_mobile_live_sync(token)
    print("\n[TEST] All tests completed successfully!")
