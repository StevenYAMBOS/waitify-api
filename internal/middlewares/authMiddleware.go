package middleware

import (
	"net/http"
	"strings"

	"github.com/StevenYAMBOS/waitify-api/internal/utils"
)

func AuthMiddleware(next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		tokenString := r.Header.Get("Authorization")
		if tokenString == "" {
			http.Error(w, `[authMiddleware.go -> AuthMiddleware()] -> Authorization header requis !`, http.StatusUnauthorized)
			return
		}

		// Remove "Bearer " prefix if present
		if strings.HasPrefix(tokenString, "Bearer ") {
			tokenString = strings.TrimPrefix(tokenString, "Bearer ")
		}

		_, err := utils.ValidateToken(tokenString)
		if err != nil {
			http.Error(w, `[authMiddleware.go -> AuthMiddleware()] -> Token invalide.`, http.StatusUnauthorized)
			return
		}

		next.ServeHTTP(w, r)
	}
}

func CORSMiddleware(next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Access-Control-Allow-Origin", "*")
		w.Header().Set("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS")
		w.Header().Set("Access-Control-Allow-Headers", "Content-Type, Authorization")

		if r.Method == "OPTIONS" {
			w.WriteHeader(http.StatusOK)
			return
		}

		next.ServeHTTP(w, r)
	}
}
