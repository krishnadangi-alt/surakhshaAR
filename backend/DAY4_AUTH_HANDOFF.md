# Day 4 - Bearer-Token Integration Handoff (Auth / RBAC)

This document is the handoff for the **dashboard** and **Unity worker-app**
teams. As of Day 4, **all `/api/v1` endpoints except the two below now require
a `Authorization: Bearer <token>` header**:

| Public (no token)          | Note |
|----------------------------|------|
| `POST /api/v1/auth/login`  | verifies credentials, returns the token |
| `GET  /api/v1/certificates/verify/{number}` | read-only QR-style verification |

Everything else (`/workers`, `/modules`, `/progress`, `/assessments`, `/sync`,
`/certificates`, `/dashboard`, `/events`, `/events/live` WebSocket) returns
`401` when the token is missing/invalid and `403` when the token's role lacks
permission.

## 1. Login to obtain a token

```
POST /api/v1/auth/login
Content-Type: application/json

{ "username": "<employee login>", "password": "<password>" }
```

Response:

```json
{
  "access_token": "<jwt>",
  "token_type": "bearer",
  "role": "admin | worker",
  "username": "admin",
  "worker_id": null,
  "expires_in": 28800
}
```

## 2. Send the token on every request

```
Authorization: Bearer <access_token>
```

For the real-time WebSocket feed (`/api/v1/events/live`), the admin token is
accepted in the `Authorization` header **or** as a `?token=` query parameter.

## 3. Roles

- **admin** — full access: dashboards, worker management, certificate issuance,
  telemetry queries, the event WebSocket, and (like a worker) its own data.
- **worker** — self-service only: submit/sync its own assessments, read its own
  progress/assessments/certificates/sync status, ingest its own telemetry.
  Workers **cannot** read other workers' data (even by changing `worker_id` in
  the path/body → `403`), access dashboards (`403`), create workers or issue
  certificates (`403`).

All authorization is enforced server-side from the token claims added at login;
no client-supplied role/worker_id is ever trusted.

## 4. Provisioning accounts

- The bootstrap **admin** is created on startup from `ADMIN_USERNAME` /
  `ADMIN_PASSWORD` in `backend/.env` (see `.env.example`).
- A worker login can be provisioned at registration time by including optional
  `username` + `password` in `POST /api/v1/workers` (admin-only endpoint):

```json
{
  "name": "Ramesh Kumar",
  "employee_id": "EMP001",
  "role": "Fire Safety Worker",
  "username": "ramesh",
  "password": "<initial-password>"
}
```

## 5. What each team must change

- **dashboard/**: on load, call `/api/v1/auth/login` with the admin account and
  attach `Authorization: Bearer <token>` to every `/api/v1/*` fetch and to the
  `ws://…/api/v1/events/live` handshake (header or `?token=`).
- **worker-app/ (Unity)**: store the token after a worker login, attach the
  `Authorization` header to every API call and sync batch, and keep the token
  out of source control.

Day 4 backend implementation is complete and fully tested without requiring
either client change.