"""Tests for sync endpoints (offline assessment scoring)."""

from events import BAD_FIRE_EVENTS, GOOD_FIRE_EVENTS, GOOD_GAS_EVENTS


def _create_worker(client):
    return client.post(
        "/api/v1/workers",
        json={"name": "Ramesh Kumar", "employee_id": "EMP001", "role": "Fire Safety Worker"},
    ).json()


def test_sync_sessions(client):
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/sync",
        json={
            "worker_id": worker["id"],
            "device_id": "device-abc-123",
            "sessions": [
                {
                    "type": "assessment",
                    "module_id": 1,
                    "score": 80.0,
                    "passed": False,
                    "weaknesses": ["wrong_evacuation_route"],
                    "occurred_at": "2026-09-01T10:00:00Z",
                }
            ],
        },
    )
    assert response.status_code == 201
    data = response.json()
    assert data["sync_id"] == 1
    assert data["worker_id"] == worker["id"]
    assert data["sessions_synced"] == 1
    assert data["assessments_created"] == 0  # legacy log-only session
    assert "synced_at" in data


def test_sync_sessions_worker_not_found(client):
    response = client.post(
        "/api/v1/sync",
        json={
            "worker_id": 999,
            "device_id": "device-abc-123",
            "sessions": [],
        },
    )
    assert response.status_code == 404
    assert response.json() == {"detail": "Worker not found"}


def test_sync_status_empty(client):
    worker = _create_worker(client)
    response = client.get(f"/api/v1/sync/status/{worker['id']}")
    assert response.status_code == 200
    data = response.json()
    assert data["worker_id"] == worker["id"]
    assert data["last_synced_at"] is None
    assert data["pending_sessions"] == 0


def test_sync_status_after_sync(client):
    worker = _create_worker(client)
    client.post(
        "/api/v1/sync",
        json={
            "worker_id": worker["id"],
            "device_id": "device-abc-123",
            "sessions": [
                {
                    "type": "assessment",
                    "module_id": 1,
                    "score": 80.0,
                    "passed": False,
                    "weaknesses": [],
                    "occurred_at": "2026-09-01T10:00:00Z",
                }
            ],
        },
    )
    response = client.get(f"/api/v1/sync/status/{worker['id']}")
    assert response.status_code == 200
    data = response.json()
    assert data["last_synced_at"] is not None
    assert data["pending_sessions"] == 0


def test_sync_status_worker_not_found(client):
    response = client.get("/api/v1/sync/status/999")
    assert response.status_code == 404
    assert response.json() == {"detail": "Worker not found"}


def test_sync_assessment_events_scores_server_side(client):
    """Offline assessment sessions with events are scored by the competency engine."""
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/sync",
        json={
            "worker_id": worker["id"],
            "device_id": "device-abc-123",
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
    data = response.json()
    assert data["sessions_synced"] == 1
    assert data["assessments_created"] == 1

    latest = client.get(f"/api/v1/assessments/{worker['id']}/latest").json()
    assert latest["scenario_type"] == "fire"
    assert latest["passed"] is True
    assert latest["score"] == 90.0


def test_sync_multiple_event_sessions(client):
    """Event sessions get sequential attempt numbers per module."""
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/sync",
        json={
            "worker_id": worker["id"],
            "device_id": "device-abc-123",
            "sessions": [
                {
                    "type": "assessment",
                    "module_id": 1,
                    "occurred_at": "2026-09-01T10:00:00Z",
                    "events": BAD_FIRE_EVENTS,
                },
                {
                    "type": "assessment",
                    "module_id": 1,
                    "occurred_at": "2026-09-01T11:00:00Z",
                    "events": GOOD_FIRE_EVENTS,
                },
                {
                    "type": "assessment",
                    "module_id": 2,
                    "occurred_at": "2026-09-01T12:00:00Z",
                    "events": GOOD_GAS_EVENTS,
                },
            ],
        },
    )
    assert response.status_code == 201
    data = response.json()
    assert data["assessments_created"] == 3

    history = client.get(f"/api/v1/assessments/{worker['id']}").json()["assessments"]
    fire_attempts = sorted(
        a["attempt_number"] for a in history if a["module_id"] == 1
    )
    assert fire_attempts == [1, 2]
    gas = next(a for a in history if a["module_id"] == 2)
    assert gas["attempt_number"] == 1
    assert gas["scenario_type"] == "gas"
    assert gas["passed"] is True


def test_sync_assessment_events_module_not_found(client):
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/sync",
        json={
            "worker_id": worker["id"],
            "device_id": "device-abc-123",
            "sessions": [
                {
                    "type": "assessment",
                    "module_id": 999,
                    "occurred_at": "2026-09-01T10:00:00Z",
                    "events": GOOD_FIRE_EVENTS,
                }
            ],
        },
    )
    assert response.status_code == 404
    assert response.json() == {
        "detail": "Module not found for synced session (module_id=999)"
    }