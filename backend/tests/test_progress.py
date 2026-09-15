"""Tests for progress endpoints (including Day 1/7/30 retention checkpoints)."""

from datetime import datetime, timedelta, timezone

from conftest import TestingSessionLocal

from app.models.assessment import Assessment
from app.models.certificate import Certificate
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


def test_get_worker_retention_schedule(client):
    worker = _create_worker(client)
    # Complete an assessment
    client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": GOOD_FIRE_EVENTS},
    )
    response = client.get(f"/api/v1/progress/{worker['id']}/retention")
    assert response.status_code == 200
    data = response.json()
    assert data["worker_id"] == worker["id"]
    assert len(data["retention_schedules"]) >= 1
    schedule = data["retention_schedules"][0]
    assert schedule["module_code"] == "fire"
    assert len(schedule["milestones"]) == 3
    days = [m["day"] for m in schedule["milestones"]]
    assert days == [1, 7, 30]


# ---------------------------------------------------------------------------
# DAY 5 - retention checkpoints: Day 1 / Day 7 / Day 30 (CERTIFY -> RETAIN)
# ---------------------------------------------------------------------------


def _utcnow():
    return datetime.now(timezone.utc)


def _seed_certificate(worker_id, module_id, issued_at, status="active"):
    """Seed a certificate row with a controlled issue date."""
    db = TestingSessionLocal()
    try:
        db.add(
            Certificate(
                certificate_number=f"SUR-2026-{9000 + worker_id * 10 + module_id:04d}",
                worker_id=worker_id,
                module_id=module_id,
                issued_at=issued_at,
                valid_until=issued_at + timedelta(days=365),
                status=status,
            )
        )
        db.commit()
    finally:
        db.close()


def _seed_assessment(worker_id, module_id, created_at, passed=True, score=90.0):
    """Seed a stored (server-scored) assessment at a controlled timestamp."""
    db = TestingSessionLocal()
    try:
        db.add(
            Assessment(
                worker_id=worker_id,
                module_id=module_id,
                attempt_number=1,
                scenario_type="fire",
                score=score,
                passed=passed,
                pass_reason="seeded",
                weaknesses=[],
                competency_scores={},
                critical_errors=[],
                events=[],
                created_at=created_at,
            )
        )
        db.commit()
    finally:
        db.close()


def _retention_schedule(client, worker_id, module_id=1):
    response = client.get(f"/api/v1/progress/{worker_id}/retention")
    assert response.status_code == 200, response.text
    schedules = response.json()["retention_schedules"]
    return next(s for s in schedules if s["module_id"] == module_id)


def test_retention_checkpoints_anchor_on_certificate_issue_date(client):
    """CERTIFY -> RETAIN: the issuance date anchors the Day 1/7/30 checkpoints."""
    worker = _create_worker(client)
    issued_at = _utcnow() - timedelta(hours=2)
    _seed_certificate(worker["id"], 1, issued_at)

    schedule = _retention_schedule(client, worker["id"])
    assert schedule["module_code"] == "fire"
    assert schedule["module_name"] == "Fire & Explosion Response"
    assert [m["day"] for m in schedule["milestones"]] == [1, 7, 30]
    assert [m["title"] for m in schedule["milestones"]] == [
        "Day 1 Immediate Retention Check",
        "Day 7 Refresher Check",
        "Day 30 Competency Audit",
    ]
    base_date = datetime.fromisoformat(schedule["base_date"])
    assert abs((base_date - issued_at).total_seconds()) < 1
    # Freshly certified: no checkpoint is due yet, nothing is recorded.
    assert [m["status"] for m in schedule["milestones"]] == [
        "pending",
        "pending",
        "pending",
    ]
    assert all(m["passed"] is None and m["score"] is None for m in schedule["milestones"])


def test_retention_day1_checkpoint_completed(client):
    """A Day 1 retention assessment marks the first checkpoint complete."""
    worker = _create_worker(client)
    base = _utcnow() - timedelta(days=2)
    _seed_certificate(worker["id"], 1, base)
    _seed_assessment(worker["id"], 1, base + timedelta(days=1), passed=True, score=90.0)

    milestones = _retention_schedule(client, worker["id"])["milestones"]
    assert milestones[0]["status"] == "completed"
    assert milestones[0]["passed"] is True
    assert milestones[0]["score"] == 90.0
    assert milestones[1]["status"] == "pending"
    assert milestones[2]["status"] == "pending"


def test_retention_day7_checkpoint_completed_without_backfilling_day1(client):
    """Checkpoint windows are bounded: a late check cannot satisfy an earlier one."""
    worker = _create_worker(client)
    base = _utcnow() - timedelta(days=35)
    _seed_certificate(worker["id"], 1, base)
    # Day 1 was never checked; this assessment falls in the Day 7 window only.
    _seed_assessment(worker["id"], 1, base + timedelta(days=8))

    milestones = _retention_schedule(client, worker["id"])["milestones"]
    assert milestones[0]["status"] == "due"
    assert milestones[0]["passed"] is None
    assert milestones[1]["status"] == "completed"
    assert milestones[2]["status"] == "due"


