package senders

import (
	"log/slog"
	"os"
)

// TestSender is a local sender sending messages
// but instead of sending via a service, or SMTP server,
// it outputs them to the standard output
type TestSender struct {
	logger *slog.Logger
}

func NewTestSender() TestSender {
	t := TestSender{}
	t.logger = slog.New(slog.NewJSONHandler(os.Stdout, nil))
	return t
}

func (t TestSender) Send(message map[string]interface{}) error {
	t.logger.Info("TestSender message sent", "id", message["id"])
	return nil
}
