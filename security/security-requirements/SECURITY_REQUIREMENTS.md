# SurakshaAR — Security Requirements

**Project:** SurakshaAR (SIH 2026, PS 26041 — Govt. of Jharkhand)
**Scope owner:** Member 5 — Cybersecurity & Certificate
**Status:** PROTOTYPE / MVP security requirements — derived from THREAT_MODEL.md
**Based on:** `security/threat-model/THREAT_MODEL.md`

---

## 1. Purpose and Scope

This document translates the threats identified in the Threat Model into concrete, implementable security requirements for the SurakshaAR MVP. It covers the backend API, worker app data handling, certificate system, QR verification, and admin dashboard access, at the level needed for Day 1 architecture — not full implementation detail.

**In scope:** Authentication, authorization, API validation, data protection, certificate/QR integrity, offline-sync security, audit logging, secure failure behavior.

**Out of scope (see Section 15):** Cryptographic signing implementation, device attestation, advanced infrastructure security, penetration testing.

---

## 2. Security Principles

- **Backend is the single source of truth** — the device/client never gets the final say on pass/fail, certificate validity, or role.
- **Least privilege** — every role/endpoint gets only the access it needs.
- **Fail safe, not fail open** — when something is invalid, ambiguous, or unverifiable, the safe default is to reject/deny, never to assume validity.
- **Minimal exposure** — return only the data a caller actually needs.
- **Design for future hardening** — data models should not block adding signing/attestation later, even though it isn't built now.

---

## 3. Authentication Requirements

| ID | Requirement | Reason | Priority |
|---|---|---|---|
| SR-AUTH-01 | Admin-facing dashboard and admin API endpoints must require authentication before granting access. | Prevents unauthorized dashboard access (T6). | MUST |
| SR-AUTH-02 | Authentication mechanism/technology is not fixed by this document — any standard mechanism (e.g., session or token-based) the backend member chooses is acceptable, as long as it enforces SR-AUTH-01. | Avoids over-specifying tech choices outside this role's scope. | MUST |
| SR-AUTH-03 | Failed authentication attempts must not reveal whether the failure was due to wrong username or wrong password (generic failure message). | Prevents account enumeration; supports secure failure (Section 13). | SHOULD |

> **ASSUMPTION:** No specific authentication technology is implemented as of Day 1 — this is a requirement for the backend member to satisfy, not a claim that it exists.

---

## 4. Authorization Requirements

