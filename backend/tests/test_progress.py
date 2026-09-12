"""Tests for progress endpoints."""

from events import BAD_FIRE_EVENTS, GOOD_FIRE_EVENTS


def _create_worker(client):
    return client.post(
        "/api/v1/workers",
        json={"name": "Ramesh Kumar", "employee_id": "EMP001", "role": "Fire Safety Worker"},
    ).json()


def test_get_progress_empty(client):
    worker = _create_worker(client)
    response = client.get(f"/api/v1/progress/{worker['id']}")
    assert response.status_code == 200
    assert response.json() == {"worker_id": worker["id"], "progress": []}


def test_get_progress_not_found(client):
    response = client.get("/api/v1/progress/999")
    assert response.status_code == 404
    assert response.json() == {"detail": "Worker not found"}


def test_update_progress(client):
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/progress",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "stage": "practice",
            "status": "completed",
        },
    )
    assert response.status_code == 200
    data = response.json()
    assert data["worker_id"] == worker["id"]
    assert data["module_id"] == 1
    assert data["stage"] == "practice"
    assert data["status"] == "completed"
    assert "updated_at" in data


def test_update_progress_upsert(client):
    worker = _create_worker(client)
    payload = {
        "worker_id": worker["id"],
        "module_id": 1,
        "stage": "assess",
        "status": "in_progress",
    }
    client.post("/api/v1/progress", json=payload)
    response = client.post(
        "/api/v1/progress",
        json={**payload, "stage": "retain", "status": "completed"},
    )
    assert response.status_code == 200
    data = response.json()
    assert data["stage"] == "retain"
    assert data["status"] == "completed"


def test_get_progress_after_update(client):
    worker = _create_worker(client)
    client.post(
        "/api/v1/progress",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "stage": "practice",
            "status": "completed",
        },
    )
    response = client.get(f"/api/v1/progress/{worker['id']}")
    assert response.status_code == 200
    data = response.json()
    assert len(data["progress"]) == 1
    item = data["progress"][0]
    assert item["module_id"] == 1
    assert item["module_code"] == "fire"
    assert item["module_name"] == "Fire & Explosion Response"
    assert item["stage"] == "practice"
    assert item["status"] == "completed"
    assert "last_updated" in item


def test_update_progress_worker_not_found(client):
    response = client.post(
        "/api/v1/progress",
        json={
            "worker_id": 999,
            "module_id": 1,
            "stage": "practice",
            "status": "completed",
        },
    )
    assert response.status_code == 404
    assert response.json() == {"detail": "Worker not found"}


def test_update_progress_module_not_found(client):
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/progress",
        json={
            "worker_id": worker["id"],
            "module_id": 999,
            "stage": "practice",
            "status": "completed",
        },
    )
    assert response.status_code == 404
    assert response.json() == {"detail": "Module not found"}


def test_update_progress_invalid_stage(client):
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/progress",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "stage": "not_a_stage",
            "status": "completed",
        },
    )
    assert response.status_code == 422


def test_get_worker_progress_nested_empty(client):
    worker = _create_worker(client)
    response = client.get(f"/api/v1/workers/{worker['id']}/progress")
    assert response.status_code == 200
    assert response.json() == {"worker_id": worker["id"], "progress": []}


def test_get_worker_progress_nested_not_found(client):
    response = client.get("/api/v1/workers/999/progress")
    assert response.status_code == 404
    assert response.json() == {"detail": "Worker not found"}


def test_get_worker_progress_nested_after_update(client):
    worker = _create_worker(client)
    client.post(
        "/api/v1/progress",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "stage": "practice",
            "status": "completed",
        },
    )
    response = client.get(f"/api/v1/workers/{worker['id']}/progress")
    assert response.status_code == 200
    item = response.json()["progress"][0]
    assert item["module_id"] == 1
    assert item["module_code"] == "fire"
    assert item["stage"] == "practice"
    assert item["status"] == "completed"
    assert item["attempt_number"] is None
    assert item["overall_score"] is None
    assert item["passed"] is None
    assert item["assessments_count"] == 0


def test_get_worker_progress_nested_reflects_assessments(client):
    """Progress retrieval reflects stored assessment data even without progress rows."""
    worker = _create_worker(client)
    client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": BAD_FIRE_EVENTS},
    )
    client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": GOOD_FIRE_EVENTS},
    )
    response = client.get(f"/api/v1/workers/{worker['id']}/progress")
    assert response.status_code == 200
    item = response.json()["progress"][0]
    assert item["module_id"] == 1
    assert item["stage"] is None
    assert item["status"] is None
    assert item["attempt_number"] == 2
    assert item["overall_score"] == 90.0
    assert item["passed"] is True
    assert item["assessments_count"] == 2