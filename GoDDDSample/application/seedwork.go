package application

import "example.com/m/domain/product"

func CreateScopes(scopeStrings []string, errors []error) []product.Scope {
	scopes := make([]product.Scope, len(scopeStrings))
	for _, scope := range scopeStrings {
		if v, err := product.NewScope(scope); err != nil {
			errors = append(errors, err)
		} else {
			scopes = append(scopes, v)
		}
	}
	return scopes
}
