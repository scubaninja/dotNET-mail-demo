from fastapi import APIRouter, Depends, HTTPException, Query
from sqlalchemy.orm import Session
from pydantic import BaseModel
from typing import List, Optional, Dict, Any

from core.database import get_db
from models.message import MarkdownEmail
from models.contact import Contact, ContactDB
from models.broadcast import Broadcast
from commands.broadcast_commands import CreateBroadcast
from commands.contact_commands import BulkTagCommand
from services.ai import Chat

router = APIRouter(tags=["admin"])

# Request/Response models
class ValidationRequest(BaseModel):
    markdown: Optional[str] = None

class ValidationResponse(BaseModel):
    valid: bool
    message: str = "The markdown is valid"
    contacts: int = 0
    data: Optional[Dict[str, Any]] = None

class ChatRequest(BaseModel):
    prompt: Optional[str] = None

class ChatResponse(BaseModel):
    success: bool
    prompt: Optional[str] = None
    reply: Optional[str] = None

class QueueBroadcastResponse(BaseModel):
    success: bool
    message: Optional[str] = None
    result: Optional[Dict[str, Any]] = None

class ContactSearchResponse(BaseModel):
    term: Optional[str] = None
    contacts: List[Contact] = []

class BulkTagRequest(BaseModel):
    tag: str
    emails: List[str]

class BulkTagResponse(BaseModel):
    success: bool = False
    message: str = "No response"
    created: int = 0
    updated: int = 0

# Routes
@router.post("/validate", response_model=ValidationResponse)
async def validate(req: ValidationRequest, db: Session = Depends(get_db)):
    """Validate the markdown for an email"""
    if not req.markdown:
        return ValidationResponse(
            valid=False,
            message="The markdown is null"
        )
    
    doc = MarkdownEmail.from_string(req.markdown)
    if not doc.is_valid():
        return ValidationResponse(
            valid=False,
            message="Ensure there is a Subject and Summary in the markdown",
            data=doc.data.dict() if doc.data else None
        )
    
    broadcast = Broadcast.from_markdown_email(doc)
    contacts = broadcast.contact_count(db)
    
    return ValidationResponse(
        valid=True,
        data=doc.data.dict() if doc.data else None,
        contacts=contacts
    )

@router.post("/queue-broadcast", response_model=QueueBroadcastResponse)
async def queue_broadcast(req: ValidationRequest, db: Session = Depends(get_db)):
    """Queue a broadcast for your contacts"""
    doc = MarkdownEmail.from_string(req.markdown)
    if not doc.is_valid():
        return QueueBroadcastResponse(
            success=False,
            message="Ensure there is a Body, Subject and Summary in the markdown"
        )
    
    command = CreateBroadcast(doc)
    res = command.execute(db)
    
    return QueueBroadcastResponse(
        success=res.inserted > 0,
        message=f"The broadcast was queued with ID {res.data.get('broadcast_id')} and {res.inserted} messages were created",
        result={"inserted": res.inserted, "data": res.data}
    )

@router.post("/get-chat", response_model=ChatResponse)
async def get_chat(req: ChatRequest):
    """Generate AI-powered content for emails"""
    if not req.prompt:
        return ChatResponse(
            success=False,
            prompt=req.prompt,
            reply="Ensure there is a Subject and Prompt in the request"
        )
    
    chat = Chat()
    res = await chat.prompt(req.prompt)
    
    return ChatResponse(
        success=True,
        prompt=req.prompt,
        reply=res
    )

@router.get("/contacts/search", response_model=ContactSearchResponse)
async def search_contacts(term: str = Query(...), db: Session = Depends(get_db)):
    """Find one or more contacts using a fuzzy match on email or name"""
    # In PostgreSQL, ~* is case-insensitive regex match
    contacts = db.query(ContactDB).filter(
        (ContactDB.email.op("~*")(term)) | (ContactDB.name.op("~*")(term))
    ).all()
    
    return ContactSearchResponse(
        term=term,
        contacts=[Contact.from_orm(c) for c in contacts]
    )

@router.post("/bulk/contacts/tag", response_model=BulkTagResponse)
async def bulk_tag_contacts(request: BulkTagRequest, db: Session = Depends(get_db)):
    """Tag a set of contacts"""
    if not request.tag or not request.emails:
        return BulkTagResponse(
            success=False,
            message="Be sure to include a tag and at least one email address and a tag"
        )
    
    # Handle multiple tags
    tags = request.tag.split(",")
    total_inserted = 0
    total_updated = 0
    
    for tag in tags:
        cmd = BulkTagCommand(
            tag=tag.strip(),
            emails=request.emails
        )
        result = cmd.execute(db)
        total_updated += result.updated
    
    return BulkTagResponse(
        success=True,
        created=total_inserted,
        updated=total_updated,
        message=f"{len(tags)} Tag(s) applied to {len(request.emails)} contacts"
    )
