"""Tests for the vision endpoints (PPE detection + stack status)."""

import base64

from ml.vision.config import STATUS_MODEL_ERROR, STATUS_OK
from ml.vision.sample_data import PNG_1X1_BASE64


def test_ppe_check_ok(client):
    response = client.post(
        "/api/v1/vision/ppe-check",
        json={"image_base64": PNG_1X1_BASE64},
    )
    assert response.status_code == 200
    data = response.json()
    assert data["status"] == STATUS_OK
    assert data["ppe_ok"] is True
    assert data["fallback_used"] is True
    assert data["required_ppe"] == ["helmet", "safety_vest"]
    items = {d["item"] for d in data["detections"]}
    assert {"helmet", "safety_vest"} <= items
    assert data["missing_items"] == []


def test_ppe_check_custom_required(client):
    response = client.post(
        "/api/v1/vision/ppe-check",
        json={
            "image_base64": PNG_1X1_BASE64,
            "required_ppe": ["helmet", "gloves"],
        },
    )
    assert response.status_code == 200
    data = response.json()
    assert data["required_ppe"] == ["helmet", "gloves"]
    assert "gloves" in data["missing_items"]
    assert data["ppe_ok"] is False


def test_ppe_check_invalid_base64_returns_422(client):
    response = client.post(
        "/api/v1/vision/ppe-check",
        json={"image_base64": "!!!not-base64!!!"},
    )
    assert response.status_code == 422


def test_ppe_check_empty_base64_returns_422(client):
    response = client.post(
        "/api/v1/vision/ppe-check",
        json={"image_base64": ""},
    )
    assert response.status_code == 422


def test_ppe_check_empty_required_returns_422(client):
    response = client.post(
        "/api/v1/vision/ppe-check",
        json={"image_base64": PNG_1X1_BASE64, "required_ppe": []},
    )
    assert response.status_code == 422


def test_model_mode_degrades_to_model_error(client):
    response = client.post(
        "/api/v1/vision/ppe-check",
        json={"image_base64": PNG_1X1_BASE64, "mode": "model"},
    )
    assert response.status_code == 200
    data = response.json()
    assert data["status"] == STATUS_MODEL_ERROR
    assert data["ppe_ok"] is False
    # Graceful degradation - the app can now fall back to tap-to-select.
    assert data["message"]


def test_invalid_image_bytes_is_graceful_model_error(client):
    junk = base64.b64encode(b"this is not an image").decode()
    response = client.post(
        "/api/v1/vision/ppe-check",
        json={"image_base64": junk},
    )
    assert response.status_code == 200
    assert response.json()["status"] == STATUS_MODEL_ERROR


def test_vision_status(client):
    response = client.get("/api/v1/vision/status")
    assert response.status_code == 200
    data = response.json()
    assert data["status"] == "ok"
    assert data["module"] == "ml.vision"
    assert data["fallback_enabled"] is True
    assert "supported_items" in data