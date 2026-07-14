"""
IMTS Database Models for all Microservices
Covers: Auth, Ideas, Workflow, Resources, Reporting, Notifications
"""

from sqlalchemy import (
    Column, Integer, String, Text, DateTime, Float, Boolean,
    ForeignKey, JSON, Date,
)
from sqlalchemy.orm import relationship
from sqlalchemy.sql import func
import enum

from backend.shared.database import Base


# ================================================================
# ENUMS
# ================================================================
class UserRole(str, enum.Enum):
    STAFF = "Staff"
    IT_ADMIN = "ITAdmin"
    INNOVATION_TEAM = "InnovationTeam"


class IdeaStatus(str, enum.Enum):
    PENDING = "Pending"
    UNDER_REVIEW = "Under Review"
    APPROVED = "Approved"
    DECLINED = "Declined"
    IN_DEVELOPMENT = "In Development"
    DEPLOYED = "Deployed"


class SubmissionType(str, enum.Enum):
    INDIVIDUAL = "individual"
    TEAM = "team"


class NotificationType(str, enum.Enum):
    STATUS_CHANGE = "status_change"
    NEW_IDEA = "new_idea"
    REVIEW_COMPLETED = "review_completed"
    GENERAL = "general"


# ================================================================
# AUTH SERVICE MODELS
# ================================================================
class User(Base):
    __tablename__ = "users"

    id = Column(Integer, primary_key=True, autoincrement=True)
    username = Column(String(100), unique=True, nullable=False, index=True)
    email = Column(String(255), unique=True, nullable=False)
    password_hash = Column(String(255), nullable=False)
    full_name = Column(String(255), nullable=False)
    role = Column(String(50), nullable=False, default=UserRole.STAFF.value)
    department = Column(String(255), nullable=True)
    business_unit = Column(String(255), nullable=True)
    station = Column(String(255), nullable=True)
    rank = Column(String(100), nullable=True)
    is_active = Column(Boolean, default=True)
    is_locked = Column(Boolean, default=False)
    last_login = Column(DateTime, nullable=True)
    created_at = Column(DateTime, server_default=func.now())
    updated_at = Column(DateTime, server_default=func.now(), onupdate=func.now())

    ideas = relationship("Idea", back_populates="submitter_user")
    reviews = relationship("Review", back_populates="reviewer_user")
    notifications = relationship("Notification", back_populates="user")

    def to_dict(self):
        return {
            "id": self.id,
            "username": self.username,
            "email": self.email,
            "full_name": self.full_name,
            "role": self.role,
            "department": self.department,
            "business_unit": self.business_unit,
            "station": self.station,
            "rank": self.rank,
            "is_active": self.is_active,
            "is_locked": self.is_locked,
            "last_login": self.last_login.isoformat() if self.last_login else None,
            "created_at": self.created_at.isoformat() if self.created_at else None,
        }


class ApiAccount(Base):
    __tablename__ = "api_accounts"

    id = Column(Integer, primary_key=True, autoincrement=True)
    name = Column(String(255), nullable=False)
    description = Column(Text, nullable=True)
    api_key_hash = Column(String(255), nullable=False)
    ip_constraint = Column(String(50), nullable=True)
    permission = Column(String(100), nullable=False)
    status = Column(String(20), default="active")
    created_at = Column(DateTime, server_default=func.now())
    expires_at = Column(DateTime, nullable=True)

    def to_dict(self):
        return {
            "id": self.id,
            "name": self.name,
            "description": self.description,
            "ip_constraint": self.ip_constraint,
            "permission": self.permission,
            "status": self.status,
            "expires_at": self.expires_at.isoformat() if self.expires_at else None,
        }


# ================================================================
# IDEAS SERVICE MODELS
# ================================================================
class Category(Base):
    __tablename__ = "categories"

    id = Column(Integer, primary_key=True, autoincrement=True)
    name = Column(String(255), unique=True, nullable=False)
    description = Column(Text, nullable=True)
    is_active = Column(Boolean, default=True)
    created_at = Column(DateTime, server_default=func.now())

    ideas = relationship("Idea", back_populates="category_obj")

    def to_dict(self):
        return {"id": self.id, "name": self.name, "description": self.description, "is_active": self.is_active}


