package models

import (
	"time"
)

// Broadcast represents an email broadcast campaign
type Broadcast struct {
	ID        int       `json:"id" db:"id"`
	EmailID   int       `json:"email_id" db:"email_id"`
	Status    string    `json:"status" db:"status"`
	Name      string    `json:"name" db:"name"`
	Slug      string    `json:"slug" db:"slug"`
	ReplyTo   string    `json:"reply_to" db:"reply_to"`
	SendToTag string    `json:"send_to_tag" db:"send_to_tag"`
	CreatedAt time.Time `json:"created_at" db:"created_at"`
}

// NewBroadcast creates a new broadcast
func NewBroadcast(name, slug string) *Broadcast {
	return &Broadcast{
		Status:    "pending",
		Name:      name,
		Slug:      slug,
		SendToTag: "*",
		CreatedAt: time.Now().UTC(),
	}
}
