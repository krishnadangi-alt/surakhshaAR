"""Day 4 - input validation and server-side score authority tests."""

from events import GOOD_FIRE_EVENTS


def _create_worker(client, employee_id="EMP-VAL-1"):
    return client.post(
        "/api/v1/workers",
        json={
            "name": "Ramesh Kumar",
            "employee_id": employee_id,
            "role": "Fire Safety Worker",
        },
    ).json()


# ---------------------------------------------------------------------------
# Server-side score authority: the client must never dictate score / result.
# ---------------------------------------------------------------------------
def test_manipulated_score_and_result_ignored(client):
    """Client-supplied score/passed/competency values never override scoring."""
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/assessments",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "events": GOOD_FIRE_EVENTS,
            "score": 0,
            "passed": False,
            "competency_scores": {"hazard_identification": {"score": 0, "passed": False}},
        },
    )
    assert response.status_code == 201
    data = response.json()
    assert data["score"] == 90.0  # server-computed
    assert data["passed"] is True  # server-computed
    assert data["competency_scores"]["hazard_identification"]["score"] == 100.0


def test_sync_legacy_passed_claim_creates_no_assessment(client):
    """A legacy sync session claiming a pass creates no assessment record."""
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/sync",
        json={
            "worker_id": worker["id"],
            "device_id": "dev-v-1",
            "sessions": [
                {
                    "type": "assessment",
                    "module_id": 1,
                    "score": 99.0,
                    "passed": True,
                    "occurred_at": "2026-09-01T10:00:00Z",
                }
            ],
        },
    )
    assert response.status_code == 201
    assert response.json()["assessments_created"] == 0
    history = client.get(f"/api/v1/assessments/{worker['id']}").json()["assessments"]
    assert history == []


# ---------------------------------------------------------------------------
# Reject malformed / inconsistent payloads with 422.
# ---------------------------------------------------------------------------
def test_sync_score_out_of_range_422(client):
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/sync",
        json={
            "worker_id": worker["id"],
            "device_id": "dev-v-2",
            "sessions": [
                {
                    "type": "assessment",
                    "module_id": 1,
                    "score": 101.0,
                    "occurred_at": "2026-09-01T10:00:00Z",
                }
            ],
        },
    )
    assert response.status_code == 422


def test_sync_empty_device_id_422(client):
    response = client.post(
        "/api/v1/sync", json={"worker_id": 999, "device_id": "", "sessions": []}
    )
    assert response.status_code == 422


def test_assessment_wrong_action_invalid_severity_422(client):
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/assessments",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "events": [{"event_type": "wrong_action", "severity": "extreme"}],
        },
    )
    assert response.status_code == 422


def test_assessment_worker_id_zero_422(client):
    response = client.post(
        "/api/v1/assessments",
        json={"worker_id": 0, "module_id": 1, "events": GOOD_FIRE_EVENTS},
    )
    assert response.status_code == 422


def test_assessment_attempt_number_zero_422(client):
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/assessments",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "attempt_number": 0,
            "events": GOOD_FIRE_EVENTS,
        },
    )
    assert response.status_code == 422


def test_assessment_missing_events_422(client):
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": []},
    )
    assert response.status_code == 422


def test_assessment_unknown_scenario_422(client):
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/assessments",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "scenario_type": "earthquake",
            "events": GOOD_FIRE_EVENTS,
        },
    )
    assert response.status_code == 422


def test_verify_certificate_malformed_number_422(client):
    response = client.get("/api/v1/certificates/verify/NOT-A-CERT")
    assert response.status_code == 422


def test_create_worker_short_name_422(client):
    response = client.post(
        "/api/v1/workers",
        json={
            "name": "X",
            "employee_id": "EMP-VAL-2",
            "role": "Fire Safety Worker",
        },
    )
    assert response.status_code == 422


def test_create_worker_username_without_password_422(client):
    response = client.post(
        "/api/v1/workers",
        json={
            "name": "Worker X",
            "employee_id": "EMP-VAL-3",
            "role": "Fire Safety Worker",
            "username": "workerx",
        },
    )
    assert response.status_code == 422


def test_event_invalid_severity_422(client):
    response = client.post(
        "/api/v1/events",
        json={
            "session_id": "sess-invalid-sev",
            "event_type": "hazard_identified",
            "severity": "extreme",
        },
    )
    assert response.status_code == 422