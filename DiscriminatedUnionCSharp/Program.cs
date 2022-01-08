/*
Based on https://stackoverflow.com/questions/63724308/using-c-sharp-9-0-records-to-build-smart-enum-like-discriminated-union-like-sum

type Shape = 
  | Circle of int
  | Rectangle of int * int

let getArea s = 
    match s with
    | Circle(r) -> Math.PI * float(r) * float(r)
    | Rectangle(l, b) -> float(l*b)
 */

namespace DiscriminatedUnionCSharp;

public abstract record Shape
{
    // Case types
    public record Circle(int Radius) : Shape;
    public record Rectangle(int Width, int Height) : Shape;
        
    // API
    public static double Area(Shape s) =>
        s switch {
            Circle c => Math.PI * c.Radius * c.Radius,
            Rectangle r => r.Width * r.Height,
            _ => throw new Exception("Unreachable")
        };
}

static class Program
{
    public static void Main()
    {
        var circle = new Shape.Circle(5);
        var rectangle = new Shape.Rectangle(4, 8);
        var a1 = Shape.Area(circle);
        var a2 = Shape.Area(rectangle);
        Console.WriteLine(a1);
        Console.WriteLine(a2);
    }
}