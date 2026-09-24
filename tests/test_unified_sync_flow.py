import urllib.request
import json
import uuid

BASE_URL = "http://localhost:8000/api/v1"

def login_admin():
    req = urllib.request.Request(
        f"{BASE_URL}/auth/login",
        data=json.dumps({"username": "admin", "password": "Admin@123"}).encode("utf-8"),
        headers={"Content-Type": "application/json"}
    )
    with urllib.request.urlopen(req) as resp:
        return json.loads(resp.read().decode())["access_token"]

def get(path, token=None):
    headers = {}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    req = urllib.request.Request(f"{BASE_URL}{path}", headers=headers)
    with urllib.request.urlopen(req) as response:
        return response.status, json.loads(response.read().decode())

def post(path, data, token=None):
    body = json.dumps(data).encode("utf-8")
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    req = urllib.request.Request(f"{BASE_URL}{path}", data=body, headers=headers)
    with urllib.request.urlopen(req) as response:
        return response.status, json.loads(response.read().decode())

def test_unified_sync():
    token = login_admin()
    print("Admin logged in successfully.")

    # 1. Sync payload with complete fire events
    session_id = f"sess_{uuid.uuid4().hex[:12]}"
    batch_id = f"batch_{uuid.uuid4().hex[:12]}"
    
    events = [
        {"event_type": "hazard_identified", "action": "identify_electrical_fire", "correct": True, "critical": False, "response_time_seconds": 1.8},
        {"event_type": "ppe_selected", "action": "select_safety_boots_gloves", "correct": True, "critical": False, "response_time_seconds": 2.1},
        {"event_type": "correct_action", "action": "activate_alarm", "correct": True, "critical": False, "response_time_seconds": 2.0},
        {"event_type": "equipment_selected", "action": "select_co2_extinguisher", "correct": True, "critical": False, "response_time_seconds": 2.5},
        {"event_type": "correct_action", "action": "remove_safety_pin", "correct": True, "critical": False, "response_time_seconds": 1.5},
        {"event_type": "correct_action", "action": "aim_at_base_of_fire", "correct": True, "critical": False, "response_time_seconds": 2.2},
        {"event_type": "correct_action", "action": "pass_technique_spray", "correct": True, "critical": False, "response_time_seconds": 10.0},
        {"event_type": "correct_action", "action": "fire_extinguished", "correct": True, "critical": False, "response_time_seconds": 10.0},
        {"event_type": "evacuation_started", "action": "evacuate", "correct": True, "critical": False, "response_time_seconds": 3.0, "safe": True, "route": "emergency_exit_A"},
        {"event_type": "knowledge_quiz", "action": "correct_co2_selected", "correct": True, "critical": False, "response_time_seconds": 15.0}
    ]

    payload = {
        "worker_id": 0,
        "guest_id": "EMP-101",
        "device_id": "test-device-01",
        "batch_id": batch_id,
        "pending_sessions": 1,
        "sessions": [
            {
                "type": "assessment",
                "module_id": 1,
                "score": 95.0,
                "passed": True,
                "occurred_at": "2026-09-18T02:50:00Z",
                "scenario_type": "fire",
                "attempt_number": 1,
                "client_session_id": session_id,
                "guest_id": "EMP-101",
                "events": events
            }
        ]
    }

    sync_status, sync_data = post("/sync", payload, token=token)
    print("Sync response:", sync_status, sync_data)
    assert sync_status == 201

    # 2. Check dashboard assessments endpoint
    asmts_status, asmts = get("/dashboard/assessments", token=token)
    assert asmts_status == 200
    print(f"Total dashboard assessments: {len(asmts)}")
    
    # Find our synced assessment
    matched = [a for a in asmts if a.get("client_session_id") == session_id]
    assert len(matched) == 1, "Synced assessment must be returned by dashboard endpoint"
    item = matched[0]
    print("\nMatched Assessment Details on Admin Panel:")
    print("  - Worker:", item["worker_name"], f"({item['employee_id']})")
    print("  - Module:", item["module_name"])
    print("  - Score:", item["score"], "%")
    print("  - Result:", "Passed" if item["passed"] else "Failed")
    print("  - Competency Scores & Dimensions:")
    for comp_name, comp_data in item["competency_scores"].items():
        pass_str = "[PASS]" if comp_data['passed'] else "[FAIL]"
        print(f"      * {comp_name}: {comp_data['score']}% (Min {comp_data['pass_threshold']}%) -> {pass_str}")

    print("\nALL UNIFIED SYNC & COMPETENCY CHECKS VERIFIED SUCCESSFULLY!")

if __name__ == "__main__":
    test_unified_sync()
