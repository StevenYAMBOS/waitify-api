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

// Récupérer les informations d'une entreprise
func GetBusinessHandler(w http.ResponseWriter, r *http.Request) {
	// Réponse JSON
	w.Header().Set("Content-Type", "application/json")

	// Méthode HTTP
	if r.Method != http.MethodGet {
		http.Error(w, `Mauvaise requête HTTP.`, http.StatusBadRequest)
	}

	// Décode JSON de la requête
	var business models.Business

	// Récupérer l'ID de l'entreprise depuis l'URL
	IDParam := r.PathValue("id")

	// Récupération dans la base de données
	err := database.DB.QueryRow(`
		SELECT id, UserId, name, business_type, phone_number, address, city, zip_code, country, created_at, updated_at
		FROM businesses WHERE id = $1
`, IDParam).Scan(
		&business.ID,
		&business.UserID,
		&business.Name,
		&business.BusinessType,
		&business.PhoneNumber,
		&business.Address,
		&business.City,
		&business.ZipCode,
		&business.Country,
		&business.CreatedAt,
		&business.UpdatedAt,
	)
	if err != nil {
		log.Println(`Erreur lors de la récupération des informations de l'entreprise : `, err)
		http.Error(w, "Erreur lors de la récupération de l'entreprise : "+err.Error(), http.StatusInternalServerError)
		return
	}

	response := models.AddBusinessResponse{
		Response: "Informations de l'entreprise récupérées avec succès.",
		Business: business,
	}

	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(response)
}

// Récupérer toutes les entreprises d'un utilisateur
func GetBusinessesHandler(w http.ResponseWriter, r *http.Request) {
	// Réponse JSON
	w.Header().Set("Content-Type", "application/json")

	// Méthode HTTP
	if r.Method != http.MethodGet {
		log.Println(`Mauvaise requête HTTP.`)
		http.Error(w, `Mauvaise requête HTTP.`, http.StatusBadRequest)
	}

	// Récupérer l'ID de l'utilisateur depuis l'URL
	IDParam := r.PathValue("id")

	// Récupération dans la base de données
	rows, err := database.DB.Query("SELECT id, UserId, name, business_type, phone_number, address, city, zip_code, country, created_at, updated_at FROM businesses WHERE UserId=$1", IDParam)
	if err != nil {
		log.Println(`Erreur lors de la récupération des entreprises de l'utilisateur : `, err)
		http.Error(w, `Erreur lors de la récupération des entreprises de l'utilisateur : `+err.Error(), http.StatusBadRequest)
	}
	defer rows.Close()

	businesses := []models.Business{}

	for rows.Next() {
		var business models.Business
		if err := rows.Scan(&business.ID,
			&business.UserID,
			&business.Name,
			&business.BusinessType,
			&business.PhoneNumber,
			&business.Address,
			&business.City,
			&business.ZipCode,
			&business.Country,
			&business.CreatedAt,
			&business.UpdatedAt,
		); err != nil {
			log.Println(`Erreur lors du scan : `, err)
			log.Fatal(err)
		}
		businesses = append(businesses, business)
	}
	if err := rows.Err(); err != nil {
		log.Println(`Erreur après le scan : `, err)
		log.Fatal(err)
	}

	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(businesses)
}

// Créer une entreprise
func AddBusinessHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, `[businessHandler.go -> AddBusinessHandler()] -> Mauvaise requête HTTP (mauvaise méthode).`, http.StatusBadRequest)
	}

	w.Header().Set("Content-Type", "application/json")

	// Décode JSON de la requête
	var business models.Business
	if err := json.NewDecoder(r.Body).Decode(&business); err != nil {
		log.Println(`[businessHandler.go -> AddBusinessHandler()] -> Mauvais corps de requête : `, err)
		http.Error(w, `[businessHandler.go -> AddBusinessHandler()] -> Mauvais corps de requête.`, http.StatusBadRequest)
		return
	}

	// Validation nom de l'entreprise
	if business.Name == "" {
		http.Error(w, `[businessHandler.go -> AddBusinessHandler()] -> Le nom de l'entreprise doit avoir au moins 1 caractère.`, http.StatusBadRequest)
		return
	}

	if err := business.ValidatePhoneNumber(); err != nil {
		log.Println("[businessHandler.go -> AddBusinessHandler()] -> Erreur format numéro de téléphone : ", err)
		http.Error(w, `[businessHandler.go -> AddBusinessHandler()] -> Erreur format de l'email.`, http.StatusBadRequest)
		return
	}

	// Validation du type
	if err := business.ValidateBusinessType(); err != nil {
		http.Error(w, `[businessHandler.go -> AddBusinessHandler()] -> Erreur format du type de commerce : `+err.Error()+"Requête reçue: "+business.BusinessType, http.StatusBadRequest)
		return
	}

	// Validation de l'adresse
	if business.Address == "" || len(business.Address) >= 100 {
		http.Error(w, `[businessHandler.go -> AddBusinessHandler()] -> L'adresse de l'entreprise doit être comprise 1 et 100 caractères.`, http.StatusBadRequest)
		return
	}

	// Validation de la ville
	if business.City == "" || len(business.City) >= 100 {
		http.Error(w, `[businessHandler.go -> AddBusinessHandler()] -> La ville de l'entreprise doit être comprise 1 et 100 caractères.`, http.StatusBadRequest)
		return
	}

	// Validation nom de l'entreprise
	if business.ZipCode == "" || len(business.ZipCode) >= 100 {
		http.Error(w, `[businessHandler.go -> AddBusinessHandler()] -> Le code postal de l'entreprise doit être comprise 1 et 100 caractères.`, http.StatusBadRequest)
		return
	}

	// Vérifier si l'utilisateur existe
	var exists bool
	err := database.DB.QueryRow("SELECT EXISTS(SELECT 1 FROM users WHERE id = $1)",
		business.UserID).Scan(&exists)
	if err != nil {
		http.Error(w, `[businessHandler.go -> AddBusinessHandler()] -> Erreur vérification si l'utilisateur existe : `+err.Error(), http.StatusInternalServerError)
		return
	}
	if !exists {
		log.Println("L'utilisateur n'existe pas : ", err)
		http.Error(w, `[businessHandler.go -> AddBusinessHandler()] -> ERREUR. L'utilisateur n'existe pas ! `+err.Error(), http.StatusConflict)
		return
	}

	// Insertion dans la base de données
	err = database.DB.QueryRow(
		"INSERT INTO businesses (id, UserId, name, business_type, phone_number, address, city, zip_code, country, created_at, updated_at) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11) RETURNING id, UserId, name, business_type, phone_number, address, city, zip_code, country, created_at, updated_at",
		uuid.New().String(), business.UserID, business.Name, business.BusinessType, business.PhoneNumber, business.Address, business.City, business.ZipCode, business.Country, time.Now(), time.Now(),
	).Scan(&business.ID, &business.UserID, &business.Name, &business.BusinessType, &business.PhoneNumber, &business.Address, &business.City, &business.ZipCode, &business.Country, &business.CreatedAt, &business.UpdatedAt)

	if err != nil {
		http.Error(w, "[businessHandler.go -> AddBusinessHandler()] -> Erreur lors de la création de l'entreprise : "+err.Error(), http.StatusInternalServerError)
		return
	}

	response := models.AddBusinessResponse{
		Response: "L'entreprise a été créée avec succès.",
		Business: business,
	}

	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(response)
}

