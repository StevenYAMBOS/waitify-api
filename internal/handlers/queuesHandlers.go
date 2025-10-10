package handlers

import (
	"database/sql"
	"encoding/json"
	"log"
	"net/http"
	"time"

	"github.com/StevenYAMBOS/waitify-api/internal/database"
	"github.com/StevenYAMBOS/waitify-api/internal/models"
	"github.com/google/uuid"
)

/*
Activer ou désactiver la file d'attente
Côté Font on va envoyer un booléen (true ou false) pour activer ou désactiver la file d'attente
*/
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

// Rejoindre une file d'attente
func JoinQueueHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, `Méthode non autorisée`, http.StatusMethodNotAllowed)
		return
	}

	w.Header().Set("Content-Type", "application/json")

	// 1. Décoder la requête
	var req models.JoinQueueRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		log.Println("Erreur parsing JSON:", err)
		http.Error(w, `Corps de requête invalide`, http.StatusBadRequest)
		return
	}

	// 2. Validation des champs obligatoires
	if req.BusinessID == uuid.Nil {
		http.Error(w, `BusinessID requis`, http.StatusBadRequest)
		return
	}
	if req.Phone == "" {
		http.Error(w, `Numéro de téléphone requis`, http.StatusBadRequest)
		return
	}
	if err := models.ValidateBusinessPhoneNumber(req.Phone); err != nil {
		http.Error(w, `Format de téléphone invalide`, http.StatusBadRequest)
		return
	}
	if req.ClientName == "" {
		http.Error(w, `Nom du client requis`, http.StatusBadRequest)
		return
	}

	// 3. Vérifier que le business existe ET que la file est active
	var business struct {
		IsQueueActive      bool
		MaxQueueSize       int
		AverageServiceTime int // en secondes
	}

	err := database.DB.QueryRow(`
		SELECT is_queue_active, max_queue_size, average_service_time
		FROM businesses
		WHERE id = $1 AND is_active = true
	`, req.BusinessID).Scan(
		&business.IsQueueActive,
		&business.MaxQueueSize,
		&business.AverageServiceTime,
	)

	if err == sql.ErrNoRows {
		http.Error(w, `Business introuvable ou inactif`, http.StatusNotFound)
		return
	}
	if err != nil {
		log.Println("Erreur DB:", err)
		http.Error(w, `Erreur serveur`, http.StatusInternalServerError)
		return
	}

	// 4. Vérifier que la file est active
	if !business.IsQueueActive {
		http.Error(w, `La file d'attente est fermée`, http.StatusForbidden)
		return
	}

	// 5. Vérifier que le client n'est pas déjà dans la file
	var alreadyInQueue bool
	err = database.DB.QueryRow(`
		SELECT EXISTS(
			SELECT 1 FROM queue_entries
			WHERE BusinessId = $1 AND phone = $2 AND status = 'waiting'
		)
	`, req.BusinessID, req.Phone).Scan(&alreadyInQueue)

	if err != nil {
		log.Println("Erreur vérification doublon:", err)
		http.Error(w, `Erreur serveur`, http.StatusInternalServerError)
		return
	}
	if alreadyInQueue {
		http.Error(w, `Vous êtes déjà dans la file d'attente`, http.StatusConflict)
		return
	}

	// 6. Vérifier que la file n'est pas pleine
	var currentQueueSize int
	err = database.DB.QueryRow(`
		SELECT COUNT(*) FROM queue_entries
		WHERE BusinessId = $1 AND status = 'waiting'
	`, req.BusinessID).Scan(&currentQueueSize)

	if err != nil {
		log.Println("Erreur comptage file:", err)
		http.Error(w, `Erreur serveur`, http.StatusInternalServerError)
		return
	}
	if currentQueueSize >= business.MaxQueueSize {
		http.Error(w, `File d'attente complète`, http.StatusServiceUnavailable)
		return
	}

	// 7. Calculer la position (sera recalculée par le trigger, mais on l'initialise)
	nextPosition := currentQueueSize + 1

	// 8. Calculer le temps d'attente estimé
	estimatedWaitMinutes := (currentQueueSize * business.AverageServiceTime) / 60

	// 9. Insérer dans la base (le trigger recalculera automatiquement les positions)
	entryID := uuid.New()
	now := time.Now()

	_, err = database.DB.Exec(`
		INSERT INTO queue_entries (
			id, BusinessId, phone, client_name, position, 
			estimated_wait_time, status, created_at, updated_at
		) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9)
	`,
		entryID,
		req.BusinessID,
		req.Phone,
		req.ClientName,
		nextPosition,
		estimatedWaitMinutes,
		"waiting",
		now,
		now,
	)

	if err != nil {
		log.Println("Erreur insertion queue_entries:", err)
		http.Error(w, `Impossible de rejoindre la file`, http.StatusInternalServerError)
		return
	}

	// 10. TODO : Envoyer SMS de confirmation (à implémenter plus tard)
	// sendSMS(req.Phone, fmt.Sprintf("Vous êtes en position %d. Temps d'attente: ~%d min", nextPosition, estimatedWaitMinutes))

	// 11. Réponse succès
	response := models.JoinQueueResponse{
		Message: "Vous avez été ajouté à la file d'attente",
		Entry: models.QueueEntry{
			ID:                entryID,
			BusinessID:        req.BusinessID,
			Phone:             req.Phone,
			ClientName:        req.ClientName,
			Position:          nextPosition,
			EstimatedWaitTime: estimatedWaitMinutes,
			Status:            "waiting",
			CreatedAt:         now,
		},
	}

	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(response)
}
