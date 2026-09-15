"""Day 6 end-to-end integration smoke test for the SurakshaAR backend.

Exercises the REAL HTTP/WebSocket surface of a running server (not the
in-process TestClient), covering the full integration flow with both roles:

  health -> 401 matrix -> admin login -> register worker (+credentials)
  -> worker login -> progress -> live events -> assessment (server-scored)
  -> certificate (admin) -> anonymous QR verification -> retention schedule
  -> offline sync -> sync status -> dashboard summary/workers/detail
  -> cross-worker 403 -> live WebSocket broadcast

Usage (with the server already running):

    python backend/scripts/smoke_test.py
    python backend/scripts/smoke_test.py --base-url http://127.0.0.1:8000

Admin credentials are read from backend/.env (ADMIN_USERNAME / ADMIN_PASSWORD)
or overridden with --username / --password. Exit code 0 means every check
passed.
"""

import argparse
import asyncio
import json
import os
import sys
from datetime import datetime, timezone
from pathlib import Path
from urllib.parse import quote

import httpx

try:  # Reuse backend/.env so the script needs no extra arguments.
    from dotenv import load_dotenv

    load_dotenv(Path(__file__).resolve().parent.parent / ".env")
except ImportError:  # pragma: no cover - python-dotenv is optional
    pass

DEFAULT_BASE_URL = "http://127.0.0.1:8000"

# Mirrors backend/tests/events.py: a clean fire run passes every competency
# (overall 90.0) and therefore satisfies the certificate eligibility gate.
GOOD_FIRE_EVENTS = [
    {
        "event_type": "hazard_identified",
        "correct": True,
        "hazard_type": "electrical_fire",
    },
    {
        "event_type": "ppe_selected",
        "correct": True,
        "items": ["helmet", "gloves", "jacket"],
    },
    {
        "event_type": "equipment_selected",
        "correct": True,
        "action": "grab_extinguisher",
    },
    {
        "event_type": "evacuation_started",
        "correct": True,
        "route": "north_exit",
    },
]


class Reporter:
    """Tiny PASS/FAIL reporter with a CI-friendly exit code."""

    def __init__(self) -> None:
        self.passed = 0
        self.failed = 0

    def check(self, name: str, ok: bool, detail: str = "") -> bool:
        suffix = f"  ({detail})" if detail else ""
        if ok:
            self.passed += 1
            print(f"  PASS  {name}{suffix}")
        else:
            self.failed += 1
            print(f"  FAIL  {name}{suffix}")
        return ok

    def summary(self) -> int:
        print(f"\n{self.passed} passed, {self.failed} failed")
        return 0 if self.failed == 0 else 1


# ---------------------------------------------------------------------------
# Small helpers
# ---------------------------------------------------------------------------
def _auth(token: str) -> dict:
    return {"Authorization": f"Bearer {token}"}


def _body(response: httpx.Response) -> dict:
    try:
        return response.json()
    except Exception:
        return {}


def _detail(response: httpx.Response) -> str:
    return str(_body(response).get("detail", response.text[:120]))


def _parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="SurakshaAR Day 6 integration smoke test")
    parser.add_argument(
        "--base-url",
        default=os.getenv("SURAKHSHAAR_BASE_URL", DEFAULT_BASE_URL),
        help="Base URL of the running backend (default: %(default)s)",
    )
    parser.add_argument("--username", default=os.getenv("ADMIN_USERNAME", "admin"))
    parser.add_argument("--password", default=os.getenv("ADMIN_PASSWORD", ""))
    parser.add_argument(
        "--skip-websocket",
        action="store_true",
        help="Skip the live WebSocket checks (no WS transport available)",
    )
    return parser.parse_args()


