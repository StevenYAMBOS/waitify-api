package models

import (
	"errors"
	"regexp"
	"time"

	"github.com/google/uuid"
	"golang.org/x/oauth2"
)

type User struct {
	ID        uuid.UUID `json:"id" db:"id"`
	Email     string    `json:"email"`
	Password  string    `json:"-" db:"password"` // "-" signifie que ça ne sera pas inclut dans le JSON
	CreatedAt time.Time `json:"created_at" db:"created_at"`
	UpdatedAt time.Time `json:"updated_at" db:"updated_at"`
}

type LoginRequest struct {
	Email    string `json:"email" binding:"required,email"`
	Password string `json:"password" binding:"required,min=6"`
}

type RegisterRequest struct {
	Email    string `json:"email" binding:"required,email"`
	Password string `json:"password" binding:"required,min=6"`
}

func (user *RegisterRequest) Validate() error {
	emailRegex := regexp.MustCompile(`^[a-z0-9._%+\-]+@[a-z0-9.\-]+\.[a-z]{2,4}$`)
	if !emailRegex.MatchString(user.Email) {
		return errors.New("[user.go] -> Format d'email invalide.")
	}
	return nil
}

func (user *RegisterRequest) ValidatePassword() error {
	if len(user.Password) < 6 {
		return errors.New("[user.go] -> Le mot de passe doit avoir au moins caractères.")
	}

	if len(user.Password) > 100 {
		return errors.New("[user.go] -> Le mot de passe ne doit pas dépasser 100 caractères.")
	}
	return nil
}

type AuthResponse struct {
	Token string `json:"token"`
	User  User   `json:"user"`
}

// Google Cloud
type App struct {
	config *oauth2.Config
}
