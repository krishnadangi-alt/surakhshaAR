# SurakshaAR — Certificate & QR Verification Architecture

**Project:** SurakshaAR (SIH 2026, PS 26041 — Govt. of Jharkhand)
**Scope owner:** Member 5 — Cybersecurity & Certificate
**Status:** PROTOTYPE / MVP architecture document — design only, not implementation.
**Based on:** `security/threat-model/THREAT_MODEL.md`, `security/security-requirements/SECURITY_REQUIREMENTS.md`

---

## 1. Purpose and Scope

The certificate and QR system exists to give a worker a portable, checkable proof that they have completed and passed a specific SurakshaAR safety training/assessment module. An employer, inspector, or admin should be able to confirm this quickly on-site (e.g., at a mine or plant), without needing to trust the physical certificate or the worker's phone.

This document defines the **architecture and design** of the certificate and QR verification system for Day 1 of the SIH 2026 MVP. It does **not** contain backend code, database migrations, QR generation code, or any implementation. It builds directly on the decisions already made in `THREAT_MODEL.md` and `SECURITY_REQUIREMENTS.md` and does not introduce new security claims beyond them.

**Scope:** Certificate lifecycle, certificate data model (logical), certificate status handling, QR content rules, verification flow, and how these map to existing security requirements — all for the Day 1 MVP only.

**Out of scope:** Cryptographic signing implementation, device attestation, production infrastructure, backend technology choices, QR library choices, database schema implementation.

---

## 2. Certificate Lifecycle

The full conceptual lifecycle of a certificate:

```
Assessment
   ↓
Backend validation
   ↓
PASS decision
   ↓
Certificate ID generation
   ↓
Certificate record
   ↓
Certificate generation
   ↓
QR reference generation
   ↓
Certificate delivery
   ↓
Verification
   ↓
Valid / Invalid / Revoked / Unknown-Malformed
```

**Step-by-step, in simple terms:**

1. **Assessment** — Worker completes a training module and takes the final assessment, on-device (possibly offline).
2. **Backend validation** — When the result reaches the backend (immediately or after sync), it is checked against expected rules — never accepted just because the device says "pass" (per SR-API-02, SR-DATA-03).
3. **PASS decision** — Only the backend decides, authoritatively, whether the assessment counts as passed.
4. **Certificate ID generation** — If passed, the backend generates a new, unique certificate ID/reference (SR-CERT-02).
5. **Certificate record** — The backend creates a certificate record (see Section 4) linked to the worker, module, and assessment.
6. **Certificate generation** — A human-readable certificate (e.g., PDF/image) is generated for the worker, showing basic details and the QR code.
7. **QR reference generation** — A QR code is generated that encodes only the certificate ID/reference (see Section 6) — never the pass/fail data itself.
8. **Certificate delivery** — The certificate (with QR) is delivered to the worker (e.g., in-app, downloadable).
9. **Verification** — Anyone scanning the QR triggers a lookup against the backend's current record (see Section 7).
10. **Outcome** — The verification always resolves to one of: Valid, Invalid, Revoked, or Unknown/Malformed — never a raw "trust me" value.

---

## 3. Certificate Issuance Rules

- **SR-CERT-01:** A certificate can only be issued after the backend has authoritatively accepted an assessment as PASS. A device-reported PASS alone is never sufficient.
- Client/device-reported pass status must never directly trigger certificate creation — it only triggers a backend re-check (per SR-API-02, SR-DATA-03).
- **SR-CERT-02:** Every certificate ID must be unique.
- Certificate IDs must be generated and controlled entirely by the backend — never client-generated or client-supplied.
- **SR-CERT-04 / SR-AUDIT-04:** Certificate issuance must be logged (who/what triggered it, when, for which assessment).
- **SR-CERT-03:** Once issued, a certificate's status (valid/revoked) is determined solely by the backend's current record at the time of verification — never by anything printed on the certificate or encoded in the QR.

---

