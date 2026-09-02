"""SurakshaAR ML — Computer Vision.

Interprets ambiguous real-world signals (camera pixels / microphone audio) that
normal code cannot interpret, e.g. "is the worker wearing a helmet and vest?".

Design principles (see ml/vision/README.md):
- Offline-first: models run on-device; no cloud inference.
- AI is never on the critical path: every detection call returns a status
  (ok | low_confidence | model_error | disabled) and the caller decides how to
  degrade gracefully.
- No AI feature may crash the app: detection code never raises.

Implementation status: a working, dependency-free integration scaffold ships
with a deterministic mock/fallback engine so the full pipeline (Unity -> backend
-> vision service) runs end-to-end with zero models. The ``PPEDetector`` accepts
a real ONNX/TFLite model path; until one is configured it degrades to the mock
fallback exactly like the README's "MODEL_ERROR -> fallback UI" flow.
"""

__version__ = "0.1.0"