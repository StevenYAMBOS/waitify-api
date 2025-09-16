package config

import (
	"fmt"
	"os"
	"time"

	"github.com/joho/godotenv"
)

type Config struct {
	Server struct {
		Port         string
		Host         string
		ReadTimeout  time.Duration
		WriteTimeout time.Duration
	}

	Database struct {
		Host     string
		Port     string
		User     string
		Password string
		DBName   string
		SSLMode  string
	}

	JWT struct {
		Secret        string
		TokenExpiry   time.Duration
		RefreshExpiry time.Duration
	}
	GCP struct {
		ClientID     string
		ClientSecret string
		RedirectURL  string
		Scopes       []string
		Endpoint     string
	}

	Environment string
}

func Load() (*Config, error) {
	godotenv.Load()

	cfg := &Config{}

	// Serveur
	cfg.Server.Port = os.Getenv("SERVER_PORT")
	cfg.Server.Host = os.Getenv("SERVER_HOST")
	cfg.Server.ReadTimeout = time.Second * 15
	cfg.Server.WriteTimeout = time.Second * 15

	// Base de données
	cfg.Database.Host = os.Getenv("DB_HOST")
	cfg.Database.Port = os.Getenv("DB_PORT")
	cfg.Database.User = os.Getenv("DB_USER")
	cfg.Database.Password = os.Getenv("DB_PASSWORD")
	cfg.Database.DBName = os.Getenv("DB_NAME")
	cfg.Database.SSLMode = os.Getenv("DB_SSLMODE")

	// JWT
	cfg.JWT.Secret = os.Getenv("JWT_SECRET")
	cfg.JWT.TokenExpiry = time.Hour * 24    // 24 heures
	cfg.JWT.RefreshExpiry = time.Hour * 168 // 7 jours

	// Google Cloud Platform
	cfg.GCP.ClientID = os.Getenv("GCP_CLIENT_ID")
	cfg.GCP.ClientSecret = os.Getenv("GCP_CLIENT_SECRET")
	cfg.GCP.RedirectURL = os.Getenv("GCP_CLIENT_CALLBACK")

	cfg.Environment = os.Getenv("ENV")

	return cfg, nil
}

// func getEnv(key, defaultValue string) string {
// 	if value := os.Getenv(key); value != "" {
// 		return value
// 	}
// 	return defaultValue
// }

func (c *Config) GetDSN() string {
	return fmt.Sprintf("host=%s port=%s user=%s password=%s dbname=%s sslmode=%s",
		c.Database.Host,
		c.Database.Port,
		c.Database.User,
		c.Database.Password,
		c.Database.DBName,
		c.Database.SSLMode,
	)
}
