package handlers

import (
	"encoding/json"
	"log"
	"net/http"

	"github.com/StevenYAMBOS/waitify-api/internal/models"
)

// Créer une entreprise
func BusinessHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, `[authHandler.go -> OnboardingHandler()] -> Mauvaise requête HTTP (mauvaise méthode).`, http.StatusBadRequest)
	}

	w.Header().Set("Content-Type", "application/json")

	// Décode JSON de la requête
	var business models.Business
	if err := json.NewDecoder(r.Body).Decode(&business); err != nil {
		log.Println(`[authHandler.go -> OnboardingHandler()] -> Mauvais corps de requête : `, err)
		http.Error(w, `[authHandler.go -> OnboardingHandler()] -> Mauvais corps de requête.`, http.StatusBadRequest)
		return
	}

	// Validation nom de l'entreprise
	if len(business.Name) < 1 {
		http.Error(w, `[authHandler.go -> OnboardingHandler()] -> Le nom de l'entreprise doit avoir au moins 1 caractère.`, http.StatusBadRequest)
		return
	}

	// Validation numéro de téléphone
	if len(business.PhoneNumber) < 1 {
		http.Error(w, `[authHandler.go -> OnboardingHandler()] -> Le nom de l'entreprise doit avoir au moins 1 caractère.`, http.StatusBadRequest)
		return
	}

	if err := business.ValidatePhoneNumber(); err != nil {
		log.Println("[authHandler.go -> OnboardingHandler()] -> Erreur format numéro de téléphone : ", err)
		http.Error(w, `[authHandler.go -> OnboardingHandler()] -> Erreur format de l'email.`, http.StatusBadRequest)
		return
	}
}
