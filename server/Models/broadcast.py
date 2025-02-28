from datetime import datetime
from typing import Optional
import sqlite3

class Broadcast:
    def __init__(self, id: Optional[int] = None, email_id: Optional[int] = None, status: str = "pending",
                 name: Optional[str] = None, slug: Optional[str] = None, reply_to: Optional[str] = None,
                 send_to_tag: str = "*", created_at: datetime = datetime.utcnow()):
        self.id = id
        self.email_id = email_id
        self.status = status
        self.name = name
        self.slug = slug
        self.reply_to = reply_to
        self.send_to_tag = send_to_tag
        self.created_at = created_at

    @staticmethod
    def from_markdown_email(doc):
        if doc is None or doc.data is None:
            raise ValueError("MarkdownEmail document cannot be null")

        return Broadcast(
            name=doc.data.subject,
            slug=doc.data.slug,
            send_to_tag=doc.data.send_to_tag
        )

    @staticmethod
    def from_markdown(markdown: str):
        if not markdown.strip():
            raise ValueError("Markdown content cannot be null or empty")

        doc = MarkdownEmail.from_string(markdown)
        return Broadcast.from_markdown_email(doc)

    def contact_count(self, conn: sqlite3.Connection) -> int:
        if conn is None:
            raise ValueError("Database connection cannot be null")

        contacts = 0
        try:
            if self.send_to_tag == "*":
                contacts = conn.execute("SELECT COUNT(1) FROM contacts WHERE subscribed = 1").fetchone()[0]
            else:
                sql = """
                    SELECT COUNT(1) AS count FROM contacts 
                    INNER JOIN tagged ON tagged.contact_id = contacts.id
                    INNER JOIN tags ON tags.id = tagged.tag_id
                    WHERE subscribed = 1
                    AND tags.slug = ?
                """
                contacts = conn.execute(sql, (self.send_to_tag,)).fetchone()[0]
        except Exception as ex:
            # Log the exception (logging mechanism not shown here)
            raise RuntimeError("An error occurred while counting contacts") from ex

        return contacts
