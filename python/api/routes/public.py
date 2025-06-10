from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session
from typing import Dict, Any

from core.database import get_db
from models.contact import Contact, ContactDB, SignUpRequest
from commands.contact_commands import ContactOptOutCommand, LinkClickedCommand

router = APIRouter(tags=["public"])

@router.get("/about")
async def about():
    """Information about the API"""
    return "Tailwind Traders Mail Services API"

@router.get("/unsubscribe/{key}")
async def unsubscribe(key: str, db: Session = Depends(get_db)):
    """Unsubscribe from the mailing list"""
    cmd = ContactOptOutCommand(key)
    result = cmd.execute(db)
    return result.updated > 0

@router.get("/link/clicked/{key}")
async def link_clicked(key: str, db: Session = Depends(get_db)):
    """Track a link click"""
    cmd = LinkClickedCommand(key)
    result = cmd.execute(db)
    return result

@router.post("/signup")
async def signup(req: SignUpRequest, db: Session = Depends(get_db)):
    """Sign up for the mailing list"""
    # Check if email already exists
    existing = db.query(ContactDB).filter(ContactDB.email == req.email).first()
    if existing:
        raise HTTPException(status_code=400, detail="Email already registered")
        
    # Create new contact
    contact = ContactDB(
        email=req.email,
        name=req.name
    )
    db.add(contact)
    db.commit()
    db.refresh(contact)
    
    return Contact.from_orm(contact)
