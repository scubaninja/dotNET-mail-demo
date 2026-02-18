package models

import "time"

// Contact represents a subscriber in the mail.contacts table.
// Mirrors C# Contact class from Models/Contact.cs.
type Contact struct {
	ID         int       `db:"id" json:"id,omitempty"`
	Email      string    `db:"email" json:"email"`
	Name       string    `db:"name" json:"name"`
	Key        string    `db:"key" json:"key"`
	Subscribed bool      `db:"subscribed" json:"subscribed"`
	CreatedAt  time.Time `db:"created_at" json:"created_at"`
	UpdatedAt  time.Time `db:"updated_at" json:"updated_at"`
}

// SignUpRequest mirrors the C# SignUpRequest class.
type SignUpRequest struct {
	Name  string `json:"name"`
	Email string `json:"email"`
}

// NewContact creates a new Contact with default values.
func NewContact(name, email string) Contact {
	return Contact{
		Name:       name,
		Email:      email,
		Subscribed: true,
	}
}
