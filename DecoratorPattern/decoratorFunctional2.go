package main

import (
	"fmt"
	"log"
	"math"
	"os"
	"sync"
	"time"
)

// Go Design Patterns - The Decorator Pattern - Part One: https://www.youtube.com/watch?v=F365lY5ECGY&list=PLJbE2Yu2zumAKLbWO3E2vKXDlQ8LT_R28

type piFunc func(int) float64

// Some decorator implementations make next call inside the decorator. How would
// this be implemented? Is the fn argument what would be called next?

func withLogger(fn piFunc, logger *log.Logger) piFunc {
	return func(n int) (result float64) {
		defer func(t time.Time) {
			logger.Printf("took=%v, n=%v, result=%v", time.Since(t), n, result)
		}(time.Now())
		return fn(n)
	}
}

func withCache(fn piFunc, cache *sync.Map) piFunc {
	return func(n int) float64 {
		key := fmt.Sprintf("%d", n)
		val, ok := cache.Load(key)
		if ok {
			return val.(float64)
		}
		result := fn(n)
		cache.Store(key, result)
		return result
	}
}

func pi(n int) float64 {
	// panic to display the onion structure
	//panic("stop")

	ch := make(chan float64)

	for k := 0; k <= n; k++ {
		// This use of gofuncs is silly and over-complicated but illustrates an
		// expensive calculation.
		go func(ch chan float64, k float64) {
			ch <- 4 * math.Pow(-1, k) / (2*k + 1)
		}(ch, float64(k))
	}

	result := 0.0
	for k := 0; k < n; k++ {
		result += <-ch
	}

	return result
}

func divide(n int) float64 {
	return float64(n / 2)
}

func main() {
	// Consider using Adapt function from
	// https://medium.com/@matryer/writing-middleware-in-golang-and-how-go-makes-it-so-much-fun-4375c1246e81
	// to compose the onion.

	// Onion structure: logger(cache(pi(n)))
	f := withCache(pi, &sync.Map{})
	//f := wrapCache(divide, &sync.Map{})
	g := withLogger(f, log.New(os.Stdout, "test ", 1))
	g(100000)
	g(100000)
}