class Idea(Base):
    __tablename__ = "ideas"

    id = Column(Integer, primary_key=True, autoincrement=True)
    title = Column(String(500), nullable=False, index=True)
    description = Column(Text, nullable=False)
    problem_statement = Column(Text, nullable=True)
    proposed_solution = Column(Text, nullable=True)
    expected_impact = Column(Text, nullable=True)
    submitter_id = Column(Integer, ForeignKey("users.id"), nullable=True)
    submitter_name = Column(String(255), nullable=False)
    submitter_email = Column(String(255), nullable=False)
    submitter_department = Column(String(255), nullable=True)
    submitter_rank = Column(String(100), nullable=True)
    business_unit = Column(String(255), nullable=True)
    station = Column(String(255), nullable=True)
    submission_type = Column(String(20), default=SubmissionType.INDIVIDUAL.value)
    team_members = Column(Text, nullable=True)
    category_id = Column(Integer, ForeignKey("categories.id"), nullable=True)
    category_name = Column(String(255), nullable=True)
    status = Column(String(50), default=IdeaStatus.PENDING.value, index=True)
    current_stage = Column(String(100), nullable=True)
    average_score = Column(Float, nullable=True)
    review_count = Column(Integer, default=0)
    submitted_at = Column(DateTime, server_default=func.now())
    updated_at = Column(DateTime, server_default=func.now(), onupdate=func.now())
    deployed_at = Column(DateTime, nullable=True)

    submitter_user = relationship("User", back_populates="ideas")
    category_obj = relationship("Category", back_populates="ideas")
    attachments = relationship("Attachment", back_populates="idea", cascade="all, delete-orphan")
    reviews = relationship("Review", back_populates="idea", cascade="all, delete-orphan")
    team_compositions = relationship("TeamComposition", back_populates="idea", cascade="all, delete-orphan")
    status_history = relationship("IdeaStatusHistory", back_populates="idea", cascade="all, delete-orphan")

    def to_dict(self, include_relations=False):
        result = {
            "id": self.id, "title": self.title, "description": self.description,
            "problem_statement": self.problem_statement, "proposed_solution": self.proposed_solution,
            "expected_impact": self.expected_impact, "submitter_id": self.submitter_id,
            "submitter_name": self.submitter_name, "submitter_email": self.submitter_email,
            "submitter_department": self.submitter_department, "submitter_rank": self.submitter_rank,
            "business_unit": self.business_unit, "station": self.station,
            "submission_type": self.submission_type, "team_members": self.team_members,
            "category_id": self.category_id, "category": self.category_name,
            "status": self.status, "current_stage": self.current_stage,
            "average_score": self.average_score, "review_count": self.review_count,
            "submitted_at": self.submitted_at.isoformat() if self.submitted_at else None,
            "updated_at": self.updated_at.isoformat() if self.updated_at else None,
            "deployed_at": self.deployed_at.isoformat() if self.deployed_at else None,
        }
        if include_relations:
            result["attachments"] = [a.to_dict() for a in self.attachments]
            result["reviews"] = [r.to_dict() for r in self.reviews]
            result["team_compositions"] = [tc.to_dict() for tc in self.team_compositions]
            result["status_history"] = [sh.to_dict() for sh in self.status_history]
        return result


class Attachment(Base):
    __tablename__ = "attachments"

    id = Column(Integer, primary_key=True, autoincrement=True)
    idea_id = Column(Integer, ForeignKey("ideas.id"), nullable=False)
    file_name = Column(String(500), nullable=False)
    file_type = Column(String(100), nullable=True)
    file_size = Column(Integer, nullable=True)
    file_data = Column(Text, nullable=True)
    uploaded_at = Column(DateTime, server_default=func.now())

    idea = relationship("Idea", back_populates="attachments")

    def to_dict(self):
        return {"id": self.id, "idea_id": self.idea_id, "file_name": self.file_name,
                "file_type": self.file_type, "file_size": self.file_size,
                "uploaded_at": self.uploaded_at.isoformat() if self.uploaded_at else None}


