"""Aggregates all v1 API routers."""

from fastapi import APIRouter

from app.api.v1 import assessments, certificates, dashboard, modules, progress, sync, vision, workers

api_router = APIRouter(prefix="/api/v1")
api_router.include_router(workers.router)
api_router.include_router(modules.router)
api_router.include_router(progress.router)
api_router.include_router(assessments.router)
api_router.include_router(sync.router)
api_router.include_router(certificates.router)
api_router.include_router(dashboard.router)
api_router.include_router(vision.router)