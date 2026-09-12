"""Live event listener utility for backend team members.

Connects to the SurakshaAR WebSocket feed and prints incoming behavioural telemetry
events from the AR application in real-time.

Usage:
    python backend/scripts/live_event_monitor.py
"""

import asyncio
import json
import sys

try:
    import websockets
except ImportError:
    print("Installing websockets package...")
    import subprocess
    subprocess.check_call([sys.executable, "-m", "pip", "install", "websockets"])
    import websockets


WS_URL = "ws://localhost:8000/api/v1/events/live"


async def monitor_events():
    print(f"Connecting to live event stream at {WS_URL}...")
    try:
        async with websockets.connect(WS_URL) as ws:
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
        print(f"ERROR: Could not connect to {WS_URL}. Is the FastAPI server running?")
        print("Run: uvicorn app.main:app --reload --port 8000 (from backend/)")
    except Exception as e:
        print(f"Connection closed: {e}")


if __name__ == "__main__":
    try:
        asyncio.run(monitor_events())
    except KeyboardInterrupt:
        print("\nMonitor stopped.")
