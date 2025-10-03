package database

import (
	"database/sql"
	"fmt"
	"log"

	// "os"
	"time"

	_ "github.com/lib/pq"
)

var DB *sql.DB

func InitDB() {
	// dbHost := os.Getenv("DB_HOST")
	// dbPort := os.Getenv("DB_PORT")
	// dbUser := os.Getenv("DB_USER")
	// dbPassword := os.Getenv("DB_PASSWORD")
	// dbName := os.Getenv("DB_NAME")

	connectionString := fmt.Sprintf("postgres://postgres:postgres@localhost:5433/waitifydb?sslmode=disable")
	// connectionString := fmt.Sprintf("host=%s port=%s user=%s password=%s dbname=%s sslmode=disable",
	// 	dbHost, dbPort, dbUser, dbPassword, dbName)

	var err error
	DB, err = sql.Open("postgres", connectionString)
	if err != nil {
		log.Fatal(`[database.go -> InitDB()] Erreur lors de la connexion à la base de données : `, err)
	}

	// Configuration du pool de connexion
	DB.SetMaxOpenConns(25)
	DB.SetMaxIdleConns(5)
	DB.SetConnMaxLifetime(5 * time.Minute)

	err = DB.Ping()
	if err != nil {
		log.Fatal(`[database.go -> InitDB()] Erreur lors de la tentative de ping à la base de données : `, err)
	}

	log.Println(`[database.go -> InitDB()] Connexion à la base de données établie !`)
}
