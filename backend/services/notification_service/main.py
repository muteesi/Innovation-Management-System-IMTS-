"""
Notification Service - Manages in-app notifications and email sending
Port: 8006
"""

import sys
import os

sys.path.insert(0, os.path.join(os.path.dirname(__file__), "../.."))

from datetime import datetime, timezone
from typing import Optional

from fastapi import FastAPI, Depends, HTTPException, status, Query
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from sqlalchemy.orm import Session
from sqlalchemy import desc

from backend.shared.database import get_db, engine, Base
from backend.shared.models import Notification, User, AuditLog
from backend.shared.auth_utils import get_current_user, require_role
from backend.shared.config import NOTIFICATION_SERVICE_PORT, SMTP_SERVER, SMTP_PORT, SMTP_USERNAME, SMTP_PASSWORD, NOTIFICATION_EMAIL_FROM

app = FastAPI(
    title="IMTS Notification Service",
    description="In-app & Email Notification Service",
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
class NotificationCreate(BaseModel):
    user_id: Optional[int] = None
    email: Optional[str] = None
    notification_type: str = "general"
    title: str
    message: str
    related_idea_id: Optional[int] = None
    send_email: bool = False


class EmailNotification(BaseModel):
    to_email: str
    subject: str
    body: str
    html_body: Optional[str] = None


@app.on_event("startup")
def startup():
    Base.metadata.create_all(bind=engine)
    print(f"🔔 Notification Service running on port {NOTIFICATION_SERVICE_PORT}")


# ================================================================
# NOTIFICATIONS CRUD
# ================================================================
@app.get("/api/notifications")
def list_notifications(
    is_read: Optional[bool] = Query(None),
    notification_type: Optional[str] = Query(None),
    page: int = Query(1, ge=1),
    per_page: int = Query(20, ge=1, le=100),
    current_user: dict = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """List notifications for current user"""
    query = db.query(Notification).filter(
        (Notification.user_id == current_user.get("user_id"))
        | (Notification.email == current_user.get("username"))
    )

    if is_read is not None:
        query = query.filter(Notification.is_read == is_read)
    if notification_type:
        query = query.filter(Notification.notification_type == notification_type)

    total = query.count()
    notifications = (
        query.order_by(desc(Notification.created_at))
        .offset((page - 1) * per_page)
        .limit(per_page)
        .all()
    )

    return {
        "notifications": [n.to_dict() for n in notifications],
        "total": total,
        "page": page,
        "per_page": per_page,
        "unread_count": db.query(Notification)
        .filter(
            Notification.is_read == False,
            Notification.user_id == current_user.get("user_id"),
        )
        .count(),
    }


@app.get("/api/notifications/unread-count")
def get_unread_count(
    current_user: dict = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """Get unread notification count for current user"""
    count = (
        db.query(Notification)
        .filter(
            Notification.is_read == False,
            Notification.user_id == current_user.get("user_id"),
        )
        .count()
    )
    return {"unread_count": count}


@app.post("/api/notifications/mark-read/{notification_id}")
def mark_notification_read(
    notification_id: int,
    current_user: dict = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """Mark a notification as read"""
    notification = (
        db.query(Notification)
        .filter(Notification.id == notification_id)
        .first()
    )
    if not notification:
        raise HTTPException(status_code=404, detail="Notification not found")

    notification.is_read = True
    notification.read_at = datetime.now(timezone.utc)
    db.commit()
    return {"message": "Notification marked as read"}


@app.post("/api/notifications/mark-all-read")
def mark_all_read(
    current_user: dict = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """Mark all notifications as read for current user"""
    db.query(Notification).filter(
        Notification.is_read == False,
        Notification.user_id == current_user.get("user_id"),
    ).update({"is_read": True, "read_at": datetime.now(timezone.utc)})
    db.commit()
    return {"message": "All notifications marked as read"}


@app.post("/api/notifications")
def create_notification(
    data: NotificationCreate,
    current_user: dict = Depends(require_role("ITAdmin", "InnovationTeam")),
    db: Session = Depends(get_db),
):
    """Create a new notification (Admin/Innovation Team only)"""
    notification = Notification(
        user_id=data.user_id,
        email=data.email,
        notification_type=data.notification_type,
        title=data.title,
        message=data.message,
        related_idea_id=data.related_idea_id,
    )
    db.add(notification)
    db.commit()
    db.refresh(notification)

    # Send email if requested
    if data.send_email and data.email:
        try:
            send_email_sync(
                to_email=data.email,
                subject=data.title,
                body=data.message,
            )
            notification.sent_via_email = True
            db.commit()
        except Exception as e:
            print(f"⚠️ Failed to send email: {e}")

    return notification.to_dict()


# ================================================================
# EMAIL SENDING
# ================================================================
def send_email_sync(to_email: str, subject: str, body: str, html_body: Optional[str] = None):
    """Send an email via SMTP (synchronous fallback)"""
    import smtplib
    from email.mime.text import MIMEText
    from email.mime.multipart import MIMEMultipart

    if not SMTP_SERVER or not SMTP_USERNAME:
        print(f"📧 Email sending not configured. Would send to {to_email}: {subject}")
        return

    msg = MIMEMultipart("alternative")
    msg["Subject"] = subject
    msg["From"] = NOTIFICATION_EMAIL_FROM
    msg["To"] = to_email

    # Plain text version
    msg.attach(MIMEText(body, "plain"))

    # HTML version (if provided)
    if html_body:
        msg.attach(MIMEText(html_body, "html"))

    try:
        with smtplib.SMTP(SMTP_SERVER, SMTP_PORT) as server:
            server.starttls()
            server.login(SMTP_USERNAME, SMTP_PASSWORD)
            server.sendmail(NOTIFICATION_EMAIL_FROM, to_email, msg.as_string())
        print(f"✅ Email sent to {to_email}")
    except Exception as e:
        print(f"❌ Email send failed: {e}")
        raise


@app.post("/api/notifications/send-email")
def send_email(
    data: EmailNotification,
    current_user: dict = Depends(require_role("ITAdmin")),
):
    """Send an email notification (IT Admin only)"""
    send_email_sync(
        to_email=data.to_email,
        subject=data.subject,
        body=data.body,
        html_body=data.html_body,
    )
    return {"message": f"Email sent to {data.to_email}"}


@app.delete("/api/notifications/{notification_id}")
def delete_notification(
    notification_id: int,
    current_user: dict = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """Delete a notification"""
    notification = (
        db.query(Notification)
        .filter(Notification.id == notification_id)
        .first()
    )
    if not notification:
        raise HTTPException(status_code=404, detail="Notification not found")

    db.delete(notification)
    db.commit()
    return {"message": "Notification deleted successfully"}


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=NOTIFICATION_SERVICE_PORT)