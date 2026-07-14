"""
Auth Service - Handles user authentication, account management, API keys
Port: 8001
"""

import sys
import os

sys.path.insert(0, os.path.join(os.path.dirname(__file__), "../.."))

from datetime import datetime, timedelta, timezone
from typing import Optional

from fastapi import FastAPI, Depends, HTTPException, status
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, EmailStr
from sqlalchemy.orm import Session

from backend.shared.database import get_db, init_db, engine, Base
from backend.shared.models import User, ApiAccount, AuditLog
from backend.shared.auth_utils import (
    hash_password,
    verify_password,
    create_access_token,
    get_current_user,
    require_role,
)
from backend.shared.config import AUTH_SERVICE_PORT

app = FastAPI(
    title="IMTS Auth Service",
    description="Authentication & Account Management Service",
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
class LoginRequest(BaseModel):
    username: str
    password: str


class UserCreate(BaseModel):
    username: str
    email: str
    password: str
    full_name: str
    role: str = "Staff"
    department: Optional[str] = None
    business_unit: Optional[str] = None
    station: Optional[str] = None
    rank: Optional[str] = None


class UserUpdate(BaseModel):
    email: Optional[str] = None
    full_name: Optional[str] = None
    role: Optional[str] = None
    department: Optional[str] = None
    business_unit: Optional[str] = None
    station: Optional[str] = None
    rank: Optional[str] = None
    is_active: Optional[bool] = None
    is_locked: Optional[bool] = None


class TokenResponse(BaseModel):
    access_token: str
    token_type: str = "bearer"
    user: dict


class ApiAccountCreate(BaseModel):
    name: str
    description: Optional[str] = None
    ip_constraint: Optional[str] = None
    permission: str
    expires_in_days: Optional[int] = 365


class PasswordChange(BaseModel):
    current_password: str
    new_password: str


# ================================================================
# SEED DATA
# ================================================================
def seed_users():
    """Seed default users matching frontend demo credentials"""
    db = next(get_db())
    try:
        existing = db.query(User).count()
        if existing > 0:
            return

        users = [
            User(
                username="admin",
                email="admin@bankofuganda.org",
                password_hash=hash_password("Admin@123"),
                full_name="System Administrator",
                role="ITAdmin",
                department="Information Technology",
            ),
            User(
                username="katumba",
                email="katumba@bankofuganda.org",
                password_hash=hash_password("Admin@123"),
                full_name="S. Katumba",
                role="ITAdmin",
                department="Information Technology",
            ),
            User(
                username="staff",
                email="staff@bankofuganda.org",
                password_hash=hash_password("Staff@123"),
                full_name="Jonathan Doe",
                role="Staff",
                department="Operations",
            ),
            User(
                username="innovator",
                email="innovator@bankofuganda.org",
                password_hash=hash_password("Innovate@123"),
                full_name="Milton Reviewer",
                role="InnovationTeam",
                department="Innovation Hub",
            ),
        ]
        db.add_all(users)
        db.commit()
        print("✅ Seed users created successfully")
    except Exception as e:
        db.rollback()
        print(f"⚠️ Seed error (may already exist): {e}")
    finally:
        db.close()


def seed_categories():
    """Seed default idea categories"""
    from backend.shared.models import Category

    db = next(get_db())
    try:
        existing = db.query(Category).count()
        if existing > 0:
            return

        categories = [
            Category(name="Fintech Solutions", description="Financial technology innovations"),
            Category(name="Risk Management", description="Risk assessment and mitigation ideas"),
            Category(name="Public Policy", description="Policy-related innovations"),
            Category(name="Security", description="Security and fraud prevention"),
            Category(name="Sustainability", description="Environmental and sustainable initiatives"),
            Category(name="Digital Banking", description="Digital banking solutions"),
            Category(name="Operations", description="Operational efficiency improvements"),
            Category(name="Process Improvement", description="Process optimization ideas"),
            Category(name="Digital Transformation", description="Digital transformation initiatives"),
            Category(name="Product Innovation", description="New product development"),
            Category(name="Customer Experience", description="Customer service improvements"),
        ]
        db.add_all(categories)
        db.commit()
        print("✅ Seed categories created successfully")
    except Exception as e:
        db.rollback()
        print(f"⚠️ Seed categories error: {e}")
    finally:
        db.close()


@app.on_event("startup")
def startup():
    Base.metadata.create_all(bind=engine)
    seed_users()
    seed_categories()
    print(f"🔐 Auth Service running on port {AUTH_SERVICE_PORT}")


# ================================================================
# AUTH ENDPOINTS
# ================================================================
@app.post("/api/auth/login", response_model=TokenResponse)
def login(request: LoginRequest, db: Session = Depends(get_db)):
    """Authenticate user and return JWT token"""
    user = db.query(User).filter(User.username == request.username.lower()).first()

    if not user or not verify_password(request.password, user.password_hash):
        # Log failed attempt
        log = AuditLog(
            username=request.username,
            action="LOGIN_FAILED",
            resource_type="auth",
            details={"reason": "invalid_credentials"},
        )
        db.add(log)
        db.commit()
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Invalid username or password",
        )

    if user.is_locked:
        raise HTTPException(
            status_code=status.HTTP_423_LOCKED,
            detail="Account is locked. Contact administrator.",
        )

    if not user.is_active:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Account is deactivated. Contact administrator.",
        )

    # Update last login
    user.last_login = datetime.now(timezone.utc)
    db.commit()

    # Create token
    token = create_access_token(
        data={
            "sub": user.username,
            "user_id": user.id,
            "role": user.role,
            "name": user.full_name,
        },
        expires_delta=timedelta(minutes=60),
    )

    # Log successful login
    log = AuditLog(
        user_id=user.id,
        username=user.username,
        action="LOGIN_SUCCESS",
        resource_type="auth",
    )
    db.add(log)
    db.commit()

    return TokenResponse(
        access_token=token,
        user={
            "id": user.id,
            "username": user.username,
            "name": user.full_name,
            "email": user.email,
            "role": user.role,
            "department": user.department,
        },
    )


