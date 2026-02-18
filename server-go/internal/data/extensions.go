package data

import (
	"strings"
	"unicode"
)

// ToSnakeCase converts a PascalCase or camelCase string to snake_case.
// This mirrors the C# StringExtensions.ToSnakeCase() method from Data/Extensions.cs.
func ToSnakeCase(s string) string {
	if s == "ID" {
		return "id"
	}
	if len(s) < 2 {
		return strings.ToLower(s)
	}
	var result strings.Builder
	for i, r := range s {
		if unicode.IsUpper(r) {
			if i > 0 {
				result.WriteByte('_')
			}
			result.WriteRune(unicode.ToLower(r))
		} else {
			result.WriteRune(r)
		}
	}
	return result.String()
}
