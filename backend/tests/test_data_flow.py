"""End-to-end tests for the real assessment data flow.

Covers the complete chain required for Day 03:

    assessment submission -> server-side validation/scoring -> persistence ->
    competency result -> worker progress -> dashboard data

These tests assert that the backend and dashboard are backed by real stored
assessment data and that client-supplied scores/pass decisions are never
trusted (server-side authority).
"""

from events import BAD_FIRE_EVENTS, GOOD_FIRE_EVENTS

# A fully realistic fire assessment: start/completed markers with timestamps,
# extra behavioural fields (response_time_seconds, scenario metadata) exactly
# as the VR worker app would send them. Note response_time_seconds is carried
# but not scored by the existing engine rules.
REALISTIC_FIRE_EVENTS = [
    {
        "event_type": "assessment_started",
        "timestamp": "2026-09-04T10:00:00Z",
        "scenario": "fire_storage_area",
    },
    {
        "event_type": "hazard_identified",
        "timestamp": "2026-09-04T10:01:15Z",
        "correct": True,
        "hazard_type": "electrical_fire",
        "response_time_seconds": 12.5,
    },
    {
        "event_type": "ppe_selected",
        "timestamp": "2026-09-04T10:02:30Z",
        "correct": True,
        "items": ["helmet", "gloves", "jacket"],
    },
    {
        "event_type": "equipment_selected",
        "timestamp": "2026-09-04T10:03:45Z",
        "correct": True,
        "action": "grab_extinguisher",
    },
    {
        "event_type": "evacuation_started",
        "timestamp": "2026-09-04T10:05:00Z",
        "correct": True,
        "route": "north_exit",
    },
    {
        "event_type": "assessment_completed",
        "timestamp": "2026-09-04T10:06:15Z",
        "completion_status": "success",
    },
]


def _create_worker(client, employee_id="EMP001"):
    return client.post(
        "/api/v1/workers",
        json={
            "name": "Ramesh Kumar",
            "employee_id": employee_id,
            "role": "Fire Safety Worker",
        },
    ).json()


def _load_assessment(assessment_id):
    """Fetch a stored Assessment row from the test database.

    Plain column and JSON values are copied out while the session is open so
    they can safely outlive the session.
    """
    from conftest import TestingSessionLocal

    from app.models.assessment import Assessment

    db = TestingSessionLocal()
    try:
        row = db.query(Assessment).filter(Assessment.id == assessment_id).first()
        if row is None:
            return None
        return {
            "worker_id": row.worker_id,
            "module_id": row.module_id,
            "client_session_id": row.client_session_id,
            "attempt_number": row.attempt_number,
            "scenario_type": row.scenario_type,
            "score": row.score,
            "passed": row.passed,
            "events": row.events,
            "competency_scores": row.competency_scores,
        }
    finally:
        db.close()


def test_assessment_submission_with_realistic_payload(client):
    """A realistic event payload is scored server-side and persisted.

    The stored row is correctly associated with the worker and module and
    preserves the raw events plus the session idempotency key.
    """
    worker = _create_worker(client)
    response = client.post(
        "/api/v1/assessments",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "client_session_id": "realistic-fire-001",
            "events": REALISTIC_FIRE_EVENTS,
        },
    )
    assert response.status_code == 201
    data = response.json()
    assert data["worker_id"] == worker["id"]
    assert data["module_id"] == 1
    assert data["scenario_type"] == "fire"
    assert data["attempt_number"] == 1  # auto-assigned
    assert data["passed"] is True
    assert data["score"] == 90.0
    assert data["critical_errors"] == []
    assert data["competency_scores"]["ppe_selection"]["score"] == 100.0

    stored = _load_assessment(data["id"])
    assert stored is not None
    assert stored["worker_id"] == worker["id"]
    assert stored["module_id"] == 1
    assert stored["client_session_id"] == "realistic-fire-001"
    assert stored["attempt_number"] == 1
    assert stored["passed"] is True
    assert stored["score"] == 90.0
    assert stored["competency_scores"]["ppe_selection"]["score"] == 100.0
    assert stored["events"] == REALISTIC_FIRE_EVENTS


