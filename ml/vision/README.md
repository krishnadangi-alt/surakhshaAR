# SurakshaAR — ML / Computer Vision

Computer-vision and on-device AI for SurakshaAR: turning the phone's **camera** and **microphone** into
safety-relevant signals — *is the worker wearing PPE? what did the worker just say?* — that the AR
training and assessment experience can react to.

> **Status:** Design & feasibility document (Day 1 of the ML/Vision workstream). No models, weights or
> inference code exist yet, and **nothing in this document has been tested**. All performance figures
> below are untested estimates or feasibility expectations, clearly labelled as such.
>
> Decisions here are grounded in what the repository actually contains: the main `README.md`,
> `worker-app/README.md`, the backend API contract (`docs/api/API.md`), the backend data models, and the
> team's SurakshaAR requirements analysis (SIH Problem Statement 26041 — Government of Jharkhand).

## Where the code will live (existing scaffold)

| Path | Purpose |
|---|---|
| `models/` | Model definitions and exported weights (ONNX / TFLite / Vosk model files) |
| `inference/` | Inference pipelines and thin wrappers around the models |
| `preprocessing/` | Input preprocessing: frame capture/resize/normalize, audio framing |
| `sample_data/` | Sample images / audio clips for development and tests |
| `tests/` | Tests for preprocessing, models and inference |

---

## 1. Overview

SurakshaAR is an offline-first, mobile-AR safety training and competency platform for mid-range Android
phones (Unity worker app + FastAPI backend + web dashboard + QR certificates), built for workers in
Jharkhand's mining, steel and mica sectors. The MVP ships **two complete AR modules** — **Fire & Explosion
Response** and **Gas Leak & Confined Space Protocol** — following the workflow
`learn → practice → assess → diagnose → retrain → reassess → certify → retain`, with Hindi + Santali
support, low-literacy-friendly UI and full offline operation.

Inside that product, the ML/CV component has exactly one job: **interpret ambiguous real-world signals** —
camera pixels and microphone audio — that ordinary code cannot interpret. Everything else in SurakshaAR
(buttons, timers, scenario state machines, scoring, sync, QR, AR overlay placement) is deterministic
programming and must **not** be built as "AI" (Section 3).

Two hard constraints shape every decision in this document:

1. **Offline-first.** No cloud inference, ever. Any AI feature must run entirely on the device or not ship.
2. **AI is never on the critical path.** The core loop (learn → practice → assess → certify) must be fully
   demonstrable with every AI feature switched off. AI enhances the experience; it never gates it.

## 2. Candidate AI Features

| # | Feature | AI Required? | Why? | Difficulty | MVP Suitability |
|---|---|---|---|---|---|
| 1 | Object recognition (recognize real objects via camera, e.g. extinguishers) | Yes | Interpreting camera pixels of arbitrary real objects is pattern recognition; cannot be rule-based | High (dataset + finetuning + edge cases) | **Low** — no MVP assessment step needs it |
| 2 | PPE recognition (detect helmet / vest worn by the trainee via camera) | Yes | Detecting a helmet in live camera pixels is object detection; cannot be rule-based | Medium (public datasets + pretrained models exist) | **High** — small, demoable, safety-relevant → **selected** |
| 3 | Hazard recognition (detect real fire / smoke / leaks via camera) | Yes | Real hazard appearance varies wildly; needs a trained detector | High (domain gap; cannot demo with real fires; hazards in scenarios are *virtual* AR overlays by design) | **Low** — future work |
| 4 | Worker action recognition (pose/gestures, e.g. crouch, aim extinguisher) | Yes | Recognizing body actions from video needs pose estimation + temporal modelling | High (flaky on mid-range devices; hard to demo reliably) | **Low** — future work |
| 5a | Voice output (Hindi/Santali spoken instructions and feedback) | **No** | Pre-recorded audio clips or the OS text-to-speech engine cover this; we build no model | Low | Already a *normal* feature of the worker app (`Localization` / `Audio`) — not this module |
| 5b | Voice input — offline Hindi speech commands (voice-first navigation) | Yes | Mapping raw audio to "which command word was spoken" is speech recognition — genuine pattern recognition | Medium (open offline models exist) | **High** — directly serves the PS low-literacy / voice-first requirement → **selected** |

