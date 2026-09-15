"""Day 4 - secret / configuration safety checks (where testable)."""

import os
from pathlib import Path

import app.config as config

BACKEND_ROOT = Path(__file__).resolve().parent.parent
APP_ROOT = BACKEND_ROOT / "app"


def test_secret_key_comes_from_environment():
    assert config.SECRET_KEY == os.environ.get("SURAKHSHAAR_SECRET_KEY")
    assert len(config.SECRET_KEY) >= 32


def test_app_source_has_no_hardcoded_secret():
    """The app source must never contain an embedded signing key / password."""
    env_value = os.environ.get("SURAKHSHAAR_SECRET_KEY", "")
    suspicious_tokens = ["changeme", "secret_key = \"test\"", "ADMIN_PASSWORD = \"admin"]
    for py in APP_ROOT.rglob("*.py"):
        if "__pycache__" in str(py):
            continue
        text = py.read_text(encoding="utf-8", errors="ignore")
        assert env_value not in text, f"secret leaked into source: {py}"
        for token in suspicious_tokens:
            assert token not in text, f"hardcoded secret placeholder in source: {py}"


def test_env_files_not_tracked():
    gitignore = (
        BACKEND_ROOT.parent / ".gitignore"
    ).read_text(encoding="utf-8")
    assert ".env" in gitignore
    assert "secrets/" in gitignore
    assert "credentials/" in gitignore


def test_backend_env_example_has_placeholders_only():
    example = BACKEND_ROOT / ".env.example"
    assert example.exists()
    text = example.read_text(encoding="utf-8")
    assert "REPLACE" in text.upper()
    assert "SURAKHSHAAR_SECRET_KEY" in text