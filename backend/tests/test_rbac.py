"""Day 4 - authorization / RBAC tests (worker role, admin role, ownership)."""

from events import GOOD_FIRE_EVENTS


def _create_other_worker(client, employee_id):
    return client.post(
        "/api/v1/workers",
        json={
            "name": "Other Worker",
            "employee_id": employee_id,
            "role": "Fire Safety Worker",
        },
    ).json()


# ---------------------------------------------------------------------------
# Worker must NOT receive admin-only dashboard functionality.
# ---------------------------------------------------------------------------
def test_worker_cannot_access_dashboard_summary(worker_client):
    wc, _ = worker_client
    assert wc.get("/api/v1/dashboard/summary").status_code == 403


def test_worker_cannot_access_dashboard_workers(worker_client):
    wc, _ = worker_client
    assert wc.get("/api/v1/dashboard/workers").status_code == 403


def test_worker_cannot_access_dashboard_worker_detail(worker_client):
    wc, worker = worker_client
    assert wc.get(f"/api/v1/dashboard/workers/{worker['id']}").status_code == 403


def test_worker_cannot_query_event_telemetry(worker_client):
    wc, _ = worker_client
    assert wc.get("/api/v1/events").status_code == 403
    assert wc.get("/api/v1/events/stats/summary").status_code == 403


# ---------------------------------------------------------------------------
# Users must NOT access another worker's protected data via worker_id tampering.
# ---------------------------------------------------------------------------
def test_worker_cannot_read_other_worker(client, worker_client):
    wc, _ = worker_client
    other = _create_other_worker(client, "EMP-OTHER-1")
    assert wc.get(f"/api/v1/workers/{other['id']}").status_code == 403


def test_worker_cannot_read_other_worker_progress(client, worker_client):
    wc, _ = worker_client
    other = _create_other_worker(client, "EMP-OTHER-2")
    assert wc.get(f"/api/v1/progress/{other['id']}").status_code == 403


def test_worker_cannot_read_other_worker_progress_nested(client, worker_client):
    wc, _ = worker_client
    other = _create_other_worker(client, "EMP-OTHER-2B")
    assert wc.get(f"/api/v1/workers/{other['id']}/progress").status_code == 403


def test_worker_cannot_read_other_worker_retention(client, worker_client):
    wc, _ = worker_client
    other = _create_other_worker(client, "EMP-OTHER-2C")
    assert wc.get(f"/api/v1/progress/{other['id']}/retention").status_code == 403


def test_worker_cannot_read_other_worker_assessments(client, worker_client):
    wc, _ = worker_client
    other = _create_other_worker(client, "EMP-OTHER-3")
    assert wc.get(f"/api/v1/assessments/{other['id']}").status_code == 403


def test_worker_cannot_read_other_worker_latest_assessment(client, worker_client):
    wc, _ = worker_client
    other = _create_other_worker(client, "EMP-OTHER-3B")
    assert wc.get(f"/api/v1/assessments/{other['id']}/latest").status_code == 403


def test_worker_cannot_read_other_worker_sync_status(client, worker_client):
    wc, _ = worker_client
    other = _create_other_worker(client, "EMP-OTHER-4")
    assert wc.get(f"/api/v1/sync/status/{other['id']}").status_code == 403


def test_worker_cannot_read_other_worker_certificates(client, worker_client):
    wc, _ = worker_client
    other = _create_other_worker(client, "EMP-OTHER-5")
    assert wc.get(f"/api/v1/certificates/{other['id']}").status_code == 403


def test_worker_cannot_submit_assessment_for_other_worker(client, worker_client):
    wc, _ = worker_client
    other = _create_other_worker(client, "EMP-OTHER-6")
    response = wc.post(
        "/api/v1/assessments",
        json={"worker_id": other["id"], "module_id": 1, "events": GOOD_FIRE_EVENTS},
    )
    assert response.status_code == 403