### Additional opportunities found while inspecting the repository

| Opportunity | AI required? | Notes |
|---|---|---|
| Weakness detection / adaptive retraining (`ml/competency/`) | Not initially | Team plan is a transparent rule/threshold engine over recorded assessment events (the backend `assessments` table already stores `score`, `passed`, `weaknesses`). ML can be added later **if real data justifies it**. Belongs to `ml/competency/`, not this module. |
| AR overlay anchoring (exits, extinguishers, hazard zones) | **No** | AR Foundation plane / image tracking does this out of the box — built-in deterministic tracking, not our AI. |
| QR certificate generation / verification | **No** | Standard algorithmic encode/decode; handled by the backend + `certificate/` module. |
| Offline sync, dashboard analytics, 1/7/30-day retention checks | **No** | Deterministic data plumbing, date arithmetic and scheduled prompts. |

## 3. AI vs Normal Unity Logic

Rule of thumb used by this module:

> **Does the feature require interpreting an ambiguous real-world signal (pixels or audio)?**
> - **YES** → it is a genuine AI/CV problem and may belong here.
> - **NO** → it is deterministic logic and belongs in the Unity app, backend or another module — **not here**.

Features that should be handled with **normal Unity/programming logic** (some already exist as backend
functionality, some are planned in `worker-app/`):

| Feature | Correct home | Why it is NOT AI |
|---|---|---|
| Buttons, menus, navigation | Unity UI (`worker-app/Assets/Scripts/Core`) | A button press is a discrete UI event with one defined handler. There is no ambiguity to interpret. |
| Timers, step sequencing, scenario triggers | `worker-app/Assets/Scripts/Training` / `Assessment` | Deterministic `if/else` + coroutines. The developer defines every state and transition. |
| State management (learn / practice / assess modes) | `worker-app/Assets/Scripts/Core` | A finite state machine with author-defined states — enumerable and deterministic. |
| Scoring & competency calculation | `ml/competency/` (rules first) | Weighted checks and thresholds on recorded events ("did the worker select the correct extinguisher?"). The correct answer is known and coded — nothing must be *inferred* from noisy data. |
| Branching scenario consequences | `worker-app/Assets/Scripts/Training` | Designer-authored state machine. |
| Tap-to-select PPE / extinguisher choice | `worker-app/Assets/Scripts/Assessment` | Simple UI event against virtual AR menu items. |
| AR overlay placement & tracking | `worker-app/Assets/Scripts/AR` | AR Foundation plane/image tracking is built-in, deterministic tracking — not a model we build. |
| Hindi/Santali voice **output** | `worker-app/Assets/Scripts/Localization` + `Assets/Audio` | Playing pre-recorded clips (or OS TTS) is playback, not inference. |
| QR generation & verification | backend + `certificate/` | Standard algorithmic encode/decode. |
| Offline storage & sync | `worker-app/Assets/Scripts/Networking`, backend `/sync` endpoints | Deterministic local persistence + HTTP. |
| Progress / retention bookkeeping (1/7/30-day checks) | backend + dashboard | Date arithmetic and scheduled prompts. |

**Why these do not need AI:** in every row there is a single, developer-defined correct behaviour and a
finite set of states and inputs. AI (machine learning) earns its complexity only where inputs are noisy,
continuous and unenumerable — real-world images and speech — and the mapping from signal to meaning must
be *learned from examples* rather than *programmed*.

Rebranding ordinary logic as "AI" inflates APK size, latency, battery drain and debugging cost for zero
benefit — and it invites judges and reviewers to ask what the model actually does. This module therefore
ships **no** AI for any of the rows above.

