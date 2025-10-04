package database

import (
	"database/sql"
	"fmt"
	"log"
	"os"

	// "os"
	"time"

	_ "github.com/lib/pq"
)

var DB *sql.DB

func InitDB() {

	connectionString := fmt.Sprintf("%s", os.Getenv("DB_URL"))

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