class TeamComposition(Base):
    __tablename__ = "team_compositions"

    id = Column(Integer, primary_key=True, autoincrement=True)
    idea_id = Column(Integer, ForeignKey("ideas.id"), nullable=False)
    composition_type = Column(String(50), nullable=False)
    composition_key = Column(String(100), nullable=False)
    composition_label = Column(String(255), nullable=True)
    staff_count = Column(Integer, nullable=False)

    idea = relationship("Idea", back_populates="team_compositions")

    def to_dict(self):
        return {"id": self.id, "idea_id": self.idea_id,
                "composition_type": self.composition_type, "composition_key": self.composition_key,
                "composition_label": self.composition_label, "staff_count": self.staff_count}


# ================================================================
# WORKFLOW SERVICE MODELS
# ================================================================
class Review(Base):
    __tablename__ = "reviews"

    id = Column(Integer, primary_key=True, autoincrement=True)
    idea_id = Column(Integer, ForeignKey("ideas.id"), nullable=False)
    reviewer_id = Column(Integer, ForeignKey("users.id"), nullable=True)
    reviewer_name = Column(String(255), nullable=True)
    score = Column(Integer, nullable=True)
    decision = Column(String(50), nullable=False)
    feedback = Column(Text, nullable=True)
    reviewed_at = Column(DateTime, server_default=func.now())

    idea = relationship("Idea", back_populates="reviews")
    reviewer_user = relationship("User", back_populates="reviews")

    def to_dict(self):
        return {"id": self.id, "idea_id": self.idea_id, "reviewer_id": self.reviewer_id,
                "reviewer_name": self.reviewer_name, "score": self.score,
                "decision": self.decision, "feedback": self.feedback,
                "reviewed_at": self.reviewed_at.isoformat() if self.reviewed_at else None}


class IdeaStatusHistory(Base):
    __tablename__ = "idea_status_history"

    id = Column(Integer, primary_key=True, autoincrement=True)
    idea_id = Column(Integer, ForeignKey("ideas.id"), nullable=False)
    from_status = Column(String(50), nullable=True)
    to_status = Column(String(50), nullable=False)
    changed_by = Column(String(255), nullable=True)
    change_note = Column(Text, nullable=True)
    changed_at = Column(DateTime, server_default=func.now())

    idea = relationship("Idea", back_populates="status_history")

    def to_dict(self):
        return {"id": self.id, "idea_id": self.idea_id, "from_status": self.from_status,
                "to_status": self.to_status, "changed_by": self.changed_by,
                "change_note": self.change_note,
                "changed_at": self.changed_at.isoformat() if self.changed_at else None}


class Timeline(Base):
    __tablename__ = "timelines"

    id = Column(Integer, primary_key=True, autoincrement=True)
    idea_id = Column(Integer, ForeignKey("ideas.id"), nullable=True)
    milestone_name = Column(String(255), nullable=False)
    milestone_description = Column(Text, nullable=True)
    start_date = Column(Date, nullable=True)
    due_date = Column(Date, nullable=True)
    completed_date = Column(Date, nullable=True)
    status = Column(String(50), default="pending")
    assigned_to = Column(String(255), nullable=True)
    created_at = Column(DateTime, server_default=func.now())

    def to_dict(self):
        return {"id": self.id, "idea_id": self.idea_id,
                "milestone_name": self.milestone_name,
                "milestone_description": self.milestone_description,
                "start_date": self.start_date.isoformat() if self.start_date else None,
                "due_date": self.due_date.isoformat() if self.due_date else None,
                "completed_date": self.completed_date.isoformat() if self.completed_date else None,
                "status": self.status, "assigned_to": self.assigned_to}


