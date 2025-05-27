package services_test

import (
	"context"
	"testing"
	"time"

	"github.com/example/mail-service/models"
	"github.com/example/mail-service/services"
	"github.com/stretchr/testify/assert"
	"github.com/stretchr/testify/mock"
)

// MockBroadcastRepository is a mock implementation of BroadcastRepository
type MockBroadcastRepository struct {
	mock.Mock
}

func (m *MockBroadcastRepository) Create(ctx context.Context, broadcast *models.Broadcast) error {
	args := m.Called(ctx, broadcast)
	return args.Error(0)
}

func (m *MockBroadcastRepository) Update(ctx context.Context, broadcast *models.Broadcast) error {
	args := m.Called(ctx, broadcast)
	return args.Error(0)
}

func (m *MockBroadcastRepository) FindBySlug(ctx context.Context, slug string) (*models.Broadcast, error) {
	args := m.Called(ctx, slug)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.Broadcast), args.Error(1)
}

func (m *MockBroadcastRepository) GetTargetContacts(ctx context.Context, tagSlug string) ([]*models.Contact, error) {
	args := m.Called(ctx, tagSlug)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).([]*models.Contact), args.Error(1)
}

// MockEmailService is a mock implementation of EmailService
type MockEmailService struct {
	mock.Mock
}

func (m *MockEmailService) CreateFromMarkdown(markdown string) (*models.Email, error) {
	args := m.Called(markdown)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.Email), args.Error(1)
}

func (m *MockEmailService) GetByID(ctx context.Context, id int) (*models.Email, error) {
	args := m.Called(ctx, id)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.Email), args.Error(1)
}

func (m *MockEmailService) QueueMessage(ctx context.Context, message *models.Message) error {
	args := m.Called(ctx, message)
	return args.Error(0)
}

func TestBroadcastService_CreateBroadcast(t *testing.T) {
	repo := new(MockBroadcastRepository)
	emailSvc := new(MockEmailService)
	service := services.NewBroadcastService(repo, emailSvc)
	ctx := context.Background()

	t.Run("creates broadcast successfully", func(t *testing.T) {
		// Setup
		markdown := "---\nslug: test\nsubject: Test Email\n---\nTest content"
		email := &models.Email{
			ID:        1,
			Slug:      "test",
			Subject:   "Test Email",
			CreatedAt: time.Now(),
		}

		emailSvc.On("CreateFromMarkdown", markdown).Return(email, nil)
		repo.On("Create", ctx, mock.AnythingOfType("*models.Broadcast")).Return(nil)

		// Execute
		broadcast, err := service.CreateBroadcast(ctx, markdown)

		// Assert
		assert.NoError(t, err)
		assert.NotNil(t, broadcast)
		assert.Equal(t, email.ID, broadcast.EmailID)
		assert.Equal(t, email.Subject, broadcast.Name)
		assert.Equal(t, email.Slug, broadcast.Slug)
		assert.Equal(t, "pending", broadcast.Status)
		repo.AssertExpectations(t)
		emailSvc.AssertExpectations(t)
	})

	t.Run("returns error when email creation fails", func(t *testing.T) {
		// Setup
		markdown := "invalid markdown"
		emailSvc.On("CreateFromMarkdown", markdown).Return(nil, assert.AnError)

		// Execute
		broadcast, err := service.CreateBroadcast(ctx, markdown)

		// Assert
		assert.Error(t, err)
		assert.Nil(t, broadcast)
		repo.AssertNotCalled(t, "Create")
		emailSvc.AssertExpectations(t)
	})
}

func TestBroadcastService_QueueBroadcast(t *testing.T) {
	repo := new(MockBroadcastRepository)
	emailSvc := new(MockEmailService)
	service := services.NewBroadcastService(repo, emailSvc)
	ctx := context.Background()

	t.Run("queues broadcast successfully", func(t *testing.T) {
		// Setup
		broadcast := &models.Broadcast{
			ID:       1,
			EmailID:  1,
			Status:   "pending",
			Slug:     "test",
			Name:     "Test Email",
		}

		email := &models.Email{
			ID:      1,
			Subject: "Test Email",
			HTML:    "<p>Test content</p>",
		}

		contacts := []*models.Contact{
			{ID: 1, Email: "user1@example.com"},
			{ID: 2, Email: "user2@example.com"},
		}

		emailSvc.On("GetByID", ctx, broadcast.EmailID).Return(email, nil)
		repo.On("GetTargetContacts", ctx, broadcast.SendToTag).Return(contacts, nil)
		emailSvc.On("QueueMessage", ctx, mock.AnythingOfType("*models.Message")).Return(nil).Times(len(contacts))
		repo.On("Update", ctx, broadcast).Return(nil)

		// Execute
		err := service.QueueBroadcast(ctx, broadcast)

		// Assert
		assert.NoError(t, err)
		assert.Equal(t, "queued", broadcast.Status)
		repo.AssertExpectations(t)
		emailSvc.AssertExpectations(t)
	})
}
