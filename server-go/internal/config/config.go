package config

import "os"

// Config holds all application configuration values loaded from environment variables.
type Config struct {
	DatabaseURL         string
	SendWorker          string
	DefaultFrom         string
	SMTPHost            string
	SMTPUser            string
	SMTPPassword        string
	SMTPPort            int
	AzureOpenAIEndpoint string
	AzureOpenAIAPIKey   string
}

// Load reads configuration from environment variables with sensible defaults.
func Load() *Config {
	return &Config{
		DatabaseURL:         getEnv("DATABASE_URL", ""),
		SendWorker:          getEnv("SEND_WORKER", ""),
		DefaultFrom:         getEnv("DEFAULT_FROM", "noreply@tailwind.dev"),
		SMTPHost:            getEnv("SMTP_HOST", ""),
		SMTPUser:            getEnv("SMTP_USER", ""),
		SMTPPassword:        getEnv("SMTP_PASSWORD", ""),
		SMTPPort:            465,
		AzureOpenAIEndpoint: getEnv("AZURE_OPENAI_ENDPOINT", ""),
		AzureOpenAIAPIKey:   getEnv("AZURE_OPENAI_API_KEY", ""),
	}
}

func getEnv(key, fallback string) string {
	if val := os.Getenv(key); val != "" {
		return val
	}
	return fallback
}
