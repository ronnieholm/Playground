package main

// Improvements:
// [ ] Turn line cross path finder into a type which implements an interface type. That way we can
//     define different implementations for analytical and numeric solutions, inclusing sinus solution.
// [ ] Handle wrap-around in x component
// [ ] Use Chaney's error package
// [ ] Follow guideline for organizing projects: https://github.com/golang-standards/project-layout
// [ ] Implement console UI with https://github.com/rivo/tview
// [ ] Implement browser UI with web sockets
// [ ] Implement browser UI with web assembly once Go 1.11 ships
// [ ] Numerically find intersection between point and line using vectors and dot product over estimation method:
//     https://www.youtube.com/watch?v=0lG53-ogF2k

import (
	"fmt"
	"image"
	"image/png"
	"log"
	"math"
	"os"
	"strconv"
	"time"

	"github.com/fogleman/gg"
)

type point struct {
	x, y float64
}

type line struct {
	a, b point
}

type fenestration struct {
	center point
	radius float64
}

type model struct {
	debug         bool
	xAxisTicks    []float64
	yAxisTicks    []float64
	gridLines     bool
	xMm           float64
	yMm           float64
	xPx           int
	yPx           int
	sews          [3]point
	sewsYOffsets  []float64
	fenestrations []fenestration
}

// padding to allow for x and y axis with ticks and legends
const paddingXPixels = 125
const paddingYPixels = 125

func (m model) draw() (image.Image, error) {
	dc := gg.NewContext(m.xPx+paddingXPixels, m.yPx+paddingYPixels)
	dc.SetRGB(1, 1, 1)
	dc.Clear()
	dc.SetRGB(0, 0, 0)

	dcMain, _ := m.drawMain()
	dc.DrawImage(dcMain.Image(), paddingXPixels/2, paddingYPixels/2)

	if len(m.xAxisTicks) > 0 {
		dcXAxis, _ := m.drawXAxis()
		dc.DrawImage(dcXAxis.Image(), (paddingXPixels/2)-xAxisWidthPx, paddingYPixels/2)
	}

	if len(m.yAxisTicks) > 0 {
		dcYAxis, _ := m.drawYAxis()
		dc.DrawImage(dcYAxis.Image(), paddingXPixels/2, (paddingYPixels/2)+m.yPx)
	}

	var x = dc.Image()
	return x, nil
}

// Cost function (https://www.youtube.com/watch?v=F6GSRDoB-Cg & https://www.youtube.com/watch?v=YovTqTY-PYY)
func distanceBetweenPoint(p, q point) float64 {
	return math.Sqrt(((p.x - q.x) * (p.x - q.x)) + ((p.y - q.y) * (p.y - q.y)))
}

