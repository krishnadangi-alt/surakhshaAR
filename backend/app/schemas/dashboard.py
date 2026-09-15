"""Dashboard request/response schemas."""

from pydantic import BaseModel

from app.schemas.assessment import AssessmentOut
from app.schemas.certificate import CertificateOut
from app.schemas.progress import ProgressItemOut


class ModuleStatOut(BaseModel):
    module_id: int
    module_name: str
    workers_enrolled: int
    certified: int


class DashboardSummaryOut(BaseModel):
    total_workers: int
    workers_in_training: int
    certified_workers: int
    total_assessments: int
    pass_rate: float
    module_stats: list[ModuleStatOut]


class DashboardWorkerOut(BaseModel):
    id: int
    name: str
    employee_id: str
    role: str
    progress: list[ProgressItemOut]
    certified_modules: list[str]


class DashboardWorkerListOut(BaseModel):
    workers: list[DashboardWorkerOut]


class DashboardWorkerDetailOut(BaseModel):
    id: int
    name: str
    employee_id: str
    role: str
    progress: list[ProgressItemOut]
    assessments: list[AssessmentOut]
    certificates: list[CertificateOut]