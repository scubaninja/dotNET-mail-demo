package data

import "testing"

func TestToSnakeCase(t *testing.T) {
	tests := []struct {
		name     string
		input    string
		expected string
	}{
		{"ID special case", "ID", "id"},
		{"single char", "A", "a"},
		{"simple camelCase", "firstName", "first_name"},
		{"PascalCase", "FirstName", "first_name"},
		{"multiple words", "CreatedAt", "created_at"},
		{"already lowercase", "email", "email"},
		{"SendToTag", "SendToTag", "send_to_tag"},
		{"EmailId", "EmailId", "email_id"},
		{"ContactId", "ContactId", "contact_id"},
		{"SendFrom", "SendFrom", "send_from"},
		{"SendTo", "SendTo", "send_to"},
		{"DelayHours", "DelayHours", "delay_hours"},
	}

	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			result := ToSnakeCase(tt.input)
			if result != tt.expected {
				t.Errorf("ToSnakeCase(%q) = %q, want %q", tt.input, result, tt.expected)
			}
		})
	}
}
