"""Assessment request/response schemas.

Assessments are behaviour-based: the client submits the raw VR session events
and the backend scores them with the ML competency engine (see
``app.services.competency_service``). Client-supplied scores are never trusted.
"""

from datetime import datetime

from pydantic import BaseModel, ConfigDict, Field


class AssessmentEvent(BaseModel):
    """A single behavioural event from the VR session.

    Only ``event_type`` is required; each event type carries its own fields
    (e.g. ``action``/``correct`` for action events, ``severity`` for
    wrong_action events). Unknown extra fields are preserved and forwarded to
    the scoring engine unchanged.
    """

    model_config = ConfigDict(extra="allow")

    event_type: str = Field(
        ...,
        description=(
            "hazard_identified | ppe_selected | equipment_selected | "
            "wrong_action | critical_action | evacuation_started | "
            "emergency_procedure | training_started | assessment_started | "
            "assessment_completed"
        ),
    )
    timestamp: str | None = None


class AssessmentCreate(BaseModel):
    worker_id: int
    module_id: int
    scenario_type: str | None = Field(
        None,
        description="Optional override; defaults to the module code (fire/gas).",
    )
    attempt_number: int | None = Field(
        None,
        description="Optional; auto-incremented per worker+module when omitted.",
    )
    client_session_id: str | None = Field(
        None,
        max_length=64,
        description=(
            "Optional client-generated idempotency key; re-submitting the same key "
            "for the same worker+module returns the stored assessment (200) and "
            "creates no duplicate record."
        ),
    )
    events: list[AssessmentEvent] = Field(..., min_length=1)


class CompetencyScoreOut(BaseModel):
    name: str
    score: float
    passed: bool
    pass_threshold: float


class WeaknessOut(BaseModel):
    competency_name: str
    score: float
    threshold: float
    severity: str
    reason: str
    affected_aspects: list[str] = []


class RetrainingModuleOut(BaseModel):
    module_id: str
    name: str
    description: str
    estimated_duration_minutes: int
    difficulty_level: str
    competencies_addressed: list[str]
    reason: str


class RetrainingPlanOut(BaseModel):
    scenario_type: str
    recommended_modules: list[RetrainingModuleOut]
    total_estimated_duration_minutes: int
    time_limit_exceeded: bool
    weaknesses_addressed: int
    total_weaknesses: int


class AssessmentOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: int
    worker_id: int
    module_id: int
    attempt_number: int
    scenario_type: str
    score: float
    passed: bool
    pass_reason: str
    weaknesses: list[WeaknessOut]
    competency_scores: dict[str, CompetencyScoreOut]
    critical_errors: list[str]
    created_at: datetime


class AssessmentHistoryOut(BaseModel):
    worker_id: int
    assessments: list[AssessmentOut]