## 4. Certificate Data Model — Architecture Only

> **IMPORTANT:** The fields below describe the **proposed logical structure** of a certificate record. They are not a claim that any database table, schema, or code currently exists. Actual implementation (database, ORM, etc.) is a decision for the backend/certificate implementation member.

### MVP fields (Day 1 logical model)

| Field | Purpose |
|---|---|
| `certificate_id` | Unique, backend-generated reference — the only thing the QR encodes. |
| `worker_reference` | Reference to the worker this certificate belongs to (not full personal data duplicated here). |
| `module` | Which training/assessment module this certificate covers (e.g., Fire & Explosion Response). |
| `assessment_reference` | Reference to the specific backend-accepted assessment record that led to this certificate. |
| `issue_date` | When the certificate was issued. |
| `status` | Current authoritative status — e.g., `valid` or `revoked` (see Section 5). |
| `certificate_metadata` | Small amount of display metadata (e.g., issuing organization label) — non-sensitive only. |

### Future fields (explicitly NOT part of Day 1 MVP)

| Field | Purpose | Status |
|---|---|---|
| `signature` / `hash` | Cryptographic signature or hash over core fields, for tamper-evidence. | FUTURE — not implemented |
| `revocation_reason` | Optional structured reason for revocation. | FUTURE — optional enhancement |
| `signing_key_version` | Tracks which signing key/version was used, if signing is added later. | FUTURE — depends on signing being added |

The MVP data model is intentionally kept simple, but its structure should not prevent adding the future fields above later without a full redesign (see Section 13, SR-CERT-05).

---

## 5. Certificate Status Model

**Possible states (Day 1 MVP):**
- **Valid** — Certificate exists, was properly issued, and has not been revoked.
- **Revoked** — Certificate was valid but has since been invalidated by an admin/backend decision (e.g., found to be issued in error).

**Who/what changes the status:**
- Only the backend, through an authorized admin action, can change a certificate's status (e.g., from Valid to Revoked). Status is never changed by the worker app, the certificate file itself, or the QR code.

**Why status must be determined by the backend:**
- The certificate (PDF/image) and QR are static, physical/digital artifacts once issued — they cannot "know" if they've been revoked later. Only a live lookup against the backend's current record reflects the true, up-to-date status (directly addressing T2 — certificate tampering, and the "Revoked Certificate Scenario" in the Threat Model).

**Why embedded data cannot decide validity:**
- If validity were decided by data embedded in the certificate/QR itself, anyone with basic editing tools could forge a "valid" result (T1, T3). Backend-authoritative status is the only way to keep issued certificates checkable after the fact, including after revocation.

---

## 6. QR Architecture

This is the most security-critical section of this document.

**The QR must contain ONLY:**
- An opaque `certificate_id` / certificate reference (or an equivalent non-sensitive reference), and nothing else of security significance.

**The QR must NEVER contain:**
- `PASS=true` or any other embedded pass/fail flag
- Raw assessment score
- Worker's full personal information
- A trusted/self-declared validity status (e.g., "valid": true baked into the code)
- Any self-contained "proof" of certification
- Sensitive assessment data of any kind

**Why:**
A QR code is just printed/displayed data — anyone can read, copy, or recreate it. If the QR itself carried the verdict (e.g., `PASS=true`), forging a valid-looking certificate would be as easy as generating a new QR image with that text (T1, T3). By making the QR a bare reference, the QR becomes worthless without a live backend that agrees the reference is currently valid — the forgeable part (the verdict) never leaves the backend's control.

---

## 7. QR Verification Flow

```
QR Scan
   ↓
Extract certificate reference
   ↓
Validate format (well-formed reference?)
   ↓
Send reference to verification service/backend
   ↓
Backend looks up trusted certificate record
   ↓
Check existence
   ↓
Check status
   ↓
Return verification result
```

