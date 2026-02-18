package commands

import "testing"

func TestCommandResult_DefaultValues(t *testing.T) {
	result := CommandResult{}
	if result.Inserted != 0 {
		t.Errorf("expected Inserted=0, got %d", result.Inserted)
	}
	if result.Updated != 0 {
		t.Errorf("expected Updated=0, got %d", result.Updated)
	}
	if result.Deleted != 0 {
		t.Errorf("expected Deleted=0, got %d", result.Deleted)
	}
	if result.Data != nil {
		t.Error("expected nil Data by default")
	}
}

func TestCommandResult_WithData(t *testing.T) {
	result := CommandResult{
		Inserted: 5,
		Updated:  2,
		Data: map[string]interface{}{
			"BroadcastId": 42,
			"EmailId":     7,
		},
	}
	if result.Inserted != 5 {
		t.Errorf("expected Inserted=5, got %d", result.Inserted)
	}
	if result.Updated != 2 {
		t.Errorf("expected Updated=2, got %d", result.Updated)
	}
	data, ok := result.Data.(map[string]interface{})
	if !ok {
		t.Fatal("expected Data to be map[string]interface{}")
	}
	if data["BroadcastId"] != 42 {
		t.Errorf("expected BroadcastId=42, got %v", data["BroadcastId"])
	}
}

func TestLinkClicked(t *testing.T) {
	tests := []struct {
		key      string
		expected string
	}{
		{"abc123", "Link clicked: abc123"},
		{"xyz-789", "Link clicked: xyz-789"},
		{"", "Link clicked: "},
	}
	for _, tt := range tests {
		t.Run(tt.key, func(t *testing.T) {
			result := LinkClicked(tt.key)
			if result != tt.expected {
				t.Errorf("LinkClicked(%q) = %q, want %q", tt.key, result, tt.expected)
			}
		})
	}
}
