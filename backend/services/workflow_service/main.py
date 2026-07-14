"""
Workflow Service - Handles reviews, timelines, status management
Port: 8003
"""

import sys
import os

sys.path.insert(0, os.path.join(os.path.dirname(__file__), "../.."))

from datetime import datetime, timezone, date
from typing import Optional, List

from fastapi import FastAPI, Depends, HTTPException, status, Query
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from sqlalchemy.orm import Session
from sqlalchemy import desc

from backend.shared.database import get_db, engine, Base
from backend.shared.models import (
    Idea,
    Review,
    Timeline,
    IdeaStatusHistory,
    AuditLog,
    Notification,
    User,
)
from backend.shared.auth_utils import get_current_user, require_role
from backend.shared.config import WORKFLOW_SERVICE_PORT

app = FastAPI(
    title="IMTS Workflow Service",
    description="Review, Timeline & Workflow Management Service",
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
class ReviewCreate(BaseModel):
    idea_id: int
    score: Optional[int] = None
    decision: str  # Approved, Declined, Under Review, In Development
    feedback: Optional[str] = None


class TimelineCreate(BaseModel):
    idea_id: Optional[int] = None
    milestone_name: str
    milestone_description: Optional[str] = None
    start_date: Optional[str] = None
    due_date: Optional[str] = None
    assigned_to: Optional[str] = None


class TimelineUpdate(BaseModel):
    milestone_name: Optional[str] = None
    milestone_description: Optional[str] = None
    start_date: Optional[str] = None
    due_date: Optional[str] = None
    completed_date: Optional[str] = None
    status: Optional[str] = None
    assigned_to: Optional[str] = None


class StatusChange(BaseModel):
    idea_id: int
    new_status: str
    change_note: Optional[str] = None


@app.on_event("startup")
def startup():
    Base.metadata.create_all(bind=engine)
    print(f"⚙️ Workflow Service running on port {WORKFLOW_SERVICE_PORT}")


# ================================================================
# REVIEWS
# ================================================================
@app.get("/api/workflow/reviews")
def list_reviews(
    idea_id: Optional[int] = Query(None),
    reviewer_id: Optional[int] = Query(None),
    page: int = Query(1, ge=1),
    per_page: int = Query(20, ge=1, le=100),
    current_user: dict = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """List reviews with optional filtering"""
    query = db.query(Review)

    if idea_id:
        query = query.filter(Review.idea_id == idea_id)
    if reviewer_id:
        query = query.filter(Review.reviewer_id == reviewer_id)

    total = query.count()
    reviews = (
        query.order_by(desc(Review.reviewed_at))
        .offset((page - 1) * per_page)
        .limit(per_page)
        .all()
    )

    return {
        "reviews": [r.to_dict() for r in reviews],
        "total": total,
        "page": page,
        "per_page": per_page,
    }


@app.post("/api/workflow/reviews")
def submit_review(
    data: ReviewCreate,
    current_user: dict = Depends(require_role("InnovationTeam")),
    db: Session = Depends(get_db),
):
    """Submit a review for an idea (Innovation Team only)"""
    idea = db.query(Idea).filter(Idea.id == data.idea_id).first()
    if not idea:
        raise HTTPException(status_code=404, detail="Idea not found")

    # Create review
    review = Review(
        idea_id=data.idea_id,
        reviewer_id=current_user.get("user_id"),
        reviewer_name=current_user.get("name", "Reviewer"),
        score=data.score,
        decision=data.decision,
        feedback=data.feedback,
    )
    db.add(review)

    # Update idea status and score
    old_status = idea.status
    idea.status = data.decision

    # Recalculate average score
    if data.score:
        all_reviews = db.query(Review).filter(Review.idea_id == data.idea_id).all()
        scores = [r.score for r in all_reviews if r.score is not None]
        if scores:
            idea.average_score = sum(scores) / len(scores)
        idea.review_count = len(all_reviews)

    # Add status history
    history = IdeaStatusHistory(
        idea_id=data.idea_id,
        from_status=old_status,
        to_status=data.decision,
        changed_by=current_user.get("name", "Reviewer"),
        change_note=data.feedback or f"Review completed: {data.decision}",
    )
    db.add(history)

    # Log audit
    log = AuditLog(
        user_id=current_user.get("user_id"),
        username=current_user.get("username"),
        action="REVIEW_SUBMITTED",
        resource_type="review",
        resource_id=review.id,
        details={
            "idea_id": data.idea_id,
            "decision": data.decision,
            "score": data.score,
        },
    )
    db.add(log)

    # Create notification for submitter
    if idea.submitter_id:
        notification = Notification(
            user_id=idea.submitter_id,
            email=idea.submitter_email,
            notification_type="review_completed",
            title=f"Idea Review Completed: {data.decision}",
            message=f"Your idea '{idea.title}' has been reviewed. Decision: {data.decision}. Feedback: {data.feedback or 'No feedback provided.'}",
            related_idea_id=idea.id,
        )
        db.add(notification)

    db.commit()
    db.refresh(review)

    return review.to_dict()


# ================================================================
# STATUS MANAGEMENT
# ================================================================
@app.post("/api/workflow/status")
def change_idea_status(
    data: StatusChange,
    current_user: dict = Depends(require_role("InnovationTeam", "ITAdmin")),
    db: Session = Depends(get_db),
):
    """Change idea status and track history"""
    idea = db.query(Idea).filter(Idea.id == data.idea_id).first()
    if not idea:
        raise HTTPException(status_code=404, detail="Idea not found")

    old_status = idea.status
    idea.status = data.new_status

    history = IdeaStatusHistory(
        idea_id=data.idea_id,
        from_status=old_status,
        to_status=data.new_status,
        changed_by=current_user.get("name", "System"),
        change_note=data.change_note,
    )
    db.add(history)

    if data.new_status == "Deployed":
        idea.deployed_at = datetime.now(timezone.utc)

    log = AuditLog(
        user_id=current_user.get("user_id"),
        username=current_user.get("username"),
        action="STATUS_CHANGED",
        resource_type="idea",
        resource_id=data.idea_id,
        details={"from": old_status, "to": data.new_status},
    )
    db.add(log)

    # Notify submitter
    if idea.submitter_id:
        notification = Notification(
            user_id=idea.submitter_id,
            email=idea.submitter_email,
            notification_type="status_change",
            title=f"Idea Status Updated: {data.new_status}",
            message=f"Your idea '{idea.title}' status changed from '{old_status}' to '{data.new_status}'.",
            related_idea_id=idea.id,
        )
        db.add(notification)

    db.commit()
    return {"message": f"Status changed from '{old_status}' to '{data.new_status}'"}


@app.get("/api/workflow/status-history/{idea_id}")
def get_status_history(
    idea_id: int,
    db: Session = Depends(get_db),
    current_user: dict = Depends(get_current_user),
):
    """Get status change history for an idea"""
    history = (
        db.query(IdeaStatusHistory)
        .filter(IdeaStatusHistory.idea_id == idea_id)
        .order_by(desc(IdeaStatusHistory.changed_at))
        .all()
    )
    return [h.to_dict() for h in history]


# ================================================================
# TIMELINES / MILESTONES
# ================================================================
@app.get("/api/workflow/timelines")
def list_timelines(
    idea_id: Optional[int] = Query(None),
    status_filter: Optional[str] = Query(None, alias="status"),
    current_user: dict = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """List timelines/milestones"""
    query = db.query(Timeline)

    if idea_id:
        query = query.filter(Timeline.idea_id == idea_id)
    if status_filter:
        query = query.filter(Timeline.status == status_filter)

    timelines = query.order_by(Timeline.due_date.asc().nullslast()).all()
    return [t.to_dict() for t in timelines]


@app.post("/api/workflow/timelines")
def create_timeline(
    data: TimelineCreate,
    current_user: dict = Depends(require_role("InnovationTeam", "ITAdmin")),
    db: Session = Depends(get_db),
):
    """Create a new timeline/milestone entry"""
    timeline = Timeline(
        idea_id=data.idea_id,
        milestone_name=data.milestone_name,
        milestone_description=data.milestone_description,
        start_date=(
            datetime.strptime(data.start_date, "%Y-%m-%d").date()
            if data.start_date
            else None
        ),
        due_date=(
            datetime.strptime(data.due_date, "%Y-%m-%d").date()
            if data.due_date
            else None
        ),
        assigned_to=data.assigned_to,
        status="pending",
    )
    db.add(timeline)
    db.commit()
    db.refresh(timeline)
    return timeline.to_dict()


@app.put("/api/workflow/timelines/{timeline_id}")
def update_timeline(
    timeline_id: int,
    data: TimelineUpdate,
    current_user: dict = Depends(require_role("InnovationTeam", "ITAdmin")),
    db: Session = Depends(get_db),
):
    """Update a timeline entry"""
    timeline = db.query(Timeline).filter(Timeline.id == timeline_id).first()
    if not timeline:
        raise HTTPException(status_code=404, detail="Timeline not found")

    update_data = data.model_dump(exclude_unset=True)
    if "start_date" in update_data and update_data["start_date"]:
        update_data["start_date"] = datetime.strptime(
            update_data["start_date"], "%Y-%m-%d"
        ).date()
    if "due_date" in update_data and update_data["due_date"]:
        update_data["due_date"] = datetime.strptime(
            update_data["due_date"], "%Y-%m-%d"
        ).date()
    if "completed_date" in update_data and update_data["completed_date"]:
        update_data["completed_date"] = datetime.strptime(
            update_data["completed_date"], "%Y-%m-%d"
        ).date()

    for key, value in update_data.items():
        setattr(timeline, key, value)

    db.commit()
    return timeline.to_dict()


@app.delete("/api/workflow/timelines/{timeline_id}")
def delete_timeline(
    timeline_id: int,
    current_user: dict = Depends(require_role("ITAdmin")),
    db: Session = Depends(get_db),
):
    """Delete a timeline entry (IT Admin only)"""
    timeline = db.query(Timeline).filter(Timeline.id == timeline_id).first()
    if not timeline:
        raise HTTPException(status_code=404, detail="Timeline not found")

    db.delete(timeline)
    db.commit()
    return {"message": "Timeline deleted successfully"}


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=WORKFLOW_SERVICE_PORT)