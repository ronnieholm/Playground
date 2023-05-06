namespace CSDecoratorPattern.ImperativeGeneric;

public interface IRequest<out TResponse>
{
}

public interface IPipelineBehavior2<in TRequest, TResponse>
{
    TResponse Run(IRequest<TResponse> request);
}

public class PerformanceBehavior2<TRequest, TResponse> : IPipelineBehavior2<TRequest, TResponse>
{
    private readonly IPipelineBehavior2<TRequest, TResponse> _next;

    public PerformanceBehavior2(IPipelineBehavior2<TRequest, TResponse> next) => _next = next;

    public TResponse Run(IRequest<TResponse> request)
    {
        Console.WriteLine("PerformanceBehavior before");
        var response = _next.Run(request);
        Console.WriteLine("PerformanceBehavior after");
        return response;
    }
}

public class LoggerBehavior2<TRequest, TResponse> : IPipelineBehavior2<TRequest, TResponse>
{
    private readonly IPipelineBehavior2<TRequest, TResponse> _next;

    public LoggerBehavior2(IPipelineBehavior2<TRequest, TResponse> next) => _next = next;
    
    public TResponse Run(IRequest<TResponse> request)
    {
        Console.WriteLine("LoggerBehavior before");
        var response = _next.Run(request);
        Console.WriteLine("LoggerBehavior after");
        return response;
    }
}

public class DispatcherBehavior2<TRequest, TResponse> : IPipelineBehavior2<TRequest, TResponse>
{
    public TResponse Run(IRequest<TResponse> request)
    {
        Console.WriteLine("DispatcherBehavior before");
        Console.WriteLine("DispatcherBehavior after");
        return default!;
    }
}

public class CreateCommand : IRequest<string>
{
}

public class ImperativeGeneric
{
    public void Main()
    {
        var request = new CreateCommand();
        // TODO: how to avoid specifying type parameters?
        var pipeline =
            new PerformanceBehavior2<CreateCommand, string>(
            new LoggerBehavior2<CreateCommand, string>(
                new DispatcherBehavior2<CreateCommand, string>()));
        var response = pipeline.Run(request);
        Console.WriteLine(response);
    }
}