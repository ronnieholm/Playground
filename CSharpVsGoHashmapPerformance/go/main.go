package main

import (
	"fmt"
	"math/rand"
	"time"
)

func main() {
	d := make(map[int]int)

	t1 := time.Now()
	for i := 0; i < 10_000_000; i++ {
		d[i] = 0
	}
	t2 := time.Now()

	c := 0
	t3 := time.Now()
	for i := 0; i < 10_000_000; i++ {
		_, ok := d[rand.Intn(100_000_000)]
		if ok {
			c = c + 1
		}
	}
	t4 := time.Now()

	fmt.Printf("%d %d %d\n", t2.Sub(t1).Milliseconds(), t4.Sub(t3).Milliseconds(), c)
}
