package models

import (
	"errors"
	"log"
	"regexp"
	"time"

	"github.com/StevenYAMBOS/waitify-api/internal/config"
	"github.com/google/uuid"
	"golang.org/x/oauth2"
	"golang.org/x/oauth2/google"
)

// Modèle utilisateur
type User struct {
	ID             uuid.UUID `json:"id" db:"id"`
	Google_id      string    `json:"google_id" db:"google_id"`
	Email          string    `json:"email" db:"email"`
	FirstName      string    `json:"first_name" db:"first_name"`
	LastName       string    `json:"last_name" db:"last_name"`
	ProfilePicture string    `json:"profile_picture" db:"profile_picture"`
	AuthProvider   string    `json:"auth_provider" db:"auth_provider"`
	Password       string    `json:"-" db:"password"` // "-" signifie que ça ne sera pas inclut dans le JSON
	CreatedAt      time.Time `json:"created_at" db:"created_at"`
	UpdatedAt      time.Time `json:"updated_at" db:"updated_at"`
}

// Format requête connexion
type LoginRequest struct {
	Email    string `json:"email" binding:"required,email"`
	Password string `json:"password" binding:"required,min=6"`
}

// Format requête inscription
type RegisterRequest struct {
	Email    string `json:"email" binding:"required,email"`
	Password string `json:"password" binding:"required,min=6"`
}

// Format validation email
func (user *RegisterRequest) Validate() error {
	emailRegex := regexp.MustCompile(`^[a-z0-9._%+\-]+@[a-z0-9.\-]+\.[a-z]{2,4}$`)
	if !emailRegex.MatchString(user.Email) {
		return errors.New("[user.go] -> Format d'email invalide.")
	}
	return nil
}

// Format validation mot de passe
func (user *RegisterRequest) ValidatePassword() error {
	if len(user.Password) < 6 {
		return errors.New("[user.go] -> Le mot de passe doit avoir au moins caractères.")
	}

	if len(user.Password) > 100 {
		return errors.New("[user.go] -> Le mot de passe ne doit pas dépasser 100 caractères.")
	}
	return nil
}

// Format réponse auhtentification
type AuthResponse struct {
	Token string `json:"token"`
	User  User   `json:"user"`
}

/* ======================= GOOGLE CLOUD ======================= */

// Config Google Cloud
type Config struct {
	GoogleLoginConfig oauth2.Config
}

// Modèle utilisateur Google (SCOPE email + profil)
type GoogleUser struct {
	ID            string `json:"id"`
	Email         string `json:"email"`
	VerifiedEmail bool   `json:"verified_email"`
	Name          string `json:"name"`
	GivenName     string `json:"given_name"`
	FamilyName    string `json:"family_name"`
	Picture       string `json:"picture"`
	Locale        string `json:"locale"`
}

var AppConfig Config

// Config Google Cloud
func GoogleConfig() oauth2.Config {
	// Variables d'environnement
	cfg, err := config.Load()
	if err != nil {
		log.Fatal(`[user.go] -> Erreur lors du chargement des variables d'environnements.`, err)
	}

	AppConfig.GoogleLoginConfig = oauth2.Config{
		ClientID:     cfg.GCP.ClientID,
		ClientSecret: cfg.GCP.ClientSecret,
		RedirectURL:  cfg.GCP.RedirectURL,
		Scopes: []string{"https://www.googleapis.com/auth/userinfo.email",
			"https://www.googleapis.com/auth/userinfo.profile"},
		Endpoint: google.Endpoint,
	}

	return AppConfig.GoogleLoginConfig
}

type GoogleAuthResponse struct {
	Token string     `json:"TOKEN"`
	User  GoogleUser `json:"USER_INFOS"`
}
