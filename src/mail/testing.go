package mail

// MockEmailSender provides a mock email sender for testing
type MockEmailSender struct {
	sendFunc func(email Email) error
}

// NewMockEmailSender creates a new mock sender
func NewMockEmailSender(sendFunc func(email Email) error) *MockEmailSender {
	return &MockEmailSender{
		sendFunc: sendFunc,
	}
}

// Send implements EmailSender.Send for testing
func (m *MockEmailSender) Send(email Email) error {
	if m.sendFunc != nil {
		return m.sendFunc(email)
	}
	return nil
}

// TestEmail creates a test email for use in tests
func TestEmail() Email {
	return Email{
		To:      "test@example.com",
		Subject: "Test Subject",
		Body:    "Test Body",
	}
}