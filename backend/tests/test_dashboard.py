"""Tests for dashboard endpoints (competency analytics)."""

from events import BAD_FIRE_EVENTS, GOOD_FIRE_EVENTS, GOOD_GAS_EVENTS


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
    assert data["common_weaknesses"] == []


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
        json={"worker_id": worker["id"], "module_id": 1, "events": GOOD_FIRE_EVENTS},
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
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": GOOD_FIRE_EVENTS},
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
        json={"worker_id": worker["id"], "module_id": 1, "events": GOOD_FIRE_EVENTS},
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


def test_dashboard_common_weaknesses(client):
    """Aggregated workforce weaknesses appear in the dashboard summary."""
    worker = _create_worker(client)
    client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": BAD_FIRE_EVENTS},
    )
    response = client.get("/api/v1/dashboard/summary")
    assert response.status_code == 200
    weaknesses = response.json()["common_weaknesses"]
    assert weaknesses, "expected aggregated weaknesses"
    names = [w["competency_name"] for w in weaknesses]
    assert "procedure_compliance" in names
    lowest = next(
        w for w in weaknesses if w["competency_name"] == "procedure_compliance"
    )
    assert lowest["count"] == 1
    assert lowest["average_score"] == 20.0
    # Sorted by count (desc), then name
    counts = [w["count"] for w in weaknesses]
    assert counts == sorted(counts, reverse=True)


def test_dashboard_worker_competency_profile(client):
    """Worker detail exposes the latest competency profile per module."""
    worker = _create_worker(client)
    client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": GOOD_FIRE_EVENTS},
    )
    client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 2, "events": GOOD_GAS_EVENTS},
    )
    response = client.get(f"/api/v1/dashboard/workers/{worker['id']}")
    assert response.status_code == 200
    data = response.json()
    profile = data["competency_profile"]
    assert [entry["module_code"] for entry in profile] == ["fire", "gas"]

    fire = profile[0]
    assert fire["overall_score"] == 90.0
    assert fire["passed"] is True
    assert fire["competencies"]["ppe_selection"]["score"] == 100.0
    assert fire["weaknesses"] == []

    gas = profile[1]
    assert gas["passed"] is True
    assert gas["competencies"]["emergency_response"]["score"] == 100.0


def test_dashboard_worker_competency_profile_empty(client):
    worker = _create_worker(client)
    response = client.get(f"/api/v1/dashboard/workers/{worker['id']}")
    assert response.status_code == 200
    assert response.json()["competency_profile"] == []