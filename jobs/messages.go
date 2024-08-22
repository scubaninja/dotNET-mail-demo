//go:build mage
// +build mage

package main

import (
	"encoding/json"
	"fmt"
	"math/rand"
	"os"

	"jobs/queuers"
	"jobs/senders"

	"github.com/magefile/mage/mg"
)

type Messages mg.Namespace

// Queue creates messages in the queue that are ready to
// send using the Queuer and Sender defined
// by the MESSAGES_TYPE environment variable with options
// "test" (default), "smtp", or "azure"
func (Messages) Queue() error {
	queuerSenderType := os.Getenv("MESSAGES_TYPE")

	var queuer Queuer
	var err error
	switch queuerSenderType {
	case "servicebus-test":
		queuer, err = queuers.NewServiceBusQueuerFromEnv()
		if err != nil {
			return err
		}
	default:
		queuer = queuers.NewTestQueuer()
	}

	// create and enqueue messages continuously
	for i := 0; i < 1000; i++ {
		map1 := map[string]interface{}{}
		map1["id"] = rand.Intn(1000)
		err := queuer.Send(map1)
		if err != nil {
			return err
		}
	}
	return nil
}

// Send iterates over messages that have been inserted
// and sends them using the Queuer and Sender defined
// by the MESSAGES_TYPE environment variable with options
// "test" (default), "smtp", or "azure"
func (Messages) Send() error {
	queuerSenderType := os.Getenv("MESSAGES_TYPE")

	var queuer Queuer
	var sender Sender
	var err error
	switch queuerSenderType {
	case "smtp":
		queuer = queuers.NewTestQueuer()
		sender, err = senders.NewSMTPSenderFromEnv()
		if err != nil {
			return err
		}
	case "azure":
		queuer = queuers.NewTestQueuer()
		sender, err = senders.NewAzureSender()
		if err != nil {
			return err
		}
	case "servicebus-test":
		queuer, err = queuers.NewServiceBusQueuerFromEnv()
		if err != nil {
			return err
		}
		sender = senders.NewTestSender()
	default:
		queuer = queuers.NewTestQueuer()
		sender = senders.NewTestSender()
	}

	for {
		map1, err := queuer.Receive()
		if err != nil {
			return err
		}
		fmt.Printf("Sending: %d\n", map1["id"])
		err = sender.Send(map1)
		if err != nil {
			return err
		}
	}
	//return nil
}

func NewMessage(to, from, subject, body string) (*Message, error) {
	m := Message{
		To:      to,
		From:    from,
		Subject: subject,
		Body:    body,
	}
	return &m, nil
}

type Message struct {
	ID      string
	To      string
	From    string
	Subject string
	Body    string
}

func (m Message) JSON() ([]byte, error) {
	return json.MarshalIndent(m, "", "    ")
}

func (m Message) ToMap() (map[string]interface{}, error) {
	map1 := map[string]interface{}{
		"to":      m.To,
		"from":    m.From,
		"subject": m.Subject,
		"body":    m.Body,
	}
	return map1, nil
}

// Queuer is an interface for sending messages to and from a queue
type Queuer interface {
	Send(map1 map[string]interface{}) error
	Receive() (map[string]interface{}, error)
}

// Sender is an interface for sending messages to a mail service
type Sender interface {
	Send(message map[string]interface{}) error
}