def test_worker_cannot_update_progress_for_other_worker(client, worker_client):
    wc, _ = worker_client
    other = _create_other_worker(client, "EMP-OTHER-6B")
    response = wc.post(
        "/api/v1/progress",
        json={
            "worker_id": other["id"],
            "module_id": 1,
            "stage": "practice",
            "status": "completed",
        },
    )
    assert response.status_code == 403


def test_worker_cannot_sync_for_other_worker(client, worker_client):
    wc, _ = worker_client
    other = _create_other_worker(client, "EMP-OTHER-7")
    response = wc.post(
        "/api/v1/sync",
        json={
            "worker_id": other["id"],
            "device_id": "dev-1",
            "sessions": [
                {
                    "type": "assessment",
                    "module_id": 1,
                    "occurred_at": "2026-09-01T10:00:00Z",
                }
            ],
        },
    )
    assert response.status_code == 403


def test_worker_cannot_post_events_for_other_worker(client, worker_client):
    wc, _ = worker_client
    other = _create_other_worker(client, "EMP-OTHER-8")
    response = wc.post(
        "/api/v1/events",
        json={
            "worker_id": other["id"],
            "session_id": "sess-cross",
            "event_type": "hazard_identified",
            "severity": "info",
        },
    )
    assert response.status_code == 403


# ---------------------------------------------------------------------------
# Admin-only actions must be blocked for workers.
# ---------------------------------------------------------------------------
def test_worker_cannot_create_worker(worker_client):
    wc, _ = worker_client
    response = wc.post(
        "/api/v1/workers",
        json={
            "name": "Hacker",
            "employee_id": "EMP-HACK-1",
            "role": "Fire Safety Worker",
        },
    )
    assert response.status_code == 403


def test_worker_cannot_issue_certificate(worker_client):
    wc, worker = worker_client
    response = wc.post(
        "/api/v1/certificates", json={"worker_id": worker["id"], "module_id": 1}
    )
    assert response.status_code == 403


# ---------------------------------------------------------------------------
# Positive cases: worker owns their data, admin owns everything.
# ---------------------------------------------------------------------------
def test_worker_can_access_own_data(worker_client):
    wc, worker = worker_client
    assert wc.get("/api/v1/modules").status_code == 200
    assert wc.get(f"/api/v1/workers/{worker['id']}").status_code == 200
    assert wc.get(f"/api/v1/progress/{worker['id']}").status_code == 200
    assert wc.get(f"/api/v1/assessments/{worker['id']}").status_code == 200
    assert wc.get(f"/api/v1/sync/status/{worker['id']}").status_code == 200
    assert wc.get(f"/api/v1/certificates/{worker['id']}").status_code == 200


def test_worker_can_submit_own_assessment(worker_client):
    wc, worker = worker_client
    response = wc.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": GOOD_FIRE_EVENTS},
    )
    assert response.status_code == 201


def test_worker_can_sync_own_data(worker_client):
    wc, worker = worker_client
    response = wc.post(
        "/api/v1/sync",
        json={
            "worker_id": worker["id"],
            "device_id": "dev-self",
            "sessions": [
                {
                    "type": "assessment",
                    "module_id": 1,
                    "occurred_at": "2026-09-01T10:00:00Z",
                    "events": GOOD_FIRE_EVENTS,
                }
            ],
        },
    )
    assert response.status_code == 201
    assert response.json()["assessments_created"] == 1


def test_worker_can_post_events_for_self(worker_client):
    wc, worker = worker_client
    response = wc.post(
        "/api/v1/events",
        json={
            "worker_id": worker["id"],
            "session_id": "sess-self",
            "event_type": "hazard_identified",
            "severity": "info",
        },
    )
    assert response.status_code == 201


def test_admin_can_access_dashboard_and_all_workers(client):
    assert client.get("/api/v1/dashboard/summary").status_code == 200
    assert client.get("/api/v1/dashboard/workers").status_code == 200
    assert client.get("/api/v1/modules").status_code == 200
    assert client.get("/api/v1/workers/999").status_code == 404  # admin sees 404, not 403