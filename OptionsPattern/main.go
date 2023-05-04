package main

import "fmt"

// The Most Efficient Struct Configuration Pattern For Golang:
// https://www.youtube.com/watch?v=MDy7JQN5MN4.

// A combination of decorator and builder patterns.

// Instead of only storing each withX argument value in a struct field and then
// call a build() function in the end, here each withX function sets the value
// directly. The traditional builder approach requires defining a separate
// builder type.

type OptFunc func(*Opts)

type Opts struct {
	maxConn int
	id      string
	tls     bool
}

func defaultOpts() Opts {
	return Opts{
		maxConn: 10,
		id:      "default",
		tls:     false,
	}
}

// withX function which does provide an argument for the Opts field.
//
// In FP, this approach would be called partial function application.
// withMaxConn takes arguments of type int and Opts, and calling withMaxConn
// with a single argument results in a closure over n and the return of a new
// function of a single argument of type Opts.
func withMaxConn(n int) OptFunc {
	return func(opts *Opts) {
		opts.maxConn = n
	}
}

func withID(id string) OptFunc {
	return func(opts *Opts) {
		opts.id = id
	}
}

// withX function which doesn't provide an argument value for the Opts field.
func withTLS(opts *Opts) {
	opts.tls = true
}

type Server struct {
	Opts
}

// allows for zero, one, or multiple options.
func newServer(opts ...OptFunc) *Server {
	o := defaultOpts()

	for _, fn := range opts {
		fn(&o)
	}

	return &Server{
		Opts: o,
	}
}

func main() {
	s1 := newServer()
	fmt.Printf("%+v\n", s1)

	s2 := newServer(withTLS, withMaxConn(99), withID("foo"))
	fmt.Printf("%+v\n", s2)
}
