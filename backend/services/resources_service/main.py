"""
Resources Service - Manages shared resources, documents, and resource categories
Port: 8004
"""

import sys
import os

sys.path.insert(0, os.path.join(os.path.dirname(__file__), "../.."))

from datetime import datetime, timezone
from typing import Optional, List

from fastapi import FastAPI, Depends, HTTPException, status, Query
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from sqlalchemy.orm import Session
from sqlalchemy import desc

from backend.shared.database import get_db, engine, Base
from backend.shared.models import Resource, ResourceCategory, AuditLog
from backend.shared.auth_utils import get_current_user, require_role
from backend.shared.config import RESOURCES_SERVICE_PORT

app = FastAPI(
    title="IMTS Resources Service",
    description="Shared Resources & Documents Management Service",
    version="1.0.0",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# ================================================================
# SCHEMAS
# ================================================================
class ResourceCreate(BaseModel):
    title: str
    description: Optional[str] = None
    resource_type: Optional[str] = "document"
    category: Optional[str] = None
    url: Optional[str] = None
    file_name: Optional[str] = None
    file_data: Optional[str] = None
    file_size: Optional[int] = None
    is_public: bool = True


class ResourceUpdate(BaseModel):
    title: Optional[str] = None
    description: Optional[str] = None
    resource_type: Optional[str] = None
    category: Optional[str] = None
    url: Optional[str] = None
    is_public: Optional[bool] = None


class ResourceCategoryCreate(BaseModel):
    name: str
    description: Optional[str] = None
    icon: Optional[str] = None


@app.on_event("startup")
def startup():
    Base.metadata.create_all(bind=engine)
    print(f"📦 Resources Service running on port {RESOURCES_SERVICE_PORT}")


# ================================================================
# RESOURCES
# ================================================================
@app.get("/api/resources")
def list_resources(
    category: Optional[str] = Query(None),
    resource_type: Optional[str] = Query(None),
    search: Optional[str] = Query(None),
    page: int = Query(1, ge=1),
    per_page: int = Query(20, ge=1, le=100),
    current_user: dict = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """List resources with filtering"""
    query = db.query(Resource)

    if category:
        query = query.filter(Resource.category == category)
    if resource_type:
        query = query.filter(Resource.resource_type == resource_type)
    if search:
        search_term = f"%{search}%"
        query = query.filter(
            (Resource.title.ilike(search_term))
            | (Resource.description.ilike(search_term))
        )

    total = query.count()
    resources = (
        query.order_by(desc(Resource.created_at))
        .offset((page - 1) * per_page)
        .limit(per_page)
        .all()
    )

    return {
        "resources": [r.to_dict() for r in resources],
        "total": total,
        "page": page,
        "per_page": per_page,
    }


@app.get("/api/resources/{resource_id}")
def get_resource(
    resource_id: int,
    db: Session = Depends(get_db),
    current_user: dict = Depends(get_current_user),
):
    """Get a single resource"""
    resource = db.query(Resource).filter(Resource.id == resource_id).first()
    if not resource:
        raise HTTPException(status_code=404, detail="Resource not found")
    return resource.to_dict()


@app.post("/api/resources")
def create_resource(
    data: ResourceCreate,
    current_user: dict = Depends(require_role("ITAdmin", "InnovationTeam")),
    db: Session = Depends(get_db),
):
    """Create a new resource"""
    resource = Resource(
        title=data.title,
        description=data.description,
        resource_type=data.resource_type,
        category=data.category,
        url=data.url,
        file_name=data.file_name,
        file_data=data.file_data,
        file_size=data.file_size,
        is_public=data.is_public,
        uploaded_by=current_user.get("user_id"),
    )
    db.add(resource)
    db.commit()
    db.refresh(resource)

    log = AuditLog(
        user_id=current_user.get("user_id"),
        username=current_user.get("username"),
        action="RESOURCE_CREATED",
        resource_type="resource",
        resource_id=resource.id,
    )
    db.add(log)
    db.commit()

    return resource.to_dict()


@app.put("/api/resources/{resource_id}")
def update_resource(
    resource_id: int,
    data: ResourceUpdate,
    current_user: dict = Depends(require_role("ITAdmin", "InnovationTeam")),
    db: Session = Depends(get_db),
):
    """Update a resource"""
    resource = db.query(Resource).filter(Resource.id == resource_id).first()
    if not resource:
        raise HTTPException(status_code=404, detail="Resource not found")

    update_data = data.model_dump(exclude_unset=True)
    for key, value in update_data.items():
        setattr(resource, key, value)

    db.commit()
    return resource.to_dict()


@app.delete("/api/resources/{resource_id}")
def delete_resource(
    resource_id: int,
    current_user: dict = Depends(require_role("ITAdmin")),
    db: Session = Depends(get_db),
):
    """Delete a resource (IT Admin only)"""
    resource = db.query(Resource).filter(Resource.id == resource_id).first()
    if not resource:
        raise HTTPException(status_code=404, detail="Resource not found")

    db.delete(resource)
    db.commit()
    return {"message": "Resource deleted successfully"}


# ================================================================
# RESOURCE CATEGORIES
# ================================================================
@app.get("/api/resources/categories")
def list_resource_categories(db: Session = Depends(get_db)):
    """List all resource categories"""
    categories = db.query(ResourceCategory).all()
    return [c.to_dict() for c in categories]


@app.post("/api/resources/categories")
def create_resource_category(
    data: ResourceCategoryCreate,
    current_user: dict = Depends(require_role("ITAdmin", "InnovationTeam")),
    db: Session = Depends(get_db),
):
    """Create a new resource category"""
    existing = (
        db.query(ResourceCategory)
        .filter(ResourceCategory.name == data.name)
        .first()
    )
    if existing:
        raise HTTPException(status_code=400, detail="Category already exists")

    category = ResourceCategory(
        name=data.name,
        description=data.description,
        icon=data.icon,
    )
    db.add(category)
    db.commit()
    db.refresh(category)
    return category.to_dict()


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=RESOURCES_SERVICE_PORT)