"""Tests for assessment endpoints."""


def _create_worker(client):
    return client.post(
        "/api/v1/workers",
        json={"name": "Ramesh Kumar", "employee_id": "EMP001", "role": "Fire Safety Worker"},
    ).json()


def test_submit_assessment(client):
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/assessments",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "attempt_number": 1,
            "score": 85.0,
            "passed": True,
            "weaknesses": ["incorrect_extinguisher_selection"],
        },
    )
    assert response.status_code == 201
    data = response.json()
    assert data["id"] == 1
    assert data["worker_id"] == worker["id"]
    assert data["module_id"] == 1
    assert data["attempt_number"] == 1
    assert data["score"] == 85.0
    assert data["passed"] is True
    assert data["weaknesses"] == ["incorrect_extinguisher_selection"]
    assert "created_at" in data


def test_submit_assessment_worker_not_found(client):
    response = client.post(
        "/api/v1/assessments",
        json={
            "worker_id": 999,
            "module_id": 1,
            "attempt_number": 1,
            "score": 85.0,
            "passed": True,
            "weaknesses": [],
        },
    )
    assert response.status_code == 404
    assert response.json() == {"detail": "Worker not found"}


def test_submit_assessment_module_not_found(client):
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/assessments",
        json={
            "worker_id": worker["id"],
            "module_id": 999,
            "attempt_number": 1,
            "score": 85.0,
            "passed": True,
            "weaknesses": [],
        },
    )
    assert response.status_code == 404
    assert response.json() == {"detail": "Module not found"}


def test_get_assessment_history(client):
    worker = _create_worker(client)
    client.post(
        "/api/v1/assessments",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "attempt_number": 1,
            "score": 60.0,
            "passed": False,
            "weaknesses": ["wrong_evacuation_route"],
        },
    )
    client.post(
        "/api/v1/assessments",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "attempt_number": 2,
            "score": 92.0,
            "passed": True,
            "weaknesses": [],
        },
    )
    response = client.get(f"/api/v1/assessments/{worker['id']}")
    assert response.status_code == 200
    data = response.json()
    assert data["worker_id"] == worker["id"]
    assert len(data["assessments"]) == 2
    # Newest first
    assert data["assessments"][0]["attempt_number"] == 2
    assert data["assessments"][1]["attempt_number"] == 1


def test_get_assessment_history_not_found(client):
    response = client.get("/api/v1/assessments/999")
    assert response.status_code == 404
    assert response.json() == {"detail": "Worker not found"}


def test_get_latest_assessment(client):
    worker = _create_worker(client)
    client.post(
        "/api/v1/assessments",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "attempt_number": 1,
            "score": 60.0,
            "passed": False,
            "weaknesses": ["wrong_evacuation_route"],
        },
    )
    client.post(
        "/api/v1/assessments",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "attempt_number": 2,
            "score": 92.0,
            "passed": True,
            "weaknesses": [],
        },
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