package alternateValidationExamples

import "fmt"

type Name struct {
	Value string
}

type Age struct {
	Value uint
}

func newName(s string) (*Name, error) {
	if len(s) > 5 {
		return nil, fmt.Errorf("name too long")
	}
	return &Name{Value: s}, nil
}

func newAge(n uint) (*Age, error) {
	if n > 100 {
		return nil, fmt.Errorf("too old")
	}
	return &Age{Value: n}, nil
}

func validate[T any](errors *[]error, fn func() (T, error)) T {
	v, err := fn()
	if err != nil {
		*errors = append(*errors, err)
	}
	return v
}

func collectErrors(errs ...error) []error {
	out := []error{}
	for _, e := range errs {
		if e != nil {
			out = append(out, e)
		}
	}
	return out
}

func Run() {
	{
		// Preferred if errors are to be transformed before passed through to higher layer.

		errs := []error{}
		a, e := newName("Ronnie")
		if e != nil {
			// Convert into new error instance holding input field name for UI correlation.
			errs = append(errs, e)
		}

		b, e := newAge(90)
		if e != nil {
			errs = append(errs, e)
		}

		fmt.Printf("%v\n", a)
		fmt.Printf("%v\n", b)
		fmt.Printf("%v\n", errs)
	}

	fmt.Println("----------------")

	{
		// Preferred if errors to be passed through to higher layer as is.

		a, e1 := newName("Ronnie")
		b, e2 := newAge(90)

		fmt.Printf("%v, %v\n", a, e1)
		fmt.Printf("%v, %v\n", b, e2)

		errs := collectErrors(e1, e2)
		if len(errs) > 0 {
			fmt.Printf("errors occurred: %v\n", errs)
		}
	}
	fmt.Println("----------------")

	{
		// Guarantees errors are collection, but is syntactically clumsy.

		var errs = []error{}
		a := validate(&errs, func() (*Name, error) { return newName("Bob") })
		b := validate(&errs, func() (*Age, error) { return newAge(101) })
		fmt.Printf("%v\n", a)
		fmt.Printf("%v\n", b)
		fmt.Printf("%v\n", errs)
	}
}
