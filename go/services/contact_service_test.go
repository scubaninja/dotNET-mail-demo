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

// MockContactRepository is a mock implementation of ContactRepository
type MockContactRepository struct {
	mock.Mock
}

func (m *MockContactRepository) Create(ctx context.Context, contact *models.Contact) error {
	args := m.Called(ctx, contact)
	return args.Error(0)
}

func (m *MockContactRepository) Update(ctx context.Context, contact *models.Contact) error {
	args := m.Called(ctx, contact)
	return args.Error(0)
}

func (m *MockContactRepository) FindByEmail(ctx context.Context, email string) (*models.Contact, error) {
	args := m.Called(ctx, email)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.Contact), args.Error(1)
}

func (m *MockContactRepository) FindByKey(ctx context.Context, key string) (*models.Contact, error) {
	args := m.Called(ctx, key)
	if args.Get(0) == nil {
		return nil, args.Error(1)
	}
	return args.Get(0).(*models.Contact), args.Error(1)
}

func TestContactService_CreateContact(t *testing.T) {
	repo := new(MockContactRepository)
	service := services.NewContactService(repo)
	ctx := context.Background()

	t.Run("creates new contact successfully", func(t *testing.T) {
		// Setup
		email := "test@example.com"
		name := "Test User"

		repo.On("FindByEmail", ctx, email).Return(nil, services.ErrNotFound)
		repo.On("Create", ctx, mock.AnythingOfType("*models.Contact")).Return(nil)

		// Execute
		contact, err := service.CreateContact(ctx, email, name)

		// Assert
		assert.NoError(t, err)
		assert.NotNil(t, contact)
		assert.Equal(t, email, contact.Email)
		assert.Equal(t, name, contact.Name)
		assert.False(t, contact.Subscribed)
		repo.AssertExpectations(t)
	})

	t.Run("returns error when contact exists", func(t *testing.T) {
		// Setup
		email := "existing@example.com"
		name := "Existing User"
		existingContact := &models.Contact{
			ID:        1,
			Email:     email,
			CreatedAt: time.Now(),
		}

		repo.On("FindByEmail", ctx, email).Return(existingContact, nil)

		// Execute
		contact, err := service.CreateContact(ctx, email, name)

		// Assert
		assert.Error(t, err)
		assert.Nil(t, contact)
		assert.Equal(t, "contact already exists", err.Error())
		repo.AssertExpectations(t)
	})
}