## 4. Selected MVP AI Features

Maximum two features were selected, using three filters: (a) it must solve a real PS requirement,
(b) an open-source pretrained path must exist so we do **not** train from scratch, and (c) it must be
demoable offline on a mid-range Android phone within the SIH timeline.

### Selected 1 — Offline Hindi voice commands (voice-first navigation)

- **Problem it solves:** many target workers have limited literacy; text-heavy menus are a barrier. The PS
  explicitly demands voice-first, low-literacy-friendly interaction in Hindi/Santali.
- **Why AI is needed:** recognizing *which* command word was spoken from raw microphone audio is speech
  recognition — pattern recognition over a noisy signal. It cannot be written as `if/else` rules.
- **MVP scope (deliberately small):** a **fixed command vocabulary only** — module names, option numbers
  ("ek", "do", "teen", "chaar"), yes/no, "repeat", "help". No free-form speech. No chatbot.
- **Why chosen:** it strengthens a *required* MVP capability (Hindi + low-literacy accessibility) instead of
  adding a gimmick; open offline models exist; and a small vocabulary keeps the demo reliable even in a
  noisy hackathon hall.

### Selected 2 — Camera-based PPE verification (helmet/vest detection)

- **Problem it solves:** "PPE selection" is a core competency in both MVP modules. That competency is
  currently assessed virtually (tap-to-select — normal logic, Section 3). PPE recognition adds a real-world
  check: before practice/assessment, the worker shows the camera they are actually wearing a helmet/vest,
  and the AR scene acknowledges it. This is the "computer vision" moment of the demo.
- **Why AI is needed:** detecting a helmet on a person's head in live camera pixels is object detection —
  impossible to express as rules.
- **MVP scope (deliberately small):** 2–4 classes only (helmet / no-helmet, optionally vest / no-vest),
  checked at defined moments (the PPE step of the scenario). The result is **advisory feedback only — it is
  NOT part of pass/fail scoring in the MVP**.
- **Why chosen:** pretrained models and public datasets exist (no training from scratch); it gives one clear
  demo beat; and its fallback (the tap-to-select PPE checklist) is already the base design — so it can be
  cut at any time without hurting the prototype.

### Why the other candidates were NOT selected

| Candidate | Reason for rejection from the MVP |
|---|---|
| General object recognition | No MVP assessment step requires identifying arbitrary real objects; equipment choice is virtual (tap-to-select). Would add model size and latency for zero scoring value. |
| Real hazard recognition (fire/smoke via camera) | Hazards in SurakshaAR scenarios are **virtual AR overlays by design**; we cannot light real fires for a demo; the dataset/domain gap is large. Revisit after the MVP. |
| Worker action recognition | Needs pose estimation + temporal modelling; unreliable on mid-range devices. A flaky live demo is worse than no demo. |
| Voice output (TTS / recorded audio) | Not AI we build — it is normal logic (Section 3) and already planned in the worker app. |
| ML-based weakness detection | Belongs to `ml/competency/`; the team plan is rules first, ML later only with real data to justify it. |

## 5. Model / Approach

Principles: open-source first, pretrained first, smallest model that works, on-device only, no training
from scratch.

### Feature 1 — Hindi voice commands

| Aspect | Choice |
|---|---|
| Technology | **Vosk** offline speech recognition (Apache-2.0) with a small Hindi model (`vosk-model-small-hi-*`; download size roughly 40–50 MB — verify when fetching) |
| Decoding | Restricted **grammar/keyword decoding** limited to the fixed command vocabulary (Vosk supports grammars). This sharply improves accuracy and speed versus open dictation, and is exactly why the MVP scope stays small |
| Runtime | Vosk runs fully on-device. On Android it is wrapped in a small Kotlin **AAR plugin** exposed to Unity via `AndroidJavaObject` (there is no official Unity plugin; a thin wrapper is the standard route) |
| Alternatives | Android `SpeechRecognizer` (offline availability varies by device/vendor — plan B); Whisper-class models (too heavy for mid-range offline MVP — rejected) |
| Santali | **No usable open offline Santali speech model exists today.** Santali ships **output-only** (pre-recorded prompts) with touch input. Honest limitation — revisited in Section 11. |
| Training from scratch | **None.** We only consume a published model. |

