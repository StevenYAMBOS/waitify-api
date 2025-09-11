package models

import (
	"errors"
	"regexp"
	"time"

	"github.com/google/uuid"
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

type AuthResponse struct {
	Token string `json:"token"`
	User  User   `json:"user"`
}
