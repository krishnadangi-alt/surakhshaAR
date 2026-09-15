"""SurakshaAR FastAPI application entry point."""

import logging
from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from app.api.v1.router import api_router
from app.config import (
    ADMIN_USERNAME,
    APP_NAME,
    APP_VERSION,
    CORS_ORIGINS,
    REQUIRE_SECRETS,
    deployment_warnings,
)
from app.models.auth_user import AuthUser
from app.database.connection import Base, SessionLocal, engine
from app.database.seed import seed_admin, seed_modules
from app import models  # noqa: F401  (ensure models are registered on Base)


logger = logging.getLogger("surakshaar.deployment")


def _log_deployment_status(db) -> None:
    """Warn loudly when the deployment lacks secrets or an admin account (DAY 6).

    A demo must never silently run with an ephemeral signing key or with no
    account able to log in. ``SURAKHSHAAR_REQUIRE_SECRETS=1`` turns the warnings
    into a hard startup failure for the real deployment.
    """
    warnings = deployment_warnings()
    admin = db.query(AuthUser).filter(AuthUser.username == ADMIN_USERNAME).first()
    if admin is None:
        warnings.append(
            "No admin account exists: set ADMIN_USERNAME/ADMIN_PASSWORD in backend/.env "
            "and restart - no client can log in until then "
            "(see backend/DAY4_AUTH_HANDOFF.md)."
        )

    if REQUIRE_SECRETS and warnings:
        raise RuntimeError(
            "Incomplete deployment configuration (SURAKHSHAAR_REQUIRE_SECRETS=1): "
            + " | ".join(warnings)
        )

    for message in warnings:
        logger.warning("[DEPLOYMENT] %s", message)
    if not warnings:
        logger.info(
            "[DEPLOYMENT] Configuration OK: persistent signing key + admin account present."
        )


@asynccontextmanager
async def lifespan(app: FastAPI):
    Base.metadata.create_all(bind=engine)
    db = SessionLocal()
    try:
        seed_modules(db)
        seed_admin(db)
        _log_deployment_status(db)
    finally:
        db.close()
    yield


app = FastAPI(
    title=APP_NAME,
    version=APP_VERSION,
    lifespan=lifespan,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=CORS_ORIGINS,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(api_router)


from pathlib import Path
from fastapi.staticfiles import StaticFiles
from fastapi.responses import FileResponse

@app.get("/health", tags=["health"])
def health_check():
    return {"status": "ok"}


DASHBOARD_DIR = Path(__file__).resolve().parent.parent.parent / "dashboard"
if DASHBOARD_DIR.exists() and (DASHBOARD_DIR / "index.html").exists():
    app.mount("/dashboard", StaticFiles(directory=str(DASHBOARD_DIR), html=True), name="dashboard")

    @app.get("/", include_in_schema=False)
    def root():
        return FileResponse(str(DASHBOARD_DIR / "index.html"))
