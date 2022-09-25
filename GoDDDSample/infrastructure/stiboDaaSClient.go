package infrastructure

import (
	"example.com/m/domain/product"
)

type StiboDaaSClient struct {
}

func (StiboDaaSClient) GetProductIds() ([]product.ExternalProductId, []error) {
	return nil, nil
}

func (StiboDaaSClient) GetProductById(id product.ExternalProductId, scopes []product.Scope) (product.Product, []error) {
	idX, err := product.NewExternalProductId("42")
	if err != nil {
		panic("")
	}
	if p, err := product.NewProduct(idX, scopes); err != nil {
		return product.Product{}, err
	} else {
		return p, nil
	}
}
