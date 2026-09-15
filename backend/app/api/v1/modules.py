"""Modules endpoints."""

from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session

from app.api.deps import get_current_user, get_db
from app.models.auth_user import AuthUser
from app.models.module import Module
from app.schemas.module import ModuleListOut

router = APIRouter(prefix="/modules", tags=["modules"])


@router.get("", response_model=ModuleListOut)
def list_modules(
    user: AuthUser = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    modules = db.query(Module).order_by(Module.id).all()
    return ModuleListOut(modules=modules)