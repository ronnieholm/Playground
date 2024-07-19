package main

// Based on "Memory layout, performance and the #soa keyword in Odin"
// (https://www.youtube.com/watch?v=ghawCl8YW6E&list).

// $ go run main.go
// Typical output:
// Array of structs
// 100000000, 1.14457597s
// Struct of arrays
// 100000000, 568.798278ms

import (
	"fmt"
	"time"
)

type particle struct {
	x, y, z    float32
	vx, vy, vz float32
}

func runArrayOfStructs(count int32) {
	particles := make([]particle, count)
	for _, p := range particles {
		p.x, p.y, p.z = 1, 2, 3
		p.vx, p.vy, p.vz = 4, 5, 6
	}

	start := time.Now()
	for _, p := range particles {
		p.x += p.vx
		p.y += p.vy
		p.z += p.vz
	}
	elapsed := time.Since(start)
	fmt.Printf("%d, %v\n", count, elapsed)
}

type particleSoa struct {
	x, y, z    []float32
	vx, vy, vz []float32
}

func runStructOfArrays(count int32) {
	p := particleSoa{}
	p.x, p.y, p.z = make([]float32, count), make([]float32, count), make([]float32, count)
	p.vx, p.vy, p.vz = make([]float32, count), make([]float32, count), make([]float32, count)

	for i := 1; i < int(count); i++ {
		p.x[i], p.y[i], p.z[i] = 1, 2, 3
		p.vx[i], p.vy[i], p.vz[i] = 4, 5, 6
	}

	start := time.Now()
	for i := 1; i < int(count); i++ {
		p.x[i] += p.vx[i]
		p.y[i] += p.vy[i]
		p.z[i] += p.vz[i]
	}
	elapsed := time.Since(start)
	fmt.Printf("%d, %v\n", count, elapsed)
}

func main() {
	fmt.Println("Array of structs")
	// runArrayOfStructs(1_000_000)
	// runArrayOfStructs(10_000_000)
	runArrayOfStructs(100_000_000)

	fmt.Println("Struct of arrays")
	// runStructOfArrays(1_000_000)
	// runStructOfArrays(10_000_000)
	runStructOfArrays(100_000_000)
}