def test_client_supplied_score_and_passed_are_ignored(client):
    """Server-side scoring is authoritative: spoofed client values never apply.

    The engine scores only the raw behavioural events, so attempting to force a
    FAIL for passing events (or a PASS for failing events) has no effect on the
    response or on the persisted row.
    """
    worker = _create_worker(client)

    # Spoof a FAIL for a genuinely passing event set.
    passing = client.post(
        "/api/v1/assessments",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "score": 0.0,
            "passed": False,
            "events": GOOD_FIRE_EVENTS,
        },
    )
    assert passing.status_code == 201
    pass_data = passing.json()
    assert pass_data["passed"] is True
    assert pass_data["score"] == 90.0

    # Spoof a PASS for a genuinely failing event set.
    failing = client.post(
        "/api/v1/assessments",
        json={
            "worker_id": worker["id"],
            "module_id": 1,
            "score": 100.0,
            "passed": True,
            "events": BAD_FIRE_EVENTS,
        },
    )
    assert failing.status_code == 201
    fail_data = failing.json()
    assert fail_data["passed"] is False
    assert fail_data["score"] == 39.0

    # The stored rows carry the server-computed verdicts, not the spoofed ones.
    stored_pass = _load_assessment(pass_data["id"])
    stored_fail = _load_assessment(fail_data["id"])
    assert stored_pass["passed"] is True
    assert stored_pass["score"] == 90.0
    assert stored_fail["passed"] is False
    assert stored_fail["score"] == 39.0


def test_end_to_end_assessment_to_dashboard_data_flow(client):
    """Worker progress and the dashboard are driven by real stored data.

    After a failing attempt, a passing attempt, and a progress upsert, every
    downstream view (workers-scoped progress, dashboard summary/list/detail)
    reflects the real persisted assessment results. Issuing a certificate then
    updates the dashboard through the competency gate.
    """
    worker = _create_worker(client)
    worker_id = worker["id"]

    # Attempt 1: FAIL. Attempt 2: PASS. Plus a progress upsert.
    client.post(
        "/api/v1/assessments",
        json={"worker_id": worker_id, "module_id": 1, "events": BAD_FIRE_EVENTS},
    )
    client.post(
        "/api/v1/assessments",
        json={"worker_id": worker_id, "module_id": 1, "events": GOOD_FIRE_EVENTS},
    )
    client.post(
        "/api/v1/progress",
        json={
            "worker_id": worker_id,
            "module_id": 1,
            "stage": "assess",
            "status": "completed",
        },
    )

    # Worker progress: progress row merged with real stored assessment stats.
    progress = client.get(f"/api/v1/workers/{worker_id}/progress").json()["progress"]
    assert len(progress) == 1
    item = progress[0]
    assert item["module_code"] == "fire"
    assert item["stage"] == "assess"
    assert item["status"] == "completed"
    assert item["attempt_number"] == 2
    assert item["overall_score"] == 90.0
    assert item["passed"] is True
    assert item["assessments_count"] == 2

    # Dashboard summary: real counts, pass rate, and weaknesses.
    summary = client.get("/api/v1/dashboard/summary").json()
    assert summary["total_workers"] == 1
    assert summary["total_assessments"] == 2
    assert summary["pass_rate"] == 50.0
    assert summary["certified_workers"] == 0
    fire_stat = next(m for m in summary["module_stats"] if m["module_id"] == 1)
    assert fire_stat["workers_enrolled"] == 1
    assert fire_stat["certified"] == 0
    weakness_names = [w["competency_name"] for w in summary["common_weaknesses"]]
    assert "procedure_compliance" in weakness_names

    # Dashboard worker list: real progress, no certificates issued yet.
    workers = client.get("/api/v1/dashboard/workers").json()["workers"]
    assert len(workers) == 1
    assert workers[0]["progress"][0]["stage"] == "assess"
    assert workers[0]["certified_modules"] == []

    # Dashboard worker detail: real assessment history + competency profile.
    detail = client.get(f"/api/v1/dashboard/workers/{worker_id}").json()
    assert [a["attempt_number"] for a in detail["assessments"]] == [2, 1]
    assert detail["assessments"][0]["passed"] is True
    assert detail["assessments"][0]["score"] == 90.0
    assert detail["assessments"][1]["passed"] is False
    profile = detail["competency_profile"][0]
    assert profile["module_code"] == "fire"
    assert profile["attempt_number"] == 2
    assert profile["overall_score"] == 90.0
    assert profile["passed"] is True

    # The stored passing assessment satisfies the certificate competency gate.
    certificate = client.post(
        "/api/v1/certificates", json={"worker_id": worker_id, "module_id": 1}
    )
    assert certificate.status_code == 201

    # The dashboard now reflects the certificate from real stored data.
    summary = client.get("/api/v1/dashboard/summary").json()
    assert summary["certified_workers"] == 1
    fire_stat = next(m for m in summary["module_stats"] if m["module_id"] == 1)
    assert fire_stat["certified"] == 1

    workers = client.get("/api/v1/dashboard/workers").json()["workers"]
    assert workers[0]["certified_modules"] == ["fire"]
