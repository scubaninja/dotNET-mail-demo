package commands

import "fmt"

// LinkClicked handles a link click event.
// Mirrors C# LinkClickedCommand from Commands/LinkClickedCommand.cs.
func LinkClicked(key string) string {
	return fmt.Sprintf("Link clicked: %s", key)
}
