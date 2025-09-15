package handlers

import (
	"database/sql"
	"encoding/json"
	"io"
	"log"
	"net/http"
	"time"

	"github.com/StevenYAMBOS/waitify-api/internal/database"
	"github.com/StevenYAMBOS/waitify-api/internal/models"
	"github.com/StevenYAMBOS/waitify-api/internal/utils"
	"github.com/google/uuid"

	"golang.org/x/crypto/bcrypt"
	"golang.org/x/oauth2"
)

// Inscription
func RegisterHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, `[authHandler.go -> RegisterHandler()] -> Mauvaise requête HTTP (mauvaise méthode).`, http.StatusBadRequest)
	}

	w.Header().Set("Content-Type", "application/json")

	var registerRequest models.RegisterRequest
	if err := json.NewDecoder(r.Body).Decode(&registerRequest); err != nil {
		log.Println(`[authHandler.go -> RegisterHandler()] -> Mauvais corps de requête : `, err)
		http.Error(w, `[authHandler.go -> RegisterHandler()] -> Corps de la requête invalide.`, http.StatusBadRequest)
		return
	}

	// Validation Email
	if registerRequest.Email == "" {
		http.Error(w, `[authHandler.go -> RegisterHandler()] -> Email requis pour s'inscrire.`, http.StatusBadRequest)
		return
	}

	if err := registerRequest.Validate(); err != nil {
		log.Println("Erreur format email : ", err)
		http.Error(w, `[authHandler.go -> RegisterHandler()] -> Erreur format de l'email.`, http.StatusBadRequest)
		return
	}

	// Validation mot de passe
	if registerRequest.Password == "" {
		http.Error(w, `[authHandler.go -> RegisterHandler()] -> Mot de passe requis pour s'inscrire.`, http.StatusBadRequest)
		return
	}

	if err := registerRequest.ValidatePassword(); err != nil {
		log.Println("Erreur format du mot de passe : ", err)
		http.Error(w, `[authHandler.go -> RegisterHandler()] -> Erreur format du mot de passe.`, http.StatusBadRequest)
		return
	}

	// Vérifier si l'utilisateur existe
	var exists bool
	err := database.DB.QueryRow("SELECT EXISTS(SELECT 1 FROM users WHERE email = $1)",
		registerRequest.Email).Scan(&exists)
	if err != nil {
		log.Println("Erreur vérification si l'utilisateur existe : ", err)
		http.Error(w, `[authHandler.go -> RegisterHandler()] -> Erreur vérification si l'utilisateur existe.`, http.StatusInternalServerError)
		return
	}
	if exists {
		log.Println("Erreur email est déjà associé à un compte : ", err)
		http.Error(w, `[authHandler.go -> RegisterHandler()] -> ERREUR. Cet email est déjà associé à un compte.`, http.StatusConflict)
		return
	}

	// Hasher le mot de passe
	hashedPassword, err := bcrypt.GenerateFromPassword([]byte(registerRequest.Password), bcrypt.DefaultCost)
	if err != nil {
		log.Println("[authHandler.go -> RegisterHandler()] -> Erreur lors du hashage du mot de passe.", err)
		http.Error(w, "[authHandler.go -> RegisterHandler()] -> Erreur lors du hashage du mot de passe.", http.StatusInternalServerError)
		return
	}

	// Insertion dans la base de données
	var user models.User
	err = database.DB.QueryRow(
		"INSERT INTO users (id, email, password, created_at, updated_at) VALUES ($1, $2, $3, $4, $5) RETURNING id, email, password, created_at, updated_at",
		uuid.New().String(), registerRequest.Email, string(hashedPassword), time.Now(), time.Now(),
	).Scan(&user.ID, &user.Email, &user.Password, &user.CreatedAt, &user.UpdatedAt)

	if err != nil {
		log.Println("Erreur insertion dans la base de données : ", err)
		http.Error(w, "[authHandler.go -> RegisterHandler()] -> Erreur lors de la création de l'utilisateur.", http.StatusInternalServerError)
		return
	}

	// Génération du token
	token, err := utils.GenerateToken(user.ID, user.Email)
	if err != nil {
		http.Error(w, "[authHandler.go -> RegisterHandler()] -> Erreur lors de la génération du token", http.StatusInternalServerError)
		return
	}

	response := models.AuthResponse{
		Token: token,
		User:  user,
	}

	json.NewEncoder(w).Encode(response)
}

