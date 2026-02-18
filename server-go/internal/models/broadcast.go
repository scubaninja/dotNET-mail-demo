package models

import (
	"strings"
	"time"
)

// Broadcast represents a bulk email campaign in the mail.broadcasts table.
// Mirrors C# Broadcast class from Models/Broadcast.cs.
type Broadcast struct {
	ID          int       `db:"id" json:"id,omitempty"`
	EmailID     int       `db:"email_id" json:"email_id,omitempty"`
	Status      string    `db:"status" json:"status"`
	Name        string    `db:"name" json:"name"`
	Slug        string    `db:"slug" json:"slug"`
	ReplyTo     string    `db:"reply_to" json:"reply_to"`
	SendToTag   string    `db:"send_to_tag" json:"send_to_tag"`
	CreatedAt   time.Time `db:"created_at" json:"created_at"`
	ProcessedAt time.Time `db:"processed_at" json:"processed_at,omitempty"`
}

// BroadcastFromMarkdownEmail creates a Broadcast from a MarkdownEmail.
// Mirrors C# Broadcast.FromMarkdownEmail() static method.
func BroadcastFromMarkdownEmail(doc *MarkdownEmail) Broadcast {
	b := Broadcast{
		Status:    "pending",
		SendToTag: "*",
	}
	if doc != nil && doc.Data != nil {
		b.Name = doc.Data.Subject
		b.Slug = doc.Data.Slug
		b.SendToTag = doc.Data.SendToTag
	}
	return b
}

// BroadcastFromMarkdown creates a Broadcast by first parsing the markdown string.
// Mirrors C# Broadcast.FromMarkdown() static method.
func BroadcastFromMarkdown(markdown string) (Broadcast, error) {
	doc, err := MarkdownEmailFromString(markdown)
	if err != nil {
		return Broadcast{}, err
	}
	return BroadcastFromMarkdownEmail(doc), nil
}

// Tag represents a tag in the mail.tags table.
// Mirrors C# Tag class from Models/Tag.cs.
type Tag struct {
	ID          int       `db:"id" json:"id,omitempty"`
	Slug        string    `db:"slug" json:"slug"`
	Name        string    `db:"name" json:"name"`
	Description string    `db:"description" json:"description,omitempty"`
	CreatedAt   time.Time `db:"created_at" json:"created_at"`
	UpdatedAt   time.Time `db:"updated_at" json:"updated_at"`
}

// NewTag creates a new Tag with auto-generated slug.
// Mirrors C# Tag(string name) constructor.
func NewTag(name string) Tag {
	return Tag{
		Name: name,
		Slug: strings.ToLower(strings.ReplaceAll(name, " ", "-")),
	}
}

// Tagged represents a contact-tag junction in the mail.tagged table.
type Tagged struct {
	ContactID int `db:"contact_id" json:"contact_id"`
	TagID     int `db:"tag_id" json:"tag_id"`
}

// Activity represents an activity log entry in the mail.activity table.
// Mirrors C# Activity class from Models/Activity.cs.
type Activity struct {
	ID          int       `db:"id" json:"id,omitempty"`
	ContactID   int       `db:"contact_id" json:"contact_id"`
	Key         string    `db:"key" json:"key"`
	Description string    `db:"description" json:"description"`
	CreatedAt   time.Time `db:"created_at" json:"created_at"`
}

// Email represents an email template in the mail.emails table.
// Mirrors C# Email class from Models/Email.cs.
type Email struct {
	ID         int       `db:"id" json:"id,omitempty"`
	SequenceID *int      `db:"sequence_id" json:"sequence_id,omitempty"`
	Slug       string    `db:"slug" json:"slug"`
	Subject    string    `db:"subject" json:"subject"`
	Preview    string    `db:"preview" json:"preview"`
	DelayHours int       `db:"delay_hours" json:"delay_hours"`
	HTML       string    `db:"html" json:"html"`
	CreatedAt  time.Time `db:"created_at" json:"created_at"`
	UpdatedAt  time.Time `db:"updated_at" json:"updated_at"`
}

// NewEmailFromMarkdown creates an Email from a MarkdownEmail document.
// Mirrors C# Email(MarkdownEmail doc) constructor.
func NewEmailFromMarkdown(doc *MarkdownEmail) Email {
	e := Email{}
	if doc != nil && doc.Data != nil {
		e.Slug = doc.Data.Slug
		e.Subject = doc.Data.Subject
		e.Preview = doc.Data.Summary
		e.HTML = doc.HTML
	}
	return e
}
