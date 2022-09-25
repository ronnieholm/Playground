package products

import (
	"example.com/m/application"
	"example.com/m/application/interfaces"
	"example.com/m/domain/product"
)

type ProductDto struct {
	Id         int
	ExternalId string
}

func MapProduct(p product.Product) ProductDto {
	return ProductDto{
		Id:         p.Id(),
		ExternalId: p.ExternalId().Value()}
}

type GetProductByIdQuery struct {
	Id     string
	Scopes []string

	ProductInformation interfaces.ProductInformation
}

func (q GetProductByIdQuery) Run() (ProductDto, []error) {
	errors := []error{}

	externalId := product.ExternalProductId{}
	if v, err := product.NewExternalProductId(q.Id); err != nil {
		errors = append(errors, err)
	} else {
		externalId = v
	}

	scopes := application.CreateScopes(q.Scopes, errors)

	if len(errors) > 0 {
		return ProductDto{}, errors
	}

	if p, err := q.ProductInformation.GetProductById(externalId, scopes); err != nil {
		return ProductDto{}, err
	} else {
		return MapProduct(p), nil
	}
}
