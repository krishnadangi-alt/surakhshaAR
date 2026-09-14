"""Live WebSocket broadcaster for real-time telemetry streaming to dashboards."""

import json
from typing import Any
from fastapi import WebSocket


class EventBroadcaster:
    def __init__(self):
        self._active_connections: list[WebSocket] = []

    async def connect(self, websocket: WebSocket):
        await websocket.accept()
        self._active_connections.append(websocket)

    def disconnect(self, websocket: WebSocket):
        if websocket in self._active_connections:
            self._active_connections.remove(websocket)

    async def broadcast(self, data: dict[str, Any]):
        message = json.dumps(data, default=str)
        dead = []
        for connection in self._active_connections:
            try:
                await connection.send_text(message)
            except Exception:
                dead.append(connection)
        for d in dead:
            self.disconnect(d)


broadcaster = EventBroadcaster()
