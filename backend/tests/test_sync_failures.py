"""Day 4 - sync failure handling, safe retry and status reporting tests."""

from conftest import TestingSessionLocal

from app.models.sync_log import SyncLog
from events import GOOD_FIRE_EVENTS


def _create_worker(client):
    return client.post(
        "/api/v1/workers",
        json={
            "name": "Ramesh Kumar",
            "employee_id": "EMP-SYNC-F",
            "role": "Fire Safety Worker",
        },
    ).json()


def _sync_log_count():
    db = TestingSessionLocal()
    try:
        return db.query(SyncLog).count()
    finally:
        db.close()


def test_sync_failed_batch_rolls_back_atomically(client):
    """A failing session aborts the whole batch: no partial assessments, no logs."""
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/sync",
        json={
            "worker_id": worker["id"],
            "device_id": "device-fail-1",
            "sessions": [
                {
                    "type": "assessment",
                    "module_id": 1,
                    "occurred_at": "2026-09-01T10:00:00Z",
                    "events": GOOD_FIRE_EVENTS,
                },
                {
                    "type": "assessment",
                    "module_id": 999,
                    "occurred_at": "2026-09-01T11:00:00Z",
                    "events": GOOD_FIRE_EVENTS,
                },
            ],
        },
    )
    assert response.status_code == 404  # bad module aborts the whole batch
    history = client.get(f"/api/v1/assessments/{worker['id']}").json()["assessments"]
    assert history == []  # nothing partially persisted
    assert _sync_log_count() == 0  # no sync log; the client can safely retry


def test_sync_failure_then_retry_succeeds(client):
    worker = _create_worker(client)
    bad = {
        "worker_id": worker["id"],
        "device_id": "device-retry-1",
        "sessions": [
            {
                "type": "assessment",
                "module_id": 999,
                "occurred_at": "2026-09-01T10:00:00Z",
                "events": GOOD_FIRE_EVENTS,
            }
        ],
    }
    assert client.post("/api/v1/sync", json=bad).status_code == 404

    good = {
        "worker_id": worker["id"],
        "device_id": "device-retry-1",
        "sessions": [
            {
                "type": "assessment",
                "module_id": 1,
                "occurred_at": "2026-09-01T10:00:00Z",
                "events": GOOD_FIRE_EVENTS,
            }
        ],
    }
    ok = client.post("/api/v1/sync", json=good)
    assert ok.status_code == 201
    assert ok.json()["assessments_created"] == 1
    history = client.get(f"/api/v1/assessments/{worker['id']}").json()["assessments"]
    assert len(history) == 1


def test_sync_status_pending_sessions_reported(client):
    worker = _create_worker(client)
    ok = client.post(
        "/api/v1/sync",
        json={
            "worker_id": worker["id"],
            "device_id": "device-pend-1",
            "pending_sessions": 4,
            "sessions": [
                {
                    "type": "assessment",
                    "module_id": 1,
                    "occurred_at": "2026-09-01T10:00:00Z",
                }
            ],
        },
    )
    assert ok.status_code == 201
    status = client.get(f"/api/v1/sync/status/{worker['id']}").json()
    assert status["last_synced_at"] is not None
    assert status["pending_sessions"] == 4


def test_sync_status_defaults_to_zero(client):
    worker = _create_worker(client)
    status = client.get(f"/api/v1/sync/status/{worker['id']}").json()
    assert status["last_synced_at"] is None
    assert status["pending_sessions"] == 0