# SurakshaAR Backend API Reference

> **Version:** v1
> **Base URL:** `http://localhost:8000/api/v1`
> **Status:** Working contract for the MVP. This document is the single source of truth for the
> mobile/Unity worker app and the web dashboard. Field names and structures defined here are fixed
> and must not change without updating this document and all consumers.

## Conventions

- **Versioning:** All routes are prefixed with `/api/v1`.
- **Methods:** Only `GET` and `POST` are used in this MVP.
- **Content-Type:** `application/json` for all requests and responses.
- **Timestamps:** ISO 8601 UTC strings, e.g. `2026-09-01T11:00:00Z`.
- **Errors:** All application errors use the JSON shape `{"detail": "<message>"}`.
  - `404 Not Found` — resource does not exist
  - `409 Conflict` — duplicate resource (e.g. duplicate `employee_id` or certificate)
  - `422 Unprocessable Entity` — request validation failure (FastAPI standard shape)
- **Authentication:** Not implemented in this MVP. The `worker_id` is passed in the path or body.
  The backend structure is ready for the security team to integrate authentication later.

## Workflow Stages

The training workflow is: `learn → practice → assess → diagnose → retrain → reassess → certify → retain`.

Valid values for the `stage` field:

| Stage | Meaning |
|---|---|
| `learn` | Worker completed the learning content |
| `practice` | Worker completed the practice mode |
| `assess` | Worker is in / completed assessment |
| `diagnose` | Weaknesses diagnosed from assessment |
| `retrain` | Targeted retraining assigned |
| `reassess` | Re-assessment completed |
| `certify` | Certificate issued |
| `retain` | Retained / ongoing monitoring |

Valid values for the `status` field: `in_progress`, `completed`.

## MVP Training Modules

| id | code | name |
|---|---|---|
| 1 | `fire` | Fire & Explosion Response |
| 2 | `gas` | Gas Leak & Confined Space Protocol |

---

## Endpoints

### 1. Register Worker

`POST /api/v1/workers`

**Request body**

```json
{
  "name": "Ramesh Kumar",
  "employee_id": "EMP001",
  "role": "Fire Safety Worker"
}
```

**Response `201 Created`**

```json
{
  "id": 1,
  "name": "Ramesh Kumar",
  "employee_id": "EMP001",
  "role": "Fire Safety Worker",
  "created_at": "2026-09-01T11:00:00Z"
}
```

**Errors**

- `409` — `{"detail": "Worker with employee_id EMP001 already exists"}`
- `422` — validation error

---

### 2. Get Worker

`GET /api/v1/workers/{worker_id}`

**Response `200 OK`**

```json
{
  "id": 1,
  "name": "Ramesh Kumar",
  "employee_id": "EMP001",
  "role": "Fire Safety Worker",
  "created_at": "2026-09-01T11:00:00Z"
}
```

**Errors**

- `404` — `{"detail": "Worker not found"}`

---

### 3. List Modules

`GET /api/v1/modules`

**Response `200 OK`**

```json
{
  "modules": [
    {
      "id": 1,
      "code": "fire",
      "name": "Fire & Explosion Response",
      "description": "Fire and explosion safety training and assessment."
    },
    {
      "id": 2,
      "code": "gas",
      "name": "Gas Leak & Confined Space Protocol",
      "description": "Gas leak and confined space safety training and assessment."
    }
  ]
}
```

**Errors**

- None

---

### 4. Get Worker Progress

`GET /api/v1/progress/{worker_id}`

**Response `200 OK`**

```json
{
  "worker_id": 1,
  "progress": [
    {
      "module_id": 1,
      "module_code": "fire",
      "module_name": "Fire & Explosion Response",
      "stage": "assess",
      "status": "in_progress",
      "last_updated": "2026-09-01T11:00:00Z"
    }
  ]
}
```

**Errors**

- `404` — `{"detail": "Worker not found"}`

---

### 5. Update Worker Progress

`POST /api/v1/progress`

**Request body**

```json
{
  "worker_id": 1,
  "module_id": 1,
  "stage": "practice",
  "status": "completed"
}
```

**Response `200 OK`**

```json
{
  "worker_id": 1,
  "module_id": 1,
  "stage": "practice",
  "status": "completed",
  "updated_at": "2026-09-01T11:05:00Z"
}
```

**Errors**

- `404` — `{"detail": "Worker not found"}` or `{"detail": "Module not found"}`
- `422` — invalid `stage` or `status`

---

### 6. Submit Assessment Result

`POST /api/v1/assessments`

**Request body**

```json
{
  "worker_id": 1,
  "module_id": 1,
  "attempt_number": 1,
  "score": 85.0,
  "passed": true,
  "weaknesses": ["incorrect_extinguisher_selection"]
}
```

**Response `201 Created`**

```json
{
  "id": 1,
  "worker_id": 1,
  "module_id": 1,
  "attempt_number": 1,
  "score": 85.0,
  "passed": true,
  "weaknesses": ["incorrect_extinguisher_selection"],
  "created_at": "2026-09-01T11:10:00Z"
}
```

**Errors**

- `404` — `{"detail": "Worker not found"}` or `{"detail": "Module not found"}`
- `422` — validation error

