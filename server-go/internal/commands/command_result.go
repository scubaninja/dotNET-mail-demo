package commands

// CommandResult represents the result of executing a command.
// Mirrors C# CommandResult class from Data/CommandResult.cs.
type CommandResult struct {
	Data     interface{} `json:"data,omitempty"`
	Inserted int         `json:"inserted"`
	Updated  int         `json:"updated"`
	Deleted  int         `json:"deleted"`
}
