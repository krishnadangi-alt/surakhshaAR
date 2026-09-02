"""Tests for assessment endpoints (ML competency engine integration)."""

from events import (
    BAD_FIRE_EVENTS,
    CRITICAL_FIRE_EVENTS,
    GOOD_FIRE_EVENTS,
    GOOD_GAS_EVENTS,
)


def _create_worker(client):
    return client.post(
        "/api/v1/workers",
        json={"name": "Ramesh Kumar", "employee_id": "EMP001", "role": "Fire Safety Worker"},
    ).json()


def test_submit_assessment_scores_events(client):
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": GOOD_FIRE_EVENTS},
    )
    assert response.status_code == 201
    data = response.json()
    assert data["id"] == 1
    assert data["worker_id"] == worker["id"]
    assert data["module_id"] == 1
    assert data["attempt_number"] == 1  # auto-assigned
    assert data["scenario_type"] == "fire"
    assert data["passed"] is True
    assert data["score"] == 90.0  # 4 x 100 (event-driven) + 50 (decision_making baseline)
    assert data["critical_errors"] == []
    assert data["weaknesses"] == []
    assert "created_at" in data

    scores = data["competency_scores"]
    assert scores["hazard_identification"]["score"] == 100.0
    assert scores["ppe_selection"]["score"] == 100.0
    assert scores["procedure_compliance"]["score"] == 100.0
    assert scores["equipment_use"]["score"] == 100.0
    assert scores["decision_making"]["score"] == 50.0
    assert scores["ppe_selection"]["passed"] is True
    assert scores["ppe_selection"]["pass_threshold"] == 80.0


def test_submit_assessment_auto_increments_attempts(client):
    worker = _create_worker(client)
    body = {"worker_id": worker["id"], "module_id": 1, "events": GOOD_FIRE_EVENTS}
    first = client.post("/api/v1/assessments", json=body).json()
    second = client.post("/api/v1/assessments", json=body).json()
    assert first["attempt_number"] == 1
    assert second["attempt_number"] == 2


def test_submit_assessment_explicit_attempt_number(client):
    worker = _create_worker(client)
    data = client.post(
        "/api/v1/assessments",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "attempt_number": 7,
            "events": GOOD_FIRE_EVENTS,
        },
    ).json()
    assert data["attempt_number"] == 7


def test_submit_failing_assessment_detects_weaknesses(client):
    worker = _create_worker(client)
    data = client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": BAD_FIRE_EVENTS},
    ).json()
    assert data["passed"] is False
    assert data["score"] == 39.0  # (25 + 50 + 20 + 50 + 25) / 5
    assert data["critical_errors"] == []

    weaknesses = {w["competency_name"]: w for w in data["weaknesses"]}
    assert len(weaknesses) == 5  # every competency below its fire threshold
    assert weaknesses["procedure_compliance"]["score"] == 20.0
    assert weaknesses["procedure_compliance"]["severity"] == "severe"
    assert weaknesses["decision_making"]["score"] == 25.0
    assert weaknesses["decision_making"]["severity"] == "severe"
    assert weaknesses["ppe_selection"]["score"] == 50.0
    assert weaknesses["ppe_selection"]["severity"] == "moderate"


def test_submit_assessment_critical_error_forces_fail(client):
    worker = _create_worker(client)
    data = client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": CRITICAL_FIRE_EVENTS},
    ).json()
    assert data["passed"] is False
    assert data["critical_errors"] == [
        "Opened door during fire - fed oxygen to the flames"
    ]
    assert "CRITICAL" in data["pass_reason"]


def test_submit_gas_assessment(client):
    worker = _create_worker(client)
    data = client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 2, "events": GOOD_GAS_EVENTS},
    ).json()
    assert data["scenario_type"] == "gas"
    assert data["passed"] is True
    assert data["score"] == 100.0
    assert data["critical_errors"] == []
    scores = data["competency_scores"]
    assert "evacuation" in scores
    assert "emergency_response" in scores
    assert scores["emergency_response"]["score"] == 100.0


