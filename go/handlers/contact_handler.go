package handlers

import (
	"encoding/json"
	"net/http"

	"github.com/go-chi/chi/v5"
)

// ContactHandler handles all contact-related HTTP endpoints
type ContactHandler struct {
	service ContactService
}

// NewContactHandler creates a new contact handler
func NewContactHandler(service ContactService) *ContactHandler {
	return &ContactHandler{
		service: service,
	}
}

// Routes returns all routes for the contact handler
func (h *ContactHandler) Routes() chi.Router {
	r := chi.NewRouter()

	r.Post("/signup", h.HandleSignup)
	r.Get("/unsubscribe/{key}", h.HandleUnsubscribe)
	r.Get("/confirm/{key}", h.HandleConfirm)

	return r
}

// ContactSignupRequest represents the signup request body
type ContactSignupRequest struct {
	Email string `json:"email"`
	Name  string `json:"name"`
}

// HandleSignup handles the contact signup endpoint
func (h *ContactHandler) HandleSignup(w http.ResponseWriter, r *http.Request) {
	var req ContactSignupRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	contact, err := h.service.CreateContact(r.Context(), req.Email, req.Name)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(contact)
}

// HandleUnsubscribe handles the unsubscribe endpoint
func (h *ContactHandler) HandleUnsubscribe(w http.ResponseWriter, r *http.Request) {
	key := chi.URLParam(r, "key")
	if err := h.service.UnsubscribeContact(r.Context(), key); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusOK)
}

// HandleConfirm handles the confirmation endpoint
func (h *ContactHandler) HandleConfirm(w http.ResponseWriter, r *http.Request) {
	key := chi.URLParam(r, "key")
	if err := h.service.ConfirmContact(r.Context(), key); err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusOK)
}
