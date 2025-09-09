package utils

import (
	"errors"

	"golang.org/x/crypto/bcrypt"
)

// HashPassword converts a plain text password into a hashed version
func HashPassword(password string) (string, error) {
	// Cost factor of 12 provides a good balance between security and performance
	bytes, err := bcrypt.GenerateFromPassword([]byte(password), 12)
	if err != nil {
		return "", errors.New("[password.go] -> Erreur lors du chargement du mot de passe.")
	}
	return string(bytes), nil
}

// CheckPasswordHash compares a password against a hash
func CheckPasswordHash(password, hash string) bool {
	err := bcrypt.CompareHashAndPassword([]byte(hash), []byte(password))
	return err == nil
}

// ValidatePassword checks password complexity requirements
func ValidatePassword(password string) error {
	if len(password) < 6 {
		return errors.New("[password.go] -> Le mot de passe doit avoir au moins caractères.")
	}

	if len(password) > 100 {
		return errors.New("[password.go] -> Le mot de passe ne doit pas dépasser 100 caractères.")
	}
	return nil
}
