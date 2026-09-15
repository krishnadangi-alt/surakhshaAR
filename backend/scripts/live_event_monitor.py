"""Live event listener utility for backend team members.

Connects to the SurakshaAR WebSocket feed and prints incoming behavioural telemetry
events from the AR application in real-time.

Usage:
    python backend/scripts/live_event_monitor.py --token <admin-jwt>
    SURAKHSHAAR_ADMIN_TOKEN=<admin-jwt> python backend/scripts/live_event_monitor.py

The live feed is admin-only (see backend/DAY4_AUTH_HANDOFF.md), so a valid admin
Bearer token is required. Obtain one with:

    curl -X POST http://localhost:8000/api/v1/auth/login -H "Content-Type: application/json" ^
         -d "{\"username\":\"admin\",\"password\":\"<ADMIN_PASSWORD>\"}"
"""

import argparse
import asyncio
import json
import os
import sys
from urllib.parse import quote

try:
    import websockets
except ImportError:
    print("Installing websockets package...")
    import subprocess
    subprocess.check_call([sys.executable, "-m", "pip", "install", "websockets"])
    import websockets


WS_BASE = os.getenv("SURAKHSHAAR_WS_URL", "ws://localhost:8000/api/v1/events/live")


def resolve_token() -> str:
    """Resolve the admin Bearer token from ``--token`` or the environment."""
    parser = argparse.ArgumentParser(description="SurakshaAR live event monitor")
    parser.add_argument("--token", help="Admin Bearer token (from /api/v1/auth/login)")
    args = parser.parse_args()
    token = args.token or os.getenv("SURAKHSHAAR_ADMIN_TOKEN", "")
    if not token:
        parser.error(
            "an admin Bearer token is required (the live feed is admin-only): pass "
            "--token <jwt> or set SURAKHSHAAR_ADMIN_TOKEN, obtained from "
            "POST /api/v1/auth/login"
        )
    return token


async def monitor_events(token: str):
    url = f"{WS_BASE}?token={quote(token)}"
    masked = f"{token[:12]}..." if len(token) > 12 else "***"
    print(f"Connecting to live event stream at {WS_BASE} (token {masked})...")
    try:
        async with websockets.connect(url) as ws:
            print("Connected! Listening for live SurakshaAR AR/quiz events...\n" + "=" * 65)
            while True:
                msg = await ws.recv()
                data = json.loads(msg)
                
                event_type = data.get("event_type", "UNKNOWN")
                worker_id = data.get("worker_id", "Anonymous")
                session_id = data.get("session_id", "")
                severity = data.get("severity", "info")
                payload = data.get("payload", {})
                timestamp = data.get("timestamp", "")
                
                # Visual badge
                badge = "[CRITICAL]" if severity == "critical" else "[ACTION]" if severity == "major" else "[EVENT]"
                
                print(f"{badge:10} | Worker: {str(worker_id):5} | Type: {event_type:22} | Time: {timestamp}")
                if payload:
                    print(f"           Details: {json.dumps(payload)}")
                print("-" * 65)
    except ConnectionRefusedError:
        print(f"ERROR: Could not connect to {WS_BASE}. Is the FastAPI server running?")
        print("Run: python run.py   (from backend/)")
    except Exception as e:
        if getattr(e, "code", None) == 1008:
            print("ERROR: The server rejected the token (WebSocket close code 1008).")
            print("       The live event feed is admin-only (backend/DAY4_AUTH_HANDOFF.md):")
            print("       obtain a token from POST /api/v1/auth/login and pass --token.")
        else:
            print(f"Connection closed: {e}")


if __name__ == "__main__":
    try:
        asyncio.run(monitor_events(resolve_token()))
    except KeyboardInterrupt:
        print("\nMonitor stopped.")
