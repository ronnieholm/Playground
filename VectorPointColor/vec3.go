package main

import "math"

type Vec3 struct {
	X, Y, Z float64
}

// Scale scales the vector by the value (return a new Vec3)
func (v Vec3) Scale(t float64) Vec3 {
	return Vec3{X: v.X * t, Y: v.Y * t, Z: v.Z * t}
}

// Mult multiplies the vector by the other one (return a new Vec3)
func (v Vec3) Mult(v2 Vec3) Vec3 {
	return Vec3{X: v.X * v2.X, Y: v.Y * v2.Y, Z: v.Z * v2.Z}
}

// Sub substracts the 2 vectors (return a new Vec3)
func (v Vec3) Sub(v2 Vec3) Vec3 {
	return Vec3{X: v.X - v2.X, Y: v.Y - v2.Y, Z: v.Z - v2.Z}
}

// Add adds the 2 vectors (return a new Vec3)
func (v Vec3) Add(v2 Vec3) Vec3 {
	return Vec3{X: v.X + v2.X, Y: v.Y + v2.Y, Z: v.Z + v2.Z}
}

// Length returns the size of the Vec3
func (v Vec3) Length() float64 {
	return math.Sqrt(v.X*v.X + v.Y*v.Y + v.Z*v.Z)
}

// Unit returns a new Vec3 with same direction and length 1
func (v Vec3) Unit() Vec3 {
	return v.Scale(1.0 / v.Length())
}

// Negate returns a new Vec3 with X/Y/Z negated
func (v Vec3) Negate() Vec3 {
	return Vec3{-v.X, -v.Y, -v.Z}
}

// Dot returns the dot product (a scalar) of two Vec3s
func Dot(v1 Vec3, v2 Vec3) float64 {
	return v1.X*v2.X + v1.Y*v2.Y + v1.Z*v2.Z
}

// Cross returns the cross product of two vectors (another new Vec3)
func Cross(v1 Vec3, v2 Vec3) Vec3 {
	return Vec3{v1.Y*v2.Z - v1.Z*v2.Y, -(v1.X*v2.Z - v1.Z*v2.X), v1.X*v2.Y - v1.Y*v2.X}
}

// Reflect simply reflects the Vec3 based on the normal n
func (v Vec3) Reflect(n Vec3) Vec3 {
	return v.Sub(n.Scale(2.0 * Dot(v, n)))
}
