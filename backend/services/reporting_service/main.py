"""
Reporting Service - Generates reports, analytics, and audit logs
Port: 8005
"""

import sys
import os

sys.path.insert(0, os.path.join(os.path.dirname(__file__), "../.."))

from datetime import datetime, timezone, timedelta
from typing import Optional

from fastapi import FastAPI, Depends, HTTPException, status, Query
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from sqlalchemy.orm import Session
from sqlalchemy import desc, func

from backend.shared.database import get_db, engine, Base
from backend.shared.models import (
    Idea, Review, User, AuditLog, Report, Notification, IdeaStatusHistory, Category
)
from backend.shared.auth_utils import get_current_user, require_role
from backend.shared.config import REPORTING_SERVICE_PORT

app = FastAPI(
    title="IMTS Reporting Service",
    description="Analytics, Reports & Audit Log Service",
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
class ReportGenerate(BaseModel):
    title: str
    report_type: str  # ideas_summary, user_activity, performance
    parameters: Optional[dict] = None


@app.on_event("startup")
def startup():
    Base.metadata.create_all(bind=engine)
    print(f"📊 Reporting Service running on port {REPORTING_SERVICE_PORT}")


# ================================================================
# AUDIT LOGS
# ================================================================
@app.get("/api/reporting/audit-logs")
def list_audit_logs(
    action: Optional[str] = Query(None),
    username: Optional[str] = Query(None),
    resource_type: Optional[str] = Query(None),
    start_date: Optional[str] = Query(None),
    end_date: Optional[str] = Query(None),
    page: int = Query(1, ge=1),
    per_page: int = Query(50, ge=1, le=200),
    current_user: dict = Depends(require_role("ITAdmin")),
    db: Session = Depends(get_db),
):
    """List audit logs with filtering (IT Admin only)"""
    query = db.query(AuditLog)

    if action:
        query = query.filter(AuditLog.action == action)
    if username:
        query = query.filter(AuditLog.username.ilike(f"%{username}%"))
    if resource_type:
        query = query.filter(AuditLog.resource_type == resource_type)
    if start_date:
        query = query.filter(
            AuditLog.created_at >= datetime.strptime(start_date, "%Y-%m-%d")
        )
    if end_date:
        query = query.filter(
            AuditLog.created_at <= datetime.strptime(end_date + " 23:59:59", "%Y-%m-%d %H:%M:%S")
        )

    total = query.count()
    logs = (
        query.order_by(desc(AuditLog.created_at))
        .offset((page - 1) * per_page)
        .limit(per_page)
        .all()
    )

    return {
        "logs": [log.to_dict() for log in logs],
        "total": total,
        "page": page,
        "per_page": per_page,
        "total_pages": (total + per_page - 1) // per_page,
    }


# ================================================================
# DASHBOARD ANALYTICS
# ================================================================
@app.get("/api/reporting/dashboard")
def get_dashboard_data(
    current_user: dict = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """Get comprehensive dashboard analytics"""
    # Idea statistics
    total_ideas = db.query(Idea).count()
    pending = db.query(Idea).filter(Idea.status == "Pending").count()
    under_review = db.query(Idea).filter(Idea.status == "Under Review").count()
    approved = db.query(Idea).filter(Idea.status == "Approved").count()
    declined = db.query(Idea).filter(Idea.status == "Declined").count()
    in_development = db.query(Idea).filter(Idea.status == "In Development").count()
    deployed = db.query(Idea).filter(Idea.status == "Deployed").count()

    # User statistics
    total_users = db.query(User).count()
    active_users = db.query(User).filter(User.is_active == True).count()

    # Review statistics
    total_reviews = db.query(Review).count()
    avg_score = db.query(func.avg(Review.score)).scalar() or 0

    # Category distribution
    category_stats = (
        db.query(Idea.category_name, func.count(Idea.id))
        .filter(Idea.category_name.isnot(None))
        .group_by(Idea.category_name)
        .order_by(desc(func.count(Idea.id)))
        .all()
    )

    # Recent ideas
    recent_ideas = (
        db.query(Idea)
        .order_by(desc(Idea.submitted_at))
        .limit(10)
        .all()
    )

    # Monthly submissions (last 6 months)
    six_months_ago = datetime.now(timezone.utc) - timedelta(days=180)
    monthly_stats = (
        db.query(
            func.strftime("%Y-%m", Idea.submitted_at).label("month"),
            func.count(Idea.id).label("count"),
        )
        .filter(Idea.submitted_at >= six_months_ago)
        .group_by(func.strftime("%Y-%m", Idea.submitted_at))
        .order_by(func.strftime("%Y-%m", Idea.submitted_at))
        .all()
    )

    return {
        "ideas": {
            "total": total_ideas,
            "pending": pending,
            "under_review": under_review,
            "approved": approved,
            "declined": declined,
            "in_development": in_development,
            "deployed": deployed,
        },
        "users": {
            "total": total_users,
            "active": active_users,
        },
        "reviews": {
            "total": total_reviews,
            "average_score": round(float(avg_score), 2),
        },
        "category_distribution": [
            {"category": cat or "Uncategorized", "count": count}
            for cat, count in category_stats
        ],
        "recent_ideas": [idea.to_dict() for idea in recent_ideas],
        "monthly_submissions": [
            {"month": row.month, "count": row.count} for row in monthly_stats
        ],
    }


# ================================================================
# REPORTS
# ================================================================
@app.get("/api/reporting/reports")
def list_reports(
    current_user: dict = Depends(require_role("ITAdmin", "InnovationTeam")),
    db: Session = Depends(get_db),
):
    """List all generated reports"""
    reports = db.query(Report).order_by(desc(Report.generated_at)).all()
    return [r.to_dict() for r in reports]


@app.post("/api/reporting/reports/generate")
def generate_report(
    data: ReportGenerate,
    current_user: dict = Depends(require_role("ITAdmin", "InnovationTeam")),
    db: Session = Depends(get_db),
):
    """Generate a new report based on type"""
    result_data = {}

    if data.report_type == "ideas_summary":
        # Get all ideas with their review data
        ideas = db.query(Idea).all()
        result_data = {
            "total_ideas": len(ideas),
            "by_status": {
                status: len([i for i in ideas if i.status == status])
                for status in ["Pending", "Under Review", "Approved", "Declined", "In Development", "Deployed"]
            },
            "by_category": (
                db.query(Idea.category_name, func.count(Idea.id))
                .group_by(Idea.category_name)
                .all()
            ),
            "average_score": round(float(db.query(func.avg(Review.score)).scalar() or 0), 2),
        }

    elif data.report_type == "user_activity":
        # Get user activity data
        users = db.query(User).all()
        result_data = {
            "total_users": len(users),
            "active_users": len([u for u in users if u.is_active]),
            "users_by_role": (
                db.query(User.role, func.count(User.id))
                .group_by(User.role)
                .all()
            ),
            "recent_logins": [
                u.to_dict() for u in db.query(User)
                .filter(User.last_login.isnot(None))
                .order_by(desc(User.last_login))
                .limit(20)
                .all()
            ],
        }

    elif data.report_type == "performance":
        # Overall system performance metrics
        result_data = {
            "total_ideas": db.query(Idea).count(),
            "total_reviews": db.query(Review).count(),
            "avg_approval_time": "N/A",  # Would need more complex query
            "conversion_rate": (
                round(
                    db.query(Idea).filter(Idea.status == "Approved").count()
                    / max(db.query(Idea).count(), 1)
                    * 100,
                    2,
                )
                if db.query(Idea).count() > 0
                else 0
            ),
        }

    report = Report(
        title=data.title,
        report_type=data.report_type,
        parameters=data.parameters,
        result_data=result_data,
        generated_by=current_user.get("user_id"),
    )
    db.add(report)
    db.commit()
    db.refresh(report)

    return report.to_dict()


# ================================================================
# IDEAS ANALYTICS (per-idea)
# ================================================================
@app.get("/api/reporting/ideas/{idea_id}/analytics")
def get_idea_analytics(
    idea_id: int,
    current_user: dict = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """Get detailed analytics for a specific idea"""
    idea = db.query(Idea).filter(Idea.id == idea_id).first()
    if not idea:
        raise HTTPException(status_code=404, detail="Idea not found")

    reviews = db.query(Review).filter(Review.idea_id == idea_id).all()
    status_history = (
        db.query(IdeaStatusHistory)
        .filter(IdeaStatusHistory.idea_id == idea_id)
        .order_by(IdeaStatusHistory.changed_at)
        .all()
    )

    # Time in each status
    status_durations = []
    for i in range(len(status_history)):
        entry = status_history[i]
        next_time = (
            status_history[i + 1].changed_at
            if i + 1 < len(status_history)
            else datetime.now(timezone.utc)
        )
        duration = (next_time - entry.changed_at).total_seconds() / 3600  # hours
        status_durations.append({
            "status": entry.to_status,
            "changed_at": entry.changed_at.isoformat(),
            "duration_hours": round(duration, 1),
        })

    return {
        "idea": idea.to_dict(),
        "reviews": [r.to_dict() for r in reviews],
        "status_timeline": status_durations,
        "total_reviews": len(reviews),
        "average_score": idea.average_score,
    }


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=REPORTING_SERVICE_PORT)