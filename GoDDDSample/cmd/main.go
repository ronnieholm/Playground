package main

import (
	"encoding/json"
	"fmt"

	"example.com/m/application/products"
	"example.com/m/infrastructure"
)

type Product struct {
	Id string `json:"id"`
}

type ProductJson products.ProductDto

func (p ProductJson) MarshallJson() ([]byte, error) {
	return []byte{}, nil
}

func main() {
	q := products.GetProductByIdQuery{Id: "123", Scopes: nil, ProductInformation: infrastructure.StiboDaaSClient{}}
	p, err := q.Run()
	if err != nil {
		panic(err)
	}

	s, _ := json.MarshalIndent(p, "", "  ")
	t := string(s)
	fmt.Printf("%s\n", t)
	fmt.Printf("%s\n", p.ExternalId)
}
