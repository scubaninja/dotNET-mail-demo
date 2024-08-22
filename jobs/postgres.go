//go:build mage
// +build mage

package main

import (
	"database/sql"
	"errors"
	"fmt"
	"os"

	_ "github.com/lib/pq"

	"github.com/magefile/mage/mg"
)

type Postgres mg.Namespace

// Sql executes a sql query against the database using database/sql and lib/pq
func (Postgres) Sql(query string) error {
	pgHost := os.Getenv("PGHOST")
	pgDatabase := os.Getenv("PGDATABASE")
	pgUser := os.Getenv("PGUSER")
	pgPassword := os.Getenv("PGPASSWORD")

	if pgHost == "" ||
		pgDatabase == "" ||
		pgUser == "" ||
		pgPassword == "" {
		return errors.New("PGHOST, PGDATABASE, PGUSER, and PGPASSWORD environment variables must be set")
	}

	if query == "" {
		query = "SELECT row_to_json(t) FROM (SELECT * FROM pg_stat_activity) t"
	}

	// connect to database
	dbURL := fmt.Sprintf("host=%s dbname=%s user=%s password=%s sslmode=require", pgHost, pgDatabase, pgUser, pgPassword)
	db, err := sql.Open("postgres", dbURL)
	if err != nil {
		return err
	}
	defer db.Close()

	// execute query
	rows, err := db.Query(query)
	if err != nil {
		return err
	}
	defer rows.Close()

	for rows.Next() {
		var json string
		err = rows.Scan(&json)
		if err != nil {
			return err
		}
		fmt.Println(json)
	}
	return nil
}
