package main

import (
	"fmt"
	"log"
	"net/http"

	"github.com/StevenYAMBOS/waitify-api/internal/config"
	"github.com/StevenYAMBOS/waitify-api/internal/database"
	"github.com/StevenYAMBOS/waitify-api/internal/handlers"
	"github.com/StevenYAMBOS/waitify-api/internal/middlewares"
	"github.com/StevenYAMBOS/waitify-api/internal/models"

	// "github.com/StevenYAMBOS/waitify-api/internal/models"
	"github.com/StevenYAMBOS/waitify-api/internal/utils"
)

func main() {
	// Variables d'environnement
	cfg, err := config.Load()
	if err != nil {
		log.Fatal(`[main.go] -> Erreur lors du chargement des variables d'environnements.`, err)
	}

	// Initialisation base de données
	database.InitDB()

	// Initialisation du JWT
	utils.InitJWT()

	// Port
	port := cfg.Server.Port

	// Initialisation GCP
	models.GoogleConfig()

	// Routeur
	r := http.NewServeMux()

	// Health check
	r.HandleFunc("/", handlers.HealthCheck)
	// Routes d'authentification
	r.HandleFunc("GET /auth/test", middlewares.CORSMiddleware(handlers.TestHandler))
	r.HandleFunc("GET /auth/google/login", middlewares.CORSMiddleware(handlers.GoogleLoginHandler))
	r.HandleFunc("GET /auth/google/callback", middlewares.CORSMiddleware(handlers.GoogleCallback))
	r.HandleFunc("POST /auth/register", middlewares.CORSMiddleware(handlers.RegisterHandler))
	r.HandleFunc("POST /auth/login", middlewares.CORSMiddleware(handlers.LoginHandler))

	// Routes utilisateur
	r.HandleFunc("GET /user/profile", middlewares.CORSMiddleware(middlewares.AuthMiddleware(handlers.ProfileHandler)))

	// Routes entreprises
	r.HandleFunc("GET /business/{id}", middlewares.CORSMiddleware(middlewares.AuthMiddleware(handlers.GetBusinessHandler)))
	r.HandleFunc("GET /businesses/user/{id}", middlewares.CORSMiddleware(middlewares.AuthMiddleware(handlers.GetBusinessesHandler)))
	r.HandleFunc("POST /business", middlewares.CORSMiddleware(middlewares.AuthMiddleware(handlers.AddBusinessHandler)))
	r.HandleFunc("PATCH /business/{id}", middlewares.CORSMiddleware(middlewares.AuthMiddleware(handlers.UpdateBusinessHandler)))
	r.HandleFunc("DELETE /business/{id}", middlewares.CORSMiddleware(middlewares.AuthMiddleware(handlers.DeleteBusinessHandler)))

	fmt.Print("[main.go] -> Serveur lançé : http://localhost", port)
	http.ListenAndServe(port, r)
}
