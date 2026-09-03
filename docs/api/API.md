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
- **Assessment scoring:** All assessments are scored **server-side** by the ML competency engine
  (`ml/competency`). Clients submit raw behavioural events; scores, pass/fail decisions and
  weaknesses returned by the API are authoritative. Client-computed scores are never trusted.

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

## Assessment Events (ML Competency Engine)

Assessment endpoints accept raw behavioural events. Each event carries an `event_type`
(plus `timestamp` and event-specific fields; unknown fields are preserved as-is).

| Event type | Fields | Scoring effect |
|---|---|---|
| `hazard_identified` | `correct`, `hazard_type` | `hazard_identification` +50 / −25 |
| `ppe_selected` | `correct`, `items` | `ppe_selection` +60 (correct, non-empty items) / −30 |
| `equipment_selected` | `correct` | `equipment_use` +50 / −25 |
| `evacuation_started` | `correct` | fire: `procedure_compliance` +50/−30 · gas: `evacuation` +50/−30 |
| `emergency_procedure` | `correct`, `action` | gas only: `emergency_response` +50/−25 (ignored for fire) |
| `wrong_action` | `severity` (`minor`/`major`) | minor −5/−3 · major −30/−25 on the procedure & decision competencies (fire: `procedure_compliance` + `decision_making` · gas: `emergency_response` + `hazard_identification`) |
| `critical_action` | `action`, `reason` | **Automatic FAIL** regardless of all scores |
| `training_started`, `assessment_started`, `assessment_completed` | — | Logged for audit; no score change |

**Pass rules** (overall pass threshold: `70.0`) — an assessment passes only if **all** hold:

1. No `critical_action` events (critical errors → automatic FAIL).
2. Overall score (mean of all competency scores) ≥ `70.0`.
3. Every competency score ≥ its per-competency pass threshold.

Per-competency thresholds — **fire**: `hazard_identification` 75, `ppe_selection` 80,
`procedure_compliance` 75, `equipment_use` 75, `decision_making` 45. **gas**:
`hazard_identification` 75, `ppe_selection` 80, `evacuation` 75, `equipment_use` 75,
`emergency_response` 70.

> ⚠️ These are prototype thresholds. They must be validated against official industrial SOPs
> and domain experts before production deployment (see `ml/competency/README.md`).

**Weakness severity** (per failing competency): `severe` < 50, `moderate` < 60, `mild` < threshold.

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

Assessments are **behaviour-based**: the client submits the raw VR session events and the
backend scores them with the ML competency engine. `scenario_type` is derived from the
module code (`fire`/`gas`) and may be overridden explicitly; `attempt_number` is
auto-incremented per worker + module when omitted.

**Request body**

```json
{
  "worker_id": 1,
  "module_id": 1,
  "client_session_id": "assess-sess-001",
  "events": [
    { "event_type": "hazard_identified", "correct": true, "hazard_type": "electrical_fire" },
    { "event_type": "ppe_selected", "correct": true, "items": ["helmet", "gloves", "jacket"] },
    { "event_type": "equipment_selected", "correct": true, "action": "grab_extinguisher" },
    { "event_type": "evacuation_started", "correct": true, "route": "north_exit" }
  ]
}
```

`client_session_id` (optional) is a client-generated idempotency key. Re-submitting the
same key for the same worker + module returns the stored assessment with `200 OK` and
creates no duplicate record; the events are not re-scored.

**Response `201 Created`**

```json
{
  "id": 1,
  "worker_id": 1,
  "module_id": 1,
  "attempt_number": 1,
  "scenario_type": "fire",
  "score": 90.0,
  "passed": true,
  "pass_reason": "Assessment passed (overall score: 90.0)",
  "weaknesses": [],
  "competency_scores": {
    "hazard_identification": { "name": "hazard_identification", "score": 100.0, "passed": true, "pass_threshold": 75.0 },
    "ppe_selection": { "name": "ppe_selection", "score": 100.0, "passed": true, "pass_threshold": 80.0 },
    "procedure_compliance": { "name": "procedure_compliance", "score": 100.0, "passed": true, "pass_threshold": 75.0 },
    "equipment_use": { "name": "equipment_use", "score": 100.0, "passed": true, "pass_threshold": 75.0 },
    "decision_making": { "name": "decision_making", "score": 50.0, "passed": true, "pass_threshold": 45.0 }
  },
  "critical_errors": [],
  "created_at": "2026-09-01T11:10:00Z"
}
```

