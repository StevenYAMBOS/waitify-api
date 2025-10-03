package handlers

import (
	"context"
	"database/sql"
	"encoding/json"
	"html/template"
	"io"
	"log"
	"net/http"
	"time"

	"github.com/StevenYAMBOS/waitify-api/internal/database"
	"github.com/StevenYAMBOS/waitify-api/internal/models"
	"github.com/StevenYAMBOS/waitify-api/internal/utils"
	"github.com/google/uuid"
	"golang.org/x/crypto/bcrypt"
)

// Inscription
func RegisterHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, `[authHandler.go -> RegisterHandler()] -> Mauvaise requête HTTP (mauvaise méthode).`, http.StatusMethodNotAllowed)
	}

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
		"INSERT INTO users (id, email, password, profile_picture, created_at, updated_at) VALUES ($1, $2, $3, $4, $5, $6) RETURNING id, email, password, profile_picture, created_at, updated_at",
		uuid.New().String(), registerRequest.Email, string(hashedPassword), registerRequest.ProfilePicture, time.Now(), time.Now(),
	).Scan(&user.ID, &user.Email, &user.Password, &user.ProfilePicture, &user.CreatedAt, &user.UpdatedAt)

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

	w.WriteHeader(http.StatusCreated)
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

// Test connexion Google
func TestHandler(w http.ResponseWriter, r *http.Request) {
	// Parsing an HTML document present in the current directory.
	t, err := template.ParseFiles("index.html")
	if err != nil {
		log.Print(err)
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}
	// serving the parsed HTML document
	t.Execute(w, nil)
}

// Portail de connexion Google Auth
func GoogleLoginHandler(w http.ResponseWriter, r *http.Request) {
	url := models.AppConfig.GoogleLoginConfig.AuthCodeURL("randomstate")
	http.Redirect(w, r, url, http.StatusTemporaryRedirect)
}

/*
 * Authentification avec Google
 * La route fonctionne de la manière suivante :
 * - Si l'utilisateur a déjà un compte enregistré dans la base de données avec son adresse email Google, alors lorsqu'il se connecte on renvoie le token de connexion généré par l'API Google au client
 * - Si l'utilisateur n'a pas de compte enregistré dans la base de données avec son adresse email Google, alors lorsqu'il s'inscrit ses données sont enregistrées dans la base de données ET on renvoie le token de connexion généré par l'API Google au client
 */
func GoogleCallback(w http.ResponseWriter, r *http.Request) {
	state := r.URL.Query().Get("state")
	if state != "randomstate" {
		http.Error(w, `[authHandlers.go -> GoogleCallback()] -> Les états ne matchent pas. "randomstate" manquant !`, http.StatusBadRequest)
	}
	code := r.URL.Query().Get("code")

	googleConnection := models.GoogleConfig()

	// Exchanging the code for an access token
	token, err := googleConnection.Exchange(context.Background(), code)
	if err != nil {
		log.Println(`[authHandler.go -> GoogleCallback()] -> Erreur lors de l'échange code <-> token : `, err)
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	// Creating an HTTP client to make authenticated request using the access key.
	// This client method also regenerate the access key using the refresh key.
	// client := models.GoogleConfig.Client(context.Background(), t)

	// Récupérer les informations publiques de l'utilisateur depuis l'API GCP
	resp, err := http.Get("https://www.googleapis.com/oauth2/v2/userinfo?access_token=" + token.AccessToken)
	if err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	defer resp.Body.Close()

	// Modèle utilisateur API Google
	var v models.GoogleUser

	// Lire le corps JSON en le décodant
	err = json.NewDecoder(resp.Body).Decode(&v)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	// Réponse
	response := models.GoogleAuthResponse{
		Token: token.AccessToken,
		User:  v,
	}

	// Vérifier si l'utilisateur existe
	var exists bool

	// Est-ce que l'utilisateur existe ?
	err = database.DB.QueryRow("SELECT EXISTS(SELECT 1 FROM users WHERE email = $1)",
		v.Email).Scan(&exists)
	if err != nil {
		log.Println("Erreur vérification si l'utilisateur existe : ", err)
		http.Error(w, `[authHandler.go -> GoogleCallback()] -> Erreur vérification si l'utilisateur existe.`, http.StatusInternalServerError)
		return
	}
	if exists {
		// Si l'utilisateur existe on renvoie le token de connexion
		log.Println("Utilisateur connecté avec succès.")
		w.WriteHeader(http.StatusAccepted)
		json.NewEncoder(w).Encode(response)
		return
	} else if !exists { // Sinon
		// Insertion dans la base de données
		var user models.User
		googleErr := database.DB.QueryRow(
			"INSERT INTO users (id, google_id, email, first_name, last_name, profile_picture, created_at, updated_at) VALUES ($1, $2, $3, $4, $5, $6, $7, $8) RETURNING id, google_id, email, first_name, last_name, profile_picture, created_at, updated_at",
			uuid.New().String(), v.ID, v.Email, v.GivenName, v.FamilyName, v.Picture, time.Now(), time.Now(),
		).Scan(&user.ID, &user.Google_id, &user.Email, &user.FirstName, &user.LastName, &user.ProfilePicture, &user.CreatedAt, &user.UpdatedAt)
		log.Println("Utilisateur créé avec succès.")
		w.WriteHeader(http.StatusCreated)
		json.NewEncoder(w).Encode(response)

		if googleErr != nil {
			log.Println("UTILISATEUR :", v)
			log.Println("[authHandler.go -> GoogleCallback()] -> Erreur insertion des données de l'utilisateur dans la base de données, vérifier le format des données envoyées : ", googleErr)
			http.Error(w, googleErr.Error(), http.StatusInternalServerError)
			return
		}
	}
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