**Verification outcomes (must include all four):**
- **Valid** — Reference found, matches an issued certificate, status is currently valid.
- **Invalid** — Reference does not match any known certificate.
- **Revoked** — Reference matches a certificate whose status is currently revoked.
- **Unknown/Malformed** — Reference is not in a recognizable format, or cannot be processed.

**Non-negotiable rules (SR-QR-02, SR-QR-04, SR-FAIL-02):**
- A QR scan by itself is **never** proof of validity — the result always comes from the backend lookup, not the QR content.
- Unknown, malformed, or error cases must **never** default to Valid. If the backend cannot clearly confirm validity, the answer is not-Valid.
- Verification must fail safely: any lookup error, timeout, or unexpected backend condition must return a non-Valid outcome (Unknown/Malformed or an explicit error state), never Valid by default.

---

## 8. Verification Response Design

**Conceptual response may include:**
- `certificate_reference` (the ID that was looked up)
- Worker name or minimal identity information (e.g., first name + last initial, or full name if organizationally acceptable — a backend/product decision, not fixed here)
- `module`
- `issue_date`
- `status`
- `verification_result` (Valid / Invalid / Revoked / Unknown-Malformed)

**Must NOT expose (SR-QR-05, SR-WDATA-02, SR-FAIL-04):**
- Full worker personal data (address, contact info, ID numbers, etc.)
- Raw assessment answers
- Raw assessment score, unless a future requirement explicitly calls for it
- Internal database details (IDs, table names, internal error messages)
- Any security-sensitive backend information

This is a conceptual response shape, not a fixed API contract or implementation — the exact endpoint/response format is a backend design decision that must respect these limits.

---

## 9. Trust Boundaries

| Boundary | Trusted side | Untrusted side | Notes |
|---|---|---|---|
| Worker device → Backend | Backend | Worker device / locally stored data | Device-reported assessment/pass data is untrusted until backend validates it (SR-DATA-01). |
| Certificate → QR | Backend-issued certificate record | The physical/digital certificate & QR artifact once it leaves the backend | Once printed/displayed, the certificate and QR are just data — they carry no inherent trust. |
| QR scanner/public verifier → Backend | Backend verification lookup | The scanned QR content itself | QR content is treated as untrusted input — it is only a lookup key, never a trusted claim (SR-QR-02). |
| Backend → Database | Backend logic + database record | — | Standard server-side trust; the database record is the authoritative source once written by validated backend logic. |

**Key point:** the only fully trusted entity in this entire flow is the **backend's current certificate record**. Everything else — the device, the printed certificate, the QR, the person scanning it — is treated as untrusted or provisional until the backend confirms it.

---

## 10. Security Controls

This architecture directly implements the following requirements from `SECURITY_REQUIREMENTS.md`:

| Requirement | How this architecture satisfies it |
|---|---|
| SR-CERT-01 | Certificate issuance step (Section 2, 3) only happens after backend-authoritative PASS. |
| SR-CERT-02 | Certificate ID generation is backend-controlled and unique (Section 3, 4). |
| SR-CERT-03 | Certificate Status Model (Section 5) makes the backend record the sole authority for status. |
| SR-CERT-04 | Issuance and status changes are logged (Section 3, 12). |
| SR-CERT-05 | Data model (Section 4) separates MVP fields from future signature fields without claiming signing exists. |
| SR-QR-01 | QR Architecture (Section 6) restricts QR content to an opaque reference only. |
| SR-QR-02 | Verification Flow (Section 7) states a scan alone is never proof. |
| SR-QR-03 | Verification outcomes explicitly include Valid/Invalid/Revoked/Unknown-Malformed. |
| SR-QR-04 | Ambiguous/ unclear cases never default to Valid (Section 7). |
| SR-QR-05 | Verification Response Design (Section 8) limits exposed fields. |
| SR-FAIL-02 | Verification Flow states errors/lookup failures must not return Valid. |

