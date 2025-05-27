package services

import (
	"context"
	"time"

	"github.com/example/mail-service/models"
)

// BroadcastService handles all broadcast-related business logic
type BroadcastService interface {
	CreateBroadcast(ctx context.Context, markdown string) (*models.Broadcast, error)
	QueueBroadcast(ctx context.Context, broadcast *models.Broadcast) error
	GetBroadcastBySlug(ctx context.Context, slug string) (*models.Broadcast, error)
}

type broadcastService struct {
	repo     BroadcastRepository
	emailSvc EmailService
}

// NewBroadcastService creates a new broadcast service
func NewBroadcastService(repo BroadcastRepository, emailSvc EmailService) BroadcastService {
	return &broadcastService{
		repo:     repo,
		emailSvc: emailSvc,
	}
}

func (s *broadcastService) CreateBroadcast(ctx context.Context, markdown string) (*models.Broadcast, error) {
	email, err := s.emailSvc.CreateFromMarkdown(markdown)
	if err != nil {
		return nil, err
	}

	broadcast := &models.Broadcast{
		EmailID:    email.ID,
		Status:     "pending",
		Name:       email.Subject,
		Slug:       email.Slug,
		SendToTag:  "*",
		CreatedAt:  time.Now().UTC(),
	}

	if err := s.repo.Create(ctx, broadcast); err != nil {
		return nil, err
	}

	return broadcast, nil
}

func (s *broadcastService) QueueBroadcast(ctx context.Context, broadcast *models.Broadcast) error {
	// Get the email template
	email, err := s.emailSvc.GetByID(ctx, broadcast.EmailID)
	if err != nil {
		return err
	}

	// Get contacts to send to
	contacts, err := s.repo.GetTargetContacts(ctx, broadcast.SendToTag)
	if err != nil {
		return err
	}

	// Create messages for each contact
	for _, contact := range contacts {
		message := models.NewMessage(
			broadcast.Slug,
			contact.Email,
			email.Subject,
			email.HTML,
		)
		if err := s.emailSvc.QueueMessage(ctx, message); err != nil {
			return err
		}
	}

	broadcast.Status = "queued"
	return s.repo.Update(ctx, broadcast)
}

func (s *broadcastService) GetBroadcastBySlug(ctx context.Context, slug string) (*models.Broadcast, error) {
	return s.repo.FindBySlug(ctx, slug)
}
