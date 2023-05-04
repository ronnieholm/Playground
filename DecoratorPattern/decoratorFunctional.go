package main

// The Power Of Golang's Decorator Pattern: https://www.youtube.com/watch?v=GipAZwKFgoA

import (
	"fmt"
	"net/http"
)

type DB interface {
	Store(string) error
}

type Store struct{}

func (s *Store) Store(v string) error {
	fmt.Println("storing into db", v)
	return nil
}

// func myExecuteFunc(s string) {
// 	// How to "inject" access to database here?
// 	fmt.Println("my ex func", s)
// }

func myExecuteFunc(db DB) ExecuteFn {
	// whatever we return must match the siguature of ExecuteFn. Through a
	// closure, we added extra functionality on top of ExecuteFn.
	return func(s string) {
		fmt.Println("my ex func", s)
		db.Store(s)
	}
}

// func makeHTTPFunc(db DB) http.HandlerFunc {
// 	// How to "inject" access to database here?
// 	return func(w http.ResponseWriter, r *http.Request) {
// 		db.Store("some http shenanigans")
// 	}
// }

func makeHTTPFunc(db DB, next httpFunc) http.HandlerFunc {
	// How to "inject" access to database here?
	return func(w http.ResponseWriter, r *http.Request) {
		db.Store("some http shenanigans")
		if err := next(db, w, r); err != nil {
		}
	}
}

func main() {
	s := &Store{}
	Execute(myExecuteFunc(s))
	http.HandleFunc("/", makeHTTPFunc(s, nextHandlerFunc))
}

func nextHandlerFunc(db DB, w http.ResponseWriter, r *http.Request) error {
	fmt.Println("in handler")
	return nil
}

// Suppose we want multiple custom HTTPFuncs
type httpFunc func(db DB, w http.ResponseWriter, r *http.Request) error

// This "interface" is defined by a third-party library

type ExecuteFn func(string)

func Execute(fn ExecuteFn) {
	fn("Foo")
}
