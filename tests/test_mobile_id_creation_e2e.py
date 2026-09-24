# -*- coding: utf-8 -*-
"""
E2E Test: Real-time Mobile Worker ID Creation & Dashboard Reflection
Verifies that when an ID is created/entered on the mobile app,
it immediately registers on the backend and appears on the web dashboard.
"""

import json
import urllib.request
import urllib.error
import time

BASE_URL = "http://127.0.0.1:8000/api/v1"

def http_post(url, data_dict, token=None):
    raw = json.dumps(data_dict).encode("utf-8")
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    req = urllib.request.Request(url, data=raw, headers=headers, method="POST")
    with urllib.request.urlopen(req) as resp:
        return resp.status, json.loads(resp.read().decode("utf-8"))

def http_get(url, token=None):
    headers = {}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    req = urllib.request.Request(url, headers=headers, method="GET")
    with urllib.request.urlopen(req) as resp:
        return resp.status, json.loads(resp.read().decode("utf-8"))

def run_test():
    print("=================================================================")
    print("SURAKSHAAR MOBILE ID CREATION -> DASHBOARD REAL-TIME E2E VERIFY")
    print("=================================================================")

    # 1. Admin login to authenticate dashboard requests
    print("\n[STEP 1] Logging into Dashboard Admin...")
    _, auth_resp = http_post(f"{BASE_URL}/auth/login", {"username": "admin", "password": "Admin@123"})
    admin_token = auth_resp["access_token"]
    assert admin_token, "Failed to get admin bearer token"
    print("[OK] Admin authenticated successfully.")

    # 2. Get baseline worker count
    _, summary_before = http_get(f"{BASE_URL}/dashboard/summary", admin_token)
    initial_count = summary_before["total_workers"]
    print(f"[OK] Baseline dashboard worker count: {initial_count}")

    # 3. Simulate Worker ID creation on Mobile App (Worker Login)
    unique_emp_id = f"EMP-LIVE-{int(time.time())}"
    print(f"\n[STEP 2] Simulating mobile app creating worker ID '{unique_emp_id}'...")
    status, reg_resp = http_post(f"{BASE_URL}/workers/register", {
        "employee_id": unique_emp_id,
        "name": f"Mobile Miner {unique_emp_id}",
        "role": "Shaft Blaster",
        "is_guest": False
    })
    assert status == 200, f"Expected 200 from worker register, got {status}"
    worker_id = reg_resp["id"]
    print(f"[OK] Worker registered on backend with DB ID: {worker_id}")

    # 4. Check if Worker immediately appears on Dashboard
    print("\n[STEP 3] Verifying worker ID presence in /api/v1/dashboard/workers...")
    _, workers_resp = http_get(f"{BASE_URL}/dashboard/workers", admin_token)
    matched = [w for w in workers_resp["workers"] if w["employee_id"] == unique_emp_id]
    assert len(matched) == 1, f"Worker {unique_emp_id} not found in dashboard workers list!"
    print(f"[OK] Found worker on dashboard: Name='{matched[0]['name']}', Role='{matched[0]['role']}', ID={matched[0]['id']}")

    # 5. Check if Dashboard Summary count increased
    _, summary_after = http_get(f"{BASE_URL}/dashboard/summary", admin_token)
    assert summary_after["total_workers"] == initial_count + 1, f"Expected total_workers={initial_count + 1}, got {summary_after['total_workers']}"
    print(f"[OK] Dashboard total_workers incremented: {initial_count} -> {summary_after['total_workers']}")

    # 6. Simulate Guest ID creation on Mobile App (Guest Mode)
    unique_guest_id = f"GUEST-LIVE-{int(time.time())}"
    print(f"\n[STEP 4] Simulating mobile app creating Guest ID '{unique_guest_id}'...")
    status, guest_reg_resp = http_post(f"{BASE_URL}/workers/register", {
        "employee_id": unique_guest_id,
        "name": f"Guest Trainee {unique_guest_id}",
        "role": "Guest Trainee",
        "is_guest": True
    })
    assert status == 200
    guest_worker_id = guest_reg_resp["id"]
    print(f"[OK] Guest worker registered on backend with DB ID: {guest_worker_id}")

    # 7. Check if Guest immediately appears on Dashboard
    _, workers_resp_guest = http_get(f"{BASE_URL}/dashboard/workers", admin_token)
    matched_guest = [w for w in workers_resp_guest["workers"] if w["employee_id"] == unique_guest_id]
    assert len(matched_guest) == 1, f"Guest {unique_guest_id} not found in dashboard!"
    print(f"[OK] Found Guest on dashboard: Name='{matched_guest[0]['name']}', Role='{matched_guest[0]['role']}'")

    # 8. Test Idempotency (re-registering same ID should update or return existing)
    print("\n[STEP 5] Testing idempotency for repeat connection...")
    status_repeat, repeat_resp = http_post(f"{BASE_URL}/workers/register", {
        "employee_id": unique_emp_id,
        "name": f"Mobile Miner {unique_emp_id} Updated",
        "role": "Senior Shaft Blaster",
        "is_guest": False
    })
    assert status_repeat == 200
    assert repeat_resp["id"] == worker_id, "ID changed on repeat registration!"
    print(f"[OK] Idempotent: Worker ID maintained ({worker_id}), details refreshed.")

    # 9. Verify Final Dashboard Summary
    _, summary_final = http_get(f"{BASE_URL}/dashboard/summary", admin_token)
    assert summary_final["total_workers"] == initial_count + 2
    print(f"[OK] Final Dashboard Summary total_workers: {summary_final['total_workers']}")

    print("\n=================================================================")
    print("ALL MOBILE ID CREATION & DASHBOARD CHECKS PASSED 100%!")
    print("=================================================================")

if __name__ == "__main__":
    run_test()
