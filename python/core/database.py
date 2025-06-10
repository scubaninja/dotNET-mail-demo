from sqlalchemy import create_engine
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker
from contextlib import contextmanager

from core.config import get_config

config = get_config()
DATABASE_URL = config.get("DATABASE_URL")

engine = create_engine(DATABASE_URL)
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)
Base = declarative_base()

@contextmanager
def get_db():
    """Database session context manager"""
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()

class DB:
    """Database connection handler, similar to the .NET IDb interface"""
    
    @staticmethod
    def connect():
        """Get a database session"""
        return SessionLocal()
    
    @staticmethod
    @contextmanager
    def connection():
        """Context manager for database connection"""
        with get_db() as db:
            yield db
            
    @staticmethod
    def postgres():
        """Get the database connection string"""
        return DATABASE_URL
