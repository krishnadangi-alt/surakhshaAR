"""Event ingestion and streaming endpoints for SurakshaAR behavioural telemetry."""

from datetime import datetime, timezone
from typing import Union
import uuid

from fastapi import APIRouter, Depends, Query, WebSocket, WebSocketDisconnect
from sqlalchemy import func
from sqlalchemy.orm import Session

from app.api.deps import get_db
from app.models.event import EventModel
from app.schemas.event import (
    EventBatchCreate,
    EventBatchOut,
    EventItemCreate,
    EventOut,
    EventStatsOut,
)
from app.services.event_broadcaster import broadcaster

router = APIRouter(prefix="/events", tags=["events"])


def utcnow():
    return datetime.now(timezone.utc)


@router.post("", response_model=EventBatchOut, status_code=201)
async def ingest_events(
    payload: Union[EventBatchCreate, EventItemCreate],
    db: Session = Depends(get_db),
):
    """Ingest live behavioural events from SurakshaAR clients.
    
    Accepts either a single event or a batch of events. Broadcasts to any connected
    dashboards over WebSockets in real time.
    """
    if isinstance(payload, EventBatchCreate):
        batch_session_id = payload.session_id or f"sess_{uuid.uuid4().hex[:12]}"
        items = payload.events
        worker_id = payload.worker_id
        device_id = payload.device_id
    else:
        batch_session_id = payload.session_id or f"sess_{uuid.uuid4().hex[:12]}"
        items = [payload]
        worker_id = payload.worker_id
        device_id = payload.device_id

    created_records = []
    for item in items:
        # Extract payload parameters
        extra_fields = item.model_dump(exclude={"event_type", "session_id", "worker_id", "module_id", "scenario_type", "severity", "device_id", "timestamp"})
        item_payload = dict(item.payload or {})
        item_payload.update(extra_fields)

        event_obj = EventModel(
            worker_id=item.worker_id or worker_id,
            session_id=item.session_id or batch_session_id,
            module_id=item.module_id,
            scenario_type=item.scenario_type or "fire",
            event_type=item.event_type,
            severity=item.severity or "info",
            payload=item_payload,
            device_id=item.device_id or device_id,
            timestamp=item.timestamp or utcnow(),
        )
        db.add(event_obj)
        created_records.append(event_obj)

    db.commit()

    # Broadcast event(s) to live WebSocket listeners
    for ev in created_records:
        await broadcaster.broadcast({
            "id": ev.id,
            "session_id": ev.session_id,
            "worker_id": ev.worker_id,
            "module_id": ev.module_id,
            "scenario_type": ev.scenario_type,
            "event_type": ev.event_type,
            "severity": ev.severity,
            "payload": ev.payload,
            "timestamp": str(ev.timestamp),
        })

    return EventBatchOut(
        received=len(items),
        stored=len(created_records),
        session_id=batch_session_id,
        status="success",
    )


@router.get("", response_model=list[EventOut])
def get_events(
    worker_id: int | None = Query(None),
    session_id: str | None = Query(None),
    scenario_type: str | None = Query(None),
    event_type: str | None = Query(None),
    severity: str | None = Query(None),
    limit: int = Query(100, ge=1, le=1000),
    offset: int = Query(0, ge=0),
    db: Session = Depends(get_db),
):
    """Query ingested events with optional filtering."""
    query = db.query(EventModel)
    if worker_id is not None:
        query = query.filter(EventModel.worker_id == worker_id)
    if session_id is not None:
        query = query.filter(EventModel.session_id == session_id)
    if scenario_type is not None:
        query = query.filter(EventModel.scenario_type == scenario_type)
    if event_type is not None:
        query = query.filter(EventModel.event_type == event_type)
    if severity is not None:
        query = query.filter(EventModel.severity == severity)

    return query.order_by(EventModel.timestamp.desc()).offset(offset).limit(limit).all()


@router.get("/session/{session_id}", response_model=list[EventOut])
def get_session_timeline(session_id: str, db: Session = Depends(get_db)):
    """Retrieve chronological event timeline for a specific training session."""
    return (
        db.query(EventModel)
        .filter(EventModel.session_id == session_id)
        .order_by(EventModel.timestamp.asc())
        .all()
    )


@router.get("/stats/summary", response_model=EventStatsOut)
def get_event_stats(db: Session = Depends(get_db)):
    """Summary statistics of ingested telemetry events."""
    total = db.query(func.count(EventModel.id)).scalar() or 0
    critical = (
        db.query(func.count(EventModel.id))
        .filter(EventModel.severity == "critical")
        .scalar()
        or 0
    )
    hazards = (
        db.query(func.count(EventModel.id))
        .filter(EventModel.event_type == "hazard_identified")
        .scalar()
        or 0
    )

    type_counts = (
        db.query(EventModel.event_type, func.count(EventModel.id))
        .group_by(EventModel.event_type)
        .all()
    )
    scenario_counts = (
        db.query(EventModel.scenario_type, func.count(EventModel.id))
        .group_by(EventModel.scenario_type)
        .all()
    )

    return EventStatsOut(
        total_events=total,
        critical_events_count=critical,
        hazards_identified_count=hazards,
        events_by_type={t: c for t, c in type_counts},
        events_by_scenario={s: c for s, c in scenario_counts},
    )


@router.websocket("/live")
async def live_event_feed(websocket: WebSocket):
    """WebSocket endpoint for real-time telemetry streaming to monitoring dashboards."""
    await broadcaster.connect(websocket)
    try:
        while True:
            # Keep connection alive; client can send pings
            await websocket.receive_text()
    except WebSocketDisconnect:
        broadcaster.disconnect(websocket)
    except Exception:
        broadcaster.disconnect(websocket)
