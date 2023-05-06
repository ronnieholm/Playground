namespace CSDecoratorPattern.ImperativeGeneric;

public interface IRequest<out TResponse>
{
}

public interface IPipelineBehavior2<in TRequest, out TResponse> where TRequest : IRequest<TResponse>
{
    TResponse Run(TRequest request);
}

public class PerformanceBehavior2<TRequest, TResponse> : IPipelineBehavior2<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IPipelineBehavior2<TRequest, TResponse> _next;

    public PerformanceBehavior2(IPipelineBehavior2<TRequest, TResponse> next) => _next = next;

    public TResponse Run(TRequest request)
    {
        Console.WriteLine("PerformanceBehavior before");
        var response = _next.Run(request);
        Console.WriteLine("PerformanceBehavior after");
        return response;
    }
}

public class LoggerBehavior2<TRequest, TResponse> : IPipelineBehavior2<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IPipelineBehavior2<TRequest, TResponse> _next;

    public LoggerBehavior2(IPipelineBehavior2<TRequest, TResponse> next) => _next = next;
    
    public TResponse Run(TRequest request)
    {
        Console.WriteLine("LoggerBehavior before");
        var response = _next.Run(request);
        Console.WriteLine("LoggerBehavior after");
        return response;
    }
}

public class DispatcherBehavior2<TRequest, TResponse> : IPipelineBehavior2<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public TResponse Run(TRequest request)
    {
        Console.WriteLine("DispatcherBehavior before");
        // Locate and call request handler here.
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
        
        // We cannot avoid specifying type parameters here, making the implementation not usable in practice. As these
        // pipeline types differ per request, different pipeline types are needed as well. MediatR gets around this by
        // using Activator.CreateInstance, passing in request and response types.
        var pipeline =
            new PerformanceBehavior2<CreateCommand, string>(
            new LoggerBehavior2<CreateCommand, string>(
                new DispatcherBehavior2<CreateCommand, string>()));
        var response = pipeline.Run(request);
        Console.WriteLine(response);
    }
}