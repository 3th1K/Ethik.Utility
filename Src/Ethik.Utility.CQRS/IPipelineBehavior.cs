namespace Ethik.Utility.CQRS;

public interface IPipelineBehavior<TRequest, TResult>
{
    Task<TResult> Handle(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken);
}
