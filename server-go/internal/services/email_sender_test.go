package services

import (
	"tailwind-mail/internal/models"
	"testing"
)

func TestInMemoryEmailSender_Send(t *testing.T) {
	sender := NewInMemoryEmailSender()
	msg := &models.Message{
		Status:   "pending",
		SendTo:   "user@example.com",
		SendFrom: "noreply@tailwind.dev",
		Subject:  "Test",
		HTML:     "<p>Hello</p>",
	}

	err := sender.Send(msg)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	if msg.Status != "sent" {
		t.Errorf("expected status 'sent', got %q", msg.Status)
	}
	if len(sender.Sent) != 1 {
		t.Errorf("expected 1 sent message, got %d", len(sender.Sent))
	}
}

func TestInMemoryEmailSender_SendBulk(t *testing.T) {
	sender := NewInMemoryEmailSender()
	msgs := []*models.Message{
		{Status: "pending", SendTo: "a@b.com", SendFrom: "x@y.com", Subject: "1", HTML: "<p>1</p>"},
		{Status: "pending", SendTo: "c@d.com", SendFrom: "x@y.com", Subject: "2", HTML: "<p>2</p>"},
		{Status: "pending", SendTo: "e@f.com", SendFrom: "x@y.com", Subject: "3", HTML: "<p>3</p>"},
	}

	count, err := sender.SendBulk(msgs)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	if count != 3 {
		t.Errorf("expected 3 sent, got %d", count)
	}
	if len(sender.Sent) != 3 {
		t.Errorf("expected 3 in Sent slice, got %d", len(sender.Sent))
	}
	for i, msg := range msgs {
		if msg.Status != "sent" {
			t.Errorf("message %d: expected status 'sent', got %q", i, msg.Status)
		}
	}
}

func TestInMemoryEmailSender_EmptyBulk(t *testing.T) {
	sender := NewInMemoryEmailSender()
	count, err := sender.SendBulk([]*models.Message{})
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	if count != 0 {
		t.Errorf("expected 0 sent, got %d", count)
	}
}