**Threats mitigated:**
- **T1 (Fake certificate creation)** — mitigated by backend-only issuance and backend-only ID generation (Sections 2, 3).
- **T2 (Certificate tampering)** — mitigated by status always being resolved from the backend record, not from certificate/QR content (Section 5).
- **T3 (QR spoofing/replay)** — mitigated by the QR containing only an opaque reference, never a self-contained verdict (Section 6).
- **T9 (Data leakage)** — mitigated by the minimal verification response design (Section 8).

---

## 11. Offline-First Considerations

- Certificate issuance always depends on a **backend-authoritative** assessment acceptance — a certificate is never created purely from an offline, unsynced "pass" on the worker's device.
- Offline worker/device data (including a locally displayed "you passed!" message) remains **provisional** until synced and accepted by the backend (SR-DATA-01, SR-OFFLINE-01).
- The worker app must not locally generate or claim a trusted, final certificate based only on a local pass result — any certificate shown before sync should be understood as pending/unconfirmed, not final.
- If a certificate needs to be **viewed offline** (e.g., the worker opens the app without network to show their certificate), the app may display a **cached copy** of certificate details for convenience — but this cached view must be clearly understood as a **display/convenience copy**, not a live verification. Actual verification (Section 7) always requires reaching the backend.
- No complicated offline-verification cryptography (e.g., offline signature checking) is introduced for Day 1 — this is explicitly deferred (see Section 13).

---

## 12. Revocation Architecture

- A certificate becomes **Revoked** through an authorized admin action against the backend record (e.g., an admin marks a certificate as revoked after discovering it was issued in error or the underlying assessment is later invalidated).
- The **backend is the sole authority** for revocation status — no other component (worker app, certificate file, QR) can revoke or un-revoke a certificate.
- The QR code itself **does not need to change** when a certificate's status changes — it still encodes the same certificate reference. What changes is the backend's record for that reference.
- Every verification request checks the **latest** backend status, so a revoked certificate will correctly resolve to "Revoked" on the very next scan, even if the printed QR is unchanged.
- Revocation actions must be **auditable** — logged with who performed the revocation and when (SR-AUDIT-04, SR-CERT-04).

---

## 13. Future Cryptographic Integrity

- The Day 1 MVP relies entirely on **backend-authoritative certificate records and live verification** — there is no cryptographic signing, hashing, or tamper-evidence mechanism implemented yet.
- Cryptographic signing (e.g., HMAC or digital signature over core certificate fields) is a **future improvement**, not part of Day 1 scope (per SR-CERT-05 and Threat Model Section 11.3).
- The certificate data model (Section 4) intentionally separates "MVP fields" from "future fields" (e.g., `signature`, `signing_key_version`) so that adding signing later does not require redesigning the whole certificate record.
- This document does not introduce blockchain, distributed ledgers, or other complex cryptographic infrastructure — these remain explicitly out of scope, matching the Threat Model and Security Requirements.

---

## 14. Architecture Diagram

```
[Worker App]                     (untrusted / provisional data)
    ↓
[Assessment]                     (offline-capable, provisional until synced)
    ↓
[Backend Validation]             (TRUSTED — authoritative check)
    ↓
[Certificate Service]            (TRUSTED — backend-controlled)
    ↓
[Certificate Record / Database]  (TRUSTED — single source of truth)
    ↓
[Certificate + QR]               (UNTRUSTED once issued — just a reference)
    ↓
[Verifier / Scanner]             (untrusted input source — anyone can scan)
    ↓
[Verification API]               (TRUSTED — resolves reference)
    ↓
[Backend Certificate Record]     (TRUSTED — authoritative lookup)
    ↓
[Valid / Invalid / Revoked / Unknown-Malformed]
```

**Trusted components:** Backend Validation, Certificate Service, Certificate Record/Database, Verification API.
**Untrusted/provisional components:** Worker App data before backend acceptance, the physical/digital Certificate + QR once issued, and the scanning party itself.

---

## 15. MVP vs Future Scope