@app.get("/api/auth/me")
def get_me(current_user: dict = Depends(get_current_user), db: Session = Depends(get_db)):
    """Get current user profile"""
    user = db.query(User).filter(User.username == current_user["username"]).first()
    if not user:
        raise HTTPException(status_code=404, detail="User not found")
    return user.to_dict()


@app.post("/api/auth/change-password")
def change_password(
    data: PasswordChange,
    current_user: dict = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """Change current user's password"""
    user = db.query(User).filter(User.username == current_user["username"]).first()
    if not user:
        raise HTTPException(status_code=404, detail="User not found")

    if not verify_password(data.current_password, user.password_hash):
        raise HTTPException(status_code=400, detail="Current password is incorrect")

    user.password_hash = hash_password(data.new_password)
    db.commit()

    log = AuditLog(
        user_id=user.id,
        username=user.username,
        action="PASSWORD_CHANGED",
        resource_type="auth",
    )
    db.add(log)
    db.commit()

    return {"message": "Password changed successfully"}


# ================================================================
# USER MANAGEMENT (IT Admin only)
# ================================================================
@app.get("/api/auth/users")
def list_users(
    current_user: dict = Depends(require_role("ITAdmin")),
    db: Session = Depends(get_db),
):
    """List all users (IT Admin only)"""
    users = db.query(User).all()
    return [u.to_dict() for u in users]


@app.post("/api/auth/users")
def create_user(
    data: UserCreate,
    current_user: dict = Depends(require_role("ITAdmin")),
    db: Session = Depends(get_db),
):
    """Create a new user (IT Admin only)"""
    existing = db.query(User).filter(
        (User.username == data.username.lower()) | (User.email == data.email)
    ).first()
    if existing:
        raise HTTPException(status_code=400, detail="Username or email already exists")

    user = User(
        username=data.username.lower(),
        email=data.email,
        password_hash=hash_password(data.password),
        full_name=data.full_name,
        role=data.role,
        department=data.department,
        business_unit=data.business_unit,
        station=data.station,
        rank=data.rank,
    )
    db.add(user)
    db.commit()
    db.refresh(user)

    log = AuditLog(
        user_id=current_user["user_id"],
        username=current_user["username"],
        action="USER_CREATED",
        resource_type="user",
        resource_id=user.id,
        details={"created_user": user.username},
    )
    db.add(log)
    db.commit()

    return user.to_dict()


@app.put("/api/auth/users/{user_id}")
def update_user(
    user_id: int,
    data: UserUpdate,
    current_user: dict = Depends(require_role("ITAdmin")),
    db: Session = Depends(get_db),
):
    """Update a user (IT Admin only)"""
    user = db.query(User).filter(User.id == user_id).first()
    if not user:
        raise HTTPException(status_code=404, detail="User not found")

    update_data = data.model_dump(exclude_unset=True)
    for key, value in update_data.items():
        setattr(user, key, value)

    db.commit()
    db.refresh(user)

    log = AuditLog(
        user_id=current_user["user_id"],
        username=current_user["username"],
        action="USER_UPDATED",
        resource_type="user",
        resource_id=user.id,
    )
    db.add(log)
    db.commit()

    return user.to_dict()


# ================================================================
# API ACCOUNTS
# ================================================================
@app.get("/api/auth/api-accounts")
def list_api_accounts(
    current_user: dict = Depends(require_role("ITAdmin")),
    db: Session = Depends(get_db),
):
    """List all API accounts"""
    accounts = db.query(ApiAccount).all()
    return [a.to_dict() for a in accounts]


@app.post("/api/auth/api-accounts")
def create_api_account(
    data: ApiAccountCreate,
    current_user: dict = Depends(require_role("ITAdmin")),
    db: Session = Depends(get_db),
):
    """Create a new API account"""
    import secrets
    import hashlib

    raw_key = f"imts_{secrets.token_hex(24)}"
    key_hash = hashlib.sha256(raw_key.encode()).hexdigest()[:20]

    account = ApiAccount(
        name=data.name,
        description=data.description,
        api_key_hash=key_hash,
        ip_constraint=data.ip_constraint,
        permission=data.permission,
        expires_at=datetime.now(timezone.utc)
        + timedelta(days=data.expires_in_days or 365),
    )
    db.add(account)
    db.commit()
    db.refresh(account)

    log = AuditLog(
        user_id=current_user["user_id"],
        username=current_user["username"],
        action="API_ACCOUNT_CREATED",
        resource_type="api_account",
        resource_id=account.id,
    )
    db.add(log)
    db.commit()

    return {**account.to_dict(), "raw_key": raw_key}


@app.post("/api/auth/api-accounts/{account_id}/rotate")
def rotate_api_key(
    account_id: int,
    current_user: dict = Depends(require_role("ITAdmin")),
    db: Session = Depends(get_db),
):
    """Rotate API key"""
    import secrets
    import hashlib

    account = db.query(ApiAccount).filter(ApiAccount.id == account_id).first()
    if not account:
        raise HTTPException(status_code=404, detail="API account not found")

    raw_key = f"imts_{secrets.token_hex(24)}"
    account.api_key_hash = hashlib.sha256(raw_key.encode()).hexdigest()[:20]
    db.commit()

    return {**account.to_dict(), "raw_key": raw_key}


@app.put("/api/auth/api-accounts/{account_id}/toggle-status")
def toggle_api_account_status(
    account_id: int,
    current_user: dict = Depends(require_role("ITAdmin")),
    db: Session = Depends(get_db),
):
    """Toggle API account status (active/locked)"""
    account = db.query(ApiAccount).filter(ApiAccount.id == account_id).first()
    if not account:
        raise HTTPException(status_code=404, detail="API account not found")

    account.status = "locked" if account.status == "active" else "active"
    db.commit()
    return account.to_dict()


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=AUTH_SERVICE_PORT)