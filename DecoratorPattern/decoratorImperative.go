package main

import "fmt"

// Implementation of decorator/pipeline/middleware pattern.
//
// Shows input flowing through handlers with one calling the next and the
// ability to update input along the way.
//
// Output from running the progra is:

// PerformanceBehavior before
// LoggerBehavior before
// Request 1 2
// LoggerBehavior after
// PerformanceBehavior after

// Inpired by:
// - https://drstearns.github.io/tutorials/gomiddleware
// - https://www.youtube.com/watch?v=xyBcoGNGrsI (pros and cons)

type PipelineBehavior interface {
	Run(request *string)
}

type PerformanceBehavior struct {
	next PipelineBehavior
	// behavior specific state goes here */
}

func NewPerformanceBehavior(next PipelineBehavior /* behavior specific arguments go here */) PerformanceBehavior {
	return PerformanceBehavior{next: next}
}

func (b PerformanceBehavior) Run(s *string) {
	println("PerformanceBehavior before")
	// modify input for the next handler
	s2 := *s + " 1"
	b.next.Run(&s2)
	println("PerformanceBehavior after")
}

type LoggerBehavior struct {
	next PipelineBehavior
}

func NewLoggerBehavior(next PipelineBehavior) LoggerBehavior {
	return LoggerBehavior{next: next}
}

func (b LoggerBehavior) Run(s *string) {
	println("LoggerBehavior before")
	s2 := *s + " 2"
	b.next.Run(&s2)
	println("LoggerBehavior after")
}

type DispatcherBehavior struct {
}

func NewDispatcherBehavior() DispatcherBehavior {
	return DispatcherBehavior{}
}

func (m DispatcherBehavior) Run(s *string) {
	fmt.Println(*s)
}

func main() {
	pipeline := NewPerformanceBehavior(NewLoggerBehavior(NewDispatcherBehavior()))
	s := "Request"
	pipeline.Run(&s)
}
