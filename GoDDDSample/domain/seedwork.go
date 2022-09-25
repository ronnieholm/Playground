package domain

import "time"

type ValueObject struct {
}

type Entity struct {
	id int
}

func (e Entity) Id() int { return e.id }

type AggregateRoot struct {
	Entity
	createdAt  time.Time
	modifiedAt time.Time
}

func (a AggregateRoot) CreatedAt() time.Time  { return a.createdAt }
func (a AggregateRoot) ModifiedAt() time.Time { return a.modifiedAt }
