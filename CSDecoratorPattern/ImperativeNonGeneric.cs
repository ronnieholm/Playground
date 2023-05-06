namespace CSDecoratorPattern.ImperativeNonGeneric;

// If all behaviors set all fields through the constructors, and all dependencies they call into are thread-safe, the
// pipeline could be instantiated only once. Otherwise, it should be instantiated per request.

// Marker interface. Not strictly required as we could replace it with object.
public interface IRequest
{
}

public interface IPipelineBehavior
{
    object Run(IRequest request);
}

public class PerformanceBehavior : IPipelineBehavior
{
    private readonly IPipelineBehavior _next;
    // Add behavior specific state

    public PerformanceBehavior(IPipelineBehavior next) => _next = next;
    
    public object Run(IRequest request)
    {
        Console.WriteLine("PerformanceBehavior before");
        var response = _next.Run(request);
        Console.WriteLine("PerformanceBehavior after");
        return response;
    }
}

public class LoggerBehavior : IPipelineBehavior
{
    private readonly IPipelineBehavior _next;

    public LoggerBehavior(IPipelineBehavior next) => _next = next;
    
    public object Run(IRequest request)
    {
        Console.WriteLine("LoggerBehavior before");
        var response = _next.Run(request);
        Console.WriteLine("LoggerBehavior after");
        return response;
    }
}

public class DispatcherBehavior : IPipelineBehavior
{
    public object Run(IRequest request)
    {
        // Locate and dispatch to individual handlers here.
        Console.WriteLine("Dispatcher");
        return 42;
    }
}

public class CreateCommand : IRequest
{
}

public class ImperativeNonGeneric
{
    public void Main()
    {
        var pipeline = new PerformanceBehavior(new LoggerBehavior(new DispatcherBehavior()));
        var command = new CreateCommand();
        var response = pipeline.Run(command);
        Console.WriteLine("Response: " + response.GetType() + " = " + response);
    }
}