package models

import (
	"bytes"
	"fmt"
	"strings"

	"github.com/yuin/goldmark"
	"gopkg.in/yaml.v3"
)

// MarkdownEmailData holds the parsed YAML frontmatter data.
type MarkdownEmailData struct {
	Subject   string `yaml:"Subject" json:"subject"`
	Summary   string `yaml:"Summary" json:"summary"`
	Slug      string `yaml:"Slug" json:"slug"`
	SendToTag string `yaml:"SendToTag" json:"send_to_tag"`
}

// MarkdownEmail represents an email defined in Markdown with YAML frontmatter.
// Mirrors C# MarkdownEmail class from Models/MarkdownEmail.cs.
type MarkdownEmail struct {
	Markdown string             `json:"markdown,omitempty"`
	HTML     string             `json:"html,omitempty"`
	Data     *MarkdownEmailData `json:"data,omitempty"`
}

// MarkdownEmailFromString parses a markdown string containing YAML frontmatter.
// Mirrors C# MarkdownEmail.FromString() static method.
func MarkdownEmailFromString(markdown string) (*MarkdownEmail, error) {
	email := &MarkdownEmail{Markdown: markdown}
	if err := email.render(); err != nil {
		return nil, err
	}
	return email, nil
}

// IsValid checks if the email has the required frontmatter fields.
// Mirrors C# MarkdownEmail.IsValid() method.
func (e *MarkdownEmail) IsValid() bool {
	return e.Data != nil && e.Data.Subject != "" && e.Data.Summary != ""
}

func (e *MarkdownEmail) render() error {
	if e.Markdown == "" {
		return fmt.Errorf("markdown is empty; set it first")
	}

	// Parse YAML frontmatter (between --- delimiters)
	parts := strings.SplitN(e.Markdown, "---", 3)
	if len(parts) >= 3 {
		e.Data = &MarkdownEmailData{}
		if err := yaml.Unmarshal([]byte(parts[1]), e.Data); err != nil {
			return fmt.Errorf("failed to parse frontmatter: %w", err)
		}
	}

	// Generate slug from subject if not provided
	if e.Data != nil {
		if e.Data.Slug == "" && e.Data.Subject != "" {
			e.Data.Slug = strings.ToLower(strings.ReplaceAll(e.Data.Subject, " ", "-"))
		}
		if e.Data.SendToTag == "" {
			e.Data.SendToTag = "*"
		}
	}

	// Render Markdown to HTML
	var buf bytes.Buffer
	md := goldmark.New()
	if err := md.Convert([]byte(e.Markdown), &buf); err != nil {
		return fmt.Errorf("failed to render markdown: %w", err)
	}
	e.HTML = buf.String()

	return nil
}
