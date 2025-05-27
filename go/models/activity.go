package models

import (
	"time"
)

// Activity represents a tracking event in the system
type Activity struct {
	ID          int       `json:"id" db:"id"`
	ContactID   int       `json:"contact_id" db:"contact_id"`
	Key         string    `json:"key" db:"key"`
	Description string    `json:"description" db:"description"`
	CreatedAt   time.Time `json:"created_at" db:"created_at"`
}

// NewActivity creates a new activity record
func NewActivity(contactID int, key, description string) *Activity {
	return &Activity{
		ContactID:   contactID,
		Key:         key,
		Description: description,
		CreatedAt:   time.Now().UTC(),
	}
}