A failing assessment returns `passed: false` with structured `weaknesses` and, when a
safety-critical mistake was made, a non-empty `critical_errors` list and a
`pass_reason` beginning with `CRITICAL ERRORS:`.

**Errors**

- `404` — `{"detail": "Worker not found"}` or `{"detail": "Module not found"}`
- `422` — validation error (e.g. empty `events`, unknown `scenario_type`)

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
      "scenario_type": "fire",
      "score": 90.0,
      "passed": true,
      "pass_reason": "Assessment passed (overall score: 90.0)",
      "weaknesses": [],
      "competency_scores": {
        "hazard_identification": { "name": "hazard_identification", "score": 100.0, "passed": true, "pass_threshold": 75.0 },
        "ppe_selection": { "name": "ppe_selection", "score": 100.0, "passed": true, "pass_threshold": 80.0 },
        "procedure_compliance": { "name": "procedure_compliance", "score": 100.0, "passed": true, "pass_threshold": 75.0 },
        "equipment_use": { "name": "equipment_use", "score": 100.0, "passed": true, "pass_threshold": 75.0 },
        "decision_making": { "name": "decision_making", "score": 50.0, "passed": true, "pass_threshold": 45.0 }
      },
      "critical_errors": [],
      "created_at": "2026-09-01T11:30:00Z"
    },
    {
      "id": 1,
      "worker_id": 1,
      "module_id": 1,
      "attempt_number": 1,
      "scenario_type": "fire",
      "score": 39.0,
      "passed": false,
      "pass_reason": "Insufficient overall competency (score: 39.0, required: 70.0)",
      "weaknesses": [
        {
          "competency_name": "procedure_compliance",
          "score": 20.0,
          "threshold": 75.0,
          "severity": "severe",
          "reason": "Score 20.0 below pass threshold 75.0",
          "affected_aspects": []
        }
      ],
      "competency_scores": {
        "procedure_compliance": { "name": "procedure_compliance", "score": 20.0, "passed": false, "pass_threshold": 75.0 }
      },
      "critical_errors": [],
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
  "scenario_type": "fire",
  "score": 90.0,
  "passed": true,
  "pass_reason": "Assessment passed (overall score: 90.0)",
  "weaknesses": [],
  "competency_scores": {
    "hazard_identification": { "name": "hazard_identification", "score": 100.0, "passed": true, "pass_threshold": 75.0 },
    "ppe_selection": { "name": "ppe_selection", "score": 100.0, "passed": true, "pass_threshold": 80.0 },
    "procedure_compliance": { "name": "procedure_compliance", "score": 100.0, "passed": true, "pass_threshold": 75.0 },
    "equipment_use": { "name": "equipment_use", "score": 100.0, "passed": true, "pass_threshold": 75.0 },
    "decision_making": { "name": "decision_making", "score": 50.0, "passed": true, "pass_threshold": 45.0 }
  },
  "critical_errors": [],
  "created_at": "2026-09-01T11:30:00Z"
}
```

**Errors**

- `404` — `{"detail": "Worker not found"}`
- `404` — `{"detail": "No assessments found for worker"}`

---

### 9. Sync Offline Session Data

`POST /api/v1/sync`

Offline assessment sessions that include raw behavioural `events` are **scored
server-side** by the ML competency engine on sync and stored as real assessment records
(`assessments_created`). This is the offline-first path: the device records events without
connectivity, and the authoritative scoring happens when connectivity returns.

Sessions **without** `events` are logged as-is (legacy clients that report their own scores
are stored in the sync log only, and do not create assessment records).

**Request body**

```json
{
  "worker_id": 1,
  "device_id": "device-abc-123",
  "batch_id": "batch-xyz-456",
  "sessions": [
    {
      "type": "assessment",
      "module_id": 1,
      "occurred_at": "2026-09-01T10:00:00Z",
      "client_session_id": "sess-001",
      "events": [
        { "event_type": "hazard_identified", "correct": true, "hazard_type": "electrical_fire" },
        { "event_type": "ppe_selected", "correct": true, "items": ["helmet", "gloves", "jacket"] },
        { "event_type": "equipment_selected", "correct": true, "action": "grab_extinguisher" },
        { "event_type": "evacuation_started", "correct": true, "route": "north_exit" }
      ]
    },
    {
      "type": "assessment",
      "module_id": 2,
      "score": 80.0,
      "passed": false,
      "weaknesses": ["wrong_evacuation_route"],
      "occurred_at": "2026-09-01T10:30:00Z"
    }
  ]
}
```

`batch_id` (optional) makes the sync batch idempotent: re-sending the same `batch_id` for a
worker returns the original sync result with `200 OK` and creates no new sync log or
assessment rows. A per-session `client_session_id` (optional) skips an assessment already
scored for the same worker + module + key, preventing duplicate assessment/event records
when a device retries.

**Response `201 Created`**

```json
{
  "sync_id": 1,
  "worker_id": 1,
  "synced_at": "2026-09-01T11:15:00Z",
  "sessions_synced": 2,
  "assessments_created": 1
}
```

**Errors**

- `404` — `{"detail": "Worker not found"}` or `{"detail": "Module not found for synced session (module_id=…)"}` (an event session referencing an unknown module)
- `422` — validation error (e.g. unsupported `scenario_type`)

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
- `409` — `{"detail": "Certificate requires a passing assessment for this module"}`
  (competency gate — certificates are issued only after a passing, engine-scored
  assessment, so a certificate always reflects demonstrated competency)

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
  "common_weaknesses": [
    { "competency_name": "procedure_compliance", "count": 12, "average_score": 45.3 },
    { "competency_name": "ppe_selection", "count": 7, "average_score": 55.0 }
  ],
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
      "scenario_type": "fire",
      "score": 90.0,
      "passed": true,
      "pass_reason": "Assessment passed (overall score: 90.0)",
      "weaknesses": [],
      "competency_scores": {
        "hazard_identification": { "name": "hazard_identification", "score": 100.0, "passed": true, "pass_threshold": 75.0 },
        "ppe_selection": { "name": "ppe_selection", "score": 100.0, "passed": true, "pass_threshold": 80.0 },
        "procedure_compliance": { "name": "procedure_compliance", "score": 100.0, "passed": true, "pass_threshold": 75.0 },
        "equipment_use": { "name": "equipment_use", "score": 100.0, "passed": true, "pass_threshold": 75.0 },
        "decision_making": { "name": "decision_making", "score": 50.0, "passed": true, "pass_threshold": 45.0 }
      },
      "critical_errors": [],
      "created_at": "2026-09-01T11:30:00Z"
    }
  ],
  "competency_profile": [
    {
      "module_id": 1,
      "module_code": "fire",
      "module_name": "Fire & Explosion Response",
      "attempt_number": 2,
      "overall_score": 90.0,
      "passed": true,
      "competencies": {
        "hazard_identification": { "name": "hazard_identification", "score": 100.0, "passed": true, "pass_threshold": 75.0 },
        "ppe_selection": { "name": "ppe_selection", "score": 100.0, "passed": true, "pass_threshold": 80.0 }
      },
      "weaknesses": []
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

### 17. Get Retraining Plan

`GET /api/v1/assessments/{assessment_id}/retraining-plan`

Recomputes the targeted retraining plan from the assessment's stored events using the ML
weakness detector and retraining recommender. Use it after a failed assessment to launch
focused practice on exactly the competencies that failed.

**Response `200 OK`**

```json
{
  "scenario_type": "fire",
  "recommended_modules": [
    {
      "module_id": "fire_proc_001",
      "name": "Fire Evacuation Procedures",
      "description": "Step-by-step evacuation protocol and safe exit procedures",
      "estimated_duration_minutes": 10,
      "difficulty_level": "beginner",
      "competencies_addressed": ["procedure_compliance"],
      "reason": "Weakness in procedure_compliance: score 20.0 (severity: severe)"
    }
  ],
  "total_estimated_duration_minutes": 43,
  "time_limit_exceeded": false,
  "weaknesses_addressed": 3,
  "total_weaknesses": 5
}
```

An assessment with no weaknesses returns an empty `recommended_modules` list.

**Errors**

- `404` — `{"detail": "Assessment not found"}`

---

### 18. PPE Detection Check (Vision / ML)

`POST /api/v1/vision/ppe-check`

Detects whether the worker is wearing the required PPE from a camera frame.
Powered by `ml/vision`; follows the module's "AI is never on the critical path"
rule — the response always carries a `status` (`ok` | `low_confidence` |
`model_error` | `disabled`) so the Unity app can degrade to the manual
tap-to-select checklist when AI is unavailable. With no model checkpoint
deployed the deterministic mock fallback answers (demo/offline mode).

**Request body**

```json
{
  "image_base64": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJ...",
  "required_ppe": ["helmet", "safety_vest"],
  "mode": "auto"
}
```

| Field | Type | Description |
|---|---|---|
| `image_base64` | string | Base64-encoded image (PNG/JPEG). |
| `required_ppe` | string[] | Optional; defaults to `["helmet", "safety_vest"]`. |
| `mode` | string \| null | Optional; `auto` (default), `mock`, or `model`. `model` returns `model_error` until a real checkpoint is configured. |

**Response `200 OK`**

```json
{
  "status": "ok",
  "ppe_ok": true,
  "detections": [
    { "item": "helmet", "confidence": 0.98 },
    { "item": "safety_vest", "confidence": 0.97 }
  ],
  "missing_items": [],
  "confidence": 0.975,
  "fallback_used": true,
  "message": "Mock PPE detector: helmet and safety vest detected (no model configured).",
  "required_ppe": ["helmet", "safety_vest"]
}
```

**Errors**

- `422` — invalid `image_base64` (not valid base64/empty) or empty `required_ppe`

Note: an *unrecognised image* is **not** an error — it returns `status: "model_error"`
with `ppe_ok: false`, so the client triggers its documented fallback UI.

---

### 19. Vision Stack Status

`GET /api/v1/vision/status`

Reports how the vision stack is configured (used by the app's capability check
at startup — the feature auto-disables when no model is loaded).

**Response `200 OK`**

```json
{
  "status": "ok",
  "module": "ml.vision",
  "version": "0.1.0",
  "mode": "auto",
  "model_path": null,
  "model_loaded": false,
  "fallback_enabled": true,
  "supported_items": ["helmet", "safety_vest"]
}
```

**Errors**

- None

---

### 20. Get Worker Progress (workers-scoped)

`GET /api/v1/workers/{worker_id}/progress`

Returns the worker's per-module progress merged with the stored assessment data. For each
module the worker has progress and/or assessments, the response carries the latest attempt
number, overall score, pass/fail decision, the number of stored assessments, and — for
modules without an explicit progress row — the latest assessment timestamp as `last_updated`.

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
      "last_updated": "2026-09-01T11:30:00Z",
      "attempt_number": 2,
      "overall_score": 90.0,
      "passed": true,
      "assessments_count": 2
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
| 17 | GET | `/api/v1/assessments/{assessment_id}/retraining-plan` | 200 |
| 18 | POST | `/api/v1/vision/ppe-check` | 200 |
| 19 | GET | `/api/v1/vision/status` | 200 |
| 20 | GET | `/api/v1/workers/{worker_id}/progress` | 200 |

**6 POST + 14 GET = 20 endpoints.**