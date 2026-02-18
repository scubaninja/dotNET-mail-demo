package services

import "tailwind-mail/internal/models"

// EmailSender defines the interface for sending emails.
// Mirrors C# IEmailSender interface from Services/Outbox.cs.
type EmailSender interface {
	Send(msg *models.Message) error
	SendBulk(msgs []*models.Message) (int, error)
}

// InMemoryEmailSender is an EmailSender that records sent messages for testing.
// Mirrors C# InMemoryEmailSender from Services/Outbox.cs.
type InMemoryEmailSender struct {
	Sent []*models.Message
}

// NewInMemoryEmailSender creates a new in-memory sender for testing.
func NewInMemoryEmailSender() *InMemoryEmailSender {
	return &InMemoryEmailSender{
		Sent: make([]*models.Message, 0),
	}
}

func (s *InMemoryEmailSender) Send(msg *models.Message) error {
	msg.MarkSent()
	s.Sent = append(s.Sent, msg)
	return nil
}

func (s *InMemoryEmailSender) SendBulk(msgs []*models.Message) (int, error) {
	for _, msg := range msgs {
		msg.MarkSent()
		s.Sent = append(s.Sent, msg)
	}
	return len(msgs), nil
}
