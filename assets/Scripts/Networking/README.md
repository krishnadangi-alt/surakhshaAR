# Networking (Unity <-> Backend + ML integration)

This folder wires the Unity AR worker app (`assets/`) to the SurakshaAR backend
(`backend/`, a FastAPI service) and to the ML modules:
* **Competency** (`ml/competency`) — assessments are submitted as raw behavioural
  *events* and scored **server-side** by the ML competency engine. The app never
  sends scores.
* **Vision** (`ml/vision`) — camera frames go to `/vision/ppe-check`; the app
  degrades to the manual tap-to-select PPE checklist when the response is not
  `ok`.

The contract documented in [docs/api/API.md](../../../docs/api/API.md) is the
single source of truth — field names, routes and error shapes match 1:1.

## Files

| File | Purpose |
|---|---|
| `ApiContracts.cs` | Serializable DTOs mirroring every backend request/response body (incl. the `<CompetencyScoreMap>` mirror of the competency-scores JSON object). |
| `SurakshaApiClient.cs` | Coroutine-based HTTP client (UnityWebRequest) with one method per endpoint (incl. `GetRetrainingPlan`, `CheckPPE`, `GetVisionStatus`). |
| `AssessmentEvents.cs` | Factories for well-formed assessment events (`hazard_identified`, `ppe_selected`, `wrong_action`, `critical_action`, ...). |
| `BackendSyncExample.cs` | Ready-to-attach demo: health -> register worker -> progress -> event-based assessment -> retraining plan -> PPE check -> offline sync. |
| `README.md` | This file. |

## Setup

1. Start the backend (it auto-creates and seeds the SQLite DB):

   ```bash
   cd backend
   pip install -r requirements.txt
   python run.py        # serves http://127.0.0.1:8000/api/v1
   ```

2. In Unity, add a persistent GameObject (e.g. a `Backend` object or the XR Origin)
   and attach **`SurakshaApiClient`**. Set `Base Url` if the backend is not on
   `http://127.0.0.1:8000/api/v1` (emulators use `http://10.0.2.2:8000/api/v1`).

3. Attach **`BackendSyncExample`** (to the same GameObject) to watch the full flow
   in the Console, or call the client directly:

   ```csharp
   var api = SurakshaApiClient.Instance; // or GetComponent<SurakshaApiClient>()

   // Submit a behavioural assessment (server scores it)
   var assessment = new ApiContracts.AssessmentCreate {
       worker_id = 1,
       module_id = 1,
       events = new[] {
           AssessmentEvents.HazardIdentified(true, "electrical_fire"),
           AssessmentEvents.PpeSelected(true, "helmet", "gloves", "jacket"),
       },
   };
   api.SubmitAssessment(assessment, result => {
       if (result.Success) {
           Debug.Log(result.Data.passed);          // authoritative pass/fail
           Debug.Log(result.Data.pass_reason);
           Debug.Log(result.Data.competency_scores.ppe_selection.score);
       }
   });

   // Fetch targeted retraining after a failed assessment
   api.GetRetrainingPlan(assessmentId, result => { ... });

   // Vision: PPE check on a camera frame
   api.CheckPPE(new ApiContracts.PPECheckRequest { image_base64 = pngBase64 }, result => {
       // result.Data.status == "ok" -> proceed; else fall back to tap UI
   });
   ```

## Notes

- **Server-side scoring is authoritative.** Client-supplied `score`/`passed` are
  never used by `POST /assessments`; only the `events` are trusted.
- `competency_scores` is a JSON object, which `JsonUtility` cannot deserialise as
  a dictionary. `CompetencyScoreMap` mirrors the fixed per-scenario key set
  (fire/gas); keys not present in the response deserialise to `null`.
- The API is unauthenticated in this MVP; `worker_id` is passed in the body/path.
- On Android, plain HTTP requires the app's network config to allow cleartext
  traffic for development.
- Keep `ApiContracts.cs` and the backend schemas in lockstep — any field renamed
  on one side must be renamed on the other (see `backend/app/schemas/`).