// Mettre à jour l'entreprise
func UpdateBusinessHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPut {
		http.Error(w, `Mauvaise requête HTTP (mauvaise méthode).`, http.StatusBadRequest)
	}

	w.Header().Set("Content-Type", "application/json")

	// Décode JSON de la requête
	var business *models.UpdatedBusiness

	if err := json.NewDecoder(r.Body).Decode(&business); err != nil {
		log.Println(`Mauvais corps de requête : `, err)
		http.Error(w, `Mauvais corps de requête.`, http.StatusBadRequest)
		return
	}

	// Récupérer l'ID de l'entreprise depuis l'URL
	IDParam := r.PathValue("id")
	log.Println(`ID : `, IDParam)

	// Vérifier si l'entreprise existe
	var businessExists bool
	errBusinesses := database.DB.QueryRow("SELECT EXISTS(SELECT 1 FROM businesses WHERE id = $1)",
		IDParam).Scan(&businessExists)
	if errBusinesses != nil {
		http.Error(w, `Erreur vérification de l'existance de l'entreprise : `+errBusinesses.Error(), http.StatusInternalServerError)
		return
	}
	if !businessExists {
		log.Println("L'entreprise avec cet 'id' n'existe pas : ", errBusinesses)
		http.Error(w, `ERREUR. L'entreprise n'existe pas ! `+errBusinesses.Error(), http.StatusConflict)
		return
	}

	// Récupération dans la base de données
	/*
		err := database.DB.QueryRow(`
						SELECT id, UserId, name, business_type, phone_number, address, city, zip_code, country, created_at, updated_at
						FROM businesses WHERE id = $1
				`, IDParam).Scan(
			&business.ID,
			&business.UserID,
			&business.Name,
			&business.BusinessType,
			&business.PhoneNumber,
			&business.Address,
			&business.City,
			&business.ZipCode,
			&business.Country,
			&business.CreatedAt,
			&business.UpdatedAt,
		)
		if err != nil {
			log.Println(`Erreur lors de la récupération des informations de l'entreprise : `, err)
			http.Error(w, "Erreur lors de la récupération de l'entreprise : "+err.Error(), http.StatusInternalServerError)
			return
		}

		// Vérification des champs
			updatedFields := make(map[string]string)
			if business.Name != "" {
				updatedFields["name"] = business.Name
			}
			if business.BusinessType != "" {
				updatedFields["business_type"] = business.BusinessType
			}
			if business.PhoneNumber != "" {
				updatedFields["phone_number"] = business.PhoneNumber
			}
			if business.Address != "" {
				updatedFields["address"] = business.Address
			}
			if business.City != "" {
				updatedFields["city"] = business.City
			}
			if business.ZipCode != "" {
				updatedFields["zip_code"] = business.ZipCode
			}
			if business.Country != "" {
				updatedFields["country"] = business.Country
			}
			updatedFields["updated_at"] = time.Now()
	*/

	// Insertion dans la base de données
	updt, err := database.DB.Exec(`UPDATE businesses SET name=$2, business_type=$3, phone_number=$4, address=$5, city=$6, zip_code=$7, country=$8, updated_at=$9 WHERE id=$1 RETURNING *;`, IDParam, &business.Name, &business.BusinessType, &business.PhoneNumber, &business.Address, &business.City, &business.ZipCode, &business.Country, time.Now())
	if err != nil {
		http.Error(w, "Erreur lors de la création de l'entreprise : "+err.Error(), http.StatusInternalServerError)
		return
	}

	// check how many rows affected
	rowsAffected, err := updt.RowsAffected()

	if err != nil {
		log.Fatalf("Error while checking the affected rows. %v", err)
	}

	log.Println(`Nombre de lignes modifiées : `, rowsAffected)
	log.Println(`Informations : `, business)

	response := models.UpdateBusinessResponse{
		Response: "L'entreprise a été modifiée avec succès.",
		Business: business,
	}

	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(response)
}
