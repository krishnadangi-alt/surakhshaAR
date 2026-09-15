"""Pytest fixtures using an isolated in-memory SQLite database.

Day 4: authentication is enabled. The shared ``client`` fixture returns a
TestClient pre-authenticated as a bootstrap admin (Bearer token), so the
existing endpoint tests exercise the real authentication/authorization layer
without changing their assertions. New Day 4 tests also use the
``worker_client`` fixture for role/ownership assertions.
"""

import os

# Configure authentication before anything imports app.config. Test-only values;
# production secrets always come from the environment (see backend/.env.example).
TEST_SECRET_KEY = "surakhshaar-test-secret-key-0000000000-0000000000-0000"
os.environ.setdefault("SURAKHSHAAR_SECRET_KEY", TEST_SECRET_KEY)
os.environ.setdefault("ADMIN_USERNAME", "admin")
os.environ.setdefault("ADMIN_PASSWORD", "adminpass123")
os.environ.setdefault("ACCESS_TOKEN_EXPIRE_MINUTES", "60")

import pytest
from fastapi.testclient import TestClient
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker
from sqlalchemy.pool import StaticPool

from app.auth.security import create_access_token, hash_password
from app.database.connection import Base, get_db
from app.database.seed import seed_modules
from app.main import app
from app.models.auth_user import AuthUser

# In-memory SQLite with a shared connection pool for the test session.
test_engine = create_engine(
    "sqlite://",
    connect_args={"check_same_thread": False},
    poolclass=StaticPool,
)
TestingSessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=test_engine)


def override_get_db():
    db = TestingSessionLocal()
    try:
        yield db
    finally:
        db.close()


app.dependency_overrides[get_db] = override_get_db

ADMIN_USERNAME = "admin"
ADMIN_PASSWORD = "adminpass123"


def _auth_headers(token: str) -> dict:
    return {"Authorization": f"Bearer {token}"}


def _ensure_admin(db):
    admin = db.query(AuthUser).filter(AuthUser.username == ADMIN_USERNAME).first()
    if admin is None:
        admin = AuthUser(
            username=ADMIN_USERNAME,
            password_hash=hash_password(ADMIN_PASSWORD),
            role="admin",
        )
        db.add(admin)
        db.commit()
        db.refresh(admin)
    return admin


@pytest.fixture()
def client():
    # Fresh schema + seed for every test.
    Base.metadata.drop_all(bind=test_engine)
    Base.metadata.create_all(bind=test_engine)
    db = TestingSessionLocal()
    try:
        seed_modules(db)
    finally:
        db.close()

    db = TestingSessionLocal()
    try:
        admin = _ensure_admin(db)
        token = create_access_token(
            subject=admin.username, role=admin.role, worker_id=admin.worker_id
        )
    finally:
        db.close()
    return TestClient(app, headers=_auth_headers(token))


@pytest.fixture()
def worker_client(client):
    """Authenticated worker account plus the created worker dict.

    Returns a tuple ``(worker_test_client, worker)`` where ``worker`` is the
    JSON payload returned by ``POST /api/v1/workers``.
    """
    response = client.post(
        "/api/v1/workers",
        json={
            "name": "Worker One",
            "employee_id": "EMP-WRK-001",
            "role": "Fire Safety Worker",
            "username": "worker1",
            "password": "workerpass123",
        },
    )
    assert response.status_code == 201, response.text
    worker = response.json()

    login = client.post(
        "/api/v1/auth/login",
        json={"username": "worker1", "password": "workerpass123"},
    )
    assert login.status_code == 200, login.text
    token = login.json()["access_token"]
    return TestClient(app, headers=_auth_headers(token)), worker


@pytest.fixture()
def anonymous_client():
    """A bare TestClient with no credentials (for 401 tests)."""
    return TestClient(app)