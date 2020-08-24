package main

import "math"

// Color as RGB value
type Color struct {
	R, G, B float64
}

var (
	// White represents maximum intensity
	White = Color{1.0, 1.0, 1.0}
	// Black represents minimum intensity
	Black = Color{}
)

// Scale scales the Color by a value (return a new Color)
func (c Color) Scale(t float64) Color {
	return Color{R: c.R * t, G: c.G * t, B: c.B * t}
}

// Mult multiplies two colors together (component by component multiplication, return a new Color)
func (c Color) Mult(c2 Color) Color {
	return Color{R: c.R * c2.R, G: c.G * c2.G, B: c.B * c2.B}
}

// Add adds the two colors (return a new Color)
func (c Color) Add(c2 Color) Color {
	return Color{R: c.R + c2.R, G: c.G + c2.G, B: c.B + c2.B}
}

// PixelValue converts a Color into a pixel value (0-255) packed into a uint32
func (c Color) PixelValue() uint32 {
	r := uint32(math.Min(255.0, c.R*255.99))
	g := uint32(math.Min(255.0, c.G*255.99))
	b := uint32(math.Min(255.0, c.B*255.99))
	return ((r & 0xFF) << 16) | ((g & 0xFF) << 8) | (b & 0xFF)
}
