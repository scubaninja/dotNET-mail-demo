package services

import (
	"context"

	"github.com/example/mail-service/models"
)

// EmailService handles all email-related business logic
type EmailService interface {
	CreateFromMarkdown(markdown string) (*models.Email, error)
	GetByID(ctx context.Context, id int) (*models.Email, error)
	QueueMessage(ctx context.Context, message *models.Message) error
}

type emailService struct {
	repo EmailRepository
}

// NewEmailService creates a new email service
func NewEmailService(repo EmailRepository) EmailService {
	return &emailService{
		repo: repo,
	}
}

func (s *emailService) CreateFromMarkdown(markdown string) (*models.Email, error) {
	// Parse markdown and extract frontmatter
	doc, err := ParseMarkdown(markdown)
	if err != nil {
		return nil, err
	}

	email := &models.Email{
		Slug:    doc.Slug,
		Subject: doc.Subject,
		Preview: doc.Preview,
		HTML:    doc.HTML,
	}

	if err := s.repo.Create(ctx, email); err != nil {
		return nil, err
	}

	return email, nil
}

func (s *emailService) GetByID(ctx context.Context, id int) (*models.Email, error) {
	return s.repo.FindByID(ctx, id)
}

func (s *emailService) QueueMessage(ctx context.Context, message *models.Message) error {
	return s.repo.CreateMessage(ctx, message)
}
