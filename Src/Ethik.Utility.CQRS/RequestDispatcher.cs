
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Threading;

namespace Ethik.Utility.CQRS;

public delegate Task<TResult> RequestHandlerDelegate<TResult>();
public class RequestDispatcher : IRequestDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    public RequestDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> SendAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken)
    {
        // Get the runtime type of the request
        var requestType = request.GetType();

        // Retrieve the generic "Dispatch" method via reflection
        var methodInfo = typeof(RequestDispatcher)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(m =>
                m.Name == "Dispatch" &&
                m.IsGenericMethodDefinition &&
                m.GetGenericArguments().Length == 2);

        if (methodInfo == null)
            throw new InvalidOperationException("Dispatch method not found.");

        // Create a concrete generic method with the resolved types
        var genericMethod = methodInfo.MakeGenericMethod(requestType, typeof(TResult));

        // Invoke the method and await the result
        var resultTask = genericMethod.Invoke(this, new object[] { request, cancellationToken }) as Task<TResult> ?? throw new ArgumentException("Argument null"); ;


        return await resultTask;
    }

    private async Task<TResult> Dispatch<TRequest, TResult>(TRequest request, CancellationToken cancellationToken)
    {
        var handler = _serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResult>>();
        var behaviors = _serviceProvider
            .GetServices<IPipelineBehavior<TRequest, TResult>>()
            .Reverse() // Reverse to ensure order matches registration (outermost first)
            .ToList();
        RequestHandlerDelegate<TResult> pipeline =
            () => handler.Handle(request, cancellationToken);
        foreach (var behavior in behaviors)
        {
            var next = pipeline;
            pipeline = () => behavior.Handle(request, next, cancellationToken);
        }

        // Execute the pipeline
        return await pipeline();
        //return handler.Handle(request, cancellation);
    }
}