# ================================================================
# RESOURCES SERVICE MODELS
# ================================================================
class Resource(Base):
    __tablename__ = "resources"

    id = Column(Integer, primary_key=True, autoincrement=True)
    title = Column(String(500), nullable=False)
    description = Column(Text, nullable=True)
    resource_type = Column(String(100), nullable=True)
    category = Column(String(255), nullable=True)
    url = Column(String(1000), nullable=True)
    file_data = Column(Text, nullable=True)
    file_name = Column(String(500), nullable=True)
    file_size = Column(Integer, nullable=True)
    is_public = Column(Boolean, default=True)
    uploaded_by = Column(Integer, ForeignKey("users.id"), nullable=True)
    created_at = Column(DateTime, server_default=func.now())
    updated_at = Column(DateTime, server_default=func.now(), onupdate=func.now())

    def to_dict(self):
        return {"id": self.id, "title": self.title, "description": self.description,
                "resource_type": self.resource_type, "category": self.category,
                "url": self.url, "file_name": self.file_name, "file_size": self.file_size,
                "is_public": self.is_public, "uploaded_by": self.uploaded_by,
                "created_at": self.created_at.isoformat() if self.created_at else None}


class ResourceCategory(Base):
    __tablename__ = "resource_categories"

    id = Column(Integer, primary_key=True, autoincrement=True)
    name = Column(String(255), unique=True, nullable=False)
    description = Column(Text, nullable=True)
    icon = Column(String(100), nullable=True)
    created_at = Column(DateTime, server_default=func.now())

    def to_dict(self):
        return {"id": self.id, "name": self.name, "description": self.description, "icon": self.icon}


# ================================================================
# REPORTING SERVICE MODELS
# ================================================================
class AuditLog(Base):
    __tablename__ = "audit_logs"

    id = Column(Integer, primary_key=True, autoincrement=True)
    user_id = Column(Integer, ForeignKey("users.id"), nullable=True)
    username = Column(String(100), nullable=True)
    action = Column(String(255), nullable=False)
    resource_type = Column(String(100), nullable=True)
    resource_id = Column(Integer, nullable=True)
    details = Column(JSON, nullable=True)
    ip_address = Column(String(50), nullable=True)
    user_agent = Column(String(500), nullable=True)
    created_at = Column(DateTime, server_default=func.now())

    def to_dict(self):
        return {"id": self.id, "user_id": self.user_id, "username": self.username,
                "action": self.action, "resource_type": self.resource_type,
                "resource_id": self.resource_id, "details": self.details,
                "ip_address": self.ip_address,
                "created_at": self.created_at.isoformat() if self.created_at else None}


class Report(Base):
    __tablename__ = "reports"

    id = Column(Integer, primary_key=True, autoincrement=True)
    title = Column(String(500), nullable=False)
    report_type = Column(String(100), nullable=False)
    parameters = Column(JSON, nullable=True)
    result_data = Column(JSON, nullable=True)
    generated_by = Column(Integer, ForeignKey("users.id"), nullable=True)
    generated_at = Column(DateTime, server_default=func.now())

    def to_dict(self):
        return {"id": self.id, "title": self.title, "report_type": self.report_type,
                "parameters": self.parameters, "generated_by": self.generated_by,
                "generated_at": self.generated_at.isoformat() if self.generated_at else None}


# ================================================================
# NOTIFICATION SERVICE MODELS
# ================================================================
class Notification(Base):
    __tablename__ = "notifications"

    id = Column(Integer, primary_key=True, autoincrement=True)
    user_id = Column(Integer, ForeignKey("users.id"), nullable=True)
    email = Column(String(255), nullable=True)
    notification_type = Column(String(50), default=NotificationType.GENERAL.value)
    title = Column(String(500), nullable=False)
    message = Column(Text, nullable=False)
    related_idea_id = Column(Integer, nullable=True)
    is_read = Column(Boolean, default=False)
    sent_via_email = Column(Boolean, default=False)
    created_at = Column(DateTime, server_default=func.now())
    read_at = Column(DateTime, nullable=True)

    user = relationship("User", back_populates="notifications")

    def to_dict(self):
        return {"id": self.id, "user_id": self.user_id, "email": self.email,
                "notification_type": self.notification_type, "title": self.title,
                "message": self.message, "related_idea_id": self.related_idea_id,
                "is_read": self.is_read, "sent_via_email": self.sent_via_email,
                "created_at": self.created_at.isoformat() if self.created_at else None,
                "read_at": self.read_at.isoformat() if self.read_at else None}