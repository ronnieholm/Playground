namespace CSDecoratorPattern.Functional;

using PipelineBehavior = Func<IRequest3, object>;

// Marker interface. Not strictly required as we could replace it with object.
public interface IRequest3
{
}

public class Functional
{
    public PipelineBehavior WithPerformanceBehavior(PipelineBehavior next /* add dependencies */)
    {
        return request =>
        {
            Console.WriteLine("PerformanceBehavior before");
            var response = next(request);
            Console.WriteLine("PerformanceBehavior after");
            return response;
        };
    }

    public PipelineBehavior WithLoggerBehavior(PipelineBehavior next)
    {
        return request =>
        {
            Console.WriteLine("LoggerBehavior before");
            var response = next(request);
            Console.WriteLine("LoggerBehavior after");
            return response;
        };
    }

    public object WithDispatcherBehavior(IRequest3 request)
    {
        Console.WriteLine("DispatcherBehavior before");
        // Locate and call request handler here.
        Console.WriteLine("DispatcherBehavior after");
        return 42;
    }
    
    public class CreateCommand : IRequest3
    {
    }
    
    public void Main()
    {
        var pipeline = WithPerformanceBehavior(WithLoggerBehavior(WithDispatcherBehavior));
        var command = new CreateCommand();
        var response = pipeline(command);
        Console.WriteLine("Response: " + response.GetType() + " = " + response);
    }
}