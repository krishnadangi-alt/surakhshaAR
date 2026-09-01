"""Module request/response schemas."""

from pydantic import BaseModel, ConfigDict


class ModuleOut(BaseModel):
    model_config = ConfigDict(from_attributes=True)

    id: int
    code: str
    name: str
    description: str


class ModuleListOut(BaseModel):
    modules: list[ModuleOut]