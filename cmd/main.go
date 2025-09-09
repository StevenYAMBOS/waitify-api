package main

import (
	"fmt"
	"io"
	"log"
	"net/http"
	"os"

	"github.com/joho/godotenv"
)

func HelloWorld(w http.ResponseWriter, r *http.Request) {
	fmt.Printf("got /hello request\n")
	io.WriteString(w, "Hello, HTTP!\n")
}

func main() {
	err := godotenv.Load()
	if err != nil {
		log.Fatal(`[main.go] -> Erreur lors du chargement des variables d'environnements.`)
	}

	port := os.Getenv("PORT")
	// database.InitDB()

	r := http.NewServeMux()
	r.HandleFunc("/", HelloWorld)

	fmt.Print("[main.go] -> Serveur lançé : http://localhost", port)
	http.ListenAndServe(port, nil)
}
