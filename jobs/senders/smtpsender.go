package senders

import (
	"errors"
	"fmt"
	"log/slog"
	"net/smtp"
	"os"
)

// SMTPSender is a sender that sends email messages via
// SMTP (Simple Mail Transfer Protocol)
type SMTPSender struct {
	logger   *slog.Logger
	From     string
	Server   string
	Port     string
	Username string
	Password string
}

func NewSMTPSenderFromEnv() (*SMTPSender, error) {
	from := os.Getenv("SMTP_FROM")
	if from == "" {
		return nil, errors.New("SMTP_FROM environment variable not set")
	}
	server := os.Getenv("SMTP_SERVER")
	if server == "" {
		return nil, errors.New("SMTP_SERVER environment variable not set")
	}
	port := os.Getenv("SMTP_PORT")
	if server == "" {
		return nil, errors.New("SMTP_PORT environment variable not set")
	}
	username := os.Getenv("SMTP_USERNAME")
	if username == "" {
		return nil, errors.New("SMTP_USERNAME environment variable not set")
	}
	password := os.Getenv("SMTP_PASSWORD")
	if password == "" {
		return nil, errors.New("SMTP_PASSWORD environment variable not set")
	}
	t := SMTPSender{
		From:     from,
		Server:   server,
		Port:     port,
		Username: username,
		Password: password,
	}
	t.logger = slog.New(slog.NewJSONHandler(os.Stdout, nil))
	return &t, nil
}

func (t SMTPSender) Send(message map[string]interface{}) error {
	a := smtp.PlainAuth("", t.Username, t.Password, t.Server)
	to := "aaron.w@on365.org"
	//to := "ivory.grimes@ethereal.email"
	subject := "hello"
	body := "world"
	msg := []byte(
		"From: " + t.From + "\r\n" +
			"To: " + to + "\r\n" +
			"Subject: " + subject + "\r\n" +
			"\r\n" +
			body + "\r\n")
	fmt.Printf("Address: %s\n", t.Server)

	addr := t.Server + ":" + t.Port
	err := smtp.SendMail(addr, a, t.From, []string{to}, msg)
	if err != nil {
		return err
	}
	t.logger.Info("SmtpSender message sent", "id", message["id"])
	return nil
}