# ---------------------------------------------------------------------------
# HTTP integration flow
# ---------------------------------------------------------------------------
def run_http_checks(
    client: httpx.Client, reporter: Reporter, username: str, password: str
) -> dict:
    """Run the authenticated HTTP flow; return context needed by the WS step."""
    context: dict = {}

    print("\n[1] Public surface + authentication gate")
    health = client.get("/health")
    reporter.check(
        "GET /health (public)", health.status_code == 200, f"HTTP {health.status_code}"
    )

    unauth = client.get("/api/v1/dashboard/summary")
    reporter.check(
        "GET /dashboard/summary without token -> 401",
        unauth.status_code == 401,
        f"HTTP {unauth.status_code}",
    )

    bad_login = client.post(
        "/api/v1/auth/login",
        json={"username": username, "password": "definitely-not-the-password"},
    )
    reporter.check(
        "POST /auth/login with a bad password -> 401",
        bad_login.status_code == 401,
        f"HTTP {bad_login.status_code}",
    )

    print("\n[2] Admin login + module catalogue")
    login = client.post(
        "/api/v1/auth/login", json={"username": username, "password": password}
    )
    if not reporter.check(
        "POST /auth/login (admin) -> 200",
        login.status_code == 200,
        f"HTTP {login.status_code}" if login.status_code != 200 else _detail(login),
    ):
        return context

    login_data = _body(login)
    admin_token = login_data.get("access_token", "")
    context["admin_token"] = admin_token
    reporter.check(
        "admin token carries role=admin",
        login_data.get("role") == "admin",
        f"role={login_data.get('role')}",
    )

    modules = client.get("/api/v1/modules", headers=_auth(admin_token))
    module_list = _body(modules).get("modules", [])
    reporter.check(
        "GET /modules with admin token -> 2 MVP modules",
        modules.status_code == 200 and len(module_list) == 2,
        f"HTTP {modules.status_code}, {len(module_list)} module(s)",
    )
    fire_module_id = next(
        (m["id"] for m in module_list if m.get("code") == "fire"), 1
    )
    context["fire_module_id"] = fire_module_id

    print("\n[3] Register a worker + worker login")
    stamp = datetime.now(timezone.utc).strftime("%H%M%S")
    employee_id = f"EMP-SMOKE-{stamp}"
    worker_username = f"smoke_worker_{stamp}"

    created = client.post(
        "/api/v1/workers",
        headers=_auth(admin_token),
        json={
            "name": "Smoke Test Worker",
            "employee_id": employee_id,
            "role": "Fire Safety Worker",
            "username": worker_username,
            "password": "SmokeWorker@2026",
        },
    )
    if not reporter.check(
        "POST /workers (admin) -> 201",
        created.status_code == 201,
        f"HTTP {created.status_code} {_detail(created)}".strip(),
    ):
        return context

    worker = _body(created)
    worker_id = worker["id"]
    context["worker_id"] = worker_id
    context["worker_username"] = worker_username

    worker_login = client.post(
        "/api/v1/auth/login",
        json={"username": worker_username, "password": "SmokeWorker@2026"},
    )
    worker_token = _body(worker_login).get("access_token", "")
    reporter.check(
        "POST /auth/login (worker) -> 200 with matching worker_id",
        worker_login.status_code == 200
        and _body(worker_login).get("worker_id") == worker_id,
        f"HTTP {worker_login.status_code}, worker_id={_body(worker_login).get('worker_id')}",
    )
    context["worker_token"] = worker_token
    if not worker_token:
        return context

    print("\n[4] Worker progress + live telemetry + server-scored assessment")
    progress = client.post(
        "/api/v1/progress",
        headers=_auth(worker_token),
        json={
            "worker_id": worker_id,
            "module_id": fire_module_id,
            "stage": "assess",
            "status": "in_progress",
        },
    )
    reporter.check(
        "POST /progress (worker, own data) -> 200",
        progress.status_code == 200,
        f"HTTP {progress.status_code} {_detail(progress)}".strip(),
    )

    events = client.post(
        "/api/v1/events",
        headers=_auth(worker_token),
        json={
            "worker_id": worker_id,
            "session_id": f"sess_smoke_{stamp}",
            "events": [
                {
                    "event_type": "hazard_identified",
                    "correct": True,
                    "severity": "info",
                    "payload": {"hazard_type": "electrical_fire"},
                },
                {
                    "event_type": "critical_action",
                    "severity": "critical",
                    "payload": {"action": "smoke_probe", "reason": "Day 6 smoke test"},
                },
            ],
        },
    )
    reporter.check(
        "POST /events (worker, own telemetry) -> 201",
        events.status_code == 201,
        f"HTTP {events.status_code} {_detail(events)}".strip(),
    )

    assessment = client.post(
        "/api/v1/assessments",
        headers=_auth(worker_token),
        json={
            "worker_id": worker_id,
            "module_id": fire_module_id,
            "client_session_id": f"cs_smoke_{stamp}",
            "events": GOOD_FIRE_EVENTS,
        },
    )
    assessment_data = _body(assessment)
    reporter.check(
        "POST /assessments (worker) -> 201, server-scored PASS",
        assessment.status_code == 201 and assessment_data.get("passed") is True,
        f"HTTP {assessment.status_code}, score={assessment_data.get('score')}",
    )

    latest = client.get(
        f"/api/v1/assessments/{worker_id}/latest",
        headers=_auth(worker_token),
        params={"module_id": fire_module_id},
    )
    reporter.check(
        "GET /assessments/{worker_id}/latest -> 200",
        latest.status_code == 200,
        f"HTTP {latest.status_code}",
    )

    print("\n[5] Certificate issuance (admin) + anonymous QR verification")
    issued = client.post(
        "/api/v1/certificates",
        headers=_auth(admin_token),
        json={"worker_id": worker_id, "module_id": fire_module_id},
    )
    certificate_number = _body(issued).get("certificate_number", "")
    reporter.check(
        "POST /certificates (admin) -> 201 with SUR-YYYY-NNNN",
        issued.status_code == 201 and certificate_number.startswith("SUR-"),
        f"HTTP {issued.status_code}, number={certificate_number or _detail(issued)}",
    )

    worker_issue = client.post(
        "/api/v1/certificates",
        headers=_auth(worker_token),
        json={"worker_id": worker_id, "module_id": fire_module_id},
    )
    reporter.check(
        "POST /certificates with a worker token -> 403",
        worker_issue.status_code == 403,
        f"HTTP {worker_issue.status_code}",
    )

    if certificate_number:
        verify = client.get(f"/api/v1/certificates/verify/{certificate_number}")
        verify_data = _body(verify)
        reporter.check(
            "GET /certificates/verify/{number} WITHOUT a token -> 200 valid",
            verify.status_code == 200 and verify_data.get("valid") is True,
            f"HTTP {verify.status_code}, valid={verify_data.get('valid')}",
        )
        context["certificate_number"] = certificate_number

    own_certs = client.get(
        f"/api/v1/certificates/{worker_id}", headers=_auth(worker_token)
    )
    reporter.check(
        "GET /certificates/{worker_id} (worker, own data) -> 200",
        own_certs.status_code == 200
        and len(_body(own_certs).get("certificates", [])) >= 1,
        f"HTTP {own_certs.status_code}",
    )

    print("\n[6] Retention schedule (Day 1/7/30) + offline sync")
    retention = client.get(
        f"/api/v1/progress/{worker_id}/retention", headers=_auth(worker_token)
    )
    schedules = _body(retention).get("retention_schedules", [])
    milestones = schedules[0]["milestones"] if schedules else []
    reporter.check(
        "GET /progress/{worker_id}/retention -> 200 with Day 1/7/30 checkpoints",
        retention.status_code == 200
        and [m["day"] for m in milestones] == [1, 7, 30],
        f"HTTP {retention.status_code}, days={[m['day'] for m in milestones]}",
    )

    sync = client.post(
        "/api/v1/sync",
        headers=_auth(worker_token),
        json={
            "worker_id": worker_id,
            "device_id": "smoke-device-01",
            "batch_id": f"batch_smoke_{stamp}",
            "pending_sessions": 0,
            "sessions": [
                {
                    "type": "assessment",
                    "module_id": fire_module_id,
                    "occurred_at": datetime.now(timezone.utc).isoformat(),
                    "scenario_type": "fire",
                    "client_session_id": f"cs_sync_{stamp}",
                    # Deliberately bogus client claims: the server must ignore them.
                    "score": 0,
                    "passed": False,
                    "events": GOOD_FIRE_EVENTS,
                }
            ],
        },
    )
    sync_data = _body(sync)
    reporter.check(
        "POST /sync (worker) -> 201, offline session re-scored server-side",
        sync.status_code == 201 and sync_data.get("assessments_created") == 1,
        f"HTTP {sync.status_code}, assessments_created={sync_data.get('assessments_created')}",
    )

    after_sync = client.get(
        f"/api/v1/assessments/{worker_id}/latest",
        headers=_auth(worker_token),
        params={"module_id": fire_module_id},
    )
    reporter.check(
        "synced assessment ignores the bogus client score/passed claims",
        _body(after_sync).get("passed") is True,
        f"passed={_body(after_sync).get('passed')}",
    )

    sync_status = client.get(
        f"/api/v1/sync/status/{worker_id}", headers=_auth(worker_token)
    )
    reporter.check(
        "GET /sync/status/{worker_id} -> 200",
        sync_status.status_code == 200,
        f"HTTP {sync_status.status_code}",
    )

    print("\n[7] Admin dashboard reads")
    summary = client.get("/api/v1/dashboard/summary", headers=_auth(admin_token))
    summary_data = _body(summary)
    reporter.check(
        "GET /dashboard/summary (admin) counts the smoke worker",
        summary.status_code == 200 and summary_data.get("total_workers", 0) >= 1,
        f"HTTP {summary.status_code}, total_workers={summary_data.get('total_workers')}",
    )

    worker_list = client.get("/api/v1/dashboard/workers", headers=_auth(admin_token))
    listed = any(w.get("id") == worker_id for w in _body(worker_list).get("workers", []))
    reporter.check(
        "GET /dashboard/workers (admin) lists the smoke worker",
        worker_list.status_code == 200 and listed,
        f"HTTP {worker_list.status_code}",
    )

    detail = client.get(
        f"/api/v1/dashboard/workers/{worker_id}", headers=_auth(admin_token)
    )
    detail_data = _body(detail)
    reporter.check(
        "GET /dashboard/workers/{id} returns assessments + certificates",
        detail.status_code == 200
        and len(detail_data.get("assessments", [])) >= 1
        and len(detail_data.get("certificates", [])) >= 1,
        f"HTTP {detail.status_code}, "
        f"assessments={len(detail_data.get('assessments', []))}, "
        f"certificates={len(detail_data.get('certificates', []))}",
    )

    stats = client.get("/api/v1/events/stats/summary", headers=_auth(admin_token))
    reporter.check(
        "GET /events/stats/summary (admin) -> 200",
        stats.status_code == 200,
        f"HTTP {stats.status_code}",
    )

    print("\n[8] Cross-role / cross-worker authorization matrix")
    other = client.post(
        "/api/v1/workers",
        headers=_auth(admin_token),
        json={
            "name": "Smoke Other Worker",
            "employee_id": f"EMP-SMOKE2-{stamp}",
            "role": "Gas Safety Worker",
        },
    )
    other_id = _body(other).get("id")
    if other_id:
        cross_progress = client.get(
            f"/api/v1/progress/{other_id}", headers=_auth(worker_token)
        )
        reporter.check(
            "worker token reading ANOTHER worker's progress -> 403",
            cross_progress.status_code == 403,
            f"HTTP {cross_progress.status_code}",
        )

    worker_dashboard = client.get(
        "/api/v1/dashboard/summary", headers=_auth(worker_token)
    )
    reporter.check(
        "worker token on /dashboard/summary -> 403",
        worker_dashboard.status_code == 403,
        f"HTTP {worker_dashboard.status_code}",
    )

    context["stamp"] = stamp
    return context


