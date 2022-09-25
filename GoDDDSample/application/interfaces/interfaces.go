package interfaces

import "example.com/m/domain/product"

type ProductInformation interface {
	GetProductIds() ([]product.ExternalProductId, []error)
	GetProductById(id product.ExternalProductId, scopes []product.Scope) (product.Product, []error)
}
