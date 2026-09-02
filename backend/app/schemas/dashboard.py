"""Dashboard request/response schemas."""

from pydantic import BaseModel

from app.schemas.assessment import AssessmentOut, CompetencyScoreOut, WeaknessOut
from app.schemas.certificate import CertificateOut
from app.schemas.progress import ProgressItemOut


class ModuleStatOut(BaseModel):
    module_id: int
    module_name: str
    workers_enrolled: int
    certified: int


class CommonWeaknessOut(BaseModel):
    competency_name: str
    count: int
    average_score: float | None = None


class DashboardSummaryOut(BaseModel):
    total_workers: int
    workers_in_training: int
    certified_workers: int
    total_assessments: int
    pass_rate: float
    module_stats: list[ModuleStatOut]
    common_weaknesses: list[CommonWeaknessOut] = []


class DashboardWorkerOut(BaseModel):
    id: int
    name: str
    employee_id: str
    role: str
    progress: list[ProgressItemOut]
    certified_modules: list[str]


class DashboardWorkerListOut(BaseModel):
    workers: list[DashboardWorkerOut]


class WorkerCompetencyProfileOut(BaseModel):
    module_id: int
    module_code: str
    module_name: str
    attempt_number: int
    overall_score: float
    passed: bool
    competencies: dict[str, CompetencyScoreOut]
    weaknesses: list[WeaknessOut]


class DashboardWorkerDetailOut(BaseModel):
    id: int
    name: str
    employee_id: str
    role: str
    progress: list[ProgressItemOut]
    assessments: list[AssessmentOut]
    certificates: list[CertificateOut]
    competency_profile: list[WorkerCompetencyProfileOut] = []