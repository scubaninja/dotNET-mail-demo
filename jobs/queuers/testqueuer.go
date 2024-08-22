package queuers

import (
	"math/rand"
	"os"
	"time"

	"log/slog"
)

// TestQueuer receiver is a local queue
type TestQueuer struct {
	logger *slog.Logger
}

func NewTestQueuer() TestQueuer {
	t := TestQueuer{}
	t.logger = slog.New(slog.NewJSONHandler(os.Stdout, nil))
	return t
}

func (t TestQueuer) Send(map1 map[string]interface{}) error {
	t.logger.Info("TestQueuer message sent", "id", map1["id"])
	return nil
}

func (t TestQueuer) Receive() (map[string]interface{}, error) {
	map1 := map[string]interface{}{}
	// generate a random number between 1 and 1000
	// and assign it to the key "id"
	map1["id"] = rand.Intn(1000)
	t.logger.Info("TestQueuer message received", "id", map1["id"])
	t.logger.Info("TestQueuer pause for 1 second")
	time.Sleep(1 * time.Second)
	return map1, nil
}
