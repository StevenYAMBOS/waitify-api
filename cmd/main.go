package main

import (
	"fmt"
	"log"
	"net/http"

	"github.com/StevenYAMBOS/waitify-api/internal/config"
	"github.com/StevenYAMBOS/waitify-api/internal/database"
	"github.com/StevenYAMBOS/waitify-api/internal/handlers"
	"github.com/StevenYAMBOS/waitify-api/internal/middlewares"
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

	// Routeur
	r := http.NewServeMux()

	// Health check
	r.HandleFunc("/", handlers.HealthCheck)
	// Routes d'authentification
	r.HandleFunc("POST /auth/register", middlewares.CORSMiddleware(handlers.RegisterHandler))
	r.HandleFunc("POST /auth/login", middlewares.CORSMiddleware(handlers.LoginHandler))
	// Route utilisateur
	r.HandleFunc("GET /admin/profile", middlewares.CORSMiddleware(middlewares.AuthMiddleware(handlers.ProfileHandler)))

	fmt.Print("[main.go] -> Serveur lançé : http://localhost:", port)
	http.ListenAndServe(port, r)
}
