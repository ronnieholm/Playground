namespace CSDecoratorPattern.Imperative;

// Marker interface. Not strictly required.
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
        // Locate and call request handler here.
        return 42;
    }
}

public class CreateCommand : IRequest
{
}

public class Imperative
{
    public void Main()
    {
        var pipeline = new PerformanceBehavior(new LoggerBehavior(new DispatcherBehavior()));
        var command = new CreateCommand();
        var response = pipeline.Run(command);
        Console.WriteLine("Response: " + response.GetType() + " = " + response);
    }
}