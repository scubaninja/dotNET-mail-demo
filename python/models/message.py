from datetime import datetime
from typing import Optional, Dict, Any
from pydantic import BaseModel, Field, EmailStr

class Message(BaseModel):
    """Represents an email message to be sent"""
    send_to: EmailStr
    send_from: EmailStr 
    subject: str
    html: str
    text: Optional[str] = None
    
    class Config:
        arbitrary_types_allowed = True

class MarkdownEmailData(BaseModel):
    """Data extracted from markdown email"""
    subject: str
    summary: str
    slug: str = ""
    send_to_tag: str = "*"
    
    class Config:
        arbitrary_types_allowed = True

class MarkdownEmail:
    """Email constructed from markdown content"""
    data: Optional[MarkdownEmailData] = None
    html: Optional[str] = None
    markdown: Optional[str] = None
    
    @classmethod
    def from_string(cls, markdown: str) -> "MarkdownEmail":
        """Create a MarkdownEmail from a markdown string"""
        if not markdown:
            raise ValueError("Markdown content cannot be empty")
            
        email = cls()
        email.markdown = markdown
        
        # Extract front matter (metadata between --- blocks)
        parts = markdown.split('---')
        if len(parts) >= 3:
            metadata = parts[1].strip()
            content = '---'.join(parts[2:]).strip()
            
            # Parse metadata
            metadata_dict = {}
            for line in metadata.splitlines():
                if ':' in line:
                    key, value = line.split(':', 1)
                    metadata_dict[key.strip().lower()] = value.strip()
            
            # Create data object
            email.data = MarkdownEmailData(
                subject=metadata_dict.get('subject', ''),
                summary=metadata_dict.get('summary', ''),
                slug=metadata_dict.get('slug', ''),
                send_to_tag=metadata_dict.get('sendtotag', '*')
            )
            
            # If slug is not provided, generate from subject
            if not email.data.slug and email.data.subject:
                email.data.slug = email.data.subject.lower().replace(' ', '-')
            
            # Convert markdown to HTML
            import markdown
            email.html = markdown.markdown(content)
        
        return email
    
    def is_valid(self) -> bool:
        """Check if the email has required fields"""
        return (
            self.data is not None and 
            self.html is not None and
            self.data.subject and 
            self.data.summary
        )
    
    def render(self) -> str:
        """Render the markdown to HTML"""
        if not self.markdown:
            raise ValueError("Markdown is null")
            
        import markdown
        return markdown.markdown(self.markdown)
