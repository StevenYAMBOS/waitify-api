package handlers

import (
	"encoding/json"
	"log"
	"net/http"

	"github.com/StevenYAMBOS/waitify-api/internal/database"
	"github.com/StevenYAMBOS/waitify-api/internal/models"
)

// Activer la file d'attente
func ActivateQueueHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPut {
		http.Error(w, `Mauvaise requête HTTP (mauvaise méthode).`, http.StatusMethodNotAllowed)
	}

	var statusRequest *models.BusinessQueueStatusRequest

	if err := json.NewDecoder(r.Body).Decode(&statusRequest); err != nil {
		log.Println(`Mauvais corps de requête : `, err)
		http.Error(w, `Corps de la requête invalide.`, http.StatusBadRequest)
		return
	}

	// Récupérer l'ID de l'entreprise depuis l'URL
	IDParam := r.PathValue("id")
	log.Println(IDParam)

	// Vérifier si l'entreprise existe
	var businessExists bool
	err := database.DB.QueryRow("SELECT EXISTS(SELECT 1 FROM businesses WHERE id = $1)",
		IDParam).Scan(&businessExists)
	if err != nil {
		http.Error(w, `Erreur vérification de l'existance de l'entreprise : `+err.Error(), http.StatusInternalServerError)
		return
	}
	if !businessExists {
		log.Println("L'entreprise avec cet 'id' n'existe pas en base de données : ", err)
		http.Error(w, `L'entreprise n'existe pas en base de données ! `+err.Error(), http.StatusConflict)
		return
	}

	// Query base de données
	updt, err := database.DB.Exec(`UPDATE businesses SET is_queue_active=$2 WHERE id=$1 RETURNING *;`, IDParam, &statusRequest.IsQueueActive)
	if err != nil {
		http.Error(w, "Erreur lors de la modification de l'entreprise : "+err.Error(), http.StatusInternalServerError)
		return
	}

	log.Println(`Informations récupérées : `, *statusRequest.IsQueueActive)

	rowsAffected, err := updt.RowsAffected()
	if err != nil {
		log.Fatalf("Erreur lors de la vérification du nombre de lignes modifiées. %v", err)
	}
	log.Println(`Nombre de lignes modifiées : `, rowsAffected)

	response := []string{"File d'attente ouverte !"}

	w.WriteHeader(200)
	json.NewEncoder(w).Encode(response)
}
