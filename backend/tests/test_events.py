"""Tests for real-time and batch behavioural event ingestion."""

import pytest


def _create_worker(client):
    return client.post(
        "/api/v1/workers",
        json={"name": "Ramesh Kumar", "employee_id": "JH-MN-004821", "role": "Equipment Operator"},
    ).json()


def test_ingest_single_event(client):
    worker = _create_worker(client)
    payload = {
        "worker_id": worker["id"],
        "session_id": "sess_test_123",
        "module_id": 1,
        "scenario_type": "fire",
        "event_type": "hazard_identified",
        "severity": "info",
        "payload": {"object_id": "electrical_panel_hazard", "time_to_identify": 4.5},
    }
    response = client.post("/api/v1/events", json=payload)
    assert response.status_code == 201
    data = response.json()
    assert data["received"] == 1
    assert data["stored"] == 1
    assert data["session_id"] == "sess_test_123"
    assert data["status"] == "success"


def test_ingest_batch_events(client):
    worker = _create_worker(client)
    payload = {
        "worker_id": worker["id"],
        "session_id": "sess_batch_999",
        "device_id": "meta_quest_ar_01",
        "events": [
            {
                "event_type": "training_started",
                "severity": "info",
                "payload": {"mode": "ar_interactive"},
            },
            {
                "event_type": "hazard_identified",
                "severity": "info",
                "payload": {"hazard": "flammable_liquid"},
            },
            {
                "event_type": "pin_removed",
                "severity": "info",
                "payload": {"tool": "co2_extinguisher"},
            },
            {
                "event_type": "critical_action",
                "severity": "critical",
                "payload": {"action": "water_on_electrical", "correct": False},
            },
        ],
    }
    response = client.post("/api/v1/events", json=payload)
    assert response.status_code == 201
    data = response.json()
    assert data["received"] == 4
    assert data["stored"] == 4
    assert data["session_id"] == "sess_batch_999"


def test_query_events(client):
    worker = _create_worker(client)
    session_id = "sess_query_demo"
    client.post(
        "/api/v1/events",
        json={
            "worker_id": worker["id"],
            "session_id": session_id,
            "event_type": "spray_started",
            "severity": "info",
            "payload": {"duration_sec": 10.0},
        },
    )

    response = client.get(f"/api/v1/events?session_id={session_id}")
    assert response.status_code == 200
    events = response.json()
    assert len(events) >= 1
    assert events[0]["event_type"] == "spray_started"
    assert events[0]["session_id"] == session_id
    assert events[0]["payload"]["duration_sec"] == 10.0


def test_get_session_timeline(client):
    worker = _create_worker(client)
    session_id = "sess_timeline_456"
    client.post(
        "/api/v1/events",
        json={
            "worker_id": worker["id"],
            "session_id": session_id,
            "events": [
                {"event_type": "hazard_identified"},
                {"event_type": "extinguisher_picked"},
                {"event_type": "fire_extinguished"},
            ],
        },
    )

    response = client.get(f"/api/v1/events/session/{session_id}")
    assert response.status_code == 200
    timeline = response.json()
    assert len(timeline) == 3
    assert timeline[0]["event_type"] == "hazard_identified"
    assert timeline[1]["event_type"] == "extinguisher_picked"
    assert timeline[2]["event_type"] == "fire_extinguished"


def test_event_stats_summary(client):
    worker = _create_worker(client)
    client.post(
        "/api/v1/events",
        json={
            "worker_id": worker["id"],
            "session_id": "sess_stats_test",
            "events": [
                {"event_type": "hazard_identified", "severity": "info"},
                {"event_type": "wrong_action", "severity": "major"},
                {"event_type": "critical_action", "severity": "critical"},
            ],
        },
    )

    response = client.get("/api/v1/events/stats/summary")
    assert response.status_code == 200
    stats = response.json()
    assert stats["total_events"] >= 3
    assert stats["critical_events_count"] >= 1
    assert stats["hazards_identified_count"] >= 1
    assert "hazard_identified" in stats["events_by_type"]


def test_websocket_live_feed_requires_admin_token(client, anonymous_client):
    """The admin-only live feed refuses anonymous and non-admin handshakes."""
    from fastapi.testclient import TestClient

    from conftest import _auth_headers

    try:
        with anonymous_client.websocket_connect("/api/v1/events/live"):
            refused = False
    except Exception:
        refused = True
    assert refused is True

    worker_login = client.post(
        "/api/v1/workers",
        json={
            "name": "WS Worker",
            "employee_id": "EMP-WS-1",
            "role": "Fire Safety Worker",
            "username": "ws_worker1",
            "password": "workerpass123",
        },
    )
    assert worker_login.status_code == 201
    worker_token = client.post(
        "/api/v1/auth/login",
        json={"username": "ws_worker1", "password": "workerpass123"},
    ).json()["access_token"]
    try:
        with TestClient(
            anonymous_client.app, headers=_auth_headers(worker_token)
        ).websocket_connect("/api/v1/events/live"):
            refused_worker = False
    except Exception:
        refused_worker = True
    assert refused_worker is True


def test_websocket_live_feed(client):
    worker = _create_worker(client)
    from fastapi.testclient import TestClient

    from app.auth.security import create_access_token
    from conftest import _auth_headers

    admin_token = create_access_token("admin", "admin")
    admin_client = TestClient(client.app, headers=_auth_headers(admin_token))
    with admin_client.websocket_connect(
        f"/api/v1/events/live?token={admin_token}"
    ) as websocket:
        # Ingest an event while the websocket client is connected
        client.post(
            "/api/v1/events",
            json={
                "worker_id": worker["id"],
                "session_id": "sess_ws_test",
                "event_type": "hazard_identified",
                "severity": "info",
                "payload": {"hazard": "high_voltage_spark"},
            },
        )
        # Verify websocket client receives the broadcasted live event
        msg = websocket.receive_json()
        assert msg["event_type"] == "hazard_identified"
        assert msg["session_id"] == "sess_ws_test"
        assert msg["payload"]["hazard"] == "high_voltage_spark"
