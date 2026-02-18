package config

import (
	"os"
	"testing"
)

func TestLoad_DefaultValues(t *testing.T) {
	// Clear any env vars that might be set
	os.Unsetenv("DATABASE_URL")
	os.Unsetenv("SEND_WORKER")
	os.Unsetenv("DEFAULT_FROM")

	cfg := Load()

	if cfg.DatabaseURL != "" {
		t.Errorf("expected empty DATABASE_URL, got %q", cfg.DatabaseURL)
	}
	if cfg.SendWorker != "" {
		t.Errorf("expected empty SEND_WORKER, got %q", cfg.SendWorker)
	}
	if cfg.DefaultFrom != "noreply@tailwind.dev" {
		t.Errorf("expected default from address, got %q", cfg.DefaultFrom)
	}
	if cfg.SMTPPort != 465 {
		t.Errorf("expected SMTP port 465, got %d", cfg.SMTPPort)
	}
}

func TestLoad_FromEnvironment(t *testing.T) {
	os.Setenv("DATABASE_URL", "postgresql://localhost:5432/testdb")
	os.Setenv("SEND_WORKER", "local")
	os.Setenv("DEFAULT_FROM", "test@example.com")
	defer func() {
		os.Unsetenv("DATABASE_URL")
		os.Unsetenv("SEND_WORKER")
		os.Unsetenv("DEFAULT_FROM")
	}()

	cfg := Load()

	if cfg.DatabaseURL != "postgresql://localhost:5432/testdb" {
		t.Errorf("expected DATABASE_URL from env, got %q", cfg.DatabaseURL)
	}
	if cfg.SendWorker != "local" {
		t.Errorf("expected SEND_WORKER=local, got %q", cfg.SendWorker)
	}
	if cfg.DefaultFrom != "test@example.com" {
		t.Errorf("expected DEFAULT_FROM from env, got %q", cfg.DefaultFrom)
	}
}

func TestGetEnv_ReturnsFallback(t *testing.T) {
	os.Unsetenv("NONEXISTENT_VAR")
	val := getEnv("NONEXISTENT_VAR", "fallback")
	if val != "fallback" {
		t.Errorf("expected fallback, got %q", val)
	}
}

func TestGetEnv_ReturnsEnvValue(t *testing.T) {
	os.Setenv("TEST_CONFIG_VAR", "from_env")
	defer os.Unsetenv("TEST_CONFIG_VAR")

	val := getEnv("TEST_CONFIG_VAR", "fallback")
	if val != "from_env" {
		t.Errorf("expected from_env, got %q", val)
	}
}
