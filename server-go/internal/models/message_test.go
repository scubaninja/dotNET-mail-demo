package models

import "testing"

func TestMessage_ReadyToSend_AllFieldsPresent(t *testing.T) {
	msg := Message{
		Status:   "pending",
		SendTo:   "user@example.com",
		SendFrom: "noreply@tailwind.dev",
		Subject:  "Test Subject",
		HTML:     "<p>Hello World</p>",
	}
	if !msg.ReadyToSend() {
		t.Error("expected message with all fields to be ready to send")
	}
}

func TestMessage_ReadyToSend_StatusNotPending(t *testing.T) {
	msg := Message{
		Status:   "sent",
		SendTo:   "user@example.com",
		SendFrom: "noreply@tailwind.dev",
		Subject:  "Test",
		HTML:     "<p>Hello</p>",
	}
	if msg.ReadyToSend() {
		t.Error("expected sent message to not be ready to send")
	}
}

func TestMessage_ReadyToSend_MissingFields(t *testing.T) {
	tests := []struct {
		name string
		msg  Message
	}{
		{
			"missing SendTo",
			Message{Status: "pending", SendFrom: "a@b.com", Subject: "s", HTML: "<p>h</p>"},
		},
		{
			"missing SendFrom",
			Message{Status: "pending", SendTo: "a@b.com", Subject: "s", HTML: "<p>h</p>"},
		},
		{
			"missing Subject",
			Message{Status: "pending", SendTo: "a@b.com", SendFrom: "b@c.com", HTML: "<p>h</p>"},
		},
		{
			"missing HTML",
			Message{Status: "pending", SendTo: "a@b.com", SendFrom: "b@c.com", Subject: "s"},
		},
		{
			"empty status",
			Message{SendTo: "a@b.com", SendFrom: "b@c.com", Subject: "s", HTML: "<p>h</p>"},
		},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if tt.msg.ReadyToSend() {
				t.Errorf("expected message with %s to not be ready to send", tt.name)
			}
		})
	}
}

func TestMessage_MarkSent(t *testing.T) {
	msg := Message{Status: "pending"}
	msg.MarkSent()

	if msg.Status != "sent" {
		t.Errorf("expected status 'sent', got %q", msg.Status)
	}
	if msg.SentAt.IsZero() {
		t.Error("expected SentAt to be set after MarkSent()")
	}
}

func TestNewMessage_Defaults(t *testing.T) {
	msg := NewMessage("test-slug", "user@example.com", "Subject", "<p>Body</p>")

	if msg.Source != "broadcast" {
		t.Errorf("expected source 'broadcast', got %q", msg.Source)
	}
	if msg.Status != "pending" {
		t.Errorf("expected status 'pending', got %q", msg.Status)
	}
	if msg.SendFrom != "noreply@tailwind.dev" {
		t.Errorf("expected default SendFrom, got %q", msg.SendFrom)
	}
	if msg.Slug != "test-slug" {
		t.Errorf("expected slug 'test-slug', got %q", msg.Slug)
	}
	if msg.SendAt.IsZero() {
		t.Error("expected SendAt to be set")
	}
}
