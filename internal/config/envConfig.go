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
	AWSS3 struct {
		AWSS3Region        string
		AWSS3Bucket        string
		AWSS3UsersDir      string
		AWSS3BusinessesDir string
	}
	AWSIAM struct {
		AWSIAMAccessKey string
		AWSIAMSecretKey string
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

	// AWS S3 bucket
	cfg.AWSS3.AWSS3Region = os.Getenv("AWS_S3_REGION")
	cfg.AWSS3.AWSS3Bucket = os.Getenv("AWS_S3_BUCKET")
	cfg.AWSS3.AWSS3UsersDir = os.Getenv("AWS_S3_BUCKET_USERS")
	cfg.AWSS3.AWSS3BusinessesDir = os.Getenv("AWS_S3_BUCKET_BUSINESSES")

	// AWS S3 bucket
	cfg.AWSIAM.AWSIAMAccessKey = os.Getenv("AWS_IAM_ACCESS_KEY")
	cfg.AWSIAM.AWSIAMSecretKey = os.Getenv("AWS_IAM_SECRET_KEY")

	// Environnement de développement
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
