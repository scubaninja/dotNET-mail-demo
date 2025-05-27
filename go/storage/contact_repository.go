package storage

import (
	"context"
	"database/sql"
	"errors"
	"time"

	"github.com/example/mail-service/models"
	"github.com/lib/pq"
)

type ContactRepository interface {
	Create(ctx context.Context, contact *models.Contact) error
	Update(ctx context.Context, contact *models.Contact) error
	FindByEmail(ctx context.Context, email string) (*models.Contact, error)
	FindByKey(ctx context.Context, key string) (*models.Contact, error)
}

type postgresContactRepository struct {
	db *sql.DB
}

func NewContactRepository(db *sql.DB) ContactRepository {
	return &postgresContactRepository{
		db: db,
	}
}

func (r *postgresContactRepository) Create(ctx context.Context, contact *models.Contact) error {
	query := `
		INSERT INTO mail.contacts (email, name, key, subscribed, created_at)
		VALUES ($1, $2, $3, $4, $5)
		RETURNING id`

	err := r.db.QueryRowContext(ctx, query,
		contact.Email,
		contact.Name,
		contact.Key,
		contact.Subscribed,
		contact.CreatedAt,
	).Scan(&contact.ID)

	if err != nil {
		if pgErr, ok := err.(*pq.Error); ok {
			if pgErr.Code == "23505" { // unique_violation
				return errors.New("contact already exists")
			}
		}
		return err
	}

	return nil
}

func (r *postgresContactRepository) Update(ctx context.Context, contact *models.Contact) error {
	query := `
		UPDATE mail.contacts
		SET email = $1, name = $2, subscribed = $3, updated_at = $4
		WHERE id = $5`

	now := time.Now().UTC()
	result, err := r.db.ExecContext(ctx, query,
		contact.Email,
		contact.Name,
		contact.Subscribed,
		now,
		contact.ID,
	)
	if err != nil {
		return err
	}

	rows, err := result.RowsAffected()
	if err != nil {
		return err
	}
	if rows == 0 {
		return errors.New("contact not found")
	}

	contact.UpdatedAt = &now
	return nil
}

func (r *postgresContactRepository) FindByEmail(ctx context.Context, email string) (*models.Contact, error) {
	contact := &models.Contact{}
	query := `
		SELECT id, email, name, key, subscribed, created_at, updated_at
		FROM mail.contacts
		WHERE email = $1`

	err := r.db.QueryRowContext(ctx, query, email).Scan(
		&contact.ID,
		&contact.Email,
		&contact.Name,
		&contact.Key,
		&contact.Subscribed,
		&contact.CreatedAt,
		&contact.UpdatedAt,
	)

	if err == sql.ErrNoRows {
		return nil, errors.New("contact not found")
	}
	if err != nil {
		return nil, err
	}

	return contact, nil
}

func (r *postgresContactRepository) FindByKey(ctx context.Context, key string) (*models.Contact, error) {
	contact := &models.Contact{}
	query := `
		SELECT id, email, name, key, subscribed, created_at, updated_at
		FROM mail.contacts
		WHERE key = $1`

	err := r.db.QueryRowContext(ctx, query, key).Scan(
		&contact.ID,
		&contact.Email,
		&contact.Name,
		&contact.Key,
		&contact.Subscribed,
		&contact.CreatedAt,
		&contact.UpdatedAt,
	)

	if err == sql.ErrNoRows {
		return nil, errors.New("contact not found")
	}
	if err != nil {
		return nil, err
	}

	return contact, nil
}