| MVP / Day 1 Architecture | Future Improvements |
|---|---|
| Backend-authoritative certificate issuance | Cryptographic signing (HMAC/digital signature) over certificate fields |
| Unique, backend-generated certificate ID | Advanced tamper-evidence (hash chains, etc.) |
| Opaque QR reference only | Device attestation / root detection for the scanning or issuing device |
| Backend-resolved verification (live lookup) | Advanced revocation infrastructure (e.g., revocation lists, caching layers with signed freshness proofs) |
| Valid / Invalid / Revoked / Unknown-Malformed outcomes | Offline cryptographic verification |
| Basic audit logging for issuance/revocation | Production-grade logging/monitoring infrastructure |
| Minimal verification response (Section 8) | Expanded verification API (e.g., detailed audit trail exposed to certain roles) |

Future items above are **not implemented** in the Day 1 MVP and must not be presented as existing features.

---

## 16. Assumptions and Limitations

- This is an **architecture/design document**, not an implementation. No backend code, QR-generation code, or database schema has been written as part of this document.
- **Backend technology is not prescribed here** — framework, language, and hosting are decisions for the backend member.
- **QR library/format is not prescribed here** — any standard QR generation approach that satisfies Section 6 (opaque reference only) is acceptable.
- **Database implementation is not prescribed here** — the data model in Section 4 is logical, not a schema.
- **Cryptographic signing is not implemented in Day 1** — see Section 13.
- **Production security hardening** (advanced monitoring, key management infrastructure, formal penetration testing, etc.) is outside Day 1 scope.
- This document is consistent with, and does not contradict, `THREAT_MODEL.md` and `SECURITY_REQUIREMENTS.md`; where a decision here overlaps with those documents, those documents remain the source of truth for the underlying threat/requirement reasoning.

---

## 17. Implementation Handoff

**Backend member needs to implement:**
- Certificate issuance logic that only fires after a backend-authoritative PASS (Section 2, 3).
- Backend-controlled, unique certificate ID generation (Section 3, 4).
- Certificate record storage matching the MVP logical fields in Section 4 (actual schema is their choice).
- Verification endpoint implementing the flow in Section 7, returning Valid/Invalid/Revoked/Unknown-Malformed and never defaulting ambiguous cases to Valid.
- Minimal-exposure verification response per Section 8.
- Basic audit logging for issuance and revocation events (Section 12, SR-AUDIT-04).

**Certificate implementation member (if applicable) needs to implement:**
- Certificate rendering (PDF/image) using the MVP fields from Section 4.
- QR generation that encodes **only** the `certificate_id`/reference (Section 6) — nothing else.
- Any offline/cached display view for the worker app, clearly distinguished from live verification (Section 11).

**Security member (this role) will need to, in later phases:**
- Review the actual implementation against this architecture and `SECURITY_REQUIREMENTS.md` once built.
- Define security test cases (in `security/security-tests/`) covering the Acceptance Criteria in Section 18.
- Revisit Section 13 (Future Cryptographic Integrity) if/when signing is prioritized.

> **Note:** This handoff is a checklist for later work — none of these implementation tasks are being started as part of this document.

---

## 18. Architecture Acceptance Criteria

The architecture is considered correctly followed if, once implemented:

1. No certificate can be created without a backend-authoritative PASS.
2. Every certificate ID is unique and generated/controlled by the backend, never the client.
3. The QR contains only an opaque certificate reference — no pass/fail data, scores, or personal data.
4. A QR scan alone is never treated as proof of validity by any component.
5. Verification always resolves through a backend lookup, not local/cached trust.
6. Valid, Invalid, Revoked, and Unknown/Malformed are all distinguishable outcomes.
7. Ambiguous, malformed, unmatched, or error cases never resolve to Valid.
8. Sensitive worker or assessment data is never exposed through the QR or an oversized verification response.
9. Future cryptographic signing remains clearly separated from MVP scope, with no claim that it is already implemented.
```