# ---------------------------------------------------------------------------
# Live WebSocket flow (admin-only feed)
# ---------------------------------------------------------------------------
def _describe_ws_error(exc: Exception) -> str:
    """Best-effort description of a rejected WebSocket handshake / close."""
    status = getattr(exc, "status_code", None)
    response = getattr(exc, "response", None)
    if status is None and response is not None:
        status = getattr(response, "status_code", None)
    code = getattr(exc, "code", None)
    parts = [type(exc).__name__]
    if status is not None:
        parts.append(f"http={status}")
    if code is not None:
        parts.append(f"close={code}")
    return ", ".join(parts)


async def run_websocket_checks(
    base_url: str, reporter: Reporter, context: dict
) -> None:
    """Verify the admin-only live feed refuses anonymous clients and broadcasts."""
    try:
        import websockets
    except ImportError:
        reporter.check(
            "live WebSocket feed",
            False,
            "the 'websockets' package is missing - run: pip install -r requirements.txt",
        )
        return

    ws_base = base_url.replace("https://", "wss://").replace("http://", "ws://")
    feed_url = f"{ws_base}/api/v1/events/live"
    admin_token = context.get("admin_token", "")
    worker_token = context.get("worker_token", "")
    worker_id = context.get("worker_id")
    fire_module_id = context.get("fire_module_id", 1)
    stamp = context.get("stamp", "0")
    session_id = f"sess_ws_smoke_{stamp}"

    print("\n[9] Live WebSocket feed (admin-only)")

    # 9a. An anonymous handshake (or an immediate close) must be refused. A feed
    #     that simply stays open without credentials is a FAILURE.
    refused = False
    detail = ""
    try:
        async with websockets.connect(feed_url, open_timeout=5) as anonymous:
            try:
                await asyncio.wait_for(anonymous.recv(), timeout=5)
                detail = "received data without a token"
            except asyncio.TimeoutError:
                detail = "connection stayed open without a token"
    except Exception as exc:  # noqa: BLE001 - any refusal is the expected outcome
        refused = True
        detail = _describe_ws_error(exc)
    reporter.check("WS /events/live without a token is refused", refused, detail)

    # 9b. A valid admin token connects and receives the broadcast event.
    def ingest_event() -> None:
        with httpx.Client(base_url=base_url, timeout=15) as ws_client:
            ws_client.post(
                "/api/v1/events",
                headers=_auth(worker_token),
                json={
                    "worker_id": worker_id,
                    "session_id": session_id,
                    "events": [
                        {
                            "event_type": "hazard_identified",
                            "module_id": fire_module_id,
                            "scenario_type": "fire",
                            "severity": "info",
                            "payload": {
                                "hazard_type": "electrical_fire",
                                "source": "smoke_test",
                            },
                        }
                    ],
                },
            )

    try:
        async with websockets.connect(
            f"{feed_url}?token={quote(admin_token)}", open_timeout=5
        ) as socket:
            reporter.check("WS /events/live accepts an admin token", True)
            await asyncio.to_thread(ingest_event)
            payload = json.loads(await asyncio.wait_for(socket.recv(), timeout=15))
            reporter.check(
                "live broadcast reaches the WebSocket subscriber",
                payload.get("event_type") == "hazard_identified"
                and payload.get("session_id") == session_id,
                f"event_type={payload.get('event_type')}",
            )
    except Exception as exc:  # noqa: BLE001
        reporter.check(
            "WS /events/live accepts an admin token", False, _describe_ws_error(exc)
        )


