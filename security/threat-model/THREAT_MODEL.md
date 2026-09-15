# SurakshaAR — Threat Model

**Project:** SurakshaAR (SIH 2026, PS 26041 — Govt. of Jharkhand)
**Scope owner:** Member 5 — Cybersecurity & Certificate
**Status:** PROTOTYPE / MVP threat model — not a production security audit.
**Last updated:** [update date when you commit]

---

## 1. Project Security Scope

This document covers the security scope for the SurakshaAR MVP:

- Worker-facing Android AR training/assessment app (offline-first)
- Backend API (assessment, sync, certificate, dashboard endpoints)
- Certificate generation & QR-based verification
- Admin/compliance dashboard

**Out of scope for Day 1 / MVP:**
- Production-grade infrastructure hardening (cloud/network security)
- Formal penetration testing
- Compliance certification (ISO, SOC2, etc.)
- Biometric or advanced identity verification
- Blockchain-based verification or distributed ledger designs

> **ASSUMPTION:** This threat model assumes an SIH hackathon MVP prototype, deployed for demo/pilot purposes, not a production industrial rollout.

---

## 2. Security Objectives

1. **Integrity** — Assessment results and certificates must not be forgeable or silently modifiable.
2. **Authenticity** — A verified certificate must genuinely correspond to a real, passed assessment, and this must be checkable, not merely assumed.
3. **Availability (offline-first)** — Workers must be able to train/assess without network, without compromising later data integrity.
4. **Confidentiality** — Worker personal data and performance records should not leak to unauthorized parties.
5. **Accountability** — Admin actions and certificate issuance should be traceable.

---

## 3. Assets

| Asset | Why it matters |
|---|---|
| Worker data (identity, profile) | Privacy; misuse risk if leaked |
| Assessment data & events | Basis for competency scoring — must not be tampered |
| Competency results | Drives certification decision |
| Certificates (issued) | Represents a real safety qualification — high-value target for forgery |
| QR references | Public-facing pointer to certificate — must not leak sensitive info, must not be treated as self-proving |
| Admin accounts | Control dashboard, can view/manage worker + certificate data |
| Backend services / API | Central trust point — all data flows through here |

---

## 4. Users / Roles (MVP)

| Role | Description |
|---|---|
| **Worker** | Trains, practices, takes assessment, receives certificate |
| **Admin** | Views dashboard, worker performance, manages certificates/compliance view |

> **PROTOTYPE DECISION:** Supervisor role intentionally excluded from Day 1 — no confirmed requirement yet. Can be added later if the team decides.

---

## 5. Trust Boundaries

Trust boundaries mark where data moves from a less-trusted zone into a more-trusted zone. Controls matter most at these borders.



