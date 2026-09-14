# SurakshaAR — Final Demo Rehearsal Guide (Presentation Script)
**Presenter / QA Lead:** Rehan  
**Audience:** SIH 2026 Evaluation Committee & Industry Mentors  
**Total Presentation Time:** 7 Minutes

---

## Presentation Outline & Timeline

```
[00:00 - 01:00]  1. Problem Statement & Architecture Overview
[01:00 - 02:30]  2. Live AR Training & Readiness Check Gate
[02:30 - 04:00]  3. Assessment Engine: Action Tracking & Zero-Tolerance Critical Errors
[04:00 - 05:00]  4. Closed-Loop Adaptive Retraining & Reassessment
[05:00 - 06:00]  5. Certificate Issuance & Mobile QR Verification
[06:00 - 07:00]  6. Day 1/7/30 Retention Schedule & Admin Dashboard Sync
```

---

## Detailed Step-by-Step Demonstration Script

### Act 1: The Core Safety Philosophy (00:00 - 01:00)
> *"Judges, industrial training today has a dangerous flaw: workers take 2D multiple-choice quizzes, get 90%, and are sent into coal mines or chemical plants. But in real life, making 9 correct choices and 1 fatal mistake means you don't survive. SurakshaAR solves this with **Action-Based Behavioural Assessment**, zero-tolerance Critical Error detection, and long-term retention verification."*

### Act 2: Training Flow & Readiness Check Gate (01:00 - 02:30)
- **Action:** Open Worker App. Select Fire Safety module.
- **Showcase:** Show the 5-stage progression:
  1. *Introduction* (Briefing)
  2. *Instruction* (Ministry of Mines SOPs)
  3. *Demonstration* (3D animated walkthrough)
  4. *Guided Practice* (Interactive hints & haptics)
  5. *Free Practice*
- **Key Point for Judges:** Attempt to tap "Final Assessment" before completing practice.
  - Show modal: *"Access Denied: You must complete guided practice and pass the Readiness Check (score $\ge 80\%$) before attempting certification."*

### Act 3: Live Assessment & Critical Error Demonstration (02:30 - 04:00)
- **Action:** Worker starts Assessment Mode.
  - Point out: **No hints, no guidance arrows, no reveals.**
- **Demo A: Good Run (Fire or Gas)**
  - Worker identifies hazard, selects SCBA respirator, isolates valve, evacuates upwind.
  - Telemetry logs live events: `HAZARD_IDENTIFIED`, `PPE_SELECTED`, `CORRECT_ACTION`.
- **Demo B: Zero-Tolerance Critical Error (Instant FAIL)**
  - Intentionally walk downwind into gas plume OR enter confined space without buddy.
  - Result Screen: **FAIL**.
  - Highlight to judges: Even if the worker did everything else right, the score header displays:
    `FAIL — CRITICAL SAFETY VIOLATION: Solitary entry into confined space / gas area`.

### Act 4: Adaptive Retraining & Reassessment (04:00 - 05:00)
- **Action:** Show the Results Screen.
- Point to **Competency Breakdown**:
  - Hazard Recognition: 100%
  - Emergency Protocol: 100%
  - Respiratory PPE: 40% (*WEAKNESS DETECTED*)
- System automatically assigns `retrain_ppe_01` (Mining PPE & Respiratory Protocol Drill).
- Worker completes the 2-minute drill.
- System unlocks Reassessment $\rightarrow$ Worker retakes drill $\rightarrow$ New Score: 95% (*IMPROVED & PASSED*).

### Act 5: Certificate Issuance & QR Verification (05:00 - 06:00)
- **Action:** Show the generated Certificate on the mobile screen:
  - Worker Name, Module, Issue Date, Expiration Date.
  - Unique Certificate ID: `SUR-2026-0101`.
  - Scannable QR Code.
- **Judge Interaction:** Ask any judge to pull out their personal phone camera and scan the on-screen QR code.
  - The QR opens the live verification page:
    `https://surakshaar.gov.in/api/v1/certificates/verify/SUR-2026-0101`
  - Validates active status, cryptographic signature, and worker identity.

### Act 6: Spaced Retention & Dashboard Compliance (06:00 - 07:00)
- **Action:** Switch to Kanishka's Web Admin Dashboard.
  - Show the live event stream in real time.
  - Show the worker's **Retention Timeline**:
    - *Day 1:* Immediate Retention Check (Due in 24 hours)
    - *Day 7:* Refresher Drill
    - *Day 30:* Annual Compliance Audit
- **Offline Proof:** Turn phone to Airplane Mode, complete an action, show data saved locally in SQLite/PlayerPrefs, turn Airplane Mode off, and show seamless auto-sync to backend.

---

## Backup Plan & Contingency

| Potential Issue | Immediate Backup Action |
|---|---|
| AR tracking lost on table | Tap "Manual Calibration" or switch to camera gyro fallback |
| Projector screen lag | Run web dashboard on presenter laptop connected via direct HDMI |
| Internet disconnected | App operates in 100% offline mode; show local certificate and sync queue |