// Find solution using numerical analysis
func shortestDistanceAndPointOnLineToPoint(l line, p point) (float64, point) {
	// TODO: rename a and b to left and right or l and r so we have l, m, r?
	a := l.a
	b := l.b

	// Initial guess of solution. Our learning rate is initially ½ * len, then 1/4 * length, then 1/8 * length, ...
	// In our model we have the benefit of knowing the global maximum and minimum.
	// Also, the cost function of all points of line segment is two lines forming a v-shape (slope is constant)
	// and what we're after is the (x,y) of the bottom of v.
	// If we overshoot in our guess of (x,y), we're reverting direction. This means we'll always converge.
	// TODO: Q: What if the initial m is the global minimum?
	// TODO: Extend a and b to double length to allow for convergence at endpoint.
	m := point{a.x + ((b.x - a.x) / 2), b.y - ((b.y - a.y) / 2)}

	aD := distanceBetweenPoint(a, p)
	bD := distanceBetweenPoint(b, p)
	mD := distanceBetweenPoint(m, p)
	const epsilon = 0.001

	for {
		// what if an endpoint is the best option? We exclude endpoint so it'll never converge.
		// If endpoint is best option, it'll match two line segments.
		if aD <= mD {
			if math.Abs(mD-aD) < epsilon {
				break
			}
			b = point{m.x, m.y}
			m = point{a.x + ((b.x - a.x) / 2), b.y - ((b.y - a.y) / 2)}
		} else if bD <= mD {
			if math.Abs(mD-bD) < epsilon {
				break
			}
			a = point{m.x, m.y}
			m = point{a.x + ((b.x - a.x) / 2), b.y - ((b.y - a.y) / 2)}
		} else if aD > mD || bD > mD {
			// If both endpoints are farther from p than midpoint,
			// go in the direction of the smallest distance endpoint:
			// aD = 49.24, mD = 42.32, and bD = 42.74
			d1 := aD - mD
			d2 := bD - mD
			if d1 < d2 {
				// Go left
				if math.Abs(mD-aD) < epsilon {
					break
				}
				b = point{m.x, m.y}
				m = point{a.x + ((b.x - a.x) / 2), b.y - ((b.y - a.y) / 2)}
			}

			// Go right
			if math.Abs(mD-aD) < epsilon {
				break
			}
			a = point{m.x, m.y}
			m = point{a.x + ((b.x - a.x) / 2), b.y - ((b.y - a.y) / 2)}
		} else {
			panic("Does this ever happen?")
		}

		aD = distanceBetweenPoint(a, p)
		bD = distanceBetweenPoint(b, p)
		mD = distanceBetweenPoint(m, p)
	}

	return mD, m
}

func (m model) drawMain() (*gg.Context, error) {
	dc := gg.NewContext(m.xPx, m.yPx)
	dc.InvertY()
	dc.Scale(float64(m.xPx)/m.xMm, float64(m.yPx)/m.yMm)

	if m.debug {
		dc.DrawRectangle(0, 0, m.xMm, m.yMm)
		dc.SetLineWidth(1)
		dc.Stroke()
	}

	// x grid lines
	for _, t := range m.xAxisTicks {
		dc.DrawLine(0, m.yMm-t, m.xMm, m.yMm-t)
		dc.Stroke()
	}

	// y grid lines
	for _, t := range m.yAxisTicks {
		dc.DrawLine(t, 0, t, m.yMm)
		dc.Stroke()
	}

	// Draw fenestrations
	for _, f := range m.fenestrations {
		dc.DrawCircle(f.center.x, f.center.y, f.radius)
		dc.Stroke()
	}

	// Draw saw curves
	if int(m.xMm)%(int(m.sews[2].x)-int(m.sews[0].x)) != 0 {
		panic("Coordinates must end up where they start")
	}

	repetitions := int(m.xMm / (m.sews[2].x - m.sews[0].x))
	for j := 0; j < len(m.sewsYOffsets); j++ {
		currentY := m.sewsYOffsets[j]
		currentX := float64(0.0)
		for i := 0; i < repetitions; i++ {
			// for each line segment, compute shortests distance to point
			x1 := currentX + m.sews[0].x
			y1 := currentY + m.sews[0].y
			x2 := currentX + m.sews[1].x
			y2 := currentY + m.sews[1].y
			x3 := currentX + m.sews[2].x
			y3 := currentY + m.sews[2].y

			d, dP := shortestDistanceAndPointOnLineToPoint(line{point{x1, y1}, point{x2, y2}}, m.fenestrations[0].center)
			d1, dP1 := shortestDistanceAndPointOnLineToPoint(line{point{x2, y2}, point{x3, y3}}, m.fenestrations[0].center)
			fmt.Printf("%v %v\n%v %v\n", d, dP, d1, dP1)

			dc.DrawLine(x1, y1, x2, y2)
			dc.DrawLine(x2, y2, x3, y3)
			dc.Stroke()
			currentX += m.sews[2].x - m.sews[0].x
			currentY += m.sews[2].y - m.sews[0].y
		}
	}

	return dc, nil
}

