package models

import (
	"strings"
	"testing"
)

func TestBroadcastFromMarkdownEmail_WithValidDoc(t *testing.T) {
	doc := &MarkdownEmail{
		Data: &MarkdownEmailData{
			Subject:   "Test Broadcast",
			Slug:      "test-broadcast",
			SendToTag: "vip",
		},
	}
	b := BroadcastFromMarkdownEmail(doc)

	if b.Name != "Test Broadcast" {
		t.Errorf("expected name 'Test Broadcast', got %q", b.Name)
	}
	if b.Slug != "test-broadcast" {
		t.Errorf("expected slug 'test-broadcast', got %q", b.Slug)
	}
	if b.SendToTag != "vip" {
		t.Errorf("expected SendToTag 'vip', got %q", b.SendToTag)
	}
	if b.Status != "pending" {
		t.Errorf("expected status 'pending', got %q", b.Status)
	}
}

func TestBroadcastFromMarkdownEmail_NilDoc(t *testing.T) {
	b := BroadcastFromMarkdownEmail(nil)
	if b.SendToTag != "*" {
		t.Errorf("expected default SendToTag '*', got %q", b.SendToTag)
	}
}

func TestBroadcastFromMarkdownEmail_NilData(t *testing.T) {
	doc := &MarkdownEmail{}
	b := BroadcastFromMarkdownEmail(doc)
	if b.SendToTag != "*" {
		t.Errorf("expected default SendToTag '*', got %q", b.SendToTag)
	}
}

func TestNewTag_SlugGeneration(t *testing.T) {
	tests := []struct {
		name         string
		expectedSlug string
	}{
		{"VIP Customers", "vip-customers"},
		{"newsletter", "newsletter"},
		{"Early Adopters", "early-adopters"},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			tag := NewTag(tt.name)
			if tag.Slug != tt.expectedSlug {
				t.Errorf("NewTag(%q).Slug = %q, want %q", tt.name, tag.Slug, tt.expectedSlug)
			}
			if tag.Name != tt.name {
				t.Errorf("NewTag(%q).Name = %q, want %q", tt.name, tag.Name, tt.name)
			}
		})
	}
}

func TestNewEmailFromMarkdown_ValidDoc(t *testing.T) {
	doc := &MarkdownEmail{
		HTML: "<p>Hello World</p>",
		Data: &MarkdownEmailData{
			Subject: "Welcome",
			Summary: "Welcome to our newsletter",
			Slug:    "welcome",
		},
	}
	e := NewEmailFromMarkdown(doc)

	if e.Slug != "welcome" {
		t.Errorf("expected slug 'welcome', got %q", e.Slug)
	}
	if e.Subject != "Welcome" {
		t.Errorf("expected subject 'Welcome', got %q", e.Subject)
	}
	if e.Preview != "Welcome to our newsletter" {
		t.Errorf("expected preview text, got %q", e.Preview)
	}
	if !strings.Contains(e.HTML, "Hello World") {
		t.Error("expected HTML to contain 'Hello World'")
	}
}

func TestNewEmailFromMarkdown_NilDoc(t *testing.T) {
	e := NewEmailFromMarkdown(nil)
	if e.Slug != "" {
		t.Errorf("expected empty slug for nil doc, got %q", e.Slug)
	}
}
