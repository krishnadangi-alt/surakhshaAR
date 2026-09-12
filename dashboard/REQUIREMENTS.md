# Dashboard Requirements

> **Status:** Requirements for the SurakshaAR web dashboard (administrators and trainers).
> The dashboard consumes the backend API defined in `docs/api/API.md`. This document defines the
> data requirements and the API endpoints the dashboard uses. It does not define the frontend
> implementation (that is the dashboard team's area).

## Purpose

The dashboard lets administrators and trainers monitor worker training progress, assessment results,
competency, and certificate issuance across the two MVP training modules:

1. Fire & Explosion Response
2. Gas Leak & Confined Space Protocol

## Data Requirements

The dashboard must display:

- **Overall summary** — total workers, workers in training, certified workers, total assessments,
  pass rate, and per-module enrollment/certification stats.
- **Worker list** — every worker with their per-module progress stage and certified modules.
- **Worker detail** — a single worker's progress, assessment history, and certificates.

## API Endpoints Consumed

| Dashboard view | Endpoint | Method |
|---|---|---|
| Summary / overview | `/api/v1/dashboard/summary` | GET |
| Worker list | `/api/v1/dashboard/workers` | GET |
| Worker detail | `/api/v1/dashboard/workers/{worker_id}` | GET |
| Worker lookup (optional) | `/api/v1/workers/{worker_id}` | GET |
| Assessment history (optional) | `/api/v1/assessments/{worker_id}` | GET |
| Certificate list (optional) | `/api/v1/certificates/{worker_id}` | GET |
| Certificate verification | `/api/v1/certificates/verify/{certificate_number}` | GET |

## Response Shapes

### Summary — `GET /api/v1/dashboard/summary`

```json
{
  "total_workers": 10,
  "workers_in_training": 6,
  "certified_workers": 4,
  "total_assessments": 25,
  "pass_rate": 80.0,
  "module_stats": [
    {
      "module_id": 1,
      "module_name": "Fire & Explosion Response",
      "workers_enrolled": 8,
      "certified": 3
    }
  ]
}
```

### Worker list — `GET /api/v1/dashboard/workers`

```json
{
  "workers": [
    {
      "id": 1,
      "name": "Ramesh Kumar",
      "employee_id": "EMP001",
      "role": "Fire Safety Worker",
      "progress": [
        {
          "module_id": 1,
          "module_code": "fire",
          "module_name": "Fire & Explosion Response",
          "stage": "certify",
          "status": "completed",
          "last_updated": "2026-09-01T11:20:00Z"
        }
      ],
      "certified_modules": ["fire"]
    }
  ]
}
```

### Worker detail — `GET /api/v1/dashboard/workers/{worker_id}`

```json
{
  "id": 1,
  "name": "Ramesh Kumar",
  "employee_id": "EMP001",
  "role": "Fire Safety Worker",
  "progress": [
    {
      "module_id": 1,
      "module_code": "fire",
      "module_name": "Fire & Explosion Response",
      "stage": "certify",
      "status": "completed",
      "last_updated": "2026-09-01T11:20:00Z"
    }
  ],
  "assessments": [
    {
      "id": 2,
      "worker_id": 1,
      "module_id": 1,
      "attempt_number": 2,
      "score": 92.0,
      "passed": true,
      "weaknesses": [],
      "created_at": "2026-09-01T11:30:00Z"
    }
  ],
  "certificates": [
    {
      "id": 1,
      "certificate_number": "SUR-2026-0001",
      "worker_id": 1,
      "module_id": 1,
      "issued_at": "2026-09-01T11:20:00Z",
      "valid_until": "2027-09-01T11:20:00Z",
      "status": "active"
    }
  ]
}
```

## Workflow Stages

The dashboard dislays the worker's current stage per module. Valid stages:

`learn → practice → assess → diagnose → retrain → reassess → certify → retain`

## Notes

- All timestamps are ISO 8601 UTC strings.
- Errors use `{"detail": "<message>"}` with HTTP 404 (not found) or 409 (conflict).
