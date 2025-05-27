package services

import (
	"context"
	"errors"

	"github.com/example/mail-service/models"
)

// ContactService handles all contact-related business logic
type ContactService interface {
	CreateContact(ctx context.Context, email, name string) (*models.Contact, error)
	UnsubscribeContact(ctx context.Context, key string) error
	ConfirmContact(ctx context.Context, key string) error
	FindContactByEmail(ctx context.Context, email string) (*models.Contact, error)
	FindContactByKey(ctx context.Context, key string) (*models.Contact, error)
}

type contactService struct {
	repo ContactRepository
}

// NewContactService creates a new contact service
func NewContactService(repo ContactRepository) ContactService {
	return &contactService{
		repo: repo,
	}
}

func (s *contactService) CreateContact(ctx context.Context, email, name string) (*models.Contact, error) {
	existing, err := s.repo.FindByEmail(ctx, email)
	if err != nil && !errors.Is(err, ErrNotFound) {
		return nil, err
	}
	if existing != nil {
		return nil, errors.New("contact already exists")
	}

	contact := models.NewContact(email, name)
	if err := s.repo.Create(ctx, contact); err != nil {
		return nil, err
	}

	return contact, nil
}

func (s *contactService) UnsubscribeContact(ctx context.Context, key string) error {
	contact, err := s.repo.FindByKey(ctx, key)
	if err != nil {
		return err
	}

	contact.Subscribed = false
	return s.repo.Update(ctx, contact)
}

func (s *contactService) ConfirmContact(ctx context.Context, key string) error {
	contact, err := s.repo.FindByKey(ctx, key)
	if err != nil {
		return err
	}

	contact.Subscribed = true
	return s.repo.Update(ctx, contact)
}

func (s *contactService) FindContactByEmail(ctx context.Context, email string) (*models.Contact, error) {
	return s.repo.FindByEmail(ctx, email)
}

func (s *contactService) FindContactByKey(ctx context.Context, key string) (*models.Contact, error) {
	return s.repo.FindByKey(ctx, key)
}