---

### 7. Get Assessment History

`GET /api/v1/assessments/{worker_id}`

**Response `200 OK`**

```json
{
  "worker_id": 1,
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
    },
    {
      "id": 1,
      "worker_id": 1,
      "module_id": 1,
      "attempt_number": 1,
      "score": 60.0,
      "passed": false,
      "weaknesses": ["incorrect_extinguisher_selection"],
      "created_at": "2026-09-01T11:10:00Z"
    }
  ]
}
```

**Errors**

- `404` — `{"detail": "Worker not found"}`

---

### 8. Get Latest Assessment

`GET /api/v1/assessments/{worker_id}/latest`

**Response `200 OK`**

```json
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
```

**Errors**

- `404` — `{"detail": "Worker not found"}`
- `404` — `{"detail": "No assessments found for worker"}`

---

### 9. Sync Offline Session Data

`POST /api/v1/sync`

**Request body**

```json
{
  "worker_id": 1,
  "device_id": "device-abc-123",
  "sessions": [
    {
      "type": "assessment",
      "module_id": 1,
      "score": 80.0,
      "passed": false,
      "weaknesses": ["wrong_evacuation_route"],
      "occurred_at": "2026-09-01T10:00:00Z"
    }
  ]
}
```

**Response `201 Created`**

```json
{
  "sync_id": 1,
  "worker_id": 1,
  "synced_at": "2026-09-01T11:15:00Z",
  "sessions_synced": 1
}
```

**Errors**

- `404` — `{"detail": "Worker not found"}`
- `422` — validation error

---

### 10. Get Sync Status

`GET /api/v1/sync/status/{worker_id}`

**Response `200 OK`**

```json
{
  "worker_id": 1,
  "last_synced_at": "2026-09-01T11:15:00Z",
  "pending_sessions": 0
}
```

**Errors**

- `404` — `{"detail": "Worker not found"}`

---

### 11. Issue Certificate

`POST /api/v1/certificates`

**Request body**

```json
{
  "worker_id": 1,
  "module_id": 1
}
```

**Response `201 Created`**

```json
{
  "id": 1,
  "certificate_number": "SUR-2026-0001",
  "worker_id": 1,
  "module_id": 1,
  "issued_at": "2026-09-01T11:20:00Z",
  "valid_until": "2027-09-01T11:20:00Z",
  "status": "active"
}
```

**Errors**

- `404` — `{"detail": "Worker not found"}` or `{"detail": "Module not found"}`
- `409` — `{"detail": "Certificate already issued for this worker and module"}`

---

### 12. Get Worker Certificates

`GET /api/v1/certificates/{worker_id}`

**Response `200 OK`**

```json
{
  "worker_id": 1,
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

**Errors**

- `404` — `{"detail": "Worker not found"}`

---

### 13. Verify Certificate

`GET /api/v1/certificates/verify/{certificate_number}`

**Response `200 OK`**

```json
{
  "certificate_number": "SUR-2026-0001",
  "valid": true,
  "worker_name": "Ramesh Kumar",
  "module_name": "Fire & Explosion Response",
  "issued_at": "2026-09-01T11:20:00Z",
  "valid_until": "2027-09-01T11:20:00Z",
  "status": "active"
}
```

**Errors**

- `404` — `{"detail": "Certificate not found"}`

---

### 14. Dashboard Summary

`GET /api/v1/dashboard/summary`

**Response `200 OK`**

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

**Errors**

- None

---

### 15. Dashboard Worker List

`GET /api/v1/dashboard/workers`

**Response `200 OK`**

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

**Errors**

- None

---

### 16. Dashboard Worker Detail

`GET /api/v1/dashboard/workers/{worker_id}`

**Response `200 OK`**

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

**Errors**

- `404` — `{"detail": "Worker not found"}`

---

## Endpoint Summary

| # | Method | Path | Status |
|---|---|---|---|
| 1 | POST | `/api/v1/workers` | 201 |
| 2 | GET | `/api/v1/workers/{worker_id}` | 200 |
| 3 | GET | `/api/v1/modules` | 200 |
| 4 | GET | `/api/v1/progress/{worker_id}` | 200 |
| 5 | POST | `/api/v1/progress` | 200 |
| 6 | POST | `/api/v1/assessments` | 201 |
| 7 | GET | `/api/v1/assessments/{worker_id}` | 200 |
| 8 | GET | `/api/v1/assessments/{worker_id}/latest` | 200 |
| 9 | POST | `/api/v1/sync` | 201 |
| 10 | GET | `/api/v1/sync/status/{worker_id}` | 200 |
| 11 | POST | `/api/v1/certificates` | 201 |
| 12 | GET | `/api/v1/certificates/{worker_id}` | 200 |
| 13 | GET | `/api/v1/certificates/verify/{certificate_number}` | 200 |
| 14 | GET | `/api/v1/dashboard/summary` | 200 |
| 15 | GET | `/api/v1/dashboard/workers` | 200 |
| 16 | GET | `/api/v1/dashboard/workers/{worker_id}` | 200 |

**5 POST + 11 GET = 16 endpoints.**