// Connexion
func LoginHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, `[authHandler.go -> LoginHandler()] -> Mauvaise requête HTTP (mauvaise méthode).`, http.StatusBadRequest)
	}

	w.Header().Set("Content-Type", "application/json")

	// Décode JSON de la requête
	var loginRequest models.LoginRequest
	if err := json.NewDecoder(r.Body).Decode(&loginRequest); err != nil {
		log.Println(`[authHandler.go -> LoginHandler()] -> Mauvais corps de requête : `, err)
		http.Error(w, `[authHandler.go -> LoginHandler()] -> Mauvais corps de requête.`, http.StatusBadRequest)
		return
	}

	// Validation des inputs
	if loginRequest.Email == "" || loginRequest.Password == "" {
		http.Error(w, `[authHandler.go -> LoginHandler()] -> L'email et le mot de passe sont requis.`, http.StatusBadRequest)
		return
	}

	// Récupérer les informations de l'utilisateur
	var user models.User
	err := database.DB.QueryRow("SELECT id, email, password, created_at, updated_at FROM users WHERE email = $1", loginRequest.Email).
		Scan(&user.ID, &user.Email, &user.Password, &user.CreatedAt, &user.UpdatedAt)

	if err == sql.ErrNoRows {
		log.Println(`[authHandler.go -> LoginHandler()] -> Mauvaises informations de connexion : `, err)
		http.Error(w, `[authHandler.go -> LoginHandler()] -> Mauvaises informations de connexion.`, http.StatusUnauthorized)
		return
	}
	if err != nil {
		log.Println(`[authHandler.go -> LoginHandler()] -> Erreur base de données : `, err)
		http.Error(w, `[authHandler.go -> LoginHandler()] -> Erreur base de données.`, http.StatusInternalServerError)
		return
	}

	// Vérification du mot de passe
	err = bcrypt.CompareHashAndPassword([]byte(user.Password), []byte(loginRequest.Password))
	if err != nil {
		log.Println(`[authHandler.go -> LoginHandler()] -> Erreur mot de passe incorrect : `, err)
		http.Error(w, `[authHandler.go -> LoginHandler()] -> Mot de passe incorrect.`, http.StatusUnauthorized)
		return
	}

	// Générer le token JWT
	token, err := utils.GenerateToken(user.ID, user.Email)
	if err != nil {
		log.Println(`[authHandler.go -> LoginHandler()] -> Erreur lors de la génération du token : `, err)
		http.Error(w, `[authHandler.go -> LoginHandler()] -> Erreur lors de la génération du token.`, http.StatusInternalServerError)
		return
	}

	response := models.AuthResponse{
		Token: token,
		User:  user,
	}

	json.NewEncoder(w).Encode(response)
}

// GOOGLE
func (a *App) oAuthHandler(w http.ResponseWriter, r *http.Request) {
	// // Variables d'environnement
	// cfg, err := config.Load()
	// if err != nil {
	// 	log.Fatal(`[main.go] -> Erreur lors du chargement des variables d'environnements.`, err)
	// }

	// conf := &oauth2.Config{
	// 	ClientID: cfg.GCP.ClientID,
	// 	ClientSecret: cfg.GCP.ClientSecret,
	// 	RedirectURL: cfg.GCP.RedirectURL,
	// 	Scopes: []string{"email", "profile"},
	// 	Endpoint: google.Endpoint,
	// }

	url := a.config.AuthCodeURL("hello world", oauth2.AccessTypeOffline)
	http.Redirect(w, r, url, http.StatusTemporaryRedirect)
}

// Health check
func HealthCheck(w http.ResponseWriter, r *http.Request) {
	log.Println("Health check !")
	io.WriteString(w, "Health check!\n")
}

// Récupérer les informations de l'utilisateur
func ProfileHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodGet {
		http.Error(w, `[authHandler.go -> ProfileHandler()] -> Mauvaise requête HTTP (mauvaise méthode).`, http.StatusBadRequest)
	}

	w.Header().Set("Content-Type", "application/json")

	// Vérification de l'autorisation
	tokenString := r.Header.Get("Authorization")
	if tokenString == "" {
		http.Error(w, `[authHandler.go -> ProfileHandler()] -> Header d'autorisation "Authorization" requis.`, http.StatusUnauthorized)
		return
	}

	// Retirer le préfixe "Bearer "
	if len(tokenString) > 7 && tokenString[:7] == "Bearer " {
		tokenString = tokenString[7:]
	}

	// Validation du token
	claims, err := utils.ValidateToken(tokenString)
	if err != nil {
		log.Println(`[authHandler.go -> ProfileHandler()] -> Token invalide : `, err)
		http.Error(w, `[authHandler.go -> ProfileHandler()] -> Token invalide.`, http.StatusUnauthorized)
		return
	}

	// Récupéreration de l'utilisateur
	var user models.User
	err = database.DB.QueryRow("SELECT id, email, created_at, updated_at FROM users WHERE id = $1", claims.UserID).
		Scan(&user.ID, &user.Email, &user.CreatedAt, &user.UpdatedAt)

	if err != nil {
		log.Println(`[authHandler.go -> ProfileHandler()] -> Utilisateur non trouvé, erreur : `, err)
		http.Error(w, `[authHandler.go -> ProfileHandler()] -> Utilisateur non trouvé`, http.StatusNotFound)
		return
	}

	json.NewEncoder(w).Encode(user)
}