| ID | Requirement | Reason | Priority |
|---|---|---|---|
| SR-AUTHZ-01 | Every API endpoint must enforce role checks (Worker vs Admin) on the server side, not only in the app UI. | Prevents privilege/role confusion (T7). | MUST |
| SR-AUTHZ-02 | A Worker-role request must never be able to perform an Admin-only action (e.g., viewing other workers' data, dashboard aggregate views), even if it guesses/crafts the request manually. | Directly mitigates T7 and reduces T6 risk via API. | MUST |
| SR-AUTHZ-03 | Hiding an admin feature in the UI is not a substitute for a server-side check. | Frontend-only restriction is trivially bypassed. | MUST |

---

## 5. Role-Based Access Requirements

| ID | Requirement | Reason | Priority |
|---|---|---|---|
| SR-ROLE-01 | MVP defines exactly two roles: **Worker** and **Admin** — no additional roles (e.g., Supervisor) at Day 1. | Matches MVP Role Model scope decision; avoids scope creep. | MUST |
| SR-ROLE-02 | Worker role can only access/submit their own assessment and certificate data — not other workers' data. | Prevents data leakage (T9) and unauthorized access. | MUST |
| SR-ROLE-03 | Admin role can view aggregate worker/assessment/certificate data through the dashboard, subject to SR-DATA-02 (minimal exposure). | Supports legitimate compliance use case without over-exposing data. | MUST |

---

## 6. API Security and Input Validation Requirements

| ID | Requirement | Reason | Priority |
|---|---|---|---|
| SR-API-01 | All incoming API requests must be validated server-side (type, format, required fields) before processing. | Prevents API manipulation (T8). | MUST |
| SR-API-02 | The backend must never accept an assessment result as "PASS" solely because the client-submitted payload says so — it must be checked against expected/allowed values and business rules. | Directly mitigates T4 (modified scores) and T8. | MUST |
| SR-API-03 | Malformed, unexpected, or out-of-range input must be rejected with a safe, generic error — not processed partially or silently accepted. | Supports secure failure (Section 13) and reduces attack surface. | MUST |
| SR-API-04 | API responses must not include internal debug information, stack traces, or raw database errors. | Prevents data/information leakage (T9). | MUST |

---

## 7. Assessment Data Integrity Requirements

| ID | Requirement | Reason | Priority |
|---|---|---|---|
| SR-DATA-01 | Assessment data submitted from a worker's device must be treated as **provisional/untrusted** until the backend validates and accepts it. | Core offline-first principle; mitigates T4, T10. | MUST |
| SR-DATA-02 | Each assessment submission must carry a unique client-generated submission/event ID so the backend can detect and reject duplicates. | Mitigates duplicate/replayed submissions (T5, T11). | MUST |
| SR-DATA-03 | The backend should re-check critical assessment outcomes against expected rules at sync time (not just store whatever arrives). | Mitigates T4, T10. | MUST |
| SR-DATA-04 | The exact sync protocol/mechanism is not specified here — this is a backend/API design decision that must satisfy SR-DATA-01 to SR-DATA-03. | Keeps this document at requirements level, not implementation. | — |

---

## 8. Worker Data Protection Requirements

| ID | Requirement | Reason | Priority |
|---|---|---|---|
| SR-WDATA-01 | Worker personal and assessment data must only be accessible to that worker and to authenticated Admin users (per SR-ROLE-02/03). | Prevents data leakage (T9) and unauthorized access (T6). | MUST |
| SR-WDATA-02 | API endpoints returning worker data must return only the fields needed for that specific use case (e.g., dashboard summary vs. detailed record). | Minimal exposure principle; mitigates T9. | MUST |
| SR-WDATA-03 | This document does not claim that data is encrypted at rest or in transit unless the backend member explicitly implements it — encryption should be treated as a SHOULD/FUTURE item unless confirmed. | Avoids overclaiming security that isn't built. | SHOULD / FUTURE (mark honestly per actual implementation) |

---

## 9. Certificate Security Requirements

| ID | Requirement | Reason | Priority |
|---|---|---|---|
| SR-CERT-01 | A certificate must only be issued for an assessment that the backend has authoritatively accepted as passed (per SR-DATA-01 to SR-DATA-03). | Mitigates fake certificate creation (T1). | MUST |
| SR-CERT-02 | Every certificate must have a unique, backend-generated certificate ID/reference. | Basis for trusted lookup; mitigates T1, T2. | MUST |
| SR-CERT-03 | Certificate status (valid/revoked) must be determined solely by the backend's current record — never by data embedded in the certificate or QR itself. | Mitigates certificate tampering (T2) and QR spoofing (T3). | MUST |
| SR-CERT-04 | Certificate issuance and any status change (e.g., revocation) must be logged (see Section 12 — Audit Logging). | Accountability; supports investigating T1/T2 incidents. | MUST |
| SR-CERT-05 | Cryptographic signing/tamper-evidence over certificate fields is **not implemented at MVP** — it is a future improvement. The certificate data model should be designed so signing can be added later without a redesign. | Matches Threat Model Section 11.3; avoids overclaiming. | FUTURE (design constraint is MUST) |

---

## 10. QR Verification Security Requirements

| ID | Requirement | Reason | Priority |
|---|---|---|---|
| SR-QR-01 | The QR code must encode only an opaque certificate reference/ID — never an embedded result such as `PASS=true`, a score, or any self-contained verdict. | Core rule from Threat Model Section 11.1; mitigates T3. | MUST |
| SR-QR-02 | A QR scan must never be treated as proof of validity by itself — validity is only established by resolving the reference against the backend. | Mitigates T3. | MUST |
| SR-QR-03 | Verification must return one of at least four distinct outcomes: **Valid, Invalid, Revoked, Unknown/Malformed.** | Prevents ambiguous cases from being mistaken for valid (Threat Model 11.2). | MUST |
| SR-QR-04 | Any case that is not clearly and positively "Valid" must never default to Valid. | Directly addresses the "fail safe" principle for certificates. | MUST |
| SR-QR-05 | Verification responses must expose only minimum necessary information (e.g., worker name, module, status, date) — not full personal data or raw assessment detail. | Mitigates data leakage (T9) via the public verification endpoint. | MUST |

---

## 11. Offline-First Security Requirements

| ID | Requirement | Reason | Priority |
|---|---|---|---|
| SR-OFFLINE-01 | Data stored locally on the worker's device must be treated as untrusted/provisional until synced and accepted by the backend. | Core offline-first principle; mitigates T10. | MUST |
| SR-OFFLINE-02 | Sync requests must include a unique submission/event identifier to allow duplicate detection (see SR-DATA-02). | Mitigates T5, T11. | MUST |
| SR-OFFLINE-03 | Device-level tamper protection (root detection, app attestation) is explicitly **not implemented** at MVP and is out of scope for Day 1. | Matches Threat Model 10 assumption; avoids overclaiming. | FUTURE |
| SR-OFFLINE-04 | The exact sync transport/protocol is a backend/API design decision, not specified here. | Keeps this document at requirements level. | — |

---

## 12. Audit Logging Requirements

| ID | Requirement | Reason | Priority |
|---|---|---|---|
| SR-AUDIT-01 | Admin authentication events (successful and failed login) should be logged with timestamp. | Accountability; helps detect unauthorized access attempts (T6). | SHOULD |
| SR-AUDIT-02 | Important admin actions (e.g., viewing/exporting worker data, revoking a certificate) should be logged with who performed the action and when. | Accountability principle (Threat Model Section 2). | SHOULD |
| SR-AUDIT-03 | Assessment acceptance and any server-side correction/rejection of a submitted result should be logged. | Supports investigating disputes tied to T4. | SHOULD |
| SR-AUDIT-04 | Certificate issuance and any status change (issued, revoked) must be logged. | Directly supports SR-CERT-04; mitigates T1, T2. | MUST |
| SR-AUDIT-05 | Logging infrastructure should remain simple (e.g., an append-only table) — no dedicated logging service/infrastructure is required for MVP. | Keeps scope realistic for an SIH prototype. | MUST (as a scope limiter) |

---

## 13. Error Handling / Secure Failure Requirements

| ID | Requirement | Reason | Priority |
|---|---|---|---|
| SR-FAIL-01 | Failed authentication or authorization must deny access by default — never grant access on an unclear/ambiguous result. | Fail-safe principle; mitigates T6, T7. | MUST |
| SR-FAIL-02 | Certificate verification must never return "Valid" for an unrecognized, malformed, or errored lookup. | Directly reinforces SR-QR-03/04. | MUST |
| SR-FAIL-03 | Invalid or malformed API input must be rejected with a generic, safe error message. | Mitigates T8, T9; avoids info leakage. | MUST |
| SR-FAIL-04 | Error responses must not expose stack traces, internal file paths, or database details. | Mitigates T9. | MUST |

---

## 14. Backend Security Requirements — see Section 16 (Checklist)

(Consolidated separately below so the backend member has one place to look.)

---

## 15. MVP vs Future Security Requirements

| MVP (Day 1 scope) | Future Improvement (explicitly deferred) |
|---|---|
| Backend-enforced auth + role checks (Worker/Admin) | Advanced identity (SSO, biometrics) |
| Server-side validation of assessment submissions | Full device attestation / root detection |
| Opaque QR reference + backend-resolved verification | Cryptographic certificate signing (HMAC/digital signature) |
| Basic append-only audit logging | Centralized logging/SIEM infrastructure |
| Minimal-exposure API responses | Field-level encryption at rest (unless already planned by backend member) |
| Duplicate-submission detection via unique ID | Formal replay-protection protocol (nonces, timestex windows, etc.) |

---

## 16. Assumptions and Limitations

- **ASSUMPTION:** No authentication, encryption, or signing technology is currently implemented — all such items are requirements for other phases/members to fulfill, not existing features.
- **ASSUMPTION:** This document assumes the API contract described in the project (assessments, sync, certificates, dashboard endpoints) remains as-is; no new endpoints are proposed here.
- **LIMITATION:** This is a requirements document, not an implementation spec or code. Actual technology choices (auth library, DB, hashing) are left to the responsible members.
- **LIMITATION:** Requirements are written to be testable in principle, but final test cases belong to `security/security-tests/` (a later task), not this document.

---

## 17. Traceability — Threat → Requirement Mapping

| Threat | Related Requirements |
|---|---|
| T1 — Fake certificate creation | SR-CERT-01, SR-CERT-02, SR-CERT-03, SR-CERT-04 |
| T2 — Certificate tampering | SR-CERT-02, SR-CERT-03, SR-CERT-04, SR-CERT-05 |
| T3 — QR spoofing/replay | SR-QR-01, SR-QR-02, SR-QR-03, SR-QR-04 |
| T4 — Modified assessment scores | SR-API-02, SR-DATA-01, SR-DATA-03 |
| T5 — Duplicate/replayed submission | SR-DATA-02, SR-OFFLINE-02 |
| T6 — Unauthorized dashboard access | SR-AUTH-01, SR-AUTHZ-01, SR-AUDIT-01, SR-FAIL-01 |
| T7 — Privilege/role confusion | SR-AUTHZ-01, SR-AUTHZ-02, SR-AUTHZ-03 |
| T8 — API manipulation | SR-API-01, SR-API-02, SR-API-03 |
| T9 — Data leakage | SR-WDATA-01, SR-WDATA-02, SR-QR-05, SR-API-04, SR-FAIL-04 |
| T10 — Offline data manipulation | SR-DATA-01, SR-DATA-03, SR-OFFLINE-01 |
| T11 — Sync-time tampering | SR-DATA-02, SR-OFFLINE-02 |

---

## 18. Backend Member — Required Security Implementation Checklist

This is the minimal, concrete list the **backend member** needs, without them needing to read the full document:

1. **Auth:** Admin endpoints/dashboard must require authentication (SR-AUTH-01). Failed login should return a generic error (SR-AUTH-03).
2. **Authorization:** Enforce Worker vs Admin role checks on every endpoint, server-side — never rely on the app hiding a button (SR-AUTHZ-01/02/03).
3. **Input validation:** Validate every incoming field server-side; reject malformed/unexpected input safely, no partial processing (SR-API-01, SR-API-03).
4. **Never trust client verdicts:** An assessment is not "PASS" just because the payload says so — re-check against expected rules server-side (SR-API-02, SR-DATA-03).
5. **Duplicate detection:** Require a unique submission/event ID on assessment and sync requests; reject/ignore duplicates (SR-DATA-02, SR-OFFLINE-02).
6. **Data exposure:** Return only fields needed for each specific endpoint/use case — no full-record dumps by default (SR-WDATA-02, SR-QR-05).
7. **Certificates:** Certificate ID must be unique and backend-generated; certificate validity is decided only by backend record, never by client/QR content (SR-CERT-01/02/03).
8. **QR verification endpoint:** Must return one of Valid / Invalid / Revoked / Unknown-Malformed — never default an unclear case to Valid (SR-QR-03/04).
9. **Audit logging:** Log certificate issuance/revocation and admin actions at minimum, with who/when (SR-AUDIT-02, SR-AUDIT-04).
10. **Errors:** No stack traces, DB errors, or internal details in API responses — generic safe error messages only (SR-FAIL-03/04).