const xAxisWidthPx = 100
const xAxisWidthMm = 20
const widthOfTickMm = 2

func (m model) drawXAxis() (*gg.Context, error) {
	dc := gg.NewContext(xAxisWidthPx, m.yPx)
	dc.Scale(float64(xAxisWidthPx/xAxisWidthMm), (float64(m.yPx) / m.yMm))

	dc.SetRGB(0, 0, 0)
	if err := dc.LoadFontFace("/usr/share/fonts/truetype/ubuntu/Ubuntu-M.ttf", 4); err != nil {
		panic(err)
	}

	if m.debug {
		dc.DrawRectangle(0, 0, xAxisWidthMm, m.yMm)
		dc.SetLineWidth(1)
		dc.Stroke()
	}

	dc.DrawLine(xAxisWidthMm, 0, xAxisWidthMm, m.yMm)
	for _, t := range m.xAxisTicks {
		dc.DrawLine(xAxisWidthMm, m.yMm-t, xAxisWidthMm-widthOfTickMm, m.yMm-t)
		dc.Stroke()
		t2 := strconv.Itoa(int(t))
		dc.DrawStringAnchored(t2, xAxisWidthMm-widthOfTickMm-5, m.yMm-t, 0.5, 0.5)
	}

	return dc, nil
}

const yAxisHightPx = 100
const yAxisHeighthMm = 20
const heightOfTickMm = 2

func (m model) drawYAxis() (*gg.Context, error) {
	dc := gg.NewContext(m.xPx, yAxisHightPx)
	dc.Scale(float64(yAxisHightPx/yAxisHeighthMm), (float64(m.xPx) / m.xMm)) // all dcs must have same scaling factor

	dc.SetRGB(0, 0, 0)
	if err := dc.LoadFontFace("/usr/share/fonts/truetype/ubuntu/Ubuntu-M.ttf", 4); err != nil {
		panic(err)
	}

	if m.debug {
		dc.DrawRectangle(0, 0, m.xMm, yAxisHeighthMm)
		dc.SetLineWidth(1)
		dc.Stroke()
	}

	dc.DrawLine(0, 0, m.xMm, 0)

	for _, t := range m.yAxisTicks {
		dc.DrawLine(t, 0, t, heightOfTickMm)
		dc.Stroke()

		t2 := strconv.Itoa(int(t))
		dc.DrawStringAnchored(t2, t, heightOfTickMm+3, 0.5, 0.5)
	}

	return dc, nil
}

func main() {
	m := model{
		debug:         false,
		xAxisTicks:    []float64{20, 40, 60, 80, 100},
		yAxisTicks:    []float64{20, 40, 60, 80, 100},
		gridLines:     true,
		xMm:           120,
		yMm:           120,
		xPx:           600,
		yPx:           600,
		sews:          [3]point{point{0, 0}, point{20, 30}, point{40, 0}},
		sewsYOffsets:  []float64{0, 40, 80},
		fenestrations: []fenestration{fenestration{point{20, 45}, 4}},
	}

	s := time.Now()
	img, _ := m.draw()
	fmt.Printf("Elapsed: %v\n", time.Since(s))

	f, err := os.Create("image.png")
	if err != nil {
		log.Fatal(err)
	}

	enc := png.Encoder{CompressionLevel: png.NoCompression}
	if err := enc.Encode(f, img); err != nil {
		f.Close()
		log.Fatal(err)
	}

	if err := f.Close(); err != nil {
		log.Fatal(err)
	}
}

func main1() {
	const S = 1024
	dc := gg.NewContext(S, S)
	dc.SetRGB(1, 1, 1)
	dc.Clear()
	dc.SetRGB(0, 0, 0)
	if err := dc.LoadFontFace("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf", 96); err != nil {
		panic(err)
	}
	dc.DrawStringAnchored("Hello, world!", S/2, S/2, 0.5, 0.5)
	dc.SavePNG("image.png")
}
