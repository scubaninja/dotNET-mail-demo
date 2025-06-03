package mail

import (
	"errors"
	"strings"
)

// Email represents an email message
type Email struct {
	To      string
	Subject string
	Body    string
}

// EmailConfig holds SMTP configuration
type EmailConfig struct {
	SMTPHost string
	SMTPPort int
	Username string
	Password string
}

// EmailSender interface for sending emails
type EmailSender interface {
	Send(email Email) error
}

// EmailService handles email operations
type EmailService struct {
	sender EmailSender
}

// NewEmailService creates a new email service
func NewEmailService(sender EmailSender) *EmailService {
	return &EmailService{
		sender: sender,
	}
}

// SendEmail sends an email using the configured sender
func (s *EmailService) SendEmail(email Email) error {
	if err := ValidateEmail(email); err != nil {
		return err
	}
	return s.sender.Send(email)
}

// ValidateEmail validates email fields
func ValidateEmail(email Email) error {
	if strings.TrimSpace(email.To) == "" {
		return errors.New("recipient email is required")
	}
	if strings.TrimSpace(email.Subject) == "" {
		return errors.New("subject is required")
	}
	if strings.TrimSpace(email.Body) == "" {
		return errors.New("body is required")
	}
	return nil
}

// SMTPSender implements EmailSender for SMTP
type SMTPSender struct {
	config EmailConfig
}

// NewSMTPSender creates a new SMTP sender
func NewSMTPSender(config EmailConfig) *SMTPSender {
	return &SMTPSender{
		config: config,
	}
}

// Send implements EmailSender.Send for SMTP
func (s *SMTPSender) Send(email Email) error {
	// TODO: Implement actual SMTP sending logic
	return nil
}