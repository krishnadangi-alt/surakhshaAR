"""Aggregates all v1 API routers."""

from fastapi import APIRouter

from app.api.v1 import (
    admin_reviews,
    assessments,
    auth,
    certificates,
    dashboard,
    events,
    modules,
    progress,
    sync,
    workers,
)

api_router = APIRouter(prefix="/api/v1")
api_router.include_router(auth.router)
api_router.include_router(workers.router)
api_router.include_router(modules.router)
api_router.include_router(progress.router)
api_router.include_router(assessments.router)
api_router.include_router(events.router)
api_router.include_router(sync.router)
api_router.include_router(certificates.router)
api_router.include_router(certificates.verify_router)  # public /verify endpoint for QR scans
api_router.include_router(admin_reviews.router)
api_router.include_router(dashboard.router)