def test_submit_assessment_empty_events_rejected(client):
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": []},
    )
    assert response.status_code == 422


def test_submit_assessment_worker_not_found(client):
    response = client.post(
        "/api/v1/assessments",
        json={"worker_id": 999, "module_id": 1, "events": GOOD_FIRE_EVENTS},
    )
    assert response.status_code == 404
    assert response.json() == {"detail": "Worker not found"}


def test_submit_assessment_module_not_found(client):
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 999, "events": GOOD_FIRE_EVENTS},
    )
    assert response.status_code == 404
    assert response.json() == {"detail": "Module not found"}


def test_get_assessment_history(client):
    worker = _create_worker(client)
    client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": BAD_FIRE_EVENTS},
    )
    client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": GOOD_FIRE_EVENTS},
    )
    response = client.get(f"/api/v1/assessments/{worker['id']}")
    assert response.status_code == 200
    data = response.json()
    assert data["worker_id"] == worker["id"]
    assert len(data["assessments"]) == 2
    # Newest first
    assert data["assessments"][0]["attempt_number"] == 2
    assert data["assessments"][0]["passed"] is True
    assert data["assessments"][1]["attempt_number"] == 1
    assert data["assessments"][1]["passed"] is False
    assert (
        data["assessments"][1]["competency_scores"]["procedure_compliance"]["score"]
        == 20.0
    )


def test_get_assessment_history_not_found(client):
    response = client.get("/api/v1/assessments/999")
    assert response.status_code == 404
    assert response.json() == {"detail": "Worker not found"}


def test_get_latest_assessment(client):
    worker = _create_worker(client)
    client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": BAD_FIRE_EVENTS},
    )
    client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": GOOD_FIRE_EVENTS},
    )
    response = client.get(f"/api/v1/assessments/{worker['id']}/latest")
    assert response.status_code == 200
    data = response.json()
    assert data["attempt_number"] == 2
    assert data["passed"] is True


def test_get_latest_assessment_none(client):
    worker = _create_worker(client)
    response = client.get(f"/api/v1/assessments/{worker['id']}/latest")
    assert response.status_code == 404
    assert response.json() == {"detail": "No assessments found for worker"}


def test_get_latest_assessment_worker_not_found(client):
    response = client.get("/api/v1/assessments/999/latest")
    assert response.status_code == 404
    assert response.json() == {"detail": "Worker not found"}


def test_get_retraining_plan_for_failed_assessment(client):
    worker = _create_worker(client)
    assessment = client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": BAD_FIRE_EVENTS},
    ).json()
    response = client.get(f"/api/v1/assessments/{assessment['id']}/retraining-plan")
    assert response.status_code == 200
    plan = response.json()
    assert plan["scenario_type"] == "fire"
    assert plan["total_weaknesses"] == 5
    assert plan["weaknesses_addressed"] == len(plan["recommended_modules"])
    assert len(plan["recommended_modules"]) >= 1
    assert plan["total_estimated_duration_minutes"] > 0
    for module in plan["recommended_modules"]:
        assert module["name"]
        assert module["reason"]
        assert module["estimated_duration_minutes"] > 0
        assert module["difficulty_level"] in {"beginner", "intermediate", "advanced"}


def test_get_retraining_plan_passing_assessment_empty(client):
    worker = _create_worker(client)
    assessment = client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": GOOD_FIRE_EVENTS},
    ).json()
    response = client.get(f"/api/v1/assessments/{assessment['id']}/retraining-plan")
    assert response.status_code == 200
    plan = response.json()
    assert plan["recommended_modules"] == []
    assert plan["total_weaknesses"] == 0
    assert plan["weaknesses_addressed"] == 0


def test_get_retraining_plan_assessment_not_found(client):
    response = client.get("/api/v1/assessments/999/retraining-plan")
    assert response.status_code == 404
    assert response.json() == {"detail": "Assessment not found"}