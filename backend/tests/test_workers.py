"""Tests for worker endpoints."""


def test_create_worker(client):
    response = client.post(
        "/api/v1/workers",
        json={"name": "Ramesh Kumar", "employee_id": "EMP001", "role": "Fire Safety Worker"},
    )
    assert response.status_code == 201
    data = response.json()
    assert data["id"] == 1
    assert data["name"] == "Ramesh Kumar"
    assert data["employee_id"] == "EMP001"
    assert data["role"] == "Fire Safety Worker"
    assert "created_at" in data


def test_create_worker_duplicate_employee_id(client):
    payload = {"name": "Ramesh Kumar", "employee_id": "EMP001", "role": "Fire Safety Worker"}
    assert client.post("/api/v1/workers", json=payload).status_code == 201
    response = client.post("/api/v1/workers", json=payload)
    assert response.status_code == 409
    assert response.json() == {"detail": "Worker with employee_id EMP001 already exists"}


def test_create_worker_validation_error(client):
    response = client.post("/api/v1/workers", json={"name": "No Role Worker"})
    assert response.status_code == 422


def test_get_worker(client):
    created = client.post(
        "/api/v1/workers",
        json={"name": "Ramesh Kumar", "employee_id": "EMP001", "role": "Fire Safety Worker"},
    ).json()
    response = client.get(f"/api/v1/workers/{created['id']}")
    assert response.status_code == 200
    assert response.json()["employee_id"] == "EMP001"


def test_get_worker_not_found(client):
    response = client.get("/api/v1/workers/999")
    assert response.status_code == 404
    assert response.json() == {"detail": "Worker not found"}