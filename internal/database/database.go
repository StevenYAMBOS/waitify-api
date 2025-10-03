package database

import (
	"context"
	"database/sql"
	"fmt"
	"log"
	"os"

	"github.com/jackc/pgx/v5"
	"github.com/joho/godotenv"
)

var DB *sql.DB

func InitDB() {
	err := godotenv.Load()
	if err != nil {
		log.Fatal("Error loading .env file")
	}
	connStr := os.Getenv("DATABASE_URL")
	conn, err := pgx.Connect(context.Background(), connStr)
	if err != nil {
		panic(err)
	}
	defer conn.Close(context.Background())
	_, err = conn.Exec(context.Background(), "CREATE TABLE IF NOT EXISTS playing_with_neon(id SERIAL PRIMARY KEY, name TEXT NOT NULL, value REAL);")
	if err != nil {
		panic(err)
	}
	_, err = conn.Exec(context.Background(), "INSERT INTO playing_with_neon(name, value) SELECT LEFT(md5(i::TEXT), 10), random() FROM generate_series(1, 10) s(i);")
	if err != nil {
		panic(err)
	}
	rows, err := conn.Query(context.Background(), "SELECT * FROM playing_with_neon")
	if err != nil {
		panic(err)
	}
	defer rows.Close()
	for rows.Next() {
		var id int32
		var name string
		var value float32
		if err := rows.Scan(&id, &name, &value); err != nil {
			panic(err)
		}
		fmt.Printf("%d | %s | %f\n", id, name, value)
	}
}

/*
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
*/
