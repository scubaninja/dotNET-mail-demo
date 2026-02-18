package api

import "net/http"

// HandleAbout returns information about the API.
// Mirrors C# PublicRoutes GET /about endpoint.
func HandleAbout(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "text/plain")
	w.Write([]byte("Tailwind Traders Mail Services API"))
}
