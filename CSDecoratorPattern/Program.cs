using CSDecoratorPattern.ImperativeNonGeneric;
using CSDecoratorPattern.ImperativeGeneric;
using CSDecoratorPattern.FunctionalNonGeneric;

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