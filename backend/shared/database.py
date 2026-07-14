"""
Shared Database Configuration for IMTS Microservices
SQLAlchemy with pyodbc for SQL Server, falls back to SQLite for dev
Exports: Base, engine, SessionLocal, get_db(), init_db()
"""

import os
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger("imts.db")

from sqlalchemy import create_engine, text
from sqlalchemy.orm import sessionmaker, declarative_base

from dotenv import load_dotenv

load_dotenv()

DB_SERVER = os.getenv("DATABASE_SERVER", "localhost")
DB_PORT = os.getenv("DATABASE_PORT", "1433")
DB_USER = os.getenv("DATABASE_USER", "sa")
DB_PASSWORD = os.getenv("DATABASE_PASSWORD", "YourStrong@Password123")
DB_NAME = os.getenv("DATABASE_NAME", "IMTS")

# Declarative base - created immediately for model imports
Base = declarative_base()

# Lazy engine and session - these get populated on first use
_engine = None
_SessionLocal = None
engine = None
SessionLocal = None


def get_engine():
    """Create engine lazily: try SQL Server, fallback to SQLite"""
    global _engine
    if _engine is not None:
        return _engine

    # Try SQL Server via pyodbc
    try:
        mssql_url = (
            f"mssql+pyodbc://{DB_USER}:{DB_PASSWORD}@{DB_SERVER}:{DB_PORT}/{DB_NAME}"
            f"?driver=ODBC+Driver+17+for+SQL+Server"
        )
        test_engine = create_engine(mssql_url, pool_pre_ping=True, pool_size=5, echo=False)
        with test_engine.connect() as conn:
            conn.execute(text("SELECT 1"))
        logger.info("Connected to SQL Server via pyodbc")
        _engine = test_engine
        return _engine
    except Exception as e:
        logger.warning(f"SQL Server unavailable: {e}")

    # Fallback to SQLite
    db_dir = os.path.dirname(os.path.abspath(__file__))
    db_path = os.path.join(db_dir, "..", "imts_dev.db")
    sqlite_engine = create_engine(
        f"sqlite:///{db_path}",
        connect_args={"check_same_thread": False},
        echo=False,
    )
    logger.info(f"Using SQLite at: {db_path}")
    _engine = sqlite_engine
    return _engine


def _ensure_globals():
    """Sync module-level engine/SessionLocal after lazy init"""
    global engine, SessionLocal
    if engine is None:
        engine = get_engine()
    if SessionLocal is None:
        SessionLocal = get_session()


def get_session():
    """Get session factory (lazy)"""
    global _SessionLocal
    if _SessionLocal is None:
        _SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=get_engine())
    return _SessionLocal


def get_db():
    """FastAPI dependency for DB session"""
    _ensure_globals()
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()


def init_db():
    """Create all tables"""
    _ensure_globals()
    from backend.shared.models import User  # noqa: F401 - ensure models registered
    Base.metadata.create_all(bind=get_engine())
    logger.info("All database tables created")