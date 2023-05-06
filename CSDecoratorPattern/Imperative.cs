namespace CSDecoratorPattern.Imperative;

// Downside: pipeline cannot return a value.

public interface IRequest
{
}

public interface IPipelineBehavior
{
    void Run(IRequest request);
}

public class PerformanceBehavior : IPipelineBehavior
{
    private readonly IPipelineBehavior _next;
    // Add behavior specific state

    public PerformanceBehavior(IPipelineBehavior next) => _next = next;
    
    public void Run(IRequest request)
    {
        Console.WriteLine("PerformanceBehavior before");
        _next.Run(request);
        Console.WriteLine("PerformanceBehavior after");
    }
}

public class LoggerBehavior : IPipelineBehavior
{
    private readonly IPipelineBehavior _next;

    public LoggerBehavior(IPipelineBehavior next) => _next = next;
    
    public void Run(IRequest request)
    {
        Console.WriteLine("LoggerBehavior before");
        _next.Run(request);
        Console.WriteLine("LoggerBehavior after");
    }
}

public class DispatcherBehavior : IPipelineBehavior
{
    public void Run(IRequest request)
    {
        Console.WriteLine(request);
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
        pipeline.Run(command);
    }
}