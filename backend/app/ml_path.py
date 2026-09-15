"""Ensure the repository-level ``ml`` package is importable from the backend.

The backend runs from ``backend/`` (uvicorn or pytest), so the ``ml`` package at
the repo root is not on ``sys.path`` by default. Importing this module first
(``import app.ml_path``) adds the repo root exactly once, regardless of which
entry point started the process. Both the vision and competency modules rely on
it.
"""

import sys
from pathlib import Path

_REPO_ROOT = Path(__file__).resolve().parents[2]

if str(_REPO_ROOT) not in sys.path:
    sys.path.insert(0, str(_REPO_ROOT))