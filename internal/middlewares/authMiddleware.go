package middlewares

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