def test_retention_day7_and_day30_checkpoints_completed(client):
    """The full Day 1 -> Day 7 -> Day 30 retention chain is tracked."""
    worker = _create_worker(client)
    base = _utcnow() - timedelta(days=35)
    _seed_certificate(worker["id"], 1, base)
    _seed_assessment(worker["id"], 1, base + timedelta(days=1), passed=True, score=88.0)
    _seed_assessment(worker["id"], 1, base + timedelta(days=8), passed=True, score=91.0)
    _seed_assessment(worker["id"], 1, base + timedelta(days=30), passed=False, score=40.0)

    milestones = _retention_schedule(client, worker["id"])["milestones"]
    assert [m["status"] for m in milestones] == ["completed", "completed", "completed"]
    assert milestones[0]["score"] == 88.0
    assert milestones[1]["score"] == 91.0
    # The Day 30 audit reports the stored server-scored result verbatim.
    assert milestones[2]["passed"] is False
    assert milestones[2]["score"] == 40.0


def test_retention_ignores_revoked_certificate_anchor(client):
    """A revoked certificate is not a valid CERTIFY anchor for retention."""
    worker = _create_worker(client)
    _seed_certificate(worker["id"], 1, _utcnow() - timedelta(days=40), status="revoked")

    response = client.get(f"/api/v1/progress/{worker['id']}/retention")
    assert response.status_code == 200
    assert response.json()["retention_schedules"] == []


def test_retention_empty_without_certificate_assessment_or_progress(client):
    worker = _create_worker(client)
    response = client.get(f"/api/v1/progress/{worker['id']}/retention")
    assert response.status_code == 200
    assert response.json() == {"worker_id": worker["id"], "retention_schedules": []}


def test_retention_falls_back_to_passing_assessment_only(client):
    """Before certification the clock anchors on the latest passing assessment."""
    worker = _create_worker(client)
    client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 1, "events": GOOD_FIRE_EVENTS},
    )
    schedule = _retention_schedule(client, worker["id"])
    assert [m["day"] for m in schedule["milestones"]] == [1, 7, 30]
    assert [m["status"] for m in schedule["milestones"]] == [
        "pending",
        "pending",
        "pending",
    ]
    assert datetime.fromisoformat(schedule["base_date"]) <= _utcnow()

    # A failing assessment for another module never anchors a schedule.
    client.post(
        "/api/v1/assessments",
        json={"worker_id": worker["id"], "module_id": 2, "events": BAD_FIRE_EVENTS},
    )
    data = client.get(f"/api/v1/progress/{worker['id']}/retention").json()
    assert [s["module_id"] for s in data["retention_schedules"]] == [1]


def test_retention_invalid_worker_ids(client):
    """Invalid worker identifiers never leak retention data."""
    not_found = client.get("/api/v1/progress/999/retention")
    assert not_found.status_code == 404
    assert not_found.json() == {"detail": "Worker not found"}
    assert client.get("/api/v1/progress/0/retention").status_code == 404
    assert client.get("/api/v1/progress/-1/retention").status_code == 404
    assert client.get("/api/v1/progress/not-a-number/retention").status_code == 422


def test_retention_requires_authentication(anonymous_client):
    response = anonymous_client.get("/api/v1/progress/1/retention")
    assert response.status_code == 401


def test_retention_cross_worker_forbidden_admin_allowed(client, worker_client):
    """Ownership is enforced server-side; admins may read any worker's schedule."""
    other = client.post(
        "/api/v1/workers",
        json={
            "name": "Other Worker",
            "employee_id": "EMP-WRK-002",
            "role": "Fire Safety Worker",
        },
    ).json()
    worker_test_client, worker = worker_client
    _seed_certificate(worker["id"], 1, _utcnow() - timedelta(hours=1))

    own = worker_test_client.get(f"/api/v1/progress/{worker['id']}/retention")
    assert own.status_code == 200
    assert own.json()["retention_schedules"][0]["module_code"] == "fire"

    forbidden = worker_test_client.get(f"/api/v1/progress/{other['id']}/retention")
    assert forbidden.status_code == 403
    assert forbidden.json() == {"detail": "Not authorized to access this worker's data"}

    assert client.get(f"/api/v1/progress/{other['id']}/retention").status_code == 200


def test_retention_checkpoint_status_is_server_computed(client):
    """Clients cannot set checkpoint values: only in_progress/completed are accepted."""
    worker = _create_worker(client)
    for invalid in ("pending", "due", "overdue", "passed", "skipped", ""):
        response = client.post(
            "/api/v1/progress",
            json={
                "worker_id": worker["id"],
                "module_id": 1,
                "stage": "retain",
                "status": invalid,
            },
        )
        assert response.status_code == 422, invalid

    ok = client.post(
        "/api/v1/progress",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "stage": "retain",
            "status": "completed",
        },
    )
    assert ok.status_code == 200
    assert ok.json()["stage"] == "retain"


def test_retention_invalid_stage_and_module_422_and_404(client):
    """Malformed retention/progress payloads are rejected, never silently stored."""
    worker = _create_worker(client)
    assert (
        client.post(
            "/api/v1/progress",
            json={
                "worker_id": worker["id"],
                "module_id": 1,
                "stage": "retention",
                "status": "completed",
            },
        ).status_code
        == 422
    )
    unknown_module = client.post(
        "/api/v1/progress",
        json={
            "worker_id": worker["id"],
            "module_id": 999,
            "stage": "retain",
            "status": "completed",
        },
    )
    assert unknown_module.status_code == 404
    assert unknown_module.json() == {"detail": "Module not found"}