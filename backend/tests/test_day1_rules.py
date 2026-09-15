"""Day 1 — API-level tests for the Rehan-confirmed scoring rules.

Exercises the confirmed rules through POST /api/v1/assessments (the authoritative
server-side scoring path). Engine-level coverage lives in
ml/competency/tests/test_day1_rules.py.
"""

import pytest

from events import BAD_FIRE_EVENTS, GOOD_FIRE_EVENTS


def _create_worker(client):
    return client.post(
        "/api/v1/workers",
        json={"name": "Ramesh Kumar", "employee_id": "EMP001", "role": "Fire Safety Worker"},
    ).json()


def _submit(client, worker_id, events, **extra):
    payload = {"worker_id": worker_id, "module_id": 1, "events": events, **extra}
    return client.post("/api/v1/assessments", json=payload).json()


def _completed(events):
    """Append the mandatory completion event to an event list."""
    return events + [{"event_type": "assessment_completed", "completion_status": "success"}]


def test_unsafe_action_penalises_but_is_not_a_critical_error(client):
    """unsafe_action: large penalty, NO automatic FAIL (no critical_errors)."""
    worker = _create_worker(client)
    events = GOOD_FIRE_EVENTS[:4] + [
        {"event_type": "unsafe_action", "action": "removed_machine_guard"},
        {"event_type": "assessment_completed", "completion_status": "success"},
    ]
    data = _submit(client, worker["id"], events)
    scores = data["competency_scores"]
    # procedure: 50 + 50 - 22 = 78; decision: 50 - 20 = 30 (< 45 threshold)
    assert scores["procedure_compliance"]["score"] == pytest.approx(78.0)
    assert scores["decision_making"]["score"] == pytest.approx(30.0)
    # Not treated as a critical safety error:
    assert data["critical_errors"] == []
    assert not data["pass_reason"].startswith("CRITICAL ERRORS")
    # It still fails — via the numeric competency rule, not the critical rule.
    assert data["passed"] is False
    assert data["pass_reason"].startswith("Failed competencies")


def test_wrong_action_with_severity_critical_is_automatic_fail(client):
    worker = _create_worker(client)
    events = GOOD_FIRE_EVENTS[:4] + [
        {"event_type": "wrong_action", "severity": "critical", "action": "disabled_alarm"},
        {"event_type": "assessment_completed", "completion_status": "success"},
    ]
    data = _submit(client, worker["id"], events)
    assert data["passed"] is False
    assert len(data["critical_errors"]) == 1
    assert data["pass_reason"].startswith("CRITICAL ERRORS")


def test_critical_true_flag_is_automatic_fail(client):
    worker = _create_worker(client)
    events = GOOD_FIRE_EVENTS[:1] + [
        {"event_type": "hazard_identified", "correct": True, "hazard_type": "fire_source",
         "critical": True, "reason": "Ignored active alarm"},
    ] + GOOD_FIRE_EVENTS[1:4] + [
        {"event_type": "assessment_completed", "completion_status": "success"},
    ]
    data = _submit(client, worker["id"], events)
    assert data["passed"] is False
    assert len(data["critical_errors"]) == 1
    assert data["pass_reason"].startswith("CRITICAL ERRORS")


def test_incomplete_assessment_must_not_pass(client):
    """Same events without a completion event -> FAIL even with passing scores."""
    worker = _create_worker(client)
    data = _submit(client, worker["id"], GOOD_FIRE_EVENTS[:4])
    assert data["score"] == pytest.approx(90.0)  # scores alone would pass
    assert data["passed"] is False
    assert data["pass_reason"].startswith("Incomplete assessment")


def test_scenario_completed_event_satisfies_completion(client):
    worker = _create_worker(client)
    events = GOOD_FIRE_EVENTS[:4] + [
        {"event_type": "scenario_completed", "completion_status": "success"},
    ]
    data = _submit(client, worker["id"], events)
    assert data["passed"] is True
    assert data["score"] == pytest.approx(90.0)


def test_response_time_rules_change_the_score(client):
    """<3s bonus vs 3-15s baseline vs >15s penalty — same events, different rt."""
    worker = _create_worker(client)
    base = GOOD_FIRE_EVENTS[:3] + [
        {"event_type": "wrong_action", "severity": "minor", "action": "wrong_angle"},
        {"event_type": "evacuation_started", "correct": True, "route": "north_exit"},
        {"event_type": "assessment_completed", "completion_status": "success"},
    ]

    def with_rt(seconds):
        events = [dict(event) for event in base]
        for event in events:
            if event["event_type"] == "wrong_action":
                event["response_time_seconds"] = seconds
        return _submit(client, worker["id"], events)

    fast = with_rt(2.0)      # procedure: 50+50-5*1.05 = 94.75; decision: 50-3*1.05 = 46.85
    baseline = with_rt(8.0)  # procedure: 95.0; decision: 47.0 (no change)
    slow = with_rt(20.0)     # procedure: 50+50-4.5 = 95.5; decision: 47.3

    assert fast["score"] == pytest.approx((100 + 100 + 100 + 94.75 + 46.85) / 5)
    assert baseline["score"] == pytest.approx((100 + 100 + 100 + 95.0 + 47.0) / 5)
    assert slow["score"] == pytest.approx((100 + 100 + 100 + 95.5 + 47.3) / 5)
    assert fast["score"] < baseline["score"] < slow["score"]
    for result in (fast, baseline, slow):
        assert result["passed"] is True


def test_attempt_number_is_recorded_and_never_penalises_score(client):
    """Attempts are audit data: no score penalty from previous failed attempts."""
    worker = _create_worker(client)
    bad = _submit(client, worker["id"], _completed(BAD_FIRE_EVENTS), attempt_number=1)
    assert bad["passed"] is False

    # Same performance retried (attempt 3): identical scoring outcome.
    good = _submit(client, worker["id"], GOOD_FIRE_EVENTS, attempt_number=3)
    again = _submit(client, worker["id"], GOOD_FIRE_EVENTS, attempt_number=4)
    assert good["attempt_number"] == 3
    assert again["attempt_number"] == 4
    assert good["score"] == again["score"] == pytest.approx(90.0)
    assert good["passed"] is again["passed"] is True
    assert bad["score"] != good["score"]  # scored on its own events, not history


def test_weaknesses_carry_affected_aspects(client):
    worker = _create_worker(client)
    data = _submit(client, worker["id"], _completed(BAD_FIRE_EVENTS))
    assert data["passed"] is False
    assert len(data["weaknesses"]) == 5  # every competency below its fire threshold
    for weakness in data["weaknesses"]:
        assert weakness["affected_aspects"], "affected_aspects must be populated"
    by_name = {w["competency_name"]: w for w in data["weaknesses"]}
    assert by_name["ppe_selection"]["affected_aspects"] == [
        "select_correct_ppe", "proper_donning", "ppe_completeness", "ppe_inspection",
    ]


def test_critical_error_fails_despite_perfect_scores(client):
    worker = _create_worker(client)
    events = GOOD_FIRE_EVENTS + [
        {"event_type": "critical_action", "action": "reentered_burning_area"},
    ]
    data = _submit(client, worker["id"], events)
    assert data["score"] == pytest.approx(90.0)  # perfect scores
    assert data["passed"] is False               # critical error wins
    assert data["pass_reason"].startswith("CRITICAL ERRORS")