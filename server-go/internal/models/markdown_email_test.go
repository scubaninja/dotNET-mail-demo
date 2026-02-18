package models

import (
	"strings"
	"testing"
)

func TestMarkdownEmailFromString_ValidFrontmatter(t *testing.T) {
	md := `---
Subject: Welcome Email
Summary: A warm welcome to new subscribers
Slug: welcome-email
SendToTag: new-users
---

# Welcome!

Thank you for signing up.
`
	email, err := MarkdownEmailFromString(md)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}

	if email.Data == nil {
		t.Fatal("expected Data to be parsed")
	}
	if email.Data.Subject != "Welcome Email" {
		t.Errorf("expected subject 'Welcome Email', got %q", email.Data.Subject)
	}
	if email.Data.Summary != "A warm welcome to new subscribers" {
		t.Errorf("expected summary, got %q", email.Data.Summary)
	}
	if email.Data.Slug != "welcome-email" {
		t.Errorf("expected slug 'welcome-email', got %q", email.Data.Slug)
	}
	if email.Data.SendToTag != "new-users" {
		t.Errorf("expected SendToTag 'new-users', got %q", email.Data.SendToTag)
	}
}

func TestMarkdownEmailFromString_GeneratesSlugFromSubject(t *testing.T) {
	md := `---
Subject: My Test Email
Summary: A test summary
---

# Hello

Some content here.
`
	email, err := MarkdownEmailFromString(md)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	if email.Data.Slug != "my-test-email" {
		t.Errorf("expected auto-generated slug 'my-test-email', got %q", email.Data.Slug)
	}
}

func TestMarkdownEmailFromString_DefaultSendToTag(t *testing.T) {
	md := `---
Subject: Newsletter
Summary: Weekly newsletter
---

Content here.
`
	email, err := MarkdownEmailFromString(md)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	if email.Data.SendToTag != "*" {
		t.Errorf("expected default SendToTag '*', got %q", email.Data.SendToTag)
	}
}

func TestMarkdownEmailFromString_RendersHTML(t *testing.T) {
	md := `---
Subject: Test
Summary: Test summary
---

# Hello World

This is a **bold** statement.
`
	email, err := MarkdownEmailFromString(md)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	if !strings.Contains(email.HTML, "<h1>Hello World</h1>") {
		t.Errorf("expected HTML to contain h1 tag, got %q", email.HTML)
	}
	if !strings.Contains(email.HTML, "<strong>bold</strong>") {
		t.Errorf("expected HTML to contain strong tag, got %q", email.HTML)
	}
}

func TestMarkdownEmailFromString_EmptyMarkdown(t *testing.T) {
	_, err := MarkdownEmailFromString("")
	if err == nil {
		t.Error("expected error for empty markdown")
	}
}

func TestMarkdownEmail_IsValid_WithRequiredFields(t *testing.T) {
	md := `---
Subject: Hello
Summary: World
---

Content.
`
	email, err := MarkdownEmailFromString(md)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	if !email.IsValid() {
		t.Error("expected email with Subject and Summary to be valid")
	}
}

func TestMarkdownEmail_IsValid_MissingSubject(t *testing.T) {
	md := `---
Summary: Only summary
---

Content.
`
	email, err := MarkdownEmailFromString(md)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	if email.IsValid() {
		t.Error("expected email without Subject to be invalid")
	}
}

func TestMarkdownEmail_IsValid_MissingSummary(t *testing.T) {
	md := `---
Subject: Only subject
---

Content.
`
	email, err := MarkdownEmailFromString(md)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	if email.IsValid() {
		t.Error("expected email without Summary to be invalid")
	}
}

func TestMarkdownEmail_IsValid_NilData(t *testing.T) {
	email := &MarkdownEmail{}
	if email.IsValid() {
		t.Error("expected email with nil Data to be invalid")
	}
}

func TestMarkdownEmailFromString_NoFrontmatter(t *testing.T) {
	md := `# Just Markdown

No frontmatter here.
`
	email, err := MarkdownEmailFromString(md)
	if err != nil {
		t.Fatalf("unexpected error: %v", err)
	}
	// Without frontmatter delimiters, Data should be nil
	if email.Data != nil {
		t.Error("expected nil Data when no frontmatter present")
	}
	if !strings.Contains(email.HTML, "Just Markdown") {
		t.Error("expected HTML to still be rendered")
	}
}
