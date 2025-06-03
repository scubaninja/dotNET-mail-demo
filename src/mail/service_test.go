package mail

import (
	"errors"
	"testing"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

// MockEmailSender is a mock implementation of EmailSender interface
type MockEmailSender struct {
	mock.Mock
}

func (m *MockEmailSender) Send(email Email) error {
	args := m.Called(email)
	return args.Error(0)
}

// TestSendEmail demonstrates a unit test using mocks
func TestSendEmail(t *testing.T) {
	// Arrange
	var sentEmail Email
	mockSender := NewMockEmailSender(func(email Email) error {
		sentEmail = email
		return nil
	})
	
	service := NewEmailService(mockSender)
	testEmail := TestEmail()
	
	// Act
	err := service.SendEmail(testEmail)
	
	// Assert
	if err != nil {
		t.Errorf("expected no error, got %v", err)
	}
	
	if sentEmail.To != testEmail.To {
		t.Errorf("expected recipient %s, got %s", testEmail.To, sentEmail.To)
	}
}

func TestSendEmail_Error(t *testing.T) {
	// Arrange
	expectedErr := errors.New("send error")
	mockSender := NewMockEmailSender(func(email Email) error {
		return expectedErr
	})
	
	service := NewEmailService(mockSender)
	testEmail := TestEmail()
	
	// Act
	err := service.SendEmail(testEmail)
	
	// Assert
	if err != expectedErr {
		t.Errorf("expected error %v, got %v", expectedErr, err)
	}
}

// TestValidateEmail demonstrates table-driven tests
func TestValidateEmail(t *testing.T) {
	tests := []struct {
		name    string
		email   Email
		wantErr bool
	}{
		{
			name: "valid email",
			email: Email{
				To: "test@example.com",
				Subject: "Test",
				Body: "Content",
			},
			wantErr: false,
		},
		{
			name: "invalid email - empty recipient",
			email: Email{
				To: "",
				Subject: "Test",
				Body: "Content",
			},
			wantErr: true,
		},
		{
			name: "invalid email - empty subject",
			email: Email{
				To: "test@example.com",
				Body: "Content",
			},
			wantErr: true,
		},
		{
			name: "invalid email - empty body",
			email: Email{
				To: "test@example.com",
				Subject: "Test",
			},
			wantErr: true,
		},
	}
	
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			err := ValidateEmail(tt.email)
			if (err != nil) != tt.wantErr {
				t.Errorf("ValidateEmail() error = %v, wantErr %v", err, tt.wantErr)
			}
		})
	}
}

// TestNewEmailService tests the creation of a new EmailService
func TestNewEmailService(t *testing.T) {
	// Arrange
	mockSender := NewMockEmailSender(nil)
	
	// Act
	service := NewEmailService(mockSender)
	
	// Assert
	if service == nil {
		t.Error("expected non-nil service")
	}
	
	if service.sender == nil {
		t.Error("expected non-nil sender")
	}
}

// Example of integration test
func TestEmailServiceIntegration(t *testing.T) {
	if testing.Short() {
		t.Skip("Skipping integration test in short mode")
	}
	
	// Arrange
	config := EmailConfig{
		SMTPHost: "localhost",
		SMTPPort: 1025,
		// Add other config as needed
	}
	
	service := NewEmailService(NewSMTPSender(config))
	
	testEmail := Email{
		To: "integration@example.com",
		Subject: "Integration Test",
		Body: "Integration test email",
	}
	
	// Act
	err := service.SendEmail(testEmail)
	
	// Assert
	assert.NoError(t, err)
}