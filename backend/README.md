# Backend

Backend services for SurakshaAR: REST API, data models, services, authentication, and database access.

> **Status:** FastAPI implementation with the ML competency engine fully integrated.
> Assessments are scored **server-side** from raw behavioural events
> (`app/services/competency_service.py` bridges to `ml/competency`), weakness detection and
> targeted retraining plans are exposed via the API, offline sessions are scored on sync, and
> certificates are gated on demonstrated competency.

## Structure

| Path | Purpose |
|---|---|
| `app/api/` | API routes / controllers (`/api/v1`) |
| `app/models/` | SQLAlchemy models (workers, modules, progress, assessments, certificates, sync logs) |
| `app/schemas/` | Pydantic request/response schemas |
| `app/services/` | Business logic — certificate numbering and the ML competency engine bridge |
| `app/auth/` | Authentication and authorization logic (reserved for the security team) |
| `app/database/` | Database connection and module seeding |
| `tests/` | Backend test suite (in-memory SQLite; `events.py` holds shared event fixtures) |
| `requirements.txt` | Python dependencies |

## Competency integration

The behavioural scoring pipeline lives in the repository-level `ml/competency` package:

```
events ─▶ CompetencyScorer ─▶ WeaknessDetector ─▶ RetrainingRecommender
```

- `POST /api/v1/assessments` stores the authoritative per-competency scores, pass/fail
  decision, weaknesses, critical errors and the raw events.
- `GET /api/v1/assessments/{assessment_id}/retraining-plan` returns the targeted retraining
  plan for a failed assessment.
- `POST /api/v1/sync` scores offline sessions that carry events (legacy log-only sessions
  are stored unchanged).
- `POST /api/v1/certificates` refuses (409) to issue a certificate without a passing
  assessment for the module.

The full contract is documented in [docs/api/API.md](../docs/api/API.md).

## Running

```bash
python -m venv .venv
.venv/Scripts/python -m pip install -r requirements.txt   # Windows
.venv/bin/python -m pip install -r requirements.txt       # Linux/macOS
.venv/Scripts/python run.py                               # http://localhost:8000 (docs at /docs)
```

## Testing

```bash
.venv/Scripts/python -m pytest tests/ -v
```