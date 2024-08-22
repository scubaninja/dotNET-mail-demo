package queuers

import (
	"context"
	"encoding/json"
	"errors"
	"os"

	"log/slog"

	"github.com/Azure/azure-sdk-for-go/sdk/azidentity"
	"github.com/Azure/azure-sdk-for-go/sdk/messaging/azservicebus"
)

// ServiceBusQueuer receiver is a local queue
type ServiceBusQueuer struct {
	logger       *slog.Logger
	client       azservicebus.Client
	QueueOrTopic string
	ctx          context.Context
}

func NewServiceBusQueuerFromEnv() (*ServiceBusQueuer, error) {
	t := ServiceBusQueuer{}
	client, err := getServiceBusClient()
	if err != nil {
		return nil, err
	}
	t.client = *client

	queueOrTopic := os.Getenv("AZURE_SERVICEBUS_QUEUE_NAME")
	if queueOrTopic == "" {
		return nil, errors.New("AZURE_SERVICEBUS_QUEUE_NAME environment variable not found")
	}
	t.QueueOrTopic = queueOrTopic

	t.ctx = context.Background()

	t.logger = slog.New(slog.NewJSONHandler(os.Stdout, nil))
	return &t, nil
}

func (t ServiceBusQueuer) Close() error {
	return t.client.Close(t.ctx)
}

func (t ServiceBusQueuer) Send(map1 map[string]interface{}) error {
	sender, err := t.client.NewSender(t.QueueOrTopic, nil)
	if err != nil {
		return nil
	}
	defer sender.Close(t.ctx)

	b, err := json.Marshal(map1)
	if err != nil {
		return err
	}
	// ensure size is not larger than 265kb
	// per docs for standard tier

	sbMessage := &azservicebus.Message{
		Body: b,
	}
	err = sender.SendMessage(t.ctx, sbMessage, nil)
	if err != nil {
		return err
	}

	t.logger.Info("ServiceBusQueuer message sent", "id", map1["id"])
	return nil
}

func (t ServiceBusQueuer) Receive() (map[string]interface{}, error) {
	receiver, err := t.client.NewReceiverForQueue(t.QueueOrTopic, nil)
	if err != nil {
		return nil, err
	}
	defer receiver.Close(t.ctx)

	count := 1
	for {
		t.logger.Info("ServiceBusQueuer receiving messages...")
		messages, err := receiver.ReceiveMessages(t.ctx, count, nil)
		if err != nil {
			return nil, err
		}

		for _, message := range messages {
			err = receiver.CompleteMessage(t.ctx, message, nil)
			if err != nil {
				return nil, err
			}
			map1 := map[string]interface{}{}
			err = json.Unmarshal(message.Body, &map1)
			if err != nil {
				return nil, err
			}
			t.logger.Info("ServiceBusQueuer message received", "id", map1["id"])
			return map1, nil
		}
	}
}

func getServiceBusClient() (*azservicebus.Client, error) {
	// todo: merge with getServiceBusClientDefault and use
	// AZURE_SERVICEBUS_CONNECTION_STRING if exists, otherwise
	// use azidentity.NewDefaultAzureCredential by default

	connectionString, ok := os.LookupEnv("AZURE_SERVICEBUS_CONNECTION_STRING") //ex: myservicebus.servicebus.windows.net
	if !ok {
		return nil, errors.New("AZURE_SERVICEBUS_CONNECTION_STRING environment variable not found")
	}

	client, err := azservicebus.NewClientFromConnectionString(connectionString, nil)
	if err != nil {
		return nil, err
	}
	return client, nil
}

func getServiceBusClientDefault() (*azservicebus.Client, error) {
	namespace, ok := os.LookupEnv("AZURE_SERVICEBUS_HOSTNAME") //ex: myservicebus.servicebus.windows.net
	if !ok {
		return nil, errors.New("AZURE_SERVICEBUS_HOSTNAME environment variable not found")
	}

	cred, err := azidentity.NewDefaultAzureCredential(nil)
	if err != nil {
		return nil, err
	}

	client, err := azservicebus.NewClient(namespace, cred, nil)
	if err != nil {
		return nil, err
	}
	return client, nil
}
