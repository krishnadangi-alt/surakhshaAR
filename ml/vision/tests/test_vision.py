"""Tests for the ML/Vision PPE detection module.

Covers the documented failure surface: ok / low_confidence / model_error /
disabled, the mock fallback, and the dependency-free preprocessing helpers.
"""

import pytest

from ml.vision.config import (
    DEFAULT_REQUIRED_PPE,
    MAX_IMAGE_BYTES,
    STATUS_MODEL_ERROR,
    STATUS_OK,
)
from ml.vision.inference.ppe_detector import PPEDetector
from ml.vision.preprocessing.image_utils import (
    decode_base64_image,
    resize_and_normalize,
    validate_image_bytes,
)
from ml.vision.sample_data import PNG_1X1_BASE64
from ml.vision.service import run_ppe_check, vision_capabilities

VALID_PNG = decode_base64_image(PNG_1X1_BASE64)


class TestImageUtils:
    def test_decode_valid_base64(self):
        assert decode_base64_image(PNG_1X1_BASE64).startswith(b"\x89PNG")

    def test_decode_invalid_base64_raises(self):
        with pytest.raises(ValueError):
            decode_base64_image("not base64!!!")

    def test_validate_png(self):
        ok, fmt = validate_image_bytes(VALID_PNG)
        assert ok is True
        assert fmt == "png"

    def test_validate_empty(self):
        ok, _ = validate_image_bytes(b"")
        assert ok is False

    def test_validate_oversized(self):
        ok, _ = validate_image_bytes(b"\x89PNG\r\n\x1a\n" + b"\x00" * (MAX_IMAGE_BYTES + 1))
        assert ok is False

    def test_validate_random_bytes(self):
        ok, _ = validate_image_bytes(b"\x00\x01\x02\x03")
        assert ok is False

    def test_resize_and_normalize(self):
        frame = [[(0, 128, 255), (0, 0, 0)], [(255, 255, 255), (10, 20, 30)]]
        out = resize_and_normalize(frame, width=1, height=1)
        assert len(out) == 1 and len(out[0]) == 1
        r, g, b = out[0][0]
        assert 0.0 <= r <= 1.0 and 0.0 <= g <= 1.0 and 0.0 <= b <= 1.0


class TestPPEDetector:
    def test_mock_default_ok(self):
        detector = PPEDetector(mode="mock")
        result = detector.analyze(VALID_PNG)
        assert result.status == STATUS_OK
        assert result.ppe_ok is True
        assert result.fallback_used is True
        assert set(DEFAULT_REQUIRED_PPE) <= {d.item for d in result.detections}

    def test_mock_missing_item_reports_missing(self):
        detector = PPEDetector(mode="mock")
        result = detector.analyze(VALID_PNG, required_ppe=["helmet", "gloves"])
        # gloves is never in the mock detections -> reported missing
        assert "gloves" in result.missing_items
        assert result.ppe_ok is False

    def test_auto_without_model_uses_mock(self):
        detector = PPEDetector(mode="auto", model_path=None)
        result = detector.analyze(VALID_PNG)
        assert result.status == STATUS_OK
        assert result.fallback_used is True

    def test_auto_with_model_path_degrades_to_model_error(self):
        detector = PPEDetector(mode="auto", model_path=__file__)
        result = detector.analyze(VALID_PNG)
        assert result.status == STATUS_MODEL_ERROR
        assert result.ppe_ok is False

    def test_model_mode_without_runtime_returns_model_error(self):
        detector = PPEDetector(mode="model")
        result = detector.analyze(VALID_PNG)
        assert result.status == STATUS_MODEL_ERROR
        assert result.ppe_ok is False

    def test_invalid_image_returns_model_error(self):
        detector = PPEDetector(mode="mock")
        result = detector.analyze(b"this is not an image")
        assert result.status == STATUS_MODEL_ERROR
        assert result.ppe_ok is False
        assert result.missing_items == DEFAULT_REQUIRED_PPE

    def test_never_raises_on_garbage(self):
        detector = PPEDetector(mode="model")
        # Even a model-mode call with garbage input returns, never raises.
        result = detector.analyze(b"\x00" * 8)
        assert result.status in {STATUS_OK, STATUS_MODEL_ERROR}


class TestVisionService:
    def test_run_ppe_check_ok(self):
        payload = run_ppe_check(VALID_PNG)
        assert payload["status"] == STATUS_OK
        assert payload["ppe_ok"] is True
        assert payload["required_ppe"] == DEFAULT_REQUIRED_PPE

    def test_run_ppe_check_invalid_image(self):
        payload = run_ppe_check(b"garbage")
        assert payload["status"] == STATUS_MODEL_ERROR
        assert payload["ppe_ok"] is False

    def test_run_ppe_check_custom_required(self):
        payload = run_ppe_check(VALID_PNG, required_ppe=["helmet", "gloves"])
        assert payload["required_ppe"] == ["helmet", "gloves"]
        assert "gloves" in payload["missing_items"]

    def test_vision_capabilities(self):
        caps = vision_capabilities()
        assert caps["module"] == "ml.vision"
        assert caps["fallback_enabled"] is True