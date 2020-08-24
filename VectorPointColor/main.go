package main

/*

The following learnings originate from attempting to implement Ray tracing in a
weekend in Go, mapping the C++ reference implementation too closely to Go.

Because Go doesn't support method nor operator overloading, implementing the
vector math at first seemed very verbose. Especially if we try to be (overly)
efficient and pass around vec3 pointers; type aliasing Vec3 to Point3 to color
makes it even worse due to casting. Don't type alias something as complex as
Vec3. Type aliases are best suited for primitive types. Create explicit Vec3,
Point3, and Color types and add only the operations needed by the application.

To overcome verbose code with lots of variables holding intermediate results,
instead create a fluent API. The downside to such API is that it isn't pointer
based. Every operation creates a new instance, causing extra allocations. The
code becomes much more readable, though.

The Vec3, Point3, and Color types in the applications are inspired by
https://github.com/ypujante/ray-tracing/blob/master/model.go

*/

func main() {
	// Allows for fluent expressions such as ones below
	// u := Cross(vup, w).Unit()

	// Notice how even without operator overloading, this code reads almost as
	// well as with overloading: origin + (u * ...) + (v * ...) + (w * ...)
	// lowerLeftCorner := origin.Translate(u.Scale(-(halfWidth * focusDist))).Translate(v.Scale(-(halfHeight * focusDist))).Translate(w.Scale(-focusDist))
}
