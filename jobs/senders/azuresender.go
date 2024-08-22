package senders

import (
	"log/slog"
	"os"
)

// AzureSender is a sender that sends email messages via
// Azure Communications Service
type AzureSender struct {
	acs    *AzureContainerServices
	logger *slog.Logger
}

func NewAzureSender() (*AzureSender, error) {
	t := AzureSender{}
	s, err := NewAzureContainerServicesFromEnv()
	if err != nil {
		return nil, err
	}
	t.acs = s
	t.logger = slog.New(slog.NewJSONHandler(os.Stdout, nil))
	return &t, nil
}

func (t AzureSender) Send(message map[string]interface{}) error {
	err := t.acs.SendOne("aaron.w@on365.org")
	if err != nil {
		return err
	}
	t.logger.Info("AzureSender message sent", "id", message["id"])
	return nil
}