[Worker's Phone - Local/Offline]
|
| <-- Trust Boundary 1: Device to Network -->
v
[Backend API - Server]
|
| <-- Trust Boundary 2: Backend to Database -->
v
[Database]

[Admin Browser] --<-- Trust Boundary 3: Admin to Backend -->-- [Backend API]

[Anyone's Phone scanning QR] --<-- Trust Boundary 4: Public Verifier to Backend -->-- [Certificate Verification Endpoint]



- **Boundary 1 (Worker device ↔ Backend):** Data (assessment results) is generated offline on an untrusted device, later synced. The backend must not blindly trust it.
- **Boundary 2 (Backend ↔ Database):** Standard server-side trust — lower risk if backend is implemented correctly.
- **Boundary 3 (Admin ↔ Backend):** Admin dashboard must authenticate/authorize before showing worker data.
- **Boundary 4 (Public ↔ Verification endpoint):** Anyone (employer, inspector) can scan a QR and hit the verification endpoint — this is a **public-facing** boundary and must reveal minimal, trusted-only information. A QR scan by itself proves nothing until the backend confirms it.

---

## 6. Threats

Organized by practical category (not full STRIDE — kept MVP-appropriate):

### 6.1 Certificate & Verification Threats
- **T1 — Fake certificate creation:** Someone creates a fake certificate (fake ID, fake QR) claiming a worker is certified without passing assessment.
- **T2 — Certificate tampering:** Legitimate certificate data (name, date, score) is altered after issuance.
- **T3 — QR spoofing/replay:** A QR code is copied, recreated, or edited to point to a different or fabricated result. This includes the specific risk of a QR encoding a raw outcome flag (e.g. `PASS=true`) that anyone could forge, rather than an opaque reference the backend must resolve.

### 6.2 Assessment Data Threats
- **T4 — Modified assessment scores:** Worker or malicious actor edits local assessment data before sync to force a "PASS."
- **T5 — Duplicate/replayed submission:** Same assessment submitted multiple times to manipulate competency records or sync state.

### 6.3 Access Control Threats
- **T6 — Unauthorized dashboard access:** Someone without admin rights accesses the compliance dashboard and views worker data.
- **T7 — Privilege/role confusion:** Worker-role account somehow able to call admin-only API endpoints.

### 6.4 API & Data Threats
- **T8 — API manipulation:** Direct/manual API calls (bypassing the app) used to submit fake data or query data without proper auth.
- **T9 — Data leakage:** Sensitive worker/assessment data exposed via overly detailed API responses or QR verification responses.

### 6.5 Offline-First Threats
- **T10 — Offline data manipulation:** Locally stored data edited directly on device (rooted phone, file access) before sync.
- **T11 — Sync-time tampering:** Data intercepted/modified in transit between device and backend during sync.

---

## 7. Threat Severity / Risk Classification

Simple MVP-appropriate scale: **Impact (High/Med/Low) × Likelihood (High/Med/Low) → Risk**

| ID | Threat | Impact | Likelihood | Risk |
|---|---|---|---|---|
| T1 | Fake certificate creation | High | Medium | **High** |
| T2 | Certificate tampering | High | Medium | **High** |
| T3 | QR spoofing/replay | High | Medium | **High** |
| T4 | Modified assessment scores | High | Medium | **High** |
| T5 | Duplicate/replayed submission | Medium | Medium | Medium |
| T6 | Unauthorized dashboard access | High | Medium | **High** |
| T7 | Privilege/role confusion | Medium | Low | Medium |
| T8 | API manipulation | Medium | Medium | Medium |
| T9 | Data leakage | Medium | Medium | Medium |
| T10 | Offline data manipulation | High | Medium | **High** |
| T11 | Sync-time tampering | Medium | Low | Medium |

> **PROTOTYPE DECISION:** T1, T2, T3, T4, T6, T10 are treated as priority ("High") threats for MVP mitigation — these directly affect certificate trustworthiness, which is the project's core value proposition. T3 was raised from Medium to High in this revision, since QR forgeability is only safe if verification design is correct — it is not a low-stakes threat.

---

## 8. Attack Scenarios (Illustrative)

1. **Fake Certificate Scenario:** A worker fails the assessment but manually edits the local app database (or intercepts the sync request) to mark `status = passed`, then obtains a certificate they did not earn.
2. **QR Trust Scenario:** An employer scans a worker's certificate QR at a mine site. If the QR encodes only a `certificate_id` and the verification endpoint independently looks up and returns trusted, current certificate status, this is safe. If instead the QR encoded an outcome value directly (e.g. `PASS=true`), anyone could edit and re-print it — this must never be the design.
3. **Dashboard Snooping Scenario:** Someone finds the dashboard URL and, without login, views all workers' assessment history and personal data because there is no auth check.
4. **Sync Race Scenario:** A worker's phone submits the same assessment twice after regaining connectivity (retry bug or intentional tampering), inflating their record or confusing competency calculation.
5. **Revoked Certificate Scenario:** A certificate is later revoked (e.g., found to be fraudulently issued), but the printed QR still resolves to a cached "valid" response instead of the backend's current, authoritative status — a verifier is misled into trusting a certificate that should now read invalid/revoked.

---

## 9. Security Controls / Mitigations (MVP-appropriate)

| Threat(s) | Control |
|---|---|
| T1, T2, T3 | Certificate verification must always resolve through a **trusted backend lookup by certificate ID/reference** — the QR carries only that reference, never a self-contained pass/fail claim. See Section 11 for the full certificate integrity approach. |
| T4, T10 | Backend re-validates assessment logic/critical rules server-side where feasible; local data changes alone should not be sufficient to flip a result without passing server-side sanity checks at sync time. |
| T5, T11 | Each assessment submission carries a unique client-generated ID (idempotency key) so duplicate syncs are detected and ignored. |
| T6, T7 | Role-based authentication + authorization on all API endpoints — Worker vs Admin scopes enforced server-side, not just hidden in UI. |
| T8, T9 | Input validation on all API endpoints; verification endpoint returns **minimal necessary fields only** (see Section 11). |
| All | Basic audit logging for certificate issuance and admin actions (who/when), even if simple (append-only log/table). |

---

## 10. Offline-First Security Considerations

- Assessment data generated offline is **untrusted until synced and validated** by the backend — offline results are provisional on the device, not final.
- Local storage (on-device) should avoid storing highly sensitive fields unnecessarily.
- The sync process should use a unique submission/event ID so the backend can detect and reject duplicate or replayed data (see T5, T11).
- Sync-time validation is a backend responsibility: the backend, not the device, is the final authority on whether a submitted result is accepted.
- **ASSUMPTION:** Full device-level tamper protection (root detection, app attestation) is out of scope for MVP — flagged as a future improvement, not a Day-1 requirement.
- **LIMITATION:** This document defines the *risks and required posture* for offline sync; it does not specify sync protocol implementation, which belongs to backend/API design, not this threat model.

---

## 11. Certificate and QR Security Considerations

This section is intentionally treated as an **architecture-level concern**, not a deferred afterthought — certificate trustworthiness is the core value the platform delivers, so its integrity model must be decided now even though implementation happens in a later phase.

### 11.1 Core rule: the QR is a reference, never a verdict
- The QR code must encode only an **opaque certificate reference/identifier** (e.g., a certificate ID), never an embedded result such as `PASS=true`, a raw score, or any other self-contained "proof."
- **A QR code by itself is never proof of validity.** It is only a pointer. Validity is determined exclusively by looking that reference up against the backend at verification time.

### 11.2 What verification must actually check
When a certificate reference is scanned/looked up, the backend must check the current, authoritative record for that ID, not any data carried in the QR itself, and must be able to return distinct outcomes for:
- **Valid** — reference exists, matches a genuinely issued certificate, and is currently active.
- **Invalid** — reference does not match any known certificate.
- **Revoked** — reference matches a certificate that has since been invalidated.
- **Unknown/malformed** — reference is not in a recognizable format.

Treating "not clearly valid" as valid-by-default is not acceptable; any ambiguous or unmatched case must resolve to a non-valid response.

### 11.3 Certificate integrity — current MVP control vs. future work

To avoid overclaiming, the integrity approach is split explicitly by what is guaranteed now versus later:

- **Current MVP control:** Certificate authenticity is enforced by making the backend the single source of truth — a certificate is only ever considered valid if its reference resolves against the backend's own issuance record. There is no cryptographic signature scheme implemented at Day 1; integrity today rests on "the reference must match a backend-held record," not on the QR content being self-verifying.
- **Architecture/security consideration (decided now, implemented later):** The certificate record structure should be designed so that a tamper-evidence mechanism (such as a signature or hash over the certificate's core fields — worker ID, module, result, issue date) can be added without redesigning the data model. This is a design constraint on Phase 4 (Certificate Architecture), not new scope for this document.
- **Future implementation/improvement:** Cryptographic signing (e.g., HMAC or digital signature over certificate contents), so that even a compromised database record could be flagged as inconsistent, is deferred to a later iteration and is **not implemented in the MVP**. This document does not claim signing exists — it only requires that the data model not preclude adding it.

### 11.4 Privacy in verification responses
- Verification responses must return the **minimum information needed to confirm validity** (e.g., worker name, module, status, issue date) — not full personal data, raw assessment answers, or internal scoring detail.

---

## 12. Assumptions and Limitations

- **ASSUMPTION:** This is a hackathon MVP/prototype threat model, not a certified production security assessment.
- **ASSUMPTION:** Cryptographic certificate signing, advanced device attestation, and formal penetration testing are future improvements, not Day-1 implementation scope — though the certificate data model must not block adding them later (see Section 11.3).
- **LIMITATION:** This document reflects the known architecture as of Day 1; it should be revisited as backend/API details solidify.
- **LIMITATION:** This is a design/analysis document only — it does not contain or imply any implementation code, which belongs to later phases (Certificate Architecture, QR Verification Architecture, and backend implementation).
- Labels used throughout — **FACT**, **ASSUMPTION**, **PROTOTYPE DECISION**, **FUTURE IMPROVEMENT** — follow the project's documentation standard so prototype security decisions are never mistaken for production-grade guarantees.