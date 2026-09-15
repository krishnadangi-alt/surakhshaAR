"""Certificate request/response schemas."""

from datetime import datetime

from pydantic import BaseModel, ConfigDict


class CertificateCreate(BaseModel):
    worker_id: int
    module_id: int


class CertificateOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: int
    certificate_number: str
    worker_id: int
    module_id: int
    issued_at: datetime
    valid_until: datetime
    status: str


class CertificateListOut(BaseModel):
    worker_id: int
    certificates: list[CertificateOut]


class CertificateVerifyOut(BaseModel):
    certificate_number: str
    valid: bool
    worker_name: str
    module_name: str
    issued_at: datetime
    valid_until: datetime
    status: str