### Feature 2 — PPE detection

| Aspect | Choice |
|---|---|
| Technology | Small pretrained **YOLO-family** object detector with PPE classes — e.g. a community "nano" (YOLOv8n/YOLO11n-class) checkpoint fine-tuned on a public PPE dataset such as the Roboflow Universe PPE sets or SH17 |
| Export & runtime | Export to **ONNX** → run inside Unity via **Unity Sentis** (Unity's on-device neural inference package; in recent Unity versions shipped as the Inference Engine package). Plan B: TensorFlow Lite + a thin native Android plugin |
| License note | Ultralytics YOLO weights are AGPL-3.0 — acceptable for this hackathon repo, but flag it before any commercial use. If licensing ever matters, Apache-2.0 alternatives (YOLOX-nano, MobileNet-SSD) drop into the same pipeline |
| Input size | Reduced resolution (~320–416 px) + frame skipping to fit mid-range devices |
| Classes used | Filter detections to `helmet`, `no-helmet` (optionally `vest`, `no-vest`, `person`) |
| Training from scratch | **None.** The POC uses published weights; at most light fine-tuning later if the POC shows accuracy gaps |

## 6. Input / Output Definition

### Feature 1 — Hindi voice commands

```
Microphone audio (16 kHz mono PCM)
  → preprocessing: capture chunks, simple voice-activity / energy gate
  → AI model: Vosk small Hindi model with restricted grammar (command vocabulary)
  → output: best-matching command + confidence
  → Unity: C# event carrying the command id
```

Example (**illustrative only — not from a real run**):

| | Value |
|---|---|
| Input | Worker says "दो" ("two") at an extinguisher-choice step |
| Raw model output | `{"text": "दो", "confidence": 0.87}` |
| App-level output | `{"command": "select_option", "value": 2, "confidence": 0.87}` |
| Unity reaction | Selects option 2; raises the same assessment event a tap would raise |

### Feature 2 — PPE verification

```
Camera frame (RGB, ~640×480, from AR Foundation camera / WebCamTexture)
  → preprocessing: downscale to ~320–416 px, normalize, tensor layout
  → AI model: nano YOLO-family PPE detector (ONNX via Unity Sentis)
  → output: detections {class, confidence, bounding box}
  → app rule: required PPE present with confidence ≥ threshold?
  → output enum: PPE_OK | PPE_MISSING_HELMET | PPE_MISSING_VEST | LOW_CONFIDENCE
  → Unity: UI feedback + (optional, advisory) assessment event
```

Example (**illustrative only — not from a real run**):

| | Value |
|---|---|
| Input | Worker faces the camera wearing a helmet, no vest |
| Raw detections | `helmet 0.81`, `person 0.90` |
| App-level output | `PPE_MISSING_VEST` |
| Unity reaction | Voice + icon prompt reminding the worker to put on the vest; the PPE step is not marked complete |

Confidence thresholds are **to be tuned during the POC** — no values are claimed here.

## 7. Expected Performance

> ⚠️ **Nothing below has been measured.** These are feasibility expectations for mid-range Android
> (Android 10+, ~4 GB RAM class devices, per the PS). The POCs in Section 9 are where real numbers come
> from — replace these estimates with tested values or drop the feature.

| Aspect | Expectation (untested estimate) |
|---|---|
| Voice commands — latency | Near-instant for short utterances with a small grammar-restricted model; suitable for interactive use |
| Voice commands — accuracy | Expected to be high on a *fixed vocabulary* in moderate noise; open dictation would be much harder — which is exactly why the vocabulary is fixed. POC target: ≥ 8/10 spoken commands recognized |
| PPE — speed | Near-real-time is plausible for a nano model at reduced resolution with frame skipping (e.g. infer every 3rd–5th frame); exact FPS on target devices is unknown until the POC |
| PPE — accuracy | Public PPE datasets report usable detection quality for helmet/vest classes, but results depend heavily on lighting, distance and camera angle. POC target: reliable helmet yes/no for front-facing torso shots at 1–2 m in normal indoor light |
| APK / storage impact | Vosk small Hindi model ≈ 40–50 MB; nano detector export likely single-digit to low-teens MB. Must be verified; may need download-on-first-run to keep the base APK small |
| Battery / thermal | Continuous camera inference is the main cost. Mitigations: run only during the PPE step, frame skipping, stop as soon as the step completes |
| Main risks | Background noise during a busy demo (voice); unusual helmets/vests, low light or distance (PPE); ONNX op-support gaps in Sentis for some exports |

## 8. Unity Integration Plan

> **Not implemented yet.** This is the plan only — no integration code is written in this task, and none is
> required by the repository at this stage.

Both features run **fully on-device**. The backend API (`docs/api/API.md`) needs **no changes** — this keeps
the offline-first guarantee intact and keeps AI out of the critical path.

```
Camera / Microphone (AR Foundation camera stream / mic capture)
      →  Preprocessing   (resize + normalize frames  |  PCM audio chunks)
      →  AI inference    (Sentis ONNX PPE detector   |  Vosk Hindi model via AAR plugin)
      →  Prediction      (detections                 |  matched command + confidence)
      →  App rule        (thresholds, command mapping, feature-flag check)
      →  Unity C# event  ("OnPpeResult", "OnVoiceCommand")
      →  AR/UI response  (voice prompt, icon feedback, optional assessment hook)
```

Integration mechanics (proposed):

| Concern | Proposal |
|---|---|
| Vision runtime | Unity Sentis loads the exported ONNX once; inference runs on a worker thread / `async` — never the main thread |
| Frame source | AR Foundation camera frame access (or `WebCamTexture` if simpler for the POC) |
| Speech runtime | Kotlin AAR plugin wrapping Vosk; Unity calls it via `AndroidJavaObject`; results arrive via `UnitySendMessage` or a C# callback bridge |
| Threading | All inference off the main thread; the UI only reacts to events |
| Throttling | PPE inference every Nth frame, and only while the PPE step is active |
| Feature flags | A single config (`ScriptableObject`) with `VoiceCommandsEnabled` and `PpeCheckEnabled` so the demo can run with AI off in one tap |
| Assessment coupling | A voice command raises the *same* event a tap raises; the PPE result is advisory-only in the MVP — assessment logic does not depend on either |
| Failure surface | Every AI call returns a status (`OK`, `LOW_CONFIDENCE`, `MODEL_ERROR`, `DISABLED`); the UI handles all four (Section 10) |

## 9. POC / Feasibility Analysis

Two deliberately tiny POCs, each roughly a day of work, **before** any Unity integration is attempted.
This is the smallest practical proof that each selected feature can work at all.

### POC A — Hindi voice commands (laptop first, then device)

| Item | Detail |
|---|---|
| Required tools | Python 3.10+, `vosk` pip package, small Hindi Vosk model, laptop microphone; later the same model inside a minimal Android test app |
| Sample input | ~10 fixed spoken commands: module names, "ek / do / teen / chaar", yes/no, "repeat", "help" — spoken 3× each, once in a quiet room and once with mild background noise |
| Expected output | For each utterance, Vosk JSON with the recognized text + confidence, mapped to the command id |
| How to test | A short script loops over the utterances (or uses the live mic) and prints the recognized command; repeat with mild noise |
| Success criteria | ≥ 8/10 commands recognized correctly in quiet conditions; latency feels instant; model loads and runs offline (radio off) on the test device |
| If it fails | Shrink the vocabulary further; try Android `SpeechRecognizer`; otherwise ship touch-only UI — which is the base design anyway. **No project risk.** |

### POC B — PPE detection (laptop first, then device)

| Item | Detail |
|---|---|
| Required tools | Python 3.10+, `ultralytics` or `onnxruntime` with a community PPE-trained nano YOLO checkpoint; 5–10 sample photos (helmet on/off, vest on/off) from the open dataset; later: a blank Unity scene + Sentis with the exported ONNX on one mid-range Android phone |
| Sample input | The sample photos; then the live phone camera pointed at a team member with and without a helmet |
| Expected output | Drawn bounding boxes with class + confidence; the app-rule enum `PPE_OK` / `PPE_MISSING_*` |
| How to test | Run inference over the sample set; then build the smallest Unity Sentis scene, export the ONNX, run on the device and log FPS + detections |
| Success criteria | Correct helmet yes/no on most samples on the laptop; on-device inference fast enough for interactive use at reduced resolution; detections stable at 1–2 m in normal indoor light |
| If it fails | Try a different checkpoint once; if still weak, cut the feature — the tap-to-select PPE checklist remains the assessment path. **No project risk.** |

**Decision gate:** if either POC misses its success criteria after one retry, that feature is dropped from
the demo and its fallback becomes the shipped behaviour. Neither POC blocks the core AR prototype.

## 10. Fallback Strategy

Golden rule: **no AI feature is allowed to block the core AR prototype.** Every AI path degrades to a
normal-logic path that is part of the base design anyway.

| Failure mode | Response |
|---|---|
| AI inference fails / model errors (crash, missing file, unsupported device) | Catch at the boundary, return `MODEL_ERROR`, disable that feature for the session, continue with normal UI. The app must never crash because of AI. |
| Confidence too low (voice or vision) | One polite retry prompt ("say again" / "step closer to the camera"); after 2 low-confidence results, fall back to touch/tap for that step and continue the scenario. |
| Model too slow on the device | Throttle (infer every Nth frame), reduce input resolution, or run inference only during the relevant step. Worst case: feature flag off, demo runs on the base design. |
| Target device cannot run the model (RAM / OS / Sentis op gaps) | Capability check at startup; feature flag auto-off; app continues normally. |
| Required resources unavailable (model download failed before entering a low-connectivity site) | Ship models in APK streaming assets if size allows; otherwise the feature auto-off. The app must never require AI assets to boot. |
| Santali speech input specifically | Not feasible with current open models — Santali is output-only (recorded audio) with touch input from day one. This is a documented design choice, not a failure. |
| Demo-day worst case | Both AI features off → SurakshaAR still runs its full loop: AR training, assessment, scoring, retraining, certificate, QR verification, offline mode. AI becomes a "bonus" in the pitch, never a dependency. |

## 11. Future Improvements (explicitly NOT in the MVP)

- **Real-hazard recognition** — detecting real smoke/fire through the camera as an extra practice beat.
- **Worker action recognition** — pose-based checks (crouch, extinguisher aim) once the core modules are stable.
- **Santali speech input** — revisit if open offline Santali STT models become available; terminology must be validated with native speakers first.
- **ML-based weakness detection** — in `ml/competency/`, only once real assessment data exists to justify it over transparent rules (per the team's own analysis).
- **Light fine-tuning of the PPE model** on our own sample images if the POC shows accuracy gaps.
- **Free-form voice Q&A / LLM features** — explicitly excluded by the team's MVP scope; high risk, no PS requirement behind it.

---

*Maintainer: ML/CV workstream (Member 3). Update this document whenever a POC produces real measurements —
replace every "estimate" above with tested numbers, or mark the feature as dropped.*

