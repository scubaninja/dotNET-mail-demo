package models

import "time"

// Message represents an email message in the mail.messages table.
// Mirrors C# Message class from Models/Message.cs.
type Message struct {
	ID        int       `db:"id" json:"id,omitempty"`
	Source    string    `db:"source" json:"source"`
	Slug      string    `db:"slug" json:"slug"`
	Status    string    `db:"status" json:"status"`
	SendTo    string    `db:"send_to" json:"send_to"`
	SendFrom  string    `db:"send_from" json:"send_from"`
	Subject   string    `db:"subject" json:"subject"`
	HTML      string    `db:"html" json:"html"`
	SendAt    time.Time `db:"send_at" json:"send_at"`
	SentAt    time.Time `db:"sent_at" json:"sent_at,omitempty"`
	CreatedAt time.Time `db:"created_at" json:"created_at"`
}

// NewMessage creates a new Message with the given parameters.
func NewMessage(slug, sendTo, subject, html string) Message {
	return Message{
		Source:   "broadcast",
		Slug:     slug,
		Status:   "pending",
		SendTo:   sendTo,
		SendFrom: "noreply@tailwind.dev",
		Subject:  subject,
		HTML:     html,
		SendAt:   time.Now().UTC(),
	}
}

// MarkSent sets the message status to "sent" and records the sent time.
// Mirrors C# Message.Sent() method.
func (m *Message) MarkSent() {
	m.Status = "sent"
	m.SentAt = time.Now().UTC()
}

// ReadyToSend checks if the message has all required fields for sending.
// Mirrors C# Message.ReadyToSend() method.
func (m *Message) ReadyToSend() bool {
	return m.Status == "pending" &&
		m.SendTo != "" &&
		m.SendFrom != "" &&
		m.HTML != "" &&
		m.Subject != ""
}
