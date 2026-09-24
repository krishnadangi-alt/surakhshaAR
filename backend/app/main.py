"""SurakshaAR FastAPI application entry point."""

import logging
logging.basicConfig(level=logging.INFO, format="%(asctime)s %(name)s %(levelname)s %(message)s")
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
from app.database.seed import seed_admin, seed_modules, seed_authoritative_certificates
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
        seed_authoritative_certificates(db)
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
from fastapi.responses import FileResponse, HTMLResponse
from fastapi import Request
from datetime import datetime, timezone
from app.models.certificate import Certificate
from app.models.worker import Worker
from app.models.module import Module
from app.templates.public_verify import render_public_verification_page


@app.get("/health", tags=["health"])
def health_check():
    return {"status": "ok"}


def _handle_public_verification(certificate_id: str, request: Request):
    """Handle public certificate verification requests by rendering the official
    Jharkhand Government verification webpage matching the reference design.
    """
    db = SessionLocal()
    try:
        cert = None
        if certificate_id.isdigit():
            cert = db.query(Certificate).filter(Certificate.id == int(certificate_id)).first()
        if not cert:
            cert = db.query(Certificate).filter(Certificate.certificate_number == certificate_id).first()

        if not cert:
            html = render_public_verification_page(None, certificate_id, status_type="INVALID")
            return HTMLResponse(content=html, status_code=404)

        worker = db.query(Worker).filter(Worker.id == cert.worker_id).first()
        module = db.query(Module).filter(Module.id == cert.module_id).first()

        now = datetime.now(timezone.utc)
        valid_until = cert.valid_until
        if valid_until is not None and valid_until.tzinfo is None:
            valid_until = valid_until.replace(tzinfo=timezone.utc)

        # Authoritative status resolution
        status_raw = (cert.status or "").upper()
        if status_raw == "REVOKED":
            status_type = "REVOKED"
        elif status_raw in ("PENDING", "PENDING_REVIEW"):
            status_type = "PENDING"
        elif valid_until is not None and valid_until < now:
            status_type = "EXPIRED"
        elif status_raw in ("ISSUED", "ACTIVE"):
            status_type = "VERIFIED"
        else:
            status_type = "INVALID"

        cert_dict = {
            "certificate_number": cert.certificate_number,
            "worker_name": cert.worker_name_snapshot or (worker.name if worker else "Trainee Worker"),
            "employee_id": cert.employee_id_snapshot or (worker.employee_id if worker else None),
            "module_name": cert.module_snapshot or (module.name if module else "Fire & Explosion Response"),
            "issued_at": cert.issued_at,
            "valid_until": cert.valid_until,
            "status": cert.status,
            "score": cert.score_snapshot,
            "competency_status": cert.competency_snapshot,
        }

        html = render_public_verification_page(cert_dict, cert.certificate_number, status_type=status_type)
        return HTMLResponse(content=html, status_code=200)
    finally:
        db.close()


# Authoritative Public Certificate Verification Routes (scanned by phone QR)
@app.get("/verify/{certificate_id}", include_in_schema=False, response_class=HTMLResponse)
def public_verify_page(certificate_id: str, request: Request):
    """Serve the public verification webpage for scanned certificate QR codes."""
    return _handle_public_verification(certificate_id, request)


@app.get("/certificate/{certificate_id}", include_in_schema=False, response_class=HTMLResponse)
def public_certificate_alias_page(certificate_id: str, request: Request):
    """Alias route matching verify.surakshaar.jharkhand.gov.in/certificate/{id}."""
    return _handle_public_verification(certificate_id, request)


DASHBOARD_DIST = Path(__file__).resolve().parent.parent.parent / "dashboard" / "dist"
if DASHBOARD_DIST.exists() and (DASHBOARD_DIST / "index.html").exists():
    app.mount("/dashboard", StaticFiles(directory=str(DASHBOARD_DIST), html=True), name="dashboard")

    @app.get("/", include_in_schema=False)
    def root():
        return FileResponse(str(DASHBOARD_DIST / "index.html"))

    app.mount("/assets", StaticFiles(directory=str(DASHBOARD_DIST / "assets")), name="assets")

else:
    @app.get("/", include_in_schema=False, response_class=HTMLResponse)
    def root():
        return """
        <!DOCTYPE html>
        <html>
        <head><title>SurakshaAR API</title>
        <style>body{font-family:sans-serif;padding:40px;background:#0f172a;color:#f8fafc;text-align:center;}
        a{color:#38bdf8;text-decoration:none;font-size:18px;font-weight:bold;}
        .card{background:#1e293b;border-radius:12px;padding:24px;max-width:500px;margin:40px auto;border:1px solid #334155;}
        .btn{display:inline-block;padding:12px 24px;margin-top:16px;background:#3b82f6;color:#fff;border-radius:8px;font-size:16px;}
        </style></head>
        <body>
        <div class="card">
            <h2>SurakshaAR Backend API &check;</h2>
            <p>The backend is running and healthy on port 8000.</p>
            <p>The <strong>Admin Dashboard</strong> runs on port <strong>5173</strong>:</p>
            <a class="btn" href="http://localhost:5173">Open Dashboard (Port 5173)</a>
            <p style="margin-top:20px;font-size:14px;color:#94a3b8;">
                Phone / Hotspot URL: <a href="http://192.168.137.1:5173">http://192.168.137.1:5173</a><br>
                API Docs: <a href="/docs">/docs</a>
            </p>
        </div>
        </body></html>
        """

