using CSDecoratorPattern.ImperativeNonGeneric;
using CSDecoratorPattern.ImperativeGeneric;
using CSDecoratorPattern.Functional;

public class Program
{
    public static void Main()
    {
        new ImperativeNonGeneric().Main();
        Console.WriteLine();
        new ImperativeGeneric().Main();
        Console.WriteLine();
        new Functional().Main();
    }
}