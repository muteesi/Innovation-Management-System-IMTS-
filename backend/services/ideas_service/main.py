"""
Ideas Service - Handles idea submission, categories, attachments, team composition
Port: 8002
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
from backend.shared.models import (
    Idea,
    Category,
    Attachment,
    TeamComposition,
    IdeaStatusHistory,
    AuditLog,
    Notification,
)
from backend.shared.auth_utils import get_current_user, require_role
from backend.shared.config import IDEAS_SERVICE_PORT

app = FastAPI(
    title="IMTS Ideas Service",
    description="Innovation Ideas Management Service",
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
class TeamCompositionCreate(BaseModel):
    composition_type: str  # station, age, gender, rank
    composition_key: str
    composition_label: Optional[str] = None
    staff_count: int


class AttachmentCreate(BaseModel):
    file_name: str
    file_type: Optional[str] = None
    file_size: Optional[int] = None
    file_data: Optional[str] = None  # Base64 encoded


class IdeaCreate(BaseModel):
    title: str
    description: str
    problem_statement: Optional[str] = None
    proposed_solution: Optional[str] = None
    expected_impact: Optional[str] = None
    submitter_name: str
    submitter_email: str
    submitter_department: Optional[str] = None
    submitter_rank: Optional[str] = None
    business_unit: Optional[str] = None
    station: Optional[str] = None
    submission_type: str = "individual"
    team_members: Optional[str] = None
    category_name: Optional[str] = None
    attachments: Optional[List[AttachmentCreate]] = None
    team_compositions: Optional[List[TeamCompositionCreate]] = None


class IdeaUpdate(BaseModel):
    title: Optional[str] = None
    description: Optional[str] = None
    problem_statement: Optional[str] = None
    proposed_solution: Optional[str] = None
    expected_impact: Optional[str] = None
    category_name: Optional[str] = None
    status: Optional[str] = None


@app.on_event("startup")
def startup():
    Base.metadata.create_all(bind=engine)
    print(f"💡 Ideas Service running on port {IDEAS_SERVICE_PORT}")


# ================================================================
# CATEGORIES
# ================================================================
@app.get("/api/ideas/categories")
def list_categories(db: Session = Depends(get_db)):
    """List all active categories"""
    categories = db.query(Category).filter(Category.is_active == True).all()
    return [c.to_dict() for c in categories]


@app.post("/api/ideas/categories")
def create_category(
    name: str,
    description: Optional[str] = None,
    current_user: dict = Depends(require_role("ITAdmin", "InnovationTeam")),
    db: Session = Depends(get_db),
):
    """Create a new category"""
    existing = db.query(Category).filter(Category.name == name).first()
    if existing:
        raise HTTPException(status_code=400, detail="Category already exists")

    category = Category(name=name, description=description)
    db.add(category)
    db.commit()
    db.refresh(category)
    return category.to_dict()


@app.put("/api/ideas/categories/{category_id}")
def update_category(
    category_id: int,
    name: Optional[str] = None,
    description: Optional[str] = None,
    is_active: Optional[bool] = None,
    current_user: dict = Depends(require_role("ITAdmin", "InnovationTeam")),
    db: Session = Depends(get_db),
):
    """Update a category"""
    category = db.query(Category).filter(Category.id == category_id).first()
    if not category:
        raise HTTPException(status_code=404, detail="Category not found")

    if name:
        category.name = name
    if description is not None:
        category.description = description
    if is_active is not None:
        category.is_active = is_active

    db.commit()
    return category.to_dict()


# ================================================================
# IDEAS
# ================================================================
@app.get("/api/ideas")
def list_ideas(
    status_filter: Optional[str] = Query(None, alias="status"),
    category_filter: Optional[str] = Query(None, alias="category"),
    search: Optional[str] = Query(None),
    submitter_id: Optional[int] = Query(None),
    page: int = Query(1, ge=1),
    per_page: int = Query(20, ge=1, le=100),
    current_user: dict = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """List ideas with filtering and pagination"""
    query = db.query(Idea)

    if status_filter and status_filter != "all":
        query = query.filter(Idea.status == status_filter)
    if category_filter and category_filter != "all":
        query = query.filter(Idea.category_name == category_filter)
    if submitter_id:
        query = query.filter(Idea.submitter_id == submitter_id)
    if search:
        search_term = f"%{search}%"
        query = query.filter(
            (Idea.title.ilike(search_term))
            | (Idea.submitter_name.ilike(search_term))
            | (Idea.submitter_email.ilike(search_term))
            | (Idea.description.ilike(search_term))
        )

    total = query.count()
    query = query.order_by(desc(Idea.submitted_at))
    ideas = query.offset((page - 1) * per_page).limit(per_page).all()

    return {
        "ideas": [idea.to_dict() for idea in ideas],
        "total": total,
        "page": page,
        "per_page": per_page,
        "total_pages": (total + per_page - 1) // per_page,
    }


@app.get("/api/ideas/{idea_id}")
def get_idea(
    idea_id: int,
    db: Session = Depends(get_db),
    current_user: dict = Depends(get_current_user),
):
    """Get a single idea with all relations"""
    idea = db.query(Idea).filter(Idea.id == idea_id).first()
    if not idea:
        raise HTTPException(status_code=404, detail="Idea not found")
    return idea.to_dict(include_relations=True)


@app.post("/api/ideas")
def create_idea(
    data: IdeaCreate,
    current_user: dict = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """Submit a new innovation idea"""
    # Find or use category
    category_id = None
    if data.category_name:
        category = (
            db.query(Category)
            .filter(Category.name == data.category_name)
            .first()
        )
        if category:
            category_id = category.id

    # Get submitter user ID if exists
    from backend.shared.models import User

    submitter = (
        db.query(User)
        .filter(User.email == data.submitter_email)
        .first()
    )

    idea = Idea(
        title=data.title,
        description=data.description,
        problem_statement=data.problem_statement,
        proposed_solution=data.proposed_solution,
        expected_impact=data.expected_impact,
        submitter_id=submitter.id if submitter else current_user.get("user_id"),
        submitter_name=data.submitter_name,
        submitter_email=data.submitter_email,
        submitter_department=data.submitter_department,
        submitter_rank=data.submitter_rank,
        business_unit=data.business_unit,
        station=data.station,
        submission_type=data.submission_type,
        team_members=data.team_members,
        category_id=category_id,
        category_name=data.category_name,
        status="Pending",
        current_stage="Submitted",
    )
    db.add(idea)
    db.flush()

    # Add attachments
    if data.attachments:
        for att in data.attachments:
            attachment = Attachment(
                idea_id=idea.id,
                file_name=att.file_name,
                file_type=att.file_type,
                file_size=att.file_size,
                file_data=att.file_data,
            )
            db.add(attachment)

    # Add team compositions
    if data.team_compositions:
        for tc in data.team_compositions:
            comp = TeamComposition(
                idea_id=idea.id,
                composition_type=tc.composition_type,
                composition_key=tc.composition_key,
                composition_label=tc.composition_label,
                staff_count=tc.staff_count,
            )
            db.add(comp)

    # Add status history entry
    history = IdeaStatusHistory(
        idea_id=idea.id,
        to_status="Pending",
        changed_by=data.submitter_name,
        change_note="Idea submitted",
    )
    db.add(history)

    # Log audit
    log = AuditLog(
        user_id=current_user.get("user_id"),
        username=current_user.get("username", data.submitter_name),
        action="IDEA_CREATED",
        resource_type="idea",
        resource_id=idea.id,
        details={"title": idea.title},
    )
    db.add(log)

    # Create notification for innovation team
    team_members = (
        db.query(User).filter(User.role == "InnovationTeam").all()
    )
    for member in team_members:
        notification = Notification(
            user_id=member.id,
            email=member.email,
            notification_type="new_idea",
            title="New Innovation Idea Submitted",
            message=f"A new idea '{idea.title}' has been submitted by {data.submitter_name}.",
            related_idea_id=idea.id,
        )
        db.add(notification)

    db.commit()
    db.refresh(idea)

    return idea.to_dict(include_relations=True)


@app.put("/api/ideas/{idea_id}")
def update_idea(
    idea_id: int,
    data: IdeaUpdate,
    current_user: dict = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """Update an idea"""
    idea = db.query(Idea).filter(Idea.id == idea_id).first()
    if not idea:
        raise HTTPException(status_code=404, detail="Idea not found")

    update_data = data.model_dump(exclude_unset=True)
    for key, value in update_data.items():
        setattr(idea, key, value)

    idea.updated_at = datetime.now(timezone.utc)
    db.commit()

    log = AuditLog(
        user_id=current_user.get("user_id"),
        username=current_user.get("username"),
        action="IDEA_UPDATED",
        resource_type="idea",
        resource_id=idea.id,
    )
    db.add(log)
    db.commit()

    return idea.to_dict()


@app.delete("/api/ideas/{idea_id}")
def delete_idea(
    idea_id: int,
    current_user: dict = Depends(require_role("ITAdmin")),
    db: Session = Depends(get_db),
):
    """Delete an idea (IT Admin only)"""
    idea = db.query(Idea).filter(Idea.id == idea_id).first()
    if not idea:
        raise HTTPException(status_code=404, detail="Idea not found")

    db.delete(idea)
    db.commit()
    return {"message": "Idea deleted successfully"}


@app.get("/api/ideas/stats/summary")
def get_ideas_summary(
    current_user: dict = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """Get summary statistics for ideas"""
    total = db.query(Idea).count()
    pending = db.query(Idea).filter(Idea.status == "Pending").count()
    under_review = db.query(Idea).filter(Idea.status == "Under Review").count()
    approved = db.query(Idea).filter(Idea.status == "Approved").count()
    declined = db.query(Idea).filter(Idea.status == "Declined").count()
    in_development = db.query(Idea).filter(Idea.status == "In Development").count()
    deployed = db.query(Idea).filter(Idea.status == "Deployed").count()

    categories = (
        db.query(Idea.category_name, db.func.count(Idea.id))
        .group_by(Idea.category_name)
        .all()
    )

    return {
        "total": total,
        "pending": pending,
        "under_review": under_review,
        "approved": approved,
        "declined": declined,
        "in_development": in_development,
        "deployed": deployed,
        "by_category": [
            {"category": cat, "count": count} for cat, count in categories if cat
        ],
    }


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=IDEAS_SERVICE_PORT)