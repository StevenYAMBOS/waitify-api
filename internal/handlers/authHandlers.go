package handlers

import (
	"encoding/json"
	"log"
	"net/http"
	"time"

	"github.com/StevenYAMBOS/waitify-api/internal/database"
	"github.com/StevenYAMBOS/waitify-api/internal/models"
	"github.com/StevenYAMBOS/waitify-api/internal/utils"

	"golang.org/x/crypto/bcrypt"
)

// Inscription
func Register(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, `[authHandler.go -> Register()] -> Mauvaise requête HTTP (mauvaise méthode).`, http.StatusBadRequest)
	}

	w.Header().Set("Content-Type", "application/json")

	var req models.RegisterRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, `[authHandler.go -> Register()] -> Corps de la requête invalide.`, http.StatusBadRequest)
		return
	}

	// Validation Email
	if req.Email == "" {
		http.Error(w, `[authHandler.go -> Register()] -> Email requis pour s'inscrire.`, http.StatusBadRequest)
		return
	}

	if err := req.Validate(); err != nil {
		log.Fatal("Erreur format email : ", err)
		http.Error(w, `[authHandler.go -> Register()] -> Erreur.`, http.StatusBadRequest)
		return
	}

	// Validation mot de passe
	if req.Password == "" {
		http.Error(w, `[authHandler.go -> Register()] -> Mot de passe requis pour s'inscrire.`, http.StatusBadRequest)
		return
	}
	// else if len(req.Password) < 6 {
	// 	http.Error(w, `[authHandler.go -> Register()] -> Le mot de passe doit avoir au moins caractères.`, http.StatusBadRequest)
	// } else {
	// 	http.Error(w, `[authHandler.go -> Register()] -> Le mot de passe ne doit pas dépasser 100 caractères.`, http.StatusBadRequest)
	// 	return
	// }

	// Vérifier si l'utilisateur existe
	var exists bool
	err := database.DB.QueryRow("SELECT EXISTS(SELECT 1 FROM users WHERE email = $1)",
		req.Email).Scan(&exists)
	if err != nil {
		log.Fatal("Erreur vérification si l'utilisateur existe : ", err)
		http.Error(w, `[authHandler.go -> Register()] -> ERREUR base de données`, http.StatusInternalServerError)
		return
	}
	if exists {
		log.Fatalln("Erreur email est déjà associé à un compte : ", err)
		http.Error(w, `[authHandler.go -> Register()] -> ERREUR. Cet email est déjà associé à un compte.`, http.StatusConflict)
		return
	}
	// var existingUser models.User
	// err := database.DB.QueryRow("SELECT id FROM users WHERE email = $1", req.Email).Scan(&existingUser.ID)
	// if err != sql.ErrNoRows {
	// 	http.Error(w, `[authHandler.go -> Register()] -> ERREUR. Cet email est déjà associé à un compte.`, http.StatusConflict)
	// 	return
	// }

	// Hasher le mot de passe
	hashedPassword, err := bcrypt.GenerateFromPassword([]byte(req.Password), bcrypt.DefaultCost)
	if err != nil {
		http.Error(w, "[authHandler.go -> Register()] -> Erreur lors du hashage du mot de passe.", http.StatusInternalServerError)
		return
	}

	// Insertion dans la base de données
	var user models.User
	err = database.DB.QueryRow(
		"INSERT INTO users (email, password, created_at, updated_at) VALUES ($1, $2, $3, $4) RETURNING id, email, created_at, updated_at",
		req.Email, string(hashedPassword), time.Now(), time.Now(),
	).Scan(&user.ID, &user.Email, &user.CreatedAt, &user.UpdatedAt)

	if err != nil {
		log.Fatalln("Erreur insertion dans la base de données : ", err)
		http.Error(w, "[authHandler.go -> Register()] -> Erreur lors de la création de l'utilisateur.", http.StatusInternalServerError)
		return
	}

	// Génération du token
	token, err := utils.GenerateToken(user.ID, user.Email)
	if err != nil {
		http.Error(w, "[authHandler.go -> Register()] -> Erreur lors de la génération du token", http.StatusInternalServerError)
		return
	}

	response := models.AuthResponse{
		Token: token,
		User:  user,
	}

	json.NewEncoder(w).Encode(response)
}

// Connexion
// func Login(w http.ResponseWriter, r *http.Request) {
// 	w.Header().Set("Content-Type", "application/json")

// 	var req models.LoginRequest
// 	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
// 		http.Error(w, "Invalid request body", http.StatusBadRequest)
// 		return
// 	}

// 	// Validate input
// 	if req.Username == "" || req.Password == "" {
// 		http.Error(w, "Username and password are required", http.StatusBadRequest)
// 		return
// 	}

// 	// Get user from database
// 	var user models.User
// 	err := database.DB.QueryRow("SELECT id, username, password, created_at, updated_at FROM users WHERE username = $1", req.Username).
// 		Scan(&user.ID, &user.Username, &user.Password, &user.CreatedAt, &user.UpdatedAt)

// 	if err == sql.ErrNoRows {
// 		http.Error(w, "Invalid credentials", http.StatusUnauthorized)
// 		return
// 	}
// 	if err != nil {
// 		http.Error(w, "Database error", http.StatusInternalServerError)
// 		return
// 	}

// 	// Check password
// 	err = bcrypt.CompareHashAndPassword([]byte(user.Password), []byte(req.Password))
// 	if err != nil {
// 		http.Error(w, "Invalid credentials", http.StatusUnauthorized)
// 		return
// 	}

// 	// Generate JWT token
// 	token, err := auth.GenerateToken(user.ID, user.Username)
// 	if err != nil {
// 		http.Error(w, "Error generating token", http.StatusInternalServerError)
// 		return
// 	}

// 	response := models.AuthResponse{
// 		Token: token,
// 		User:  user,
// 	}

// 	json.NewEncoder(w).Encode(response)
// }

// // Profil utilisateur
// func Profile(w http.ResponseWriter, r *http.Request) {
// 	w.Header().Set("Content-Type", "application/json")

// 	tokenString := r.Header.Get("Authorization")
// 	if tokenString == "" {
// 		http.Error(w, "Authorization header required", http.StatusUnauthorized)
// 		return
// 	}

// 	// Remove "Bearer " prefix if present
// 	if len(tokenString) > 7 && tokenString[:7] == "Bearer " {
// 		tokenString = tokenString[7:]
// 	}

// 	claims, err := auth.ValidateToken(tokenString)
// 	if err != nil {
// 		http.Error(w, "Invalid token", http.StatusUnauthorized)
// 		return
// 	}

// 	// Get user from database
// 	var user models.User
// 	err = database.DB.QueryRow("SELECT id, username, created_at, updated_at FROM users WHERE id = $1", claims.UserID).
// 		Scan(&user.ID, &user.Username, &user.CreatedAt, &user.UpdatedAt)

// 	if err != nil {
// 		http.Error(w, "User not found", http.StatusNotFound)
// 		return
// 	}

// 	json.NewEncoder(w).Encode(user)
// }
