package main

import (
	"fmt"
	"io"
	"log"
	"net/http"

	"github.com/StevenYAMBOS/waitify-api/internal/config"
	"github.com/StevenYAMBOS/waitify-api/internal/database"
)

func HelloWorld(w http.ResponseWriter, r *http.Request) {
	fmt.Printf("got /hello request\n")
	io.WriteString(w, "Hello, HTTP!\n")
}

func main() {
	// Variables d'environnement
	cfg, err := config.Load()
	if err != nil {
		log.Fatal(`[main.go] -> Erreur lors du chargement des variables d'environnements.`, err)
	}

	// Initialisation base de données
	database.InitDB()

	// Port
	port := cfg.Server.Port

	r := http.NewServeMux()
	r.HandleFunc("/", HelloWorld)

	fmt.Print("[main.go] -> Serveur lançé : http://localhost", port)
	http.ListenAndServe(port, r)
}
