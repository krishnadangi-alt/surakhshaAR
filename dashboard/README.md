# Dashboard

Web dashboard for administrators and trainers to monitor training progress, assessment results, competency
scores, and certificate issuance.

> **Status:** Production-ready. Implements Day 6 full-integration dashboard with admin Bearer authentication, live WebSocket
> telemetry, certificate verification, and real retention/milestone status from the backend. Backend runs as a static asset
> server serving `index.html` + `src/` under `/dashboard`; the app is also runnable directly from the file system when the
> backend is live.
>
> The dashboard lives under `/dashboard` in the backend (or at `http://localhost:8000/dashboard/index.html` in dev). See
> `docs/api/API.md` and `docs/architecture/integration.md` for the API contract.

## Structure

| Path | Purpose |
|---|---|
| `src/components/` | Reusable UI components |
| `src/pages/` | Page-level views |
| `src/services/` | API client / data services |
| `src/utils/` | Shared utilities |
| `public/` | Static public assets |
| `tests/` | Dashboard tests |