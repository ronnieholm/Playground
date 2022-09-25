package product

import (
	"errors"

	"example.com/m/domain"
)

type ExternalProductId struct {
	// What's that value in struct embedding ValueObject? We should just "type string ExternalProductId" in this case
	domain.ValueObject
	value string
}

func NewExternalProductId(id string) (ExternalProductId, error) {
	if len(id) > 10 {
		return ExternalProductId{}, errors.New("string too long")
	}
	return ExternalProductId{value: id}, nil
}

func (v ExternalProductId) Value() string                       { return v.value }
func (v ExternalProductId) Equals(other ExternalProductId) bool { return v.Value() == other.Value() }

type Scope struct {
	value string
}

func NewScope(scope string) (Scope, error) {
	if scope != "foo" {
		return Scope{}, errors.New("invalid scope")
	}
	return Scope{value: scope}, nil
}

func (v Scope) Value() string           { return v.value }
func (v Scope) Equals(other Scope) bool { return v.Value() == other.Value() }

type Product struct {
	domain.AggregateRoot
	externalId ExternalProductId
	scopes     []Scope
}

func NewProduct(externalId ExternalProductId, scopes []Scope) (Product, []error) {
	return Product{
		externalId: externalId,
		scopes:     scopes,
	}, nil
}

func (p Product) ExternalId() ExternalProductId { return p.externalId }
func (p Product) Scopes() []Scope               { return p.scopes }
func (p Product) Equals(other Product) bool     { return p.Id() == other.Id() }
