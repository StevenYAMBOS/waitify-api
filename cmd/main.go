package main

import (
	"fmt"
	"log"
	"net/http"
	"os"

	"github.com/StevenYAMBOS/waitify-api/internal/database"
	"github.com/go-chi/chi/v5"
	"github.com/go-chi/chi/v5/middleware"
	"github.com/joho/godotenv"
)

func main() {
	err := godotenv.Load()
	if err != nil {
		log.Fatal(`[main.go] -> Erreur lors du chargement des variables d'environnements.`)
	}

	port := os.Getenv("PORT")
	database.InitDB()

	r := chi.NewRouter()
	r.Use(middleware.Logger)
	r.Get("/", func(w http.ResponseWriter, r *http.Request) {
		w.Write([]byte("Health check !"))
	})
	fmt.Print("[main.go] -> Serveur lançé : http://localhost", port)
	http.ListenAndServe(port, r)
}
