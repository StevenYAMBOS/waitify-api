package handlers

import (
	"encoding/json"
	"log"
	"net/http"
	"time"

	"github.com/StevenYAMBOS/waitify-api/internal/database"
	"github.com/StevenYAMBOS/waitify-api/internal/models"
	"github.com/google/uuid"
)

// Créer une entreprise
func AddBusinessHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, `[authHandler.go -> AddBusinessHandler()] -> Mauvaise requête HTTP (mauvaise méthode).`, http.StatusBadRequest)
	}

	w.Header().Set("Content-Type", "application/json")

	// Décode JSON de la requête
	var business models.Business
	if err := json.NewDecoder(r.Body).Decode(&business); err != nil {
		log.Println(`[authHandler.go -> AddBusinessHandler()] -> Mauvais corps de requête : `, err)
		http.Error(w, `[authHandler.go -> AddBusinessHandler()] -> Mauvais corps de requête.`, http.StatusBadRequest)
		return
	}

	// Validation nom de l'entreprise
	if business.Name == "" {
		http.Error(w, `[authHandler.go -> AddBusinessHandler()] -> Le nom de l'entreprise doit avoir au moins 1 caractère.`, http.StatusBadRequest)
		return
	}

	if err := business.ValidatePhoneNumber(); err != nil {
		log.Println("[authHandler.go -> AddBusinessHandler()] -> Erreur format numéro de téléphone : ", err)
		http.Error(w, `[authHandler.go -> AddBusinessHandler()] -> Erreur format de l'email.`, http.StatusBadRequest)
		return
	}

	// Validation du type
	if err := business.ValidateBusinessType(); err != nil {
		http.Error(w, `[authHandler.go -> AddBusinessHandler()] -> Erreur format du type : `+err.Error(), http.StatusBadRequest)
		return
	}

	// Validation de l'adresse
	if business.Address == "" || len(business.Address) < 100 {
		http.Error(w, `[authHandler.go -> AddBusinessHandler()] -> L'adresse de l'entreprise doit être comprise 1 et 100 caractères.`, http.StatusBadRequest)
		return
	}

	// Validation de la ville
	if business.City == "" || len(business.City) < 100 {
		http.Error(w, `[authHandler.go -> AddBusinessHandler()] -> La ville de l'entreprise doit être comprise 1 et 100 caractères.`, http.StatusBadRequest)
		return
	}

	// Validation nom de l'entreprise
	if business.ZipCode == "" || len(business.ZipCode) < 100 {
		http.Error(w, `[authHandler.go -> AddBusinessHandler()] -> Le code postal de l'entreprise doit être comprise 1 et 100 caractères.`, http.StatusBadRequest)
		return
	}

	// Vérifier si l'utilisateur existe
	var exists bool
	err := database.DB.QueryRow("SELECT EXISTS(SELECT 1 FROM users WHERE id = $1)",
		business.UserID).Scan(&exists)
	if err != nil {
		http.Error(w, `[authHandler.go -> AddBusinessHandler()] -> Erreur vérification si l'utilisateur existe : `+err.Error(), http.StatusInternalServerError)
		return
	}
	if exists {
		log.Println("Erreur email est déjà associé à un compte : ", err)
		http.Error(w, `[authHandler.go -> AddBusinessHandler()] -> ERREUR. Cet email est déjà associé à un compte.`, http.StatusConflict)
		return
	}

	// Insertion dans la base de données
	err = database.DB.QueryRow(
		"INSERT INTO businesses (id, UserId, name, phone_number, address, city, zip_code, country, created_at, updated_at) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10) RETURNING id, UserId, name, phone_number, address, city, zip_code, country, created_at, updated_at",
		uuid.New().String(), business.UserID, business.Name, business.PhoneNumber, business.Address, business.City, business.ZipCode, business.Country, time.Now(), time.Now(),
	).Scan(&business.ID, &business.UserID, &business.Name, &business.PhoneNumber, &business.Address, &business.City, &business.ZipCode, &business.Country, &business.CreatedAt, &business.UpdatedAt)

	if err != nil {
		http.Error(w, "[authHandler.go -> AddBusinessHandler()] -> Erreur lors de la création de l'entreprise : "+err.Error(), http.StatusInternalServerError)
		return
	}

	response := models.AddBusinessResponse{
		Response: "L'entreprise a été créée avec succès.",
		Business: business,
	}

	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(response)
}
