package models

import (
	"time"
)

// Email represents an email template
type Email struct {
	ID        int       `json:"id" db:"id"`
	Slug      string    `json:"slug" db:"slug"`
	Subject   string    `json:"subject" db:"subject"`
	Preview   string    `json:"preview" db:"preview"`
	HTML      string    `json:"html" db:"html"`
	CreatedAt time.Time `json:"created_at" db:"created_at"`
}

// Message represents an email to be sent
type Message struct {
	ID        int       `json:"id" db:"id"`
	Source    string    `json:"source" db:"source"`
	Slug      string    `json:"slug" db:"slug"`
	Status    string    `json:"status" db:"status"`
	SendTo    string    `json:"send_to" db:"send_to"`
	SendFrom  string    `json:"send_from" db:"send_from"`
	Subject   string    `json:"subject" db:"subject"`
	HTML      string    `json:"html" db:"html"`
	SendAt    time.Time `json:"send_at" db:"send_at"`
	SentAt    time.Time `json:"sent_at" db:"sent_at"`
	CreatedAt time.Time `json:"created_at" db:"created_at"`
}

// NewMessage creates a new message
func NewMessage(slug, sendTo, subject, html string) *Message {
	return &Message{
		Source:    "broadcast",
		Slug:      slug,
		Status:    "pending",
		SendTo:    sendTo,
		SendFrom:  "noreply@example.com",
		Subject:   subject,
		HTML:      html,
		SendAt:    time.Now().UTC(),
		CreatedAt: time.Now().UTC(),
	}
}

// IsReadyToSend checks if the message is ready to be sent
func (m *Message) IsReadyToSend() bool {
	return m.Status == "pending" &&
		m.SendTo != "" &&
		m.SendFrom != "" &&
		m.HTML != "" &&
		m.Subject != ""
}

// MarkAsSent marks the message as sent
func (m *Message) MarkAsSent() {
	m.Status = "sent"
	m.SentAt = time.Now().UTC()
}
