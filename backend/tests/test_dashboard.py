"""Tests for dashboard endpoints."""


def _create_worker(client, employee_id="EMP001"):
    return client.post(
        "/api/v1/workers",
        json={
            "name": "Ramesh Kumar",
            "employee_id": employee_id,
            "role": "Fire Safety Worker",
        },
    ).json()


def test_dashboard_summary_empty(client):
    response = client.get("/api/v1/dashboard/summary")
    assert response.status_code == 200
    data = response.json()
    assert data["total_workers"] == 0
    assert data["workers_in_training"] == 0
    assert data["certified_workers"] == 0
    assert data["total_assessments"] == 0
    assert data["pass_rate"] == 0.0
    assert len(data["module_stats"]) == 2
    assert data["module_stats"][0]["module_name"] == "Fire & Explosion Response"
    assert data["module_stats"][0]["workers_enrolled"] == 0
    assert data["module_stats"][0]["certified"] == 0


def test_dashboard_summary_with_data(client):
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
    client.post(
        "/api/v1/assessments",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "attempt_number": 1,
            "score": 85.0,
            "passed": True,
            "weaknesses": [],
        },
    )
    client.post(
        "/api/v1/certificates",
        json={"worker_id": worker["id"], "module_id": 1},
    )
    response = client.get("/api/v1/dashboard/summary")
    assert response.status_code == 200
    data = response.json()
    assert data["total_workers"] == 1
    assert data["certified_workers"] == 1
    assert data["workers_in_training"] == 0
    assert data["total_assessments"] == 1
    assert data["pass_rate"] == 100.0
    module_stat = data["module_stats"][0]
    assert module_stat["workers_enrolled"] == 1
    assert module_stat["certified"] == 1


def test_dashboard_worker_list(client):
    worker = _create_worker(client)
    client.post(
        "/api/v1/progress",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "stage": "certify",
            "status": "completed",
        },
    )
    client.post(
        "/api/v1/certificates",
        json={"worker_id": worker["id"], "module_id": 1},
    )
    response = client.get("/api/v1/dashboard/workers")
    assert response.status_code == 200
    data = response.json()
    assert len(data["workers"]) == 1
    item = data["workers"][0]
    assert item["id"] == worker["id"]
    assert item["name"] == "Ramesh Kumar"
    assert item["employee_id"] == "EMP001"
    assert len(item["progress"]) == 1
    assert item["progress"][0]["stage"] == "certify"
    assert item["certified_modules"] == ["fire"]


def test_dashboard_worker_detail(client):
    worker = _create_worker(client)
    client.post(
        "/api/v1/progress",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "stage": "certify",
            "status": "completed",
        },
    )
    client.post(
        "/api/v1/assessments",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "attempt_number": 1,
            "score": 92.0,
            "passed": True,
            "weaknesses": [],
        },
    )
    client.post(
        "/api/v1/certificates",
        json={"worker_id": worker["id"], "module_id": 1},
    )
    response = client.get(f"/api/v1/dashboard/workers/{worker['id']}")
    assert response.status_code == 200
    data = response.json()
    assert data["id"] == worker["id"]
    assert len(data["progress"]) == 1
    assert len(data["assessments"]) == 1
    assert len(data["certificates"]) == 1
    assert data["certificates"][0]["certificate_number"] == "SUR-2026-0001"


def test_dashboard_worker_detail_not_found(client):
    response = client.get("/api/v1/dashboard/workers/999")
    assert response.status_code == 404
    assert response.json() == {"detail": "Worker not found"}