def main() -> int:
    args = _parse_args()
    reporter = Reporter()
    base_url = args.base_url.rstrip("/")

    print("=" * 78)
    print(f"SurakshaAR Day 6 integration smoke test -> {base_url}")
    print("=" * 78)

    if not args.password:
        print(
            "\nERROR: no admin password available. Set ADMIN_PASSWORD in backend/.env "
            "or pass --password.",
            file=sys.stderr,
        )
        return 2

    try:
        with httpx.Client(base_url=base_url, timeout=20) as client:
            context = run_http_checks(client, reporter, args.username, args.password)
    except httpx.HTTPError as exc:
        print(
            f"\nERROR: could not reach the backend at {base_url}: {exc}", file=sys.stderr
        )
        print("Start it first:  python run.py   (from backend/)", file=sys.stderr)
        return 2

    if not context.get("worker_token"):
        print(
            "\nAborted before the WebSocket step: the HTTP flow did not complete.",
            file=sys.stderr,
        )
        return reporter.summary()

    if args.skip_websocket:
        print("\n[9] Live WebSocket feed: SKIPPED (--skip-websocket)")
    else:
        asyncio.run(run_websocket_checks(base_url, reporter, context))

    exit_code = reporter.summary()
    print("RESULT:", "ALL CHECKS PASSED" if exit_code == 0 else "FAILURES DETECTED")
    return exit_code


if __name__ == "__main__":
    sys.exit(main())

