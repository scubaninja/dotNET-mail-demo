package models

import "testing"

func TestNewContact_DefaultValues(t *testing.T) {
	c := NewContact("Jane Doe", "jane@example.com")

	if c.Name != "Jane Doe" {
		t.Errorf("expected name 'Jane Doe', got %q", c.Name)
	}
	if c.Email != "jane@example.com" {
		t.Errorf("expected email 'jane@example.com', got %q", c.Email)
	}
	if !c.Subscribed {
		t.Error("expected new contact to be subscribed by default")
	}
	if c.ID != 0 {
		t.Errorf("expected ID to be zero for new contact, got %d", c.ID)
	}
}

func TestNewContact_FieldAssignment(t *testing.T) {
	tests := []struct {
		name  string
		email string
	}{
		{"Alice", "alice@example.com"},
		{"Bob Smith", "bob@company.org"},
		{"", "empty@test.com"},
	}

	for _, tt := range tests {
		t.Run(tt.email, func(t *testing.T) {
			c := NewContact(tt.name, tt.email)
			if c.Name != tt.name {
				t.Errorf("expected name %q, got %q", tt.name, c.Name)
			}
			if c.Email != tt.email {
				t.Errorf("expected email %q, got %q", tt.email, c.Email)
			}
		})
	}
}
