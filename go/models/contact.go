package models

import (
	"time"
)

// Contact represents a subscriber in the system
type Contact struct {
	ID          int        `json:"id" db:"id"`
	Email       string     `json:"email" db:"email"`
	Name        string     `json:"name" db:"name"`
	Key         string     `json:"key" db:"key"`
	Subscribed  bool       `json:"subscribed" db:"subscribed"`
	CreatedAt   time.Time  `json:"created_at" db:"created_at"`
	UpdatedAt   *time.Time `json:"updated_at" db:"updated_at"`
}

// NewContact creates a new contact with default values
func NewContact(email, name string) *Contact {
	return &Contact{
		Email:      email,
		Name:       name,
		Key:        generateUUID(),
		Subscribed: false,
		CreatedAt:  time.Now().UTC(),
	}
}
