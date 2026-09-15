"""Application configuration for the SurakshaAR backend.

Secrets (the JWT signing key, admin bootstrap credentials) are read from the
environment / ``backend/.env`` and are never committed to the repository.
See ``backend/.env.example`` for the full list of variables.
"""

import os
import secrets

try:  # python-dotenv is optional; process environment remains the source of truth.
    from dotenv import load_dotenv
except ImportError:  # pragma: no cover - dev-only convenience
    def load_dotenv(*_args, **_kwargs):  # type: ignore[no-redef]
        return None

APP_NAME = "SurakshaAR Backend API"
APP_VERSION = "1.0.0"

# SQLite database file stored in the backend directory.
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DATABASE_URL = os.getenv(
    "DATABASE_URL", f"sqlite:///{os.path.join(BASE_DIR, 'surakhshaar.db')}"
)

# ---------------------------------------------------------------------------
# Authentication / security settings (DAY 4)
# ---------------------------------------------------------------------------
load_dotenv(os.path.join(BASE_DIR, ".env"))

# JWT signing key - MUST be provided via the SURAKHSHAAR_SECRET_KEY
# environment variable in any real deployment. A random ephemeral key is
# generated only so the dev server can boot without configuration; tokens
# signed with it will not survive a restart.
SECRET_KEY = os.getenv("SURAKHSHAAR_SECRET_KEY") or secrets.token_urlsafe(48)
ALGORITHM = "HS256"
ACCESS_TOKEN_EXPIRE_MINUTES = int(os.getenv("ACCESS_TOKEN_EXPIRE_MINUTES", "480"))

# Optional bootstrap admin account seeded at startup (dashboard login).
ADMIN_USERNAME = os.getenv("ADMIN_USERNAME", "admin")
ADMIN_PASSWORD = os.getenv("ADMIN_PASSWORD")  # None => admin account is not seeded

# True when a real signing key was supplied through the environment / .env
# (as opposed to the ephemeral development fallback generated above).
HAS_PERSISTENT_SECRET = bool(os.getenv("SURAKHSHAAR_SECRET_KEY"))

# ---------------------------------------------------------------------------
# Server / CORS / deployment settings (DAY 6)
# ---------------------------------------------------------------------------


def _env_flag(name: str, default: str = "0") -> bool:
    """Parse a boolean environment variable (1/true/yes/on)."""
    return os.getenv(name, default).strip().lower() in {"1", "true", "yes", "on"}


# Bind address / port used by ``python run.py`` and the documented uvicorn command.
HOST = os.getenv("SURAKHSHAAR_HOST", "0.0.0.0")
PORT = int(os.getenv("SURAKHSHAAR_PORT", "8000"))

# Auto-reload is a development-only convenience; keep it off in production.
RELOAD = _env_flag("SURAKHSHAAR_RELOAD", "1")

# Comma-separated browser origins allowed to call the API. Defaults to ``*``
# because the dashboard is served either from the backend origin (``/dashboard``)
# or opened from the file system during an offline demo; an unconfigured
# deployment must never silently break a client.
CORS_ORIGINS = [
    origin.strip()
    for origin in os.getenv("SURAKHSHAAR_CORS_ORIGINS", "*").split(",")
    if origin.strip()
] or ["*"]

# When enabled the server refuses to boot without a real signing key and a
# bootstrap admin account (recommended for the real/graded deployment).
REQUIRE_SECRETS = _env_flag("SURAKHSHAAR_REQUIRE_SECRETS", "0")


def deployment_warnings() -> list[str]:
    """Return actionable warnings for an incomplete deployment configuration.

    Returns an empty list when the environment is production-ready. Used by the
    application lifespan (and the deployment tests) so a demo never silently runs
    without a signing key or without any account able to log in.
    """
    warnings: list[str] = []
    if not HAS_PERSISTENT_SECRET:
        warnings.append(
            "SURAKHSHAAR_SECRET_KEY is not set: a random ephemeral key was generated, "
            "so every issued Bearer token is invalidated on restart. Set it in "
            "backend/.env (see backend/.env.example)."
        )
    if not ADMIN_PASSWORD:
        warnings.append(
            "ADMIN_PASSWORD is not set: no bootstrap admin account is seeded, so no "
            "client can log in. Set ADMIN_USERNAME/ADMIN_PASSWORD in backend/.env."
        )
    return warnings
