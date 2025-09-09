package database

import (
	"database/sql"
	"time"

	_ "github.com/lib/pq"
)

type Database struct {
	DB *sql.DB
}

func InitDB(connectionString string) (*Database, error) {
	// Ouvre la connexion à la base de données
	db, err := sql.Open("postgres", connectionString)
	if err != nil {
		return nil, err
	}

	// Configuration du pool de connexion
	db.SetMaxOpenConns(25)
	db.SetMaxIdleConns(5)
	db.SetConnMaxLifetime(5 * time.Minute)

	// Vérifier que la connexion a fonctionné
	if err := db.Ping(); err != nil {
		return nil, err
	}

	return &Database{DB: db}, nil
}
