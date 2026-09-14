"""SurakshaAR FastAPI application entry point."""

from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from app.api.v1.router import api_router
from app.config import APP_NAME, APP_VERSION
from app.database.connection import Base, SessionLocal, engine
from app.database.seed import seed_modules
from app import models  # noqa: F401  (ensure models are registered on Base)


@asynccontextmanager
async def lifespan(app: FastAPI):
    Base.metadata.create_all(bind=engine)
    db = SessionLocal()
    try:
        seed_modules(db)
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
    allow_origins=["*"],
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