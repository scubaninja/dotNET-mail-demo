package api

import (
	"net/http"
	"net/http/httptest"
	"testing"
)

func TestHandleAbout_ReturnsCorrectResponse(t *testing.T) {
	req := httptest.NewRequest("GET", "/about", nil)
	w := httptest.NewRecorder()

	HandleAbout(w, req)

	if w.Code != http.StatusOK {
		t.Errorf("expected status %d, got %d", http.StatusOK, w.Code)
	}

	expected := "Tailwind Traders Mail Services API"
	if w.Body.String() != expected {
		t.Errorf("expected body %q, got %q", expected, w.Body.String())
	}
}

func TestHandleAbout_ContentType(t *testing.T) {
	req := httptest.NewRequest("GET", "/about", nil)
	w := httptest.NewRecorder()

	HandleAbout(w, req)

	ct := w.Header().Get("Content-Type")
	if ct != "text/plain" {
		t.Errorf("expected Content-Type 'text/plain', got %q", ct)
	}
}
