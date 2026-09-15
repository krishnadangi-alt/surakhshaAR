"""Minimal, dependency-free authentication primitives (Day 4).

- Password hashing: PBKDF2-HMAC-SHA256 with a per-user random salt (stdlib
  ``hashlib``/``hmac``) - never stores plaintext passwords.
- Access tokens: JSON Web Tokens (JWT, HS256) signed with the
  ``SURAKHSHAAR_SECRET_KEY`` environment variable. No third-party JWT library
  is required, which keeps the offline MVP dependency-free.
"""

import base64
import hashlib
import hmac
import json
import secrets
import time

from app.config import ACCESS_TOKEN_EXPIRE_MINUTES, ALGORITHM, SECRET_KEY

_PBKDF2_ITERATIONS = 120_000
_HASH_ALGO = "sha256"


def _b64url_encode(data: bytes) -> str:
    return base64.urlsafe_b64encode(data).rstrip(b"=").decode("ascii")


def _b64url_decode(data: str) -> bytes:
    padding = "=" * (-len(data) % 4)
    return base64.urlsafe_b64decode(data + padding)


# ---------------------------------------------------------------------------
# Password hashing / verification
# ---------------------------------------------------------------------------
def hash_password(password: str) -> str:
    """Hash a plaintext password with a fresh random salt (PBKDF2-HMAC-SHA256)."""
    salt = secrets.token_bytes(16)
    dk = hashlib.pbkdf2_hmac(
        _HASH_ALGO, password.encode("utf-8"), salt, _PBKDF2_ITERATIONS
    )
    return (
        f"pbkdf2_sha256${_PBKDF2_ITERATIONS}$"
        f"{_b64url_encode(salt)}${_b64url_encode(dk)}"
    )


def verify_password(password: str, stored_hash: str) -> bool:
    """Constant-time password verification against a stored hash string."""
    try:
        _algo, iterations, salt_b64, dk_b64 = stored_hash.split("$")
        salt = _b64url_decode(salt_b64)
        expected = _b64url_decode(dk_b64)
    except (ValueError, AttributeError):
        return False
    calc = hashlib.pbkdf2_hmac(
        _HASH_ALGO, password.encode("utf-8"), salt, int(iterations)
    )
    return hmac.compare_digest(calc, expected)


# ---------------------------------------------------------------------------
# JWT (HS256) signing / verification
# ---------------------------------------------------------------------------
def create_access_token(
    subject: str,
    role: str,
    worker_id: int | None = None,
    expires_minutes: int = ACCESS_TOKEN_EXPIRE_MINUTES,
) -> str:
    """Mint a signed bearer token for the given subject/role."""
    header = {"alg": ALGORITHM, "typ": "JWT"}
    now = int(time.time())
    payload = {
        "sub": subject,
        "role": role,
        "iat": now,
        "exp": now + int(expires_minutes or 0) * 60,
    }
    if worker_id is not None:
        payload["wid"] = worker_id

    header_b64 = _b64url_encode(
        json.dumps(header, separators=(",", ":")).encode("utf-8")
    )
    payload_b64 = _b64url_encode(
        json.dumps(payload, separators=(",", ":")).encode("utf-8")
    )
    signing_input = f"{header_b64}.{payload_b64}".encode("utf-8")
    signature = hmac.new(
        SECRET_KEY.encode("utf-8"), signing_input, hashlib.sha256
    ).digest()
    return f"{header_b64}.{payload_b64}.{_b64url_encode(signature)}"


def decode_access_token(token: str) -> dict | None:
    """Validate a token and return its claims, or None when invalid/expired."""
    try:
        header_b64, payload_b64, sig_b64 = token.split(".")
        signing_input = f"{header_b64}.{payload_b64}".encode("utf-8")
        signature = _b64url_decode(sig_b64)
        expected = hmac.new(
            SECRET_KEY.encode("utf-8"), signing_input, hashlib.sha256
        ).digest()
        if not hmac.compare_digest(signature, expected):
            return None
        payload = json.loads(_b64url_decode(payload_b64))
        if payload.get("exp", 0) < int(time.time()):
            return None
        return payload
    except Exception:
        return None
