"""Day 4 - authentication tests (login, token validity, 401 handling)."""

from app.auth.security import create_access_token


def _login(client, username, password):
    return client.post(
        "/api/v1/auth/login", json={"username": username, "password": password}
    )


def test_login_admin_success(client):
    response = _login(client, "admin", "adminpass123")
    assert response.status_code == 200
    data = response.json()
    assert data["token_type"] == "bearer"
    assert data["role"] == "admin"
    assert data["username"] == "admin"
    assert data["worker_id"] is None
    assert data["access_token"]
    assert data["expires_in"] > 0


def test_login_worker_success(client):
    created = client.post(
        "/api/v1/workers",
        json={
            "name": "Worker One",
            "employee_id": "EMP-AUTH-1",
            "role": "Fire Safety Worker",
            "username": "worker_auth",
            "password": "workerpass123",
        },
    )
    assert created.status_code == 201
    response = _login(client, "worker_auth", "workerpass123")
    assert response.status_code == 200
    data = response.json()
    assert data["role"] == "worker"
    assert data["worker_id"] == created.json()["id"]


def test_login_invalid_password_401(client):
    response = _login(client, "admin", "wrong-password")
    assert response.status_code == 401
    assert response.json() == {"detail": "Invalid username or password"}


def test_login_unknown_user_401(client):
    response = _login(client, "nobody", "whatever123")
    assert response.status_code == 401


def test_login_missing_fields_422(client):
    response = client.post("/api/v1/auth/login", json={"username": "admin"})
    assert response.status_code == 422


def test_protected_endpoint_requires_token(anonymous_client):
    response = anonymous_client.get("/api/v1/modules")
    assert response.status_code == 401


def test_invalid_token_401(client):
    response = client.get(
        "/api/v1/modules", headers={"Authorization": "Bearer not-a-real-token"}
    )
    assert response.status_code == 401


def test_expired_token_401(client):
    expired = create_access_token("admin", "admin", expires_minutes=-1)
    response = client.get(
        "/api/v1/modules", headers={"Authorization": f"Bearer {expired}"}
    )
    assert response.status_code == 401


def test_tampered_token_401(client):
    good = create_access_token("admin", "admin")
    header, payload, _sig = good.split(".")
    tampered = f"{header}.{payload}.AAAA"
    response = client.get(
        "/api/v1/modules", headers={"Authorization": f"Bearer {tampered}"}
    )
    assert response.status_code == 401